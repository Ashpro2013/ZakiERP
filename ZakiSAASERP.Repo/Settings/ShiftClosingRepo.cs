using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using ZakiSAASERP.Interface;
using ZakiSAASERP.Interface.Settings;
using ZakiSAASERP.Models.DTO;
using ZakiSAASERP.Models.Settings;

namespace ZakiSAASERP.Repo.Settings;

public class ShiftClosingRepo : IShiftClosingRepo
{
    private readonly IDbContextProvider _contextProvider;
    private static bool _tableChecked = false;
    private static readonly SemaphoreSlim _tableLock = new(1, 1);

    public ShiftClosingRepo(IDbContextProvider contextProvider)
    {
        _contextProvider = contextProvider;
    }

    private async Task EnsureTableCreatedAsync(TenantDBContext db)
    {
        if (_tableChecked) return;
        await _tableLock.WaitAsync();
        try
        {
            if (_tableChecked) return;
            var provider = db.Database.ProviderName ?? string.Empty;

            if (provider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                await db.Database.ExecuteSqlRawAsync(@"
                    CREATE TABLE IF NOT EXISTS ShiftClosingRecords (
                        ShiftClosingRecordId INTEGER PRIMARY KEY AUTOINCREMENT,
                        ShiftCode TEXT NOT NULL,
                        UserName TEXT NOT NULL,
                        Terminal TEXT NOT NULL,
                        FK_BranchId INTEGER NULL,
                        DateFrom TEXT NULL,
                        DateTo TEXT NULL,
                        OpeningBalance REAL NOT NULL,
                        TotalSale REAL NOT NULL,
                        TotalSalesReturn REAL NOT NULL,
                        NetSales REAL NOT NULL,
                        TotalCashSales REAL NOT NULL,
                        TotalCardSales REAL NOT NULL,
                        TotalCreditSales REAL NOT NULL,
                        TotalPayment REAL NOT NULL,
                        TotalCashReceipt REAL NOT NULL,
                        TotalCardReceipt REAL NOT NULL,
                        ExpectedCash REAL NOT NULL,
                        ActualCash REAL NOT NULL,
                        Difference REAL NOT NULL,
                        NewOpeningBalance REAL NOT NULL,
                        Status TEXT NOT NULL,
                        Remarks TEXT NULL,
                        CreatedAt TEXT NOT NULL
                    );");
            }
            else if (provider.Contains("SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                await db.Database.ExecuteSqlRawAsync(@"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ShiftClosingRecords')
                    BEGIN
                        CREATE TABLE ShiftClosingRecords (
                            ShiftClosingRecordId INT IDENTITY(1,1) PRIMARY KEY,
                            ShiftCode NVARCHAR(50) NOT NULL,
                            UserName NVARCHAR(100) NOT NULL,
                            Terminal NVARCHAR(100) NOT NULL,
                            FK_BranchId INT NULL,
                            DateFrom DATETIME2 NULL,
                            DateTo DATETIME2 NULL,
                            OpeningBalance DECIMAL(18,2) NOT NULL,
                            TotalSale DECIMAL(18,2) NOT NULL,
                            TotalSalesReturn DECIMAL(18,2) NOT NULL,
                            NetSales DECIMAL(18,2) NOT NULL,
                            TotalCashSales DECIMAL(18,2) NOT NULL,
                            TotalCardSales DECIMAL(18,2) NOT NULL,
                            TotalCreditSales DECIMAL(18,2) NOT NULL,
                            TotalPayment DECIMAL(18,2) NOT NULL,
                            TotalCashReceipt DECIMAL(18,2) NOT NULL,
                            TotalCardReceipt DECIMAL(18,2) NOT NULL,
                            ExpectedCash DECIMAL(18,2) NOT NULL,
                            ActualCash DECIMAL(18,2) NOT NULL,
                            Difference DECIMAL(18,2) NOT NULL,
                            NewOpeningBalance DECIMAL(18,2) NOT NULL,
                            Status NVARCHAR(50) NOT NULL,
                            Remarks NVARCHAR(500) NULL,
                            CreatedAt DATETIME2 NOT NULL
                        );
                    END");
            }
            else if (provider.Contains("MySql", StringComparison.OrdinalIgnoreCase))
            {
                await db.Database.ExecuteSqlRawAsync(@"
                    CREATE TABLE IF NOT EXISTS ShiftClosingRecords (
                        ShiftClosingRecordId INT AUTO_INCREMENT PRIMARY KEY,
                        ShiftCode VARCHAR(50) NOT NULL,
                        UserName VARCHAR(100) NOT NULL,
                        Terminal VARCHAR(100) NOT NULL,
                        FK_BranchId INT NULL,
                        DateFrom DATETIME NULL,
                        DateTo DATETIME NULL,
                        OpeningBalance DECIMAL(18,2) NOT NULL,
                        TotalSale DECIMAL(18,2) NOT NULL,
                        TotalSalesReturn DECIMAL(18,2) NOT NULL,
                        NetSales DECIMAL(18,2) NOT NULL,
                        TotalCashSales DECIMAL(18,2) NOT NULL,
                        TotalCardSales DECIMAL(18,2) NOT NULL,
                        TotalCreditSales DECIMAL(18,2) NOT NULL,
                        TotalPayment DECIMAL(18,2) NOT NULL,
                        TotalCashReceipt DECIMAL(18,2) NOT NULL,
                        TotalCardReceipt DECIMAL(18,2) NOT NULL,
                        ExpectedCash DECIMAL(18,2) NOT NULL,
                        ActualCash DECIMAL(18,2) NOT NULL,
                        Difference DECIMAL(18,2) NOT NULL,
                        NewOpeningBalance DECIMAL(18,2) NOT NULL,
                        Status VARCHAR(50) NOT NULL,
                        Remarks VARCHAR(500) NULL,
                        CreatedAt DATETIME NOT NULL
                    );");
            }
            else if (provider.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) || provider.Contains("Postgre", StringComparison.OrdinalIgnoreCase))
            {
                await db.Database.ExecuteSqlRawAsync(@"
                    CREATE TABLE IF NOT EXISTS ""ShiftClosingRecords"" (
                        ""ShiftClosingRecordId"" SERIAL PRIMARY KEY,
                        ""ShiftCode"" VARCHAR(50) NOT NULL,
                        ""UserName"" VARCHAR(100) NOT NULL,
                        ""Terminal"" VARCHAR(100) NOT NULL,
                        ""FK_BranchId"" INT NULL,
                        ""DateFrom"" TIMESTAMP NULL,
                        ""DateTo"" TIMESTAMP NULL,
                        ""OpeningBalance"" NUMERIC(18,2) NOT NULL,
                        ""TotalSale"" NUMERIC(18,2) NOT NULL,
                        ""TotalSalesReturn"" NUMERIC(18,2) NOT NULL,
                        ""NetSales"" NUMERIC(18,2) NOT NULL,
                        ""TotalCashSales"" NUMERIC(18,2) NOT NULL,
                        ""TotalCardSales"" NUMERIC(18,2) NOT NULL,
                        ""TotalCreditSales"" NUMERIC(18,2) NOT NULL,
                        ""TotalPayment"" NUMERIC(18,2) NOT NULL,
                        ""TotalCashReceipt"" NUMERIC(18,2) NOT NULL,
                        ""TotalCardReceipt"" NUMERIC(18,2) NOT NULL,
                        ""ExpectedCash"" NUMERIC(18,2) NOT NULL,
                        ""ActualCash"" NUMERIC(18,2) NOT NULL,
                        ""Difference"" NUMERIC(18,2) NOT NULL,
                        ""NewOpeningBalance"" NUMERIC(18,2) NOT NULL,
                        ""Status"" VARCHAR(50) NOT NULL,
                        ""Remarks"" VARCHAR(500) NULL,
                        ""CreatedAt"" TIMESTAMP NOT NULL
                    );");
            }
            else
            {
                var creator = db.Database.GetService<IRelationalDatabaseCreator>();
                try { creator.CreateTables(); } catch { }
            }

            _tableChecked = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ShiftClosingRepo] EnsureTableCreatedAsync warning: {ex.Message}");
        }
        finally
        {
            _tableLock.Release();
        }
    }

    public async Task<OutputResponse> SaveShiftClosingAsync(ShiftClosingRecord record)
    {
        try
        {
            await using var db = _contextProvider.CreateDbContext();
            await EnsureTableCreatedAsync(db);

            if (string.IsNullOrWhiteSpace(record.ShiftCode))
            {
                record.ShiftCode = "SC-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
            }

            record.NetSales = record.TotalSale - record.TotalSalesReturn;
            record.ExpectedCash = record.OpeningBalance + record.TotalCashSales + record.TotalCashReceipt - record.TotalPayment;
            record.Difference = record.ActualCash - record.ExpectedCash;
            record.Status = record.Difference == 0 ? "Balanced" : record.Difference > 0 ? "Overage" : "Shortage";
            if (record.DateTo == null) record.DateTo = DateTime.Now;
            if (record.CreatedAt == default) record.CreatedAt = DateTime.Now;

            db.ShiftClosingRecords.Add(record);
            await db.SaveChangesAsync();

            return new OutputResponse(record.ShiftClosingRecordId, $"Shift {record.ShiftCode} closing details saved successfully.", record, true);
        }
        catch (Exception ex)
        {
            return new OutputResponse(0, $"Error saving shift closing: {ex.Message}", null, false);
        }
    }

    public async Task<ShiftClosingReportResponse> GetShiftClosingReportAsync(ShiftClosingReportRequest req)
    {
        try
        {
            await using var db = _contextProvider.CreateDbContext();
            await EnsureTableCreatedAsync(db);

            var from = req.FromDate.Date;
            var to = req.ToDate.Date.AddDays(1).AddTicks(-1);

            var query = db.ShiftClosingRecords
                .Where(x => (x.DateTo >= from && x.DateTo <= to) || (x.DateTo == null && x.CreatedAt >= from && x.CreatedAt <= to))
                .AsQueryable();

            if (req.BranchId > 0)
            {
                query = query.Where(x => x.FK_BranchId == req.BranchId);
            }

            if (!string.IsNullOrWhiteSpace(req.Terminal) && !string.Equals(req.Terminal, "All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(x => x.Terminal.Contains(req.Terminal));
            }

            if (!string.IsNullOrWhiteSpace(req.UserName) && !string.Equals(req.UserName, "All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(x => x.UserName.Contains(req.UserName));
            }

            var list = await query
                .OrderByDescending(x => x.DateTo ?? x.CreatedAt)
                .ThenByDescending(x => x.ShiftClosingRecordId)
                .ToListAsync();

            var totalShifts = list.Count;
            var totalNetSales = list.Sum(x => x.NetSales);
            var totalCashSales = list.Sum(x => x.TotalCashSales);
            var totalCardSales = list.Sum(x => x.TotalCardSales);
            var totalCreditSales = list.Sum(x => x.TotalCreditSales);
            var totalReturns = list.Sum(x => x.TotalSalesReturn);
            var totalActualCash = list.Sum(x => x.ActualCash);
            var totalExpectedCash = list.Sum(x => x.ExpectedCash);
            var totalDifference = list.Sum(x => x.Difference);

            return new ShiftClosingReportResponse(
                list,
                totalShifts,
                totalNetSales,
                totalCashSales,
                totalCardSales,
                totalCreditSales,
                totalReturns,
                totalActualCash,
                totalExpectedCash,
                totalDifference
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ShiftClosingRepo] GetShiftClosingReportAsync error: {ex.Message}");
            return new ShiftClosingReportResponse(new List<ShiftClosingRecord>(), 0, 0, 0, 0, 0, 0, 0, 0, 0);
        }
    }

    public async Task<ShiftClosingRecord?> GetShiftClosingByIdAsync(int id)
    {
        try
        {
            await using var db = _contextProvider.CreateDbContext();
            await EnsureTableCreatedAsync(db);
            return await db.ShiftClosingRecords.FirstOrDefaultAsync(x => x.ShiftClosingRecordId == id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ShiftClosingRepo] GetShiftClosingByIdAsync error: {ex.Message}");
            return null;
        }
    }
}
