namespace ZakiSAASERP.Repo.Reports;

public class InventoryReportRepo(IDbContextProvider contextProvider) : IInventoryReportRepo
{
    public async Task<StockReportResponse> GetStockReportAsync(StockReportRequest req)
    {
        await using var db = contextProvider.CreateDbContext();
        int? branchId = req.BranchId == 0 ? null : req.BranchId;
        int? storeId = req.StoreId == 0 ? null : req.StoreId;
        int? categoryId = req.CategoryId == 0 ? null : req.CategoryId;
        int? brandId = req.BrandId == 0 ? null : req.BrandId;
        var query = await db.StockTranLogs
    .Where(t =>
        (branchId == null || t.FK_BranchId == branchId) &&
        (storeId == null || t.FK_StoreId == storeId) &&
        (categoryId == null || t.Item.FK_CategoryId == categoryId) &&
        (brandId == null || t.Item.FK_BrandId == brandId) &&
        t.TranDate < req.Date.Value.Date.AddDays(1)
    )
    .GroupBy(t => new
    {
        t.FK_ItemId,
        ItemName = t.Item.ItemName,
        UnitName = t.Unit.Name
    })
    .Select(g => new
    {
        ItemName = g.Key.ItemName!,
        UnitName = g.Key.UnitName!,
        StockQty = g.Sum(x => (x.InQuantity ?? 0) - (x.OutQuantity ?? 0)),
        Rate = g.OrderByDescending(x => x.TranDate)
                .Select(x => x.InRate ?? x.OutRate ?? 0)
                .FirstOrDefault()
    })
    .Where(r =>
        req.ReportType == 0 ||
        (req.ReportType == 1 && r.StockQty > 0) ||
        (req.ReportType == 2 && r.StockQty < 0) ||
        (req.ReportType == 3 && r.StockQty == 0)
    )
    .AsNoTracking()
    .ToListAsync();


        int sl = 1;

        var result = query
            .Select(x => new StockReportDtls(
                SL: sl++,
                ItemName: x.ItemName,
                StockQty: x.StockQty,
                Unit: x.UnitName,
                Rate: x.Rate,
                StockValue: x.StockQty * x.Rate
            ))
            .ToList();
        var totalStockValue = result.Sum(r => r.StockValue);
        result.Add(new StockReportDtls(
            SL: null,
            ItemName: "Total",
            StockQty: result.Sum(r => r.StockQty),
            Unit: "",
            Rate: null,
            StockValue: totalStockValue
        ));
        return new StockReportResponse(result, totalStockValue);
    }
    public async Task<StockTransferReportResponse> GetStockTransferReportAsync(StockTransferReportRequest req)
    {
        await using var db = contextProvider.CreateDbContext();
        var BranchId = req.BranchId == 0 ? null : req.BranchId;
        var query = await (from h in db.InternalMaterialTransferHdrs
                           join d in db.InternalMaterialTransferDtls on h.IntMatTransId equals d.FK_IntMatTransId
                           join i in db.ItemMasters on d.FK_ItemId equals i.ItemId
                           where (BranchId == null || h.FK_BranchId == BranchId)
                           && (!req.FromDate.HasValue || h.VoucherDate >= req.FromDate)
                           && (!req.ToDate.HasValue || h.VoucherDate <= req.ToDate)
                           && !h.IsDeleted
                           select new
                           {
                               h.VoucherDate,
                               h.VoucherNo,
                               i.ItemName,
                               d.Quantity,
                               d.Rate,
                               d.Amount
                           }).ToListAsync();

        int sl = 1;
        var result = query.Select(x => new StockTransferReportDtls(
            SL: sl++,
            Date: x.VoucherDate?.ToString("dd/MM/yyyy"),
            DocNo: x.VoucherNo,
            ItemName: x.ItemName,
            Quantity: x.Quantity,
            Rate: x.Rate,
            Amount: x.Amount
        )).ToList();

        var totalAmount = result.Sum(x => x.Amount);
        return new StockTransferReportResponse(result, totalAmount);
    }
    public async Task<ItemTransactionHistoryReportResponse> GetItemTransactionHistoryReportAsync(ItemTransactionHistoryReportRequest req)
    {
        await using var db = contextProvider.CreateDbContext();
        var query = db.StockTranLogs.AsQueryable();

        if (req.BranchId.HasValue && req.BranchId.Value > 0)
        {
            query = query.Where(x => x.FK_BranchId == req.BranchId);
        }

        if (req.StoreId.HasValue && req.StoreId.Value > 0)
        {
            query = query.Where(x => x.FK_StoreId == req.StoreId);
        }

        if (req.ItemId.HasValue && req.ItemId.Value > 0)
        {
            query = query.Where(x => x.FK_ItemId == req.ItemId);
        }

        if (req.FromDate.HasValue)
        {
            query = query.Where(x => x.TranDate >= req.FromDate);
        }

        if (req.ToDate.HasValue)
        {
            query = query.Where(x => x.TranDate <= req.ToDate);
        }

        var result = await query.Select(x => new
        {
            x.TranDate,
            x.VchrType,
            x.VchrNo,
            x.InQuantity,
            x.OutQuantity,
            UnitName = x.Unit != null ? x.Unit.Name : "",
            Rate = x.InRate ?? x.OutRate ?? 0,
            BranchName = x.Branch != null ? x.Branch.Name : "",
            StoreName = x.Store != null ? x.Store.Name : ""
        })
        .OrderBy(x => x.TranDate)
        .ToListAsync();

        int sl = 1;
        var reportItems = result.Select(x => new ItemTransactionHistoryReportDtls(
            sl++,
            x.TranDate?.ToString("dd/MM/yyyy") ?? "",
            x.VchrType ?? "",
            x.VchrNo,
            x.InQuantity ?? 0,
            x.OutQuantity ?? 0,
            x.UnitName,
            x.Rate,
            x.BranchName,
            x.StoreName
        )).ToList();
        decimal totalInQty = reportItems.Sum(x => x.InQty);
        decimal totalOutQty = reportItems.Sum(x => x.OutQty);
        reportItems.Add(new ItemTransactionHistoryReportDtls(
            SL: 0,
            Date: "Total",
            VoucherType: "",
            VoucherNo: "",
            InQty: totalInQty,
            OutQty: totalOutQty,
            Unit: "",
            Rate: 0,
            BranchName: "",
            StoreName: ""
        ));
        return new ItemTransactionHistoryReportResponse(reportItems, totalInQty, totalOutQty);
    }
    public async Task<StockAdjustmentReportResponse> GetStockAdjustmentReportAsync(StockAdjustmentReportRequest req)
    {
        await using var db = contextProvider.CreateDbContext();
        int? branchId = req.BranchId == 0 ? null : req.BranchId;

        var query = await (from h in db.StockAdjustmentHdrs
                           join d in db.StockAdjustmentDtls on h.StockAdjId equals d.FK_StockAdjId
                           join i in db.ItemMasters on d.FK_ItemId equals i.ItemId
                           join u in db.UnitMasters on d.FK_UnitId equals u.UnitId into unitGroup
                           from u in unitGroup.DefaultIfEmpty()
                           where (branchId == null || h.FK_BranchId == branchId)
                           && (!req.FromDate.HasValue || h.VoucherDate >= req.FromDate.Value.Date)
                           && (!req.ToDate.HasValue || h.VoucherDate < req.ToDate.Value.Date.AddDays(1))
                           && !h.IsDeleted
                           orderby h.VoucherDate
                           select new
                           {
                               h.VoucherDate,
                               h.VoucherNo,
                               h.ReferenceNo,
                               h.Remarks,
                               i.ItemCode,
                               i.ItemName,
                               d.CurrentQty,
                               d.Quantity,
                               d.NewQty,
                               UnitName = u != null ? u.Name : "",
                               d.Amount
                           }).ToListAsync();

        int sl = 1;
        var result = query.Select(x => new StockAdjustmentReportDtls(
            SL: sl++,
            Date: x.VoucherDate?.ToString("dd-MM-yyyy"),
            VoucherNo: x.VoucherNo,
            RefNo: x.ReferenceNo,
            ItemCode: x.ItemCode,
            ItemName: x.ItemName,
            CurrentQty: x.CurrentQty,
            Quantity: x.Quantity,
            NewQty: x.NewQty,
            Unit: x.UnitName,
            Amount: x.Amount,
            Remarks: x.Remarks
        )).ToList();

        var totalQty = result.Sum(x => x.Quantity ?? 0);
        var totalAmount = result.Sum(x => x.Amount ?? 0);

        return new StockAdjustmentReportResponse(result, totalQty, totalAmount);
    }

}



