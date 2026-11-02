using ZakiSAASERP.Interface.Transaction.Sales;
using ZakiSAASERP.Models.Transactions.Sales;
using ZakiSAASERP.Models.ViewModel;
using ZakiSAASERP.Repo.Transaction.Inventory;
using AshproStringExtension;

namespace ZakiSAASERP.Repo.Transaction.Sales;

public class DeliveryNoteRtnRepo : IDeliveryNoteRtnRepo
{
    private readonly IDbContextProvider _contextProvider;
    private readonly StockService _stockService;

    public DeliveryNoteRtnRepo(IDbContextProvider contextProvider, StockService stockService)
    {
        _contextProvider = contextProvider;
        _stockService = stockService;
    }
    public async Task<OutputResponse> CreateAsync(DeliveryNoteRtnDTO entity)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var hdr = entity.DeliveryNoteRtnHdr;
            var dtls = entity.DeliveryNoteRtnDtls;
            var next = await GetNextVchrNo(hdr.FK_BranchId);
            hdr.DeliveryNoteRtnNo = next.ToString();
            if (hdr.FK_StoreId == 0)
                hdr.FK_StoreId = (await db.StoreMasters.FirstOrDefaultAsync())!.StoreId;
            await db.DeliveryNoteRtnHdrs.AddAsync(hdr);
            await db.SaveChangesAsync();
            int headerId = hdr.DeliveryNoteRtnId;
            var details = new List<DeliveryNoteRtnDtls>();
            foreach (var item in dtls)
            {
                var itemMaster = await db.ItemMasters.AsNoTracking().Include(x => x.MultiUnits).FirstOrDefaultAsync(x => x.ItemId == item.FK_ItemId);
                item.FK_ItemBaseUnit = itemMaster?.FK_UnitId ?? 0;

                // Resolve QtyPerUnit if missing or invalid
                if (item.QtyPerUnit <= 0)
                {
                    if (item.FK_UnitId == item.FK_ItemBaseUnit)
                    {
                        item.QtyPerUnit = 1;
                    }
                    else
                    {
                        var conversion = itemMaster?.MultiUnits?.FirstOrDefault(x => x.FK_UnitId == item.FK_UnitId);
                        item.QtyPerUnit = conversion?.QTYPerUnit ?? 1;
                    }
                }
                var dtl = item.CopyTo<DeliveryNoteRtnDtls>();
                dtl.FK_DeliveryNoteRtnHdrId = headerId;
                dtl.FK_StoreId = item.FK_StoreId == 0 ? null : item.FK_StoreId;
                details.Add(dtl);
            }
            await db.DeliveryNoteRtnDtls.AddRangeAsync(details);
            var tHdr = hdr.CopyTo<TransactionHdr>();
            tHdr.HdrId = headerId;
            tHdr.HdrNo = hdr.DeliveryNoteRtnNo;
            tHdr.PartyId = hdr.FK_CustomerId;
            tHdr.VoucherType = "Delivery Note Return";
            tHdr.Remarks = $"By Delivery Note Return No: {hdr.DeliveryNoteRtnNo}";
            await _stockService.StockMovementForTransaction(db, dtls, tHdr, hdr.DeliveryNoteRtnDate.ToDateTime(), true);
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return new OutputResponse(headerId, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new OutputResponse(0, ex.Message);
        }
    }

    public async Task<OutputResponse> DeleteAsync(int id)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            if (id == 0) return new OutputResponse(0, "Invalid identifier.");
            var hdr = await db.DeliveryNoteRtnHdrs.AsNoTracking().FirstOrDefaultAsync(x => x.DeliveryNoteRtnId == id);
            if (hdr != null)
            {
                await _stockService.ReverseStockMovements(hdr.DeliveryNoteRtnNo, "Delivery Note Return", hdr.FK_BranchId, db);
            }

            await db.DeliveryNoteRtnDtls.Where(x => x.FK_DeliveryNoteRtnHdrId == id).ExecuteDeleteAsync();
            await db.DeliveryNoteRtnHdrs.Where(x => x.DeliveryNoteRtnId == id).ExecuteDeleteAsync();
            await transaction.CommitAsync();
            return new OutputResponse(id, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new OutputResponse(0, ex.Message);
        }
    }

    public async Task<TransactionPDTO> GetAllWithPagination(PaginationModel pagination)
    {
        await using var db = _contextProvider.CreateDbContext();
        var query = db.DeliveryNoteRtnHdrs.AsNoTracking().Include(x => x.Customer).Where(x => !x.IsDeleted).AsQueryable();

        if (!string.IsNullOrWhiteSpace(pagination.search))
        {
            var term = pagination.search.ToLower();
            query = query.Where(x => x.DeliveryNoteRtnNo.ToLower().Contains(term) || (x.RefNo ?? string.Empty).ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(pagination.sortColumn))
        {
            bool desc = pagination.sortDirection == "desc";
            query = pagination.sortColumn switch
            {
                "Code" => desc ? query.OrderByDescending(x => x.DeliveryNoteRtnNo) : query.OrderBy(x => x.DeliveryNoteRtnNo),
                "Name" => desc ? query.OrderByDescending(x => x.Customer!.Name) : query.OrderBy(x => x.Customer!.Name),
                "Description" => desc ? query.OrderByDescending(x => x.DeliveryNoteRtnDate) : query.OrderBy(x => x.DeliveryNoteRtnDate),
                _ => desc ? query.OrderByDescending(x => x.DeliveryNoteRtnId) : query.OrderBy(x => x.DeliveryNoteRtnId)
            };
        }
        else
        {
            query = query.OrderByDescending(x => x.DeliveryNoteRtnId);
        }

        int count = await query.CountAsync();
        var list = await query
            .Skip((pagination.page - 1) * pagination.pageSize)
            .Take(pagination.pageSize)
            .ToListAsync();
        var finalList = list.Where(x => x.FK_BranchId == pagination.BranchId).Select(e => new ExMasters
        {
            Id = e.DeliveryNoteRtnId,
            Name = e.CustomerName,
            Code = e.DeliveryNoteRtnNo,
            IsSelected = true,
            Amount = e.GrandTotal,
            ExtraField = e.RefNo,
            Description = e.DeliveryNoteRtnDate.HasValue ? e.DeliveryNoteRtnDate.Value.ToString("dd-MM-yyyy") : ""
        }).ToList();
        return new TransactionPDTO(finalList, count);
    }

    public async Task<DeliveryNoteRtnDTO> GetAsync(int Id)
    {
        await using var db = _contextProvider.CreateDbContext();
        var hdr = await db.DeliveryNoteRtnHdrs.AsNoTracking().FirstOrDefaultAsync(x => x.DeliveryNoteRtnId == Id);
        if (hdr == null) return null!;
        if (hdr.DeliveryNoteId > 0)
        {
            var dn = await db.DeliveryNoteHdrs.AsNoTracking()
                             .Select(x => new { x.DeliveryNoteId, x.DeliveryNoteNo })
                             .FirstOrDefaultAsync(x => x.DeliveryNoteId == hdr.DeliveryNoteId);
            if (dn != null)
            {
                hdr.DeliveryNoteNo = dn.DeliveryNoteNo;
            }
        }
        var dtls = await db.DeliveryNoteRtnDtls
            .AsNoTracking()
            .Where(x => x.FK_DeliveryNoteRtnHdrId == Id)
            .ToListAsync();
        var mapped = dtls.Select(d => new TransactionDtls
        {
            DtlsId = d.DeliveryNoteRtnDtlsId,
            HdrId = d.FK_DeliveryNoteRtnHdrId,
            FK_ItemId = d.FK_ItemId,
            ItemName = d.ItemName,
            Barcode = d.Barcode,
            FK_UnitId = d.FK_UnitId,
            Quantity = d.Quantity,
            QtyPerUnit = d.QtyPerUnit,
            ExRate = d.ExRate,
            InRate = d.InRate,
            GrossAmount = d.GrossAmount,
            DiscountPerc = d.DiscountPerc,
            DiscountAmnt = d.DiscountAmnt,
            TaxableAmnt = d.TaxableAmnt,
            TaxPerc = d.TaxPerc,
            TaxAmount = d.TaxAmount,
            LineTotal = d.LineTotal,
            IsDeleted = d.IsDeleted,
            FK_StoreId = d.FK_StoreId ?? 0
        }).ToList();
        return new DeliveryNoteRtnDTO(hdr, mapped);
    }

    public async Task<int> GetNextVchrNo(int BranchId)
    {
        try
        {
            await using var db = _contextProvider.CreateDbContext();
            var maxNo = await db.DeliveryNoteRtnHdrs
                .AsNoTracking()
                .Where(x => x.FK_BranchId == BranchId)
                .OrderByDescending(x => x.DeliveryNoteRtnId)
                .Select(x => x.DeliveryNoteRtnNo)
                .FirstOrDefaultAsync();

            if (int.TryParse(maxNo, out int val)) return val + 1;
            return 1;
        }
        catch
        {
            return 1;
        }
    }

    public async Task<OutputResponse> UpdateAsync(DeliveryNoteRtnDTO entity)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var hdr = entity.DeliveryNoteRtnHdr;
            var existing = await db.DeliveryNoteRtnHdrs.FindAsync(hdr.DeliveryNoteRtnId);
            if (existing is null) return new OutputResponse(0, "Record not found.");

            if (hdr.IsDeleted) // Cancellation Logic
            {
                existing.IsDeleted = true;

                // Reverse Stock
                await _stockService.ReverseStockMovements(existing.DeliveryNoteRtnNo, "Delivery Note Return", existing.FK_BranchId, db);

                // Mark Details as Deleted
                await db.DeliveryNoteRtnDtls.Where(x => x.FK_DeliveryNoteRtnHdrId == existing.DeliveryNoteRtnId)
                      .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsDeleted, true));

                await db.SaveChangesAsync();
                await transaction.CommitAsync();
                return new OutputResponse(existing.DeliveryNoteRtnId, "Cancelled Successfully", null);
            }

            existing.RefNo = hdr.RefNo;
            existing.GrossAmount = hdr.GrossAmount;
            existing.LineDsicount = hdr.LineDsicount;
            existing.TaxableAmount = hdr.TaxableAmount;
            existing.TaxTotal = hdr.TaxTotal;
            existing.NetAmount = hdr.NetAmount;
            existing.AdditionalAmount = hdr.AdditionalAmount;
            existing.Freight = hdr.Freight;
            existing.AdditionalDiscPertge = hdr.AdditionalDiscPertge;
            existing.AdditionalDiscAmnt = hdr.AdditionalDiscAmnt;
            existing.AdditionalDiscountType = hdr.AdditionalDiscountType;
            existing.GrandTotal = hdr.GrandTotal;
            existing.Remarks = hdr.Remarks;
            // existing.IsDeleted = hdr.IsDeleted; // Handled above
            existing.FK_BranchId = hdr.FK_BranchId;
            existing.FK_CustomerId = hdr.FK_CustomerId;
            existing.CustomerName = hdr.CustomerName;
            existing.FK_SalesManId = hdr.FK_SalesManId;
            existing.FK_UserId = hdr.FK_UserId;
            existing.FK_FinancialYearId = hdr.FK_FinancialYearId;
            existing.FK_ProjectId = hdr.FK_ProjectId;
            existing.DeliveryNoteRtnDate = hdr.DeliveryNoteRtnDate;
            existing.EntryDate = hdr.EntryDate;

            // remove old details
            // remove old details
            // Reverse Stock
            await _stockService.ReverseStockMovements(existing.DeliveryNoteRtnNo, "Delivery Note Return", existing.FK_BranchId, db);
            await db.DeliveryNoteRtnDtls.Where(x => x.FK_DeliveryNoteRtnHdrId == hdr.DeliveryNoteRtnId).ExecuteDeleteAsync();

            var details = entity.DeliveryNoteRtnDtls?.Select(d => new DeliveryNoteRtnDtls
            {
                FK_DeliveryNoteRtnHdrId = hdr.DeliveryNoteRtnId,
                FK_ItemId = d.FK_ItemId,
                ItemName = d.ItemName,
                Barcode = d.Barcode,
                FK_UnitId = d.FK_UnitId,
                Quantity = d.Quantity,
                QtyPerUnit = d.QtyPerUnit,
                ExRate = d.ExRate,
                InRate = d.InRate,
                GrossAmount = d.GrossAmount,
                DiscountPerc = d.DiscountPerc,
                DiscountAmnt = d.DiscountAmnt,
                TaxableAmnt = d.TaxableAmnt,
                TaxPerc = d.TaxPerc,
                TaxAmount = d.TaxAmount,
                LineTotal = d.LineTotal,
                IsDeleted = d.IsDeleted,
                FK_StoreId = d.FK_StoreId == 0 ? null : d.FK_StoreId
            }).ToList() ?? new List<DeliveryNoteRtnDtls>();

            if (details.Count > 0)
            {
                await db.DeliveryNoteRtnDtls.AddRangeAsync(details);
            }

            // Populate Base Unit and Resolve QtyPerUnit for Stock Movement
            if (entity.DeliveryNoteRtnDtls != null)
            {
                foreach (var item in entity.DeliveryNoteRtnDtls)
                {
                    var itemMaster = await db.ItemMasters.AsNoTracking().Include(x => x.MultiUnits).FirstOrDefaultAsync(x => x.ItemId == item.FK_ItemId);
                    item.FK_ItemBaseUnit = itemMaster?.FK_UnitId ?? 0;

                    // Resolve QtyPerUnit if missing or invalid
                    if (item.QtyPerUnit <= 0)
                    {
                        if (item.FK_UnitId == item.FK_ItemBaseUnit)
                        {
                            item.QtyPerUnit = 1;
                        }
                        else
                        {
                            var conversion = itemMaster?.MultiUnits?.FirstOrDefault(x => x.FK_UnitId == item.FK_UnitId);
                            item.QtyPerUnit = conversion?.QTYPerUnit ?? 1;
                        }
                    }
                }
            }

            // Process stock movements for updated transaction
            var tHdr = existing.CopyTo<TransactionHdr>();
            tHdr.HdrId = existing.DeliveryNoteRtnId;
            tHdr.HdrNo = existing.DeliveryNoteRtnNo;
            tHdr.PartyId = existing.FK_CustomerId;
            tHdr.VoucherType = "Delivery Note Return";
            tHdr.Remarks = $"By Delivery Note Return No: {existing.DeliveryNoteRtnNo}";
            await _stockService.StockMovementForTransaction(db, entity.DeliveryNoteRtnDtls, tHdr, existing.DeliveryNoteRtnDate.ToDateTime(), true);

            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return new OutputResponse(existing.DeliveryNoteRtnId, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new OutputResponse(0, ex.Message);
        }
    }
}



