using ZakiSAASERP.Interface.Transaction.Sales;
using ZakiSAASERP.Models.Company;
using ZakiSAASERP.Models.DTO;
using ZakiSAASERP.Models.Masters;
using ZakiSAASERP.Models.Others;
using ZakiSAASERP.Models.Services;
using ZakiSAASERP.Models.Settings;
using ZakiSAASERP.Models.Transactions.Sales;
using ZakiSAASERP.Models.ViewModel;
using ZakiSAASERP.POS.Services;
using AshproStringExtension;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Threading;

namespace ZakiSAASERP.POS.ViewModels;

public partial class POSViewModel : ObservableObject
{
    private readonly IPosDataService _posDataService;
    private readonly PrintHelper _printHelper;
    private readonly POSSettingsService _settingsService;
    private readonly ICustomerDisplayService _customerDisplayService;
    private readonly IZatcaRepo _zatcaRepo;
    private readonly ShiftService _shiftService;
    private readonly DispatcherTimer _clockTimer;

    #region Observable Properties

    [ObservableProperty]
    private POSSettings settings = new();

    [ObservableProperty]
    private int zatcaPendingCount;

    [ObservableProperty]
    private bool hasZatcaPending;

    public bool IsZatcaEnabled => ZakiSAASERP.Models.Services.PermanentSettingsData.IsZatca && ZakiSAASERP.Models.Services.PermanentSettingsData.ZatcaPhase == 2;

    [ObservableProperty]
    private int heldInvoicesCount;

    [ObservableProperty]
    private bool hasHeldInvoices;

    public string HeldCountBadgeBackground => HeldInvoicesCount > 0 ? "#EF4444" : "#334155";

    [ObservableProperty]
    private bool isCustomerDisplayOpen;

    [ObservableProperty]
    private string invoiceNo = "";

    [ObservableProperty]
    private DateTime salesDate = DateTime.Now;

    [ObservableProperty]
    private string customerName = "Cash Customer";

    [ObservableProperty]
    private int customerId;

    [ObservableProperty]
    private string salesmanName = "";

    [ObservableProperty]
    private int salesmanId;

    [ObservableProperty]
    private string barcodeSearchQuery = "";

    [ObservableProperty]
    private string activeInputField = "BarcodeSearch";

    [ObservableProperty]
    private string numPadBuffer = "";

    private bool _replaceBufferOnInput = false;

    [ObservableProperty]
    private bool showNumPad = true;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string statusMessage = "Ready";

    [ObservableProperty]
    private string currentTime = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");

    [ObservableProperty]
    private ObservableCollection<TransactionDtls> cartItems = [];

    [ObservableProperty]
    private TransactionDtls? selectedCartItem;

    [ObservableProperty]
    private ObservableCollection<CategoryMaster> categories = [];

    [ObservableProperty]
    private CategoryMaster? selectedCategory;

    private List<ExMasters> _allProducts = [];

    private TransactionHdr? _lastSavedHdr;
    private List<TransactionDtls>? _lastSavedItems;
    private Party? _lastSavedParty;

    [ObservableProperty]
    private ObservableCollection<ExMasters> displayedProducts = [];

    [ObservableProperty]
    private int productPage = 1;

    [ObservableProperty]
    private int totalProductPages = 1;

    [ObservableProperty]
    private int totalProductsCount;

    [ObservableProperty]
    private int pageSize = 15;

    [ObservableProperty]
    private decimal totalQty;

    [ObservableProperty]
    private decimal grossAmount;

    [ObservableProperty]
    private decimal discountAmount;

    [ObservableProperty]
    private decimal discountPercentage;

    [ObservableProperty]
    private decimal taxAmount;

    [ObservableProperty]
    private decimal grandTotal;

    [ObservableProperty]
    private decimal? tendered;

    [ObservableProperty]
    private decimal? changes;

    public List<CommonModel> CustomersList { get; private set; } = [];
    public List<CommonModel> UnitsList { get; private set; } = [];
    public List<CommonModel> SalesmenList { get; private set; } = [];

    #endregion

    #region Dialog & UI Interaction Delegates

    public Func<decimal, Task<(bool success, string paymentType, decimal tendered, decimal change)>>? ShowPaymentDialogHandler;
    public Func<Task<CustomerMaster?>>? ShowCustomerSearchHandler;
    public Func<Task<int?>>? ShowHeldInvoicesHandler;
    public Func<Task<int?>>? ShowBillSearchHandler;
    public Func<string?, Task<ItemDialogDTO?>>? ShowItemSearchHandler;
    public Func<decimal, decimal, Task<decimal?>>? ShowDiscountHandler;
    public Func<Task<bool>>? ShowSettingsDialogHandler;
    public Func<Task>? ShowZatcaPendingHandler;
    public Func<Task<bool>>? ShowShiftDialogHandler;
    public Func<string, string, Task<bool>>? ConfirmActionHandler;
    public Func<string?, Task<ItemMaster?>>? ShowItemMasterDialogHandler;
    public Action<string, string>? ShowNotificationHandler;
    public Action? FocusBarcodeHandler;
    public Action<string>? FocusCartCellHandler;
    public Action<string>? ActiveBufferUpdatedHandler;

    #endregion

    public POSViewModel(
        IPosDataService posDataService,
        PrintHelper printHelper,
        POSSettingsService settingsService,
        ICustomerDisplayService customerDisplayService,
        IZatcaRepo zatcaRepo,
        ShiftService shiftService)
    {
        _posDataService = posDataService;
        _printHelper = printHelper;
        _settingsService = settingsService;
        _customerDisplayService = customerDisplayService;
        _zatcaRepo = zatcaRepo;
        _shiftService = shiftService;

        Settings = _settingsService.CurrentSettings;
        ShowNumPad = Settings.ShowNumPadByDefault;

        _settingsService.OnSettingsChanged += () =>
        {
            Settings = _settingsService.CurrentSettings;
            if (!Settings.ShowCategoryPanel)
            {
                SelectedCategory = null;
                FilterProducts();
            }
            ShowNumPad = Settings.ShowNumPadByDefault;
            OnPropertyChanged(nameof(Settings));
        };

        _clockTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _clockTimer.Tick += (_, _) =>
        {
            CurrentTime = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
        };
        _clockTimer.Start();
    }

    #region Lifecycle & Initialization

