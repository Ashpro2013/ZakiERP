using AshproSAASERP.Interface.Transaction.Sales;
using AshproSAASERP.Models.Transactions.Sales;
using AshproSAASERP.Models.ViewModel;
using AshproSAASERP.Repo.CodeGenerationServices;
using AshproSAASERP.Repo.Transaction.Inventory;
using AshproStringExtension;

namespace AshproSAASERP.Repo.Transaction.Sales;

public class DeliveryNoteRepo : IDeliveryNoteRepo
{
    private readonly IDbContextProvider _contextProvider;
    private readonly CodeSequenceService _codeService;
    private readonly StockService _stockService;
    private readonly IPermanentSettingsRepo _settingsRepo;

    public DeliveryNoteRepo(IDbContextProvider contextProvider, CodeSequenceService codeService, StockService stockService, IPermanentSettingsRepo settingsRepo)
    {
        _contextProvider = contextProvider;
        _codeService = codeService;
        _stockService = stockService;
        _settingsRepo = settingsRepo;
    }

    public async Task<string> GetNextDeliveryNoteNoAsync()
    {
        await using var db = _contextProvider.CreateDbContext();
        return await _codeService.GetNextUniqueCodeAsync(
            CodeKeys.DeliveryNote,
            async (code) => await db.DeliveryNoteHdrs.AnyAsync(x => x.DeliveryNoteNo == code)
        );
    }
    public async Task<OutputResponse> CreateAsync(DeliveryNoteDTO entity)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var hdr = entity.DeliveryNoteHdr;
            var dtls = entity.DeliveryNoteDtls;

            bool isAuto = await _codeService.IsAutoCodeEnabledAsync(CodeKeys.DeliveryNote, db);
            if (hdr.FK_StoreId == 0)
            {
                var defStore = await db.StoreMasters.FirstOrDefaultAsync(x => x.FK_BranchId == hdr.FK_BranchId && x.IsDefaultStore)
                               ?? await db.StoreMasters.FirstOrDefaultAsync(x => x.FK_BranchId == hdr.FK_BranchId) ?? await db.StoreMasters.FirstOrDefaultAsync();
                hdr.FK_StoreId = defStore?.StoreId ?? 0;
            }

            foreach (var item in dtls)
            {
                if (item.FK_StoreId == 0) item.FK_StoreId = hdr.FK_StoreId;
            }

            if (!isAuto)
            {
                var next = await GetNextVchrNo(hdr.FK_BranchId);
                hdr.DeliveryNoteNo = next.ToString();
            }

            await db.DeliveryNoteHdrs.AddAsync(hdr);
            await db.SaveChangesAsync();

            if (isAuto)
            {
                await _codeService.ConsumeCodeAsync(CodeKeys.DeliveryNote, db);
            }

            int headerId = hdr.DeliveryNoteId;
            var details = new List<DeliveryNoteDtls>();
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

