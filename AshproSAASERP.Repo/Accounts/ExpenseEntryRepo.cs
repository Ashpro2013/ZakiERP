using AshproSAASERP.Interface.Transaction.Accounts;
using AshproSAASERP.Models.Transactions.Accounts;
using AshproSAASERP.Repo.CodeGenerationServices;
using Microsoft.Extensions.Logging;

namespace AshproSAASERP.Repo.Accounts;

public class ExpenseEntryRepo(IDbContextProvider contextProvider, CodeSequenceService codeService, ILogger<ExpenseEntryRepo> logger) : IExpenseEntryRepo
{
    public async Task<string> GetNextExpenseNoAsync()
    {
        await using var db = contextProvider.CreateDbContext();
        return await codeService.GetNextUniqueCodeAsync(
            CodeKeys.ExpenseEntry,
            async (code) => await db.ExpenseEntryHdrs.AnyAsync(x => x.ExpenseNo == code)
        );
    }

    public async Task<OutputResponse> CreateAsync(ExpenseEntryDTO entity)
    {
        await using var db = contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            // Validate at least one detail line
            if (entity.ExpenseEntryDtls == null || entity.ExpenseEntryDtls.Count == 0)
            {
                return new OutputResponse(0, "AtLeastOneLineRequired");
            }

            // Generate Expense No if auto
            bool isAuto = await codeService.IsAutoCodeEnabledAsync("ExpenseEntry", db);
            if (string.IsNullOrEmpty(entity.ExpenseNo) || entity.ExpenseNo == "0")
            {
                entity.ExpenseNo = await GetNextExpenseNoAsync();
            }

            // Calculate totals
            entity.Gross = entity.ExpenseEntryDtls.Sum(x => x.Amount);
            entity.VatTotal = entity.ExpenseEntryDtls.Sum(x => x.VatAmt);
            entity.NetTotal = entity.Gross + entity.VatTotal;

            // Create header
            var hdr = new ExpenseEntryHdr
            {
                ExpenseNo = entity.ExpenseNo,
                EntryDate = entity.EntryDate,
                FK_AccountId = entity.FK_AccountId,
                FK_BranchId = entity.FK_BranchId,
                FK_UserId = entity.FK_UserId,
                FK_ProjectId = entity.FK_ProjectId,
                FK_SalesmanId = entity.FK_SalesmanId,
                FK_SupplierId = entity.FK_SupplierId,
                Gross = entity.Gross,
                VatTotal = entity.VatTotal,
                NetTotal = entity.NetTotal,
                Narration = entity.Narration,
                ReferenceNo = entity.ReferenceNo,
                BillType = entity.BillType,
                VatNo = entity.VatNo,
                TransactionType = "Expense",
                CreatedDate = DateTime.Now
            };

            await db.ExpenseEntryHdrs.AddAsync(hdr);
            await db.SaveChangesAsync();

            if (isAuto)
            {
                await codeService.ConsumeCodeAsync("ExpenseEntry", db);
            }

            // Create details - using new entities
            int lineNo = 1;
            foreach (var dtl in entity.ExpenseEntryDtls)
            {
                var newDtl = new ExpenseEntryDtls
                {
                    FK_ExpenseEntryId = hdr.ExpenseEntryId,
                    FK_AccountId = dtl.FK_AccountId,
                    FK_SupplierId = dtl.FK_SupplierId, // Save Supplier
                    Amount = dtl.Amount,
                    VatPer = dtl.VatPer,
                    VatAmt = dtl.VatAmt,
                    IncAmt = dtl.IncAmt,
                    Narration = dtl.Narration,
                    InvoiceNo = dtl.InvoiceNo,
                    VatNo = dtl.VatNo,
                    LineNo = lineNo++,
                };
                await db.ExpenseEntryDtls.AddAsync(newDtl);
            }

            await db.SaveChangesAsync();

            // Post to TblAccounting
            await PostToLedger(db, hdr, entity.ExpenseEntryDtls);
            await db.SaveChangesAsync();

            await transaction.CommitAsync();
            return new OutputResponse(hdr.ExpenseEntryId, "SuccessfullySaved");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Error creating expense entry");
            return new OutputResponse(0, "ErrorOccurred");
        }
    }

    public async Task<OutputResponse> UpdateAsync(ExpenseEntryDTO entity)
    {
        await using var db = contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            // Validate at least one detail line
            if (entity.ExpenseEntryDtls == null || entity.ExpenseEntryDtls.Count == 0)
            {
                return new OutputResponse(0, "AtLeastOneLineRequired");
            }

            var hdr = await db.ExpenseEntryHdrs.FindAsync(entity.ExpenseId);
            if (hdr == null)
            {
                return new OutputResponse(0, "RecordNotFound");
            }

            // Calculate totals
            entity.Gross = entity.ExpenseEntryDtls.Sum(x => x.Amount);
            entity.VatTotal = entity.ExpenseEntryDtls.Sum(x => x.VatAmt);
            entity.NetTotal = entity.Gross + entity.VatTotal;

            // Update header
            hdr.EntryDate = entity.EntryDate;
            hdr.FK_AccountId = entity.FK_AccountId;
            hdr.FK_BranchId = entity.FK_BranchId;
            hdr.FK_ProjectId = entity.FK_ProjectId;
            hdr.FK_SalesmanId = entity.FK_SalesmanId;
            hdr.FK_SupplierId = entity.FK_SupplierId;
            hdr.Gross = entity.Gross;
            hdr.VatTotal = entity.VatTotal;
            hdr.NetTotal = entity.NetTotal;
            hdr.Narration = entity.Narration;
            hdr.ReferenceNo = entity.ReferenceNo;
            hdr.BillType = entity.BillType;
            hdr.VatNo = entity.VatNo;
            hdr.ModifiedDate = DateTime.Now;

            // Delete old details
            var existingDtls = await db.ExpenseEntryDtls
                .Where(x => x.FK_ExpenseEntryId == entity.ExpenseId)
                .ToListAsync();
            db.ExpenseEntryDtls.RemoveRange(existingDtls);

            // Delete old accounting entries
            await db.TblAccountings
                .Where(x => x.Type == "ExpenseEntry" && x.VoucherId == hdr.ExpenseEntryId)
                .ExecuteDeleteAsync();

            // Add new details
            int lineNo = 1;
            foreach (var dtl in entity.ExpenseEntryDtls)
            {
                var newDtl = new ExpenseEntryDtls
                {
                    FK_ExpenseEntryId = hdr.ExpenseEntryId,
                    FK_AccountId = dtl.FK_AccountId,
                    FK_SupplierId = dtl.FK_SupplierId, // Save Supplier
                    Amount = dtl.Amount,
                    VatPer = dtl.VatPer,
                    VatAmt = dtl.VatAmt,
                    IncAmt = dtl.IncAmt,
                    Narration = dtl.Narration,
                    InvoiceNo = dtl.InvoiceNo,
                    VatNo = dtl.VatNo,
                    LineNo = lineNo++,
                };
                await db.ExpenseEntryDtls.AddAsync(newDtl);
            }

            await db.SaveChangesAsync();

            // Post to TblAccounting
            await PostToLedger(db, hdr, entity.ExpenseEntryDtls);
            await db.SaveChangesAsync();

            await transaction.CommitAsync();
            return new OutputResponse(hdr.ExpenseEntryId, "SuccessfullyUpdated");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Error updating expense entry");
            return new OutputResponse(0, "ErrorOccurred");
        }
    }

    public async Task<OutputResponse> DeleteAsync(int id)
    {
        await using var db = contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var hdr = await db.ExpenseEntryHdrs.FindAsync(id);
            if (hdr == null)
            {
                return new OutputResponse(0, "RecordNotFound");
            }

            // Delete accounting entries (Hard delete from ledger to reverse effect)
            await db.TblAccountings
                .Where(x => x.Type == "ExpenseEntry" && x.VoucherId == hdr.ExpenseEntryId)
                .ExecuteDeleteAsync();

            // Soft Delete Header
            hdr.IsDeleted = true;

            // Soft Delete Details
            var details = await db.ExpenseEntryDtls
                .Where(x => x.FK_ExpenseEntryId == id)
                .ToListAsync();

            foreach (var dtl in details)
            {
                dtl.IsDeleted = true;
            }

            await db.SaveChangesAsync();

            await transaction.CommitAsync();
            return new OutputResponse(1, "SuccessfullyDeleted");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Error deleting expense entry");
            return new OutputResponse(0, "ErrorOccurred");
        }
    }

    public async Task<List<ExpenseEntryHdr>> GetAllAsync()
    {
        await using var db = contextProvider.CreateDbContext();
        return await db.ExpenseEntryHdrs
            .AsNoTracking()
            .Where(x => x.IsDeleted != true)
            .OrderByDescending(x => x.EntryDate)
            .ThenByDescending(x => x.ExpenseEntryId)
            .ToListAsync();
    }

    public async Task<ExpenseEntryDTO?> GetByIdAsync(int id)
    {
        await using var db = contextProvider.CreateDbContext();
        var hdr = await db.ExpenseEntryHdrs
            .AsNoTracking()
            .Include(x => x.PaymentAccount)
            .Include(x => x.Supplier) // Include Header Supplier
            .FirstOrDefaultAsync(x => x.ExpenseEntryId == id);

        if (hdr == null) return null;

        var dtls = await db.ExpenseEntryDtls
            .AsNoTracking()
            .Include(x => x.Account)
            .Include(x => x.Supplier) // Include Detail Supplier
            .Where(x => x.FK_ExpenseEntryId == id && x.IsDeleted != true)
            .OrderBy(x => x.LineNo)
            .ToListAsync();

        // Populate Names
        foreach (var dtl in dtls)
        {
            dtl.AccountName = dtl.Account?.Name;
            dtl.AccountCode = dtl.Account?.Code;
            dtl.SupplierName = dtl.Supplier?.Name; // Map Supplier Name
            dtl.VatId = null;
        }

        return new ExpenseEntryDTO
        {
            ExpenseId = hdr.ExpenseEntryId,
            ExpenseNo = hdr.ExpenseNo,
            EntryDate = hdr.EntryDate,
            FK_AccountId = hdr.FK_AccountId,
            FK_BranchId = hdr.FK_BranchId,
            FK_UserId = hdr.FK_UserId,
            FK_ProjectId = hdr.FK_ProjectId,
            FK_SalesmanId = hdr.FK_SalesmanId,
            FK_SupplierId = hdr.FK_SupplierId,
            SupplierName = hdr.Supplier?.Name, // Map Header Supplier Name DTO Logic required? DTO has no SupplierName prop, but UI binds FK.
            Gross = hdr.Gross,
            VatTotal = hdr.VatTotal,
            NetTotal = hdr.NetTotal,
            Narration = hdr.Narration,
            ReferenceNo = hdr.ReferenceNo,
            BillType = hdr.BillType,
            VatNo = hdr.VatNo,
            PaymentAccountName = hdr.PaymentAccount?.Name,
            ExpenseEntryDtls = dtls
        };
    }

    public async Task<(List<ExpenseEntryHdr>, int)> GetAllWithPagination(PaginationModel pagination)
    {
        await using var db = contextProvider.CreateDbContext();
        var query = db.ExpenseEntryHdrs.AsNoTracking()
            .Where(x => x.FK_BranchId == pagination.BranchId && x.IsDeleted != true)
            .AsQueryable();

        if (!string.IsNullOrEmpty(pagination.search))
        {
            var term = pagination.search.ToLower();
            query = query.Where(x => x.ExpenseNo.Contains(term) ||
                                    (x.Narration != null && x.Narration.Contains(term)));
        }

        var total = await query.CountAsync();
        query = query.OrderByDescending(x => x.EntryDate).ThenByDescending(x => x.ExpenseEntryId);

        var list = await query.Skip((pagination.page - 1) * pagination.pageSize)
                             .Take(pagination.pageSize)
                             .ToListAsync();

        return (list, total);
    }

    #region Private Methods

    private async Task PostToLedger(TenantDBContext db, ExpenseEntryHdr hdr, List<ExpenseEntryDtls> details)
    {
        var accountingList = new List<TblAccounting>();

        // Fetch VAT Input Account (standard input VAT account)
        var vatInputAccount = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.Code == "110003010001");
        int? vatInputAccId = vatInputAccount?.AccountId;

        // Post each expense line
        foreach (var dtl in details)
        {
            // Debit expense account for Base Amount ONLY
            accountingList.Add(new TblAccounting
            {
                EntryDate = hdr.EntryDate,
                VoucherId = hdr.ExpenseEntryId,
                VoucherNo = hdr.ExpenseNo,
                FK_AccountId = dtl.FK_AccountId,
                Amount = dtl.Amount, // Base Amount
                Narration = (string.IsNullOrWhiteSpace(dtl.Narration)
                    ? $"Expense Entry - {hdr.ExpenseNo}"
                    : dtl.Narration) + (dtl.VatAmt > 0 ? " - VAT" : ""),
                Type = "ExpenseEntry",
                BillType = "Debit",
                FK_BranchId = hdr.FK_BranchId,
                FK_UserId = hdr.FK_UserId
            });

            // Calculate VAT if percentage exists
            if (dtl.VatPer > 0 && vatInputAccId.HasValue)
            {
                decimal vatAmount = dtl.VatAmt > 0 ? dtl.VatAmt : Math.Round(dtl.Amount * (dtl.VatPer / 100), 2);

                // Debit VAT Account
                accountingList.Add(new TblAccounting
                {
                    EntryDate = hdr.EntryDate,
                    VoucherId = hdr.ExpenseEntryId,
                    VoucherNo = hdr.ExpenseNo,
                    FK_AccountId = vatInputAccId,
                    Amount = vatAmount,
                    Narration = $"VAT Input - {hdr.ExpenseNo}",
                    Type = "ExpenseEntry",
                    BillType = hdr.BillType,
                    FK_BranchId = hdr.FK_BranchId,
                    FK_UserId = hdr.FK_UserId
                });
            }
        }

        if (accountingList.Count > 0)
        {
            await db.TblAccountings.AddRangeAsync(accountingList);
        }

        #endregion
    }
}
