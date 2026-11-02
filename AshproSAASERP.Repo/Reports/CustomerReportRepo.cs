namespace AshproSAASERP.Repo.Reports;

public class CustomerReportRepo(IDbContextProvider contextProvider) : ICustomerReportRepo
{
    public async Task<CustomerStatementResponse> GetCustomerStatementAsync(CustomerStatementRequest req)
    {
        await using var db = contextProvider.CreateDbContext();
        var customer = await db.CustomerMasters.FirstOrDefaultAsync(x => x.CustomerId == req.CustomerId);
        if (customer == null)
        {
            return new CustomerStatementResponse(new List<CustomerStatementReportDtls>(), 0, 0, 0, 0);
        }

        int? customerAccountId = customer.FK_AccountId;
        if (customerAccountId == null || customerAccountId == 0)
        {
            // Fallback: Find account by Code match
            var acc = await db.AccountMasters.FirstOrDefaultAsync(x => x.Code == customer.Code);
            customerAccountId = acc?.AccountId;
        }

        if (customerAccountId == null || customerAccountId == 0)
        {
            return new CustomerStatementResponse(new List<CustomerStatementReportDtls>(), 0, 0, 0, 0, customer);
        }

        var query = db.TblAccountings
       .Where(x => x.FK_AccountId == customerAccountId)
       .AsQueryable();

        if (req.BranchId != 0)
        {
            query = query.Where(x => x.FK_BranchId == req.BranchId);
        }


        // 3. Calculate Opening Balance
        decimal openingBalance = 0;

        // Fetch all transactions before FromDate
        var allPre = await query.Where(x => x.EntryDate < req.FromDate).ToListAsync();

        foreach (var t in allPre)
        {
            ParseTransaction(t, out decimal dr, out decimal cr);
            openingBalance += (dr - cr);
        }

        // 4. Fetch Range Transactions
        var toDate = req.ToDate.Date.AddDays(1).AddTicks(-1);
        var transactions = await query
            .Where(x => x.EntryDate >= req.FromDate && x.EntryDate < toDate)
            .OrderBy(x => x.EntryDate)
            .ThenBy(x => x.TblAccountId)
            .ToListAsync();

        // Process List
        var resultList = new List<CustomerStatementReportDtls>();
        decimal runningBalance = openingBalance;

        decimal totalDebit = 0;
        decimal totalCredit = 0;

        foreach (var t in transactions)
        {
            decimal dr = 0, cr = 0;
            ParseTransaction(t, out dr, out cr);

            runningBalance += (dr - cr);
            totalDebit += dr;
            totalCredit += cr;

            resultList.Add(new CustomerStatementReportDtls(
                0,
                t.EntryDate?.ToString("dd/MM/yyyy") ?? "",
                t.VoucherNo ?? "",
                t.Type ?? "",
                t.Narration ?? "",
                dr == 0 ? null : dr,
                cr == 0 ? null : cr,
                runningBalance,
                "", // RefNo - Note: TblAccounting doesn't have RefNo column, might need adjustment if required later
                t.VoucherId ?? 0
            )); // SL will be indexed later
        }

        // SL numbering
        int sl = 1;
        resultList = resultList.Select(x => x with { SL = sl++ }).ToList();

        return new CustomerStatementResponse(
            resultList,
            openingBalance,
            totalDebit,
            totalCredit,
            runningBalance,
            customer
        );
    }

    private void ParseTransaction(Models.Transactions.Accounts.TblAccounting t, out decimal debit, out decimal credit)
    {

        debit = 0;
        credit = 0;
        decimal amt = t.Amount ?? 0;
        // Handle Negative Amount (if any repo stores it that way) - typical for "Credit" fields in some designs
        // But here we need to know if it is logically a Debit or Credit to the Customer.

        if (t.Type == "Sales")
        {
            // Sales Invoice -> Customer Debit
            // Ignore BillType "Credit" as it refers to Payment Mode (Credit Sale), not Ledger Side.
            if (amt >= 0) debit = amt;
            else credit = Math.Abs(amt);
        }
        else if (t.Type == "CustomerReceipt")
        {
            // Receipt -> Customer Credit
            credit = Math.Abs(amt);
        }
        else if (t.Type == "JournalEntry" || t.Type == "ExpenseEntry" || t.Type == "OpeningBalance" || t.Type == "CustomerOPEntry")
        {
            if (t.BillType == "Debit") debit = amt;
            else if (t.BillType == "Credit") credit = Math.Abs(amt);
            else
            {
                if (amt >= 0) debit = amt;
                else credit = Math.Abs(amt);
            }
        }
        else
        {
            // Default Fallback
            if (t.BillType == "Debit") debit = amt;
            else if (t.BillType == "Credit") credit = Math.Abs(amt);
            else
            {
                // Check if it looks like a payment (Credit) or Invoice (Debit)
                // This is risky. Let's assume Debit if positive, Credit if negative?
                // But ReceiptRepo stores positive.
                // We will assume Credit for unknown types if they seem "Receipt-like".
                // Safest: 
                if (amt >= 0) debit = amt;
                else credit = Math.Abs(amt);
            }
        }
    }

