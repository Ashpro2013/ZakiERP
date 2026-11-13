using Microsoft.EntityFrameworkCore;
using ZakiSAASERP.Models.DTO;
using ZakiSAASERP.Repo.Transaction.Inventory;
using ZakiSAASERP.Tests.Common;

namespace ZakiSAASERP.Tests.Inventory;

public class StockMovementTests : IDisposable
{
    private readonly Common.TestDbContextFactoryWrapper _dbWrapper;

    public StockMovementTests()
    {
        _dbWrapper = new Common.TestDbContextFactoryWrapper();
    }

    public void Dispose()
    {
        _dbWrapper.Dispose();
    }

    [Fact]
    public async Task ProcessStockMovement_Inward_CreatesAuditLogAndIncrementsBalance()
    {
        // Arrange
        var db = _dbWrapper.DbContext;
        var service = new StockService();
        int itemId = 10;
        int branchId = 1;

        var movement = new StockMovementDto
        {
            ItemId = itemId,
            BranchId = branchId,
            StoreId = 1,
            VoucherId = 100,
            VoucherNo = "PUR-2026-001",
            VoucherDate = DateTime.Today,
            VoucherType = "PURCHASE",
            FormType = "PurchaseInvoice",
            IsInward = true,
            Quantity = 50m,
            ActualQuantity = 50m,
            Rate = 25m,
            ActualRate = 25m,
            Narration = "Purchase Goods Inward"
        };

        // Act
        await service.ProcessStockMovement(movement, db);
        await db.SaveChangesAsync();

        // Assert: 1. Audit log created
        var log = await db.StockTranLogs.FirstOrDefaultAsync(x => x.VchrNo == "PUR-2026-001");
        Assert.NotNull(log);
        Assert.Equal(itemId, log.FK_ItemId);
        Assert.Equal(50m, log.InQuantity);
        Assert.Equal(0m, log.OutQuantity);
        Assert.Equal(25m, log.ActualRate);

        // Assert: 2. Stock balance created
        decimal stock = await service.GetCurrentStock(itemId, branchId, db);
        Assert.Equal(50m, stock);
    }

    [Fact]
    public async Task ProcessStockMovement_OutwardAfterInward_DecrementsBalanceAtomically()
    {
        // Arrange
        var db = _dbWrapper.DbContext;
        var service = new StockService();
        int itemId = 20;
        int branchId = 1;

        // Step 1: Inward 100 units
        var inward = new StockMovementDto
        {
            ItemId = itemId,
            BranchId = branchId,
            VoucherNo = "PUR-2026-002",
            VoucherDate = DateTime.Today,
            FormType = "PurchaseInvoice",
            IsInward = true,
            Quantity = 100m,
            ActualQuantity = 100m
        };
        await service.ProcessStockMovement(inward, db);
        await db.SaveChangesAsync();

        // Step 2: Outward 35 units via Sales Invoice
        var outward = new StockMovementDto
        {
            ItemId = itemId,
            BranchId = branchId,
            VoucherNo = "SIN-2026-001",
            VoucherDate = DateTime.Today,
            FormType = "SalesInvoice",
            IsInward = false,
            Quantity = 35m,
            ActualQuantity = 35m
        };

        // Act
        await service.ProcessStockMovement(outward, db);
        await db.SaveChangesAsync();

        // Assert
        decimal remainingStock = await service.GetCurrentStock(itemId, branchId, db);
        Assert.Equal(65m, remainingStock); // 100 - 35 = 65

        // Verify both audit logs exist
        var logs = await db.StockTranLogs.Where(x => x.FK_ItemId == itemId).ToListAsync();
        Assert.Equal(2, logs.Count);

        var outLog = logs.First(x => x.VchrNo == "SIN-2026-001");
        Assert.Equal(0m, outLog.InQuantity);
        Assert.Equal(35m, outLog.OutQuantity);
    }

    [Fact]
    public async Task ProcessStockMovements_Batch_ProcessesAllItems()
    {
        // Arrange
        var db = _dbWrapper.DbContext;
        var service = new StockService();
        int branchId = 1;

        var movements = new List<StockMovementDto>
        {
            new StockMovementDto
            {
                ItemId = 31,
                BranchId = branchId,
                VoucherNo = "OP-001",
                FormType = "OpeningStockEntry",
                IsInward = true,
                Quantity = 150m,
                ActualQuantity = 150m
            },
            new StockMovementDto
            {
                ItemId = 32,
                BranchId = branchId,
                VoucherNo = "OP-001",
                FormType = "OpeningStockEntry",
                IsInward = true,
                Quantity = 75m,
                ActualQuantity = 75m
            }
        };

        // Act
        await service.ProcessStockMovements(movements, db);
        await db.SaveChangesAsync();

        // Assert
        decimal stock31 = await service.GetCurrentStock(31, branchId, db);
        decimal stock32 = await service.GetCurrentStock(32, branchId, db);

        Assert.Equal(150m, stock31);
        Assert.Equal(75m, stock32);
        Assert.Equal(2, await db.StockTranLogs.CountAsync());
    }

