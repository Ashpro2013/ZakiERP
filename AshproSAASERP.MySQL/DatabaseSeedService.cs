using AshproSAASERP.Interface;
using AshproSAASERP.Interface.Others;
using AshproSAASERP.Models.Company;
using AshproSAASERP.Models.Masters;
using AshproSAASERP.Models.User;
using Microsoft.EntityFrameworkCore;

namespace AshproSAASERP.MySQL;

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
                        UserName = exlcompany!.UserName,
                        PassWord = exlcompany.PassWord,
                        FK_BranchId = iBranchId,
                        PrevilageId = 1
                    };
                    var companyDtls = new CompanyDetails
                    {
                        Name = exlcompany.CompanyName,
                        Contact = exlcompany.ContactNo,
                        Email = exlcompany.Email,
                        FK_BranchId = iBranchId
                    };
                    context.CompanyDetails.Add(companyDtls);
                    context.UserDetails.Add(user);
                    var fyear = new FinancialYear
                    {
                        FromDate = exlcompany.SubscriptionStartedAt,
                        ToDate = exlcompany.SubscriptionEndingDate,
                        IsClosed = false,
                        Name = exlcompany.SubscriptionStartedAt!.Value.Year.ToString(),
                        RefNo = 1
                    };

                    context.FinancialYears.Add(fyear);
                    context.UnitMasters.Add(new UnitMaster { Name = "PCs", Code = "PCs", Remarks = "Default Unit" });
                    var cat = new CategoryMaster { Name = "General", Code = "Gen", Remarks = "Default Category" };
                    context.CategoryMasters.Add(cat);
                    var country = new CountryMaster { Name = "Saudi Arabia", Code = "SA" };
                    context.CountryMasters.Add(country);
                    await context.SaveChangesAsync();
                    context.SubCategoryMasters.Add(new SubCategoryMaster { Name = "General", Code = "Gen", Remarks = "Default SubCategory", FK_CategoryId = cat.CategoryId });
                    context.SalesManMasters.Add(new SalesManMaster { Name = "SM", Code = "SM", Remarks = "Default Salesman", Designation = "Salesman" });
                    context.StoreMasters.Add(new StoreMaster { Name = "Main Store", Code = "Main", IsDefaultStore = true, FK_BranchId = iBranchId });
                    context.VatMasters.Add(new VatMaster { Name = "VAT 15%", Code = "VAT15", Percentage = 15, IsDefault = true });
                    context.CurrencyMasters.Add(new CurrencyMaster { Name = "Saudi Riyal", Code = "SAR", Rate = 1 });
                    context.CustomerGroupMasters.Add(new CustomerGroupMaster { Name = "General", Code = "Gen", Remarks = "Default Customer Group" });
                    context.CustomerLocationMasters.Add(new CustomerLocationMaster { Name = "General", Code = "Gen", Remarks = "Default Customer Location" });
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

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding database: {ex.Message}");
            }
        }
    }
    private string GetAcMstSql()
    {
        return @"INSERT INTO AccountMaster
    (Code, Name, NameArabic, FK_AccountHeadId, FK_AcGroupId,
     Opening, DbCr, FK_AcTypeId, FK_AccountSubGroupId, Flag, IsReserved)
SELECT
    t.Code,
    t.Name,
    t.NameArabic,
    t.FK_AccountHeadId,
    t.FK_AcGroupId,
    t.Opening,
    t.DbCr,
    t.FK_AcTypeId,
    t.FK_AccountSubGroupId,
    t.Flag,
    t.IsReserved
FROM (
    SELECT '110001010001' AS Code,'Cash in Hand' AS Name,'نقد في اليد' AS NameArabic,1 AS FK_AccountHeadId,1 AS FK_AcGroupId,0 AS Opening,'Debit' AS DbCr,1 AS FK_AcTypeId,1 AS FK_AccountSubGroupId,'Acc' AS Flag,1 AS IsReserved
    UNION ALL
    SELECT '110001020001','Bank Account 1','حساب بنكي 1',1,1,0,'Debit',1,2,'Acc',1
    UNION ALL
    SELECT '110001020002','Bank Account 2','حساب بنكي 2',1,1,0,'Debit',1,2,'Acc',1
    UNION ALL
    SELECT '110002010001','Customer – Local','عميل - محلي',1,2,0,'Debit',1,3,'Acc',1
    UNION ALL
    SELECT '120001010001','Office Building','مبنى المكاتب',2,3,0,'Debit',1,4,'Acc',1
    UNION ALL
    SELECT '120001020001','Delivery Vehicle','سيارة توصيل',2,3,0,'Debit',1,5,'Acc',1
    UNION ALL
    SELECT '120001030001','Office Equipment','معدات مكتبية',2,3,0,'Debit',1,6,'Acc',1
    UNION ALL
    SELECT '210001010001','Supplier – Local','مورد - محلي',3,4,0,'Credit',2,7,'Acc',1
    UNION ALL
    SELECT '210002010001','VAT Output @15%','ضريبة المخرجات @15%',3,5,0,'Credit',2,8,'Acc',1
    UNION ALL
    SELECT '210002020001','VAT Payable to ZATCA','ضريبة مستحقة للزكاة',3,5,0,'Credit',2,9,'Acc',1
    UNION ALL
    SELECT '220001010001','Long-Term Loan','قرض طويل الأجل',4,6,0,'Credit',2,10,'Acc',1
    UNION ALL
    SELECT '310001010001','Sales','المبيعات',5,7,0,'Credit',3,11,'Acc',1
    UNION ALL
    SELECT '310002010001','Discount Received','خصم مكتسب',5,8,0,'Credit',3,12,'Acc',1
    UNION ALL
    SELECT '310002020001','Interest Income','دخل الفوائد',5,8,0,'Credit',3,13,'Acc',1
    UNION ALL
    SELECT '410001010001','Office Rent','إيجار المكتب',6,9,0,'Debit',4,14,'Acc',1
    UNION ALL
    SELECT '410001020001','Staff Salary','رواتب الموظفين',6,9,0,'Debit',4,15,'Acc',1
    UNION ALL
    SELECT '410002010001','Electricity Expense','مصاريف الكهرباء',6,10,0,'Debit',4,16,'Acc',1
    UNION ALL
    SELECT '410002020001','Water Expense','مصاريف المياه',6,10,0,'Debit',4,17,'Acc',1
    UNION ALL
    SELECT '410003010001','Delivery Expense','مصاريف التوصيل',6,11,0,'Debit',4,18,'Acc',1
    UNION ALL
    SELECT '410004010001','Depreciation – Vehicle','إهلاك - مركبة',6,12,0,'Debit',4,19,'Acc',1
    UNION ALL
    SELECT '410005010001','Bank Charges','رسوم بنكية',6,13,0,'Debit',4,20,'Acc',1
    UNION ALL
    SELECT '330001010001','Opening Balance Equity','رصيد الافتتاحي',7,14,0,'Credit',2,21,'Acc',1
    UNION ALL
    SELECT '420001010001','Purchase Account','حساب المشتريات',8,15,0,'Debit',4,22,'Acc',1
    UNION ALL
    SELECT '110003010001','VAT Input @15%','ضريبة المدخلات @15%',1,16,0,'Debit',1,23,'Acc',1
    UNION ALL
    SELECT '110004010001','Inventory Asset','أصل المخزون',1,17,0,'Debit',1,24,'Acc',1
) t
WHERE NOT EXISTS (SELECT 1 FROM AccountMaster);";
    }
    private string GetAccSubGrpSql()
    {
        return @"INSERT INTO AccountSubGroup
           (FK_AcGroupId, Code, Name, NameArabic, Remarks, FK_AcHeadId, IsReserved)
SELECT
    t.FK_AcGroupId,
    t.Code,
    t.Name,
    t.NameArabic,
    t.Remarks,
    t.FK_AcHeadId,
    t.IsReserved
FROM (
    SELECT 1 AS FK_AcGroupId,'11000101' AS Code,'Cash' AS Name,'نقد' AS NameArabic,NULL AS Remarks,1 AS FK_AcHeadId,1 AS IsReserved
    UNION ALL
    SELECT 1,'11000102','Bank','بنك',NULL,1,1
    UNION ALL
    SELECT 2,'11000201','Sundry Debtors','مدينون متنوعون',NULL,1,1
    UNION ALL
    SELECT 3,'12000101','Building','مبنى',NULL,2,1
    UNION ALL
    SELECT 3,'12000102','Vehicles','مركبات',NULL,2,1
    UNION ALL
    SELECT 3,'12000103','Equipment','معدات',NULL,2,1
    UNION ALL
    SELECT 4,'21000101','Sundry Creditors','دائنون متنوعون',NULL,3,1
    UNION ALL
    SELECT 5,'21000201','VAT Output','مخرجات الضريبة',NULL,3,1
    UNION ALL
    SELECT 5,'21000202','VAT Payable','ضريبة مستحقة',NULL,3,1
    UNION ALL
    SELECT 6,'22000101','Bank Loan','قرض بنكي',NULL,4,1
    UNION ALL
    SELECT 7,'31000101','Local Sales','مبيعات محلية',NULL,5,1
    UNION ALL
    SELECT 8,'31000201','Discount','خصم',NULL,5,1
    UNION ALL
    SELECT 8,'31000202','Interest','فائدة',NULL,5,1
    UNION ALL
    SELECT 9,'41000101','Rent','إيجار',NULL,6,1
    UNION ALL
    SELECT 9,'41000102','Salary','راتب',NULL,6,1
    UNION ALL
    SELECT 10,'41000201','Electricity','كهرباء',NULL,6,1
    UNION ALL
    SELECT 10,'41000202','Water','ماء',NULL,6,1
    UNION ALL
    SELECT 11,'41000301','Transport','نقل',NULL,6,1
    UNION ALL
    SELECT 12,'41000401','Vehicles','مركبات',NULL,6,1
    UNION ALL
    SELECT 13,'41000501','Bank Charges','رسوم بنكية',NULL,6,1
    UNION ALL
    SELECT 14,'33000101','Owners Equity','حقوق الملكية',NULL,7,1
    UNION ALL
    SELECT 15,'42000101','Local Purchases','مشتريات محلية',NULL,8,1
    UNION ALL
    SELECT 16,'11000301','VAT Input','مدخلات الضريبة',NULL,1,1
    UNION ALL
    SELECT 17,'11000401','Stock','مخزون',NULL,1,1
) t
WHERE NOT EXISTS (SELECT 1 FROM AccountSubGroup);";
    }
    private string GetAccGrpSql()
    {
        return @"INSERT INTO AccountGroup
           (Code, Name, NameArabic, Remarks, FK_AcHeadId, IsReserved)
SELECT
    t.Code,
    t.Name,
    t.NameArabic,
    t.Remarks,
    t.FK_AcHeadId,
    t.IsReserved
FROM (
    SELECT '110001' AS Code,'Cash & Bank' AS Name,'النقد والبنك' AS NameArabic,NULL AS Remarks,1 AS FK_AcHeadId,1 AS IsReserved
    UNION ALL
    SELECT '110002','Accounts Receivable','حسابات القبض',NULL,1,1
    UNION ALL
    SELECT '120001','Property Plant & Equip','الممتلكات والمنشآت والمعدات',NULL,2,1
    UNION ALL
    SELECT '210001','Accounts Payable','حسابات الدفع',NULL,3,1
    UNION ALL
    SELECT '210002','VAT Payable','ضريبة القيمة المضافة المستحقة',NULL,3,1
    UNION ALL
    SELECT '220001','Loans','قروض',NULL,4,1
    UNION ALL
    SELECT '310001','Sales','المبيعات',NULL,5,1
    UNION ALL
    SELECT '310002','Other Income','إيرادات أخرى',NULL,5,1
    UNION ALL
    SELECT '410001','Administrative','إداري',NULL,6,1
    UNION ALL
    SELECT '410002','Utilities','خدمات',NULL,6,1
    UNION ALL
    SELECT '410003','Selling Expenses','مصاريف البيع',NULL,6,1
    UNION ALL
    SELECT '410004','Depreciation','إهلاك',NULL,6,1
    UNION ALL
    SELECT '410005','Financial Expenses','مصاريف مالية',NULL,6,1
    UNION ALL
    SELECT '330001','Capital Accounts','حسابات رأس المال',NULL,7,1
    UNION ALL
    SELECT '420001','Purchases','المشتريات',NULL,8,1
    UNION ALL
    SELECT '110003','VAT Receivable','ضريبة القيمة المضافة المستحقة القبض',NULL,1,1
    UNION ALL
    SELECT '110004','Inventory Assets','أصول المخزون',NULL,1,1
) t
WHERE NOT EXISTS (SELECT 1 FROM AccountGroup);";
    }
    private string GetAccHdSQL()
    {
        return @"INSERT INTO AccountHeadMaster
               (Code, Name, NameArabic, FK_AcTypeId, IsReserved)
SELECT
    t.Code,
    t.Name,
    t.NameArabic,
    t.FK_AcTypeId,
    t.IsReserved
FROM (
    SELECT '1100' AS Code,'Current Assets' AS Name,'الأصول المتداولة' AS NameArabic,1 AS FK_AcTypeId,1 AS IsReserved
    UNION ALL
    SELECT '1200','Fixed Assets','الأصول الثابتة',1,1
    UNION ALL
    SELECT '2100','Current Liabilities','الالتزامات المتداولة',2,1
    UNION ALL
    SELECT '2200','Non-Current Liabilities','الالتزامات غير المتداولة',2,1
    UNION ALL
    SELECT '3100','Revenue','إيرادات',3,1
    UNION ALL
    SELECT '4100','Operating Expenses','مصاريف تشغيلية',4,1
    UNION ALL
    SELECT '3300','Equity','حقوق الملكية',2,1
    UNION ALL
    SELECT '4200','Cost of Sales','تكلفة المبيعات',4,1
) t
WHERE NOT EXISTS (SELECT 1 FROM AccountHeadMaster);";
    }
    private string GetAccountTypeSQL()
    {
        return @"
INSERT INTO AccountType
           (AcTypeName)
SELECT t.AcTypeName
FROM (
    SELECT 'Asset' AS AcTypeName
    UNION ALL
    SELECT 'Liability'
    UNION ALL
    SELECT 'Income'
    UNION ALL
    SELECT 'Expense'
) t
WHERE NOT EXISTS (SELECT 1 FROM AccountType);";
    }
    private string GetErpTypeSQL()
    {
        return @"
INSERT INTO ERPType (TypeName, Description, IsCustom, CreatedDate)
SELECT
    t.TypeName,
    t.Description,
    t.IsCustom,
    t.CreatedDate
FROM (
    SELECT 'Trading' AS TypeName, 'Sales-focused business with inventory and customer management' AS Description, 0 AS IsCustom, '2025-12-24 11:20:10.377' AS CreatedDate
    UNION ALL
    SELECT 'Manufacturing', 'Production-based business with full procurement, manufacturing and sales', 0, '2025-12-24 11:20:10.377'
    UNION ALL
    SELECT 'Transportation', 'Logistics and delivery services with minimal inventory', 0, '2025-12-24 11:20:10.377'
    UNION ALL
    SELECT 'Retail', 'Point-of-sale and customer-focused retail operations', 0, '2025-12-24 11:20:10.377'
    UNION ALL
    SELECT 'Services', 'Service-based business with project management and minimal inventory', 0, '2025-12-24 11:20:10.377'
    UNION ALL
    SELECT 'Custom', 'Fully customizable menu configuration - select specific menus to show', 1, '2025-12-24 11:20:10.377'
) t
WHERE NOT EXISTS (SELECT 1 FROM ERPType);
";
    }
    private string GetSettingsSeed()
    {
        return @"
/* =========================
   PERMANENT SETTINGS SEED
   ========================= */
INSERT INTO PermanentSettings (SettingKey, SettingValue)
SELECT *
FROM (
    SELECT 'DefaultCashAccount' AS SettingKey, NULL AS SettingValue
    UNION ALL SELECT 'DefaultBankAccount', NULL

    -- Item Code
    UNION ALL SELECT 'AutoItemCode','0'
    UNION ALL SELECT 'ItemCodePrefix','IT'
    UNION ALL SELECT 'ItemCodeStartNumber','1'
    UNION ALL SELECT 'ItemCodeZeroPadding','3'
    UNION ALL SELECT 'CategoryWiseItemCode','0'
    UNION ALL SELECT 'SubCategoryWiseItemCode','0'
    UNION ALL SELECT 'CodeSeparator','-'

    -- Customer Code
    UNION ALL SELECT 'AutoCustomerCode','0'
    UNION ALL SELECT 'CustomerCodePrefix','CUS'
    UNION ALL SELECT 'CustomerCodeStartNumber','1'
    UNION ALL SELECT 'CustomerCodeZeroPadding','3'

    -- Supplier Code
    UNION ALL SELECT 'AutoSupplierCode','0'
    UNION ALL SELECT 'SupplierCodePrefix','SUP'
    UNION ALL SELECT 'SupplierCodeStartNumber','1'
    UNION ALL SELECT 'SupplierCodeZeroPadding','3'

    -- Category Code
    UNION ALL SELECT 'AutoCategoryCode','0'
    UNION ALL SELECT 'CategoryCodePrefix','CAT'
    UNION ALL SELECT 'CategoryCodeStartNumber','1'
    UNION ALL SELECT 'CategoryCodeZeroPadding','3'

    -- SubCategory Code
    UNION ALL SELECT 'AutoSubCategoryCode','0'
    UNION ALL SELECT 'SubCategoryCodePrefix','SUB'
    UNION ALL SELECT 'SubCategoryCodeStartNumber','1'
    UNION ALL SELECT 'SubCategoryCodeZeroPadding','3'
    UNION ALL SELECT 'CategoryWiseSubCategoryCode','0'

    -- Quotation
    UNION ALL SELECT 'AutoQuotationNo','1'
    UNION ALL SELECT 'QuotationNoPrefix','QT'
    UNION ALL SELECT 'QuotationNoStartNumber','1'
    UNION ALL SELECT 'QuotationNoZeroPadding','4'

    -- Sales Order
    UNION ALL SELECT 'AutoSalesOrderNo','1'
    UNION ALL SELECT 'SalesOrderNoPrefix','SO'
    UNION ALL SELECT 'SalesOrderNoStartNumber','1'
    UNION ALL SELECT 'SalesOrderNoZeroPadding','4'

    -- Delivery Note
    UNION ALL SELECT 'AutoDeliveryNoteNo','1'
    UNION ALL SELECT 'DeliveryNoteNoPrefix','DN'
    UNION ALL SELECT 'DeliveryNoteNoStartNumber','1'
    UNION ALL SELECT 'DeliveryNoteNoZeroPadding','4'
    UNION ALL SELECT 'DeliveryNoteStockAffect','0'

    -- Purchase Order
    UNION ALL SELECT 'AutoPurchaseOrderNo','1'
    UNION ALL SELECT 'PurchaseOrderNoPrefix','PO'
    UNION ALL SELECT 'PurchaseOrderNoStartNumber','1'
    UNION ALL SELECT 'PurchaseOrderNoZeroPadding','4'

    -- Purchase
    UNION ALL SELECT 'AutoPurchaseNo','1'
    UNION ALL SELECT 'PurchaseNoPrefix','P'
    UNION ALL SELECT 'PurchaseNoStartNumber','1'
    UNION ALL SELECT 'PurchaseNoZeroPadding','4'

    -- Defaults
    UNION ALL SELECT 'DefaultFocusOn','ItemCode'
    UNION ALL SELECT 'DefaultBillType','Cash'
    UNION ALL SELECT 'AllowNegativeStock','0'
    UNION ALL SELECT 'ShowCostInSales','0'
    UNION ALL SELECT 'RequireBarcode','0'
    UNION ALL SELECT 'SalesInvoice_EnableUpdate','0'
    UNION ALL SELECT 'SalesInvoice_EnableCancel','0'
    UNION ALL SELECT 'SalesReturn_EnableUpdate','0'
    UNION ALL SELECT 'SalesReturn_EnableCancel','0'

    -- Image Settings
    UNION ALL SELECT 'ImageStorageMode','0'
    UNION ALL SELECT 'ImageClientFolder','DefaultClient'
    UNION ALL SELECT 'ImageLocalBasePath','\\\\192.168.1.11\\D\\Images'
    UNION ALL SELECT 'ImageAwsRegion','us-east-1'
    UNION ALL SELECT 'ImageAwsBucket','erp-images'
    UNION ALL SELECT 'ImageAwsAccessKey',''
    UNION ALL SELECT 'ImageAwsSecretKey',''

    -- Journal
    UNION ALL SELECT 'AutoJournalEntryNo','1'
    UNION ALL SELECT 'JournalEntryNoPrefix','JV'
    UNION ALL SELECT 'JournalEntryNoStartNumber','1'
    UNION ALL SELECT 'JournalEntryNoZeroPadding','4'

    -- Expense
    UNION ALL SELECT 'AutoExpenseEntryNo','1'
    UNION ALL SELECT 'ExpenseEntryNoPrefix','EXP'
    UNION ALL SELECT 'ExpenseEntryNoStartNumber','1'
    UNION ALL SELECT 'ExpenseEntryNoZeroPadding','4'

    -- Customer Receipt
    UNION ALL SELECT 'AutoCustomerReceiptNo','1'
    UNION ALL SELECT 'CustomerReceiptNoPrefix','CR'
    UNION ALL SELECT 'CustomerReceiptNoStartNumber','1'
    UNION ALL SELECT 'CustomerReceiptNoZeroPadding','6'

    -- Stock Adjustment
    UNION ALL SELECT 'AutoStockAdjustmentNo','0'
    UNION ALL SELECT 'StockAdjustmentNoPrefix','SA'
    UNION ALL SELECT 'StockAdjustmentNoStartNumber','1'
    UNION ALL SELECT 'StockAdjustmentNoZeroPadding','4'

    -- Material Transfer In
    UNION ALL SELECT 'AutoMaterialTransferInNo','0'
    UNION ALL SELECT 'MaterialTransferInNoPrefix','MTI'
    UNION ALL SELECT 'MaterialTransferInNoStartNumber','1'
    UNION ALL SELECT 'MaterialTransferInNoZeroPadding','4'

    -- Material Transfer Out
    UNION ALL SELECT 'AutoMaterialTransferOutNo','0'
    UNION ALL SELECT 'MaterialTransferOutNoPrefix','MTO'
    UNION ALL SELECT 'MaterialTransferOutNoStartNumber','1'
    UNION ALL SELECT 'MaterialTransferOutNoZeroPadding','4'

    -- Job Order
    UNION ALL SELECT 'AutoJobOrderNo','0'
    UNION ALL SELECT 'JobOrderNoPrefix','JO'
    UNION ALL SELECT 'JobOrderNoStartNumber','1'
    UNION ALL SELECT 'JobOrderNoZeroPadding','4'

    -- Job Card
    UNION ALL SELECT 'AutoJobCardNo','0'
    UNION ALL SELECT 'JobCardNoPrefix','JC'
    UNION ALL SELECT 'JobCardNoStartNumber','1'
    UNION ALL SELECT 'JobCardNoZeroPadding','4'

    -- Material Transfer Request
    UNION ALL SELECT 'AutoMaterialTransferRequestNo','0'
    UNION ALL SELECT 'MaterialTransferRequestNoPrefix','MTR'
    UNION ALL SELECT 'MaterialTransferRequestNoStartNumber','1'
    UNION ALL SELECT 'MaterialTransferRequestNoZeroPadding','4'
) t
WHERE NOT EXISTS (
    SELECT 1 FROM PermanentSettings p WHERE p.SettingKey = t.SettingKey
);
";
    }
    private string GetMenuSeedSql()
    {
        return @"

    /* =========================
       SUPER PARENTS (FAST INSERT)
       ========================= */
    INSERT INTO MenuDetails 
    (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder)
    SELECT * FROM (
        SELECT 'Masters','Masters','البيانات الأساسية',NULL,NULL,'AddCircle',1,1,0,0,0,0
        UNION ALL SELECT 'Transactions','Transactions','المعاملات',NULL,NULL,'ShoppingCart',2,1,0,0,0,0
        UNION ALL SELECT 'Accounting','Accounting','المحاسبة',NULL,NULL,'AccountBalance',3,1,0,0,0,0
        UNION ALL SELECT 'Reports','Reports','التقارير',NULL,NULL,'Assessment',4,1,0,0,0,0
        UNION ALL SELECT 'Settings','Settings','الإعدادات',NULL,NULL,'Settings',5,1,0,0,0,0
    ) AS t(Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder)
    WHERE NOT EXISTS (
        SELECT 1 FROM MenuDetails m WHERE m.Name = t.Name
    );


    /* =========================
       GET MASTER ID ONCE (OPTIMIZED)
       ========================= */
    SET @MasterId = (SELECT Id FROM MenuDetails WHERE Name = 'Masters' LIMIT 1);


    /* =========================
       MASTERS SUB-GROUPS
       ========================= */
    INSERT INTO MenuDetails
    (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder)
    SELECT * FROM (
        SELECT 'Inventory','Inventory','المخزون',@MasterId,NULL,'Inventory',1,1,0,0,0,0
        UNION ALL SELECT 'Accounts','Accounts','الحسابات',@MasterId,NULL,'AccountBalance',2,1,0,0,0,0
        UNION ALL SELECT 'GeneralMasters','General Masters','الإعدادات العامة',@MasterId,NULL,'SettingsApplications',3,1,0,0,0,0
        UNION ALL SELECT 'Parties','Parties','الأطراف',@MasterId,NULL,'People',4,1,0,0,0,0
    ) AS t(Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder)
    WHERE NOT EXISTS (
        SELECT 1 FROM MenuDetails m WHERE m.Name = t.Name AND m.ParentId <=> t.ParentId
    );

    

/* =========================
   MASTERS -> INVENTORY ITEMS
   ========================= */

    SET @InventoryId = (SELECT Id FROM MenuDetails WHERE Name = 'Inventory' LIMIT 1);

    INSERT INTO MenuDetails
    (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder)
    SELECT * FROM (
        SELECT 'Unit Master','Unit Master','إدارة الوحدات',@InventoryId,'AshproSAASERP.Shared.Pages.Masters.UnitMasterView','UnitMasterView',1,1,0,0,0,0
        UNION ALL SELECT 'CategoryMaster','Category Master','إدارة الفئات',@InventoryId,'AshproSAASERP.Shared.Pages.Masters.CategoryMasterView','CategoryMasterView',2,1,0,0,0,0
        UNION ALL SELECT 'SubCategoryMaster','Sub Category','إدارة الفئات الفرعية',@InventoryId,'AshproSAASERP.Shared.Pages.Masters.SubCategoryMasterView','SubCategoryMasterView',3,1,0,0,0,0
        UNION ALL SELECT 'BrandMaster','Brand Master','إدارة العلامات التجارية',@InventoryId,'AshproSAASERP.Shared.Pages.Masters.BrandMastersView','BrandMastersView',4,1,0,0,0,0
        UNION ALL SELECT 'ItemMaster','Item Master','إدارة الأصناف',@InventoryId,'AshproSAASERP.Shared.Pages.Masters.ItemMasterView','ItemMasterView',5,1,0,0,0,0
        UNION ALL SELECT 'StoreMaster','Store Master','إدارة المخازن',@InventoryId,'AshproSAASERP.Shared.Pages.Masters.StoreMasterView','StoreMasterView',6,1,0,0,0,0
        UNION ALL SELECT 'OpeningStockEntry','Opening Stock Entry','إدخال الرصيد الافتتاحي',@InventoryId,'AshproSAASERP.Shared.Pages.Masters.OpeningStockEntryView','OpeningStockEntryView',7,1,0,0,0,0
    ) AS t(Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder)
    WHERE NOT EXISTS (
        SELECT 1 FROM MenuDetails m WHERE m.Name = t.Name AND m.ParentId <=> t.ParentId
    );
/* =========================
   MASTERS -> ACCOUNTS ITEMS
   ========================= */

    SET @AccountsId = (SELECT Id FROM MenuDetails WHERE Name = 'Accounts' LIMIT 1);

    INSERT INTO MenuDetails
    (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder)
    SELECT * FROM (
        SELECT 'AccountHeadTitle','Account Head','رؤوس الحسابات',@AccountsId,'AshproSAASERP.Shared.Pages.Masters.AccountHeadView','AccountHeadView',1,1,0,0,0,0
        UNION ALL SELECT 'AccountGroup','Account Group','مجموعات الحسابات',@AccountsId,'AshproSAASERP.Shared.Pages.Masters.AccountGroupView','AccountGroupView',2,1,0,0,0,0
        UNION ALL SELECT 'AccountSubGroup','Account SubGroup','المجموعات الفرعية للحسابات',@AccountsId,'AshproSAASERP.Shared.Pages.Masters.AccountSubGroupView','AccountSubGroupView',3,1,0,0,0,0
        UNION ALL SELECT 'AccountsPage','Accounts','الحسابات',@AccountsId,'AshproSAASERP.Shared.Pages.Masters.AccountsView','AccountsView',4,1,0,0,0,0
    ) AS t(Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder)
    WHERE NOT EXISTS (
        SELECT 1 FROM MenuDetails m WHERE m.Name = t.Name AND m.ParentId <=> t.ParentId
    );


/* =========================
   MASTERS -> GENERAL MASTERS ITEMS
   ========================= */

    SET @GeneralMastersId = (SELECT Id FROM MenuDetails WHERE Name = 'GeneralMasters' LIMIT 1);

    INSERT INTO MenuDetails
    (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder)
    SELECT * FROM (
        SELECT 'BranchTitle','Branch','الفروع',@GeneralMastersId,'AshproSAASERP.Shared.Pages.Masters.BranchView','BranchView',1,1,0,0,0,0
        UNION ALL SELECT 'FinancilaYearTitle','Financial Year','السنة المالية',@GeneralMastersId,'AshproSAASERP.Shared.Pages.Masters.FinancialYearView','FinancialYearView',2,1,0,0,0,0
        UNION ALL SELECT 'SalesManMaster','Salesman Master','إدارة مندوبي المبيعات',@GeneralMastersId,'AshproSAASERP.Shared.Pages.Masters.SalesManMasterView','SalesManMasterView',3,1,0,0,0,0
        UNION ALL SELECT 'PurchaserMaster','Purchaser Master','إدارة مسؤولي المشتريات',@GeneralMastersId,'AshproSAASERP.Shared.Pages.Masters.PurchaserMasterView','PurchaserMasterView',4,1,0,0,0,0
        UNION ALL SELECT 'CountryMaster','Country Master','إدارة الدول',@GeneralMastersId,'AshproSAASERP.Shared.Pages.Masters.CountryMasterView','CountryMasterView',5,1,0,0,0,0
        UNION ALL SELECT 'VatMaster','Vat Master','إدارة ضريبة القيمة المضافة',@GeneralMastersId,'AshproSAASERP.Shared.Pages.Masters.VatMasterView','VatMasterView',6,1,0,0,0,0
        UNION ALL SELECT 'CurrencyMaster','CurrencyMaster','إدارة العملات',@GeneralMastersId,'AshproSAASERP.Shared.Pages.Masters.CurrencyMasterView','CurrencyMasterView',7,1,0,0,0,0
        UNION ALL SELECT 'ProjectMaster','Project Master','إدارة المشاريع',@GeneralMastersId,'AshproSAASERP.Shared.Pages.Masters.ProjectMasterView','ProjectMasterView',8,1,0,0,0,0
        UNION ALL SELECT 'VehicleMaster','Vehicle Master','إدارة المركبات',@GeneralMastersId,'AshproSAASERP.Shared.Pages.Masters.VehicleMasterView','VehicleMasterView',9,1,0,0,0,0
        UNION ALL SELECT 'RepairMaster','Repair Master','إدارة الصيانة',@GeneralMastersId,'AshproSAASERP.Shared.Pages.Masters.RepairMasterView','RepairMasterView',10,1,0,0,0,0
        UNION ALL SELECT 'TechnicianMaster','Technician','إدارة الفنيين',@GeneralMastersId,'AshproSAASERP.Shared.Pages.Masters.TechnicianMasterView','TechnicianMasterView',11,1,0,0,0,0
    ) AS t(Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder)
    WHERE NOT EXISTS (
        SELECT 1 FROM MenuDetails m WHERE m.Name = t.Name AND m.ParentId <=> t.ParentId
    );

/* =========================
   MASTERS -> PARTIES ITEMS
   ========================= */

    SET @PartiesId = (SELECT Id FROM MenuDetails WHERE Name = 'Parties' LIMIT 1);

    INSERT INTO MenuDetails
    (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder)
    SELECT * FROM (
        SELECT 'CustomerMaster','Customer Master','إدارة العملاء',@PartiesId,'AshproSAASERP.Shared.Pages.Masters.CustomerMasterView','CustomerMasterView',1,1,0,0,0,0
        UNION ALL SELECT 'SupplierMaster','Supplier Master','إدارة الموردين',@PartiesId,'AshproSAASERP.Shared.Pages.Masters.SupplierMasterView','SupplierMasterView',2,1,0,0,0,0
        UNION ALL SELECT 'CustomerGroupMaster','Customer Group Master','إدارة مجموعات العملاء',@PartiesId,'AshproSAASERP.Shared.Pages.Masters.CustomerGroupMasterView','CustomerGroupMasterView',3,1,0,0,0,0
        UNION ALL SELECT 'CustomerLocationMaster','Customer Location','مواقع العملاء',@PartiesId,'AshproSAASERP.Shared.Pages.Masters.CustomerLocationMasterView','CustomerLocationMasterView',4,1,0,0,0,0
        UNION ALL SELECT 'CustomerOpeningEntry','Customer Opening Entry','إدخال الرصيد الافتتاحي للعملاء',@PartiesId,'AshproSAASERP.Shared.Pages.Masters.CustomerOPEntryView','CustomerOPEntryView',5,1,0,0,0,0
        UNION ALL SELECT 'SupplierOpeningEntry','Supplier Opening Entry','إدخال الرصيد الافتتاحي للموردين',@PartiesId,'AshproSAASERP.Shared.Pages.Masters.SupplierOPEntryView','SupplierOPEntryView',6,1,0,0,0,0
    ) AS t(Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder)
    WHERE NOT EXISTS (
        SELECT 1 FROM MenuDetails m WHERE m.Name = t.Name AND m.ParentId <=> t.ParentId
    );


/* =========================
   TRANSACTIONS SUB-GROUPS
   ========================= */

    SET @TransactionsId = (SELECT Id FROM MenuDetails WHERE Name = 'Transactions' LIMIT 1);

    INSERT INTO MenuDetails
    (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder)
    SELECT * FROM (
        SELECT 'Sales','Sales','المبيعات',@TransactionsId,NULL,'ShoppingBag',1,1,0,0,0,0
        UNION ALL SELECT 'Purchase','Purchase','المشتريات',@TransactionsId,NULL,'ShoppingBasket',2,1,0,0,0,0
        UNION ALL SELECT 'Inventory_Trans','Inventory','المخزون',@TransactionsId,NULL,'FmdGood',3,1,0,0,0,0
        UNION ALL SELECT 'Others','Others','أخرى',@TransactionsId,NULL,'CardMembership',4,1,0,0,0,0
    ) AS t(Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder)
    WHERE NOT EXISTS (
        SELECT 1 FROM MenuDetails m WHERE m.Name = t.Name AND m.ParentId <=> t.ParentId
    );


/* =========================
   TRANSACTIONS ITEMS
   ========================= */

-- Sales
    SET @SalesId = (SELECT Id FROM MenuDetails WHERE Name = 'Sales' LIMIT 1);

    INSERT INTO MenuDetails
    (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder)
    SELECT * FROM (
        SELECT 'SalesQuotTitle','Sales Quotation','عرض سعر المبيعات',@SalesId,'AshproSAASERP.Shared.Pages.Transactions.Sales.SalesQuotationView','SalesQuotationView',1,1,0,0,0,0
        UNION ALL SELECT 'ProformaInvoiceTitle','Proforma Invoice','فاتورة مبدئية',@SalesId,'AshproSAASERP.Shared.Pages.Transactions.Sales.ProformaInvoiceView','ProformaInvoiceView',2,1,0,0,0,0
        UNION ALL SELECT 'SalesOrderTitle','Sales Order','أمر بيع',@SalesId,'AshproSAASERP.Shared.Pages.Transactions.Sales.SalesOrderView','SalesOrderView',3,1,0,0,0,0
        UNION ALL SELECT 'SalesInvoiceTitle','Sales Invoice','فاتورة مبيعات',@SalesId,'AshproSAASERP.Shared.Pages.Transactions.Sales.SalesInvoiceView','SalesInvoiceView',4,1,0,0,0,0
        UNION ALL SELECT 'SalesInvoiceRtnTitle','Sales Invoice Return','مرتجع فاتورة مبيعات',@SalesId,'AshproSAASERP.Shared.Pages.Transactions.Sales.SalesInvoiceRtnView','SalesInvoiceRtnView',5,1,0,0,0,0
        UNION ALL SELECT 'EnquiryRegistration','Enquiry Registration','تسجيل الاستفسارات',@SalesId,'AshproSAASERP.Shared.Pages.Transactions.Sales.EnquiryRegistrationView','EnquiryRegistrationView',6,1,0,0,0,0
        UNION ALL SELECT 'DeliveryNoteTitle','Delivery Note','إشعار تسليم',@SalesId,'AshproSAASERP.Shared.Pages.Transactions.Sales.DeliveryNoteView','DeliveryNoteView',7,1,0,0,0,0
        UNION ALL SELECT 'Delivery Note Return','Delivery Note Return','مرتجع إشعار التسليم',@SalesId,'AshproSAASERP.Shared.Pages.Transactions.Sales.DeliveryNoteRtnView','DeliveryNoteRtnView',8,1,0,0,0,0
    ) AS t(Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder)
    WHERE NOT EXISTS (
        SELECT 1 FROM MenuDetails m WHERE m.Name = t.Name AND m.ParentId <=> t.ParentId
    );

-- Purchase
    SET @PurchaseId = (SELECT Id FROM MenuDetails WHERE Name = 'Purchase' LIMIT 1);

    INSERT INTO MenuDetails
    (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder)
    SELECT * FROM (
        SELECT 'PurchaseOrderTitle','Purchase Order','أمر شراء',@PurchaseId,'AshproSAASERP.Shared.Pages.Transactions.Purchase.PurchaseOrderView','PurchaseOrderView',1,1,0,0,0,0
        UNION ALL SELECT 'PurchaseTitle','Purchase','المشتريات',@PurchaseId,'AshproSAASERP.Shared.Pages.Transactions.Purchase.PurchaseView','PurchaseView',2,1,0,0,0,0
        UNION ALL SELECT 'GoodsReceipt','Goods Receipt','استلام البضائع',@PurchaseId,'AshproSAASERP.Shared.Pages.Transactions.Purchase.GoodsReceiptView','GoodsReceiptView',3,1,0,0,0,0
        UNION ALL SELECT 'Purchase Return','Purchase Return','مرتجع مشتريات',@PurchaseId,'AshproSAASERP.Shared.Pages.Transactions.Purchase.PurchaseReturnView','PurchaseReturnView',4,1,0,0,0,0
    ) AS t(Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder)
    WHERE NOT EXISTS (
        SELECT 1 FROM MenuDetails m WHERE m.Name = t.Name AND m.ParentId <=> t.ParentId
    );
-- Inventory (Transactions)
    SET @InventoryTransId = (SELECT Id FROM MenuDetails WHERE Name = 'Inventory_Trans' LIMIT 1);

    INSERT INTO MenuDetails
    (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder)
    SELECT * FROM (
        SELECT 'Damage Stock','Damage Stock','مخزون تالف',@InventoryTransId,'AshproSAASERP.Shared.Pages.Transactions.Inventory.DamageStockView','DamageStockView',1,1,0,0,0,0
        UNION ALL SELECT 'StockAdjustment','Stock Adjustment','تسوية المخزون',@InventoryTransId,'AshproSAASERP.Shared.Pages.Transactions.Inventory.StockAdjustmentView','StockAdjustmentView',2,1,0,0,0,0
        UNION ALL SELECT 'MaterialTransferRequest','Material Transfer Request','طلب نقل المواد',@InventoryTransId,'AshproSAASERP.Shared.Pages.Transactions.Inventory.MaterialTransferRequestView','MaterialTransferRequestView',3,1,0,0,0,0
        UNION ALL SELECT 'MaterialTransferApproval','Material Transfer Approval','اعتماد نقل المواد',@InventoryTransId,'AshproSAASERP.Shared.Pages.Transactions.Inventory.MaterialTransferApprovalView','MaterialTransferApprovalView',4,1,0,0,0,0
        UNION ALL SELECT 'MaterialTransferIN','Material Transfer IN','إدخال نقل المواد',@InventoryTransId,'AshproSAASERP.Shared.Pages.Transactions.Inventory.MaterialTransferInView','MaterialTransferInView',5,1,0,0,0,0
        UNION ALL SELECT 'MaterialTransferOUT','Material Transfer OUT','إخراج نقل المواد',@InventoryTransId,'AshproSAASERP.Shared.Pages.Transactions.Inventory.MaterialTransferOutView','MaterialTransferOutView',6,1,0,0,0,0
        UNION ALL SELECT 'InternalMaterialTransfer','Internal Material Transfer','نقل المواد الداخلي',@InventoryTransId,'AshproSAASERP.Shared.Pages.Transactions.Inventory.InternalMaterialTransferView','InternalMaterialTransferView',7,1,0,0,0,0
    ) AS t(Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder)
    WHERE NOT EXISTS (
        SELECT 1 FROM MenuDetails m WHERE m.Name = t.Name AND m.ParentId <=> t.ParentId
    );

-- Others
    SET @OthersId = (SELECT Id FROM MenuDetails WHERE Name = 'Others' LIMIT 1);

    INSERT INTO MenuDetails
    (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder)
    SELECT * FROM (
        SELECT 'JobOrder','Job Order','أمر عمل',@OthersId,'AshproSAASERP.Shared.Pages.Transactions.Workshop.JobOrderView','JobOrderView',1,1,0,0,0,0
        UNION ALL SELECT 'JobCard','Job Card','بطاقة عمل',@OthersId,'AshproSAASERP.Shared.Pages.Transactions.Workshop.JobCardView','JobCardView',2,1,0,0,0,0
    ) AS t(Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder)
    WHERE NOT EXISTS (
        SELECT 1 FROM MenuDetails m WHERE m.Name = t.Name AND m.ParentId <=> t.ParentId
    );


/* =========================
   ACCOUNTING ITEMS
   ========================= */

    SET @AccountingId = (SELECT Id FROM MenuDetails WHERE Name = 'Accounting' LIMIT 1);

    INSERT INTO MenuDetails
    (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder)
    SELECT * FROM (
        SELECT 'JournalEntry','Journal Entry','قيد يومية',@AccountingId,'AshproSAASERP.Shared.Pages.Transactions.Accounts.JournalEntryView','JournalEntryView',1,1,0,0,0,0
        UNION ALL SELECT 'CustomerReceipt','Customer Receipt','سند قبض العملاء',@AccountingId,'AshproSAASERP.Shared.Pages.Transactions.Accounts.CustomerReceiptView','CustomerReceiptView',2,1,0,0,0,0
        UNION ALL SELECT 'ExpenseEntry','Expense Entry','قيد مصروفات',@AccountingId,'AshproSAASERP.Shared.Pages.Transactions.Accounts.ExpenseEntryView','ExpenseEntryView',3,1,0,0,0,0
        UNION ALL SELECT 'AccountOpeningEntry','Account Opening Entry','قيد افتتاحي للحسابات',@AccountingId,'AshproSAASERP.Shared.Pages.Transactions.Accounts.AccountOpeningEntryView','AccountOpeningEntryView',4,1,0,0,0,0
        UNION ALL SELECT 'SupplierPayment','Supplier Payment','سند دفع الموردين',@AccountingId,'AshproSAASERP.Shared.Pages.Transactions.Accounts.SupplierPaymentView','SupplierPaymentView',5,1,0,0,0,0
    ) AS t(Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder)
    WHERE NOT EXISTS (
        SELECT 1 FROM MenuDetails m WHERE m.Name = t.Name AND m.ParentId <=> t.ParentId
    );


/* =========================
   REPORTS SUB-GROUPS
   ========================= */

    SET @ReportsId = (SELECT Id FROM MenuDetails WHERE Name = 'Reports' LIMIT 1);

    INSERT INTO MenuDetails
    (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder)
    SELECT * FROM (
        SELECT 'Rpt_Sales','Sales Reports','تقارير المبيعات',@ReportsId,NULL,'ShoppingBag',1,1,0,0,0,0
        UNION ALL SELECT 'Rpt_Monitor','Monitor Reports','تقارير المراقبة',@ReportsId,NULL,'Monitor',2,1,0,0,0,0
        UNION ALL SELECT 'Rpt_Supplier','Supplier Reports','تقارير الموردين',@ReportsId,NULL,'AssignmentInd',3,1,0,0,0,0
        UNION ALL SELECT 'Rpt_Customer','Customer Reports','تقارير العملاء',@ReportsId,NULL,'Person',4,1,0,0,0,0
        UNION ALL SELECT 'Rpt_Purchase','Purchase Reports','تقارير المشتريات',@ReportsId,NULL,'ShoppingBasket',5,1,0,0,0,0
        UNION ALL SELECT 'Rpt_Inventory','Inventory Reports','تقارير المخزون',@ReportsId,NULL,'FmdGood',6,1,0,0,0,0
        UNION ALL SELECT 'Rpt_Accounts','Accounts Reports','تقارير الحسابات',@ReportsId,NULL,'AccountBox',7,1,0,0,0,0
        UNION ALL SELECT 'Rpt_Enquiry','Enquiry Reports','تقارير الاستفسارات',@ReportsId,NULL,'Assignment',8,1,0,0,0,0
    ) AS t(Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder)
    WHERE NOT EXISTS (
        SELECT 1 FROM MenuDetails m WHERE m.Name = t.Name AND m.ParentId <=> t.ParentId
    );


/* =========================
   REPORTS ITEMS
   ========================= */

-- Sales Reports
    SET @RptSalesId = (SELECT Id FROM MenuDetails WHERE Name = 'Rpt_Sales' LIMIT 1);

    INSERT INTO MenuDetails
    (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder)
    SELECT * FROM (
        SELECT 'Sales Report','Sales Report','تقرير المبيعات',@RptSalesId,'AshproSAASERP.Shared.Pages.Reports.Sales.SalesReportsView','SalesReportsView',1,1,0,0,0,0
        UNION ALL SELECT 'Sales Return Report','Sales Return Report','تقرير إرجاع المبيعات',@RptSalesId,'AshproSAASERP.Shared.Pages.Reports.Sales.SalesReturnReportView','SalesReturnReportView',12,1,0,0,0,0
        UNION ALL SELECT 'Sales Details Report','Sales Details Report','تقرير تفاصيل المبيعات',@RptSalesId,'AshproSAASERP.Shared.Pages.Reports.Sales.SalesDetailsView','SalesDetailsView',2,1,0,0,0,0
        UNION ALL SELECT 'Sales Return Details Report','Sales Return Details Report','تقرير تفاصيل مرتجعات المبيعات',@RptSalesId,'AshproSAASERP.Shared.Pages.Reports.Sales.SalesReturnDetailsReportView','SalesReturnDetailsReportView',13,1,0,0,0,0
        UNION ALL SELECT 'Customer Wise Sales Report','Customer Wise Sales Report','تقرير المبيعات حسب العميل',@RptSalesId,'AshproSAASERP.Shared.Pages.Reports.Sales.CustomerWiseSalesReportsView','CustomerWiseSalesReportsView',3,1,0,0,0,0
        UNION ALL SELECT 'Category Wise Report','Category Wise Report','تقرير حسب الفئة',@RptSalesId,'AshproSAASERP.Shared.Pages.Reports.Sales.CategoryWiseReportView','CategoryWiseReportView',4,1,0,0,0,0
        UNION ALL SELECT 'Sales Quotation Register','Sales Quotation Register','سجل عروض أسعار المبيعات',@RptSalesId,'AshproSAASERP.Shared.Pages.Reports.Sales.SalesQuotationRegisterView','SalesQuotationRegisterView',5,1,0,0,0,0
        UNION ALL SELECT 'Customer Wise Quotation Report','Customer Wise Quotation Report','تقرير عروض الأسعار حسب العميل',@RptSalesId,'AshproSAASERP.Shared.Pages.Reports.Sales.CustomerWiseQuotationReportView','CustomerWiseQuotationReportView',6,1,0,0,0,0
        UNION ALL SELECT 'Sales Order Register','Sales Order Register','سجل أوامر البيع',@RptSalesId,'AshproSAASERP.Shared.Pages.Reports.Sales.SalesOrderReportView','SalesOrderReportView',7,1,0,0,0,0
        UNION ALL SELECT 'Delivery Note Register','Delivery Note Register','سجل إشعارات التسليم',@RptSalesId,'AshproSAASERP.Shared.Pages.Reports.Sales.DeliveryNoteRegisterView','DeliveryNoteRegisterView',8,1,0,0,0,0
        UNION ALL SELECT 'Date Wise Profit Report','Date Wise Profit Report','تقرير الأرباح حسب التاريخ',@RptSalesId,'AshproSAASERP.Shared.Pages.Reports.Sales.DateWiseProfitReportView','DateWiseProfitReportView',9,1,0,0,0,0
        UNION ALL SELECT 'Proforma Invoice Report','Proforma Invoice Report','تقرير الفاتورة المبدئية',@RptSalesId,'AshproSAASERP.Shared.Pages.Reports.Sales.ProformaInvoiceReportView','ProformaInvoiceReportView',10,1,0,0,0,0
        UNION ALL SELECT 'Vat Report','Vat Report','تقرير ضريبة القيمة المضافة',@RptSalesId,'AshproSAASERP.Shared.Pages.Reports.Sales.VatReportView','VatReportView',11,1,0,0,0,0
    ) AS t(Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder)
    WHERE NOT EXISTS (
        SELECT 1 FROM MenuDetails m WHERE m.Name = t.Name AND m.ParentId <=> t.ParentId
    );

-- Monitor Reports
    SET @RptMonitorId = (SELECT Id FROM MenuDetails WHERE Name = 'Rpt_Monitor' LIMIT 1);

    INSERT INTO MenuDetails
    (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder)
    SELECT * FROM (
        SELECT 'Sales Monitor Report','Sales Monitor Report','تقرير مراقبة المبيعات',@RptMonitorId,'AshproSAASERP.Shared.Pages.Reports.Transactions.SalesMonitorReport','SalesMonitorReport',1,1,0,0,0,0
        UNION ALL SELECT 'Purchase Monitor Report','Purchase Monitor Report','تقرير مراقبة المشتريات',@RptMonitorId,'AshproSAASERP.Shared.Pages.Reports.Transactions.PurchaseMonitorReport','PurchaseMonitorReport',2,1,0,0,0,0
        UNION ALL SELECT 'Transaction Monitor Report','Transaction Monitor Report','تقرير مراقبة المعاملات',@RptMonitorId,'AshproSAASERP.Shared.Pages.Reports.Transactions.TransactionMonitorReport','TransactionMonitorReport',3,1,0,0,0,0
    ) AS t(Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder)
    WHERE NOT EXISTS (
        SELECT 1 FROM MenuDetails m WHERE m.Name = t.Name AND m.ParentId <=> t.ParentId
    );

-- Supplier Reports
    SET @RptSupplierId = (SELECT Id FROM MenuDetails WHERE Name = 'Rpt_Supplier' LIMIT 1);

    INSERT INTO MenuDetails
    (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder)
    SELECT * FROM (
        SELECT 'Supplier Statement','Supplier Statement','كشف حساب المورد',@RptSupplierId,'AshproSAASERP.Shared.Pages.Reports.Accounts.SupplierStatementReport','SupplierStatementReport',1,1,0,0,0,0
        UNION ALL SELECT 'Supplier Pending Invoices','Supplier Pending Invoices','فواتير الموردين المعلقة',@RptSupplierId,'AshproSAASERP.Shared.Pages.Reports.Accounts.SupplierPendingInvoiceReports','SupplierPendingInvoiceReports',2,1,0,0,0,0
        UNION ALL SELECT 'Supplier Summary','Supplier Summary','ملخص الموردين',@RptSupplierId,'AshproSAASERP.Shared.Pages.Reports.Accounts.SupplierSummaryReport','SupplierSummaryReport',3,1,0,0,0,0
        UNION ALL SELECT 'Supplier List','Supplier List','قائمة الموردين',@RptSupplierId,'AshproSAASERP.Shared.Pages.Reports.Accounts.SupplierListReport','SupplierListReport',4,1,0,0,0,0
        UNION ALL SELECT 'Supplier Payment Report','Supplier Payment Report','تقرير مدفوعات الموردين',@RptSupplierId,'AshproSAASERP.Shared.Pages.Reports.Accounts.SupplierPaymentReports','SupplierPaymentReports',5,1,0,0,0,0
    ) AS t(Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder)
    WHERE NOT EXISTS (
        SELECT 1 FROM MenuDetails m WHERE m.Name = t.Name AND m.ParentId <=> t.ParentId
    );

-- Customer Reports
    SET @RptCustomerId = (SELECT Id FROM MenuDetails WHERE Name = 'Rpt_Customer' LIMIT 1);

    INSERT INTO MenuDetails
    (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder)
    SELECT * FROM (
        SELECT 'Customer Statement','Customer Statement','كشف حساب العميل',@RptCustomerId,'AshproSAASERP.Shared.Pages.Reports.Sales.CustomerStatementView','CustomerStatementView',1,1,0,0,0,0
        UNION ALL SELECT 'Customer Summary','Customer Summary','ملخص العملاء',@RptCustomerId,'AshproSAASERP.Shared.Pages.Reports.Accounts.CustomerSummaryReport','CustomerSummaryReport',2,1,0,0,0,0
        UNION ALL SELECT 'Customer Pending Invoice Report','Customer Pending Invoice Report','فواتير العملاء المعلقة',@RptCustomerId,'AshproSAASERP.Shared.Pages.Reports.Accounts.CustomerPendingInvoiceReports','CustomerPendingInvoiceReports',3,1,0,0,0,0
        UNION ALL SELECT 'Customer List','Customer List','قائمة العملاء',@RptCustomerId,'AshproSAASERP.Shared.Pages.Reports.Accounts.CustomerListReport','CustomerListReport',4,1,0,0,0,0
    ) AS t(Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder)
    WHERE NOT EXISTS (
        SELECT 1 FROM MenuDetails m WHERE m.Name = t.Name AND m.ParentId <=> t.ParentId
    );

-- Purchase Reports
    SET @RptPurchaseId = (SELECT Id FROM MenuDetails WHERE Name = 'Rpt_Purchase' LIMIT 1);

    INSERT INTO MenuDetails
    (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder)
    SELECT * FROM (
        SELECT 'Purchase Summary','Purchase Summary','ملخص المشتريات',@RptPurchaseId,'AshproSAASERP.Shared.Pages.Reports.Purchase.PurchaseSummaryView','PurchaseSummaryView',1,1,0,0,0,0
        UNION ALL SELECT 'Purchase Details','Purchase Details','تفاصيل المشتريات',@RptPurchaseId,'AshproSAASERP.Shared.Pages.Reports.Purchase.PurchaseDetailsView','PurchaseDetailsView',2,1,0,0,0,0
        UNION ALL SELECT 'Purchase Order Report','Purchase Order Report','تقرير أوامر الشراء',@RptPurchaseId,'AshproSAASERP.Shared.Pages.Reports.Purchase.PurchaseOrderReportView','PurchaseOrderReportView',3,1,0,0,0,0
        UNION ALL SELECT 'Purchase Return Summary','Purchase Return Summary','ملخص إرجاع الشراء',@RptPurchaseId,'AshproSAASERP.Shared.Pages.Reports.Purchase.PurchaseReturnSummaryView','PurchaseReturnSummaryView',4,1,0,0,0,0
        UNION ALL SELECT 'Purchase Return Details','Purchase Return Details','تفاصيل إرجاع الشراء',@RptPurchaseId,'AshproSAASERP.Shared.Pages.Reports.Purchase.PurchaseReturnDetailsView','PurchaseReturnDetailsView',5,1,0,0,0,0
    ) AS t(Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder)
    WHERE NOT EXISTS (
        SELECT 1 FROM MenuDetails m WHERE m.Name = t.Name AND m.ParentId <=> t.ParentId
    );

-- Inventory Reports
    SET @RptInventoryId = (SELECT Id FROM MenuDetails WHERE Name = 'Rpt_Inventory' LIMIT 1);

    INSERT INTO MenuDetails
    (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder)
    SELECT * FROM (
        SELECT 'Stock Report','Stock Report','تقرير المخزون',@RptInventoryId,'AshproSAASERP.Shared.Pages.Reports.Inventory.StockReportView','StockReportView',1,1,0,0,0,0
        UNION ALL SELECT 'Stock Transfer Report','Stock Transfer Report','تقرير نقل المخزون',@RptInventoryId,'AshproSAASERP.Shared.Pages.Reports.Inventory.StockTransferReportView','StockTransferReportView',2,1,0,0,0,0
        UNION ALL SELECT 'Item Transaction History Report','Item Transaction History Report','تقرير سجل حركات الأصناف',@RptInventoryId,'AshproSAASERP.Shared.Pages.Reports.Inventory.ItemTransactionHistoryReportView','ItemTransactionHistoryReportView',3,1,0,0,0,0
        UNION ALL SELECT 'Stock Adjustment Report','Stock Adjustment Report','تقرير تسوية المخزون',@RptInventoryId,'AshproSAASERP.Shared.Pages.Reports.Inventory.StockAdjustmentView','StockAdjustmentView',4,1,0,0,0,0
    ) AS t(Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder)
    WHERE NOT EXISTS (
        SELECT 1 FROM MenuDetails m WHERE m.Name = t.Name AND m.ParentId <=> t.ParentId
    );

-- Accounts Reports
    SET @RptAccountsId = (SELECT Id FROM MenuDetails WHERE Name = 'Rpt_Accounts' LIMIT 1);

    INSERT INTO MenuDetails
    (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder)
    SELECT * FROM (
        SELECT 'Journal Entry Report','Journal Entry Report','تقرير قيود اليومية',@RptAccountsId,'AshproSAASERP.Shared.Pages.Reports.Accounts.JournalEntryReportView','JournalEntryReportView',1,1,0,0,0,0
        UNION ALL SELECT 'Trial Balance Report','Trial Balance Report','تقرير ميزان المراجعة',@RptAccountsId,'AshproSAASERP.Shared.Pages.Reports.Accounts.TrialBalanceView','TrialBalanceView',2,1,0,0,0,0
        UNION ALL SELECT 'Expense Report','Expense Report','تقرير المصروفات',@RptAccountsId,'AshproSAASERP.Shared.Pages.Reports.Accounts.ExpenseReportView','ExpenseReportView',3,1,0,0,0,0
        UNION ALL SELECT 'Profit And Loss Report','Profit And Loss Report','تقرير الأرباح والخسائر',@RptAccountsId,'AshproSAASERP.Shared.Pages.Reports.Accounts.ProfitAndLossReportView','ProfitAndLossReportView',4,1,0,0,0,0
        UNION ALL SELECT 'Balance Sheet Report','Balance Sheet Report','تقرير الميزانية العمومية',@RptAccountsId,'AshproSAASERP.Shared.Pages.Reports.Accounts.BalanceSheetReportView','BalanceSheetReportView',5,1,0,0,0,0
        UNION ALL SELECT 'Chart Of Account Report','Chart Of Account Report','تقرير دليل الحسابات',@RptAccountsId,'AshproSAASERP.Shared.Pages.Reports.Accounts.ChartOfAccountReportView','ChartOfAccountReportView',6,1,0,0,0,0
        UNION ALL SELECT 'LedgerReport','Ledger Report','تقرير دفتر الأستاذ',@RptAccountsId,'AshproSAASERP.Shared.Pages.Reports.Accounts.LedgerReportView','LedgerReportView',7,1,0,0,0,0
    ) AS t(Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder)
    WHERE NOT EXISTS (
        SELECT 1 FROM MenuDetails m WHERE m.Name = t.Name AND m.ParentId <=> t.ParentId
    );

-- Enquiry Register Report
    SET @RptEnquiryId = (SELECT Id FROM MenuDetails WHERE Name = 'Rpt_Enquiry' LIMIT 1);

    INSERT INTO MenuDetails
    (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder)
    SELECT * FROM (
        SELECT 'Enquiry Registration Report','Enquiry Registration Report','تقرير تسجيل الاستفسارات',@RptEnquiryId,'AshproSAASERP.Shared.Pages.Reports.Sales.EnquiryRegistrationReportView','EnquiryRegistrationReportView',1,1,0,0,0,0
    ) AS t(Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder)
    WHERE NOT EXISTS (
        SELECT 1 FROM MenuDetails m WHERE m.Name = t.Name AND m.ParentId <=> t.ParentId
    );

-- Others Reports
    SET @RptSalesId = (SELECT Id FROM MenuDetails WHERE Name = 'Rpt_Sales' LIMIT 1);

    INSERT INTO MenuDetails
    (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder)
    SELECT * FROM (
        SELECT 'PendingZatca','Zatca Report','تقرير الزكاة والضريبة (زاتكا)',@RptSalesId,'AshproSAASERP.Shared.Pages.Reports.Transaction.ZatcaPendingView','ZatcaPendingView',1,1,0,0,0,0
    ) AS t(Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder)
    WHERE NOT EXISTS (
        SELECT 1 FROM MenuDetails m WHERE m.Name = t.Name AND m.ParentId <=> t.ParentId
    );


/* =========================
   SETTINGS ITEMS
   ========================= */

    SET @SettingsId = (SELECT Id FROM MenuDetails WHERE Name = 'Settings' LIMIT 1);

    INSERT INTO MenuDetails
    (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder)
    SELECT * FROM (
        SELECT 'ManageUser','Manage User','إدارة المستخدمين',@SettingsId,'AshproSAASERP.Shared.Pages.Users.UserView','UserView',1,1,0,0,0,0
        UNION ALL SELECT 'PrintDesignTitle','Print Design','تصميم الطباعة',@SettingsId,'AshproSAASERP.Shared.Pages.Others.PrintDesignerView','PrintDesignerView',2,1,0,0,0,0
        UNION ALL SELECT 'Barcode Design','Barcode Design','تصميم الباركود',@SettingsId,'AshproSAASERP.Shared.Pages.Others.BarcodeDesignerView','BarcodeDesignerView',3,1,0,0,0,0
        UNION ALL SELECT 'CompanyDetails','Company Details','بيانات الشركة',@SettingsId,'AshproSAASERP.Shared.Pages.Settings.CompanyDetailsView','CompanyDetailsView',4,1,0,0,0,0
        UNION ALL SELECT 'ZATCA CSR','ZATCA CSR','زاتكا CSR',@SettingsId,'AshproSAASERP.Shared.Pages.Settings.ZatcaCSRView','ZatcaCSRView',5,1,0,0,0,0
        UNION ALL SELECT 'User Previlege','User Previlege','صلاحيات المستخدمين',@SettingsId,'AshproSAASERP.Shared.Pages.Users.UserPrevilageView','UserPrevilageView',6,1,0,0,0,0
        UNION ALL SELECT 'ZATCA Pending','ZATCA Pending Invoices','فواتير زاتكا المعلقة',@SettingsId,'AshproSAASERP.Shared.Pages.Settings.ZatcaPendingUploadView','ZatcaPendingUploadView',7,1,0,0,0,0
        UNION ALL SELECT 'ProductImport','Product Import','استيراد المنتجات',@SettingsId,'AshproSAASERP.Shared.Pages.Settings.ProductImportView','ProductImportView',8,1,0,0,0,0
        UNION ALL SELECT 'ProductExport','Product Export','تصدير المنتجات',@SettingsId,'AshproSAASERP.Shared.Pages.Settings.ProductExportView','ProductExportView',9,1,0,0,0,0
        UNION ALL SELECT 'PrintingBarcode','Printing Barcode','طباعة الباركود',@SettingsId,'AshproSAASERP.Shared.Pages.Settings.PrintingBarcodeView','PrintingBarcodeView',10,1,0,0,0,0
    ) AS t(Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder)
    WHERE NOT EXISTS (
        SELECT 1 FROM MenuDetails m WHERE m.Name = t.Name AND m.ParentId <=> t.ParentId
    );

";
    }
    private string GetMenuVisibilitySeedSql()
    {
        return @"INSERT INTO MenuVisibility (MenuDetailId, IsVisible, LastModified)
SELECT Id, 1, NOW()
FROM MenuDetails
WHERE EXISTS (SELECT 1 FROM MenuDetails)
  AND NOT EXISTS (SELECT 1 FROM MenuVisibility);
";
    }
}
