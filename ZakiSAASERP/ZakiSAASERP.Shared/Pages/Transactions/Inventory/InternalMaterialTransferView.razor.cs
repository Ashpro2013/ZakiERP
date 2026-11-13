using ZakiSAASERP.Models.Enums;
using ZakiSAASERP.Models.Transactions.Inventory;
using FileDownloader;
using Microsoft.AspNetCore.Components.Web;

namespace ZakiSAASERP.Shared.Pages.Transactions.Inventory;

public partial class InternalMaterialTransferView : ComponentBase, ITabAware
{
    [Parameter] public int Id { get; set; }
    [Parameter] public int TabId { get; set; }
    [Inject] IViewService? api { get; set; }
    [Inject] IViewService _viewService { get; set; } = default!;
    [Inject] ISnackbar _snackbar { get; set; } = default!;
    [Inject] NavigationManager _navigationManager { get; set; } = default!;
    [Inject] IDialogService _dialogService { get; set; } = default!;
    [Inject] ModelService Dialog { get; set; } = default!;
    [Inject] IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] public Downloader Downloader { get; set; } = default!;
    [Inject] IBrowserViewportService BrowserViewportService { get; set; } = default!;

    private int _activeTabIndex = 0;

    private List<BranchMaster> Branches = new();
    private List<CommonModel> UnitList = new();
    private MudTextField<string> barcodeField;
    private MudSelect<int> fromStoreSelect;
    private MudSelect<int> toStoreSelect;
    private MudNumericField<decimal> txtQuantity;
    private MudNumericField<decimal> txtRate;
    private InternalMaterialTransferHdr hdr = new();
    private List<StoreMaster> Stores = new();
    private InternalMaterialTransferDtls transaction = new();

    // To track original quantities for editing to correctly calculate available stock
    private Dictionary<int, decimal> _originalQty = new();
    private bool _processing = false;
    bool DisablePrintButton => hdr.IntMatTransId == 0 || _processing;

    private int Width { get; set; } = 0;

    protected override async Task OnInitializedAsync()
    {
        var size = await BrowserViewportService.GetCurrentBrowserWindowSizeAsync();
        Width = size.Width;
        await LoadMasters();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (Id > 0)
        {
            await LoadTransaction(Id);
        }
        else
        {
            await LoadNew();
        }
    }

    private async Task LoadMasters()
    {
        Stores = await _viewService.GetAllStores();
        Branches = await _viewService.GetAllBranches();
        UnitList = await _viewService.GetCombofill("Unit", 1);
    }

    private async Task LoadNew()
    {
        hdr = new InternalMaterialTransferHdr
        {
            VoucherDate = DateTime.Now,
            FK_BranchId = AppData.BranchId,
            InternalMaterialTransferDetails = new List<InternalMaterialTransferDtls>()
        };
        // Conditional Default Row
        if (Width > 600)
        {
            hdr.InternalMaterialTransferDetails.Add(new InternalMaterialTransferDtls { SL = 1 });
        }
        else
        {
            // Mobile: Initialize a separate transaction object for the form
            transaction = new InternalMaterialTransferDtls();
        }

        // Set default stores in HEADER
        var defaultStore = Stores.FirstOrDefault(x => x.IsDefaultStore && x.FK_BranchId == AppData.BranchId)?.StoreId
                          ?? Stores.FirstOrDefault(x => x.FK_BranchId == AppData.BranchId)?.StoreId
                          ?? Stores.FirstOrDefault(x => x.IsDefaultStore && x.FK_BranchId == null)?.StoreId
                          ?? Stores.FirstOrDefault(x => x.FK_BranchId == null)?.StoreId ?? 0;
        hdr.FK_FromStoreId = defaultStore;
        hdr.FK_ToStoreId = defaultStore;

        // Auto Code logic matching Material Transfer IN
        if (PermanentSettingsData.AutoInternalMaterialTransferNo)
        {
            var nextCode = await _viewService.GetNextInternalMaterialTransferCode();
            hdr.VoucherNo = nextCode;

            // Fallback if API returns empty/zero
            if (string.IsNullOrEmpty(hdr.VoucherNo) || hdr.VoucherNo == "0")
            {
                var nextNo = await _viewService.GetNewInternalMaterialTransferNo(AppData.BranchId);
                hdr.VoucherNo = nextNo.ToString();
                // We don't necessarily need to set IsAutoCode = false here unless we want to unlock the field
            }
        }
        else
        {
            // "Auto code but not based on PR" (Simple incremental number)
            var nextNo = await _viewService.GetNewInternalMaterialTransferNo(AppData.BranchId);
            hdr.VoucherNo = nextNo.ToString();
        }

        if (string.IsNullOrEmpty(hdr.EnteredBy)) hdr.EnteredBy = AppData.UserName;
        StateHasChanged();
    }

    private async Task LoadTransaction(int id)
    {
        hdr = await _viewService.GetInternalMaterialTransferById(id);
        if (hdr == null)
        {
            _snackbar.Add(Localizer("TransactionNotFound"), Severity.Error);
            return;
        }

        // Cache original quantities
        _originalQty.Clear();
        foreach (var item in hdr.InternalMaterialTransferDetails)
        {
            if (item.IntMatTransDtlsId > 0 && item.Quantity > 0)
            {
                _originalQty[item.IntMatTransDtlsId] = item.Quantity;
            }

            // Populate NotMapped Display properties
            if (string.IsNullOrEmpty(item.ItemName) && item.Item is not null) item.ItemName = item.Item.ItemName;
            if (string.IsNullOrEmpty(item.Barcode) && item.Item is not null) item.Barcode = item.Item.Barcode;
            if (string.IsNullOrEmpty(item.UnitName) && item.Unit is not null) item.UnitName = item.Unit.Name;

            // Apply ItemSearchMode Logic
            if (PermanentSettingsData.ItemSearchMode == 2 && item.Item is not null && item.Item.MultiUnits is not null)
            {
                var conversion = item.Item.MultiUnits.FirstOrDefault(x => x.FK_UnitId == item.FK_UnitId);
                if (conversion is not null)
                {
                    if (!string.IsNullOrEmpty(conversion.MultiUnitName))
                        item.ItemName = conversion.MultiUnitName;

                    if (!string.IsNullOrEmpty(conversion.Barcode))
                        item.Barcode = conversion.Barcode;
                }
            }

            await UpdateItemStock(item);
        }
        if (hdr.InternalMaterialTransferDetails == null) hdr.InternalMaterialTransferDetails = new List<InternalMaterialTransferDtls>();
        CalculateTotal();
        StateHasChanged();

        // Focus validation for stores if needed
    }

    private void AddItem()
    {
        if (hdr.InternalMaterialTransferDetails.Any())
        {
            var lastItem = hdr.InternalMaterialTransferDetails.Last();
            if (lastItem.FK_ItemId == 0)
            {
                _snackbar.Add(Localizer("PleaseSelectProductInLastRow"), Severity.Warning);
                return;
            }
        }

        int maxSl = hdr.InternalMaterialTransferDetails.Any() ? hdr.InternalMaterialTransferDetails.Max(x => x.SL) : 0;
        var newDetail = new InternalMaterialTransferDtls { SL = maxSl + 1 };
        // No need to set stores on items - they use header stores
        hdr.InternalMaterialTransferDetails.Add(newDetail);

        // If Desktop, we bind to the new row
        if (Width > 600) transaction = newDetail;
    }

    private async Task AddLineItem()
    {
        if (string.IsNullOrEmpty(transaction.Barcode) || transaction.FK_ItemId == 0)
        {
            _snackbar.Add(Localizer("InvalidItem"), Severity.Warning);
            return;
        }

        // Validate Quantity
        string? qtyError = ValidateQuantity(transaction.Quantity, transaction);
        if (!string.IsNullOrEmpty(qtyError))
        {
            _snackbar.Add(qtyError, Severity.Warning);
            return;
        }

        if (!hdr.InternalMaterialTransferDetails.Contains(transaction))
        {
            if (ValidateDuplicateItems(transaction))
            {
                _snackbar.Add(Localizer("ItemAlreadyExists"), Severity.Warning);
                return;
            }
            int maxSl = hdr.InternalMaterialTransferDetails.Any() ? hdr.InternalMaterialTransferDetails.Max(x => x.SL) : 0;
            transaction.SL = maxSl + 1;
            hdr.InternalMaterialTransferDetails.Add(transaction);
        }

        // Reset for new entry
        transaction = new InternalMaterialTransferDtls();

        CalculateTotal();
        await FocusById("mobile-barcode");
    }

    private async Task MobileSearch()
    {
        // Use header-level store for item search (From Store)
        int searchStoreId = hdr.FK_FromStoreId > 0 ? hdr.FK_FromStoreId : 0;
        var result = await Dialog.GetItemSearchMethod(transaction.Barcode ?? string.Empty, "Product Search", searchStoreId);

        if (result is not null)
        {
            await ApplyItemFromSearch(result, transaction);
        }
    }

    private async Task FocusById(string id)
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("eval", $"document.getElementById('{id}').focus()");
        }
        catch { }
    }

    private void RemoveItem(InternalMaterialTransferDtls item)
    {
        hdr.InternalMaterialTransferDetails.Remove(item);
        CalculateTotal();
    }

    private async Task OnEnterMethod(KeyboardEventArgs e, InternalMaterialTransferDtls item)
    {
        if (e.Code == "Enter" || e.Code == "NumpadEnter")
        {
            await OpenItemSearchDialog(item);
        }
    }

    private async Task TaskOnEnterMethod(KeyboardEventArgs e, int id, InternalMaterialTransferDtls item)
    {
        if (e.Code == "Enter" || e.Code == "NumpadEnter")
        {
            if (id == 2) // Quantity: Move to Rate
            {
                CalculateRowAmount(item);
                await txtRate.FocusAsync();
            }
            else if (id == 4) // Rate
            {
                CalculateRowAmount(item);

                // Validation for Common Stores (use header stores)
                if (hdr.FK_FromStoreId == hdr.FK_ToStoreId)
                {
                    _snackbar.Add(Localizer("FromAndToStoreCannotBeSame"), Severity.Warning);
                    return;
                }

                if (ValidateDuplicateItems(item))
                {
                    _snackbar.Add(Localizer("ItemAlreadyExists"), Severity.Warning);
                    return;
                }

                AddItem();
                StateHasChanged();
                await Task.Delay(100);
                // Focus the barcode of the new row
                if (barcodeField is not null) await barcodeField.FocusAsync();
            }
        }
    }



    private async Task OpenItemSearchDialog(InternalMaterialTransferDtls details)
    {
        // Use header-level store for item search
        int searchStoreId = hdr.FK_FromStoreId > 0 ? hdr.FK_FromStoreId : 0;
        var result = await Dialog.GetItemSearchMethod(details.Barcode ?? string.Empty, "Product Search", searchStoreId);

        if (result is not null)
        {
            await ApplyItemFromSearch(result, details);
        }
    }

    private async Task ApplyItemFromSearch(ItemSelectionResult result, InternalMaterialTransferDtls targetItem)
    {
        string itemName = (PermanentSettingsData.ItemSearchMode == 2 && !string.IsNullOrEmpty(result.MultiUnitName))
                          ? result.MultiUnitName : result.ItemName;

        decimal qtyPerUnit = result.QtyPerUnit;
        if (qtyPerUnit <= 0) qtyPerUnit = 1;

        await MapItemToDetail(result.FK_ItemId, itemName, result.Barcode, result.FK_UnitId, result.UnitName,
            qtyPerUnit, result.Rate ?? 0, targetItem);

        await FocusQuantity(targetItem);
    }

    private async Task FocusQuantity(InternalMaterialTransferDtls item)
    {
        try
        {
            int index = hdr.InternalMaterialTransferDetails.IndexOf(item);
            // Wait for render
            await Task.Delay(100);
            await JSRuntime.InvokeVoidAsync("eval", $"document.getElementById('qty-{index}').querySelector('input').focus()");
        }
        catch
        {
            // Ignore JS errors if element not found
        }
    }

    private async Task MapItemToDetail(int fk_ItemId, string itemName, string barcode, int unitId, string unitName,
                                       decimal qtyPerUnit, decimal rate, InternalMaterialTransferDtls details)
    {
        var item = await _viewService.GetItemById(fk_ItemId);
        if (item is not null)
        {
            details.FK_ItemId = item.ItemId;

            // Allow override from search result
            details.ItemName = !string.IsNullOrEmpty(itemName) ? itemName : item.ItemName;
            details.Barcode = !string.IsNullOrEmpty(barcode) ? barcode : item.Barcode;
            details.Item = item;

            // Set Unit
            details.FK_UnitId = unitId > 0 ? unitId : (item.FK_UnitId ?? 0);
            details.UnitName = !string.IsNullOrEmpty(unitName) ? unitName : item.Unit?.Name;

            if (item.MultiUnits == null || !item.MultiUnits.Any())
            {
                item.MultiUnits = await _viewService.GetItemMultiUnits(item.ItemId);
            }

            // If unitId was passed (from search), try to find it in multiunits for context if needed, 
            // but we already trust the passed values.

            details.QtyPerUnit = qtyPerUnit > 0 ? qtyPerUnit : 1;

            details.Rate = rate > 0 ? rate : (item.StandardCost ?? 0);

            // Stock defaults are now based on HEADER stores, not individual items
            CalculateRowAmount(details);
            await UpdateItemStock(details);
        }
    }

    private async Task UpdateUnit(InternalMaterialTransferDtls details, int unitId)
    {
        details.FK_UnitId = unitId;
        if (details.Item?.MultiUnits is not null)
        {
            var unit = details.Item.MultiUnits.FirstOrDefault(x => x.FK_UnitId == unitId);
            details.QtyPerUnit = unit?.QTYPerUnit ?? 1;
        }
        else
        {
            details.QtyPerUnit = 1;
        }
        await UpdateItemStock(details);
        CalculateRowAmount(details);
    }

    private void CalculateRowAmount(InternalMaterialTransferDtls row)
    {
        row.Amount = row.Quantity * row.Rate;
        CalculateTotal();
    }

    private async Task UpdateItemStock(InternalMaterialTransferDtls details)
    {
        // Reset stock quantities
        details.FromStoreCurrentQty = 0;
        details.ToStoreCurrentQty = 0;

        // Use header-level stores for ALL items
        if (details.FK_ItemId > 0 && hdr.FK_FromStoreId > 0)
        {
            decimal baseStockFrom = await _viewService.GetItemCurrentStock(details.FK_ItemId, hdr.FK_FromStoreId, hdr.IntMatTransId);

            // If editing, add back the quantity (because we took it OUT) to show pre-transfer state
            if (hdr.IntMatTransId > 0 && _originalQty.TryGetValue(details.IntMatTransDtlsId, out decimal origQty))
            {
                baseStockFrom += (origQty * details.QtyPerUnit);
            }

            details.FromStoreCurrentQty = (details.QtyPerUnit > 0) ? baseStockFrom / details.QtyPerUnit : baseStockFrom;
        }

        if (details.FK_ItemId > 0 && hdr.FK_ToStoreId > 0)
        {
            decimal baseStockTo = await _viewService.GetItemCurrentStock(details.FK_ItemId, hdr.FK_ToStoreId, hdr.IntMatTransId);

            // If editing, subtract the quantity (because we put it IN) to show pre-transfer state
            if (hdr.IntMatTransId > 0 && _originalQty.TryGetValue(details.IntMatTransDtlsId, out decimal origQty))
            {
                baseStockTo -= (origQty * details.QtyPerUnit);
            }

            details.ToStoreCurrentQty = (details.QtyPerUnit > 0) ? baseStockTo / details.QtyPerUnit : baseStockTo;
        }
        StateHasChanged();
    }

    private string? ValidateQuantity(decimal value, InternalMaterialTransferDtls item)
    {
        if (value <= 0) return Localizer("QuantityMustBeGreaterThanZero");
        if (value > item.FromStoreCurrentQty && !PermanentSettingsData.AllowNegativeStock) return Localizer("InsufficientQuantity");
        return null;
    }

    bool ValidateDuplicateItems(InternalMaterialTransferDtls currentItem)
    {
        if (currentItem == null || currentItem.FK_ItemId == 0) return false;
        // Since stores are header-level, only check Item and Unit
        return hdr.InternalMaterialTransferDetails.Any(x => x != currentItem
                             && x.FK_ItemId == currentItem.FK_ItemId
                             && x.FK_UnitId == currentItem.FK_UnitId);
    }



    private async Task OnFromStoreChanged(int storeId)
    {
        hdr.FK_FromStoreId = storeId;
        foreach (var item in hdr.InternalMaterialTransferDetails)
        {
            await UpdateItemStock(item);
        }
    }

    private async Task OnToStoreChanged(int storeId)
    {
        hdr.FK_ToStoreId = storeId;
        foreach (var item in hdr.InternalMaterialTransferDetails)
        {
            await UpdateItemStock(item);
        }
    }

    private void CalculateTotal()
    {
        hdr.TotalQty = hdr.InternalMaterialTransferDetails.Sum(x => x.Quantity);
        hdr.TotalAmount = hdr.InternalMaterialTransferDetails.Sum(x => x.Amount);
    }

    private async Task Save()
    {
        if (_processing) return;
        _processing = true;
        try
        {
            var validDetails = hdr.InternalMaterialTransferDetails.Where(x => x.FK_ItemId > 0 && x.Quantity > 0).ToList();
            if (!validDetails.Any())
            {
                _snackbar.Add(Localizer("PleaseAddAtLeastOneItem"), Severity.Warning);
                return;
            }

            // Stock Validation
            if (!PermanentSettingsData.AllowNegativeStock)
            {
                var insufficientItems = validDetails.Where(x => !string.IsNullOrEmpty(ValidateQuantity(x.Quantity, x))).ToList();
                if (insufficientItems.Any())
                {
                    bool? result3 = await _dialogService.ShowMessageBox(
                        Localizer("Warning"),
                        $"{Localizer("CheckQuantity")} - {string.Join(", ", insufficientItems.Select(x => x.ItemName))}\n{Localizer("ProceedWithNegativeStock")}",
                        yesText: Localizer("Yes"), cancelText: Localizer("No"));

                    if (result3 != true)
                    {
                        return;
                    }
                }
            }

            // Validation for Common Stores (header-level)
            if (hdr.FK_FromStoreId == 0)
            {
                _snackbar.Add(Localizer("FromStoreRequired"), Severity.Warning);
                return;
            }
            if (hdr.FK_ToStoreId == 0)
            {
                _snackbar.Add(Localizer("ToStoreRequired"), Severity.Warning);
                return;
            }

            if (hdr.FK_FromStoreId == hdr.FK_ToStoreId)
            {
                _snackbar.Add(Localizer("FromAndToStoreCannotBeSame"), Severity.Warning);
                return;
            }

            // Duplicate Check
            if (validDetails.Any(x => ValidateDuplicateItems(x)))
            {
                _snackbar.Add(Localizer("DuplicateItemStoreUnit"), Severity.Warning);
                return;
            }

            hdr.FK_UserId = AppData.UserId ?? 0;

            OutputResponse result;
            if (hdr.IntMatTransId > 0)
            {
                result = await _viewService.UpdateInternalMaterialTransfer(hdr);
            }
            else
            {
                result = await _viewService.CreateInternalMaterialTransfer(hdr);
            }

            if (result.Id > 0)
            {
                if (hdr.IntMatTransId == 0) // New Save
                {
                    hdr.IntMatTransId = result.Id;
                    if (await Dialog.DeleteDialogMethod(Localizer("PrintDialog")))
                    {
                        await PrintMethod();
                    }



                    await LoadNew();
                    _snackbar.Add(Localizer("SaveMessage"), Severity.Success);
                }
                else // Update
                {
                    _snackbar.Add(Localizer("UpdateMessage"), Severity.Info);
                    // Usually on Update we stay on the record or clear? User said "while save or update clear all".
                    // So clear all for update too.
                    await LoadNew();
                }
            }
            else
            {
                _snackbar.Add($"Error: {result.Message}", Severity.Error);
            }
        }

        finally
        {
            _processing = false;
        }
    }

    private void RowClickEvent(TableRowClickEventArgs<InternalMaterialTransferDtls> tableRowClickEventArgs)
    {
        transaction = tableRowClickEventArgs.Item;
    }

    private void RowClicked(DataGridRowClickEventArgs<InternalMaterialTransferDtls> args)
    {
        transaction = args.Item;
        _activeTabIndex = 1; // Switch to Details Tab on Mobile
    }

    private void DltTransaction(InternalMaterialTransferDtls item)
    {
        hdr.InternalMaterialTransferDetails.Remove(item);
        if (hdr.InternalMaterialTransferDetails.Count == 0) AddItem();
        CalculateTotal();
    }

    private async void OnChangeUnit(int unitId, InternalMaterialTransferDtls item)
    {
        await UpdateUnit(item, unitId);
    }

    private async Task OnBlurQtyPerUnit(InternalMaterialTransferDtls item)
    {
        await UpdateItemStock(item);
        // StateHasChanged(); // UpdateItemStock calls this
    }

    private async Task Delete()
    {
        bool? result = await _dialogService.ShowMessageBox(
           Localizer("Warning"),
           Localizer("DeleteConfirmation"),
           yesText: Localizer("Delete"), cancelText: Localizer("Cancel"));

        if (result == true)
        {
            var res = await _viewService.DeleteInternalMaterialTransfer(hdr.IntMatTransId);
            if (res.Id > 0)
            {
                _snackbar.Add(Localizer("DeletedSuccessfully"), Severity.Success);
                await LoadNew();
            }
            else
            {
                _snackbar.Add($"Error: {res.Message}", Severity.Error);
            }
        }
    }

    private async Task Clear()
    {
        await LoadNew();
    }

    private async Task SearchMethod()
    {
        var result = await Dialog.TransactionSearchMethod("Internal Material Transfer");
        if (result is not null && result.Id > 0)
        {
            await LoadTransaction(result.Id);
        }
    }

    async Task PrintHelperMethod(bool IsPreview)
    {
        try
        {
            var validDetails = hdr.InternalMaterialTransferDetails.Where(x => x.FK_ItemId > 0 && x.Quantity > 0).ToList();
            if (!validDetails.Any())
            {
                _snackbar.Add(Localizer("NoItemsToPrint"), Severity.Warning);
                return;
            }

            var pdto = await _viewService.GetPrintDesignAsync(new(0, InvoiceTypes.Internal_Material_Transfer.ToString()));

            var thdr = new TransactionHdr();
            thdr.HdrDate = hdr.VoucherDate.Value;
            thdr.HdrNo = hdr.VoucherNo;
            thdr.RefNo = hdr.ReferenceNo;
            thdr.Remarks = hdr.Remarks;
            thdr.EnteredBy = hdr.EnteredBy;

            // Map Branch name (not user name)
            var branch = Branches.FirstOrDefault(x => x.BranchId == hdr.FK_BranchId);

            // Map header-level stores for print
            var fromStore = Stores.FirstOrDefault(x => x.StoreId == hdr.FK_FromStoreId);
            var toStore = Stores.FirstOrDefault(x => x.StoreId == hdr.FK_ToStoreId);
            thdr.Extra1 = fromStore?.Name ?? "";
            thdr.Extra2 = toStore?.Name ?? "";

            thdr.TotalQuantity = hdr.TotalQty;
            thdr.GrandTotal = hdr.TotalAmount;

            var party = new Party();
            party.Name = branch?.Name ?? "";

            var tdtls = new List<TransactionDtls>();
            foreach (var item in validDetails)
            {
                var td = new TransactionDtls();
                td.SL = item.SL;
                td.FK_ItemId = item.FK_ItemId;
                td.ItemName = item.ItemName;
                td.Barcode = item.Barcode;
                td.FK_UnitId = item.FK_UnitId;
                td.UnitName = item.UnitName;
                td.Quantity = item.Quantity;
                td.QtyPerUnit = item.QtyPerUnit;
                td.LineTotal = item.Amount;

                tdtls.Add(td);
            }

            PrintParameterDTO dto = new PrintParameterDTO(pdto.hdr, pdto.dtls, thdr, tdtls, party, AppData.CompanyDetails!, null);
            await CrudHelper.PrintInvoiceMethodLocal(Downloader, dto, Dialog, IsPreview);
        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod(ex.Message);
        }
    }

    async Task PrintPreviewMethod() => await PrintHelperMethod(true);
    async Task PrintMethod() => await PrintHelperMethod(false);

    private string Localizer(string key) => ArabicLocalization.Localizer(key);
}
