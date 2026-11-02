using ZakiSAASERP.Models.Transactions.Accounts;

namespace ZakiSAASERP.Repo.Masters
{
    public class SupplierOPEntryRepo(IDbContextProvider contextProvider) : ISupplierOPEntryRepo
    {
        public async Task<string> GetNewDocNo()
        {
            await using var db = contextProvider.CreateDbContext();
            try
            {
                var maxDoc = await db.SupplierOPEntries
                   .AsNoTracking()
                   .OrderByDescending(x => x.SupplierOPEntryId)
                   .Select(x => x.DocNo)
                   .FirstOrDefaultAsync();

                if (int.TryParse(maxDoc, out int val))
                {
                    return (val + 1).ToString();
                }
                return "1";
            }
            catch
            {
                return "1";
            }
        }

        public async Task<OutputResponse> CreateAsync(SupplierOPEntry header, List<SupplierOPEntryDetails> details)
        {
            await using var db = contextProvider.CreateDbContext();
            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                header.DocNo = await GetNewDocNo();

                await db.SupplierOPEntries.AddAsync(header);
                await db.SaveChangesAsync();

                int headerId = header.SupplierOPEntryId;

                foreach (var item in details)
                {
                    item.FK_SupplierOPEntryId = headerId;
                    item.DocNo = header.DocNo;
                    item.EntryDate = header.EntryDate;

                    if (item.FK_SupplierId.HasValue)
                    {
                        await db.SupplierOPEntryUpdated
                           .Where(x => x.FK_SupplierId == item.FK_SupplierId)
                           .ExecuteDeleteAsync();
                    }

                    item.Supplier = null;
                }

                await db.SupplierOPEntryDetails.AddRangeAsync(details);

                var updatedEntries = details.Select(d => new SupplierOPEntryUpdated
                {
                    FK_SupplierOPEntryId = headerId,
                    FK_SupplierId = d.FK_SupplierId,
                    DocNo = d.DocNo,
                    EntryDate = d.EntryDate,
                    PurchaseNo = d.PurchaseNo,
                    PurchaseDate = d.PurchaseDate,
                    PurchaseAmt = d.PurchaseAmt,
                    ReceivedAmt = d.ReceivedAmt,
                    DiscountAmt = d.DiscountAmt,
                    ReturnedAmt = d.ReturnedAmt,
                    BalanceAmt = d.BalanceAmt,
                    FK_BranchId = d.FK_BranchId
                }).ToList();

                await db.SupplierOPEntryUpdated.AddRangeAsync(updatedEntries);

                // Posting to TblAccounting - INSIDE transaction for proper rollback
                await PostToLedger(db, header.SupplierOPEntryId, header.EntryDate, details, header.FK_BranchId, header.FK_UserId);

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

        private async Task PostToLedger(TenantDBContext db, int voucherId, DateTime entryDate, List<SupplierOPEntryDetails> details, int branchId, int? userId)
        {
            var accountingList = new List<TblAccounting>();

            // Find Opening Balance Equity Account - Code: 330001010001
            var openingHead = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.Code == "330001010001");
            if (openingHead == null)
            {
                throw new Exception("Default Account 'Opening Balance Equity' (Code: 330001010001) not found. Please ensure database scripts are executed.");
            }
            int? openingHeadId = openingHead.AccountId;

            // Get Supplier Accounts
            var supplierIds = details.Where(x => x.FK_SupplierId.HasValue).Select(x => x.FK_SupplierId.Value).Distinct().ToList();
            var supplierData = await db.SupplierMasters
                .Where(x => supplierIds.Contains(x.SupplierId))
                .Select(x => new { x.SupplierId, x.Code, x.FK_AccountId })
                .ToListAsync();

            // Fallback: If FK_AccountId is null, find by Code
            var missingAccountCodes = supplierData
                .Where(x => x.FK_AccountId == null)
                .Select(x => x.Code)
                .Distinct()
                .ToList();

            var accountHeadsMap = new Dictionary<string, int>();
            if (missingAccountCodes.Any())
            {
                accountHeadsMap = await db.AccountMasters
                    .Where(x => missingAccountCodes.Contains(x.Code))
                    .Select(x => new { x.Code, x.AccountId })
                    .ToDictionaryAsync(x => x.Code, x => x.AccountId);
            }


            await db.TblAccountings
               .Where(x => x.VoucherId == voucherId && x.Type == "SupplierOPEntry")
               .ExecuteDeleteAsync();

            // 1. Cleanup Old Entries for ALL involved Suppliers First (By FK_SupplierId)
            foreach (var supp in supplierData)
            {
                // Delete entries where this Supplier is involved in a SupplierOPEntry
                // This is the robust "ERP Standard" way: Delete by Entity ID + Transaction Type
                await db.TblAccountings.Where(x =>
                     x.Type == "SupplierOPEntry" &&
                     x.FK_SupplierId == supp.SupplierId
                ).ExecuteDeleteAsync();
            }

            foreach (var item in details)
            {
                var supp = supplierData.FirstOrDefault(x => x.SupplierId == item.FK_SupplierId);
                if (supp == null) continue;

                int? headId = supp.FK_AccountId;
                if (headId == null && accountHeadsMap.ContainsKey(supp.Code)) headId = accountHeadsMap[supp.Code];

                if (supp.Code != null)
                {

                    // Narration with Reference
                    var narr1 = $"Opening Balance - {supp.Code} - Ref: {item.PurchaseNo ?? item.DocNo}";
                    var narr2 = $"Opening Balance Offset - {supp.Code}";

                    if (headId.HasValue && openingHeadId.HasValue)
                    {
                        // Credit Supplier (Liability)
                        accountingList.Add(new TblAccounting
                        {
                            EntryDate = entryDate,
                            VoucherId = voucherId,
                            VoucherNo = item.DocNo,
                            FK_AccountId = headId,
                            Amount = -(decimal?)item.BalanceAmt, // Credit (Negative)
                            Narration = narr1,
                            Type = "SupplierOPEntry",
                            BillType = "Opening Balance",
                            FK_BranchId = branchId,
                            FK_UserId = userId,
                            FK_SupplierId = item.FK_SupplierId // Store Supplier ID
                        });

                        // Debit Opening Equity (offset)
                        accountingList.Add(new TblAccounting
                        {
                            EntryDate = entryDate,
                            VoucherId = voucherId,
                            VoucherNo = item.DocNo,
                            FK_AccountId = openingHeadId,
                            Amount = (decimal?)item.BalanceAmt, // Debit (Positive)
                            Narration = narr2,
                            Type = "SupplierOPEntry",
                            BillType = "Opening Balance",
                            FK_BranchId = branchId,
                            FK_UserId = userId,
                            FK_SupplierId = item.FK_SupplierId // Link Offset to Supplier for Cleanup Integrity
                        });
                    }
                    else if (!headId.HasValue)
                    {
                        throw new Exception($"Account not found for Supplier Code: {supp.Code}. Please ensure Supplier has a corresponding Account.");
                    }
                }
            }

            if (accountingList.Count > 0)
            {
                await db.TblAccountings.AddRangeAsync(accountingList);
            }
        }

