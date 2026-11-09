using ZakiSAASERP.Interface;
using ZakiSAASERP.Interface.Others;
using ZakiSAASERP.Models.Company;
using ZakiSAASERP.Models.Masters;
using ZakiSAASERP.Models.User;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.RegularExpressions;

namespace ZakiSAASERP.Oracle;

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
                        Name = exlcompany.SubscriptionStartedAt?.Year.ToString() ?? DateTime.Now.Year.ToString(),
                        RefNo = 1
                    };

                    context.FinancialYears.Add(fyear);
                    context.UnitMasters.Add(new UnitMaster { Name = "PCs", Code = "PCs", Remarks = "Default Unit" });

                    var category = new CategoryMaster { Name = "General", Code = "Gen", Remarks = "Default Category" };
                    context.CategoryMasters.Add(category);

                    var country = new CountryMaster { Name = "Saudi Arabia", Code = "SA" };
                    context.CountryMasters.Add(country);

                    var customerGroup = new CustomerGroupMaster { Name = "General", Code = "Gen", Remarks = "Default Customer Group" };
                    context.CustomerGroupMasters.Add(customerGroup);

                    await context.SaveChangesAsync();

                    int categoryId = category.CategoryId;
                    int countryId = country.CountryId;
                    int customerGroupId = customerGroup.CustomerGroupId;

                    context.SubCategoryMasters.Add(new SubCategoryMaster { Name = "General", Code = "Gen", Remarks = "Default SubCategory", FK_CategoryId = categoryId });
                    context.SalesManMasters.Add(new SalesManMaster { Name = "SM", Code = "SM", Remarks = "Default Salesman", Designation = "Salesman" });
                    context.StoreMasters.Add(new StoreMaster { Name = "Main Store", Code = "Main", IsDefaultStore = true, FK_BranchId = iBranchId });
                    context.VatMasters.Add(new VatMaster { Name = "VAT 15%", Code = "VAT15", Percentage = 15, IsDefault = true });
                    context.CurrencyMasters.Add(new CurrencyMaster { Name = "Saudi Riyal", Code = "SAR", Rate = 1 });
                    context.CustomerLocationMasters.Add(new CustomerLocationMaster { Name = "General", Code = "Gen", Remarks = "Default Customer Location" });
                    context.CustomerMasters.Add(new CustomerMaster { Code = "C1001", Name = "Cash Customer", NameArabic = "عميل نقدي", FK_CustomerGroupId = customerGroupId, FK_CountryId = countryId, IsSupplier = false, CusBillType = "Cash", IsActive = true });
                    context.SupplierMasters.Add(new SupplierMaster { Code = "S1001", Name = "Cash Supplier", NameArabic = "مورد نقدي", FK_CountryId = countryId, IsCustomer = false, IsActive = true });
                    context.PurchaserMasters.Add(new PurchaserMaster { Code = "P1001", Name = "Purchaser", NameArabic = "مشتري", CreditLimit = 0, Remarks = "Default Purchaser" });
                    context.ProjectMasters.Add(new ProjectMaster { Code = "PRJ1001", Name = "General Project", Description = "Default Project", Status = "Active" });

                    await context.SaveChangesAsync();
                }

                if (!await context.MenuDetails.AnyAsync())
                {
                    var sql = GetMenuSeedSql();
                    await ExecuteOracleSqlAsync(context, sql);
                }

                if (!await context.PermanentSettings.AnyAsync())
                {
                    var settingssql = GetSettingsSeed();
                    await ExecuteOracleSqlAsync(context, settingssql);
                }

                if (await context.MenuDetails.AnyAsync() && !await context.MenuVisibilities.AnyAsync())
                {
                    var menuvisibiltysql = "BEGIN INSERT INTO \"MenuVisibility\" (\"MenuDetailId\", \"IsVisible\", \"LastModified\") SELECT \"Id\", 1, SYSDATE FROM \"MenuDetails\"; END;";
                    await context.Database.ExecuteSqlRawAsync(menuvisibiltysql);
                }

                // Ensure any missing menus (e.g. PrintingBarcode), visibilities, presets, and settings are seeded for existing databases
                await MenuSeedHelper.EnsureSeedDataAsync(context);

                // Oracle views and triggers creation
                var triggers = GetTriggerSQLs();
                foreach (var trigger in triggers)
                {
                    await context.Database.ExecuteSqlRawAsync(trigger);
                }

                var viewsql = GetViewSQL();
                await context.Database.ExecuteSqlRawAsync(viewsql);

                if (!await context.ERPTypes.AnyAsync())
                {
                    var erptypesql = GetErpTypeSQL();
                    await ExecuteOracleSqlAsync(context, erptypesql);
                }

                if (!await context.AccountTypes.AnyAsync())
                {
                    var actypesql = GetAccountTypeSQL();
                    await ExecuteOracleSqlAsync(context, actypesql);
                }

                if (!await context.AccountHeadMasters.AnyAsync())
                {
                    var acchdsql = GetAccHdSQL();
                    await ExecuteOracleSqlAsync(context, acchdsql);
                }

                if (!await context.AccountGroups.AnyAsync())
                {
                    var accgrpsql = GetAccGrpSql();
                    await ExecuteOracleSqlAsync(context, accgrpsql);
                }

                if (!await context.AccountSubGroups.AnyAsync())
                {
                    var accsubgrpsql = GetAccSubGrpSql();
                    await ExecuteOracleSqlAsync(context, accsubgrpsql);
                }

                if (!await context.AccountMasters.AnyAsync())
                {
                    var accmstrsql = GetAcMstSql();
                    await ExecuteOracleSqlAsync(context, accmstrsql);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding database: {ex.Message}");
                throw;
            }
        }
    }

    #region 🔹 Oracle Helper SQL Executor

    private async Task ExecuteOracleSqlAsync(DbContext context, string sql)
    {
        var oracleSql = ConvertSqlToOracle(sql);
        await context.Database.ExecuteSqlRawAsync(oracleSql);
    }

    private string ConvertSqlToOracle(string sql)
    {
        var statements = sql.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        var sb = new StringBuilder();
        sb.AppendLine("BEGIN");

        foreach (var stmt in statements)
        {
            var trimmed = stmt.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;

            // Handle multi-row INSERTs: INSERT INTO Table (cols) VALUES (val1), (val2)...
            var match = Regex.Match(trimmed, @"INSERT\s+INTO\s+([\w""]+)\s*\(([^)]+)\)\s*VALUES\s*([\s\S]+)", RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                sb.AppendLine($"  {trimmed};");
                continue;
            }

            var tableName = match.Groups[1].Value;
            var columns = match.Groups[2].Value;
            var valuesPart = match.Groups[3].Value.Trim();

            // Auto-quote table name if not already quoted
            tableName = $"\"{tableName.Replace("\"", "")}\"";

            // Auto-quote column names
            var colList = columns.Split(',');
            for (int i = 0; i < colList.Length; i++)
            {
                var col = colList[i].Trim().Replace("\"", "");
                colList[i] = $"\"{col}\"";
            }
            var quotedColumns = string.Join(", ", colList);

            var rows = SplitValues(valuesPart);
            foreach (var row in rows)
            {
                // Auto-quote subqueries inside values
                var processedRow = row
                    .Replace("SELECT Id FROM MenuDetails WHERE Name=", "SELECT \"Id\" FROM \"MenuDetails\" WHERE \"Name\"=")
                    .Replace(" AND ParentId=", " AND \"ParentId\"=");

                sb.AppendLine($"  INSERT INTO {tableName} ({quotedColumns}) VALUES ({processedRow});");
            }
        }

        sb.AppendLine("END;");
        return sb.ToString();
    }

    private List<string> SplitValues(string valuesPart)
    {
        var list = new List<string>();
        int depth = 0;
        int start = -1;
        bool inQuotes = false;

        for (int i = 0; i < valuesPart.Length; i++)
        {
            char c = valuesPart[i];
            if (c == '\'' && (i == 0 || valuesPart[i - 1] != '\\'))
            {
                inQuotes = !inQuotes;
            }

            if (!inQuotes)
            {
                if (c == '(')
                {
                    depth++;
                    if (depth == 1) start = i;
                }
                else if (c == ')')
                {
                    depth--;
                    if (depth == 0 && start != -1)
                    {
                        list.Add(valuesPart.Substring(start + 1, i - start - 1));
                        start = -1;
                    }
                }
            }
        }
        return list;
    }

    #endregion

    #region 🔹 Trigger & View SQLs

    private string[] GetTriggerSQLs()
    {
        return new string[]
        {
            @"CREATE OR REPLACE TRIGGER TR_StockTranLog_Insert
            AFTER INSERT ON ""StockTranLog""
            FOR EACH ROW
            DECLARE
                v_exists NUMBER;
            BEGIN
                SELECT COUNT(*) INTO v_exists FROM ""ItemStockBalance"" 
                WHERE ""FK_ItemId"" = :new.""FK_ItemId"" AND ""FK_BranchId"" = :new.""FK_BranchId"";
                
                IF v_exists > 0 THEN
                    UPDATE ""ItemStockBalance""
                    SET ""CurrentQty"" = ""CurrentQty"" + (COALESCE(:new.""InQuantity"", 0) - COALESCE(:new.""OutQuantity"", 0)),
                        ""LastUpdated"" = SYSDATE
                    WHERE ""FK_ItemId"" = :new.""FK_ItemId"" AND ""FK_BranchId"" = :new.""FK_BranchId"";
                ELSE
                    INSERT INTO ""ItemStockBalance"" (""FK_ItemId"", ""FK_BranchId"", ""CurrentQty"", ""LastUpdated"")
                    VALUES (:new.""FK_ItemId"", :new.""FK_BranchId"", (COALESCE(:new.""InQuantity"", 0) - COALESCE(:new.""OutQuantity"", 0)), SYSDATE);
                END IF;
            END;",

            @"CREATE OR REPLACE TRIGGER TR_StockTranLog_Update
            AFTER UPDATE ON ""StockTranLog""
            FOR EACH ROW
            DECLARE
                v_exists NUMBER;
            BEGIN
                -- Subtract old values
                UPDATE ""ItemStockBalance""
                SET ""CurrentQty"" = ""CurrentQty"" - (COALESCE(:old.""InQuantity"", 0) - COALESCE(:old.""OutQuantity"", 0)),
                    ""LastUpdated"" = SYSDATE
                WHERE ""FK_ItemId"" = :old.""FK_ItemId"" AND ""FK_BranchId"" = :old.""FK_BranchId"";

                -- Add new values
                SELECT COUNT(*) INTO v_exists FROM ""ItemStockBalance"" 
                WHERE ""FK_ItemId"" = :new.""FK_ItemId"" AND ""FK_BranchId"" = :new.""FK_BranchId"";
                
                IF v_exists > 0 THEN
                    UPDATE ""ItemStockBalance""
                    SET ""CurrentQty"" = ""CurrentQty"" + (COALESCE(:new.""InQuantity"", 0) - COALESCE(:new.""OutQuantity"", 0)),
                        ""LastUpdated"" = SYSDATE
                    WHERE ""FK_ItemId"" = :new.""FK_ItemId"" AND ""FK_BranchId"" = :new.""FK_BranchId"";
                ELSE
                    INSERT INTO ""ItemStockBalance"" (""FK_ItemId"", ""FK_BranchId"", ""CurrentQty"", ""LastUpdated"")
                    VALUES (:new.""FK_ItemId"", :new.""FK_BranchId"", (COALESCE(:new.""InQuantity"", 0) - COALESCE(:new.""OutQuantity"", 0)), SYSDATE);
                END IF;
            END;",

            @"CREATE OR REPLACE TRIGGER TR_StockTranLog_Delete
            AFTER DELETE ON ""StockTranLog""
            FOR EACH ROW
            BEGIN
                UPDATE ""ItemStockBalance""
                SET ""CurrentQty"" = ""CurrentQty"" - (COALESCE(:old.""InQuantity"", 0) - COALESCE(:old.""OutQuantity"", 0)),
                    ""LastUpdated"" = SYSDATE
                WHERE ""FK_ItemId"" = :old.""FK_ItemId"" AND ""FK_BranchId"" = :old.""FK_BranchId"";
            END;"
        };
    }

    private string GetViewSQL()
    {
        return @"
        CREATE OR REPLACE VIEW ""BarcodeData""
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

            b.""Name"" ""BrandName"",
            u.""Name"" ""UnitName"",
            c.""Name"" ""CategoryName"",
            sc.""Name"" ""SubCategoryName"",
            s.""Name"" ""SupplierName""

        FROM ""ItemMaster"" i
        LEFT JOIN ""BrandMaster"" b       ON i.""FK_BrandId"" = b.""BrandId""
        LEFT JOIN ""UnitMaster"" u        ON i.""FK_UnitId"" = u.""UnitId""
        LEFT JOIN ""CategoryMaster"" c    ON i.""FK_CategoryId"" = c.""CategoryId""
        LEFT JOIN ""SubCategoryMaster"" sc ON i.""FK_SubCategoryId"" = sc.""SubCategoryId""
        LEFT JOIN ""SupplierMaster"" s    ON i.""FK_SupplierId"" = s.""SupplierId""";
    }

    #endregion

    #region 🔹 Seed SQL Data Methods

    private string GetAcMstSql()
    {
        return @"
        INSERT INTO AccountMaster
        (Code, Name, NameArabic, FK_AccountHeadId, FK_AcGroupId,
         Opening, DbCr, FK_AcTypeId, FK_AccountSubGroupId, Flag, IsReserved)
        VALUES
        ('110001010001','Cash in Hand','نقد في اليد',1,1,0,'Debit',1,1,'Acc',1),
        ('110001020001','Bank Account 1','حساب بنكي 1',1,1,0,'Debit',1,2,'Acc',1),
        ('110001020002','Bank Account 2','حساب بنكي 2',1,1,0,'Debit',1,2,'Acc',1),
        ('110002010001','Customer – Local','عميل - محلي',1,2,0,'Debit',1,3,'Acc',1),
        ('120001010001','Office Building','مبنى المكاتب',2,3,0,'Debit',1,4,'Acc',1),
        ('120001020001','Delivery Vehicle','سيارة توصيل',2,3,0,'Debit',1,5,'Acc',1),
        ('120001030001','Office Equipment','معدات مكتبية',2,3,0,'Debit',1,6,'Acc',1),
        ('210001010001','Supplier – Local','مورد - محلي',3,4,0,'Credit',2,7,'Acc',1),
        ('210002010001','VAT Output @15%','ضريبة المخرجات @15%',3,5,0,'Credit',2,8,'Acc',1),
        ('210002020001','VAT Payable to ZATCA','ضريبة مستحقة للزكاة',3,5,0,'Credit',2,9,'Acc',1),
        ('220001010001','Long-Term Loan','قرض طويل الأجل',4,6,0,'Credit',2,10,'Acc',1),
        ('310001010001','Sales','المبيعات',5,7,0,'Credit',3,11,'Acc',1),
        ('310002010001','Discount Received','خصم مكتسب',5,8,0,'Credit',3,12,'Acc',1),
        ('310002020001','Interest Income','دخل الفوائد',5,8,0,'Credit',3,13,'Acc',1),
        ('410001010001','Office Rent','إيجار المكتب',6,9,0,'Debit',4,14,'Acc',1),
        ('410001020001','Staff Salary','رواتب الموظفين',6,9,0,'Debit',4,15,'Acc',1),
        ('410002010001','Electricity Expense','مصاريف الكهرباء',6,10,0,'Debit',4,16,'Acc',1),
        ('410002020001','Water Expense','مصاريف المياه',6,10,0,'Debit',4,17,'Acc',1),
        ('410003010001','Delivery Expense','مصاريف التوصيل',6,11,0,'Debit',4,18,'Acc',1),
        ('410004010001','Depreciation – Vehicle','إهلاك - مركبة',6,12,0,'Debit',4,19,'Acc',1),
        ('410005010001','Bank Charges','رسوم بنكية',6,13,0,'Debit',4,20,'Acc',1),
        ('330001010001','Opening Balance Equity','رصيد الافتتاحي',7,14,0,'Credit',2,21,'Acc',1),
        ('420001010001','Purchase Account','حساب المشتريات',8,15,0,'Debit',4,22,'Acc',1),
        ('110003010001','VAT Input @15%','ضريبة المدخلات @15%',1,16,0,'Debit',1,23,'Acc',1),
        ('110004010001','Inventory Asset','أصل المخزون',1,17,0,'Debit',1,24,'Acc',1);";
    }

    private string GetAccSubGrpSql()
    {
        return @"
        INSERT INTO AccountSubGroup
               (FK_AcGroupId,Code,Name,NameArabic,Remarks,FK_AcHeadId,IsReserved)
        VALUES
        (1,'11000101','Cash','نقد',NULL,1,1),
        (1,'11000102','Bank','بنك',NULL,1,1),
        (2,'11000201','Sundry Debtors','مدينون متنوعون',NULL,1,1),
        (3,'12000101','Building','مبنى',NULL,2,1),
        (3,'12000102','Vehicles','مركبات',NULL,2,1),
        (3,'12000103','Equipment','معدات',NULL,2,1),
        (4,'21000101','Sundry Creditors','دائنون متنوعون',NULL,3,1),
        (5,'21000201','VAT Output','مخرجات الضريبة',NULL,3,1),
        (5,'21000202','VAT Payable','ضريبة مستحقة',NULL,3,1),
        (6,'22000101','Bank Loan','قرض بنكي',NULL,4,1),
        (7,'31000101','Local Sales','مبيعات محلية',NULL,5,1),
        (8,'31000201','Discount','خصم',NULL,5,1),
        (8,'31000202','Interest','فائدة',NULL,5,1),
        (9,'41000101','Rent','إيجار',NULL,6,1),
        (9,'41000102','Salary','راتب',NULL,6,1),
        (10,'41000201','Electricity','كهرباء',NULL,6,1),
        (10,'41000202','Water','ماء',NULL,6,1),
        (11,'41000301','Transport','نقل',NULL,6,1),
        (12,'41000401','Vehicles','مركبات',NULL,6,1),
        (13,'41000501','Bank Charges','رسوم بنكية',NULL,6,1),
        (14,'33000101','Owners Equity','حقوق الملكية',NULL,7,1),
        (15,'42000101','Local Purchases','مشتريات محلية',NULL,8,1),
        (16,'11000301','VAT Input','مدخلات الضريبة',NULL,1,1),
        (17,'11000401','Stock','مخزون',NULL,1,1);";
    }

    private string GetAccGrpSql()
    {
        return @"
        INSERT INTO AccountGroup
               (Code,Name,NameArabic,Remarks,FK_AcHeadId,IsReserved)
        VALUES
        ('110001','Cash & Bank','النقد والبنك',NULL,1,1),
        ('110002','Accounts Receivable','حسابات القبض',NULL,1,1),
        ('120001','Property Plant & Equip','الممتلكات والمنشآت والمعدات',NULL,2,1),
        ('210001','Accounts Payable','حسابات الدفع',NULL,3,1),
        ('210002','VAT Payable','ضريبة القيمة المضافة المستحقة',NULL,3,1),
        ('220001','Loans','قروض',NULL,4,1),
        ('310001','Sales','المبيعات',NULL,5,1),
        ('310002','Other Income','إيرادات أخرى',NULL,5,1),
        ('410001','Administrative','إداري',NULL,6,1),
        ('410002','Utilities','خدمات',NULL,6,1),
        ('410003','Selling Expenses','مصاريف البيع',NULL,6,1),
        ('410004','Depreciation','إهلاك',NULL,6,1),
        ('410005','Financial Expenses','مصاريف مالية',NULL,6,1),
        ('330001','Capital Accounts','حسابات رأس المال',NULL,7,1),
        ('420001','Purchases','المشتريات',NULL,8,1),
        ('110003','VAT Receivable','ضريبة القيمة المضافة المستحقة القبض',NULL,1,1),
        ('110004','Inventory Assets','أصول المخزون',NULL,1,1);";
    }

    private string GetAccHdSQL()
    {
        return @"
        INSERT INTO AccountHeadMaster
                   (Code, Name, NameArabic, FK_AcTypeId, IsReserved)
        VALUES
        ('1100','Current Assets','الأصول المتداولة',1,1),
        ('1200','Fixed Assets','الأصول الثابتة',1,1),
        ('2100','Current Liabilities','الالتزامات المتداولة',2,1),
        ('2200','Non-Current Liabilities','الالتزامات غير المتداولة',2,1),
        ('3100','Revenue','إيرادات',3,1),
        ('4100','Operating Expenses','مصاريف تشغيلية',4,1),
        ('3300','Equity','حقوق الملكية',2,1),
        ('4200','Cost of Sales','تكلفة المبيعات',4,1);";
    }

    private string GetAccountTypeSQL()
    {
        return @"
        INSERT INTO AccountType
                   (AcTypeName)
        VALUES
        ('Asset'),
        ('Liability'),
        ('Income'),
        ('Expense');";
    }

    private string GetErpTypeSQL()
    {
        return @"
        INSERT INTO ERPType (TypeName,Description,IsCustom,CreatedDate)
             VALUES
        ('Trading','Sales-focused business with inventory and customer management',0,SYSDATE),
        ('Manufacturing','Production-based business with full procurement, manufacturing and sales',0,SYSDATE),
        ('Transportation','Logistics and delivery services with minimal inventory',0,SYSDATE),
        ('Retail','Point-of-sale and customer-focused retail operations',0,SYSDATE),
        ('Services','Service-based business with project management and minimal inventory',0,SYSDATE),
        ('Custom','Fully customizable menu configuration - select specific menus to show',1,SYSDATE);";
    }

    private string GetSettingsSeed()
    {
        return @"
        INSERT INTO PermanentSettings (SettingKey, SettingValue)
        VALUES
        ('DefaultCashAccount',null),
        ('DefaultBankAccount',null),
        ('AutoItemCode','0'),
        ('ItemCodePrefix','IT'),
        ('ItemCodeStartNumber','1'),
        ('ItemCodeZeroPadding','3'),
        ('CategoryWiseItemCode','0'),
        ('SubCategoryWiseItemCode','0'),
        ('CodeSeparator','-'),
        ('AutoCustomerCode','0'),
        ('CustomerCodePrefix','CUS'),
        ('CustomerCodeStartNumber','1'),
        ('CustomerCodeZeroPadding','3'),
        ('AutoSupplierCode','0'),
        ('SupplierCodePrefix','SUP'),
        ('SupplierCodeStartNumber','1'),
        ('SupplierCodeZeroPadding','3'),
        ('AutoCategoryCode','0'),
        ('CategoryCodePrefix','CAT'),
        ('CategoryCodeStartNumber','1'),
        ('CategoryCodeZeroPadding','3'),
        ('AutoSubCategoryCode','0'),
        ('SubCategoryCodePrefix','SUB'),
        ('SubCategoryCodeStartNumber','1'),
        ('SubCategoryCodeZeroPadding','3'),
        ('CategoryWiseSubCategoryCode','0'),
        ('AutoQuotationNo','1'),
        ('QuotationNoPrefix','QT'),
        ('QuotationNoStartNumber','1'),
        ('QuotationNoZeroPadding','4'),
        ('AutoSalesOrderNo','1'),
        ('SalesOrderNoPrefix','SO'),
        ('SalesOrderNoStartNumber','1'),
        ('SalesOrderNoZeroPadding','4'),
        ('AutoDeliveryNoteNo','1'),
        ('DeliveryNoteNoPrefix','DN'),
        ('DeliveryNoteNoStartNumber','1'),
        ('DeliveryNoteNoZeroPadding','4'),
        ('DeliveryNoteStockAffect','0'),
        ('AutoPurchaseOrderNo','1'),
        ('PurchaseOrderNoPrefix','PO'),
        ('PurchaseOrderNoStartNumber','1'),
        ('PurchaseOrderNoZeroPadding','4'),
        ('AutoPurchaseNo','1'),
        ('PurchaseNoPrefix','P'),
        ('PurchaseNoStartNumber','1'),
        ('PurchaseNoZeroPadding','4'),
        ('DefaultFocusOn','ItemCode'),
        ('DefaultBillType','Cash'),
        ('AllowNegativeStock','0'),
        ('ShowCostInSales','0'),
        ('RequireBarcode','0'),
        ('SalesInvoice_EnableUpdate','0'),
        ('SalesInvoice_EnableCancel','0'),
        ('SalesReturn_EnableUpdate','0'),
        ('SalesReturn_EnableCancel','0'),
        ('ImageStorageMode','0'),
        ('ImageClientFolder','DefaultClient'),
        ('ImageLocalBasePath','\\192.168.1.11\D\Images'),
        ('ImageAwsRegion','us-east-1'),
        ('ImageAwsBucket','erp-images'),
        ('ImageAwsAccessKey',''),
        ('ImageAwsSecretKey',''),
        ('AutoJournalEntryNo','1'),
        ('JournalEntryNoPrefix','JV'),
        ('JournalEntryNoStartNumber','1'),
        ('JournalEntryNoZeroPadding','4'),
        ('AutoExpenseEntryNo','1'),
        ('ExpenseEntryNoPrefix','EXP'),
        ('ExpenseEntryNoStartNumber','1'),
        ('ExpenseEntryNoZeroPadding','4'),
        ('AutoCustomerReceiptNo','1'),
        ('CustomerReceiptNoPrefix','CR'),
        ('CustomerReceiptNoStartNumber','1'),
        ('CustomerReceiptNoZeroPadding','6'),
        ('AutoStockAdjustmentNo','0'),
        ('StockAdjustmentNoPrefix','SA'),
        ('StockAdjustmentNoStartNumber','1'),
        ('StockAdjustmentNoZeroPadding','4'),
        ('AutoMaterialTransferInNo','0'),
        ('MaterialTransferInNoPrefix','MTI'),
        ('MaterialTransferInNoStartNumber','1'),
        ('MaterialTransferInNoZeroPadding','4'),
        ('AutoMaterialTransferOutNo','0'),
        ('MaterialTransferOutNoPrefix','MTO'),
        ('MaterialTransferOutNoStartNumber','1'),
        ('MaterialTransferOutNoZeroPadding','4'),
        ('AutoJobOrderNo','0'),
        ('JobOrderNoPrefix','JO'),
        ('JobOrderNoStartNumber','1'),
        ('JobOrderNoZeroPadding','4'),
        ('AutoJobCardNo','0'),
        ('JobCardNoPrefix','JC'),
        ('JobCardNoStartNumber','1'),
        ('JobCardNoZeroPadding','4'),
        ('AutoMaterialTransferRequestNo', '0'),
        ('MaterialTransferRequestNoPrefix', 'MTR'),
        ('MaterialTransferRequestNoStartNumber', '1'),
        ('MaterialTransferRequestNoZeroPadding', '4');";
    }

    private string GetMenuSeedSql()
    {
        return @"
        INSERT INTO MenuDetails (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder) VALUES
        ('Masters', 'Masters', 'البيانات الأساسية', NULL, NULL, 'AddCircle', 1, 1, 0, 0, 0, 0),
        ('Transactions', 'Transactions', 'المعاملات', NULL, NULL, 'ShoppingCart', 2, 1, 0, 0, 0, 0),
        ('Accounting', 'Accounting', 'المحاسبة', NULL, NULL, 'AccountBalance', 3, 1, 0, 0, 0, 0),
        ('Reports', 'Reports', 'التقارير', NULL, NULL, 'Assessment', 4, 1, 0, 0, 0, 0),
        ('Settings', 'Settings', 'الإعدادات', NULL, NULL, 'Settings', 5, 1, 0, 0, 0, 0);

        INSERT INTO MenuDetails (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder) VALUES
        ('Inventory', 'Inventory', 'المخزون', (SELECT Id FROM MenuDetails WHERE Name='Masters'), NULL, 'Inventory', 1, 1, 0, 0, 0, 0),
        ('Accounts', 'Accounts', 'الحسابات', (SELECT Id FROM MenuDetails WHERE Name='Masters'), NULL, 'AccountBalance', 2, 1, 0, 0, 0, 0),
        ('GeneralMasters', 'General Masters', 'الإعدادات العامة', (SELECT Id FROM MenuDetails WHERE Name='Masters'), NULL, 'SettingsApplications', 3, 1, 0, 0, 0, 0),
        ('Parties', 'Parties', 'الأطراف', (SELECT Id FROM MenuDetails WHERE Name='Masters'), NULL, 'People', 4, 1, 0, 0, 0, 0);

        INSERT INTO MenuDetails (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder) VALUES 
        ('Unit Master', 'Unit Master', 'إدارة الوحدات', (SELECT Id FROM MenuDetails WHERE Name='Inventory' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Masters')), 'ZakiSAASERP.Shared.Pages.Masters.UnitMasterView', 'UnitMasterView', 1, 1, 0, 0, 0, 0),
        ('CategoryMaster', 'Category Master', 'إدارة الفئات', (SELECT Id FROM MenuDetails WHERE Name='Inventory' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Masters')), 'ZakiSAASERP.Shared.Pages.Masters.CategoryMasterView', 'CategoryMasterView', 2, 1, 0, 0, 0, 0),
        ('SubCategoryMaster', 'Sub Category', 'إدارة الفئات الفرعية', (SELECT Id FROM MenuDetails WHERE Name='Inventory' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Masters')), 'ZakiSAASERP.Shared.Pages.Masters.SubCategoryMasterView', 'SubCategoryMasterView', 3, 1, 0, 0, 0, 0),
        ('BrandMaster', 'Brand Master', 'إدارة العلامات التجارية', (SELECT Id FROM MenuDetails WHERE Name='Inventory' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Masters')), 'ZakiSAASERP.Shared.Pages.Masters.BrandMastersView', 'BrandMastersView', 4, 1, 0, 0, 0, 0),
        ('ItemMaster', 'Item Master', 'إدارة الأصناف', (SELECT Id FROM MenuDetails WHERE Name='Inventory' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Masters')), 'ZakiSAASERP.Shared.Pages.Masters.ItemMasterView', 'ItemMasterView', 5, 1, 0, 0, 0, 0),
        ('StoreMaster', 'Store Master', 'إدارة المخازن', (SELECT Id FROM MenuDetails WHERE Name='Inventory' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Masters')), 'ZakiSAASERP.Shared.Pages.Masters.StoreMasterView', 'StoreMasterView', 6, 1, 0, 0, 0, 0),
        ('OpeningStockEntry', 'Opening Stock Entry', 'إدخل الرصيد الافتتاحي', (SELECT Id FROM MenuDetails WHERE Name='Inventory' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Masters')), 'ZakiSAASERP.Shared.Pages.Masters.OpeningStockEntryView', 'OpeningStockEntryView', 7, 1, 0, 0, 0, 0);

        INSERT INTO MenuDetails (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder) VALUES 
        ('AccountHeadTitle', 'Account Head', 'رؤوس الحسابات', (SELECT Id FROM MenuDetails WHERE Name='Accounts' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Masters')), 'ZakiSAASERP.Shared.Pages.Masters.AccountHeadView', 'AccountHeadView', 1, 1, 0, 0, 0, 0),
        ('AccountGroup', 'Account Group', 'مجموعات الحسابات', (SELECT Id FROM MenuDetails WHERE Name='Accounts' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Masters')), 'ZakiSAASERP.Shared.Pages.Masters.AccountGroupView', 'AccountGroupView', 2, 1, 0, 0, 0, 0),
        ('AccountSubGroup', 'Account SubGroup', 'المجموعات الفرعية للحسابات', (SELECT Id FROM MenuDetails WHERE Name='Accounts' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Masters')), 'ZakiSAASERP.Shared.Pages.Masters.AccountSubGroupView', 'AccountSubGroupView', 3, 1, 0, 0, 0, 0),
        ('AccountsPage', 'Accounts', 'الحسابات', (SELECT Id FROM MenuDetails WHERE Name='Accounts' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Masters')), 'ZakiSAASERP.Shared.Pages.Masters.AccountsView', 'AccountsView', 4, 1, 0, 0, 0, 0);

        INSERT INTO MenuDetails (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder) VALUES 
        ('BranchTitle', 'Branch', 'الفروع', (SELECT Id FROM MenuDetails WHERE Name='GeneralMasters'), 'ZakiSAASERP.Shared.Pages.Masters.BranchView', 'BranchView', 1, 1, 0, 0, 0, 0),
        ('FinancilaYearTitle', 'Financial Year', 'السنة المالية', (SELECT Id FROM MenuDetails WHERE Name='GeneralMasters'), 'ZakiSAASERP.Shared.Pages.Masters.FinancialYearView', 'FinancialYearView', 2, 1, 0, 0, 0, 0),
        ('SalesManMaster', 'Salesman Master', 'إدارة مندوبي المبيعات', (SELECT Id FROM MenuDetails WHERE Name='GeneralMasters'), 'ZakiSAASERP.Shared.Pages.Masters.SalesManMasterView', 'SalesManMasterView', 3, 1, 0, 0, 0, 0),
        ('PurchaserMaster', 'Purchaser Master', 'إدارة مسؤولي المشتريات', (SELECT Id FROM MenuDetails WHERE Name='GeneralMasters'), 'ZakiSAASERP.Shared.Pages.Masters.PurchaserMasterView', 'PurchaserMasterView', 4, 1, 0, 0, 0, 0),
        ('CountryMaster', 'Country Master', 'إدارة الدول', (SELECT Id FROM MenuDetails WHERE Name='GeneralMasters'), 'ZakiSAASERP.Shared.Pages.Masters.CountryMasterView', 'CountryMasterView', 5, 1, 0, 0, 0, 0),
        ('VatMaster', 'Vat Master', 'إدارة ضريبة القيمة المضافة', (SELECT Id FROM MenuDetails WHERE Name='GeneralMasters'), 'ZakiSAASERP.Shared.Pages.Masters.VatMasterView', 'VatMasterView', 6, 1, 0, 0, 0, 0),
        ('CurrencyMaster', 'CurrencyMaster', 'إدارة العملات', (SELECT Id FROM MenuDetails WHERE Name='GeneralMasters'), 'ZakiSAASERP.Shared.Pages.Masters.CurrencyMasterView', 'CurrencyMasterView', 7, 1, 0, 0, 0, 0),
        ('ProjectMaster', 'Project Master', 'إدارة المشاريع', (SELECT Id FROM MenuDetails WHERE Name='GeneralMasters'), 'ZakiSAASERP.Shared.Pages.Masters.ProjectMasterView', 'ProjectMasterView', 8, 1, 0, 0, 0, 0),
        ('VehicleMaster', 'Vehicle Master', 'إدارة المركبات', (SELECT Id FROM MenuDetails WHERE Name='GeneralMasters'), 'ZakiSAASERP.Shared.Pages.Masters.VehicleMasterView', 'VehicleMasterView', 9, 1, 0, 0, 0, 0),
        ('RepairMaster', 'Repair Master', 'إدارة الصيانة', (SELECT Id FROM MenuDetails WHERE Name='GeneralMasters'), 'ZakiSAASERP.Shared.Pages.Masters.RepairMasterView', 'RepairMasterView', 10, 1, 0, 0, 0, 0),
        ('TechnicianMaster', 'Technician', 'إدارة الفنيين', (SELECT Id FROM MenuDetails WHERE Name='GeneralMasters'), 'ZakiSAASERP.Shared.Pages.Masters.TechnicianMasterView', 'TechnicianMasterView', 11, 1, 0, 0, 0, 0);

        INSERT INTO MenuDetails (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder) VALUES 
        ('CustomerMaster', 'Customer Master', 'إدارة العملاء', (SELECT Id FROM MenuDetails WHERE Name='Parties'), 'ZakiSAASERP.Shared.Pages.Masters.CustomerMasterView', 'CustomerMasterView', 1, 1, 0, 0, 0, 0),
        ('SupplierMaster', 'Supplier Master', 'إدارة الموردين', (SELECT Id FROM MenuDetails WHERE Name='Parties'), 'ZakiSAASERP.Shared.Pages.Masters.SupplierMasterView', 'SupplierMasterView', 2, 1, 0, 0, 0, 0),
        ('CustomerGroupMaster', 'Customer Group Master', 'إدارة مجموعات العملاء', (SELECT Id FROM MenuDetails WHERE Name='Parties'), 'ZakiSAASERP.Shared.Pages.Masters.CustomerGroupMasterView', 'CustomerGroupMasterView', 3, 1, 0, 0, 0, 0),
        ('CustomerLocationMaster', 'Customer Location', 'مواقع العملاء', (SELECT Id FROM MenuDetails WHERE Name='Parties'), 'ZakiSAASERP.Shared.Pages.Masters.CustomerLocationMasterView', 'CustomerLocationMasterView', 4, 1, 0, 0, 0, 0),
        ('CustomerOpeningEntry', 'Customer Opening Entry', 'إدخال الرصيد الافتتاحي للعملاء', (SELECT Id FROM MenuDetails WHERE Name='Parties'), 'ZakiSAASERP.Shared.Pages.Masters.CustomerOPEntryView', 'CustomerOPEntryView', 5, 1, 0, 0, 0, 0),
        ('SupplierOpeningEntry', 'Supplier Opening Entry', 'إدخال الرصيد الافتتاحي للموردين', (SELECT Id FROM MenuDetails WHERE Name='Parties'), 'ZakiSAASERP.Shared.Pages.Masters.SupplierOPEntryView', 'SupplierOPEntryView', 6, 1, 0, 0, 0, 0);

        INSERT INTO MenuDetails (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder) VALUES
        ('Sales', 'Sales', 'المبيعات', (SELECT Id FROM MenuDetails WHERE Name='Transactions'), NULL, 'ShoppingBag', 1, 1, 0, 0, 0, 0),
        ('Purchase', 'Purchase', 'المشتريات', (SELECT Id FROM MenuDetails WHERE Name='Transactions'), NULL, 'ShoppingBasket', 2, 1, 0, 0, 0, 0),
        ('Inventory_Trans', 'Inventory', 'المخزون', (SELECT Id FROM MenuDetails WHERE Name='Transactions'), NULL, 'FmdGood', 3, 1, 0, 0, 0, 0),
        ('Others', 'Others', 'أخرى', (SELECT Id FROM MenuDetails WHERE Name='Transactions'), NULL, 'CardMembership', 4, 1, 0, 0, 0, 0);

        INSERT INTO MenuDetails (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder) VALUES 
        ('SalesQuotTitle', 'Sales Quotation', 'عرض سعر المبيعات', (SELECT Id FROM MenuDetails WHERE Name='Sales' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Transactions')), 'ZakiSAASERP.Shared.Pages.Transactions.Sales.SalesQuotationView', 'SalesQuotationView', 1, 1, 0, 0, 0, 0),
        ('ProformaInvoiceTitle', 'Proforma Invoice', 'فاتورة مبدئية', (SELECT Id FROM MenuDetails WHERE Name='Sales' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Transactions')), 'ZakiSAASERP.Shared.Pages.Transactions.Sales.ProformaInvoiceView', 'ProformaInvoiceView', 2, 1, 0, 0, 0, 0),
        ('SalesOrderTitle', 'Sales Order', 'أمر بيع', (SELECT Id FROM MenuDetails WHERE Name='Sales' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Transactions')), 'ZakiSAASERP.Shared.Pages.Transactions.Sales.SalesOrderView', 'SalesOrderView', 3, 1, 0, 0, 0, 0),
        ('SalesInvoiceTitle', 'Sales Invoice', 'فاتورة مبيعات', (SELECT Id FROM MenuDetails WHERE Name='Sales' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Transactions')), 'ZakiSAASERP.Shared.Pages.Transactions.Sales.SalesInvoiceView', 'SalesInvoiceView', 4, 1, 0, 0, 0, 0),
        ('SalesInvoiceRtnTitle', 'Sales Invoice Return', 'مرتجع فاتورة مبيعات', (SELECT Id FROM MenuDetails WHERE Name='Sales' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Transactions')), 'ZakiSAASERP.Shared.Pages.Transactions.Sales.SalesInvoiceRtnView', 'SalesInvoiceRtnView', 5, 1, 0, 0, 0, 0),
        ('EnquiryRegistration', 'Enquiry Registration', 'تسجيل الاستفسارات', (SELECT Id FROM MenuDetails WHERE Name='Sales' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Transactions')), 'ZakiSAASERP.Shared.Pages.Transactions.Sales.EnquiryRegistrationView', 'EnquiryRegistrationView', 6, 1, 0, 0, 0, 0),
        ('DeliveryNoteTitle', 'Delivery Note', 'إشعار تسليم', (SELECT Id FROM MenuDetails WHERE Name='Sales' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Transactions')), 'ZakiSAASERP.Shared.Pages.Transactions.Sales.DeliveryNoteView', 'DeliveryNoteView', 7, 1, 0, 0, 0, 0),
        ('Delivery Note Return', 'Delivery Note Return', 'مرتجع إشعار التسليم', (SELECT Id FROM MenuDetails WHERE Name='Sales' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Transactions')), 'ZakiSAASERP.Shared.Pages.Transactions.Sales.DeliveryNoteRtnView', 'DeliveryNoteRtnView', 8, 1, 0, 0, 0, 0);

        INSERT INTO MenuDetails (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder) VALUES 
        ('PurchaseOrderTitle', 'Purchase Order', 'أمر شراء', (SELECT Id FROM MenuDetails WHERE Name='Purchase' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Transactions')), 'ZakiSAASERP.Shared.Pages.Transactions.Purchase.PurchaseOrderView', 'PurchaseOrderView', 1, 1, 0, 0, 0, 0),
        ('PurchaseTitle', 'Purchase', 'المشتريات', (SELECT Id FROM MenuDetails WHERE Name='Purchase' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Transactions')), 'ZakiSAASERP.Shared.Pages.Transactions.Purchase.PurchaseView', 'PurchaseView', 2, 1, 0, 0, 0, 0),
        ('GoodsReceipt', 'Goods Receipt', 'استلام البضائع', (SELECT Id FROM MenuDetails WHERE Name='Purchase' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Transactions')), 'ZakiSAASERP.Shared.Pages.Transactions.Purchase.GoodsReceiptView', 'GoodsReceiptView', 3, 1, 0, 0, 0, 0),
        ('Purchase Return', 'Purchase Return', 'مرتجع مشتريات', (SELECT Id FROM MenuDetails WHERE Name='Purchase' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Transactions')), 'ZakiSAASERP.Shared.Pages.Transactions.Purchase.PurchaseReturnView', 'PurchaseReturnView', 4, 1, 0, 0, 0, 0);

        INSERT INTO MenuDetails (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder) VALUES 
        ('Damage Stock', 'Damage Stock', 'مخزون تالف', (SELECT Id FROM MenuDetails WHERE Name='Inventory_Trans' ), 'ZakiSAASERP.Shared.Pages.Transactions.Inventory.DamageStockView', 'DamageStockView', 1, 1, 0, 0, 0, 0),
        ('StockAdjustment', 'Stock Adjustment', 'تسوية المخزون', (SELECT Id FROM MenuDetails WHERE Name='Inventory_Trans'), 'ZakiSAASERP.Shared.Pages.Transactions.Inventory.StockAdjustmentView', 'StockAdjustmentView', 2, 1, 0, 0, 0, 0),
        ('MaterialTransferRequest', 'Material Transfer Request', 'طلب نقل المواد', (SELECT Id FROM MenuDetails WHERE Name='Inventory_Trans'), 'ZakiSAASERP.Shared.Pages.Transactions.Inventory.MaterialTransferRequestView', 'MaterialTransferRequestView', 3, 1, 0, 0, 0, 0),
        ('MaterialTransferApproval', 'Material Transfer Approval', 'اعتماد نقل المواد', (SELECT Id FROM MenuDetails WHERE Name='Inventory_Trans'), 'ZakiSAASERP.Shared.Pages.Transactions.Inventory.MaterialTransferApprovalView', 'MaterialTransferApprovalView', 4, 1, 0, 0, 0, 0),
        ('MaterialTransferIN', 'Material Transfer IN', 'إدخال نقل المواد', (SELECT Id FROM MenuDetails WHERE Name='Inventory_Trans'), 'ZakiSAASERP.Shared.Pages.Transactions.Inventory.MaterialTransferInView', 'MaterialTransferInView', 5, 1, 0, 0, 0, 0),
        ('MaterialTransferOUT', 'Material Transfer OUT', 'إخراج نقل المواد', (SELECT Id FROM MenuDetails WHERE Name='Inventory_Trans'), 'ZakiSAASERP.Shared.Pages.Transactions.Inventory.MaterialTransferOutView', 'MaterialTransferOutView', 6, 1, 0, 0, 0, 0),
        ('InternalMaterialTransfer', 'Internal Material Transfer', 'نقل المواد الداخلي', (SELECT Id FROM MenuDetails WHERE Name='Inventory_Trans'), 'ZakiSAASERP.Shared.Pages.Transactions.Inventory.InternalMaterialTransferView', 'InternalMaterialTransferView', 7, 1, 0, 0, 0, 0);

        INSERT INTO MenuDetails (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder) VALUES 
        ('JobOrder', 'Job Order', 'أمر عمل', (SELECT Id FROM MenuDetails WHERE Name='Others' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Transactions')), 'ZakiSAASERP.Shared.Pages.Transactions.Workshop.JobOrderView', 'JobOrderView', 1, 1, 0, 0, 0, 0),
        ('JobCard', 'Job Card', 'بطاقة عمل', (SELECT Id FROM MenuDetails WHERE Name='Others' AND ParentId=(SELECT Id FROM MenuDetails WHERE Name='Transactions')), 'ZakiSAASERP.Shared.Pages.Transactions.Workshop.JobCardView', 'JobCardView', 2, 1, 0, 0, 0, 0);

        INSERT INTO MenuDetails (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder) VALUES 
        ('JournalEntry', 'Journal Entry', 'قيد يومية', (SELECT Id FROM MenuDetails WHERE Name='Accounting'), 'ZakiSAASERP.Shared.Pages.Transactions.Accounts.JournalEntryView', 'JournalEntryView', 1, 1, 0, 0, 0, 0),
        ('CustomerReceipt', 'Customer Receipt', 'سند قبض العملاء', (SELECT Id FROM MenuDetails WHERE Name='Accounting'), 'ZakiSAASERP.Shared.Pages.Transactions.Accounts.CustomerReceiptView', 'CustomerReceiptView', 2, 1, 0, 0, 0, 0),
        ('ExpenseEntry', 'Expense Entry', 'قيد مصروفات', (SELECT Id FROM MenuDetails WHERE Name='Accounting'), 'ZakiSAASERP.Shared.Pages.Transactions.Accounts.ExpenseEntryView', 'ExpenseEntryView', 3, 1, 0, 0, 0, 0),
        ('AccountOpeningEntry', 'Account Opening Entry', 'قيد افتتاحي للحسابات', (SELECT Id FROM MenuDetails WHERE Name='Accounting'), 'ZakiSAASERP.Shared.Pages.Transactions.Accounts.AccountOpeningEntryView', 'AccountOpeningEntryView', 4, 1, 0, 0, 0, 0),
        ('SupplierPayment', 'Supplier Payment', 'سند دفع الموردين', (SELECT Id FROM MenuDetails WHERE Name='Accounting'), 'ZakiSAASERP.Shared.Pages.Transactions.Accounts.SupplierPaymentView', 'SupplierPaymentView', 5, 1, 0, 0, 0, 0);

        INSERT INTO MenuDetails (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder) VALUES
        ('Rpt_Sales', 'Sales Reports', 'تقارير المبيعات', (SELECT Id FROM MenuDetails WHERE Name='Reports'), NULL, 'ShoppingBag', 1, 1, 0, 0, 0, 0),
        ('Rpt_Monitor', 'Monitor Reports', 'تقارير المراقبة', (SELECT Id FROM MenuDetails WHERE Name='Reports'), NULL, 'Monitor', 2, 1, 0, 0, 0, 0),
        ('Rpt_Supplier', 'Supplier Reports', 'تقارير الموردين', (SELECT Id FROM MenuDetails WHERE Name='Reports'), NULL, 'AssignmentInd', 3, 1, 0, 0, 0, 0),
        ('Rpt_Customer', 'Customer Reports', 'تقارير العملاء', (SELECT Id FROM MenuDetails WHERE Name='Reports'), NULL, 'Person', 4, 1, 0, 0, 0, 0),
        ('Rpt_Purchase', 'Purchase Reports', 'تقارير المشتريات', (SELECT Id FROM MenuDetails WHERE Name='Reports'), NULL, 'ShoppingBasket', 5, 1, 0, 0, 0, 0),
        ('Rpt_Inventory', 'Inventory Reports', 'تقارير المخزون', (SELECT Id FROM MenuDetails WHERE Name='Reports'), NULL, 'FmdGood', 6, 1, 0, 0, 0, 0),
        ('Rpt_Accounts', 'Accounts Reports', 'تقارير الحسابات', (SELECT Id FROM MenuDetails WHERE Name='Reports'), NULL, 'AccountBox', 7, 1, 0, 0, 0, 0),
        ('Rpt_Enquiry', 'Enquiry Reports', 'تقارير الاستفسارات', (SELECT Id FROM MenuDetails WHERE Name='Reports'), NULL, 'Assignment', 8, 1, 0, 0, 0, 0);

        INSERT INTO MenuDetails (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder) VALUES 
        ('Sales Report', 'Sales Report', 'تقرير المبيعات', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Sales'), 'ZakiSAASERP.Shared.Pages.Reports.Sales.SalesReportsView', 'SalesReportsView', 1, 1, 0, 0, 0, 0),
        ('Sales Return Report', 'Sales Return Report', 'تقرير إرجاع المبيعات', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Sales'), 'ZakiSAASERP.Shared.Pages.Reports.Sales.SalesReturnReportView', 'SalesReturnReportView', 12, 1, 0, 0, 0, 0),
        ('Sales Details Report', 'Sales Details Report', 'تقرير تفاصيل المبيعات', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Sales'), 'ZakiSAASERP.Shared.Pages.Reports.Sales.SalesDetailsView', 'SalesDetailsView', 2, 1, 0, 0, 0, 0),
        ('Sales Return Details Report', 'Sales Return Details Report', 'تقرير تفاصيل مرتجعات المبيعات', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Sales'), 'ZakiSAASERP.Shared.Pages.Reports.Sales.SalesReturnDetailsReportView', 'SalesReturnDetailsReportView', 13, 1, 0, 0, 0, 0),
        ('Customer Wise Sales Report', 'Customer Wise Sales Report', 'تقرير المبيعات حسب العميل', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Sales'), 'ZakiSAASERP.Shared.Pages.Reports.Sales.CustomerWiseSalesReportsView', 'CustomerWiseSalesReportsView', 3, 1, 0, 0, 0, 0),
        ('Category Wise Report', 'Category Wise Report', 'تقرير حسب الفئة', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Sales'), 'ZakiSAASERP.Shared.Pages.Reports.Sales.CategoryWiseReportView', 'CategoryWiseReportView', 4, 1, 0, 0, 0, 0),
        ('Sales Quotation Register', 'Sales Quotation Register', 'سجل عروض أسعار المبيعات', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Sales'), 'ZakiSAASERP.Shared.Pages.Reports.Sales.SalesQuotationRegisterView', 'SalesQuotationRegisterView', 5, 1, 0, 0, 0, 0),
        ('Customer Wise Quotation Report', 'Customer Wise Quotation Report', 'تقرير عروض الأسعار حسب العميل', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Sales'), 'ZakiSAASERP.Shared.Pages.Reports.Sales.CustomerWiseQuotationReportView', 'CustomerWiseQuotationReportView', 6, 1, 0, 0, 0, 0),
        ('Sales Order Register', 'Sales Order Register', 'سجل أوامر البيع', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Sales'), 'ZakiSAASERP.Shared.Pages.Reports.Sales.SalesOrderReportView', 'SalesOrderReportView', 7, 1, 0, 0, 0, 0),
        ('Delivery Note Register', 'Delivery Note Register', 'سجل إشعارات التسليم', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Sales'), 'ZakiSAASERP.Shared.Pages.Reports.Sales.DeliveryNoteRegisterView', 'DeliveryNoteRegisterView', 8, 1, 0, 0, 0, 0),
        ('Date Wise Profit Report', 'Date Wise Profit Report', 'تقرير الأرباح حسب التاريخ', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Sales'), 'ZakiSAASERP.Shared.Pages.Reports.Reports.DateWiseProfitReportView', 'DateWiseProfitReportView', 9, 1, 0, 0, 0, 0),
        ('Proforma Invoice Report', 'Proforma Invoice Report', 'تقرير الفاتورة المبدئية', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Sales'), 'ZakiSAASERP.Shared.Pages.Reports.Sales.ProformaInvoiceReportView', 'ProformaInvoiceReportView', 10, 1, 0, 0, 0, 0),
        ('Vat Report', 'Vat Report', 'تقرير ضريبة القيمة المضافة', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Sales'), 'ZakiSAASERP.Shared.Pages.Reports.Sales.VatReportView', 'VatReportView', 11, 1, 0, 0, 0, 0),
        ('Shift Closing Report', 'Shift Closing Report', 'تقرير إغلاق الوردية', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Sales'), 'ZakiSAASERP.Shared.Pages.Reports.Sales.ShiftClosingReportView', 'ShiftClosingReportView', 14, 1, 0, 0, 0, 0);

        INSERT INTO MenuDetails (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder) VALUES 
        ('Sales Monitor Report', 'Sales Monitor Report', 'تقرير مراقبة المبيعات', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Monitor'), 'ZakiSAASERP.Shared.Pages.Reports.Transactions.SalesMonitorReport', 'SalesMonitorReport', 1, 1, 0, 0, 0, 0),
        ('Purchase Monitor Report', 'Purchase Monitor Report', 'تقرير مراقبة المشتريات', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Monitor'), 'ZakiSAASERP.Shared.Pages.Reports.Transactions.PurchaseMonitorReport', 'PurchaseMonitorReport', 2, 1, 0, 0, 0, 0),
        ('Transaction Monitor Report', 'Transaction Monitor Report', 'تقرير مراقبة المعاملات', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Monitor'), 'ZakiSAASERP.Shared.Pages.Reports.Transactions.TransactionMonitorReport', 'TransactionMonitorReport', 3, 1, 0, 0, 0, 0);

        INSERT INTO MenuDetails (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder) VALUES 
        ('Supplier Statement', 'Supplier Statement', 'كشف حساب المورد', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Supplier'), 'ZakiSAASERP.Shared.Pages.Reports.Accounts.SupplierStatementReport', 'SupplierStatementReport', 1, 1, 0, 0, 0, 0),
        ('Supplier Pending Invoices', 'Supplier Pending Invoices', 'فواتير الموردين المعلقة', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Supplier'), 'ZakiSAASERP.Shared.Pages.Reports.Accounts.SupplierPendingInvoiceReports', 'SupplierPendingInvoiceReports', 2, 1, 0, 0, 0, 0),
        ('Supplier Summary', 'Supplier Summary', 'ملخص الموردين', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Supplier'), 'ZakiSAASERP.Shared.Pages.Reports.Accounts.SupplierSummaryReport', 'SupplierSummaryReport', 3, 1, 0, 0, 0, 0),
        ('Supplier List', 'Supplier List', 'قائمة الموردين', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Supplier'), 'ZakiSAASERP.Shared.Pages.Reports.Accounts.SupplierListReport', 'SupplierListReport', 4, 1, 0, 0, 0, 0),
        ('Supplier Payment Report', 'Supplier Payment Report', 'تقرير مدفوعات الموردين', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Supplier'), 'ZakiSAASERP.Shared.Pages.Reports.Accounts.SupplierPaymentReports', 'SupplierPaymentReports', 5, 1, 0, 0, 0, 0);

        INSERT INTO MenuDetails (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder) VALUES 
        ('Customer Statement', 'Customer Statement', 'كشف حساب العميل', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Customer'), 'ZakiSAASERP.Shared.Pages.Reports.Sales.CustomerStatementView', 'CustomerStatementView', 1, 1, 0, 0, 0, 0),
        ('Customer Summary', 'Customer Summary', 'ملخص العملاء', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Customer'), 'ZakiSAASERP.Shared.Pages.Reports.Accounts.CustomerSummaryReport', 'CustomerSummaryReport', 2, 1, 0, 0, 0, 0),
        ('Customer Pending Invoice Report', 'Customer Pending Invoice Report', 'فواتير العملاء المعلقة', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Customer'), 'ZakiSAASERP.Shared.Pages.Reports.Accounts.CustomerPendingInvoiceReports', 'CustomerPendingInvoiceReports', 3, 1, 0, 0, 0, 0),
        ('Customer List', 'Customer List', 'قائمة العملاء', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Customer'), 'ZakiSAASERP.Shared.Pages.Reports.Accounts.CustomerListReport', 'CustomerListReport', 4, 1, 0, 0, 0, 0);

        INSERT INTO MenuDetails (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder) VALUES 
        ('Purchase Summary', 'Purchase Summary', 'ملخص المشتريات', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Purchase'), 'ZakiSAASERP.Shared.Pages.Reports.Purchase.PurchaseSummaryView', 'PurchaseSummaryView', 1, 1, 0, 0, 0, 0),
        ('Purchase Details', 'Purchase Details', 'تفاصيل المشتريات', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Purchase'), 'ZakiSAASERP.Shared.Pages.Reports.Purchase.PurchaseDetailsView', 'PurchaseDetailsView', 2, 1, 0, 0, 0, 0),
        ('Purchase Order Report', 'Purchase Order Report', 'تقرير أوامر الشراء', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Purchase'), 'ZakiSAASERP.Shared.Pages.Reports.Purchase.PurchaseOrderReportView', 'PurchaseOrderReportView', 3, 1, 0, 0, 0, 0),
        ('Purchase Return Summary', 'Purchase Return Summary', 'ملخص إرجاع الشراء', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Purchase'), 'ZakiSAASERP.Shared.Pages.Reports.Purchase.PurchaseReturnSummaryView', 'PurchaseReturnSummaryView', 4, 1, 0, 0, 0, 0),
        ('Purchase Return Details', 'Purchase Return Details', 'تفاصيل إرجاع الشراء', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Purchase'), 'ZakiSAASERP.Shared.Pages.Reports.Purchase.PurchaseReturnDetailsView', 'PurchaseReturnDetailsView', 5, 1, 0, 0, 0, 0);

        INSERT INTO MenuDetails (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder) VALUES 
        ('Stock Report', 'Stock Report', 'تقرير المخزون', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Inventory'), 'ZakiSAASERP.Shared.Pages.Reports.Inventory.StockReportView', 'StockReportView', 1, 1, 0, 0, 0, 0),
        ('Stock Transfer Report', 'Stock Transfer Report', 'تقرير نقل المخزون', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Inventory'), 'ZakiSAASERP.Shared.Pages.Reports.Inventory.StockTransferReportView', 'StockTransferReportView', 2, 1, 0, 0, 0, 0),
        ('Item Transaction History Report', 'Item Transaction History Report', 'تقرير سجل حركات الأصناف', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Inventory'), 'ZakiSAASERP.Shared.Pages.Reports.Inventory.ItemTransactionHistoryReportView', 'ItemTransactionHistoryReportView', 3, 1, 0, 0, 0, 0),
        ('Stock Adjustment Report', 'Stock Adjustment Report', 'تقرير تسوية المخزون', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Inventory'), 'ZakiSAASERP.Shared.Pages.Reports.Inventory.StockAdjustmentView', 'StockAdjustmentView', 4, 1, 0, 0, 0, 0);

        INSERT INTO MenuDetails (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder) VALUES 
        ('Journal Entry Report', 'Journal Entry Report', 'تقرير قيود اليومية', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Accounts'), 'ZakiSAASERP.Shared.Pages.Reports.Accounts.JournalEntryReportView', 'JournalEntryReportView', 1, 1, 0, 0, 0, 0),
        ('Trial Balance Report', 'Trial Balance Report', 'تقرير ميزان المراجعة', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Accounts'), 'ZakiSAASERP.Shared.Pages.Reports.Accounts.TrialBalanceView', 'TrialBalanceView', 2, 1, 0, 0, 0, 0),
        ('Expense Report', 'Expense Report', 'تقرير المصروفات', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Accounts'), 'ZakiSAASERP.Shared.Pages.Reports.Accounts.ExpenseReportView', 'ExpenseReportView', 3, 1, 0, 0, 0, 0),
        ('Profit And Loss Report', 'Profit And Loss Report', 'تقرير الأرباح والخسائر', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Accounts'), 'ZakiSAASERP.Shared.Pages.Reports.Accounts.ProfitAndLossReportView', 'ProfitAndLossReportView', 4, 1, 0, 0, 0, 0),
        ('Balance Sheet Report', 'Balance Sheet Report', 'تقرير الميزانية العمومية', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Accounts'), 'ZakiSAASERP.Shared.Pages.Reports.Accounts.BalanceSheetReportView', 'BalanceSheetReportView', 5, 1, 0, 0, 0, 0),
        ('Chart Of Account Report', 'Chart Of Account Report', 'تقرير دليل الحسابات', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Accounts'), 'ZakiSAASERP.Shared.Pages.Reports.Accounts.ChartOfAccountReportView', 'ChartOfAccountReportView', 6, 1, 0, 0, 0, 0),
        ('LedgerReport', 'Ledger Report', 'تقرير دفتر الأستاذ', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Accounts'), 'ZakiSAASERP.Shared.Pages.Reports.Accounts.LedgerReportView', 'LedgerReportView', 7, 1, 0, 0, 0, 0);

        INSERT INTO MenuDetails (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder) VALUES 
        ('EnquiryRegistrationReport', 'Enquiry Registration Report', 'تقرير تسجيل الاستفسارات', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Enquiry'), 'ZakiSAASERP.Shared.Pages.Reports.Sales.EnquiryRegistrationReportView', 'EnquiryRegistrationReportView', 1, 1, 0, 0, 0, 0);

        INSERT INTO MenuDetails (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder) VALUES 
        ('PendingZatca', 'Zatca Report', 'تقرير الزكاة والضريبة (زاتكا)', (SELECT Id FROM MenuDetails WHERE Name='Rpt_Sales'), 'ZakiSAASERP.Shared.Pages.Reports.Transaction.ZatcaPendingView', 'ZatcaPendingView', 1, 1, 0, 0, 0, 0);

        INSERT INTO MenuDetails (Name, MenuCaption, ArabicCaption, ParentId, AssemblyName, ClassName, Position, Active, IsFavorite, IsFavorite2, FilterOneOrder, FilterTwoOrder) VALUES 
        ('ManageUser', 'Manage User', 'إدارة المستخدمين', (SELECT Id FROM MenuDetails WHERE Name='Settings'), 'ZakiSAASERP.Shared.Pages.Users.UserView', 'UserView', 1, 1, 0, 0, 0, 0),
        ('PrintDesignTitle', 'Print Design', 'تصميم الطباعة', (SELECT Id FROM MenuDetails WHERE Name='Settings'), 'ZakiSAASERP.Shared.Pages.Others.PrintDesignerView', 'PrintDesignerView', 2, 1, 0, 0, 0, 0),
        ('Barcode Design', 'Barcode Design', 'تصميم الباركود', (SELECT Id FROM MenuDetails WHERE Name='Settings'), 'ZakiSAASERP.Shared.Pages.Others.BarcodeDesignerView', 'BarcodeDesignerView', 3, 1, 0, 0, 0, 0),
        ('CompanyDetails', 'Company Details', 'بيانات الشركة', (SELECT Id FROM MenuDetails WHERE Name='Settings'), 'ZakiSAASERP.Shared.Pages.Settings.CompanyDetailsView', 'CompanyDetailsView', 4, 1, 0, 0, 0, 0),
        ('ZATCA CSR', 'ZATCA CSR', 'زاتكا CSR', (SELECT Id FROM MenuDetails WHERE Name='Settings'), 'ZakiSAASERP.Shared.Pages.Settings.ZatcaCSRView', 'ZatcaCSRView', 5, 1, 0, 0, 0, 0),
        ('User Previlege', 'User Previlege', 'صلاحيات المستخدمين', (SELECT Id FROM MenuDetails WHERE Name='Settings'), 'ZakiSAASERP.Shared.Pages.Users.UserPrevilageView', 'UserPrevilageView', 6, 1, 0, 0, 0, 0),
        ('ZATCA Pending', 'ZATCA Pending Invoices', 'فواتير زاتكا المعلقة', (SELECT Id FROM MenuDetails WHERE Name='Settings'), 'ZakiSAASERP.Shared.Pages.Settings.ZatcaPendingUploadView', 'ZatcaPendingUploadView', 7, 1, 0, 0, 0, 0),
        ('ProductImport', 'Product Import', 'استيراد المنتجات', (SELECT Id FROM MenuDetails WHERE Name='Settings'), 'ZakiSAASERP.Shared.Pages.Settings.ProductImportView', 'ProductImportView', 8, 1, 0, 0, 0, 0),
        ('ProductExport', 'Product Export', 'تصدير المنتجات', (SELECT Id FROM MenuDetails WHERE Name='Settings'), 'ZakiSAASERP.Shared.Pages.Settings.ProductExportView', 'ProductExportView', 9, 1, 0, 0, 0, 0),
        ('PrintingBarcode', 'Printing Barcode', 'طباعة الباركود', (SELECT Id FROM MenuDetails WHERE Name='Settings'), 'ZakiSAASERP.Shared.Pages.Settings.PrintingBarcodeView', 'PrintingBarcodeView', 10, 1, 0, 0, 0, 0);";
    }

    #endregion
}
