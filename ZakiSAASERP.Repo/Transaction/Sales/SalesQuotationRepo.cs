using ZakiSAASERP.Interface.Transaction.Sales;
using ZakiSAASERP.Models.Transactions.Sales;
using ZakiSAASERP.Models.ViewModel;
using ZakiSAASERP.Repo.CodeGenerationServices;
using AshproStringExtension;

namespace ZakiSAASERP.Repo.Transaction.Sales;

public class SalesQuotationRepo(IDbContextProvider _contextProvider, CodeSequenceService _codeService) : ISalesQuotationRepo
{
    public async Task<string> GetNextQuotationNoAsync()
    {
        await using var db = _contextProvider.CreateDbContext();
        return await _codeService.GetNextUniqueCodeAsync(
            CodeKeys.Quotation,
            async (code) => await db.SalesQuotationHdrs.AnyAsync(x => x.SalesQuotationNo == code)
        );
    }

    public async Task<OutputResponse> CreateAsync(SalesQuotationDTO entity)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var hdr = entity.QuotationHdr;
            var dtls = entity.QuotationDtls;

            bool isAuto = await _codeService.IsAutoCodeEnabledAsync(CodeKeys.Quotation, db);
            if (string.IsNullOrEmpty(hdr.SalesQuotationNo) || hdr.SalesQuotationNo == "0")
            {
                var next = await GetNextVchrNo(hdr.FK_BranchId);
                hdr.SalesQuotationNo = next.ToString();
            }
            else
            {
                if (await db.SalesQuotationHdrs.AnyAsync(x => x.SalesQuotationNo == hdr.SalesQuotationNo && x.FK_BranchId == hdr.FK_BranchId))
                {
                    return new OutputResponse(0, "NoAlreadyExists");
                }
            }

            await db.SalesQuotationHdrs.AddAsync(hdr);
            await db.SaveChangesAsync();

            if (isAuto)
            {
                await _codeService.ConsumeCodeAsync(CodeKeys.Quotation, db);
            }
            int headerId = hdr.SalesQuotationId;
            var details = new List<SalesQuotationDtls>();
            foreach (var item in dtls)
            {
                var dtl = item.CopyTo<SalesQuotationDtls>();
                dtl.FK_SalesQuotationHdrId = headerId;
                dtl.FK_StoreId = item.FK_StoreId == 0 ? null : item.FK_StoreId;
                details.Add(dtl);
            }
            await db.SalesQuotationDtls.AddRangeAsync(details);
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
            await db.SalesQuotationDtls.Where(x => x.FK_SalesQuotationHdrId == id).ExecuteDeleteAsync();
            await db.SalesQuotationHdrs.Where(x => x.SalesQuotationId == id).ExecuteDeleteAsync();
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
        var query = db.SalesQuotationHdrs.AsNoTracking().Include(x => x.Customer).Where(x => !x.IsDeleted).AsQueryable();

