using ZakiSAASERP.Models.Transactions.Purchase;
using ZakiSAASERP.Models.Settings;
using ZakiSAASERP.Shared.Pages.Masters;
using Microsoft.AspNetCore.Components.Web;

namespace ZakiSAASERP.Shared.Pages.Transactions.Purchase;

public partial class PurchaseReturnView : ComponentBase, ITabAware
{
    #region Variables
    [Inject] IBrowserViewportService ResizeService { get; set; }
    private WebUserPrivilege? privilege;
    List<TransactionDtls> entTransactions = new();
    private string _searchString;
    PurchaseReturnHdr hdr = new();
    TransactionDtls transaction = new();
    List<ExMasters> products = new();
    List<CommonModel>? supplierList = new();
    List<CommonModel> unitlist = new();
    List<CommonModel> itemUnitlist = new();
    List<CommonModel> purchaserList = new();
    List<CommonModel> projectList = new();
    List<StoreMaster> storeList = new();
    Party party = new();
    bool IsEditing = false;
    bool DisablePrintButton = true;
    bool DisableCancelButton = true;
    private bool IsAutoCode => PermanentSettingsData.AutoPurchaseReturnNo;

    MudTextField<string> txtVoucherNo;
    MudDatePicker txtVoucherDate;
    MudTextField<string> txtRefNo;
    MudTextField<string> txtSupplier;
    MudSelect<int?> cmbPurchaser;
    MudTextField<string> txtRemarks;
    MudNumericField<decimal?>? txtAdditional;
    MudNumericField<decimal?>? txtFreight;
    MudNumericField<decimal?>? txtAdditionalDisc;
    MudSelect<int?> cmbProject;
    MudNumericField<decimal> txtQuantity;
    MudTextField<string> txtBarcode;
    MudTextField<string> textField;
    MudNumericField<decimal> txtRate;
    MudNumericField<decimal> txtDiscount;
    MudNumericField<decimal> txtTax;
    MudNumericField<decimal> txtNet;
    private int _activeTabIndex = 0;
    string strQuantityFormat => PermanentSettingsData.QuantityFormat;
    string strCurencyFormat => PermanentSettingsData.CurrencyFormat;
    public int Width { get; set; }
    public decimal Gross { get; set; } = 0;


    // New Properties for Refactoring
    int ReturnMode { get; set; } = 0; // 0 = From Invoice, 1 = Direct
    int LoadMode { get; set; } = 0;   // 0 = Fill Qty, 1 = Zero Qty
    List<TransactionDtls> OriginalPurchaseDtls { get; set; } = new(); // Source of truth validation
    List<string> BillTypes = new() { "Cash", "Credit", "Bank", "Transfer", "Cheque" };
    PrintDesignDTO pdto;
    #endregion

    #region Properties
    [Parameter] public int TabId { get; set; }
    [Parameter] public int? Id { get; set; }
    #endregion

