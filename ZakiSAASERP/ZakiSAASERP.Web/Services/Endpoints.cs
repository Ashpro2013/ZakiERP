using ZakiSAASERP.Interface.Masters;
using ZakiSAASERP.Interface.Reports;
using ZakiSAASERP.Interface.Services;
using ZakiSAASERP.Interface.Settings;
using ZakiSAASERP.Models.Settings;
using ZakiSAASERP.Interface.Transaction.Accounts;
using ZakiSAASERP.Interface.Transaction.Inventory;
using ZakiSAASERP.Interface.Transaction.Purchase;
using ZakiSAASERP.Interface.Transaction.Sales;
using ZakiSAASERP.Interface.Transaction.Workshop;
using ZakiSAASERP.Models.Company;
using ZakiSAASERP.Models.DTO;
using ZakiSAASERP.Models.Masters;
using ZakiSAASERP.Models.Transactions.Inventory;
using ZakiSAASERP.Models.Transactions.Sales;
using ZakiSAASERP.Models.Transactions.Workshop;
using ZakiSAASERP.Models.ViewModel;
using AshproStringExtension;
using Microsoft.AspNetCore.Mvc;

namespace ZakiSAASERP.Web.Services;

public static class Endpoints
{
    #region Endpoints
    public static WebApplication MapEndpoints(this WebApplication app)
    {
        #region Masters
        app.MapGroup("/api/accountGroup").MapAccountGroupEndpoints().RequireAuthorization();
        app.MapGroup("/api/accountSubGroup").MapAccountSubGroupEndpoints().RequireAuthorization();
        app.MapGroup("/api/accountType").MapAccountTypeEndpoints().RequireAuthorization();
        app.MapGroup("/api/accounthead").MapAccountHeadEndpoints().RequireAuthorization();
        app.MapGroup("/api/account").MapAccountEndpoints().RequireAuthorization();
        app.MapGroup("/api/branch").MapBranchEndpoints().RequireAuthorization();
        app.MapGroup("/api/financialyear").MapFinancialYearEndpoints().RequireAuthorization();
        app.MapGroup("/api/general").MapGeneralEndpoints().RequireAuthorization();

        LoginEndPoint.MapUserEndpoints(app.MapGroup("/api/user"));
        app.MapGroup("/api/unit").MapUnitEndpoints().RequireAuthorization();
        app.MapGroup("/api/userdetails").MapUserDetailsEndpoints().RequireAuthorization();
        app.MapGroup("/api/salesMan").MapSalesManEndpoints().RequireAuthorization();
        app.MapGroup("/api/category").MapCategoryEndpoints().RequireAuthorization();
        app.MapGroup("/api/subCategory").MapSubCategoryEndpoints().RequireAuthorization();
        app.MapGroup("/api/customerGroup").MapCustomerGroupMasterEndpoints().RequireAuthorization();
        app.MapGroup("/api/country").MapCountryMasterEndpoints().RequireAuthorization();
        app.MapGroup("/api/customerLocation").MapCustomerLocationMasterEndpoints().RequireAuthorization();
        app.MapGroup("/api/vat").MapVatEndpoints().RequireAuthorization();
        app.MapGroup("/api/image").MapImageEndpoints().RequireAuthorization();
        app.MapGroup("/api/admin").MapAdminCompanyEndpoint().RequireAuthorization();
        app.MapGroup("/api/brand").MapBrandEndpoints().RequireAuthorization();
        app.MapGroup("/api/currency").MapCurrencyEndpoints().RequireAuthorization();
        app.MapGroup("/api/purchaser").MapPurchaserEndpoints().RequireAuthorization();
        app.MapGroup("/api/project").MapProjectEndpoints().RequireAuthorization();
        app.MapGroup("/api/customer").MapCustomerMasterEndpoints().RequireAuthorization();
        app.MapGroup("/api/supplier").MapSupplierMasterEndpoints().RequireAuthorization();
        app.MapGroup("/api/adminUser").MapAdminUserEndpoint().RequireAuthorization();
        app.MapGroup("/api/item").MapItemEndpoints().RequireAuthorization();
        app.MapGroup("/api/customeropentry").MapCustomerOPEntryEndpoints().RequireAuthorization();
        app.MapGroup("/api/supplieropentry").MapSupplierOPEntryEndpoints().RequireAuthorization();
        app.MapGroup("/api/storemaster").MapStoreMasterEndpoints().RequireAuthorization();
        app.MapGroup("/api/openingstockentry").MapOpeningStockEntryEndpoints().RequireAuthorization();
        app.MapGroup("/api/companydetails").MapCompanyDetailsEndpoints().RequireAuthorization();
        app.MapGroup("/api/permanentSettings").MapPermanentSettingsEndpoints().RequireAuthorization();
        app.MapGroup("/api/menuvisibility").MapMenuVisibilityEndpoints().RequireAuthorization();
        app.MapGroup("/api/accountopeningentry").MapAccountOpeningEntryEndpoints().RequireAuthorization();
        app.MapGroup("/api/vehicle").MapVehicleEndpoints().RequireAuthorization();
        app.MapGroup("/api/repair").MapRepairEndpoints().RequireAuthorization();
        app.MapGroup("/api/technician").MapTechnicianEndpoints().RequireAuthorization();
        #endregion

        #region Transactions

        #region Sales
        app.MapGroup("/api/quotation").MapSalesQuotationEndpoints().RequireAuthorization();
        app.MapGroup("/api/salesorder").MapSalesOrderEndpoints().RequireAuthorization();
        app.MapGroup("/api/deliverynote").MapDeliveryNoteEndpoints().RequireAuthorization();
        app.MapGroup("/api/proformainvoice").MapProformaInvoiceEndpoints().RequireAuthorization();
        app.MapGroup("/api/salesinvoice").MapSalesInvoiceEndpoints().RequireAuthorization();
        app.MapGroup("/api/salesinvoicehold").MapSalesInvoiceHoldEndpoints().RequireAuthorization();
        app.MapGroup("/api/salesinvoicertn").MapSalesInvoiceRtnEndpoints().RequireAuthorization();
        app.MapGroup("/api/dnrtn").MapDeliveryNoteRtnEndpoints();
        #endregion

        #region Purchase
        app.MapGroup("/api/purchaseorder").MapPurchaseOrderEndpoints().RequireAuthorization();
        app.MapGroup("/api/purchase").MapPurchaseEndpoints().RequireAuthorization();
        app.MapGroup("/api/goodsreceipt").MapGoodsReceiptEndpoints().RequireAuthorization();
        app.MapGroup("/api/enquiry").MapEnquiryEndpoints().RequireAuthorization();
        app.MapGroup("/api/purchasereturn").MapPurchaseReturnEndpoints().RequireAuthorization();
        app.MapGroup("/api/supplierpayment").MapSupplierPaymentEndpoints().RequireAuthorization();
        #endregion

        #region Accounting
        app.MapGroup("/api/journalentry").MapJournalEntryEndpoints().RequireAuthorization();
        app.MapGroup("/api/expenseentry").MapExpenseEntryEndpoints().RequireAuthorization();
        app.MapGroup("/api/customerreceipt").MapCustomerReceiptEndpoints().RequireAuthorization();
        #endregion

        #endregion

        #region General
        app.MapGroup("/api/printdesign").MapPrintDesignerEndpoints().RequireAuthorization();
        app.MapGroup("/api/zatca").MapZatcaEndpoints().RequireAuthorization();
        app.MapGroup("/api/barcodedesign").MapBarcodeDesignEndpoint().RequireAuthorization();
        app.MapGroup("/api/userprivileges").MapUserPrevilegesEndpoints().RequireAuthorization();
        app.MapGroup("/api/zatcapending").MapZatcaPendingEndpoints().RequireAuthorization();
        //app.MapGet("/", () => "Infogate");
        #endregion

        #region Inventory
        app.MapGroup("/api/damagestock").MapDamageStockEndpoints().RequireAuthorization();
        app.MapGroup("/api/stockadjustment").MapStockAdjustmentEndpoints().RequireAuthorization();
        app.MapGroup("/api/materialtransferrequest").MapMaterialTransferRequestEndpoints().RequireAuthorization();
        app.MapGroup("/api/materialtransferin").MapMaterialTransferInEndpoints().RequireAuthorization();
        app.MapGroup("/api/materialtransferout").MapMaterialTransferOutEndpoints().RequireAuthorization();
        app.MapGroup("/api/internalmaterialtransfer").MapInternalMaterialTransferEndpoints().RequireAuthorization();
        #endregion

        #region Workshop
        app.MapGroup("/api/joborder").MapJobOrderEndpoints().RequireAuthorization();
        app.MapGroup("/api/jobcard").MapJobCardEndpoints().RequireAuthorization();
        #endregion

        #region Reports
        app.MapGroup("/api/accountreport").MapAccountReportEndpoints().RequireAuthorization();
        app.MapGroup("/api/customerreport").MapCustomerReportEndpoints().RequireAuthorization();
        app.MapGroup("/api/supplierreport").MapSupplierReportEndpoints().RequireAuthorization();
        app.MapGroup("/api/purchasereport").MapPurchaseReportEndpoints().RequireAuthorization();
        app.MapGroup("/api/salesreport").MapSalesReportEndpoints().RequireAuthorization();
        app.MapGroup("/api/inventoryreport").MapInventoryReportEndpoints().RequireAuthorization();
        app.MapGroup("/api/transactionreport").MapTransactionReportEndpoints().RequireAuthorization();
        app.MapGroup("/api/shift").MapShiftEndpoints().RequireAuthorization();
        #endregion

        return app;
    }
    #endregion

