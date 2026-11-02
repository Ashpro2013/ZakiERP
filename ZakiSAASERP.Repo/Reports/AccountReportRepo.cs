namespace ZakiSAASERP.Repo.Reports;

public class AccountReportRepo(IDbContextProvider contextProvider) : IAccountReportRepo
{
    public async Task<JournalEntryReportResponse> GetJournalEntryReport(JournalEntryReportRequest entries)
    {
        try
        {
            await using var db = contextProvider.CreateDbContext();
            var BranchId = entries.BranchId == 0 ? null : entries.BranchId;
            var AccountId = entries.AccountId == 0 ? null : entries.AccountId;
            var query = from journal in db.JournalEntryDtls
                        join hdr in db.JournalEntryHdrs on journal.FK_JournalId equals hdr.JournalId
                        join ac in db.AccountMasters on journal.FK_AccountId equals ac.AccountId
                        where (BranchId == null || hdr.FK_BranchId == BranchId)
                        && hdr.JournalDate >= entries.FromDate!.Value.Date && hdr.JournalDate < entries.ToDate!.Value.Date.AddDays(1)
                        && (AccountId == null || journal.FK_AccountId == AccountId)
                        select new
                        {
                            hdr.JournalDate,
                            hdr.JournalNo,
                            ac.Code,
                            ac.Name,
                            journal.Narration,
                            journal.DebitAmount,
                            journal.CreditAmount,
                            journal.DebitVat,
                            journal.CreditVat
                        };

            var result = await query.ToListAsync();

            var reportItems = result.Select((x, index) => new JournalEntryReportDtls(
                index + 1,
                x.Code,
                x.Name,
                x.JournalNo,
                x.JournalDate.ToString("dd/MM/yyyy"),
                x.Narration,
                x.DebitAmount,
                x.DebitVat,
                x.CreditAmount,
                x.CreditVat
            )).ToList();
            var totalDebit = reportItems.Sum(x => x.Debit);
            var totalCredit = reportItems.Sum(x => x.Credit);

            reportItems.Add(new JournalEntryReportDtls(
                   null,            // SL
                   "",              // AccountCode
                   "TOTAL",         // AccountName
                   "",              // InvNo
                   "",              // Date
                   "",              // Narration
                   totalDebit, // Debit
                   null,            // DebitVat
                   totalCredit, // Credit
                   null             // CreditVat
               ));

            return new JournalEntryReportResponse(reportItems, totalDebit, totalCredit);
        }
        catch (Exception)
        {
            // Handle exception appropriately, maybe log it
            throw;
        }
    }
    public async Task<TrialBalanceReportResponse> GetTrialBalanceReport(TrialBalanceReportRequest request)
    {
        try
        {
            await using var db = contextProvider.CreateDbContext();
            var branchId = request.BranchId == 0 ? null : (int?)request.BranchId;

            var fromDate = request.FromDate == DateTime.MinValue
                ? DateTime.Today
                : request.FromDate;

            var toDate = request.ToDate == DateTime.MinValue
                ? DateTime.Today
                : request.ToDate;

            // Group transactions by AccountId
            var ledgerGroups = await db.TblAccountings
                .Where(x =>
                    (branchId == null || x.FK_BranchId == branchId) &&
                    x.EntryDate < toDate.Date.AddDays(1) &&
                    x.FK_AccountId.HasValue
                )
                .GroupBy(x => x.FK_AccountId)
                .Select(g => new
                {
                    AccountId = g.Key!.Value,

                    // Before FromDate => Opening Transactions
                    OpeningSum = g
                        .Where(x => x.EntryDate < fromDate)
                        .Sum(x => x.Amount),

                    // Period Debit
                    DebitSum = g
                        .Where(x => x.EntryDate >= fromDate.Date &&
                                    x.EntryDate < toDate.Date.AddDays(1) &&
                                    x.Amount > 0)
                        .Sum(x => x.Amount),

                    // Period Credit (Negative values)
                    CreditSum = g
                        .Where(x => x.EntryDate >= fromDate.Date &&
                                    x.EntryDate < toDate.Date.AddDays(1) &&
                                    x.Amount < 0)
                        .Sum(x => x.Amount)
                })
                .ToListAsync();


            // Convert to Dictionary for fast lookup
            var ledgerDict = ledgerGroups
                .ToDictionary(x => x.AccountId);


            var accounts = await db.AccountMasters
                .AsNoTracking()
                .ToListAsync();


            var reportItems = new List<TrialBalanceReportDtls>();


            foreach (var acc in accounts)
            {
                ledgerDict.TryGetValue(acc.AccountId, out var ledger);


                // Transaction values
                decimal transOpening = ledger?.OpeningSum ?? 0;
                decimal transDebit = ledger?.DebitSum ?? 0;
                decimal transCredit = Math.Abs(ledger?.CreditSum ?? 0);


                // Master Opening (- = Debit, + = Credit)
                decimal masterOpening = (decimal)acc.Opening;

                if (acc.DbCr == "Cr" || acc.DbCr == "Credit")
                {
                    masterOpening = Math.Abs(masterOpening); // Credit = +
                }
                else
                {
                    masterOpening = -Math.Abs(masterOpening); // Debit = -
                }


                // Net balances
                decimal openingBalance = masterOpening + transOpening;

                decimal closingBalance = openingBalance + transDebit - transCredit;


                // Hide zero balance if needed
                if (request.ShowZeroBalance ||
                    openingBalance != 0 ||
                    transDebit != 0 ||
                    transCredit != 0 ||
                    closingBalance != 0)
                {
                    reportItems.Add(new TrialBalanceReportDtls(
                        acc.AccountId,
                        acc.Code,
                        acc.Name,
                        openingBalance,
                        transDebit,
                        transCredit,
                        closingBalance
                    ));
                }
            }
            reportItems.Add(new TrialBalanceReportDtls(
                null,
                "", "TOTAL",
                reportItems.Sum(x => x.Opening),
                reportItems.Sum(x => x.TransactionDebit),
                reportItems.Sum(x => x.TransactionCredit),
                reportItems.Sum(x => x.Closing)
                ));

            return new TrialBalanceReportResponse
            {
                Items = reportItems,

                TotalOpening = reportItems.Sum(x => x.Opening),
                TotalTransactionDebit = reportItems.Sum(x => x.TransactionDebit),
                TotalTransactionCredit = reportItems.Sum(x => x.TransactionCredit),
                TotalClosing = reportItems.Sum(x => x.Closing)
            };
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<ExpenseReportResponse> GetExpenseReport(ExpenseReportRequest request)
    {
        try
        {
            await using var db = contextProvider.CreateDbContext();
            var branchId = request.BranchId == 0 ? null : request.BranchId;

            var fromDate = request.FromDate ?? DateTime.Today;
            var toDate = request.ToDate ?? DateTime.Today;

            var query = from hdr in db.ExpenseEntryHdrs
                        join dtls in db.ExpenseEntryDtls on hdr.ExpenseEntryId equals dtls.FK_ExpenseEntryId
                        join ac in db.AccountMasters on dtls.FK_AccountId equals ac.AccountId
                        where (branchId == null || hdr.FK_BranchId == branchId)
                              && hdr.EntryDate >= fromDate.Date
                              && hdr.EntryDate < toDate.Date.AddDays(1)
                        select new
                        {
                            hdr.EntryDate,
                            hdr.ExpenseNo,
                            ac.Code,
                            ac.Name,
                            dtls.Amount,
                            dtls.VatAmt,
                            TotalAmount = dtls.Amount + dtls.VatAmt
                        };

            var data = await query.ToListAsync();

            var reportItems = data.Select((x, index) => new ExpenseReportDtls(
                index + 1,
                x.EntryDate.ToString("dd/MM/yyyy"),
                x.ExpenseNo,
                x.Code,
                x.Name,
                x.Amount,
                x.VatAmt,
                x.TotalAmount
            )).ToList();

            var totalAmount = reportItems.Sum(x => x.Amount);
            var totalVat = reportItems.Sum(x => x.VatAmount);
            var totalNet = reportItems.Sum(x => x.TotalAmount);
            reportItems.Add(new ExpenseReportDtls(
    null,            // SL
    "",              // Date
    "TOTAL",         // DocNo (used as label)
    "",              // AccountCode
    "",              // AccountName
    totalAmount, // Amount
    totalVat,    // VatAmount
    totalNet     // TotalAmount
));



            return new ExpenseReportResponse(reportItems, totalAmount, totalVat, totalNet);
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<ProfitAndLossReportResponse> GetProfitAndLossReport(ProfitAndLossReportRequest request)
    {
        try
        {
            await using var db = contextProvider.CreateDbContext();
            var branchId = request.BranchId == 0 ? null : request.BranchId;


            var fromDate = request.FromDate ?? DateTime.Today;
            var toDate = request.ToDate ?? DateTime.Today;

            // ---------------- Get Transactions ----------------
            var query = from trans in db.TblAccountings
                        join ac in db.AccountMasters on trans.FK_AccountId equals ac.AccountId
                        join head in db.AccountHeadMasters on ac.FK_AccountHeadId equals head.AccountHeadId
                        join type in db.AccountTypes on head.FK_AcTypeId equals type.AcTypeId
                        where (branchId == null || trans.FK_BranchId == branchId)
                              && trans.EntryDate >= fromDate.Date
                              && trans.EntryDate < toDate.Date.AddDays(1)
                        select new
                        {
                            ac.Code,
                            ac.Name,
                            trans.Amount,
                            TypeName = type.AcTypeName
                        };


            var data = await query.ToListAsync();


            // ---------------- Opening / Closing Stock (WAC Valuation) ----------------
            decimal openingStock = await GetStockValueAsync(db, branchId, fromDate.Date.AddDays(-1));
            decimal closingStock = await GetStockValueAsync(db, branchId, toDate.Date);


            // ---------------- Group ----------------
            var grouped = data
                .GroupBy(x => new { x.Code, x.Name, x.TypeName })
                .Select(g => new
                {
                    g.Key.Code,
                    g.Key.Name,
                    g.Key.TypeName,
                    Amount = g.Sum(x => (decimal?)x.Amount ?? 0)
                })
                .ToList();


            var reportItems = new List<ProfitAndLossPrintItem>();


            // ---------------- Classification ----------------
            foreach (var item in grouped)
            {
                string category = "";

                string lowerType = item.TypeName.ToLower();
                string lowerName = item.Name.ToLower();   // ✅ Added


                // ---------------- INCOME ----------------
                if (lowerType.Contains("income") ||
                    lowerType.Contains("sales") ||
                    lowerType.Contains("revenue"))
                {
                    category = "Income";
                }

                // ---------------- COGS (Purchase Fixed) ----------------
                else if (
                    lowerName.Contains("purchase") ||      // ✅ FIX
                    lowerName.Contains("purchases") ||     // ✅ FIX
                    lowerType.Contains("cost of goods") ||
                    lowerType.Contains("cogs") ||
                    lowerType.Contains("opening stock") ||
                    lowerType.Contains("direct expense")
                )
                {
                    category = "COGS";
                }

                // ---------------- EXPENSE ----------------
                else if (lowerType.Contains("expense"))
                {
                    category = "Expense";
                }


                // ---------------- Add Item ----------------
                if (!string.IsNullOrEmpty(category))
                {
                    decimal displayAmount = item.Amount;

                    // Income stored as credit (-), show as +
                    if (category == "Income")
                    {
                        displayAmount = -item.Amount;
                    }

                    reportItems.Add(new ProfitAndLossPrintItem(
                        item.Code,
                        item.Name,
                        displayAmount,
                        category
                    ));
                }
            }


            // ---------------- Totals ----------------
            var totalIncome = reportItems
                .Where(x => x.Category == "Income")
                .Sum(x => x.Amount);

            var totalCOGS = reportItems
                .Where(x => x.Category == "COGS")
                .Sum(x => x.Amount);

            var totalExpense = reportItems
                .Where(x => x.Category == "Expense")
                .Sum(x => x.Amount);


            // ---------------- Profit Calculation ----------------
            var grossProfit =
                (totalIncome + closingStock)
                - (totalCOGS + openingStock);


            var netProfit = grossProfit - totalExpense;


            // ---------------- Return ----------------
            return new ProfitAndLossReportResponse(
                reportItems,
                totalIncome,
                totalCOGS,
                totalExpense,
                grossProfit,
                netProfit,
                openingStock,
                closingStock
            );
        }
        catch (Exception ex)
        {
            throw new Exception("Error generating Profit & Loss Report", ex);
        }
    }
    public async Task<BalanceSheetReportResponse> GetBalanceSheetReport(BalanceSheetReportRequest request)
    {
        try
        {
            await using var db = contextProvider.CreateDbContext();
            var branchId = request.BranchId == 0 ? null : request.BranchId;

            // 1. Get Transactions
            // We need to fetch balances for Assets, Liabilities, and Capital
            // Logic similar to P&L but filtering for different Account Types

            var toDate = request.ToDate ?? DateTime.Today;

            var query = from trans in db.TblAccountings
                        join ac in db.AccountMasters on trans.FK_AccountId equals ac.AccountId
                        join head in db.AccountHeadMasters on ac.FK_AccountHeadId equals head.AccountHeadId
                        join type in db.AccountTypes on head.FK_AcTypeId equals type.AcTypeId
                        where (branchId == null || trans.FK_BranchId == branchId)
                              && trans.EntryDate < toDate.Date.AddDays(1) // Balance Sheet is "As on Date"
                        select new
                        {
                            ac.Code,
                            ac.Name,
                            trans.Amount,
                            TypeName = type.AcTypeName
                        };

            var data = await query.ToListAsync();

            // Group by Account
            var grouped = data
                .GroupBy(x => new { x.Code, x.Name, x.TypeName })
                .Select(g => new
                {
                    g.Key.Code,
                    g.Key.Name,
                    g.Key.TypeName,
                    Amount = g.Sum(x => (decimal?)x.Amount ?? 0)
                })
                .ToList();

            var reportItems = new List<BalanceSheetPrintItem>();

            // 2. Classify
            foreach (var item in grouped)
            {
                string category = "";
                string lowerType = item.TypeName.ToLower();

                // Assets
                if (lowerType.Contains("asset"))
                {
                    category = "Asset";
                }
                // Liabilities
                else if (lowerType.Contains("liability") || lowerType.Contains("liabilities"))
                {
                    category = "Liability";
                }
                // Capital / Equity
                else if (lowerType.Contains("capital") || lowerType.Contains("equity"))
                {
                    category = "Capital";
                }

                if (!string.IsNullOrEmpty(category) && item.Amount != 0)
                {
                    decimal displayAmount = item.Amount;

                    // Conventional Sign handling
                    // Assets are Debit (+).
                    // Liabilities/Capital are Credit (-).

                    if (category == "Liability" || category == "Capital")
                    {
                        displayAmount = Math.Abs(item.Amount);
                    }

                    reportItems.Add(new BalanceSheetPrintItem(
                        item.Code,
                        item.Name,
                        displayAmount,
                        category
                    ));
                }
            }

            // 3. Closing Stock (Asset) - WAC Valuation
            decimal closingStockVal = await GetStockValueAsync(db, branchId, toDate.Date);

            // Add Closing Stock to Assets list
            if (closingStockVal != 0)
            {
                reportItems.Add(new BalanceSheetPrintItem("STK", "Closing Stock", closingStockVal, "Asset"));
            }

            // 4. Net Profit / Loss
            // We need to calculate Net Profit/Loss to balance the sheet
            // Ideally we should call GetProfitAndLossReport, but to avoid circular dependencies or overhead,
            // let's create a simplified P&L calculation here or just fetch income/expense totals.

            // Re-query for Income/Expense to get Net Profit
            // reusing 'query' but just changing where clause criteria conceptually
            // Actually 'data' already has everything if we didn't filter by Date in the memory... 
            // WAIT! Balance sheet TblAccounting query was `<= ToDate`. P&L is `From ... To`.
            // Net Profit must be for the period? 
            // Usually Balance Sheet is accumulated. But Profit is usually for the financial year.
            // For now, let's assume the user wants P&L for the same period as requested (or from beginning if Start Date is not provided commonly for BS).
            // Request has FromDate. Let's use it for P&L calculation.

            var pnlRequest = new ProfitAndLossReportRequest(request.BranchId, request.FromDate, request.ToDate);
            // We can call the local method:
            var pnlReport = await GetProfitAndLossReport(pnlRequest);
            decimal netProfit = pnlReport.NetProfit;


            // 5. Totals
            var totalAssets = reportItems.Where(x => x.Category == "Asset").Sum(x => x.Amount);
            var totalLiabilities = reportItems.Where(x => x.Category == "Liability").Sum(x => x.Amount);
            var totalCapital = reportItems.Where(x => x.Category == "Capital").Sum(x => x.Amount);

            var liabilitiesSideTotal = totalLiabilities + totalCapital + netProfit;
            var accountBalanceDifference = totalAssets - liabilitiesSideTotal;

            // Add Net Profit to Capital/Liabilities side equivalent
            // If Profit -> Add to Capital (Liability side)
            // If Loss -> Deduct from Capital (Asset side usually or negative liability)

            // We will pass it separate in response, UI will decide where to show.

            return new BalanceSheetReportResponse(
                reportItems,
                totalLiabilities + totalCapital,
                totalAssets,
                netProfit,
                closingStockVal,
                accountBalanceDifference
            );
        }
        catch (Exception ex)
        {
            throw new Exception("Error generating Balance Sheet Report", ex);
        }
    }
    public async Task<ChartOfAccountReportResponse> GetChartOfAccountReport(ChartOfAccountReportRequest request)
    {
        try
        {
            await using var db = contextProvider.CreateDbContext();
            var accountGroupId = request.AccountGroupId == 0 ? null : request.AccountGroupId;

            var query = from ac in db.AccountMasters
                        join ag in db.AccountGroups on ac.FK_AcGroupId equals ag.AccountGroupId
                        where (accountGroupId == null || ac.FK_AcGroupId == accountGroupId)
                        select new
                        {
                            AccountGroupName = ag.Name,
                            AccountCode = ac.Code,
                            AccountName = ac.Name
                        };

            var result = await query.ToListAsync();

            var reportItems = result.Select((x, index) => new ChartOfAccountReportDtls(
                index + 1,
                x.AccountGroupName,
                x.AccountCode,
                x.AccountName
            )).ToList();

            return new ChartOfAccountReportResponse(reportItems);
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<LedgerReportResponse> GetLedgerReport(LedgerReportRequest request)
    {
        try
        {
            await using var db = contextProvider.CreateDbContext();
            var branchId = request.BranchId == 0 ? null : request.BranchId;
            var accountId = request.AccountId == 0 ? null : request.AccountId;

            if (accountId == null)
            {
                return new LedgerReportResponse(new List<LedgerReportDtls>(), 0, 0, "", "");
            }

            var fromDate = request.FromDate ?? DateTime.MinValue;
            var toDate = request.ToDate ?? DateTime.MaxValue;

            // Opening Balance
            decimal openingBalance = await db.TblAccountings
                .Where(x => (branchId == null || x.FK_BranchId == branchId) &&
                            x.FK_AccountId == accountId &&
                            x.EntryDate < fromDate)
                .SumAsync(x => x.Amount ?? 0);

            // Transactions
            var transactions = await db.TblAccountings
                .Where(x => (branchId == null || x.FK_BranchId == branchId) &&
                            x.FK_AccountId == accountId &&
                            x.EntryDate >= fromDate.Date &&
                            x.EntryDate < toDate.Date.AddDays(1))
                .OrderBy(x => x.EntryDate)
                .ThenBy(x => x.VoucherNo)
                .Select(x => new
                {
                    x.EntryDate,
                    x.VoucherId,
                    x.VoucherNo,
                    x.Type,
                    x.Narration,
                    x.Amount
                })
                .ToListAsync();

            var reportItems = new List<LedgerReportDtls>();
            decimal runningBalance = openingBalance;

            // Add Opening Balance Row
            if (openingBalance != 0)
            {
                reportItems.Add(new LedgerReportDtls(
                   1,
                   null,
                   fromDate.ToString("dd/MM/yyyy"),
                   "",
                   "Opening Balance",
                   "",
                   openingBalance > 0 ? openingBalance : 0,
                   openingBalance < 0 ? Math.Abs(openingBalance) : 0,
                   runningBalance > 0 ? $"{runningBalance} Dr" : $"{Math.Abs(runningBalance)} Cr"
               ));
            }
            // If opening balance is 0 and no transactions, list is empty? Or start with 0 balance?
            // If transactions exist but opening is 0, we can skip opening row or show it as 0. 
            // Usually if 0, we don't need to show it unless desired. I'll stick to if != 0.

            int sl = reportItems.Count + 1;
            foreach (var trans in transactions)
            {
                runningBalance += (trans.Amount ?? 0);

                decimal debit = 0;
                decimal credit = 0;

                if (trans.Amount > 0) debit = trans.Amount ?? 0;
                else credit = Math.Abs(trans.Amount ?? 0);

                reportItems.Add(new LedgerReportDtls(
                    sl++,
                    trans.VoucherId,
                    trans.EntryDate?.ToString("dd/MM/yyyy"),
                    trans.VoucherNo,
                    trans.Type,
                    trans.Narration,
                    debit,
                    credit,
                    runningBalance > 0 ? $"{runningBalance} Dr" : $"{Math.Abs(runningBalance)} Cr"
                ));
            }
            var periodDebit = reportItems.Sum(x => x.Debit);
            var periodCredit = reportItems.Sum(x => x.Credit);
            reportItems.Add(new LedgerReportDtls(
                null,
                null,
                "",              // Date
                "",              // VoucherNo
                "TOTAL",         // VoucherType (used as label)
                "",              // Narration
                periodDebit,     // Debit
                periodCredit,    // Credit
                ""             // Balance (not needed for total row)
            ));
            return new LedgerReportResponse(reportItems, periodDebit, periodCredit, runningBalance > 0 ? $"{runningBalance} Dr" : $"{Math.Abs(runningBalance)} Cr", openingBalance > 0 ? $"{openingBalance} Dr" : $"{Math.Abs(openingBalance)} Cr");
        }
        catch (Exception)
        {
            throw;
        }
    }

    private async Task<decimal> GetStockValueAsync(TenantDBContext db, int? branchId, DateTime date)
    {
        // 1. Get all Inward transactions up to date (Opening Stock, Purchases, etc.)
        // We use this to calculate the historical Weighted Average Cost
        var inwardLogs = await db.StockTranLogs
            .Where(x => (branchId == null || x.FK_BranchId == branchId) &&
                        x.VchrDate < date.Date.AddDays(1) &&
                        (x.InQuantity ?? 0) > 0)
            .Select(x => new { x.FK_ItemId, x.InQuantity, Rate = x.ActualRate ?? x.InRate ?? 0 })
            .ToListAsync();

        if (!inwardLogs.Any()) return 0;

        // 2. Calculate WAC per Item
        var wacPerItem = inwardLogs
            .GroupBy(x => x.FK_ItemId)
            .Select(g => new
            {
                ItemId = g.Key,
                WAC = g.Sum(x => (x.InQuantity ?? 0) * x.Rate) / (g.Sum(x => x.InQuantity ?? 0) == 0 ? 1 : g.Sum(x => x.InQuantity ?? 0))
            })
            .ToDictionary(x => x.ItemId, x => x.WAC);

        // 3. Get Net Quantities per Item up to that date
        var netQuantities = await db.StockTranLogs
            .Where(x => (branchId == null || x.FK_BranchId == branchId) &&
                        x.VchrDate < date.Date.AddDays(1))
            .GroupBy(x => x.FK_ItemId)
            .Select(g => new
            {
                ItemId = g.Key,
                Qty = g.Sum(x => (x.InQuantity ?? 0) - (x.OutQuantity ?? 0))
            })
            .ToListAsync();

        // 4. Calculate Total Value
        decimal totalValue = 0;
        foreach (var item in netQuantities)
        {
            if (wacPerItem.TryGetValue(item.ItemId, out decimal wac))
            {
                totalValue += item.Qty * wac;
            }
        }

        return totalValue;
    }
}
