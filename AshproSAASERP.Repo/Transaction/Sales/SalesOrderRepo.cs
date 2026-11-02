using AshproSAASERP.Interface.Transaction.Sales;
using AshproSAASERP.Models.Transactions.Sales;
using AshproSAASERP.Models.ViewModel;
using AshproSAASERP.Repo.CodeGenerationServices;
using AshproStringExtension;

namespace AshproSAASERP.Repo.Transaction.Sales;

public class SalesOrderRepo : ISalesOrderRepo
{
    private readonly IDbContextProvider _contextProvider;
    private readonly CodeSequenceService _codeService;

    public SalesOrderRepo(IDbContextProvider contextProvider, CodeSequenceService codeService)
    {
        _contextProvider = contextProvider;
        _codeService = codeService;
    }

    public async Task<string> GetNextSalesOrderNoAsync()
    {
        await using var db = _contextProvider.CreateDbContext();
        return await _codeService.GetNextUniqueCodeAsync(
            CodeKeys.SalesOrder,
            async (code) => await db.SalesOrderHdrs.AnyAsync(x => x.SalesOrderNo == code)
        );
    }

    public async Task<OutputResponse> CreateAsync(SalesOrderDTO entity)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var hdr = entity.OrderHdr;
            var dtls = entity.OrderDtls;

            bool isAuto = await _codeService.IsAutoCodeEnabledAsync(CodeKeys.SalesOrder, db);
            if (!isAuto)
            {
                var next = await GetNextVchrNo(hdr.FK_BranchId);
                hdr.SalesOrderNo = next.ToString();
            }

            await db.SalesOrderHdrs.AddAsync(hdr);
            await db.SaveChangesAsync();

            if (isAuto)
            {
                await _codeService.ConsumeCodeAsync(CodeKeys.SalesOrder, db);
            }

            int headerId = hdr.SalesOrderId;
            var details = new List<SalesOrderDtls>();
            foreach (var item in dtls)
            {
                var dtl = item.CopyTo<SalesOrderDtls>();
                dtl.FK_SalesOrderHdrId = headerId;
                dtl.FK_StoreId = item.FK_StoreId == 0 ? null : item.FK_StoreId;
                details.Add(dtl);
            }
            await db.SalesOrderDtls.AddRangeAsync(details);
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
            await db.SalesOrderDtls.Where(x => x.FK_SalesOrderHdrId == id).ExecuteDeleteAsync();
            await db.SalesOrderHdrs.Where(x => x.SalesOrderId == id).ExecuteDeleteAsync();
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
        var query = db.SalesOrderHdrs.AsNoTracking().Include(x => x.Customer).Where(x => !x.IsDeleted).AsQueryable();

        if (!string.IsNullOrWhiteSpace(pagination.search))
        {
            var term = pagination.search.ToLower();
            query = query.Where(x => x.SalesOrderNo.ToLower().Contains(term) || (x.RefNo ?? string.Empty).ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(pagination.sortColumn))
        {
            bool desc = pagination.sortDirection == "desc";
            query = pagination.sortColumn switch
            {
                "Code" => desc ? query.OrderByDescending(x => x.SalesOrderNo) : query.OrderBy(x => x.SalesOrderNo),
                "Name" => desc ? query.OrderByDescending(x => x.Customer!.Name) : query.OrderBy(x => x.Customer!.Name),
                "Description" => desc ? query.OrderByDescending(x => x.SalesOrderDate) : query.OrderBy(x => x.SalesOrderDate),
                _ => desc ? query.OrderByDescending(x => x.SalesOrderId) : query.OrderBy(x => x.SalesOrderId)
            };
        }
        else
        {
            query = query.OrderByDescending(x => x.SalesOrderId);
        }

        int count = await query.CountAsync();
        var list = await query
            .Skip((pagination.page - 1) * pagination.pageSize)
            .Take(pagination.pageSize)
            .ToListAsync();
        var finalList = list.Where(x => x.FK_BranchId == pagination.BranchId).Select(e => new ExMasters
        {
            Id = e.SalesOrderId,
            Name = e.CustomerName,
            Code = e.SalesOrderNo,
            IsSelected = true,
            Amount = e.GrandTotal,
            ExtraField = e.RefNo,
            Description = e.SalesOrderDate.Value.ToString("dd-MM-yyyy")
        }).ToList();
        return new TransactionPDTO(finalList, count);
    }
    public async Task<SalesOrderDTO> GetAsync(int Id)
    {
        await using var db = _contextProvider.CreateDbContext();
        var hdr = await db.SalesOrderHdrs.AsNoTracking().FirstOrDefaultAsync(x => x.SalesOrderId == Id);
        if (hdr == null) return null!;
        if ((hdr.SalesQuotationId ?? 0) > 0)
        {
            hdr.QuotationNo = await db.SalesQuotationHdrs.AsNoTracking().Where(x => x.SalesQuotationId == hdr.SalesQuotationId).Select(x => x.SalesQuotationNo).FirstOrDefaultAsync();
        }
        var dtls = await db.SalesOrderDtls
            .AsNoTracking()
            .Where(x => x.FK_SalesOrderHdrId == Id)
            .ToListAsync();
        var mapped = dtls.Select(d => new TransactionDtls
        {
            DtlsId = d.SalesOrderDtlsId,
            HdrId = d.FK_SalesOrderHdrId,
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
        return new SalesOrderDTO(hdr, mapped);
    }
    public async Task<int> GetNextVchrNo(int BranchId)
    {
        try
        {
            await using var db = _contextProvider.CreateDbContext();
            var maxNo = await db.SalesOrderHdrs
                .AsNoTracking()
                .Where(x => x.FK_BranchId == BranchId)
                .OrderByDescending(x => x.SalesOrderId)
                .Select(x => x.SalesOrderNo)
                .FirstOrDefaultAsync();

            if (int.TryParse(maxNo, out int val)) return val + 1;
            return 1;
        }
        catch
        {
            return 1;
        }
    }
    public async Task<OutputResponse> UpdateAsync(SalesOrderDTO entity)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var hdr = entity.OrderHdr;
            var existing = await db.SalesOrderHdrs.FindAsync(hdr.SalesOrderId);
            if (existing is null) return new OutputResponse(0, "Record not found.");

            if (hdr.IsDeleted) // Cancellation Logic
            {
                existing.IsDeleted = true;

                // Mark Details as Deleted
                await db.SalesOrderDtls.Where(x => x.FK_SalesOrderHdrId == existing.SalesOrderId)
                      .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsDeleted, true));

                await db.SaveChangesAsync();
                await transaction.CommitAsync();
                return new OutputResponse(existing.SalesOrderId, "Cancelled Successfully", null);
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
            existing.SalesOrderDate = hdr.SalesOrderDate;
            existing.EntryDate = hdr.EntryDate;
            existing.SalesQuotationId = hdr.SalesQuotationId;

            // remove old details
            await db.SalesOrderDtls.Where(x => x.FK_SalesOrderHdrId == hdr.SalesOrderId).ExecuteDeleteAsync();

            var details = entity.OrderDtls?.Select(d => new SalesOrderDtls
            {
                FK_SalesOrderHdrId = hdr.SalesOrderId,
                FK_ItemId = d.FK_ItemId,
                ItemName = d.ItemName,
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
            }).ToList() ?? new List<SalesOrderDtls>();

            if (details.Count > 0)
            {
                await db.SalesOrderDtls.AddRangeAsync(details);
            }

            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return new OutputResponse(existing.SalesOrderId, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new OutputResponse(0, ex.Message);
        }
    }
}



