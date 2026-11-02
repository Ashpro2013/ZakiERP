using ZakiSAASERP.Interface;
using ZakiSAASERP.Interface.Others;
using ZakiSAASERP.Shared.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ZakiSAASERP.Desk.Services;

public class DatabaseMigrationServiceDesk(
    IServiceProvider serviceProvider,
    IConProviderMaui conProvider) : IDatabaseMigrationService
{
    public async Task<(bool Success, string Message)> MigrateAndSeedAsync(
        string provider,
        string connectionString,
        Action<string>? statusCallback = null)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            return (false, "Please select a database provider.");
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return (false, "Please enter a connection string.");
        }

        try
        {
            statusCallback?.Invoke("Connecting to database server...");

            var options = AppUtility.CreateDbContextOptions(provider, connectionString);
            await using var db = new TenantDBContext(options);

            // 5 minute timeout for creating DB / running all migrations
            db.Database.SetCommandTimeout(300);

            statusCallback?.Invoke("Checking database schema and applying migrations...");
            await db.Database.MigrateAsync();

            statusCallback?.Invoke("Database schema updated. Verifying initial data...");

            var hasBranches = await db.BranchMasters.AnyAsync();
            if (!hasBranches)
            {
                statusCallback?.Invoke("Seeding new database with default company and admin user...");
                using var scope = serviceProvider.CreateScope();
                var seedService = provider switch
                {
                    "SQLite" => (IDatabaseSeedService)scope.ServiceProvider.GetRequiredService<ZakiSAASERP.SQlite.DatabaseSeedService>(),
                    "PSQL" => (IDatabaseSeedService)scope.ServiceProvider.GetRequiredService<ZakiSAASERP.PSQL.DatabaseSeedService>(),
                    "MySQL" => (IDatabaseSeedService)scope.ServiceProvider.GetRequiredService<ZakiSAASERP.MySQL.DatabaseSeedService>(),
                    "Oracle" => (IDatabaseSeedService)scope.ServiceProvider.GetRequiredService<ZakiSAASERP.Oracle.DatabaseSeedService>(),
                    _ => (IDatabaseSeedService)scope.ServiceProvider.GetRequiredService<ZakiSAASERP.MSSQL.DatabaseSeedService>()
                };

                await seedService.Seed(db, null);
            }

            // Ensure essential menu items and visibilities exist
            EnsureMenuDetails(db);

            // Successfully applied! Update active memory state & Windows Registry
            AppData.DBProvider = provider;
            AppData.ConnectionString = connectionString;
            conProvider.SetConnectionString(connectionString);
            RegistryHelper.SetData("DBProvider", provider);
            RegistryHelper.SetData("ConString", connectionString);

            statusCallback?.Invoke("Database ready.");
            return (true, "Database updated and connected successfully.");
        }
        catch (Exception ex)
        {
            try
            {
                string crashPath = OperatingSystem.IsWindows()
                    ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AshproSAAS", "crash.txt")
                    : Path.Combine(FileSystem.AppDataDirectory, "crash.txt");
                string? directory = Path.GetDirectoryName(crashPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                File.WriteAllText(crashPath, ex.ToString());
            }
            catch
            {
                // Ignore secondary logging errors
            }

            return (false, "Database connection or migration failed: " + ex.Message);
        }
    }

    private static void EnsureMenuDetails(TenantDBContext db)
    {
        try
        {
            ZakiSAASERP.Interface.Others.MenuSeedHelper.EnsureSeedDataAsync(db).GetAwaiter().GetResult();

            var settingsMenu = db.MenuDetails.FirstOrDefault(m => m.Name == "Settings");
            if (settingsMenu != null && !db.MenuDetails.Any(m => m.Name == "ProductImport"))
            {
                var importMenu = new ZakiSAASERP.Models.User.MenuDetails
                {
                    Name = "ProductImport",
                    MenuCaption = "Product Import",
                    ArabicCaption = "استيراد المنتجات",
                    ParentId = settingsMenu.Id,
                    AssemblyName = "ZakiSAASERP.Shared.Pages.Settings.ProductImportView",
                    ClassName = "ProductImportView",
                    Position = 8,
                    Active = true
                };
                db.MenuDetails.Add(importMenu);
                db.SaveChanges();

                if (!db.MenuVisibilities.Any(v => v.MenuDetailId == importMenu.Id))
                {
                    db.MenuVisibilities.Add(new ZakiSAASERP.Models.Settings.MenuVisibility
                    {
                        MenuDetailId = importMenu.Id,
                        IsVisible = true,
                        LastModified = DateTime.Now
                    });
                    db.SaveChanges();
                }

                if (db.ERPTypePresets.Any())
                {
                    var erpTypeIds = db.ERPTypes.Select(t => t.Id).ToList();
                    foreach (var typeId in erpTypeIds)
                    {
                        if (!db.ERPTypePresets.Any(p => p.ERPTypeId == typeId && p.MenuDetailId == importMenu.Id))
                        {
                            db.ERPTypePresets.Add(new ZakiSAASERP.Models.Settings.ERPTypePreset
                            {
                                ERPTypeId = typeId,
                                MenuDetailId = importMenu.Id,
                                IsVisible = true
                            });
                        }
                    }
                    db.SaveChanges();
                }
            }

            if (settingsMenu != null && !db.MenuDetails.Any(m => m.Name == "ProductExport"))
            {
                var exportMenu = new ZakiSAASERP.Models.User.MenuDetails
                {
                    Name = "ProductExport",
                    MenuCaption = "Product Export",
                    ArabicCaption = "تصدير المنتجات",
                    ParentId = settingsMenu.Id,
                    AssemblyName = "ZakiSAASERP.Shared.Pages.Settings.ProductExportView",
                    ClassName = "ProductExportView",
                    Position = 9,
                    Active = true
                };
                db.MenuDetails.Add(exportMenu);
                db.SaveChanges();

                if (!db.MenuVisibilities.Any(v => v.MenuDetailId == exportMenu.Id))
                {
                    db.MenuVisibilities.Add(new ZakiSAASERP.Models.Settings.MenuVisibility
                    {
                        MenuDetailId = exportMenu.Id,
                        IsVisible = true,
                        LastModified = DateTime.Now
                    });
                    db.SaveChanges();
                }

                if (db.ERPTypePresets.Any())
                {
                    var erpTypeIds = db.ERPTypes.Select(t => t.Id).ToList();
                    foreach (var typeId in erpTypeIds)
                    {
                        if (!db.ERPTypePresets.Any(p => p.ERPTypeId == typeId && p.MenuDetailId == exportMenu.Id))
                        {
                            db.ERPTypePresets.Add(new ZakiSAASERP.Models.Settings.ERPTypePreset
                            {
                                ERPTypeId = typeId,
                                MenuDetailId = exportMenu.Id,
                                IsVisible = true
                            });
                        }
                    }
                    db.SaveChanges();
                }
            }

            if (settingsMenu != null && !db.MenuDetails.Any(m => m.Name == "PrintingBarcode"))
            {
                var barcodeMenu = new ZakiSAASERP.Models.User.MenuDetails
                {
                    Name = "PrintingBarcode",
                    MenuCaption = "Printing Barcode",
                    ArabicCaption = "طباعة الباركود",
                    ParentId = settingsMenu.Id,
                    AssemblyName = "ZakiSAASERP.Shared.Pages.Settings.PrintingBarcodeView",
                    ClassName = "PrintingBarcodeView",
                    Position = 10,
                    Active = true
                };
                db.MenuDetails.Add(barcodeMenu);
                db.SaveChanges();

                if (!db.MenuVisibilities.Any(v => v.MenuDetailId == barcodeMenu.Id))
                {
                    db.MenuVisibilities.Add(new ZakiSAASERP.Models.Settings.MenuVisibility
                    {
                        MenuDetailId = barcodeMenu.Id,
                        IsVisible = true,
                        LastModified = DateTime.Now
                    });
                    db.SaveChanges();
                }

                if (db.ERPTypePresets.Any())
                {
                    var erpTypeIds = db.ERPTypes.Select(t => t.Id).ToList();
                    foreach (var typeId in erpTypeIds)
                    {
                        if (!db.ERPTypePresets.Any(p => p.ERPTypeId == typeId && p.MenuDetailId == barcodeMenu.Id))
                        {
                            db.ERPTypePresets.Add(new ZakiSAASERP.Models.Settings.ERPTypePreset
                            {
                                ERPTypeId = typeId,
                                MenuDetailId = barcodeMenu.Id,
                                IsVisible = true
                            });
                        }
                    }
                    db.SaveChanges();
                }
            }
        }
        catch
        {
            // Non-fatal if menu sync fails on specific schema
        }
    }
}