    public async Task<List<CustomerSummaryDTO>> GetCustomerSummaryAsync(DateTime fromDate, DateTime toDate, int branchId, int? customerId = null)
    {
        await using var db = contextProvider.CreateDbContext();
        var toDateEndOfDay = toDate.Date.AddDays(1).AddTicks(-1);

        // 1. Get All Customers
        var query = db.CustomerMasters.AsNoTracking().AsQueryable();
        if (customerId.HasValue)
            query = query.Where(x => x.CustomerId == customerId.Value);

        var customers = await query
            .Select(x => new { x.CustomerId, x.Code, x.Name, FK_AccountId = x.FK_AccountId })
            .ToListAsync();

        if (!customers.Any()) return new List<CustomerSummaryDTO>();

        // 2. Resolve Account IDs (FK or Fallback by Code)
        var customerCodes = customers.Where(x => !x.FK_AccountId.HasValue && !string.IsNullOrEmpty(x.Code))
                                     .Select(x => x.Code).Distinct().ToList();

        var fallbackAccounts = new Dictionary<string, int>();
        if (customerCodes.Any())
        {
            fallbackAccounts = await db.AccountMasters.AsNoTracking()
                 .Where(x => customerCodes.Contains(x.Code))
                 .Select(x => new { x.Code, x.AccountId })
                 .ToDictionaryAsync(k => k.Code, v => v.AccountId);
        }

        var finalAccountMap = new Dictionary<int, List<(string Code, string Name)>>();
        var accountIds = new HashSet<int>();

        foreach (var cust in customers)
        {
            int? accId = cust.FK_AccountId;
            if (!accId.HasValue && !string.IsNullOrEmpty(cust.Code) && fallbackAccounts.ContainsKey(cust.Code))
            {
                accId = fallbackAccounts[cust.Code];
            }

            if (accId.HasValue)
            {
                if (!finalAccountMap.ContainsKey(accId.Value))
                {
                    finalAccountMap[accId.Value] = new List<(string Code, string Name)>();
                }
                finalAccountMap[accId.Value].Add((cust.Code, cust.Name ?? ""));
                accountIds.Add(accId.Value);
            }
        }

        if (accountIds.Count == 0) return new List<CustomerSummaryDTO>();

        // 3. Get Raw Transactions (Need Type & BillType for correct logic)
        // We cannot use simple SQL grouping because ParseTransaction logic relies on strings (Type/BillType)
        var queryTrans = db.TblAccountings
            .AsNoTracking()
            .Where(x => x.EntryDate <= toDateEndOfDay
                        && (branchId == 0 || x.FK_BranchId == branchId)
                        && x.FK_AccountId.HasValue
                        && accountIds.Contains(x.FK_AccountId.Value));

        // Fetch minimal required columns necessary for ParseTransaction logic
        // We adapt ParseTransaction logic: it needs Type, BillType, Amount.
        var transactions = await queryTrans
            .Select(x => new
            {
                x.FK_AccountId,
                x.EntryDate,
                x.Amount,
                x.Type,
                x.BillType
            })
            .ToListAsync();

        // 4. In-Memory Aggregation
        var summaryCache = new Dictionary<int, (decimal Opening, decimal Debit, decimal Credit)>();

        foreach (var t in transactions)
        {
            int accId = t.FK_AccountId!.Value; // Safe due to filter

            // Replicating ParseTransaction Logic locally or we can use the method if we map it
            // Let's inline the logic for performance on anonymous object
            decimal dr = 0;
            decimal cr = 0;
            decimal amt = t.Amount ?? 0;

            if (t.Type == "Sales")
            {
                // Sales Invoice -> Customer Debit
                // Ignore BillType "Credit" as it refers to Payment Mode (Credit Sale), not Ledger Side.
                if (amt >= 0) dr = amt;
                else cr = Math.Abs(amt);
            }
            else if (t.Type == "CustomerReceipt")
            {
                cr = Math.Abs(amt);
            }
            else if (t.Type == "JournalEntry" || t.Type == "ExpenseEntry" || t.Type == "OpeningBalance" || t.Type == "CustomerOPEntry")
            {
                if (t.BillType == "Debit") dr = amt;
                else if (t.BillType == "Credit") cr = Math.Abs(amt);
                else { if (amt >= 0) dr = amt; else cr = Math.Abs(amt); }
            }
            else
            {
                // Fallback
                if (t.BillType == "Debit") dr = amt;
                else if (t.BillType == "Credit") cr = Math.Abs(amt);
                else { if (amt >= 0) dr = amt; else cr = Math.Abs(amt); }
            }

            if (!summaryCache.ContainsKey(accId))
            {
                summaryCache[accId] = (0, 0, 0);
            }

            var current = summaryCache[accId];

            if (t.EntryDate < fromDate || t.Type == "CustomerOPEntry")
            {
                // Opening Balance Accumulation (Dr - Cr)
                current.Opening += (dr - cr);
            }
            else
            {
                // Current Range
                current.Debit += dr;
                current.Credit += cr;
            }

            summaryCache[accId] = current;
        }

        // 5. Merge Results
        var results = new List<CustomerSummaryDTO>();

        foreach (var cust in customers)
        {
            int? accId = cust.FK_AccountId;
            if (!accId.HasValue && !string.IsNullOrEmpty(cust.Code) && fallbackAccounts.ContainsKey(cust.Code))
            {
                accId = fallbackAccounts[cust.Code];
            }

            if (!accId.HasValue)
            {
                // Skip if no account, effectively 0 balance
                continue;
            }

            decimal dispOpening = 0;
            decimal dispDebit = 0;
            decimal dispCredit = 0;

            if (summaryCache.ContainsKey(accId.Value))
            {
                var vals = summaryCache[accId.Value];
                dispOpening = vals.Opening;
                dispDebit = vals.Debit;
                dispCredit = vals.Credit;
            }

            decimal dispClosing = dispOpening + dispDebit - dispCredit;

            // Filter out Zero Closing Balance
            if (dispClosing == 0)
            {
                continue;
            }


            results.Add(new CustomerSummaryDTO
            {
                Code = cust.Code,
                Name = cust.Name,
                OpeningBalance = dispOpening,
                DebitAmount = dispDebit,
                CreditAmount = dispCredit,
                ClosingBalance = dispClosing
            });
        }

        var orderedResults = results.OrderBy(x => x.Name).ToList();
        int sl = 1;
        foreach (var item in orderedResults)
        {
            item.SL = sl++;
        }
        return orderedResults;
    }


