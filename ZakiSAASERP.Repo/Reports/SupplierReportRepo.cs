namespace ZakiSAASERP.Repo.Reports
{
    public class SupplierReportRepo(IDbContextProvider contextProvider) : ISupplierReportRepo
    {
        public async Task<List<SupplierStatementDTO>> GetSupplierStatementAsync(DateTime fromDate, DateTime toDate, int branchId, int? supplierId = null)
        {
            await using var db = contextProvider.CreateDbContext();
            // Fix Date Range
            var toDateEndOfDay = toDate.Date.AddDays(1).AddTicks(-1);

            // 1. Identify Account IDs and Metadata
            if (!supplierId.HasValue && branchId == 0) return new List<SupplierStatementDTO>();

            var querySuppliers = db.SupplierMasters.AsNoTracking();
            if (supplierId.HasValue)
                querySuppliers = querySuppliers.Where(x => x.SupplierId == supplierId.Value);

            var suppliers = await querySuppliers
                .Select(x => new { x.SupplierId, x.Code, x.Name, x.FK_AccountId })
                .ToListAsync();

            if (!suppliers.Any()) return new List<SupplierStatementDTO>();

            // Resolve Accounts
            var accountIds = new List<int>();
            var supplierIdToAccId = new Dictionary<int, int>();

            var supplierCodes = suppliers.Where(x => !x.FK_AccountId.HasValue).Select(x => x.Code).ToList();
            var fallbackAccounts = new Dictionary<string, int>();

            if (supplierCodes.Any())
            {
                fallbackAccounts = await db.AccountMasters.AsNoTracking()
                     .Where(x => supplierCodes.Contains(x.Code))
                     .Select(x => new { x.Code, x.AccountId })
                     .ToDictionaryAsync(k => k.Code, v => v.AccountId);
            }

            foreach (var sup in suppliers)
            {
                int? accId = sup.FK_AccountId;
                if (!accId.HasValue && !string.IsNullOrEmpty(sup.Code) && fallbackAccounts.ContainsKey(sup.Code))
                {
                    accId = fallbackAccounts[sup.Code];
                }

                if (accId.HasValue)
                {
                    accountIds.Add(accId.Value);
                    supplierIdToAccId[sup.SupplierId] = accId.Value;
                }
            }

            if (!accountIds.Any()) return new List<SupplierStatementDTO>();

            // 2. Fetch All Relevant Transactions
            var queryTrans = db.TblAccountings
                .AsNoTracking() // No Tracking for reports
                .Where(x => x.FK_AccountId.HasValue
                         && accountIds.Contains(x.FK_AccountId.Value));

            if (branchId != 0)
            {
                queryTrans = queryTrans.Where(x => x.FK_BranchId == branchId);
            }

            // We need all transactions UP TO EndDate for this logic
            var transactions = await queryTrans
                .Where(x => x.EntryDate <= toDateEndOfDay)
                .OrderBy(x => x.EntryDate)
                .ThenBy(x => x.TblAccountId)
                .Select(x => new
                {
                    x.EntryDate,
                    x.VoucherId,
                    x.VoucherNo,
                    x.Type,
                    x.FK_AccountId,
                    x.Narration,
                    x.BillType,
                    x.Amount
                }).ToListAsync();

            var result = new List<SupplierStatementDTO>();
            int sl = 1;

            foreach (var sup in suppliers)
            {
                int activeAccountId = 0;
                if (supplierIdToAccId.ContainsKey(sup.SupplierId)) activeAccountId = supplierIdToAccId[sup.SupplierId];
                else continue;

                // Filter transactions for this supplier's account
                var supTrans = transactions.Where(x => x.FK_AccountId == activeAccountId).ToList();

                decimal openingBalance = 0;
                // Calculate Opening Balance (Everything before FromDate)
                foreach (var t in supTrans.Where(x => x.EntryDate < fromDate))
                {
                    ParseTransaction(t.Amount ?? 0, t.Type ?? "", t.BillType ?? "", out decimal dr, out decimal cr);
                    // For Suppliers (Liability): Opening = Credit - Debit
                    openingBalance += (cr - dr);
                }

                // Current Range Transactions
                var currentTrans = supTrans.Where(x => x.EntryDate >= fromDate).ToList();

                // Add Opening Row
                decimal opDr = 0, opCr = 0;
                if (openingBalance < 0) opDr = Math.Abs(openingBalance); // Negative Liability = Asset (Debit balance)
                else opCr = openingBalance;

                result.Add(new SupplierStatementDTO
                {
                    SL = sl++,
                    Date = fromDate, // Show as From Date
                    VoucherNo = "",
                    VoucherType = "Opening Balance",
                    SupplierName = sup.Name,
                    SupplierCode = sup.Code,
                    Narration = "B/F",
                    Debit = opDr,
                    Credit = opCr,
                    Balance = openingBalance
                });

                decimal runningBalance = openingBalance;

                foreach (var t in currentTrans)
                {
                    ParseTransaction(t.Amount ?? 0, t.Type ?? "", t.BillType ?? "", out decimal dr, out decimal cr);
                    runningBalance += (cr - dr);

                    result.Add(new SupplierStatementDTO
                    {
                        SL = sl++,
                        Date = t.EntryDate,
                        VoucherNo = t.VoucherNo ?? t.VoucherId.ToString(),
                        VoucherId = t.VoucherId,
                        VoucherType = t.Type ?? "",
                        SupplierName = sup.Name,
                        SupplierCode = sup.Code,
                        Narration = t.Narration,
                        Debit = dr,
                        Credit = cr,
                        Balance = runningBalance,
                        PaymentMode = t.BillType
                    });
                }
            }
            return result;
        }

        private void ParseTransaction(decimal amt, string type, string billType, out decimal dr, out decimal cr)
        {
            dr = 0; cr = 0;
            // Standardize logic
            if (type == "Purchase")
            {
                // Purchase Invoice -> Credit Supplier
                if (billType == "Debit") dr = amt; // Unusual, maybe return?
                else cr = Math.Abs(amt);
            }
            else if (type == "SupplierPayment")
            {
                // Payment -> Debit Supplier
                dr = Math.Abs(amt);
            }
            else if (type == "Purchase Return")
            {
                // P.Return -> Debit Supplier
                dr = Math.Abs(amt);
            }
            else
            {
                // Fallback for Manual Journals/OPEntry
                if (billType == "Debit") dr = amt;
                else if (billType == "Credit") cr = Math.Abs(amt);
                else
                {
                    // Guess based on sign
                    if (amt >= 0) dr = amt; else cr = Math.Abs(amt);

                }
            }
        }

        public async Task<List<SupplierListDTO>> GetSupplierListAsync()
        {
            try
            {
                await using var db = contextProvider.CreateDbContext();
                var query = db.SupplierMasters.AsNoTracking().AsQueryable();
                var list = await query.Select(x => new SupplierListDTO
                {
                    Code = x.Code,
                    Name = x.Name,
                    Address = x.Address ?? "",
                    Telephone = x.Telephone ?? "",
                    Mail = x.Email ?? ""
                }).OrderBy(x => x.Name).ToListAsync();
                int sl = 1;
                foreach (var item in list) item.SL = sl++;
                return list;
            }
            catch (Exception) { throw; }
        }

        public async Task<List<SupplierSummaryDTO>> GetSupplierSummaryAsync(DateTime fromDate, DateTime toDate, int branchId, int? supplierId = null)
        {
            await using var db = contextProvider.CreateDbContext();
            var toDateEndOfDay = toDate.Date.AddDays(1).AddTicks(-1);

            // 1. Get Suppliers
            var query = db.SupplierMasters.AsNoTracking().AsQueryable();
            if (supplierId.HasValue) query = query.Where(x => x.SupplierId == supplierId.Value);

            var suppliers = await query
               .Select(x => new { x.SupplierId, x.Code, x.Name, FK_AccountId = x.FK_AccountId })
               .ToListAsync();

            if (!suppliers.Any()) return new List<SupplierSummaryDTO>();

            // 2. Resolve Accounts
            var supplierCodes = suppliers.Where(x => !x.FK_AccountId.HasValue && !string.IsNullOrEmpty(x.Code))
                                         .Select(x => x.Code).Distinct().ToList();
            var fallbackAccounts = new Dictionary<string, int>();
            if (supplierCodes.Any())
            {
                fallbackAccounts = await db.AccountMasters.AsNoTracking()
                     .Where(x => supplierCodes.Contains(x.Code))
                     .ToDictionaryAsync(x => x.Code, x => x.AccountId);
            }

            var activeAccountIds = new HashSet<int>();
            var supplierIdToAccId = new Dictionary<int, int>();
            foreach (var s in suppliers)
            {
                if (s.FK_AccountId.HasValue)
                {
                    activeAccountIds.Add(s.FK_AccountId.Value);
                    supplierIdToAccId[s.SupplierId] = s.FK_AccountId.Value;
                }
                else if (!string.IsNullOrEmpty(s.Code) && fallbackAccounts.ContainsKey(s.Code))
                {
                    int accId = fallbackAccounts[s.Code];
                    activeAccountIds.Add(accId);
                    supplierIdToAccId[s.SupplierId] = accId;
                }
            }

            if (activeAccountIds.Count == 0) return new List<SupplierSummaryDTO>();

            // 3. Fetch All Transactions (Pure Ledger)
            var transactions = await db.TblAccountings.AsNoTracking()
                .Where(x => x.EntryDate <= toDateEndOfDay
                            && (branchId == 0 || x.FK_BranchId == branchId)
                            && x.FK_AccountId.HasValue
                            && activeAccountIds.Contains(x.FK_AccountId.Value))
                .Select(x => new { x.FK_AccountId, x.EntryDate, x.Amount, x.Type, x.BillType })
                .ToListAsync();

            var summaryCache = new Dictionary<int, (decimal Opening, decimal Debit, decimal Credit)>();

            foreach (var t in transactions)
            {
                int accId = t.FK_AccountId!.Value;
                ParseTransaction(t.Amount ?? 0, t.Type ?? "", t.BillType ?? "", out decimal dr, out decimal cr);

                if (!summaryCache.ContainsKey(accId)) summaryCache[accId] = (0, 0, 0);
                var current = summaryCache[accId];

                if (t.EntryDate < fromDate)
                {
                    current.Opening += (cr - dr); // Liability Opening
                }
                else
                {
                    current.Debit += dr;
                    current.Credit += cr;
                }
                summaryCache[accId] = current;
            }

            // 4. Merge
            var results = new List<SupplierSummaryDTO>();
            foreach (var sup in suppliers)
            {
                int? accId = null;
                if (supplierIdToAccId.ContainsKey(sup.SupplierId)) accId = supplierIdToAccId[sup.SupplierId];

                if (accId == null) continue;

                decimal dispOpening = 0, dispDebit = 0, dispCredit = 0;
                if (summaryCache.ContainsKey(accId.Value))
                {
                    var vals = summaryCache[accId.Value];
                    dispOpening = vals.Opening;
                    dispDebit = vals.Debit;
                    dispCredit = vals.Credit;
                }

                // Closing = Opening + Cr - Dr
                decimal dispClosing = dispOpening + dispCredit - dispDebit;

                if (dispOpening == 0 && dispDebit == 0 && dispCredit == 0 && dispClosing == 0) continue;

                results.Add(new SupplierSummaryDTO
                {
                    Code = sup.Code,
                    Name = sup.Name,
                    OpeningBalance = dispOpening,
                    DebitAmount = dispDebit,
                    CreditAmount = dispCredit,
                    ClosingBalance = dispClosing
                });
            }

            var orderedResults = results.OrderBy(x => x.Name).ToList();
            int sl = 1;
            foreach (var item in orderedResults) item.SL = sl++;
            return orderedResults;
        }

        public async Task<List<SupplierPendingInvoiceDTO>> GetSupplierPendingInvoiceAsync(DateTime fromDate, DateTime toDate, int branchId, int? supplierId = null)
        {
            await using var db = contextProvider.CreateDbContext();
            var toDateEndOfDay = toDate.Date.AddDays(1).AddTicks(-1);

            var query = db.PurchaseHdrs.AsNoTracking()
                .Where(x => x.BillType == "Credit"
                         && x.PurchaseDate >= fromDate && x.PurchaseDate <= toDateEndOfDay);

            if (branchId > 0)
                query = query.Where(x => x.FK_BranchId == branchId);

            if (supplierId.HasValue && supplierId > 0)
                query = query.Where(x => x.FK_SupplierId == supplierId);

            var purchases = await query.ToListAsync();

            // Get supplier names efficiently
            var supplierIds = purchases.Select(x => x.FK_SupplierId).Distinct().ToList();
            var supplierNames = await db.SupplierMasters.AsNoTracking()
                .Where(x => supplierIds.Contains(x.SupplierId))
                .ToDictionaryAsync(x => x.SupplierId, x => new { x.Name, x.Code });

            // Fetch Discounts from SupplierPaymentDtls
            var purchaseIds = purchases.Select(x => x.PurchaseId).ToList();
            var discounts = await db.SupplierPaymentDtls.AsNoTracking()
                .Where(x => x.FK_PurchaseId.HasValue && x.FK_PurchaseId > 0 && purchaseIds.Contains(x.FK_PurchaseId.Value))
                .GroupBy(x => x.FK_PurchaseId!.Value)
                .Select(g => new { PurchaseId = g.Key, TotalDiscount = g.Sum(x => x.Discount) })
                .ToDictionaryAsync(x => x.PurchaseId, x => x.TotalDiscount);

            var purchaseNos = purchases.Select(x => x.PurchaseNo).ToList();
            var returns = await db.PurchaseReturnHdrs.AsNoTracking()
                .Where(x => x.FK_PurchaseId.HasValue && purchaseIds.Contains(x.FK_PurchaseId.Value) && !x.IsDeleted) // x.FK_PurchaseId links to original purchase
                .GroupBy(x => x.FK_PurchaseId!.Value)
                .Select(g => new { PurchaseId = g.Key, ReturnAmount = g.Sum(x => x.GrandTotal) })
                .ToDictionaryAsync(x => x.PurchaseId, x => x.ReturnAmount);

            var result = new List<SupplierPendingInvoiceDTO>();
            int sl = 1;

            foreach (var p in purchases.OrderBy(x => x.PurchaseDate))
            {
                // Filter "Pending" here based on dynamic calculation
                decimal totalPaymentRecorded = p.PaymentAmount ?? 0;
                decimal discountInfo = discounts.ContainsKey(p.PurchaseId) ? discounts[p.PurchaseId] : 0;
                decimal returnInfo = returns.ContainsKey(p.PurchaseId) ? returns[p.PurchaseId] : 0;

                // So Balance = GrandTotal - PaymentAmount - ReturnAmount.
                decimal balance = p.GrandTotal - totalPaymentRecorded - returnInfo;

                if (balance <= 0.00m) continue; // Skip fully paid/returned

                string supName = "";
                string supCode = "";
                if (supplierNames.TryGetValue(p.FK_SupplierId, out var sup))
                {
                    supName = sup.Name;
                    supCode = sup.Code;
                }

                // Pure values
                decimal actualPaid = totalPaymentRecorded - discountInfo;

                result.Add(new SupplierPendingInvoiceDTO
                {
                    SL = sl++,
                    Date = p.PurchaseDate,
                    VoucherNo = p.PurchaseNo,
                    VoucherType = "Purchase",
                    VoucherId = p.PurchaseId,
                    SupplierName = supName,
                    SupplierCode = supCode,
                    Narration = p.Narration,
                    Credit = p.GrandTotal,          // Invoice Amount
                    Debit = actualPaid,             // Paid Amount (Cash only)
                    ReturnAmount = returnInfo,      // Returned Amount (New)
                    Discount = discountInfo,        // Discount
                    Balance = balance, // Pending Amount

                    NetAmount = p.GrandTotal,
                    PaidAmount = actualPaid,
                    PaymentMode = p.BillType
                });
            }

            // Fetch Opening Entries logic (Added)
            var queryOp = db.SupplierOPEntryUpdated.AsNoTracking().Where(x => (x.BalanceAmt ?? 0) > 0);
            // Note: Filter > 0 balance at DB level is good optimization but strict logic requires dynamic calc.
            // But we can filter by Supplier/Branch first.

            if (supplierId.HasValue && supplierId > 0)
                queryOp = queryOp.Where(x => x.FK_SupplierId == supplierId);

            // Branch? Opening Entry usually has BranchId? Yes in Updated table.
            if (branchId > 0)
                queryOp = queryOp.Where(x => x.FK_BranchId == branchId);

            var openingEntries = await queryOp.ToListAsync();

            if (openingEntries.Any())
            {
                var opIdentities = openingEntries.Select(x => (x.PurchaseNo ?? "OPENING") + " - OPENING").ToList();

                // Fetch payments against these opening entries
                var existingPayments = await db.SupplierPaymentDtls.AsNoTracking()
                    .Where(x => x.FK_PurchaseId == null && opIdentities.Contains(x.Narration))
                    .GroupBy(x => x.Narration)
                    .Select(g => new { Identifier = g.Key, TotalPaid = g.Sum(x => x.PayingAmount + x.Discount) })
                    .ToDictionaryAsync(x => x.Identifier, x => x.TotalPaid);

                foreach (var op in openingEntries)
                {
                    string identifier = (op.PurchaseNo ?? "OPENING") + " - OPENING";
                    decimal paid = existingPayments.ContainsKey(identifier) ? existingPayments[identifier] : 0;
                    decimal balance = (op.PurchaseAmt ?? 0) - paid;

                    if (balance > 0)
                    {
                        // Get Supplier Name for OP Entry
                        string supName = "";
                        string supCode = "";
                        // If cached supplier not found (e.g. OP only supplier), we might miss name.
                        // But usually OP entry supplier exists in Master.
                        if (op.FK_SupplierId.HasValue)
                        {
                            // If we didn't fetch this supplier in Purchase loop, we need to try fetch?
                            // Or use Cache.
                            if (supplierNames.TryGetValue(op.FK_SupplierId.Value, out var sup))
                            {
                                supName = sup.Name;
                                supCode = sup.Code;
                            }
                            else
                            {

                            }
                        }

                        result.Add(new SupplierPendingInvoiceDTO
                        {
                            SL = sl++,
                            Date = op.PurchaseDate,
                            VoucherNo = op.PurchaseNo ?? "OPENING", // Show Original Invoice No
                            VoucherType = "Opening Balance",
                            VoucherId = op.SupplierOPEntryUpdatedId,
                            SupplierName = supName,
                            SupplierCode = supCode,
                            Narration = "Opening Balance",
                            Credit = op.PurchaseAmt ?? 0,
                            Debit = paid,
                            Discount = 0,
                            Balance = balance,

                            NetAmount = op.PurchaseAmt ?? 0,
                            PaidAmount = paid,
                            PaymentMode = "Credit"
                        });
                    }
                }
            }

            return result.OrderBy(x => x.Date).ToList();
        }


        public async Task<List<SupplierPaymentReportDTO>> GetSupplierPaymentReportAsync(DateTime fromDate, DateTime toDate, int branchId, int? supplierId, string? billType)
        {
            await using var db = contextProvider.CreateDbContext();
            var toDateEndOfDay = toDate.Date.AddDays(1).AddTicks(-1);

            var query = from p in db.SupplierPaymentHdrs.AsNoTracking()
                        join s in db.SupplierMasters.AsNoTracking() on p.FK_SupplierId equals s.SupplierId
                        where !p.IsDeleted && p.PaidDate >= fromDate && p.PaidDate <= toDateEndOfDay
                        select new { p, s };

            if (branchId > 0)
            {
                query = query.Where(x => x.p.FK_BranchId == branchId);
            }

            if (supplierId.HasValue && supplierId > 0)
            {
                query = query.Where(x => x.p.FK_SupplierId == supplierId);
            }

            // Bill Type Filter
            if (!string.IsNullOrEmpty(billType) && billType != "All")
            {
                query = query.Where(x => x.p.PayMode == billType);
            }
            else
            {
                query = query.Where(x => x.p.PayMode != "Credit");
            }

            var list = await query.Select(x => new SupplierPaymentReportDTO
            {
                Date = x.p.PaidDate,
                VoucherNo = x.p.VoucherNo,
                VoucherType = "Payment",
                PaymentMode = x.p.PayMode,
                SupplierName = x.s.Name,
                SupplierCode = x.s.Code,
                Narration = x.p.Narration,
                Amount = x.p.Amount,
                ChequeNo = x.p.ChequeNo,
                ChequeDate = x.p.ChequeDate,
                Bank = x.p.Bank
            }).ToListAsync();

            int sl = 1;
            foreach (var item in list)
            {
                item.SL = sl++;
            }

            return list.OrderBy(x => x.Date).ToList();
        }
    }
}