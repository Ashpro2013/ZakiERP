using AshproSAASERP.Models.Transactions.Accounts;

namespace AshproSAASERP.Repo.Masters;

public class AccountOpeningEntryRepo(IDbContextProvider contextProvider) : IAccountOpeningEntryRepo
{
    public async Task<string> GetNewDocNo(int branchId)
    {
        await using var db = contextProvider.CreateDbContext();
        var result = await db.AccountOpeningEntryHdrs
            .Where(x => x.EntryDate.HasValue && x.EntryDate.Value.Month == DateTime.Now.Month && x.EntryDate.Value.Year == DateTime.Now.Year && x.FK_BranchId == branchId)
            .OrderByDescending(x => x.AccountOpeningEntryId)
            .Select(x => x.VoucherNo)
            .FirstOrDefaultAsync();
        if (string.IsNullOrEmpty(result))
        {
            return "1001";
        }
        if (int.TryParse(result, out int no))
        {
            return (no + 1).ToString();
        }
        return "1001";
    }

    public async Task<OutputResponse> CreateAsync(AccountOpeningEntryHdr value, List<AccountOpeningEntryDtls> details)
    {
        await using var db = contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var next = await GetNewDocNo(value.FK_BranchId ?? 0);
            value.VoucherNo = next;

            await db.AccountOpeningEntryHdrs.AddAsync(value);
            await db.SaveChangesAsync();

            int headerId = value.AccountOpeningEntryId;

            foreach (var item in details)
            {
                item.FK_AccountOpeningEntryId = headerId;
                item.Account = null;
            }

            await db.AccountOpeningEntryDtls.AddRangeAsync(details);
            await db.SaveChangesAsync();

            await PostToLedger(headerId, value.VoucherDate, details, value.FK_BranchId, value.FK_UserId, value.VoucherNo);

            await transaction.CommitAsync();
            return new OutputResponse(headerId, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new OutputResponse(0, ex.InnerException?.Message ?? ex.Message);
        }
    }

    public async Task<OutputResponse> UpdateAsync(AccountOpeningEntryHdr value, List<AccountOpeningEntryDtls> details)
    {
        await using var db = contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var existingHeader = await db.AccountOpeningEntryHdrs.FirstOrDefaultAsync(x => x.AccountOpeningEntryId == value.AccountOpeningEntryId);
            if (existingHeader == null)
            {
                return new OutputResponse(0, "Record not found");
            }

            db.Entry(existingHeader).CurrentValues.SetValues(value);

            var existingDetails = await db.AccountOpeningEntryDtls
                .Where(x => x.FK_AccountOpeningEntryId == value.AccountOpeningEntryId)
                .ToListAsync();

            foreach (var item in details)
            {
                var existingItem = existingDetails.FirstOrDefault(x => x.FK_AccountId == item.FK_AccountId);
                if (existingItem != null)
                {
                    existingItem.Debit = item.Debit;
                    existingItem.Credit = item.Credit;
                    existingItem.Narration = item.Narration;
                    existingItem.IsDeleted = item.IsDeleted;
                }
                else
                {
                    item.FK_AccountOpeningEntryId = value.AccountOpeningEntryId;
                    item.Account = null;
                    await db.AccountOpeningEntryDtls.AddAsync(item);
                }
            }

            await db.SaveChangesAsync();

            await PostToLedger(value.AccountOpeningEntryId, value.VoucherDate, details, value.FK_BranchId, value.FK_UserId, value.VoucherNo);

            await transaction.CommitAsync();
            return new OutputResponse(value.AccountOpeningEntryId, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new OutputResponse(0, ex.InnerException?.Message ?? ex.Message);
        }
    }

    public async Task<OutputResponse> DeleteAsync(int id)
    {
        await using var db = contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var header = await db.AccountOpeningEntryHdrs.FirstOrDefaultAsync(x => x.AccountOpeningEntryId == id);
            if (header == null)
            {
                return new OutputResponse(0, "Record not found");
            }

            var journals = await db.TblAccountings.Where(x => x.VoucherId == id && x.Narration.StartsWith("Accounts Opening Entry By:")).ToListAsync();
            db.TblAccountings.RemoveRange(journals);

            header.IsDeleted = true;
            var details = await db.AccountOpeningEntryDtls.Where(x => x.FK_AccountOpeningEntryId == id).ToListAsync();
            details.ForEach(x => x.IsDeleted = true);

            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return new OutputResponse(id, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new OutputResponse(0, ex.Message);
        }
    }

    public async Task<AccountOpeningEntryHdr> GetAsync(int id)
    {
        await using var db = contextProvider.CreateDbContext();
        return await db.AccountOpeningEntryHdrs.AsNoTracking().FirstOrDefaultAsync(x => x.AccountOpeningEntryId == id) ?? new AccountOpeningEntryHdr();
    }

    public async Task<AccountOpeningEntryDTO> GetFullAsync(int id)
    {
        await using var db = contextProvider.CreateDbContext();
        var header = await db.AccountOpeningEntryHdrs.AsNoTracking().FirstOrDefaultAsync(x => x.AccountOpeningEntryId == id);
        if (header != null)
        {
            var details = await db.AccountOpeningEntryDtls
                .AsNoTracking()
                .Where(x => x.FK_AccountOpeningEntryId == id)
                .Include(x => x.Account)
                .ToListAsync();
            return new AccountOpeningEntryDTO(header, details);
        }
        return new AccountOpeningEntryDTO { Message = "Record not found" };
    }

