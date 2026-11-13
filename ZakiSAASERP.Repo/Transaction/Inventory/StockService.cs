using Microsoft.EntityFrameworkCore;
using ZakiSAASERP.Interface;
using ZakiSAASERP.Models.Masters;
using ZakiSAASERP.Models.Transactions;
using ZakiSAASERP.Models.Transactions.Inventory;
using ZakiSAASERP.Models.ViewModel;

namespace ZakiSAASERP.Repo.Transaction.Inventory;

/// <summary>
/// Service for managing stock movements and balances.
/// Handles StockTranLog (audit history) and ItemStockBalance (atomic balance tracking).
/// </summary>
public class StockService
{
    /// <summary>
    /// Records a stock transaction in StockTranLog and updates ItemStockBalance atomically.
    /// </summary>
    public async Task ProcessStockMovement(StockMovementDto movement, TenantDBContext db)
    {
        if (movement == null || movement.ItemId == 0 || movement.BranchId == 0)
            return;

        // 1. Create StockTranLog entry
        var stockLog = new StockTranLog
        {
            FK_ItemId = movement.ItemId,
            VchrId = movement.VoucherId,
            VchrNo = movement.VoucherNo,
            VchrDate = movement.VoucherDate,
            VchrType = movement.VoucherType,
            VchrFormType = movement.FormType,
            AccountId = movement.AccountId,
            FK_StoreId = movement.StoreId,
            InQuantity = movement.IsInward ? movement.ActualQuantity : 0,
            OutQuantity = movement.IsInward ? 0 : movement.ActualQuantity,
            FK_UnitId = movement.UnitId,
            FK_ActualUnitId = movement.ActualUnitId,
            ActualUnitQty = movement.ActualQuantity,
            InRate = movement.IsInward ? movement.Rate : 0,
            OutRate = movement.IsInward ? 0 : movement.Rate,
            ActualRate = movement.ActualRate,
            Narration = movement.Narration,
            FK_UserId = movement.UserId,
            FK_BranchId = movement.BranchId,
            TranDate = DateTime.Now
        };

        await db.StockTranLogs.AddAsync(stockLog);

        // 2. Update ItemStockBalance atomically
        decimal qtyChange = movement.IsInward ? movement.ActualQuantity : -movement.ActualQuantity;
        if (qtyChange != 0)
        {
            await UpdateStockBalanceInternal(db, movement.ItemId, movement.BranchId, qtyChange);
        }
    }

    /// <summary>
    /// Process multiple stock movements in batch
    /// </summary>
    public async Task ProcessStockMovements(List<StockMovementDto> movements, TenantDBContext db)
    {
        if (movements == null || movements.Count == 0)
            return;

        foreach (var movement in movements)
        {
            await ProcessStockMovement(movement, db);
        }
    }

    /// <summary>
    /// Update stock balance only (without logging to StockTranLog)
    /// </summary>
    public async Task UpdateStockBalance(int itemId, int branchId, decimal qtyChange, TenantDBContext db)
    {
        await UpdateStockBalanceInternal(db, itemId, branchId, qtyChange);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Internal method to update stock balance using atomic database increments.
    /// Employs ExecuteUpdateAsync to directly increment CurrentQty at the database engine level,
    /// eliminating lost-update race conditions without requiring table-wide locks.
    /// Safely falls back to local entity tracking or new record creation if no row exists yet.
    /// </summary>
    private async Task UpdateStockBalanceInternal(TenantDBContext db, int itemId, int branchId, decimal qtyChange)
    {
        if (itemId == 0 || branchId == 0 || qtyChange == 0)
            return;

        // 1. Attempt atomic database increment directly via ExecuteUpdateAsync
        int rowsUpdated = await db.ItemStockBalances
            .Where(x => x.FK_ItemId == itemId && x.FK_BranchId == branchId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.CurrentQty, b => b.CurrentQty + qtyChange)
                .SetProperty(b => b.LastUpdated, DateTime.Now));

        if (rowsUpdated > 0)
        {
            // If the entity is also tracked locally in current context, keep its in-memory state in sync
            var local = db.ItemStockBalances.Local
                .FirstOrDefault(x => x.FK_ItemId == itemId && x.FK_BranchId == branchId);
            if (local != null)
            {
                local.CurrentQty += qtyChange;
                local.LastUpdated = DateTime.Now;
            }
            return;
        }

        // 2. Row does not exist in DB yet: check if it's already staged in ChangeTracker (added earlier in same transaction)
        var tracked = db.ItemStockBalances.Local
            .FirstOrDefault(x => x.FK_ItemId == itemId && x.FK_BranchId == branchId);

        if (tracked != null)
        {
            tracked.CurrentQty += qtyChange;
            tracked.LastUpdated = DateTime.Now;
        }
        else
        {
            // 3. Stage new stock balance row to be inserted when SaveChangesAsync is called
            db.ItemStockBalances.Add(new ItemStockBalance
            {
                FK_ItemId = itemId,
                FK_BranchId = branchId,
                CurrentQty = qtyChange,
                LastUpdated = DateTime.Now
            });
        }
    }