        public async Task<OutputResponse> UpdateAsync(SupplierOPEntry header, List<SupplierOPEntryDetails> details)
        {
            await using var db = contextProvider.CreateDbContext();
            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                var existingHeader = await db.SupplierOPEntries.FindAsync(header.SupplierOPEntryId);
                if (existingHeader == null) return new OutputResponse(0, "Record not found");

                existingHeader.EnteredBy = header.EnteredBy;
                existingHeader.Remarks = header.Remarks;
                existingHeader.EntryDate = header.EntryDate;
                existingHeader.FK_UserId = header.FK_UserId;

                await db.SupplierOPEntryDetails.Where(x => x.FK_SupplierOPEntryId == header.SupplierOPEntryId).ExecuteDeleteAsync();

                await db.SupplierOPEntryUpdated.Where(x => x.FK_SupplierOPEntryId == header.SupplierOPEntryId).ExecuteDeleteAsync();

                foreach (var item in details)
                {
                    item.FK_SupplierOPEntryId = header.SupplierOPEntryId;
                    item.DocNo = header.DocNo;
                    item.EntryDate = header.EntryDate;

                    if (item.FK_SupplierId.HasValue)
                    {
                        await db.SupplierOPEntryUpdated
                           .Where(x => x.FK_SupplierId == item.FK_SupplierId && x.FK_BranchId == item.FK_BranchId)
                           .ExecuteDeleteAsync();
                    }

                    item.Supplier = null;
                }

                await db.SupplierOPEntryDetails.AddRangeAsync(details);

                var updatedEntries = details.Select(d => new SupplierOPEntryUpdated
                {
                    FK_SupplierOPEntryId = header.SupplierOPEntryId,
                    FK_SupplierId = d.FK_SupplierId,
                    DocNo = d.DocNo,
                    EntryDate = d.EntryDate,
                    PurchaseNo = d.PurchaseNo,
                    PurchaseDate = d.PurchaseDate,
                    PurchaseAmt = d.PurchaseAmt,
                    ReceivedAmt = d.ReceivedAmt,
                    DiscountAmt = d.DiscountAmt,
                    ReturnedAmt = d.ReturnedAmt,
                    BalanceAmt = d.BalanceAmt,
                    FK_BranchId = d.FK_BranchId
                }).ToList();
                await db.SupplierOPEntryUpdated.AddRangeAsync(updatedEntries);

                // Update Accounting - INSIDE transaction for proper rollback
                await db.TblAccountings.Where(x => x.VoucherId == header.SupplierOPEntryId && x.Type == "SupplierOPEntry").ExecuteDeleteAsync();
                await PostToLedger(db, header.SupplierOPEntryId, header.EntryDate, details, header.FK_BranchId, header.FK_UserId);

                await db.SaveChangesAsync();
                await transaction.CommitAsync();

                return new OutputResponse(header.SupplierOPEntryId, null);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new OutputResponse(0, ex.Message);
            }
        }

