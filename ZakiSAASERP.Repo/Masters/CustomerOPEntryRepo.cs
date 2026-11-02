using ZakiSAASERP.Models.Transactions.Accounts;

namespace ZakiSAASERP.Repo.Masters
{
    public class CustomerOPEntryRepo(IDbContextProvider contextProvider) : ICustomerOPEntryRepo
    {
        public async Task<string> GetNewDocNo()
        {
            await using var db = contextProvider.CreateDbContext();
            try
            {
                var maxDoc = await db.CustomerOPEntries
                   .AsNoTracking()
                   .OrderByDescending(x => x.CustomerOPEntryId)
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

        public async Task<OutputResponse> CreateAsync(CustomerOPEntry header, List<CustomerOPEntryDetails> details)
        {
            await using var db = contextProvider.CreateDbContext();
            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                // 1. Get DocNo (Simple auto-increment logic or similar to SP)
                header.DocNo = await GetNewDocNo();

                // 2. Insert Header
                await db.CustomerOPEntries.AddAsync(header);
                await db.SaveChangesAsync(); // Get Header ID

                int headerId = header.CustomerOPEntryId;

                // 3. Process each detail item
                foreach (var item in details)
                {
                    item.FK_CustomerOPEntryId = headerId;
                    item.DocNo = header.DocNo;
                    item.EntryDate = header.EntryDate;

                    if (item.FK_CustomerId.HasValue)
                    {
                        await db.CustomerOPEntryUpdated
                           .Where(x => x.FK_CustomerId == item.FK_CustomerId)
                           .ExecuteDeleteAsync();
                    }


                    item.Customer = null;
                }

                // 4. Insert into Details Table
                await db.CustomerOPEntryDetails.AddRangeAsync(details);

                // 5. Insert into Updated Table (Clone logic)
                var updatedEntries = details.Select(d => new CustomerOPEntryUpdated
                {
                    FK_CustomerOPEntryId = headerId,
                    FK_CustomerId = d.FK_CustomerId,
                    DocNo = d.DocNo,
                    EntryDate = d.EntryDate,
                    InvoiceNo = d.InvoiceNo,
                    InvoiceDate = d.InvoiceDate,
                    InvoiceAmt = d.InvoiceAmt,
                    ReceivedAmt = d.ReceivedAmt,
                    DiscountAmt = d.DiscountAmt,
                    ReturnedAmt = d.ReturnedAmt,
                    BalanceAmt = d.BalanceAmt,
                    FK_BranchId = d.FK_BranchId,
                    PONo = d.PONo
                }).ToList();

                await db.CustomerOPEntryUpdated.AddRangeAsync(updatedEntries);

                // Posting to TblAccounting - INSIDE transaction for proper rollback
                await PostToLedger(db, header.CustomerOPEntryId, header.EntryDate, details, header.FK_BranchId, header.FK_UserId);

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

        private async Task PostToLedger(TenantDBContext db, int voucherId, DateTime entryDate, List<CustomerOPEntryDetails> details, int branchId, int? userId)
        {
            var accountingList = new List<TblAccounting>();

            // Find Opening Balance Equity Account - Code: 330001010001
            var openingHead = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.Code == "330001010001");
            if (openingHead == null)
            {
                throw new Exception("Default Account 'Opening Balance Equity' (Code: 330001010001) not found. Please ensure database scripts are executed.");
            }
            int? openingHeadId = openingHead.AccountId;

            // Get all AccountIds for involved customers
            var customerIds = details.Where(x => x.FK_CustomerId.HasValue).Select(x => x.FK_CustomerId.Value).Distinct().ToList();
            var customerHeads = await db.CustomerMasters
                .Where(x => customerIds.Contains(x.CustomerId))
                .Select(x => new { x.CustomerId, x.Code, x.FK_AccountId })
                .ToListAsync();

            var customerCodes = customerHeads.Where(x => x.FK_AccountId == null || x.FK_AccountId == 0).Select(x => x.Code).ToList();
            var accountHeadsMap = new Dictionary<string, int>();

            if (customerCodes.Any())
            {
                accountHeadsMap = await db.AccountMasters
                    .Where(x => customerCodes.Contains(x.Code))
                    .Select(x => new { x.Code, x.AccountId })
                    .ToDictionaryAsync(x => x.Code, x => x.AccountId);
            }

            // 1. Cleanup Old Entries for ALL involved Customers First (By FK_CustomerId)
            foreach (var cust in customerHeads)
            {
                // Delete entries where this Customer is involved in a CustomerOPEntry
                // Robust "ERP Standard" Cleanup
                await db.TblAccountings.Where(x =>
                     x.Type == "CustomerOPEntry" &&
                     x.FK_CustomerId == cust.CustomerId
                ).ExecuteDeleteAsync();
            }

            foreach (var item in details)
            {
                var custData = customerHeads.FirstOrDefault(x => x.CustomerId == item.FK_CustomerId);
                var customerCode = custData?.Code;
                int? headId = custData?.FK_AccountId;

                if ((headId == null || headId == 0) && customerCode != null && accountHeadsMap.ContainsKey(customerCode))
                {
                    headId = accountHeadsMap[customerCode];
                }

                if (customerCode != null)
                {
                    // Narration with Invoice Reference
                    var narr1 = $"Opening Balance - {customerCode} - Inv: {item.InvoiceNo}";
                    var narr2 = $"Opening Balance Offset - {customerCode}";

                    // IMPORTANT: Only create entries if BOTH accounts exist (Double-Entry requirement)
                    if (headId.HasValue && openingHeadId.HasValue)
                    {
                        // Debit Customer Control (Asset)
                        accountingList.Add(new TblAccounting
                        {
                            EntryDate = entryDate,
                            VoucherId = voucherId,
                            // Use the specific Invoice No as the Voucher No for the Ledger so it appears in Statements
                            VoucherNo = !string.IsNullOrEmpty(item.InvoiceNo) ? item.InvoiceNo : item.DocNo,
                            FK_AccountId = headId,
                            Amount = (decimal?)item.BalanceAmt, // Debit
                            Narration = narr1,
                            Type = "CustomerOPEntry",
                            BillType = "Opening Balance",
                            FK_BranchId = branchId,
                            FK_UserId = userId,
                            FK_CustomerId = item.FK_CustomerId // Store Customer ID
                        });

                        // Credit Opening Equity
                        accountingList.Add(new TblAccounting
                        {
                            EntryDate = entryDate,
                            VoucherId = voucherId,
                            VoucherNo = !string.IsNullOrEmpty(item.InvoiceNo) ? item.InvoiceNo : item.DocNo,
                            FK_AccountId = openingHeadId,
                            Amount = -(decimal?)item.BalanceAmt, // Credit (Negative)
                            Narration = narr2,
                            Type = "CustomerOPEntry",
                            BillType = "Opening Balance",
                            FK_BranchId = branchId,
                            FK_UserId = userId,
                            FK_CustomerId = item.FK_CustomerId // Link Offset to Customer for Cleanup Integrity
                        });
                    }
                    else if (!headId.HasValue)
                    {
                        throw new Exception($"Account not found for Customer Code: {customerCode}. Please ensure Customer has a corresponding Account.");
                    }
                }
            }

            if (accountingList.Count > 0)
            {
                await db.TblAccountings.AddRangeAsync(accountingList);
            }
        }

        public async Task<OutputResponse> UpdateAsync(CustomerOPEntry header, List<CustomerOPEntryDetails> details)
        {
            await using var db = contextProvider.CreateDbContext();
            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                // 1. Update Header
                var existingHeader = await db.CustomerOPEntries.FindAsync(header.CustomerOPEntryId);
                if (existingHeader == null) return new OutputResponse(0, "Record not found");

                existingHeader.EnteredBy = header.EnteredBy;
                existingHeader.Remarks = header.Remarks;
                existingHeader.EntryDate = header.EntryDate;
                existingHeader.FK_UserId = header.FK_UserId;

                // 2. Delete old details for this ID
                await db.CustomerOPEntryDetails.Where(x => x.FK_CustomerOPEntryId == header.CustomerOPEntryId).ExecuteDeleteAsync();

                await db.CustomerOPEntryUpdated.Where(x => x.FK_CustomerOPEntryId == header.CustomerOPEntryId).ExecuteDeleteAsync();

                // 3. Re-insert logic (similar to Create but checking updated table specifically)
                foreach (var item in details)
                {
                    item.FK_CustomerOPEntryId = header.CustomerOPEntryId;
                    item.DocNo = header.DocNo;
                    item.EntryDate = header.EntryDate;

                    // A. Remove any CURRENT 'Updated' entry for this customer (even if from another doc? Replicating SP logic: "DELETE oud... WHERE oud.BranchCode = @Code")
                    if (item.FK_CustomerId.HasValue)
                    {
                        await db.CustomerOPEntryUpdated
                           .Where(x => x.FK_CustomerId == item.FK_CustomerId && x.FK_BranchId == item.FK_BranchId)
                           .ExecuteDeleteAsync();
                    }

                    // Prevent EF from trying to insert/update the Customer entity itself
                    item.Customer = null;
                }

                await db.CustomerOPEntryDetails.AddRangeAsync(details);

                var updatedEntries = details.Select(d => new CustomerOPEntryUpdated
                {
                    FK_CustomerOPEntryId = header.CustomerOPEntryId,
                    FK_CustomerId = d.FK_CustomerId,
                    DocNo = d.DocNo,
                    EntryDate = d.EntryDate,
                    InvoiceNo = d.InvoiceNo,
                    InvoiceDate = d.InvoiceDate,
                    InvoiceAmt = d.InvoiceAmt,
                    ReceivedAmt = d.ReceivedAmt,
                    DiscountAmt = d.DiscountAmt,
                    ReturnedAmt = d.ReturnedAmt,
                    BalanceAmt = d.BalanceAmt,
                    FK_BranchId = d.FK_BranchId,
                    PONo = d.PONo
                }).ToList();
                await db.CustomerOPEntryUpdated.AddRangeAsync(updatedEntries);

                // Update Accounting - INSIDE transaction for proper rollback
                await db.TblAccountings.Where(x => x.VoucherId == header.CustomerOPEntryId && x.Type == "CustomerOPEntry").ExecuteDeleteAsync();
                await PostToLedger(db, header.CustomerOPEntryId, header.EntryDate, details, header.FK_BranchId, header.FK_UserId);

                await db.SaveChangesAsync();
                await transaction.CommitAsync();

                return new OutputResponse(header.CustomerOPEntryId, null);
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

                var docNo = await db.CustomerOPEntries.Where(x => x.CustomerOPEntryId == id).Select(x => x.DocNo).FirstOrDefaultAsync();

                // Delete Details
                var details = await db.CustomerOPEntryDetails.Where(x => x.FK_CustomerOPEntryId == id).ExecuteDeleteAsync();
                var updated = await db.CustomerOPEntryUpdated.Where(x => x.FK_CustomerOPEntryId == id).ExecuteDeleteAsync();

                if (!string.IsNullOrEmpty(docNo))
                {
                    await db.TblAccountings.Where(x => x.VoucherId == id && x.Type == "CustomerOPEntry").ExecuteDeleteAsync();
                }

                // Delete Header
                var header = await db.CustomerOPEntries.Where(x => x.CustomerOPEntryId == id).ExecuteDeleteAsync();

                await transaction.CommitAsync();
                return new OutputResponse(id, null);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new OutputResponse(0, ex.Message);
            }
        }

        public async Task<List<CustomerOPEntry>> GetAllAsync()
        {
            await using var db = contextProvider.CreateDbContext();
            return await db.CustomerOPEntries.AsNoTracking().ToListAsync();
        }

        public async Task<CustomerOPEntry> GetAsync(int id)
        {
            await using var db = contextProvider.CreateDbContext();
            return await db.CustomerOPEntries.AsNoTracking().FirstOrDefaultAsync(x => x.CustomerOPEntryId == id);
        }

        public async Task<CustomerOPEntryDTO> GetFullAsync(int id)
        {
            await using var db = contextProvider.CreateDbContext();
            var header = await db.CustomerOPEntries
               .Include(x => x.Branch)
               .AsNoTracking()
               .FirstOrDefaultAsync(x => x.CustomerOPEntryId == id);

            if (header == null) return null;

            var details = await db.CustomerOPEntryDetails
                .Where(x => x.FK_CustomerOPEntryId == id)
                .AsNoTracking()
                .Select(x => new CustomerOPEntryDetails
                {
                    CustomerOPEntryDetailsId = x.CustomerOPEntryDetailsId,
                    FK_CustomerOPEntryId = x.FK_CustomerOPEntryId,
                    FK_CustomerId = x.FK_CustomerId,
                    DocNo = x.DocNo,
                    EntryDate = x.EntryDate,
                    InvoiceNo = x.InvoiceNo,
                    InvoiceDate = x.InvoiceDate,
                    InvoiceAmt = x.InvoiceAmt,
                    ReceivedAmt = x.ReceivedAmt,
                    DiscountAmt = x.DiscountAmt,
                    ReturnedAmt = x.ReturnedAmt,
                    BalanceAmt = x.BalanceAmt,
                    FK_BranchId = x.FK_BranchId,
                    PONo = x.PONo,
                    Customer = x.Customer == null ? null : new CustomerMaster
                    {
                        CustomerId = x.Customer.CustomerId,
                        Code = x.Customer.Code,
                        Name = x.Customer.Name,
                        NameArabic = x.Customer.NameArabic
                    }
                })
                .ToListAsync();

            return new CustomerOPEntryDTO
            {
                CustomerOPEntryId = header.CustomerOPEntryId,
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

        public async Task<(List<CustomerOPEntry>, int)> GetAllWithPagination(PaginationModel pagination)
        {
            await using var db = contextProvider.CreateDbContext();
            var query = db.CustomerOPEntries.Include(x => x.Branch).AsNoTracking().AsSplitQuery();

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
                    _ => desc ? query.OrderByDescending(x => x.CustomerOPEntryId) : query.OrderBy(x => x.CustomerOPEntryId)
                };
            }
            else
            {
                query = query.OrderByDescending(x => x.CustomerOPEntryId);
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