    #region Initialize
    protected async override Task OnInitializedAsync()
    {
        try
        {
            var size = await ResizeService.GetCurrentBrowserWindowSizeAsync();
            Width = size.Width;
            if (AppData.privileges is not null)
            {
                privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "Purchase Return");
            }

            // Use IndexedDB caching for faster dropdown loading
            supplierList = await indexdbHelper.LoadComboFromIndexedDB("Suppliers");
            unitlist = await indexdbHelper.LoadComboFromIndexedDB("Unit");
            purchaserList = await indexdbHelper.LoadComboFromIndexedDB("Purchaser");
            projectList = await indexdbHelper.LoadComboFromIndexedDB("Project");
            storeList = await api.GetAllStores();
            itemUnitlist = unitlist;
            products = await api.GetMasters("product", 1);
            pdto = await indexdbHelper.LoadPrintDesignFromIndexedDB("Purchase_Return");
            if (Width > 600)
            {
                AddNewRow();
            }
            if (Id is not null && Id > 0)
            {
                await PurchaseReturnLoadById(Id.Value);
            }
            else
                await NewClick();
            StateHasChanged();
        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod(ex.Message);
        }
    }
    #endregion

    #region Methods
    async Task FocusMethod()
    {
        if (!entTransactions.Any() || !string.IsNullOrEmpty(entTransactions.LastOrDefault()!.ItemName))
        {
            AddNewRow();
        }
        StateHasChanged();
        IsEditing = false;
        txtBarcode?.FocusAsync();
    }
    async Task CreatePartyMethod()
    {
        var result = await Dialog.MasterCreationDialogMethod<SupplierMasterView>();
        if (!result!.Canceled && result.Data is not null)
        {
            supplierList.Add(AppData.PassingValue);
            hdr.FK_SupplierId = AppData.PassingValue.Id;
            hdr.SupplierName = AppData.PassingValue.Name;
            var supplier = await api.GetSupplierById(AppData.PassingValue.Id);
            party = supplier.CopyTo<Party>();
            StateHasChanged();
        }
    }
    async Task CreatePurchaserMethod()
    {
        var result = await Dialog.MasterCreationDialogMethod<PurchaserMasterView>();
        if (!result!.Canceled && result.Data is not null)
        {
            purchaserList.Add(AppData.PassingValue);
            hdr.FK_PurchaserId = AppData.PassingValue.Id;
            StateHasChanged();
        }
    }
    async Task PurchaserSearchMethod()
    {
        var purchaserAllList = await api.GetAllPurchaser();
        var parameters = new DialogParameters
        {
            { "PurchaserList", purchaserAllList }
        };
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            MaxWidth = MaxWidth.Medium
        };
        var dialog = await DialogService.ShowAsync<PurchaserSearchDialog>(Localizer("PurchaserSearch"), parameters, options);
        var result = await dialog.Result;
        if (!result!.Canceled && result.Data is PurchaserMaster selectedPurchaser)
        {
            hdr.FK_PurchaserId = selectedPurchaser.PurchaserId;
            StateHasChanged();
        }
    }
    async Task PartySearchMethod()
    {
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            MaxWidth = MaxWidth.Large
        };
        var dialog = await DialogService.ShowAsync<SupplierSearchDialog>(Localizer("SupplierSearch"), options);
        var result = await dialog.Result;
        if (!result!.Canceled && result.Data is SupplierMaster selectedSupplier)
        {
            hdr.FK_SupplierId = selectedSupplier.SupplierId;
            hdr.SupplierName = selectedSupplier.Name;
            var supplier = await api.GetSupplierById(selectedSupplier.SupplierId);
            party = supplier.CopyTo<Party>();
            StateHasChanged();
        }
    }
    async Task OpenPurchaseSearch()
    {
        var parameters = new DialogParameters<PurchaseSearchDialog>
        {
            { x => x.MultiSelection, false } // Single selection only
        };
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            MaxWidth = MaxWidth.Large
        };
        var dialog = await DialogService.ShowAsync<PurchaseSearchDialog>(Localizer("PurchaseSearch"), parameters, options);
        var result = await dialog.Result;
        if (!result.Canceled && result.Data is List<ExMasters> selectedPurchases)
        {
            // Enforce Single Selection
            if (selectedPurchases.Count > 1)
            {
                snackbar.Add("Please select only one Purchase Invoice for return.", Severity.Warning);
                return;
            }

            if (selectedPurchases.Any())
            {
                var selectedPur = selectedPurchases.First();
                hdr.FK_PurchaseId = selectedPur.Id;
                hdr.PurchaseVoucherNo = selectedPur.Code;

                // Fetch Full Details
                var purchaseDto = await api.GetPurchaseById(selectedPur.Id);
                if (purchaseDto is not null && purchaseDto.Dtls is not null)
                {
                    // Set Header Info
                    hdr.BillType = purchaseDto.Hdr.BillType; // Auto-select BillType
                    hdr.FK_SupplierId = purchaseDto.Hdr.FK_SupplierId;
                    hdr.SupplierName = purchaseDto.Hdr.SupplierName;
                    hdr.InvoiceNo = purchaseDto.Hdr.InvoiceNo; // Map Supplier Invoice No

                    // Map Additional Details
                    hdr.FK_PurchaserId = purchaseDto.Hdr.FK_PurchaserId;
                    hdr.FK_ProjectId = purchaseDto.Hdr.FK_ProjectId;
                    hdr.CurrencyId = purchaseDto.Hdr.FK_CurrencyId;
                    //hdr.CurrencyRate = purchaseDto.Hdr.CurrencyRate;
                    hdr.FK_AccountId = purchaseDto.Hdr.FK_AccountId;

                    // Map Remarks and Additional Discounts
                    hdr.Remarks = purchaseDto.Hdr.Remarks;
                    hdr.AdditionalDiscPertge = purchaseDto.Hdr.AdditionalDiscPertge;
                    hdr.AdditionalDiscountType = purchaseDto.Hdr.AdditionalDiscountType;
                    hdr.AdditionalDiscAmnt = purchaseDto.Hdr.AdditionalDiscAmnt;
                    hdr.AdditionalAmount = purchaseDto.Hdr.AdditionalAmount;

                    // Store Original Details for Validation
                    OriginalPurchaseDtls = purchaseDto.Dtls.Select(x => x.CopyTo<TransactionDtls>()).ToList();

                    entTransactions.Clear();

                    // Fetch Balance Items (Original - Returned)
                    var balanceItems = await api.GetPurchaseReturnBalance(selectedPur.Id);

                    if (balanceItems == null || !balanceItems.Any())
                    {
                        snackbar.Add("This Invoice is already fully returned.", Severity.Warning);
                        hdr.FK_PurchaseId = 0;
                        return;
                    }

                    foreach (var item in balanceItems)
                    {
                        var newItem = item.CopyTo<TransactionDtls>();
                        newItem.SL = entTransactions.Count + 1;

                        // Handle Load Mode
                        if (LoadMode == 1) // Zero Qty
                        {
                            newItem.Quantity = 0;
                            newItem.LineTotal = 0;
                            newItem.TaxAmount = 0;
                            newItem.DiscountAmnt = 0;
                            // Recalculate if needed, but 0 qty implies 0 totals usually
                        }

                        // Ensure Store is set
                        if (newItem.FK_StoreId == 0 && storeList.Any())
                        {
                            var defStore = storeList.FirstOrDefault(x => x.IsDefaultStore) ?? storeList.FirstOrDefault();
                            newItem.FK_StoreId = defStore?.StoreId ?? 0;
                        }

                        entTransactions.Add(newItem);
                    }
                }
            }

            AddNewRow();
            TotalFindMethod();

            // Recalculate Additional Discount based on Percentage and NetAmount
            // Only if NOT Inclusive (Type 2). For Type 2, we trust the loaded values.
            if (hdr.AdditionalDiscountType != 2 && (hdr.AdditionalDiscPertge > 0 || (hdr.AdditionalAmount ?? 0) > 0))
            {
                int iType = hdr.AdditionalDiscAmnt > 0 ? 2 : 1;
                await DiscountMethod(new FocusEventArgs(), iType);
            }
            StateHasChanged();
        }
        StateHasChanged();
    }


    TransactionGridSettings gridSettings => PermanentSettingsData.PurchaseReturnSettings;

    void TotalFindMethod()
    {
        foreach (var item in entTransactions)
        {
            if (item.InRate == 0 && item.ExRate > 0)
            {
                item.InRate = Math.Round(item.ExRate * (1 + (item.TaxPerc / 100m)), 2, MidpointRounding.AwayFromZero);
            }
        }
        hdr.GrossAmount = entTransactions.Sum(x => x.GrossAmount);
        hdr.LineDsicount = entTransactions.Sum(x => x.DiscountAmnt);
        hdr.TaxableAmount = entTransactions.Sum(x => x.TaxableAmnt);
        hdr.TaxTotal = entTransactions.Sum(x => x.TaxAmount);
        hdr.NetAmount = entTransactions.Sum(x => x.LineTotal);
        hdr.GrandTotal = hdr.NetAmount;
    }

    private void OnExRateChanged(TransactionDtls row, decimal newExRate)
    {
        row.ExRate = newExRate;
        row.InRate = Math.Round(newExRate * (1 + (row.TaxPerc / 100m)), 2, MidpointRounding.AwayFromZero);
        TransactionHelper.GetTransactionAfterEdit(row);
        TotalFindMethod();
        StateHasChanged();
    }

    private void OnInRateChanged(TransactionDtls row, decimal newInRate)
    {
        row.InRate = newInRate;
        decimal taxMultiplier = 1 + (row.TaxPerc / 100m);
        row.ExRate = taxMultiplier != 0 ? Math.Round(newInRate / taxMultiplier, 4, MidpointRounding.AwayFromZero) : newInRate;
        TransactionHelper.GetTransactionAfterEdit(row);
        TotalFindMethod();
        StateHasChanged();
    }

    private void OnDiscPerChanged(TransactionDtls row, decimal newDiscPer)
    {
        row.DiscountPerc = newDiscPer;
        TransactionHelper.GetTransactionAfterEdit(row);
        TotalFindMethod();
        StateHasChanged();
    }

    private void OnDiscAmountChanged(TransactionDtls row, decimal newDiscAmount)
    {
        row.DiscountAmnt = newDiscAmount;
        if (row.GrossAmount > 0)
        {
            row.DiscountPerc = Math.Round((newDiscAmount / row.GrossAmount) * 100m, 4, MidpointRounding.AwayFromZero);
        }
        TransactionHelper.GetTransactionAfterEdit(row);
        TotalFindMethod();
        StateHasChanged();
    }
    #endregion

    #region Mobile
    async Task AddLineItem()
    {
        if (string.IsNullOrEmpty(transaction.ItemName)) { return; }
        if (!entTransactions.Any(x => x.SL == transaction.SL))
        {
            transaction.SL = entTransactions.Count + 1;
            entTransactions.Add(transaction);
        }
        transaction = new();
        itemUnitlist.Clear();
        itemUnitlist = unitlist;
        IsEditing = false;
        TotalFindMethod();
        await txtBarcode.FocusAsync();
        StateHasChanged();
    }
    private void RowClicked(DataGridRowClickEventArgs<TransactionDtls> args)
    {
        transaction = args.Item;
        IsEditing = true;
    }
    async Task MobileBarcodeChanged(string sVal)
    {
        if (string.IsNullOrEmpty(sVal) && IsEditing) { return; }
        if (!products.Any())
            products = await api.GetMasters("product", 1);
        if (!products.Any()) { return; }
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            MaxWidth = MaxWidth.Large
        };
        var selectedProduct = await Dialog.GetItemSearchMethod(sVal, IsSales: false);
        if (selectedProduct is not null)
        {
            IsEditing = true;
            transaction.FK_ItemId = selectedProduct.FK_ItemId;
            transaction.ItemName = selectedProduct.MultiUnitName;
            transaction.Barcode = selectedProduct.Barcode;
            transaction.FK_UnitId = selectedProduct.FK_UnitId;
            transaction.UnitName = unitlist.FirstOrDefault(x => x.Id == selectedProduct.FK_UnitId)?.Name;
            transaction.Quantity = 1;
            transaction.QtyPerUnit = 1;
            transaction.ExRate = (selectedProduct.Rate > 0 ? selectedProduct.Rate : selectedProduct.Cost) ?? 0;
            transaction.InRate = selectedProduct.IncRate ?? transaction.ExRate;
            transaction.TaxPerc = await api.GetValueMethod("vatFromItemId", selectedProduct.FK_ItemId);
            transaction.DiscountPerc = 0;
            transaction = TransactionHelper.GetTransactionAfterEdit(transaction);

            // Use pre-populated units from ItemSelectionResult (respects ItemSearchMode)
            unitlist.Clear();
            unitlist = selectedProduct.AvailableUnits ?? new List<CommonModel>();
            itemUnitlist = unitlist.ToList();

            TotalFindMethod();
        }
    }
    #endregion

    #region Desktop
    private void RowClickEvent(TableRowClickEventArgs<TransactionDtls> args)
    {
        transaction = args.Item!;
        IsEditing = false;
    }
    async Task OnBlurEvent(FocusEventArgs arg) => await FocusMethod();
    async Task Keydown(KeyboardEventArgs e, int fieldId)
    {
        if (e.Key == "Enter")
        {
            await DownFocusMethod(fieldId);
        }
        else if (e.Key == "ArrowDown")
        {
            await DownFocusMethod(fieldId);
        }
        else if (e.Key == "ArrowUp")
        {
            if (fieldId == 2)
            {
                await txtVoucherDate.FocusAsync();
            }
            else if (fieldId == 3)
            {
                await txtSupplier.FocusAsync();
            }
            else if (fieldId == 4)
            {
                await cmbPurchaser.FocusAsync();
            }
            else if (fieldId == 5)
            {
                _activeTabIndex = 0;
                await Task.Delay(100);
                await txtRemarks.FocusAsync();
            }
            else if (fieldId == 6)
            {
                await txtAdditional.FocusAsync();
            }
        }
    }

    private async Task DownFocusMethod(int fieldId)
    {
        if (fieldId == 1)
        {
            await txtVoucherDate.FocusAsync();
        }
        else if (fieldId == 2)
        {
            await cmbPurchaser.FocusAsync();
        }
        else if (fieldId == 3)
        {
            cmbPurchaser?.FocusAsync();
        }
        else if (fieldId == 4)
        {
            _activeTabIndex = 1;
            await Task.Delay(100);
            await txtAdditional!.FocusAsync();
        }
        else if (fieldId == 5)
        {
            await txtAdditionalDisc!.FocusAsync();
        }
        else if (fieldId == 6)
        {
            await cmbProject.FocusAsync();
        }
    }
    #endregion

    #region Common
    async Task DiscountMethod(FocusEventArgs e, int iType)
    {
        if (hdr.AdditionalDiscountType == 2)
        {
            var result = await TransactionHelper.AdditionalDiscountMethod(entTransactions, iType, hdr.AdditionalDiscPertge, hdr.AdditionalDiscAmnt);
            hdr.AdditionalDiscPertge = result.Item1 ?? 0;
            hdr.AdditionalDiscAmnt = result.Item2 ?? 0;
            TotalFindMethod();
        }
        else if (hdr.AdditionalDiscountType == 1)
        {
            var result = await TransactionHelper.AddDiscType2Method(entTransactions, iType, hdr.AdditionalDiscPertge, hdr.AdditionalDiscAmnt, hdr.GrandTotal);
            hdr.AdditionalDiscPertge = result.Item1 ?? 0;
            hdr.AdditionalDiscAmnt = result.Item2 ?? 0;
            TotalFindMethod();
        }
        else
        {
            decimal baseTotal = hdr.NetAmount + (hdr.AdditionalAmount ?? 0) + (hdr.Freight ?? 0);
            if ((hdr.AdditionalDiscAmnt ?? 0) == 0 && (hdr.AdditionalDiscPertge ?? 0) == 0)
            {
                hdr.GrandTotal = Math.Round(baseTotal);
                hdr.AdditionalDiscAmnt = baseTotal - hdr.GrandTotal;
                hdr.AdditionalDiscPertge = (hdr.GrandTotal == 0) ? 0 : Math.Round((100 * (hdr.AdditionalDiscAmnt ?? 0)) / hdr.GrandTotal, 2);
            }
            else
            {
                var result = TransactionHelper.AddDiscType3Method(iType, hdr.AdditionalDiscPertge, hdr.AdditionalDiscAmnt, baseTotal);
                hdr.AdditionalDiscPertge = result.Item1 ?? 0;
                hdr.AdditionalDiscAmnt = result.Item2 ?? 0;
                hdr.GrandTotal = result.Item3 ?? 0;
            }
        }
        StateHasChanged();
    }
    private async Task DltTransaction(TransactionDtls item)
    {
        entTransactions.Remove(item);
        if (!entTransactions.Any())
        {
            await FocusMethod();
        }
        int sl = 1;
        entTransactions.ForEach(x => x.SL = sl++);
        TotalFindMethod();
        StateHasChanged();
    }

    async Task OnChangeUnit(int iUnitId)
    {
        if (transaction.FK_ItemId == 0 || IsEditing) { return; }
        transaction.FK_UnitId = iUnitId;
        transaction.UnitName = unitlist.FirstOrDefault(x => x.Id == iUnitId)?.Name;
        var multiUnit = await api.GetItemMultiUnit(transaction.FK_ItemId, iUnitId, "");
        if (multiUnit is not null)
        {
            transaction.QtyPerUnit = multiUnit.QTYPerUnit ?? 0;
            transaction.ExRate = multiUnit.Rate ?? 0;
            transaction.ItemName = multiUnit.MultiUnitName;
            transaction.ItemArabicName = multiUnit.MultiUnitArabicName;
            transaction.Barcode = multiUnit.Barcode;
        }
        transaction = TransactionHelper.GetTransactionAfterEdit(transaction);
        TotalFindMethod();
        StateHasChanged();
    }
    private void OnBlurQtEvent(FocusEventArgs arg)
    {
        if (transaction.FK_ItemId == 0) { return; }

        // Validation for "From Invoice" Mode
        if (ReturnMode == 0 && OriginalPurchaseDtls.Any())
        {
            var originalItem = OriginalPurchaseDtls.FirstOrDefault(x => x.FK_ItemId == transaction.FK_ItemId && x.FK_UnitId == transaction.FK_UnitId);
            if (originalItem is not null)
            {
                if (transaction.Quantity > originalItem.Quantity)
                {
                    snackbar.Add($"Return Quantity ({transaction.Quantity}) cannot exceed Purchase Quantity ({originalItem.Quantity}).", Severity.Error);
                    transaction.Quantity = originalItem.Quantity; // Reset to Max
                }
            }
            else
            {
                // Item not found in original list - should basically not happen if search is restricted, but good safety
            }
        }

        transaction = TransactionHelper.GetTransactionAfterEdit(transaction);
        TotalFindMethod();
        StateHasChanged();
    }
    async Task TaskOnEnterMethod(KeyboardEventArgs e, int index)
    {
        if (e.Key == "Enter")
        {
            switch (index)
            {
                case 2:
                    await txtRate.FocusAsync();
                    await txtRate.SelectAsync();
                    break;
                case 4:
                    await txtDiscount.FocusAsync();
                    await txtDiscount.SelectAsync();
                    break;
                case 6:
                case 5:
                    await txtTax.FocusAsync();
                    await txtTax.SelectAsync();
                    break;
                case 7:
                case 8:
                    await txtNet.FocusAsync();
                    break;
                case 9:
                    await FocusMethod();
                    break;
                default:
                    break;
            }
            IsEditing = true;
            TotalFindMethod();
        }
    }
    async Task OnEnterMethod(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            if (string.IsNullOrEmpty(transaction.Barcode))
            {
                await BarcodeChanged("");
            }
            else
            {
                var multiUnit = await api.GetItemMultiUnit(1, 1, transaction.Barcode);
                if (multiUnit is not null)
                {
                    itemUnitlist = new List<CommonModel>();
                    var unit = unitlist.FirstOrDefault(x => x.Id == multiUnit.FK_UnitId);
                    if (unit is not null)
                    {
                        itemUnitlist.Add(new CommonModel
                        {
                            Id = unit.Id,
                            Name = unit.Name
                        });
                    }
                    transaction.ItemName = multiUnit.MultiUnitName;
                    transaction.Barcode = multiUnit.Barcode;
                    transaction.ItemArabicName = multiUnit.MultiUnitArabicName;
                    transaction.QtyPerUnit = multiUnit.QTYPerUnit ?? 0;
                    transaction.FK_UnitId = multiUnit.FK_UnitId;
                    transaction.FK_ItemId = multiUnit.FK_ItemId;
                    transaction.UnitName = unitlist.FirstOrDefault(x => x.Id == transaction.FK_UnitId).Name;
                    transaction.ExRate = multiUnit.Rate ?? 0;
                    transaction.InRate = multiUnit.IncRate ?? 0;
                    transaction.TaxPerc = await api.GetValueMethod("vatFromItemId", multiUnit.FK_ItemId);
                    transaction.Quantity = 1;
                    transaction = TransactionHelper.GetTransactionAfterEdit(transaction);
                }
                else
                {
                    await BarcodeChanged(transaction.Barcode);
                }
            }
            await txtQuantity.FocusAsync();
            StateHasChanged();
        }
    }
    async Task OnBarcodBlurEvent(FocusEventArgs arg)
    {
        var multiUnit = await api.GetItemMultiUnit(transaction.FK_ItemId, transaction.FK_UnitId, transaction.Barcode);
        if (multiUnit == null) { return; }
        transaction.QtyPerUnit = (multiUnit.QTYPerUnit ?? 0) > 0 ? multiUnit.QTYPerUnit.Value : 1;
        transaction.ExRate = multiUnit.Rate ?? 0;
        transaction.ItemName = multiUnit.MultiUnitName;
        transaction.UnitName = unitlist.FirstOrDefault(x => x.Id == multiUnit.FK_UnitId)?.Name;
        transaction.Barcode = multiUnit.Barcode;
        transaction.ItemArabicName = multiUnit.MultiUnitArabicName;
        transaction.FK_ItemId = multiUnit.FK_ItemId;
        transaction.Quantity = 1;
        transaction.FK_UnitId = multiUnit.FK_UnitId;
        transaction.TaxPerc = await api.GetValueMethod("vatFromItemId", multiUnit.FK_ItemId);
        transaction.DiscountPerc = 0;
        transaction = TransactionHelper.GetTransactionAfterEdit(transaction);
        var allUnits = await api.GetCombofill("munit", multiUnit.FK_ItemId);
        unitlist.Clear();
        unitlist = allUnits ?? new List<CommonModel>();

        // Also update itemUnitlist for mobile view
        itemUnitlist = unitlist.ToList();
        TotalFindMethod();
        StateHasChanged();
    }
    async Task BarcodeChanged(string sVal)
    {
        if (string.IsNullOrEmpty(sVal) && IsEditing) { return; }

        // Restricted Search Logic for "From Invoice" Mode (ReturnMode == 0)
        if (ReturnMode == 0 && OriginalPurchaseDtls.Any())
        {
            // 1. Search locally in Original Invoice Items
            var matches = OriginalPurchaseDtls.Where(x =>
                (x.ItemName?.Contains(sVal, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (x.Barcode?.Contains(sVal, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (x.ItemArabicName?.Contains(sVal, StringComparison.OrdinalIgnoreCase) ?? false)
            ).ToList();

            if (!matches.Any())
            {
                snackbar.Add("Item not found in this Purchase Invoice.", Severity.Error);
                return;
            }

            if (matches.Count > 1)
            {
                // Try to find exact barcode match to resolve ambiguity
                var exact = matches.FirstOrDefault(x => x.Barcode?.Equals(sVal, StringComparison.OrdinalIgnoreCase) == true);
                if (exact is not null)
                {
                    await SelectLocalItem(exact);
                    return;
                }

                snackbar.Add($"Multiple items found in this invoice matching '{sVal}'. Please better refine your search (e.g. Scanned Barcode).", Severity.Warning);
                return;
            }

            // Single Match Found
            await SelectLocalItem(matches.First());
            return;
        }

        // Standard Global Search (Direct Return Mode)
        var selectedProduct = await Dialog.GetItemSearchMethod(sVal, IsSales: false);

        if (selectedProduct is not null)
        {
            IsEditing = true;
            transaction.FK_ItemId = selectedProduct.FK_ItemId;
            transaction.ItemName = selectedProduct.MultiUnitName;
            transaction.Barcode = selectedProduct.Barcode;
            transaction.FK_UnitId = selectedProduct.FK_UnitId;
            transaction.UnitName = unitlist.FirstOrDefault(x => x.Id == selectedProduct.FK_UnitId)?.Name;
            transaction.Quantity = 1;

            transaction.QtyPerUnit = 1;
            transaction.ExRate = (selectedProduct.Rate > 0 ? selectedProduct.Rate : selectedProduct.Cost) ?? 0;
            transaction.InRate = selectedProduct.IncRate ?? transaction.ExRate;
            transaction.TaxPerc = await api.GetValueMethod("vatFromItemId", selectedProduct.FK_ItemId);
            transaction.DiscountPerc = 0;
            transaction = TransactionHelper.GetTransactionAfterEdit(transaction);

            // Use pre-populated units from ItemSelectionResult
            unitlist.Clear();
            unitlist = selectedProduct.AvailableUnits ?? new List<CommonModel>();
            itemUnitlist = unitlist.ToList();

            TotalFindMethod();
        }
    }

    async Task SelectLocalItem(TransactionDtls item)
    {
        IsEditing = true;
        transaction.FK_ItemId = item.FK_ItemId;
        transaction.ItemName = item.ItemName;
        transaction.Barcode = item.Barcode;
        transaction.FK_UnitId = item.FK_UnitId;
        // Fetch unit name if missing, or use from list
        transaction.UnitName = item.UnitName; // Assuming mapped
        transaction.Quantity = 1; // Default to 1, user can edit. Or use item.Quantity (Balance)? No, usually scan = 1.

        transaction.QtyPerUnit = item.QtyPerUnit;
        transaction.ExRate = item.ExRate;
        transaction.InRate = item.InRate;
        transaction.TaxPerc = item.TaxPerc;
        transaction.TaxAmount = 0; // Recalculate based on 1 qty
        transaction.DiscountPerc = item.DiscountPerc; // Inherit discount % from invoice? Yes.

        // Recalculate totals for Qty 1
        transaction.LineTotal = (transaction.Quantity * transaction.ExRate) - transaction.DiscountAmnt + transaction.TaxAmount; // Approx, call helper
        transaction = TransactionHelper.GetTransactionAfterEdit(transaction);

        // Populate Unit List using GetItemMultiUnits (plural)
        var multiUnits = await api.GetItemMultiUnits(item.FK_ItemId);
        if (multiUnits is not null)
        {
            unitlist = multiUnits.Select(x => new CommonModel
            {
                Id = x.FK_UnitId,
                Name = x.Unit?.Name ?? x.MultiUnitName ?? "Unknown"
            }).DistinctBy(x => x.Id).ToList();
        }
        else
        {
            unitlist = new List<CommonModel>();
        }
        itemUnitlist = unitlist.ToList();

        // Ensure the invoice unit is selected
        if (unitlist.Any(x => x.Id == item.FK_UnitId))
        {
            transaction.FK_UnitId = item.FK_UnitId;
            transaction.UnitName = unitlist.First(x => x.Id == item.FK_UnitId).Name;
        }

        TotalFindMethod();
    }

    private bool _isSaving = false;

    async Task SaveOrUpdate()
    {
        if (_isSaving) return;
        if (privilege is not null && hdr.PurchaseReturnId == 0 && !privilege.SaveData)
        {
            await Dialog.DialogMethod(Localizer("No...Privilege"));
            return;
        }
        if (privilege is not null && hdr.PurchaseReturnId != 0 && !privilege.EditData)
        {
            await Dialog.DialogMethod(Localizer("No...Privilege"));
            return;
        }

        // Validation: Bonded with Original Invoice
        if (ReturnMode == 0 && OriginalPurchaseDtls.Any())
        {
            foreach (var item in entTransactions.Where(x => !string.IsNullOrEmpty(x.ItemName)))
            {
                var original = OriginalPurchaseDtls.FirstOrDefault(x => x.FK_ItemId == item.FK_ItemId && x.FK_UnitId == item.FK_UnitId);
                if (original is not null)
                {
                    if (item.Quantity > original.Quantity)
                    {
                        snackbar.Add($"Item '{item.ItemName}' quantity ({item.Quantity}) exceeds purchased quantity ({original.Quantity}).", Severity.Error);
                        return;
                    }
                }
                else
                {
                    snackbar.Add($"Item '{item.ItemName}' does not belong to the selected invoice.", Severity.Error);
                    return;
                }
            }
        }

        if (string.IsNullOrEmpty(hdr.BillType))
        {
            snackbar.Add("Please select a Bill Type.", Severity.Warning);
            return;
        }

        _isSaving = true;
        StateHasChanged();

        try
        {
            entTransactions = entTransactions.Where(x => !string.IsNullOrEmpty(x.ItemName)).ToList();
            if (!entTransactions.Any()) { await NewClick(); return; }
            hdr.FK_BranchId = AppData.BranchId;
            hdr.FK_UserId = AppData.UserId ?? 0;
            hdr.FK_FinancialYearId = AppData.FYearId ?? 0;
            var selectedSupplier = supplierList.FirstOrDefault(x => x.Name == hdr.SupplierName);
            if (selectedSupplier is not null)
            {
                hdr.FK_SupplierId = selectedSupplier.Id;
            }
            else
            {
                hdr.FK_SupplierId = supplierList.FirstOrDefault(x => x.Name == "Cash Supplier")?.Id ?? supplierList.FirstOrDefault()?.Id ?? 0;
            }
            if (hdr.PurchaseReturnId == 0)
            {
                hdr.EntryDate = DateTime.Now;
                var result = await api.CreatePurchaseReturn(new PurchaseReturnDTO(hdr, entTransactions));
                if (string.IsNullOrEmpty(result.Message))
                {
                    if (PermanentSettingsData.EnableShiftClosing && ShiftService != null)
                    {
                        try
                        {
                            var terminal = "Terminal 1";
                            var user = !string.IsNullOrWhiteSpace(AppData.UserName) ? AppData.UserName : "User";
                            await ShiftService.RecordPurchaseAsync(hdr.GrandTotal, hdr.BillType, true, terminal, user);
                        }
                        catch { }
                    }
                    hdr.PurchaseReturnId = result.Id;
                    if (await Dialog.DeleteDialogMethod(Localizer("PrintDialog")))
                    {
                        await PrintMethod();
                    }
                    DisablePrintButton = false;
                    snackbar.Add(Localizer("SaveMessage"), Severity.Success);
                    await NewClick();
                }
                else
                {
                    snackbar.Add(Localizer(result.Message), Severity.Warning);
                }
            }
            else
            {
                var result = await api.UpdatePurchaseReturn(new PurchaseReturnDTO(hdr, entTransactions));
                if (!string.IsNullOrEmpty(result.Message))
                {
                    snackbar.Add(Localizer(result.Message), Severity.Warning);
                }
                else
                {
                    snackbar.Add(Localizer("UpdateMessage"), Severity.Info);
                    await NewClick();
                }
            }
        }
        catch (Exception ex)
        {
            snackbar.Add(ex.Message, Severity.Error);
        }
        finally
        {
            _isSaving = false;
            StateHasChanged();
        }
    }
    async Task Cancel()
    {
        if (hdr.PurchaseReturnId == 0 || !entTransactions.Any()) { return; }
        if (privilege is not null && !privilege.DeleteData)
        {
            await Dialog.DialogMethod(Localizer("No...Privilege"));
            return;
        }
        if (await Dialog.DeleteDialogMethod(Localizer("CancelDialog")))
        {
            hdr.IsDeleted = true;
            entTransactions.ForEach(x => x.IsDeleted = true);
            var result = await api.UpdatePurchaseReturn(new PurchaseReturnDTO(hdr, entTransactions));

            if (result.Id > 0)
            {
                if (!string.IsNullOrEmpty(result.Message))
                    snackbar.Add(result.Message, Severity.Success);
                else
                    snackbar.Add(Localizer("CancelledMessage"), Severity.Success);

                await NewClick();
            }
            else
            {
                if (!string.IsNullOrEmpty(result.Message))
                    snackbar.Add(result.Message, Severity.Success);
                else
                    snackbar.Add(Localizer("CancelledMessage"), Severity.Success); // Fallback
            }
        }
    }
    async Task Delete()
    {
        if (hdr.PurchaseReturnId == 0 || !entTransactions.Any()) { return; }
        if (privilege is not null && !privilege.DeleteData)
        {
            await Dialog.DialogMethod(Localizer("No...Privilege"));
            return;
        }
        if (await Dialog.DeleteDialogMethod())
        {
            var result = await api.DeletePurchaseReturn(hdr.PurchaseReturnId);
            if (string.IsNullOrEmpty(result.Message))
            {
                snackbar.Add(Localizer("DeletedMessage"), Severity.Error);
                await NewClick();
            }
            else
            {
                snackbar.Add(Localizer(result.Message), Severity.Warning);
            }
        }
    }
    async Task NewClick()
    {
        hdr = new();
        if (IsAutoCode)
        {
            await LoadAutoCodeIfEnabled();
        }
        else
        {
            hdr.PurchaseReturnNo = (await api.GetNewPurchaseReturnDocNo(AppData.BranchId)).ToString();
        }
        hdr.PurchaseReturnDate = DateTime.Now.Date;
        if (supplierList.Any())
        {
            hdr.FK_SupplierId = supplierList.FirstOrDefault().Id;
            hdr.SupplierName = supplierList.FirstOrDefault().Name;
        }
        if (purchaserList.Any())
            hdr.FK_PurchaserId = purchaserList.FirstOrDefault()!.Id;
        if (projectList.Any())
            hdr.FK_ProjectId = projectList.FirstOrDefault()!.Id;
        hdr.AdditionalAmount = 0;
        hdr.Freight = 0;
        hdr.AdditionalDiscAmnt = 0;
        hdr.AdditionalDiscPertge = 0;
        hdr.IsDeleted = false;
        hdr.IsDeleted = false;
        entTransactions = new();
        DisablePrintButton = true;
        DisableCancelButton = true;
        party = new Party();
        party.Name = "Cash Supplier";
        var cashSup = supplierList.FirstOrDefault(x => x.Name == party.Name);
        if (cashSup is not null)
            party.Code = cashSup.Code;

        AddNewRow();

        // Reset Modes on New
        ReturnMode = 0;
        LoadMode = 0;
        OriginalPurchaseDtls.Clear();
    }
    void OnReturnModeChanged(int value)
    {
        ReturnMode = value;
        if (ReturnMode == 1) // Direct
        {
            hdr.FK_PurchaseId = 0;
            OriginalPurchaseDtls.Clear();
        }
        else
        {
            hdr.FK_PurchaseId = 0;
            entTransactions.Clear();
            AddNewRow();
        }
    }

    async Task PrintHelperMethod(bool IsPreview = false)
    {
        try
        {
            var transctions = entTransactions.Where(x => !string.IsNullOrEmpty(x.ItemName)).ToList();
            if (hdr.PurchaseReturnId == 0 || !entTransactions.Any()) { return; }
            var pdto = await api.GetPrintDesignAsync(new PrintDesignReq(0, "Purchase_Return"));
            if (pdto.hdr == null || !pdto.dtls.Any()) { return; }
            var transHdr = hdr.CopyTo<TransactionHdr>();
            transHdr.HdrNo = hdr.PurchaseReturnNo;
            transHdr.InvoiceDate = (hdr.PurchaseReturnDate ?? DateTime.Now).ToString("dd-MM-yyyy");
            party.Name = hdr.SupplierName; // Sync name from header to party
            PrintParameterDTO dto = new PrintParameterDTO(pdto.hdr, pdto.dtls, transHdr, transctions, party, AppData.CompanyDetails, null);
            await CrudHelper.PrintInvoiceMethodLocal(Downloader, dto, Dialog, IsPreview);
        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod(ex.Message);
        }
    }
    async Task PrintPrviewMethod() => await PrintHelperMethod(true);
    async Task PrintMethod() => await PrintHelperMethod(false);
    async Task SearchMethod()
    {
        var result = await Dialog.TransactionSearchMethod("Purchase Return");
        if (result is not null)
        {
            await PurchaseReturnLoadById(result.Id);
        }
    }

    private async Task PurchaseReturnLoadById(int Id)
    {
        var dto = await api.GetPurchaseReturnById(Id);
        entTransactions = dto.PurchaseReturnDtls;
        entTransactions.ForEach(x => x.SL = entTransactions.IndexOf(x) + 1);
        AddNewRow();
        hdr = dto.PurchaseReturnHdr;
        if (hdr.FK_PurchaseId > 0)
        {
            var pur = await api.GetPurchaseById(hdr.FK_PurchaseId ?? 0);
            if (pur is not null)
            {
                hdr.PurchaseVoucherNo = pur.Hdr.PurchaseNo;
            }
        }

        // Populate party details for printing
        if (hdr.FK_SupplierId > 0)
        {
            var supplier = await api.GetSupplierById(hdr.FK_SupplierId.Value);
            party = supplier.CopyTo<Party>();
        }

        DisablePrintButton = false;
        DisableCancelButton = false;
        TotalFindMethod();
        // Logic aligned with SalesQuotationView/SalesInvoiceView
        if (hdr.AdditionalDiscountType != 2)
        {
            int iType = hdr.AdditionalDiscAmnt > 0 ? 2 : 1;
            await DiscountMethod(new FocusEventArgs(), iType);
        }
        StateHasChanged();
    }

    private void AddNewRow()
    {
        // Find default store
        var allowedStores = storeList.Where(x => x.FK_BranchId == AppData.BranchId || x.FK_BranchId == null).ToList();
        var defaultStoreId = allowedStores.FirstOrDefault(x => x.IsDefaultStore)?.StoreId
                           ?? allowedStores.FirstOrDefault()?.StoreId
                           ?? 0;
        var lastItem = entTransactions.LastOrDefault();
        if (lastItem is not null)
        {
            if (string.IsNullOrEmpty(lastItem.ItemName))
            {
                snackbar.Add(Localizer("ItemNameRequired"), Severity.Warning);
                return;
            }
            if (lastItem.Quantity <= 0)
            {
                snackbar.Add(Localizer("QtyCannotBeZero"), Severity.Warning);
                return;
            }
            // Validate item exists
            if (lastItem.FK_ItemId == 0)
            {
                snackbar.Add($"{Localizer("InvalidItem")}: {lastItem.Barcode ?? lastItem.ItemName}", Severity.Error);
                return;
            }
        }

        transaction = new();
        transaction.SL = entTransactions.Count + 1;
        transaction.UnitName = unitlist.FirstOrDefault()?.Name ?? "";
        transaction.FK_UnitId = unitlist.FirstOrDefault()?.Id ?? 0;
        transaction.FK_StoreId = defaultStoreId;
        entTransactions.Add(transaction);
    }

    private async Task InvoicePreviewForReturn()
    {
        try
        {
            if ((hdr.FK_PurchaseId ?? 0) == 0)
            {
                snackbar.Add(Localizer("Please Select Purchase Voucher"), Severity.Warning);
                return;
            }

            var fullPurchase = await api.GetPurchaseById(hdr.FK_PurchaseId ?? 0);
            if (fullPurchase is not null && fullPurchase.Hdr is not null)
            {
                // Get Print Design for Purchase
                var pdto = await api.GetPrintDesignAsync(new PrintDesignReq(0, "Purchase"));
                if (pdto.hdr == null || !pdto.dtls.Any())
                {
                    await Dialog.DialogMethod("Print Design for Purchase not found");
                    return;
                }

                // Prepare for Printing
                // Need Party info for Purchase
                var purParty = new Party();
                if (fullPurchase.Hdr.FK_SupplierId > 0)
                {
                    var supplier = await api.GetSupplierById(fullPurchase.Hdr.FK_SupplierId);
                    if (supplier is not null)
                    {
                        purParty = supplier.CopyTo<Party>();
                    }
                    else
                    {
                        purParty.Name = fullPurchase.Hdr.SupplierName ?? "Unknown Supplier";
                    }
                }
                else
                {
                    purParty.Name = fullPurchase.Hdr.SupplierName ?? "";
                }

                var transHdr = fullPurchase.Hdr.CopyTo<TransactionHdr>();
                transHdr.HdrNo = fullPurchase.Hdr.PurchaseNo;
                transHdr.InvoiceDate = (fullPurchase.Hdr.PurchaseDate ?? DateTime.Now).ToString("dd-MM-yyyy");

                // Map Dtls
                var transDtls = fullPurchase.Dtls.Select(d => d.CopyTo<TransactionDtls>()).ToList();

                PrintParameterDTO dto = new PrintParameterDTO(pdto.hdr, pdto.dtls, transHdr, transDtls, purParty, AppData.CompanyDetails, null);
                await CrudHelper.PrintInvoiceMethodLocal(Downloader, dto, Dialog, true);
            }
            else
            {
                snackbar.Add(Localizer("Purchase Voucher Not Found"), Severity.Warning);
            }
        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod(ex.Message);
        }
    }
    void OnLoadModeChanged(int value)
    {
        LoadMode = value;

        // Update quantities for all existing items in the grid
        foreach (var item in entTransactions.Where(x => !string.IsNullOrEmpty(x.ItemName)))
        {
            if (LoadMode == 1) // Zero Qty - Clear all quantities
            {
                item.Quantity = 0;
                item.LineTotal = 0;
                item.TaxAmount = 0;
                item.DiscountAmnt = 0;
            }
            else if (LoadMode == 0 && OriginalPurchaseDtls.Any()) // Fill Qty - Restore balance quantities
            {
                // Find the corresponding balance item
                var balanceItem = OriginalPurchaseDtls.FirstOrDefault(x =>
                    x.FK_ItemId == item.FK_ItemId && x.FK_UnitId == item.FK_UnitId);

                if (balanceItem is not null)
                {
                    // Restore the balance quantity and recalculate
                    item.Quantity = balanceItem.Quantity;
                    item.ExRate = balanceItem.ExRate;
                    item.DiscountAmnt = balanceItem.DiscountAmnt;
                    item.DiscountPerc = balanceItem.DiscountPerc;
                    item.TaxAmount = balanceItem.TaxAmount;
                    item.TaxPerc = balanceItem.TaxPerc;
                    item.LineTotal = balanceItem.LineTotal;
                }
            }
        }

        // Recalculate totals
        TotalFindMethod();
        StateHasChanged();
    }
    private async Task LoadAutoCodeIfEnabled()
    {
        if (IsAutoCode && hdr.PurchaseReturnId == 0)
        {
            hdr.PurchaseReturnNo = await api.GetNextPurchaseReturnCode();
        }
    }
    #endregion
}
