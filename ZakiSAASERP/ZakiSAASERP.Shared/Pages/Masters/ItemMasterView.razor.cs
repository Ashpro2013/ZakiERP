using ZakiSAASERP.Shared.Helpers;
using ZakiSAASERP.Shared.Services;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using System.IO;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ZakiSAASERP.Shared.Pages.Masters;

public partial class ItemMasterView : ITabAware
{
    [Inject] public ISnackbar Snackbar { get; set; }
    [Inject] public IJSRuntime JSRuntime { get; set; }
    [Parameter] public bool IsDialog { get; set; } = false;
    [Parameter] public int TabId { get; set; }

    #region Variables
    private ItemMaster item { get; set; } = new();
    private EditForm? form;
    private string? itemImagePreview;
    private string searchText = "";
    MudDataGrid<ItemMaster>? grid;
    private List<CategoryMaster> categoryList = new();
    private List<SubCategoryMaster> subCategoryList = new();
    private List<UnitMaster> unitList = new();
    private List<BrandMaster> brandList = new();
    private List<VatMaster> vatList = new();
    private decimal selectedVatPercentage = 0;

    // Selected Names for Display
    private string? unitName;
    private string? brandName;
    private string? categoryName;
    private string? subCategoryName;

    private List<string> stockModes = new() { "Inventory", "Non Inventory" };

    // Filtered SubCategories based on selected category
    private IEnumerable<SubCategoryMaster> FilteredSubCategories
    {
        get
        {
            if (item.FK_CategoryId.HasValue && item.FK_CategoryId.Value > 0)
            {
                return subCategoryList.Where(sc => sc.FK_CategoryId == item.FK_CategoryId || sc.SubCategoryId == item.FK_SubCategoryId);
            }
            return subCategoryList;
        }
    }

    // Stock Info (readonly display)
    private decimal openingStock = 0;
    private decimal currentStock = 0;
    private List<StoreStockDTO> storeWiseStock = new();

    private int activeTabIndex = 0;

    // Select Refs
    private MudSelect<int?>? ddlSubCategoryRef;
    private MudSelect<int?>? ddlBrandRef;
    private MudSelect<int?>? ddlUnitRef;
    private MudSelect<int?>? ddlVatRef;
    private MudSelect<string>? ddlVatCategoryRef;
    private MudSelect<string>? ddlStockModeRef;

    // Default Data Cache
    private CategoryMaster? defaultCategory;
    private SubCategoryMaster? defaultSubCategory;
    private UnitMaster? defaultUnit;

    /// <summary>
    /// Whether auto item code generation is enabled (from PermanentSettingsData)
    /// </summary>
    private bool IsAutoCode => PermanentSettingsData.AutoItemCode;
    private bool IsCategoryWise => PermanentSettingsData.CategoryWiseItemCode;
    private bool IsSubCategoryWise => PermanentSettingsData.SubCategoryWiseItemCode;
    private WebUserPrivilege? privilege;
    #endregion

