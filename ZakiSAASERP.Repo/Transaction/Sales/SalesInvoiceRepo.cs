using ZakiSAASERP.Interface.Transaction.Sales;
using ZakiSAASERP.Models.Transactions.Sales;
using ZakiSAASERP.Models.ViewModel;
using ZakiSAASERP.Repo.CodeGenerationServices;
using ZakiSAASERP.Repo.Transaction.Inventory;
using AshproStringExtension;

namespace ZakiSAASERP.Repo.Transaction.Sales;

public class SalesInvoiceRepo : ISalesInvoiceRepo
{
    private readonly IDbContextProvider _contextProvider;
    private readonly CodeSequenceService _codeService;
    private readonly StockService _stockService;
    private readonly IGeneralRepo _grepo;
    private readonly IZatcaUploadService _zService;
    private readonly ICustomerMasterRepo _custRepo;
    private readonly IICVRepo _icvRepo;
    private readonly IPermanentSettingsRepo _settingsRepo;

    public SalesInvoiceRepo(
        IDbContextProvider contextProvider,
        CodeSequenceService codeService,
        StockService stockService,
        IGeneralRepo grepo,
        IZatcaUploadService zService,
        ICustomerMasterRepo custRepo,
        IICVRepo icvRepo,
        IPermanentSettingsRepo settingsRepo)
    {
        _contextProvider = contextProvider;
        _codeService = codeService;
        _stockService = stockService;
        _grepo = grepo;
        _zService = zService;
        _custRepo = custRepo;
        _icvRepo = icvRepo;
        _settingsRepo = settingsRepo;
    }
    public async Task<string> GetNextInvoiceNoAsync()
    {
        await using var db = _contextProvider.CreateDbContext();
        return await _codeService.GetNextUniqueCodeAsync(
            CodeKeys.SalesInvoice,
            async (code) => await db.SalesInvoiceHdrs.AnyAsync(x => x.SalesInvoiceNo == code)
        );
    }

    public async Task<OutputResponse> CreateAsync(SalesInvoiceDTO entity)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var hdr = entity.InvoiceHdr;
            var dtls = entity.InvoiceDtls;

