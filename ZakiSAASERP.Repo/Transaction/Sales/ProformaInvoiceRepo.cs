using ZakiSAASERP.Interface.Transaction.Sales;
using ZakiSAASERP.Models.Transactions.Sales;
using ZakiSAASERP.Models.ViewModel;
using ZakiSAASERP.Repo.CodeGenerationServices;
using AshproStringExtension;
using Microsoft.Extensions.Logging;

namespace ZakiSAASERP.Repo.Transaction.Sales;

public class ProformaInvoiceRepo : IProformaInvoiceRepo
{
    private readonly IDbContextProvider _contextProvider;
    private readonly CodeSequenceService _codeService;
    private readonly ILogger<ProformaInvoiceRepo> _logger;

    public ProformaInvoiceRepo(IDbContextProvider contextProvider, CodeSequenceService codeService, ILogger<ProformaInvoiceRepo> logger)
    {
        _contextProvider = contextProvider;
        _codeService = codeService;
        _logger = logger;
    }

    public async Task<string> GetNextProformaInvoiceNoAsync()
    {
        await using var db = _contextProvider.CreateDbContext();
        return await _codeService.GetNextUniqueCodeAsync(
            CodeKeys.ProformaInvoice,
            async (code) => await db.ProformaInvoiceHdrs.AnyAsync(x => x.ProformaInvoiceNo == code)
        );
    }

    public async Task<OutputResponse> CreateAsync(ProformaInvoiceDTO entity)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var hdr = entity.Hdr;
            var dtls = entity.Dtls;

            bool isAuto = await _codeService.IsAutoCodeEnabledAsync(CodeKeys.ProformaInvoice, db);
            if (string.IsNullOrEmpty(hdr.ProformaInvoiceNo) || hdr.ProformaInvoiceNo == "0")
            {
                var next = await GetNextVchrNo(hdr.FK_BranchId);
                hdr.ProformaInvoiceNo = next.ToString();
            }
            else
            {
                if (await db.ProformaInvoiceHdrs.AnyAsync(x => x.ProformaInvoiceNo == hdr.ProformaInvoiceNo && x.FK_BranchId == hdr.FK_BranchId))
                {
                    return new OutputResponse(0, "NoAlreadyExists");
                }
            }

            await db.ProformaInvoiceHdrs.AddAsync(hdr);
            if (hdr.FK_ProjectId == 0) hdr.FK_ProjectId = null;
            hdr.FK_StoreId = null;
            await db.SaveChangesAsync();

            if (isAuto)
            {
                await _codeService.ConsumeCodeAsync(CodeKeys.ProformaInvoice, db);
            }
            int headerId = hdr.ProformaInvoiceId;

            var details = new List<ProformaInvoiceDtls>();
            foreach (var item in dtls)
            {
                var itemMaster = await db.ItemMasters.AsNoTracking().Include(x => x.MultiUnits).FirstOrDefaultAsync(x => x.ItemId == item.FK_ItemId);

                // Resolve QtyPerUnit if missing or invalid
                if (item.QtyPerUnit <= 0)
                {
                    var baseUnitId = itemMaster?.FK_UnitId ?? 0;
                    if (item.FK_UnitId == baseUnitId)
                    {
                        item.QtyPerUnit = 1;
                    }
                    else
                    {
                        var conversion = itemMaster?.MultiUnits?.FirstOrDefault(x => x.FK_UnitId == item.FK_UnitId);
                        item.QtyPerUnit = conversion?.QTYPerUnit ?? 1;
                    }
                }

                var dtl = item.CopyTo<ProformaInvoiceDtls>();
                dtl.FK_StoreId = item.FK_StoreId == 0 ? null : item.FK_StoreId;
                dtl.FK_ProformaInvoiceHdrId = headerId;
                details.Add(dtl);
            }
            await db.ProformaInvoiceDtls.AddRangeAsync(details);
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



