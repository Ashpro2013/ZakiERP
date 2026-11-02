using AshproSAASERP.Interface.Transaction.Inventory;
using AshproSAASERP.Models.ViewModel;
using AshproStringExtension;

namespace AshproSAASERP.Repo.Transaction.Inventory;

public class DamageStockRepo : IDamageStockRepo
{
    private readonly IDbContextProvider _contextProvider;
    private readonly StockService _stockService;

    public DamageStockRepo(IDbContextProvider contextProvider, StockService stockService)
    {
        _contextProvider = contextProvider;
        _stockService = stockService;
    }

    public async Task<string> GetNextVoucherNo(int branchId)
    {
        try
        {
            await using var db = _contextProvider.CreateDbContext();
            var maxNo = await db.DamageStockHdrs
                .AsNoTracking()
                .Where(x => x.FK_BranchId == branchId && x.DamageStockNo != null)
                .OrderByDescending(x => x.DamageStockId)
                .Select(x => x.DamageStockNo)
                .FirstOrDefaultAsync();

            if (maxNo != null && int.TryParse(maxNo, out int val))
            {
                return (val + 1).ToString();
            }
            return "1001";
        }
        catch
        {
            return "1001";
        }
    }

    public async Task<DamageStockDTO> Create(DamageStockDTO master)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            await db.DamageStockHdrs.AddAsync(master.Header);
            await db.SaveChangesAsync();

            foreach (var item in master.Details)
            {
                item.FK_DamageStockHdrId = master.Header.DamageStockId;
                item.Unit = null;
                item.Item = null;
                item.Store = null;
                item.DamageStockHdr = null;
                await db.DamageStockDtls.AddAsync(item);
            }
            await db.SaveChangesAsync();

            // Stock Movement
            var defaultStore = await db.StoreMasters.FirstOrDefaultAsync(x => x.FK_BranchId == master.Header.FK_BranchId)
                               ?? await db.StoreMasters.FirstOrDefaultAsync();
            int defStoreId = defaultStore?.StoreId ?? 1;

            var tHdr = master.Header.CopyTo<TransactionHdr>();
            tHdr.HdrId = master.Header.DamageStockId;
            tHdr.HdrNo = master.Header.DamageStockNo;
            tHdr.PartyId = 0;
            tHdr.FK_StoreId = defStoreId;
            tHdr.VoucherType = "Damage Stock";
            tHdr.Remarks = $"By Damage Stock No: {master.Header.DamageStockNo}";

            // Map Dtls to TransactionDtls
            // Optimize: Fetch all ItemMasters in one go
            var itemIds = master.Details.Select(x => x.FK_ItemId).Distinct().ToList();
            var itemsMap = await db.ItemMasters
                                         .AsNoTracking()
                                         .Include(x => x.MultiUnits)
                                         .Where(x => itemIds.Contains(x.ItemId))
                                         .ToDictionaryAsync(x => x.ItemId);

            var transDtls = new List<TransactionDtls>();
            foreach (var d in master.Details)
            {
                var itemMaster = itemsMap.GetValueOrDefault(d.FK_ItemId);
                int itemBaseUnit = itemMaster?.FK_UnitId ?? 0;

                decimal qtyPerUnit = d.QtyPerUnit;
                if (qtyPerUnit <= 0)
                {
                    if (d.FK_UnitId == itemBaseUnit)
                    {
                        qtyPerUnit = 1;
                    }
                    else
                    {
                        var conversion = itemMaster?.MultiUnits?.FirstOrDefault(x => x.FK_UnitId == d.FK_UnitId);
                        qtyPerUnit = conversion?.QTYPerUnit ?? 1;
                    }
                }

                transDtls.Add(new TransactionDtls
                {
                    FK_ItemId = d.FK_ItemId,
                    FK_UnitId = d.FK_UnitId,
                    Quantity = d.Quantity,
                    QtyPerUnit = qtyPerUnit,
                    FK_StoreId = (d.FK_StoreId == null || d.FK_StoreId == 0) ? defStoreId : d.FK_StoreId.Value,
                    FK_ItemBaseUnit = itemBaseUnit
                });
            }

