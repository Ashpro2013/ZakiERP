namespace ZakiSAASERP.Repo.Reports;

public class SalesReportRepo(IDbContextProvider contextProvider) : ISalesReportRepo
{
    public async Task<SalesReportResponse> GetSalesReportAsync(SalesReportRequest req)
    {
        await using var db = contextProvider.CreateDbContext();
        var salesQuery = db.SalesInvoiceHdrs
            .Where(x => x.SalesInvoiceDate >= req.FromDate.Date && x.SalesInvoiceDate <= req.ToDate.Date.AddDays(1).AddTicks(-1))
            .AsQueryable();
        var returnQuery = db.SalesInvoiceRtnHdrs
            .Where(x => x.SalesInvoiceRtnDate >= req.FromDate.Date && x.SalesInvoiceRtnDate <= req.ToDate.Date.AddDays(1).AddTicks(-1))
            .AsQueryable();
        // Filter by Branch
        if (req.BranchId > 0)
        {
            salesQuery = salesQuery.Where(x => x.FK_BranchId == req.BranchId);
            returnQuery = returnQuery.Where(x => x.FK_BranchId == req.BranchId);
        }
        // Filter by Customer
        if (req.CustomerId.HasValue && req.CustomerId > 0)
        {
            salesQuery = salesQuery.Where(x => x.FK_CustomerId == req.CustomerId);
            returnQuery = returnQuery.Where(x => x.FK_CustomerId == req.CustomerId);
        }
        // Filter by Salesman
        if (req.SalesmanId.HasValue && req.SalesmanId > 0)
        {
            salesQuery = salesQuery.Where(x => x.FK_SalesManId == req.SalesmanId);
            returnQuery = returnQuery.Where(x => x.FK_SalesManId == req.SalesmanId);
        }
        // Filter by BillType
        // Note: If grouping is enabled, BillType is usually "All", but if specific is selected, we still respect it.
        if (!string.IsNullOrEmpty(req.BillType) && req.BillType != "All")
        {
            salesQuery = salesQuery.Where(x => x.BillType == req.BillType);
            // Returns excluded if specific bill type selected (as per previous logic)
            returnQuery = returnQuery.Where(x => 1 == 0);
        }
        var salesList = await salesQuery.Select(x => new SalesReportDtls(
            0, // SL
            x.SalesInvoiceId,
            x.SalesInvoiceNo,
            x.SalesInvoiceDate.HasValue ? x.SalesInvoiceDate.Value.ToString("dd/MM/yyyy") : "",
            x.Customer != null ? x.Customer.Name : "",
            x.SalesMan != null ? x.SalesMan.Name : "",
            x.BillType ?? "Cash",
            "Sales",
            x.GrossAmount,
            x.LineDsicount, // Discount
            x.TaxableAmount,
            x.TaxTotal,
            x.GrandTotal
        )).ToListAsync();
        var returnList = new List<SalesReportDtls>();
        if ((req.InvoiceType == "All" || req.InvoiceType == "Sales Return") && (string.IsNullOrEmpty(req.BillType) || req.BillType == "All"))
        {
            returnList = await returnQuery.Select(x => new SalesReportDtls(
                0, // SL
                x.SalesInvoiceRtnId,
                x.SalesInvoiceRtnNo,
                x.SalesInvoiceRtnDate.HasValue ? x.SalesInvoiceRtnDate.Value.ToString("dd/MM/yyyy") : "",
                x.Customer != null ? x.Customer.Name : "",
                x.SalesMan != null ? x.SalesMan.Name : "",
                db.SalesInvoiceHdrs.Where(s => s.SalesInvoiceId == x.SalesInvoiceId).Select(s => s.BillType).FirstOrDefault() ?? "Cash",
                "Return",
                x.GrossAmount,
                x.LineDsicount, // Discount
                x.TaxableAmount,
                x.TaxTotal,
                x.GrandTotal
            )).ToListAsync();
        }
        var allItems = new List<SalesReportDtls>();
        if (req.InvoiceType == "All" || req.InvoiceType == "Sales")
        {
            allItems.AddRange(salesList);
        }
        if (req.InvoiceType == "All" || req.InvoiceType == "Sales Return")
        {
            allItems.AddRange(returnList);
        }
        allItems = allItems.OrderBy(x =>
        {
            if (x.InvType == "Return" || x.InvType == "Sales Return") return "Returns";
            return string.IsNullOrWhiteSpace(x.BillType) ? "Others" : x.BillType;
        })
        .ThenBy(x =>
        {
            DateTime.TryParseExact(x.Date, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime dt);
            return dt;
        }).ToList();
        int sl = 1;
        var finalReports = allItems.Select(x => x with { SL = sl++ }).ToList();
        var totalNet = allItems.Sum(x => x.InvType == "Return" ? -(x.NetAmount ?? 0) : (x.NetAmount ?? 0));
        var totalVat = allItems.Sum(x => x.InvType == "Return" ? -(x.VatAmount ?? 0) : (x.VatAmount ?? 0));
        var totalAmt = allItems.Sum(x => x.InvType == "Return" ? -(x.Amount ?? 0) : (x.Amount ?? 0));
        var totalGross = allItems.Sum(x => x.InvType == "Return" ? -(x.GrossAmount ?? 0) : (x.GrossAmount ?? 0));
        var totalDiscount = allItems.Sum(x => x.InvType == "Return" ? -(x.DiscountAmount ?? 0) : (x.DiscountAmount ?? 0));
        finalReports.Add(new SalesReportDtls(
            null, // SL
            null, // Id
            "",   // InvoiceNo
            "",   // Date
            "TOTAL", // CustomerName
            "",   // SalesmanName
            "",   // BillType
            "",   // InvType
            totalGross,
            totalDiscount,
            totalAmt,
            totalVat,
            totalNet
        ));
        return new SalesReportResponse(finalReports, totalGross, totalDiscount, totalAmt, totalVat, totalNet);
    }

