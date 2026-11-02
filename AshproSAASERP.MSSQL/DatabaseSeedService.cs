using AshproSAASERP.Interface;
using AshproSAASERP.Interface.Others;
using AshproSAASERP.Models.Company;
using AshproSAASERP.Models.Masters;
using AshproSAASERP.Models.User;
using Microsoft.EntityFrameworkCore;

namespace AshproSAASERP.MSSQL;

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
                    context.CountryMasters.Add(new CountryMaster { Name = "Saudi Arabia", Code = "SA" });

                    await context.SaveChangesAsync();
                    int iBranchId = branch.BranchId;
                    var user = new UserDetails
                    {
                        UserName = exlcompany.UserName,
                        PassWord = exlcompany.PassWord,
                        FK_BranchId = iBranchId,
                        PrevilageId = 1
                    };
                    var companyDtls = new CompanyDetails
                    {
                        Name = exlcompany.CompanyName,
                        Contact = exlcompany.ContactNo,
                        Email = exlcompany.Email,
                        FK_BranchId = iBranchId,
                        FK_CountryId = 1
                    };
                    context.CompanyDetails.Add(companyDtls);
                    context.UserDetails.Add(user);
                    var fyear = new FinancialYear
                    {
                        FromDate = exlcompany.SubscriptionStartedAt,
                        ToDate = exlcompany.SubscriptionEndingDate,
                        IsClosed = false,
                        Name = exlcompany.SubscriptionStartedAt.Value.Year.ToString(),
                        RefNo = 1
                    };

                    context.FinancialYears.Add(fyear);
                    context.UnitMasters.Add(new UnitMaster { Name = "PCs", Code = "PCs", Remarks = "Default Unit" });
                    context.BrandMasters.Add(new BrandMaster { Name = "General", Code = "GEN", Remarks = "Default Brand" });
                    context.CategoryMasters.Add(new CategoryMaster { Name = "General", Code = "Gen", Remarks = "Default Category" });
                    context.SubCategoryMasters.Add(new SubCategoryMaster { Name = "General", Code = "Gen", Remarks = "Default SubCategory", FK_CategoryId = 1 });
                    context.SalesManMasters.Add(new SalesManMaster { Name = "SM", Code = "SM", Remarks = "Default Salesman", Designation = "Salesman" });
                    context.StoreMasters.Add(new StoreMaster { Name = "Main Store", Code = "Main", IsDefaultStore = true, FK_BranchId = iBranchId });
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
                    var sql = GetMenuSeedSql();
                    await context.Database.ExecuteSqlRawAsync(sql);
                }

                if (!await context.PermanentSettings.AnyAsync())
                {
                    var settingssql = GetSettingsSeed();
                    await context.Database.ExecuteSqlRawAsync(settingssql);
                }

                if (await context.MenuDetails.AnyAsync() && !await context.MenuVisibilities.AnyAsync())
                {
                    var menuvisibiltysql = GetMenuVisibilitySeedSql();
                    await context.Database.ExecuteSqlRawAsync(menuvisibiltysql);
                }

                // Ensure any missing menus (e.g. PrintingBarcode), visibilities, presets, and settings are seeded for existing databases
                await MenuSeedHelper.EnsureSeedDataAsync(context);

                if (!await context.ERPTypes.AnyAsync())
                {
                    var erptypesql = GetErpTypeSQL();
                    await context.Database.ExecuteSqlRawAsync(erptypesql);
                }
                var actypesql = GetAccountTypeSQL();
                await context.Database.ExecuteSqlRawAsync(actypesql);
                var acchdsql = GetAccHdSQL();
                await context.Database.ExecuteSqlRawAsync(acchdsql);
                var accgrpsql = GetAccGrpSql();
                await context.Database.ExecuteSqlRawAsync(accgrpsql);
                var accsubgrpsql = GetAccSubGrpSql();
                await context.Database.ExecuteSqlRawAsync(accsubgrpsql);
                var accmstrsql = GetAcMstSql();
                await context.Database.ExecuteSqlRawAsync(accmstrsql);
                // Seed logic here
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding database: {ex.Message}"); // Handle exceptions
            }
        }
    }
    private string GetAcMstSql()
    {
        return @"IF NOT EXISTS (SELECT 1 FROM AccountMaster)
BEGIN
    INSERT INTO [dbo].[AccountMaster]
    ([Code],[Name],[NameArabic],[FK_AccountHeadId],[FK_AcGroupId],
     [Opening],[DbCr],[FK_AcTypeId],[FK_AccountSubGroupId],[Flag],[IsReserved])
    VALUES
    ('110001010001','Cash in Hand',N'نقد في اليد',1,1,0,'Debit',1,1,'Acc',1),
    ('110001020001','Bank Account 1',N'حساب بنكي 1',1,1,0,'Debit',1,2,'Acc',1),
    ('110001020002','Bank Account 2',N'حساب بنكي 2',1,1,0,'Debit',1,2,'Acc',1),
    ('110002010001','Customer – Local',N'عميل - محلي',1,2,0,'Debit',1,3,'Acc',1),
    ('120001010001','Office Building',N'مبنى المكاتب',2,3,0,'Debit',1,4,'Acc',1),
    ('120001020001','Delivery Vehicle',N'سيارة توصيل',2,3,0,'Debit',1,5,'Acc',1),
    ('120001030001','Office Equipment',N'معدات مكتبية',2,3,0,'Debit',1,6,'Acc',1),
    ('210001010001','Supplier – Local',N'مورد - محلي',3,4,0,'Credit',2,7,'Acc',1),
    ('210002010001','VAT Output @15%',N'ضريبة المخرجات @15%',3,5,0,'Credit',2,8,'Acc',1),
    ('210002020001','VAT Payable to ZATCA',N'ضريبة مستحقة للزكاة',3,5,0,'Credit',2,9,'Acc',1),
    ('220001010001','Long-Term Loan',N'قرض طويل الأجل',4,6,0,'Credit',2,10,'Acc',1),
    ('310001010001','Sales',N'المبيعات',5,7,0,'Credit',3,11,'Acc',1),
    ('310002010001','Discount Received',N'خصم مكتسب',5,8,0,'Credit',3,12,'Acc',1),
    ('310002020001','Interest Income',N'دخل الفوائد',5,8,0,'Credit',3,13,'Acc',1),
    ('410001010001','Office Rent',N'إيجار المكتب',6,9,0,'Debit',4,14,'Acc',1),
    ('410001020001','Staff Salary',N'رواتب الموظفين',6,9,0,'Debit',4,15,'Acc',1),
    ('410002010001','Electricity Expense',N'مصاريف الكهرباء',6,10,0,'Debit',4,16,'Acc',1),
    ('410002020001','Water Expense',N'مصاريف المياه',6,10,0,'Debit',4,17,'Acc',1),
    ('410003010001','Delivery Expense',N'مصاريف التوصيل',6,11,0,'Debit',4,18,'Acc',1),
    ('410004010001','Depreciation – Vehicle',N'إهلاك - مركبة',6,12,0,'Debit',4,19,'Acc',1),
    ('410005010001','Bank Charges',N'رسوم بنكية',6,13,0,'Debit',4,20,'Acc',1),
    ('330001010001','Opening Balance Equity',N'رصيد الافتتاحي',7,14,0,'Credit',2,21,'Acc',1),
    ('420001010001','Purchase Account',N'حساب المشتريات',8,15,0,'Debit',4,22,'Acc',1),
    ('110003010001','VAT Input @15%',N'ضريبة المدخلات @15%',1,16,0,'Debit',1,23,'Acc',1),
    ('110004010001','Inventory Asset',N'أصل المخزون',1,17,0,'Debit',1,24,'Acc',1);
END";
    }
    private string GetAccSubGrpSql()
    {
        return @"IF NOT EXISTS (SELECT 1 FROM AccountSubGroup)
BEGIN
    INSERT INTO [dbo].[AccountSubGroup]
           ([FK_AcGroupId],[Code],[Name],[NameArabic],[Remarks],[FK_AcHeadId],[IsReserved])
    VALUES
    (1,'11000101','Cash',N'نقد',NULL,1,1),
    (1,'11000102','Bank',N'بنك',NULL,1,1),
    (2,'11000201','Sundry Debtors',N'مدينون متنوعون',NULL,1,1),
    (3,'12000101','Building',N'مبنى',NULL,2,1),
    (3,'12000102','Vehicles',N'مركبات',NULL,2,1),
    (3,'12000103','Equipment',N'معدات',NULL,2,1),
    (4,'21000101','Sundry Creditors',N'دائنون متنوعون',NULL,3,1),
    (5,'21000201','VAT Output',N'مخرجات الضريبة',NULL,3,1),
    (5,'21000202','VAT Payable',N'ضريبة مستحقة',NULL,3,1),
    (6,'22000101','Bank Loan',N'قرض بنكي',NULL,4,1),
    (7,'31000101','Local Sales',N'مبيعات محلية',NULL,5,1),
    (8,'31000201','Discount',N'خصم',NULL,5,1),
    (8,'31000202','Interest',N'فائدة',NULL,5,1),
    (9,'41000101','Rent',N'إيجار',NULL,6,1),
    (9,'41000102','Salary',N'راتب',NULL,6,1),
    (10,'41000201','Electricity',N'كهرباء',NULL,6,1),
    (10,'41000202','Water',N'ماء',NULL,6,1),
    (11,'41000301','Transport',N'نقل',NULL,6,1),
    (12,'41000401','Vehicles',N'مركبات',NULL,6,1),
    (13,'41000501','Bank Charges',N'رسوم بنكية',NULL,6,1),
    (14,'33000101','Owners Equity',N'حقوق الملكية',NULL,7,1),
    (15,'42000101','Local Purchases',N'مشتريات محلية',NULL,8,1),
    (16,'11000301','VAT Input',N'مدخلات الضريبة',NULL,1,1),
    (17,'11000401','Stock',N'مخزون',NULL,1,1);
END";
    }
    private string GetAccGrpSql()
    {
        return @"IF NOT EXISTS (SELECT 1 FROM AccountGroup)
BEGIN
    INSERT INTO [dbo].[AccountGroup]
           ([Code],[Name],[NameArabic],[Remarks],[FK_AcHeadId],[IsReserved])
    VALUES
    ('110001','Cash & Bank',N'النقد والبنك',NULL,1,1),
    ('110002','Accounts Receivable',N'حسابات القبض',NULL,1,1),
    ('120001','Property Plant & Equip',N'الممتلكات والمنشآت والمعدات',NULL,2,1),
    ('210001','Accounts Payable',N'حسابات الدفع',NULL,3,1),
    ('210002','VAT Payable',N'ضريبة القيمة المضافة المستحقة',NULL,3,1),
    ('220001','Loans',N'قروض',NULL,4,1),
    ('310001','Sales',N'المبيعات',NULL,5,1),
    ('310002','Other Income',N'إيرادات أخرى',NULL,5,1),
    ('410001','Administrative',N'إداري',NULL,6,1),
    ('410002','Utilities',N'خدمات',NULL,6,1),
    ('410003','Selling Expenses',N'مصاريف البيع',NULL,6,1),
    ('410004','Depreciation',N'إهلاك',NULL,6,1),
    ('410005','Financial Expenses',N'مصاريف مالية',NULL,6,1),
    ('330001','Capital Accounts',N'حسابات رأس المال',NULL,7,1),
    ('420001','Purchases',N'المشتريات',NULL,8,1),
    ('110003','VAT Receivable',N'ضريبة القيمة المضافة المستحقة القبض',NULL,1,1),
    ('110004','Inventory Assets',N'أصول المخزون',NULL,1,1);
END";
    }
    private string GetAccHdSQL()
    {
        return @"IF NOT EXISTS (SELECT 1 FROM AccountHeadMaster)
BEGIN
    INSERT INTO AccountHeadMaster
               (Code, Name, NameArabic, FK_AcTypeId, IsReserved)
    VALUES
    ('1100','Current Assets',N'الأصول المتداولة',1,1),
    ('1200','Fixed Assets',N'الأصول الثابتة',1,1),
    ('2100','Current Liabilities',N'الالتزامات المتداولة',2,1),
    ('2200','Non-Current Liabilities',N'الالتزامات غير المتداولة',2,1),
    ('3100','Revenue',N'إيرادات',3,1),
    ('4100','Operating Expenses',N'مصاريف تشغيلية',4,1),
    ('3300','Equity',N'حقوق الملكية',2,1),
    ('4200','Cost of Sales',N'تكلفة المبيعات',4,1);
END";
    }
    private string GetAccountTypeSQL()
    {
        return @"
IF NOT EXISTS (SELECT 1 FROM AccountType)
BEGIN

INSERT INTO AccountType
           (AcTypeName)
VALUES
('Asset'),
('Liability'),
('Income'),
('Expense');
END";
    }
    private string GetErpTypeSQL()
    {
        return @"
IF NOT EXISTS (SELECT 1 FROM ERPType)
BEGIN
INSERT INTO ERPType (TypeName,[Description],IsCustom ,CreatedDate)
     VALUES
('Trading'	,'Sales-focused business with inventory and customer management',	0	,'2025-12-24 11:20:10.377'),
('Manufacturing','Production-based business with full procurement, manufacturing and sales',	0	,'2025-12-24 11:20:10.377'),
('Transportation',	'Logistics and delivery services with minimal inventory',	0	,'2025-12-24 11:20:10.377'),
('Retail',	'Point-of-sale and customer-focused retail operations',	0	,'2025-12-24 11:20:10.377'),
('Services',	'Service-based business with project management and minimal inventory',	0	,'2025-12-24 11:20:10.377'),
('Custom',	'Fully customizable menu configuration - select specific menus to show',	1	,'2025-12-24 11:20:10.377');
End
";
    }
    private string GetSettingsSeed()
    {
        return @"
IF NOT EXISTS (SELECT 1 FROM PermanentSettings)
BEGIN
    INSERT INTO PermanentSettings (SettingKey, SettingValue)
    VALUES
 ('DefaultCashAccount',null),
    ('DefaultBankAccount',null),
    -- Item Code
    ('AutoItemCode','1'),
    ('ItemCodePrefix','IT'),
    ('ItemCodeStartNumber','1'),
    ('ItemCodeZeroPadding','3'),
    ('CategoryWiseItemCode','0'),
    ('SubCategoryWiseItemCode','0'),
    ('CodeSeparator','-'),

    -- Customer Code
    ('AutoCustomerCode','1'),
    ('CustomerCodePrefix','CUS'),
    ('CustomerCodeStartNumber','1'),
    ('CustomerCodeZeroPadding','3'),

    -- Supplier Code
    ('AutoSupplierCode','1'),
    ('SupplierCodePrefix','SUP'),
    ('SupplierCodeStartNumber','1'),
    ('SupplierCodeZeroPadding','3'),

    -- Category Code
    ('AutoCategoryCode','1'),
    ('CategoryCodePrefix','CAT'),
    ('CategoryCodeStartNumber','1'),
    ('CategoryCodeZeroPadding','3'),

    -- SubCategory Code
    ('AutoSubCategoryCode','1'),
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
    ('AutoStockAdjustmentNo','1'),
    ('StockAdjustmentNoPrefix','SA'),
    ('StockAdjustmentNoStartNumber','1'),
    ('StockAdjustmentNoZeroPadding','4'),

    -- Material Transfer In
    ('AutoMaterialTransferInNo','1'),
    ('MaterialTransferInNoPrefix','MTI'),
    ('MaterialTransferInNoStartNumber','1'),
    ('MaterialTransferInNoZeroPadding','4'),

    -- Material Transfer Out
    ('AutoMaterialTransferOutNo','1'),
    ('MaterialTransferOutNoPrefix','MTO'),
    ('MaterialTransferOutNoStartNumber','1'),
    ('MaterialTransferOutNoZeroPadding','4'),

    -- Job Order
    ('AutoJobOrderNo','1'),
    ('JobOrderNoPrefix','JO'),
    ('JobOrderNoStartNumber','1'),
    ('JobOrderNoZeroPadding','4'),

    -- Job Card
    ('AutoJobCardNo','1'),
    ('JobCardNoPrefix','JC'),
    ('JobCardNoStartNumber','1'),
    ('JobCardNoZeroPadding','4'),

    ('AutoMaterialTransferRequestNo', '1'),
    ('MaterialTransferRequestNoPrefix', 'MTR'),
    ('MaterialTransferRequestNoStartNumber', '1'),
    ('MaterialTransferRequestNoZeroPadding', '4');
END

";
    }
    private string GetMenuSeedSql()
    {
        return @"
IF NOT EXISTS (SELECT 1 FROM MenuDetails)
BEGIN
/* =========================
   SUPER PARENTS
   ========================= */
INSERT INTO MenuDetails (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder) VALUES
('Masters', 'Masters', N'البيانات الأساسية', NULL, NULL, 'AddCircle', 1, 1, 0, 0, 0, 0),
('Transactions', 'Transactions', N'المعاملات', NULL, NULL, 'ShoppingCart', 2, 1, 0, 0, 0, 0),
('Accounting', 'Accounting', N'المحاسبة', NULL, NULL, 'AccountBalance', 3, 1, 0, 0, 0, 0),
('Reports', 'Reports', N'التقارير', NULL, NULL, 'Assessment', 4, 1, 0, 0, 0, 0),
('Settings', 'Settings', N'الإعدادات', NULL, NULL, 'Settings', 5, 1, 0, 0, 0, 0);
/* =========================
   MASTERS SUB-GROUPS
   ========================= */
INSERT INTO MenuDetails (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder) VALUES
('Inventory', 'Inventory', N'المخزون', (SELECT Id FROM MenuDetails WHERE Name='Masters'), NULL, 'Inventory', 1, 1, 0, 0, 0, 0),
('Accounts', 'Accounts', N'الحسابات', (SELECT Id FROM MenuDetails WHERE Name='Masters'), NULL, 'AccountBalance', 2, 1, 0, 0, 0, 0),
('GeneralMasters', 'General Masters', N'الإعدادات العامة', (SELECT Id FROM MenuDetails WHERE Name='Masters'), NULL, 'SettingsApplications', 3, 1, 0, 0, 0, 0),
('Parties', 'Parties', N'الأطراف', (SELECT Id FROM MenuDetails WHERE Name='Masters'), NULL, 'People', 4, 1, 0, 0, 0, 0);


/* =========================
   MASTERS -> INVENTORY ITEMS
   ========================= */
INSERT INTO MenuDetails (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder) VALUES 
('Unit Master', 'Unit Master', N'إدارة الوحدات', (SELECT Id FROM MenuDetails WHERE Name='Inventory' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Masters')), 'AshproSAASERP.Shared.Pages.Masters.UnitMasterView', 'UnitMasterView', 1, 1, 0, 0, 0, 0),
('CategoryMaster', 'Category Master', N'إدارة الفئات', (SELECT Id FROM MenuDetails WHERE Name='Inventory' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Masters')), 'AshproSAASERP.Shared.Pages.Masters.CategoryMasterView', 'CategoryMasterView', 2, 1, 0, 0, 0, 0),
('SubCategoryMaster', 'Sub Category', N'إدارة الفئات الفرعية', (SELECT Id FROM MenuDetails WHERE Name='Inventory' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Masters')), 'AshproSAASERP.Shared.Pages.Masters.SubCategoryMasterView', 'SubCategoryMasterView', 3, 1, 0, 0, 0, 0),
('BrandMaster', 'Brand Master', N'إدارة العلامات التجارية', (SELECT Id FROM MenuDetails WHERE Name='Inventory' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Masters')), 'AshproSAASERP.Shared.Pages.Masters.BrandMastersView', 'BrandMastersView', 4, 1, 0, 0, 0, 0),
('ItemMaster', 'Item Master', N'إدارة الأصناف', (SELECT Id FROM MenuDetails WHERE Name='Inventory' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Masters')), 'AshproSAASERP.Shared.Pages.Masters.ItemMasterView', 'ItemMasterView', 5, 1, 0, 0, 0, 0),
('StoreMaster', 'Store Master', N'إدارة المخازن', (SELECT Id FROM MenuDetails WHERE Name='Inventory' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Masters')), 'AshproSAASERP.Shared.Pages.Masters.StoreMasterView', 'StoreMasterView', 6, 1, 0, 0, 0, 0),
('OpeningStockEntry', 'Opening Stock Entry', N'إدخال الرصيد الافتتاحي', (SELECT Id FROM MenuDetails WHERE Name='Inventory' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Masters')), 'AshproSAASERP.Shared.Pages.Masters.OpeningStockEntryView', 'OpeningStockEntryView', 7, 1, 0, 0, 0, 0);

/* =========================
   MASTERS -> ACCOUNTS ITEMS
   ========================= */
INSERT INTO MenuDetails (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder) VALUES 
('AccountHeadTitle', 'Account Head', N'رؤوس الحسابات', (SELECT Id FROM MenuDetails WHERE Name='Accounts' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Masters')), 'AshproSAASERP.Shared.Pages.Masters.AccountHeadView', 'AccountHeadView', 1, 1, 0, 0, 0, 0),
('AccountGroup', 'Account Group', N'مجموعات الحسابات', (SELECT Id FROM MenuDetails WHERE Name='Accounts' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Masters')), 'AshproSAASERP.Shared.Pages.Masters.AccountGroupView', 'AccountGroupView', 2, 1, 0, 0, 0, 0),
('AccountSubGroup', 'Account SubGroup', N'المجموعات الفرعية للحسابات', (SELECT Id FROM MenuDetails WHERE Name='Accounts' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Masters')), 'AshproSAASERP.Shared.Pages.Masters.AccountSubGroupView', 'AccountSubGroupView', 3, 1, 0, 0, 0, 0),
('AccountsPage', 'Accounts', N'الحسابات', (SELECT Id FROM MenuDetails WHERE Name='Accounts' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Masters')), 'AshproSAASERP.Shared.Pages.Masters.AccountsView', 'AccountsView', 4, 1, 0, 0, 0, 0);


/* =========================
   MASTERS -> GENERAL MASTERS ITEMS
   ========================= */
INSERT INTO MenuDetails (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder) VALUES 
('BranchTitle', 'Branch', N'الفروع', (SELECT Id FROM MenuDetails WHERE Name='GeneralMasters'), 'AshproSAASERP.Shared.Pages.Masters.BranchView', 'BranchView', 1, 1, 0, 0, 0, 0),
('FinancilaYearTitle', 'Financial Year', N'السنة المالية', (SELECT Id FROM MenuDetails WHERE Name='GeneralMasters'), 'AshproSAASERP.Shared.Pages.Masters.FinancialYearView', 'FinancialYearView', 2, 1, 0, 0, 0, 0),
('SalesManMaster', 'Salesman Master', N'إدارة مندوبي المبيعات', (SELECT Id FROM MenuDetails WHERE Name='GeneralMasters'), 'AshproSAASERP.Shared.Pages.Masters.SalesManMasterView', 'SalesManMasterView', 3, 1, 0, 0, 0, 0),
('PurchaserMaster', 'Purchaser Master', N'إدارة مسؤولي المشتريات', (SELECT Id FROM MenuDetails WHERE Name='GeneralMasters'), 'AshproSAASERP.Shared.Pages.Masters.PurchaserMasterView', 'PurchaserMasterView', 4, 1, 0, 0, 0, 0),
('CountryMaster', 'Country Master', N'إدارة الدول', (SELECT Id FROM MenuDetails WHERE Name='GeneralMasters'), 'AshproSAASERP.Shared.Pages.Masters.CountryMasterView', 'CountryMasterView', 5, 1, 0, 0, 0, 0),
('VatMaster', 'Vat Master', N'إدارة ضريبة القيمة المضافة', (SELECT Id FROM MenuDetails WHERE Name='GeneralMasters'), 'AshproSAASERP.Shared.Pages.Masters.VatMasterView', 'VatMasterView', 6, 1, 0, 0, 0, 0),
('CurrencyMaster', 'CurrencyMaster', N'إدارة العملات', (SELECT Id FROM MenuDetails WHERE Name='GeneralMasters'), 'AshproSAASERP.Shared.Pages.Masters.CurrencyMasterView', 'CurrencyMasterView', 7, 1, 0, 0, 0, 0),
('ProjectMaster', 'Project Master', N'إدارة المشاريع', (SELECT Id FROM MenuDetails WHERE Name='GeneralMasters'), 'AshproSAASERP.Shared.Pages.Masters.ProjectMasterView', 'ProjectMasterView', 8, 1, 0, 0, 0, 0),
('VehicleMaster', 'Vehicle Master', N'إدارة المركبات', (SELECT Id FROM MenuDetails WHERE Name='GeneralMasters'), 'AshproSAASERP.Shared.Pages.Masters.VehicleMasterView', 'VehicleMasterView', 9, 1, 0, 0, 0, 0),
('RepairMaster', 'Repair Master', N'إدارة الصيانة', (SELECT Id FROM MenuDetails WHERE Name='GeneralMasters'), 'AshproSAASERP.Shared.Pages.Masters.RepairMasterView', 'RepairMasterView', 10, 1, 0, 0, 0, 0),
('TechnicianMaster', 'Technician', N'إدارة الفنيين', (SELECT Id FROM MenuDetails WHERE Name='GeneralMasters'), 'AshproSAASERP.Shared.Pages.Masters.TechnicianMasterView', 'TechnicianMasterView', 11, 1, 0, 0, 0, 0);

/* =========================
   MASTERS -> PARTIES ITEMS
   ========================= */
INSERT INTO MenuDetails (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder) VALUES 
('CustomerMaster', 'Customer Master', N'إدارة العملاء', (SELECT Id FROM MenuDetails WHERE Name='Parties'), 'AshproSAASERP.Shared.Pages.Masters.CustomerMasterView', 'CustomerMasterView', 1, 1, 0, 0, 0, 0),
('SupplierMaster', 'Supplier Master', N'إدارة الموردين', (SELECT Id FROM MenuDetails WHERE Name='Parties'), 'AshproSAASERP.Shared.Pages.Masters.SupplierMasterView', 'SupplierMasterView', 2, 1, 0, 0, 0, 0),
('CustomerGroupMaster', 'Customer Group Master', N'إدارة مجموعات العملاء', (SELECT Id FROM MenuDetails WHERE Name='Parties'), 'AshproSAASERP.Shared.Pages.Masters.CustomerGroupMasterView', 'CustomerGroupMasterView', 3, 1, 0, 0, 0, 0),
('CustomerLocationMaster', 'Customer Location', N'مواقع العملاء', (SELECT Id FROM MenuDetails WHERE Name='Parties'), 'AshproSAASERP.Shared.Pages.Masters.CustomerLocationMasterView', 'CustomerLocationMasterView', 4, 1, 0, 0, 0, 0),
('CustomerOpeningEntry', 'Customer Opening Entry', N'إدخال الرصيد الافتتاحي للعملاء', (SELECT Id FROM MenuDetails WHERE Name='Parties'), 'AshproSAASERP.Shared.Pages.Masters.CustomerOPEntryView', 'CustomerOPEntryView', 5, 1, 0, 0, 0, 0),
('SupplierOpeningEntry', 'Supplier Opening Entry', N'إدخال الرصيد الافتتاحي للموردين', (SELECT Id FROM MenuDetails WHERE Name='Parties'), 'AshproSAASERP.Shared.Pages.Masters.SupplierOPEntryView', 'SupplierOPEntryView', 6, 1, 0, 0, 0, 0);


/* =========================
   TRANSACTIONS SUB-GROUPS
   ========================= */
INSERT INTO MenuDetails (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder) VALUES
('Sales', 'Sales', N'المبيعات', (SELECT Id FROM MenuDetails WHERE Name='Transactions'), NULL, 'ShoppingBag', 1, 1, 0, 0, 0, 0),
('Purchase', 'Purchase', N'المشتريات', (SELECT Id FROM MenuDetails WHERE Name='Transactions'), NULL, 'ShoppingBasket', 2, 1, 0, 0, 0, 0),
('Inventory_Trans', 'Inventory', N'المخزون', (SELECT Id FROM MenuDetails WHERE Name='Transactions'), NULL, 'FmdGood', 3, 1, 0, 0, 0, 0),
('Others', 'Others', N'أخرى', (SELECT Id FROM MenuDetails WHERE Name='Transactions'), NULL, 'CardMembership', 4, 1, 0, 0, 0, 0);


/* =========================
   TRANSACTIONS ITEMS
   ========================= */

-- Sales
INSERT INTO MenuDetails (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder) VALUES 
('SalesQuotTitle', 'Sales Quotation', N'عرض سعر المبيعات', (SELECT Id FROM MenuDetails WHERE Name='Sales' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Transactions')), 'AshproSAASERP.Shared.Pages.Transactions.Sales.SalesQuotationView', 'SalesQuotationView', 1, 1, 0, 0, 0, 0),
('ProformaInvoiceTitle', 'Proforma Invoice', N'فاتورة مبدئية', (SELECT Id FROM MenuDetails WHERE Name='Sales' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Transactions')), 'AshproSAASERP.Shared.Pages.Transactions.Sales.ProformaInvoiceView', 'ProformaInvoiceView', 2, 1, 0, 0, 0, 0),
('SalesOrderTitle', 'Sales Order', N'أمر بيع', (SELECT Id FROM MenuDetails WHERE Name='Sales' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Transactions')), 'AshproSAASERP.Shared.Pages.Transactions.Sales.SalesOrderView', 'SalesOrderView', 3, 1, 0, 0, 0, 0),
('SalesInvoiceTitle', 'Sales Invoice', N'فاتورة مبيعات', (SELECT Id FROM MenuDetails WHERE Name='Sales' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Transactions')), 'AshproSAASERP.Shared.Pages.Transactions.Sales.SalesInvoiceView', 'SalesInvoiceView', 4, 1, 0, 0, 0, 0),
('SalesInvoiceRtnTitle', 'Sales Invoice Return', N'مرتجع فاتورة مبيعات', (SELECT Id FROM MenuDetails WHERE Name='Sales' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Transactions')), 'AshproSAASERP.Shared.Pages.Transactions.Sales.SalesInvoiceRtnView', 'SalesInvoiceRtnView', 5, 1, 0, 0, 0, 0),
('EnquiryRegistration', 'Enquiry Registration', N'تسجيل الاستفسارات', (SELECT Id FROM MenuDetails WHERE Name='Sales' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Transactions')), 'AshproSAASERP.Shared.Pages.Transactions.Sales.EnquiryRegistrationView', 'EnquiryRegistrationView', 6, 1, 0, 0, 0, 0),
('DeliveryNoteTitle', 'Delivery Note', N'إشعار تسليم', (SELECT Id FROM MenuDetails WHERE Name='Sales' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Transactions')), 'AshproSAASERP.Shared.Pages.Transactions.Sales.DeliveryNoteView', 'DeliveryNoteView', 7, 1, 0, 0, 0, 0),
('Delivery Note Return', 'Delivery Note Return', N'مرتجع إشعار التسليم', (SELECT Id FROM MenuDetails WHERE Name='Sales' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Transactions')), 'AshproSAASERP.Shared.Pages.Transactions.Sales.DeliveryNoteRtnView', 'DeliveryNoteRtnView', 8, 1, 0, 0, 0, 0);

-- Purchase
INSERT INTO MenuDetails (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder) VALUES 
('PurchaseOrderTitle', 'Purchase Order', N'أمر شراء', (SELECT Id FROM MenuDetails WHERE Name='Purchase' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Transactions')), 'AshproSAASERP.Shared.Pages.Transactions.Purchase.PurchaseOrderView', 'PurchaseOrderView', 1, 1, 0, 0, 0, 0),
('PurchaseTitle', 'Purchase', N'المشتريات', (SELECT Id FROM MenuDetails WHERE Name='Purchase' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Transactions')), 'AshproSAASERP.Shared.Pages.Transactions.Purchase.PurchaseView', 'PurchaseView', 2, 1, 0, 0, 0, 0),
('GoodsReceipt', 'Goods Receipt', N'استلام البضائع', (SELECT Id FROM MenuDetails WHERE Name='Purchase' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Transactions')), 'AshproSAASERP.Shared.Pages.Transactions.Purchase.GoodsReceiptView', 'GoodsReceiptView', 3, 1, 0, 0, 0, 0),
('Purchase Return', 'Purchase Return', N'مرتجع مشتريات', (SELECT Id FROM MenuDetails WHERE Name='Purchase' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Transactions')), 'AshproSAASERP.Shared.Pages.Transactions.Purchase.PurchaseReturnView', 'PurchaseReturnView', 4, 1, 0, 0, 0, 0);
-- Inventory (Transactions)
INSERT INTO MenuDetails (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder) VALUES 
('Damage Stock', 'Damage Stock', N'مخزون تالف', (SELECT Id FROM MenuDetails WHERE Name='Inventory_Trans' ), 'AshproSAASERP.Shared.Pages.Transactions.Inventory.DamageStockView', 'DamageStockView', 1, 1, 0, 0, 0, 0),
('StockAdjustment', 'Stock Adjustment', N'تسوية المخزون', (SELECT Id FROM MenuDetails WHERE Name='Inventory_Trans'), 'AshproSAASERP.Shared.Pages.Transactions.Inventory.StockAdjustmentView', 'StockAdjustmentView', 2, 1, 0, 0, 0, 0),
('MaterialTransferRequest', 'Material Transfer Request', N'طلب نقل المواد', (SELECT Id FROM MenuDetails WHERE Name='Inventory_Trans'), 'AshproSAASERP.Shared.Pages.Transactions.Inventory.MaterialTransferRequestView', 'MaterialTransferRequestView', 3, 1, 0, 0, 0, 0),
('MaterialTransferApproval', 'Material Transfer Approval', N'اعتماد نقل المواد', (SELECT Id FROM MenuDetails WHERE Name='Inventory_Trans'), 'AshproSAASERP.Shared.Pages.Transactions.Inventory.MaterialTransferApprovalView', 'MaterialTransferApprovalView', 4, 1, 0, 0, 0, 0),
('MaterialTransferIN', 'Material Transfer IN', N'إدخال نقل المواد', (SELECT Id FROM MenuDetails WHERE Name='Inventory_Trans'), 'AshproSAASERP.Shared.Pages.Transactions.Inventory.MaterialTransferInView', 'MaterialTransferInView', 5, 1, 0, 0, 0, 0),
('MaterialTransferOUT', 'Material Transfer OUT', N'إخراج نقل المواد', (SELECT Id FROM MenuDetails WHERE Name='Inventory_Trans'), 'AshproSAASERP.Shared.Pages.Transactions.Inventory.MaterialTransferOutView', 'MaterialTransferOutView', 6, 1, 0, 0, 0, 0),
('InternalMaterialTransfer', 'Internal Material Transfer', N'نقل المواد الداخلي', (SELECT Id FROM MenuDetails WHERE Name='Inventory_Trans'), 'AshproSAASERP.Shared.Pages.Transactions.Inventory.InternalMaterialTransferView', 'InternalMaterialTransferView', 7, 1, 0, 0, 0, 0);

-- Others
INSERT INTO MenuDetails (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder) VALUES 
('JobOrder', 'Job Order', N'أمر عمل', (SELECT Id FROM MenuDetails WHERE Name='Others' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Transactions')), 'AshproSAASERP.Shared.Pages.Transactions.Workshop.JobOrderView', 'JobOrderView', 1, 1, 0, 0, 0, 0),
('JobCard', 'Job Card', N'بطاقة عمل', (SELECT Id FROM MenuDetails WHERE Name='Others' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Transactions')), 'AshproSAASERP.Shared.Pages.Transactions.Workshop.JobCardView', 'JobCardView', 2, 1, 0, 0, 0, 0);


/* =========================
   ACCOUNTING ITEMS
   ========================= */
INSERT INTO MenuDetails (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder) VALUES 
('JournalEntry', 'Journal Entry', N'قيد يومية', (SELECT Id FROM MenuDetails WHERE Name='Accounting'), 'AshproSAASERP.Shared.Pages.Transactions.Accounts.JournalEntryView', 'JournalEntryView', 1, 1, 0, 0, 0, 0),
('CustomerReceipt', 'Customer Receipt', N'سند قبض العملاء', (SELECT Id FROM MenuDetails WHERE Name='Accounting'), 'AshproSAASERP.Shared.Pages.Transactions.Accounts.CustomerReceiptView', 'CustomerReceiptView', 2, 1, 0, 0, 0, 0),
('ExpenseEntry', 'Expense Entry', N'قيد مصروفات', (SELECT Id FROM MenuDetails WHERE Name='Accounting'), 'AshproSAASERP.Shared.Pages.Transactions.Accounts.ExpenseEntryView', 'ExpenseEntryView', 3, 1, 0, 0, 0, 0),
('AccountOpeningEntry', 'Account Opening Entry', N'قيد افتتاحي للحسابات', (SELECT Id FROM MenuDetails WHERE Name='Accounting'), 'AshproSAASERP.Shared.Pages.Transactions.Accounts.AccountOpeningEntryView', 'AccountOpeningEntryView', 4, 1, 0, 0, 0, 0),
('SupplierPayment', 'Supplier Payment', N'سند دفع الموردين', (SELECT Id FROM MenuDetails WHERE Name='Accounting'), 'AshproSAASERP.Shared.Pages.Transactions.Accounts.SupplierPaymentView', 'SupplierPaymentView', 5, 1, 0, 0, 0, 0);


/* =========================
   REPORTS SUB-GROUPS
   ========================= */
INSERT INTO MenuDetails (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder) VALUES
('Rpt_Sales', 'Sales Reports', N'تقارير المبيعات', (SELECT Id FROM MenuDetails WHERE Name='Reports'), NULL, 'ShoppingBag', 1, 1, 0, 0, 0, 0),
('Rpt_Monitor', 'Monitor Reports', N'تقارير المراقبة', (SELECT Id FROM MenuDetails WHERE Name='Reports'), NULL, 'Monitor', 2, 1, 0, 0, 0, 0),
('Rpt_Supplier', 'Supplier Reports', N'تقارير الموردين', (SELECT Id FROM MenuDetails WHERE Name='Reports'), NULL, 'AssignmentInd', 3, 1, 0, 0, 0, 0),
('Rpt_Customer', 'Customer Reports', N'تقارير العملاء', (SELECT Id FROM MenuDetails WHERE Name='Reports'), NULL, 'Person', 4, 1, 0, 0, 0, 0),
('Rpt_Purchase', 'Purchase Reports', N'تقارير المشتريات', (SELECT Id FROM MenuDetails WHERE Name='Reports'), NULL, 'ShoppingBasket', 5, 1, 0, 0, 0, 0),
('Rpt_Inventory', 'Inventory Reports', N'تقارير المخزون', (SELECT Id FROM MenuDetails WHERE Name='Reports'), NULL, 'FmdGood', 6, 1, 0, 0, 0, 0),
('Rpt_Accounts', 'Accounts Reports', N'تقارير الحسابات', (SELECT Id FROM MenuDetails WHERE Name='Reports'), NULL, 'AccountBox', 7, 1, 0, 0, 0, 0),
('Rpt_Enquiry', 'Enquiry Reports', N'تقارير الاستفسارات', (SELECT Id FROM MenuDetails WHERE Name='Reports'), NULL, 'Assignment', 8, 1, 0, 0, 0, 0);


/* =========================
   REPORTS ITEMS
   ========================= */

-- Sales Reports
INSERT INTO MenuDetails (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder) VALUES 
('Sales Report', 'Sales Report', N'تقرير المبيعات', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Sales'), 'AshproSAASERP.Shared.Pages.Reports.Sales.SalesReportsView', 'SalesReportsView', 1, 1, 0, 0, 0, 0),
('Sales Return Report', 'Sales Return Report', N'تقرير إرجاع المبيعات', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Sales'), 'AshproSAASERP.Shared.Pages.Reports.Sales.SalesReturnReportView', 'SalesReturnReportView', 12, 1, 0, 0, 0, 0),
('Sales Details Report', 'Sales Details Report', N'تقرير تفاصيل المبيعات', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Sales'), 'AshproSAASERP.Shared.Pages.Reports.Sales.SalesDetailsView', 'SalesDetailsView', 2, 1, 0, 0, 0, 0),
('Sales Return Details Report', 'Sales Return Details Report', N'تقرير تفاصيل مرتجعات المبيعات', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Sales'), 'AshproSAASERP.Shared.Pages.Reports.Sales.SalesReturnDetailsReportView', 'SalesReturnDetailsReportView', 13, 1, 0, 0, 0, 0),
('Customer Wise Sales Report', 'Customer Wise Sales Report', N'تقرير المبيعات حسب العميل', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Sales'), 'AshproSAASERP.Shared.Pages.Reports.Sales.CustomerWiseSalesReportsView', 'CustomerWiseSalesReportsView', 3, 1, 0, 0, 0, 0),
('Category Wise Report', 'Category Wise Report', N'تقرير حسب الفئة', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Sales'), 'AshproSAASERP.Shared.Pages.Reports.Sales.CategoryWiseReportView', 'CategoryWiseReportView', 4, 1, 0, 0, 0, 0),
('Sales Quotation Register', 'Sales Quotation Register', N'سجل عروض أسعار المبيعات', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Sales'), 'AshproSAASERP.Shared.Pages.Reports.Sales.SalesQuotationRegisterView', 'SalesQuotationRegisterView', 5, 1, 0, 0, 0, 0),
('Customer Wise Quotation Report', 'Customer Wise Quotation Report', N'تقرير عروض الأسعار حسب العميل', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Sales'), 'AshproSAASERP.Shared.Pages.Reports.Sales.CustomerWiseQuotationReportView', 'CustomerWiseQuotationReportView', 6, 1, 0, 0, 0, 0),
('Sales Order Register', 'Sales Order Register', N'سجل أوامر البيع', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Sales'), 'AshproSAASERP.Shared.Pages.Reports.Sales.SalesOrderReportView', 'SalesOrderReportView', 7, 1, 0, 0, 0, 0),
('Delivery Note Register', 'Delivery Note Register', N'سجل إشعارات التسليم', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Sales'), 'AshproSAASERP.Shared.Pages.Reports.Sales.DeliveryNoteRegisterView', 'DeliveryNoteRegisterView', 8, 1, 0, 0, 0, 0),
('Date Wise Profit Report', 'Date Wise Profit Report', N'تقرير الأرباح حسب التاريخ', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Sales'), 'AshproSAASERP.Shared.Pages.Reports.Sales.DateWiseProfitReportView', 'DateWiseProfitReportView', 9, 1, 0, 0, 0, 0),
('Proforma Invoice Report', 'Proforma Invoice Report', N'تقرير الفاتورة المبدئية', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Sales'), 'AshproSAASERP.Shared.Pages.Reports.Sales.ProformaInvoiceReportView', 'ProformaInvoiceReportView', 10, 1, 0, 0, 0, 0),
('Vat Report', 'Vat Report', N'تقرير ضريبة القيمة المضافة', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Sales'), 'AshproSAASERP.Shared.Pages.Reports.Sales.VatReportView', 'VatReportView', 11, 1, 0, 0, 0, 0);

-- Monitor Reports
INSERT INTO MenuDetails (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder) VALUES 
('Sales Monitor Report', 'Sales Monitor Report', N'تقرير مراقبة المبيعات', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Monitor'), 'AshproSAASERP.Shared.Pages.Reports.Transactions.SalesMonitorReport', 'SalesMonitorReport', 1, 1, 0, 0, 0, 0),
('Purchase Monitor Report', 'Purchase Monitor Report', N'تقرير مراقبة المشتريات', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Monitor'), 'AshproSAASERP.Shared.Pages.Reports.Transactions.PurchaseMonitorReport', 'PurchaseMonitorReport', 2, 1, 0, 0, 0, 0),
('Transaction Monitor Report', 'Transaction Monitor Report', N'تقرير مراقبة المعاملات', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Monitor'), 'AshproSAASERP.Shared.Pages.Reports.Transactions.TransactionMonitorReport', 'TransactionMonitorReport', 3, 1, 0, 0, 0, 0);

-- Supplier Reports
INSERT INTO MenuDetails (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder) VALUES 
('Supplier Statement', 'Supplier Statement', N'كشف حساب المورد', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Supplier'), 'AshproSAASERP.Shared.Pages.Reports.Accounts.SupplierStatementReport', 'SupplierStatementReport', 1, 1, 0, 0, 0, 0),
('Supplier Pending Invoices', 'Supplier Pending Invoices', N'فواتير الموردين المعلقة', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Supplier'), 'AshproSAASERP.Shared.Pages.Reports.Accounts.SupplierPendingInvoiceReports', 'SupplierPendingInvoiceReports', 2, 1, 0, 0, 0, 0),
('Supplier Summary', 'Supplier Summary', N'ملخص الموردين', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Supplier'), 'AshproSAASERP.Shared.Pages.Reports.Accounts.SupplierSummaryReport', 'SupplierSummaryReport', 3, 1, 0, 0, 0, 0),
('Supplier List', 'Supplier List', N'قائمة الموردين', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Supplier'), 'AshproSAASERP.Shared.Pages.Reports.Accounts.SupplierListReport', 'SupplierListReport', 4, 1, 0, 0, 0, 0),
('Supplier Payment Report', 'Supplier Payment Report', N'تقرير مدفوعات الموردين', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Supplier'), 'AshproSAASERP.Shared.Pages.Reports.Accounts.SupplierPaymentReports', 'SupplierPaymentReports', 5, 1, 0, 0, 0, 0);

-- Customer Reports
INSERT INTO MenuDetails (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder) VALUES 
('Customer Statement', 'Customer Statement', N'كشف حساب العميل', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Customer'), 'AshproSAASERP.Shared.Pages.Reports.Sales.CustomerStatementView', 'CustomerStatementView', 1, 1, 0, 0, 0, 0),
('Customer Summary', 'Customer Summary', N'ملخص العملاء', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Customer'), 'AshproSAASERP.Shared.Pages.Reports.Accounts.CustomerSummaryReport', 'CustomerSummaryReport', 2, 1, 0, 0, 0, 0),
('Customer Pending Invoice Report', 'Customer Pending Invoice Report', N'فواتير العملاء المعلقة', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Customer'), 'AshproSAASERP.Shared.Pages.Reports.Accounts.CustomerPendingInvoiceReports', 'CustomerPendingInvoiceReports', 3, 1, 0, 0, 0, 0),
('Customer List', 'Customer List', N'قائمة العملاء', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Customer'), 'AshproSAASERP.Shared.Pages.Reports.Accounts.CustomerListReport', 'CustomerListReport', 4, 1, 0, 0, 0, 0);

-- Purchase Reports
INSERT INTO MenuDetails (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder) VALUES 
('Purchase Summary', 'Purchase Summary', N'ملخص المشتريات', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Purchase'), 'AshproSAASERP.Shared.Pages.Reports.Purchase.PurchaseSummaryView', 'PurchaseSummaryView', 1, 1, 0, 0, 0, 0),
('Purchase Details', 'Purchase Details', N'تفاصيل المشتريات', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Purchase'), 'AshproSAASERP.Shared.Pages.Reports.Purchase.PurchaseDetailsView', 'PurchaseDetailsView', 2, 1, 0, 0, 0, 0),
('Purchase Order Report', 'Purchase Order Report', N'تقرير أوامر الشراء', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Purchase'), 'AshproSAASERP.Shared.Pages.Reports.Purchase.PurchaseOrderReportView', 'PurchaseOrderReportView', 3, 1, 0, 0, 0, 0),
('Purchase Return Summary', 'Purchase Return Summary', N'ملخص إرجاع الشراء', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Purchase'), 'AshproSAASERP.Shared.Pages.Reports.Purchase.PurchaseReturnSummaryView', 'PurchaseReturnSummaryView', 4, 1, 0, 0, 0, 0),
('Purchase Return Details', 'Purchase Return Details', N'تفاصيل إرجاع الشراء', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Purchase'), 'AshproSAASERP.Shared.Pages.Reports.Purchase.PurchaseReturnDetailsView', 'PurchaseReturnDetailsView', 5, 1, 0, 0, 0, 0);

-- Inventory Reports
INSERT INTO MenuDetails (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder) VALUES 
('Stock Report', 'Stock Report', N'تقرير المخزون', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Inventory'), 'AshproSAASERP.Shared.Pages.Reports.Inventory.StockReportView', 'StockReportView', 1, 1, 0, 0, 0, 0),
('Stock Transfer Report', 'Stock Transfer Report', N'تقرير نقل المخزون', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Inventory'), 'AshproSAASERP.Shared.Pages.Reports.Inventory.StockTransferReportView', 'StockTransferReportView', 2, 1, 0, 0, 0, 0),
('Item Transaction History Report', 'Item Transaction History Report', N'تقرير سجل حركات الأصناف', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Inventory'), 'AshproSAASERP.Shared.Pages.Reports.Inventory.ItemTransactionHistoryReportView', 'ItemTransactionHistoryReportView', 3, 1, 0, 0, 0, 0),
('Stock Adjustment Report', 'Stock Adjustment Report', N'تقرير تسوية المخزون', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Inventory'), 'AshproSAASERP.Shared.Pages.Reports.Inventory.StockAdjustmentView', 'StockAdjustmentView', 4, 1, 0, 0, 0, 0);

-- Accounts Reports
INSERT INTO MenuDetails (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder) VALUES 
('Journal Entry Report', 'Journal Entry Report', N'تقرير قيود اليومية', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Accounts'), 'AshproSAASERP.Shared.Pages.Reports.Accounts.JournalEntryReportView', 'JournalEntryReportView', 1, 1, 0, 0, 0, 0),
('Trial Balance Report', 'Trial Balance Report', N'تقرير ميزان المراجعة', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Accounts'), 'AshproSAASERP.Shared.Pages.Reports.Accounts.TrialBalanceView', 'TrialBalanceView', 2, 1, 0, 0, 0, 0),
('Expense Report', 'Expense Report', N'تقرير المصروفات', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Accounts'), 'AshproSAASERP.Shared.Pages.Reports.Accounts.ExpenseReportView', 'ExpenseReportView', 3, 1, 0, 0, 0, 0),
('Profit And Loss Report', 'Profit And Loss Report', N'تقرير الأرباح والخسائر', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Accounts'), 'AshproSAASERP.Shared.Pages.Reports.Accounts.ProfitAndLossReportView', 'ProfitAndLossReportView', 4, 1, 0, 0, 0, 0),
('Balance Sheet Report', 'Balance Sheet Report', N'تقرير الميزانية العمومية', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Accounts'), 'AshproSAASERP.Shared.Pages.Reports.Accounts.BalanceSheetReportView', 'BalanceSheetReportView', 5, 1, 0, 0, 0, 0),
('Chart Of Account Report', 'Chart Of Account Report', N'تقرير دليل الحسابات', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Accounts'), 'AshproSAASERP.Shared.Pages.Reports.Accounts.ChartOfAccountReportView', 'ChartOfAccountReportView', 6, 1, 0, 0, 0, 0),
('LedgerReport', 'Ledger Report', N'تقرير دفتر الأستاذ', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Accounts'), 'AshproSAASERP.Shared.Pages.Reports.Accounts.LedgerReportView', 'LedgerReportView', 7, 1, 0, 0, 0, 0);

-- Enquiry Register Report
INSERT INTO MenuDetails (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder) VALUES 
('Enquiry Registration Report', 'Enquiry Registration Report', N'تقرير تسجيل الاستفسارات', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Enquiry'), 'AshproSAASERP.Shared.Pages.Reports.Sales.EnquiryRegistrationReportView', 'EnquiryRegistrationReportView', 1, 1, 0, 0, 0, 0);

-- Others Reports
INSERT INTO MenuDetails (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder) VALUES 
('PendingZatca', 'Zatca Report', N'تقرير الزكاة والضريبة (زاتكا)', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Sales'), 'AshproSAASERP.Shared.Pages.Reports.Transaction.ZatcaPendingView', 'ZatcaPendingView', 1, 1, 0, 0, 0, 0);


/* =========================
   SETTINGS ITEMS
   ========================= */
INSERT INTO MenuDetails (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder) VALUES 
('ManageUser', 'Manage User', N'إدارة المستخدمين', (SELECT Id FROM MenuDetails WHERE Name='Settings'), 'AshproSAASERP.Shared.Pages.Users.UserView', 'UserView', 1, 1, 0, 0, 0, 0),
('PrintDesignTitle', 'Print Design', N'تصميم الطباعة', (SELECT Id FROM MenuDetails WHERE Name='Settings'), 'AshproSAASERP.Shared.Pages.Others.PrintDesignerView', 'PrintDesignerView', 2, 1, 0, 0, 0, 0),
('Barcode Design', 'Barcode Design', N'تصميم الباركود', (SELECT Id FROM MenuDetails WHERE Name='Settings'), 'AshproSAASERP.Shared.Pages.Others.BarcodeDesignerView', 'BarcodeDesignerView', 3, 1, 0, 0, 0, 0),
('CompanyDetails', 'Company Details', N'بيانات الشركة', (SELECT Id FROM MenuDetails WHERE Name='Settings'), 'AshproSAASERP.Shared.Pages.Settings.CompanyDetailsView', 'CompanyDetailsView', 4, 1, 0, 0, 0, 0),
('ZATCA CSR', 'ZATCA CSR', N'زاتكا CSR', (SELECT Id FROM MenuDetails WHERE Name='Settings'), 'AshproSAASERP.Shared.Pages.Settings.ZatcaCSRView', 'ZatcaCSRView', 5, 1, 0, 0, 0, 0),
('User Previlege', 'User Previlege', N'صلاحيات المستخدمين', (SELECT Id FROM MenuDetails WHERE Name='Settings'), 'AshproSAASERP.Shared.Pages.Users.UserPrevilageView', 'UserPrevilageView', 6, 1, 0, 0, 0, 0),
('ZATCA Pending', 'ZATCA Pending Invoices', N'فواتير زاتكا المعلقة', (SELECT Id FROM MenuDetails WHERE Name='Settings'), 'AshproSAASERP.Shared.Pages.Settings.ZatcaPendingUploadView', 'ZatcaPendingUploadView', 7, 1, 0, 0, 0, 0),
('ProductImport', 'Product Import', N'استيراد المنتجات', (SELECT Id FROM MenuDetails WHERE Name='Settings'), 'AshproSAASERP.Shared.Pages.Settings.ProductImportView', 'ProductImportView', 8, 1, 0, 0, 0, 0),
('ProductExport', 'Product Export', N'تصدير المنتجات', (SELECT Id FROM MenuDetails WHERE Name='Settings'), 'AshproSAASERP.Shared.Pages.Settings.ProductExportView', 'ProductExportView', 9, 1, 0, 0, 0, 0),
('PrintingBarcode', 'Printing Barcode', N'طباعة الباركود', (SELECT Id FROM MenuDetails WHERE Name='Settings'), 'AshproSAASERP.Shared.Pages.Settings.PrintingBarcodeView', 'PrintingBarcodeView', 10, 1, 0, 0, 0, 0);

End
";
    }
    private string GetMenuVisibilitySeedSql()
    {
        return @"
IF EXISTS (SELECT 1 FROM MenuDetails)
BEGIN
IF NOT EXISTS (SELECT 1 FROM MenuVisibility)
BEGIN
    INSERT INTO MenuVisibility (MenuDetailId, IsVisible,LastModified)
    SELECT Id, 1,GetDate() FROM MenuDetails;
END
END
";
    }

}
