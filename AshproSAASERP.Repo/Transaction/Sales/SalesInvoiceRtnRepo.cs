using AshproSAASERP.Interface.Others;
using AshproSAASERP.Interface.Transaction.Sales;
using AshproSAASERP.Models.Transactions.Accounts;
using AshproSAASERP.Models.Transactions.Sales;
using AshproSAASERP.Models.ViewModel;
using AshproSAASERP.Repo.CodeGenerationServices;
using AshproSAASERP.Repo.Transaction.Inventory;
using AshproStringExtension;

namespace AshproSAASERP.Repo.Transaction.Sales;

public class SalesInvoiceRtnRepo : ISalesInvoiceRtnRepo
{
    private readonly IDbContextProvider _contextProvider;
    private readonly CodeSequenceService _codeService;
    private readonly StockService _stockService;
    private readonly IGeneralRepo _grepo;
    private readonly IZatcaUploadService _zService;
    private readonly ICustomerMasterRepo _custRepo;
    private readonly IICVRepo _icvRepo;
    private readonly IPermanentSettingsRepo _settingsRepo;

    public SalesInvoiceRtnRepo(
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

    public async Task<string> GetNextSalesInvoiceRtnNoAsync()
    {
        await using var db = _contextProvider.CreateDbContext();
        return await _codeService.GetNextUniqueCodeAsync(
            CodeKeys.SalesInvoiceRtn,
            async (code) => await db.SalesInvoiceRtnHdrs.AnyAsync(x => x.SalesInvoiceRtnNo == code)
        );
    }
    public async Task<OutputResponse> CreateAsync(SalesInvoiceRtnDTO entity)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var hdr = entity.QuotationHdr;
            if (hdr.SalesInvoiceId > 0)
            {
                bool hasPaid = await HasReceiptPaidAsync(db, hdr.SalesInvoiceId);
                if (hasPaid)
                    return new OutputResponse(0, "InvoiceReturnBlockedByReceipt");
            }
            var dtls = entity.QuotationDtls;

            bool isAuto = await _codeService.IsAutoCodeEnabledAsync(CodeKeys.SalesInvoiceRtn, db);
            if (!isAuto)
            {
                var next = await GetNextVchrNo(hdr.FK_BranchId);
                hdr.SalesInvoiceRtnNo = next.ToString();
            }
            if (hdr.FK_StoreId == 0)
                hdr.FK_StoreId = (await db.StoreMasters.FirstOrDefaultAsync())!.StoreId;
            await db.SalesInvoiceRtnHdrs.AddAsync(hdr);
            await db.SaveChangesAsync();
            if (isAuto)
            {
                await _codeService.ConsumeCodeAsync(CodeKeys.SalesInvoiceRtn, db);
            }
            int headerId = hdr.SalesInvoiceRtnId;
            var details = new List<SalesInvoiceRtnDtls>();
            foreach (var item in dtls)
            {
                item.FK_ItemBaseUnit = (await db.ItemMasters.FirstOrDefaultAsync())!.FK_UnitId ?? 0;
                var dtl = item.CopyTo<SalesInvoiceRtnDtls>();
                dtl.FK_SalesInvoiceRtnHdrId = headerId;
                dtl.FK_StoreId = item.FK_StoreId == 0 ? null : item.FK_StoreId;
                details.Add(dtl);
            }
            await db.SalesInvoiceRtnDtls.AddRangeAsync(details);
            var tHdr = hdr.CopyTo<TransactionHdr>();
            tHdr.HdrId = headerId;
            tHdr.HdrNo = hdr.SalesInvoiceRtnNo;
            tHdr.PartyId = hdr.FK_CustomerId;
            tHdr.VoucherType = "Sales Return";
            tHdr.Remarks = $"By Return No: {hdr.SalesInvoiceRtnNo}";
            await _stockService.StockMovementForTransaction(db, dtls, tHdr, hdr.SalesInvoiceRtnDate.ToDateTime(), true);
            await PostToLedger(db, hdr);
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            if (entity.ZatcaPhase == 2)
            {
                var customer = await _custRepo.GetAsync(hdr.FK_CustomerId);
                string voucherType = "Simplified Credit Note";
                if ((customer != null && !string.IsNullOrWhiteSpace(customer.VatNo) && !entity.IsSimplified)
                    || (customer != null && !string.IsNullOrWhiteSpace(customer.VatNo) && entity.IsSimplified && hdr.GrandTotal > 1000))
                {
                    voucherType = "Standard Credit Note";
                }
                if (await _icvRepo.ICVisExist(hdr.SalesInvoiceId, voucherType, hdr.FK_BranchId))
                {
                    return new OutputResponse(headerId, "Already uploaded the zatca e-invoice!", hdr);
                }
                bool IsCash = hdr.BillType == "Cash" ? true : false;
                var response = await _zService.UploadZatcaInvoice(tHdr, dtls, entity.CompanyDetails, customer, voucherType, true, IsCash);
                if (response.IsUploaded)
                {
                    hdr.InvoiceQRCode = response.QrCodeString;
                    return new OutputResponse(headerId, response.ErrorMessage, hdr,true);
                }
            }
            return new OutputResponse(headerId, null, hdr, true);
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
            var hdr = await db.SalesInvoiceRtnHdrs.AsNoTracking().FirstOrDefaultAsync(x => x.SalesInvoiceRtnId == id);
            if (hdr != null)
            {
                // Reverse Stock
                await _stockService.ReverseStockMovements(hdr.SalesInvoiceRtnNo, "Sales Return", hdr.FK_BranchId, db);

                await db.TblAccountings.Where(x => x.VoucherId == hdr.SalesInvoiceRtnId && x.Type == "SalesReturn").ExecuteDeleteAsync();
            }
            await db.SalesInvoiceRtnDtls.Where(x => x.FK_SalesInvoiceRtnHdrId == id).ExecuteDeleteAsync();
            await db.SalesInvoiceRtnHdrs.Where(x => x.SalesInvoiceRtnId == id).ExecuteDeleteAsync();
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
        var query = db.SalesInvoiceRtnHdrs.AsNoTracking().Include(x => x.Customer).Where(x => !x.IsDeleted).AsQueryable();

        if (!string.IsNullOrWhiteSpace(pagination.search))
        {
            var term = pagination.search.ToLower();
            query = query.Where(x => x.SalesInvoiceRtnNo.ToLower().Contains(term) || (x.RefNo ?? string.Empty).ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(pagination.sortColumn))
        {
            bool desc = pagination.sortDirection == "desc";
            query = pagination.sortColumn switch
            {
                "Code" => desc ? query.OrderByDescending(x => x.SalesInvoiceRtnNo) : query.OrderBy(x => x.SalesInvoiceRtnNo),
                "Name" => desc ? query.OrderByDescending(x => x.Customer!.Name) : query.OrderBy(x => x.Customer!.Name),
                "Description" => desc ? query.OrderByDescending(x => x.SalesInvoiceRtnDate) : query.OrderBy(x => x.SalesInvoiceRtnDate),
                _ => desc ? query.OrderByDescending(x => x.SalesInvoiceRtnId) : query.OrderBy(x => x.SalesInvoiceRtnId)
            };
        }
        else
        {
            query = query.OrderByDescending(x => x.SalesInvoiceRtnId);
        }

        int count = await query.CountAsync();
        var list = await query
            .Skip((pagination.page - 1) * pagination.pageSize)
            .Take(pagination.pageSize)
            .ToListAsync();
        var finalList = list.Where(x => x.FK_BranchId == pagination.BranchId).Select(e => new ExMasters
        {
            Id = e.SalesInvoiceRtnId,
            Name = e.CustomerName,
            Code = e.SalesInvoiceRtnNo,
            IsSelected = true,
            Amount = e.GrandTotal,
            ExtraField = e.RefNo,
            Description = e.SalesInvoiceRtnDate.HasValue ? e.SalesInvoiceRtnDate.Value.ToString("dd-MM-yyyy") : ""
        }).ToList();
        return new TransactionPDTO(finalList, count);
    }
    public async Task<SalesInvoiceRtnDTO> GetAsync(int Id)
    {
        await using var db = _contextProvider.CreateDbContext();
        var hdr = await db.SalesInvoiceRtnHdrs.AsNoTracking().FirstOrDefaultAsync(x => x.SalesInvoiceRtnId == Id);
        if (hdr == null) return null!;
        if (hdr.SalesInvoiceId > 0)
        {
            var invoice = await db.SalesInvoiceHdrs.AsNoTracking()
                             .Select(x => new { x.SalesInvoiceId, x.SalesInvoiceNo })
                             .FirstOrDefaultAsync(x => x.SalesInvoiceId == hdr.SalesInvoiceId);
            if (invoice != null)
            {
                hdr.SalesInvoiceNo = invoice.SalesInvoiceNo;
            }
        }
        var dtls = await db.SalesInvoiceRtnDtls
            .AsNoTracking()
            .Where(x => x.FK_SalesInvoiceRtnHdrId == Id)
            .ToListAsync();
        var mapped = dtls.Select(d => new TransactionDtls
        {
            DtlsId = d.SalesInvoiceRtnDtlsId,
            HdrId = d.FK_SalesInvoiceRtnHdrId,
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
        return new SalesInvoiceRtnDTO(hdr, mapped);
    }
    public async Task<int> GetNextVchrNo(int BranchId)
    {
        try
        {
            await using var db = _contextProvider.CreateDbContext();
            var maxNo = await db.SalesInvoiceRtnHdrs
                .AsNoTracking()
                .Where(x => x.FK_BranchId == BranchId)
                .OrderByDescending(x => x.SalesInvoiceRtnId)
                .Select(x => x.SalesInvoiceRtnNo)
                .FirstOrDefaultAsync();

            if (int.TryParse(maxNo, out int val)) return val + 1;
            return 1;
        }
        catch
        {
            return 1;
        }
    }
    public async Task<OutputResponse> UpdateAsync(SalesInvoiceRtnDTO entity)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var hdr = entity.QuotationHdr;
            var existing = await db.SalesInvoiceRtnHdrs.FindAsync(hdr.SalesInvoiceRtnId);
            if (existing is null) return new OutputResponse(0, "Record not found.");

            if (!hdr.IsDeleted && hdr.SalesInvoiceId > 0)
            {
                bool hasPaid = await HasReceiptPaidAsync(db, hdr.SalesInvoiceId);
                if (hasPaid)
                    return new OutputResponse(0, "InvoiceReturnBlockedByReceipt");
            }

            if (hdr.IsDeleted) // Cancellation Logic
            {
                var enableCancelSetting = await _settingsRepo.GetSetting("SalesReturn_EnableCancel");
                if (string.IsNullOrEmpty(enableCancelSetting))
                    enableCancelSetting = await _settingsRepo.GetSetting("SalesInvoiceRtn_EnableCancel");
                bool enableCancel = enableCancelSetting == "1" || enableCancelSetting.Equals("true", StringComparison.OrdinalIgnoreCase);
                if (!enableCancel)
                {
                    return new OutputResponse(0, "CancelDisabledInSettings");
                }

                existing.IsDeleted = true;

                // Reverse Stock (if applicable)
                await _stockService.ReverseStockMovements(existing.SalesInvoiceRtnNo, "Sales Return", existing.FK_BranchId, db);

                // Update Accounting
                await db.TblAccountings.Where(x => x.VoucherId == existing.SalesInvoiceRtnId && x.Type == "SalesReturn").ExecuteDeleteAsync();

                // Mark Details as Deleted
                await db.SalesInvoiceRtnDtls.Where(x => x.FK_SalesInvoiceRtnHdrId == existing.SalesInvoiceRtnId)
                      .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsDeleted, true));

                await db.SaveChangesAsync();
                await transaction.CommitAsync();
                return new OutputResponse(existing.SalesInvoiceRtnId, "Cancelled Successfully", null);
            }

            // Editing / Updating Logic
            var enableUpdateSetting = await _settingsRepo.GetSetting("SalesReturn_EnableUpdate");
            if (string.IsNullOrEmpty(enableUpdateSetting))
                enableUpdateSetting = await _settingsRepo.GetSetting("SalesInvoiceRtn_EnableUpdate");
            bool enableUpdate = enableUpdateSetting == "1" || enableUpdateSetting.Equals("true", StringComparison.OrdinalIgnoreCase);
            if (!enableUpdate)
            {
                return new OutputResponse(0, "UpdateDisabledInSettings");
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
            existing.SalesInvoiceRtnDate = hdr.SalesInvoiceRtnDate;
            existing.EntryDate = hdr.EntryDate;
            existing.BillType = hdr.BillType;

            // remove old details
            // Reverse Stock
            await _stockService.ReverseStockMovements(existing.SalesInvoiceRtnNo, "Sales Return", existing.FK_BranchId, db);
            await db.SalesInvoiceRtnDtls.Where(x => x.FK_SalesInvoiceRtnHdrId == hdr.SalesInvoiceRtnId).ExecuteDeleteAsync();

            var details = entity.QuotationDtls?.Select(d => new SalesInvoiceRtnDtls
            {
                FK_SalesInvoiceRtnHdrId = hdr.SalesInvoiceRtnId,
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
            }).ToList() ?? new List<SalesInvoiceRtnDtls>();

            if (details.Count > 0)
            {
                await db.SalesInvoiceRtnDtls.AddRangeAsync(details);
            }

            // Populate Base Unit and Resolve QtyPerUnit for Stock Movement
            if (entity.QuotationDtls != null)
            {
                foreach (var item in entity.QuotationDtls)
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
            tHdr.HdrId = existing.SalesInvoiceRtnId;
            tHdr.HdrNo = existing.SalesInvoiceRtnNo;
            tHdr.PartyId = existing.FK_CustomerId;
            tHdr.VoucherType = "Sales Return";
            tHdr.Remarks = $"By Return No: {existing.SalesInvoiceRtnNo}";
            await _stockService.StockMovementForTransaction(db, entity.QuotationDtls, tHdr, existing.SalesInvoiceRtnDate.ToDateTime(), true);

            // Update Accounting
            await db.TblAccountings.Where(x => x.VoucherId == hdr.SalesInvoiceRtnId && x.Type == "SalesReturn").ExecuteDeleteAsync();
            await PostToLedger(db, hdr);

            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return new OutputResponse(existing.SalesInvoiceRtnId, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new OutputResponse(0, ex.Message);
        }
    }

    private async Task PostToLedger(TenantDBContext db, SalesInvoiceRtnHdr hdr)
    {
        var accountingList = new List<TblAccounting>();

        // 1. Sales Return Account (Debit) - Code: 310001020001
        var salesReturnAcc = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.Code == "310001020001");
        int? salesReturnAccId = salesReturnAcc?.AccountId;

        // 2. VAT Output Account (Debit - Reversing) - Code: 210002010001
        var vatAccount = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.Code == "210002010001");
        int? vatAccId = vatAccount?.AccountId;

        // 3. Credit Account (Customer / Cash / Bank)
        int? creditAccId = null;

        // Determine Payment Mode/Account
        // Sales Return usually credits the customer if it was a credit sale.
        // Or Cash if it's Cash return.
        // Assuming Logic based on Payment Mode or defaulting to Customer for Credit.
        // Does SalesInvoiceRtnHdr have BillType? Yes, CopyTo<TransactionHdr> implies it might, checking updateAsync...
        // hdr DOES NOT seem to have BillType explicitly used in logic but `UpdateAsync` maps it. 
        // Let's check definition of SalesInvoiceRtnHdr to be sure.
        // Assuming BillType exists.

        if (string.Equals(hdr.BillType, "Cash", StringComparison.OrdinalIgnoreCase))
        {
            var cashSetting = await db.PermanentSettings.AsNoTracking().FirstOrDefaultAsync(x => x.SettingKey == "DefaultCashAccount");
            if (cashSetting != null && int.TryParse(cashSetting.SettingValue, out int defCashId) && defCashId > 0)
            {
                var defCashAcc = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.AccountId == defCashId);
                creditAccId = defCashAcc?.AccountId;
            }
            if (!creditAccId.HasValue)
            {
                var cashAcc = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.Code.StartsWith("11000101") || x.Name == "Cash in Hand" || x.Name.Contains("Cash"));
                creditAccId = cashAcc?.AccountId;
            }
        }
        else if (string.Equals(hdr.BillType, "Credit", StringComparison.OrdinalIgnoreCase)) // Customer
        {
            var customer = await db.CustomerMasters.AsNoTracking().FirstOrDefaultAsync(x => x.CustomerId == hdr.FK_CustomerId);
            if (customer != null)
            {
                var custAcc = await db.AccountMasters.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Code == customer.Code);
                creditAccId = custAcc?.AccountId;
            }
        }
        else // Bank / Card / Transfer
        {
            var bankSetting = await db.PermanentSettings.AsNoTracking().FirstOrDefaultAsync(x => x.SettingKey == "DefaultBankAccount");
            if (bankSetting != null && int.TryParse(bankSetting.SettingValue, out int defBankId) && defBankId > 0)
            {
                var defAcc = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.AccountId == defBankId);
                creditAccId = defAcc?.AccountId;
            }
            if (!creditAccId.HasValue)
            {
                var bankAcc = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.Code.StartsWith("11000102") || x.Name.Contains("Bank") || (x.NameArabic != null && x.NameArabic.Contains("بنك")));
                creditAccId = bankAcc?.AccountId;
            }
        }

        // Create Entries

        // Debit Sales Return (Taxable Amount)
        if (salesReturnAccId.HasValue && hdr.TaxableAmount > 0)
        {
            accountingList.Add(new TblAccounting
            {
                EntryDate = hdr.SalesInvoiceRtnDate ?? DateTime.Now,
                VoucherId = hdr.SalesInvoiceRtnId,
                VoucherNo = hdr.SalesInvoiceRtnNo,
                FK_AccountId = salesReturnAccId,
                Amount = hdr.TaxableAmount, // Debit (+ve)
                Narration = $"Sales Return - {hdr.CustomerName}",
                Type = "SalesReturn",
                BillType = hdr.BillType,
                FK_BranchId = hdr.FK_BranchId,
                FK_UserId = hdr.FK_UserId,
                FK_CustomerId = hdr.FK_CustomerId
            });
        }

        // Debit VAT Output (Tax Total) used to reduce liability
        if (vatAccId.HasValue && hdr.TaxTotal > 0)
        {
            accountingList.Add(new TblAccounting
            {
                EntryDate = hdr.SalesInvoiceRtnDate ?? DateTime.Now,
                VoucherId = hdr.SalesInvoiceRtnId,
                VoucherNo = hdr.SalesInvoiceRtnNo,
                FK_AccountId = vatAccId,
                Amount = hdr.TaxTotal, // Debit (+ve)
                Narration = $"VAT Output Reversal - {hdr.SalesInvoiceRtnNo}",
                Type = "SalesReturn",
                BillType = hdr.BillType,
                FK_BranchId = hdr.FK_BranchId,
                FK_UserId = hdr.FK_UserId,
                FK_CustomerId = hdr.FK_CustomerId
            });
        }

        // Credit Customer/Cash (Grand Total)
        if (creditAccId.HasValue && hdr.GrandTotal > 0)
        {
            accountingList.Add(new TblAccounting
            {
                EntryDate = hdr.SalesInvoiceRtnDate ?? DateTime.Now,
                VoucherId = hdr.SalesInvoiceRtnId,
                VoucherNo = hdr.SalesInvoiceRtnNo,
                FK_AccountId = creditAccId,
                Amount = -hdr.GrandTotal, // Credit (-ve)
                Narration = $"Sales Return Refund/Credit - {hdr.SalesInvoiceRtnNo}",
                Type = "SalesReturn",
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

    private async Task<bool> HasReceiptPaidAsync(TenantDBContext db, int salesInvoiceId)
    {
        return await db.CustomerReceiptDtls
            .Join(db.CustomerReceiptHdrs,
                d => d.FK_CustomerReceiptId,
                h => h.CustomerReceiptId,
                (d, h) => new { d, h })
            .AnyAsync(x => x.d.FK_InvoiceId == salesInvoiceId
                        && x.d.PaymentAmount > 0
                        && !x.h.IsDeleted);
    }
}



