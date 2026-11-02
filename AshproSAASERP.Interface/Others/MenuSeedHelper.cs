using AshproSAASERP.Models.Settings;
using AshproSAASERP.Models.User;
using Microsoft.EntityFrameworkCore;

namespace AshproSAASERP.Interface.Others;

public static class MenuSeedHelper
{
    public static readonly (string Name, string MenuCaption, string ArabicCaption, string ParentName, string? AssemblyName, string? ClassName, int Position)[] AllSystemMenus = new[]
    {
        // ================= ROOT MENUS =================
        ("Masters", "Masters", "البيانات الأساسية", "", null, "AddCircle", 1),
        ("Transactions", "Transactions", "المعاملات", "", null, "ShoppingCart", 2),
        ("Accounting", "Accounting", "المحاسبة", "", null, "AccountBalance", 3),
        ("Reports", "Reports", "التقارير", "", null, "Assessment", 4),
        ("Settings", "Settings", "الإعدادات", "", null, "Settings", 5),

        // ================= MASTERS SUB-GROUPS =================
        ("Inventory", "Inventory", "المخزون", "Masters", null, "Inventory", 1),
        ("Accounts", "Accounts", "الحسابات", "Masters", null, "AccountBalance", 2),
        ("GeneralMasters", "General Masters", "الإعدادات العامة", "Masters", null, "SettingsApplications", 3),
        ("Parties", "Parties", "الأطراف", "Masters", null, "People", 4),

        // Inventory Masters
        ("Unit Master", "Unit Master", "إدارة الوحدات", "Inventory", "AshproSAASERP.Shared.Pages.Masters.UnitMasterView", "UnitMasterView", 1),
        ("CategoryMaster", "Category Master", "إدارة الفئات", "Inventory", "AshproSAASERP.Shared.Pages.Masters.CategoryMasterView", "CategoryMasterView", 2),
        ("SubCategoryMaster", "Sub Category", "إدارة الفئات الفرعية", "Inventory", "AshproSAASERP.Shared.Pages.Masters.SubCategoryMasterView", "SubCategoryMasterView", 3),
        ("BrandMaster", "Brand Master", "إدارة العلامات التجارية", "Inventory", "AshproSAASERP.Shared.Pages.Masters.BrandMastersView", "BrandMastersView", 4),
        ("ItemMaster", "Item Master", "إدارة الأصناف", "Inventory", "AshproSAASERP.Shared.Pages.Masters.ItemMasterView", "ItemMasterView", 5),
        ("StoreMaster", "Store Master", "إدارة المخازن", "Inventory", "AshproSAASERP.Shared.Pages.Masters.StoreMasterView", "StoreMasterView", 6),
        ("OpeningStockEntry", "Opening Stock Entry", "إدخل الرصيد الافتتاحي", "Inventory", "AshproSAASERP.Shared.Pages.Masters.OpeningStockEntryView", "OpeningStockEntryView", 7),

        // Accounts Masters
        ("AccountHeadTitle", "Account Head", "رؤوس الحسابات", "Accounts", "AshproSAASERP.Shared.Pages.Masters.AccountHeadView", "AccountHeadView", 1),
        ("AccountGroup", "Account Group", "مجموعات الحسابات", "Accounts", "AshproSAASERP.Shared.Pages.Masters.AccountGroupView", "AccountGroupView", 2),
        ("AccountSubGroup", "Account SubGroup", "المجموعات الفرعية للحسابات", "Accounts", "AshproSAASERP.Shared.Pages.Masters.AccountSubGroupView", "AccountSubGroupView", 3),
        ("AccountsPage", "Accounts", "الحسابات", "Accounts", "AshproSAASERP.Shared.Pages.Masters.AccountsView", "AccountsView", 4),

        // General Masters
        ("BranchTitle", "Branch", "الفروع", "GeneralMasters", "AshproSAASERP.Shared.Pages.Masters.BranchView", "BranchView", 1),
        ("FinancilaYearTitle", "Financial Year", "السنة المالية", "GeneralMasters", "AshproSAASERP.Shared.Pages.Masters.FinancialYearView", "FinancialYearView", 2),
        ("SalesManMaster", "Salesman Master", "إدارة مندوبي المبيعات", "GeneralMasters", "AshproSAASERP.Shared.Pages.Masters.SalesManMasterView", "SalesManMasterView", 3),
        ("PurchaserMaster", "Purchaser Master", "إدارة مسؤولي المشتريات", "GeneralMasters", "AshproSAASERP.Shared.Pages.Masters.PurchaserMasterView", "PurchaserMasterView", 4),
        ("CountryMaster", "Country Master", "إدارة الدول", "GeneralMasters", "AshproSAASERP.Shared.Pages.Masters.CountryMasterView", "CountryMasterView", 5),
        ("VatMaster", "Vat Master", "إدارة ضريبة القيمة المضافة", "GeneralMasters", "AshproSAASERP.Shared.Pages.Masters.VatMasterView", "VatMasterView", 6),
        ("CurrencyMaster", "CurrencyMaster", "إدارة العملات", "GeneralMasters", "AshproSAASERP.Shared.Pages.Masters.CurrencyMasterView", "CurrencyMasterView", 7),
        ("ProjectMaster", "Project Master", "إدارة المشاريع", "GeneralMasters", "AshproSAASERP.Shared.Pages.Masters.ProjectMasterView", "ProjectMasterView", 8),
        ("VehicleMaster", "Vehicle Master", "إدارة المركبات", "GeneralMasters", "AshproSAASERP.Shared.Pages.Masters.VehicleMasterView", "VehicleMasterView", 9),
        ("RepairMaster", "Repair Master", "إدارة الصيانة", "GeneralMasters", "AshproSAASERP.Shared.Pages.Masters.RepairMasterView", "RepairMasterView", 10),
        ("TechnicianMaster", "Technician", "إدارة الفنيين", "GeneralMasters", "AshproSAASERP.Shared.Pages.Masters.TechnicianMasterView", "TechnicianMasterView", 11),

        // Parties
        ("CustomerMaster", "Customer Master", "إدارة العملاء", "Parties", "AshproSAASERP.Shared.Pages.Masters.CustomerMasterView", "CustomerMasterView", 1),
        ("SupplierMaster", "Supplier Master", "إدارة الموردين", "Parties", "AshproSAASERP.Shared.Pages.Masters.SupplierMasterView", "SupplierMasterView", 2),
        ("CustomerGroupMaster", "Customer Group Master", "إدارة مجموعات العملاء", "Parties", "AshproSAASERP.Shared.Pages.Masters.CustomerGroupMasterView", "CustomerGroupMasterView", 3),
        ("CustomerLocationMaster", "Customer Location", "مواقع العملاء", "Parties", "AshproSAASERP.Shared.Pages.Masters.CustomerLocationMasterView", "CustomerLocationMasterView", 4),
        ("CustomerOpeningEntry", "Customer Opening Entry", "إدخال الرصيد الافتتاحي للعملاء", "Parties", "AshproSAASERP.Shared.Pages.Masters.CustomerOPEntryView", "CustomerOPEntryView", 5),
        ("SupplierOpeningEntry", "Supplier Opening Entry", "إدخال الرصيد الافتتاحي للموردين", "Parties", "AshproSAASERP.Shared.Pages.Masters.SupplierOPEntryView", "SupplierOPEntryView", 6),

        // ================= TRANSACTIONS =================
        ("Sales", "Sales", "المبيعات", "Transactions", null, "ShoppingBag", 1),
        ("Purchase", "Purchase", "المشتريات", "Transactions", null, "ShoppingBasket", 2),
        ("Inventory_Trans", "Inventory", "المخزون", "Transactions", null, "FmdGood", 3),
        ("Others", "Others", "أخرى", "Transactions", null, "CardMembership", 4),

        // Sales Transactions
        ("SalesQuotTitle", "Sales Quotation", "عرض سعر المبيعات", "Sales", "AshproSAASERP.Shared.Pages.Transactions.Sales.SalesQuotationView", "SalesQuotationView", 1),
        ("ProformaInvoiceTitle", "Proforma Invoice", "فاتورة مبدئية", "Sales", "AshproSAASERP.Shared.Pages.Transactions.Sales.ProformaInvoiceView", "ProformaInvoiceView", 2),
        ("SalesOrderTitle", "Sales Order", "أمر بيع", "Sales", "AshproSAASERP.Shared.Pages.Transactions.Sales.SalesOrderView", "SalesOrderView", 3),
        ("SalesInvoiceTitle", "Sales Invoice", "فاتورة مبيعات", "Sales", "AshproSAASERP.Shared.Pages.Transactions.Sales.SalesInvoiceView", "SalesInvoiceView", 4),
        ("SalesInvoiceRtnTitle", "Sales Invoice Return", "مرتجع فاتورة مبيعات", "Sales", "AshproSAASERP.Shared.Pages.Transactions.Sales.SalesInvoiceRtnView", "SalesInvoiceRtnView", 5),
        ("EnquiryRegistration", "Enquiry Registration", "تسجيل الاستفسارات", "Sales", "AshproSAASERP.Shared.Pages.Transactions.Sales.EnquiryRegistrationView", "EnquiryRegistrationView", 6),
        ("DeliveryNoteTitle", "Delivery Note", "إشعار تسليم", "Sales", "AshproSAASERP.Shared.Pages.Transactions.Sales.DeliveryNoteView", "DeliveryNoteView", 7),
        ("Delivery Note Return", "Delivery Note Return", "مرتجع إشعار التسليم", "Sales", "AshproSAASERP.Shared.Pages.Transactions.Sales.DeliveryNoteRtnView", "DeliveryNoteRtnView", 8),

        // Purchase Transactions
        ("PurchaseOrderTitle", "Purchase Order", "أمر شراء", "Purchase", "AshproSAASERP.Shared.Pages.Transactions.Purchase.PurchaseOrderView", "PurchaseOrderView", 1),
        ("PurchaseTitle", "Purchase", "المشتريات", "Purchase", "AshproSAASERP.Shared.Pages.Transactions.Purchase.PurchaseView", "PurchaseView", 2),
        ("GoodsReceipt", "Goods Receipt", "استلام البضائع", "Purchase", "AshproSAASERP.Shared.Pages.Transactions.Purchase.GoodsReceiptView", "GoodsReceiptView", 3),
        ("Purchase Return", "Purchase Return", "مرتجع مشتريات", "Purchase", "AshproSAASERP.Shared.Pages.Transactions.Purchase.PurchaseReturnView", "PurchaseReturnView", 4),

        // Inventory Transactions
        ("Damage Stock", "Damage Stock", "مخزون تالف", "Inventory_Trans", "AshproSAASERP.Shared.Pages.Transactions.Inventory.DamageStockView", "DamageStockView", 1),
        ("StockAdjustment", "Stock Adjustment", "تسوية المخزون", "Inventory_Trans", "AshproSAASERP.Shared.Pages.Transactions.Inventory.StockAdjustmentView", "StockAdjustmentView", 2),
        ("MaterialTransferRequest", "Material Transfer Request", "طلب نقل المواد", "Inventory_Trans", "AshproSAASERP.Shared.Pages.Transactions.Inventory.MaterialTransferRequestView", "MaterialTransferRequestView", 3),
        ("MaterialTransferApproval", "Material Transfer Approval", "اعتماد نقل المواد", "Inventory_Trans", "AshproSAASERP.Shared.Pages.Transactions.Inventory.MaterialTransferApprovalView", "MaterialTransferApprovalView", 4),
        ("MaterialTransferIN", "Material Transfer IN", "إدخال نقل المواد", "Inventory_Trans", "AshproSAASERP.Shared.Pages.Transactions.Inventory.MaterialTransferInView", "MaterialTransferInView", 5),
        ("MaterialTransferOUT", "Material Transfer OUT", "إخراج نقل المواد", "Inventory_Trans", "AshproSAASERP.Shared.Pages.Transactions.Inventory.MaterialTransferOutView", "MaterialTransferOutView", 6),
        ("InternalMaterialTransfer", "Internal Material Transfer", "نقل المواد الداخلي", "Inventory_Trans", "AshproSAASERP.Shared.Pages.Transactions.Inventory.InternalMaterialTransferView", "InternalMaterialTransferView", 7),

        // Workshop / Others Transactions
        ("JobOrder", "Job Order", "أمر عمل", "Others", "AshproSAASERP.Shared.Pages.Transactions.Workshop.JobOrderView", "JobOrderView", 1),
        ("JobCard", "Job Card", "بطاقة عمل", "Others", "AshproSAASERP.Shared.Pages.Transactions.Workshop.JobCardView", "JobCardView", 2),

        // ================= ACCOUNTING =================
        ("JournalEntry", "Journal Entry", "قيد يومية", "Accounting", "AshproSAASERP.Shared.Pages.Transactions.Accounts.JournalEntryView", "JournalEntryView", 1),
        ("CustomerReceipt", "Customer Receipt", "سند قبض العملاء", "Accounting", "AshproSAASERP.Shared.Pages.Transactions.Accounts.CustomerReceiptView", "CustomerReceiptView", 2),
        ("ExpenseEntry", "Expense Entry", "قيد مصروفات", "Accounting", "AshproSAASERP.Shared.Pages.Transactions.Accounts.ExpenseEntryView", "ExpenseEntryView", 3),
        ("AccountOpeningEntry", "Account Opening Entry", "قيد افتتاحي للحسابات", "Accounting", "AshproSAASERP.Shared.Pages.Transactions.Accounts.AccountOpeningEntryView", "AccountOpeningEntryView", 4),
        ("SupplierPayment", "Supplier Payment", "سند دفع الموردين", "Accounting", "AshproSAASERP.Shared.Pages.Transactions.Accounts.SupplierPaymentView", "SupplierPaymentView", 5),

        // ================= REPORTS =================
        ("Rpt_Sales", "Sales Reports", "تقارير المبيعات", "Reports", null, "ShoppingBag", 1),
        ("Rpt_Monitor", "Monitor Reports", "تقارير المراقبة", "Reports", null, "Monitor", 2),
        ("Rpt_Supplier", "Supplier Reports", "تقارير الموردين", "Reports", null, "AssignmentInd", 3),
        ("Rpt_Customer", "Customer Reports", "تقارير العملاء", "Reports", null, "Person", 4),
        ("Rpt_Purchase", "Purchase Reports", "تقارير المشتريات", "Reports", null, "ShoppingBasket", 5),
        ("Rpt_Inventory", "Inventory Reports", "تقارير المخزون", "Reports", null, "FmdGood", 6),
        ("Rpt_Accounts", "Accounts Reports", "تقارير الحسابات", "Reports", null, "AccountBox", 7),
        ("Rpt_Enquiry", "Enquiry Reports", "تقارير الاستفسارات", "Reports", null, "Assignment", 8),

        // Report Sub-items
        ("PendingZatca", "Zatca Report", "تقرير الزكاة والضريبة (زاتكا)", "Rpt_Sales", "AshproSAASERP.Shared.Pages.Reports.Transaction.ZatcaPendingView", "ZatcaPendingView", 1),
        ("EnquiryRegistrationReport", "Enquiry Registration Report", "تقرير تسجيل الاستفسارات", "Rpt_Enquiry", "AshproSAASERP.Shared.Pages.Reports.Sales.EnquiryRegistrationReportView", "EnquiryRegistrationReportView", 1),

        // ================= SETTINGS =================
        ("ManageUser", "Manage User", "إدارة المستخدمين", "Settings", "AshproSAASERP.Shared.Pages.Users.UserView", "UserView", 1),
        ("PrintDesignTitle", "Print Design", "تصميم الطباعة", "Settings", "AshproSAASERP.Shared.Pages.Others.PrintDesignerView", "PrintDesignerView", 2),
        ("Barcode Design", "Barcode Design", "تصميم الباركود", "Settings", "AshproSAASERP.Shared.Pages.Others.BarcodeDesignerView", "BarcodeDesignerView", 3),
        ("CompanyDetails", "Company Details", "بيانات الشركة", "Settings", "AshproSAASERP.Shared.Pages.Settings.CompanyDetailsView", "CompanyDetailsView", 4),
        ("ZATCA CSR", "ZATCA CSR", "زاتكا CSR", "Settings", "AshproSAASERP.Shared.Pages.Settings.ZatcaCSRView", "ZatcaCSRView", 5),
        ("User Previlege", "User Previlege", "صلاحيات المستخدمين", "Settings", "AshproSAASERP.Shared.Pages.Users.UserPrevilageView", "UserPrevilageView", 6),
        ("ZATCA Pending", "ZATCA Pending Invoices", "فواتير زاتكا المعلقة", "Settings", "AshproSAASERP.Shared.Pages.Settings.ZatcaPendingUploadView", "ZatcaPendingUploadView", 7),
        ("ProductImport", "Product Import", "استيراد المنتجات", "Settings", "AshproSAASERP.Shared.Pages.Settings.ProductImportView", "ProductImportView", 8),
        ("ProductExport", "Product Export", "تصدير المنتجات", "Settings", "AshproSAASERP.Shared.Pages.Settings.ProductExportView", "ProductExportView", 9),
        ("PrintingBarcode", "Printing Barcode", "طباعة الباركود", "Settings", "AshproSAASERP.Shared.Pages.Settings.PrintingBarcodeView", "PrintingBarcodeView", 10),
    };

