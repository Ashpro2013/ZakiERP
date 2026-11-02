using AshproSAASERP.Interface.Transaction.Purchase;
using AshproSAASERP.Models.Transactions.Purchase;
using AshproSAASERP.Models.ViewModel;
using AshproSAASERP.Repo.CodeGenerationServices;
using AshproStringExtension;

namespace AshproSAASERP.Repo.Transaction.Purchase;

public class PurchaseOrderRepo : IPurchaseOrderRepo
{
    private readonly IDbContextProvider _contextProvider;
    private readonly CodeSequenceService _codeService;

    public PurchaseOrderRepo(IDbContextProvider contextProvider, CodeSequenceService codeService)
    {
        _contextProvider = contextProvider;
        _codeService = codeService;
    }

    public async Task<string> GetNextPurchaseOrderNoAsync()
    {
        await using var db = _contextProvider.CreateDbContext();
        return await _codeService.GetNextUniqueCodeAsync(
            CodeKeys.PurchaseOrder,
            async (code) => await db.PurchaseOrderHdrs.AnyAsync(x => x.PurchaseOrderNo == code)
        );
    }

    public async Task<OutputResponse> CreateAsync(PurchaseOrderDTO entity)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var hdr = entity.PurchaseOdrHdr;
            var dtls = entity.PurchaseOdrDtls;

            bool isAuto = await _codeService.IsAutoCodeEnabledAsync(CodeKeys.PurchaseOrder, db);
            if (!isAuto)
            {
                var next = await GetNextVchrNo(hdr.FK_BranchId);
                hdr.PurchaseOrderNo = next.ToString();
            }

            await db.PurchaseOrderHdrs.AddAsync(hdr);
            await db.SaveChangesAsync();

            if (isAuto)
            {
                await _codeService.ConsumeCodeAsync(CodeKeys.PurchaseOrder, db);
            }

            int headerId = hdr.PurchaseOrderId;
            var details = new List<PurchaseOrderDtls>();
            foreach (var item in dtls)
            {
                var dtl = item.CopyTo<PurchaseOrderDtls>();
                dtl.FK_PurchaseOrderHdrId = headerId;
                dtl.FK_StoreId = item.FK_StoreId == 0 ? null : item.FK_StoreId;
                details.Add(dtl);
            }
            await db.PurchaseOrderDtls.AddRangeAsync(details);
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
            await db.PurchaseOrderDtls.Where(x => x.FK_PurchaseOrderHdrId == id).ExecuteDeleteAsync();
            await db.PurchaseOrderHdrs.Where(x => x.PurchaseOrderId == id).ExecuteDeleteAsync();
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
        var query = db.PurchaseOrderHdrs.AsNoTracking().Include(x => x.Supplier).Where(x => !x.IsDeleted).AsQueryable();

        if (!string.IsNullOrWhiteSpace(pagination.search))
        {
            var term = pagination.search.ToLower();
            query = query.Where(x => x.PurchaseOrderNo.ToLower().Contains(term) || (x.RefNo ?? string.Empty).ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(pagination.sortColumn))
        {
            bool desc = pagination.sortDirection == "desc";
            query = pagination.sortColumn switch
            {
                "Code" => desc ? query.OrderByDescending(x => x.PurchaseOrderNo) : query.OrderBy(x => x.PurchaseOrderNo),
                "Name" => desc ? query.OrderByDescending(x => x.Supplier!.Name) : query.OrderBy(x => x.Supplier!.Name),
                "Description" => desc ? query.OrderByDescending(x => x.PurchaseOrderDate) : query.OrderBy(x => x.PurchaseOrderDate),
                _ => desc ? query.OrderByDescending(x => x.PurchaseOrderId) : query.OrderBy(x => x.PurchaseOrderId)
            };
        }
        else
        {
            query = query.OrderByDescending(x => x.PurchaseOrderId);
        }

        int count = await query.CountAsync();
        var list = await query
            .Skip((pagination.page - 1) * pagination.pageSize)
            .Take(pagination.pageSize)
            .ToListAsync();
        var finalList = list.Where(x => x.FK_BranchId == pagination.BranchId).Select(e => new ExMasters
        {
            Id = e.PurchaseOrderId,
            Name = e.Supplier!.Name,
            Code = e.PurchaseOrderNo,
            IsSelected = true,
            Amount = e.GrandTotal,
            ExtraField = e.RefNo,
            Description = e.PurchaseOrderDate.Value.ToString("dd-MM-yyyy")
        }).ToList();
        return new TransactionPDTO(finalList, count);
    }

