using ZakiSAASERP.Interface.Transaction.Inventory;
using ZakiSAASERP.Models.Transactions.Purchase;
using ZakiSAASERP.Models.ViewModel;
using ZakiSAASERP.Repo.CodeGenerationServices;
using AshproStringExtension;

namespace ZakiSAASERP.Repo.Transaction.Inventory;

public class GoodsReceiptRepo : IGoodsReceiptRepo
{
    private readonly IDbContextProvider _contextProvider;
    private readonly StockService _stockService;
    private readonly CodeSequenceService _codeService;

    public GoodsReceiptRepo(IDbContextProvider contextProvider, StockService stockService, CodeSequenceService codeService)
    {
        _contextProvider = contextProvider;
        _stockService = stockService;
        _codeService = codeService;
    }

    public async Task<string> GetNextGoodsReceiptNoAsync()
    {
        await using var db = _contextProvider.CreateDbContext();
        return await _codeService.GetNextUniqueCodeAsync(
            CodeKeys.GoodsReceipt,
            async (code) => await db.GoodsReceiptHdrs.AnyAsync(x => x.GoodsReceiptNo == code)
        );
    }


    public async Task<string> GetNewDocNo(int branchId)
    {
        await using var db = _contextProvider.CreateDbContext();
        var result = await db.GoodsReceiptHdrs
            .Where(x => x.EntryDate.HasValue && x.EntryDate.Value.Month == DateTime.Now.Month && x.EntryDate.Value.Year == DateTime.Now.Year && x.FK_BranchId == branchId)
            .OrderByDescending(x => x.GoodsReceiptNo)
            .Select(x => x.GoodsReceiptNo)
            .FirstOrDefaultAsync();
        if (string.IsNullOrEmpty(result))
        {
            return "1001";
        }
        if (int.TryParse(result, out int no))
        {
            return (no + 1).ToString();
        }
        return "1001";
    }

    public async Task<GoodsReceiptDTO> Create(GoodsReceiptDTO master)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var hdr = master.Header.CopyTo<GoodsReceiptHdr>();

            // Handle optional FKs
            if (hdr.FK_PurchaserId == 0) hdr.FK_PurchaserId = null;

            await db.GoodsReceiptHdrs.AddAsync(hdr);
            await db.SaveChangesAsync();

            var savedDetails = new List<GoodsReceiptDtls>();
            foreach (var item in master.Details)
            {
                var dtl = item.CopyTo<GoodsReceiptDtls>();
                dtl.FK_GoodsReceiptHdrId = hdr.GoodsReceiptId;
                if (dtl.FK_StoreId == null || dtl.FK_StoreId == 0)
                {
                    var defaultStore = await db.StoreMasters
                        .Where(x => x.FK_BranchId == hdr.FK_BranchId || x.FK_BranchId == null)
                        .FirstOrDefaultAsync(x => x.IsDefaultStore) ??
                        await db.StoreMasters.FirstOrDefaultAsync();
                    dtl.FK_StoreId = defaultStore?.StoreId;
                }
                await db.GoodsReceiptDtls.AddAsync(dtl);
                savedDetails.Add(dtl);
            }
            await db.SaveChangesAsync();

            // Stock Movement
            var tHdr = hdr.CopyTo<TransactionHdr>();
            tHdr.HdrId = hdr.GoodsReceiptId;
            tHdr.HdrNo = hdr.GoodsReceiptNo;
            tHdr.PartyId = hdr.FK_SupplierId;
            tHdr.VoucherType = "Goods Receipt";
            tHdr.Remarks = $"By Goods Receipt No: {hdr.GoodsReceiptNo}";

            // Map Dtls to TransactionDtls with proper Base Unit resolution
            var transDtls = new List<TransactionDtls>();
            foreach (var d in master.Details)
            {
                var itemMaster = await db.ItemMasters.AsNoTracking().FirstOrDefaultAsync(x => x.ItemId == d.FK_ItemId);

                transDtls.Add(new TransactionDtls
                {
                    FK_ItemId = d.FK_ItemId,
                    FK_UnitId = d.FK_UnitId,
                    Quantity = d.Quantity,
                    FK_StoreId = d.FK_StoreId ?? 0,
                    FK_ItemBaseUnit = itemMaster?.FK_UnitId ?? 0
                });
            }

            // Goods Receipt = Stock IN = true
            await _stockService.StockMovementForTransaction(db, transDtls, tHdr, hdr.GoodsReceiptDate.ToDateTime(), true);
            await db.SaveChangesAsync();
            await transaction.CommitAsync();

