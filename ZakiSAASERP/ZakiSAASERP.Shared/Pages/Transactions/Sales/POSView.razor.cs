using ZakiSAASERP.Shared.SubWindows;
using ZakiSAASERP.Models.Settings;
using ZakiSAASERP.Models.Others;
using Microsoft.AspNetCore.Components.Web;

namespace ZakiSAASERP.Shared.Pages.Transactions.Sales;

public partial class POSView : ITabAware, IDisposable
{
    #region Variables
    [Parameter] public int TabId { get; set; }

    private POSSettings posSettings = new();
    private List<TransactionDtls> entTransactions = new();
    private List<ExMasters> products = new();
    private List<CategoryMaster> categories = new();
    private CategoryMaster? selectedCategory;
    private List<CommonModel> customerList = new();
    private List<CommonModel> unitlist = new();
    private List<CommonModel> salesmanList = new();

    private SalesInvoiceHdr salesInvoice = new();
    private WebUserPrivilege? privilege;
    private PrintDesignDTO? pdto;
    private Party party = new();
    private MudTextField<string>? txtInvoiceNo;
    private MudTextField<string>? txtBarcode;
    public string _barcodeSearchQuery { get; set; } = string.Empty;
    private string _numPadBuffer = "";
    private string _activeInputField = "BarcodeSearch";
    private bool _isProcessingBarcode = false;
    private TransactionDtls? _selectedRow;
    private decimal _totalQty = 0;

    private bool _shouldFocusRate = false;
    private string _focusTargetId = "";
    private bool _showNumPad = true;
    private bool _forceShowNumPad = false;
    private bool _isShortScreen = false;
    private Microsoft.JSInterop.DotNetObjectReference<POSView>? _dotNetRef;
    private bool _isActionInProgress = false;
    private bool _isFullScreen = false;

    private bool _isSaving = false;
    string strQuantityFormat => PermanentSettingsData.QuantityFormat;
    string strCurencyFormat => PermanentSettingsData.CurrencyFormat;
    private bool IsAutoCode => PermanentSettingsData.AutoSalesInvoiceNo;
    private int _mobileStep = 1;
    private int _productPage = 1;
    private readonly Dictionary<int, string> _categoryImageCache = new();
    private readonly Dictionary<int, string> _productImageCache = new();
    private int heldInvoicesCount = 0;
    #endregion

    #region Lifecycle Methods
    protected override async Task OnInitializedAsync()
    {
        try
        {
            if (PosSettingsService != null)
            {
                posSettings = PosSettingsService.CurrentSettings ?? new();
                PosSettingsService.OnSettingsChanged += HandleSettingsChanged;
                if (posSettings.FillFullScreen) _isFullScreen = true;
            }
            else
            {
                posSettings = new();
            }

            if (AppData.privileges is not null)
            {
                privilege = AppData.privileges.FirstOrDefault(x =>
                    string.Equals(x.Caption, "POS", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(x.Caption, "POSView", StringComparison.OrdinalIgnoreCase))
                    ?? AppData.privileges.FirstOrDefault(x => string.Equals(x.Caption, "SalesInvoiceTitle", StringComparison.OrdinalIgnoreCase));
            }

            customerList = await api.GetCombofill("Customers", AppData.BranchId);
            unitlist = await api.GetCombofill("Unit", 1);
            salesmanList = await api.GetCombofill("SalesMan", 1);
            products = await api.GetMasters("posproduct", 1);
            categories = await api.GetAllCategory();
            if (selectedCategory == null && categories != null && categories.Any())
            {
                selectedCategory = categories.FirstOrDefault();
            }

            await NewClick();
            await RefreshHeldInvoicesCountAsync();
            StateHasChanged();
        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod($"Initialization Error: {ex.Message}");
        }
    }

    private void HandleSettingsChanged()
    {
        posSettings = PosSettingsService.CurrentSettings;
        _isFullScreen = posSettings.FillFullScreen;
        if (!posSettings.ShowCategoryPanel)
        {
            selectedCategory = null;
        }
        _productPage = 1;
        InvokeAsync(StateHasChanged);
    }
    public void Dispose()
    {
        if (PosSettingsService != null)
        {
            PosSettingsService.OnSettingsChanged -= HandleSettingsChanged;
        }
        try
        {
            _ = js.InvokeVoidAsync("unregisterPosShortcuts");
            _ = js.InvokeVoidAsync("unregisterPosGlobalFocusHandler");
        }
        catch { }
        _dotNetRef?.Dispose();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                if (PosSettingsService != null)
                {
                    posSettings = await PosSettingsService.LoadSettingsAsync() ?? new();
                }
                var isShort = await js.InvokeAsync<bool>("eval", "window.innerHeight < 820");
                _isShortScreen = isShort;
                _showNumPad = posSettings.ShowNumPadByDefault && !isShort;
                StateHasChanged();

                _dotNetRef = Microsoft.JSInterop.DotNetObjectReference.Create(this);
                await js.InvokeVoidAsync("eval", @"
                    window.registerResizeListener = (dotNetHelper) => {
                        window.addEventListener('resize', () => {
                            dotNetHelper.invokeMethodAsync('OnWindowResize', window.innerHeight);
                        });
                    };
                ");
                await js.InvokeVoidAsync("registerResizeListener", _dotNetRef);
                await js.InvokeVoidAsync("registerPosShortcuts", _dotNetRef);
                await js.InvokeVoidAsync("initPosGlobalFocusHandler");
            }
            catch (Exception)
            {
                // Fallback in case JS is not ready or fails
            }

            await Task.Delay(300);
            await FocusBarcodeInputAsync();
        }
        else if (_shouldFocusRate && !string.IsNullOrEmpty(_focusTargetId))
        {
            _shouldFocusRate = false;
            var target = _focusTargetId;
            _focusTargetId = "";
            await Task.Delay(50);
            await js.InvokeVoidAsync("selectTextById", target);
        }
    }

    private async Task FocusBarcodeInputAsync()
    {
        _activeInputField = "BarcodeSearch";
        if (txtBarcode != null)
        {
            try
            {
                await txtBarcode.FocusAsync();
            }
            catch { }
        }
    }

    private async Task ClearBarcodeInputAsync()
    {
        _barcodeSearchQuery = string.Empty;
        _numPadBuffer = string.Empty;
        if (txtBarcode != null)
        {
            try
            {
                await txtBarcode.Clear();
                await txtBarcode.SetText(string.Empty);
            }
            catch { }
        }
        try
        {
            await js.InvokeVoidAsync("clearPosBarcodeInputs");
        }
        catch { }
        await InvokeAsync(StateHasChanged);
    }