    public async Task<PurchaseOrderDTO> GetAsync(int id)
    {
        await using var db = _contextProvider.CreateDbContext();
        var hdr = await db.PurchaseOrderHdrs.AsNoTracking().FirstOrDefaultAsync(x => x.PurchaseOrderId == id);
        if (hdr == null) return null!;
        var dtls = await db.PurchaseOrderDtls
            .AsNoTracking()
            .Include(x => x.Unit)
            .Where(x => x.FK_PurchaseOrderHdrId == id)
            .ToListAsync();
        var mapped = dtls.Select(d => new TransactionDtls
        {
            DtlsId = d.PurchaseOrderDtlsId,
            HdrId = d.FK_PurchaseOrderHdrId,
            FK_ItemId = d.FK_ItemId,
            ItemName = d.ItemName,
            Barcode = d.Barcode,
            FK_UnitId = d.FK_UnitId,
            UnitName = d.Unit!.Name,
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

        return new PurchaseOrderDTO(hdr, mapped);
    }

    public async Task<int> GetNextVchrNo(int branchId)
    {
        try
        {
            await using var db = _contextProvider.CreateDbContext();
            var maxNo = await db.PurchaseOrderHdrs
                .AsNoTracking()
                .Where(x => x.FK_BranchId == branchId)
                .OrderByDescending(x => x.PurchaseOrderId)
                .Select(x => x.PurchaseOrderNo)
                .FirstOrDefaultAsync();

            if (int.TryParse(maxNo, out int val)) return val + 1;
            return 1;
        }
        catch
        {
            return 1;
        }
    }

    public async Task<OutputResponse> UpdateAsync(PurchaseOrderDTO entity)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var hdr = entity.PurchaseOdrHdr;
            var existing = await db.PurchaseOrderHdrs.FindAsync(hdr.PurchaseOrderId);
            if (existing is null) return new OutputResponse(0, "Record not found.");

            if (hdr.IsDeleted) // Cancellation Logic
            {
                existing.IsDeleted = true;

                // Mark Details as Deleted
                await db.PurchaseOrderDtls.Where(x => x.FK_PurchaseOrderHdrId == existing.PurchaseOrderId)
                      .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsDeleted, true));

                await db.SaveChangesAsync();
                await transaction.CommitAsync();
                return new OutputResponse(existing.PurchaseOrderId, "Cancelled Successfully", null);
            }

            existing.RefNo = hdr.RefNo;
            existing.GrossAmount = hdr.GrossAmount;
            existing.LineDsicount = hdr.LineDsicount;
            existing.TaxableAmount = hdr.TaxableAmount;
            existing.TaxTotal = hdr.TaxTotal;
            existing.NetAmount = hdr.NetAmount;
            existing.GrandTotal = hdr.GrandTotal;
            existing.Remarks = hdr.Remarks;
            // existing.IsDeleted = hdr.IsDeleted; // Handled above
            existing.FK_BranchId = hdr.FK_BranchId;
            existing.FK_SupplierId = hdr.FK_SupplierId;
            existing.FK_UserId = hdr.FK_UserId;
            existing.PurchaseOrderDate = hdr.PurchaseOrderDate;
            existing.EntryDate = hdr.EntryDate;

            // remove old details
            await db.PurchaseOrderDtls.Where(x => x.FK_PurchaseOrderHdrId == hdr.PurchaseOrderId).ExecuteDeleteAsync();

            var details = entity.PurchaseOdrDtls?.Select(d => new PurchaseOrderDtls
            {
                FK_PurchaseOrderHdrId = hdr.PurchaseOrderId,
                FK_ItemId = d.FK_ItemId,
                ItemName = d.ItemName,
                FK_UnitId = d.FK_UnitId,
                Barcode = d.Barcode,
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
            }).ToList() ?? new List<PurchaseOrderDtls>();

            if (details.Count > 0)
            {
                await db.PurchaseOrderDtls.AddRangeAsync(details);
            }

            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return new OutputResponse(existing.PurchaseOrderId, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new OutputResponse(0, ex.Message);
        }
    }
}