    [Fact]
    public async Task ReverseStockMovements_OutwardVoucher_RestoresBalanceAndRemovesLogs()
    {
        // Arrange
        var db = _dbWrapper.DbContext;
        var service = new StockService();
        int itemId = 40;
        int branchId = 1;

        // Inward 50
        await service.ProcessStockMovement(new StockMovementDto
        {
            ItemId = itemId,
            BranchId = branchId,
            VoucherNo = "PUR-40",
            FormType = "PurchaseInvoice",
            IsInward = true,
            Quantity = 50m,
            ActualQuantity = 50m
        }, db);
        await db.SaveChangesAsync();

        // Outward 20 via SalesInvoice SIN-99
        await service.ProcessStockMovement(new StockMovementDto
        {
            ItemId = itemId,
            BranchId = branchId,
            VoucherNo = "SIN-99",
            FormType = "SalesInvoice",
            IsInward = false,
            Quantity = 20m,
            ActualQuantity = 20m
        }, db);
        await db.SaveChangesAsync();

        Assert.Equal(30m, await service.GetCurrentStock(itemId, branchId, db));

        // Act: Reverse voucher SIN-99
        await service.ReverseStockMovements("SIN-99", "SalesInvoice", branchId, db);
        await db.SaveChangesAsync();

        // Assert: Stock restored to 50
        decimal restoredStock = await service.GetCurrentStock(itemId, branchId, db);
        Assert.Equal(50m, restoredStock);

        // SIN-99 logs must be removed
        var remainingLogs = await db.StockTranLogs.Where(x => x.VchrNo == "SIN-99").ToListAsync();
        Assert.Empty(remainingLogs);

        // PUR-40 log must remain
        var purchaseLog = await db.StockTranLogs.FirstOrDefaultAsync(x => x.VchrNo == "PUR-40");
        Assert.NotNull(purchaseLog);
    }

    [Fact]
    public async Task ReverseStockMovements_InwardVoucher_DeductsBalanceCorrectly()
    {
        // Arrange
        var db = _dbWrapper.DbContext;
        var service = new StockService();
        int itemId = 50;
        int branchId = 1;

        // Inward 80
        await service.ProcessStockMovement(new StockMovementDto
        {
            ItemId = itemId,
            BranchId = branchId,
            VoucherNo = "PUR-50",
            FormType = "PurchaseInvoice",
            IsInward = true,
            Quantity = 80m,
            ActualQuantity = 80m
        }, db);
        await db.SaveChangesAsync();

        Assert.Equal(80m, await service.GetCurrentStock(itemId, branchId, db));

        // Act: Cancel/Reverse PUR-50
        await service.ReverseStockMovements("PUR-50", "PurchaseInvoice", branchId, db);
        await db.SaveChangesAsync();

        // Assert: Stock is 0 and log is removed
        decimal finalStock = await service.GetCurrentStock(itemId, branchId, db);
        Assert.Equal(0m, finalStock);
        Assert.Empty(await db.StockTranLogs.Where(x => x.VchrNo == "PUR-50").ToListAsync());
    }

    [Fact]
    public async Task ReverseStockMovementsForItem_ReversesOnlySpecifiedItem()
    {
        // Arrange: Multi-item sales invoice with item 61 and item 62
        var db = _dbWrapper.DbContext;
        var service = new StockService();
        int branchId = 1;

        // Set initial stock: 100 for each
        await service.ProcessStockMovement(new StockMovementDto { ItemId = 61, BranchId = branchId, VoucherNo = "INIT", FormType = "Init", IsInward = true, Quantity = 100, ActualQuantity = 100 }, db);
        await service.ProcessStockMovement(new StockMovementDto { ItemId = 62, BranchId = branchId, VoucherNo = "INIT", FormType = "Init", IsInward = true, Quantity = 100, ActualQuantity = 100 }, db);
        await db.SaveChangesAsync();

        // Sale of both items in SIN-MULTI
        await service.ProcessStockMovement(new StockMovementDto { ItemId = 61, BranchId = branchId, VoucherNo = "SIN-MULTI", FormType = "SalesInvoice", IsInward = false, Quantity = 10, ActualQuantity = 10 }, db);
        await service.ProcessStockMovement(new StockMovementDto { ItemId = 62, BranchId = branchId, VoucherNo = "SIN-MULTI", FormType = "SalesInvoice", IsInward = false, Quantity = 25, ActualQuantity = 25 }, db);
        await db.SaveChangesAsync();

        Assert.Equal(90m, await service.GetCurrentStock(61, branchId, db));
        Assert.Equal(75m, await service.GetCurrentStock(62, branchId, db));

        // Act: Reverse only Item 61 from SalesInvoice
        await service.ReverseStockMovementsForItem(61, "SalesInvoice", branchId, db);
        await db.SaveChangesAsync();

        // Assert: Item 61 restored to 100; Item 62 remains 75
        Assert.Equal(100m, await service.GetCurrentStock(61, branchId, db));
        Assert.Equal(75m, await service.GetCurrentStock(62, branchId, db));

        // SIN-MULTI log for Item 61 is removed; Item 62 log still exists
        Assert.Null(await db.StockTranLogs.FirstOrDefaultAsync(x => x.VchrNo == "SIN-MULTI" && x.FK_ItemId == 61));
        Assert.NotNull(await db.StockTranLogs.FirstOrDefaultAsync(x => x.VchrNo == "SIN-MULTI" && x.FK_ItemId == 62));
    }

    [Fact]
    public async Task ProcessStockMovement_InvalidOrZeroQuantity_DoesNotUpdate()
    {
        var db = _dbWrapper.DbContext;
        var service = new StockService();

        // Zero item id
        await service.ProcessStockMovement(new StockMovementDto { ItemId = 0, BranchId = 1, ActualQuantity = 10, IsInward = true }, db);
        // Zero branch id
        await service.ProcessStockMovement(new StockMovementDto { ItemId = 1, BranchId = 0, ActualQuantity = 10, IsInward = true }, db);
        // Null movement
        await service.ProcessStockMovement(null!, db);
        await db.SaveChangesAsync();

        Assert.Equal(0, await db.StockTranLogs.CountAsync());
        Assert.Equal(0, await db.ItemStockBalances.CountAsync());
    }
}