    private async Task OnInvoiceNoKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await FocusBarcodeInputAsync();
        }
    }
    #endregion

    #region POS Keyboard & Input Actions
    private void ToggleNumPad()
    {
        if (_showNumPad)
        {
            _showNumPad = false;
            _forceShowNumPad = false;
        }
        else
        {
            _showNumPad = true;
            if (_isShortScreen)
            {
                _forceShowNumPad = true;
            }
        }
        _ = FocusBarcodeInputAsync();
    }

    private async Task ToggleFullScreen()
    {
        _isFullScreen = !_isFullScreen;
        try
        {
            if (_isFullScreen)
            {
                await js.InvokeVoidAsync("openFullscreen");
            }
            else
            {
                await js.InvokeVoidAsync("closeFullscreen");
            }
        }
        catch { }
        await InvokeAsync(StateHasChanged);
    }

    [Microsoft.JSInterop.JSInvokable]
    public void OnWindowResize(int height)
    {
        var isShort = height < 820;
        if (isShort != _isShortScreen)
        {
            _isShortScreen = isShort;
            if (isShort && !_forceShowNumPad)
            {
                _showNumPad = false;
            }
            else if (!isShort && !_forceShowNumPad)
            {
                _showNumPad = true;
            }
            InvokeAsync(StateHasChanged);
        }
    }

    [Microsoft.JSInterop.JSInvokable]
    public async Task OnPosShortcutPressed(string key)
    {
        if (_isSaving || _isActionInProgress) return;
        try
        {
            _isActionInProgress = true;
            switch (key?.ToUpperInvariant())
            {
                case "F1":
                    await NewClick();
                    break;
                case "F2":
                    HoldBillClick();
                    break;
                case "F3":
                    RetrieveBillClick();
                    break;
                case "F4":
                    await SearchProductLookUp();
                    break;
                case "F5":
                    await OpenDiscountDialog();
                    break;
                case "F6":
                    await SearchCustomerClick();
                    break;
                case "F7":
                    await SearchBillClick();
                    break;
                case "F8":
                    await PrintBillClick();
                    break;
                case "F9":
                    await PdfBillClick();
                    break;
                case "F10":
                    await OpenPaymentDialog();
                    break;
                case "F11":
                    await OpenPaymentDialog();
                    break;
                case "F12":
                    await SaveInvoiceClick("Bank");
                    break;
            }
        }
        catch (Exception ex)
        {
            snackbar.Add($"Shortcut action error: {ex.Message}", Severity.Error);
        }
        finally
        {
            _isActionInProgress = false;
            await FocusBarcodeInputAsync();
        }
    }

    private void SetActiveField(string fieldName)
    {
        _activeInputField = fieldName;
        _numPadBuffer = "";

        if (fieldName == "BarcodeSearch")
        {
            _numPadBuffer = _barcodeSearchQuery;
        }
        else if (fieldName == "Discount")
        {
            _numPadBuffer = salesInvoice.AdditionalDiscAmnt?.ToString() ?? "";
        }

        StateHasChanged();
    }

    private void SetActiveCell(TransactionDtls row, string fieldName)
    {
        _selectedRow = row;
        _activeInputField = fieldName;
        _numPadBuffer = "";

        if (fieldName == "Qty")
        {
            _numPadBuffer = row.Quantity.ToString("F2");
        }
        else if (fieldName == "Rate")
        {
            _numPadBuffer = row.ExRate.ToString("F2");
            _ = js.InvokeVoidAsync("selectTextById", $"rate-input-{row.SL}");
        }
        else if (fieldName == "IncRate")
        {
            _numPadBuffer = row.InRate.ToString("F2");
            _ = js.InvokeVoidAsync("selectTextById", $"incrate-input-{row.SL}");
        }
        else if (fieldName == "Disc%")
        {
            _numPadBuffer = row.DiscountPerc.ToString("F2");
            _ = js.InvokeVoidAsync("selectTextById", $"disc-input-{row.SL}");
        }
        else if (fieldName == "DiscAmt")
        {
            _numPadBuffer = row.DiscountAmnt.ToString("F2");
            _ = js.InvokeVoidAsync("selectTextById", $"discamt-input-{row.SL}");
        }

        StateHasChanged();
    }

    private bool IsFieldActive(string fieldName)
    {
        return _activeInputField == fieldName;
    }

    private bool IsCellActive(TransactionDtls row, string fieldName)
    {
        return _selectedRow == row && _activeInputField == fieldName;
    }

    private async Task HandleNumPadKey(string key)
    {
        if (string.IsNullOrEmpty(_activeInputField))
        {
            _activeInputField = "BarcodeSearch";
        }

        if (key == "Clear")
        {
            _numPadBuffer = "";
            UpdateActiveFieldFromBuffer();
            if (_activeInputField == "BarcodeSearch")
            {
                await ClearBarcodeInputAsync();
            }
            await FocusBarcodeInputAsync();
            return;
        }

        if (key == "Enter")
        {
            await CommitActiveField();
            return;
        }

        if (key == "Backspace")
        {
            if (_numPadBuffer.Length > 0)
            {
                _numPadBuffer = _numPadBuffer.Substring(0, _numPadBuffer.Length - 1);
            }
            UpdateActiveFieldFromBuffer();
            return;
        }

        if (key == "Qty")
        {
            if (_selectedRow != null)
            {
                SetActiveCell(_selectedRow, "Qty");
            }
            return;
        }

        if (key == "Rate")
        {
            if (_selectedRow != null)
            {
                SetActiveCell(_selectedRow, "Rate");
            }
            return;
        }

        // Handle decimal point safety
        if (key == "." && _numPadBuffer.Contains("."))
        {
            return;
        }

        // Clear default zero if just typing digit
        if (_numPadBuffer == "0" && key != ".")
        {
            _numPadBuffer = "";
        }

        _numPadBuffer += key;
        UpdateActiveFieldFromBuffer();
    }

    private void UpdateActiveFieldFromBuffer()
    {
        var valStr = string.IsNullOrEmpty(_numPadBuffer) ? "0" : _numPadBuffer;
        decimal.TryParse(valStr, out decimal parsedVal);

        if (_activeInputField == "BarcodeSearch")
        {
            _barcodeSearchQuery = _numPadBuffer;
        }
        else if (_activeInputField == "Qty" && _selectedRow != null)
        {
            _selectedRow.Quantity = parsedVal;
            _selectedRow = TransactionHelper.GetTransactionAfterEdit(_selectedRow, posSettings.ShowIncRate);
            _ = TotalFindMethod();
        }
        else if (_activeInputField == "Rate" && _selectedRow != null)
        {
            _selectedRow.ExRate = parsedVal;
            UpdateInRate(_selectedRow);
            _selectedRow = TransactionHelper.GetTransactionAfterEdit(_selectedRow, posSettings.ShowIncRate);
            _ = TotalFindMethod();
        }
        else if (_activeInputField == "IncRate" && _selectedRow != null)
        {
            _selectedRow.InRate = parsedVal;
            UpdateExRate(_selectedRow);
            _selectedRow = TransactionHelper.GetTransactionAfterEdit(_selectedRow, true);
            _ = TotalFindMethod();
        }
        else if (_activeInputField == "Disc%" && _selectedRow != null)
        {
            _selectedRow.DiscountPerc = parsedVal;
            _selectedRow = TransactionHelper.GetTransactionAfterEdit(_selectedRow, posSettings.ShowIncRate);
            _ = TotalFindMethod();
        }
        else if (_activeInputField == "DiscAmt" && _selectedRow != null)
        {
            _selectedRow.DiscountAmnt = parsedVal;
            if (_selectedRow.GrossAmount > 0)
            {
                _selectedRow.DiscountPerc = Math.Round((parsedVal / _selectedRow.GrossAmount) * 100m, 4, MidpointRounding.AwayFromZero);
            }
            _selectedRow = TransactionHelper.GetTransactionAfterEdit(_selectedRow, posSettings.ShowIncRate);
            _selectedRow.DiscountAmnt = parsedVal;
            _selectedRow.TaxableAmnt = _selectedRow.GrossAmount - _selectedRow.DiscountAmnt;
            var taxRate = _selectedRow.TaxPerc > 0 ? _selectedRow.TaxPerc : 0m;
            _selectedRow.TaxAmount = Math.Round(_selectedRow.TaxableAmnt * (taxRate / 100m), 2, MidpointRounding.AwayFromZero);
            _selectedRow.LineTotal = _selectedRow.TaxableAmnt + _selectedRow.TaxAmount;
            _ = TotalFindMethod();
        }
        else if (_activeInputField == "Discount")
        {
            salesInvoice.AdditionalDiscAmnt = parsedVal;
            _ = TotalFindMethod();
        }
    }

    private async Task CommitActiveField()
    {
        if (_activeInputField == "BarcodeSearch")
        {
            if (_isProcessingBarcode) return;
            if (!string.IsNullOrWhiteSpace(_barcodeSearchQuery))
            {
                var barcode = _barcodeSearchQuery.Trim();
                _isProcessingBarcode = true;
                try
                {
                    await ClearBarcodeInputAsync();
                    await Task.Yield();
                    await ProcessBarcode(barcode);
                }
                finally
                {
                    await ClearBarcodeInputAsync();
                    _isProcessingBarcode = false;
                }
            }
        }
        else
        {
            if (_activeInputField == "Rate" && _selectedRow != null)
            {
                UpdateInRate(_selectedRow);
                _selectedRow = TransactionHelper.GetTransactionAfterEdit(_selectedRow, posSettings.ShowIncRate);
            }
            else if (_activeInputField == "IncRate" && _selectedRow != null)
            {
                UpdateExRate(_selectedRow);
                _selectedRow = TransactionHelper.GetTransactionAfterEdit(_selectedRow, true);
            }
            else if (_activeInputField == "Disc%" && _selectedRow != null)
            {
                _selectedRow = TransactionHelper.GetTransactionAfterEdit(_selectedRow, posSettings.ShowIncRate);
            }
            else if (_activeInputField == "DiscAmt" && _selectedRow != null)
            {
                if (_selectedRow.GrossAmount > 0)
                {
                    _selectedRow.DiscountPerc = Math.Round((_selectedRow.DiscountAmnt / _selectedRow.GrossAmount) * 100m, 4, MidpointRounding.AwayFromZero);
                }
                _selectedRow = TransactionHelper.GetTransactionAfterEdit(_selectedRow, posSettings.ShowIncRate, true);
            }
            // Reset focus to barcode search after committing a cell change
            _activeInputField = "BarcodeSearch";
            _numPadBuffer = "";
        }
        await TotalFindMethod();
        await FocusBarcodeInputAsync();
    }
    private async void OnBlurQtEvent(FocusEventArgs arg, TransactionDtls row)
    {
        _selectedRow = row;
        if (_selectedRow == null || _selectedRow.FK_ItemId == 0) { return; }
        if (_selectedRow.InRate == 0 && _selectedRow.ExRate > 0)
        {
            UpdateInRate(_selectedRow);
        }
        _selectedRow = TransactionHelper.GetTransactionAfterEdit(_selectedRow, posSettings.ShowIncRate);
        await TotalFindMethod();
        await FocusBarcodeInputAsync();
    }
    private async void OnBlurDiscAmntEvent(FocusEventArgs arg, TransactionDtls row)
    {
        _selectedRow = row;
        if (_selectedRow == null || _selectedRow.FK_ItemId == 0) { return; }
        if (_selectedRow.InRate == 0 && _selectedRow.ExRate > 0)
        {
            UpdateInRate(_selectedRow);
        }
        _selectedRow = TransactionHelper.GetTransactionAfterEdit(_selectedRow, posSettings.ShowIncRate, true);
        await TotalFindMethod();
        await FocusBarcodeInputAsync();
    }
    private async Task OnNumericFieldKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await CommitActiveField();
        }
    }

    private void OnBarcodeSearchInput(Microsoft.AspNetCore.Components.ChangeEventArgs e)
    {
        _barcodeSearchQuery = e.Value?.ToString() ?? "";
        _numPadBuffer = _barcodeSearchQuery;
    }

    private void UpdateInRate(TransactionDtls row)
    {
        if (row == null) return;
        var taxRate = row.TaxPerc > 0 ? row.TaxPerc : 15m;
        row.InRate = Math.Round(row.ExRate * (1 + (taxRate / 100m)), 2, MidpointRounding.AwayFromZero);
    }

    private void UpdateExRate(TransactionDtls row)
    {
        if (row == null) return;
        var taxRate = row.TaxPerc > 0 ? row.TaxPerc : 15m;
        row.ExRate = Math.Round(row.InRate / (1 + (taxRate / 100m)), 4, MidpointRounding.AwayFromZero);
    }

    private async Task OnRowRateChanged(TransactionDtls row)
    {
        if (row == null) return;
        UpdateInRate(row);
        _selectedRow = TransactionHelper.GetTransactionAfterEdit(row, posSettings.ShowIncRate);
        await TotalFindMethod();
        if (_activeInputField == "Rate")
        {
            _numPadBuffer = row.ExRate.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
    }

    private async Task OnRowIncRateChanged(TransactionDtls row)
    {
        if (row == null) return;
        UpdateExRate(row);
        _selectedRow = TransactionHelper.GetTransactionAfterEdit(row, true);
        await TotalFindMethod();
        if (_activeInputField == "IncRate")
        {
            _numPadBuffer = row.InRate.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
    }

    private async Task OnRowDiscountPercChanged(TransactionDtls row)
    {
        if (row == null) return;
        _selectedRow = TransactionHelper.GetTransactionAfterEdit(row, posSettings.ShowIncRate);
        await TotalFindMethod();
        if (_activeInputField == "Disc%")
        {
            _numPadBuffer = row.DiscountPerc.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
    }

    private async Task OnRowDiscountAmountChanged(TransactionDtls row)
    {
        if (row == null) return;
        if (row.GrossAmount > 0)
        {
            row.DiscountPerc = Math.Round((row.DiscountAmnt / row.GrossAmount) * 100m, 4, MidpointRounding.AwayFromZero);
        }
        else
        {
            row.DiscountPerc = 0;
        }
        _selectedRow = TransactionHelper.GetTransactionAfterEdit(row, posSettings.ShowIncRate, true);
        await TotalFindMethod();
        if (_activeInputField == "DiscAmt")
        {
            _numPadBuffer = row.DiscountAmnt.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
    }

    private async Task OnBarcodeSearchKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            _activeInputField = "BarcodeSearch";
            await CommitActiveField();
        }
    }
    #endregion

    #region POS Main Operations
    async Task DiscountMethod()
    {
        int iType = 2;
        salesInvoice.AdditionalDiscountType = 2;
        if (salesInvoice.AdditionalDiscountType == 2)
        {
            var result = await TransactionHelper.AdditionalDiscountMethod(entTransactions, iType, salesInvoice.AdditionalDiscPertge, salesInvoice.AdditionalDiscAmnt,strCurencyFormat, posSettings.ShowIncRate);
            string percentageStr = result.Item1?.ToString(strCurencyFormat) ?? "0";
            string discamount = result.Item2?.ToString(strCurencyFormat) ?? "0";
            salesInvoice.AdditionalDiscPertge = percentageStr.ToDecimal();
            salesInvoice.AdditionalDiscAmnt = discamount.ToDecimal();
           await TotalFindMethod();

        }
        else if (salesInvoice.AdditionalDiscountType == 1)
        {
            var result = await TransactionHelper.AddDiscType2Method(entTransactions, iType, salesInvoice.AdditionalDiscPertge, salesInvoice.AdditionalDiscAmnt, salesInvoice.GrandTotal);
            string percentageStr = result.Item1?.ToString(strCurencyFormat) ?? "0";
            string discamount = result.Item2?.ToString(strCurencyFormat) ?? "0";
            salesInvoice.AdditionalDiscPertge = percentageStr.ToDecimal();
            salesInvoice.AdditionalDiscAmnt = discamount.ToDecimal();
            await  TotalFindMethod();
        }
        else
        {
            decimal baseTotal = salesInvoice.NetAmount + (salesInvoice.AdditionalAmount ?? 0) + (salesInvoice.Freight ?? 0);
            var result = TransactionHelper.AddDiscType3Method(iType, salesInvoice.AdditionalDiscPertge, salesInvoice.AdditionalDiscAmnt, baseTotal);
            string percentageStr = result.Item1?.ToString(strCurencyFormat) ?? "0";
            string discamount = result.Item2?.ToString(strCurencyFormat) ?? "0";
            salesInvoice.AdditionalDiscPertge = percentageStr.ToDecimal();
            salesInvoice.AdditionalDiscAmnt = discamount.ToDecimal();
            salesInvoice.GrandTotal = result.Item3 ?? 0;
        }
        await FocusBarcodeInputAsync();
    }
    private async Task ProcessBarcode(string barcode, bool allowCreatePrompt = true)
    {
        try
        {
            var trimmedBarcode = barcode?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(trimmedBarcode)) return;

            var multiUnit = await api.GetItemMultiUnit(1, 1, trimmedBarcode);
            if (multiUnit == null)
            {
                var existingByBarcode = await api.GetItemByBarcode(trimmedBarcode);
                if (existingByBarcode != null &&
                    string.Equals(existingByBarcode.Barcode?.Trim(), trimmedBarcode, StringComparison.OrdinalIgnoreCase))
                {
                    multiUnit = new ItemMultiUnit
                    {
                        FK_ItemId = existingByBarcode.ItemId,
                        FK_UnitId = existingByBarcode.FK_UnitId ?? 1,
                        MultiUnitName = existingByBarcode.ItemName,
                        MultiUnitArabicName = existingByBarcode.ItemNameArabic,
                        Barcode = existingByBarcode.Barcode,
                        QTYPerUnit = 1,
                        Rate = existingByBarcode.SalesRate,
                        IncRate = existingByBarcode.InclusiveRate
                    };
                }
            }

            if (multiUnit != null)
            {
                if ((multiUnit.IncRate ?? 0) == 0 && (multiUnit.Rate ?? 0) > 0)
                {
                    var itemDetails = await api.GetItemById(multiUnit.FK_ItemId);
                    if (itemDetails != null && (itemDetails.InclusiveRate ?? 0) > 0)
                    {
                        multiUnit.IncRate = itemDetails.InclusiveRate;
                    }
                }
                await ProcessMultiUnit(multiUnit);
            }
            else
            {
                if (!allowCreatePrompt)
                {
                    return;
                }

                bool? confirm = await DialogService.ShowMessageBox(
                    Localizer("ProductNotAvailable"),
                    $"{Localizer("ProductInBarcodeNotAvailable")} ({trimmedBarcode}).\n\n{Localizer("DoYouWantNewProduct")}",
                    yesText: Localizer("Yes"),
                    cancelText: Localizer("No"));

                if (confirm == true)
                {
                    AppData.PassingValue = null;
                    var extraParams = new Dictionary<string, object?>
                    {
                        { "InitialBarcode", trimmedBarcode },
                        { "CloseOnSave", true }
                    };
                    var result = await Dialog.MasterCreationDialogMethod<ZakiSAASERP.Shared.Pages.Masters.ItemMasterView>(extraParams, MaxWidth.ExtraLarge);
                    if (result != null && !result.Canceled && (result.Data is CommonModel || AppData.PassingValue != null))
                    {
                        var createdModel = result.Data as CommonModel ?? AppData.PassingValue;
                        products = await api.GetMasters("posproduct", 1);
                        _productImageCache.Clear();

                        var createdMultiUnit = await api.GetItemMultiUnit(1, 1, trimmedBarcode);
                        if (createdMultiUnit != null)
                        {
                            await ProcessMultiUnit(createdMultiUnit);
                        }
                        else if (createdModel != null)
                        {
                            var newProd = products.FirstOrDefault(x => x.Id == createdModel.Id);
                            if (newProd is not null)
                            {
                                await AddProductToBill(newProd);
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            snackbar.Add($"Barcode scan error: {ex.Message}", Severity.Error);
        }
    }

    private async Task ProcessMultiUnit(ItemMultiUnit multiUnit)
    {
        var existing = entTransactions.FirstOrDefault(x => x.FK_ItemId == multiUnit.FK_ItemId && x.FK_UnitId == multiUnit.FK_UnitId);
        if (existing != null)
        {
            existing.Quantity += 1;
            TransactionHelper.GetTransactionAfterEdit(existing, posSettings.ShowIncRate);
            _selectedRow = existing;
        }
        else
        {
            var unit = unitlist.FirstOrDefault(x => x.Id == multiUnit.FK_UnitId);
            var trans = new TransactionDtls
            {
                ItemName = multiUnit.MultiUnitName,
                Barcode = multiUnit.Barcode,
                ItemArabicName = multiUnit.MultiUnitArabicName,
                QtyPerUnit = (multiUnit.QTYPerUnit ?? 0) > 0 ? multiUnit.QTYPerUnit.Value : 1,
                FK_UnitId = multiUnit.FK_UnitId,
                FK_ItemId = multiUnit.FK_ItemId,
                UnitName = unit?.Name ?? "PCS",
                ExRate = multiUnit.Rate ?? 0,
                InRate = multiUnit.IncRate ?? 0,
                TaxPerc = await api.GetValueMethod("vatFromItemId", multiUnit.FK_ItemId),
                Quantity = 1,
                SL = entTransactions.Count + 1
            };
            if (trans.InRate == 0 && trans.ExRate > 0)
            {
                UpdateInRate(trans);
            }
            trans = TransactionHelper.GetTransactionAfterEdit(trans, posSettings.ShowIncRate);
            entTransactions.Add(trans);
            _selectedRow = trans;
        }

        await TotalFindMethod();
        await FocusBarcodeInputAsync();
    }

    private void SelectCategory(CategoryMaster? category)
    {
        selectedCategory = category;
        _productPage = 1;
        _ = FocusBarcodeInputAsync();
    }

    

    private void OnProductPageChanged(int newPage)
    {
        _productPage = newPage;
        StateHasChanged();
    }

    private string? GetCategoryImage(CategoryMaster cat)
    {
        if (cat.ItemImage == null || cat.ItemImage.Length == 0) return null;
        if (!_categoryImageCache.TryGetValue(cat.CategoryId, out var img))
        {
            img = $"data:image/png;base64,{Convert.ToBase64String(cat.ItemImage)}";
            _categoryImageCache[cat.CategoryId] = img;
        }
        return img;
    }

    private string? GetProductImage(ExMasters prod)
    {
        if (prod.ItemImage == null || prod.ItemImage.Length == 0) return null;
        if (!_productImageCache.TryGetValue(prod.Id, out var img))
        {
            img = $"data:image/png;base64,{Convert.ToBase64String(prod.ItemImage)}";
            _productImageCache[prod.Id] = img;
        }
        return img;
    }

    private void RowClickEvent(TableRowClickEventArgs<TransactionDtls> args)
    {
        _selectedRow = args.Item;
    }

    private async void OnRowFieldBlur(TransactionDtls row)
    {
        if (row == null || row.FK_ItemId == 0) return;
        if (row.InRate == 0 && row.ExRate > 0)
        {
            UpdateInRate(row);
        }
        _selectedRow = TransactionHelper.GetTransactionAfterEdit(row, posSettings.ShowIncRate);
        await TotalFindMethod();
        await FocusBarcodeInputAsync();
    }

    private async Task AddProductToBill(ExMasters product)
    {
        try
        {
            var multiUnit = await api.GetItemMultiUnit(product.Id, 0, "");
            if (multiUnit == null)
            {
                var multiUnits = await api.GetItemMultiUnits(product.Id);
                if (multiUnits != null && multiUnits.Any())
                {
                    multiUnit = multiUnits.First();
                }
            }
            if (multiUnit != null && ((multiUnit.Rate ?? 0) == 0 || (multiUnit.IncRate ?? 0) == 0))
            {
               var itemDetails = await api.GetItemById(product.Id);
               if (itemDetails != null)
               {
                   if ((multiUnit.Rate ?? 0) == 0) multiUnit.Rate = itemDetails.SalesRate;
                   if ((multiUnit.IncRate ?? 0) == 0) multiUnit.IncRate = itemDetails.InclusiveRate;
               }
            }
            if (multiUnit != null && (multiUnit.IncRate ?? 0) == 0 && product.Amount > 0)
            {
                multiUnit.IncRate = product.Amount;
            }
            if (multiUnit != null)
            {
                await ProcessMultiUnit(multiUnit);
                await FocusBarcodeInputAsync();
            }
            else
            {
                snackbar.Add($"Product info not found for {product.Name}", Severity.Warning);
            }
        }
        catch (Exception ex)
        {
            snackbar.Add($"Error adding product: {ex.Message}", Severity.Error);
        }
    }

    private async Task SearchProductLookUp()
    {
        var selectedProduct = await Dialog.GetItemSearchMethod("");
        if (selectedProduct != null)
        {
            var simulatedMultiUnit = new ItemMultiUnit
            {
                FK_ItemId = selectedProduct.FK_ItemId,
                FK_UnitId = selectedProduct.FK_UnitId,
                MultiUnitName = selectedProduct.MultiUnitName,
                MultiUnitArabicName = selectedProduct.MultiUnitArabicName,
                Barcode = selectedProduct.Barcode,
                QTYPerUnit = selectedProduct.QTYPerUnit,
                Rate = selectedProduct.Rate,
                IncRate = selectedProduct.IncRate
            };
            if ((simulatedMultiUnit.IncRate ?? 0) == 0 && (simulatedMultiUnit.Rate ?? 0) > 0)
            {
                var itemDetails = await api.GetItemById(simulatedMultiUnit.FK_ItemId);
                if (itemDetails != null && (itemDetails.InclusiveRate ?? 0) > 0)
                {
                    simulatedMultiUnit.IncRate = itemDetails.InclusiveRate;
                }
            }
            await ProcessMultiUnit(simulatedMultiUnit);
        }
        await FocusBarcodeInputAsync();
    }

    private async void IncrementSelectedQty()
    {
        if (_selectedRow != null)
        {
            _selectedRow.Quantity += 1;
            _selectedRow = TransactionHelper.GetTransactionAfterEdit(_selectedRow, posSettings.ShowIncRate);
            await TotalFindMethod();
            await FocusBarcodeInputAsync();
        }
    }

    private async void DecrementSelectedQty()
    {
        if (_selectedRow != null && _selectedRow.Quantity > 1)
        {
            _selectedRow.Quantity -= 1;
            _selectedRow = TransactionHelper.GetTransactionAfterEdit(_selectedRow, posSettings.ShowIncRate);
            await TotalFindMethod();
            await FocusBarcodeInputAsync();
        }
    }

    private async void DeleteSelectedLine()
    {
        if (_selectedRow != null)
        {
            entTransactions.Remove(_selectedRow);

            // Re-index SL numbers
            int sl = 1;
            entTransactions.ForEach(x => x.SL = sl++);

            _selectedRow = entTransactions.LastOrDefault();
            await TotalFindMethod();
            await FocusBarcodeInputAsync();
        }
    }
    private async Task TotalFindMethod()
    {
        foreach (var item in entTransactions)
        {
            if (item.InRate == 0 && item.ExRate > 0)
            {
                UpdateInRate(item);
                TransactionHelper.GetTransactionAfterEdit(item, posSettings.ShowIncRate);
            }
        }
        decimal unDiscountedNetAmount = entTransactions.Sum(x => x.LineTotal);
        decimal addDisc = salesInvoice.AdditionalDiscAmnt ?? 0m;
        if (addDisc > unDiscountedNetAmount)
        {
            addDisc = unDiscountedNetAmount;
            salesInvoice.AdditionalDiscAmnt = addDisc;
        }
        salesInvoice.GrossAmount = entTransactions.Sum(x => x.GrossAmount);
        salesInvoice.LineDsicount = entTransactions.Sum(x => x.DiscountAmnt);
        salesInvoice.TaxableAmount = entTransactions.Sum(x => x.TaxableAmnt);
        salesInvoice.TaxTotal = entTransactions.Sum(x => x.TaxAmount);
        salesInvoice.NetAmount = entTransactions.Sum(x => x.LineTotal);
        salesInvoice.GrandTotal = salesInvoice.NetAmount;
        _totalQty = entTransactions.Sum(x => x.Quantity);
        var items = new List<SaleItem>();
        entTransactions.ForEach(x => items.Add(new SaleItem
        {
            ItemName = x.ItemName,
            Qty = x.Quantity,
            Rate = x.InRate > 0 ? x.InRate : x.ExRate,
            Total = x.LineTotal,
        }));
        if (AppData.IsMaui && AppData.IsWindows)
        {
            await CustomerDisplay.UpdateAsync(new CustomerDisplayDto { Items = items });
        }
        else
        {
            await SendCustomerDisplay(items);
        }
    }

    private async Task DiscountMethod(FocusEventArgs e, int iType)
    {
        await TotalFindMethod();
        await FocusBarcodeInputAsync();
    }
    #endregion

    #region POS Document Actions (Save, New, Load)
    private async Task OpenPaymentDialog()
    {
        if (_isSaving) return;

        if (privilege is not null && salesInvoice.SalesInvoiceId == 0 && !privilege.SaveData)
        {
            await Dialog.DialogMethod("No Save Privilege");
            return;
        }

        entTransactions = entTransactions.Where(x => !string.IsNullOrEmpty(x.ItemName)).ToList();
        if (!entTransactions.Any())
        {
            snackbar.Add("No items added to the bill", Severity.Warning);
            return;
        }

        if (salesInvoice.GrandTotal < 0)
        {
            await SaveInvoiceClick("Cash");
            return;
        }

        var parameters = new DialogParameters
        {
            { nameof(PaymentDialog.TotalAmount), salesInvoice.GrandTotal }
        };

        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        };

        var dialog = await DialogService.ShowAsync<PaymentDialog>("Payment", parameters, options);
        var result = await dialog.Result;

        if (!result.Canceled && result.Data is PaymentResult paymentResult)
        {
            await SaveInvoiceClick("Cash", paymentResult.Tendered, paymentResult.Changes);
        }
        await FocusBarcodeInputAsync();
    }

    private async Task SaveInvoiceClick(string paymentType = "Cash", decimal? tendered = null, decimal? changes = null)
    {
        if (_isSaving) return;

        if (privilege is not null && salesInvoice.SalesInvoiceId == 0 && !privilege.SaveData)
        {
            await Dialog.DialogMethod("No Save Privilege");
            return;
        }

        entTransactions = entTransactions.Where(x => !string.IsNullOrEmpty(x.ItemName)).ToList();
        if (!entTransactions.Any())
        {
            snackbar.Add("No items added to the bill", Severity.Warning);
            return;
        }
        if(PermanentSettingsData.ZatcaPhase == 2 && salesInvoice.SalesInvoiceId>0)
        {
            snackbar.Add("You cannot edit or delete this transaction as it has been e-Invoiced.", Severity.Warning);
            return;
        }
        _isSaving = true;
        StateHasChanged();

        try
        {
            salesInvoice.BillType = paymentType;
            salesInvoice.FK_BranchId = AppData.BranchId;
            salesInvoice.FK_UserId = AppData.UserId ?? 0;
            salesInvoice.FK_FinancialYearId = AppData.FYearId ?? 0;

            var selectedCustomer = customerList.FirstOrDefault(x => x.Name == salesInvoice.CustomerName);
            if (selectedCustomer is not null)
            {
                salesInvoice.FK_CustomerId = selectedCustomer.Id;
            }
            else
            {
                var cashCustomer = customerList.FirstOrDefault(x => x.Name.Contains("Cash")) ?? customerList.FirstOrDefault();
                salesInvoice.FK_CustomerId = cashCustomer?.Id ?? 0;
                salesInvoice.CustomerName = cashCustomer?.Name ?? "Cash Customer";
            }

            salesInvoice.EntryDate = DateTime.Now;

            if (tendered.HasValue)
            {
                salesInvoice.Tendered = tendered.Value;
                salesInvoice.Changes = changes ?? (tendered.Value - salesInvoice.GrandTotal);
            }
            else if (paymentType == "Cash")
            {
                salesInvoice.Tendered = salesInvoice.GrandTotal;
                salesInvoice.Changes = 0m;
            }

            var result = await api.CreateSalesInvoice(new SalesInvoiceDTO(salesInvoice, entTransactions, AppData.CompanyDetails!, PermanentSettingsData.ZatcaPhase, true, PermanentSettingsData.UploadSimplifiedXMLRealtime));

            if (result.IsSuccess)
            {
                if (PermanentSettingsData.EnableShiftClosing && ShiftService != null)
                {
                    try
                    {
                        var terminal = "Terminal 1";
                        var user = !string.IsNullOrWhiteSpace(AppData.UserName) ? AppData.UserName : "User";
                        bool isReturn = salesInvoice.GrandTotal < 0 || entTransactions.All(x => x.Quantity < 0);
                        await ShiftService.RecordSaleAsync(salesInvoice.GrandTotal, paymentType, isReturn, terminal, user);
                    }
                    catch { }
                }

                if (posSettings.AutoPrintBill)
                {
                    await PrintBillClick(salesInvoice.Tendered, salesInvoice.Changes);
                }
                if(string.IsNullOrEmpty(result.Message))
                {
                    snackbar.Add(Localizer("SaveMessage"), Severity.Success);
                }
                else
                {
                    snackbar.Add(result.Message, Severity.Info);
                }
                await NewClick();
            }
            else
            {
                await Dialog.DialogMethod($"Save Error: {result.Message}");
            }
        }
        catch (Exception ex)
        {
            snackbar.Add($"Error saving invoice: {ex.Message}", Severity.Error);
        }
        finally
        {
            _isSaving = false;
            await FocusBarcodeInputAsync();
        }
    }

    private async Task NewClick()
    {
        salesInvoice = new SalesInvoiceHdr();
        salesInvoice.AdditionalDiscountType = 2;
        salesInvoice.Tendered = null;
        salesInvoice.Changes = null;
        entTransactions.Clear();

        if (selectedCategory == null && categories != null && categories.Any())
        {
            selectedCategory = categories.FirstOrDefault();
        }

        if (IsAutoCode)
        {
            await LoadAutoCodeIfEnabled();
        }
        else
        {
            salesInvoice.SalesInvoiceNo = (await api.GetNewSalesInvoiceDocNo(AppData.BranchId)).ToString();
        }

        salesInvoice.SalesInvoiceDate = DateTime.Now.Date;
        var defaultCustomer = customerList.FirstOrDefault(x => x.Name.Contains("Cash")) ?? customerList.FirstOrDefault();
        if (defaultCustomer != null)
        {
            salesInvoice.FK_CustomerId = defaultCustomer.Id;
            salesInvoice.CustomerName = defaultCustomer.Name;
        }

        await ClearBarcodeInputAsync();
        _activeInputField = "BarcodeSearch";
        _selectedRow = null;
        _mobileStep = 1;

        await TotalFindMethod();
        await FocusBarcodeInputAsync();
    }

    #region Mobile Helpers
    private void SetMobileStep(int step)
    {
        _mobileStep = step;
        if (step == 1)
        {
            _ = FocusBarcodeInputAsync();
        }
        else
        {
            StateHasChanged();
        }
    }

    private async void IncrementQty(TransactionDtls item)
    {
        if (item != null)
        {
            item.Quantity += 1;
            TransactionHelper.GetTransactionAfterEdit(item, posSettings.ShowIncRate);
            await TotalFindMethod();
            await FocusBarcodeInputAsync();
        }
    }

    private async void DecrementQty(TransactionDtls item)
    {
        if (item != null && item.Quantity > 1)
        {
            item.Quantity -= 1;
            TransactionHelper.GetTransactionAfterEdit(item, posSettings.ShowIncRate);
            await TotalFindMethod();
            await FocusBarcodeInputAsync();
        }
    }

    private async void RemoveItem(TransactionDtls item)
    {
        if (item != null)
        {
            entTransactions.Remove(item);
            int sl = 1;
            entTransactions.ForEach(x => x.SL = sl++);
            await TotalFindMethod();
            await FocusBarcodeInputAsync();
        }
    }
    #endregion

    async Task LoadAutoCodeIfEnabled()
    {
        if (IsAutoCode && salesInvoice.SalesInvoiceId == 0)
        {
            salesInvoice.SalesInvoiceNo = await api.GetNextSalesInvoiceCode();
        }
    }
    #endregion

    #region Secondary POS Button Actions (Hold, Retrieve, Price Check, etc.)
    private async void HoldBillClick()
    {
        if (!entTransactions.Any())
        {
            snackbar.Add("No items to hold", Severity.Warning);
            return;
        }

        try
        {
            salesInvoice.FK_BranchId = salesInvoice.FK_BranchId > 0 ? salesInvoice.FK_BranchId : AppData.BranchId;
            salesInvoice.FK_UserId = salesInvoice.FK_UserId > 0 ? salesInvoice.FK_UserId : (AppData.UserId ?? 0);
            salesInvoice.FK_FinancialYearId = salesInvoice.FK_FinancialYearId > 0 ? salesInvoice.FK_FinancialYearId : (AppData.FYearId ?? 0);

            var holdHdr = salesInvoice.CopyTo<SalesInvoiceHoldHdr>();
            holdHdr.SalesInvoiceHoldHdrId = 0;
            if (string.IsNullOrWhiteSpace(holdHdr.SalesInvoiceNo))
                holdHdr.SalesInvoiceNo = "HOLD-" + DateTime.Now.ToString("yyMMddHHmmss");
            holdHdr.RefNo = AppData.Terminal;

            var holdDto = new SalesInvoiceHoldDTO(holdHdr, entTransactions.ToList());
            var res = await api.CreateSalesInvoiceHold(holdDto);
            if (res.Id > 0)
            {
                snackbar.Add("Transaction placed on Hold", Severity.Info);
                _ = NewClick();
                await RefreshHeldInvoicesCountAsync();
            }
            else
            {
                snackbar.Add($"Failed to hold transaction: {res.Message}", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            snackbar.Add($"Error holding transaction: {ex.Message}", Severity.Error);
        }
    }

    private async void RetrieveBillClick()
    {
        try
        {
            var branchId = salesInvoice.FK_BranchId > 0 ? salesInvoice.FK_BranchId : AppData.BranchId;
            var heldList = await api.GetAllSalesInvoiceHold(branchId, AppData.Terminal);
            if (heldList == null || !heldList.Any())
            {
                snackbar.Add("No held transactions to retrieve", Severity.Warning);
                return;
            }

            int selectedHoldId = 0;
            if (heldList.Count == 1)
            {
                selectedHoldId = heldList.First().SalesInvoiceHoldHdrId;
            }
            else
            {
                var parameters = new DialogParameters { ["BranchId"] = branchId, ["Terminal"] = AppData.Terminal };
                var dialog = await DialogService.ShowAsync<SalesInvoiceHoldSearchDialog>("Held Transactions", parameters, new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true });
                var result = await dialog.Result;
                if (!result.Canceled && result.Data is int holdId)
                {
                    selectedHoldId = holdId;
                }
                else
                {
                    return;
                }
            }

            if (selectedHoldId > 0)
            {
                var heldDto = await api.GetSalesInvoiceHoldById(selectedHoldId);
                if (heldDto != null && heldDto.HoldHdr != null)
                {
                    salesInvoice = heldDto.HoldHdr.CopyTo<SalesInvoiceHdr>();
                    salesInvoice.SalesInvoiceId = 0;
                    entTransactions = heldDto.HoldDtls ?? new List<TransactionDtls>();
                    _selectedRow = entTransactions.LastOrDefault();

                    await api.DeleteSalesInvoiceHold(selectedHoldId);

                    await TotalFindMethod();
                    snackbar.Add("Held transaction retrieved successfully", Severity.Success);
                    await FocusBarcodeInputAsync();
                }
            }
        }
        catch (Exception ex)
        {
            snackbar.Add($"Error retrieving held transaction: {ex.Message}", Severity.Error);
        }
        finally
        {
            await RefreshHeldInvoicesCountAsync();
        }
    }

    private async Task RefreshHeldInvoicesCountAsync()
    {
        try
        {
            var branchId = salesInvoice.FK_BranchId > 0 ? salesInvoice.FK_BranchId : AppData.BranchId;
            var heldList = await api.GetAllSalesInvoiceHold(branchId, AppData.Terminal);
            heldInvoicesCount = heldList?.Count ?? 0;
            await InvokeAsync(StateHasChanged);
        }
        catch
        {
            heldInvoicesCount = 0;
        }
    }

    private async Task SearchCustomerClick()
    {
        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Medium };
        var dialog = await DialogService.ShowAsync<CustomerSearchDialog>("Search Customer", options);
        var result = await dialog.Result;

        if (!result.Canceled && result.Data is CustomerMaster selectedCustomer)
        {
            salesInvoice.FK_CustomerId = selectedCustomer.CustomerId;
            salesInvoice.CustomerName = selectedCustomer.Name ?? "";
            snackbar.Add($"Customer Selected: {selectedCustomer.Name}", Severity.Info);
        }
        await FocusBarcodeInputAsync();
    }

    private async Task SalesReportClick()
    {
        await CrudHelper.Navigation(tabService, 0, "Sales Report");
    }

    private async Task SearchBillClick()
    {
        var result = await Dialog.TransactionSearchMethod("Sales Invoice");
        if (result is not null)
        {
            var dto = await api.GetSalesInvoiceById(result.Id);
            entTransactions = dto.InvoiceDtls;
            entTransactions.ForEach(x => x.SL = entTransactions.IndexOf(x) + 1);
            salesInvoice = dto.InvoiceHdr;

            _barcodeSearchQuery = "";
            _numPadBuffer = "";
            _activeInputField = "BarcodeSearch";
            _selectedRow = entTransactions.LastOrDefault();

            await TotalFindMethod();
        }
        await FocusBarcodeInputAsync();
    }

    private async Task CreateCustomerClick()
    {
        var result = await Dialog.MasterCreationDialogMethod<ZakiSAASERP.Shared.Pages.Masters.CustomerMasterView>();
        if (!result!.Canceled && result.Data is not null)
        {
            customerList.Add(AppData.PassingValue);
            salesInvoice.FK_CustomerId = AppData.PassingValue.Id;
            salesInvoice.CustomerName = AppData.PassingValue.Name;
        }
        await FocusBarcodeInputAsync();
    }


    private async Task CreateProductClick()
    {
        var result = await Dialog.MasterCreationDialogMethod<ZakiSAASERP.Shared.Pages.Masters.ItemMasterView>();
        if (!result!.Canceled && result.Data is not null && AppData.PassingValue is not null)
        {
            products = await api.GetMasters("posproduct", 1);
            _productImageCache.Clear();
            var newProd = products.FirstOrDefault(x => x.Id == AppData.PassingValue.Id);
            if (newProd is not null)
            {
                await AddProductToBill(newProd);
            }
        }
        await FocusBarcodeInputAsync();
    }

    private async Task OpenShiftDialog()
    {
        try
        {
            if (DialogService == null) return;

            var options = new DialogOptions
            {
                CloseOnEscapeKey = true,
                MaxWidth = MaxWidth.Medium,
                FullWidth = true
            };

            var dialog = await DialogService.ShowAsync<ShiftDialog>("Shift Reconciliation & Handover", options);
            await dialog.Result;
        }
        catch (Exception ex)
        {
            snackbar.Add($"Shift dialog error: {ex.Message}", Severity.Error);
        }
    }

    private async Task OpenSettings()
    {
        var parameters = new DialogParameters
        {
            { "Settings", posSettings.Clone() }
        };
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        };
        var dialog = await DialogService.ShowAsync<POSSettingsDialog>("POS Settings", parameters, options);
        var result = await dialog.Result;

        if (!result.Canceled && result.Data is POSSettings updatedSettings)
        {
            posSettings = updatedSettings;
            if (!posSettings.ShowCategoryPanel)
            {
                selectedCategory = null;
            }
            _productPage = 1;
            await PosSettingsService.SaveSettingsAsync(posSettings);
            if (posSettings.ShowNumPadByDefault && !_isShortScreen)
            {
                _showNumPad = true;
            }
            snackbar.Add("POS display and layout settings updated successfully", Severity.Success);
        }
        await FocusBarcodeInputAsync();
    }

    private async Task OpenCalculatorDialog()
    {
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            NoHeader = true,
            MaxWidth = MaxWidth.Small
        };
        var dialog = await DialogService.ShowAsync<CalculatorDialog>("Calculator", options);
        await dialog.Result;
        await FocusBarcodeInputAsync();
    }

    private async Task OpenCashDrawerClick()
    {
        try
        {
            await formFactor.OpenCashDrawer();
            snackbar.Add("Cash drawer opened successfully.", Severity.Success);
        }
        catch (Exception ex)
        {
            snackbar.Add($"Error opening cash drawer: {ex.Message}", Severity.Error);
        }
        await FocusBarcodeInputAsync();
    }

    private async Task OpenDiscountDialog()
    {
        var parameters = new DialogParameters
        {
            { "CurrentDiscount", salesInvoice.AdditionalDiscAmnt ?? 0m }
        };
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            MaxWidth = MaxWidth.ExtraSmall,
            FullWidth = true
        };
        var dialog = await DialogService.ShowAsync<DiscountDialog>("Enter Discount Amount", parameters, options);
        var result = await dialog.Result;

        if (!result.Canceled && result.Data is decimal discountAmount)
        {
            salesInvoice.AdditionalDiscAmnt = discountAmount;
            await DiscountMethod();
        }
        await FocusBarcodeInputAsync();
    }

    private async Task PrintBillClick() => await PrintBillClick(salesInvoice.Tendered, salesInvoice.Changes);

    private async Task PrintBillClick(decimal? tendered, decimal? changes)
    {
        try
        {
            var transctions = entTransactions.Where(x => !string.IsNullOrEmpty(x.ItemName)).ToList();
            if (!transctions.Any())
            {
                snackbar.Add("No items to print", Severity.Warning);
                return;
            }

            if (pdto == null || pdto.hdr == null || !pdto.dtls.Any())
            {
                pdto = await indexdbHelper.LoadPrintDesignFromIndexedDB("Sales_Invoice_Thermal", AppData.IsWindows);
            }

            if (pdto == null || pdto.hdr == null || !pdto.dtls.Any())
            {
                snackbar.Add("Print design not found", Severity.Warning);
                return;
            }

            var hdr = salesInvoice.CopyTo<TransactionHdr>();
            hdr.HdrDate = salesInvoice.SalesInvoiceDate ?? DateTime.Now;
            hdr.HdrNo = salesInvoice.SalesInvoiceNo;
            hdr.TotalQuantity = entTransactions.Sum(x => x.Quantity);
            hdr.QRCode = salesInvoice.InvoiceQRCode;
            hdr.InvoiceDate = (salesInvoice.SalesInvoiceDate ?? DateTime.Now).ToString("dd-MM-yyyy");
            hdr.Tendered = tendered ?? salesInvoice.Tendered ?? salesInvoice.GrandTotal;
            hdr.Changes = changes ?? salesInvoice.Changes ?? 0m;
            hdr.BillType = salesInvoice.BillType; 
            hdr.BillTypeArabic = GetBillTypeArabic(salesInvoice.BillType ?? "Cash");
            var customer = await api.GetCustomerById(salesInvoice.FK_CustomerId);
            party = customer.CopyTo<Party>();
            PrintParameterDTO dto = new PrintParameterDTO(pdto.hdr, pdto.dtls, hdr, transctions, party, AppData.CompanyDetails!, null);
            if (!AppData.IsMaui && !AppData.IsWindows)
            {
                await CrudHelper.PrintInvoiceMethod(dto, js);
            }
            else
            {
                if (AppData.IsWindows)
                {
                    formFactor.DirectPrinting(dto);
                }
                else
                {
                    await CrudHelper.PrintInvoiceMethodLocal(Downloader, dto, Dialog, false);
                }
            }
        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod($"Print error: {ex.Message}");
        }
        finally
        {
            await FocusBarcodeInputAsync();
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
    private async Task PdfBillClick()
    {
        try
        {
            var transctions = entTransactions.Where(x => !string.IsNullOrEmpty(x.ItemName)).ToList();
            if (!transctions.Any())
            {
                snackbar.Add("No items to export to PDF", Severity.Warning);
                return;
            }

            if (pdto == null || pdto.hdr == null || !pdto.dtls.Any())
            {
                pdto = await indexdbHelper.LoadPrintDesignFromIndexedDB("Sales_Invoice_Thermal", AppData.IsWindows);
            }

            if (pdto == null || pdto.hdr == null || !pdto.dtls.Any())
            {
                snackbar.Add("Print design not found", Severity.Warning);
                return;
            }

            var hdr = salesInvoice.CopyTo<TransactionHdr>();
            hdr.HdrDate = salesInvoice.SalesInvoiceDate ?? DateTime.Now;
            hdr.HdrNo = salesInvoice.SalesInvoiceNo;
            hdr.TotalQuantity = entTransactions.Sum(x => x.Quantity);
            hdr.QRCode = salesInvoice.InvoiceQRCode;
            hdr.InvoiceDate = (salesInvoice.SalesInvoiceDate ?? DateTime.Now).ToString("dd-MM-yyyy");
            hdr.Tendered = salesInvoice.Tendered ?? salesInvoice.GrandTotal;
            hdr.Changes = salesInvoice.Changes ?? 0m;

            party = new Party
            {
                Name = salesInvoice.CustomerName,
                CustomerId = salesInvoice.FK_CustomerId
            };

            var printHdr = new PrintDesignHdr
            {
                PrintDesignHdrId = pdto.hdr.PrintDesignHdrId,
                DesignName = pdto.hdr.DesignName,
                VoucherType = pdto.hdr.VoucherType,
                IsDefault = pdto.hdr.IsDefault,
                PageHeight = pdto.hdr.PageHeight,
                PageWidth = pdto.hdr.PageWidth,
                LinesPerPage = pdto.hdr.LinesPerPage,
                SpaceBtwnLines = pdto.hdr.SpaceBtwnLines,
                BagroundImagePath = pdto.hdr.BagroundImagePath,
                PrinterName = "Microsoft Print to PDF",
                PrintType = pdto.hdr.PrintType,
                FormType = pdto.hdr.FormType,
                FK_BranchId = pdto.hdr.FK_BranchId
            };

            PrintParameterDTO dto = new PrintParameterDTO(printHdr, pdto.dtls, hdr, transctions, party, AppData.CompanyDetails!, null);
            if (AppData.IsWindows)
            {
                await CrudHelper.PrintInvoiceMethodLocal(Downloader, dto, Dialog, false);
            }
            else
            {
                await CrudHelper.PrintInvoiceMethodLocal(Downloader, dto, Dialog, false);
            }
        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod($"PDF error: {ex.Message}");
        }
        finally
        {
            await FocusBarcodeInputAsync();
        }
    }

    async Task OpenCustomerDisplay()
    {
        try
        {
            if (AppData.IsMaui)
            {
                await CustomerDisplay.OpenAsync();
                var items = entTransactions.Select(x => new SaleItem
                {
                    ItemName = x.ItemName,
                    Qty = x.Quantity,
                    Rate = x.InRate > 0 ? x.InRate : x.ExRate,
                    Total = x.LineTotal,
                }).ToList();
                await CustomerDisplay.UpdateAsync(new CustomerDisplayDto { Items = items });
            }
            else
            {
                await js.InvokeVoidAsync("customerDisplay.openWindow");
                var items = entTransactions.Select(x => new SaleItem
                {
                    ItemName = x.ItemName,
                    Qty = x.Quantity,
                    Rate = x.InRate > 0 ? x.InRate : x.ExRate,
                    Total = x.LineTotal,
                }).ToList();
                await SendCustomerDisplay(items);
            }
        }
        catch (Exception ex)
        {
            snackbar.Add($"Customer display error: {ex.Message}", Severity.Warning);
        }
        finally
        {
            await FocusBarcodeInputAsync();
        }
    }

    async Task SendCustomerDisplay(List<SaleItem> Items)
    {
        try
        {
            var dto = new CustomerDisplayDto
            {
                Items = Items
            };

            await js.InvokeVoidAsync(
                "customerDisplay.sendData",
                dto);
        }
        catch
        {
            // Ignore customer display sync errors to not block transaction entry
        }
    }
    #endregion
}
