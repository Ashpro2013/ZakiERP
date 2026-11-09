using ZakiSAASERP.Interface;
using ZakiSAASERP.Interface.Others;
using ZakiSAASERP.Models.Company;
using ZakiSAASERP.Models.Masters;
using ZakiSAASERP.Models.User;
using Microsoft.EntityFrameworkCore;

namespace ZakiSAASERP.PSQL;

public class DatabaseSeedService(IDbContextProvider contextProvider) : IDatabaseSeedService
{
    public async Task Seed(TenantDBContext? context = null, ExLCompany? exlcompany = null)
    {
        context ??= contextProvider.CreateDbContext();
        exlcompany ??= new ExLCompany
        {
            CompanyName = "Default Company",
            UserName = "admin",
            PassWord = "123",
            ContactNo = "1234567890",
            Email = "admin@company.com",
            SubscriptionStartedAt = DateTime.Now,
            SubscriptionEndingDate = DateTime.Now.AddDays(7),
            CompanyCode = "1001",
            SubscriptionType = "Standard"
        };
        await using (context)
        {
            try
            {
                if (!await context.BranchMasters.AnyAsync())
                {
                    var branch = new BranchMaster { Name = "Main Branch", Code = "Main", ShowInLogin = true };
                    context.BranchMasters.Add(branch);
                    await context.SaveChangesAsync();
                    int iBranchId = branch.BranchId;
                    var user = new UserDetails
                    {
                        UserName = exlcompany.UserName,
                        PassWord = exlcompany.PassWord,
                        FK_BranchId = iBranchId,
                        PrevilageId = 1
                    };

                    context.UserDetails.Add(user);
                    var companyDtls = new CompanyDetails
                    {
                        Name = exlcompany.CompanyName,
                        Contact = exlcompany.ContactNo,
                        Email = exlcompany.Email,
                    };
                    context.CompanyDetails.Add(companyDtls);
                    var fyear = new FinancialYear
                    {
                        FromDate = exlcompany.SubscriptionStartedAt,
                        ToDate = exlcompany.SubscriptionStartedAt,
                        IsClosed = false,
                        Name = exlcompany.SubscriptionStartedAt.Value.Year.ToString(),
                        RefNo = 1
                    };
                    context.FinancialYears.Add(fyear);
                    context.UnitMasters.Add(new UnitMaster { Name = "PCs", Code = "PCs", Remarks = "Default Unit" });
                    context.CategoryMasters.Add(new CategoryMaster { Name = "General", Code = "Gen", Remarks = "Default Category" });
                    context.SubCategoryMasters.Add(new SubCategoryMaster { Name = "General", Code = "Gen", Remarks = "Default SubCategory", FK_CategoryId = 1 });
                    context.SalesManMasters.Add(new SalesManMaster { Name = "SM", Code = "SM", Remarks = "Default Salesman", Designation = "Salesman" });
                    context.StoreMasters.Add(new StoreMaster { Name = "Main Store", Code = "Main", IsDefaultStore = true, FK_BranchId = iBranchId });
                    context.CountryMasters.Add(new CountryMaster { Name = "Saudi Arabia", Code = "SA" });
                    context.VatMasters.Add(new VatMaster { Name = "VAT 15%", Code = "VAT15", Percentage = 15, IsDefault = true });
                    context.CurrencyMasters.Add(new CurrencyMaster { Name = "Saudi Riyal", Code = "SAR", Rate = 1 });
                    context.CustomerGroupMasters.Add(new CustomerGroupMaster { Name = "General", Code = "Gen", Remarks = "Default Customer Group" });
                    context.CustomerLocationMasters.Add(new CustomerLocationMaster { Name = "General", Code = "Gen", Remarks = "Default Customer Location" });
                    context.CustomerMasters.Add(new CustomerMaster { Code = "C1001", Name = "Cash Customer", NameArabic = "عميل نقدي", FK_CustomerGroupId = 1, FK_CountryId = 1, IsSupplier = false, CusBillType = "Cash", IsActive = true });
                    context.SupplierMasters.Add(new SupplierMaster { Code = "S1001", Name = "Cash Supplier", NameArabic = "مورد نقدي", FK_CountryId = 1, IsCustomer = false, IsActive = true });
                    context.PurchaserMasters.Add(new PurchaserMaster { Code = "P1001", Name = "Purchaser", NameArabic = "مشتري", CreditLimit = 0, Remarks = "Default Purchaser" });
                    context.ProjectMasters.Add(new ProjectMaster { Code = "PRJ1001", Name = "General Project", Description = "Default Project", Status = "Active" });

                    await context.SaveChangesAsync();
                }

                if (!await context.MenuDetails.AnyAsync())
                {
                    try
                    {
                        var sql = GetMenuSeedSql();
                        await context.Database.ExecuteSqlRawAsync(sql);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }

                if (!await context.PermanentSettings.AnyAsync())
                {
                    try
                    {
                        var settingssql = GetSettingsSeed();
                        await context.Database.ExecuteSqlRawAsync(settingssql);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }

                if (await context.MenuDetails.AnyAsync() && !await context.MenuVisibilities.AnyAsync())
                {
                    try
                    {
                        var menuvisibiltysql = GetMenuVisibilitySeedSql();
                        await context.Database.ExecuteSqlRawAsync(menuvisibiltysql);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }

                // Ensure any missing menus (e.g. PrintingBarcode), visibilities, presets, and settings are seeded for existing databases
                await MenuSeedHelper.EnsureSeedDataAsync(context);
                try
                {
                    var triggersql = GetTriggerSQL();
                    await context.Database.ExecuteSqlRawAsync(triggersql);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                try
                {
                    var viewsql = GetViewSQL();
                    await context.Database.ExecuteSqlRawAsync(viewsql);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                try
                {
                    var erptypesql = GetErpTypeSQL();
                    await context.Database.ExecuteSqlRawAsync(erptypesql);
                }
                catch (Exception)
                {

                    throw;
                }
                try
                {
                    var acctypesql = GetAccountTypeSQL();
                    await context.Database.ExecuteSqlRawAsync(acctypesql);
                }
                catch (Exception)
                {

                    throw;
                }
                try
                {
                    var acchdtypesql = GetAccHdSQL();
                    await context.Database.ExecuteSqlRawAsync(acchdtypesql);
                }
                catch (Exception)
                {

                    throw;
                }
                try
                {
                    var accgrpsql = GetAccGrpSql();
                    await context.Database.ExecuteSqlRawAsync(accgrpsql);
                }
                catch (Exception)
                {

                    throw;
                }
                try
                {
                    var accsubgrpsql = GetAccSubGrpSql();
                    await context.Database.ExecuteSqlRawAsync(accsubgrpsql);
                }
                catch (Exception)
                {

                    throw;
                }
                try
                {
                    var accmstrsql = GetAcMstSql();
                    await context.Database.ExecuteSqlRawAsync(accmstrsql);
                }
                catch (Exception)
                {

                    throw;
                } // Seed logic here
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding database: {ex.Message}");// Handle exceptions
            }
        }
    }
    private string GetAcMstSql()
    {
        return @"INSERT INTO ""AccountMaster""
(""AccountId"",""Code"",""Name"",""NameArabic"",""FK_AccountHeadId"",""FK_AcGroupId"",
 ""Opening"",""DbCr"",""FK_AcTypeId"",""FK_AccountSubGroupId"",""Flag"",""IsReserved"")
SELECT * FROM (
VALUES
(1,'110001010001','Cash in Hand','نقد في اليد',1,1,0,'Debit',1,1,'Acc',TRUE),
(2,'110001020001','Bank Account 1','حساب بنكي 1',1,1,0,'Debit',1,2,'Acc',TRUE),
(3,'110001020002','Bank Account 2','حساب بنكي 2',1,1,0,'Debit',1,2,'Acc',TRUE),
(4,'110002010001','Customer – Local','عميل - محلي',1,2,0,'Debit',1,3,'Acc',TRUE),
(5,'120001010001','Office Building','مبنى المكاتب',2,3,0,'Debit',1,4,'Acc',TRUE),
(6,'120001020001','Delivery Vehicle','سيارة توصيل',2,3,0,'Debit',1,5,'Acc',TRUE),
(7,'120001030001','Office Equipment','معدات مكتبية',2,3,0,'Debit',1,6,'Acc',TRUE),
(8,'210001010001','Supplier – Local','مورد - محلي',3,4,0,'Credit',2,7,'Acc',TRUE),
(9,'210002010001','VAT Output @15%','ضريبة المخرجات @15%',3,5,0,'Credit',2,8,'Acc',TRUE),
(10,'210002020001','VAT Payable to ZATCA','ضريبة مستحقة للزكاة',3,5,0,'Credit',2,9,'Acc',TRUE),
(11,'220001010001','Long-Term Loan','قرض طويل الأجل',4,6,0,'Credit',2,10,'Acc',TRUE),
(12,'310001010001','Sales','المبيعات',5,7,0,'Credit',3,11,'Acc',TRUE),
(13,'310002010001','Discount Received','خصم مكتسب',5,8,0,'Credit',3,12,'Acc',TRUE),
(14,'310002020001','Interest Income','دخل الفوائد',5,8,0,'Credit',3,13,'Acc',TRUE),
(15,'410001010001','Office Rent','إيجار المكتب',6,9,0,'Debit',4,14,'Acc',TRUE),
(16,'410001020001','Staff Salary','رواتب الموظفين',6,9,0,'Debit',4,15,'Acc',TRUE),
(17,'410002010001','Electricity Expense','مصاريف الكهرباء',6,10,0,'Debit',4,16,'Acc',TRUE),
(18,'410002020001','Water Expense','مصاريف المياه',6,10,0,'Debit',4,17,'Acc',TRUE),
(19,'410003010001','Delivery Expense','مصاريف التوصيل',6,11,0,'Debit',4,18,'Acc',TRUE),
(20,'410004010001','Depreciation – Vehicle','إهلاك - مركبة',6,12,0,'Debit',4,19,'Acc',TRUE),
(21,'410005010001','Bank Charges','رسوم بنكية',6,13,0,'Debit',4,20,'Acc',TRUE),
(22,'330001010001','Opening Balance Equity','رصيد الافتتاحي',7,14,0,'Credit',2,21,'Acc',TRUE),
(23,'420001010001','Purchase Account','حساب المشتريات',8,15,0,'Debit',4,22,'Acc',TRUE),
(24,'110003010001','VAT Input @15%','ضريبة المدخلات @15%',1,16,0,'Debit',1,23,'Acc',TRUE),
(25,'110004010001','Inventory Asset','أصل المخزون',1,17,0,'Debit',1,24,'Acc',TRUE)
) AS v(""AccountId"",""Code"",""Name"",""NameArabic"",""FK_AccountHeadId"",""FK_AcGroupId"",
       ""Opening"",""DbCr"",""FK_AcTypeId"",""FK_AccountSubGroupId"",""Flag"",""IsReserved"")
WHERE NOT EXISTS (
    SELECT 1 
    FROM ""AccountMaster"" e 
    WHERE e.""AccountId"" = v.""AccountId""
);";
    }
    private string GetAccSubGrpSql()
    {
        return @"INSERT INTO ""AccountSubGroup""
(""SubGroupId"",""FK_AcGroupId"",""Code"",""Name"",""NameArabic"",""Remarks"",""FK_AcHeadId"",""IsReserved"")
SELECT * FROM (
VALUES
(1,1,'11000101','Cash','نقد',NULL,1,TRUE),
(2,1,'11000102','Bank','بنك',NULL,1,TRUE),
(3,2,'11000201','Sundry Debtors','مدينون متنوعون',NULL,1,TRUE),
(4,3,'12000101','Building','مبنى',NULL,2,TRUE),
(5,3,'12000102','Vehicles','مركبات',NULL,2,TRUE),
(6,3,'12000103','Equipment','معدات',NULL,2,TRUE),
(7,4,'21000101','Sundry Creditors','دائنون متنوعون',NULL,3,TRUE),
(8,4,'21000201','VAT Output','مخرجات الضريبة',NULL,3,TRUE),
(9,4,'21000202','VAT Payable','ضريبة مستحقة',NULL,3,TRUE),
(10,5,'22000101','Bank Loan','قرض بنكي',NULL,4,TRUE),
(11,6,'31000101','Local Sales','مبيعات محلية',NULL,5,TRUE),
(12,6,'31000201','Discount','خصم',NULL,5,TRUE),
(13,6,'31000202','Interest','فائدة',NULL,5,TRUE),
(14,7,'41000101','Rent','إيجار',NULL,6,TRUE),
(15,7,'41000102','Salary','راتب',NULL,6,TRUE),
(16,8,'41000201','Electricity','كهرباء',NULL,6,TRUE),
(17,8,'41000202','Water','ماء',NULL,6,TRUE),
(18,9,'41000301','Transport','نقل',NULL,6,TRUE),
(19,10,'41000401','Vehicles','مركبات',NULL,6,TRUE),
(20,11,'41000501','Bank Charges','رسوم بنكية',NULL,6,TRUE),
(21,12,'33000101','Owners Equity','حقوق الملكية',NULL,7,TRUE),
(22,13,'42000101','Local Purchases','مشتريات محلية',NULL,8,TRUE),
(23,14,'11000301','VAT Input','مدخلات الضريبة',NULL,1,TRUE),
(24,15,'11000401','Stock','مخزون',NULL,1,TRUE)
) AS v(""SubGroupId"",""FK_AcGroupId"",""Code"",""Name"",""NameArabic"",""Remarks"",""FK_AcHeadId"",""IsReserved"")
WHERE NOT EXISTS (
    SELECT 1 
    FROM ""AccountSubGroup"" e 
    WHERE e.""SubGroupId"" = v.""SubGroupId""
);";
    }
    private string GetAccGrpSql()
    {
        return @"INSERT INTO ""AccountGroup""
(""AccountGroupId"",""Code"",""Name"",""NameArabic"",""Remarks"",""FK_AcHeadId"",""IsReserved"")
SELECT * FROM (
VALUES
(1,'110001','Cash & Bank','النقد والبنك',NULL,1,TRUE),
(2,'110002','Accounts Receivable','حسابات القبض',NULL,1,TRUE),
(3,'120001','Property Plant & Equip','الممتلكات والمنشآت والمعدات',NULL,2,TRUE),
(4,'210001','Accounts Payable','حسابات الدفع',NULL,3,TRUE),
(5,'210002','VAT Payable','ضريبة القيمة المضافة المستحقة',NULL,3,TRUE),
(6,'220001','Loans','قروض',NULL,4,TRUE),
(7,'310001','Sales','المبيعات',NULL,5,TRUE),
(8,'310002','Other Income','إيرادات أخرى',NULL,5,TRUE),
(9,'410001','Administrative','إداري',NULL,6,TRUE),
(10,'410002','Utilities','خدمات',NULL,6,TRUE),
(11,'410003','Selling Expenses','مصاريف البيع',NULL,6,TRUE),
(12,'410004','Depreciation','إهلاك',NULL,6,TRUE),
(13,'410005','Financial Expenses','مصاريف مالية',NULL,6,TRUE),
(14,'330001','Capital Accounts','حسابات رأس المال',NULL,7,TRUE),
(15,'420001','Purchases','المشتريات',NULL,8,TRUE),
(16,'110003','VAT Receivable','ضريبة القيمة المضافة المستحقة القبض',NULL,1,TRUE),
(17,'110004','Inventory Assets','أصول المخزون',NULL,1,TRUE)
) AS v(""AccountGroupId"",""Code"",""Name"",""NameArabic"",""Remarks"",""FK_AcHeadId"",""IsReserved"")
WHERE NOT EXISTS (
    SELECT 1 
    FROM ""AccountGroup"" e 
    WHERE e.""AccountGroupId"" = v.""AccountGroupId""
);";
    }
    private string GetAccHdSQL()
    {
        return @"
INSERT INTO ""AccountHeadMaster""
(""AccountHeadId"",""Code"",""Name"",""NameArabic"",""FK_AcTypeId"",""IsReserved"")
SELECT * FROM (
VALUES
(1,'1100','Current Assets',N'الأصول المتداولة',1,TRUE),
(2,'1200','Fixed Assets',N'الأصول الثابتة',1,TRUE),
(3,'2100','Current Liabilities',N'الالتزامات المتداولة',2,TRUE),
(4,'2200','Non-Current Liabilities',N'الالتزامات غير المتداولة',2,TRUE),
(5,'3100','Revenue',N'إيرادات',3,TRUE),
(6,'4100','Operating Expenses',N'مصاريف تشغيلية',4,TRUE),
(7,'3300','Equity',N'حقوق الملكية',2,TRUE),
(8,'4200','Cost of Sales',N'تكلفة المبيعات',4,TRUE)
) AS v(""AccountHeadId"",""Code"",""Name"",""NameArabic"",""FK_AcTypeId"",""IsReserved"")
WHERE NOT EXISTS (
    SELECT 1 
    FROM ""AccountHeadMaster"" e 
    WHERE e.""AccountHeadId"" = v.""AccountHeadId""
);";
    }
    private string GetAccountTypeSQL()
    {
        return @"INSERT INTO ""AccountType""
(""AcTypeId"",""AcTypeName"")
SELECT * FROM (
VALUES
(1, 'Asset'),
(2, 'Liability'),
(3, 'Income'),
(4, 'Expense')
) AS v(""AcTypeId"",""AcTypeName"")
WHERE NOT EXISTS (
    SELECT 1 
    FROM ""AccountType"" e 
    WHERE e.""AcTypeId"" = v.""AcTypeId""
);";
    }
    private string GetErpTypeSQL()
    {
        return @" 
INSERT INTO ""ERPType""
(""TypeName"",""Description"",""IsCustom"",""CreatedDate"")
SELECT * FROM (
VALUES
('Trading','Sales-focused business with inventory and customer management',FALSE,NOW()),
('Manufacturing','Production-based business with full procurement, manufacturing and sales',FALSE,NOW()),
('Transportation','Logistics and delivery services with minimal inventory',FALSE,NOW()),
('Retail','Point-of-sale and customer-focused retail operations',FALSE,NOW()),
('Services','Service-based business with project management and minimal inventory',FALSE,NOW()),
('Custom','Fully customizable menu configuration - select specific menus to show',TRUE,NOW())
) AS v(""TypeName"",""Description"",""IsCustom"",""CreatedDate"")
WHERE NOT EXISTS (
    SELECT 1 FROM ""ERPType"" e WHERE e.""TypeName"" = v.""TypeName""
);
";
    }
    private string GetSettingsSeed()
    {
        return @"
INSERT INTO ""PermanentSettings"" (""SettingKey"",""SettingValue"")
VALUES
    ('DefaultCashAccount',null),
    ('DefaultBankAccount',null),
    -- Item Code
    ('AutoItemCode','0'),
    ('ItemCodePrefix','IT'),
    ('ItemCodeStartNumber','1'),
    ('ItemCodeZeroPadding','3'),
    ('CategoryWiseItemCode','0'),
    ('SubCategoryWiseItemCode','0'),
    ('CodeSeparator','-'),

    -- Customer Code
    ('AutoCustomerCode','0'),
    ('CustomerCodePrefix','CUS'),
    ('CustomerCodeStartNumber','1'),
    ('CustomerCodeZeroPadding','3'),

    -- Supplier Code
    ('AutoSupplierCode','0'),
    ('SupplierCodePrefix','SUP'),
    ('SupplierCodeStartNumber','1'),
    ('SupplierCodeZeroPadding','3'),

    -- Category Code
    ('AutoCategoryCode','0'),
    ('CategoryCodePrefix','CAT'),
    ('CategoryCodeStartNumber','1'),
    ('CategoryCodeZeroPadding','3'),

    -- SubCategory Code
    ('AutoSubCategoryCode','0'),
    ('SubCategoryCodePrefix','SUB'),
    ('SubCategoryCodeStartNumber','1'),
    ('SubCategoryCodeZeroPadding','3'),
    ('CategoryWiseSubCategoryCode','0'),

    -- Quotation
    ('AutoQuotationNo','1'),
    ('QuotationNoPrefix','QT'),
    ('QuotationNoStartNumber','1'),
    ('QuotationNoZeroPadding','4'),

    -- Sales Order
    ('AutoSalesOrderNo','1'),
    ('SalesOrderNoPrefix','SO'),
    ('SalesOrderNoStartNumber','1'),
    ('SalesOrderNoZeroPadding','4'),

    -- Delivery Note
    ('AutoDeliveryNoteNo','1'),
    ('DeliveryNoteNoPrefix','DN'),
    ('DeliveryNoteNoStartNumber','1'),
    ('DeliveryNoteNoZeroPadding','4'),
    ('DeliveryNoteStockAffect','0'),

    -- Purchase Order
    ('AutoPurchaseOrderNo','1'),
    ('PurchaseOrderNoPrefix','PO'),
    ('PurchaseOrderNoStartNumber','1'),
    ('PurchaseOrderNoZeroPadding','4'),

    -- Purchase
    ('AutoPurchaseNo','1'),
    ('PurchaseNoPrefix','P'),
    ('PurchaseNoStartNumber','1'),
    ('PurchaseNoZeroPadding','4'),

    -- Defaults
    ('DefaultFocusOn','ItemCode'),
    ('DefaultBillType','Cash'),
    ('AllowNegativeStock','0'),
    ('ShowCostInSales','0'),
    ('RequireBarcode','0'),
    ('AutoBarcodeGeneration','1'),
    ('SalesInvoice_EnableUpdate','0'),
    ('SalesInvoice_EnableCancel','0'),
    ('SalesReturn_EnableUpdate','0'),
    ('SalesReturn_EnableCancel','0'),

    -- Image Settings
    ('ImageStorageMode','0'),
    ('ImageClientFolder','DefaultClient'),
    ('ImageLocalBasePath','\\192.168.1.11\D\Images'),
    ('ImageAwsRegion','us-east-1'),
    ('ImageAwsBucket','erp-images'),
    ('ImageAwsAccessKey',''),
    ('ImageAwsSecretKey',''),

    -- Journal
    ('AutoJournalEntryNo','1'),
    ('JournalEntryNoPrefix','JV'),
    ('JournalEntryNoStartNumber','1'),
    ('JournalEntryNoZeroPadding','4'),

    -- Expense
    ('AutoExpenseEntryNo','1'),
    ('ExpenseEntryNoPrefix','EXP'),
    ('ExpenseEntryNoStartNumber','1'),
    ('ExpenseEntryNoZeroPadding','4'),

    -- Customer Receipt
    ('AutoCustomerReceiptNo','1'),
    ('CustomerReceiptNoPrefix','CR'),
    ('CustomerReceiptNoStartNumber','1'),
    ('CustomerReceiptNoZeroPadding','6'),

    -- Stock Adjustment
    ('AutoStockAdjustmentNo','0'),
    ('StockAdjustmentNoPrefix','SA'),
    ('StockAdjustmentNoStartNumber','1'),
    ('StockAdjustmentNoZeroPadding','4'),

    -- Material Transfer In
    ('AutoMaterialTransferInNo','0'),
    ('MaterialTransferInNoPrefix','MTI'),
    ('MaterialTransferInNoStartNumber','1'),
    ('MaterialTransferInNoZeroPadding','4'),

    -- Material Transfer Out
    ('AutoMaterialTransferOutNo','0'),
    ('MaterialTransferOutNoPrefix','MTO'),
    ('MaterialTransferOutNoStartNumber','1'),
    ('MaterialTransferOutNoZeroPadding','4'),

    -- Job Order
    ('AutoJobOrderNo','0'),
    ('JobOrderNoPrefix','JO'),
    ('JobOrderNoStartNumber','1'),
    ('JobOrderNoZeroPadding','4'),

    -- Job Card
    ('AutoJobCardNo','0'),
    ('JobCardNoPrefix','JC'),
    ('JobCardNoStartNumber','1'),
    ('JobCardNoZeroPadding','4'),

    ('AutoMaterialTransferRequestNo', '0'),
    ('MaterialTransferRequestNoPrefix', 'MTR'),
    ('MaterialTransferRequestNoStartNumber', '1'),
    ('MaterialTransferRequestNoZeroPadding', '4')
ON CONFLICT (""SettingKey"") DO NOTHING;
";
    }
    private string GetMenuSeedSql()
    {
        return @"
/* =========================
   SUPER PARENTS
   ========================= */
INSERT INTO ""MenuDetails""
(""Name"",""MenuCaption"",""ArabicCaption"",""ParentId"",""AssemblyName"",""ClassName"",""Position"",""Active"",""IsFavorite"",""IsFavorite2"",""FilterOneOrder"",""FilterTwoOrder"")

-- ROOT MENUS
SELECT * FROM (VALUES
('Masters','Masters','البيانات الأساسية',NULL::int,NULL::text,'AddCircle',1,TRUE,FALSE,FALSE,0,0),
('Transactions','Transactions','المعاملات',NULL::int,NULL::text,'ShoppingCart',2,TRUE,FALSE,FALSE,0,0),
('Accounting','Accounting','المحاسبة',NULL::int,NULL::text,'AccountBalance',3,TRUE,FALSE,FALSE,0,0),
('Reports','Reports','التقارير',NULL::int,NULL::text,'Assessment',4,TRUE,FALSE,FALSE,0,0),
('Settings','Settings','الإعدادات',NULL::int,NULL::text,'Settings',5,TRUE,FALSE,FALSE,0,0)
) AS v(""Name"",""MenuCaption"",""ArabicCaption"",""ParentId"",""AssemblyName"",""ClassName"",""Position"",""Active"",""IsFavorite"",""IsFavorite2"",""FilterOneOrder"",""FilterTwoOrder"")

ON CONFLICT (""Name"") DO NOTHING;
/* =========================
   MASTERS SUB-GROUPS
   ========================= */
INSERT INTO ""MenuDetails""
(""Name"",""MenuCaption"",""ArabicCaption"",""ParentId"",""AssemblyName"",""ClassName"",""Position"",""Active"",""IsFavorite"",""IsFavorite2"",""FilterOneOrder"",""FilterTwoOrder"")
SELECT 
    v.""Name"",
    v.""MenuCaption"",
    v.""ArabicCaption"",
    p.""Id"",
    NULL,
    v.""ClassName"",
    v.""Position"",TRUE,FALSE,FALSE,0,0
FROM (
VALUES
('Inventory','Inventory','المخزون','Inventory',1),
('Accounts','Accounts','الحسابات','AccountBalance',2),
('GeneralMasters','General Masters','الإعدادات العامة','SettingsApplications',3),
('Parties','Parties','الأطراف','People',4)
) AS v(""Name"",""MenuCaption"",""ArabicCaption"",""ClassName"",""Position"")
JOIN ""MenuDetails"" p ON p.""Name""='Masters'
ON CONFLICT (""Name"") DO NOTHING;

/* =========================
   MASTERS -> INVENTORY ITEMS
   ========================= */
INSERT INTO ""MenuDetails""
(""Name"",""MenuCaption"",""ArabicCaption"",""ParentId"",""AssemblyName"",""ClassName"",""Position"",""Active"",""IsFavorite"",""IsFavorite2"",""FilterOneOrder"",""FilterTwoOrder"")

SELECT 
    v.""Name"",
    v.""MenuCaption"",
    v.""ArabicCaption"",
    inv.""Id"",
    v.""AssemblyName"",
    v.""ClassName"",
    v.""Position"",TRUE,FALSE,FALSE,0,0

FROM (
VALUES
('Unit Master','Unit Master','إدارة الوحدات','ZakiSAASERP.Shared.Pages.Masters.UnitMasterView','UnitMasterView',1),
('CategoryMaster','Category Master','إدارة الفئات','ZakiSAASERP.Shared.Pages.Masters.CategoryMasterView','CategoryMasterView',2),
('SubCategoryMaster','Sub Category','إدارة الفئات الفرعية','ZakiSAASERP.Shared.Pages.Masters.SubCategoryMasterView','SubCategoryMasterView',3),
('BrandMaster','Brand Master','إدارة العلامات التجارية','ZakiSAASERP.Shared.Pages.Masters.BrandMastersView','BrandMastersView',4),
('ItemMaster','Item Master','إدارة الأصناف','ZakiSAASERP.Shared.Pages.Masters.ItemMasterView','ItemMasterView',5),
('StoreMaster','Store Master','إدارة المخازن','ZakiSAASERP.Shared.Pages.Masters.StoreMasterView','StoreMasterView',6),
('OpeningStockEntry','Opening Stock Entry','إدخال الرصيد الافتتاحي','ZakiSAASERP.Shared.Pages.Masters.OpeningStockEntryView','OpeningStockEntryView',7)
) AS v(""Name"",""MenuCaption"",""ArabicCaption"",""AssemblyName"",""ClassName"",""Position"")

JOIN ""MenuDetails"" inv 
    ON inv.""Name"" = 'Inventory'
    AND inv.""ParentId"" = (
        SELECT ""Id"" FROM ""MenuDetails"" WHERE ""Name""='Masters'
    )

ON CONFLICT (""Name"") DO NOTHING;
/* =========================
   MASTERS -> ACCOUNTS ITEMS
   ========================= */
INSERT INTO ""MenuDetails""
(""Name"",""MenuCaption"",""ArabicCaption"",""ParentId"",""AssemblyName"",""ClassName"",""Position"",""Active"",""IsFavorite"",""IsFavorite2"",""FilterOneOrder"",""FilterTwoOrder"")

SELECT
    v.""Name"",
    v.""MenuCaption"",
    v.""ArabicCaption"",
    acc.""Id"",
    v.""AssemblyName"",
    v.""ClassName"",
    v.""Position"",TRUE,FALSE,FALSE,0,0

FROM (
VALUES
('AccountHeadTitle','Account Head','رؤوس الحسابات','ZakiSAASERP.Shared.Pages.Masters.AccountHeadView','AccountHeadView',1),
('AccountGroup','Account Group','مجموعات الحسابات','ZakiSAASERP.Shared.Pages.Masters.AccountGroupView','AccountGroupView',2),
('AccountSubGroup','Account SubGroup','المجموعات الفرعية للحسابات','ZakiSAASERP.Shared.Pages.Masters.AccountSubGroupView','AccountSubGroupView',3),
('AccountsPage','Accounts','الحسابات','ZakiSAASERP.Shared.Pages.Masters.AccountsView','AccountsView',4)
) AS v(""Name"",""MenuCaption"",""ArabicCaption"",""AssemblyName"",""ClassName"",""Position"")

JOIN ""MenuDetails"" acc
    ON acc.""Name"" = 'Accounts'
    AND acc.""ParentId"" = (
        SELECT ""Id"" FROM ""MenuDetails"" WHERE ""Name""='Masters'
    )

ON CONFLICT (""Name"") DO NOTHING;


/* =========================
   MASTERS -> GENERAL MASTERS ITEMS
   ========================= */
INSERT INTO ""MenuDetails""
(""Name"",""MenuCaption"",""ArabicCaption"",""ParentId"",""AssemblyName"",""ClassName"",""Position"",""Active"",""IsFavorite"",""IsFavorite2"",""FilterOneOrder"",""FilterTwoOrder"")

SELECT
    v.""Name"",
    v.""MenuCaption"",
    v.""ArabicCaption"",
    gm.""Id"",
    v.""AssemblyName"",
    v.""ClassName"",
    v.""Position"",TRUE,FALSE,FALSE,0,0

FROM (
VALUES
('BranchTitle','Branch','الفروع','ZakiSAASERP.Shared.Pages.Masters.BranchView','BranchView',1),
('FinancilaYearTitle','Financial Year','السنة المالية','ZakiSAASERP.Shared.Pages.Masters.FinancialYearView','FinancialYearView',2),
('SalesManMaster','Salesman Master','إدارة مندوبي المبيعات','ZakiSAASERP.Shared.Pages.Masters.SalesManMasterView','SalesManMasterView',3),
('PurchaserMaster','Purchaser Master','إدارة مسؤولي المشتريات','ZakiSAASERP.Shared.Pages.Masters.PurchaserMasterView','PurchaserMasterView',4),
('CountryMaster','Country Master','إدارة الدول','ZakiSAASERP.Shared.Pages.Masters.CountryMasterView','CountryMasterView',5),
('VatMaster','Vat Master','إدارة ضريبة القيمة المضافة','ZakiSAASERP.Shared.Pages.Masters.VatMasterView','VatMasterView',6),
('CurrencyMaster','CurrencyMaster','إدارة العملات','ZakiSAASERP.Shared.Pages.Masters.CurrencyMasterView','CurrencyMasterView',7),
('ProjectMaster','Project Master','إدارة المشاريع','ZakiSAASERP.Shared.Pages.Masters.ProjectMasterView','ProjectMasterView',8),
('VehicleMaster','Vehicle Master','إدارة المركبات','ZakiSAASERP.Shared.Pages.Masters.VehicleMasterView','VehicleMasterView',9),
('RepairMaster','Repair Master','إدارة الصيانة','ZakiSAASERP.Shared.Pages.Masters.RepairMasterView','RepairMasterView',10),
('TechnicianMaster','Technician','إدارة الفنيين','ZakiSAASERP.Shared.Pages.Masters.TechnicianMasterView','TechnicianMasterView',11)
) AS v(""Name"",""MenuCaption"",""ArabicCaption"",""AssemblyName"",""ClassName"",""Position"")

JOIN ""MenuDetails"" gm
    ON gm.""Name"" = 'GeneralMasters'

ON CONFLICT (""Name"") DO NOTHING;

/* =========================
   MASTERS -> PARTIES ITEMS
   ========================= */
INSERT INTO ""MenuDetails""
(""Name"",""MenuCaption"",""ArabicCaption"",""ParentId"",""AssemblyName"",""ClassName"",""Position"",""Active"",""IsFavorite"",""IsFavorite2"",""FilterOneOrder"",""FilterTwoOrder"")

SELECT
    v.""Name"",
    v.""MenuCaption"",
    v.""ArabicCaption"",
    pt.""Id"",
    v.""AssemblyName"",
    v.""ClassName"",
    v.""Position"",TRUE,FALSE,FALSE,0,0

FROM (
VALUES
('CustomerMaster','Customer Master','إدارة العملاء','ZakiSAASERP.Shared.Pages.Masters.CustomerMasterView','CustomerMasterView',1),
('SupplierMaster','Supplier Master','إدارة الموردين','ZakiSAASERP.Shared.Pages.Masters.SupplierMasterView','SupplierMasterView',2),
('CustomerGroupMaster','Customer Group Master','إدارة مجموعات العملاء','ZakiSAASERP.Shared.Pages.Masters.CustomerGroupMasterView','CustomerGroupMasterView',3),
('CustomerLocationMaster','Customer Location','مواقع العملاء','ZakiSAASERP.Shared.Pages.Masters.CustomerLocationMasterView','CustomerLocationMasterView',4),
('CustomerOpeningEntry','Customer Opening Entry','إدخال الرصيد الافتتاحي للعملاء','ZakiSAASERP.Shared.Pages.Masters.CustomerOPEntryView','CustomerOPEntryView',5),
('SupplierOpeningEntry','Supplier Opening Entry','إدخال الرصيد الافتتاحي للموردين','ZakiSAASERP.Shared.Pages.Masters.SupplierOPEntryView','SupplierOPEntryView',6)
) AS v(""Name"",""MenuCaption"",""ArabicCaption"",""AssemblyName"",""ClassName"",""Position"")

JOIN ""MenuDetails"" pt
    ON pt.""Name"" = 'Parties'

ON CONFLICT (""Name"") DO NOTHING;


/* =========================
   TRANSACTIONS SUB-GROUPS
   ========================= */
INSERT INTO ""MenuDetails""
(""Name"",""MenuCaption"",""ArabicCaption"",""ParentId"",""AssemblyName"",""ClassName"",""Position"",""Active"",""IsFavorite"",""IsFavorite2"",""FilterOneOrder"",""FilterTwoOrder"")

SELECT
    v.""Name"",
    v.""MenuCaption"",
    v.""ArabicCaption"",
    t.""Id"",
    NULL,
    v.""ClassName"",
    v.""Position"",TRUE,FALSE,FALSE,0,0

FROM (
VALUES
('Sales','Sales','المبيعات','ShoppingBag',1),
('Purchase','Purchase','المشتريات','ShoppingBasket',2),
('Inventory_Trans','Inventory','المخزون','FmdGood',3),
('Others','Others','أخرى','CardMembership',4)
) AS v(""Name"",""MenuCaption"",""ArabicCaption"",""ClassName"",""Position"")

JOIN ""MenuDetails"" t
    ON t.""Name"" = 'Transactions'

ON CONFLICT (""Name"") DO NOTHING;


/* =========================
   TRANSACTIONS ITEMS
   ========================= */

-- Sales
INSERT INTO ""MenuDetails""
(""Name"",""MenuCaption"",""ArabicCaption"",""ParentId"",""AssemblyName"",""ClassName"",""Position"",""Active"",""IsFavorite"",""IsFavorite2"",""FilterOneOrder"",""FilterTwoOrder"")

SELECT
    v.""Name"",
    v.""MenuCaption"",
    v.""ArabicCaption"",
    sl.""Id"",
    v.""AssemblyName"",
    v.""ClassName"",
    v.""Position"",TRUE,FALSE,FALSE,0,0

FROM (
VALUES
('SalesQuotTitle','Sales Quotation','عرض سعر المبيعات','ZakiSAASERP.Shared.Pages.Transactions.Sales.SalesQuotationView','SalesQuotationView',1),
('ProformaInvoiceTitle','Proforma Invoice','فاتورة مبدئية','ZakiSAASERP.Shared.Pages.Transactions.Sales.ProformaInvoiceView','ProformaInvoiceView',2),
('SalesOrderTitle','Sales Order','أمر بيع','ZakiSAASERP.Shared.Pages.Transactions.Sales.SalesOrderView','SalesOrderView',3),
('SalesInvoiceTitle','Sales Invoice','فاتورة مبيعات','ZakiSAASERP.Shared.Pages.Transactions.Sales.SalesInvoiceView','SalesInvoiceView',4),
('SalesInvoiceRtnTitle','Sales Invoice Return','مرتجع فاتورة مبيعات','ZakiSAASERP.Shared.Pages.Transactions.Sales.SalesInvoiceRtnView','SalesInvoiceRtnView',5),
('EnquiryRegistration','Enquiry Registration','تسجيل الاستفسارات','ZakiSAASERP.Shared.Pages.Transactions.Sales.EnquiryRegistrationView','EnquiryRegistrationView',6),
('DeliveryNoteTitle','Delivery Note','إشعار تسليم','ZakiSAASERP.Shared.Pages.Transactions.Sales.DeliveryNoteView','DeliveryNoteView',7),
('Delivery Note Return','Delivery Note Return','مرتجع إشعار التسليم','ZakiSAASERP.Shared.Pages.Transactions.Sales.DeliveryNoteRtnView','DeliveryNoteRtnView',8)
) AS v(""Name"",""MenuCaption"",""ArabicCaption"",""AssemblyName"",""ClassName"",""Position"")

JOIN ""MenuDetails"" sl
    ON sl.""Name"" = 'Sales'
    AND sl.""ParentId"" = (
        SELECT ""Id"" FROM ""MenuDetails"" WHERE ""Name"" = 'Transactions'
    )

ON CONFLICT (""Name"") DO NOTHING;

-- Purchase
INSERT INTO ""MenuDetails""
(""Name"",""MenuCaption"",""ArabicCaption"",""ParentId"",""AssemblyName"",""ClassName"",""Position"",""Active"",""IsFavorite"",""IsFavorite2"",""FilterOneOrder"",""FilterTwoOrder"")

SELECT
    v.""Name"",
    v.""MenuCaption"",
    v.""ArabicCaption"",
    pur.""Id"",
    v.""AssemblyName"",
    v.""ClassName"",
    v.""Position"",TRUE,FALSE,FALSE,0,0

FROM (
VALUES
('PurchaseOrderTitle','Purchase Order','أمر شراء','ZakiSAASERP.Shared.Pages.Transactions.Purchase.PurchaseOrderView','PurchaseOrderView',1),
('PurchaseTitle','Purchase','المشتريات','ZakiSAASERP.Shared.Pages.Transactions.Purchase.PurchaseView','PurchaseView',2),
('GoodsReceipt','Goods Receipt','استلام البضائع','ZakiSAASERP.Shared.Pages.Transactions.Purchase.GoodsReceiptView','GoodsReceiptView',3),
('Purchase Return','Purchase Return','مرتجع مشتريات','ZakiSAASERP.Shared.Pages.Transactions.Purchase.PurchaseReturnView','PurchaseReturnView',4)
) AS v(""Name"",""MenuCaption"",""ArabicCaption"",""AssemblyName"",""ClassName"",""Position"")

JOIN ""MenuDetails"" pur
    ON pur.""Name"" = 'Purchase'
    AND pur.""ParentId"" = (
        SELECT ""Id"" FROM ""MenuDetails"" WHERE ""Name"" = 'Transactions'
    )

ON CONFLICT (""Name"") DO NOTHING;
-- Inventory (Transactions)
INSERT INTO ""MenuDetails""
(""Name"",""MenuCaption"",""ArabicCaption"",""ParentId"",""AssemblyName"",""ClassName"",""Position"",""Active"",""IsFavorite"",""IsFavorite2"",""FilterOneOrder"",""FilterTwoOrder"")

SELECT
    v.""Name"",
    v.""MenuCaption"",
    v.""ArabicCaption"",
    inv.""Id"",
    v.""AssemblyName"",
    v.""ClassName"",
    v.""Position"",TRUE,FALSE,FALSE,0,0

FROM (
VALUES
('Damage Stock','Damage Stock','مخزون تالف','ZakiSAASERP.Shared.Pages.Transactions.Inventory.DamageStockView','DamageStockView',1),
('StockAdjustment','Stock Adjustment','تسوية المخزون','ZakiSAASERP.Shared.Pages.Transactions.Inventory.StockAdjustmentView','StockAdjustmentView',2),
('MaterialTransferRequest','Material Transfer Request','طلب نقل المواد','ZakiSAASERP.Shared.Pages.Transactions.Inventory.MaterialTransferRequestView','MaterialTransferRequestView',3),
('MaterialTransferApproval','Material Transfer Approval','اعتماد نقل المواد','ZakiSAASERP.Shared.Pages.Transactions.Inventory.MaterialTransferApprovalView','MaterialTransferApprovalView',4),
('MaterialTransferIN','Material Transfer IN','إدخال نقل المواد','ZakiSAASERP.Shared.Pages.Transactions.Inventory.MaterialTransferInView','MaterialTransferInView',5),
('MaterialTransferOUT','Material Transfer OUT','إخراج نقل المواد','ZakiSAASERP.Shared.Pages.Transactions.Inventory.MaterialTransferOutView','MaterialTransferOutView',6),
('InternalMaterialTransfer','Internal Material Transfer','نقل المواد الداخلي','ZakiSAASERP.Shared.Pages.Transactions.Inventory.InternalMaterialTransferView','InternalMaterialTransferView',7)
) AS v(""Name"",""MenuCaption"",""ArabicCaption"",""AssemblyName"",""ClassName"",""Position"")

JOIN ""MenuDetails"" inv
    ON inv.""Name"" = 'Inventory_Trans'

ON CONFLICT (""Name"") DO NOTHING;

-- Others
INSERT INTO ""MenuDetails""
(""Name"",""MenuCaption"",""ArabicCaption"",""ParentId"",""AssemblyName"",""ClassName"",""Position"",""Active"",""IsFavorite"",""IsFavorite2"",""FilterOneOrder"",""FilterTwoOrder"")

SELECT
    v.""Name"",
    v.""MenuCaption"",
    v.""ArabicCaption"",
    oth.""Id"",
    v.""AssemblyName"",
    v.""ClassName"",
    v.""Position"",TRUE,FALSE,FALSE,0,0

FROM (
VALUES
('JobOrder','Job Order','أمر عمل','ZakiSAASERP.Shared.Pages.Transactions.Workshop.JobOrderView','JobOrderView',1),
('JobCard','Job Card','بطاقة عمل','ZakiSAASERP.Shared.Pages.Transactions.Workshop.JobCardView','JobCardView',2)
) AS v(""Name"",""MenuCaption"",""ArabicCaption"",""AssemblyName"",""ClassName"",""Position"")

JOIN ""MenuDetails"" oth
    ON oth.""Name"" = 'Others'
    AND oth.""ParentId"" = (
        SELECT ""Id"" FROM ""MenuDetails"" WHERE ""Name"" = 'Transactions'
    )

ON CONFLICT (""Name"") DO NOTHING;


/* =========================
   ACCOUNTING ITEMS
   ========================= */
INSERT INTO ""MenuDetails""
(""Name"",""MenuCaption"",""ArabicCaption"",""ParentId"",""AssemblyName"",""ClassName"",""Position"",""Active"",""IsFavorite"",""IsFavorite2"",""FilterOneOrder"",""FilterTwoOrder"")

SELECT
    v.""Name"",
    v.""MenuCaption"",
    v.""ArabicCaption"",
    ac.""Id"",
    v.""AssemblyName"",
    v.""ClassName"",
    v.""Position"",TRUE,FALSE,FALSE,0,0

FROM (
VALUES
('JournalEntry','Journal Entry','قيد يومية','ZakiSAASERP.Shared.Pages.Transactions.Accounts.JournalEntryView','JournalEntryView',1),
('CustomerReceipt','Customer Receipt','سند قبض العملاء','ZakiSAASERP.Shared.Pages.Transactions.Accounts.CustomerReceiptView','CustomerReceiptView',2),
('ExpenseEntry','Expense Entry','قيد مصروفات','ZakiSAASERP.Shared.Pages.Transactions.Accounts.ExpenseEntryView','ExpenseEntryView',3),
('AccountOpeningEntry','Account Opening Entry','قيد افتتاحي للحسابات','ZakiSAASERP.Shared.Pages.Transactions.Accounts.AccountOpeningEntryView','AccountOpeningEntryView',4),
('SupplierPayment','Supplier Payment','سند دفع الموردين','ZakiSAASERP.Shared.Pages.Transactions.Accounts.SupplierPaymentView','SupplierPaymentView',5)
) AS v(""Name"",""MenuCaption"",""ArabicCaption"",""AssemblyName"",""ClassName"",""Position"")

JOIN ""MenuDetails"" ac
    ON ac.""Name"" = 'Accounting'

ON CONFLICT (""Name"") DO NOTHING;


/* =========================
   REPORTS SUB-GROUPS
   ========================= */
INSERT INTO ""MenuDetails""
(""Name"",""MenuCaption"",""ArabicCaption"",""ParentId"",""AssemblyName"",""ClassName"",""Position"",""Active"",""IsFavorite"",""IsFavorite2"",""FilterOneOrder"",""FilterTwoOrder"")

SELECT
    v.""Name"",
    v.""MenuCaption"",
    v.""ArabicCaption"",
    r.""Id"",
    NULL,
    v.""ClassName"",
    v.""Position"",TRUE,FALSE,FALSE,0,0

FROM (
VALUES
('Rpt_Sales','Sales Reports','تقارير المبيعات','ShoppingBag',1),
('Rpt_Monitor','Monitor Reports','تقارير المراقبة','Monitor',2),
('Rpt_Supplier','Supplier Reports','تقارير الموردين','AssignmentInd',3),
('Rpt_Customer','Customer Reports','تقارير العملاء','Person',4),
('Rpt_Purchase','Purchase Reports','تقارير المشتريات','ShoppingBasket',5),
('Rpt_Inventory','Inventory Reports','تقارير المخزون','FmdGood',6),
('Rpt_Accounts','Accounts Reports','تقارير الحسابات','AccountBox',7),
('Rpt_Enquiry','Enquiry Reports','تقارير الاستفسارات','Assignment',8)
) AS v(""Name"",""MenuCaption"",""ArabicCaption"",""ClassName"",""Position"")

JOIN ""MenuDetails"" r
    ON r.""Name"" = 'Reports'

ON CONFLICT (""Name"") DO NOTHING;


/* =========================
   REPORTS ITEMS
   ========================= */

-- Sales Reports
INSERT INTO ""MenuDetails""
(""Name"",""MenuCaption"",""ArabicCaption"",""ParentId"",""AssemblyName"",""ClassName"",""Position"",""Active"",""IsFavorite"",""IsFavorite2"",""FilterOneOrder"",""FilterTwoOrder"")

SELECT
    v.""Name"",
    v.""MenuCaption"",
    v.""ArabicCaption"",
    rs.""Id"",
    v.""AssemblyName"",
    v.""ClassName"",
    v.""Position"",TRUE,FALSE,FALSE,0,0

FROM (
VALUES
('Sales Report','Sales Report','تقرير المبيعات','ZakiSAASERP.Shared.Pages.Reports.Sales.SalesReportsView','SalesReportsView',1),
('Sales Return Report','Sales Return Report','تقرير إرجاع المبيعات','ZakiSAASERP.Shared.Pages.Reports.Sales.SalesReturnReportView','SalesReturnReportView',12),
('Sales Details Report','Sales Details Report','تقرير تفاصيل المبيعات','ZakiSAASERP.Shared.Pages.Reports.Sales.SalesDetailsView','SalesDetailsView',2),
('Sales Return Details Report','Sales Return Details Report','تقرير تفاصيل مرتجعات المبيعات','ZakiSAASERP.Shared.Pages.Reports.Sales.SalesReturnDetailsReportView','SalesReturnDetailsReportView',13),
('Customer Wise Sales Report','Customer Wise Sales Report','تقرير المبيعات حسب العميل','ZakiSAASERP.Shared.Pages.Reports.Sales.CustomerWiseSalesReportsView','CustomerWiseSalesReportsView',3),
('Category Wise Report','Category Wise Report','تقرير حسب الفئة','ZakiSAASERP.Shared.Pages.Reports.Sales.CategoryWiseReportView','CategoryWiseReportView',4),
('Sales Quotation Register','Sales Quotation Register','سجل عروض أسعار المبيعات','ZakiSAASERP.Shared.Pages.Reports.Sales.SalesQuotationRegisterView','SalesQuotationRegisterView',5),
('Customer Wise Quotation Report','Customer Wise Quotation Report','تقرير عروض الأسعار حسب العميل','ZakiSAASERP.Shared.Pages.Reports.Sales.CustomerWiseQuotationReportView','CustomerWiseQuotationReportView',6),
('Sales Order Register','Sales Order Register','سجل أوامر البيع','ZakiSAASERP.Shared.Pages.Reports.Sales.SalesOrderReportView','SalesOrderReportView',7),
('Delivery Note Register','Delivery Note Register','سجل إشعارات التسليم','ZakiSAASERP.Shared.Pages.Reports.Sales.DeliveryNoteRegisterView','DeliveryNoteRegisterView',8),
('Date Wise Profit Report','Date Wise Profit Report','تقرير الأرباح حسب التاريخ','ZakiSAASERP.Shared.Pages.Reports.Sales.DateWiseProfitReportView','DateWiseProfitReportView',9),
('Proforma Invoice Report','Proforma Invoice Report','تقرير الفاتورة المبدئية','ZakiSAASERP.Shared.Pages.Reports.Sales.ProformaInvoiceReportView','ProformaInvoiceReportView',10),
('Vat Report','Vat Report','تقرير ضريبة القيمة المضافة','ZakiSAASERP.Shared.Pages.Reports.Sales.VatReportView','VatReportView',11),
('Shift Closing Report','Shift Closing Report','تقرير إغلاق الوردية','ZakiSAASERP.Shared.Pages.Reports.Sales.ShiftClosingReportView','ShiftClosingReportView',14)
) AS v(""Name"",""MenuCaption"",""ArabicCaption"",""AssemblyName"",""ClassName"",""Position"")

JOIN ""MenuDetails"" rs
    ON rs.""Name"" = 'Rpt_Sales'

ON CONFLICT (""Name"") DO NOTHING;

-- Monitor Reports
INSERT INTO ""MenuDetails""
(""Name"",""MenuCaption"",""ArabicCaption"",""ParentId"",""AssemblyName"",""ClassName"",""Position"",""Active"",""IsFavorite"",""IsFavorite2"",""FilterOneOrder"",""FilterTwoOrder"")

SELECT
    v.""Name"",
    v.""MenuCaption"",
    v.""ArabicCaption"",
    rm.""Id"",
    v.""AssemblyName"",
    v.""ClassName"",
    v.""Position"",TRUE,FALSE,FALSE,0,0

FROM (
VALUES
('Sales Monitor Report','Sales Monitor Report','تقرير مراقبة المبيعات','ZakiSAASERP.Shared.Pages.Reports.Transactions.SalesMonitorReport','SalesMonitorReport',1),
('Purchase Monitor Report','Purchase Monitor Report','تقرير مراقبة المشتريات','ZakiSAASERP.Shared.Pages.Reports.Transactions.PurchaseMonitorReport','PurchaseMonitorReport',2),
('Transaction Monitor Report','Transaction Monitor Report','تقرير مراقبة المعاملات','ZakiSAASERP.Shared.Pages.Reports.Transactions.TransactionMonitorReport','TransactionMonitorReport',3)
) AS v(""Name"",""MenuCaption"",""ArabicCaption"",""AssemblyName"",""ClassName"",""Position"")

JOIN ""MenuDetails"" rm
    ON rm.""Name"" = 'Rpt_Monitor'

ON CONFLICT (""Name"") DO NOTHING;

-- Supplier Reports
INSERT INTO ""MenuDetails""
(""Name"",""MenuCaption"",""ArabicCaption"",""ParentId"",""AssemblyName"",""ClassName"",""Position"",""Active"",""IsFavorite"",""IsFavorite2"",""FilterOneOrder"",""FilterTwoOrder"")

SELECT
    v.""Name"",
    v.""MenuCaption"",
    v.""ArabicCaption"",
    rsp.""Id"",
    v.""AssemblyName"",
    v.""ClassName"",
    v.""Position"",TRUE,FALSE,FALSE,0,0

FROM (
VALUES
('Supplier Statement','Supplier Statement','كشف حساب المورد','ZakiSAASERP.Shared.Pages.Reports.Accounts.SupplierStatementReport','SupplierStatementReport',1),
('Supplier Pending Invoices','Supplier Pending Invoices','فواتير الموردين المعلقة','ZakiSAASERP.Shared.Pages.Reports.Accounts.SupplierPendingInvoiceReports','SupplierPendingInvoiceReports',2),
('Supplier Summary','Supplier Summary','ملخص الموردين','ZakiSAASERP.Shared.Pages.Reports.Accounts.SupplierSummaryReport','SupplierSummaryReport',3),
('Supplier List','Supplier List','قائمة الموردين','ZakiSAASERP.Shared.Pages.Reports.Accounts.SupplierListReport','SupplierListReport',4),
('Supplier Payment Report','Supplier Payment Report','تقرير مدفوعات الموردين','ZakiSAASERP.Shared.Pages.Reports.Accounts.SupplierPaymentReports','SupplierPaymentReports',5)
) AS v(""Name"",""MenuCaption"",""ArabicCaption"",""AssemblyName"",""ClassName"",""Position"")

JOIN ""MenuDetails"" rsp
    ON rsp.""Name"" = 'Rpt_Supplier'

ON CONFLICT (""Name"") DO NOTHING;

-- Customer Reports
INSERT INTO ""MenuDetails""
(""Name"",""MenuCaption"",""ArabicCaption"",""ParentId"",""AssemblyName"",""ClassName"",""Position"",""Active"",""IsFavorite"",""IsFavorite2"",""FilterOneOrder"",""FilterTwoOrder"")

SELECT
    v.""Name"",
    v.""MenuCaption"",
    v.""ArabicCaption"",
    rc.""Id"",
    v.""AssemblyName"",
    v.""ClassName"",
    v.""Position"",TRUE,FALSE,FALSE,0,0

FROM (
VALUES
('Customer Statement','Customer Statement','كشف حساب العميل','ZakiSAASERP.Shared.Pages.Reports.Sales.CustomerStatementView','CustomerStatementView',1),
('Customer Summary','Customer Summary','ملخص العملاء','ZakiSAASERP.Shared.Pages.Reports.Accounts.CustomerSummaryReport','CustomerSummaryReport',2),
('Customer Pending Invoice Report','Customer Pending Invoice Report','فواتير العملاء المعلقة','ZakiSAASERP.Shared.Pages.Reports.Accounts.CustomerPendingInvoiceReports','CustomerPendingInvoiceReports',3),
('Customer List','Customer List','قائمة العملاء','ZakiSAASERP.Shared.Pages.Reports.Accounts.CustomerListReport','CustomerListReport',4)
) AS v(""Name"",""MenuCaption"",""ArabicCaption"",""AssemblyName"",""ClassName"",""Position"")

JOIN ""MenuDetails"" rc
    ON rc.""Name"" = 'Rpt_Customer'

ON CONFLICT (""Name"") DO NOTHING;

-- Purchase Reports
INSERT INTO ""MenuDetails""
(""Name"",""MenuCaption"",""ArabicCaption"",""ParentId"",""AssemblyName"",""ClassName"",""Position"",""Active"",""IsFavorite"",""IsFavorite2"",""FilterOneOrder"",""FilterTwoOrder"")

SELECT
    v.""Name"",
    v.""MenuCaption"",
    v.""ArabicCaption"",
    rp.""Id"",
    v.""AssemblyName"",
    v.""ClassName"",
    v.""Position"",TRUE,FALSE,FALSE,0,0

FROM (
VALUES
('Purchase Summary','Purchase Summary','ملخص المشتريات','ZakiSAASERP.Shared.Pages.Reports.Purchase.PurchaseSummaryView','PurchaseSummaryView',1),
('Purchase Details','Purchase Details','تفاصيل المشتريات','ZakiSAASERP.Shared.Pages.Reports.Purchase.PurchaseDetailsView','PurchaseDetailsView',2),
('Purchase Order Report','Purchase Order Report','تقرير أوامر الشراء','ZakiSAASERP.Shared.Pages.Reports.Purchase.PurchaseOrderReportView','PurchaseOrderReportView',3),
('Purchase Return Summary','Purchase Return Summary','ملخص إرجاع الشراء','ZakiSAASERP.Shared.Pages.Reports.Purchase.PurchaseReturnSummaryView','PurchaseReturnSummaryView',4),
('Purchase Return Details','Purchase Return Details','تفاصيل إرجاع الشراء','ZakiSAASERP.Shared.Pages.Reports.Purchase.PurchaseReturnDetailsView','PurchaseReturnDetailsView',5)
) AS v(""Name"",""MenuCaption"",""ArabicCaption"",""AssemblyName"",""ClassName"",""Position"")

JOIN ""MenuDetails"" rp
    ON rp.""Name"" = 'Rpt_Purchase'

ON CONFLICT (""Name"") DO NOTHING;

-- Inventory Reports
INSERT INTO ""MenuDetails""
(""Name"",""MenuCaption"",""ArabicCaption"",""ParentId"",""AssemblyName"",""ClassName"",""Position"",""Active"",""IsFavorite"",""IsFavorite2"",""FilterOneOrder"",""FilterTwoOrder"")

SELECT
    v.""Name"",
    v.""MenuCaption"",
    v.""ArabicCaption"",
    ri.""Id"",
    v.""AssemblyName"",
    v.""ClassName"",
    v.""Position"",TRUE,FALSE,FALSE,0,0

FROM (
VALUES
('Stock Report','Stock Report','تقرير المخزون','ZakiSAASERP.Shared.Pages.Reports.Inventory.StockReportView','StockReportView',1),
('Stock Transfer Report','Stock Transfer Report','تقرير نقل المخزون','ZakiSAASERP.Shared.Pages.Reports.Inventory.StockTransferReportView','StockTransferReportView',2),
('Item Transaction History Report','Item Transaction History Report','تقرير سجل حركات الأصناف','ZakiSAASERP.Shared.Pages.Reports.Inventory.ItemTransactionHistoryReportView','ItemTransactionHistoryReportView',3),
('Stock Adjustment Report','Stock Adjustment Report','تقرير تسوية المخزون','ZakiSAASERP.Shared.Pages.Reports.Inventory.StockAdjustmentView','StockAdjustmentView',4)
) AS v(""Name"",""MenuCaption"",""ArabicCaption"",""AssemblyName"",""ClassName"",""Position"")

JOIN ""MenuDetails"" ri
    ON ri.""Name"" = 'Rpt_Inventory'

ON CONFLICT (""Name"") DO NOTHING;

-- Accounts Reports
INSERT INTO ""MenuDetails""
(""Name"",""MenuCaption"",""ArabicCaption"",""ParentId"",""AssemblyName"",""ClassName"",""Position"",""Active"",""IsFavorite"",""IsFavorite2"",""FilterOneOrder"",""FilterTwoOrder"")

SELECT
    v.""Name"",
    v.""MenuCaption"",
    v.""ArabicCaption"",
    ra.""Id"",
    v.""AssemblyName"",
    v.""ClassName"",
    v.""Position"",TRUE,FALSE,FALSE,0,0

FROM (
VALUES
('Journal Entry Report','Journal Entry Report','تقرير قيود اليومية','ZakiSAASERP.Shared.Pages.Reports.Accounts.JournalEntryReportView','JournalEntryReportView',1),
('Trial Balance Report','Trial Balance Report','تقرير ميزان المراجعة','ZakiSAASERP.Shared.Pages.Reports.Accounts.TrialBalanceView','TrialBalanceView',2),
('Expense Report','Expense Report','تقرير المصروفات','ZakiSAASERP.Shared.Pages.Reports.Accounts.ExpenseReportView','ExpenseReportView',3),
('Profit And Loss Report','Profit And Loss Report','تقرير الأرباح والخسائر','ZakiSAASERP.Shared.Pages.Reports.Accounts.ProfitAndLossReportView','ProfitAndLossReportView',4),
('Balance Sheet Report','Balance Sheet Report','تقرير الميزانية العمومية','ZakiSAASERP.Shared.Pages.Reports.Accounts.BalanceSheetReportView','BalanceSheetReportView',5),
('Chart Of Account Report','Chart Of Account Report','تقرير دليل الحسابات','ZakiSAASERP.Shared.Pages.Reports.Accounts.ChartOfAccountReportView','ChartOfAccountReportView',6),
('LedgerReport','Ledger Report','تقرير دفتر الأستاذ','ZakiSAASERP.Shared.Pages.Reports.Accounts.LedgerReportView','LedgerReportView',7)
) AS v(""Name"",""MenuCaption"",""ArabicCaption"",""AssemblyName"",""ClassName"",""Position"")

JOIN ""MenuDetails"" ra
    ON ra.""Name"" = 'Rpt_Accounts'

ON CONFLICT (""Name"") DO NOTHING;

-- Enquiry Register Report
INSERT INTO ""MenuDetails""
(""Name"",""MenuCaption"",""ArabicCaption"",""ParentId"",""AssemblyName"",""ClassName"",""Position"",""Active"",""IsFavorite"",""IsFavorite2"",""FilterOneOrder"",""FilterTwoOrder"")

SELECT
    v.""Name"",
    v.""MenuCaption"",
    v.""ArabicCaption"",
    re.""Id"",
    v.""AssemblyName"",
    v.""ClassName"",
    v.""Position"",TRUE,FALSE,FALSE,0,0

FROM (
VALUES
('Enquiry Registration Report','Enquiry Registration Report','تقرير تسجيل الاستفسارات','ZakiSAASERP.Shared.Pages.Reports.Sales.EnquiryRegistrationReportView','EnquiryRegistrationReportView',1)
) AS v(""Name"",""MenuCaption"",""ArabicCaption"",""AssemblyName"",""ClassName"",""Position"")

JOIN ""MenuDetails"" re
    ON re.""Name"" = 'Rpt_Enquiry'

ON CONFLICT (""Name"") DO NOTHING;

-- Others Reports
INSERT INTO ""MenuDetails""
(""Name"",""MenuCaption"",""ArabicCaption"",""ParentId"",""AssemblyName"",""ClassName"",""Position"",""Active"",""IsFavorite"",""IsFavorite2"",""FilterOneOrder"",""FilterTwoOrder"")

SELECT
    v.""Name"",
    v.""MenuCaption"",
    v.""ArabicCaption"",
    ro.""Id"",
    v.""AssemblyName"",
    v.""ClassName"",
    v.""Position"",TRUE,FALSE,FALSE,0,0

FROM (
VALUES
('PendingZatca','Zatca Report','تقرير الزكاة والضريبة (زاتكا)','ZakiSAASERP.Shared.Pages.Reports.Transaction.ZatcaPendingView','ZatcaPendingView',1)
) AS v(""Name"",""MenuCaption"",""ArabicCaption"",""AssemblyName"",""ClassName"",""Position"")

JOIN ""MenuDetails"" ro
    ON ro.""Name"" = 'Rpt_Sales'

ON CONFLICT (""Name"") DO NOTHING;


/* =========================
   SETTINGS ITEMS
   ========================= */
INSERT INTO ""MenuDetails""
(""Name"",""MenuCaption"",""ArabicCaption"",""ParentId"",""AssemblyName"",""ClassName"",""Position"",""Active"",""IsFavorite"",""IsFavorite2"",""FilterOneOrder"",""FilterTwoOrder"")

SELECT
    v.""Name"",
    v.""MenuCaption"",
    v.""ArabicCaption"",
    st.""Id"",
    v.""AssemblyName"",
    v.""ClassName"",
    v.""Position"",TRUE,FALSE,FALSE,0,0

FROM (
VALUES
('ManageUser','Manage User','إدارة المستخدمين','ZakiSAASERP.Shared.Pages.Users.UserView','UserView',1),
('PrintDesignTitle','Print Design','تصميم الطباعة','ZakiSAASERP.Shared.Pages.Others.PrintDesignerView','PrintDesignerView',2),
('Barcode Design','Barcode Design','تصميم الباركود','ZakiSAASERP.Shared.Pages.Others.BarcodeDesignerView','BarcodeDesignerView',3),
('CompanyDetails','Company Details','بيانات الشركة','ZakiSAASERP.Shared.Pages.Settings.CompanyDetailsView','CompanyDetailsView',4),
('ZATCA CSR','ZATCA CSR','زاتكا CSR','ZakiSAASERP.Shared.Pages.Settings.ZatcaCSRView','ZatcaCSRView',5),
('User Previlege','User Previlege','صلاحيات المستخدمين','ZakiSAASERP.Shared.Pages.Users.UserPrevilageView','UserPrevilageView',6),
('ZATCA Pending','ZATCA Pending Invoices','فواتير زاتكا المعلقة','ZakiSAASERP.Shared.Pages.Settings.ZatcaPendingUploadView','ZatcaPendingUploadView',7),
('ProductImport','Product Import','استيراد المنتجات','ZakiSAASERP.Shared.Pages.Settings.ProductImportView','ProductImportView',8),
('ProductExport','Product Export','تصدير المنتجات','ZakiSAASERP.Shared.Pages.Settings.ProductExportView','ProductExportView',9),
('PrintingBarcode','Printing Barcode','طباعة الباركود','ZakiSAASERP.Shared.Pages.Settings.PrintingBarcodeView','PrintingBarcodeView',10)
) AS v(""Name"",""MenuCaption"",""ArabicCaption"",""AssemblyName"",""ClassName"",""Position"")

JOIN ""MenuDetails"" st
    ON st.""Name"" = 'Settings'

ON CONFLICT (""Name"") DO NOTHING;
";
    }
    private string GetMenuVisibilitySeedSql()
    {
        return @"
INSERT INTO ""MenuVisibility"" (""MenuDetailId"",""IsVisible"",""LastModified"")
SELECT md.""Id"", True, NOW()
FROM ""MenuDetails"" md
WHERE NOT EXISTS (
    SELECT 1 FROM ""MenuVisibility"" mv 
    WHERE mv.""MenuDetailId"" = md.""Id""
);
";
    }
    private string GetTriggerSQL()
    {
        return @"CREATE OR REPLACE FUNCTION fn_stocktranlog_updatebalance()
RETURNS TRIGGER AS
$$
BEGIN

    -- INSERT
    IF TG_OP = 'INSERT' THEN

        INSERT INTO ""ItemStockBalance"" (""FK_ItemId"",""FK_BranchId"",""CurrentQty"",""LastUpdated"")
        VALUES (NEW.""FK_ItemId"", NEW.""FK_BranchId"",
                COALESCE(NEW.""InQuantity"",0) - COALESCE(NEW.""OutQuantity"",0),
                NOW())
        ON CONFLICT (""FK_ItemId"",""FK_BranchId"")
        DO UPDATE SET
            ""CurrentQty"" = ""ItemStockBalance"".""CurrentQty"" +
                (COALESCE(EXCLUDED.""CurrentQty"",0)),
            ""LastUpdated"" = NOW();

        RETURN NEW;
    END IF;

    -- DELETE
    IF TG_OP = 'DELETE' THEN

        UPDATE ""ItemStockBalance""
        SET ""CurrentQty"" = ""CurrentQty"" -
            (COALESCE(OLD.""InQuantity"",0) - COALESCE(OLD.""OutQuantity"",0)),
            ""LastUpdated"" = NOW()
        WHERE ""FK_ItemId"" = OLD.""FK_ItemId""
          AND ""FK_BranchId"" = OLD.""FK_BranchId"";

        RETURN OLD;
    END IF;

    -- UPDATE
    IF TG_OP = 'UPDATE' THEN

        -- Remove old values
        UPDATE ""ItemStockBalance""
        SET ""CurrentQty"" = ""CurrentQty"" -
            (COALESCE(OLD.""InQuantity"",0) - COALESCE(OLD.""OutQuantity"",0))
        WHERE ""FK_ItemId"" = OLD.""FK_ItemId""
          AND ""FK_BranchId"" = OLD.""FK_BranchId"";

        -- Add new values
        INSERT INTO ""ItemStockBalance"" (""FK_ItemId"",""FK_BranchId"",""CurrentQty"",""LastUpdated"")
        VALUES (NEW.""FK_ItemId"", NEW.""FK_BranchId"",
                COALESCE(NEW.""InQuantity"",0) - COALESCE(NEW.""OutQuantity"",0),
                NOW())
        ON CONFLICT (""FK_ItemId"",""FK_BranchId"")
        DO UPDATE SET
            ""CurrentQty"" = ""ItemStockBalance"".""CurrentQty"" +
                (COALESCE(EXCLUDED.""CurrentQty"",0)),
            ""LastUpdated"" = NOW();

        RETURN NEW;
    END IF;

    RETURN NULL;
END;
$$ LANGUAGE plpgsql;
";
    }
    private string GetViewSQL()
    {
        return @"
CREATE OR REPLACE VIEW barcodedata
AS
SELECT
    i.""ItemId"",
    i.""ItemCode"",
    i.""ItemName"",
    i.""ItemNameArabic"",
    i.""SalesRate"",
    i.""PurchaseRate"",
    i.""WholeSaleRate"",
    i.""MinSaleRate"",
    i.""Location"",
    i.""StandardCost"",
    i.""StockMode"",
    i.""Notes"",
    i.""Model"",
    i.""Fitted"",
    i.""ItemImageName"",
    i.""ItemExpiry"",
    i.""ItemType"",
    i.""Barcode"",
    i.""WeightPerCarton"",
    i.""LandingCost"",
    i.""InclusiveRate"",

    b.""Name"" AS ""BrandName"",
    u.""Name"" AS ""UnitName"",
    c.""Name"" AS ""CategoryName"",
    sc.""Name"" AS ""SubCategoryName"",
    s.""Name"" AS ""SupplierName""

FROM ""ItemMaster"" i
LEFT JOIN ""BrandMaster"" b        ON i.""FK_BrandId"" = b.""BrandId""
LEFT JOIN ""UnitMaster"" u         ON i.""FK_UnitId"" = u.""UnitId""
LEFT JOIN ""CategoryMaster"" c     ON i.""FK_CategoryId"" = c.""CategoryId""
LEFT JOIN ""SubCategoryMaster"" sc ON i.""FK_SubCategoryId"" = sc.""SubCategoryId""
LEFT JOIN ""SupplierMaster"" s     ON i.""FK_SupplierId"" = s.""SupplierId"";
";
    }

}