    /// <summary>
    /// Reverse stock movements for a voucher and restores ItemStockBalance accordingly.
    /// </summary>
    public async Task ReverseStockMovements(string voucherNo, string formType, int branchId, TenantDBContext db)
    {
        if (string.IsNullOrWhiteSpace(voucherNo))
            return;

        // Get all stock logs for this voucher
        var logs = await db.StockTranLogs
            .Where(x => x.VchrNo == voucherNo && x.VchrFormType == formType && x.FK_BranchId == branchId)
            .ToListAsync();

        if (logs.Count == 0)
            return;

        // Group by ItemId to compute net reverse quantity
        var itemGroups = logs
            .GroupBy(x => x.FK_ItemId)
            .Select(g => new
            {
                ItemId = g.Key,
                // Inward movement originally added InQuantity; reversing it deducts InQuantity.
                // Outward movement originally deducted OutQuantity; reversing it restores OutQuantity.
                NetReverseQty = g.Sum(x => (x.OutQuantity ?? 0) - (x.InQuantity ?? 0))
            })
            .ToList();

        foreach (var item in itemGroups)
        {
            if (item.NetReverseQty != 0)
            {
                await UpdateStockBalanceInternal(db, item.ItemId, branchId, item.NetReverseQty);
            }
        }

        // Delete the stock logs
        db.StockTranLogs.RemoveRange(logs);
    }

    /// <summary>
    /// Reverse stock movements for a specific item within a form type and branch
    /// Used when same item is saved in different document (e.g., opening stock replacement)
    /// </summary>
    public async Task ReverseStockMovementsForItem(int itemId, string formType, int branchId, TenantDBContext db)
    {
        if (itemId == 0)
            return;

        // Get all stock logs for this item in this form type and branch
        var logs = await db.StockTranLogs
            .Where(x => x.FK_ItemId == itemId && x.VchrFormType == formType && x.FK_BranchId == branchId)
            .ToListAsync();

        if (logs.Count == 0)
            return;

        decimal netReverseQty = logs.Sum(x => (x.OutQuantity ?? 0) - (x.InQuantity ?? 0));
        if (netReverseQty != 0)
        {
            await UpdateStockBalanceInternal(db, itemId, branchId, netReverseQty);
        }

        // Delete the stock logs
        db.StockTranLogs.RemoveRange(logs);
    }

    /// <summary>
    /// Get current stock for an item at a branch, checking local ChangeTracker before DB.
    /// </summary>
    public async Task<decimal> GetCurrentStock(int itemId, int branchId, TenantDBContext db)
    {
        var local = db.ItemStockBalances.Local
            .FirstOrDefault(x => x.FK_ItemId == itemId && x.FK_BranchId == branchId);

        if (local != null)
            return local.CurrentQty;

        var balance = await db.ItemStockBalances
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.FK_ItemId == itemId && x.FK_BranchId == branchId);