            // Damage Stock = Stock OUT = false
            await _stockService.StockMovementForTransaction(db, transDtls, tHdr, master.Header.EntryDate.ToDateTime(), false);
            await db.SaveChangesAsync();
            await transaction.CommitAsync();

            return new DamageStockDTO(master.Header, master.Details) { Id = master.Header.DamageStockId };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new DamageStockDTO { Message = ex.Message };
        }
    }

    public async Task<DamageStockDTO> Update(DamageStockDTO master)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var existingHeader = await db.DamageStockHdrs.FirstOrDefaultAsync(x => x.DamageStockId == master.Header.DamageStockId);
            if (existingHeader != null)
            {
                string oldNo = existingHeader.DamageStockNo;
                db.Entry(existingHeader).CurrentValues.SetValues(master.Header);

                var existingDetails = await db.DamageStockDtls.Where(x => x.FK_DamageStockHdrId == master.Header.DamageStockId).ToListAsync();
                db.DamageStockDtls.RemoveRange(existingDetails);

                foreach (var item in master.Details)
                {
                    item.FK_DamageStockHdrId = master.Header.DamageStockId;
                    item.DamageStockHdr = null;
                    item.DamageStockDtlsId = 0;
                    item.Unit = null;
                    item.Item = null;
                    item.Store = null;
                    await db.DamageStockDtls.AddAsync(item);
                }

                await db.SaveChangesAsync();

                // Stock Movement
                // 1. Reverse Old
                await _stockService.ReverseStockMovements(oldNo, "Damage Stock", existingHeader.FK_BranchId, db);

                // 2. Add New
                var defaultStore = await db.StoreMasters.FirstOrDefaultAsync(x => x.FK_BranchId == master.Header.FK_BranchId)
                                   ?? await db.StoreMasters.FirstOrDefaultAsync();
                int defStoreId = defaultStore?.StoreId ?? 1;

                var tHdr = master.Header.CopyTo<TransactionHdr>();
                tHdr.HdrId = master.Header.DamageStockId;
                tHdr.HdrNo = master.Header.DamageStockNo;
                tHdr.PartyId = 0;
                tHdr.FK_StoreId = defStoreId;
                tHdr.VoucherType = "Damage Stock";
                tHdr.Remarks = $"By Damage Stock No: {master.Header.DamageStockNo} (Updated)";

                var transDtls = new List<TransactionDtls>();

                // Optimize: Fetch all ItemMasters in one go
                var itemIds = master.Details.Select(x => x.FK_ItemId).Distinct().ToList();
                var itemsMap = await db.ItemMasters
                                             .AsNoTracking()
                                             .Include(x => x.MultiUnits)
                                             .Where(x => itemIds.Contains(x.ItemId))
                                             .ToDictionaryAsync(x => x.ItemId);

                foreach (var d in master.Details)
                {
                    var itemMaster = itemsMap.GetValueOrDefault(d.FK_ItemId);
                    int itemBaseUnit = itemMaster?.FK_UnitId ?? 0;

                    decimal qtyPerUnit = d.QtyPerUnit;
                    if (qtyPerUnit <= 0)
                    {
                        if (d.FK_UnitId == itemBaseUnit)
                        {
                            qtyPerUnit = 1;
                        }
                        else
                        {
                            var conversion = itemMaster?.MultiUnits?.FirstOrDefault(x => x.FK_UnitId == d.FK_UnitId);
                            qtyPerUnit = conversion?.QTYPerUnit ?? 1;
                        }
                    }

                    transDtls.Add(new TransactionDtls
                    {
                        FK_ItemId = d.FK_ItemId,
                        FK_UnitId = d.FK_UnitId,
                        Quantity = d.Quantity,
                        QtyPerUnit = qtyPerUnit,
                        FK_StoreId = (d.FK_StoreId == null || d.FK_StoreId == 0) ? defStoreId : d.FK_StoreId.Value,
                        FK_ItemBaseUnit = itemBaseUnit
                    });
                }

                await _stockService.StockMovementForTransaction(db, transDtls, tHdr, master.Header.EntryDate.ToDateTime(), false);
                await db.SaveChangesAsync();
                await transaction.CommitAsync();

                return new DamageStockDTO(master.Header, master.Details) { Id = master.Header.DamageStockId };
            }
            return new DamageStockDTO { Message = "Record not found" };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new DamageStockDTO { Message = ex.Message };
        }
    }

    public async Task<DamageStockDTO> Delete(int id)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var header = await db.DamageStockHdrs.FirstOrDefaultAsync(x => x.DamageStockId == id);
            if (header != null)
            {
                // Reverse Stock
                await _stockService.ReverseStockMovements(header.DamageStockNo, "Damage Stock", header.FK_BranchId, db);

                header.IsDeleted = true;
                var details = await db.DamageStockDtls.Where(x => x.FK_DamageStockHdrId == id).ToListAsync();
                details.ForEach(x => x.IsDeleted = true);
                await db.SaveChangesAsync();
                await transaction.CommitAsync();
                return new DamageStockDTO { Id = id, Message = "Deleted Successfully" };
            }
            return new DamageStockDTO { Message = "Record not found" };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new DamageStockDTO { Message = ex.Message };
        }
    }

    public async Task<DamageStockDTO> GetById(int id)
    {
        await using var db = _contextProvider.CreateDbContext();
        var header = await db.DamageStockHdrs
            .Include(x => x.Project)
            .Include(x => x.Account)
            .Include(x => x.Branch)
            .FirstOrDefaultAsync(x => x.DamageStockId == id);

        if (header != null)
        {
            var details = await db.DamageStockDtls
                .Where(x => x.FK_DamageStockHdrId == id)
                .Include(x => x.Unit)
                .ToListAsync();
            return new DamageStockDTO(header, details);
        }
        return new DamageStockDTO { Message = "Record not found" };
    }

    public async Task<TransactionPDTO> GetAllWithPagination(PaginationModel pagination)
    {
        await using var db = _contextProvider.CreateDbContext();
        var query = db.DamageStockHdrs.AsNoTracking()
            .Include(x => x.Account)
            .Where(x => !x.IsDeleted).AsQueryable();

        if (pagination.BranchId > 0)
        {
            query = query.Where(x => x.FK_BranchId == pagination.BranchId || x.FK_BranchId == 0);
        }

        if (!string.IsNullOrEmpty(pagination.search))
        {
            var term = pagination.search.ToLower();
            query = query.Where(x => x.DamageStockNo.ToLower().Contains(term)
                                     || (x.Remarks != null && x.Remarks.ToLower().Contains(term))
                                     || (x.Account != null && (x.Account.Name.ToLower().Contains(term) || (x.Account.NameArabic != null && x.Account.NameArabic.ToLower().Contains(term)))));
        }

        if (!string.IsNullOrWhiteSpace(pagination.sortColumn))
        {
            bool desc = pagination.sortDirection == "desc";
            query = pagination.sortColumn switch
            {
                "Code" => desc ? query.OrderByDescending(x => x.DamageStockNo) : query.OrderBy(x => x.DamageStockNo),
                "Name" => desc ? query.OrderByDescending(x => x.Account!.Name) : query.OrderBy(x => x.Account!.Name),
                "Description" => desc ? query.OrderByDescending(x => x.EntryDate) : query.OrderBy(x => x.EntryDate),
                _ => desc ? query.OrderByDescending(x => x.DamageStockId) : query.OrderBy(x => x.DamageStockId)
            };
        }
        else
        {
            query = query.OrderByDescending(x => x.DamageStockId);
        }

        var total = await query.CountAsync();

        var list = await query.Skip((pagination.page - 1) * pagination.pageSize).Take(pagination.pageSize)
            .ToListAsync();

        var finalList = list.Select(x => new ExMasters
        {
            Id = x.DamageStockId,
            Code = x.DamageStockNo,
            Name = x.Account != null ? x.Account.Name : "",
            Description = x.EntryDate.HasValue ? x.EntryDate.Value.ToString("dd-MM-yyyy") : "",
            Amount = x.TotalValue,
            IsSelected = true
        })
            .ToList();

        return new TransactionPDTO(finalList, total);
    }
}