        if (!string.IsNullOrWhiteSpace(pagination.search))
        {
            var term = pagination.search.ToLower();
            query = query.Where(x => x.SalesQuotationNo.ToLower().Contains(term)
                                  || (x.RefNo ?? string.Empty).ToLower().Contains(term)
                                  || (x.Customer!.Name ?? string.Empty).ToLower().Contains(term)
                                  || (x.Customer!.Code ?? string.Empty).ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(pagination.sortColumn))
        {
            bool desc = pagination.sortDirection == "desc";
            query = pagination.sortColumn switch
            {
                "Code" => desc ? query.OrderByDescending(x => x.SalesQuotationNo) : query.OrderBy(x => x.SalesQuotationNo),
                "Name" => desc ? query.OrderByDescending(x => x.Customer!.Name) : query.OrderBy(x => x.Customer!.Name),
                "Description" => desc ? query.OrderByDescending(x => x.SalesQuotationDate) : query.OrderBy(x => x.SalesQuotationDate),
                _ => desc ? query.OrderByDescending(x => x.SalesQuotationId) : query.OrderBy(x => x.SalesQuotationId)
            };
        }
        else
        {
            query = query.OrderByDescending(x => x.SalesQuotationId);
        }

        int count = await query.CountAsync();
        var list = await query
            .Skip((pagination.page - 1) * pagination.pageSize)
            .Take(pagination.pageSize)
            .ToListAsync();
        var finalList = list.Where(x => x.FK_BranchId == pagination.BranchId).Select(e => new ExMasters
        {
            Id = e.SalesQuotationId,
            Name = e.CustomerName,
            Code = e.SalesQuotationNo,
            IsSelected = true,
            Amount = e.GrandTotal,
            ExtraField = e.Customer?.Code ?? "",
            Description = e.SalesQuotationDate.HasValue ? e.SalesQuotationDate.Value.ToString("dd-MM-yyyy") : ""
        }).ToList();
        return new TransactionPDTO(finalList, count);
    }
    public async Task<SalesQuotationDTO> GetAsync(int Id)
    {
        await using var db = _contextProvider.CreateDbContext();
        var hdr = await db.SalesQuotationHdrs.AsNoTracking().FirstOrDefaultAsync(x => x.SalesQuotationId == Id);
        if (hdr == null) return null!;

        if (hdr.DeliveryNoteId > 0)
        {
            var dnNo = await db.DeliveryNoteHdrs.Where(x => x.DeliveryNoteId == hdr.DeliveryNoteId).Select(x => x.DeliveryNoteNo).FirstOrDefaultAsync();
            hdr.DeliveryNoteNo = dnNo;
        }

        var dtls = await db.SalesQuotationDtls
            .AsNoTracking()
            .Where(x => x.FK_SalesQuotationHdrId == Id)
            .ToListAsync();
        int SL = 1;
        var mapped = dtls.Select(d => new TransactionDtls
        {
            SL = SL++,
            DtlsId = d.SalesQuotationDtlsId,
            HdrId = d.FK_SalesQuotationHdrId,
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
        return new SalesQuotationDTO(hdr, mapped);
    }
    public async Task<int> GetNextVchrNo(int BranchId)
    {
        try
        {
            await using var db = _contextProvider.CreateDbContext();
            var codes = await db.SalesQuotationHdrs
                .AsNoTracking()
                .Where(x => x.FK_BranchId == BranchId)
                .OrderByDescending(x => x.SalesQuotationId)
                .Select(x => x.SalesQuotationNo)
                .Take(20)
                .ToListAsync();

            if (codes.Count == 0) return 1;

            int maxVal = 0;
            foreach (var code in codes)
            {
                if (int.TryParse(code, out int val))
                {
                    if (val > maxVal) maxVal = val;
                }
            }

            return maxVal + 1;
        }
        catch
        {
            return 1;
        }
    }
    public async Task<OutputResponse> UpdateAsync(SalesQuotationDTO entity)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var hdr = entity.QuotationHdr;
            var existing = await db.SalesQuotationHdrs.FindAsync(hdr.SalesQuotationId);
            if (existing is null) return new OutputResponse(0, "Record not found.");
            if (hdr.SalesQuotationNo != existing.SalesQuotationNo)
            {
                if (await db.SalesQuotationHdrs.AnyAsync(x => x.SalesQuotationNo == hdr.SalesQuotationNo && x.FK_BranchId == hdr.FK_BranchId && x.SalesQuotationId != hdr.SalesQuotationId))
                {
                    return new OutputResponse(0, "NoAlreadyExists");
                }
            }

            if (hdr.IsDeleted) // Cancellation Logic
            {
                existing.IsDeleted = true;

                // Mark Details as Deleted
                await db.SalesQuotationDtls.Where(x => x.FK_SalesQuotationHdrId == existing.SalesQuotationId)
                      .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsDeleted, true));

                await db.SaveChangesAsync();
                await transaction.CommitAsync();
                return new OutputResponse(existing.SalesQuotationId, "Cancelled Successfully", null);
            }

            existing.SalesQuotationNo = hdr.SalesQuotationNo;
            existing.CustomerName = hdr.CustomerName;
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
            existing.FK_SalesManId = hdr.FK_SalesManId;
            existing.FK_UserId = hdr.FK_UserId;
            existing.FK_FinancialYearId = hdr.FK_FinancialYearId;
            existing.FK_ProjectId = hdr.FK_ProjectId;
            existing.SalesQuotationDate = hdr.SalesQuotationDate;
            existing.EntryDate = hdr.EntryDate;
            existing.DeliveryNoteId = hdr.DeliveryNoteId;

            // remove old details
            await db.SalesQuotationDtls.Where(x => x.FK_SalesQuotationHdrId == hdr.SalesQuotationId).ExecuteDeleteAsync();

            var details = entity.QuotationDtls?.Select(d => new SalesQuotationDtls
            {
                FK_SalesQuotationHdrId = hdr.SalesQuotationId,
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
            }).ToList() ?? new List<SalesQuotationDtls>();

            if (details.Count > 0)
            {
                await db.SalesQuotationDtls.AddRangeAsync(details);
            }

            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return new OutputResponse(existing.SalesQuotationId, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new OutputResponse(0, ex.Message);
        }
    }
}



