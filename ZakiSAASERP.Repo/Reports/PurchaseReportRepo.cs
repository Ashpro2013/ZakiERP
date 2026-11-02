namespace ZakiSAASERP.Repo.Reports;

public class PurchaseReportRepo(IDbContextProvider contextProvider) : IPurchaseReportRepo
{
    public async Task<PurchaseReportResponse> GetPurchaseReportAsync(PurchaseReportRequest req)
    {
        await using var db = contextProvider.CreateDbContext();
        var purchaseQuery = db.PurchaseHdrs
            .Where(x => x.PurchaseDate >= req.FromDate.Date && x.PurchaseDate <= req.ToDate.Date.AddDays(1).AddTicks(-1) && !x.IsDeleted)
            .AsQueryable();

        // Filter by Branch
        if (req.BranchId > 0)
        {
            purchaseQuery = purchaseQuery.Where(x => x.FK_BranchId == req.BranchId);
        }

        // Filter by Supplier
        if (req.SupplierId.HasValue && req.SupplierId > 0)
        {
            purchaseQuery = purchaseQuery.Where(x => x.FK_SupplierId == req.SupplierId);
        }

        // Filter by BillType
        if (!string.IsNullOrEmpty(req.BillType) && req.BillType != "All")
        {
            purchaseQuery = purchaseQuery.Where(x => x.BillType == req.BillType);
        }

        var purchaseList = await purchaseQuery
            .Include(x => x.Supplier)
            .OrderBy(x => x.PurchaseDate)
            .Select(x => new PurchaseReportDtls(
                0, // SL
                x.PurchaseNo,
                x.PurchaseDate.HasValue ? x.PurchaseDate.Value.ToString("dd-MMM-yyyy") : "",
                x.BillType ?? "Cash",
                x.SupplierVatNo ?? "",
                x.Supplier != null ? x.Supplier.Name : x.SupplierName ?? "",
                x.GrossAmount,
                x.LineDsicount,
                x.TaxTotal,
                x.GrandTotal
            )).ToListAsync();

        // Add serial numbers
        int sl = 1;
        var finalReports = purchaseList.Select(x => x with { SL = sl++ }).ToList();

        var totalGross = purchaseList.Sum(x => x.GrossAmount ?? 0);
        var totalDiscount = purchaseList.Sum(x => x.DiscountAmount ?? 0);
        var totalVat = purchaseList.Sum(x => x.VatAmount ?? 0);
        var totalNet = purchaseList.Sum(x => x.NetAmount ?? 0);
        finalReports.Add(new PurchaseReportDtls(
            null,
            "Total",
            null,
            null,
            null,
            null,
            totalGross,
            totalDiscount,
            totalVat,
            totalNet
        ));
        return new PurchaseReportResponse(finalReports, totalGross, totalDiscount, totalVat, totalNet);
    }
    public async Task<PurchaseDetailsReportResponse> GetPurchaseDetailsAsync(PurchaseDetailsReportRequest req)
    {
        await using var db = contextProvider.CreateDbContext();
        var detailsQuery = db.PurchaseDtls
            .Include(x => x.PurchaseHdr)
            .Include(x => x.Item)
            .Where(x => x.PurchaseHdr.PurchaseDate >= req.FromDate.Date &&
                       x.PurchaseHdr.PurchaseDate <= req.ToDate.Date.AddDays(1).AddTicks(-1) &&
                       !x.PurchaseHdr.IsDeleted &&
                       !x.IsDeleted)
            .AsQueryable();

        // Filter by Branch
        if (req.BranchId > 0)
        {
            detailsQuery = detailsQuery.Where(x => x.PurchaseHdr.FK_BranchId == req.BranchId);
        }

        // Filter by Supplier
        if (req.SupplierId.HasValue && req.SupplierId > 0)
        {
            detailsQuery = detailsQuery.Where(x => x.PurchaseHdr.FK_SupplierId == req.SupplierId);
        }

        // Filter by Item Name
        if (!string.IsNullOrEmpty(req.ItemName))
        {
            detailsQuery = detailsQuery.Where(x => x.ItemName.Contains(req.ItemName));
        }

        var detailsList = await detailsQuery
            .OrderBy(x => x.PurchaseHdr.PurchaseDate)
            .ThenBy(x => x.PurchaseHdr.PurchaseNo)
            .Select(x => new PurchaseDetailsReportDtls(
                0, // SL
                x.PurchaseHdr.PurchaseDate.HasValue ? x.PurchaseHdr.PurchaseDate.Value.ToString("dd-MMM-yyyy") : "",
                x.PurchaseHdr.PurchaseNo,
                x.PurchaseHdr.InvoiceNo ?? "",
                x.Item != null ? x.Item.ItemCode : "",
                x.ItemName ?? "",
                x.Quantity,
                x.ExRate,
                x.Quantity * x.ExRate  // Calculate Amount as Qty * Rate
            )).ToListAsync();

        // Add serial numbers
        int sl = 1;
        var finalReports = detailsList.Select(x => x with { SL = sl++ }).ToList();

        var totalQty = detailsList.Sum(x => x.Quantity);
        var totalAmount = detailsList.Sum(x => x.Amount);
        finalReports.Add(new PurchaseDetailsReportDtls(
            null,
            "Total",
            null,
            null,
            null,
            null,
            totalQty,
            null,
            totalAmount
        ));
        return new PurchaseDetailsReportResponse(finalReports, totalQty, totalAmount);
    }
    public async Task<PurchaseOrderReportResponse> GetPurchaseOrderReport(PurchaseOrderReportRequest req)
    {
        await using var db = contextProvider.CreateDbContext();
        int? BranchId = req.BranchId > 0 ? req.BranchId : null;
        int? SupplierId = req.SupplierId > 0 ? req.SupplierId : null;
        var query = db.PurchaseOrderHdrs
            .Where(x => x.PurchaseOrderDate >= req.FromDate.Value.Date && x.PurchaseOrderDate <= req.ToDate.Value.Date.AddDays(1).AddTicks(-1) && !x.IsDeleted &&
                (BranchId == null || x.FK_BranchId == BranchId) &&
                (SupplierId == null || x.FK_SupplierId == SupplierId)
            ).AsQueryable();
        var list = await query
            .Include(x => x.Supplier)
            .OrderBy(x => x.PurchaseOrderDate)
            .Select(x => new PurchaseOrderReportDtls(
                0, // SL
                x.PurchaseOrderDate.HasValue ? x.PurchaseOrderDate.Value.ToString("dd-MMM-yyyy") : "",
                x.PurchaseOrderNo,
                x.RefNo ?? "",
                x.Supplier != null ? x.Supplier.Name : "",
                x.TaxTotal,
                x.GrandTotal
            )).ToListAsync();

        int sl = 1;
        var finalReports = list.Select(x => x with { SL = sl++ }).ToList();

        var totalVat = list.Sum(x => x.VatAmount ?? 0);
        var totalNet = list.Sum(x => x.TotalAmount ?? 0);
        finalReports.Add(new PurchaseOrderReportDtls(
            null,
            "Total",
            null,
            null,
            null,
            totalVat,
            totalNet
        ));
        return new PurchaseOrderReportResponse(finalReports, totalVat, totalNet);
    }
    public async Task<PurchaseReturnReportResponse> GetPurchaseReturnReportAsync(PurchaseReturnReportRequest req)
    {
        await using var db = contextProvider.CreateDbContext();
        var query = db.PurchaseReturnHdrs
            .Where(x => x.PurchaseReturnDate >= req.FromDate.Date && x.PurchaseReturnDate <= req.ToDate.Date.AddDays(1).AddTicks(-1) && !x.IsDeleted)
            .AsQueryable();

        if (req.BranchId > 0)
        {
            query = query.Where(x => x.FK_BranchId == req.BranchId);
        }

        if (req.SupplierId.HasValue && req.SupplierId > 0)
        {
            query = query.Where(x => x.FK_SupplierId == req.SupplierId);
        }

        if (!string.IsNullOrEmpty(req.BillType) && req.BillType != "All")
        {
            query = query.Where(x => x.BillType == req.BillType);
        }

        var list = await query
            .Include(x => x.Supplier)
            .OrderBy(x => x.PurchaseReturnDate)
            .Select(x => new PurchaseReturnReportDtls(
                0, // SL
                x.PurchaseReturnNo,
                x.PurchaseReturnDate.HasValue ? x.PurchaseReturnDate.Value.ToString("dd-MMM-yyyy") : "",
                x.BillType ?? "Cash",
                x.Supplier != null ? x.Supplier.VatNo : "",
                x.Supplier != null ? x.Supplier.Name : x.SupplierName ?? "",
                x.GrossAmount,
                x.LineDsicount,
                x.TaxTotal,
                x.GrandTotal
            )).ToListAsync();

        int sl = 1;
        var finalReports = list.Select(x => x with { SL = sl++ }).ToList();

        var totalGross = list.Sum(x => x.GrossAmount ?? 0);
        var totalDiscount = list.Sum(x => x.DiscountAmount ?? 0);
        var totalVat = list.Sum(x => x.VatAmount ?? 0);
        var totalNet = list.Sum(x => x.NetAmount ?? 0);
        finalReports.Add(new PurchaseReturnReportDtls(
            null,
            "Total",
            null,
            null,
            null,
            null,
            totalGross,
            totalDiscount,
            totalVat,
            totalNet
        ));
        return new PurchaseReturnReportResponse(finalReports, totalGross, totalDiscount, totalVat, totalNet);
    }
    public async Task<PurchaseMonitorResponse> GetPurchaseMonitorReportAsync(PurchaseMonitorRequest req)
    {
        await using var db = contextProvider.CreateDbContext();
        // 1. Base Query
        var purchaseQuery = db.PurchaseHdrs
            .Where(x => x.PurchaseDate >= req.FromDate.Date && x.PurchaseDate <= req.ToDate.Date.AddDays(1).AddTicks(-1) && !x.IsDeleted)
            .AsQueryable();

        if (req.BranchId > 0)
            purchaseQuery = purchaseQuery.Where(x => x.FK_BranchId == req.BranchId);

        if (req.SupplierId.HasValue && req.SupplierId > 0)
            purchaseQuery = purchaseQuery.Where(x => x.FK_SupplierId == req.SupplierId);

        if (!string.IsNullOrEmpty(req.BillType) && req.BillType != "All")
            purchaseQuery = purchaseQuery.Where(x => x.BillType == req.BillType);

        if (!string.IsNullOrEmpty(req.SearchTerm))
        {
            purchaseQuery = purchaseQuery.Where(x =>
                (x.PurchaseNo != null && x.PurchaseNo.Contains(req.SearchTerm)) ||
                (x.Supplier != null && x.Supplier.Name.Contains(req.SearchTerm))
            );
        }

        // 2. Project with Balance for Status Filtering
        var projectedQuery = purchaseQuery.Select(x => new
        {
            Hdr = x,
            Paid = x.BillType == "Credit"
                   ? (db.SupplierPaymentDtls.Where(p => p.FK_PurchaseId == x.PurchaseId && !p.IsDeleted).Sum(p => (decimal?)p.PayingAmount) ?? 0)
                   : x.GrandTotal,
            Discount = x.BillType == "Credit"
                       ? (db.SupplierPaymentDtls.Where(p => p.FK_PurchaseId == x.PurchaseId && !p.IsDeleted).Sum(p => (decimal?)p.Discount) ?? 0)
                       : 0,
            Return = db.PurchaseReturnHdrs.Where(r => r.FK_PurchaseId == x.PurchaseId && !r.IsDeleted).Sum(r => (decimal?)r.GrandTotal) ?? 0
        }).Select(x => new
        {
            x.Hdr,
            x.Paid,
            x.Discount,
            x.Return,
            Balance = x.Hdr.BillType == "Credit"
                      ? (x.Hdr.GrandTotal - x.Paid - x.Discount - x.Return)
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

        // 4. Calculate Grand Totals
        var grandTotals = await projectedQuery
            .GroupBy(x => 1)
            .Select(g => new
            {
                TotalAmount = g.Sum(x => x.Hdr.GrandTotal),
                TotalPaid = g.Sum(x => x.Paid),
                TotalReturn = g.Sum(x => x.Return),
                TotalDiscount = g.Sum(x => x.Discount),
                TotalBalance = g.Sum(x => x.Balance),
                Count = g.Count()
            })
            .FirstOrDefaultAsync();

        int totalCount = grandTotals?.Count ?? 0;
        decimal totalAmount = grandTotals?.TotalAmount ?? 0;
        decimal totalPaid = grandTotals?.TotalPaid ?? 0;
        decimal totalReturn = grandTotals?.TotalReturn ?? 0;
        decimal totalDiscount = grandTotals?.TotalDiscount ?? 0;
        decimal totalBalance = grandTotals?.TotalBalance ?? 0;

        // 5. No Pagination - Return All
        var pagedQuery = projectedQuery.OrderByDescending(x => x.Hdr.PurchaseDate);

        var pagedData = await pagedQuery
            .Select(x => new
            {
                x.Hdr,
                SupplierName = x.Hdr.Supplier != null ? x.Hdr.Supplier.Name : "",
                x.Paid,
                x.Discount,
                x.Return,
                x.Balance
            })
            .ToListAsync<dynamic>();

        // 6. Map to DTO
        var reportItems = new List<PurchaseMonitorDtls>();
        int sl = 1;

        foreach (var item in pagedData)
        {
            var purchase = (ZakiSAASERP.Models.Transactions.Purchase.PurchaseHdr)item.Hdr;

            reportItems.Add(new PurchaseMonitorDtls(
                sl++,
                purchase.PurchaseId,
                purchase.PurchaseNo,
                purchase.PurchaseDate.HasValue ? purchase.PurchaseDate.Value.ToString("dd/MM/yyyy") : "",
                item.SupplierName,
                purchase.BillType ?? "Cash",
                purchase.GrandTotal,
                item.Paid,
                item.Discount,
                item.Return,
                item.Balance,
                item.Balance <= 0.01m ? "Closed" : "Pending"
            ));
        }

        return new PurchaseMonitorResponse(reportItems, totalAmount, totalPaid, totalDiscount, totalReturn, totalBalance, totalCount);
    }

    public async Task<List<PurchaseMonitorPaymentItem>> GetPaymentsByPurchaseAsync(int purchaseId)
    {
        await using var db = contextProvider.CreateDbContext();
        var list = await (from d in db.SupplierPaymentDtls
                          join h in db.SupplierPaymentHdrs on d.FK_PaymentId equals h.PaymentId
                          where d.FK_PurchaseId == purchaseId && !d.IsDeleted && !h.IsDeleted
                          select new PurchaseMonitorPaymentItem(
                              h.VoucherNo,
                              h.PaymentId,
                              h.PaidDate.HasValue ? h.PaidDate.Value.ToString("dd/MM/yyyy") : "",
                              d.PayingAmount,
                              d.Discount
                          )).ToListAsync();

        return list;
    }
    public async Task<PurchaseReturnDetailsReportResponse> GetPurchaseReturnDetailsAsync(PurchaseReturnDetailsReportRequest req)
    {
        await using var db = contextProvider.CreateDbContext();
        var detailsQuery = db.PurchaseReturnDtls
            .Include(x => x.PurchaseReturnHdr)
            .Include(x => x.Item)
            .Where(x => x.PurchaseReturnHdr.PurchaseReturnDate >= req.FromDate.Date &&
                       x.PurchaseReturnHdr.PurchaseReturnDate <= req.ToDate.Date.AddDays(1).AddTicks(-1) &&
                       !x.PurchaseReturnHdr.IsDeleted &&
                       !x.IsDeleted)
            .AsQueryable();

        // Filter by Branch
        if (req.BranchId > 0)
        {
            detailsQuery = detailsQuery.Where(x => x.PurchaseReturnHdr.FK_BranchId == req.BranchId);
        }

        // Filter by Supplier
        if (req.SupplierId.HasValue && req.SupplierId > 0)
        {
            detailsQuery = detailsQuery.Where(x => x.PurchaseReturnHdr.FK_SupplierId == req.SupplierId);
        }

        // Filter by Item Name
        if (!string.IsNullOrEmpty(req.ItemName))
        {
            detailsQuery = detailsQuery.Where(x => x.ItemName.Contains(req.ItemName));
        }

        var list = await detailsQuery
            .OrderBy(x => x.PurchaseReturnHdr.PurchaseReturnDate)
                .ThenBy(x => x.PurchaseReturnHdr.PurchaseReturnNo)
            .Select(x => new PurchaseReturnDetailsReportDtls(
                0, // SL
                x.PurchaseReturnHdr.PurchaseReturnDate.HasValue ? x.PurchaseReturnHdr.PurchaseReturnDate.Value.ToString("dd-MMM-yyyy") : "",
                x.PurchaseReturnHdr.PurchaseReturnNo, // ReturnNo
                x.PurchaseReturnHdr.RefNo ?? "", // RefNo / InvoiceNo
                x.Item != null ? x.Item.ItemCode : "",
                x.ItemName ?? "",
                x.Quantity,
                x.ExRate,
                x.Quantity * x.ExRate // Amount
            )).ToListAsync();

        int sl = 1;
        var finalReports = list.Select(x => x with { SL = sl++ }).ToList();

        var totalQty = list.Sum(x => x.Quantity);
        var totalAmount = list.Sum(x => x.Amount);
        finalReports.Add(new PurchaseReturnDetailsReportDtls(
            null,
            "", // Date
            "Total", // ReturnNo
            null, // InvoiceNo
            null, // ItemCode
            null, // ItemName
            totalQty, // Quantity
            null, // Rate
            totalAmount // Amount
        ));

        return new PurchaseReturnDetailsReportResponse(finalReports, totalQty, totalAmount);
    }
}
