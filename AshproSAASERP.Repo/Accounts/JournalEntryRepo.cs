using AshproSAASERP.Interface.Transaction.Accounts;
using AshproSAASERP.Models.Transactions.Accounts;
using AshproSAASERP.Repo.CodeGenerationServices;
using Microsoft.Extensions.Logging;

namespace AshproSAASERP.Repo.Accounts;

public class JournalEntryRepo(IDbContextProvider contextProvider, CodeSequenceService codeService, ILogger<JournalEntryRepo> logger) : IJournalEntryRepo
{
    public async Task<string> GetNextJournalNoAsync()
    {
        await using var db = contextProvider.CreateDbContext();
        return await codeService.GetNextUniqueCodeAsync(
            CodeKeys.JournalEntry,
            async (code) => await db.JournalEntryHdrs.AnyAsync(x => x.JournalNo == code)
        );
    }

    public async Task<OutputResponse> CreateAsync(JournalEntryDTO entity)
    {
        await using var db = contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            // Validate double-entry
            var validationResult = ValidateDoubleEntry(entity.Details);
            if (validationResult.Id <= 0)
            {
                return validationResult;
            }

            // Generate Journal No if auto
            bool isAuto = await codeService.IsAutoCodeEnabledAsync("JournalEntry", db);
            if (string.IsNullOrEmpty(entity.JournalNo) || entity.JournalNo == "0")
            {
                entity.JournalNo = await GetNextJournalNoAsync();
            }

            // Calculate totals (Base + VAT)
            entity.TotalDebit = entity.Details.Sum(x => x.DebitAmount + x.DebitVat);
            entity.TotalCredit = entity.Details.Sum(x => x.CreditAmount + x.CreditVat);

            // Create header
            var hdr = new JournalEntryHdr
            {
                JournalNo = entity.JournalNo,
                JournalDate = entity.JournalDate,
                FK_BranchId = entity.FK_BranchId,
                FK_UserId = entity.FK_UserId,
                Narration = entity.Narration,
                ReferenceNo = entity.ReferenceNo,
                FK_ProjectId = entity.FK_ProjectId,
                FK_SalesmanId = entity.FK_SalesmanId,
                TotalDebit = entity.TotalDebit,
                TotalCredit = entity.TotalCredit,
                Status = entity.Status,
                CreatedDate = DateTime.Now
            };

            await db.JournalEntryHdrs.AddAsync(hdr);
            await db.SaveChangesAsync();

            if (isAuto)
            {
                await codeService.ConsumeCodeAsync("JournalEntry", db);
            }

            // Create details - using new entities to avoid navigation property tracking issues
            int lineNo = 1;
            foreach (var dtl in entity.Details)
            {
                var newDtl = new JournalEntryDtls
                {
                    FK_JournalId = hdr.JournalId,
                    FK_AccountId = dtl.FK_AccountId,
                    DebitAmount = dtl.DebitAmount,
                    CreditAmount = dtl.CreditAmount,
                    Narration = dtl.Narration,
                    LineNo = lineNo++,
                    // Extended properties
                    InvoiceNo = dtl.InvoiceNo,
                    InvoiceAmt = dtl.InvoiceAmt,
                    VatNo = dtl.VatNo,
                    Name = dtl.Name,
                    AccType = dtl.AccType,
                    DebitVat = dtl.DebitVat,
                    CreditVat = dtl.CreditVat,
                    VatPer = dtl.VatPer,
                    VatId = dtl.VatId,
                    DebitVatper = dtl.DebitVatper,
                    CreditVatper = dtl.CreditVatper,
                    TotalDebit = dtl.TotalDebit,
                    TotalCredit = dtl.TotalCredit
                };
                await db.JournalEntryDtls.AddAsync(newDtl);
            }

            await db.SaveChangesAsync();

            // Post to TblAccounting if Posted
            if (entity.Status == "Posted")
            {
                await PostToLedger(db, hdr, entity.Details);
                await db.SaveChangesAsync();
            }

            await transaction.CommitAsync();
            return new OutputResponse(hdr.JournalId, "SuccessfullySaved");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Error creating journal entry");
            return new OutputResponse(0, "ErrorOccurred");
        }
    }

    public async Task<OutputResponse> UpdateAsync(JournalEntryDTO entity)
    {
        await using var db = contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            // Validate double-entry
            var validationResult = ValidateDoubleEntry(entity.Details);
            if (validationResult.Id <= 0)
            {
                return validationResult;
            }

            var hdr = await db.JournalEntryHdrs.FindAsync(entity.JournalId);
            if (hdr == null)
            {
                return new OutputResponse(0, "RecordNotFound");
            }

            // Calculate totals (Base + VAT)
            entity.TotalDebit = entity.Details.Sum(x => x.DebitAmount + x.DebitVat);
            entity.TotalCredit = entity.Details.Sum(x => x.CreditAmount + x.CreditVat);

            // Update header
            hdr.JournalDate = entity.JournalDate;
            hdr.FK_BranchId = entity.FK_BranchId;
            hdr.Narration = entity.Narration;
            hdr.ReferenceNo = entity.ReferenceNo;
            hdr.FK_ProjectId = entity.FK_ProjectId;
            hdr.FK_SalesmanId = entity.FK_SalesmanId;
            hdr.TotalDebit = entity.TotalDebit;
            hdr.TotalCredit = entity.TotalCredit;
            hdr.Status = entity.Status;
            hdr.ModifiedDate = DateTime.Now;

            // Delete old details
            var existingDtls = await db.JournalEntryDtls
                .Where(x => x.FK_JournalId == entity.JournalId)
                .ToListAsync();
            db.JournalEntryDtls.RemoveRange(existingDtls);

            // Delete old accounting entries
            await db.TblAccountings
                .Where(x => x.Type == "JournalEntry" && x.VoucherId == hdr.JournalId)
                .ExecuteDeleteAsync();

            // Add new details - using new entities to avoid navigation property tracking issues  
            int lineNo = 1;
            foreach (var dtl in entity.Details)
            {
                var newDtl = new JournalEntryDtls
                {
                    FK_JournalId = hdr.JournalId,
                    FK_AccountId = dtl.FK_AccountId,
                    DebitAmount = dtl.DebitAmount,
                    CreditAmount = dtl.CreditAmount,
                    Narration = dtl.Narration,
                    LineNo = lineNo++,
                    // Extended properties
                    InvoiceNo = dtl.InvoiceNo,
                    InvoiceAmt = dtl.InvoiceAmt,
                    VatNo = dtl.VatNo,
                    Name = dtl.Name,
                    AccType = dtl.AccType,
                    DebitVat = dtl.DebitVat,
                    CreditVat = dtl.CreditVat,
                    VatPer = dtl.VatPer,
                    VatId = dtl.VatId,
                    DebitVatper = dtl.DebitVatper,
                    CreditVatper = dtl.CreditVatper,
                    TotalDebit = dtl.TotalDebit,
                    TotalCredit = dtl.TotalCredit
                };
                await db.JournalEntryDtls.AddAsync(newDtl);
            }

            await db.SaveChangesAsync();

            // Post to TblAccounting if Posted
            if (entity.Status == "Posted")
            {
                await PostToLedger(db, hdr, entity.Details);
            }

            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return new OutputResponse(hdr.JournalId, "SuccessfullyUpdated");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Error updating journal entry");
            return new OutputResponse(0, "ErrorOccurred");
        }
    }

    public async Task<OutputResponse> DeleteAsync(int id)
    {
        await using var db = contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var hdr = await db.JournalEntryHdrs.FindAsync(id);
            if (hdr == null)
            {
                return new OutputResponse(0, "RecordNotFound");
            }

            // Delete accounting entries
            await db.TblAccountings
                .Where(x => x.Type == "JournalEntry" && x.VoucherId == hdr.JournalId)
                .ExecuteDeleteAsync();

            // Delete journal (details cascade)
            db.JournalEntryHdrs.Remove(hdr);
            await db.SaveChangesAsync();

            await transaction.CommitAsync();
            return new OutputResponse(1, "SuccessfullyDeleted");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Error deleting journal entry");
            return new OutputResponse(0, "ErrorOccurred");
        }
    }

    public async Task<List<JournalEntryHdr>> GetAllAsync()
    {
        await using var db = contextProvider.CreateDbContext();
        return await db.JournalEntryHdrs
            .AsNoTracking()
            .OrderByDescending(x => x.JournalDate)
            .ThenByDescending(x => x.JournalId)
            .ToListAsync();
    }

    public async Task<JournalEntryDTO?> GetByIdAsync(int id)
    {
        await using var db = contextProvider.CreateDbContext();
        var hdr = await db.JournalEntryHdrs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.JournalId == id);

        if (hdr == null) return null;

        var dtls = await db.JournalEntryDtls
            .AsNoTracking()
            .Include(x => x.Account)
            .Where(x => x.FK_JournalId == id)
            .OrderBy(x => x.LineNo)
            .ToListAsync();

        // Populate AccountName and AccountCode
        foreach (var dtl in dtls)
        {
            dtl.AccountName = dtl.Account?.Name;
            dtl.AccountCode = dtl.Account?.Code;
        }

        return new JournalEntryDTO
        {
            JournalId = hdr.JournalId,
            JournalNo = hdr.JournalNo,
            JournalDate = hdr.JournalDate,
            FK_BranchId = hdr.FK_BranchId,
            FK_UserId = hdr.FK_UserId,
            Narration = hdr.Narration,
            ReferenceNo = hdr.ReferenceNo,
            FK_ProjectId = hdr.FK_ProjectId,
            FK_SalesmanId = hdr.FK_SalesmanId,
            TotalDebit = hdr.TotalDebit,
            TotalCredit = hdr.TotalCredit,
            Status = hdr.Status,
            Details = dtls
        };
    }

    #region Private Methods

    private OutputResponse ValidateDoubleEntry(List<JournalEntryDtls> details)
    {
        if (details == null || details.Count < 2)
        {
            return new OutputResponse(0, "AtLeastTwoLinesRequired");
        }

        decimal totalDebit = details.Sum(x => x.DebitAmount + x.DebitVat);
        decimal totalCredit = details.Sum(x => x.CreditAmount + x.CreditVat);

        if (Math.Abs(totalDebit - totalCredit) > 0.001m)
        {
            return new OutputResponse(0, "DebitCreditMustBalance");
        }

        // Check no line has both debit and credit
        foreach (var line in details)
        {
            if (line.DebitAmount > 0 && line.CreditAmount > 0)
            {
                return new OutputResponse(0, "LineCannotHaveBothDebitAndCredit");
            }

            if (line.DebitAmount == 0 && line.CreditAmount == 0)
            {
                return new OutputResponse(0, "LineMusHaveDebitOrCredit");
            }
        }

        return new OutputResponse(1, "Valid");
    }

    private async Task PostToLedger(TenantDBContext db, JournalEntryHdr hdr, List<JournalEntryDtls> details)
    {
        var accountingList = new List<TblAccounting>();

        // Fetch VAT Account IDs
        var vatInputAccount = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.Code == "110003010001");
        int? vatInputAccId = vatInputAccount?.AccountId;

        var vatOutputAccount = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.Code == "210002010001");
        int? vatOutputAccId = vatOutputAccount?.AccountId;

        foreach (var dtl in details)
        {
            decimal amount = dtl.DebitAmount > 0 ? dtl.DebitAmount : -dtl.CreditAmount;

            // Construct standardized built-in narration
            string baseNarration = $"Journal Entry - {hdr.JournalNo}";
            string headerNarration = string.IsNullOrWhiteSpace(hdr.Narration) ? baseNarration : $"{baseNarration} - {hdr.Narration}";

            accountingList.Add(new TblAccounting
            {
                EntryDate = hdr.JournalDate,
                VoucherId = hdr.JournalId,
                VoucherNo = hdr.JournalNo,
                FK_AccountId = dtl.FK_AccountId,
                Amount = amount,
                // Use line narration if present, otherwise fallback to standardized header narration
                Narration = string.IsNullOrWhiteSpace(dtl.Narration) ? headerNarration : dtl.Narration,
                Type = "JournalEntry",
                BillType = "Journal",
                FK_BranchId = hdr.FK_BranchId,
                FK_UserId = hdr.FK_UserId
            });

            // VAT Logic
            if (dtl.VatPer > 0)
            {
                if (dtl.DebitVat > 0 && vatInputAccId.HasValue)
                {
                    accountingList.Add(new TblAccounting
                    {
                        EntryDate = hdr.JournalDate,
                        VoucherId = hdr.JournalId,
                        VoucherNo = hdr.JournalNo,
                        FK_AccountId = vatInputAccId,
                        Amount = dtl.DebitVat,
                        Narration = $"VAT Input - {hdr.JournalNo}",
                        Type = "JournalEntry",
                        BillType = "Journal",
                        FK_BranchId = hdr.FK_BranchId,
                        FK_UserId = hdr.FK_UserId
                    });
                }
                else if (dtl.CreditVat > 0 && vatOutputAccId.HasValue)
                {
                    accountingList.Add(new TblAccounting
                    {
                        EntryDate = hdr.JournalDate,
                        VoucherId = hdr.JournalId,
                        VoucherNo = hdr.JournalNo,
                        FK_AccountId = vatOutputAccId,
                        Amount = -dtl.CreditVat, // Credit must be negative
                        Narration = $"VAT Output - {hdr.JournalNo}",
                        Type = "JournalEntry",
                        BillType = "Journal",
                        FK_BranchId = hdr.FK_BranchId,
                        FK_UserId = hdr.FK_UserId
                    });
                }
            }
        }

        await db.TblAccountings.AddRangeAsync(accountingList);
    }

    #endregion
}