    public static async Task EnsureSeedDataAsync(TenantDBContext context)
    {
        try
        {
            await EnsureMissingMenuDetailsAsync(context);
            await EnsureMissingVisibilitiesAsync(context);
            await EnsureMissingERPTypePresetsAsync(context);
            await EnsureMissingSettingsAsync(context);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MenuSeedHelper] Error ensuring seed data: {ex.Message}");
        }
    }

    public static async Task EnsureMissingMenuDetailsAsync(TenantDBContext context)
    {
        var existingMenus = await context.MenuDetails.ToListAsync();
        var existingByName = existingMenus
            .GroupBy(m => m.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        bool anyAdded = false;

        foreach (var item in AllSystemMenus)
        {
            if (!existingByName.ContainsKey(item.Name))
            {
                int? parentId = null;
                if (!string.IsNullOrEmpty(item.ParentName))
                {
                    if (existingByName.TryGetValue(item.ParentName, out var parentMenu))
                    {
                        parentId = parentMenu.Id;
                    }
                }

                var newMenu = new MenuDetails
                {
                    Name = item.Name,
                    MenuCaption = item.MenuCaption,
                    ArabicCaption = item.ArabicCaption,
                    ParentId = parentId,
                    AssemblyName = item.AssemblyName,
                    ClassName = item.ClassName,
                    Position = item.Position,
                    Active = true
                };

                context.MenuDetails.Add(newMenu);
                existingByName[item.Name] = newMenu;
                anyAdded = true;
            }
        }

        if (anyAdded)
        {
            await context.SaveChangesAsync();
        }
    }

    public static async Task EnsureMissingVisibilitiesAsync(TenantDBContext context)
    {
        if (!await context.MenuDetails.AnyAsync()) return;

        var existingVisibilityMenuIds = (await context.MenuVisibilities
            .Select(v => v.MenuDetailId)
            .ToListAsync())
            .ToHashSet();

        var allMenuIds = await context.MenuDetails.Select(m => m.Id).ToListAsync();
        var missingMenuIds = allMenuIds.Where(id => !existingVisibilityMenuIds.Contains(id)).ToList();

        if (missingMenuIds.Count > 0)
        {
            foreach (var menuId in missingMenuIds)
            {
                context.MenuVisibilities.Add(new MenuVisibility
                {
                    MenuDetailId = menuId,
                    IsVisible = true,
                    LastModified = DateTime.Now
                });
            }
            await context.SaveChangesAsync();
        }
    }

    public static async Task EnsureMissingERPTypePresetsAsync(TenantDBContext context)
    {
        if (!await context.ERPTypePresets.AnyAsync() || !await context.ERPTypes.AnyAsync()) return;

        var erpTypeIds = await context.ERPTypes.Select(t => t.Id).ToListAsync();
        var allMenuIds = await context.MenuDetails.Select(m => m.Id).ToListAsync();

        bool presetChanged = false;
        foreach (var typeId in erpTypeIds)
        {
            var existingPresets = (await context.ERPTypePresets
                .Where(p => p.ERPTypeId == typeId)
                .Select(p => p.MenuDetailId)
                .ToListAsync())
                .ToHashSet();

            var missingForType = allMenuIds.Where(id => !existingPresets.Contains(id)).ToList();
            foreach (var missingMenuId in missingForType)
            {
                context.ERPTypePresets.Add(new ERPTypePreset
                {
                    ERPTypeId = typeId,
                    MenuDetailId = missingMenuId,
                    IsVisible = true
                });
                presetChanged = true;
            }
        }

        if (presetChanged)
        {
            await context.SaveChangesAsync();
        }
    }

    public static async Task EnsureMissingSettingsAsync(TenantDBContext context)
    {
        var existingKeys = (await context.PermanentSettings.Select(s => s.SettingKey).ToListAsync()).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var defaultSettings = new (string Key, string Value)[]
        {
            ("ItemCodePrefix","IT"),
            ("ItemCodeStartNumber","1"),
            ("ItemCodeZeroPadding","3"),
            ("CategoryWiseItemCode","0"),
            ("SubCategoryWiseItemCode","0"),
            ("CodeSeparator","-"),
            ("AutoCustomerCode","1"),
            ("CustomerCodePrefix","CUS"),
            ("CustomerCodeStartNumber","1"),
            ("CustomerCodeZeroPadding","3"),
            ("AutoSupplierCode","1"),
            ("SupplierCodePrefix","SUP"),
            ("SupplierCodeStartNumber","1"),
            ("SupplierCodeZeroPadding","3"),
            ("AutoCategoryCode","1"),
            ("CategoryCodePrefix","CAT"),
            ("CategoryCodeStartNumber","1"),
            ("CategoryCodeZeroPadding","3"),
            ("AutoSubCategoryCode","1"),
            ("SubCategoryCodePrefix","SUB"),
            ("SubCategoryCodeStartNumber","1"),
            ("SubCategoryCodeZeroPadding","3"),
            ("CategoryWiseSubCategoryCode","0"),
            ("AutoQuotationNo","1"),
            ("QuotationNoPrefix","QT"),
            ("QuotationNoStartNumber","1"),
            ("QuotationNoZeroPadding","4"),
            ("AutoSalesOrderNo","1"),
            ("SalesOrderNoPrefix","SO"),
            ("SalesOrderNoStartNumber","1"),
            ("SalesOrderNoZeroPadding","4"),
            ("AutoDeliveryNoteNo","1"),
            ("DeliveryNoteNoPrefix","DN"),
            ("DeliveryNoteNoStartNumber","1"),
            ("DeliveryNoteNoZeroPadding","4"),
            ("DeliveryNoteStockAffect","0"),
            ("AutoPurchaseOrderNo","1"),
            ("PurchaseOrderNoPrefix","PO"),
            ("PurchaseOrderNoStartNumber","1"),
            ("PurchaseOrderNoZeroPadding","4"),
            ("AutoPurchaseNo","1"),
            ("PurchaseNoPrefix","P"),
            ("PurchaseNoStartNumber","1"),
            ("PurchaseNoZeroPadding","4"),
            ("DefaultFocusOn","ItemCode"),
            ("DefaultBillType","Cash"),
            ("AllowNegativeStock","0"),
            ("ShowCostInSales","0"),
            ("RequireBarcode","0"),
            ("SalesInvoice_EnableUpdate","0"),
            ("SalesInvoice_EnableCancel","0"),
            ("SalesReturn_EnableUpdate","0"),
            ("SalesReturn_EnableCancel","0"),
            ("ImageStorageMode","0"),
            ("ImageClientFolder","DefaultClient"),
            ("ImageLocalBasePath","\\\\192.168.1.11\\D\\Images"),
            ("ImageAwsRegion","us-east-1"),
            ("ImageAwsBucket","erp-images"),
            ("ImageAwsAccessKey",""),
            ("ImageAwsSecretKey",""),
            ("AutoJournalEntryNo","1"),
            ("JournalEntryNoPrefix","JV"),
            ("JournalEntryNoStartNumber","1"),
            ("JournalEntryNoZeroPadding","4"),
            ("AutoExpenseEntryNo","1"),
            ("ExpenseEntryNoPrefix","EXP"),
            ("ExpenseEntryNoStartNumber","1"),
            ("ExpenseEntryNoZeroPadding","4"),
            ("AutoCustomerReceiptNo","1"),
            ("CustomerReceiptNoPrefix","CR"),
            ("CustomerReceiptNoStartNumber","1"),
            ("CustomerReceiptNoZeroPadding","6"),
            ("AutoStockAdjustmentNo","1"),
            ("StockAdjustmentNoPrefix","SA"),
            ("StockAdjustmentNoStartNumber","1"),
            ("StockAdjustmentNoZeroPadding","4"),
            ("AutoMaterialTransferInNo","1"),
            ("MaterialTransferInNoPrefix","MTI"),
            ("MaterialTransferInNoStartNumber","1"),
            ("MaterialTransferInNoZeroPadding","4"),
            ("AutoMaterialTransferOutNo","1"),
            ("MaterialTransferOutNoPrefix","MTO"),
            ("MaterialTransferOutNoStartNumber","1"),
            ("MaterialTransferOutNoZeroPadding","4"),
            ("AutoJobOrderNo","1"),
            ("JobOrderNoPrefix","JO"),
            ("JobOrderNoStartNumber","1"),
            ("JobOrderNoZeroPadding","4"),
            ("AutoJobCardNo","1"),
            ("JobCardNoPrefix","JC"),
            ("JobCardNoStartNumber","1"),
            ("JobCardNoZeroPadding","4"),
            ("AutoMaterialTransferRequestNo", "1"),
            ("MaterialTransferRequestNoPrefix", "MTR"),
            ("MaterialTransferRequestNoStartNumber", "1"),
            ("MaterialTransferRequestNoZeroPadding", "4")
        };

        bool anyAdded = false;
        foreach (var s in defaultSettings)
        {
            if (!existingKeys.Contains(s.Key))
            {
                context.PermanentSettings.Add(new PermanentSettings
                {
                    SettingKey = s.Key,
                    SettingValue = s.Value
                });
                existingKeys.Add(s.Key);
                anyAdded = true;
            }
        }

        if (anyAdded)
        {
            await context.SaveChangesAsync();
        }
    }
}
