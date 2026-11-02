using AshproSAASERP.Models.Transactions.Accounts;
using AshproSAASERP.Repo.Transaction.Inventory;

namespace AshproSAASERP.Repo.Masters
{
    public class OpeningStockEntryRepo(IDbContextProvider contextProvider, StockService stockService) : IOpeningStockEntryRepo
    {
        public async Task<string> GetNewDocNo()
        {
            try
            {
                await using var db = contextProvider.CreateDbContext();
                var maxDoc = await db.OpeningStockEntries
                   .AsNoTracking()
                   .OrderByDescending(x => x.OpeningStockEntryId)
                   .Select(x => x.DocumentNo)
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

        public async Task<List<OpeningStockEntryItemForUpdate>> GetExistingStockForItem(int itemId)
        {
            await using var db = contextProvider.CreateDbContext();
            return await db.OpeningStockEntryItemForUpdate
                .Where(x => x.FK_ItemId == itemId)
                .Include(x => x.Store)
                .Include(x => x.Unit)
                .Include(x => x.Header)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<OutputResponse> CreateAsync(OpeningStockEntry header, List<OpeningStockEntryItem> details)
        {
            await using var db = contextProvider.CreateDbContext();
            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                header.DocumentNo = await GetNewDocNo();

                await db.OpeningStockEntries.AddAsync(header);
                await db.SaveChangesAsync();

                int headerId = header.OpeningStockEntryId;

                foreach (var item in details)
                {
                    item.FK_OpeningStockEntryId = headerId;
                    item.DocumentNo = header.DocumentNo;

                    await db.OpeningStockEntryItemForUpdate
                       .Where(x => x.FK_ItemId == item.FK_ItemId
                                && x.FK_StoreId == item.FK_StoreId
                                && x.FK_BranchId == header.FK_BranchId
                                && x.FK_UnitId == item.FK_UnitId)
                       .ExecuteDeleteAsync();

                    item.Item = null;
                    item.Store = null;
                    item.Unit = null;
                    item.Header = null;
                }

                await db.OpeningStockEntryItems.AddRangeAsync(details);

                var updatedEntries = details.Select(d => new OpeningStockEntryItemForUpdate
                {
                    FK_OpeningStockEntryId = headerId,
                    FK_ItemId = d.FK_ItemId,
                    FK_StoreId = d.FK_StoreId,
                    FK_BranchId = header.FK_BranchId,
                    FK_UnitId = d.FK_UnitId,
                    SlNo = d.SlNo,
                    Qty = d.Qty,
                    QtyPerUnit = d.QtyPerUnit,
                    Rate = d.Rate,
                    Amount = d.Amount,
                    VatPer = d.VatPer,
                    VatAmt = d.VatAmt,
                    LocName = d.LocName
                }).ToList();

                await db.OpeningStockEntryItemForUpdate.AddRangeAsync(updatedEntries);

                // Process stock movements for each item
                foreach (var item in details)
                {
                    await stockService.ReverseStockMovementsForItem(item.FK_ItemId, "OpeningStockEntry", header.FK_BranchId, db);

                    var baseUnitId = await GetItemBaseUnitId(item.FK_ItemId);
                    decimal actualQty = item.Qty * item.QtyPerUnit;
                    decimal actualRate = item.QtyPerUnit > 0 ? item.Rate / item.QtyPerUnit : item.Rate;

                    await stockService.ProcessStockMovement(new StockMovementDto
                    {
                        ItemId = item.FK_ItemId,
                        BranchId = header.FK_BranchId,
                        StoreId = item.FK_StoreId,
                        VoucherId = headerId,
                        VoucherNo = header.DocumentNo,
                        VoucherDate = header.EntryDate,
                        VoucherType = "OPENING",
                        FormType = "OpeningStockEntry",
                        IsInward = true,
                        Quantity = item.Qty,
                        UnitId = item.FK_UnitId,
                        ActualUnitId = baseUnitId,
                        ActualQuantity = actualQty,
                        Rate = item.Rate,
                        ActualRate = actualRate,
                        Narration = $"Opening Stock - {item.FK_ItemId}",
                        UserId = header.FK_UserId
                    }, db);
                }

                // Posting to TblAccounting - INSIDE transaction for proper rollback
                await PostToLedger(header.OpeningStockEntryId, header.EntryDate, details, header.FK_BranchId, header.FK_UserId, header.DocumentNo);

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

        private async Task PostToLedger(int voucherId, DateTime entryDate, List<OpeningStockEntryItem> details, int branchId, int? userId, string documentNo)
        {
            await using var db = contextProvider.CreateDbContext();
            var accountingList = new List<TblAccounting>();

            var openingHead = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.Code == "330001010001");
            if (openingHead == null)
            {
                throw new Exception("Default Account 'Opening Balance Equity' (Code: 330001010001) not found. Please ensure database scripts are executed.");
            }
            int? openingHeadId = openingHead.AccountId;

            // Stock Account (Inventory) - Code: 110004010001
            var stockHead = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.Code == "110004010001");
            if (stockHead == null)
            {
                throw new Exception("Default Account 'Inventory Asset' (Code: 110004010001) not found. Please ensure database scripts are executed.");
            }
            int? stockHeadId = stockHead.AccountId;

            foreach (var item in details)
            {
                // DELETE existing logic: Remove any 'OpeningStockEntry' type accounting entries for this Specific Item
                var narr1 = "Opening Stock - Item " + item.FK_ItemId;
                var narr2 = "Opening Stock Offset - Item " + item.FK_ItemId;

                await db.TblAccountings
                    .Where(x => x.Type == "OpeningStockEntry" && (x.Narration == narr1 || x.Narration == narr2))
                    .ExecuteDeleteAsync();

                // IMPORTANT: Only create entries if BOTH accounts exist (Double-Entry requirement)
                if (stockHeadId.HasValue && openingHeadId.HasValue)
                {
                    // Debit Stock (Asset)
                    accountingList.Add(new TblAccounting
                    {
                        EntryDate = entryDate,
                        VoucherId = voucherId,
                        VoucherNo = documentNo,
                        FK_AccountId = stockHeadId,
                        Amount = (decimal?)item.Amount, // Debit
                        Narration = narr1,
                        Type = "OpeningStockEntry",
                        BillType = "Debit",
                        FK_BranchId = branchId,
                        FK_UserId = userId
                    });

                    // Credit Opening Equity
                    accountingList.Add(new TblAccounting
                    {
                        EntryDate = entryDate,
                        VoucherId = voucherId,
                        VoucherNo = documentNo,
                        FK_AccountId = openingHeadId,
                        Amount = -(decimal?)item.Amount, // Credit
                        Narration = narr2,
                        Type = "OpeningStockEntry",
                        BillType = "Credit",
                        FK_BranchId = branchId,
                        FK_UserId = userId
                    });
                }
            }

            if (accountingList.Count > 0)
            {
                await db.TblAccountings.AddRangeAsync(accountingList);
            }
        }

        /// <summary>
        /// Get the base unit ID for an item from ItemMaster
        /// </summary>
        private async Task<int> GetItemBaseUnitId(int itemId)
        {
            await using var db = contextProvider.CreateDbContext();
            var item = await db.ItemMasters
                .AsNoTracking()
                .Where(x => x.ItemId == itemId)
                .Select(x => x.FK_UnitId)
                .FirstOrDefaultAsync();
            return item ?? 0;
        }

        public async Task<OutputResponse> UpdateAsync(OpeningStockEntry header, List<OpeningStockEntryItem> details)
        {
            await using var db = contextProvider.CreateDbContext();
            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                var existingHeader = await db.OpeningStockEntries.FindAsync(header.OpeningStockEntryId);
                if (existingHeader == null) return new OutputResponse(0, "Record not found");

                existingHeader.EnteredBy = header.EnteredBy;
                existingHeader.Remarks = header.Remarks;
                existingHeader.EntryDate = header.EntryDate;
                existingHeader.FK_UserId = header.FK_UserId;

                await db.OpeningStockEntryItems.Where(x => x.FK_OpeningStockEntryId == header.OpeningStockEntryId).ExecuteDeleteAsync();
                await db.OpeningStockEntryItemForUpdate.Where(x => x.FK_OpeningStockEntryId == header.OpeningStockEntryId).ExecuteDeleteAsync();

                foreach (var item in details)
                {
                    item.FK_OpeningStockEntryId = header.OpeningStockEntryId;
                    item.DocumentNo = header.DocumentNo;

                    await db.OpeningStockEntryItemForUpdate
                       .Where(x => x.FK_ItemId == item.FK_ItemId
                                && x.FK_StoreId == item.FK_StoreId
                                && x.FK_BranchId == header.FK_BranchId
                                && x.FK_UnitId == item.FK_UnitId)
                       .ExecuteDeleteAsync();

                    item.Item = null;
                    item.Store = null;
                    item.Unit = null;
                    item.Header = null;
                }

                await db.OpeningStockEntryItems.AddRangeAsync(details);

                var updatedEntries = details.Select(d => new OpeningStockEntryItemForUpdate
                {
                    FK_OpeningStockEntryId = header.OpeningStockEntryId,
                    FK_ItemId = d.FK_ItemId,
                    FK_StoreId = d.FK_StoreId,
                    FK_BranchId = header.FK_BranchId,
                    FK_UnitId = d.FK_UnitId,
                    SlNo = d.SlNo,
                    Qty = d.Qty,
                    QtyPerUnit = d.QtyPerUnit,
                    Rate = d.Rate,
                    Amount = d.Amount,
                    VatPer = d.VatPer,
                    VatAmt = d.VatAmt,
                    LocName = d.LocName
                }).ToList();
                await db.OpeningStockEntryItemForUpdate.AddRangeAsync(updatedEntries);

                // Reverse old stock movements and add new ones
                await stockService.ReverseStockMovements(header.DocumentNo, "OpeningStockEntry", header.FK_BranchId, db);

                foreach (var item in details)
                {
                    var baseUnitId = await GetItemBaseUnitId(item.FK_ItemId);
                    decimal actualQty = item.Qty * item.QtyPerUnit;
                    decimal actualRate = item.QtyPerUnit > 0 ? item.Rate / item.QtyPerUnit : item.Rate;

                    await stockService.ProcessStockMovement(new StockMovementDto
                    {
                        ItemId = item.FK_ItemId,
                        BranchId = header.FK_BranchId,
                        StoreId = item.FK_StoreId,
                        VoucherId = header.OpeningStockEntryId,
                        VoucherNo = header.DocumentNo,
                        VoucherDate = header.EntryDate,
                        VoucherType = "OPENING",
                        FormType = "OpeningStockEntry",
                        IsInward = true,
                        Quantity = item.Qty,
                        UnitId = item.FK_UnitId,
                        ActualUnitId = baseUnitId,
                        ActualQuantity = actualQty,
                        Rate = item.Rate,
                        ActualRate = actualRate,
                        Narration = $"Opening Stock - {item.FK_ItemId}",
                        UserId = header.FK_UserId
                    }, db);
                }

                // Update Accounting - INSIDE transaction for proper rollback
                await db.TblAccountings.Where(x => x.VoucherId == header.OpeningStockEntryId && x.Type == "OpeningStockEntry").ExecuteDeleteAsync();
                await PostToLedger(header.OpeningStockEntryId, header.EntryDate, details, header.FK_BranchId, header.FK_UserId, header.DocumentNo);

                await db.SaveChangesAsync();
                await transaction.CommitAsync();

                return new OutputResponse(header.OpeningStockEntryId, null);
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

                var entry = await db.OpeningStockEntries.Where(x => x.OpeningStockEntryId == id).FirstOrDefaultAsync();
                if (entry == null) return new OutputResponse(0, "Entry not found");

                // Reverse stock movements
                await stockService.ReverseStockMovements(entry.DocumentNo, "OpeningStockEntry", entry.FK_BranchId, db);
                await db.SaveChangesAsync();

                await db.OpeningStockEntryItems.Where(x => x.FK_OpeningStockEntryId == id).ExecuteDeleteAsync();
                await db.OpeningStockEntryItemForUpdate.Where(x => x.FK_OpeningStockEntryId == id).ExecuteDeleteAsync();

                if (!string.IsNullOrEmpty(entry.DocumentNo))
                {
                    await db.TblAccountings.Where(x => x.VoucherId == id && x.Type == "OpeningStockEntry").ExecuteDeleteAsync();
                }

                await db.OpeningStockEntries.Where(x => x.OpeningStockEntryId == id).ExecuteDeleteAsync();

                await transaction.CommitAsync();
                return new OutputResponse(id, null);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new OutputResponse(0, ex.Message);
            }
        }

        public async Task<List<OpeningStockEntry>> GetAllAsync()
        {
            await using var db = contextProvider.CreateDbContext();
            return await db.OpeningStockEntries.AsNoTracking().ToListAsync();
        }

        public async Task<OpeningStockEntry> GetAsync(int id)
        {
            await using var db = contextProvider.CreateDbContext();
            return await db.OpeningStockEntries.AsNoTracking().FirstOrDefaultAsync(x => x.OpeningStockEntryId == id);
        }

        public async Task<OpeningStockEntryDTO> GetFullAsync(int id)
        {
            await using var db = contextProvider.CreateDbContext();
            var header = await db.OpeningStockEntries
               .Include(x => x.Branch)
               .AsNoTracking()
               .FirstOrDefaultAsync(x => x.OpeningStockEntryId == id);

            if (header == null) return null;

            var details = await db.OpeningStockEntryItems
                .Where(x => x.FK_OpeningStockEntryId == id)
                .AsNoTracking()
                .Include(x => x.Item)
                .Include(x => x.Unit)
                .Include(x => x.Store)
                .ToListAsync();

            return new OpeningStockEntryDTO
            {
                OpeningStockEntryId = header.OpeningStockEntryId,
                DocumentNo = header.DocumentNo,
                EntryDate = header.EntryDate,
                EnteredBy = header.EnteredBy,
                Remarks = header.Remarks,
                FK_BranchId = header.FK_BranchId,
                BranchName = header.Branch?.Name,
                FK_UserId = header.FK_UserId,
                EntryDetails = details
            };
        }

        public async Task<(List<OpeningStockEntry>, int)> GetAllWithPagination(PaginationModel pagination)
        {
            await using var db = contextProvider.CreateDbContext();
            var query = db.OpeningStockEntries.Include(x => x.Branch).AsNoTracking().AsSplitQuery();

            if (!string.IsNullOrWhiteSpace(pagination.search))
            {
                var term = pagination.search.ToLower();
                query = query.Where(x => x.DocumentNo.ToLower().Contains(term) || x.EnteredBy.ToLower().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(pagination.sortColumn))
            {
                bool desc = pagination.sortDirection == "desc";
                query = pagination.sortColumn switch
                {
                    "DocumentNo" => desc ? query.OrderByDescending(x => x.DocumentNo) : query.OrderBy(x => x.DocumentNo),
                    "EntryDate" => desc ? query.OrderByDescending(x => x.EntryDate) : query.OrderBy(x => x.EntryDate),
                    _ => desc ? query.OrderByDescending(x => x.OpeningStockEntryId) : query.OrderBy(x => x.OpeningStockEntryId)
                };
            }
            else
            {
                query = query.OrderByDescending(x => x.OpeningStockEntryId);
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



