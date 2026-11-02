using AshproSAASERP.Models.Transactions.Inventory;
using Microsoft.AspNetCore.Components.Web;

namespace AshproSAASERP.Shared.Pages.Transactions.Inventory;

public partial class StockAdjustmentView : ComponentBase, ITabAware
{
    [Parameter] public int TabId { get; set; }
    #region Variables
    [Inject] IBrowserViewportService ResizeService { get; set; }
    [Inject] IJSRuntime JSRuntime { get; set; }

    // Data Models
    StockAdjustmentHdr hdr = new();
    List<StockAdjustmentDtls> dtls = new();
    StockAdjustmentDtls transaction = new();
    private WebUserPrivilege? privilege;

    // Lists
    List<StoreMaster> stores = new();
    List<BranchMaster> branches = new();

    List<CommonModel> unitlist = new();
    List<CommonModel> itemUnitlist = new();

    // UI State
    bool IsEditing = false;
    bool _processing = false;
    private int _activeTabIndex = 0;
    string strQuantityFormat => PermanentSettingsData.QuantityFormat;
    string strCurencyFormat => PermanentSettingsData.CurrencyFormat;
    bool IsAutoCode => PermanentSettingsData.AutoStockAdjustmentNo;
    private string _searchString;

    public int Width { get; set; }

    // UI References
    MudTextField<string> txtVoucherNo;
    MudDatePicker txtVoucherDate;
    MudTextField<string> txtRefNo;
    MudTextField<string> txtRemarks;

    MudTextField<string> txtBarcode;
    MudTextField<string> textField; // Grid Barcode
    MudNumericField<decimal> txtQuantity;
    MudNumericField<decimal> txtRate;
    MudTextField<string> txtDescription;

    #endregion