        public async Task<OutputResponse> DeleteAsync(int id)
        {
            await using var db = contextProvider.CreateDbContext();
            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {

                var docNo = await db.SupplierOPEntries.Where(x => x.SupplierOPEntryId == id).Select(x => x.DocNo).FirstOrDefaultAsync();

                await db.SupplierOPEntryDetails.Where(x => x.FK_SupplierOPEntryId == id).ExecuteDeleteAsync();
                await db.SupplierOPEntryUpdated.Where(x => x.FK_SupplierOPEntryId == id).ExecuteDeleteAsync();

                if (!string.IsNullOrEmpty(docNo))
                {
                    await db.TblAccountings.Where(x => x.VoucherId == id && x.Type == "SupplierOPEntry").ExecuteDeleteAsync();
                }

                await db.SupplierOPEntries.Where(x => x.SupplierOPEntryId == id).ExecuteDeleteAsync();

                await transaction.CommitAsync();
                return new OutputResponse(id, null);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new OutputResponse(0, ex.Message);
            }
        }

        public async Task<List<SupplierOPEntry>> GetAllAsync()
        {
            await using var db = contextProvider.CreateDbContext();
            return await db.SupplierOPEntries.AsNoTracking().ToListAsync();
        }

        public async Task<SupplierOPEntry> GetAsync(int id)
        {
            await using var db = contextProvider.CreateDbContext();
            return await db.SupplierOPEntries.AsNoTracking().FirstOrDefaultAsync(x => x.SupplierOPEntryId == id);
        }

        public async Task<SupplierOPEntryDTO> GetFullAsync(int id)
        {
            await using var db = contextProvider.CreateDbContext();
            var header = await db.SupplierOPEntries
               .Include(x => x.Branch)
               .AsNoTracking()
               .FirstOrDefaultAsync(x => x.SupplierOPEntryId == id);

            if (header == null) return null;

            var details = await db.SupplierOPEntryDetails
                .Where(x => x.FK_SupplierOPEntryId == id)
                .AsNoTracking()
                .Select(x => new SupplierOPEntryDetails
                {
                    SupplierOPEntryDetailsId = x.SupplierOPEntryDetailsId,
                    FK_SupplierOPEntryId = x.FK_SupplierOPEntryId,
                    FK_SupplierId = x.FK_SupplierId,
                    DocNo = x.DocNo,
                    EntryDate = x.EntryDate,
                    PurchaseNo = x.PurchaseNo,
                    PurchaseDate = x.PurchaseDate,
                    PurchaseAmt = x.PurchaseAmt,
                    ReceivedAmt = x.ReceivedAmt,
                    DiscountAmt = x.DiscountAmt,
                    ReturnedAmt = x.ReturnedAmt,
                    BalanceAmt = x.BalanceAmt,
                    FK_BranchId = x.FK_BranchId,
                    Supplier = x.Supplier == null ? null : new SupplierMaster
                    {
                        SupplierId = x.Supplier.SupplierId,
                        Code = x.Supplier.Code,
                        Name = x.Supplier.Name,
                        NameArabic = x.Supplier.NameArabic
                    }
                })
                .ToListAsync();

            return new SupplierOPEntryDTO
            {
                SupplierOPEntryId = header.SupplierOPEntryId,
                DocNo = header.DocNo,
                EntryDate = header.EntryDate,
                EnteredBy = header.EnteredBy,
                Remarks = header.Remarks,
                FK_BranchId = header.FK_BranchId,
                BranchName = header.Branch?.Name,
                FK_UserId = header.FK_UserId,
                EntryDetails = details
            };
        }

        public async Task<(List<SupplierOPEntry>, int)> GetAllWithPagination(PaginationModel pagination)
        {
            await using var db = contextProvider.CreateDbContext();
            var query = db.SupplierOPEntries.Include(x => x.Branch).AsNoTracking().AsSplitQuery();

            if (!string.IsNullOrWhiteSpace(pagination.search))
            {
                var term = pagination.search.ToLower();
                query = query.Where(x => x.DocNo.ToLower().Contains(term) || x.EnteredBy.ToLower().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(pagination.sortColumn))
            {
                bool desc = pagination.sortDirection == "desc";
                query = pagination.sortColumn switch
                {
                    "DocNo" => desc ? query.OrderByDescending(x => x.DocNo) : query.OrderBy(x => x.DocNo),
                    "EntryDate" => desc ? query.OrderByDescending(x => x.EntryDate) : query.OrderBy(x => x.EntryDate),
                    _ => desc ? query.OrderByDescending(x => x.SupplierOPEntryId) : query.OrderBy(x => x.SupplierOPEntryId)
                };
            }
            else
            {
                query = query.OrderByDescending(x => x.SupplierOPEntryId);
            }

            int count = await query.CountAsync();
            var list = await query
                .Skip((pagination.page - 1) * pagination.pageSize)
                .Take(pagination.pageSize)
                .ToListAsync();
            return (list, count);
        }
    }
}