    public async Task<List<CustomerListDTO>> GetCustomerListAsync()
    {
        try
        {
            await using var db = contextProvider.CreateDbContext();
            var query = db.CustomerMasters.AsNoTracking()
                .Include(x => x.CustomerGroup)
                .Include(x => x.Country)
                .Include(x => x.Salesman)
                .AsQueryable();

            var list = await query.Select(x => new CustomerListDTO
            {
                Code = x.Code,
                Name = x.Name ?? "",
                NameArabic = x.NameArabic,
                Address = x.Address,
                AddressArabic = x.AddressArabic,
                CreditLimit = x.CreditLimit,
                CreditDays = x.CreditDays,
                Contact = x.Contact,
                ContactArabic = x.ContactArabic,
                Telephone = x.Telephone,
                PhoneArabic = x.PhoneArabic,
                Mobile = x.Mobile,
                MobileArabic = x.MobileArabic,
                Mail = x.Email,
                VatNo = x.VatNo,
                VatNoArabic = x.VatNoArabic,
                BuildingNo = x.BuildingNo,
                BuildingNoArabic = x.BuildingNoArabic,
                StreetName = x.StreetName,
                StreetNameArabic = x.StreetNameArabic,
                District = x.District,
                DistrictArabic = x.DistrictArabic,
                City = x.City,
                CityArabic = x.CityArabic,
                PostalCode = x.PostalCode,
                PostalCodeArabic = x.PostalCodeArabic,
                AdditionalNo = x.AdditionalNo,
                AdditionalNoArabic = x.AdditionalNoArabic,
                OtherSellerNo = x.OtherSellerNo,
                OtherSellerNoArabic = x.OtherSellerNoArabic,
                CountrySubentity = x.CountrySubentity,
                CountrySubentityArabic = x.CountrySubentityArabic,
                IdentificationNo = x.IdentificationNo,
                IdentificationNoArabic = x.IdentificationNoArabic,
                Identification = x.Identification,
                CusBillType = x.CusBillType,
                NeedsFill = x.NeedsFill,
                IsSupplier = x.IsSupplier,
                IsActive = x.IsActive,
                DiscountPercentage = x.DiscountPercentage,
                Neighbourhood = x.Neighbourhood,
                NeighbourhoodArabic = x.NeighbourhoodArabic,
                GroupVat = x.GroupVat,
                CustomerGroupName = x.CustomerGroup != null ? x.CustomerGroup.Name : null,
                CountryName = x.Country != null ? x.Country.Name : null,
                SalesmanName = x.Salesman != null ? x.Salesman.Name : null
            }).OrderBy(x => x.Name).ToListAsync();

            int sl = 1;
            foreach (var item in list)
            {
                item.SL = sl++;
            }
            return list;
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<List<CustomerPendingInvoiceDTO>> GetCustomerPendingInvoiceAsync(DateTime fromDate, DateTime toDate, int branchId, int? customerId = null)
    {
        await using var db = contextProvider.CreateDbContext();
        var toDateEndOfDay = toDate.Date.AddDays(1).AddTicks(-1);

        var query = db.SalesInvoiceHdrs.AsNoTracking()
            .Where(x => x.BillType == "Credit"
                     && x.SalesInvoiceDate >= fromDate && x.SalesInvoiceDate <= toDateEndOfDay);

        if (branchId > 0)
            query = query.Where(x => x.FK_BranchId == branchId);

        if (customerId.HasValue && customerId > 0)
            query = query.Where(x => x.FK_CustomerId == customerId);

        var invoices = await query.ToListAsync();

        // Get customer names efficiently
        var customerIds = invoices.Select(x => x.FK_CustomerId).Distinct().ToList();
        var customerNames = await db.CustomerMasters.AsNoTracking()
            .Where(x => customerIds.Contains(x.CustomerId))
            .ToDictionaryAsync(x => x.CustomerId, x => new { x.Name, x.Code });

        // Fetch Discounts & Payments from CustomerReceiptDtls
        var invoiceIds = invoices.Select(x => x.SalesInvoiceId).ToList();
        var payments = await db.CustomerReceiptDtls.AsNoTracking()
            .Where(x => x.FK_InvoiceId.HasValue && invoiceIds.Contains(x.FK_InvoiceId.Value))
            .GroupBy(x => x.FK_InvoiceId!.Value)
            .Select(g => new { InvoiceId = g.Key, TotalPaid = g.Sum(x => x.PaymentAmount), TotalDiscount = g.Sum(x => x.Discount) })
            .ToDictionaryAsync(x => x.InvoiceId, x => new { x.TotalPaid, x.TotalDiscount });

        // Fetch Returns
        var returns = await db.SalesInvoiceRtnHdrs.AsNoTracking()
            .Where(x => invoiceIds.Contains(x.SalesInvoiceId) && !x.IsDeleted)
            .GroupBy(x => x.SalesInvoiceId)
            .Select(g => new { InvoiceId = g.Key, ReturnAmount = g.Sum(x => x.GrandTotal) })
            .ToDictionaryAsync(x => x.InvoiceId, x => x.ReturnAmount);

        var result = new List<CustomerPendingInvoiceDTO>();
        int sl = 1;

        foreach (var p in invoices.OrderBy(x => x.SalesInvoiceDate))
        {
            decimal paidInfo = 0;
            decimal discountInfo = 0;
            if (payments.TryGetValue(p.SalesInvoiceId, out var pm))
            {
                paidInfo = pm.TotalPaid;
                discountInfo = pm.TotalDiscount;
            }

            decimal returnInfo = returns.ContainsKey(p.SalesInvoiceId) ? returns[p.SalesInvoiceId] : 0;

            decimal balance = p.GrandTotal - paidInfo - discountInfo - returnInfo;

            if (balance <= 0.00m) continue;

            string custName = "";
            string custCode = "";
            if (customerNames.TryGetValue(p.FK_CustomerId, out var cust))
            {
                custName = cust.Name;
                custCode = cust.Code;
            }

            result.Add(new CustomerPendingInvoiceDTO
            {
                SL = sl++,
                Date = p.SalesInvoiceDate,
                VoucherNo = p.SalesInvoiceNo,
                VoucherType = "Sales",
                VoucherId = p.SalesInvoiceId,
                CustomerName = custName,
                CustomerCode = custCode,
                Narration = p.Remarks ?? "",
                Debit = p.GrandTotal,
                Credit = paidInfo + returnInfo,
                Discount = discountInfo,
                Balance = balance,
                NetAmount = p.GrandTotal,
                PaidAmount = paidInfo,
                ReturnAmount = returnInfo,
                PaymentMode = p.BillType ?? ""
            });
        }

        // Fetch Opening Entries logic
        var queryOp = db.CustomerOPEntryUpdated.AsNoTracking().AsQueryable();

        // Apply Date Filter (Same as Sales Invoices)
        // User expects pending invoices in this range, including Opening Entries dated in this range.
        queryOp = queryOp.Where(x => x.InvoiceDate >= fromDate && x.InvoiceDate <= toDateEndOfDay);

        if (customerId.HasValue && customerId > 0)
            queryOp = queryOp.Where(x => x.FK_CustomerId == customerId);

        if (branchId > 0)
            queryOp = queryOp.Where(x => x.FK_BranchId == branchId);

        var openingEntries = await queryOp.ToListAsync();

        if (openingEntries.Any())
        {
            var opInvoices = openingEntries.Select(x => (x.InvoiceNo ?? "OPENING") + " - OPENING").Distinct().ToList();
            var custIds = openingEntries.Where(x => x.FK_CustomerId.HasValue).Select(x => x.FK_CustomerId!.Value).Distinct().ToList();

            // Fetch payments with Customer Context and !IsDeleted check
            var paymentsList = await db.CustomerReceiptDtls.AsNoTracking()
                   .Include(x => x.Receipt)
                   .Where(x => x.FK_InvoiceId == null
                          && opInvoices.Contains(x.InvoiceNo)
                          && x.Receipt != null
                          && !x.Receipt.IsDeleted) // Filter deleted receipts
                   .Select(x => new
                   {
                       InvoiceNo = x.InvoiceNo,
                       // Handle nullable FK_CustomerId in Receipt if necessary, but it should be required. Use 0 if null.
                       CustomerId = x.Receipt!.FK_CustomerId,
                       Amount = x.PaymentAmount + x.Discount
                   })
                   .ToListAsync();

            // Filter by fetched Customer IDs (in memory to avoid complex EF translation if List contains many items)
            // Or if custIds is small, we could have filtered in DB. 
            // Given we need to match (InvoiceNo, CustomerId), let's filter in memory or refine DB query.
            // Improve DB query above to filter by custIds if possible?
            // queryOp already filtered by CustomerId if provided. 
            // If CustomerId NOT provided, custIds can be large.
            // But we only care about payments for the Opening Entries we fetched.

            var paymentsDict = paymentsList
                   .Where(x => custIds.Contains(x.CustomerId))
                   .GroupBy(x => (x.InvoiceNo, x.CustomerId))
                   .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

            foreach (var op in openingEntries)
            {
                string identifier = (op.InvoiceNo ?? "OPENING") + " - OPENING";
                int cid = op.FK_CustomerId ?? 0;

                decimal paid = 0;
                if (paymentsDict.TryGetValue((identifier, cid), out var pVal))
                {
                    paid = pVal;
                }
                decimal returned = op.ReturnedAmt ?? 0;
                decimal balance = (op.InvoiceAmt ?? 0) - paid - returned;

                if (balance > 0)
                {
                    string custName = "";
                    string custCode = "";
                    if (op.FK_CustomerId.HasValue)
                    {
                        var c = await db.CustomerMasters.AsNoTracking()
                            .Where(x => x.CustomerId == op.FK_CustomerId)
                            .Select(x => new { x.Name, x.Code })
                            .FirstOrDefaultAsync();
                        if (c != null)
                        {
                            custName = c.Name;
                            custCode = c.Code;
                        }
                    }

                    result.Add(new CustomerPendingInvoiceDTO
                    {
                        SL = sl++,
                        Date = op.InvoiceDate,
                        VoucherNo = op.InvoiceNo ?? "OPENING",
                        VoucherType = "Opening Balance",
                        VoucherId = op.CustomerOPEntryUpdatedId,
                        CustomerName = custName,
                        CustomerCode = custCode,
                        Narration = "Opening Balance",
                        Debit = op.InvoiceAmt ?? 0,
                        Credit = paid + returned,
                        Discount = 0,
                        Balance = balance,
                        NetAmount = op.InvoiceAmt ?? 0,
                        PaidAmount = paid,
                        ReturnAmount = returned,
                        PaymentMode = "Credit"
                    });
                }
            }

        }

        return result.OrderBy(x => x.Date)
                     .ThenBy(x => x.VoucherType != "Opening Balance")
                     .ThenBy(x => x.VoucherNo)
                     .ToList();
    }
}