    #region Initialize
    protected async override Task OnInitializedAsync()
    {
        try
        {
            var size = await ResizeService.GetCurrentBrowserWindowSizeAsync();
            Width = size.Width;

            // Load Masters
            branches = await api.GetAllBranches();
            stores = await api.GetAllStores();
            if (AppData.privileges is not null)
            {
                privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "StockAdjustment");
            }
            unitlist = await api.GetCombofill("Unit", 1);
            unitlist = await api.GetCombofill("Unit", 1);
            itemUnitlist = unitlist.ToList();

            // Default Row always
            AddNewRow();

            await NewClick();
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod(ex.Message);
        }
    }
    #endregion

    #region Methods
    private async Task OnBranchChanged(int value)
    {
        if (hdr.FK_BranchId == value) return;

        if (dtls.Any(x => x.FK_ItemId != 0))
        {
            bool? result = await DialogService.ShowMessageBox(
                Localizer("Warning"),
                Localizer("ChangeBranchWarning"),
                yesText: Localizer("Yes"), cancelText: Localizer("No"));

            if (result != true)
            {
                // UI binding might need manual reset if we don't proceed
                StateHasChanged();
                return;
            }
        }

        hdr.FK_BranchId = value;

        // Reset stores in details to new branch defaults
        var newDefaultStore = GetValidStoreId(value);

        foreach (var item in dtls)
        {
            item.FK_StoreId = newDefaultStore;
            await UpdateItemStock(item, maintainNewQty: false);
        }
        TotalFindMethod();
        StateHasChanged();
    }
    private async Task FocusMethod()
    {
        // Validation: Check for duplicates before adding new row or leaving current
        if (ValidateDuplicateItems(transaction))
        {
            snackbar.Add(Localizer("ItemAlreadyExists"), Severity.Warning);
            return;
        }

        if (!dtls.Any() || !string.IsNullOrEmpty(dtls.LastOrDefault()!.ItemName))
        {
            AddNewRow();
        }
        StateHasChanged();
        IsEditing = false;
    }

    bool ValidateDuplicateItems(StockAdjustmentDtls currentItem)
    {
        if (currentItem == null || currentItem.FK_ItemId == 0) return false;
        // Logic: Exclude current item instance, check for same Item + Unit + Store
        return dtls.Any(x => x != currentItem
                             && x.FK_ItemId == currentItem.FK_ItemId
                             && x.FK_UnitId == currentItem.FK_UnitId
                             && x.FK_StoreId == currentItem.FK_StoreId);
    }

    void TotalFindMethod()
    {
        // Recalculate line totals if needed
        foreach (var item in dtls)
        {
            item.Amount = item.Quantity * item.Rate;
        }

        hdr.TotalAmount = dtls.Sum(x => x.Amount);
    }

    decimal TotalQty => dtls.Sum(x => x.Quantity);

    private Func<StockAdjustmentDtls, bool> _quickFilter => x =>
    {
        if (string.IsNullOrWhiteSpace(_searchString))
            return true;

        if (x.Barcode?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
            return true;
        if (x.ItemName?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
            return true;
        return false;
    };

    // --- Mobile Methods ---

    private async Task AddLineItem()
    {
        if (string.IsNullOrEmpty(transaction.ItemName))
        {
            snackbar.Add(Localizer("Please Select Item"), Severity.Warning);
            return;
        }

        if (transaction == dtls.LastOrDefault())
        {
            AddNewRow();
        }
        else
        {
            var last = dtls.LastOrDefault();
            if (last is not null && string.IsNullOrEmpty(last.ItemName))
            {
                transaction = last;
            }
            else
            {
                AddNewRow();
            }
        }
    }

    private void RowClicked(DataGridRowClickEventArgs<StockAdjustmentDtls> args)
    {
        transaction = args.Item;
        IsEditing = true;
        StateHasChanged();
    }

    private async Task MobileSearch()
    {
        await MobileSearchMethod(transaction.Barcode ?? "");
    }

    private async Task MobileSearchMethod(string sVal)
    {
        var selectedProduct = await Dialog.GetItemSearchMethod(sVal);
        if (selectedProduct is not null)
        {
            await ApplyItemFromSearch(selectedProduct);
        }
        StateHasChanged();
    }

    private async Task AddNewMobileEntry()
    {
        var last = dtls.LastOrDefault();
        if (last is not null && string.IsNullOrEmpty(last.ItemName))
        {
            transaction = last;
        }
        else
        {
            AddNewRow(); // Adds to dtls and sets transaction = newRow
        }

        // Ensure UI updates
        StateHasChanged();

        // Focus barcode?
        await Task.Delay(100);
        if (Width < 600) await FocusById("mobile-qty"); // Or barcode field if available.. 
        // Mobile view has barcode field as first item in form.
        // But Wait, usually we want to scan next item. 
    }

    #region Desktop
    private async Task OnChangeStore(int iStoreId, StockAdjustmentDtls item)
    {
        item.FK_StoreId = iStoreId;
        if (item.FK_ItemId > 0)
        {
            await UpdateItemStock(item, maintainNewQty: true);
        }
        TotalFindMethod();
        StateHasChanged();
    }

    private async Task RowClickEvent(TableRowClickEventArgs<StockAdjustmentDtls> args)
    {
        transaction = args.Item!;
        await RefreshUnitList(transaction.FK_ItemId);
        IsEditing = false;
    }

    private async Task OnBlurEvent(FocusEventArgs arg) => await FocusMethod();
    private async Task Keydown(KeyboardEventArgs e, int fieldId)
    {
        if (e.Key == "Enter" || e.Key == "ArrowDown")
        {
            await DownFocusMethod(fieldId);
        }
        else if (e.Key == "ArrowUp")
        {
            if (fieldId == 2) await txtVoucherDate.FocusAsync();
            // Add more nav if needed
        }
    }

    private async Task DownFocusMethod(int fieldId)
    {
        if (fieldId == 1) await txtVoucherDate.FocusAsync();
        else if (fieldId == 2 && txtBarcode is not null) await txtBarcode.FocusAsync(); // Removed Store focus
    }
    #endregion

    #region Common
    private async Task DltTransaction(StockAdjustmentDtls item)
    {
        dtls.Remove(item);
        if (!dtls.Any())
        {
            await FocusMethod();
        }
        int sl = 1;
        dtls.ForEach(x => x.SL = sl++);
        TotalFindMethod();
        StateHasChanged();
    }

    private async Task OnChangeUnit(int iUnitId, StockAdjustmentDtls item)
    {
        if (item.FK_ItemId == 0 || IsEditing) { return; }
        item.FK_UnitId = iUnitId;
        item.UnitName = unitlist.FirstOrDefault(x => x.Id == iUnitId)?.Name;

        item.QtyPerUnit = await GetQtyPerUnit(item.FK_ItemId, iUnitId);

        // Refresh Stock Display for new Unit
        await UpdateItemStock(item, maintainNewQty: false);

        StateHasChanged();
    }



    private async Task OnEnterMethod(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            if (string.IsNullOrEmpty(transaction.Barcode))
            {
                await BarcodeChanged("");
            }
            else
            {
                await BarcodeChanged(transaction.Barcode);
            }
            await txtQuantity.FocusAsync();
            StateHasChanged();
        }
    }

    private async Task OnBarcodBlurEvent(FocusEventArgs arg)
    {
        if (transaction.FK_ItemId == 0 || IsEditing) { return; }
        // Logic to refresh item details if barcode changed manually
    }


    private async Task BarcodeChanged(string sVal, StockAdjustmentDtls targetItem = null)
    {
        var workingItem = targetItem ?? transaction;

        if (string.IsNullOrEmpty(sVal) && IsEditing) { return; }

        // Smart Search: Only for ItemCode mode (mode 1)
        if (PermanentSettingsData.ItemSearchMode == 1 && !string.IsNullOrWhiteSpace(sVal))
        {
            var pagination = new PaginationModel(1, 2, "ItemCode", "asc", sVal);
            var searchResult = await api.GetItemPDetails(pagination);

            if (searchResult.TotalCount == 1 && searchResult.PItems.Count == 1)
            {
                // Single match found - auto select
                var selectedItem = searchResult.PItems[0];

                decimal qtyPerUnit = await GetQtyPerUnit(selectedItem.ItemId, selectedItem.FK_UnitId ?? 0);
                int stockLookupStoreId = workingItem.FK_StoreId > 0 ? workingItem.FK_StoreId : GetValidStoreId(hdr.FK_BranchId);

                await MapItemToDetail(selectedItem.ItemId, selectedItem.ItemName, selectedItem.Barcode,
                                      selectedItem.FK_UnitId ?? 0, selectedItem.Unit?.Name ?? "Unit",
                                      qtyPerUnit, selectedItem.PurchaseRate ?? 0, stockLookupStoreId, workingItem);
                return;
            }
        }

        // Mode 2 or Multiple Matches: Open Dialog
        // Use row store ID if set, otherwise try to use valid store for branch
        int searchStoreId = workingItem.FK_StoreId > 0 ? workingItem.FK_StoreId : GetValidStoreId(hdr.FK_BranchId);
        var selectedProduct = await Dialog.GetItemSearchMethod(sVal, "Product Search", searchStoreId);
        if (selectedProduct is not null)
        {
            ApplyItemFromSearch(selectedProduct, workingItem);
        }
    }

    private async Task RefreshUnitList(int itemId)
    {
        if (itemId == 0)
        {
            unitlist = itemUnitlist.ToList();
            if (transaction.FK_UnitId == 0 && unitlist.Any())
            {
                transaction.FK_UnitId = unitlist.First().Id;
                transaction.UnitName = unitlist.First().Name;
            }
            return;
        }

        var fullItem = await api.GetItemById(itemId);
        if (fullItem is not null)
        {
            unitlist.Clear();
            // Add base unit
            if (fullItem.Unit is not null)
            {
                unitlist.Add(new CommonModel { Id = fullItem.Unit.UnitId, Name = fullItem.Unit.Name });
            }

            // Add multi units
            if (fullItem.MultiUnits is not null)
            {
                foreach (var mu in fullItem.MultiUnits)
                {
                    if (mu.Unit is not null && !unitlist.Any(x => x.Id == mu.Unit.UnitId))
                    {
                        unitlist.Add(new CommonModel { Id = mu.Unit.UnitId, Name = mu.Unit.Name });
                    }
                }
            }
            // itemUnitlist = unitlist.ToList(); // Do not overwrite generic cache here

            // Re-bind unit name if needed
            // if (transaction.FK_UnitId == 0 && unitlist.Any()) // Removed to avoid overwriting existing unit logic
            // {
            //     transaction.FK_UnitId = unitlist.First().Id;
            //     transaction.UnitName = unitlist.First().Name;
            // }
        }
    }

    private async Task ApplyItemFromSearch(ItemSelectionResult result, StockAdjustmentDtls targetItem = null)
    {
        IsEditing = true;
        var workingItem = targetItem ?? transaction;

        string itemName = (PermanentSettingsData.ItemSearchMode == 2 && !string.IsNullOrEmpty(result.MultiUnitName))
                          ? result.MultiUnitName : result.ItemName;

        decimal qtyPerUnit = await GetQtyPerUnit(result.FK_ItemId, result.FK_UnitId);
        if (qtyPerUnit == 1 && result.QtyPerUnit > 0) qtyPerUnit = result.QtyPerUnit;

        await MapItemToDetail(result.FK_ItemId, itemName, result.Barcode, result.FK_UnitId, result.UnitName,
                              qtyPerUnit, result.Rate ?? 0, workingItem.FK_StoreId, workingItem);
    }

    private async Task ApplyItemFromBarcode(ItemMultiUnit multiUnit, StockAdjustmentDtls targetItem = null)
    {
        IsEditing = true;
        var workingItem = targetItem ?? transaction;

        var unitName = itemUnitlist.FirstOrDefault(u => u.Id == multiUnit.FK_UnitId)?.Name ?? "Unit";

        await MapItemToDetail(multiUnit.FK_ItemId, multiUnit.MultiUnitName, multiUnit.Barcode,
                              multiUnit.FK_UnitId, unitName, multiUnit.QTYPerUnit ?? 1,
                              multiUnit.Rate ?? 0, workingItem.FK_StoreId, workingItem);
    }

    private async Task MapItemToDetail(int itemId, string itemName, string barcode, int unitId, string unitName,
                                       decimal qtyPerUnit, decimal rate, int storeId, StockAdjustmentDtls targetItem)
    {
        // If targetItem is null, we create a new one?? No, calling methods should provide valid target.
        // But for safety:
        if (targetItem == null)
        {
            AddNewRow();
            targetItem = transaction;
        }

        targetItem.FK_ItemId = itemId;
        targetItem.ItemName = itemName;
        targetItem.Barcode = barcode;
        targetItem.FK_UnitId = unitId;
        targetItem.UnitName = unitName;
        targetItem.QtyPerUnit = qtyPerUnit;
        targetItem.Rate = rate;
        targetItem.FK_StoreId = storeId > 0 ? storeId : GetValidStoreId(hdr.FK_BranchId); // Ensure store is valid
        targetItem.Description = "";

        // Stock update handled by helper
        await UpdateItemStock(targetItem, maintainNewQty: false);

        // Focus logic
        await Task.Delay(100);
        if (Width < 600) await FocusById("mobile-qty");
        else await FocusById($"qty-{dtls.IndexOf(targetItem)}");
        await RefreshUnitList(itemId);

        StateHasChanged();
    }

    private async Task SaveOrUpdate()
    {
        if (_processing) return;
        if (privilege is not null && hdr.StockAdjId == 0 && !privilege.SaveData)
        {
            Dialog.DialogMethod(Localizer("No...Privilege"));
            return;
        }
        if (privilege is not null && hdr.StockAdjId != 0 && !privilege.EditData)
        {
            Dialog.DialogMethod(Localizer("No...Privilege"));
            return;
        }
        _processing = true;
        try
        {
            // Use temporary variable to preserve original dtls if save fails
            var validDetails = dtls.Where(x => !string.IsNullOrEmpty(x.ItemName) && x.Quantity != 0).ToList();
            if (!validDetails.Any())
            {
                snackbar.Add(Localizer("NoValidItemsToSave"), Severity.Warning);
                return;
            }


            hdr.FK_UserId = AppData.UserId ?? 0;
            hdr.FK_FinancialYearId = AppData.FYearId ?? 0;
            // hdr.EnteredBy moved to NewClick to allow manual edit

            // Map details to child collection if needed, or send separate. 
            // The API expects Header with Details.
            hdr.StockAdjustmentDetails = validDetails;

            // Final Validation before Save
            if (validDetails.Any(x => ValidateDuplicateItems(x)))
            {
                snackbar.Add(Localizer("ItemAlreadyExists"), Severity.Warning);
                _processing = false;
                return;
            }

            if (hdr.StockAdjId == 0)
            {
                var result = await api.CreateStockAdjustment(hdr);
                if (string.IsNullOrEmpty(result.Message))
                {
                    hdr.StockAdjId = result.Id;
                    snackbar.Add(Localizer("SaveMessage"), Severity.Success);
                    await NewClick();
                }
                else
                {
                    Dialog.DialogMethod(result.Message);
                }
            }
            else
            {
                var result = await api.UpdateStockAdjustment(hdr);
                if (string.IsNullOrEmpty(result.Message))
                {
                    snackbar.Add(Localizer("UpdateMessage"), Severity.Info);
                    await NewClick();
                }
                else
                {
                    Dialog.DialogMethod(result.Message);
                }
            }

        }
        catch (Exception ex)
        {
            Dialog.DialogMethod(ex.Message);
        }
        finally
        {
            _processing = false;
        }
    }

    private async Task Delete()
    {
        if (hdr.StockAdjId == 0) return;
        if (privilege is not null && !privilege.DeleteData)
        {
            Dialog.DialogMethod(Localizer("No...Privilege"));
            return;
        }
        if (await Dialog.DeleteDialogMethod())
        {
            var result = await api.DeleteStockAdjustment(hdr.StockAdjId);
            if (string.IsNullOrEmpty(result.Message))
            {
                snackbar.Add(Localizer("DeletedMessage"), Severity.Error);
                await NewClick();
            }
            else
            {
                Dialog.DialogMethod(result.Message);
            }
        }
    }

    private async Task Print()
    {
        if (hdr.StockAdjId == 0) return;
        // Standard Print method (PrintDesign or Report)
        // Since no specific print design ID is known, we use a dialog or default ID.
        if (hdr.StockAdjId > 0)
        {
            //await Dialog.PrintDialogMethod("StockAdjustment", hdr.StockAdjId);
        }
    }

    private async Task NewClick()
    {
        hdr = new();
        hdr.StockAdjId = 0;
        hdr.EntryDate = DateTime.Now;
        hdr.VoucherDate = DateTime.Now;

        // Check if auto-code is enabled
        if (PermanentSettingsData.AutoStockAdjustmentNo)
        {
            // Auto-generate formatted code (SA0001, SA0002, etc.)
            hdr.VoucherNo = await api.GetNextStockAdjustmentCode();
        }
        else
        {
            // Suggest next manual number (largest + 1)
            var nextNo = await api.GetNewStockAdjustmentDocNo(AppData.BranchId);
            hdr.VoucherNo = nextNo.ToString();
        }

        hdr.EnteredBy = AppData.UserName;

        // Default Branch
        hdr.FK_BranchId = AppData.BranchId > 0 ? AppData.BranchId : (branches.FirstOrDefault()?.BranchId ?? 0);

        // Default Store based on Branch
        // if (stores.Any()) hdr.FK_StoreId = stores.First().StoreId;

        dtls = new();
        AddNewRow();
    }
    private void OnBlurQtEvent(FocusEventArgs arg)
    {
        if (transaction.FK_ItemId == 0) { return; }

        // Logic: Adj Qty = New Qty - Current Qty
        transaction.Quantity = transaction.NewQty - transaction.CurrentQty;
        transaction.Amount = transaction.Quantity * transaction.Rate; // Calc Amount
        TotalFindMethod(); // Update Header Total
        StateHasChanged();

    }

    private async Task TaskOnEnterMethod(KeyboardEventArgs e, int index)
    {
        if (e.Key == "Enter")
        {
            switch (index)
            {
                case 2: // New Qty
                    await txtRate.FocusAsync();
                    await txtRate.SelectAsync();
                    break;
                case 4: // Cost (Rate)
                    await txtDescription.FocusAsync();
                    break;
                case 5: // Reason
                    await FocusMethod(); // Add new row
                    break;
                default:
                    break;
            }
            IsEditing = true;
            TotalFindMethod();
        }
    }

    // Load Existing Details Logic (need to populate Current/New Qty)
    private async Task SearchMethod()
    {
        var result = await Dialog.TransactionSearchMethod("Stock Adjustment");
        if (result is not null)
        {
            var dto = await api.GetStockAdjustmentById(result.Id);
            if (dto == null) return;
            hdr = dto;
            snackbar.Add("Caution: You are editing a posted transaction. Updating will reverse previous stock effects.", Severity.Warning);
            dtls = hdr.StockAdjustmentDetails;

            // Populate logic for Existing Records
            foreach (var item in dtls)
            {
                // Fetch Current Stock (Base) using Exclude Logic
                if (item.FK_StoreId == 0 && stores.Any()) item.FK_StoreId = stores.First().StoreId;
                var baseStock = await api.GetItemCurrentStock(item.FK_ItemId, item.FK_StoreId, hdr.StockAdjId);

                // Convert to Unit
                item.CurrentQty = item.QtyPerUnit > 0 ? baseStock / item.QtyPerUnit : baseStock;

                // Calc NewQty based on CurrentQty + Adjustment
                item.NewQty = item.CurrentQty + item.Quantity;
            }

            if (!dtls.Any()) AddNewRow();
            else
            {
                int sl = 1;
                dtls.ForEach(x => x.SL = sl++);
                transaction = dtls.Last();
            }
            TotalFindMethod();
            StateHasChanged();
        }
    }


    private void AddNewRow()
    {
        transaction = new();
        transaction.SL = dtls.Count + 1;
        if (unitlist.Any())
        {
            transaction.UnitName = unitlist.FirstOrDefault()!.Name;
            transaction.FK_UnitId = unitlist.FirstOrDefault()!.Id;
        }

        transaction.FK_StoreId = GetValidStoreId(hdr.FK_BranchId);

        dtls.Add(transaction);

    }

    private int GetValidStoreId(int branchId)
    {
        return stores.FirstOrDefault(x => x.FK_BranchId == branchId && x.IsDefaultStore)?.StoreId
               ?? stores.FirstOrDefault(x => x.FK_BranchId == branchId)?.StoreId
               ?? stores.FirstOrDefault(x => x.FK_BranchId == null && x.IsDefaultStore)?.StoreId
               ?? stores.FirstOrDefault(x => x.FK_BranchId == null)?.StoreId
               ?? 0;
    }

    private async Task<decimal> GetQtyPerUnit(int itemId, int unitId)
    {
        if (itemId == 0 || unitId == 0) return 1;
        var mu = await api.GetItemMultiUnit(itemId, unitId, "");
        return mu?.QTYPerUnit ?? 1;
    }

    private async Task UpdateItemStock(StockAdjustmentDtls item, bool maintainNewQty)
    {
        if (item.FK_ItemId > 0)
        {
            if (item.FK_StoreId == 0) item.FK_StoreId = GetValidStoreId(hdr.FK_BranchId);

            var baseStock = await api.GetItemCurrentStock(item.FK_ItemId, item.FK_StoreId, hdr.StockAdjId);
            item.CurrentQty = (item.QtyPerUnit > 0) ? baseStock / item.QtyPerUnit : baseStock;

            if (maintainNewQty)
            {
                // Maintain Physical Count (NewQty), Recalc Adj
                item.Quantity = item.NewQty - item.CurrentQty;
            }
            else
            {
                // Reset Adjustment (NewQty = CurrentQty)
                item.NewQty = item.CurrentQty;
                item.Quantity = 0;
            }
            item.Amount = item.Quantity * item.Rate;
        }
    }



    private async Task FocusById(string id)
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("eval", $"var e = document.getElementById('{id}'); if(e) {{ var inp = e.querySelector('input'); if(inp) {{ inp.focus(); if(inp.select) inp.select(); }} else {{ e.focus(); if(e.select) e.select(); }} }}");
        }
        catch { /* Ignore */ }
    }
    #endregion
    #endregion
}
