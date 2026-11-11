using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Globalization;
using ZakiSAASERP.Models.Masters;
using ZakiSAASERP.Models.Services;
using ZakiSAASERP.Models.Settings;
using ZakiSAASERP.POS.Services;

namespace ZakiSAASERP.POS.ViewModels;

public partial class ItemMasterViewModel(IPosDataService posDataService) : ObservableObject
{
    private List<SubCategoryMaster> _allSubCategories = [];
    private bool _isInitializing;
    private bool _isUpdatingRates;
    private string _lastEditedRate = "exclusive";

    [ObservableProperty]
    private string itemCode = "";

    [ObservableProperty]
    private string itemName = "";

    [ObservableProperty]
    private string itemNameArabic = "";

    [ObservableProperty]
    private string barcode = "";

    [ObservableProperty]
    private string purchaseRateText = "0";

    [ObservableProperty]
    private string standardCostText = "0";

    [ObservableProperty]
    private string salesRateText = "0";

    [ObservableProperty]
    private string inclusiveRateText = "0";

    [ObservableProperty]
    private string stockMode = "Inventory";

    [ObservableProperty]
    private string location = "";

    [ObservableProperty]
    private string notes = "";

    [ObservableProperty]
    private bool isSaleable = true;

    [ObservableProperty]
    private ObservableCollection<CategoryMaster> categories = [];

    [ObservableProperty]
    private CategoryMaster? selectedCategory;

    [ObservableProperty]
    private ObservableCollection<SubCategoryMaster> filteredSubCategories = [];

    [ObservableProperty]
    private SubCategoryMaster? selectedSubCategory;

    [ObservableProperty]
    private ObservableCollection<BrandMaster> brands = [];

    [ObservableProperty]
    private BrandMaster? selectedBrand;

    [ObservableProperty]
    private ObservableCollection<UnitMaster> units = [];

    [ObservableProperty]
    private UnitMaster? selectedUnit;

    [ObservableProperty]
    private ObservableCollection<VatMaster> vats = [];

    [ObservableProperty]
    private VatMaster? selectedVat;

    [ObservableProperty]
    private decimal vatPercentage = 15m;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string errorMessage = "";

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool IsAutoCode => PermanentSettingsData.AutoItemCode;
    public List<string> StockModes { get; } = ["Inventory", "Non Inventory"];

    public bool DialogResult { get; private set; }
    public ItemMaster? CreatedItem { get; private set; }
    public event Action? RequestClose;

    partial void OnErrorMessageChanged(string value)
    {
        OnPropertyChanged(nameof(HasError));
    }