            return new GoodsReceiptDTO(hdr, savedDetails) { Id = hdr.GoodsReceiptId };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new GoodsReceiptDTO { Message = ex.Message };
        }
    }

    public async Task<GoodsReceiptDTO> Update(GoodsReceiptDTO master)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var existingHeader = await db.GoodsReceiptHdrs.FirstOrDefaultAsync(x => x.GoodsReceiptId == master.Header.GoodsReceiptId);
            if (existingHeader != null)
            {
                var hdr = master.Header.CopyTo<GoodsReceiptHdr>();

                // Handle optional FKs
                if (hdr.FK_PurchaserId == 0) hdr.FK_PurchaserId = null;

                db.Entry(existingHeader).CurrentValues.SetValues(hdr);

                var existingDetails = await db.GoodsReceiptDtls.Where(x => x.FK_GoodsReceiptHdrId == master.Header.GoodsReceiptId).ToListAsync();
                db.GoodsReceiptDtls.RemoveRange(existingDetails);

                var savedDetails = new List<GoodsReceiptDtls>();
                foreach (var item in master.Details)
                {
                    var dtl = item.CopyTo<GoodsReceiptDtls>();
                    dtl.FK_GoodsReceiptHdrId = hdr.GoodsReceiptId;

                    if (dtl.FK_StoreId == null || dtl.FK_StoreId == 0)
                    {
                        var defaultStore = await db.StoreMasters
                            .Where(x => x.FK_BranchId == hdr.FK_BranchId || x.FK_BranchId == null)
                            .FirstOrDefaultAsync(x => x.IsDefaultStore) ??
                            await db.StoreMasters.FirstOrDefaultAsync();
                        dtl.FK_StoreId = defaultStore?.StoreId;
                    }

                    await db.GoodsReceiptDtls.AddAsync(dtl);
                    savedDetails.Add(dtl);
                }

                await db.SaveChangesAsync();

                // Stock Movement (Full Replace for Update)
                // 1. Reverse Old
                await _stockService.ReverseStockMovements(existingHeader.GoodsReceiptNo, "Goods Receipt", existingHeader.FK_BranchId, db);

                // 2. Add New
                var tHdr = hdr.CopyTo<TransactionHdr>();
                tHdr.HdrId = hdr.GoodsReceiptId;
                tHdr.HdrNo = hdr.GoodsReceiptNo;
                tHdr.PartyId = hdr.FK_SupplierId;
                tHdr.VoucherType = "Goods Receipt";
                tHdr.Remarks = $"By Goods Receipt No: {hdr.GoodsReceiptNo} (Updated)";

                var transDtls = new List<TransactionDtls>();
                foreach (var d in master.Details)
                {
                    var itemMaster = await db.ItemMasters.AsNoTracking().FirstOrDefaultAsync(x => x.ItemId == d.FK_ItemId);
                    transDtls.Add(new TransactionDtls
                    {
                        FK_ItemId = d.FK_ItemId,
                        FK_UnitId = d.FK_UnitId,
                        Quantity = d.Quantity,
                        FK_StoreId = d.FK_StoreId ?? 0,
                        FK_ItemBaseUnit = itemMaster?.FK_UnitId ?? 0
                    });
                }

                await _stockService.StockMovementForTransaction(db, transDtls, tHdr, hdr.GoodsReceiptDate.ToDateTime(), true);
                await db.SaveChangesAsync();
                await transaction.CommitAsync();

                return new GoodsReceiptDTO(hdr, savedDetails) { Id = hdr.GoodsReceiptId };
            }
            return new GoodsReceiptDTO { Message = "Record not found" };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new GoodsReceiptDTO { Message = ex.Message };
        }
    }

    public async Task<GoodsReceiptDTO> Delete(int id)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var header = await db.GoodsReceiptHdrs.FirstOrDefaultAsync(x => x.GoodsReceiptId == id);
            if (header != null)
            {
                // Reverse Stock
                await _stockService.ReverseStockMovements(header.GoodsReceiptNo, "Goods Receipt", header.FK_BranchId, db);

                header.IsDeleted = true;
                var details = await db.GoodsReceiptDtls.Where(x => x.FK_GoodsReceiptHdrId == id).ToListAsync();
                details.ForEach(x => x.IsDeleted = true);
                await db.SaveChangesAsync();
                await transaction.CommitAsync();
                return new GoodsReceiptDTO { Id = id, Message = "Cancelled Successfully" };
            }
            return new GoodsReceiptDTO { Message = "Record not found" };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new GoodsReceiptDTO { Message = ex.Message };
        }
    }

    public async Task<GoodsReceiptDTO> GetById(int id)
    {
        await using var db = _contextProvider.CreateDbContext();
        var header = await db.GoodsReceiptHdrs
            .Include(x => x.Purchaser)
            .Include(x => x.Supplier)
            .Include(x => x.Branch)
            .FirstOrDefaultAsync(x => x.GoodsReceiptId == id);

        if (header != null)
        {
            var details = await db.GoodsReceiptDtls
                .Where(x => x.FK_GoodsReceiptHdrId == id)
                .Include(x => x.Unit)
                .ToListAsync();
            return new GoodsReceiptDTO(header, details);
        }
        return new GoodsReceiptDTO { Message = "Record not found" };
    }

    public async Task<TransactionPDTO> GetAllWithPagination(PaginationModel pagination)
    {
        await using var db = _contextProvider.CreateDbContext();
        var query = db.GoodsReceiptHdrs.AsNoTracking().Include(x => x.Supplier).Where(x => !x.IsDeleted && x.FK_BranchId == pagination.BranchId).AsQueryable();

        if (!string.IsNullOrEmpty(pagination.search))
        {
            var term = pagination.search.ToLower();
            query = query.Where(x => x.GoodsReceiptNo.Contains(term) || (x.Supplier != null && x.Supplier.Name.ToLower().Contains(term)));
        }

        var total = await query.CountAsync();
        query = query.OrderByDescending(x => x.GoodsReceiptId);

        var list = await query.Skip((pagination.page - 1) * pagination.pageSize).Take(pagination.pageSize)
            .ToListAsync();

        var finalList = list.Select(e => new ExMasters
        {
            Id = e.GoodsReceiptId,
            Name = e.Supplier?.Name ?? "",
            Code = e.GoodsReceiptNo,
            IsSelected = true,
            Amount = e.GrandTotal,
            ExtraField = e.RefNo,
            Description = e.GoodsReceiptDate.HasValue ? e.GoodsReceiptDate.Value.ToString("dd-MM-yyyy") : ""
        }).ToList();

        return new TransactionPDTO(finalList, total);
    }
}