    public async Task<(List<AccountOpeningEntryHdr>, int)> GetAllWithPagination(PaginationModel pagination)
    {
        await using var db = contextProvider.CreateDbContext();
        var query = db.AccountOpeningEntryHdrs.AsNoTracking().Where(x => !x.IsDeleted && x.FK_BranchId == pagination.BranchId).AsQueryable();

        if (!string.IsNullOrEmpty(pagination.search))
        {
            var term = pagination.search.ToLower();
            query = query.Where(x => x.VoucherNo.Contains(term));
        }

        var total = await query.CountAsync();
        query = query.OrderByDescending(x => x.AccountOpeningEntryId);

        var list = await query.Skip((pagination.page - 1) * pagination.pageSize).Take(pagination.pageSize).ToListAsync();

        return (list, total);
    }

    public async Task<List<AccountOpeningEntryHdr>> GetAllAsync()
    {
        await using var db = contextProvider.CreateDbContext();
        return await db.AccountOpeningEntryHdrs.AsNoTracking().Where(x => !x.IsDeleted).OrderByDescending(x => x.AccountOpeningEntryId).ToListAsync();
    }

    private async Task PostToLedger(int voucherId, DateTime? entryDate, List<AccountOpeningEntryDtls> details, int? branchId, int? userId, string voucherNo)
    {
        await using var db = contextProvider.CreateDbContext();
        // Optimization: Batch Delete with Chunking to avoid SQL Parameter Limit (2100)
        var accountIds = details.Where(x => x.FK_AccountId.HasValue).Select(x => x.FK_AccountId).Distinct().ToList();

        if (accountIds.Any())
        {
            const int batchSize = 2000;
            for (int i = 0; i < accountIds.Count; i += batchSize)
            {
                var batch = accountIds.Skip(i).Take(batchSize).ToList();
                await db.TblAccountings
                    .Where(x => x.Type == "AccountOpeningEntry" && x.FK_AccountId.HasValue && batch.Contains(x.FK_AccountId) && x.FK_BranchId == branchId)
                    .ExecuteDeleteAsync();
            }
        }

        var ledgerEntries = new List<TblAccounting>();

        foreach (var item in details)
        {
            // User Request: "insert if it zero also"
            // We treat the input as the absolute truth for this account's opening balance.
            // Even if Debit is 0, we insert it as explicitly requested to establish the zero balance.

            // Debit Entry
            var debitEntry = new TblAccounting
            {
                EntryDate = entryDate,
                VoucherId = voucherId,
                VoucherNo = voucherNo,
                FK_AccountId = item.FK_AccountId,
                Amount = item.Debit ?? 0,
                Narration = !string.IsNullOrEmpty(item.Narration) ? item.Narration : $"Accounts Opening Entry By:{voucherNo}",
                Type = "AccountOpeningEntry",
                BillType = "Opening Balance", // Standardized
                FK_BranchId = branchId,
                FK_UserId = userId
            };
            ledgerEntries.Add(debitEntry);

            // Credit Entry
            var creditEntry = new TblAccounting
            {
                EntryDate = entryDate,
                VoucherId = voucherId,
                VoucherNo = voucherNo,
                FK_AccountId = item.FK_AccountId,
                Amount = -(item.Credit ?? 0), // Negative for Credit
                Narration = !string.IsNullOrEmpty(item.Narration) ? item.Narration : $"Accounts Opening Entry By:{voucherNo}",
                Type = "AccountOpeningEntry",
                BillType = "Opening Balance", // Standardized
                FK_BranchId = branchId,
                FK_UserId = userId
            };
            ledgerEntries.Add(creditEntry);
        }

        if (ledgerEntries.Any())
        {
            // EF Core typically handles Batch Insert chunking automatically, but for very large sets, 
            // explicit UpdateRange/AddRange is usually fine.
            await db.TblAccountings.AddRangeAsync(ledgerEntries);
            await db.SaveChangesAsync();
        }
    }

    public async Task<List<AccountOpeningEntryDtls>> GetExistingOpeningBalances(int branchId)
    {
        await using var db = contextProvider.CreateDbContext();
        var data = await db.TblAccountings
            .Where(x => x.Type == "AccountOpeningEntry" && x.FK_BranchId == branchId)
            .GroupBy(x => x.FK_AccountId)
            .Select(g => new
            {
                AccountId = g.Key,
                DebitSum = g.Where(x => x.Amount > 0).Sum(x => x.Amount),
                CreditSum = g.Where(x => x.Amount < 0).Sum(x => Math.Abs(x.Amount ?? 0))
            }).ToListAsync();

        var accountIds = data.Select(x => x.AccountId).ToList();
        var accounts = await db.AccountMasters.Where(x => accountIds.Contains(x.AccountId)).ToListAsync();

        var result = new List<AccountOpeningEntryDtls>();
        foreach (var item in data)
        {
            if (item.DebitSum == 0 && item.CreditSum == 0) continue;

            var dtls = new AccountOpeningEntryDtls
            {
                FK_AccountId = item.AccountId,
                Debit = item.DebitSum > item.CreditSum ? item.DebitSum - item.CreditSum : 0,
                Credit = item.CreditSum > item.DebitSum ? item.CreditSum - item.DebitSum : 0,
                Account = accounts.FirstOrDefault(x => x.AccountId == item.AccountId)
            };
            result.Add(dtls);
        }
        return result;
    }
}