    public async Task InitializeAsync(string? initialBarcode = null)
    {
        _isInitializing = true;
        IsBusy = true;
        ErrorMessage = "";
        DialogResult = false;
        CreatedItem = null;

        try
        {
            var catsTask = posDataService.GetCategoriesAsync();
            var subCatsTask = posDataService.GetSubCategoriesAsync();
            var brandsTask = posDataService.GetBrandsAsync();
            var unitsTask = posDataService.GetAllUnitsAsync();
            var vatsTask = posDataService.GetVatsAsync();

            await Task.WhenAll(catsTask, subCatsTask, brandsTask, unitsTask, vatsTask);

            Categories = new ObservableCollection<CategoryMaster>(catsTask.Result ?? []);
            _allSubCategories = subCatsTask.Result ?? [];
            Brands = new ObservableCollection<BrandMaster>(brandsTask.Result ?? []);
            Units = new ObservableCollection<UnitMaster>(unitsTask.Result ?? []);
            Vats = new ObservableCollection<VatMaster>(vatsTask.Result ?? []);

            SelectedCategory = Categories.FirstOrDefault(c => c.Name.Equals("GENERAL", StringComparison.OrdinalIgnoreCase))
                ?? Categories.FirstOrDefault();

            FilterSubCategories();
            SelectedSubCategory = FilteredSubCategories.FirstOrDefault(sc => string.Equals(sc.Name, "GENERAL", StringComparison.OrdinalIgnoreCase))
                ?? FilteredSubCategories.FirstOrDefault();

            SelectedBrand = Brands.FirstOrDefault();
            SelectedUnit = Units.FirstOrDefault(u => u.Name.Equals("EACH", StringComparison.OrdinalIgnoreCase))
                ?? Units.FirstOrDefault();

            SelectedVat = Vats.FirstOrDefault(v => v.IsDefault) ?? Vats.FirstOrDefault();
            VatPercentage = SelectedVat?.Percentage ?? 0m;

            Barcode = initialBarcode?.Trim() ?? "";
            ItemCode = "";
            ItemName = "";
            ItemNameArabic = "";
            PurchaseRateText = "0";
            StandardCostText = "0";
            SalesRateText = "0";
            InclusiveRateText = "0";
            StockMode = "Inventory";
            Location = "";
            Notes = "";
            IsSaleable = true;
            _lastEditedRate = "exclusive";

            if (IsAutoCode)
            {
                await LoadAutoCodeAsync();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load master data: {ex.Message}";
        }
        finally
        {
            _isInitializing = false;
            IsBusy = false;
        }
    }

    partial void OnSelectedCategoryChanged(CategoryMaster? value)
    {
        if (_isInitializing) return;
        FilterSubCategories();
        SelectedSubCategory = FilteredSubCategories.FirstOrDefault();
        if (IsAutoCode)
        {
            _ = LoadAutoCodeAsync();
        }
    }

    partial void OnSelectedSubCategoryChanged(SubCategoryMaster? value)
    {
        if (_isInitializing) return;
        if (IsAutoCode && PermanentSettingsData.SubCategoryWiseItemCode)
        {
            _ = LoadAutoCodeAsync();
        }
    }

    partial void OnSelectedVatChanged(VatMaster? value)
    {
        VatPercentage = value?.Percentage ?? 0m;
        if (_isInitializing) return;

        if (_lastEditedRate == "inclusive")
            RecalculateSalesRateFromInclusive();
        else
            RecalculateInclusiveRateFromSales();
    }

    partial void OnSalesRateTextChanged(string value)
    {
        if (_isInitializing || _isUpdatingRates) return;
        _lastEditedRate = "exclusive";
        if (TryParseDecimal(value, out _) && !value.EndsWith('.'))
        {
            RecalculateInclusiveRateFromSales();
        }
    }

    partial void OnInclusiveRateTextChanged(string value)
    {
        if (_isInitializing || _isUpdatingRates) return;
        _lastEditedRate = "inclusive";
        if (TryParseDecimal(value, out _) && !value.EndsWith('.'))
        {
            RecalculateSalesRateFromInclusive();
        }
    }

    private void FilterSubCategories()
    {
        if (SelectedCategory != null && SelectedCategory.CategoryId > 0)
        {
            FilteredSubCategories = new ObservableCollection<SubCategoryMaster>(
                _allSubCategories.Where(sc => sc.FK_CategoryId == SelectedCategory.CategoryId));
        }
        else
        {
            FilteredSubCategories = new ObservableCollection<SubCategoryMaster>(_allSubCategories);
        }
    }

    private async Task LoadAutoCodeAsync()
    {
        try
        {
            if (!IsAutoCode) return;

            bool shouldFetch = PermanentSettingsData.SubCategoryWiseItemCode
                ? SelectedCategory != null && SelectedSubCategory != null
                : !PermanentSettingsData.CategoryWiseItemCode || SelectedCategory != null;

            if (!shouldFetch) return;

            var code = await posDataService.GetNextItemCodeAsync(
                SelectedCategory?.CategoryId,
                SelectedSubCategory?.SubCategoryId,
                SelectedCategory?.Code,
                SelectedSubCategory?.Code);

            if (!string.IsNullOrWhiteSpace(code))
            {
                ItemCode = code;
                if (PermanentSettingsData.AutoBarcodeGeneration && string.IsNullOrWhiteSpace(Barcode))
                {
                    Barcode = code;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ItemMasterViewModel] AutoCode error: {ex.Message}");
        }
    }

    private void RecalculateInclusiveRateFromSales()
    {
        if (!TryParseDecimal(SalesRateText, out decimal exRate)) return;

        _isUpdatingRates = true;
        try
        {
            int dec = PermanentSettingsData.CurrencyDecimalPlaces;
            decimal factor = 1m + (VatPercentage / 100m);
            decimal inc = Math.Round(exRate * factor, dec, MidpointRounding.AwayFromZero);
            InclusiveRateText = inc.ToString($"F{dec}", CultureInfo.InvariantCulture);
        }
        finally
        {
            _isUpdatingRates = false;
        }
    }

    private void RecalculateSalesRateFromInclusive()
    {
        if (!TryParseDecimal(InclusiveRateText, out decimal incRate)) return;

        _isUpdatingRates = true;
        try
        {
            int dec = PermanentSettingsData.CurrencyDecimalPlaces;
            decimal factor = 1m + (VatPercentage / 100m);
            decimal ex = factor != 0m
                ? Math.Round(incRate / factor, dec, MidpointRounding.AwayFromZero)
                : incRate;
            SalesRateText = ex.ToString($"F{dec}", CultureInfo.InvariantCulture);
        }
        finally
        {
            _isUpdatingRates = false;
        }
    }

    [RelayCommand]
    private void ClearBarcode()
    {
        Barcode = "";
    }

    [RelayCommand]
    private void GenerateBarcodeFromCode()
    {
        if (!string.IsNullOrWhiteSpace(ItemCode))
        {
            Barcode = ItemCode.Trim();
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy) return;
        ErrorMessage = "";

        if (string.IsNullOrWhiteSpace(ItemCode))
        {
            ErrorMessage = "Item Code is required.";
            return;
        }
        if (string.IsNullOrWhiteSpace(ItemName))
        {
            ErrorMessage = "Item Name is required.";
            return;
        }
        if (SelectedUnit == null || SelectedUnit.UnitId <= 0)
        {
            ErrorMessage = "Unit is required.";
            return;
        }

        string? finalBarcode = string.IsNullOrWhiteSpace(Barcode)
            ? (PermanentSettingsData.AutoBarcodeGeneration ? ItemCode.Trim() : null)
            : Barcode.Trim();

        if (PermanentSettingsData.RequireBarcode && string.IsNullOrWhiteSpace(finalBarcode))
        {
            ErrorMessage = "Barcode is required.";
            return;
        }

        TryParseDecimal(PurchaseRateText, out decimal purchaseRate);
        TryParseDecimal(StandardCostText, out decimal standardCost);
        TryParseDecimal(SalesRateText, out decimal salesRate);
        TryParseDecimal(InclusiveRateText, out decimal inclusiveRate);

        int dec = PermanentSettingsData.CurrencyDecimalPlaces;
        decimal factor = 1m + (VatPercentage / 100m);
        if (_lastEditedRate == "inclusive" && inclusiveRate > 0 && salesRate == 0)
        {
            salesRate = factor != 0m ? Math.Round(inclusiveRate / factor, dec, MidpointRounding.AwayFromZero) : inclusiveRate;
        }
        else if (salesRate > 0 && inclusiveRate == 0)
        {
            inclusiveRate = Math.Round(salesRate * factor, dec, MidpointRounding.AwayFromZero);
        }

        purchaseRate = Math.Round(purchaseRate, dec, MidpointRounding.AwayFromZero);
        standardCost = Math.Round(standardCost, dec, MidpointRounding.AwayFromZero);
        salesRate = Math.Round(salesRate, dec, MidpointRounding.AwayFromZero);
        inclusiveRate = Math.Round(inclusiveRate, dec, MidpointRounding.AwayFromZero);

        IsBusy = true;
        try
        {
            var newItem = new ItemMaster
            {
                ItemCode = ItemCode.Trim(),
                ItemName = ItemName.Trim(),
                ItemNameArabic = string.IsNullOrWhiteSpace(ItemNameArabic) ? null : ItemNameArabic.Trim(),
                Barcode = finalBarcode,
                FK_CategoryId = SelectedCategory?.CategoryId,
                FK_SubCategoryId = SelectedSubCategory?.SubCategoryId,
                FK_BrandId = SelectedBrand?.BrandId,
                FK_UnitId = SelectedUnit.UnitId,
                FK_VatId = SelectedVat?.VatId,
                VatCategory = "S",
                StockMode = StockMode,
                Location = string.IsNullOrWhiteSpace(Location) ? null : Location.Trim(),
                Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim(),
                PurchaseRate = purchaseRate,
                StandardCost = standardCost,
                SalesRate = salesRate,
                InclusiveRate = inclusiveRate,
                IsActive = true,
                IsSaleable = IsSaleable,
                ItemType = "Normal",
                MultiUnits =
                [
                    new ItemMultiUnit
                    {
                        FK_UnitId = SelectedUnit.UnitId,
                        MultiUnitName = $"{ItemName.Trim()}-{SelectedUnit.Name}",
                        MultiUnitArabicName = string.IsNullOrWhiteSpace(ItemNameArabic) ? null : ItemNameArabic.Trim(),
                        QTYPerUnit = 1m,
                        Rate = salesRate,
                        IncRate = inclusiveRate,
                        Cost = standardCost,
                        Barcode = finalBarcode
                    }
                ]
            };

            var res = await posDataService.CreateItemAsync(newItem);
            if (res.Id > 0)
            {
                newItem.ItemId = res.Id;
                CreatedItem = newItem;
                DialogResult = true;
                RequestClose?.Invoke();
            }
            else
            {
                ErrorMessage = res.Message ?? "Failed to create product.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        DialogResult = false;
        RequestClose?.Invoke();
    }

    private static bool TryParseDecimal(string? input, out decimal value)
    {
        value = 0m;
        if (string.IsNullOrWhiteSpace(input)) return true;
        return decimal.TryParse(input.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out value)
            || decimal.TryParse(input.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out value);
    }
}