            bool isAuto = await _codeService.IsAutoCodeEnabledAsync(CodeKeys.SalesInvoice, db);
            if (hdr.FK_StoreId == 0)
                hdr.FK_StoreId = (await db.StoreMasters.FirstOrDefaultAsync())!.StoreId;
            if (!isAuto)
            {
                var next = await GetNextVchrNo(hdr.FK_BranchId);
                hdr.SalesInvoiceNo = next.ToString();
            }
            var tmaster = hdr.CopyTo<TransactionHdr>();
            tmaster.HdrNo = hdr.SalesInvoiceNo;
            if (entity.ZatcaPhase == 1)
            {
                hdr.InvoiceQRCode = _grepo.GetZatcaQRCodeValFirstPhase(tmaster, entity.CompanyDetails, hdr.SalesInvoiceDate);
            }
            if (hdr.FK_SalesManId == 0)
            {
                var salesman = await db.SalesManMasters.FirstOrDefaultAsync();
                if (salesman != null)
                    hdr.FK_SalesManId = salesman.SalesmanId;
            }
            await db.SalesInvoiceHdrs.AddAsync(hdr);
            await db.SaveChangesAsync();
            if (isAuto)
            {
                await _codeService.ConsumeCodeAsync(CodeKeys.SalesInvoice, db);
            }
            int headerId = hdr.SalesInvoiceId;
            var details = new List<SalesInvoiceDtls>();
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
                var dtl = item.CopyTo<SalesInvoiceDtls>();
                dtl.FK_SalesInvoiceHdrId = headerId;
                dtl.FK_StoreId = item.FK_StoreId == 0 ? null : item.FK_StoreId;
                details.Add(dtl);
            }
            await db.SalesInvoiceDtls.AddRangeAsync(details);
            var tHdr = hdr.CopyTo<TransactionHdr>();
            tHdr.HdrId = headerId;
            tHdr.HdrNo = hdr.SalesInvoiceNo;
            tHdr.PartyId = hdr.FK_CustomerId;
            tHdr.VoucherType = "Sales";
            tHdr.Remarks = $"By Sales No: {hdr.SalesInvoiceNo}";
            var stockSetting = await _settingsRepo.GetSetting("DeliveryNoteStockAffect");
            bool stockAffect = stockSetting == "1" || stockSetting.Equals("true", StringComparison.OrdinalIgnoreCase);

            // Extract unique Delivery Note IDs from details and save to header
            var dnIds = dtls.Where(x => x.FK_DeliveryNoteDtlsId.HasValue && x.FK_DeliveryNoteDtlsId > 0)
                .Select(x => x.FK_DeliveryNoteDtlsId!.Value)
                .Distinct()
                .ToList();

            if (dnIds.Any())
            {
                var dnHdrIds = await db.DeliveryNoteDtls
                    .Where(x => dnIds.Contains(x.DeliveryNoteDtlsId))
                    .Select(x => x.FK_DeliveryNoteHdrId)
                    .Distinct()
                    .ToListAsync();
                hdr.DeliveryNoteIds = string.Join(",", dnHdrIds);
            }

            await ApplyStockWithDeliveryNoteLogic(db, dtls, tHdr, hdr.SalesInvoiceDate.ToDateTime(), stockAffect);

            //await stockService.StockReverseForDeliveryNoteInSales(dtls, tHdr); // Deprecated
            await PostToLedger(db, hdr);
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            bool isUpload = true;
            if (entity.ZatcaPhase == 2)
            {
                var customer = await _custRepo.GetAsync(hdr.FK_CustomerId);
                string voucherType = "Simplified Invoice";
                if ((customer != null && !string.IsNullOrWhiteSpace(customer.VatNo) && !entity.IsSimplified)
                    || (customer != null && !string.IsNullOrWhiteSpace(customer.VatNo) && entity.IsSimplified && hdr.GrandTotal > 1000))
                {
                    voucherType = "Standard Invoice";
                    isUpload = entity.IsUpload;
                }
                if (await _icvRepo.ICVisExist(hdr.SalesInvoiceId, voucherType, hdr.FK_BranchId))
                {
                    return new OutputResponse(headerId, "Already uploaded the zatca e-invoice!", hdr);
                }
                bool IsCash = hdr.BillType == "Cash" ? true : false;
                var lineDiscount = dtls.Sum(x => x.DiscountAmnt);
                if (lineDiscount > 0) { tHdr.AdditionalDiscAmnt = 0; }
                var response = await _zService.UploadZatcaInvoice(tHdr, dtls, entity.CompanyDetails, customer, voucherType, isUpload, IsCash);
                try
                {
                    if (response.IsUploaded)
                    {
                        hdr.InvoiceQRCode = response.QrCodeString;
                        await db.SalesInvoiceHdrs.Where(x => x.SalesInvoiceId == headerId).ExecuteUpdateAsync(x => x.SetProperty(e => e.InvoiceQRCode, hdr.InvoiceQRCode));
                        return new OutputResponse(headerId, null, hdr,true);
                    }
                    else
                    {
                        return new OutputResponse(headerId, response.ErrorMessage ?? "Zatca upload failed.", hdr);
                    }
                }
                catch (Exception ex)
                {
                    await db.SalesInvoiceHdrs.Where(x => x.SalesInvoiceId == headerId).ExecuteUpdateAsync(x => x.SetProperty(e => e.InvoiceQRCode, ""));
                    return new OutputResponse(headerId, ex.Message, hdr);
                }   
            }
            return new OutputResponse(headerId, null, hdr,true);
        }
        catch (Exception ex)
        {
            try { await transaction.RollbackAsync(); } catch { }
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
            var hdr = await db.SalesInvoiceHdrs.AsNoTracking().FirstOrDefaultAsync(x => x.SalesInvoiceId == id);
            if (hdr != null)
            {
                // Check if Used in Delivery Note
                var usedItems = await db.SalesInvoiceDtls.Where(x => x.FK_SalesInvoiceHdrId == id && x.DeliveredQty > 0).ToListAsync();
                if (usedItems.Any())
                {
                    return new OutputResponse(0, "CannotDeleteSIDelivered");
                }

                // Check if Used in Sales Invoice Return
                var returnExists = await db.SalesInvoiceRtnHdrs.AnyAsync(x => x.SalesInvoiceId == id && !x.IsDeleted);
                if (returnExists)
                {
                    return new OutputResponse(0, "CannotDeleteSIReturned");
                }

                // Reverse Stock
                await _stockService.ReverseStockMovements(hdr.SalesInvoiceNo, "Sales", hdr.FK_BranchId, db);

                await db.TblAccountings.Where(x => x.VoucherId == hdr.SalesInvoiceId && x.Type == "Sales").ExecuteDeleteAsync();
            }

            // Revert Delivery Note Usage (Invoiced Qty)
            var oldDetails = await db.SalesInvoiceDtls.Where(x => x.FK_SalesInvoiceHdrId == id).ToListAsync();
            await RevertDeliveryNoteUsage(db, oldDetails);

            await db.SalesInvoiceDtls.Where(x => x.FK_SalesInvoiceHdrId == id).ExecuteDeleteAsync();
            await db.SalesInvoiceHdrs.Where(x => x.SalesInvoiceId == id).ExecuteDeleteAsync();
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
        var query = db.SalesInvoiceHdrs.AsNoTracking().Include(x => x.Customer).Where(x => !x.IsDeleted).AsQueryable();

        if (pagination.BranchId > 0)
        {
            query = query.Where(x => x.FK_BranchId == pagination.BranchId);
        }

        if (!string.IsNullOrWhiteSpace(pagination.search))
        {
            var term = pagination.search.ToLower();
            query = query.Where(x => x.SalesInvoiceNo.ToLower().Contains(term)
                || (x.CustomerName ?? string.Empty).ToLower().Contains(term)
                || (x.Customer != null && x.Customer.Name != null && x.Customer.Name.ToLower().Contains(term))
                || (x.RefNo ?? string.Empty).ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(pagination.sortColumn))
        {
            bool desc = pagination.sortDirection == "desc";
            query = pagination.sortColumn switch
            {
                "Code" => desc ? query.OrderByDescending(x => x.SalesInvoiceNo) : query.OrderBy(x => x.SalesInvoiceNo),
                "Name" => desc ? query.OrderByDescending(x => x.CustomerName) : query.OrderBy(x => x.CustomerName),
                "Description" => desc ? query.OrderByDescending(x => x.SalesInvoiceDate) : query.OrderBy(x => x.SalesInvoiceDate),
                _ => desc ? query.OrderByDescending(x => x.SalesInvoiceId) : query.OrderBy(x => x.SalesInvoiceId)
            };
        }
        else
        {
            query = query.OrderByDescending(x => x.SalesInvoiceId);
        }

        int count = await query.CountAsync();
        var list = await query
            .Skip((pagination.page - 1) * pagination.pageSize)
            .Take(pagination.pageSize)
            .ToListAsync();
        var finalList = list.Select(e => new ExMasters
        {
            Id = e.SalesInvoiceId,
            Name = !string.IsNullOrWhiteSpace(e.CustomerName) ? e.CustomerName : (e.Customer?.Name ?? "Cash Customer"),
            Code = e.SalesInvoiceNo,
            IsSelected = true,
            Amount = e.GrandTotal,
            ExtraField = e.RefNo,
            ReferenceNo = e.BillType ?? "Cash",
            Description = e.SalesInvoiceDate?.ToString("dd-MM-yyyy") ?? ""
        }).ToList();
        return new TransactionPDTO(finalList, count);
    }

    public async Task<SalesInvoiceDTO> GetAsync(int Id)
    {
        await using var db = _contextProvider.CreateDbContext();
        var hdr = await db.SalesInvoiceHdrs.AsNoTracking().FirstOrDefaultAsync(x => x.SalesInvoiceId == Id);
        if (hdr == null) return null!;
        if ((hdr.QuotationId ?? 0) > 0)
        {
            hdr.QuotationNo = await db.SalesQuotationHdrs.AsNoTracking().Where(x => x.SalesQuotationId == hdr.QuotationId).Select(x => x.SalesQuotationNo).FirstOrDefaultAsync();
        }
        var dtls = await db.SalesInvoiceDtls
            .AsNoTracking().Include(x => x.Unit).Include(x => x.Item)
            .Where(x => x.FK_SalesInvoiceHdrId == Id)
            .ToListAsync();
        var mapped = dtls.Select(d => new TransactionDtls
        {
            DtlsId = d.SalesInvoiceDtlsId,
            HdrId = d.FK_SalesInvoiceHdrId,
            FK_ItemId = d.FK_ItemId,
            ItemName = d.ItemName,
            ItemArabicName = d.Item?.ItemNameArabic ?? string.Empty,
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
            RefDtlsId = d.FK_DeliveryNoteDtlsId,
            RefType = d.FK_DeliveryNoteDtlsId.HasValue ? "DeliveryNote" : null,
            FK_DeliveryNoteDtlsId = d.FK_DeliveryNoteDtlsId,
            DeliveredQty = d.DeliveredQty,
            UnitName = d.Unit?.Name ?? string.Empty
        }).ToList();
        return new SalesInvoiceDTO(hdr, mapped, null);
    }

    public async Task<int> GetNextVchrNo(int BranchId)
    {
        try
        {
            await using var db = _contextProvider.CreateDbContext();
            var maxNo = await db.SalesInvoiceHdrs
                .AsNoTracking()
                .Where(x => x.FK_BranchId == BranchId)
                .OrderByDescending(x => x.SalesInvoiceId)
                .Select(x => x.SalesInvoiceNo)
                .FirstOrDefaultAsync();

            if (int.TryParse(maxNo, out int val)) return val + 1;
            return 1;
        }
        catch
        {
            return 1;
        }
    }

    public async Task<OutputResponse> UpdateAsync(SalesInvoiceDTO entity)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var hdr = entity.InvoiceHdr;
            var existing = await db.SalesInvoiceHdrs.FindAsync(hdr.SalesInvoiceId);
            if (existing is null) return new OutputResponse(0, "Record not found.");
            if (hdr.IsDeleted) // Cancellation Logic
            {
                var enableCancelSetting = await _settingsRepo.GetSetting("SalesInvoice_EnableCancel");
                bool enableCancel = enableCancelSetting == "1" || enableCancelSetting.Equals("true", StringComparison.OrdinalIgnoreCase);
                if (!enableCancel)
                {
                    return new OutputResponse(0, "CancelDisabledInSettings");
                }

                // Check if Used in Delivery Note
                var usedItems = await db.SalesInvoiceDtls.Where(x => x.FK_SalesInvoiceHdrId == existing.SalesInvoiceId && x.DeliveredQty > 0).ToListAsync();
                if (usedItems.Any())
                {
                    return new OutputResponse(0, "CannotCancelSIDelivered");
                }

                // Check if Used in Sales Invoice Return
                var returnExists = await db.SalesInvoiceRtnHdrs.AnyAsync(x => x.SalesInvoiceId == existing.SalesInvoiceId && !x.IsDeleted);
                if (returnExists)
                {
                    return new OutputResponse(0, "CannotCancelSIReturned");
                }

                existing.IsDeleted = true;

                // Reverse Stock
                await _stockService.ReverseStockMovements(existing.SalesInvoiceNo, "Sales", existing.FK_BranchId, db);

                // Delete Accounting
                await db.TblAccountings.Where(x => x.VoucherId == existing.SalesInvoiceId && x.Type == "Sales").ExecuteDeleteAsync();

                // Revert Delivery Note Usage (Invoiced Qty)
                var oldDtls = await db.SalesInvoiceDtls.Where(x => x.FK_SalesInvoiceHdrId == existing.SalesInvoiceId).ToListAsync();
                await RevertDeliveryNoteUsage(db, oldDtls);

                // Mark Details as Deleted
                await db.SalesInvoiceDtls.Where(x => x.FK_SalesInvoiceHdrId == existing.SalesInvoiceId)
                      .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsDeleted, true));

                await db.SaveChangesAsync();
                await transaction.CommitAsync();
                return new OutputResponse(existing.SalesInvoiceId, "Cancelled Successfully", null);
            }

            // Editing / Updating Logic
            var enableUpdateSetting = await _settingsRepo.GetSetting("SalesInvoice_EnableUpdate");
            bool enableUpdate = enableUpdateSetting == "1" || enableUpdateSetting.Equals("true", StringComparison.OrdinalIgnoreCase);
            if (!enableUpdate)
            {
                return new OutputResponse(0, "UpdateDisabledInSettings");
            }

            // Check if Used in Delivery Note
            var usedItemsEdit = await db.SalesInvoiceDtls.Where(x => x.FK_SalesInvoiceHdrId == existing.SalesInvoiceId && x.DeliveredQty > 0).ToListAsync();
            if (usedItemsEdit.Any())
            {
                return new OutputResponse(0, "CannotEditSIDelivered");
            }

            // Check if Used in Sales Invoice Return
            var returnExistsEdit = await db.SalesInvoiceRtnHdrs.AnyAsync(x => x.SalesInvoiceId == existing.SalesInvoiceId && !x.IsDeleted);
            if (returnExistsEdit)
            {
                return new OutputResponse(0, "CannotEditSIReturned");
            }

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
            existing.SalesInvoiceDate = hdr.SalesInvoiceDate;
            existing.EntryDate = hdr.EntryDate;
            existing.InvoiceQRCode = hdr.InvoiceQRCode;
            existing.HijriDate = hdr.HijriDate;
            existing.BillType = hdr.BillType;
            existing.CurrencyId = hdr.CurrencyId;
            existing.ExchangeRate = hdr.ExchangeRate;
            existing.QuotationId = hdr.QuotationId;
            existing.SalesOrderId = hdr.SalesOrderId;
            existing.ProformaInvoiceId = hdr.ProformaInvoiceId;
            // remove old details
            // Reverse Stock for old transaction
            await _stockService.ReverseStockMovements(existing.SalesInvoiceNo, "Sales", existing.FK_BranchId, db);

            // Revert Delivery Note Usage
            var oldDetails = await db.SalesInvoiceDtls.Where(x => x.FK_SalesInvoiceHdrId == hdr.SalesInvoiceId).ToListAsync();
            await RevertDeliveryNoteUsage(db, oldDetails);

            await db.SalesInvoiceDtls.Where(x => x.FK_SalesInvoiceHdrId == hdr.SalesInvoiceId).ExecuteDeleteAsync();

            var details = entity.InvoiceDtls?.Select(d => new SalesInvoiceDtls
            {
                FK_SalesInvoiceHdrId = hdr.SalesInvoiceId,
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
                FK_DeliveryNoteDtlsId = d.FK_DeliveryNoteDtlsId,
                DeliveredQty = d.DeliveredQty
            }).ToList() ?? new List<SalesInvoiceDtls>();

            if (details.Count > 0)
            {
                await db.SalesInvoiceDtls.AddRangeAsync(details);
            }

            // Populate Base Unit and Resolve QtyPerUnit for Stock Movement
            foreach (var item in entity.InvoiceDtls)
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

            // Process stock movements for updated transaction
            var tHdr = existing.CopyTo<TransactionHdr>();
            tHdr.HdrId = existing.SalesInvoiceId;
            tHdr.HdrNo = existing.SalesInvoiceNo;
            tHdr.PartyId = existing.FK_CustomerId;
            tHdr.VoucherType = "Sales";
            tHdr.Remarks = $"By Sales No: {existing.SalesInvoiceNo}";
            var stockSetting = await _settingsRepo.GetSetting("DeliveryNoteStockAffect");
            bool stockAffect = stockSetting == "1" || stockSetting.Equals("true", StringComparison.OrdinalIgnoreCase);

            // Extract unique Delivery Note IDs from details and save to header
            var dnIds = entity.InvoiceDtls.Where(x => x.FK_DeliveryNoteDtlsId.HasValue && x.FK_DeliveryNoteDtlsId > 0)
                .Select(x => x.FK_DeliveryNoteDtlsId!.Value)
                .Distinct()
                .ToList();

            if (dnIds.Any())
            {
                var dnHdrIds = await db.DeliveryNoteDtls
                    .Where(x => dnIds.Contains(x.DeliveryNoteDtlsId))
                    .Select(x => x.FK_DeliveryNoteHdrId)
                    .Distinct()
                    .ToListAsync();
                existing.DeliveryNoteIds = string.Join(",", dnHdrIds);
            }
            else
            {
                existing.DeliveryNoteIds = null; // Clear if no DNs
            }

            await ApplyStockWithDeliveryNoteLogic(db, entity.InvoiceDtls, tHdr, existing.SalesInvoiceDate.ToDateTime(), stockAffect);

            // Update Accounting: Delete old and create new
            await db.TblAccountings.Where(x => x.VoucherId == existing.SalesInvoiceId && x.Type == "Sales").ExecuteDeleteAsync();
            await PostToLedger(db, existing);

            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return new OutputResponse(existing.SalesInvoiceId, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new OutputResponse(0, ex.Message);
        }
    }

    private async Task PostToLedger(TenantDBContext db, SalesInvoiceHdr hdr)
    {
        var accountingList = new List<ZakiSAASERP.Models.Transactions.Accounts.TblAccounting>();

        // 1. Sales Account (Credit) - Code: 310001010001
        var salesAccount = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.Code == "310001010001");
        int? salesAccId = salesAccount?.AccountId;

        // 2. VAT Output Account (Credit) - Code: 210002010001
        var vatAccount = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.Code == "210002010001");
        int? vatAccId = vatAccount?.AccountId;

        // 3. Debit Account (Customer / Cash / Bank)
        int? debitAccId = null;

        if (string.Equals(hdr.BillType, "Cash", StringComparison.OrdinalIgnoreCase))
        {
            var cashSetting = await db.PermanentSettings.AsNoTracking().FirstOrDefaultAsync(x => x.SettingKey == "DefaultCashAccount");
            if (cashSetting != null && int.TryParse(cashSetting.SettingValue, out int defCashId) && defCashId > 0)
            {
                var defCashAcc = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.AccountId == defCashId);
                debitAccId = defCashAcc?.AccountId;
            }
            if (!debitAccId.HasValue)
            {
                var cashAcc = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.Code.StartsWith("11000101") || x.Name == "Cash in Hand" || x.Name.Contains("Cash"));
                debitAccId = cashAcc?.AccountId;
            }
        }
        else if (string.Equals(hdr.BillType, "Credit", StringComparison.OrdinalIgnoreCase)) // Customer
        {
            var customer = await db.CustomerMasters.AsNoTracking().FirstOrDefaultAsync(x => x.CustomerId == hdr.FK_CustomerId);
            if (customer != null)
            {
                var custAcc = await db.AccountMasters.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Code == customer.Code);
                debitAccId = custAcc?.AccountId;
            }
        }
        else // Bank / Card / Transfer
        {
            var bankSetting = await db.PermanentSettings.AsNoTracking().FirstOrDefaultAsync(x => x.SettingKey == "DefaultBankAccount");
            if (bankSetting != null && int.TryParse(bankSetting.SettingValue, out int defBankId) && defBankId > 0)
            {
                var defAcc = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.AccountId == defBankId);
                debitAccId = defAcc?.AccountId;
            }
            if (!debitAccId.HasValue)
            {
                var bankAcc = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.Code.StartsWith("11000102") || x.Name.Contains("Bank") || (x.NameArabic != null && x.NameArabic.Contains("بنك")));
                debitAccId = bankAcc?.AccountId;
            }
        }

        // Create Entries

        // Credit Sales Account (Taxable Amount)
        if (salesAccId.HasValue && hdr.TaxableAmount > 0)
        {
            accountingList.Add(new ZakiSAASERP.Models.Transactions.Accounts.TblAccounting
            {
                EntryDate = hdr.SalesInvoiceDate ?? DateTime.Now,
                VoucherId = hdr.SalesInvoiceId,
                VoucherNo = hdr.SalesInvoiceNo,
                FK_AccountId = salesAccId,
                Amount = -hdr.TaxableAmount, // Credit (-ve)
                Narration = $"Sales - {hdr.CustomerName}",
                Type = "Sales",
                BillType = hdr.BillType,
                FK_BranchId = hdr.FK_BranchId,
                FK_UserId = hdr.FK_UserId,
                FK_CustomerId = hdr.FK_CustomerId
            });
        }

        // Credit VAT Output Account (Tax Total)
        if (vatAccId.HasValue && hdr.TaxTotal > 0)
        {
            accountingList.Add(new ZakiSAASERP.Models.Transactions.Accounts.TblAccounting
            {
                EntryDate = hdr.SalesInvoiceDate ?? DateTime.Now,
                VoucherId = hdr.SalesInvoiceId,
                VoucherNo = hdr.SalesInvoiceNo,
                FK_AccountId = vatAccId,
                Amount = -hdr.TaxTotal, // Credit (-ve)
                Narration = $"VAT Output - {hdr.SalesInvoiceNo}",
                Type = "Sales",
                BillType = hdr.BillType,
                FK_BranchId = hdr.FK_BranchId,
                FK_UserId = hdr.FK_UserId,
                FK_CustomerId = hdr.FK_CustomerId
            });
        }

        // Debit Customer/Cash/Bank (Grand Total)
        if (debitAccId.HasValue && hdr.GrandTotal > 0)
        {
            accountingList.Add(new ZakiSAASERP.Models.Transactions.Accounts.TblAccounting
            {
                EntryDate = hdr.SalesInvoiceDate ?? DateTime.Now,
                VoucherId = hdr.SalesInvoiceId,
                VoucherNo = hdr.SalesInvoiceNo,
                FK_AccountId = debitAccId,
                Amount = hdr.GrandTotal, // Debit (+ve)
                Narration = $"Sales Invoice - {hdr.SalesInvoiceNo}",
                Type = "Sales",
                BillType = hdr.BillType,
                FK_BranchId = hdr.FK_BranchId,
                FK_UserId = hdr.FK_UserId,
                FK_CustomerId = hdr.FK_CustomerId
            });
        }

        if (accountingList.Count > 0)
        {
            await db.TblAccountings.AddRangeAsync(accountingList);
        }
    }
    private async Task RevertDeliveryNoteUsage(TenantDBContext db, List<SalesInvoiceDtls> dtls)
    {
        var linkedItems = dtls.Where(x => x.FK_DeliveryNoteDtlsId.HasValue && x.FK_DeliveryNoteDtlsId > 0).ToList();
        if (!linkedItems.Any()) return;

        var grouped = linkedItems.GroupBy(x => x.FK_DeliveryNoteDtlsId.Value)
                                 .Select(g => new { DtlsId = g.Key, TotalQty = g.Sum(x => x.Quantity) })
                                 .ToList();

        foreach (var item in grouped)
        {
            var dnDtl = await db.DeliveryNoteDtls.FirstOrDefaultAsync(x => x.DeliveryNoteDtlsId == item.DtlsId);
            if (dnDtl != null)
            {
                dnDtl.InvoicedQty -= item.TotalQty;
                // db.Update(dnDtl); // Not strictly needed if tracked, but explicit is fine.
            }
        }
        await db.SaveChangesAsync();
    }
    private async Task ApplyStockWithDeliveryNoteLogic(TenantDBContext db, List<TransactionDtls> dtls, TransactionHdr tHdr, DateTime date, bool stockAffect)
    {
        var stockItems = new List<TransactionDtls>();

        // 1. Identify Items linked to Valid Delivery Notes
        var dnIds = dtls.Where(x => x.FK_DeliveryNoteDtlsId.HasValue && x.FK_DeliveryNoteDtlsId > 0)
                        .Select(x => x.FK_DeliveryNoteDtlsId.Value).Distinct().ToList();

        // 2. Fetch DN Details (Need Quantity and InvoicedQty)
        // Note: InvoicedQty here should be the value BEFORE adding current transaction's quantity if we hadn't updated it yet.
        // BUT my code flow might have already deleted the old ones.
        // For 'CreateAsync', InvoicedQty is from BEFORE this invoice.
        // For 'UpdateAsync', we reverted old usage, so InvoicedQty is effectively "before this invoice" again.

        var dnMap = new Dictionary<int, DeliveryNoteDtls>();
        if (dnIds.Any())
        {
            dnMap = await db.DeliveryNoteDtls.Where(x => dnIds.Contains(x.DeliveryNoteDtlsId))
                                             .ToDictionaryAsync(x => x.DeliveryNoteDtlsId);
        }

        foreach (var item in dtls)
        {
            if (item.FK_DeliveryNoteDtlsId.HasValue && item.FK_DeliveryNoteDtlsId > 0 && dnMap.TryGetValue(item.FK_DeliveryNoteDtlsId.Value, out var dnDtl))
            {
                // Capture state before increment
                decimal prevInvoiced = dnDtl.InvoicedQty;
                decimal dnQty = dnDtl.Quantity; // The Original Delivery Note Qty

                // Update InvoicedQty
                dnDtl.InvoicedQty += item.Quantity;

                // Stock Logic
                if (!stockAffect)
                {
                    // Case: Delivery Note did NOT affect Stock.
                    // So Invoice MUST affect stock fully.
                    stockItems.Add(item.CopyTo<TransactionDtls>());
                }
                else
                {
                    // Case: Delivery Note DID affect Stock.
                    // We only deduct the EXCESS quantity.

                    decimal totalInvoiced = dnDtl.InvoicedQty;

                    // Formula: New Excess - Old Excess
                    decimal excessNow = Math.Max(0, totalInvoiced - dnQty);
                    decimal excessPrev = Math.Max(0, prevInvoiced - dnQty);
                    decimal deductQty = excessNow - excessPrev;

                    if (deductQty > 0)
                    {
                        // Create a clone item with adjusted quantity
                        var stockItem = item.CopyTo<TransactionDtls>();
                        stockItem.Quantity = deductQty;
                        // Ensure Gross/Total follows if needed, but StockService usually cares about Qty.
                        stockItems.Add(stockItem);
                    }
                }
            }
            else
            {
                // Not linked, or invalid link
                stockItems.Add(item.CopyTo<TransactionDtls>());
            }
        }

        // Save changes to persist InvoicedQty updates
        await db.SaveChangesAsync();

        if (stockItems.Any())
        {
            await _stockService.StockMovementForTransaction(db, stockItems, tHdr, date, false);
        }
    }
    public async Task<List<SalesInvoiceHdr>> GetByBranchAsync()
    {
        await using var db = _contextProvider.CreateDbContext();
        var headers = await db.SalesInvoiceHdrs
            .AsNoTracking()
            .OrderByDescending(x => x.SalesInvoiceId)
            .ToListAsync();

        return headers;
    }

    public async Task<DateTime?> GetOldestUnpaidInvoiceDateAsync(int customerId)
    {
        await using var db = _contextProvider.CreateDbContext();

        // Get all credit invoices for this customer that are not deleted
        var invoiceIds = await db.SalesInvoiceHdrs
            .AsNoTracking()
            .Where(h => h.FK_CustomerId == customerId
                     && !h.IsDeleted
                     && h.BillType == "Credit")
            .Select(h => new { h.SalesInvoiceId, h.SalesInvoiceDate })
            .ToListAsync();

        if (!invoiceIds.Any()) return null;

        // Get amounts paid against those invoices from customer receipts (TblAccounting)
        var invoiceIdList = invoiceIds.Select(x => x.SalesInvoiceId).ToList();

        var paidAmounts = await db.TblAccountings
            .AsNoTracking()
            .Where(a => invoiceIdList.Contains(a.VoucherId ?? 0) && a.Type == "CustomerReceipt")
            .GroupBy(a => a.VoucherId)
            .Select(g => new { VoucherId = g.Key, TotalPaid = g.Sum(a => a.Amount) })
            .ToListAsync();

        // Get grand totals from invoices
        var invoiceTotals = await db.SalesInvoiceHdrs
            .AsNoTracking()
            .Where(h => invoiceIdList.Contains(h.SalesInvoiceId))
            .Select(h => new { h.SalesInvoiceId, h.GrandTotal, h.SalesInvoiceDate })
            .ToListAsync();

        // Find oldest invoice where OutstandingAmount > 0
        DateTime? oldest = null;
        foreach (var inv in invoiceTotals)
        {
            var paid = paidAmounts.FirstOrDefault(p => p.VoucherId == inv.SalesInvoiceId)?.TotalPaid ?? 0;
            var outstanding = inv.GrandTotal - paid;
            if (outstanding > 0 && inv.SalesInvoiceDate.HasValue)
            {
                if (oldest == null || inv.SalesInvoiceDate.Value.Date < oldest.Value.Date)
                    oldest = inv.SalesInvoiceDate.Value.Date;
            }
        }
        return oldest;
    }
}