        return balance?.CurrentQty ?? 0;
    }

    /// <summary>
    /// Check if sufficient stock is available
    /// </summary>
    public async Task<bool> HasSufficientStock(int itemId, int branchId, decimal requiredQty, TenantDBContext db)
    {
        var currentStock = await GetCurrentStock(itemId, branchId, db);
        return currentStock >= requiredQty;
    }

    /// <summary>
    /// Recalculate ItemStockBalance from StockTranLog
    /// </summary>
    public async Task RecalculateStockBalance(TenantDBContext db, int? itemId = null, int? branchId = null)
    {
        var query = db.StockTranLogs.AsQueryable();
        if (itemId.HasValue)
            query = query.Where(x => x.FK_ItemId == itemId.Value);
        if (branchId.HasValue)
            query = query.Where(x => x.FK_BranchId == branchId.Value);

        var groupedStock = await query
            .GroupBy(x => new { x.FK_ItemId, x.FK_BranchId })
            .Select(g => new
            {
                g.Key.FK_ItemId,
                g.Key.FK_BranchId,
                TotalIn = g.Sum(x => x.InQuantity ?? 0),
                TotalOut = g.Sum(x => x.OutQuantity ?? 0)
            })
            .ToListAsync();

        foreach (var stock in groupedStock)
        {
            if (stock.FK_BranchId == null) continue;
            decimal newQty = stock.TotalIn - stock.TotalOut;

            int rowsUpdated = await db.ItemStockBalances
                .Where(x => x.FK_ItemId == stock.FK_ItemId && x.FK_BranchId == stock.FK_BranchId.Value)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(b => b.CurrentQty, newQty)
                    .SetProperty(b => b.LastUpdated, DateTime.Now));

            if (rowsUpdated == 0)
            {
                db.ItemStockBalances.Add(new ItemStockBalance
                {
                    FK_ItemId = stock.FK_ItemId,
                    FK_BranchId = stock.FK_BranchId.Value,
                    CurrentQty = newQty,
                    LastUpdated = DateTime.Now
                });
                await db.SaveChangesAsync();
            }
        }
    }

    public async Task StockMovementForTransaction(TenantDBContext db, List<TransactionDtls> transactionDtls, TransactionHdr hdr, DateTime vchrDate, bool IsIn = true)
    {
        if (transactionDtls == null || transactionDtls.Count == 0)
            return;

        var store = await db.StoreMasters.FirstOrDefaultAsync();
        int defaultStoreId = store?.StoreId ?? 0;

        foreach (var x in transactionDtls)
        {
            x.FK_StoreId = x.FK_StoreId == 0 ? defaultStoreId : x.FK_StoreId;
            var stockLog = new StockTranLog
            {
                FK_ItemId = x.FK_ItemId,
                VchrId = hdr.HdrId,
                VchrNo = hdr.HdrNo,
                VchrDate = vchrDate,
                VchrType = hdr.VoucherType,
                VchrFormType = hdr.VoucherType,
                AccountId = hdr.PartyId,
                FK_StoreId = x.FK_StoreId,
                InQuantity = IsIn ? x.Quantity * x.QtyPerUnit : 0,
                OutQuantity = IsIn ? 0 : x.Quantity * x.QtyPerUnit,
                FK_UnitId = x.FK_ItemBaseUnit,
                FK_ActualUnitId = x.FK_UnitId,
                ActualUnitQty = x.Quantity,
                InRate = IsIn ? (x.QtyPerUnit == 0 ? 0 : x.ExRate / x.QtyPerUnit) : 0,
                OutRate = IsIn ? 0 : (x.QtyPerUnit == 0 ? 0 : x.ExRate / x.QtyPerUnit),
                ActualRate = x.ExRate,
                Narration = hdr.Remarks,
                FK_UserId = hdr.FK_UserId,
                FK_BranchId = hdr.FK_BranchId,
                TranDate = DateTime.Now
            };

            decimal qtyDelta = x.Quantity * x.QtyPerUnit;
            decimal balanceChange = IsIn ? qtyDelta : -qtyDelta;

            await UpdateStockBalanceInternal(db, x.FK_ItemId, hdr.FK_BranchId, balanceChange);
            await db.StockTranLogs.AddAsync(stockLog);
        }
    }

    public async Task StockReverseForDeliveryNoteInSales(TenantDBContext db, List<TransactionDtls> transactionDtls, TransactionHdr hdr)
    {
        if (transactionDtls == null || transactionDtls.Count == 0)
            return;

        foreach (var item in transactionDtls)
        {
            var stock = await db.StockTranLogs.FirstOrDefaultAsync(x => x.VchrId == hdr.DeliveryNoteId && x.VchrType == "Delivery Note" && x.FK_BranchId == hdr.FK_BranchId && x.FK_ItemId == item.FK_ItemId);
            if (stock != null)
            {
                var stockLog = new StockTranLog
                {
                    FK_ItemId = item.FK_ItemId,
                    VchrId = stock.VchrId,
                    VchrNo = stock.VchrNo,
                    VchrDate = stock.VchrDate,
                    VchrType = "Delivery Note",
                    VchrFormType = "Delivery Note",
                    AccountId = hdr.PartyId,
                    FK_StoreId = hdr.FK_StoreId,
                    InQuantity = stock.OutQuantity,
                    OutQuantity = 0,
                    FK_UnitId = stock.FK_UnitId,
                    FK_ActualUnitId = stock.FK_ActualUnitId,
                    ActualUnitQty = stock.ActualUnitQty,
                    InRate = stock.InRate,
                    OutRate = 0,
                    ActualRate = stock.ActualRate,
                    Narration = $"By Delivery Note Reverse No {stock.VchrNo}",
                    FK_UserId = hdr.FK_UserId,
                    FK_BranchId = hdr.FK_BranchId,
                    TranDate = DateTime.Now
                };
                await db.StockTranLogs.AddAsync(stockLog);

                decimal qty = stock.OutQuantity ?? 0;
                if (qty > 0)
                {
                    await UpdateStockBalanceInternal(db, item.FK_ItemId, hdr.FK_BranchId, qty);
                }
            }
        }
    }
}