    public async Task<SalesReturnReportResponse> GetSalesReturnReportAsync(SalesReturnReportRequest req)
    {
        await using var db = contextProvider.CreateDbContext();
        var returnQuery = db.SalesInvoiceRtnHdrs
            .Where(x => x.SalesInvoiceRtnDate >= req.FromDate.Date && x.SalesInvoiceRtnDate <= req.ToDate.Date.AddDays(1).AddTicks(-1))
            .AsQueryable();

        // Filter by Branch
        if (req.BranchId > 0)
        {
            returnQuery = returnQuery.Where(x => x.FK_BranchId == req.BranchId);
        }
        // Filter by Customer
        if (req.CustomerId.HasValue && req.CustomerId > 0)
        {
            returnQuery = returnQuery.Where(x => x.FK_CustomerId == req.CustomerId);
        }
        // Filter by Salesman
        if (req.SalesmanId.HasValue && req.SalesmanId > 0)
        {
            returnQuery = returnQuery.Where(x => x.FK_SalesManId == req.SalesmanId);
        }

        var returnList = await returnQuery.Select(x => new SalesReturnReportDtls(
            0, // SL
            x.SalesInvoiceRtnId,
            x.SalesInvoiceRtnNo,
            x.SalesInvoiceRtnDate.HasValue ? x.SalesInvoiceRtnDate.Value.ToString("dd/MM/yyyy") : "",
            x.Customer != null ? x.Customer.Name : "",
            x.SalesMan != null ? x.SalesMan.Name : "",
            db.SalesInvoiceHdrs.Where(s => s.SalesInvoiceId == x.SalesInvoiceId).Select(s => s.BillType).FirstOrDefault() ?? "Cash",
            "Return",
            x.GrossAmount,
            x.LineDsicount, // Discount
            x.TaxableAmount,
            x.TaxTotal,
            x.GrandTotal
        )).ToListAsync();

        int sl = 1;
        var finalReports = returnList.Select(x => x with { SL = sl++ }).ToList();

        var totalNet = returnList.Sum(x => x.NetAmount ?? 0);
        var totalVat = returnList.Sum(x => x.VatAmount ?? 0);
        var totalAmt = returnList.Sum(x => x.Amount ?? 0);
        var totalGross = returnList.Sum(x => x.GrossAmount ?? 0);
        var totalDiscount = returnList.Sum(x => x.DiscountAmount ?? 0);

        finalReports.Add(new SalesReturnReportDtls(
            null, // SL
            null, // Id
            "",   // InvoiceNo
            "",   // Date
            "TOTAL", // CustomerName
            "",   // SalesmanName
            "",   // BillType
            "",   // InvType
            totalGross,
            totalDiscount,
            totalAmt,
            totalVat,
            totalNet
        ));

        return new SalesReturnReportResponse(finalReports, totalGross, totalDiscount, totalAmt, totalVat, totalNet);
    }
    public async Task<SalesMonitorResponse> GetSalesMonitorReportAsync(SalesMonitorRequest req)
    {
        await using var db = contextProvider.CreateDbContext();
        // 1. Base Query
        var salesQuery = db.SalesInvoiceHdrs
            .Where(x => x.SalesInvoiceDate >= req.FromDate.Date && x.SalesInvoiceDate <= req.ToDate.Date.AddDays(1).AddTicks(-1) && !x.IsDeleted)
            .AsQueryable();

        if (req.BranchId > 0)
            salesQuery = salesQuery.Where(x => x.FK_BranchId == req.BranchId);

        if (req.CustomerId.HasValue && req.CustomerId > 0)
            salesQuery = salesQuery.Where(x => x.FK_CustomerId == req.CustomerId);

        if (req.SalesmanId.HasValue && req.SalesmanId > 0)
            salesQuery = salesQuery.Where(x => x.FK_SalesManId == req.SalesmanId);

        if (!string.IsNullOrEmpty(req.BillType) && req.BillType != "All")
            salesQuery = salesQuery.Where(x => x.BillType == req.BillType);

        if (!string.IsNullOrEmpty(req.SearchTerm))
        {
            salesQuery = salesQuery.Where(x =>
                (x.SalesInvoiceNo != null && x.SalesInvoiceNo.Contains(req.SearchTerm)) ||
                (x.Customer != null && x.Customer.Name.Contains(req.SearchTerm)) ||
                (x.SalesMan != null && x.SalesMan.Name.Contains(req.SearchTerm))
            );
        }

        // 2. Project with Balance for Status Filtering
        // We use a projection to calculate balance on the server side to enable filtering
        var projectedQuery = salesQuery.Select(x => new
        {
            Hdr = x,
            Received = x.BillType == "Credit"
                       ? (db.CustomerReceiptDtls.Where(r => r.FK_InvoiceId == x.SalesInvoiceId).Sum(r => (decimal?)r.PaymentAmount) ?? 0)
                       : x.GrandTotal,
            Discount = x.BillType == "Credit"
                       ? (db.CustomerReceiptDtls.Where(r => r.FK_InvoiceId == x.SalesInvoiceId).Sum(r => (decimal?)r.Discount) ?? 0)
                       : 0,
            Return = db.SalesInvoiceRtnHdrs.Where(r => r.SalesInvoiceId == x.SalesInvoiceId && !r.IsDeleted).Sum(r => (decimal?)r.GrandTotal) ?? 0
        }).Select(x => new
        {
            x.Hdr,
            x.Received,
            x.Discount,
            x.Return,
            Balance = x.Hdr.BillType == "Credit"
                      ? (x.Hdr.GrandTotal - x.Received - x.Discount - x.Return)
                      : 0
        });

        // 3. Apply Status Filter
        if (req.Status == "Closed")
        {
            projectedQuery = projectedQuery.Where(x => x.Balance <= 0.01m);
        }
        else if (req.Status == "Pending")
        {
            projectedQuery = projectedQuery.Where(x => x.Balance > 0.01m);
        }

        // 4. Calculate Grand Totals (On the Full Filtered Set)
        var grandTotals = await projectedQuery
            .GroupBy(x => 1)
            .Select(g => new
            {
                TotalAmount = g.Sum(x => x.Hdr.GrandTotal),
                TotalReceived = g.Sum(x => x.Received),
                TotalReturn = g.Sum(x => x.Return),
                TotalDiscount = g.Sum(x => x.Discount),
                TotalBalance = g.Sum(x => x.Balance),
                Count = g.Count()
            })
            .FirstOrDefaultAsync();

        int totalCount = grandTotals?.Count ?? 0;
        decimal totalAmount = grandTotals?.TotalAmount ?? 0;
        decimal totalReceived = grandTotals?.TotalReceived ?? 0;
        decimal totalReturn = grandTotals?.TotalReturn ?? 0;
        decimal totalDiscount = grandTotals?.TotalDiscount ?? 0;
        decimal totalBalance = grandTotals?.TotalBalance ?? 0;

        // 5. No Pagination - Return All
        var pagedQuery = projectedQuery.OrderByDescending(x => x.Hdr.SalesInvoiceDate);

        var pagedData = await pagedQuery
            .Select(x => new
            {
                x.Hdr,
                CustomerName = x.Hdr.Customer != null ? x.Hdr.Customer.Name : "",
                SalesmanName = x.Hdr.SalesMan != null ? x.Hdr.SalesMan.Name : "",
                x.Received,
                x.Discount,
                x.Return,
                x.Balance
            })
            .ToListAsync<dynamic>();

        // 6. Map to DTO
        var reportItems = new List<SalesMonitorDtls>();
        int sl = 1;

        foreach (var item in pagedData)
        {
            // dynamic item access
            var hdr = item.Hdr; // EF Core dynamic proxy or object
            // Just casts
            var sale = (ZakiSAASERP.Models.Transactions.Sales.SalesInvoiceHdr)item.Hdr;

            reportItems.Add(new SalesMonitorDtls(
                sl++,
                sale.SalesInvoiceId,
                sale.SalesInvoiceNo,
                sale.SalesInvoiceDate.HasValue ? sale.SalesInvoiceDate.Value.ToString("dd/MM/yyyy") : "",
                item.CustomerName,
                item.SalesmanName,
                sale.BillType ?? "Cash",
                sale.GrandTotal,
                item.Received,
                item.Discount,
                item.Return,
                item.Balance,
                item.Balance <= 0.01m ? "Closed" : "Pending"
            ));
        }

        return new SalesMonitorResponse(reportItems, totalAmount, totalReceived, totalDiscount, totalReturn, totalBalance, totalCount);
    }
    public async Task<List<SalesMonitorReceiptItem>> GetReceiptsByInvoiceAsync(int invoiceId)
    {
        await using var db = contextProvider.CreateDbContext();
        var list = await db.CustomerReceiptDtls
            .Include(x => x.Receipt)
            .Where(x => x.FK_InvoiceId == invoiceId)
            .Select(x => new SalesMonitorReceiptItem(
                x.Receipt != null ? x.Receipt.ReceiptNo : "",
                x.FK_CustomerReceiptId,
                x.Receipt != null ? x.Receipt.ReceiptDate.ToString("dd/MM/yyyy") : "",
                x.PaymentAmount,
                x.Discount
            ))
            .ToListAsync();

        return list;
    }
    public async Task<CustomerWiseSalesReportResponse> GetCustomerWiseSalesReportAsync(CustomerWiseSalesReportRequest req)
    {
        await using var db = contextProvider.CreateDbContext();
        var salesQuery = db.SalesInvoiceHdrs
            .Where(x => x.SalesInvoiceDate >= req.FromDate.Date && x.SalesInvoiceDate <= req.ToDate.Date.AddDays(1).AddTicks(-1))
            .AsQueryable();
        var returnQuery = db.SalesInvoiceRtnHdrs
            .Where(x => x.SalesInvoiceRtnDate >= req.FromDate.Date && x.SalesInvoiceRtnDate <= req.ToDate.Date.AddDays(1).AddTicks(-1))
            .AsQueryable();
        if (req.BranchId > 0)
        {
            salesQuery = salesQuery.Where(x => x.FK_BranchId == req.BranchId);
            returnQuery = returnQuery.Where(x => x.FK_BranchId == req.BranchId);
        }
        if (req.CustomerId.HasValue && req.CustomerId > 0)
        {
            salesQuery = salesQuery.Where(x => x.FK_CustomerId == req.CustomerId);
            returnQuery = returnQuery.Where(x => x.FK_CustomerId == req.CustomerId);
        }
        if (req.SalesmanId.HasValue && req.SalesmanId > 0)
        {
            salesQuery = salesQuery.Where(x => x.FK_SalesManId == req.SalesmanId);
            returnQuery = returnQuery.Where(x => x.FK_SalesManId == req.SalesmanId);
        }
        if (!string.IsNullOrEmpty(req.BillType) && req.BillType != "All")
        {
            salesQuery = salesQuery.Where(x => x.BillType == req.BillType);
            returnQuery = returnQuery.Where(x => 1 == 0);
        }
        var salesList = await salesQuery.Select(x => new CustomerWiseSalesReportDtls(
            0, // SL
            x.SalesInvoiceId,
            x.SalesInvoiceNo,
            x.SalesInvoiceDate.HasValue ? x.SalesInvoiceDate.Value.ToString("dd/MM/yyyy") : "",
            x.Customer != null ? x.Customer.Name : "",
            x.SalesMan != null ? x.SalesMan.Name : "",
            x.BillType ?? "Cash",
            "Sales",
            x.GrossAmount,
            x.LineDsicount, // Discount
            x.TaxableAmount,
            x.TaxTotal,
            x.GrandTotal
        )).ToListAsync();
        var returnList = new List<CustomerWiseSalesReportDtls>();
        if ((req.InvoiceType == "All" || req.InvoiceType == "Sales Return") && (string.IsNullOrEmpty(req.BillType) || req.BillType == "All"))
        {
            returnList = await returnQuery.Select(x => new CustomerWiseSalesReportDtls(
                0, // SL
                x.SalesInvoiceRtnId,
                x.SalesInvoiceRtnNo,
                x.SalesInvoiceRtnDate.HasValue ? x.SalesInvoiceRtnDate.Value.ToString("dd/MM/yyyy") : "",
                x.Customer != null ? x.Customer.Name : "",
                x.SalesMan != null ? x.SalesMan.Name : "",
                db.SalesInvoiceHdrs.Where(s => s.SalesInvoiceId == x.SalesInvoiceId).Select(s => s.BillType).FirstOrDefault() ?? "Cash",
                "Return",
                x.GrossAmount,
                x.LineDsicount, // Discount
                x.TaxableAmount,
                x.TaxTotal,
                x.GrandTotal
            )).ToListAsync();
        }
        var allItems = new List<CustomerWiseSalesReportDtls>();
        if (req.InvoiceType == "All" || req.InvoiceType == "Sales")
        {
            allItems.AddRange(salesList);
        }
        if (req.InvoiceType == "All" || req.InvoiceType == "Sales Return")
        {
            allItems.AddRange(returnList);
        }
        allItems = allItems.OrderBy(x => x.CustomerName)
        .ThenBy(x =>
        {
            DateTime.TryParseExact(x.Date, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime dt);
            return dt;
        }).ToList();
        int sl = 1;
        var finalReports = allItems.Select(x => x with { SL = sl++ }).ToList();
        var totalNet = allItems.Sum(x => x.InvType == "Return" ? -(x.NetAmount ?? 0) : (x.NetAmount ?? 0));
        var totalVat = allItems.Sum(x => x.InvType == "Return" ? -(x.VatAmount ?? 0) : (x.VatAmount ?? 0));
        var totalAmt = allItems.Sum(x => x.InvType == "Return" ? -(x.Amount ?? 0) : (x.Amount ?? 0));
        var totalGross = allItems.Sum(x => x.InvType == "Return" ? -(x.GrossAmount ?? 0) : (x.GrossAmount ?? 0));
        var totalDiscount = allItems.Sum(x => x.InvType == "Return" ? -(x.DiscountAmount ?? 0) : (x.DiscountAmount ?? 0));
        finalReports.Add(new CustomerWiseSalesReportDtls(
            null, // SL
            null, // Id
            "",   // InvoiceNo
            "",   // Date
            "TOTAL", // CustomerName
            "",   // SalesmanName
            "",   // BillType
            "",   // InvType
            totalGross,
            totalDiscount,
            totalAmt,
            totalVat,
            totalNet
        ));
        return new CustomerWiseSalesReportResponse(finalReports, totalVat, totalNet);
    }
    public async Task<SalesDetailsReportResponse> GetSalesDetailsAsync(SalesDetailsReportRequest req)
    {
        await using var db = contextProvider.CreateDbContext();
        var salesQuery = from d in db.SalesInvoiceDtls
                         join h in db.SalesInvoiceHdrs on d.FK_SalesInvoiceHdrId equals h.SalesInvoiceId
                         where h.SalesInvoiceDate >= req.FromDate && h.SalesInvoiceDate <= req.ToDate
                         select new { Hdr = h, Dtl = d, Type = "Sales" };
        var returnQuery = from d in db.SalesInvoiceRtnDtls
                          join h in db.SalesInvoiceRtnHdrs on d.FK_SalesInvoiceRtnHdrId equals h.SalesInvoiceRtnId
                          where h.SalesInvoiceRtnDate >= req.FromDate && h.SalesInvoiceRtnDate <= req.ToDate
                          select new { Hdr = (dynamic)h, Dtl = (dynamic)d, Type = "Return" };
        var salesQ = db.SalesInvoiceDtls
            .Include(x => x.SalesInvoiceHdr)
            .Include(x => x.SalesInvoiceHdr.Customer)
            .Include(x => x.SalesInvoiceHdr.SalesMan)
            .Include(x => x.SalesInvoiceHdr.User)
            .Include(x => x.Unit)
            .Where(x => x.SalesInvoiceHdr.SalesInvoiceDate >= req.FromDate.Date && x.SalesInvoiceHdr.SalesInvoiceDate <= req.ToDate.Date.AddDays(1).AddTicks(-1))
            .AsQueryable();
        var returnQ = db.SalesInvoiceRtnDtls
            .Include(x => x.SalesInvoiceRtnHdr)
            .Include(x => x.SalesInvoiceRtnHdr.Customer)
            .Include(x => x.SalesInvoiceRtnHdr.SalesMan)
            .Include(x => x.SalesInvoiceRtnHdr.User)
            .Include(x => x.Unit)
            .Where(x => x.SalesInvoiceRtnHdr.SalesInvoiceRtnDate >= req.FromDate.Date && x.SalesInvoiceRtnHdr.SalesInvoiceRtnDate <= req.ToDate.Date.AddDays(1).AddTicks(-1))
            .AsQueryable();
        if (req.BranchId > 0)
        {
            salesQ = salesQ.Where(x => x.SalesInvoiceHdr.FK_BranchId == req.BranchId);
            returnQ = returnQ.Where(x => x.SalesInvoiceRtnHdr.FK_BranchId == req.BranchId);
        }
        if (req.CustomerId.HasValue && req.CustomerId > 0)
        {
            salesQ = salesQ.Where(x => x.SalesInvoiceHdr.FK_CustomerId == req.CustomerId);
            returnQ = returnQ.Where(x => x.SalesInvoiceRtnHdr.FK_CustomerId == req.CustomerId);
        }
        if (req.SalesmanId.HasValue && req.SalesmanId > 0)
        {
            salesQ = salesQ.Where(x => x.SalesInvoiceHdr.FK_SalesManId == req.SalesmanId);
            returnQ = returnQ.Where(x => x.SalesInvoiceRtnHdr.FK_SalesManId == req.SalesmanId);
        }
        if (!string.IsNullOrEmpty(req.BillType) && req.BillType != "All")
        {
            salesQ = salesQ.Where(x => x.SalesInvoiceHdr.BillType == req.BillType);
            returnQ = returnQ.Where(x => 1 == 0);
        }
        if (!string.IsNullOrEmpty(req.ItemName))
        {
            salesQ = salesQ.Where(x => x.ItemName.Contains(req.ItemName));
            returnQ = returnQ.Where(x => x.ItemName.Contains(req.ItemName));
        }
        var salesListObj = await salesQ.Join(db.ItemMasters, d => d.FK_ItemId, i => i.ItemId, (d, i) => new { d, i.ItemCode }).ToListAsync();
        var returnListObj = await returnQ.Join(db.ItemMasters, d => d.FK_ItemId, i => i.ItemId, (d, i) => new { d, i.ItemCode }).ToListAsync();
        var allItems = new List<SalesDetailsReportDtls>();
        if (req.InvoiceType == "All" || req.InvoiceType == "Sales")
        {
            var sItems = salesListObj.Select(x => new SalesDetailsReportDtls(
                0, // SL
                x.d.SalesInvoiceHdr.SalesInvoiceId,
                x.d.SalesInvoiceHdr.SalesInvoiceNo,
                x.d.SalesInvoiceHdr.SalesInvoiceDate.HasValue ? x.d.SalesInvoiceHdr.SalesInvoiceDate.Value.ToString("dd/MM/yyyy") : "",
                x.d.SalesInvoiceHdr.Customer != null ? x.d.SalesInvoiceHdr.Customer.Name : "",
                x.d.SalesInvoiceHdr.SalesMan != null ? x.d.SalesInvoiceHdr.SalesMan.Name : "",
                x.ItemCode,
                x.d.ItemName,
                x.d.Unit != null ? x.d.Unit.Name : "",
                x.d.Quantity,
                x.d.InRate, // Rate
                x.d.ExRate * x.d.Quantity,
                x.d.GrossAmount,
                x.d.DiscountAmnt,
                x.d.TaxAmount,
                x.d.LineTotal,
                x.d.SalesInvoiceHdr.User != null ? x.d.SalesInvoiceHdr.User.UserName : "",
                "Sales",
                x.d.ExRate
            ));
            allItems.AddRange(sItems);
        }
        if (req.InvoiceType == "All" || req.InvoiceType == "Sales Return")
        {
            var rItems = returnListObj.Select(x => new SalesDetailsReportDtls(
               0, // SL
               x.d.SalesInvoiceRtnHdr.SalesInvoiceRtnId,
               x.d.SalesInvoiceRtnHdr.SalesInvoiceRtnNo,
               x.d.SalesInvoiceRtnHdr.SalesInvoiceRtnDate.HasValue ? x.d.SalesInvoiceRtnHdr.SalesInvoiceRtnDate.Value.ToString("dd/MM/yyyy") : "",
               x.d.SalesInvoiceRtnHdr.Customer != null ? x.d.SalesInvoiceRtnHdr.Customer.Name : "",
               x.d.SalesInvoiceRtnHdr.SalesMan != null ? x.d.SalesInvoiceRtnHdr.SalesMan.Name : "",
               x.ItemCode,
               x.d.ItemName,
               x.d.Unit != null ? x.d.Unit.Name : "",
               x.d.Quantity,
               x.d.InRate,
               x.d.ExRate * x.d.Quantity,
               x.d.GrossAmount,
               x.d.DiscountAmnt,
               x.d.TaxAmount,
               x.d.LineTotal,
               x.d.SalesInvoiceRtnHdr.User != null ? x.d.SalesInvoiceRtnHdr.User.UserName : "",
               "Return",
               x.d.ExRate
           )).ToList();
            allItems.AddRange(rItems);
        }
        allItems = allItems.OrderBy(x =>
        {
            DateTime.TryParseExact(x.Date, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime dt);
            return dt;
        }).ToList();
        int sl = 1;
        var finalReports = allItems.Select(x => x with { SL = sl++ }).ToList();
        var totalGross = allItems.Sum(x => x.InvType == "Return" ? -(x.GrossAmount ?? 0) : (x.GrossAmount ?? 0));
        var totalDiscount = allItems.Sum(x => x.InvType == "Return" ? -(x.DiscountAmount ?? 0) : (x.DiscountAmount ?? 0));
        var totalTax = allItems.Sum(x => x.InvType == "Return" ? -(x.TaxAmount ?? 0) : (x.TaxAmount ?? 0));
        var totalAmount = allItems.Sum(x => x.InvType == "Return" ? -(x.TotalAmount ?? 0) : (x.TotalAmount ?? 0));
        return new SalesDetailsReportResponse(finalReports, totalGross, totalDiscount, totalTax, totalAmount);
    }