    #region Masters
    private static RouteGroupBuilder MapBrandEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/pagination", async ([AsParameters] PaginationModel pmodel, IBrandMasterRepo repo) =>
        {
            var data = await repo.GetAllWithPagination(pmodel);
            return new BrandPDTO(data.Item1, data.Item2);
        });
        group.MapGet("/", async (IBrandMasterRepo repo) => await repo.GetAllAsync());
        group.MapGet("/{id:int}", async (int id, IBrandMasterRepo repo) => await repo.GetAsync(id));
        group.MapPost("/", async (BrandMaster Brand, IBrandMasterRepo repo) =>
        {
            var data = await repo.CreateAsync(Brand);
            return data;
        });
        group.MapPut("/", async (BrandMaster Brand, IBrandMasterRepo repo) => await repo.UpdateAsync(Brand));
        group.MapDelete("/{id:int}", async (int id, IBrandMasterRepo repo) => await repo.DeleteAsync(id));
        return group;
    }
    private static RouteGroupBuilder MapCurrencyEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/pagination", async ([AsParameters] PaginationModel pmodel, ICurrencyMasterRepo repo) =>
        {
            var data = await repo.GetAllWithPagination(pmodel);
            return new CurrencyPDTO(data.Item1, data.Item2);
        });
        group.MapGet("/", async (ICurrencyMasterRepo repo) => await repo.GetAllAsync());
        group.MapGet("/{id:int}", async (int id, ICurrencyMasterRepo repo) => await repo.GetAsync(id));
        group.MapPost("/", async (CurrencyMaster Currency, ICurrencyMasterRepo repo) =>
        {
            var data = await repo.CreateAsync(Currency);
            return data;
        });
        group.MapPut("/", async (CurrencyMaster Currency, ICurrencyMasterRepo repo) => await repo.UpdateAsync(Currency));
        group.MapDelete("/{id:int}", async (int id, ICurrencyMasterRepo repo) => await repo.DeleteAsync(id));
        return group;
    }
    private static RouteGroupBuilder MapPurchaserEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/pagination", async ([AsParameters] PaginationModel pmodel, IPurchaserMasterRepo repo) =>
        {
            var data = await repo.GetAllWithPagination(pmodel);
            return new PurchaserPDTO(data.Item1, data.Item2);
        });
        group.MapGet("/", async (IPurchaserMasterRepo repo) => await repo.GetAllAsync());
        group.MapGet("/{id:int}", async (int id, IPurchaserMasterRepo repo) => await repo.GetAsync(id));
        group.MapPost("/", async (PurchaserMaster Purchaser, IPurchaserMasterRepo repo) =>
        {
            var data = await repo.CreateAsync(Purchaser);
            return data;
        });
        group.MapPut("/", async (PurchaserMaster Purchaser, IPurchaserMasterRepo repo) => await repo.UpdateAsync(Purchaser));
        group.MapDelete("/{id:int}", async (int id, IPurchaserMasterRepo repo) => await repo.DeleteAsync(id));
        return group;
    }
    private static RouteGroupBuilder MapItemEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/pagination", async ([AsParameters] PaginationModel pmodel, IItemMasterRepo repo) =>
        {
            var data = await repo.GetAllWithPagination(pmodel);
            return new ItemMasterPDTO(data.Item1, data.Item2);
        });
        group.MapGet("/multipagination", async ([AsParameters] PaginationModel pmodel, IItemMasterRepo repo) =>
        {
            return await repo.GetItemMultiUnitPDTO(pmodel);
        });
        group.MapGet("/multiunit", async (int itemId, int unitId, string? barcode, IItemMasterRepo repo) => await repo.GetItemMultiUnit(itemId, unitId, barcode));
        group.MapGet("/", async (IItemMasterRepo repo) => await repo.GetAllAsync());
        group.MapGet("/{id:int}", async (int id, IItemMasterRepo repo) => await repo.GetAsync(id));
        group.MapPost("/", async (ItemMaster item, IItemMasterRepo repo) => await repo.CreateAsync(item));
        group.MapPut("/", async (ItemMaster item, IItemMasterRepo repo) => await repo.UpdateAsync(item));
        group.MapDelete("/{id:int}", async (int id, IItemMasterRepo repo) => await repo.DeleteAsync(id));
        group.MapGet("/nextcode", async (int? categoryId, int? subCategoryId, string? catCode, string? subCatCode, IItemMasterRepo repo)
            => await repo.GetNextItemCodeAsync(categoryId, subCategoryId, catCode, subCatCode));
        group.MapGet("/dialogpagination", async ([AsParameters] PaginationModel pmodel, IItemMasterRepo repo) =>
        {
            var data = await repo.GetItemsForDialogWithStock(pmodel);
            return new ItemDialogPDTO(data.Item1, data.Item2);
        });
        group.MapGet("/multidialogpagination", async ([AsParameters] PaginationModel pmodel, IItemMasterRepo repo) =>
        {
            var data = await repo.GetMultiUnitItemsForDialogWithStock(pmodel);
            return new ItemMultiUnitDialogPDTO(data.Item1, data.Item2);
        });
        group.MapGet("/multiunitsByItem/{itemId:int}", async (int itemId, IItemMasterRepo repo) => await repo.GetItemMultiUnits(itemId));
        group.MapGet("/stockinfo/{itemId:int}", async (int itemId, IItemMasterRepo repo) => await repo.GetItemStockInfoAsync(itemId));
        group.MapGet("/barcodeitems", async (int? categoryId, IItemMasterRepo repo) => await repo.GetBarcodeItemsAsync(categoryId));
        return group;
    }
    private static RouteGroupBuilder MapAccountHeadEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/pagination", async ([AsParameters] PaginationModel pmodel, IAccountHeadRepo repo) =>
        {
            var data = await repo.GetAllWithPagination(pmodel);
            return new AccountHeadPDTO(data.Item1, data.Item2);
        });
        group.MapGet("/", async (IAccountHeadRepo repo) => await repo.GetAllAsync());
        group.MapGet("/{id:int}", async (int id, IAccountHeadRepo repo) => await repo.GetAsync(id));
        group.MapPost("/", async (AccountHeadMaster accountHead, IAccountHeadRepo repo) => await repo.CreateAsync(accountHead));
        group.MapPut("/", async (AccountHeadMaster accountHead, IAccountHeadRepo repo) => await repo.UpdateAsync(accountHead));
        group.MapDelete("/{id:int}", async (int id, IAccountHeadRepo repo) => await repo.DeleteAsync(id));
        group.MapGet("/nextcode/{typeId}", async (int typeId, IAccountHeadRepo repo) => await repo.GetNextCode(typeId));
        return group;
    }
    private static RouteGroupBuilder MapAccountGroupEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/pagination", async ([AsParameters] PaginationModel pmodel, IAccountGroupRepo repo) =>
        {
            var data = await repo.GetAllWithPagination(pmodel);
            return new AccountGroupPDTO(data.Item1, data.Item2);
        });
        group.MapGet("/", async (IAccountGroupRepo repo) => await repo.GetAllAsync());
        group.MapGet("/{id:int}", async (int id, IAccountGroupRepo repo) => await repo.GetAsync(id));
        group.MapPost("/", async (AccountGroup accountGroup, IAccountGroupRepo repo) => await repo.CreateAsync(accountGroup));
        group.MapPut("/", async (AccountGroup accountGroup, IAccountGroupRepo repo) => await repo.UpdateAsync(accountGroup));
        group.MapDelete("/{id:int}", async (int id, IAccountGroupRepo repo) => await repo.DeleteAsync(id));
        group.MapGet("/nextcode/{accountHeadId}", async (int accountHeadId, IAccountGroupRepo repo) => await repo.GetNextCode(accountHeadId));
        return group;
    }
    private static RouteGroupBuilder MapAccountSubGroupEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/pagination", async ([AsParameters] PaginationModel pmodel, IAccountSubGroupRepo repo) =>
        {
            var data = await repo.GetAllWithPagination(pmodel);
            return new AccountSubGroupPDTO(data.Item1, data.Item2);
        });
        group.MapGet("/", async (IAccountSubGroupRepo repo) => await repo.GetAllAsync());
        group.MapGet("/{id:int}", async (int id, IAccountSubGroupRepo repo) => await repo.GetAsync(id));
        group.MapPost("/", async (AccountSubGroup accountSubGroup, IAccountSubGroupRepo repo) => await repo.CreateAsync(accountSubGroup));
        group.MapPut("/", async (AccountSubGroup accountSubGroup, IAccountSubGroupRepo repo) => await repo.UpdateAsync(accountSubGroup));
        group.MapDelete("/{id:int}", async (int id, IAccountSubGroupRepo repo) => await repo.DeleteAsync(id));
        group.MapGet("/nextcode/{accountGroupId}", async (int accountGroupId, IAccountSubGroupRepo repo) => await repo.GetNextCode(accountGroupId));
        return group;
    }
    private static RouteGroupBuilder MapAccountTypeEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (IAccountTypeRepo repo) => await repo.GetAllAsync());
        return group;
    }
    private static RouteGroupBuilder MapAccountEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/pagination", async ([AsParameters] PaginationModel pmodel, IAccountMasterRepo repo) =>
        {
            var data = await repo.GetAllWithPagination(pmodel);
            return new AccountPDTO(data.Item1, data.Item2);
        });
        group.MapGet("/", async (IAccountMasterRepo repo) => await repo.GetAllAsync());
        group.MapGet("/{id:int}", async (int id, IAccountMasterRepo repo) => await repo.GetAsync(id));
        group.MapPost("/", async (AccountMaster account, IAccountMasterRepo repo) => await repo.CreateAsync(account));
        group.MapPut("/", async (AccountMaster account, IAccountMasterRepo repo) => await repo.UpdateAsync(account));
        group.MapDelete("/{id:int}", async (int id, IAccountMasterRepo repo) => await repo.DeleteAsync(id));
        group.MapGet("/nextcode/{subGroupId}", async (int subGroupId, IAccountMasterRepo repo) => await repo.GetNextCode(subGroupId));
        group.MapGet("/cashandbank", async (IAccountMasterRepo repo) => await repo.GetCashAndBankAccountsAsync());
        return group;
    }
    private static RouteGroupBuilder MapBranchEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/pagination", async ([AsParameters] PaginationModel pmodel, IBranchRepo repo) =>
        {
            var data = await repo.GetAllWithPagination(pmodel);
            return new BranchPDTO(data.Item1, data.Item2);
        });
        group.MapGet("/", async (IBranchRepo repo) => await repo.GetAllAsync());
        group.MapGet("/{id:int}", async (int id, IBranchRepo repo) => await repo.GetAsync(id));
        group.MapPost("/", async (BranchMaster branch, IBranchRepo repo) => await repo.CreateAsync(branch));
        group.MapPut("/", async (BranchMaster branch, IBranchRepo repo) => await repo.UpdateAsync(branch));
        group.MapDelete("/{id:int}", async (int id, IBranchRepo repo) => await repo.DeleteAsync(id));
        return group;
    }
    private static RouteGroupBuilder MapFinancialYearEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/pagination", async ([AsParameters] PaginationModel pmodel, IFinancialYearRepo repo) =>
        {
            var data = await repo.GetAllWithPagination(pmodel);
            return new FinancialYearPDTO(data.Item1, data.Item2);
        });
        group.MapGet("/", async (IFinancialYearRepo repo) => await repo.GetAllAsync());
        group.MapGet("/maxrefno", async (IFinancialYearRepo repo) => await repo.GetMaxRefNo());
        group.MapGet("/{id:int}", async (int id, IFinancialYearRepo repo) => await repo.GetAsync(id));
        group.MapPost("/", async (FinancialYear financialYear, IFinancialYearRepo repo) => await repo.CreateAsync(financialYear));
        group.MapPut("/", async (FinancialYear financialYear, IFinancialYearRepo repo) => await repo.UpdateAsync(financialYear));
        group.MapDelete("/{id:int}", async (int id, IFinancialYearRepo repo) => await repo.DeleteAsync(id));
        return group;
    }
    private static RouteGroupBuilder MapUnitEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (IUnitMasterRepo repo) => await repo.GetAllAsync());
        group.MapGet("/pagination", async ([AsParameters] PaginationModel pmodel, IUnitMasterRepo repo) =>
        {
            var data = await repo.GetAllWithPagination(pmodel);
            return new UnitPDTO(data.Item1, data.Item2);
        });
        group.MapGet("/{id:int}", async (int id, IUnitMasterRepo repo) => await repo.GetAsync(id));
        group.MapPost("/", async (UnitMaster unit, IUnitMasterRepo repo) => await repo.CreateAsync(unit));
        group.MapPut("/", async (UnitMaster unit, IUnitMasterRepo repo) => await repo.UpdateAsync(unit));
        group.MapDelete("/{id:int}", async (int id, IUnitMasterRepo repo) => await repo.DeleteAsync(id));
        return group;
    }
    private static RouteGroupBuilder MapUserDetailsEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/pagination", async ([AsParameters] PaginationModel pmodel, IUserRepo repo) =>
        {
            var data = await repo.GetAllWithPagination(pmodel);
            return new UserPDTO(data.Item1, data.Item2);
        });
        group.MapGet("/", async (IUserRepo repo) => await repo.GetAllAsync());
        group.MapGet("/{id:int}", async (int id, IUserRepo repo) => await repo.GetAsync(id));
        group.MapPost("/", async (ZakiSAASERP.Models.User.UserDetails user, IUserRepo repo) => await repo.CreateAsync(user));
        group.MapPut("/", async (ZakiSAASERP.Models.User.UserDetails user, IUserRepo repo) => await repo.UpdateAsync(user));
        group.MapDelete("/{id:int}", async (int id, IUserRepo repo) => await repo.DeleteAsync(id));
        group.MapGet("/branches/{username}", async (string username, IUserRepo repo) => await repo.GetBranchesForUser(username));
        group.MapPost("/savebranches", async (List<ZakiSAASERP.Models.User.UserBranch> userBranches, IUserRepo repo) => await repo.SaveUserBranches(userBranches));
        return group;
    }
    private static RouteGroupBuilder MapSalesManEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/pagination", async ([AsParameters] PaginationModel pmodel, ISalesManMasterRepo repo) =>
        {
            var data = await repo.GetAllWithPagination(pmodel);
            return new SalesManPDTO(data.Item1, data.Item2);
        });
        group.MapGet("/", async (ISalesManMasterRepo repo) => await repo.GetAllAsync());
        group.MapGet("/{id:int}", async (int id, ISalesManMasterRepo repo) => await repo.GetAsync(id));
        group.MapPost("/", async (SalesManMaster salesMan, ISalesManMasterRepo repo) =>
        {
            var data = await repo.CreateAsync(salesMan);
            return data;
        });
        group.MapPut("/", async (SalesManMaster salesMan, ISalesManMasterRepo repo) => await repo.UpdateAsync(salesMan));
        group.MapDelete("/{id:int}", async (int id, ISalesManMasterRepo repo) => await repo.DeleteAsync(id));
        return group;
    }
    private static RouteGroupBuilder MapCategoryEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/pagination", async ([AsParameters] PaginationModel pmodel, ICategoryMasterRepo repo) =>
        {
            var data = await repo.GetAllWithPagination(pmodel);
            return new CategoryPDTO(data.Item1, data.Item2);
        });
        group.MapGet("/", async (ICategoryMasterRepo repo) => await repo.GetAllAsync());
        group.MapGet("/nextcode", async (ICategoryMasterRepo repo) => await repo.GetNextCategoryCodeAsync());
        group.MapGet("/{id:int}", async (int id, ICategoryMasterRepo repo) => await repo.GetAsync(id));
        group.MapPost("/", async (CategoryMaster category, ICategoryMasterRepo repo) => await repo.CreateAsync(category));
        group.MapPut("/", async (CategoryMaster category, ICategoryMasterRepo repo) => await repo.UpdateAsync(category));
        group.MapDelete("/{id:int}", async (int id, ICategoryMasterRepo repo) => await repo.DeleteAsync(id));
        return group;
    }
    private static RouteGroupBuilder MapVehicleEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/pagination", async ([AsParameters] PaginationModel pmodel, IVehicleMasterRepo repo) =>
        {
            var data = await repo.GetAllWithPagination(pmodel);
            return new VehiclePDTO(data.Item1, data.Item2);
        });
        group.MapGet("/", async (IVehicleMasterRepo repo) => await repo.GetAllAsync());
        group.MapGet("/{id:int}", async (int id, IVehicleMasterRepo repo) => await repo.GetAsync(id));
        group.MapPost("/", async (VehicleMaster vehicle, IVehicleMasterRepo repo) => await repo.CreateAsync(vehicle));
        group.MapPut("/", async (VehicleMaster vehicle, IVehicleMasterRepo repo) => await repo.UpdateAsync(vehicle));
        group.MapDelete("/{id:int}", async (int id, IVehicleMasterRepo repo) => await repo.DeleteAsync(id));
        return group;
    }
    private static RouteGroupBuilder MapRepairEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/pagination", async ([AsParameters] PaginationModel pmodel, IRepairMasterRepo repo) =>
        {
            var data = await repo.GetAllWithPagination(pmodel);
            return new RepairPDTO(data.Item1, data.Item2);
        });
        group.MapGet("/", async (IRepairMasterRepo repo) => await repo.GetAllAsync());
        group.MapGet("/{id:int}", async (int id, IRepairMasterRepo repo) => await repo.GetAsync(id));
        group.MapPost("/", async (RepairMaster repair, IRepairMasterRepo repo) => await repo.CreateAsync(repair));
        group.MapPut("/", async (RepairMaster repair, IRepairMasterRepo repo) => await repo.UpdateAsync(repair));
        group.MapDelete("/{id:int}", async (int id, IRepairMasterRepo repo) => await repo.DeleteAsync(id));
        return group;
    }
    private static RouteGroupBuilder MapTechnicianEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/pagination", async ([AsParameters] PaginationModel pmodel, ITechnicianMasterRepo repo) =>
        {
            var data = await repo.GetAllWithPagination(pmodel);
            return new TechnicianPDTO(data.Item1, data.Item2);
        });
        group.MapGet("/", async (ITechnicianMasterRepo repo) => await repo.GetAllAsync());
        group.MapGet("/{id:int}", async (int id, ITechnicianMasterRepo repo) => await repo.GetAsync(id));
        group.MapPost("/", async (TechnicianMaster entity, ITechnicianMasterRepo repo) => await repo.CreateAsync(entity));
        group.MapPut("/", async (TechnicianMaster entity, ITechnicianMasterRepo repo) => await repo.UpdateAsync(entity));
        group.MapDelete("/{id:int}", async (int id, ITechnicianMasterRepo repo) => await repo.DeleteAsync(id));
        // MapGet nextcode
        group.MapGet("/nextcode", async (ITechnicianMasterRepo repo) => await repo.GetNextCode());
        return group;
    }
    private static RouteGroupBuilder MapImageEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/upload", async (HttpRequest request, ICommonImageRepo repo, IWebHostEnvironment env) =>
        {
            if (!request.HasFormContentType)
                return Results.BadRequest("Invalid content type.");

            var form = await request.ReadFormAsync();
            var file = form.Files["file"];
            var entityType = form["entityType"].ToString();
            var entityIdStr = form["entityId"].ToString();
            var basePath = form["basePath"].ToString();
            var clientFolder = form["clientFolder"].ToString();

            if (file is null || file.Length == 0)
                return Results.BadRequest("No file uploaded.");

            if (string.IsNullOrEmpty(entityType) || string.IsNullOrEmpty(entityIdStr) || !int.TryParse(entityIdStr, out int entityId))
                return Results.BadRequest("Invalid entity parameters.");

            // Resolve WEBROOT token to actual server webroot path
            if (basePath == "WEBROOT")
            {
                basePath = env.WebRootPath;
            }

            using var stream = file.OpenReadStream();
            var path = await repo.UploadImageAsync(stream, file.FileName, entityType, entityId, basePath, clientFolder);

            return Results.Ok(path);
        });

        group.MapGet("/fetch", async (HttpRequest request, ICommonImageRepo repo, IWebHostEnvironment env) =>
        {
            var b64Base = request.Query["basePath"].ToString();
            var b64Image = request.Query["imagePath"].ToString();

            // Allow client to pass AWS settings directly (Stateless, Multi-tenant)
            var mode = request.Query["mode"].ToString();
            var b64Region = request.Query["region"].ToString();
            var b64Bucket = request.Query["bucket"].ToString();
            var b64AccessKey = request.Query["accessKey"].ToString();
            var b64SecretKey = request.Query["secretKey"].ToString();

            // Only ImagePath is strictly required
            if (string.IsNullOrEmpty(b64Image))
                return Results.BadRequest("Image path parameter is missing.");

            // Decode Base64 params
            string basePath = "", imagePath = "";
            string? region = null, bucket = null, accessKey = null, secretKey = null;
            try
            {
                if (!string.IsNullOrEmpty(b64Base))
                    basePath = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(b64Base));

                imagePath = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(b64Image));

                if (!string.IsNullOrEmpty(b64Region))
                    region = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(b64Region));

                if (!string.IsNullOrEmpty(b64Bucket))
                    bucket = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(b64Bucket));

                if (!string.IsNullOrEmpty(b64AccessKey))
                    accessKey = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(b64AccessKey));

                if (!string.IsNullOrEmpty(b64SecretKey))
                    secretKey = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(b64SecretKey));
            }
            catch
            {
                return Results.BadRequest("Invalid path encoding.");
            }

            // Resolve WEBROOT token
            if (basePath == "WEBROOT")
            {
                basePath = env.WebRootPath;
            }

            var physicalPath = await repo.GetImagePathAsync(basePath, imagePath, mode, region, bucket, accessKey, secretKey);
            if (string.IsNullOrEmpty(physicalPath)) return Results.NotFound();

            // If it's a URL (AWS S3 presigned URL), redirect
            if (physicalPath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                return Results.Redirect(physicalPath);
            }

            // If it's a Local File
            if (!File.Exists(physicalPath)) return Results.NotFound();

            // Improve content type detection if possible, defaulting to jpeg
            var extension = Path.GetExtension(imagePath).TrimStart('.').ToLower();
            var contentType = extension == "png" ? "image/png" : "image/jpeg";
            // "Real World" Best Practice:
            // 1. Pass the Path string, not a Stream. ASP.NET Core optimizations (sendfile) kick in.
            // 2. Used directly, this automatically handles ETags and Last-Modified for browser caching.
            return Results.File(physicalPath, contentType);
        }).AllowAnonymous();

        group.MapDelete("/delete", async (HttpRequest request, ICommonImageRepo repo, IWebHostEnvironment env) =>
        {
            var b64Base = request.Query["basePath"].ToString();
            var b64Image = request.Query["imagePath"].ToString();
            var b64Client = request.Query["clientFolder"].ToString();

            if (string.IsNullOrEmpty(b64Image))
                return Results.BadRequest("Image path parameter is missing.");

            // Decode Base64 params
            string basePath = "", imagePath = "", clientFolder = "";
            try
            {
                if (!string.IsNullOrEmpty(b64Base))
                    basePath = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(b64Base));

                imagePath = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(b64Image));

                if (!string.IsNullOrEmpty(b64Client))
                    clientFolder = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(b64Client));
            }
            catch
            {
                return Results.BadRequest("Invalid path encoding.");
            }

            // Resolve WEBROOT token
            if (basePath == "WEBROOT")
            {
                basePath = env.WebRootPath;
            }

            var result = await repo.DeleteImageAsync(basePath, imagePath, clientFolder);
            return result ? Results.Ok() : Results.NotFound("Image not found or already deleted.");
        });

        return group;
    }
    private static RouteGroupBuilder MapSubCategoryEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/pagination", async ([AsParameters] PaginationModel pmodel, ISubCategoryMasterRepo repo) =>
        {
            var data = await repo.GetAllWithPagination(pmodel);
            return new SubCategoryPDTO(data.Item1, data.Item2);
        });
        group.MapGet("/", async (ISubCategoryMasterRepo repo) => await repo.GetAllAsync());
        group.MapGet("/nextcode/{categoryId:int}", async (int categoryId, string? categoryCode, ISubCategoryMasterRepo repo) => await repo.GetNextSubCategoryCodeAsync(categoryId, categoryCode));
        group.MapGet("/{id:int}", async (int id, ISubCategoryMasterRepo repo) => await repo.GetAsync(id));
        group.MapPost("/", async (SubCategoryMaster subCategory, ISubCategoryMasterRepo repo) => await repo.CreateAsync(subCategory));
        group.MapPut("/", async (SubCategoryMaster subCategory, ISubCategoryMasterRepo repo) => await repo.UpdateAsync(subCategory));
        group.MapDelete("/{id:int}", async (int id, ISubCategoryMasterRepo repo) => await repo.DeleteAsync(id));
        return group;
    }
    private static RouteGroupBuilder MapVatEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/pagination", async ([AsParameters] PaginationModel pmodel, IVatMasterRepo repo) =>
        {
            var data = await repo.GetAllWithPagination(pmodel);
            return new VatPDTO(data.Item1, data.Item2);
        });
        group.MapGet("/", async (IVatMasterRepo repo) => await repo.GetAllAsync());
        group.MapGet("/{id:int}", async (int id, IVatMasterRepo repo) => await repo.GetAsync(id));
        group.MapPost("/", async (VatMaster vat, IVatMasterRepo repo) => await repo.CreateAsync(vat));
        group.MapPut("/", async (VatMaster vat, IVatMasterRepo repo) => await repo.UpdateAsync(vat));
        group.MapDelete("/{id:int}", async (int id, IVatMasterRepo repo) => await repo.DeleteAsync(id));
        group.MapGet("/categorydetails/{code}", async (string code, IVatMasterRepo repo) => await repo.GetVatCategoryDetails(code));
        return group;
    }
    private static RouteGroupBuilder MapCustomerGroupMasterEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/pagination", async ([AsParameters] PaginationModel pmodel, ICustomerGroupMasterRepo repo) =>
        {
            var data = await repo.GetAllWithPagination(pmodel);
            return new CustomerGroupPDTO(data.Item1, data.Item2);
        });
        group.MapGet("/", async (ICustomerGroupMasterRepo repo) => await repo.GetAllAsync());
        group.MapGet("/{id:int}", async (int id, ICustomerGroupMasterRepo repo) => await repo.GetAsync(id));
        group.MapPost("/", async (CustomerGroupMaster customerGroup, ICustomerGroupMasterRepo repo) => await repo.CreateAsync(customerGroup));
        group.MapPut("/", async (CustomerGroupMaster customerGroup, ICustomerGroupMasterRepo repo) => await repo.UpdateAsync(customerGroup));
        group.MapDelete("/{id:int}", async (int id, ICustomerGroupMasterRepo repo) => await repo.DeleteAsync(id));
        return group;
    }
    private static RouteGroupBuilder MapCountryMasterEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/pagination", async ([AsParameters] PaginationModel pmodel, ICountryMasterRepo repo) =>
        {
            var data = await repo.GetAllWithPagination(pmodel);
            return new CountryPDTO(data.Item1, data.Item2);
        });
        group.MapGet("/", async (ICountryMasterRepo repo) => await repo.GetAllAsync());
        group.MapGet("/{id:int}", async (int id, ICountryMasterRepo repo) => await repo.GetAsync(id));
        group.MapPost("/", async (CountryMaster country, ICountryMasterRepo repo) => await repo.CreateAsync(country));
        group.MapPut("/", async (CountryMaster country, ICountryMasterRepo repo) => await repo.UpdateAsync(country));
        group.MapDelete("/{id:int}", async (int id, ICountryMasterRepo repo) => await repo.DeleteAsync(id));
        return group;
    }
    private static RouteGroupBuilder MapCustomerLocationMasterEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/pagination", async ([AsParameters] PaginationModel pmodel, ICustomerLocationMasterRepo repo) =>
        {
            var data = await repo.GetAllWithPagination(pmodel);
            return new CustomerLocationPDTO(data.Item1, data.Item2);
        });
        group.MapGet("/", async (ICustomerLocationMasterRepo repo) => await repo.GetAllAsync());
        group.MapGet("/{id:int}", async (int id, ICustomerLocationMasterRepo repo) => await repo.GetAsync(id));
        group.MapPost("/", async (CustomerLocationMaster customerLocation, ICustomerLocationMasterRepo repo) => await repo.CreateAsync(customerLocation));
        group.MapPut("/", async (CustomerLocationMaster customerLocation, ICustomerLocationMasterRepo repo) => await repo.UpdateAsync(customerLocation));
        group.MapDelete("/{id:int}", async (int id, ICustomerLocationMasterRepo repo) => await repo.DeleteAsync(id));
        return group;
    }
    private static RouteGroupBuilder MapProjectEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/pagination", async ([AsParameters] PaginationModel pmodel, IProjectMasterRepo repo) =>
        {
            var data = await repo.GetAllWithPagination(pmodel);
            return new ProjectPDTO(data.Item1, data.Item2);
        });
        group.MapGet("/", async (IProjectMasterRepo repo) => await repo.GetAllAsync());
        group.MapGet("/{id:int}", async (int id, IProjectMasterRepo repo) => await repo.GetAsync(id));
        group.MapPost("/", async (ProjectMaster project, IProjectMasterRepo repo) => await repo.CreateAsync(project));
        group.MapPut("/", async (ProjectMaster project, IProjectMasterRepo repo) => await repo.UpdateAsync(project));
        group.MapDelete("/{id:int}", async (int id, IProjectMasterRepo repo) => await repo.DeleteAsync(id));
        return group;
    }
    private static RouteGroupBuilder MapCustomerMasterEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/pagination", async ([AsParameters] PaginationModel pmodel, ICustomerMasterRepo repo) =>
        {
            var data = await repo.GetAllWithPagination(pmodel);
            return new CustomerPDTO(data.Item1, data.Item2);
        });
        group.MapGet("/", async (ICustomerMasterRepo repo) => await repo.GetAllAsync());
        group.MapGet("/{id:int}", async (int id, ICustomerMasterRepo repo) => await repo.GetAsync(id));
        group.MapPost("/", async (CustomerMaster Customer, ICustomerMasterRepo repo) =>
        {
            var data = await repo.CreateAsync(Customer);
            return data;
        });
        group.MapPut("/", async (CustomerMaster Customer, ICustomerMasterRepo repo) => await repo.UpdateAsync(Customer));
        group.MapDelete("/{id:int}", async (int id, ICustomerMasterRepo repo) => await repo.DeleteAsync(id));
        group.MapGet("/nextcode", async (ICustomerMasterRepo repo) => await repo.GetNextCustomerCodeAsync());
        group.MapGet("/outstanding-balance/{customerId:int}", async (int customerId, ICustomerMasterRepo repo) => await repo.GetCustomerOutstandingBalanceAsync(customerId));
        return group;
    }
    private static RouteGroupBuilder MapSupplierMasterEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/pagination", async ([AsParameters] PaginationModel pmodel, ISupplierMasterRepo repo) =>
        {
            var data = await repo.GetAllWithPagination(pmodel);
            return new SupplierPDTO(data.Item1, data.Item2);
        });
        group.MapGet("/", async (ISupplierMasterRepo repo) => await repo.GetAllAsync());
        group.MapGet("/{id:int}", async (int id, ISupplierMasterRepo repo) => await repo.GetAsync(id));
        group.MapPost("/", async (SupplierMaster Supplier, ISupplierMasterRepo repo) =>
        {
            var data = await repo.CreateAsync(Supplier);
            return data;
        });
        group.MapPut("/", async (SupplierMaster Supplier, ISupplierMasterRepo repo) => await repo.UpdateAsync(Supplier));
        group.MapDelete("/{id:int}", async (int id, ISupplierMasterRepo repo) => await repo.DeleteAsync(id));
        group.MapGet("/nextcode", async (ISupplierMasterRepo repo) => await repo.GetNextSupplierCodeAsync());
        return group;
    }

    #region CustomerOPEntry
    private static RouteGroupBuilder MapCustomerOPEntryEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/pagination", async ([AsParameters] PaginationModel pmodel, ICustomerOPEntryRepo repo) =>
        {
            var data = await repo.GetAllWithPagination(pmodel);
            return new CustomerOpEntryPDTO(data.Item1, data.Item2);
        });
        group.MapGet("/", async (ICustomerOPEntryRepo repo) => await repo.GetAllAsync());

        group.MapGet("/{id:int}", async (int id, ICustomerOPEntryRepo repo) => await repo.GetFullAsync(id));

        group.MapGet("/newdocno", async (ICustomerOPEntryRepo repo) => await repo.GetNewDocNo());

        group.MapPost("/", async (CustomerOPEntryDTO dto, ICustomerOPEntryRepo repo) =>
        {
            // Mapping DTO to Entity
            var header = new CustomerOPEntry
            {
                CustomerOPEntryId = dto.CustomerOPEntryId,
                DocNo = dto.DocNo,
                EntryDate = dto.EntryDate,
                EnteredBy = dto.EnteredBy,
                Remarks = dto.Remarks,
                FK_BranchId = dto.FK_BranchId,
                FK_UserId = dto.FK_UserId
            };
            // Details handled inside Repo CreateAsync
            return await repo.CreateAsync(header, dto.EntryDetails);
        });

        return group;
    }
    #endregion

    #region SupplierOPEntry
    private static RouteGroupBuilder MapSupplierOPEntryEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/pagination", async ([AsParameters] PaginationModel pmodel, ISupplierOPEntryRepo repo) =>
        {
            var data = await repo.GetAllWithPagination(pmodel);
            return new SupplierOpEntryPDTO(data.Item1, data.Item2);
        });
        group.MapGet("/", async (ISupplierOPEntryRepo repo) => await repo.GetAllAsync());

        group.MapGet("/{id:int}", async (int id, ISupplierOPEntryRepo repo) => await repo.GetFullAsync(id));
        group.MapGet("/newdocno", async (ISupplierOPEntryRepo repo) => await repo.GetNewDocNo());
        group.MapPost("/", async (SupplierOPEntryDTO dto, ISupplierOPEntryRepo repo) =>
        {
            // Mapping DTO to Entity
            var header = new SupplierOPEntry
            {
                SupplierOPEntryId = dto.SupplierOPEntryId,
                DocNo = dto.DocNo,
                EntryDate = dto.EntryDate,
                EnteredBy = dto.EnteredBy,
                Remarks = dto.Remarks,
                FK_BranchId = dto.FK_BranchId,
                FK_UserId = dto.FK_UserId
            };
            // Details handled inside Repo CreateAsync
            return await repo.CreateAsync(header, dto.EntryDetails);
        });
        return group;
    }
    #endregion

    #region Storemaster
    private static RouteGroupBuilder MapStoreMasterEndpoints(this RouteGroupBuilder group)
    {

        group.MapGet("/", async (IStoreMasterRepo repo) => await repo.GetAllAsync());
        group.MapGet("/{id:int}", async (int id, IStoreMasterRepo repo) => await repo.GetAsync(id));
        group.MapPost("/", async (StoreMaster store, IStoreMasterRepo repo) => await repo.CreateAsync(store));
        group.MapPut("/", async (StoreMaster store, IStoreMasterRepo repo) => await repo.UpdateAsync(store));
        group.MapDelete("/{id:int}", async (int id, IStoreMasterRepo repo) => await repo.DeleteAsync(id));
        group.MapGet("/checkdefault", async (int excludeId, int? branchId, IStoreMasterRepo repo) =>
        {
            return await repo.CheckIfDefaultStoreExists(excludeId, branchId);
        });
        return group;
    }
    #endregion

    #region OpeningStockEntry
    private static RouteGroupBuilder MapOpeningStockEntryEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/pagination", async ([AsParameters] PaginationModel pmodel, IOpeningStockEntryRepo repo) =>
        {
            var data = await repo.GetAllWithPagination(pmodel);
            return new OpeningStockEntryPDTO(data.Item1, data.Item2);
        });
        group.MapGet("/", async (IOpeningStockEntryRepo repo) => await repo.GetAllAsync());

        group.MapGet("/{id:int}", async (int id, IOpeningStockEntryRepo repo) => await repo.GetFullAsync(id));

        group.MapGet("/newdocno", async (IOpeningStockEntryRepo repo) => await repo.GetNewDocNo());

        group.MapGet("/existing/{itemId}", async (int itemId, IOpeningStockEntryRepo repo) => await repo.GetExistingStockForItem(itemId));

        group.MapPost("/", async (OpeningStockEntryDTO dto, IOpeningStockEntryRepo repo) =>
        {
            var header = new OpeningStockEntry
            {
                OpeningStockEntryId = dto.OpeningStockEntryId,
                DocumentNo = dto.DocumentNo,
                EntryDate = dto.EntryDate,
                EnteredBy = dto.EnteredBy,
                Remarks = dto.Remarks,
                FK_BranchId = dto.FK_BranchId,
                FK_UserId = dto.FK_UserId
            };
            return await repo.CreateAsync(header, dto.EntryDetails);
        });

        group.MapPut("/", async (OpeningStockEntryDTO dto, IOpeningStockEntryRepo repo) =>
        {
            var header = new OpeningStockEntry
            {
                OpeningStockEntryId = dto.OpeningStockEntryId,
                DocumentNo = dto.DocumentNo,
                EntryDate = dto.EntryDate,
                EnteredBy = dto.EnteredBy,
                Remarks = dto.Remarks,
                FK_BranchId = dto.FK_BranchId,
                FK_UserId = dto.FK_UserId
            };
            return await repo.UpdateAsync(header, dto.EntryDetails);
        });

        group.MapDelete("/{id:int}", async (int id, IOpeningStockEntryRepo repo) => await repo.DeleteAsync(id));

        return group;
    }
    #endregion

    #region StockAdjustment
    private static RouteGroupBuilder MapStockAdjustmentEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/nextcode", async (IStockAdjustmentRepo repo) => await repo.GetVoucherNo(1)); // Auto-code generation
        group.MapGet("/NextNo/{branchId:int}", async (int branchId, IStockAdjustmentRepo repo) => await repo.GetNextVchrNo(branchId)); // Manual number suggestion
        group.MapGet("/{id:int}", async (int id, IStockAdjustmentRepo repo) => await repo.GetByIdAsync(id));
        group.MapPost("/", async (StockAdjustmentHdr hdr, IStockAdjustmentRepo repo) => await repo.CreateAsync(hdr));
        group.MapPut("/", async (StockAdjustmentHdr hdr, IStockAdjustmentRepo repo) => await repo.UpdateAsync(hdr));
        group.MapDelete("/{id:int}", async (int id, IStockAdjustmentRepo repo) => await repo.DeleteStockAdjustment(id));

        group.MapPost("/get-all", async (PaginationModel pmodel, IStockAdjustmentRepo repo) =>
        {
            var data = await repo.GetAllAsync(pmodel);
            return data;
        });

        group.MapGet("/current-stock", async (int itemId, int? storeId, int? excludeVoucherId, IStockAdjustmentRepo repo) => await repo.GetItemCurrentStock(itemId, storeId, excludeVoucherId));

        return group;
    }
    #endregion

    #region CompanyDetails
    private static RouteGroupBuilder MapCompanyDetailsEndpoints(this RouteGroupBuilder group)
    {
        // GET /api/companydetails/branch/{branchId} - Get company details by branch
        group.MapGet("/branch/{branchId:int}", async (int branchId, ICompanyDetailsRepo repo) =>
            await repo.GetByBranchIdAsync(branchId));

        // GET /api/companydetails/exists/{branchId} - Check if details exist for branch
        group.MapGet("/exists/{branchId:int}", async (int branchId, ICompanyDetailsRepo repo) =>
            await repo.ExistsForBranchAsync(branchId));

        // POST /api/companydetails - Create new company details
        group.MapPost("/", async (CompanyDetails details, ICompanyDetailsRepo repo) =>
            await repo.CreateAsync(details));

        // PUT /api/companydetails - Update company details
        group.MapPut("/", async (CompanyDetails details, ICompanyDetailsRepo repo) =>
            await repo.UpdateAsync(details));

        return group;
    }
    #endregion


    #region AccountOpeningEntry
    private static RouteGroupBuilder MapAccountOpeningEntryEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/pagination", async ([AsParameters] PaginationModel pmodel, IAccountOpeningEntryRepo repo) =>
        {
            var data = await repo.GetAllWithPagination(pmodel);
            var transactions = data.Item1.Select(e => new ExMasters
            {
                Id = e.AccountOpeningEntryId,
                Code = e.VoucherNo,
                IsSelected = true,
                Amount = e.DebitTotal ?? 0,
                Description = e.EntryDate?.ToString("dd-MM-yyyy") ?? ""
            }).ToList();
            return new TransactionPDTO(transactions, data.Item2);
        });
        group.MapGet("/{id:int}", async (int id, IAccountOpeningEntryRepo repo) => await repo.GetFullAsync(id));
        group.MapGet("/newdocno/{branchId:int}", async (int branchId, IAccountOpeningEntryRepo repo) => await repo.GetNewDocNo(branchId));
        group.MapGet("/existing/{branchId:int}", async (int branchId, IAccountOpeningEntryRepo repo) => await repo.GetExistingOpeningBalances(branchId));
        group.MapPost("/", async (AccountOpeningEntryDTO dto, IAccountOpeningEntryRepo repo) =>
        {
            var header = new AccountOpeningEntryHdr
            {
                AccountOpeningEntryId = dto.Header.AccountOpeningEntryId,
                VoucherNo = dto.Header.VoucherNo,
                VoucherDate = dto.Header.VoucherDate,
                EntryDate = dto.Header.EntryDate,
                DebitTotal = dto.Header.DebitTotal,
                CreditTotal = dto.Header.CreditTotal,
                Remarks = dto.Header.Remarks,
                FK_BranchId = dto.Header.FK_BranchId,
                FK_UserId = dto.Header.FK_UserId,
                FK_FinancialYearId = dto.Header.FK_FinancialYearId
            };
            return await repo.CreateAsync(header, dto.Details);
        });
        group.MapPut("/", async (AccountOpeningEntryDTO dto, IAccountOpeningEntryRepo repo) =>
        {
            var header = new AccountOpeningEntryHdr
            {
                AccountOpeningEntryId = dto.Header.AccountOpeningEntryId,
                VoucherNo = dto.Header.VoucherNo,
                VoucherDate = dto.Header.VoucherDate,
                EntryDate = dto.Header.EntryDate,
                DebitTotal = dto.Header.DebitTotal,
                CreditTotal = dto.Header.CreditTotal,
                Remarks = dto.Header.Remarks,
                FK_BranchId = dto.Header.FK_BranchId,
                FK_UserId = dto.Header.FK_UserId,
                FK_FinancialYearId = dto.Header.FK_FinancialYearId
            };
            return await repo.UpdateAsync(header, dto.Details);
        });
        group.MapDelete("/{id:int}", async (int id, IAccountOpeningEntryRepo repo) => await repo.DeleteAsync(id));
        return group;
    }
    #endregion

    #region DamageStock
    private static RouteGroupBuilder MapDamageStockEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/pagination", async ([AsParameters] PaginationModel pmodel, [FromServices] IDamageStockRepo repo) =>
        {
            var data = await repo.GetAllWithPagination(pmodel);
            return data;
        });
        group.MapGet("/{id:int}", async (int id, [FromServices] IDamageStockRepo repo) => await repo.GetById(id));
        group.MapGet("/NextNo/{branchId:int}", async (int branchId, [FromServices] IDamageStockRepo repo) => await repo.GetNextVoucherNo(branchId));
        group.MapPost("/", async (DamageStockDTO dto, [FromServices] IDamageStockRepo repo) => await repo.Create(dto));
        group.MapPut("/", async (DamageStockDTO dto, [FromServices] IDamageStockRepo repo) => await repo.Update(dto));
        group.MapDelete("/{id:int}", async (int id, [FromServices] IDamageStockRepo repo) => await repo.Delete(id));
        return group;
    }
    #endregion

    #endregion

    #region PermanentSettings

    private static RouteGroupBuilder MapPermanentSettingsEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async ([FromServices] IPermanentSettingsRepo repo) =>
            await repo.GetAllSettings()
        );

        group.MapPut("/", async (
            [FromBody] List<PermanentSettings> settings,
            [FromServices] IPermanentSettingsRepo repo
        ) =>
            await repo.UpdateSettings(settings)
        );

        return group;
    }
    #endregion

    #region Others
    private static RouteGroupBuilder MapGeneralEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/chartvalues", async (int branchId, string format, IGeneralRepo repo) => await repo.GetChartValues(branchId, format));
        group.MapGet("/arabictext", async (string text, HttpClient http) =>
        {
            GoogleTrans gtrans = new();
            var result = gtrans.Translate(text);
            return result;
        });
        group.MapGet("/combofill", async (string head, int branchId, IGeneralRepo repo) => await repo.GetComboFill(head, branchId));
        group.MapGet("/masters", async (string head, int branchId, IGeneralRepo repo) => await repo.GetMasters(head, branchId));
        group.MapGet("/decimalval", async (string head, int id, IGeneralRepo repo) => await repo.GetValueMethod(head, id));
        return group;
    }
    private static RouteGroupBuilder MapPrintDesignerEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/design", async (PrintDesignReq req, IPrintDesignRepo repo) =>
        {
            var response = await repo.GetAsync(req);
            return response;
        });
        group.MapPost("/", async (PrintDesignDTO dto, IPrintDesignRepo repo) => await repo.CreateAsync(dto));
        group.MapPut("/", async (PrintDesignDTO dto, IPrintDesignRepo repo) => await repo.UpdateAsync(dto));
        group.MapDelete("/{id:int}", async (int id, IPrintDesignRepo repo) => await repo.DeleteAsync(id));
        group.MapPost("/pagination", async (PaginationModel pmodel, IPrintDesignRepo repo) =>
        {
            var data = await repo.GetAllWithPagination(pmodel);

            if (data is null)
                return Results.NoContent(); // <-- AND Change Refit return type (see below)

            return Results.Ok(new TransactionPDTO(data.PTransactions, data.TotalCount));
        });
        return group;
    }
    private static RouteGroupBuilder MapBarcodeDesignEndpoint(this RouteGroupBuilder group)
    {
        group.MapPost("/pagination", async (PaginationModel pmodel, IBarcodeDesignRepo repo) => await repo.GetAllWithPagination(pmodel));
        group.MapGet("/{id:int}", async (int id, IBarcodeDesignRepo repo) => await repo.GetAsync(id));
        group.MapGet("/", async (IBarcodeDesignRepo repo) => await repo.GetAsync());
        group.MapPost("/", async (BarcodeDesignDTO dto, IBarcodeDesignRepo repo) => await repo.CreateAsync(dto));
        group.MapPut("/", async (BarcodeDesignDTO dto, IBarcodeDesignRepo repo) => await repo.UpdateAsync(dto));
        group.MapDelete("/{id:int}", async (int id, IBarcodeDesignRepo repo) => await repo.DeleteAsync(id));
        group.MapGet("/all", async (IBarcodeDesignRepo repo) => await repo.GetAllDesignsAsync());
        return group;
    }
    private static RouteGroupBuilder MapZatcaEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/pendingcount", async (ICVReqDTO dto, IZatcaRepo repo) => await repo.GetPendingIcvDataAsync(dto));
        group.MapPost("/pendinglist", async (IcvPendReqDTO pmodel, IZatcaRepo repo) => await repo.GetPendingIcvListAsync(pmodel));
        group.MapPost("/all", async (IcvReuploadAll dto, IZatcaRepo repo, IWebHostEnvironment? env) =>
        {
            await repo.ReUploadAll(dto.icvs,dto.Company);
        });
        group.MapPost("/selected", async (IcvReupload dto, IZatcaRepo repo, IWebHostEnvironment? env) =>
        {
            return await repo.ReUploadSelected(dto.icv,  dto.Company);
        });
        group.MapPost("/generatecsr", async (CSRRequest request, IZatcaRepo repo) => await repo.GenerateCSR(request));
        group.MapGet("/{id:int}", async (int id, IZatcaRepo repo) => await repo.GetCSR(id));
        group.MapPost("/", async (CSRResponse dto, IZatcaRepo repo) => await repo.AddNewCSR(dto));
        group.MapGet("/", async (IZatcaRepo repo) => await repo.GetFailedCount());
        return group;
    }
    private static RouteGroupBuilder MapZatcaPendingEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async ([AsParameters] PaginationModel pmodel, IZatcaInvoiceRepo repo) =>
        await repo.GetAllZatcaInvoiceWithPagination(pmodel));
        group.MapPost("/", async (int BranchId, IZatcaUploadService service) => await service.UploadPendingSimplifiedInvoices(BranchId));
        return group;
    }
    #endregion

    #region Admin

    #region AdminCompany
    private static RouteGroupBuilder MapAdminCompanyEndpoint(this RouteGroupBuilder group)
    {
        group.MapGet("/pagination", async ([AsParameters] PaginationModel pmodel, ICompanyCreationRepo repo) =>
        {
            var data = await repo.GetAllWithPagination(pmodel);
            return data;
        });
        group.MapGet("/", async (ICompanyCreationRepo repo) => await repo.GetAllAsync());
        group.MapGet("/{id:int}", async (string id, ICompanyCreationRepo repo) => await repo.GetAsync(new LoginCompany { CompanyCode = id }));
        group.MapPost("/", async (ExLCompany exlcompany, ICompanyCreationRepo repo, ICompanyHelper chRepo) =>
        {
            try
            {
                var company = new LoginCompany
                {
                    CompanyCode = exlcompany.CompanyCode,
                    CompanyName = exlcompany.CompanyName,
                    Email = exlcompany.Email,
                    ContactNo = exlcompany.ContactNo,
                    CompanyAddress = exlcompany.CompanyAddress,
                    SubscriptionType = exlcompany.SubscriptionType,
                    ConnectionString = exlcompany.ConnectionString,
                    CreatedDate = exlcompany.CreatedDate,
                    DBMainVersion = exlcompany.DBMainVersion,
                    DBSubVersion = exlcompany.DBSubVersion,
                    IsExpired = exlcompany.IsExpired,
                    SubscriptionEndingDate = exlcompany.SubscriptionEndingDate,
                    SubscriptionStartedAt = exlcompany.SubscriptionStartedAt,
                    Id = exlcompany.Id

                };
                if (await repo.Exist(company))
                {
                    return Results.BadRequest("Company Already Exist!");
                }

                var result = await chRepo.CreateCompanyDatabase(exlcompany);
                if (result.Item1 && !string.IsNullOrEmpty(result.Item2.ConnectionString))
                {
                    var data = await repo.CreateAsync(result.Item2);
                    return Results.Ok(data);
                }
                else
                {
                    return Results.BadRequest("Failed to create company database");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        });
        group.MapPut("/", async (ExLCompany exlcompany, ICompanyCreationRepo repo, ICompanyHelper chRepo) =>
        {
            try
            {
                var company = exlcompany.CopyTo<LoginCompany>();
                if (company != null)
                {
                    var updatedcompany = await chRepo.UpdateDatabase(company);
                    var data = await repo.UpdateAsync(company!);
                    return Results.Ok(company);
                }
                else
                {
                    return Results.BadRequest("Failed to create company database");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        });
        group.MapDelete("/{id:int}", async (int id, ICompanyCreationRepo repo) => await repo.DeleteAsync(id));
        return group;
    }
    #endregion

    #region Admin User
    private static RouteGroupBuilder MapAdminUserEndpoint(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (IAdminUserRepo repo) => await repo.GetAllAsync());
        group.MapPost("/login", async (LoginRequest req, IAdminUserRepo repo) => await repo.GetForLoginAsync(req.EmailOrUserName, req.Password));
        group.MapPost("/", async (AdminUser adminUser, IAdminUserRepo repo) =>
        {
            if (await repo.Exist(adminUser)) { return Results.BadRequest("Username Already Exist!"); }
            var data = await repo.CreateAsync(adminUser);
            return Results.Ok(data);
        });
        group.MapPut("/", async (AdminUser adminUser, IAdminUserRepo repo) => await repo.UpdateAsync(adminUser));
        group.MapDelete("/{id:int}", async (int id, IAdminUserRepo repo) => await repo.DeleteAsync(id));
        return group;
    }
    #endregion

    #endregion

    #region Transactions

    #region Sales
    private static RouteGroupBuilder MapSalesQuotationEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/NextNo/{id:int}", async (int id, ISalesQuotationRepo repo) =>
        {
            try
            {
                int ival = await repo.GetNextVchrNo(id);
                return ival;
            }
            catch (Exception ex)
            {
                string s = ex.Message;
                return 1;
            }

        });
        group.MapGet("/pagination", async ([AsParameters] PaginationModel pmodel, ISalesQuotationRepo repo) =>
        {
            var data = await repo.GetAllWithPagination(pmodel);
            return data;
        });
        group.MapGet("/{id:int}", async (int id, ISalesQuotationRepo repo) => await repo.GetAsync(id));
        group.MapPost("/", async (SalesQuotationDTO salesQuotation, ISalesQuotationRepo repo) => await repo.CreateAsync(salesQuotation));
        group.MapPut("/", async (SalesQuotationDTO salesQuotation, ISalesQuotationRepo repo) => await repo.UpdateAsync(salesQuotation));
        group.MapDelete("/{id:int}", async (int id, ISalesQuotationRepo repo) => await repo.DeleteAsync(id));
        group.MapGet("/nextcode", async (ISalesQuotationRepo repo) => await repo.GetNextQuotationNoAsync());
        return group;
    }
    private static RouteGroupBuilder MapSalesOrderEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/NextNo/{id:int}", async (int id, ISalesOrderRepo repo) =>
        {
            try
            {
                int ival = await repo.GetNextVchrNo(id);
                return ival;
            }
            catch (Exception ex)
            {
                string s = ex.Message;
                return 1;
            }

        });
        group.MapGet("/pagination", async ([AsParameters] PaginationModel pmodel, ISalesOrderRepo repo) =>
        {
            var data = await repo.GetAllWithPagination(pmodel);
            return data;
        });
        group.MapGet("/{id:int}", async (int id, ISalesOrderRepo repo) => await repo.GetAsync(id));
        group.MapPost("/", async (SalesOrderDTO salesOrder, ISalesOrderRepo repo) => await repo.CreateAsync(salesOrder));
        group.MapPut("/", async (SalesOrderDTO salesOrder, ISalesOrderRepo repo) => await repo.UpdateAsync(salesOrder));
        group.MapDelete("/{id:int}", async (int id, ISalesOrderRepo repo) => await repo.DeleteAsync(id));
        group.MapGet("/nextcode", async ([FromServices] ISalesOrderRepo repo) => await repo.GetNextSalesOrderNoAsync());
        return group;
    }
    private static RouteGroupBuilder MapDeliveryNoteEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/NextNo/{id:int}", async (int id, IDeliveryNoteRepo repo) =>
        {
            try
            {
                int ival = await repo.GetNextVchrNo(id);
                return ival;
            }
            catch (Exception ex)
            {
                string s = ex.Message;
                return 1;
            }

        });
        group.MapGet("/pagination", async ([AsParameters] PaginationModel pmodel, IDeliveryNoteRepo repo) =>
        {
            var data = await repo.GetAllWithPagination(pmodel);
            return data;
        });
        group.MapGet("/{id:int}", async (int id, IDeliveryNoteRepo repo) => await repo.GetAsync(id));
        group.MapPost("/", async (DeliveryNoteDTO deliveryNote, IDeliveryNoteRepo repo) => await repo.CreateAsync(deliveryNote));
        group.MapPut("/", async (DeliveryNoteDTO deliveryNote, IDeliveryNoteRepo repo) => await repo.UpdateAsync(deliveryNote));
        group.MapDelete("/{id:int}", async (int id, IDeliveryNoteRepo repo) => await repo.DeleteAsync(id));
        group.MapGet("/nextcode", async ([FromServices] IDeliveryNoteRepo repo) => await repo.GetNextDeliveryNoteNoAsync());
        group.MapGet("/pending/{customerId:int}", async (int customerId, IDeliveryNoteRepo repo) => await repo.GetPendingDeliveryNotes(customerId));
        return group;
    }
    private static RouteGroupBuilder MapDeliveryNoteRtnEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/nextno/{id}", async (int id, IDeliveryNoteRtnRepo repo) =>
        {
            try
            {
                int ival = await repo.GetNextVchrNo(id);
                return ival;
            }
            catch (Exception ex)
            {
                return 1;
            }
        });
        group.MapGet("/pagination", async ([AsParameters] PaginationModel pmodel, IDeliveryNoteRtnRepo repo) =>
        {
            var data = await repo.GetAllWithPagination(pmodel);
            return data;
        });
        group.MapGet("/{id:int}", async (int id, IDeliveryNoteRtnRepo repo) => await repo.GetAsync(id));
        group.MapPost("/", async (DeliveryNoteRtnDTO deliveryNoteRtn, IDeliveryNoteRtnRepo repo) => await repo.CreateAsync(deliveryNoteRtn));
        group.MapPut("/", async (DeliveryNoteRtnDTO deliveryNoteRtn, IDeliveryNoteRtnRepo repo) => await repo.UpdateAsync(deliveryNoteRtn));
        group.MapDelete("/{id:int}", async (int id, IDeliveryNoteRtnRepo repo) => await repo.DeleteAsync(id));
        return group;
    }
    private static RouteGroupBuilder MapSalesInvoiceEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/NextNo/{id:int}", async (int id, ISalesInvoiceRepo repo) =>
        {
            try
            {
                int ival = await repo.GetNextVchrNo(id);
                return ival;
            }
            catch (Exception ex)
            {
                string s = ex.Message;
                return 1;
            }

        });
        group.MapPost("/pagination", async (PaginationModel pmodel, ISalesInvoiceRepo repo) =>
        {
            var data = await repo.GetAllWithPagination(pmodel);
            return data;
        });
        group.MapGet("/{id:int}", async (int id, ISalesInvoiceRepo repo) => await repo.GetAsync(id));
        group.MapPost("/", async (SalesInvoiceDTO salesInvoice, ISalesInvoiceRepo repo) => await repo.CreateAsync(salesInvoice));
        group.MapPut("/", async (SalesInvoiceDTO salesInvoice, ISalesInvoiceRepo repo) => await repo.UpdateAsync(salesInvoice));
        group.MapDelete("/{id:int}", async (int id, ISalesInvoiceRepo repo) => await repo.DeleteAsync(id));
        group.MapGet("/nextcode", async ([FromServices] ISalesInvoiceRepo repo) => await repo.GetNextInvoiceNoAsync());
        group.MapGet("/oldest-unpaid-date/{customerId:int}", async (int customerId, ISalesInvoiceRepo repo) =>
            await repo.GetOldestUnpaidInvoiceDateAsync(customerId));
        return group;
    }
    private static RouteGroupBuilder MapSalesInvoiceHoldEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/pagination", async (PaginationModel pmodel, ISalesInvoiceHoldRepo repo) => await repo.GetAllWithPagination(pmodel));
        group.MapGet("/{id:int}", async (int id, ISalesInvoiceHoldRepo repo) => await repo.GetAsync(id));
        group.MapPost("/", async (SalesInvoiceHoldDTO salesInvoiceHold, ISalesInvoiceHoldRepo repo) => await repo.CreateAsync(salesInvoiceHold));
        group.MapDelete("/{id:int}", async (int id, ISalesInvoiceHoldRepo repo) => await repo.DeleteAsync(id));
        group.MapDelete("/all/{branchId:int}", async (int branchId, ISalesInvoiceHoldRepo repo, string? terminal = null) => await repo.DeleteAllAsync(branchId, terminal));
        group.MapGet("/bybranch/{branchId:int}", async (int branchId, ISalesInvoiceHoldRepo repo, string? terminal = null) => await repo.GetAllHeldAsync(branchId, terminal));
        return group;
    }
    private static RouteGroupBuilder MapSalesInvoiceRtnEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/NextNo/{id:int}", async (int id, ISalesInvoiceRtnRepo repo) =>
        {
            try
            {
                int ival = await repo.GetNextVchrNo(id);
                return ival;
            }
            catch (Exception ex)
            {
                return 1;
            }
        });
        group.MapGet("/pagination", async ([AsParameters] PaginationModel pmodel, ISalesInvoiceRtnRepo repo) =>
        {
            var data = await repo.GetAllWithPagination(pmodel);
            return data;
        });
        group.MapGet("/{id:int}", async (int id, ISalesInvoiceRtnRepo repo) => await repo.GetAsync(id));
        group.MapPost("/", async (SalesInvoiceRtnDTO salesInvoiceRtn, ISalesInvoiceRtnRepo repo) => await repo.CreateAsync(salesInvoiceRtn));
        group.MapPut("/", async (SalesInvoiceRtnDTO salesInvoiceRtn, ISalesInvoiceRtnRepo repo) => await repo.UpdateAsync(salesInvoiceRtn));
        group.MapDelete("/{id:int}", async (int id, ISalesInvoiceRtnRepo repo) => await repo.DeleteAsync(id));
        group.MapGet("/nextcode", async ([FromServices] ISalesInvoiceRtnRepo repo) => await repo.GetNextSalesInvoiceRtnNoAsync());
        return group;
    }
    #endregion

    #region Purchase
    private static RouteGroupBuilder MapPurchaseOrderEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/NextNo/{id:int}", async (int id, [FromServices] IPurchaseOrderRepo repo) =>
        {
            try
            {
                int ival = await repo.GetNextVchrNo(id);
                return ival;
            }
            catch (Exception ex)
            {
                string s = ex.Message;
                return 1;
            }

        });
        group.MapGet("/pagination", async ([AsParameters] PaginationModel pmodel, [FromServices] IPurchaseOrderRepo repo) =>
        {
            var data = await repo.GetAllWithPagination(pmodel);
            return data;
        });
        group.MapGet("/{id:int}", async (int id, [FromServices] IPurchaseOrderRepo repo) => await repo.GetAsync(id));
        group.MapPost("/", async (PurchaseOrderDTO purchaseOrder, [FromServices] IPurchaseOrderRepo repo) => await repo.CreateAsync(purchaseOrder));
        group.MapPut("/", async (PurchaseOrderDTO purchaseOrder, [FromServices] IPurchaseOrderRepo repo) => await repo.UpdateAsync(purchaseOrder));
        group.MapDelete("/{id:int}", async (int id, [FromServices] IPurchaseOrderRepo repo) => await repo.DeleteAsync(id));
        group.MapGet("/nextcode", async ([FromServices] IPurchaseOrderRepo repo) => await repo.GetNextPurchaseOrderNoAsync());
        return group;
    }
    #endregion

    #region Goods Receipt
    private static RouteGroupBuilder MapGoodsReceiptEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/NextNo/{id:int}", async (int id, [FromServices] IGoodsReceiptRepo repo) =>
        {
            try
            {
                string sval = await repo.GetNewDocNo(id);
                return sval;
            }
            catch (Exception ex)
            {
                string s = ex.Message;
                return "1001";
            }

        });
        group.MapGet("/pagination", async ([AsParameters] PaginationModel pmodel, [FromServices] IGoodsReceiptRepo repo) =>
        {
            var data = await repo.GetAllWithPagination(pmodel);
            return data;
        });
        group.MapGet("/{id:int}", async (int id, [FromServices] IGoodsReceiptRepo repo) => await repo.GetById(id));
        group.MapPost("/", async (GoodsReceiptDTO goodsReceipt, [FromServices] IGoodsReceiptRepo repo) => await repo.Create(goodsReceipt));
        group.MapPut("/", async (GoodsReceiptDTO goodsReceipt, [FromServices] IGoodsReceiptRepo repo) => await repo.Update(goodsReceipt));
        group.MapDelete("/{id:int}", async (int id, [FromServices] IGoodsReceiptRepo repo) => await repo.Delete(id));
        group.MapGet("/nextcode", async ([FromServices] IGoodsReceiptRepo repo) => await repo.GetNextGoodsReceiptNoAsync());
        return group;
    }
    #endregion

    #region Proforma Invoice
    private static RouteGroupBuilder MapProformaInvoiceEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/NextNo/{id:int}", async (int id, [FromServices] IProformaInvoiceRepo repo) =>
        {
            try
            {
                int ival = await repo.GetNextVchrNo(id);
                return ival;
            }
            catch (Exception ex)
            {
                string s = ex.Message;
                return 1;
            }

        });
        group.MapGet("/pagination", async ([AsParameters] PaginationModel pmodel, [FromServices] IProformaInvoiceRepo repo) =>
        {
            var data = await repo.GetAllWithPagination(pmodel);
            return data;
        });
        group.MapGet("/{id:int}", async (int id, [FromServices] IProformaInvoiceRepo repo) => await repo.GetAsync(id));
        group.MapPost("/", async (ProformaInvoiceDTO proformaInvoice, [FromServices] IProformaInvoiceRepo repo) => await repo.CreateAsync(proformaInvoice));
        group.MapPut("/", async (ProformaInvoiceDTO proformaInvoice, [FromServices] IProformaInvoiceRepo repo) => await repo.UpdateAsync(proformaInvoice));
        group.MapDelete("/{id:int}", async (int id, [FromServices] IProformaInvoiceRepo repo) => await repo.DeleteAsync(id));
        group.MapGet("/nextcode", async ([FromServices] IProformaInvoiceRepo repo) => await repo.GetNextProformaInvoiceNoAsync());
        group.MapGet("/fromquotation/{quotationId:int}", async (int quotationId, [FromServices] IProformaInvoiceRepo repo) => await repo.CreateFromQuotationAsync(quotationId));
        return group;
    }
    #endregion

    #region Enquiry
    private static RouteGroupBuilder MapEnquiryEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/NextNo/{branchId:int}", async (int branchId, IEnquiryRepo repo) => await repo.GetNextEnquiryNoAsync("", branchId));
        group.MapGet("/{id:int}", async (int id, IEnquiryRepo repo) => await repo.GetByIdAsync(id));
        group.MapGet("/", async (IEnquiryRepo repo) => await repo.GetAllAsync(""));
        group.MapPost("/", async (RFQHdr rfq, IEnquiryRepo repo) => await repo.SaveAsync(rfq));
        group.MapPut("/", async (RFQHdr rfq, IEnquiryRepo repo) => await repo.UpdateAsync(rfq));
        group.MapDelete("/{id:int}", async (int id, IEnquiryRepo repo) => await repo.DeleteAsync(id));
        return group;
    }
    #endregion

    #region Purchase Return
    private static RouteGroupBuilder MapPurchaseReturnEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/NextNo/{id:int}", async (int id, [FromServices] IPurchaseReturnRepo repo) =>
        {
            try
            {
                int ival = await repo.GetNextVchrNo(id);
                return ival;
            }
            catch (Exception ex)
            {
                return 1;
            }
        });
        group.MapGet("/pagination", async ([AsParameters] PaginationModel pmodel, [FromServices] IPurchaseReturnRepo repo) =>
        {
            var data = await repo.GetAllWithPagination(pmodel);
            return data;
        });
        group.MapGet("/{id:int}", async (int id, [FromServices] IPurchaseReturnRepo repo) => await repo.GetAsync(id));
        group.MapPost("/", async (PurchaseReturnDTO dto, [FromServices] IPurchaseReturnRepo repo) => await repo.CreateAsync(dto));
        group.MapPut("/", async (PurchaseReturnDTO dto, [FromServices] IPurchaseReturnRepo repo) => await repo.UpdateAsync(dto));
        group.MapDelete("/{id:int}", async (int id, [FromServices] IPurchaseReturnRepo repo) => await repo.DeleteAsync(id));
        group.MapGet("/nextcode", async ([FromServices] IPurchaseReturnRepo repo) => await repo.GetNextPurchaseReturnNoAsync());
        group.MapGet("/balance/{purchaseId}", async (int purchaseId, [FromServices] IPurchaseReturnRepo repo) => await repo.GetPurchaseReturnBalance(purchaseId));
        return group;
    }

    #endregion

    #region Journal Entry
    private static RouteGroupBuilder MapJournalEntryEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (IJournalEntryRepo repo) => await repo.GetAllAsync());
        group.MapGet("/{id:int}", async (int id, IJournalEntryRepo repo) => await repo.GetByIdAsync(id));
        group.MapPost("/", async (ZakiSAASERP.Models.Transactions.Accounts.JournalEntryDTO entry, IJournalEntryRepo repo) => await repo.CreateAsync(entry));
        group.MapPut("/", async (ZakiSAASERP.Models.Transactions.Accounts.JournalEntryDTO entry, IJournalEntryRepo repo) => await repo.UpdateAsync(entry));
        group.MapDelete("/{id:int}", async (int id, IJournalEntryRepo repo) => await repo.DeleteAsync(id));
        group.MapGet("/nextno", async (IJournalEntryRepo repo) => await repo.GetNextJournalNoAsync());
        return group;
    }
    #endregion

    #region Expense Entry
    private static RouteGroupBuilder MapExpenseEntryEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (IExpenseEntryRepo repo) => await repo.GetAllAsync());
        group.MapGet("/{id:int}", async (int id, IExpenseEntryRepo repo) => await repo.GetByIdAsync(id));
        group.MapPost("/", async (ZakiSAASERP.Models.Transactions.Accounts.ExpenseEntryDTO entry, IExpenseEntryRepo repo) => await repo.CreateAsync(entry));
        group.MapPut("/", async (ZakiSAASERP.Models.Transactions.Accounts.ExpenseEntryDTO entry, IExpenseEntryRepo repo) => await repo.UpdateAsync(entry));
        group.MapDelete("/{id:int}", async (int id, IExpenseEntryRepo repo) => await repo.DeleteAsync(id));
        group.MapGet("/nextcode", async (IExpenseEntryRepo repo) => await repo.GetNextExpenseNoAsync());
        group.MapPost("/pagination", async ([FromBody] PaginationModel pmodel, [FromServices] IExpenseEntryRepo repo) =>
        {
            var data = await repo.GetAllWithPagination(pmodel);
            var transactions = data.Item1.Select(e => new ExMasters
            {
                Id = e.ExpenseEntryId,
                Code = e.ExpenseNo,
                Description = e.EntryDate.ToString("dd/MM/yyyy"),
                Amount = e.NetTotal
            }).ToList();
            return new TransactionPDTO(transactions, data.Item2);
        });
        return group;
    }

    #region Customer Receipt
    private static RouteGroupBuilder MapCustomerReceiptEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (ICustomerReceiptRepo repo) => await repo.GetAllAsync());
        group.MapGet("/{id:int}", async (int id, ICustomerReceiptRepo repo) => await repo.GetByIdAsync(id));
        group.MapPost("/", async (ZakiSAASERP.Models.Transactions.Accounts.CustomerReceiptDTO receipt, ICustomerReceiptRepo repo) => await repo.CreateAsync(receipt));
        group.MapPut("/", async (ZakiSAASERP.Models.Transactions.Accounts.CustomerReceiptDTO receipt, ICustomerReceiptRepo repo) => await repo.UpdateAsync(receipt));
        group.MapDelete("/{id:int}", async (int id, ICustomerReceiptRepo repo) => await repo.DeleteAsync(id));
        group.MapGet("/nextno", async (ICustomerReceiptRepo repo) => await repo.GetNextReceiptNoAsync());
        group.MapGet("/customer/{customerId:int}/invoices", async (int customerId, int branchId, ICustomerReceiptRepo repo) =>
            await repo.GetCustomerOutstandingInvoicesAsync(customerId, branchId));
        group.MapGet("/customer/{customerId:int}/balance", async (int customerId, int branchId, ICustomerReceiptRepo repo) =>
            await repo.GetCustomerBalanceAsync(customerId, branchId));
        return group;
    }
    #endregion

    #endregion

    #endregion

    #region Menu Visibility
    private static RouteGroupBuilder MapMenuVisibilityEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/configuration", async ([FromServices] ZakiSAASERP.Interface.Settings.IMenuVisibilityRepo repo) =>
        {
            try
            {
                return await repo.GetMenuConfigurationAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("========== ERROR ==========");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("===========================");
                throw; // 🔥 ensures 500 with real error
            }
        });
        group.MapGet("/erptypes", async ([FromServices] ZakiSAASERP.Interface.Settings.IMenuVisibilityRepo repo) => await repo.GetAllERPTypesAsync());
        group.MapGet("/preset/{erpTypeId:int}", async (int erpTypeId, [FromServices] ZakiSAASERP.Interface.Settings.IMenuVisibilityRepo repo) => await repo.GetERPTypePresetAsync(erpTypeId));
        group.MapPut("/erptype/{erpTypeId:int}", async (int erpTypeId, [FromQuery] string modifiedBy, [FromServices] ZakiSAASERP.Interface.Settings.IMenuVisibilityRepo repo) => await repo.UpdateClientERPTypeAsync(erpTypeId, modifiedBy));
        group.MapPost("/custom", async ([FromBody] List<ZakiSAASERP.Models.Settings.MenuVisibilityItem> items, [FromServices] ZakiSAASERP.Interface.Settings.IMenuVisibilityRepo repo) => await repo.SaveCustomMenuVisibilityAsync(items));
        group.MapGet("/visible", async ([FromServices] ZakiSAASERP.Interface.Settings.IMenuVisibilityRepo repo) => await repo.GetVisibleMenusAsync());
        group.MapGet("/all", async ([FromServices] ZakiSAASERP.Interface.Settings.IMenuVisibilityRepo repo) => await repo.GetAllMenuDetailsAsync());
        return group;
    }

    #region Purchase
    private static RouteGroupBuilder MapPurchaseEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/NextNo/{id}", async (int id, [FromServices] IPurchaseRepo repo) =>
        {
            try
            {
                int ival = await repo.GetNextVchrNo(id);
                return ival;
            }
            catch (Exception ex)
            {
                string s = ex.Message;
                return 1;
            }
        });

        group.MapGet("/pagination", async ([AsParameters] PaginationModel pmodel, [FromServices] IPurchaseRepo repo) =>
        {
            var data = await repo.GetAllWithPagination(pmodel);
            return new TransactionPDTO(data.PTransactions, data.TotalCount);
        });

        group.MapGet("/{id:int}", async (int id, [FromServices] IPurchaseRepo repo) => await repo.GetAsync(id));

        group.MapPost("/", async (PurchaseDTO purchase, [FromServices] IPurchaseRepo repo) => await repo.CreateAsync(purchase));

        group.MapPut("/", async (PurchaseDTO purchase, [FromServices] IPurchaseRepo repo) => await repo.UpdateAsync(purchase));

        group.MapDelete("/{id:int}", async (int id, [FromServices] IPurchaseRepo repo) => await repo.DeleteAsync(id));

        group.MapGet("/nextcode", async ([FromServices] IPurchaseRepo repo) => await repo.GetNextPurchaseNoAsync());

        return group;
    }

    private static RouteGroupBuilder MapSupplierPaymentEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/NextNo/{id:int}", async (int id, [FromServices] ISupplierPaymentRepo repo) =>
        {
            try
            {
                return await repo.GetNextVoucherNoAsync(id);
            }
            catch
            {
                return "1";
            }
        });
        group.MapGet("/pagination", async ([AsParameters] PaginationModel pmodel, [FromServices] ISupplierPaymentRepo repo) =>
        {
            var data = await repo.GetAllWithPagination(pmodel);
            return new SupplierPaymentPDTO(data.PTransactions, data.TotalCount);
        });
        group.MapGet("/{id:int}", async (int id, [FromServices] ISupplierPaymentRepo repo) => await repo.GetAsync(id));
        group.MapPost("/", async (SupplierPaymentDTO dto, [FromServices] ISupplierPaymentRepo repo) => await repo.CreateAsync(dto));
        group.MapPut("/", async (SupplierPaymentDTO dto, [FromServices] ISupplierPaymentRepo repo) => await repo.UpdateAsync(dto));
        group.MapDelete("/{id:int}", async (int id, [FromServices] ISupplierPaymentRepo repo) => await repo.DeleteAsync(id));
        group.MapGet("/pendinginvoices/{supplierId:int}", async (int supplierId, [FromServices] ISupplierPaymentRepo repo) => await repo.GetPendingPurchaseInvoices(supplierId));
        group.MapGet("/supplier/{supplierId:int}/balance", async (int supplierId, int branchId, [FromServices] ISupplierPaymentRepo repo) =>
            await repo.GetSupplierBalanceAsync(supplierId, branchId));
        return group;
    }
    #endregion
    #endregion

    #region Reports
    private static RouteGroupBuilder MapTransactionReportEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/monitor", async (TransactionMonitorRequest req, ITransactionReportRepo repo) =>
        {
            return await repo.GetTransactionMonitorReportAsync(req);
        });
        return group;
    }
    private static RouteGroupBuilder MapSupplierReportEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/statement", async (DateTime fromDate, DateTime toDate, int branchId, int? supplierId, ISupplierReportRepo repo) =>
        {
            return await repo.GetSupplierStatementAsync(fromDate, toDate, branchId, supplierId);
        });
        group.MapGet("/summary", async (DateTime fromDate, DateTime toDate, int branchId, int? supplierId, ISupplierReportRepo repo) =>
        {
            return await repo.GetSupplierSummaryAsync(fromDate, toDate, branchId, supplierId);
        });
        group.MapGet("/list", async (ISupplierReportRepo repo) =>
        {
            return await repo.GetSupplierListAsync();
        });
        group.MapGet("/supplierpendinginvoice", async (DateTime fromDate, DateTime toDate, int branchId, int? supplierId, ISupplierReportRepo repo) =>
        {
            return await repo.GetSupplierPendingInvoiceAsync(fromDate, toDate, branchId, supplierId);
        });
        group.MapGet("/supplierpayment", async (DateTime fromDate, DateTime toDate, int branchId, int? supplierId, string? billType, ISupplierReportRepo repo) =>
        {
            return await repo.GetSupplierPaymentReportAsync(fromDate, toDate, branchId, supplierId, billType);
        });
        return group;
    }
    private static RouteGroupBuilder MapCustomerReportEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/statement", async (CustomerStatementRequest req, ICustomerReportRepo repo) => await repo.GetCustomerStatementAsync(req));
        group.MapGet("/summary", async (DateTime fromDate, DateTime toDate, int branchId, int? customerId, ICustomerReportRepo repo) => await repo.GetCustomerSummaryAsync(fromDate, toDate, branchId, customerId));
        group.MapGet("/list", async (ICustomerReportRepo repo) =>
        {
            return await repo.GetCustomerListAsync();
        });
        group.MapGet("/customerpendinginvoice", async (DateTime fromDate, DateTime toDate, int branchId, int? customerId, ICustomerReportRepo repo) =>
        {
            return await repo.GetCustomerPendingInvoiceAsync(fromDate, toDate, branchId, customerId);
        });
        return group;
    }
    private static RouteGroupBuilder MapInventoryReportEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/stockreport", async (StockReportRequest req, IInventoryReportRepo repo) =>
        {
            try
            {
                var result = await repo.GetStockReportAsync(req);

                return result;
            }
            catch (Exception)
            {

                throw;
            }
        });
        group.MapPost("/stocktransferreport", async (StockTransferReportRequest req, IInventoryReportRepo repo) =>
        {
            try
            {
                var result = await repo.GetStockTransferReportAsync(req);

                return result;
            }
            catch (Exception)
            {

                throw;
            }
        });
        group.MapPost("/itemtransactionhistory", async (ItemTransactionHistoryReportRequest req, IInventoryReportRepo repo) =>
        {
            try
            {
                var result = await repo.GetItemTransactionHistoryReportAsync(req);
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        });
        group.MapPost("/stockadjustmentreport", async (StockAdjustmentReportRequest req, IInventoryReportRepo repo) =>
        {
            try
            {
                var result = await repo.GetStockAdjustmentReportAsync(req);

                return result;
            }
            catch (Exception)
            {

                throw;
            }
        });
        return group;
    }
    private static RouteGroupBuilder MapSalesReportEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/", async (SalesReportRequest req, ISalesReportRepo repo) => await repo.GetSalesReportAsync(req));
        group.MapPost("/salessrvrrpt", async (SalesReportRequest req, ISalesReportRepo repo, IPrintDesignRepo prepo, SyncfusionPrintHelper printHelper) =>
        {
            var design = await prepo.GetAsync(new PrintDesignReq(0, "Sales_Report"));
            var result = await repo.GetSalesReportAsync(req);
            var reportdtls = result.Reports;
            var reportHdr = new SalesReportHdr(
               $"{req.FromDate:dd-MM-yyyy} - {req.ToDate:dd-MM-yyyy}",
               DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
               (result.TotalGross ?? 0).ToString("N2"),
               (result.TotalDiscount ?? 0).ToString("N2"),
               (result.TotalAmount ?? 0).ToString("N2"),
               (result.TotalVat ?? 0).ToString("N2"),
               (result.TotalNet ?? 0).ToString("N2")
           );


            Dictionary<string, object?> ReportHdr = reportHdr.GetType().GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(reportHdr));

            Dictionary<string, object> ReportDtls = new() { { "Details", reportdtls } };
            var stream = await printHelper.PrintReportMethodNew(design, ReportHdr!, ReportDtls);
            if (stream == null)
                return Results.BadRequest("PDF generation failed.");

            return Results.File(
                stream,
                "application/pdf", "Report.pdf");
        });
        group.MapPost("/salesreturn", async (SalesReturnReportRequest req, ISalesReportRepo repo) => await repo.GetSalesReturnReportAsync(req));
        group.MapPost("/salesreturnsrvrrpt", async (SalesReturnReportRequest req, ISalesReportRepo repo, IPrintDesignRepo prepo, SyncfusionPrintHelper printHelper) =>
        {
            var design = await prepo.GetAsync(new PrintDesignReq(0, "Sales_Return_Report"));
            if (design == null || design.hdr == null)
            {
                return Results.BadRequest("Print design 'Sales Return Report' not found.");
            }

            var result = await repo.GetSalesReturnReportAsync(req);

            var reportdtls = result.Reports;
            var reportHdr = new SalesReturnReportHdr(
               $"{req.FromDate:dd-MM-yyyy} - {req.ToDate:dd-MM-yyyy}",
               DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
               (result.TotalGross ?? 0).ToString("N2"),
               (result.TotalDiscount ?? 0).ToString("N2"),
               (result.TotalAmount ?? 0).ToString("N2"),
               (result.TotalVat ?? 0).ToString("N2"),
               (result.TotalNet ?? 0).ToString("N2")
           );

            Dictionary<string, object?> ReportHdr = reportHdr.GetType().GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(reportHdr));
            Dictionary<string, object> ReportDtls = new() { { "Details", reportdtls } };

            var stream = await printHelper.PrintReportMethodNew(design, ReportHdr!, ReportDtls);
            if (stream == null)
                return Results.BadRequest("PDF generation failed.");

            return Results.File(
                stream,
                "application/pdf", "Report.pdf");
        });//test
        group.MapPost("/salesreturndetails", async (SalesReturnDetailsReportRequest req, ISalesReportRepo repo) => await repo.GetSalesReturnDetailsAsync(req));
        group.MapPost("/salesreturndetailssrvrrpt", async (SalesReturnDetailsReportRequest req, ISalesReportRepo repo, IPrintDesignRepo prepo, SyncfusionPrintHelper printHelper) =>
        {
            var design = await prepo.GetAsync(new PrintDesignReq(0, "Sales_Return_Details_Report"));
            if (design == null || design.hdr == null)
            {
                return Results.BadRequest("Print design 'Sales Return Details Report' not found.");
            }

            var result = await repo.GetSalesReturnDetailsAsync(req);

            var reportdtls = result.Reports;
            // Calculate Taxable since it's not explicitly in Response
            var totalTaxable = (result.TotalGross ?? 0) - (result.TotalDiscount ?? 0);

            var reportHdr = new SalesReturnDetailsReportHdr(
               $"{req.FromDate:dd-MM-yyyy} - {req.ToDate:dd-MM-yyyy}",
               DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
               (result.TotalGross ?? 0).ToString("N2"),
               (result.TotalDiscount ?? 0).ToString("N2"),
               totalTaxable.ToString("N2"),
               (result.TotalTax ?? 0).ToString("N2"),
               (result.TotalAmount ?? 0).ToString("N2")
           );

            Dictionary<string, object?> ReportHdr = reportHdr.GetType().GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(reportHdr));
            Dictionary<string, object> ReportDtls = new() { { "Details", reportdtls } };

            var stream = await printHelper.PrintReportMethodNew(design, ReportHdr!, ReportDtls);
            if (stream == null)
                return Results.BadRequest("PDF generation failed.");

            return Results.File(
                stream,
                "application/pdf", "Report.pdf");
        });
        group.MapPost("/monitor", async (SalesMonitorRequest req, ISalesReportRepo repo) => await repo.GetSalesMonitorReportAsync(req));
        group.MapGet("/receipts/{invoiceId:int}", async (int invoiceId, ISalesReportRepo repo) => await repo.GetReceiptsByInvoiceAsync(invoiceId));
        group.MapPost("/salesdetails", async (SalesDetailsReportRequest req, ISalesReportRepo repo) => await repo.GetSalesDetailsAsync(req));
        group.MapPost("/customerwise", async (CustomerWiseSalesReportRequest req, ISalesReportRepo repo) => await repo.GetCustomerWiseSalesReportAsync(req));
        group.MapPost("/categorywise", async (CategoryWiseSalesReportRequest req, ISalesReportRepo repo) => await repo.GetCategoryWiseSalesReportAsync(req));
        group.MapPost("/salesquotationregister", async (SalesQuotationRegisterRequest req, ISalesReportRepo repo) => await repo.GetSalesQuotationRegisterAsync(req));
        group.MapPost("/customerquotation", async (CustomerWiseQuotationReportRequest req, ISalesReportRepo repo) => await repo.GetCustomerWiseQuotationReportAsync(req));
        group.MapPost("/deliverynoteregister", async (DeliveryNoteRegisterRequest req, ISalesReportRepo repo) => await repo.GetDeliveryNoteRegisterAsync(req));
        group.MapPost("/datewiseprofit", async (DateWiseProfitReportRequest req, ISalesReportRepo repo) => await repo.GetDateWiseProfitReportAsync(req));
        group.MapPost("/proformainvoice", async (ProformaInvoiceReportRequest req, ISalesReportRepo repo) => await repo.GetProformaInvoiceReportAsync(req));
        group.MapPost("/salesorderreport", async (SalesOrderReportRequest req, ISalesReportRepo repo) => await repo.GetSalesOrderReportAsync(req));
        group.MapPost("/enquiryregistration", async (EnquiryRegistrationReportRequest req, ISalesReportRepo repo) => await repo.GetEnquiryRegistrationReportAsync(req));
        group.MapPost("/salesdetailssrvrrpt", async (SalesDetailsReportRequest req, ISalesReportRepo repo, IPrintDesignRepo prepo, SyncfusionPrintHelper printHelper) =>
        {
            var design = await prepo.GetAsync(new PrintDesignReq(0, "SalesDetails_Report"));
            if (design == null || design.hdr == null)
            {
                design = await prepo.GetAsync(new PrintDesignReq(0, "Sales_Report"));
            }

            var result = await repo.GetSalesDetailsAsync(req);
            var reportdtls = result.Reports;
            var reportHdr = new SalesReportHdr(
               $"{req.FromDate:dd-MM-yyyy} - {req.ToDate:dd-MM-yyyy}",
               DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
               (result.TotalGross ?? 0).ToString("N2"),
               (result.TotalDiscount ?? 0).ToString("N2"),
               (result.TotalAmount ?? 0).ToString("N2"),
               (result.TotalTax ?? 0).ToString("N2"),
               (result.TotalAmount ?? 0).ToString("N2")
           );

            Dictionary<string, object?> ReportHdr = reportHdr.GetType().GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(reportHdr));
            Dictionary<string, object> ReportDtls = new() { { "Details", reportdtls } };

            var stream = await printHelper.PrintReportMethodNew(design, ReportHdr!, ReportDtls);
            if (stream == null)
                return Results.BadRequest("PDF generation failed.");

            return Results.File(
                stream,
                "application/pdf", "Report.pdf");
        });
        group.MapPost("/vatreport", async (VatReportRequest req, ISalesReportRepo repo) => await repo.GetVatReportAsync(req));
        group.MapPost("/custwisesalesrpt", async (CustomerWiseSalesReportRequest req, ISalesReportRepo repo, IPrintDesignRepo prepo, SyncfusionPrintHelper printHelper) =>
        {
            var design = await prepo.GetAsync(new PrintDesignReq(0, "Customer_Wise_Sales_Report"));
            if (design == null || design.hdr == null)
            {
                return Results.BadRequest("Print design 'Customer Wise Sales Report' not found.");
            }

            var result = await repo.GetCustomerWiseSalesReportAsync(req);

            if (result == null || !result.Items.Any())
            {
                // Create an empty list if no results, or handle as needed. 
                // Usually the report generator might handle empty lists or we return BadRequest?
                // For now proceeding as if valid, but checking Items ensures we have data structure.
                // However, following the pattern of previous endpoints (like salessrvrrpt where it just proceeds).
                // But wait, salessrvrrpt access result.Reports directly.
            }

            var reportdtls = result.Items;
            var reportHeader = new CustomerWiseSalesReportHdr(
                $"{req.FromDate:dd-MM-yyyy} - {req.ToDate:dd-MM-yyyy}",
                DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss")
            );

            Dictionary<string, object?> ReportHdr = reportHeader.GetType().GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(reportHeader));
            Dictionary<string, object> ReportDtls = new() { { "Details", reportdtls } };

            var stream = await printHelper.PrintReportMethodNew(design, ReportHdr!, ReportDtls);
            if (stream == null)
                return Results.BadRequest("PDF generation failed.");

            return Results.File(
                stream,
                "application/pdf", "Report.pdf");
        });
        group.MapPost("/custwisequotationrpt", async (CustomerWiseQuotationReportRequest req, ISalesReportRepo repo, IPrintDesignRepo prepo, SyncfusionPrintHelper printHelper) =>
        {
            var design = await prepo.GetAsync(new PrintDesignReq(0, "Customer_Wise_Quotation_Report"));

            // If design not found, fallback or throw error. Here we assume we might want to fallback or just return bad request.
            if (design == null || design.hdr == null)
            {
                return Results.BadRequest("Print design 'Customer_Wise_Quotation_Report' not found.");
            }

            var result = await repo.GetCustomerWiseQuotationReportAsync(req);

            var reportdtls = result.Items;
            var reportHeader = new CustomerWiseQuotationReportHdr(
                $"{req.FromDate:dd-MM-yyyy} - {req.ToDate:dd-MM-yyyy}",
                DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
                (result.TotalVat ?? 0).ToString("N2"),
                (result.TotalNet ?? 0).ToString("N2")
            );

            Dictionary<string, object?> ReportHdr = reportHeader.GetType().GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(reportHeader));
            Dictionary<string, object> ReportDtls = new() { { "Details", reportdtls } };

            var stream = await printHelper.PrintReportMethodNew(design, ReportHdr!, ReportDtls);
            if (stream == null)
                return Results.BadRequest("PDF generation failed.");

            return Results.File(
                stream,
                "application/pdf", "Report.pdf");
        });
        group.MapPost("/salesquotationregsrvrrpt", async (SalesQuotationRegisterRequest req, ISalesReportRepo repo, IPrintDesignRepo prepo, SyncfusionPrintHelper printHelper) =>
        {
            var design = await prepo.GetAsync(new PrintDesignReq(0, "Sales_Quotation_Register"));
            if (design == null || design.hdr == null)
            {
                return Results.BadRequest("Print design 'Sales_Quotation_Register' not found.");
            }

            var result = await repo.GetSalesQuotationRegisterAsync(req);

            var reportdtls = result.Items ?? new();
            var reportHeader = new SalesQuotationRegisterHdr(
                $"{req.FromDate:dd-MM-yyyy} - {req.ToDate:dd-MM-yyyy}",
                DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
                (result.TotalVat ?? 0).ToString("N2"),
                (result.TotalNet ?? 0).ToString("N2")
            );

            Dictionary<string, object?> ReportHdr = reportHeader.GetType().GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(reportHeader));
            Dictionary<string, object> ReportDtls = new() { { "Details", reportdtls } };

            var stream = await printHelper.PrintReportMethodNew(design, ReportHdr!, ReportDtls);
            if (stream == null)
                return Results.BadRequest("PDF generation failed.");

            return Results.File(
                stream,
                "application/pdf", "Report.pdf");
        });
        group.MapPost("/salesorderregsrvrrpt", async (SalesOrderReportRequest req, ISalesReportRepo repo, IPrintDesignRepo prepo, SyncfusionPrintHelper printHelper) =>
        {
            var design = await prepo.GetAsync(new PrintDesignReq(0, "Sales_Order_Register"));
            if (design == null || design.hdr == null)
            {
                return Results.BadRequest("Print design 'Sales_Order_Report' not found.");
            }

            var result = await repo.GetSalesOrderReportAsync(req);

            var reportdtls = result.Items ?? new();
            var reportHeader = new SalesOrderReportHdr(
                $"{req.FromDate:dd-MM-yyyy} - {req.ToDate:dd-MM-yyyy}",
                DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
                (result.TotalVat ?? 0).ToString("N2"),
                (result.TotalNet ?? 0).ToString("N2")
            );

            Dictionary<string, object?> ReportHdr = reportHeader.GetType().GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(reportHeader));
            Dictionary<string, object> ReportDtls = new() { { "Details", reportdtls } };

            var stream = await printHelper.PrintReportMethodNew(design, ReportHdr!, ReportDtls);
            if (stream == null)
                return Results.BadRequest("PDF generation failed.");

            return Results.File(
                stream,
                "application/pdf", "Report.pdf");
        });
        group.MapPost("/proformainvoicesrvrrpt", async (ProformaInvoiceReportRequest req, ISalesReportRepo repo, IPrintDesignRepo prepo, SyncfusionPrintHelper printHelper) =>
        {
            var design = await prepo.GetAsync(new PrintDesignReq(0, "Proforma_Invoice_Report"));
            if (design == null || design.hdr == null)
            {
                return Results.BadRequest("Print design 'Proforma_Invoice_Report' not found.");
            }

            var result = await repo.GetProformaInvoiceReportAsync(req);

            var reportdtls = result.Items ?? new();
            var reportHeader = new ProformaInvoiceReportHdr(
                $"{req.FromDate:dd-MM-yyyy} - {req.ToDate:dd-MM-yyyy}",
                DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
                (result.TotalDiscount ?? 0).ToString("N2"),
                (result.TotalVat ?? 0).ToString("N2"),
                (result.TotalNet ?? 0).ToString("N2")
            );

            Dictionary<string, object?> ReportHdr = reportHeader.GetType().GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(reportHeader));
            Dictionary<string, object> ReportDtls = new() { { "Details", reportdtls } };

            var stream = await printHelper.PrintReportMethodNew(design, ReportHdr!, ReportDtls);
            if (stream == null)
                return Results.BadRequest("PDF generation failed.");

            return Results.File(
                stream,
                "application/pdf", "Report.pdf");
        });
        group.MapPost("/datewiseprofitsrvrrpt", async (DateWiseProfitReportRequest req, ISalesReportRepo repo, IPrintDesignRepo prepo, SyncfusionPrintHelper printHelper) =>
        {
            var design = await prepo.GetAsync(new PrintDesignReq(0, "Date_Wise_Profit_Report"));
            if (design == null || design.hdr == null)
            {
                return Results.BadRequest("Print design 'Date_Wise_Profit_Report' not found.");
            }

            var result = await repo.GetDateWiseProfitReportAsync(req);

            var reportdtls = result.Items ?? new();
            var reportHeader = new DateWiseProfitReportHdr(
                $"{req.FromDate:dd-MM-yyyy} - {req.ToDate:dd-MM-yyyy}",
                DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
                (result.TotalSalesAmount ?? 0).ToString("N2"),
                (result.TotalSalesReturn ?? 0).ToString("N2"),
                (result.TotalNetSales ?? 0).ToString("N2"),
                (result.TotalCost ?? 0).ToString("N2"),
                (result.TotalProfit ?? 0).ToString("N2"),
                (result.TotalProfitPercentage ?? 0).ToString("N2")
            );

            Dictionary<string, object?> ReportHdr = reportHeader.GetType().GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(reportHeader));
            Dictionary<string, object> ReportDtls = new() { { "Details", reportdtls } };

            var stream = await printHelper.PrintReportMethodNew(design, ReportHdr!, ReportDtls);
            if (stream == null)
                return Results.BadRequest("PDF generation failed.");

            return Results.File(
                stream,
                "application/pdf", "Report.pdf");
        });
        group.MapPost("/deliverynoteregistersrvrrpt", async (DeliveryNoteRegisterRequest req, ISalesReportRepo repo, IPrintDesignRepo prepo, SyncfusionPrintHelper printHelper) =>
        {
            var design = await prepo.GetAsync(new PrintDesignReq(0, "Delivery_Note_Register"));
            if (design == null || design.hdr == null)
            {
                return Results.BadRequest("Print design 'Delivery_Note_Register' not found.");
            }

            var result = await repo.GetDeliveryNoteRegisterAsync(req);

            var reportdtls = result.Items ?? new();
            var reportHeader = new DeliveryNoteRegisterHdr(
                $"{req.FromDate:dd-MM-yyyy} - {req.ToDate:dd-MM-yyyy}",
                DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
                (result.TotalVat ?? 0).ToString("N2"),
                (result.TotalNet ?? 0).ToString("N2")
            );

            Dictionary<string, object?> ReportHdr = reportHeader.GetType().GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(reportHeader));
            Dictionary<string, object> ReportDtls = new() { { "Details", reportdtls } };

            var stream = await printHelper.PrintReportMethodNew(design, ReportHdr!, ReportDtls);
            if (stream == null)
                return Results.BadRequest("PDF generation failed.");

            return Results.File(
                stream,
                "application/pdf", "Report.pdf");
        });
        return group;
    }
    private static RouteGroupBuilder MapPurchaseReportEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/", async (PurchaseReportRequest req, IPurchaseReportRepo repo) => await repo.GetPurchaseReportAsync(req));
        group.MapPost("/details", async (PurchaseDetailsReportRequest req, IPurchaseReportRepo repo) => await repo.GetPurchaseDetailsAsync(req));
        group.MapPost("/purchaseorder", async (PurchaseOrderReportRequest req, IPurchaseReportRepo repo) => await repo.GetPurchaseOrderReport(req));
        group.MapPost("/monitor", async (PurchaseMonitorRequest req, IPurchaseReportRepo repo) => await repo.GetPurchaseMonitorReportAsync(req));
        group.MapGet("/payments/{purchaseId}", async (int purchaseId, IPurchaseReportRepo repo) => await repo.GetPaymentsByPurchaseAsync(purchaseId));
        group.MapPost("/purchasesummarysrvrrpt", async (PurchaseReportRequest req, IPurchaseReportRepo repo, IPrintDesignRepo prepo, SyncfusionPrintHelper printHelper) =>
        {
            var design = await prepo.GetAsync(new PrintDesignReq(0, "Purchase_Report"));
            if (design == null || design.hdr == null)
            {
                return Results.BadRequest("Print design 'Purchase Report' not found.");
            }

            var result = await repo.GetPurchaseReportAsync(req);

            var reportdtls = result.Reports ?? new();
            var reportHeader = new PurchaseReportHdr(
                $"{req.FromDate:dd-MM-yyyy} - {req.ToDate:dd-MM-yyyy}",
                DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
                (result.TotalGross ?? 0).ToString("N2"),
                (result.TotalDiscount ?? 0).ToString("N2"),
                (result.TotalVat ?? 0).ToString("N2"),
                (result.TotalNet ?? 0).ToString("N2")
            );

            Dictionary<string, object?> ReportHdr = reportHeader.GetType().GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(reportHeader));
            Dictionary<string, object> ReportDtls = new() { { "Details", reportdtls } };

            var stream = await printHelper.PrintReportMethodNew(design, ReportHdr!, ReportDtls);
            if (stream == null)
                return Results.BadRequest("PDF generation failed.");

            return Results.File(
                stream,
                "application/pdf", "Report.pdf");
        });

        group.MapPost("/purchasereturnsummarysrvrrpt", async (PurchaseReturnReportRequest req, IPurchaseReportRepo repo, IPrintDesignRepo prepo, SyncfusionPrintHelper printHelper) =>
        {
            var design = await prepo.GetAsync(new PrintDesignReq(0, "Purchase_Return_Report"));
            if (design == null || design.hdr == null)
            {
                // Fallback or error? Assuming user will create design.
                // Or maybe default to Purchase_Summary_Report design if similar?
                // Let's stick to its own design name.
                return Results.BadRequest("Print design 'Purchase Return Report' not found.");
            }

            var result = await repo.GetPurchaseReturnReportAsync(req);

            var reportdtls = result.Reports ?? new();
            var reportHeader = new PurchaseReturnReportHdr(
                $"{req.FromDate:dd-MM-yyyy} - {req.ToDate:dd-MM-yyyy}",
                DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
                (result.TotalGross ?? 0).ToString("N2"),
                (result.TotalDiscount ?? 0).ToString("N2"),
                (result.TotalVat ?? 0).ToString("N2"),
                (result.TotalNet ?? 0).ToString("N2")
            );

            Dictionary<string, object?> ReportHdr = reportHeader.GetType().GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(reportHeader));
            Dictionary<string, object> ReportDtls = new() { { "Details", reportdtls } };

            var stream = await printHelper.PrintReportMethodNew(design, ReportHdr!, ReportDtls);
            if (stream == null)
                return Results.BadRequest("PDF generation failed.");

            return Results.File(
                stream,
                "application/pdf", "Report.pdf");
        });

        group.MapPost("/purchasereturnreport", async (PurchaseReturnReportRequest req, IPurchaseReportRepo repo) => await repo.GetPurchaseReturnReportAsync(req));

        group.MapPost("/purchasereturndetails", async (PurchaseReturnDetailsReportRequest req, IPurchaseReportRepo repo) => await repo.GetPurchaseReturnDetailsAsync(req));

        group.MapPost("/purchasereturndetailssrvrrpt", async (PurchaseReturnDetailsReportRequest req, IPurchaseReportRepo repo, IPrintDesignRepo prepo, SyncfusionPrintHelper printHelper) =>
        {
            var design = await prepo.GetAsync(new PrintDesignReq(0, "Purchase_Return_Details_Report"));
            if (design == null || design.hdr == null)
            {
                return Results.BadRequest("Print design 'Purchase_Return_Details_Report' not found.");
            }

            var result = await repo.GetPurchaseReturnDetailsAsync(req);

            var reportdtls = result.Reports ?? new();
            var ReportHdr = new Dictionary<string, object?>
            {
                { "DateRange", $"{req.FromDate:dd-MM-yyyy} - {req.ToDate:dd-MM-yyyy}" },
                { "PrintDate", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss") },
                { "TotalQuantity", (result.TotalQuantity ?? 0).ToString("N2") },
                { "TotalAmount", (result.TotalAmount ?? 0).ToString("N2") }
            };
            Dictionary<string, object> ReportDtls = new() { { "Details", reportdtls } };

            var stream = await printHelper.PrintReportMethodNew(design, ReportHdr!, ReportDtls);
            if (stream == null)
                return Results.BadRequest("PDF generation failed.");

            return Results.File(
                stream,
                "application/pdf", "Report.pdf");
        });

        group.MapPost("/purchasedetailssrvrrpt", async (PurchaseDetailsReportRequest req, IPurchaseReportRepo repo, IPrintDesignRepo prepo, SyncfusionPrintHelper printHelper) =>
        {
            var design = await prepo.GetAsync(new PrintDesignReq(0, "Purchase_Details_Report"));
            if (design == null || design.hdr == null)
            {
                return Results.BadRequest("Print design 'Purchase_Details_Report' not found.");
            }

            var result = await repo.GetPurchaseDetailsAsync(req);

            var reportdtls = result.Reports ?? new();
            var ReportHdr = new Dictionary<string, object?>
            {
                { "DateRange", $"{req.FromDate:dd-MM-yyyy} - {req.ToDate:dd-MM-yyyy}" },
                { "PrintDate", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss") },
                { "TotalQuantity", (result.TotalQuantity ?? 0).ToString("N2") },
                { "TotalAmount", (result.TotalAmount ?? 0).ToString("N2") }
            };
            Dictionary<string, object> ReportDtls = new() { { "Details", reportdtls } };

            var stream = await printHelper.PrintReportMethodNew(design, ReportHdr!, ReportDtls);
            if (stream == null)
                return Results.BadRequest("PDF generation failed.");

            return Results.File(
                stream,
                "application/pdf", "Report.pdf");
        });

        group.MapPost("/purchaseorderregistersrvrrpt", async (PurchaseOrderReportRequest req, IPurchaseReportRepo repo, IPrintDesignRepo prepo, SyncfusionPrintHelper printHelper) =>
        {
            var design = await prepo.GetAsync(new PrintDesignReq(0, "Purchase_Order_Report"));
            if (design == null || design.hdr == null)
            {
                return Results.BadRequest("Print design 'Purchase_Order_Report' not found.");
            }

            var result = await repo.GetPurchaseOrderReport(req);

            var reportdtls = result.Items ?? new();
            var reportHeader = new PurchaseOrderReportHdr(
               $"{req.FromDate:dd-MM-yyyy} - {req.ToDate:dd-MM-yyyy}",
               DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
               (result.TotalVat ?? 0).ToString("N2"),
               (result.TotalNet ?? 0).ToString("N2")
           );

            Dictionary<string, object?> ReportHdr = reportHeader.GetType().GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(reportHeader));
            Dictionary<string, object> ReportDtls = new() { { "Details", reportdtls } };

            var stream = await printHelper.PrintReportMethodNew(design, ReportHdr!, ReportDtls);
            if (stream == null)
                return Results.BadRequest("PDF generation failed.");

            return Results.File(
                stream,
                "application/pdf", "Report.pdf");
        });
        return group;
    }
    #endregion

    #region MaterialTransferRequest
    private static RouteGroupBuilder MapMaterialTransferRequestEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/nextcode", async (IMaterialTransferRequestRepo repo) => await repo.GetVoucherNo(1)); // Auto-code generation
        group.MapGet("/NextNo/{branchId:int}", async (int branchId, IMaterialTransferRequestRepo repo) => await repo.GetNextVchrNo(branchId)); // Manual number suggestion
        group.MapGet("/{id:int}", async (int id, IMaterialTransferRequestRepo repo) => await repo.GetByIdAsync(id));
        group.MapPost("/", async (MaterialTransferRequestHdr hdr, IMaterialTransferRequestRepo repo) => await repo.CreateAsync(hdr));
        group.MapPut("/", async (MaterialTransferRequestHdr hdr, IMaterialTransferRequestRepo repo) => await repo.UpdateAsync(hdr));
        group.MapDelete("/{id:int}", async (int id, IMaterialTransferRequestRepo repo) => await repo.DeleteMaterialTransferRequest(id));

        group.MapPost("/get-all", async (PaginationModel pmodel, IMaterialTransferRequestRepo repo) =>
        {
            var data = await repo.GetAllAsync(pmodel);
            return data;
        });

        // Approval Endpoints
        group.MapPost("/approvallist", async (MTRApprovalFilter filter, IMaterialTransferRequestRepo repo) =>
            await repo.GetApprovalList(filter));

        group.MapPost("/approve", async (ApproveMTRRequest request, IMaterialTransferRequestRepo repo) =>
            await repo.ApproveMaterialTransferRequest(request.Id, request.UserId, request.Details));

        group.MapPost("/reject", async (RejectMTRRequest request, IMaterialTransferRequestRepo repo) =>
            await repo.RejectMaterialTransferRequest(request.Id, request.UserId, request.RejectionReason));

        return group;
    }
    #endregion

    #region MaterialTransferIn
    private static RouteGroupBuilder MapMaterialTransferInEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/nextcode", async (IMaterialTransferInRepo repo) => await repo.GetVoucherNo(1)); // Auto-code generation
        group.MapGet("/NextNo/{branchId:int}", async (int branchId, IMaterialTransferInRepo repo) => await repo.GetNextVchrNo(branchId)); // Manual number suggestion
        group.MapGet("/{id:int}", async (int id, IMaterialTransferInRepo repo) => await repo.GetByIdAsync(id));
        group.MapPost("/", async (MaterialTransferInHdr hdr, IMaterialTransferInRepo repo) => await repo.CreateAsync(hdr));
        group.MapPut("/", async (MaterialTransferInHdr hdr, IMaterialTransferInRepo repo) => await repo.UpdateAsync(hdr));
        group.MapDelete("/{id:int}", async (int id, IMaterialTransferInRepo repo) => await repo.DeleteMaterialTransferIn(id));

        group.MapPost("/get-all", async (PaginationModel pmodel, IMaterialTransferInRepo repo) =>
        {
            var data = await repo.GetAllAsync(pmodel);
            return data;
        });

        return group;
    }
    #endregion

    #region MaterialTransferOut
    private static RouteGroupBuilder MapMaterialTransferOutEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/nextcode", async (IMaterialTransferOutRepo repo) => await repo.GetVoucherNo(1)); // Auto-code generation
        group.MapGet("/NextNo/{branchId:int}", async (int branchId, IMaterialTransferOutRepo repo) => await repo.GetNextVchrNo(branchId)); // Manual number suggestion
        group.MapGet("/{id:int}", async (int id, IMaterialTransferOutRepo repo) => await repo.GetByIdAsync(id));
        group.MapPost("/", async (MaterialTransferOutHdr hdr, IMaterialTransferOutRepo repo) => await repo.CreateAsync(hdr));
        group.MapPut("/", async (MaterialTransferOutHdr hdr, IMaterialTransferOutRepo repo) => await repo.UpdateAsync(hdr));
        group.MapDelete("/{id:int}", async (int id, IMaterialTransferOutRepo repo) => await repo.DeleteMaterialTransferOut(id));

        group.MapPost("/get-all", async (PaginationModel pmodel, IMaterialTransferOutRepo repo) =>
        {
            var data = await repo.GetAllAsync(pmodel);
            return data;
        });

        return group;
    }
    #endregion

    #region InternalMaterialTransfer
    private static RouteGroupBuilder MapInternalMaterialTransferEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/nextcode", async (IInternalMaterialTransferRepo repo) => await repo.GetVoucherNo(1)); // Auto-code generation
        group.MapGet("/NextNo/{branchId:int}", async (int branchId, IInternalMaterialTransferRepo repo) => await repo.GetNextVchrNo(branchId)); // Manual number suggestion
        group.MapGet("/{id:int}", async (int id, IInternalMaterialTransferRepo repo) => await repo.GetByIdAsync(id));
        group.MapPost("/", async (InternalMaterialTransferHdr hdr, IInternalMaterialTransferRepo repo) => await repo.CreateAsync(hdr));
        group.MapPut("/", async (InternalMaterialTransferHdr hdr, IInternalMaterialTransferRepo repo) => await repo.UpdateAsync(hdr));
        group.MapDelete("/{id:int}", async (int id, IInternalMaterialTransferRepo repo) => await repo.DeleteAsync(id));

        group.MapPost("/get-all", async (PaginationModel pmodel, IInternalMaterialTransferRepo repo) =>
        {
            var data = await repo.GetAllAsync(pmodel);
            return data;
        });

        return group;
    }
    #endregion

    #region User Previleges
    private static RouteGroupBuilder MapUserPrevilegesEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (IUserPrivilegeRepo repo) => await repo.GetAllAsync());
        group.MapGet("/{id:int}", async (int id, IUserPrivilegeRepo repo) => await repo.GetAsync(id));
        group.MapGet("/web", async ([AsParameters] PrivilegeDTO dto, IUserPrivilegeRepo repo) =>
        {
            try
            {
                var result = await repo.GetWebUserPrivileges(dto.id);
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        });
        group.MapPost("/", async (UserPrivilegeDTO dto, IUserPrivilegeRepo repo) => await repo.CreateAsync(dto));
        group.MapPut("/", async (UserPrivilegeDTO dto, IUserPrivilegeRepo repo) => await repo.UpdateAsync(dto));
        group.MapDelete("/{id:int}", async (int id, IUserPrivilegeRepo repo) => await repo.DeleteAsync(id));
        group.MapPost("/pagination", async (PaginationModel pmodel, IUserPrivilegeRepo repo) => await repo.GetAllWithPagination(pmodel));
        return group;
    }
    #endregion

    #region Workshop
    private static RouteGroupBuilder MapJobOrderEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/nextcode/{branchId:int}", async (int branchId, [FromServices] IJobOrderRepo repo) => await repo.GetVoucherNo(branchId));
        group.MapGet("/NextNo/{branchId:int}", async (int branchId, [FromServices] IJobOrderRepo repo) => await repo.GetNextVchrNo(branchId));
        group.MapGet("/{id:int}", async (int id, [FromServices] IJobOrderRepo repo) => await repo.GetByIdAsync(id));
        group.MapPost("/", async (JobOrderHdr hdr, [FromServices] IJobOrderRepo repo) => await repo.CreateAsync(hdr));
        group.MapPut("/", async (JobOrderHdr hdr, [FromServices] IJobOrderRepo repo) => await repo.UpdateAsync(hdr));
        group.MapDelete("/{id:int}", async (int id, [FromServices] IJobOrderRepo repo) => await repo.DeleteAsync(id));

        group.MapPost("/get-all", async (PaginationModel pmodel, [FromServices] IJobOrderRepo repo) =>
        {
            var data = await repo.GetAllWithPagination(pmodel);
            return data;
        });

        return group;
    }
    private static RouteGroupBuilder MapJobCardEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/nextcode/{branchId:int}", async (int branchId, [FromServices] IJobCardRepo repo) => await repo.GetVoucherNo(branchId));
        group.MapGet("/NextNo/{branchId:int}", async (int branchId, [FromServices] IJobCardRepo repo) => await repo.GetNextVchrNo(branchId));
        group.MapGet("/{id:int}", async (int id, [FromServices] IJobCardRepo repo) => await repo.GetByIdAsync(id));
        group.MapPost("/", async (JobCardHdr hdr, [FromServices] IJobCardRepo repo) => await repo.CreateAsync(hdr));
        group.MapPut("/", async (JobCardHdr hdr, [FromServices] IJobCardRepo repo) => await repo.UpdateAsync(hdr));
        group.MapDelete("/{id:int}", async (int id, [FromServices] IJobCardRepo repo) => await repo.DeleteAsync(id));

        group.MapPost("/get-all", async (PaginationModel pmodel, [FromServices] IJobCardRepo repo) =>
        {
            var data = await repo.GetAllWithPagination(pmodel);
            return data;
        });

        group.MapGet("/history/{plateNo}", async (string plateNo, [FromServices] IJobCardRepo repo) => await repo.GetJobCardHistory(plateNo));

        return group;
    }
    #endregion

    #region Accounts
    private static RouteGroupBuilder MapAccountReportEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/journalentry", async (JournalEntryReportRequest entries, IAccountReportRepo repo) =>
        {
            return await repo.GetJournalEntryReport(entries);
        });

        group.MapPost("/trialbalancereport", async (TrialBalanceReportRequest req, IAccountReportRepo repo) =>
        {
            return await repo.GetTrialBalanceReport(req);
        });

        group.MapPost("/expensereport", async (ExpenseReportRequest req, IAccountReportRepo repo) =>
        {
            return await repo.GetExpenseReport(req);
        });

        group.MapPost("/profitandlossreport", async (ProfitAndLossReportRequest req, IAccountReportRepo repo) =>
        {
            return await repo.GetProfitAndLossReport(req);
        });
        group.MapPost("/balancesheetreport", async (BalanceSheetReportRequest req, IAccountReportRepo repo) =>
        {
            return await repo.GetBalanceSheetReport(req);
        });
        group.MapPost("/chartofaccount", async (ChartOfAccountReportRequest req, IAccountReportRepo repo) =>
        {
            return await repo.GetChartOfAccountReport(req);
        });
        group.MapPost("/ledgerreport", async (LedgerReportRequest req, IAccountReportRepo repo) =>
        {
            return await repo.GetLedgerReport(req);
        });
        return group;
    }
    #endregion


    #region Shift Closing
    private static RouteGroupBuilder MapShiftEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/close", async (ShiftClosingRecord record, IShiftClosingRepo repo) =>
        {
            return await repo.SaveShiftClosingAsync(record);
        });

        group.MapPost("/report", async (ShiftClosingReportRequest req, IShiftClosingRepo repo) =>
        {
            return await repo.GetShiftClosingReportAsync(req);
        });

        group.MapGet("/{id:int}", async (int id, IShiftClosingRepo repo) =>
        {
            var record = await repo.GetShiftClosingByIdAsync(id);
            return record is not null ? Results.Ok(record) : Results.NotFound();
        });

        return group;
    }
    #endregion
}