    public async Task<TransactionPDTO> GetAllWithPagination(PaginationModel pagination)
    {
        await using var db = _contextProvider.CreateDbContext();
        var query = db.ProformaInvoiceHdrs.AsNoTracking().Include(x => x.Customer).Where(x => !x.IsDeleted).AsQueryable();

        if (!string.IsNullOrWhiteSpace(pagination.search))
        {
            var term = pagination.search.ToLower();
            query = query.Where(x => x.ProformaInvoiceNo.ToLower().Contains(term) || (x.RefNo ?? string.Empty).ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(pagination.sortColumn))
        {
            bool desc = pagination.sortDirection == "desc";
            query = pagination.sortColumn switch
            {
                "Code" => desc ? query.OrderByDescending(x => x.ProformaInvoiceNo) : query.OrderBy(x => x.ProformaInvoiceNo),
                "Name" => desc ? query.OrderByDescending(x => x.Customer!.Name) : query.OrderBy(x => x.Customer!.Name),
                "Description" => desc ? query.OrderByDescending(x => x.ProformaInvoiceDate) : query.OrderBy(x => x.ProformaInvoiceDate),
                _ => desc ? query.OrderByDescending(x => x.ProformaInvoiceId) : query.OrderBy(x => x.ProformaInvoiceId)
            };
        }
        else
        {
            query = query.OrderByDescending(x => x.ProformaInvoiceId);
        }

        int count = await query.CountAsync();
        var list = await query
            .Skip((pagination.page - 1) * pagination.pageSize)
            .Take(pagination.pageSize)
            .ToListAsync();
        var finalList = list.Where(x => x.FK_BranchId == pagination.BranchId).Select(e => new ExMasters
        {
            Id = e.ProformaInvoiceId,
            Name = e.CustomerName,
            Code = e.ProformaInvoiceNo,
            IsSelected = true,
            Amount = e.GrandTotal,
            ExtraField = e.RefNo,
            Description = e.ProformaInvoiceDate.Value.ToString("dd-MM-yyyy")
        }).ToList();
        return new TransactionPDTO(finalList, count);
    }

    public async Task<ProformaInvoiceDTO> GetAsync(int Id)
    {
        await using var db = _contextProvider.CreateDbContext();
        var hdr = await db.ProformaInvoiceHdrs.AsNoTracking().FirstOrDefaultAsync(x => x.ProformaInvoiceId == Id);
        if (hdr == null) return null!;
        var dtls = await db.ProformaInvoiceDtls
            .AsNoTracking()
            .Where(x => x.FK_ProformaInvoiceHdrId == Id)
            .ToListAsync();
        var mapped = dtls.Select(d => new TransactionDtls
        {
            DtlsId = d.ProformaInvoiceDtlsId,
            HdrId = d.FK_ProformaInvoiceHdrId,
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
        if (hdr.FK_AccountId.HasValue && hdr.FK_AccountId > 0)
        {
            var acc = await db.AccountMasters.AsNoTracking()
                .Where(x => x.AccountId == hdr.FK_AccountId)
                .Select(x => new { x.Code, x.Name })
                .FirstOrDefaultAsync();
            if (acc != null)
            {
                hdr.AccountNo = acc.Code;
                hdr.AccountName = acc.Name;
            }
        }
        return new ProformaInvoiceDTO(hdr, mapped);
    }

    public async Task<int> GetNextVchrNo(int BranchId)
    {
        await using var db = _contextProvider.CreateDbContext();
        try
        {
            var codes = await db.ProformaInvoiceHdrs
                .AsNoTracking()
                .Where(x => x.FK_BranchId == BranchId)
                .OrderByDescending(x => x.ProformaInvoiceId)
                .Select(x => x.ProformaInvoiceNo)
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

    public async Task<OutputResponse> UpdateAsync(ProformaInvoiceDTO entity)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var hdr = entity.Hdr;
            var existing = await db.ProformaInvoiceHdrs.FindAsync(hdr.ProformaInvoiceId);
            if (existing is null) return new OutputResponse(0, "Record not found.");

            if (hdr.ProformaInvoiceNo != existing.ProformaInvoiceNo)
            {
                if (await db.ProformaInvoiceHdrs.AnyAsync(x => x.ProformaInvoiceNo == hdr.ProformaInvoiceNo && x.FK_BranchId == hdr.FK_BranchId && x.ProformaInvoiceId != hdr.ProformaInvoiceId))
                {
                    return new OutputResponse(0, "NoAlreadyExists");
                }
            }


            if (hdr.IsDeleted) // Cancellation Logic
            {
                existing.IsDeleted = true;

                // Mark Details as Deleted
                await db.ProformaInvoiceDtls.Where(x => x.FK_ProformaInvoiceHdrId == existing.ProformaInvoiceId)
                      .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsDeleted, true));

                await db.SaveChangesAsync();
                await transaction.CommitAsync();
                return new OutputResponse(existing.ProformaInvoiceId, "Cancelled Successfully", null);
            }

            existing.ProformaInvoiceNo = hdr.ProformaInvoiceNo;
            existing.CustomerName = hdr.CustomerName;
            existing.RefNo = hdr.RefNo;
            existing.GrossAmount = hdr.GrossAmount;
            existing.LineDsicount = hdr.LineDsicount;
            existing.TaxableAmount = hdr.TaxableAmount;
            existing.TaxTotal = hdr.TaxTotal;
            existing.NetAmount = hdr.NetAmount;
            existing.AdditionalAmount = hdr.AdditionalAmount;
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
            existing.FK_ProjectId = (hdr.FK_ProjectId == 0) ? null : hdr.FK_ProjectId;
            existing.ProformaInvoiceDate = hdr.ProformaInvoiceDate;
            existing.EntryDate = hdr.EntryDate;

            // Additional Business Fields
            existing.BillType = hdr.BillType;

            existing.CustomerVatNo = hdr.CustomerVatNo;
            existing.DueDate = hdr.DueDate;
            existing.CreditDays = hdr.CreditDays;
            existing.FK_StoreId = null;

            // UI Optimization
            existing.FK_AccountId = hdr.FK_AccountId;
            existing.Attention = hdr.Attention;
            existing.DeliveryTo = hdr.DeliveryTo;
            existing.SectionNo = hdr.SectionNo;
            existing.HijriDate = hdr.HijriDate;

            // Source Fields
            existing.SourceDocType = hdr.SourceDocType;
            existing.SourceDocNo = hdr.SourceDocNo;
            existing.QuotationId = hdr.QuotationId;


            // remove old details
            await db.ProformaInvoiceDtls.Where(x => x.FK_ProformaInvoiceHdrId == hdr.ProformaInvoiceId).ExecuteDeleteAsync();

            var details = new List<ProformaInvoiceDtls>();
            foreach (var item in entity.Dtls)
            {
                var itemMaster = await db.ItemMasters.AsNoTracking().Include(x => x.MultiUnits).FirstOrDefaultAsync(x => x.ItemId == item.FK_ItemId);

                // Resolve QtyPerUnit if missing or invalid
                if (item.QtyPerUnit <= 0)
                {
                    var baseUnitId = itemMaster?.FK_UnitId ?? 0;
                    if (item.FK_UnitId == baseUnitId)
                    {
                        item.QtyPerUnit = 1;
                    }
                    else
                    {
                        var conversion = itemMaster?.MultiUnits?.FirstOrDefault(x => x.FK_UnitId == item.FK_UnitId);
                        item.QtyPerUnit = conversion?.QTYPerUnit ?? 1;
                    }
                }

                var d = new ProformaInvoiceDtls
                {
                    FK_ProformaInvoiceHdrId = hdr.ProformaInvoiceId,
                    FK_ItemId = item.FK_ItemId,
                    ItemName = item.ItemName,
                    Barcode = item.Barcode,
                    FK_UnitId = item.FK_UnitId,
                    Quantity = item.Quantity,
                    QtyPerUnit = item.QtyPerUnit,
                    ExRate = item.ExRate,
                    InRate = item.InRate,
                    GrossAmount = item.GrossAmount,
                    DiscountPerc = item.DiscountPerc,
                    DiscountAmnt = item.DiscountAmnt,
                    TaxableAmnt = item.TaxableAmnt,
                    TaxPerc = item.TaxPerc,
                    TaxAmount = item.TaxAmount,
                    LineTotal = item.LineTotal,
                    IsDeleted = item.IsDeleted,
                    FK_StoreId = item.FK_StoreId == 0 ? null : item.FK_StoreId
                };
                details.Add(d);
            }

            if (details.Count > 0)
            {
                await db.ProformaInvoiceDtls.AddRangeAsync(details);
            }

            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return new OutputResponse(existing.ProformaInvoiceId, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new OutputResponse(0, ex.Message);
        }
    }



    public async Task<ProformaInvoiceDTO> CreateFromQuotationAsync(int quotationId)
    {
        await using var db = _contextProvider.CreateDbContext();
        try
        {
            // Load quotation with details
            var quotation = await db.SalesQuotationHdrs
                .AsNoTracking()
                .Include(x => x.Customer)
                .FirstOrDefaultAsync(x => x.SalesQuotationId == quotationId);

            if (quotation == null)
                throw new Exception("Quotation not found");

            var quotationDetails = await db.SalesQuotationDtls
                .AsNoTracking()
                .Where(x => x.FK_SalesQuotationHdrId == quotationId)
                .ToListAsync();

            // Create new Proforma Invoice Header from Quotation
            var proformaHdr = new ProformaInvoiceHdr
            {
                ProformaInvoiceNo = "", // Will be set on save
                RefNo = quotation.RefNo,
                GrossAmount = quotation.GrossAmount,
                LineDsicount = quotation.LineDsicount,
                TaxableAmount = quotation.TaxableAmount,
                TaxTotal = quotation.TaxTotal,
                NetAmount = quotation.NetAmount,
                AdditionalAmount = quotation.AdditionalAmount,
                AdditionalDiscPertge = quotation.AdditionalDiscPertge,
                AdditionalDiscAmnt = quotation.AdditionalDiscAmnt,
                AdditionalDiscountType = quotation.AdditionalDiscountType,
                GrandTotal = quotation.GrandTotal,
                Remarks = quotation.Remarks,
                FK_BranchId = quotation.FK_BranchId,
                FK_CustomerId = quotation.FK_CustomerId,
                CustomerName = quotation.CustomerName,
                FK_SalesManId = quotation.FK_SalesManId,
                FK_ProjectId = quotation.FK_ProjectId == 0 ? null : quotation.FK_ProjectId,
                ProformaInvoiceDate = DateTime.Now.Date,

                // Source tracking
                SourceDocType = "Quotation",
                SourceDocNo = quotation.SalesQuotationNo,
                QuotationId = quotationId,

                // Copy customer details if available
                CustomerVatNo = quotation.Customer?.VatNo,

                // Default values for new fields
                BillType = "Credit",
                CreditDays = quotation.Customer != null && quotation.Customer.CreditDays.HasValue
                    ? (int)quotation.Customer.CreditDays.Value
                    : null,
                DueDate = quotation.Customer != null && quotation.Customer.CreditDays.HasValue
                    ? DateTime.Now.Date.AddDays((double)quotation.Customer.CreditDays.Value)
                    : null
            };

            // Map quotation details to proforma details
            var proformaDtls = quotationDetails.Select(qd => new TransactionDtls
            {
                FK_ItemId = qd.FK_ItemId,
                ItemName = qd.ItemName,
                Barcode = qd.Barcode,
                FK_UnitId = qd.FK_UnitId,
                Quantity = qd.Quantity,
                QtyPerUnit = qd.QtyPerUnit,
                ExRate = qd.ExRate,
                InRate = qd.InRate,
                GrossAmount = qd.GrossAmount,
                DiscountPerc = qd.DiscountPerc,
                DiscountAmnt = qd.DiscountAmnt,
                TaxableAmnt = qd.TaxableAmnt,
                TaxPerc = qd.TaxPerc,
                TaxAmount = qd.TaxAmount,
                LineTotal = qd.LineTotal,
                IsDeleted = false,
                FK_StoreId = qd.FK_StoreId ?? 0
            }).ToList();

            return new ProformaInvoiceDTO(proformaHdr, proformaDtls);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error creating proforma from quotation: {ex.Message}");
        }
    }
    public async Task<OutputResponse> DeleteAsync(int id)
    {
        await using var db = _contextProvider.CreateDbContext();
        try
        {
            if (id == 0) return new OutputResponse(0, "Invalid ID");

            await db.ProformaInvoiceHdrs.Where(x => x.ProformaInvoiceId == id).ExecuteDeleteAsync();
            await db.ProformaInvoiceDtls.Where(x => x.FK_ProformaInvoiceHdrId == id).ExecuteDeleteAsync();

            return new OutputResponse(id, null);
        }
        catch (Exception ex)
        {
            return new OutputResponse(0, ex.Message);
        }
    }
}