    public async Task<SalesReturnDetailsReportResponse> GetSalesReturnDetailsAsync(SalesReturnDetailsReportRequest req)
    {
        await using var db = contextProvider.CreateDbContext();

        var returnQ = db.SalesInvoiceRtnDtls
            .Include(x => x.SalesInvoiceRtnHdr)
            .Include(x => x.SalesInvoiceRtnHdr.Customer)
            .Include(x => x.SalesInvoiceRtnHdr.SalesMan)
            .Include(x => x.SalesInvoiceRtnHdr.User)
            .Include(x => x.Unit)
            .Where(x => x.SalesInvoiceRtnHdr.SalesInvoiceRtnDate >= req.FromDate.Date && x.SalesInvoiceRtnHdr.SalesInvoiceRtnDate <= req.ToDate.Date.AddDays(1).AddTicks(-1))
            .AsQueryable();

        if (req.BranchId > 0)
        {
            returnQ = returnQ.Where(x => x.SalesInvoiceRtnHdr.FK_BranchId == req.BranchId);
        }
        if (req.CustomerId.HasValue && req.CustomerId > 0)
        {
            returnQ = returnQ.Where(x => x.SalesInvoiceRtnHdr.FK_CustomerId == req.CustomerId);
        }
        if (req.SalesmanId.HasValue && req.SalesmanId > 0)
        {
            returnQ = returnQ.Where(x => x.SalesInvoiceRtnHdr.FK_SalesManId == req.SalesmanId);
        }
        if (!string.IsNullOrEmpty(req.ItemName))
        {
            returnQ = returnQ.Where(x => x.ItemName.Contains(req.ItemName));
        }

        var returnListObj = await returnQ.Join(db.ItemMasters, d => d.FK_ItemId, i => i.ItemId, (d, i) => new { d, i.ItemCode }).ToListAsync();

        var rItems = returnListObj.Select(x => new SalesReturnDetailsReportDtls(
               0, // SL
               x.d.SalesInvoiceRtnHdr.SalesInvoiceRtnId,
               x.d.SalesInvoiceRtnHdr.SalesInvoiceRtnNo,
               x.d.SalesInvoiceRtnHdr.SalesInvoiceRtnDate.HasValue ? x.d.SalesInvoiceRtnHdr.SalesInvoiceRtnDate.Value.ToString("dd/MM/yyyy") : "",
               x.d.SalesInvoiceRtnHdr.Customer != null ? x.d.SalesInvoiceRtnHdr.Customer.Name : "",
               x.d.SalesInvoiceRtnHdr.SalesMan != null ? x.d.SalesInvoiceRtnHdr.SalesMan.Name : "",
               x.ItemCode,
               x.d.ItemName,
               x.d.Unit != null ? x.d.Unit.Name : "",
               x.d.Quantity,
               x.d.InRate,
               x.d.ExRate * x.d.Quantity,
               x.d.GrossAmount,
               x.d.DiscountAmnt,
               x.d.TaxAmount,
               x.d.LineTotal,
               x.d.SalesInvoiceRtnHdr.User != null ? x.d.SalesInvoiceRtnHdr.User.UserName : "",
               "Return",
               x.d.ExRate
           )).ToList();

        rItems = rItems.OrderBy(x =>
        {
            DateTime.TryParseExact(x.Date, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime dt);
            return dt;
        }).ToList();

        int sl = 1;
        var finalReports = rItems.Select(x => x with { SL = sl++ }).ToList();

        var totalGross = rItems.Sum(x => x.GrossAmount ?? 0);
        var totalDiscount = rItems.Sum(x => x.DiscountAmount ?? 0);
        var totalTax = rItems.Sum(x => x.TaxAmount ?? 0);
        var totalAmount = rItems.Sum(x => x.TotalAmount ?? 0);

        return new SalesReturnDetailsReportResponse(finalReports, totalGross, totalDiscount, totalTax, totalAmount);
    }
    public async Task<CategoryWiseSalesReportResponse> GetCategoryWiseSalesReportAsync(CategoryWiseSalesReportRequest req)
    {
        await using var db = contextProvider.CreateDbContext();
        int? branchId = req.BranchId == 0 ? null : req.BranchId;
        int? categoryId = req.CategoryId == 0 ? null : req.CategoryId;
        string billtype = req.BillType == "All" ? null : req.BillType;
        var sales = db.SalesInvoiceDtls
            .Include(x => x.SalesInvoiceHdr)
            .Include(x => x.Item)
            .Include(x => x.Item.Category)
            .Where(x =>
                (branchId == null || x.SalesInvoiceHdr.FK_BranchId == branchId) &&
                (categoryId == null || x.Item.FK_CategoryId == categoryId) &&
                (billtype == null || x.SalesInvoiceHdr.BillType == billtype) &&
                x.SalesInvoiceHdr.SalesInvoiceDate >= req.FromDate.Value.Date &&
                x.SalesInvoiceHdr.SalesInvoiceDate <= req.ToDate.Value.Date.AddDays(1).AddTicks(-1)
            )
            .Select(x => new
            {
                x.Item.FK_CategoryId,
                CategoryName = x.Item.Category.Name,
                Gross = x.GrossAmount,
                Discount = x.DiscountAmnt,
                Tax = x.TaxAmount,
                Net = x.LineTotal
            });
        var returns = db.SalesInvoiceRtnDtls
            .Include(x => x.SalesInvoiceRtnHdr)
            .Include(x => x.Item)
            .Include(x => x.Item.Category)
            .Where(x =>
                (branchId == null || x.SalesInvoiceRtnHdr.FK_BranchId == branchId) &&
                (categoryId == null || x.Item.FK_CategoryId == categoryId) &&
                (billtype == null || x.SalesInvoiceRtnHdr.BillType == billtype) &&
                x.SalesInvoiceRtnHdr.SalesInvoiceRtnDate >= req.FromDate.Value.Date &&
                x.SalesInvoiceRtnHdr.SalesInvoiceRtnDate <= req.ToDate.Value.Date.AddDays(1).AddTicks(-1)
            )
            .Select(x => new
            {
                x.Item.FK_CategoryId,
                CategoryName = x.Item.Category.Name,
                Gross = -(x.GrossAmount),
                Discount = -(x.DiscountAmnt),
                Tax = -(x.TaxAmount),
                Net = -(x.LineTotal)
            });
        var combined = await sales.Concat(returns).ToListAsync();
        var grouped = combined
            .GroupBy(x => new { x.FK_CategoryId, x.CategoryName })
            .Select(g => new CategoryWiseSalesReportDtls(
                0,
                g.Key.CategoryName,
                g.Sum(x => x.Gross),
                g.Sum(x => x.Discount),
                g.Sum(x => x.Gross - x.Discount), // Taxable
                g.Sum(x => x.Tax),
                g.Sum(x => x.Net)
            ))
            .OrderBy(x => x.CategoryName)
            .ToList();

        int sl = 1;
        var finalList = grouped.Select(x => x with { SL = sl++ }).ToList();
        var totalGross = finalList.Sum(x => x.GrossAmount ?? 0);
        var totalDiscount = finalList.Sum(x => x.DiscountAmount ?? 0);
        var totalTaxable = finalList.Sum(x => x.Taxable ?? 0);
        var totalVat = finalList.Sum(x => x.VatAmount ?? 0);
        var totalNet = finalList.Sum(x => x.NetAmount ?? 0);
        finalList.Add(new CategoryWiseSalesReportDtls(
            null,
            "TOTAL",
            totalGross,
            totalDiscount,
            totalTaxable,
            totalVat,
            totalNet
        ));
        return new CategoryWiseSalesReportResponse(finalList, totalGross, totalDiscount, totalTaxable, totalVat, totalNet);
    }
    public async Task<SalesQuotationRegisterResponse> GetSalesQuotationRegisterAsync(SalesQuotationRegisterRequest req)
    {
        await using var db = contextProvider.CreateDbContext();
        int? branchId = req.BranchId == 0 ? null : req.BranchId;
        var list = await db.SalesQuotationHdrs
            .Include(x => x.Customer)
            .Where(x =>
                (branchId == null || x.FK_BranchId == branchId) &&
                x.SalesQuotationDate >= req.FromDate.Value.Date &&
                x.SalesQuotationDate <= req.ToDate.Value.Date.AddDays(1).AddTicks(-1) &&
                !x.IsDeleted
            )
            .OrderBy(x => x.SalesQuotationDate)
            .Select(x => new SalesQuotationRegisterDtls(
                0, // SL to be generated
                x.SalesQuotationDate.HasValue ? x.SalesQuotationDate.Value.ToString("dd-MM-yyyy") : "",
                x.SalesQuotationNo,
                x.RefNo,
                x.Customer != null ? x.Customer.Name : x.CustomerName,
                x.Customer != null ? x.Customer.Address : "",
                x.Customer != null ? x.Customer.Telephone : "",
                x.TaxTotal,
                x.GrandTotal // Assuming GrandTotal is NetAmount requested
            ))
            .ToListAsync();
        int sl = 1;
        var finalList = list.Select(x => x with { SL = sl++ }).ToList();
        var totalVat = finalList.Sum(x => x.VatAmount ?? 0);
        var totalNet = finalList.Sum(x => x.NetAmount ?? 0);
        finalList.Add(new SalesQuotationRegisterDtls(
                  null,             // SL
                  "",             // Date
                  "",             // QuotationNo
                  "",             // RefNo
                  "TOTAL",        // CustomerName
                  "",             // Address
                  "",             // Telephone
                  totalVat,    // VatAmount
                  totalNet     // NetAmount
              ));
        return new SalesQuotationRegisterResponse(finalList, totalVat, totalNet);
    }
    public async Task<CustomerWiseQuotationReportResponse> GetCustomerWiseQuotationReportAsync(CustomerWiseQuotationReportRequest req)
    {
        await using var db = contextProvider.CreateDbContext();
        int? branchId = req.BranchId == 0 ? null : req.BranchId;
        int? customerId = req.CustomerId == 0 ? null : req.CustomerId;
        var list = await db.SalesQuotationHdrs
            .Include(x => x.Customer)
            .Where(x =>
                (branchId == null || x.FK_BranchId == branchId) &&
                (customerId == null || x.FK_CustomerId == customerId) &&
                x.SalesQuotationDate >= req.FromDate.Value.Date &&
                x.SalesQuotationDate <= req.ToDate.Value.Date.AddDays(1).AddTicks(-1) &&
                !x.IsDeleted
            )
            .OrderBy(x => x.SalesQuotationDate)
            .Select(x => new CustomerWiseQuotationReportDtls(
                0,
                x.SalesQuotationDate.HasValue ? x.SalesQuotationDate.Value.ToString("dd-MM-yyyy") : string.Empty,
                x.SalesQuotationNo,
                x.RefNo,
                x.Customer != null ? x.Customer.Name : x.CustomerName,
                x.TaxTotal,
                x.GrandTotal
            ))
            .ToListAsync();
        int sl = 1;
        var finalList = list.Select(x => x with { SL = sl++ }).ToList();
        var totalVat = finalList.Sum(x => x.VatAmount ?? 0);
        var totalNet = finalList.Sum(x => x.NetAmount ?? 0);
        finalList.Add(new CustomerWiseQuotationReportDtls(
                    null,
                    "",
                    "",
                    "",
                    "TOTAL",
                    totalVat,
                    totalNet
                ));
        return new CustomerWiseQuotationReportResponse(finalList, totalVat, totalNet);
    }
    public async Task<DeliveryNoteRegisterResponse> GetDeliveryNoteRegisterAsync(DeliveryNoteRegisterRequest req)
    {
        await using var db = contextProvider.CreateDbContext();
        int? branchId = req.BranchId == 0 ? null : req.BranchId;
        var list = await db.DeliveryNoteHdrs
            .Include(x => x.Customer)
            .Where(x =>
                (branchId == null || x.FK_BranchId == branchId) &&
                x.DeliveryNoteDate >= req.FromDate.Value.Date &&
                x.DeliveryNoteDate <= req.ToDate.Value.Date.AddDays(1).AddTicks(-1) &&
                !x.IsDeleted
            )
            .OrderBy(x => x.DeliveryNoteDate)
            .Select(x => new DeliveryNoteRegisterDtls(
                0,
                x.DeliveryNoteDate.HasValue ? x.DeliveryNoteDate.Value.ToString("dd-MM-yyyy") : "",
                x.DeliveryNoteNo,
                x.RefNo,
                x.Customer != null ? x.Customer.Name : x.CustomerName,
                x.Customer != null ? x.Customer.Address : "",
                x.Customer != null ? x.Customer.Telephone : "",
                x.TaxTotal,
                x.GrandTotal
            ))
            .ToListAsync();
        int sl = 1;
        var finalList = list.Select(x => x with { SL = sl++ }).ToList();
        var totalVat = finalList.Sum(x => x.VatAmount ?? 0);
        var totalNet = finalList.Sum(x => x.NetAmount ?? 0);
        finalList.Add(new DeliveryNoteRegisterDtls(
                    null,
                    "",
                    "",
                    "",
                    "TOTAL",
                    "",
                    "",
                    totalVat,
                    totalNet
                ));
        return new DeliveryNoteRegisterResponse(finalList, totalVat, totalNet);
    }
    public async Task<DateWiseProfitReportResponse> GetDateWiseProfitReportAsync(DateWiseProfitReportRequest req)
    {
        await using var db = contextProvider.CreateDbContext();
        int? branchId = req.BranchId == 0 ? null : req.BranchId;
        var salesData = await db.SalesInvoiceDtls
            .Include(x => x.SalesInvoiceHdr)
            .Include(x => x.Item)
            .Where(x =>
                (branchId == null || x.SalesInvoiceHdr.FK_BranchId == branchId) &&
                x.SalesInvoiceHdr.SalesInvoiceDate >= req.FromDate.Value.Date &&
                x.SalesInvoiceHdr.SalesInvoiceDate <= req.ToDate.Value.Date.AddDays(1).AddTicks(-1) &&
                !x.SalesInvoiceHdr.IsDeleted
            )
            .GroupBy(x => x.SalesInvoiceHdr.SalesInvoiceDate)
            .Select(g => new
            {
                Date = g.Key,
                SalesAmount = g.Sum(x => x.LineTotal),
                Cost = g.Sum(x => x.Quantity * (x.Item.PurchaseRate ?? 0))
            })
            .ToListAsync();
        var returnData = await db.SalesInvoiceRtnDtls
            .Include(x => x.SalesInvoiceRtnHdr)
            .Include(x => x.Item)
            .Where(x =>
                (branchId == null || x.SalesInvoiceRtnHdr.FK_BranchId == branchId) &&
                x.SalesInvoiceRtnHdr.SalesInvoiceRtnDate >= req.FromDate.Value.Date &&
                x.SalesInvoiceRtnHdr.SalesInvoiceRtnDate <= req.ToDate.Value.Date.AddDays(1).AddTicks(-1) &&
                !x.SalesInvoiceRtnHdr.IsDeleted
            )
            .GroupBy(x => x.SalesInvoiceRtnHdr.SalesInvoiceRtnDate)
            .Select(g => new
            {
                Date = g.Key,
                ReturnAmount = g.Sum(x => x.LineTotal),
                ReturnCost = g.Sum(x => x.Quantity * (x.Item.PurchaseRate ?? 0))
            })
            .ToListAsync();
        var allDates = salesData.Select(x => x.Date).Union(returnData.Select(x => x.Date)).Distinct().OrderBy(x => x);
        var reportItems = new List<DateWiseProfitReportDtls>();
        foreach (var date in allDates)
        {
            var sales = salesData.FirstOrDefault(x => x.Date == date);
            var returns = returnData.FirstOrDefault(x => x.Date == date);
            decimal salesAmount = sales?.SalesAmount ?? 0;
            decimal returnAmount = returns?.ReturnAmount ?? 0;
            decimal salesCost = sales?.Cost ?? 0;
            decimal returnCost = returns?.ReturnCost ?? 0;
            decimal netSales = salesAmount - returnAmount;
            decimal netCost = salesCost - returnCost;
            decimal profitAmount = netSales - netCost;
            decimal profitPercentage = netSales > 0 ? (profitAmount / netSales) * 100 : 0;
            reportItems.Add(new DateWiseProfitReportDtls(
                0, // SL will be assigned later
                date.HasValue ? date.Value.ToString("dd-MM-yyyy") : "",
                salesAmount,
                returnAmount,
                netSales,
                netCost,
                profitAmount,
                profitPercentage
            ));
        }
        int sl = 1;
        var finalList = reportItems.Select(x => x with { SL = sl++ }).ToList();
        var totalSalesAmount = finalList.Sum(x => x.SalesAmount);
        var totalSalesReturn = finalList.Sum(x => x.SalesReturn);
        var totalNetSales = finalList.Sum(x => x.NetSales);
        var totalCost = finalList.Sum(x => x.Cost);
        var totalProfit = finalList.Sum(x => x.ProfitAmount);
        var totalProfitPercentage = totalNetSales > 0 ? (totalProfit / totalNetSales) * 100 : 0;
        finalList.Add(new DateWiseProfitReportDtls
        (
           null,
            "",
            totalSalesAmount,
            totalSalesReturn,
            totalNetSales,
            totalCost,
            totalProfit,
            totalProfitPercentage
        ));
        return new DateWiseProfitReportResponse(
            finalList,
            totalSalesAmount,
            totalSalesReturn,
            totalNetSales,
            totalCost,
            totalProfit,
            totalProfitPercentage
        );
    }
    public async Task<ProformaInvoiceReportResponse> GetProformaInvoiceReportAsync(ProformaInvoiceReportRequest req)
    {
        await using var db = contextProvider.CreateDbContext();
        int? branchId = req.BranchId == 0 ? null : req.BranchId;
        var list = await db.ProformaInvoiceHdrs
            .Include(x => x.Customer)
            .Where(x =>
                (branchId == null || x.FK_BranchId == branchId) &&
                x.ProformaInvoiceDate >= req.FromDate.Value.Date &&
                x.ProformaInvoiceDate <= req.ToDate.Value.Date.AddDays(1).AddTicks(-1) &&
                !x.IsDeleted
            )
            .OrderBy(x => x.ProformaInvoiceDate)
            .Select(x => new ProformaInvoiceReportDtls(
                0,
                x.ProformaInvoiceDate.HasValue ? x.ProformaInvoiceDate.Value.ToString("dd-MM-yyyy") : "",
                x.ProformaInvoiceNo,
                x.BillType,
                x.Customer != null ? x.Customer.Name : x.CustomerName,
                x.LineDsicount,
                x.TaxTotal,
                x.GrandTotal
            ))
            .ToListAsync();
        int sl = 1;
        var finalList = list.Select(x => x with { SL = sl++ }).ToList();
        var totalDiscount = finalList.Sum(x => x.Discount ?? 0);
        var totalVat = finalList.Sum(x => x.VatAmount ?? 0);
        var totalNet = finalList.Sum(x => x.TotalAmount ?? 0);
        finalList.Add(new ProformaInvoiceReportDtls(
                   null,            // SL
                   "",              // Date
                   "TOTAL",        // ProformaInvoiceNo
                   "",              // BillType
                   "",              // CustomerName
                   totalDiscount, // Discount
                   totalVat,     // VatAmount
                   totalNet      // TotalAmount
               ));
        return new ProformaInvoiceReportResponse(finalList, totalDiscount, totalVat, totalNet);
    }
    public async Task<SalesOrderReportResponse> GetSalesOrderReportAsync(SalesOrderReportRequest req)
    {
        await using var db = contextProvider.CreateDbContext();
        int? branchId = req.BranchId == 0 ? null : req.BranchId;
        int? customerId = req.CustomerId == 0 ? null : req.CustomerId;
        var list = await db.SalesOrderHdrs
            .Include(x => x.Customer)
            .Where(x =>
                (branchId == null || x.FK_BranchId == branchId) &&
                (customerId == null || x.FK_CustomerId == customerId) &&
                x.SalesOrderDate >= req.FromDate.Value.Date &&
                x.SalesOrderDate < req.ToDate.Value.Date.AddDays(1) &&
                !x.IsDeleted
            )
            .OrderBy(x => x.SalesOrderDate)
            .Select(x => new SalesOrderReportDtls(
                0,
                x.SalesOrderDate.HasValue ? x.SalesOrderDate.Value.ToString("dd-MM-yyyy") : "",
                x.SalesOrderNo,
                x.RefNo,
                x.Customer != null ? x.Customer.Name : x.CustomerName,
                x.TaxTotal,
                x.GrandTotal
            ))
            .ToListAsync();

        int sl = 1;
        var finalList = list.Select(x => x with { SL = sl++ }).ToList();
        var totalVat = finalList.Sum(x => x.VatAmount ?? 0);
        var totalNet = finalList.Sum(x => x.TotalAmount ?? 0);
        finalList.Add(new SalesOrderReportDtls(
                   null,            // SL
                   "",              // Date
                   "TOTAL",         // SalesOrderNo
                   "",              // RefNo
                   "",              // CustomerName
                   totalVat, // VatAmount
                   totalNet  // TotalAmount
               ));
        return new SalesOrderReportResponse(finalList, totalVat, totalNet);
    }
    public async Task<EnquiryRegistrationReportResponse> GetEnquiryRegistrationReportAsync(EnquiryRegistrationReportRequest req)
    {
        await using var db = contextProvider.CreateDbContext();
        var query = db.RFQHdrs.Where(x => !x.IsDeleted).AsQueryable();

        if (req.BranchId > 0)
        {
            query = query.Where(x => x.FK_BranchId == req.BranchId);
        }

        if (req.CustomerId.HasValue && req.CustomerId > 0)
        {
            query = query.Where(x => x.FK_CustomerId == req.CustomerId);
        }

        if (req.SalesmanId.HasValue && req.SalesmanId > 0)
        {
            query = query.Where(x => x.FK_SalesManId == req.SalesmanId);
        }

        // Robust date filtering
        DateTime fromDate = req.FromDate.Date;
        DateTime toDate = req.ToDate.Date.AddDays(1);
        query = query.Where(x => x.EntryDate >= fromDate && x.EntryDate < toDate);

        var list = await query
            .OrderBy(x => x.EntryDate)
            .Select(x => new EnquiryRegistrationReportDtls(
                0,
                x.EntryDate.HasValue ? x.EntryDate.Value.ToString("dd-MM-yyyy") : "",
                x.RFQNo,
                x.Customer != null ? x.Customer.Name : "",
                x.SalesMan != null ? x.SalesMan.Name : "",
                x.Remarks,
                x.CustomerRefNo,
                x.User != null ? x.User.UserName : ""
            )).ToListAsync();

        int sl = 1;
        var finalList = list.Select(x => x with { SL = sl++ }).ToList();

        return new EnquiryRegistrationReportResponse(finalList);
    }
    public async Task<VatReportResponse> GetVatReportAsync(VatReportRequest req)
    {
        await using var db = contextProvider.CreateDbContext();
        int? branchId = req.BranchId == 0 ? null : req.BranchId;

        DateTime fromDate = req.FromDate?.Date ?? DateTime.Now.Date;
        DateTime toDate = req.ToDate?.Date ?? DateTime.Now.Date;

        // =========================
        // 1. SALES (DETAILS)
        // =========================
        var sales = await db.SalesInvoiceDtls
            .Include(x => x.SalesInvoiceHdr)
            .Where(x =>
                (branchId == null || x.SalesInvoiceHdr!.FK_BranchId == branchId) &&
                x.SalesInvoiceHdr!.SalesInvoiceDate >= fromDate &&
                x.SalesInvoiceHdr.SalesInvoiceDate < toDate.AddDays(1) &&
                !x.SalesInvoiceHdr.IsDeleted
            )
            .GroupBy(x => x.TaxPerc)
            .Select(g => new VatTemp
            {
                Type = "Sales",
                TaxPerc = g.Key,
                Taxable = g.Sum(x => x.TaxableAmnt),
                Vat = g.Sum(x => x.TaxAmount)
            })
            .ToListAsync();


        // SALES (HEADER TOTAL)
        var salesHeaderVat = await db.SalesInvoiceHdrs
            .Where(x =>
                (branchId == null || x.FK_BranchId == branchId) &&
                x.SalesInvoiceDate >= fromDate &&
                x.SalesInvoiceDate < toDate.AddDays(1) &&
                !x.IsDeleted
            )
            .SumAsync(x => (decimal?)x.TaxTotal) ?? 0;


        AdjustVatDifference(sales, salesHeaderVat);


        // =========================
        // 2. SALES RETURN
        // =========================
        var salesReturn = await db.SalesInvoiceRtnDtls
            .Include(x => x.SalesInvoiceRtnHdr)
            .Where(x =>
                (branchId == null || x.SalesInvoiceRtnHdr!.FK_BranchId == branchId) &&
                x.SalesInvoiceRtnHdr!.SalesInvoiceRtnDate >= fromDate &&
                x.SalesInvoiceRtnHdr.SalesInvoiceRtnDate < toDate.AddDays(1) &&
                !x.SalesInvoiceRtnHdr.IsDeleted
            )
            .GroupBy(x => x.TaxPerc)
            .Select(g => new VatTemp
            {
                Type = "Sales Return",
                TaxPerc = g.Key,
                Taxable = g.Sum(x => x.TaxableAmnt),
                Vat = g.Sum(x => x.TaxAmount)
            })
            .ToListAsync();


        var salesReturnHeaderVat = await db.SalesInvoiceRtnHdrs
            .Where(x =>
                (branchId == null || x.FK_BranchId == branchId) &&
                x.SalesInvoiceRtnDate >= fromDate &&
                x.SalesInvoiceRtnDate < toDate.AddDays(1) &&
                !x.IsDeleted
            )
            .SumAsync(x => (decimal?)x.TaxTotal) ?? 0;


        AdjustVatDifference(salesReturn, salesReturnHeaderVat);


        // =========================
        // 3. PURCHASE
        // =========================
        var purchase = await db.PurchaseDtls
            .Include(x => x.PurchaseHdr)
            .Where(x =>
                (branchId == null || x.PurchaseHdr!.FK_BranchId == branchId) &&
                x.PurchaseHdr!.PurchaseDate >= fromDate &&
                x.PurchaseHdr.PurchaseDate < toDate.AddDays(1) &&
                !x.PurchaseHdr.IsDeleted
            )
            .GroupBy(x => x.TaxPerc)
            .Select(g => new VatTemp
            {
                Type = "Purchase",
                TaxPerc = g.Key,
                Taxable = g.Sum(x => x.TaxableAmnt),
                Vat = g.Sum(x => x.TaxAmount)
            })
            .ToListAsync();


        var purchaseHeaderVat = await db.PurchaseHdrs
            .Where(x =>
                (branchId == null || x.FK_BranchId == branchId) &&
                x.PurchaseDate >= fromDate &&
                x.PurchaseDate < toDate.AddDays(1) &&
                !x.IsDeleted
            )
            .SumAsync(x => (decimal?)x.TaxTotal) ?? 0;


        AdjustVatDifference(purchase, purchaseHeaderVat);


        // =========================
        // 4. PURCHASE RETURN
        // =========================
        var purchaseReturn = await db.PurchaseReturnDtls
            .Include(x => x.PurchaseReturnHdr)
            .Where(x =>
                (branchId == null || x.PurchaseReturnHdr!.FK_BranchId == branchId) &&
                x.PurchaseReturnHdr!.PurchaseReturnDate >= fromDate &&
                x.PurchaseReturnHdr.PurchaseReturnDate < toDate.AddDays(1) &&
                !x.PurchaseReturnHdr.IsDeleted
            )
            .GroupBy(x => x.TaxPerc)
            .Select(g => new VatTemp
            {
                Type = "Purchase Return",
                TaxPerc = g.Key,
                Taxable = g.Sum(x => x.TaxableAmnt),
                Vat = g.Sum(x => x.TaxAmount)
            })
            .ToListAsync();


        var purchaseReturnHeaderVat = await db.PurchaseReturnHdrs
            .Where(x =>
                (branchId == null || x.FK_BranchId == branchId) &&
                x.PurchaseReturnDate >= fromDate &&
                x.PurchaseReturnDate < toDate.AddDays(1) &&
                !x.IsDeleted
            )
            .SumAsync(x => (decimal?)x.TaxTotal) ?? 0;


        AdjustVatDifference(purchaseReturn, purchaseReturnHeaderVat);


        // =========================
        // 5. EXPENSE
        // =========================
        var expenses = await db.ExpenseEntryDtls
            .Include(x => x.ExpenseHdr)
            .Where(x =>
                (branchId == null || x.ExpenseHdr!.FK_BranchId == branchId) &&
                x.ExpenseHdr!.EntryDate >= fromDate &&
                x.ExpenseHdr.EntryDate < toDate.AddDays(1) &&
                !x.IsDeleted
            )
            .GroupBy(x => x.VatPer)
            .Select(g => new VatTemp
            {
                Type = "Expenses",
                TaxPerc = g.Key,
                Taxable = g.Sum(x => x.Amount),
                Vat = g.Sum(x => x.VatAmt)
            })
            .ToListAsync();


        var expenseHeaderVat = await db.ExpenseEntryHdrs
            .Where(x =>
                (branchId == null || x.FK_BranchId == branchId) &&
                x.EntryDate >= fromDate &&
                x.EntryDate < toDate.AddDays(1)
            )
            .SumAsync(x => (decimal?)x.VatTotal) ?? 0;


        AdjustVatDifference(expenses, expenseHeaderVat);


        // =========================
        // FINAL RESULT
        // =========================

        var all = sales
            .Concat(salesReturn)
            .Concat(purchase)
            .Concat(purchaseReturn)
            .Concat(expenses)
            .ToList();


        int sl = 1;

        var finalList = all
            .Select(x => new VatReportDtls(
                sl++,
                x.Type,
                x.TaxPerc,
                x.Taxable,
                x.Vat
            ))
            .ToList();


        var outputVat = salesHeaderVat - salesReturnHeaderVat;
        var inputVat = (purchaseHeaderVat - purchaseReturnHeaderVat) + expenseHeaderVat;
        var payableVat = outputVat - inputVat;


        var totalTaxable = finalList.Sum(x => x.Taxable ?? 0);


        finalList.Add(new VatReportDtls(
            null,
            "Vat Payable",
            null,
            null,
            payableVat
        ));


        return new VatReportResponse(finalList, totalTaxable, payableVat);
    }


    // =================================================
    // HELPER CLASS
    // =================================================

    public class VatTemp
    {
        public string Type { get; set; } = "";
        public decimal? TaxPerc { get; set; }
        public decimal Taxable { get; set; }
        public decimal Vat { get; set; }
    }


    // =================================================
    // ADJUSTMENT METHOD
    // =================================================

    private void AdjustVatDifference(
        List<VatTemp> list,
        decimal headerVat)
    {
        if (list == null || list.Count == 0)
            return;

        var detailVat = list.Sum(x => x.Vat);

        var diff = headerVat - detailVat;

        if (diff == 0)
            return;

        // Adjust highest tax slab
        var row = list
            .OrderByDescending(x => x.TaxPerc ?? 0)
            .FirstOrDefault();

        if (row != null)
        {
            row.Vat += diff;
        }
    }

}