    protected override async Task OnInitializedAsync()
    {
        if (AppData.privileges is not null)
            privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "Item Master" || x.Caption == "ItemMaster");
        await LoadDropdowns();
        // Ensure new item state has one empty row
        if (item.ItemId == 0) await NewClick();
    }


    private async Task LoadDropdowns()
    {
        try
        {
            // Execute parallel fetches for performance

            brandList = await api.GetAllBrand();
            vatList = await api.GetAllVat();

            categoryList = await api.GetAllCategory();
            defaultCategory = categoryList.FirstOrDefault(c => c.Name.Equals("GENERAL", StringComparison.OrdinalIgnoreCase));

            subCategoryList = await api.GetAllSubCategory();
            defaultSubCategory = subCategoryList.FirstOrDefault(c => c.Name.Equals("GENERAL", StringComparison.OrdinalIgnoreCase));

            unitList = await api.GetAllUnits();
            defaultUnit = unitList.FirstOrDefault(u => u.Name.Equals("EACH", StringComparison.OrdinalIgnoreCase));

            // Default VAT Logic
            var defaultVat = vatList.FirstOrDefault(v => v.IsDefault) ?? vatList.FirstOrDefault();
            if (defaultVat is not null)
            {
                OnVatChanged(defaultVat.VatId);
            }
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Snackbar.Add(ex.Message, Severity.Error);
        }
    }

    #region Methods
    private async Task<GridData<ItemMaster>> LoadServerData(GridState<ItemMaster> state)
    {
        var pagination = new PaginationModel(
             page: state.Page + 1,
            pageSize: state.PageSize,
            sortColumn: state.SortDefinitions?.FirstOrDefault()?.SortBy,
            sortDirection: state.SortDefinitions?.FirstOrDefault()?.Descending == true ? "desc" : "asc",
            search: searchText
        );
        var result = await api.GetItemPDetails(pagination);
        return new GridData<ItemMaster>
        {
            Items = result.PItems,
            TotalItems = result.TotalCount
        };
    }
    private async Task OnAdornmentClick()
    {
        searchText = "";
        StateHasChanged();
        if (grid is not null)
        {
            await grid.ReloadServerData();
        }
    }
    private async Task OnSearchKeyDown(KeyboardEventArgs args)
    {
        if (grid is not null)
        {
            await grid.ReloadServerData();
        }
    }
    private async Task OnSearchChanged(string searchValue)
    {
        if (grid is not null)
        {
            searchText = searchValue;
            await grid.ReloadServerData();
        }
    }
    
    private bool _isSaving = false;

    async Task SaveOrUpdate()
    {
        if (_isSaving) return;

        if (form?.EditContext != null && !form.EditContext.Validate())
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(item.ItemName))
        {
            Snackbar.Add(Localizer("EnterNameFirst"), Severity.Warning);
            return;
        }

        // Category Validation (required if CategoryWise is enabled)
        if (IsCategoryWise && (item.FK_CategoryId == null || item.FK_CategoryId == 0))
        {
            Snackbar.Add(Localizer("SelectCategoryFirst"), Severity.Warning);
            return;
        }

        // SubCategory Validation (required if SubCategoryWise is enabled)
        if (IsSubCategoryWise && (item.FK_SubCategoryId == null || item.FK_SubCategoryId == 0))
        {
            Snackbar.Add(Localizer("SelectSubCategoryFirst"), Severity.Warning);
            return;
        }

        // VAT Validation
        if (selectedVatPercentage == 0)
        {
            if (item.VatCategory != "Z")
            {
                Snackbar.Add(Localizer("VatZeroRequiresZCategory"), Severity.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(item.VatExcCode))
            {
                Snackbar.Add(Localizer("VatExemptionCodeRequired"), Severity.Warning);
                return;
            }
        }

        // --- END VALIDATIONS ---

        // Clean up empty rows before processing
        // Remove rows where UnitId is 0 or null (invalid/empty rows)
        if (item.MultiUnits is not null)
        {
            item.MultiUnits = item.MultiUnits.Where(x => x.FK_UnitId > 0).ToList();

            var muBarcodes = item.MultiUnits
                .Where(x => !string.IsNullOrWhiteSpace(x.Barcode))
                .Select(x => x.Barcode!.Trim())
                .ToList();

            if (muBarcodes.Count != muBarcodes.Distinct(StringComparer.OrdinalIgnoreCase).Count())
            {
                Snackbar.Add(Localizer("DuplicateBarcodeInUnits"), Severity.Warning);
                return;
            }
        }

        // Store navigation properties to restore after save (for display purposes)
        var tempUnit = item.Unit;
        var tempCategory = item.Category;
        var tempSubCategory = item.SubCategory;
        var tempBrand = item.Brand;
        // Backup multi-unit navigation properties
        var tempMultiUnitUnits = item.MultiUnits?.Select(m => new { m, m.Unit }).ToList();

        _isSaving = true;
        StateHasChanged();

        try
        {
            // Null out navigation properties to prevent EF errors
            item.Unit = null;
            item.Category = null;
            item.SubCategory = null;
            item.Brand = null;

            if (item.MultiUnits is not null)
            {
                foreach (var mu in item.MultiUnits)
                {
                    mu.Unit = null;
                    mu.ItemMaster = null;
                }
            }

            if (string.IsNullOrWhiteSpace(item.Barcode))
            {
                if (PermanentSettingsData.AutoBarcodeGeneration)
                {
                    item.Barcode = item.ItemCode;
                }
                else
                {
                    item.Barcode = null;
                }
            }
            else
            {
                item.Barcode = item.Barcode.Trim();
            }

            if (PermanentSettingsData.RequireBarcode && string.IsNullOrWhiteSpace(item.Barcode))
            {
                Snackbar.Add(Localizer("BarcodeIsRequired"), Severity.Warning);
                RestoreNavProps(tempUnit, tempCategory, tempSubCategory, tempBrand, tempMultiUnitUnits);
                return;
            }

            if (item.MultiUnits == null || !item.MultiUnits.Any())
            {
                item.MultiUnits = new List<ItemMultiUnit>();

                if (item.FK_UnitId.HasValue)
                {
                    var mainUnitName = !string.IsNullOrWhiteSpace(unitName)
                        ? unitName
                        : tempUnit?.Name;

                    var defaultUnit = new ItemMultiUnit
                    {
                        FK_UnitId = item.FK_UnitId.Value,
                        MultiUnitName = $"{item.ItemName}-{mainUnitName}",
                        QTYPerUnit = 1,
                        Rate = item.SalesRate,
                        IncRate = item.InclusiveRate,
                        Cost = item.StandardCost,
                        Barcode = item.Barcode
                    };

                    item.MultiUnits.Add(defaultUnit);
                }
                else
                {
                    Snackbar.Add(Localizer("SelectUnitFirst"), Severity.Warning);
                    RestoreNavProps(tempUnit, tempCategory, tempSubCategory, tempBrand, tempMultiUnitUnits);
                    return;
                }
            }
            else
            {
                var mainUnitName = !string.IsNullOrWhiteSpace(unitName)
                    ? unitName
                    : tempUnit?.Name;
                var unit = item.MultiUnits.FirstOrDefault(mu => (item.FK_UnitId.HasValue && mu.FK_UnitId == item.FK_UnitId) || mu.QTYPerUnit == 1) ?? item.MultiUnits.FirstOrDefault();
                if (unit != null)
                {
                    if (item.FK_UnitId.HasValue) unit.FK_UnitId = item.FK_UnitId.Value;
                    unit.MultiUnitName = $"{item.ItemName}-{mainUnitName}";
                    unit.QTYPerUnit = 1;
                    unit.Rate = item.SalesRate;
                    unit.IncRate = item.InclusiveRate;
                    unit.Cost = item.StandardCost;
                    if (string.IsNullOrWhiteSpace(unit.Barcode)) unit.Barcode = item.Barcode;
                }
            }

            // Ensure mutual calculation if one rate was entered but not the other
            if (lastEditedRate == "inclusive" && (item.SalesRate == null || item.SalesRate == 0) && item.InclusiveRate.HasValue && item.InclusiveRate > 0)
            {
                CalculateItemRateFromInclusiveRate();
            }
            else if ((item.InclusiveRate == null || item.InclusiveRate == 0) && item.SalesRate.HasValue && item.SalesRate > 0)
            {
                CalculateItemInclusiveRateFromRate();
            }
            else if ((item.SalesRate == null || item.SalesRate == 0) && item.InclusiveRate.HasValue && item.InclusiveRate > 0)
            {
                CalculateItemRateFromInclusiveRate();
            }
            SyncDefaultMultiUnitRate();

            if (item.SalesRate.HasValue) item.SalesRate = Math.Round(item.SalesRate.Value, PermanentSettingsData.CurrencyDecimalPlaces, MidpointRounding.AwayFromZero);
            if (item.InclusiveRate.HasValue) item.InclusiveRate = Math.Round(item.InclusiveRate.Value, PermanentSettingsData.CurrencyDecimalPlaces, MidpointRounding.AwayFromZero);
            if (item.PurchaseRate.HasValue) item.PurchaseRate = Math.Round(item.PurchaseRate.Value, PermanentSettingsData.CurrencyDecimalPlaces, MidpointRounding.AwayFromZero);
            if (item.WholeSaleRate.HasValue) item.WholeSaleRate = Math.Round(item.WholeSaleRate.Value, PermanentSettingsData.CurrencyDecimalPlaces, MidpointRounding.AwayFromZero);
            if (item.MinSaleRate.HasValue) item.MinSaleRate = Math.Round(item.MinSaleRate.Value, PermanentSettingsData.CurrencyDecimalPlaces, MidpointRounding.AwayFromZero);
            if (item.StandardCost.HasValue) item.StandardCost = Math.Round(item.StandardCost.Value, PermanentSettingsData.CurrencyDecimalPlaces, MidpointRounding.AwayFromZero);
            if (item.LandingCost.HasValue) item.LandingCost = Math.Round(item.LandingCost.Value, PermanentSettingsData.CurrencyDecimalPlaces, MidpointRounding.AwayFromZero);

            if (item.MultiUnits != null)
            {
                foreach (var mu in item.MultiUnits)
                {
                    if (mu.Rate.HasValue) mu.Rate = Math.Round(mu.Rate.Value, PermanentSettingsData.CurrencyDecimalPlaces, MidpointRounding.AwayFromZero);
                    if (mu.IncRate.HasValue) mu.IncRate = Math.Round(mu.IncRate.Value, PermanentSettingsData.CurrencyDecimalPlaces, MidpointRounding.AwayFromZero);
                    if (mu.Cost.HasValue) mu.Cost = Math.Round(mu.Cost.Value, PermanentSettingsData.CurrencyDecimalPlaces, MidpointRounding.AwayFromZero);
                    if (mu.QTYPerUnit.HasValue) mu.QTYPerUnit = Math.Round(mu.QTYPerUnit.Value, PermanentSettingsData.QuantityDecimalPlaces, MidpointRounding.AwayFromZero);
                }
            }

            if (item.ItemId == 0)
            {
                if (privilege is not null && !privilege.SaveData) { Dialog.DialogMethod(Localizer("NoSavePrivilege")); RestoreNavProps(tempUnit, tempCategory, tempSubCategory, tempBrand, tempMultiUnitUnits); return; }
                var result = await api.CreateItem(item);
                if ((result.Message is not null && result.Message.Contains("Saved Successfully")) || (result.Id > 0 && result.Message == null))
                {
                    if (IsDialog)
                    {
                        AppData.PassingValue = new CommonModel { Id = result.Id, Name = item.ItemName };
                    }
                    Snackbar.Add(Localizer("SaveMessage"), Severity.Success);
                    await NewClick();
                    if (IsAutoCode)
                    {
                        await Task.Delay(50);
                        await FocusElement("txtName");
                    }
                    else
                    {
                        await Task.Delay(50);
                        await FocusElement("txtCode");
                    }
                }
                else
                {
                    RestoreNavProps(tempUnit, tempCategory, tempSubCategory, tempBrand, tempMultiUnitUnits);
                    Snackbar.Add(Localizer(result.Message ?? "Failed"), Severity.Error);
                }
            }
            else
            {
                if (privilege is not null && !privilege.EditData) { Dialog.DialogMethod(Localizer("NoEditPrivilege")); RestoreNavProps(tempUnit, tempCategory, tempSubCategory, tempBrand, tempMultiUnitUnits); return; }
                var result = await api.UpdateItem(item);
                if ((result.Message is not null && result.Message.Contains("Updated Successfully")) || (result.Id > 0 && result.Message == null))
                {
                    if (IsDialog)
                    {
                        AppData.PassingValue = new CommonModel { Id = result.Id, Name = item.ItemName };
                    }
                    Snackbar.Add(Localizer("UpdateMessage"), Severity.Success);
                    await NewClick();
                    if (IsAutoCode)
                    {
                        await Task.Delay(50);
                        await FocusElement("txtName");
                    }
                    else
                    {
                        await Task.Delay(50);
                        await FocusElement("txtCode");
                    }
                }
                else
                {
                    RestoreNavProps(tempUnit, tempCategory, tempSubCategory, tempBrand, tempMultiUnitUnits);
                    Snackbar.Add(Localizer(result.Message ?? "Failed"), Severity.Error);
                }
            }
        }
        catch (Exception ex)
        {
            RestoreNavProps(tempUnit, tempCategory, tempSubCategory, tempBrand, tempMultiUnitUnits);
            Snackbar.Add(ex.Message, Severity.Error);
        }
        finally
        {
            _isSaving = false;
            StateHasChanged();
        }
    }
    private void OnBlurQtEvent(ItemMultiUnit unitContext)
    {
        var unit = item.MultiUnits.FirstOrDefault(mu => mu.FK_UnitId == unitContext.FK_UnitId) ?? unitContext;
        unit.Rate = Math.Round((item.SalesRate ?? 0) * (unitContext.QTYPerUnit ?? 0), PermanentSettingsData.CurrencyDecimalPlaces, MidpointRounding.AwayFromZero);
        unit.IncRate = Math.Round((item.InclusiveRate ?? 0) * (unitContext.QTYPerUnit ?? 0), PermanentSettingsData.CurrencyDecimalPlaces, MidpointRounding.AwayFromZero);
        unit.Cost = Math.Round((item.StandardCost ?? 0) * (unitContext.QTYPerUnit ?? 0), PermanentSettingsData.CurrencyDecimalPlaces, MidpointRounding.AwayFromZero);
        StateHasChanged();
    }
    private void RestoreNavProps(UnitMaster? tUnit, CategoryMaster? tCat, SubCategoryMaster? tSub, BrandMaster? tBrand, dynamic? tMultiUnits)
    {
        item.Unit = tUnit;
        item.Category = tCat;
        item.SubCategory = tSub;
        item.Brand = tBrand;
        if (item.MultiUnits is not null && tMultiUnits is not null)
        {
            if (tMultiUnits is IEnumerable<dynamic> list)
            {
                foreach (var entry in list)
                {
                    var m = entry.m as ItemMultiUnit;
                    var u = entry.Unit as UnitMaster;
                    if (m is not null) m.Unit = u;
                }
            }
        }
    }

    async Task Delete()
    {
        try
        {
            if (privilege is not null && !privilege.DeleteData) { Dialog.DialogMethod(Localizer("NoDeletePrivilege")); return; }
            if (!await Dialog.DeleteDialogMethod())
                return;
            var result = await api.DeleteItem(item.ItemId);
            if ((result.Message is not null && result.Message.Contains("Deleted Successfully")) || (result.Id > 0 && result.Message == null))
            {
                Snackbar.Add(Localizer("DeleteMessage"), Severity.Success);
                await NewClick();
            }
            else
            {
                if ((result.Message ?? "").Contains("FK_", StringComparison.OrdinalIgnoreCase) || (result.Message ?? "").Contains("REFERENCE", StringComparison.OrdinalIgnoreCase) || (result.Message ?? "").Contains("Constraint", StringComparison.OrdinalIgnoreCase))
                    Dialog.DialogMethod(ArabicLocalization.GetFriendlyForeignKeyError(result.Message));
                else
                    Dialog.DialogMethod(result.Message ?? "Failed");
            }
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("FK_", StringComparison.OrdinalIgnoreCase) || ex.Message.Contains("REFERENCE", StringComparison.OrdinalIgnoreCase) || ex.Message.Contains("Constraint", StringComparison.OrdinalIgnoreCase))
                Dialog.DialogMethod(ArabicLocalization.GetFriendlyForeignKeyError(ex.Message));
            else
                Dialog.DialogMethod(ex.Message);
        }
    }
    async Task PrintBarcode()
    {
        try
        {
            var dialog = await Dialog.InputDialogMethod("Barcode Count", "Enter Barcode Count");
            List<BarcodeData> barcodeDataList = new();
            var data = item.CopyTo<BarcodeData>();
            data.Quantity = int.Parse(dialog ?? "1");
            barcodeDataList.Add(data);
            var dto = await api.GetBarcodeDesignAsync();
           
            if (dto == null)
            {
                Snackbar.Add(Localizer("No Design Found"), Severity.Warning);
                return;
            }
            
            if (AppData.IsWindows)
            {
                var hdr = dto.hdr;
                hdr.PrinterName = PermanentSettingsData.BarcodePrinterName;
                var dtls = dto.dtls;
                foreach (var item in dtls)
                {
                    if(string.IsNullOrEmpty(item.Prefix))
                        item.Prefix = "";
                }
                await formFactor.PrintBarcode(new BarcodeDesignDTO(hdr,dtls), barcodeDataList);
            }
            else
            {
                await CrudHelper.PrintBarcodeMethod(dto, barcodeDataList, JSRuntime, Dialog);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add(ex.Message, Severity.Error);
        }
    }
    async Task NewClick()
    {
        try
        {
            item = new ItemMaster();
            item.IsActive = true;
            item.IsSaleable = true;
            item.StockMode = "Inventory";
            item.VatCategory = "S";
            item.ItemType = "Normal";
            item.MultiUnits = new List<ItemMultiUnit>();
            // Add Empty Row for Input
            item.MultiUnits.Add(new ItemMultiUnit());

            itemImagePreview = null;

            // Clear Names
            unitName = null;
            brandName = null;
            unitName = null;
            brandName = null;
            categoryName = null;
            subCategoryName = null;

            // Apply Defaults
            if (defaultCategory is not null)
            {
                item.FK_CategoryId = defaultCategory.CategoryId;
                categoryName = defaultCategory.Name;
                item.Category = defaultCategory;
            }
            if (defaultSubCategory is not null)
            {
                item.FK_SubCategoryId = defaultSubCategory.SubCategoryId;
                subCategoryName = defaultSubCategory.Name;
                item.SubCategory = defaultSubCategory;
            }
            if (defaultUnit is not null)
            {
                item.FK_UnitId = defaultUnit.UnitId;
                unitName = defaultUnit.Name;
                item.Unit = defaultUnit;
            }

            selectedVatPercentage = 0;
            lastEditedRate = "exclusive";

            // Reset stock info
            openingStock = 0;
            currentStock = 0;
            storeWiseStock = new();

            activeTabIndex = 0; // Reset tab

            if (grid is not null) await grid.ReloadServerData();

            // Re-apply default VAT
            var defaultVat = vatList.FirstOrDefault(v => v.IsDefault) ?? vatList.FirstOrDefault();
            if (defaultVat is not null)
            {
                OnVatChanged(defaultVat.VatId);
            }

            // Load auto-code based on defaults already applied
            // Defaults satisfy category/subcategory requirements
            if (IsAutoCode)
            {
                await LoadAutoCodeIfEnabled();
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add(ex.Message, Severity.Error);
        }
    }

    async Task RowClicked(DataGridRowClickEventArgs<ItemMaster> args)
    {
        // Handle double click only
        try
        {
            var fullData = await api.GetItemById(args.Item.ItemId);
            await PopulateItemAsync(fullData);
        }
        catch (Exception ex)
        {
            Snackbar.Add(ex.Message, Severity.Error);
        }
    }

    private async Task PopulateItemAsync(ItemMaster fullData)
    {
        if (fullData == null) return;
        item = fullData;

        if (item.MultiUnits == null) item.MultiUnits = new List<ItemMultiUnit>();

        if (item.SalesRate.HasValue) item.SalesRate = Math.Round(item.SalesRate.Value, PermanentSettingsData.CurrencyDecimalPlaces, MidpointRounding.AwayFromZero);
        if (item.InclusiveRate.HasValue) item.InclusiveRate = Math.Round(item.InclusiveRate.Value, PermanentSettingsData.CurrencyDecimalPlaces, MidpointRounding.AwayFromZero);
        if (item.PurchaseRate.HasValue) item.PurchaseRate = Math.Round(item.PurchaseRate.Value, PermanentSettingsData.CurrencyDecimalPlaces, MidpointRounding.AwayFromZero);
        if (item.WholeSaleRate.HasValue) item.WholeSaleRate = Math.Round(item.WholeSaleRate.Value, PermanentSettingsData.CurrencyDecimalPlaces, MidpointRounding.AwayFromZero);
        if (item.MinSaleRate.HasValue) item.MinSaleRate = Math.Round(item.MinSaleRate.Value, PermanentSettingsData.CurrencyDecimalPlaces, MidpointRounding.AwayFromZero);
        if (item.StandardCost.HasValue) item.StandardCost = Math.Round(item.StandardCost.Value, PermanentSettingsData.CurrencyDecimalPlaces, MidpointRounding.AwayFromZero);
        if (item.LandingCost.HasValue) item.LandingCost = Math.Round(item.LandingCost.Value, PermanentSettingsData.CurrencyDecimalPlaces, MidpointRounding.AwayFromZero);

        foreach (var mu in item.MultiUnits)
        {
            if (mu.Rate.HasValue) mu.Rate = Math.Round(mu.Rate.Value, PermanentSettingsData.CurrencyDecimalPlaces, MidpointRounding.AwayFromZero);
            if (mu.IncRate.HasValue) mu.IncRate = Math.Round(mu.IncRate.Value, PermanentSettingsData.CurrencyDecimalPlaces, MidpointRounding.AwayFromZero);
            if (mu.Cost.HasValue) mu.Cost = Math.Round(mu.Cost.Value, PermanentSettingsData.CurrencyDecimalPlaces, MidpointRounding.AwayFromZero);
            if (mu.QTYPerUnit.HasValue) mu.QTYPerUnit = Math.Round(mu.QTYPerUnit.Value, PermanentSettingsData.QuantityDecimalPlaces, MidpointRounding.AwayFromZero);
        }

        // Set Names from Navigation Properties
        unitName = item.Unit?.Name;
        if (unitName == null && item.FK_UnitId.HasValue)
        {
            var matchedUnit = unitList.FirstOrDefault(x => x.UnitId == item.FK_UnitId);
            unitName = matchedUnit?.Name;
            item.Unit = matchedUnit;
        }

        brandName = item.Brand?.Name;
        if (brandName == null && item.FK_BrandId.HasValue)
        {
            var matchedBrand = brandList.FirstOrDefault(x => x.BrandId == item.FK_BrandId);
            brandName = matchedBrand?.Name;
            item.Brand = matchedBrand;
        }

        categoryName = item.Category?.Name;
        if (categoryName == null && item.FK_CategoryId.HasValue)
        {
            var matchedCategory = categoryList.FirstOrDefault(x => x.CategoryId == item.FK_CategoryId);
            categoryName = matchedCategory?.Name;
            item.Category = matchedCategory;
        }

        subCategoryName = item.SubCategory?.Name;
        if (subCategoryName == null && item.FK_SubCategoryId.HasValue)
        {
            var matchedSub = subCategoryList.FirstOrDefault(x => x.SubCategoryId == item.FK_SubCategoryId);
            subCategoryName = matchedSub?.Name;
            item.SubCategory = matchedSub;
        }

        lastEditedRate = "exclusive";
        SetVat(item.FK_VatId, recalculate: false); // Set percentage without overwriting loaded inclusive rate
        OnVatCategoryChanged(item.VatCategory ?? "S");

        itemImagePreview = fullData.ItemImage is not null
            ? "data:image/png;base64," + Convert.ToBase64String(fullData.ItemImage)
            : null;

        // Load stock info
        try
        {
            var stockInfo = await api.GetItemStockInfo(item.ItemId);
            openingStock = stockInfo.OpeningStock;
            currentStock = stockInfo.CurrentStock;
            storeWiseStock = stockInfo.StoreWiseStock ?? new();
        }
        catch { openingStock = 0; currentStock = 0; storeWiseStock = new(); }

        activeTabIndex = 0;
        StateHasChanged();
    }

    private async Task UploadItemImage(IBrowserFile file)
    {
        try
        {
            // Validate file first
            var bytes = await FileHelper.ValidateAndConvertAsync(file);
            if (bytes == null)
            {
                await Dialog.DialogMethod(Localizer("MaxFileSize1MB"));
                return;
            }
            var extension = Path.GetExtension(file.Name)?.ToLower();
            if (extension is not ".png" and not ".jpg" and not ".jpeg")
            {
                await Dialog.DialogMethod(Localizer("InvalidImageFormat"));
                return;
            }
            item.ItemImage = bytes;
            item.ItemImageName = file.Name;
            var base64 = Convert.ToBase64String(bytes);
            itemImagePreview = $"data:image/{extension.TrimStart('.')};base64,{base64}";
        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod(ex.Message);
        }
    }

    async Task RemoveItemImage()
    {
        try
        {
            itemImagePreview = null;
            item.ItemImage = null;
            item.ItemImageName = null;
        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod(ex.Message);
        }
    }

    private async Task OpenStoreStockDialog()
    {
        if (storeWiseStock == null || !storeWiseStock.Any())
        {
            Snackbar.Add("No store-wise stock data available", Severity.Info);
            return;
        }
        var parameters = new DialogParameters { ["StoreStocks"] = storeWiseStock };
        await DialogService.ShowAsync<StoreStockDialog>(Localizer("StoreWiseStock"), parameters, new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true });
    }

    private async Task ToArabic()
    {
        if (string.IsNullOrWhiteSpace(item.ItemName))
        {
            Snackbar.Add(Localizer("EnterNameFirst"), Severity.Warning);
            return;
        }
        item.ItemNameArabic = await api.GetArabicText(item.ItemName);
    }

    private void AddNewUnitRow()
    {
        if (item.MultiUnits == null) item.MultiUnits = new List<ItemMultiUnit>();
        item.MultiUnits.Add(new ItemMultiUnit
        {
            QTYPerUnit = 0,
            Rate = 0
        });
        StateHasChanged();
    }

    void RemoveMultiUnit(ItemMultiUnit unit)
    {
        item.MultiUnits.Remove(unit);
    }

    private bool _isSearchDialogOpen = false;

    private async Task OpenMultiUnitSearchRow(ItemMultiUnit unitContext)
    {
        if (_isSearchDialogOpen) return;
        _isSearchDialogOpen = true;
        try
        {
            var dialog = DialogService.Show<UnitSearchDialog>(Localizer("SearchUnit"));
            var result = await dialog.Result;
            if (!result.Canceled && result.Data is UnitMaster selected)
            {
                unitContext.FK_UnitId = selected.UnitId;
                unitContext.Unit = selected;
                unitContext.MultiUnitName = $"{item.ItemName}-{selected.Name}";

                StateHasChanged();
                await Task.Delay(100);

                // Explicitly focus name field
                int rowIndex = item.MultiUnits.IndexOf(unitContext);
                if (rowIndex >= 0) await FocusElement($"input-row-{rowIndex}-arabic");
            }
        }
        finally
        {
            await Task.Delay(200);
            _isSearchDialogOpen = false;
        }
    }

    private void CalculateInclusiveRateFromRateRow(ItemMultiUnit unit)
    {
        if (unit.Rate.HasValue)
        {
            unit.Rate = Math.Round(unit.Rate.Value, PermanentSettingsData.CurrencyDecimalPlaces, MidpointRounding.AwayFromZero);
            var vatPerc = GetEffectiveVatPercentage();
            if (vatPerc == 0)
                unit.IncRate = unit.Rate;
            else
                unit.IncRate = Math.Round(unit.Rate.Value * (1 + vatPerc / 100), PermanentSettingsData.CurrencyDecimalPlaces, MidpointRounding.AwayFromZero);
        }
        else
        {
            unit.IncRate = null;
        }
        StateHasChanged();
    }

    private void CalculateRateFromInclusiveRateRow(ItemMultiUnit unit)
    {
        if (unit.IncRate.HasValue)
        {
            unit.IncRate = Math.Round(unit.IncRate.Value, PermanentSettingsData.CurrencyDecimalPlaces, MidpointRounding.AwayFromZero);
            var vatPerc = GetEffectiveVatPercentage();
            if (vatPerc == 0)
                unit.Rate = unit.IncRate;
            else
                unit.Rate = Math.Round(unit.IncRate.Value / (1 + vatPerc / 100), PermanentSettingsData.CurrencyDecimalPlaces, MidpointRounding.AwayFromZero);
        }
        else
        {
            unit.Rate = null;
        }
        StateHasChanged();
    }

    // --- SEARCH METHODS ---

    private async Task OpenUnitSearch()
    {
        if (_isSearchDialogOpen) return;
        _isSearchDialogOpen = true;
        try
        {
            var dialog = DialogService.Show<UnitSearchDialog>(Localizer("SearchUnit"));
            var result = await dialog.Result;
            if (!result.Canceled && result.Data is UnitMaster selected)
            {
                OnUnitChanged(selected.UnitId);

                await Task.Delay(100);
                if (ddlVatRef is not null) await ddlVatRef.FocusAsync();
            }
            else
            {
                if (ddlUnitRef is not null) await ddlUnitRef.FocusAsync();
            }
        }
        finally
        {
            await Task.Delay(200);
            _isSearchDialogOpen = false;
        }
    }

    private async Task OpenBrandSearch()
    {
        if (_isSearchDialogOpen) return;
        _isSearchDialogOpen = true;
        try
        {
            var dialog = DialogService.Show<BrandSearchDialog>(Localizer("SearchBrand"), new DialogParameters { ["brandList"] = brandList });
            var result = await dialog.Result;
            if (!result.Canceled && result.Data is BrandMaster selected)
            {
                OnBrandChanged(selected.BrandId);

                await Task.Delay(100);
                if (ddlUnitRef is not null) await ddlUnitRef.FocusAsync();
            }
            else
            {
                if (ddlBrandRef is not null) await ddlBrandRef.FocusAsync();
            }
        }
        finally
        {
            await Task.Delay(200);
            _isSearchDialogOpen = false;
        }
    }

    private void OnCategoryChanged(int? categoryId)
    {
        if (categoryId.HasValue && categoryId.Value > 0)
        {
            if (item.FK_SubCategoryId.HasValue && item.FK_SubCategoryId.Value > 0)
            {
                var currentSub = subCategoryList.FirstOrDefault(s => s.SubCategoryId == item.FK_SubCategoryId);
                if (currentSub != null && currentSub.FK_CategoryId.HasValue && currentSub.FK_CategoryId != categoryId)
                {
                    item.FK_SubCategoryId = null;
                    subCategoryName = null;
                    item.SubCategory = null;
                }
            }
        }
        else
        {
            item.FK_SubCategoryId = null;
            subCategoryName = null;
            item.SubCategory = null;
        }

        // Clear item code if auto-code with category-wise or subcategory-wise is enabled
        // This ensures a new code will be generated based on the new category
        if (IsAutoCode && (IsCategoryWise || IsSubCategoryWise) && item.ItemId == 0)
        {
            item.ItemCode = null;
        }
    }

    private async Task OnSubCategoryChanged(int? subCategoryId)
    {
        item.FK_SubCategoryId = subCategoryId;
        var selected = subCategoryList.FirstOrDefault(s => s.SubCategoryId == subCategoryId);
        item.SubCategory = selected;
        subCategoryName = selected?.Name;

        // Auto-link category if not yet selected
        if (selected != null && selected.FK_CategoryId.HasValue && selected.FK_CategoryId.Value > 0 && (item.FK_CategoryId == null || item.FK_CategoryId == 0))
        {
            item.FK_CategoryId = selected.FK_CategoryId;
            item.Category = selected.Category ?? categoryList.FirstOrDefault(c => c.CategoryId == selected.FK_CategoryId);
            categoryName = item.Category?.Name;
        }

        if (IsAutoCode && IsSubCategoryWise && item.ItemId == 0)
        {
            item.ItemCode = null;
        }

        await LoadAutoCodeIfEnabled();
        StateHasChanged();
    }

    private void OnBrandChanged(int? brandId)
    {
        item.FK_BrandId = brandId;
        var selected = brandList.FirstOrDefault(b => b.BrandId == brandId);
        item.Brand = selected;
        brandName = selected?.Name;
        StateHasChanged();
    }

    private void OnUnitChanged(int? unitId)
    {
        item.FK_UnitId = unitId;
        var selected = unitList.FirstOrDefault(u => u.UnitId == unitId);
        item.Unit = selected;
        unitName = selected?.Name;

        // Auto-update first multi-unit if it exists
        if (item.MultiUnits != null && item.MultiUnits.Count > 0 && selected != null)
        {
            var firstUnit = item.MultiUnits[0];
            if (firstUnit != null)
            {
                firstUnit.FK_UnitId = selected.UnitId;
                firstUnit.Unit = selected;
                firstUnit.MultiUnitName = $"{item.ItemName}-{selected.Name}";
            }
        }

        // Sync main unit rate if multi units exist
        SyncDefaultMultiUnitRate();
        StateHasChanged();
    }

    /// <summary>
    /// Loads auto-generated item code based on category/subcategory settings.
    /// Called when category or subcategory is selected.
    /// </summary>
    private async Task LoadAutoCodeIfEnabled()
    {
        // Only load auto code for new items
        if (!IsAutoCode || item.ItemId > 0) return;

        try
        {
            // Get category and subcategory codes
            string? catCode = null;
            string? subCatCode = null;

            if (IsCategoryWise && item.FK_CategoryId.HasValue)
            {
                item.Category ??= categoryList.FirstOrDefault(c => c.CategoryId == item.FK_CategoryId);
                catCode = item.Category?.Code;
            }

            if (IsSubCategoryWise && item.FK_SubCategoryId.HasValue)
            {
                item.SubCategory ??= subCategoryList.FirstOrDefault(s => s.SubCategoryId == item.FK_SubCategoryId);
                subCatCode = item.SubCategory?.Code;
            }

            // Determine when to fetch code based on settings
            bool shouldFetch = false;
            if (IsSubCategoryWise)
            {
                // Both category and subcategory must be selected
                shouldFetch = item.FK_CategoryId.HasValue && item.FK_SubCategoryId.HasValue;
            }
            else if (IsCategoryWise)
            {
                // Only category must be selected
                shouldFetch = item.FK_CategoryId.HasValue;
            }
            else
            {
                // Auto only - can fetch anytime
                shouldFetch = true;
            }

            if (shouldFetch)
            {
                var code = await api.GetNextItemCode(
                    item.FK_CategoryId,
                    item.FK_SubCategoryId,
                    catCode,
                    subCatCode
                );

                if (!string.IsNullOrEmpty(code))
                {
                    item.ItemCode = code;
                    if (PermanentSettingsData.AutoBarcodeGeneration && string.IsNullOrWhiteSpace(item.Barcode))
                    {
                        item.Barcode = code;
                    }
                    StateHasChanged();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading auto item code: {ex.Message}");
        }
    }

    private void OnItemCodeBlur()
    {
        if (PermanentSettingsData.AutoBarcodeGeneration && string.IsNullOrWhiteSpace(item.Barcode) && !string.IsNullOrWhiteSpace(item.ItemCode))
        {
            item.Barcode = item.ItemCode;
            StateHasChanged();
        }
    }

    private void ClearBarcode()
    {
        item.Barcode = string.Empty;
        StateHasChanged();
    }

    private async Task SearchByBarcode()
    {
        if (string.IsNullOrWhiteSpace(item.Barcode))
        {
            Snackbar.Add(Localizer("EnterBarcodeFirst"), Severity.Info);
            return;
        }

        try
        {
            var barcodeToSearch = item.Barcode.Trim();
            var found = await api.GetItemByBarcode(barcodeToSearch);
            if (found != null && found.ItemId > 0)
            {
                await PopulateItemAsync(found);
                Snackbar.Add(Localizer("ItemLoadedSuccessfully"), Severity.Success);
            }
            else
            {
                Snackbar.Add(Localizer("ItemNotFound"), Severity.Warning);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add(ex.Message, Severity.Error);
        }
    }

    private void GenerateBarcodeFromCode()
    {
        if (!string.IsNullOrWhiteSpace(item.ItemCode))
        {
            item.Barcode = item.ItemCode.Trim();
            StateHasChanged();
        }
        else
        {
            Snackbar.Add(Localizer("EnterCodeFirst"), Severity.Info);
        }
    }

    private string lastEditedRate = "exclusive";

    private decimal GetEffectiveVatPercentage()
    {
        if (item.FK_VatId.HasValue && vatList != null)
        {
            var selectedVat = vatList.FirstOrDefault(v => v.VatId == item.FK_VatId.Value);
            if (selectedVat != null) return selectedVat.Percentage ?? 0m;
        }
        return selectedVatPercentage;
    }

    private void OnVatChanged(int? vatId) => SetVat(vatId, recalculate: true);

    private void SetVat(int? vatId, bool recalculate = true)
    {
        item.FK_VatId = vatId;
        if (vatId.HasValue)
        {
            var selectedVat = vatList.FirstOrDefault(v => v.VatId == vatId.Value);
            selectedVatPercentage = selectedVat?.Percentage ?? 0;
        }
        else
        {
            selectedVatPercentage = 0;
        }

        if (recalculate)
        {
            if (lastEditedRate == "inclusive" && item.InclusiveRate.HasValue && item.InclusiveRate > 0)
            {
                CalculateItemRateFromInclusiveRate();
            }
            else
            {
                CalculateItemInclusiveRateFromRate();
            }
            SyncDefaultMultiUnitRate();
        }
        else
        {
            if ((item.InclusiveRate == null || item.InclusiveRate == 0) && item.SalesRate.HasValue && item.SalesRate > 0)
            {
                CalculateItemInclusiveRateFromRate();
            }
            else if ((item.SalesRate == null || item.SalesRate == 0) && item.InclusiveRate.HasValue && item.InclusiveRate > 0)
            {
                CalculateItemRateFromInclusiveRate();
            }
            SyncDefaultMultiUnitRate();
        }
        StateHasChanged();
    }

    private void OnExclusiveRateBlur()
    {
        CalculateItemInclusiveRateFromRate();
        SyncDefaultMultiUnitRate();
        StateHasChanged();
    }

    private void OnInclusiveRateBlur()
    {
        CalculateItemRateFromInclusiveRate();
        SyncDefaultMultiUnitRate();
        StateHasChanged();
    }

    private void CalculateItemInclusiveRateFromRate()
    {
        if (item.SalesRate.HasValue)
        {
            item.SalesRate = Math.Round(item.SalesRate.Value, PermanentSettingsData.CurrencyDecimalPlaces, MidpointRounding.AwayFromZero);
            var vatPerc = GetEffectiveVatPercentage();
            if (vatPerc == 0)
                item.InclusiveRate = item.SalesRate;
            else
                item.InclusiveRate = Math.Round(item.SalesRate.Value * (1 + vatPerc / 100), PermanentSettingsData.CurrencyDecimalPlaces, MidpointRounding.AwayFromZero);
        }
        else
        {
            item.InclusiveRate = null;
        }
    }

    private void CalculateItemRateFromInclusiveRate()
    {
        if (item.InclusiveRate.HasValue)
        {
            item.InclusiveRate = Math.Round(item.InclusiveRate.Value, PermanentSettingsData.CurrencyDecimalPlaces, MidpointRounding.AwayFromZero);
            var vatPerc = GetEffectiveVatPercentage();
            if (vatPerc == 0)
                item.SalesRate = item.InclusiveRate;
            else
                item.SalesRate = Math.Round(item.InclusiveRate.Value / (1 + vatPerc / 100), PermanentSettingsData.CurrencyDecimalPlaces, MidpointRounding.AwayFromZero);
        }
        else
        {
            item.SalesRate = null;
        }
    }

    private void SyncDefaultMultiUnitRate()
    {
        if (item.MultiUnits != null && item.MultiUnits.Any())
        {
            var unit = item.MultiUnits.FirstOrDefault(mu => (item.FK_UnitId.HasValue && mu.FK_UnitId == item.FK_UnitId) || mu.QTYPerUnit == 1);
            if (unit != null)
            {
                unit.Rate = item.SalesRate;
                unit.IncRate = item.InclusiveRate;
            }
        }
    }

    private List<VATCategoryDetails> currentVatExcList = new();

    private async Task OnVatCategoryChanged(string category)
    {
        item.VatCategory = category;
        if (!string.IsNullOrWhiteSpace(category))
        {
            currentVatExcList = await api.GetVATCategoryDetails(category);
            var details = currentVatExcList.FirstOrDefault();

            if (currentVatExcList.Count == 1 && details is not null)
            {
                item.VatExcCode = details.ExemptionCode;
                item.VatExcName = details.ExemptionTextEN;
            }
            else
            {
                item.VatExcCode = null;
                item.VatExcName = null;
            }
        }
        else
        {
            currentVatExcList.Clear();
            item.VatExcCode = null;
            item.VatExcName = null;
        }
    }

    private async Task OpenVatExcSearch()
    {
        if (_isSearchDialogOpen) return;
        _isSearchDialogOpen = true;
        try
        {
            if (currentVatExcList == null || !currentVatExcList.Any())
            {
                if (!string.IsNullOrWhiteSpace(item.VatCategory))
                {
                    currentVatExcList = await api.GetVATCategoryDetails(item.VatCategory);
                }

                if (currentVatExcList == null || !currentVatExcList.Any())
                {
                    Snackbar.Add(Localizer("NoExemptionCodesForCategory"), Severity.Warning);
                    return;
                }
            }

            var dialog = DialogService.Show<VatExcSearchDialog>(Localizer("SearchVatExemption"), new DialogParameters { ["vatExcList"] = currentVatExcList }, new DialogOptions { MaxWidth = MaxWidth.Large, FullWidth = true });
            var result = await dialog.Result;
            if (!result.Canceled && result.Data is VATCategoryDetails selected)
            {
                item.VatExcCode = selected.ExemptionCode;
                item.VatExcName = selected.ExemptionTextEN;

                StateHasChanged();
                await Task.Delay(100);
                await FocusElement("txtExclusiveRate");
            }
            else
            {
                await FocusElement("txtVatExcCode");
            }
        }
        finally
        {
            await Task.Delay(200);
            _isSearchDialogOpen = false;
        }
    }

    private async Task OpenCategorySearch()
    {
        if (_isSearchDialogOpen) return;
        _isSearchDialogOpen = true;
        try
        {
            var dialog = DialogService.Show<CategorySearchDialog>(Localizer("SearchCategory"));
            var result = await dialog.Result;
            if (!result.Canceled && result.Data is CategoryMaster selected)
            {
                item.FK_CategoryId = selected.CategoryId;
                item.Category = selected;
                categoryName = selected.Name;
                OnCategoryChanged(selected.CategoryId);

                // Load auto-code if category-wise is enabled
                await LoadAutoCodeIfEnabled();

                // Focus Next: SubCategory
                StateHasChanged();
                await Task.Delay(100);
                if (ddlSubCategoryRef is not null) await ddlSubCategoryRef.FocusAsync();
            }
            else
            {
                await FocusElement("txtCategory");
            }
        }
        finally
        {
            await Task.Delay(200);
            _isSearchDialogOpen = false;
        }
    }

    private async Task OpenSubCategorySearch()
    {
        if (_isSearchDialogOpen) return;
        _isSearchDialogOpen = true;
        try
        {
            if (item.FK_CategoryId == 0 || item.FK_CategoryId == null)
            {
                Snackbar.Add(Localizer("SelectCategoryFirst"), Severity.Warning);
                await FocusElement("txtCategory");
                return;
            }

            var dialog = DialogService.Show<SubCategorySearchDialog>(Localizer("SearchSubCategory"), new DialogParameters { ["CategoryId"] = item.FK_CategoryId });
            var result = await dialog.Result;
            if (!result.Canceled && result.Data is SubCategoryMaster selected)
            {
                item.FK_SubCategoryId = selected.SubCategoryId;
                subCategoryName = selected.Name;
                item.SubCategory = selected;

                // Clear item code if auto-code with subcategory-wise is enabled
                // This ensures a new code will be generated based on the new subcategory
                if (IsAutoCode && IsSubCategoryWise && item.ItemId == 0)
                {
                    item.ItemCode = null;
                }

                // Load auto-code if subcategory-wise is enabled
                await LoadAutoCodeIfEnabled();

                // Focus Next: Brand
                StateHasChanged();
                await Task.Delay(100);
                if (ddlBrandRef is not null) await ddlBrandRef.FocusAsync();
            }
            else
            {
                if (ddlSubCategoryRef is not null) await ddlSubCategoryRef.FocusAsync();
            }
        }
        finally
        {
            await Task.Delay(200);
            _isSearchDialogOpen = false;
        }
    }

    // --- KEYBOARD & FOCUS METHODS ---

    // Main Form Navigation
    private async Task HandleMainFormKeyDown(KeyboardEventArgs e, string field)
    {
        bool isSelectField = field is "subcategory" or "brand" or "mainunit" or "vat" or "vatcategory" or "stockmode";

        if (e.Key == "Enter" || e.Key == "NumpadEnter" || (!isSelectField && e.Key == "ArrowDown"))
        {
            switch (field)
            {
                case "code": await FocusElement("txtName"); break;
                case "name": await FocusElement("txtArabicName"); break;
                case "arabic": await FocusElement("txtBarcode"); break;
                case "barcode": await FocusElement("txtCategory"); break;
                case "category": await OpenCategorySearch(); break;
                case "subcategory": if (ddlBrandRef is not null) await ddlBrandRef.FocusAsync(); break;
                case "brand": if (ddlUnitRef is not null) await ddlUnitRef.FocusAsync(); break;
                case "mainunit": if (ddlVatRef is not null) await ddlVatRef.FocusAsync(); break;
                case "vat": if (ddlVatCategoryRef is not null) await ddlVatCategoryRef.FocusAsync(); break;
                case "vatcategory": await FocusElement("txtVatExcCode"); break; // Fallback or direct
                case "vatexccode": await FocusElement("txtVatExcName"); break;
                case "vatexcname": await FocusElement("txtExclusiveRate"); break;
                case "exclusiverate": await FocusElement("txtInclusiveRate"); break;
                case "inclusiverate": await FocusElement("txtPurchaseRate"); break;
                case "purchaserate": await FocusElement("txtWholeSaleRate"); break;
                case "wholesalerate": await FocusElement("txtMinSaleRate"); break;
                case "minsalerate": await FocusElement("txtStandardCost"); break;
                case "standardcost": await FocusElement("txtLandingCost"); break;
                case "landingcost": if (ddlStockModeRef is not null) await ddlStockModeRef.FocusAsync(); break;
                case "stockmode": await FocusElement("txtModel"); break;
                case "model": await FocusElement("txtFitted"); break;
                case "fitted": await FocusElement("txtItemExpiry"); break;
                case "itemexpiry": await FocusElement("txtWeightPerCarton"); break;
                case "weight": await FocusElement("txtLocation"); break;
                case "location": await FocusElement("txtNotes"); break;
                case "notes":
                    // Switch to MultiUnit Tab
                    activeTabIndex = 1;
                    StateHasChanged();
                    await Task.Delay(150);
                    if (item.MultiUnits == null || item.MultiUnits.Count == 0) AddNewUnitRow();
                    await FocusElement("input-row-0-unit");
                    break;
            }
        }
        else if (e.Key == "ArrowUp" && !isSelectField)
        {
            switch (field)
            {
                case "name": await FocusElement("txtCode"); break;
                case "arabic": await FocusElement("txtName"); break;
                case "barcode": await FocusElement("txtArabicName"); break;
                case "category": await FocusElement("txtBarcode"); break;
                case "subcategory": await FocusElement("txtCategory"); break;
                case "brand": if (ddlSubCategoryRef is not null) await ddlSubCategoryRef.FocusAsync(); break;
                case "mainunit": if (ddlBrandRef is not null) await ddlBrandRef.FocusAsync(); break;
                case "vat": if (ddlUnitRef is not null) await ddlUnitRef.FocusAsync(); break;
                case "vatcategory": if (ddlVatRef is not null) await ddlVatRef.FocusAsync(); break;
                case "vatexccode": if (ddlVatCategoryRef is not null) await ddlVatCategoryRef.FocusAsync(); break;
                case "vatexcname": await FocusElement("txtVatExcCode"); break;
                case "exclusiverate":
                    // Check if Exemption Fields were visible? Assume flow back to Category or Name?
                    // Simplest: go back to VatCategory or excname if present. 
                    // Let's go to VatCategory for safe default
                    if (ddlVatCategoryRef is not null) await ddlVatCategoryRef.FocusAsync();
                    break;
                case "inclusiverate": await FocusElement("txtExclusiveRate"); break;
                case "purchaserate": await FocusElement("txtInclusiveRate"); break;
                case "wholesalerate": await FocusElement("txtPurchaseRate"); break;
                case "minsalerate": await FocusElement("txtWholeSaleRate"); break;
                case "standardcost": await FocusElement("txtMinSaleRate"); break;
                case "landingcost": await FocusElement("txtStandardCost"); break;
                case "stockmode": await FocusElement("txtLandingCost"); break;
                case "model": if (ddlStockModeRef is not null) await ddlStockModeRef.FocusAsync(); break;
                case "fitted": await FocusElement("txtModel"); break;
                case "itemexpiry": await FocusElement("txtFitted"); break;
                case "weight": await FocusElement("txtItemExpiry"); break;
                case "location": await FocusElement("txtWeightPerCarton"); break;
                case "notes": await FocusElement("txtLocation"); break;
            }
        }
    }

    private async Task HandleKeyDown(KeyboardEventArgs e, int rowIndex, string currentField)
    {
        if (e.Key == "ArrowUp")
        {
            if (rowIndex > 0)
                await FocusElement($"input-row-{rowIndex - 1}-{currentField}");
            else
            {
                // Go back to notes if on first row? optional
                if (rowIndex == 0)
                {
                    activeTabIndex = 0;
                    StateHasChanged();
                    await Task.Delay(100);
                    await FocusElement("txtNotes");
                }
            }
        }
        else if (e.Key == "ArrowDown")
        {
            if (rowIndex < item.MultiUnits.Count - 1)
                await FocusElement($"input-row-{rowIndex + 1}-{currentField}");
        }
        else if (e.Key == "Enter" || e.Key == "NumpadEnter")
        {
            switch (currentField)
            {
                case "unit": await OpenMultiUnitSearchRow(item.MultiUnits[rowIndex]); break;
                case "name": await FocusElement($"input-row-{rowIndex}-arabic"); break;
                case "arabic": await FocusElement($"input-row-{rowIndex}-qty"); break;
                case "qty": await FocusElement($"input-row-{rowIndex}-rate"); break;
                case "rate": await FocusElement($"input-row-{rowIndex}-incrate"); break;
                case "incrate": await FocusElement($"input-row-{rowIndex}-cost"); break;
                case "cost": await FocusElement($"input-row-{rowIndex}-barcode"); break;
                case "barcode":
                    AddNewUnitRow();
                    await Task.Delay(100);
                    await FocusElement($"input-row-{rowIndex + 1}-unit");
                    break;
            }
        }
    }

    private async Task FocusElement(string elementId)
    {
        try
        {
            // Simple JS eval to focus and select by ID
            await JSRuntime.InvokeVoidAsync("eval", $"var el = document.getElementById('{elementId}'); if (el) {{ el.focus(); if (el.select) el.select(); }}");
        }
        catch
        {
            // Ignore focus errors
        }
    }

    #endregion
}