                var dtl = item.CopyTo<DeliveryNoteDtls>();
                dtl.FK_DeliveryNoteHdrId = headerId;
                dtl.FK_StoreId = item.FK_StoreId == 0 ? null : item.FK_StoreId;
                details.Add(dtl);
            }
            await db.DeliveryNoteDtls.AddRangeAsync(details);
            var tHdr = hdr.CopyTo<TransactionHdr>();
            tHdr.HdrId = headerId;
            tHdr.HdrNo = hdr.DeliveryNoteNo;
            tHdr.PartyId = hdr.FK_CustomerId;
            tHdr.VoucherType = "Delivery Note";
            tHdr.Remarks = $"By Delivery Note No: {hdr.DeliveryNoteNo}";
            var stockItems = new List<TransactionDtls>();
            foreach (var item in dtls)
            {
                if (item.FK_SalesInvoiceDtlsId.HasValue && item.FK_SalesInvoiceDtlsId > 0)
                {
                    // Update Sales Invoice Dtls : Delivered Qty
                    await db.SalesInvoiceDtls.Where(x => x.SalesInvoiceDtlsId == item.FK_SalesInvoiceDtlsId)
                         .ExecuteUpdateAsync(s => s.SetProperty(x => x.DeliveredQty, x => x.DeliveredQty + item.Quantity));
                }
                else
                {
                    stockItems.Add(item);
                }
            }

            var stockSetting = await _settingsRepo.GetSetting("DeliveryNoteStockAffect");
            bool stockAffect = stockSetting == "1" || stockSetting.Equals("true", StringComparison.OrdinalIgnoreCase);

            if (stockAffect)
            {
                await _stockService.StockMovementForTransaction(db, stockItems, tHdr, hdr.DeliveryNoteDate.ToDateTime(), false);
            }
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
            var hdr = await db.DeliveryNoteHdrs.AsNoTracking().FirstOrDefaultAsync(x => x.DeliveryNoteId == id);
            if (hdr != null)
            {
                // Check if Used in Sales Invoice
                var usedItems = await db.DeliveryNoteDtls.Where(x => x.FK_DeliveryNoteHdrId == id && x.InvoicedQty > 0).ToListAsync();
                if (usedItems.Any())
                {
                    return new OutputResponse(0, "CannotDeleteDNInvoiced");
                }

                await _stockService.ReverseStockMovements(hdr.DeliveryNoteNo, "Delivery Note", hdr.FK_BranchId, db);
            }

            // Revert Sales Invoice Usage (Delivered Qty)
            var oldDetails = await db.DeliveryNoteDtls.Where(x => x.FK_DeliveryNoteHdrId == id).ToListAsync();
            await RevertSalesInvoiceUsage(db, oldDetails);

            await db.DeliveryNoteDtls.Where(x => x.FK_DeliveryNoteHdrId == id).ExecuteDeleteAsync();
            await db.DeliveryNoteHdrs.Where(x => x.DeliveryNoteId == id).ExecuteDeleteAsync();
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
        var query = db.DeliveryNoteHdrs.AsNoTracking().Include(x => x.Customer).Where(x => !x.IsDeleted).AsQueryable();

        if (!string.IsNullOrWhiteSpace(pagination.search))
        {
            var term = pagination.search.ToLower();
            query = query.Where(x => x.DeliveryNoteNo.ToLower().Contains(term) || (x.RefNo ?? string.Empty).ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(pagination.sortColumn))
        {
            bool desc = pagination.sortDirection == "desc";
            query = pagination.sortColumn switch
            {
                "Code" => desc ? query.OrderByDescending(x => x.DeliveryNoteNo) : query.OrderBy(x => x.DeliveryNoteNo),
                "Name" => desc ? query.OrderByDescending(x => x.Customer!.Name) : query.OrderBy(x => x.Customer!.Name),
                "Description" => desc ? query.OrderByDescending(x => x.DeliveryNoteDate) : query.OrderBy(x => x.DeliveryNoteDate),
                _ => desc ? query.OrderByDescending(x => x.DeliveryNoteId) : query.OrderBy(x => x.DeliveryNoteId)
            };
        }
        else
        {
            query = query.OrderByDescending(x => x.DeliveryNoteId);
        }

        int count = await query.CountAsync();
        var list = await query
            .Skip((pagination.page - 1) * pagination.pageSize)
            .Take(pagination.pageSize)
            .ToListAsync();
        var finalList = list.Where(x => x.FK_BranchId == pagination.BranchId).Select(e => new ExMasters
        {
            Id = e.DeliveryNoteId,
            Name = e.CustomerName,
            Code = e.DeliveryNoteNo,
            IsSelected = true,
            Amount = e.GrandTotal,
            ExtraField = e.RefNo,
            Description = e.DeliveryNoteDate.Value.ToString("dd-MM-yyyy")
        }).ToList();
        return new TransactionPDTO(finalList, count);
    }
    public async Task<DeliveryNoteDTO> GetAsync(int Id)
    {
        await using var db = _contextProvider.CreateDbContext();
        var hdr = await db.DeliveryNoteHdrs.AsNoTracking().FirstOrDefaultAsync(x => x.DeliveryNoteId == Id);
        if (hdr == null) return null!;
        var dtls = await db.DeliveryNoteDtls
            .AsNoTracking()
            .Where(x => x.FK_DeliveryNoteHdrId == Id)
            .ToListAsync();
        var mapped = dtls.Select(d => new TransactionDtls
        {
            DtlsId = d.DeliveryNoteDtlsId,
            HdrId = d.FK_DeliveryNoteHdrId,
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
            FK_StoreId = d.FK_StoreId ?? 0,
            RefDtlsId = d.FK_SalesInvoiceDtlsId,
            RefType = d.FK_SalesInvoiceDtlsId.HasValue ? "SalesInvoice" : null,
            FK_SalesInvoiceDtlsId = d.FK_SalesInvoiceDtlsId
        }).ToList();
        return new DeliveryNoteDTO(hdr, mapped);
    }

    public async Task<int> GetNextVchrNo(int BranchId)
    {
        try
        {
            await using var db = _contextProvider.CreateDbContext();
            var maxNo = await db.DeliveryNoteHdrs
                .AsNoTracking()
                .Where(x => x.FK_BranchId == BranchId)
                .OrderByDescending(x => x.DeliveryNoteId)
                .Select(x => x.DeliveryNoteNo)
                .FirstOrDefaultAsync();

            if (int.TryParse(maxNo, out int val)) return val + 1;
            return 1;
        }
        catch
        {
            return 1;
        }
    }

    public async Task<OutputResponse> UpdateAsync(DeliveryNoteDTO entity)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var hdr = entity.DeliveryNoteHdr;
            var existing = await db.DeliveryNoteHdrs.FindAsync(hdr.DeliveryNoteId);
            if (existing is null) return new OutputResponse(0, "Record not found.");

            if (hdr.FK_StoreId == 0)
            {
                var defStore = await db.StoreMasters.FirstOrDefaultAsync(x => x.FK_BranchId == hdr.FK_BranchId && x.IsDefaultStore)
                               ?? await db.StoreMasters.FirstOrDefaultAsync(x => x.FK_BranchId == hdr.FK_BranchId)
                               ?? await db.StoreMasters.FirstOrDefaultAsync();
                hdr.FK_StoreId = defStore?.StoreId ?? 0;
            }

            // Ensure Details have valid Store ID before processing
            if (entity.DeliveryNoteDtls != null)
            {
                foreach (var item in entity.DeliveryNoteDtls)
                {
                    if (item.FK_StoreId == 0) item.FK_StoreId = hdr.FK_StoreId;
                }
            }

            if (hdr.IsDeleted) // Cancellation Logic
            {
                // Check if Used in Sales Invoice
                var usedItems = await db.DeliveryNoteDtls.Where(x => x.FK_DeliveryNoteHdrId == existing.DeliveryNoteId && x.InvoicedQty > 0).ToListAsync();
                if (usedItems.Any())
                {
                    return new OutputResponse(0, "CannotCancelDNInvoiced");
                }

                existing.IsDeleted = true;

                // Reverse Stock
                await _stockService.ReverseStockMovements(existing.DeliveryNoteNo, "Delivery Note", existing.FK_BranchId, db);

                // Revert Sales Invoice Usage (Delivered Qty)
                var oldDtls = await db.DeliveryNoteDtls.Where(x => x.FK_DeliveryNoteHdrId == existing.DeliveryNoteId).ToListAsync();
                await RevertSalesInvoiceUsage(db, oldDtls);

                // Mark Details as Deleted
                await db.DeliveryNoteDtls.Where(x => x.FK_DeliveryNoteHdrId == existing.DeliveryNoteId)
                      .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsDeleted, true));

                await db.SaveChangesAsync();
                await transaction.CommitAsync();
                return new OutputResponse(existing.DeliveryNoteId, "Cancelled Successfully", null);
            }

            // Check if Used in Sales Invoice (For regular update)
            var usedItemsEdit = await db.DeliveryNoteDtls.Where(x => x.FK_DeliveryNoteHdrId == existing.DeliveryNoteId && x.InvoicedQty > 0).ToListAsync();
            if (usedItemsEdit.Any())
            {
                return new OutputResponse(0, "CannotEditDNInvoiced");
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
            existing.FK_StoreId = hdr.FK_StoreId; // Ensure Store is updated
            // existing.IsDeleted = hdr.IsDeleted; // Handled above
            existing.FK_BranchId = hdr.FK_BranchId;
            existing.FK_CustomerId = hdr.FK_CustomerId;
            existing.CustomerName = hdr.CustomerName;
            existing.FK_SalesManId = hdr.FK_SalesManId;
            existing.FK_UserId = hdr.FK_UserId;
            existing.FK_FinancialYearId = hdr.FK_FinancialYearId;
            existing.FK_ProjectId = hdr.FK_ProjectId;
            existing.DeliveryNoteDate = hdr.DeliveryNoteDate;
            existing.DeliveryNoteDate = hdr.DeliveryNoteDate;
            existing.EntryDate = hdr.EntryDate;
            existing.SalesInvoiceId = hdr.SalesInvoiceId;

            // remove old details
            // Reverse Stock
            await _stockService.ReverseStockMovements(existing.DeliveryNoteNo, "Delivery Note", existing.FK_BranchId, db);

            // Revert Sales Invoice Usage (Delivered Qty)
            var oldDetails = await db.DeliveryNoteDtls.Where(x => x.FK_DeliveryNoteHdrId == hdr.DeliveryNoteId).ToListAsync();
            await RevertSalesInvoiceUsage(db, oldDetails);

            await db.DeliveryNoteDtls.Where(x => x.FK_DeliveryNoteHdrId == hdr.DeliveryNoteId).ExecuteDeleteAsync();

            var details = entity.DeliveryNoteDtls?.Select(d => new DeliveryNoteDtls
            {
                FK_DeliveryNoteHdrId = hdr.DeliveryNoteId,
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
                FK_StoreId = d.FK_StoreId == 0 ? null : d.FK_StoreId,
                FK_SalesInvoiceDtlsId = d.FK_SalesInvoiceDtlsId
            }).ToList() ?? new List<DeliveryNoteDtls>();

            if (details.Count > 0)
            {
                await db.DeliveryNoteDtls.AddRangeAsync(details);
            }

            // Populate Base Unit and Resolve QtyPerUnit for Stock Movement
            if (entity.DeliveryNoteDtls != null)
            {
                foreach (var item in entity.DeliveryNoteDtls)
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
            tHdr.HdrId = existing.DeliveryNoteId;
            tHdr.HdrNo = existing.DeliveryNoteNo;
            tHdr.PartyId = existing.FK_CustomerId;
            tHdr.VoucherType = "Delivery Note";
            tHdr.Remarks = $"By Delivery Note No: {existing.DeliveryNoteNo}";
            var stockItems = new List<TransactionDtls>();
            foreach (var item in entity.DeliveryNoteDtls)
            {
                if (item.FK_SalesInvoiceDtlsId.HasValue && item.FK_SalesInvoiceDtlsId > 0)
                {
                    // Update Sales Invoice Dtls : Delivered Qty
                    await db.SalesInvoiceDtls.Where(x => x.SalesInvoiceDtlsId == item.FK_SalesInvoiceDtlsId)
                         .ExecuteUpdateAsync(s => s.SetProperty(x => x.DeliveredQty, x => x.DeliveredQty + item.Quantity));
                }
                else
                {
                    stockItems.Add(item);
                }
            }
            var stockSetting = await _settingsRepo.GetSetting("DeliveryNoteStockAffect");
            bool stockAffect = stockSetting == "1" || stockSetting.Equals("true", StringComparison.OrdinalIgnoreCase);

            if (stockAffect)
            {
                await _stockService.StockMovementForTransaction(db, stockItems, tHdr, existing.DeliveryNoteDate.ToDateTime(), false);
            }

            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return new OutputResponse(existing.DeliveryNoteId, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new OutputResponse(0, ex.Message);
        }
    }
    public async Task<List<PendingDeliveryNoteDTO>> GetPendingDeliveryNotes(int customerId)
    {
        await using var db = _contextProvider.CreateDbContext();
        // Fetch all delivery note items for the customer that are not deleted
        var items = await (from d in db.DeliveryNoteDtls
                           join h in db.DeliveryNoteHdrs on d.FK_DeliveryNoteHdrId equals h.DeliveryNoteId
                           join u in db.UnitMasters on d.FK_UnitId equals u.UnitId
                           where h.FK_CustomerId == customerId && !h.IsDeleted && !d.IsDeleted
                           let usedQty = db.SalesInvoiceDtls
                               .Where(s => s.FK_DeliveryNoteDtlsId == d.DeliveryNoteDtlsId && !s.IsDeleted)
                               .Sum(s => (decimal?)s.Quantity) ?? 0
                           where d.Quantity > usedQty
                           select new
                           {
                               Hdr = h,
                               Dtl = d,
                               UnitName = u.Name,
                               PendingQty = d.Quantity - usedQty
                           }).ToListAsync();

        // Group by Header to form the hierarchy
        var result = items.GroupBy(x => x.Hdr.DeliveryNoteId)
            .Select(g => new PendingDeliveryNoteDTO
            {
                DeliveryNoteId = g.Key,
                DeliveryNoteNo = g.First().Hdr.DeliveryNoteNo,
                DeliveryNoteDate = g.First().Hdr.DeliveryNoteDate,
                IsSelected = false,
                Items = g.Select(i => new PendingDeliveryNoteItemDTO
                {
                    DtlsId = i.Dtl.DeliveryNoteDtlsId,
                    HdrId = i.Dtl.FK_DeliveryNoteHdrId,
                    FK_ItemId = i.Dtl.FK_ItemId,
                    ItemName = i.Dtl.ItemName,
                    Barcode = i.Dtl.Barcode,
                    FK_UnitId = i.Dtl.FK_UnitId,
                    UnitName = i.UnitName,
                    Quantity = i.PendingQty, // Set Quantity to PendingQty so it loads correctly in Grid
                    PendingQty = i.PendingQty,
                    OriginalQty = i.Dtl.Quantity,
                    QtyPerUnit = i.Dtl.QtyPerUnit,
                    ExRate = i.Dtl.ExRate,
                    InRate = i.Dtl.InRate,
                    GrossAmount = i.Dtl.GrossAmount,
                    DiscountPerc = i.Dtl.DiscountPerc,
                    DiscountAmnt = i.Dtl.DiscountAmnt,
                    TaxableAmnt = i.Dtl.TaxableAmnt,
                    TaxPerc = i.Dtl.TaxPerc,
                    TaxAmount = i.Dtl.TaxAmount,
                    LineTotal = i.Dtl.LineTotal,
                    FK_StoreId = i.Dtl.FK_StoreId ?? 0,
                    FK_DeliveryNoteDtlsId = i.Dtl.DeliveryNoteDtlsId // IMPORTANT: Link back for future tracking
                }).ToList()
            }).OrderByDescending(x => x.DeliveryNoteDate.HasValue ? x.DeliveryNoteDate.Value.Date : DateTime.MinValue)
              .ThenByDescending(x => x.DeliveryNoteNo)
              .ToList();

        return result;
    }
    private async Task RevertSalesInvoiceUsage(TenantDBContext db, List<DeliveryNoteDtls> dtls)
    {
        var linkedItems = dtls.Where(x => x.FK_SalesInvoiceDtlsId.HasValue && x.FK_SalesInvoiceDtlsId > 0).ToList();
        if (!linkedItems.Any()) return;

        var grouped = linkedItems.GroupBy(x => x.FK_SalesInvoiceDtlsId.Value)
                                 .Select(g => new { DtlsId = g.Key, TotalQty = g.Sum(x => x.Quantity) })
                                 .ToList();

        foreach (var item in grouped)
        {
            var siDtl = await db.SalesInvoiceDtls.FirstOrDefaultAsync(x => x.SalesInvoiceDtlsId == item.DtlsId);
            if (siDtl != null)
            {
                siDtl.DeliveredQty -= item.TotalQty;
            }
        }
        await db.SaveChangesAsync();
    }
}