    [RelayCommand]
    public async Task InitializeAsync()
    {
        IsBusy = true;
        StatusMessage = "Loading settings and master data...";

        try
        {
            await _posDataService.InitializeSystemSettingsAsync();

            int branchId = AppData.BranchId > 0 ? AppData.BranchId : 1;

            CustomersList = await _posDataService.GetCustomersAsync(branchId);
            UnitsList = await _posDataService.GetUnitsAsync();
            SalesmenList = await _posDataService.GetSalesmenAsync();
            var cats = await _posDataService.GetCategoriesAsync();
            _allProducts = await _posDataService.GetPosProductsAsync(branchId);

            Categories = new ObservableCollection<CategoryMaster>(cats ?? []);
            if (Categories.Any())
            {
                SelectedCategory = Categories.First();
                FilterProducts();
            }

            var defaultCust = CustomersList.FirstOrDefault(x => x.Name.Contains("Cash", StringComparison.OrdinalIgnoreCase))
                           ?? CustomersList.FirstOrDefault();
            if (defaultCust != null)
            {
                CustomerId = defaultCust.Id;
                CustomerName = defaultCust.Name;
            }

            await NewBillAsync();
            await RefreshZatcaPendingAsync();
            await RefreshHeldInvoicesAsync();
            StatusMessage = "Ready for transactions";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Init Error: {ex.Message}";
            ShowNotificationHandler?.Invoke("Error", $"Initialization Error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            FocusBarcodeHandler?.Invoke();
        }
    }

    #endregion

    #region Products & Categories

    [RelayCommand]
    public void SelectCategory(CategoryMaster? category)
    {
        SelectedCategory = category;
        ProductPage = 1;
        FilterProducts();
        FocusBarcodeHandler?.Invoke();
    }

    private void FilterProducts()
    {
        var filtered = SelectedCategory == null
            ? _allProducts
            : _allProducts.Where(p => p.CategoryId == SelectedCategory.CategoryId).ToList();

        TotalProductsCount = filtered.Count;
        TotalProductPages = Math.Max(1, (int)Math.Ceiling((double)TotalProductsCount / PageSize));
        if (ProductPage > TotalProductPages) ProductPage = TotalProductPages;
        if (ProductPage < 1) ProductPage = 1;

        var paged = filtered.Skip((ProductPage - 1) * PageSize).Take(PageSize).ToList();
        DisplayedProducts = new ObservableCollection<ExMasters>(paged);
    }

    private async Task RefreshProductsAsync()
    {
        int branchId = AppData.BranchId > 0 ? AppData.BranchId : 1;
        _allProducts = await _posDataService.GetPosProductsAsync(branchId);
        FilterProducts();
    }

    [RelayCommand]
    public void NextProductPage()
    {
        if (ProductPage < TotalProductPages)
        {
            ProductPage++;
            FilterProducts();
        }
    }

    [RelayCommand]
    public void PrevProductPage()
    {
        if (ProductPage > 1)
        {
            ProductPage--;
            FilterProducts();
        }
    }

    #endregion

    #region Cart & Line Item Operations

    [RelayCommand]
    public async Task AddProductToBillAsync(ExMasters? product)
    {
        if (product == null) return;

        try
        {
            var multiUnit = await _posDataService.GetItemMultiUnitByProductIdAsync(product.Id);
            if (multiUnit == null)
            {
                var itemDetails = await _posDataService.GetItemByIdAsync(product.Id);
                if (itemDetails != null)
                {
                    multiUnit = new ItemMultiUnit
                    {
                        FK_ItemId = itemDetails.ItemId,
                        FK_UnitId = itemDetails.FK_UnitId ?? 1,
                        MultiUnitName = itemDetails.ItemName,
                        MultiUnitArabicName = itemDetails.ItemNameArabic,
                        Barcode = itemDetails.Barcode ?? itemDetails.ItemCode,
                        QTYPerUnit = 1,
                        Rate = itemDetails.SalesRate,
                        IncRate = itemDetails.InclusiveRate
                    };
                }
            }

            if (multiUnit != null)
            {
                if ((multiUnit.IncRate ?? 0) == 0 && product.Amount > 0)
                {
                    multiUnit.IncRate = product.Amount;
                }
                await AddMultiUnitToCartAsync(multiUnit);
            }
            else
            {
                ShowNotificationHandler?.Invoke("Warning", $"Product details not found for {product.Name}");
            }
        }
        catch (Exception ex)
        {
            ShowNotificationHandler?.Invoke("Error", $"Error adding product: {ex.Message}");
        }
        finally
        {
            FocusBarcodeHandler?.Invoke();
        }
    }

    private async Task AddMultiUnitToCartAsync(ItemMultiUnit multiUnit)
    {
        var existing = CartItems.FirstOrDefault(x => x.FK_ItemId == multiUnit.FK_ItemId && x.FK_UnitId == multiUnit.FK_UnitId);
        if (existing != null)
        {
            existing.Quantity += 1;

            TransactionHelper.GetTransactionAfterEdit(existing, Settings.ShowIncRate);
            SelectedCartItem = existing;
        }
        else
        {
            var unit = UnitsList.FirstOrDefault(x => x.Id == multiUnit.FK_UnitId);
            decimal vat = await _posDataService.GetVatPercentageAsync(multiUnit.FK_ItemId);
            if (vat <= 0) vat = 15m;

            decimal qty = 1;
            decimal exRate = multiUnit.Rate ?? 0;
            decimal inRate = multiUnit.IncRate ?? 0;

            if (inRate == 0 && exRate > 0)
            {
                inRate = Math.Round(exRate * (1 + (vat / 100m)), 2, MidpointRounding.AwayFromZero);
            }
            else if (exRate == 0 && inRate > 0)
            {
                exRate = Math.Round(inRate / (1 + (vat / 100m)), 4, MidpointRounding.AwayFromZero);
            }

            var line = new TransactionDtls
            {
                SL = CartItems.Count + 1,
                FK_ItemId = multiUnit.FK_ItemId,
                FK_UnitId = multiUnit.FK_UnitId,
                ItemName = multiUnit.MultiUnitName ?? "Item",
                ItemArabicName = multiUnit.MultiUnitArabicName,
                Barcode = multiUnit.Barcode,
                UnitName = unit?.Name ?? "PCS",
                Quantity = qty,
                QtyPerUnit = (multiUnit.QTYPerUnit ?? 0) > 0 ? multiUnit.QTYPerUnit!.Value : 1,
                ExRate = exRate,
                InRate = inRate,
                TaxPerc = vat
            };

            TransactionHelper.GetTransactionAfterEdit(line, Settings.ShowIncRate);
            CartItems.Add(line);
            SelectedCartItem = line;
        }

        RecalculateTotals();
    }

    [RelayCommand]
    public void IncrementQty()
    {
        if (SelectedCartItem != null)
        {
            SelectedCartItem.Quantity += 1;
            TransactionHelper.GetTransactionAfterEdit(SelectedCartItem, Settings.ShowIncRate);
            RecalculateTotals();
            FocusBarcodeHandler?.Invoke();
        }
    }

    [RelayCommand]
    public void DecrementQty()
    {
        if (SelectedCartItem != null)
        {
            if (SelectedCartItem.Quantity > 1)
            {
                SelectedCartItem.Quantity -= 1;
                TransactionHelper.GetTransactionAfterEdit(SelectedCartItem, Settings.ShowIncRate);
                RecalculateTotals();
                FocusBarcodeHandler?.Invoke();
            }
        }
    }

    [RelayCommand]
    public void DeleteSelectedLine()
    {
        if (SelectedCartItem != null)
        {
            CartItems.Remove(SelectedCartItem);
            int sl = 1;
            foreach (var item in CartItems)
            {
                item.SL = sl++;
            }
            SelectedCartItem = CartItems.LastOrDefault();
            RecalculateTotals();
            FocusBarcodeHandler?.Invoke();
        }
    }


    public void UpdateInRate(TransactionDtls row)
    {
        if (row == null) return;
        var taxRate = row.TaxPerc > 0 ? row.TaxPerc : 15m;
        row.InRate = Math.Round(row.ExRate * (1 + (taxRate / 100m)), 2, MidpointRounding.AwayFromZero);
    }

    public void UpdateExRate(TransactionDtls row)
    {
        if (row == null) return;
        var taxRate = row.TaxPerc > 0 ? row.TaxPerc : 15m;
        row.ExRate = Math.Round(row.InRate / (1 + (taxRate / 100m)), 4, MidpointRounding.AwayFromZero);
    }

    public void OnRowFieldEdited(TransactionDtls row, string header)
    {
        if (row == null) return;

        switch (header)
        {
            case "Ex. Rate":
            case "Rate":
                UpdateInRate(row);
                TransactionHelper.GetTransactionAfterEdit(row, Settings.ShowIncRate);
                if (ActiveInputField == "Rate")
                {
                    NumPadBuffer = row.ExRate.ToString("F2");
                }
                break;

            case "Inc. Rate":
            case "IncRate":
                UpdateExRate(row);
                TransactionHelper.GetTransactionAfterEdit(row, true);
                if (ActiveInputField == "IncRate")
                {
                    NumPadBuffer = row.InRate.ToString("F2");
                }
                break;

            case "Qty":
                TransactionHelper.GetTransactionAfterEdit(row, Settings.ShowIncRate);
                if (ActiveInputField == "Qty")
                {
                    NumPadBuffer = row.Quantity.ToString("G29");
                }
                break;

            case "Disc (%)":
                TransactionHelper.GetTransactionAfterEdit(row, Settings.ShowIncRate);
                if (ActiveInputField == "Disc%")
                {
                    NumPadBuffer = row.DiscountPerc.ToString("F2");
                }
                break;

            case "Disc":
                if (row.GrossAmount > 0)
                {
                    row.DiscountPerc = Math.Round((row.DiscountAmnt / row.GrossAmount) * 100m, 4, MidpointRounding.AwayFromZero);
                }
                else
                {
                    row.DiscountPerc = 0;
                }
                TransactionHelper.GetTransactionAfterEdit(row, Settings.ShowIncRate, true);
                if (ActiveInputField == "DiscAmt")
                {
                    NumPadBuffer = row.DiscountAmnt.ToString("F2");
                }
                break;

            default:
                TransactionHelper.GetTransactionAfterEdit(row, Settings.ShowIncRate);
                break;
        }

        RecalculateTotals();
    }

    public void RecalculateTotals()
    {
        foreach (var item in CartItems)
        {
            if (item.InRate == 0 && item.ExRate > 0)
            {
                UpdateInRate(item);
            }
            else if (item.ExRate == 0 && item.InRate > 0)
            {
                UpdateExRate(item);
            }
            TransactionHelper.GetTransactionAfterEdit(item, Settings.ShowIncRate);
        }

        TotalQty = CartItems.Sum(x => x.Quantity);
        GrossAmount = CartItems.Sum(x => x.GrossAmount);

        decimal lineDisc = CartItems.Sum(x => x.DiscountAmnt);
        if (DiscountAmount > 0)
        {
            // If additional invoice discount is applied
            lineDisc += DiscountAmount;
        }

        TaxAmount = CartItems.Sum(x => x.TaxAmount);
        GrandTotal = CartItems.Sum(x => x.LineTotal) - DiscountAmount;
        if (GrandTotal < 0 )
        {
            GrandTotal = 0;
        }

        UpdateCustomerDisplay();
    }

    #endregion

    #region Barcode & Search

    [RelayCommand]
    public async Task ProcessBarcodeAsync()
    {
        if (string.IsNullOrWhiteSpace(BarcodeSearchQuery)) return;

        string barcode = BarcodeSearchQuery.Trim();
        BarcodeSearchQuery = "";
        NumPadBuffer = "";

        try
        {
            var multiUnit = await _posDataService.GetItemMultiUnitByBarcodeAsync(barcode);
            if (multiUnit == null)
            {
                var itemByBarcode = await _posDataService.GetItemByBarcodeAsync(barcode);
                if (itemByBarcode != null &&
                    string.Equals(itemByBarcode.Barcode?.Trim(), barcode, StringComparison.OrdinalIgnoreCase))
                {
                    multiUnit = new ItemMultiUnit
                    {
                        FK_ItemId = itemByBarcode.ItemId,
                        FK_UnitId = itemByBarcode.FK_UnitId ?? 1,
                        MultiUnitName = itemByBarcode.ItemName,
                        MultiUnitArabicName = itemByBarcode.ItemNameArabic,
                        Barcode = itemByBarcode.Barcode,
                        QTYPerUnit = 1,
                        Rate = itemByBarcode.SalesRate,
                        IncRate = itemByBarcode.InclusiveRate
                    };
                }
            }

            if (multiUnit != null)
            {
                if ((multiUnit.IncRate ?? 0) == 0 && (multiUnit.Rate ?? 0) > 0)
                {
                    var itemDetails = await _posDataService.GetItemByIdAsync(multiUnit.FK_ItemId);
                    if (itemDetails != null && (itemDetails.InclusiveRate ?? 0) > 0)
                    {
                        multiUnit.IncRate = itemDetails.InclusiveRate;
                    }
                }
                await AddMultiUnitToCartAsync(multiUnit);
            }
            else
            {
                bool createNew = false;
                if (ConfirmActionHandler != null)
                {
                    createNew = await ConfirmActionHandler.Invoke(
                        "Product Not Available",
                        $"The Product in the given Barcode ({barcode}) is not available.\n\nDo you want new product?");
                }
                else
                {
                    ShowNotificationHandler?.Invoke("Warning", $"The Product in the given Barcode ({barcode}) is not available.");
                }

                if (createNew && ShowItemMasterDialogHandler != null)
                {
                    var createdItem = await ShowItemMasterDialogHandler.Invoke(barcode);
                    if (createdItem != null)
                    {
                        await RefreshProductsAsync();

                        var newMultiUnit = await _posDataService.GetItemMultiUnitByBarcodeAsync(barcode)
                            ?? await _posDataService.GetItemMultiUnitByProductIdAsync(createdItem.ItemId)
                            ?? new ItemMultiUnit
                            {
                                FK_ItemId = createdItem.ItemId,
                                FK_UnitId = createdItem.FK_UnitId ?? 1,
                                MultiUnitName = createdItem.ItemName,
                                MultiUnitArabicName = createdItem.ItemNameArabic,
                                Barcode = createdItem.Barcode,
                                QTYPerUnit = 1,
                                Rate = createdItem.SalesRate,
                                IncRate = createdItem.InclusiveRate
                            };

                        await AddMultiUnitToCartAsync(newMultiUnit);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            ShowNotificationHandler?.Invoke("Error", $"Barcode error: {ex.Message}");
        }
        finally
        {
            FocusBarcodeHandler?.Invoke();
        }
    }

    [RelayCommand]
    public async Task LookUpProductAsync()
    {
        if (ShowItemSearchHandler != null)
        {
            var selected = await ShowItemSearchHandler.Invoke(null);
            if (selected != null)
            {
                var simulated = new ItemMultiUnit
                {
                    FK_ItemId = selected.ItemId,
                    FK_UnitId = selected.FK_UnitId ?? 1,
                    MultiUnitName = selected.ItemName,
                    MultiUnitArabicName = selected.ItemNameArabic,
                    Barcode = selected.Barcode,
                    QTYPerUnit = 1,
                    Rate = selected.SalesRate,
                    IncRate = selected.InclusiveRate
                };
                await AddMultiUnitToCartAsync(simulated);
            }
        }
        FocusBarcodeHandler?.Invoke();
    }

    #endregion

    #region Bill Actions (F1 - F12)

    [RelayCommand]
    public async Task NewBillAsync()
    {
        CartItems.Clear();
        SelectedCartItem = null;
        DiscountAmount = 0;
        DiscountPercentage = 0;
        Tendered = null;
        Changes = null;
        BarcodeSearchQuery = "";
        NumPadBuffer = "";
        ActiveInputField = "BarcodeSearch";

        int branchId = AppData.BranchId > 0 ? AppData.BranchId : 1;
        bool isAuto = PermanentSettingsData.AutoSalesInvoiceNo;
        InvoiceNo = await _posDataService.GetNextInvoiceNoAsync(branchId, isAuto);
        SalesDate = DateTime.Now;

        var defaultCust = CustomersList.FirstOrDefault(x => x.Name.Contains("Cash", StringComparison.OrdinalIgnoreCase))
                       ?? CustomersList.FirstOrDefault();
        if (defaultCust != null)
        {
            CustomerId = defaultCust.Id;
            CustomerName = defaultCust.Name;
        }

        RecalculateTotals();
        StatusMessage = "New bill started";
        FocusBarcodeHandler?.Invoke();
    }

    [RelayCommand]
    public async Task HoldBillAsync()
    {
        if (!CartItems.Any())
        {
            ShowNotificationHandler?.Invoke("Warning", "No items in bill to place on hold");
            return;
        }

        try
        {
            int branchId = AppData.BranchId > 0 ? AppData.BranchId : 1;
            int userId = AppData.UserId ?? 1;
            int fYearId = AppData.FYearId ?? 1;

            var holdHdr = new SalesInvoiceHoldHdr
            {
                SalesInvoiceNo = "HOLD-" + DateTime.Now.ToString("yyMMddHHmmss"),
                SalesInvoiceDate = SalesDate,
                RefNo = AppData.Terminal,
                FK_BranchId = branchId,
                FK_CustomerId = CustomerId > 0 ? CustomerId : 1,
                CustomerName = CustomerName,
                FK_UserId = userId,
                FK_FinancialYearId = fYearId,
                GrossAmount = GrossAmount,
                LineDsicount = DiscountAmount,
                TaxTotal = TaxAmount,
                NetAmount = GrandTotal,
                GrandTotal = GrandTotal
            };

            var holdDto = new SalesInvoiceHoldDTO(holdHdr, CartItems.ToList());
            var res = await _posDataService.HoldInvoiceAsync(holdDto);

            if (res.Id > 0)
            {
                await NewBillAsync();
                await RefreshHeldInvoicesAsync();
            }
            else
            {
                ShowNotificationHandler?.Invoke("Error", $"Failed to hold transaction: {res.Message}");
            }
        }
        catch (Exception ex)
        {
            ShowNotificationHandler?.Invoke("Error", $"Error holding bill: {ex.Message}");
        }
        finally
        {
            FocusBarcodeHandler?.Invoke();
        }
    }

    [RelayCommand]
    public async Task RetrieveBillAsync()
    {
        try
        {
            if (ShowHeldInvoicesHandler != null)
            {
                int? selectedHoldId = await ShowHeldInvoicesHandler.Invoke();
                if (selectedHoldId.HasValue && selectedHoldId.Value > 0)
                {
                    var holdDto = await _posDataService.GetHeldInvoiceByIdAsync(selectedHoldId.Value);
                    if (holdDto?.HoldHdr != null)
                    {
                        CartItems.Clear();
                        if (holdDto.HoldDtls != null)
                        {
                            int sl = 1;
                            foreach (var line in holdDto.HoldDtls)
                            {
                                line.SL = sl++;
                                CartItems.Add(line);
                            }
                        }

                        CustomerName = holdDto.HoldHdr.CustomerName ?? "Cash Customer";
                        CustomerId = holdDto.HoldHdr.FK_CustomerId;
                        DiscountAmount = holdDto.HoldHdr.LineDsicount;

                        // Delete the held transaction from database after retrieval
                        await _posDataService.DeleteHeldInvoiceAsync(selectedHoldId.Value);

                        RecalculateTotals();
                        SelectedCartItem = CartItems.LastOrDefault();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            ShowNotificationHandler?.Invoke("Error", $"Error retrieving held bill: {ex.Message}");
        }
        finally
        {
            await RefreshHeldInvoicesAsync();
            FocusBarcodeHandler?.Invoke();
        }
    }


    [RelayCommand]
    public async Task OpenDiscountDialogAsync()
    {
        if (ShowDiscountHandler != null)
        {
            var result = await ShowDiscountHandler.Invoke(DiscountAmount, GrossAmount);
            if (result.HasValue)
            {
                DiscountAmount = result.Value;
                RecalculateTotals();
            }
        }
        FocusBarcodeHandler?.Invoke();
    }

    [RelayCommand]
    public async Task SearchCustomerAsync()
    {
        if (ShowCustomerSearchHandler != null)
        {
            var customer = await ShowCustomerSearchHandler.Invoke();
            if (customer != null)
            {
                CustomerId = customer.CustomerId;
                CustomerName = customer.Name ?? "Customer";
                ShowNotificationHandler?.Invoke("Info", $"Customer Selected: {CustomerName}");
            }
        }
        FocusBarcodeHandler?.Invoke();
    }

    [RelayCommand]
    public async Task SearchBillAsync()
    {
        try
        {
            if (ShowBillSearchHandler != null)
            {
                int? selectedInvoiceId = await ShowBillSearchHandler.Invoke();
                if (selectedInvoiceId.HasValue && selectedInvoiceId.Value > 0)
                {
                    var invoiceDto = await _posDataService.GetInvoiceByIdAsync(selectedInvoiceId.Value);
                    if (invoiceDto?.InvoiceHdr != null)
                    {
                        CartItems.Clear();
                        if (invoiceDto.InvoiceDtls != null)
                        {
                            int sl = 1;
                            foreach (var line in invoiceDto.InvoiceDtls)
                            {
                                line.SL = sl++;
                                CartItems.Add(line);
                            }
                        }

                        CustomerName = invoiceDto.InvoiceHdr.CustomerName ?? "Cash Customer";
                        CustomerId = invoiceDto.InvoiceHdr.FK_CustomerId;
                        DiscountAmount = invoiceDto.InvoiceHdr.LineDsicount;
                        InvoiceNo = invoiceDto.InvoiceHdr.SalesInvoiceNo;

                        // Save reference to re-print or inspect
                        _lastSavedHdr = new TransactionHdr
                        {
                            HdrNo = invoiceDto.InvoiceHdr.SalesInvoiceNo,
                            HdrDate = invoiceDto.InvoiceHdr.SalesInvoiceDate ?? DateTime.Today,
                            InvoiceDate = (invoiceDto.InvoiceHdr.SalesInvoiceDate ?? DateTime.Today).ToString("dd-MM-yyyy"),
                            TotalQuantity = CartItems.Sum(x => x.Quantity),
                            PartyId = invoiceDto.InvoiceHdr.FK_CustomerId,
                            CustomerName = invoiceDto.InvoiceHdr.CustomerName,
                            GrossAmount = invoiceDto.InvoiceHdr.GrossAmount,
                            LineDsicount = invoiceDto.InvoiceHdr.LineDsicount,
                            TaxableAmount = invoiceDto.InvoiceHdr.TaxableAmount,
                            TaxTotal = invoiceDto.InvoiceHdr.TaxTotal,
                            NetAmount = invoiceDto.InvoiceHdr.NetAmount,
                            GrandTotal = invoiceDto.InvoiceHdr.GrandTotal,
                            Tendered = invoiceDto.InvoiceHdr.Tendered ?? invoiceDto.InvoiceHdr.GrandTotal,
                            Changes = invoiceDto.InvoiceHdr.Changes ?? 0,
                            QRCode = invoiceDto.InvoiceHdr.InvoiceQRCode,
                            InvoiceQRCode = invoiceDto.InvoiceHdr.InvoiceQRCode,
                            BillType = invoiceDto.InvoiceHdr.BillType ?? "Cash",
                            BillTypeArabic = GetBillTypeArabic(invoiceDto.InvoiceHdr.BillType ?? "Cash")
                        };
                        _lastSavedItems = CartItems.ToList();
                        _lastSavedParty = new Party
                        {
                            CustomerId = invoiceDto.InvoiceHdr.FK_CustomerId,
                            Name = invoiceDto.InvoiceHdr.CustomerName
                        };

                        RecalculateTotals();
                        SelectedCartItem = CartItems.LastOrDefault();
                        ShowNotificationHandler?.Invoke("Info", $"Loaded invoice {invoiceDto.InvoiceHdr.SalesInvoiceNo}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            ShowNotificationHandler?.Invoke("Error", $"Failed to load bill: {ex.Message}");
        }
        finally
        {
            FocusBarcodeHandler?.Invoke();
        }
    }
    private string GetBillTypeArabic(string billType)
    {
        return billType switch
        {
            "Cash" => "نقدي",
            "Bank" => "بطاقة",
            "Credit" => "آجل",
            _ => billType
        };
    }
    [RelayCommand]
    public async Task PrintBillAsync()
    {
        if (!CartItems.Any())
        {
            if (_lastSavedHdr != null && _lastSavedItems != null && _lastSavedItems.Any())
            {
                // Reprint last saved receipt if current cart is empty
                await PrintReceiptInternalAsync(_lastSavedHdr, _lastSavedItems, _lastSavedParty ?? new Party { CustomerId = _lastSavedHdr.PartyId, Name = _lastSavedHdr.CustomerName ?? "Cash Customer" });
                return;
            }

            ShowNotificationHandler?.Invoke("Warning", "No items to print");
            return;
        }

        var qr = GenerateInvoiceQRCode(GrandTotal, TaxAmount, InvoiceNo, SalesDate);
        var hdr = new TransactionHdr
        {
            HdrNo = InvoiceNo,
            HdrDate = SalesDate,
            InvoiceDate = SalesDate.ToString("dd-MM-yyyy"),
            TotalQuantity = TotalQty,
            GrossAmount = GrossAmount,
            LineDsicount = DiscountAmount,
            TaxableAmount = GrossAmount - DiscountAmount,
            TaxTotal = TaxAmount,
            NetAmount = GrandTotal,
            GrandTotal = GrandTotal,
            Tendered = Tendered ?? GrandTotal,
            Changes = Changes ?? 0m,
            BillType = "Cash",
            BillTypeArabic = GetBillTypeArabic("Cash"),
            PartyId = CustomerId,
            CustomerName = CustomerName,
            QRCode = qr,
            InvoiceQRCode = qr
        };

        var party = new Party
        {
            CustomerId = CustomerId,
            Name = CustomerName
        };

        await PrintReceiptInternalAsync(hdr, CartItems.ToList(), party);
    }

    private async Task PrintReceiptInternalAsync(TransactionHdr hdr, List<TransactionDtls> items, Party party)
    {
        if (items == null || !items.Any())
        {
            ShowNotificationHandler?.Invoke("Warning", "No items to print");
            return;
        }

        try
        {
            var design = await _posDataService.GetPrintDesignAsync("Sales_Invoice_Thermal");
            if (design == null)
            {
                ShowNotificationHandler?.Invoke("Error", "Thermal print design not found");
                return;
            }

            var company = AppData.CompanyDetails ?? new CompanyDetails
            {
                Name = "ZAKI ERP STORE",
                VatNo = "300000000000003"
            };

            var printDto = new PrintParameterDTO(design.hdr, design.dtls, hdr, items, party, company, null);
            _printHelper.PrintInvoice(printDto);
            ShowNotificationHandler?.Invoke("Success", "Receipt sent to printer");
        }
        catch (Exception ex)
        {
            ShowNotificationHandler?.Invoke("Error", $"Print error: {ex.Message}");
        }
        finally
        {
            FocusBarcodeHandler?.Invoke();
        }
    }

    [RelayCommand]
    public async Task PdfBillAsync()
    {
        await PrintBillAsync();
    }

    [RelayCommand]
    public async Task OpenPaymentDialogAsync()
    {
        if (!CartItems.Any())
        {
            ShowNotificationHandler?.Invoke("Warning", "No items in bill to pay");
            return;
        }

        if (ShowPaymentDialogHandler != null)
        {
            var (success, paymentType, tenderVal, changeVal) = await ShowPaymentDialogHandler.Invoke(GrandTotal);
            if (success)
            {
                Tendered = tenderVal;
                Changes = changeVal;
                await SaveInvoiceAsync(paymentType, tenderVal, changeVal);
            }
        }
        FocusBarcodeHandler?.Invoke();
    }

    [RelayCommand]
    public async Task PayCardQuickAsync()
    {
        if (!CartItems.Any())
        {
            ShowNotificationHandler?.Invoke("Warning", "No items in bill to pay");
            return;
        }

        await SaveInvoiceAsync("Bank", GrandTotal, 0m);
        FocusBarcodeHandler?.Invoke();
    }

    private async Task SaveInvoiceAsync(string paymentType, decimal tenderVal, decimal changeVal)
    {
        IsBusy = true;
        StatusMessage = "Saving invoice...";

        try
        {
            int branchId = AppData.BranchId > 0 ? AppData.BranchId : 1;
            int userId = AppData.UserId ?? 1;
            int fYearId = AppData.FYearId ?? 1;

            var salesHdr = new SalesInvoiceHdr
            {
                SalesInvoiceNo = InvoiceNo,
                SalesInvoiceDate = SalesDate,
                RefNo = AppData.Terminal,
                EntryDate = DateTime.Now,
                FK_BranchId = branchId,
                FK_UserId = userId,
                FK_FinancialYearId = fYearId,
                FK_CustomerId = CustomerId > 0 ? CustomerId : 1,
                CustomerName = CustomerName,
                BillType = paymentType,
                GrossAmount = GrossAmount,
                LineDsicount = DiscountAmount,
                TaxableAmount = GrossAmount - DiscountAmount,
                TaxTotal = TaxAmount,
                NetAmount = GrandTotal,
                GrandTotal = GrandTotal,
                Tendered = tenderVal,
                Changes = changeVal,
                Remarks = ""
            };

            var company = AppData.CompanyDetails ?? new CompanyDetails
            {
                Name = "ZAKI ERP STORE",
                VatNo = "300000000000003"
            };

            // Snapshot the invoice items and details before any async save / clear
            var savedItems = CartItems.ToList();
            var savedParty = new Party
            {
                CustomerId = salesHdr.FK_CustomerId,
                Name = salesHdr.CustomerName
            };

            var qr = GenerateInvoiceQRCode(salesHdr.GrandTotal, salesHdr.TaxTotal, salesHdr.SalesInvoiceNo, salesHdr.SalesInvoiceDate);
            var savedHdr = new TransactionHdr
            {
                HdrNo = salesHdr.SalesInvoiceNo,
                HdrDate = salesHdr.SalesInvoiceDate ?? DateTime.Today,
                InvoiceDate = (salesHdr.SalesInvoiceDate ?? DateTime.Today).ToString("dd-MM-yyyy"),
                TotalQuantity = savedItems.Sum(x => x.Quantity),
                GrossAmount = salesHdr.GrossAmount,
                LineDsicount = salesHdr.LineDsicount,
                TaxableAmount = salesHdr.TaxableAmount,
                TaxTotal = salesHdr.TaxTotal,
                NetAmount = salesHdr.NetAmount,
                GrandTotal = salesHdr.GrandTotal,
                Tendered = salesHdr.Tendered ?? salesHdr.GrandTotal,
                Changes = salesHdr.Changes ?? 0m,
                BillType = salesHdr.BillType,
                BillTypeArabic = GetBillTypeArabic(salesHdr.BillType ?? "Cash"),
                PartyId = salesHdr.FK_CustomerId,
                CustomerName = salesHdr.CustomerName,
                QRCode = qr,
                InvoiceQRCode = qr
            };

            // Keep reference for reprinting
            _lastSavedHdr = savedHdr;
            _lastSavedItems = savedItems;
            _lastSavedParty = savedParty;

            var dto = new SalesInvoiceDTO(salesHdr, savedItems, company, PermanentSettingsData.ZatcaPhase, true, PermanentSettingsData.UploadSimplifiedXMLRealtime);
            var result = await _posDataService.SaveInvoiceAsync(dto);

            if (result.IsSuccess)
            {
                ShowNotificationHandler?.Invoke("Success", string.IsNullOrEmpty(result.Message) ? "Invoice saved successfully" : result.Message);

                // Record sale in shift
                if (PermanentSettingsData.EnableShiftClosing)
                {
                    var user = !string.IsNullOrWhiteSpace(AppData.UserName) ? AppData.UserName : "Cashier";
                    await _shiftService.RecordSaleAsync(salesHdr.GrandTotal, paymentType, false, "Terminal 1", user);
                }

                // Auto print receipt if enabled - await receipt generation BEFORE resetting bill
                if (Settings.AutoPrintBill)
                {
                    await PrintReceiptInternalAsync(savedHdr, savedItems, savedParty);
                }

                // Refresh ZATCA Pending Count
                _ = RefreshZatcaPendingAsync();

                // Start next bill only after printing completes
                await NewBillAsync();
            }
            else
            {
                ShowNotificationHandler?.Invoke("Error", $"Save Error: {result.Message}");
            }
        }
        catch (Exception ex)
        {
            ShowNotificationHandler?.Invoke("Error", $"Save Exception: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            FocusBarcodeHandler?.Invoke();
        }
    }

    [RelayCommand]
    public async Task OpenCashDrawerAsync()
    {
        try
        {
            await _printHelper.OpenCashDrawer();
            ShowNotificationHandler?.Invoke("Success", "Cash drawer signal sent");
        }
        catch (Exception ex)
        {
            ShowNotificationHandler?.Invoke("Error", $"Cash drawer error: {ex.Message}");
        }
        finally
        {
            FocusBarcodeHandler?.Invoke();
        }
    }

    private string GenerateInvoiceQRCode() => GenerateInvoiceQRCode(GrandTotal, TaxAmount, InvoiceNo, SalesDate);

    private string GenerateInvoiceQRCode(decimal grandTotal, decimal taxAmount, string? invoiceNo = null, DateTime? salesDate = null)
    {
        try
        {
            string seller = AppData.CompanyDetails?.Name ?? "ZAKI ERP";
            string vatNo = AppData.CompanyDetails?.VatNo ?? "300000000000003";
            string time = (salesDate ?? DateTime.Now).ToString("yyyy-MM-ddTHH:mm:ssZ");
            string total = grandTotal.ToString("F2", CultureInfo.InvariantCulture);
            string tax = taxAmount.ToString("F2", CultureInfo.InvariantCulture);

            byte[] bSeller = Encoding.UTF8.GetBytes(seller);
            byte[] bVat = Encoding.UTF8.GetBytes(vatNo);
            byte[] bTime = Encoding.UTF8.GetBytes(time);
            byte[] bTotal = Encoding.UTF8.GetBytes(total);
            byte[] bTax = Encoding.UTF8.GetBytes(tax);

            using var ms = new MemoryStream();
            void WriteTlv(byte tag, byte[] val)
            {
                ms.WriteByte(tag);
                ms.WriteByte((byte)val.Length);
                ms.Write(val, 0, val.Length);
            }

            WriteTlv(1, bSeller);
            WriteTlv(2, bVat);
            WriteTlv(3, bTime);
            WriteTlv(4, bTotal);
            WriteTlv(5, bTax);

            return Convert.ToBase64String(ms.ToArray());
        }
        catch
        {
            return $"{invoiceNo ?? InvoiceNo}|{grandTotal:F2}|{(salesDate ?? DateTime.Now):yyyy-MM-dd HH:mm}";
        }
    }

    #endregion

    #region NumPad Interaction

    [RelayCommand]
    public void ToggleNumPad()
    {
        ShowNumPad = !ShowNumPad;
        FocusBarcodeHandler?.Invoke();
    }

    public void SetActiveField(string fieldName)
    {
        ActiveInputField = fieldName;
        NumPadBuffer = "";
        _replaceBufferOnInput = false;

        if (fieldName == "BarcodeSearch")
        {
            NumPadBuffer = BarcodeSearchQuery;
        }
        else if (fieldName == "Discount")
        {
            NumPadBuffer = DiscountAmount > 0 ? DiscountAmount.ToString("F2") : "";
            _replaceBufferOnInput = true;
        }
    }

    public void SetActiveCell(TransactionDtls row, string fieldName)
    {
        SelectedCartItem = row;
        ActiveInputField = fieldName;
        NumPadBuffer = "";
        _replaceBufferOnInput = true;

        if (fieldName == "Qty")
        {
            NumPadBuffer = row.Quantity.ToString("G29");
        }
        else if (fieldName == "Rate")
        {
            NumPadBuffer = row.ExRate.ToString("F2");
        }
        else if (fieldName == "IncRate")
        {
            NumPadBuffer = row.InRate.ToString("F2");
        }
        else if (fieldName == "Disc%")
        {
            NumPadBuffer = row.DiscountPerc.ToString("F2");
        }
        else if (fieldName == "DiscAmt")
        {
            NumPadBuffer = row.DiscountAmnt.ToString("F2");
        }
    }

    private string GetPreferredRateField()
    {
        if (Settings.ShowIncRate && Settings.AllowEditIncRate && (!Settings.ShowExRate || !Settings.AllowEditExcRate))
        {
            return "IncRate";
        }
        if (Settings.ShowExRate && Settings.AllowEditExcRate)
        {
            return "Rate";
        }
        if (Settings.ShowIncRate)
        {
            return "IncRate";
        }
        return "Rate";
    }

    [RelayCommand]
    public async Task HandleNumPadKey(string key)
    {
        if (string.IsNullOrEmpty(ActiveInputField))
        {
            ActiveInputField = "BarcodeSearch";
        }

        if (key == "Clear")
        {
            NumPadBuffer = "";
            UpdateActiveFieldFromBuffer();
            ActiveBufferUpdatedHandler?.Invoke(NumPadBuffer);
            FocusBarcodeHandler?.Invoke();
            return;
        }

        if (key == "Enter")
        {
            await CommitActiveField();
            return;
        }

        if (key == "Qty")
        {
            if (SelectedCartItem == null && CartItems.Any())
            {
                SelectedCartItem = CartItems.LastOrDefault();
            }
            if (SelectedCartItem != null)
            {
                SetActiveCell(SelectedCartItem, "Qty");
            }
            FocusCartCellHandler?.Invoke("Qty");
            return;
        }

        if (key == "Rate")
        {
            if (SelectedCartItem == null && CartItems.Any())
            {
                SelectedCartItem = CartItems.LastOrDefault();
            }
            if (SelectedCartItem != null)
            {
                string rateField = GetPreferredRateField();
                SetActiveCell(SelectedCartItem, rateField);
            }
            FocusCartCellHandler?.Invoke("Rate");
            return;
        }

        if (key == "Backspace")
        {
            if (_replaceBufferOnInput)
            {
                NumPadBuffer = "";
                _replaceBufferOnInput = false;
            }
            else if (NumPadBuffer.Length > 0)
            {
                NumPadBuffer = NumPadBuffer[..^1];
            }
            UpdateActiveFieldFromBuffer();
            ActiveBufferUpdatedHandler?.Invoke(NumPadBuffer);
            return;
        }

        if (_replaceBufferOnInput && key != "Enter" && key != "Clear" && key != "Qty" && key != "Rate")
        {
            NumPadBuffer = "";
            _replaceBufferOnInput = false;
        }

        if (key == "." && NumPadBuffer.Contains('.'))
        {
            return;
        }

        if (NumPadBuffer == "0" && key != ".")
        {
            NumPadBuffer = "";
        }

        NumPadBuffer += key;
        UpdateActiveFieldFromBuffer();
        ActiveBufferUpdatedHandler?.Invoke(NumPadBuffer);
    }

    private void UpdateActiveFieldFromBuffer()
    {
        decimal.TryParse(NumPadBuffer, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal parsedVal);

        if (ActiveInputField == "BarcodeSearch")
        {
            BarcodeSearchQuery = NumPadBuffer;
        }
        else if (ActiveInputField == "Qty" && SelectedCartItem != null)
        {
            SelectedCartItem.Quantity = parsedVal;
            TransactionHelper.GetTransactionAfterEdit(SelectedCartItem, Settings.ShowIncRate);
            RecalculateTotals();
        }
        else if (ActiveInputField == "Rate" && SelectedCartItem != null)
        {
            SelectedCartItem.ExRate = parsedVal;
            UpdateInRate(SelectedCartItem);
            TransactionHelper.GetTransactionAfterEdit(SelectedCartItem, Settings.ShowIncRate);
            RecalculateTotals();
        }
        else if (ActiveInputField == "IncRate" && SelectedCartItem != null)
        {
            SelectedCartItem.InRate = parsedVal;
            UpdateExRate(SelectedCartItem);
            TransactionHelper.GetTransactionAfterEdit(SelectedCartItem, true);
            RecalculateTotals();
        }
        else if (ActiveInputField == "Disc%" && SelectedCartItem != null)
        {
            SelectedCartItem.DiscountPerc = parsedVal;
            TransactionHelper.GetTransactionAfterEdit(SelectedCartItem, Settings.ShowIncRate);
            RecalculateTotals();
        }
        else if (ActiveInputField == "DiscAmt" && SelectedCartItem != null)
        {
            SelectedCartItem.DiscountAmnt = parsedVal;
            if (SelectedCartItem.GrossAmount > 0)
            {
                SelectedCartItem.DiscountPerc = Math.Round((parsedVal / SelectedCartItem.GrossAmount) * 100m, 4, MidpointRounding.AwayFromZero);
            }
            else
            {
                SelectedCartItem.DiscountPerc = 0;
            }
            TransactionHelper.GetTransactionAfterEdit(SelectedCartItem, Settings.ShowIncRate, true);
            RecalculateTotals();
        }
        else if (ActiveInputField == "Discount")
        {
            DiscountAmount = parsedVal;
            RecalculateTotals();
        }
    }

    private async Task CommitActiveField()
    {
        _replaceBufferOnInput = false;
        if (ActiveInputField == "BarcodeSearch")
        {
            if (!string.IsNullOrEmpty(BarcodeSearchQuery))
            {
                await ProcessBarcodeAsync();
            }
        }
        else
        {
            ActiveInputField = "BarcodeSearch";
            NumPadBuffer = "";
            RecalculateTotals();
        }
        FocusBarcodeHandler?.Invoke();
    }

    #endregion

    #region Settings, Customer Display, ZATCA, & Shift Commands

    public void UpdateCustomerDisplay()
    {
        try
        {
            var dto = new CustomerDisplayDto
            {
                Items = CartItems.Select(x => new SaleItem
                {
                    ItemName = x.ItemName,
                    Qty = x.Quantity,
                    Rate = x.InRate > 0 ? x.InRate : x.ExRate,
                    Total = x.LineTotal
                }).ToList()
            };
            _ = _customerDisplayService.UpdateAsync(dto);
        }
        catch { }
    }

    [RelayCommand]
    public async Task ToggleCustomerDisplayAsync()
    {
        if (_customerDisplayService.IsOpen)
        {
            await _customerDisplayService.CloseAsync();
            IsCustomerDisplayOpen = false;
        }
        else
        {
            await _customerDisplayService.OpenAsync();
            IsCustomerDisplayOpen = true;
            UpdateCustomerDisplay();
        }
    }

    [RelayCommand]
    public async Task RefreshHeldInvoicesAsync()
    {
        try
        {
            int branchId = AppData.BranchId > 0 ? AppData.BranchId : 1;
            var list = await _posDataService.GetHeldInvoicesAsync(branchId, AppData.Terminal);
            HeldInvoicesCount = list?.Count ?? 0;
            HasHeldInvoices = HeldInvoicesCount > 0;
            OnPropertyChanged(nameof(HeldCountBadgeBackground));
        }
        catch
        {
            HeldInvoicesCount = 0;
            HasHeldInvoices = false;
            OnPropertyChanged(nameof(HeldCountBadgeBackground));
        }
    }

    [RelayCommand]
    public async Task RefreshZatcaPendingAsync()
    {
        if (!ZakiSAASERP.Models.Services.PermanentSettingsData.IsZatca)
        {
            ZatcaPendingCount = 0;
            HasZatcaPending = false;
            OnPropertyChanged(nameof(IsZatcaEnabled));
            return;
        }

        try
        {
            ZatcaPendingCount = await _zatcaRepo.GetFailedCount();
            HasZatcaPending = ZatcaPendingCount > 0;
            OnPropertyChanged(nameof(IsZatcaEnabled));
        }
        catch
        {
            ZatcaPendingCount = 0;
            HasZatcaPending = false;
        }
    }

    [RelayCommand]
    public async Task OpenZatcaPendingDialogAsync()
    {
        await RefreshZatcaPendingAsync();
    }

    [RelayCommand]
    public async Task OpenSettingsDialogAsync()
    {
        if (ShowSettingsDialogHandler != null)
        {
            bool saved = await ShowSettingsDialogHandler.Invoke();
            if (saved)
            {
                Settings = _settingsService.CurrentSettings;
                if (!Settings.ShowCategoryPanel)
                {
                    SelectedCategory = null;
                    FilterProducts();
                }
                ShowNumPad = Settings.ShowNumPadByDefault;
                OnPropertyChanged(nameof(Settings));
            }
        }
    }

    [RelayCommand]
    public async Task OpenShiftDialogAsync()
    {
        if (ShowShiftDialogHandler != null)
        {
            bool changed = await ShowShiftDialogHandler.Invoke();
            if (changed)
            {
                StatusMessage = "New shift started.";
                ShowNotificationHandler?.Invoke("Success", "Shift closed and new shift started.");
            }
        }
    }

    #endregion
}

