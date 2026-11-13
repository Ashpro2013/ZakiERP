using ZakiSAASERP.Models.Transactions.Inventory;
using Microsoft.AspNetCore.Components.Web;

namespace ZakiSAASERP.Shared.Pages.Transactions.Inventory;

public partial class MaterialTransferInView : ComponentBase, ITabAware
{
    [Parameter] public int TabId { get; set; }
    // Variables
    MaterialTransferInHdr hdr = new();
    List<MaterialTransferInDtls> dtls = new();
    MaterialTransferInDtls transaction = new();
    private WebUserPrivilege? privilege;
    Dictionary<int, decimal> _originalQty = new();

    // Masters
    List<BranchMaster> branches = new();
    List<StoreMaster> stores = new();
    List<CommonModel> unitlist = new();
    List<CommonModel> itemUnitlist = new();

    // UI State
    private bool _processing = false;
    bool DisablePrintButton = false;
    private int _activeTabIndex = 0;
    private MudTextField<string>? txtVoucherNo;
    private MudDatePicker? txtVoucherDate;
    private MudTextField<string>? txtRemarks;
    private MudNumericField<decimal>? txtQuantity;
    private MudNumericField<decimal>? txtRate;
    private MudTextField<string>? textField;

    private string strCurencyFormat => PermanentSettingsData.CurrencyFormat;
    private string strQuantityFormat => PermanentSettingsData.QuantityFormat;
    public bool IsAutoCode { get; set; } = PermanentSettingsData.AutoMaterialTransferInNo;
    public decimal Width { get; set; }
    public decimal TotalQty { get; set; }
    public bool IsLocked => false; // No approval for Material Transfer IN

    [Inject] public IBrowserViewportService ResizeService { get; set; }
    [Inject] IJSRuntime JSRuntime { get; set; }

    protected async override Task OnInitializedAsync()
    {
        try
        {
            var size = await ResizeService.GetCurrentBrowserWindowSizeAsync();
            Width = size.Width;

            // Load Masters
            if (AppData.privileges is not null)
            {
                privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "MaterialTransferIN");
            }
            branches = await api.GetAllBranches();
            stores = await api.GetAllStores();
            unitlist = await api.GetCombofill("Unit", 1);
            itemUnitlist = unitlist.ToList();

            // Default Row always on Desktop
            if (Width > 600)
            {
                AddNewRow();
            }
            else
            {
                // On Mobile, initialize transaction for entry but don't add to list
                transaction = new MaterialTransferInDtls
                {
                    FK_FromStoreId = GetValidStoreId(hdr.FK_FromBranchId),
                    FK_ToStoreId = GetValidStoreId(hdr.FK_ToBranchId)
                };
            }

            await NewClick();
            if (string.IsNullOrEmpty(hdr.EnteredBy)) hdr.EnteredBy = AppData.UserName;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod(ex.Message);
        }
    }

    private async Task NewClick()
    {
        try
        {
            hdr = new();
            hdr.MatTransInId = 0;
            hdr.EntryDate = DateTime.Now;
            hdr.VoucherDate = DateTime.Now;

            IsAutoCode = PermanentSettingsData.AutoMaterialTransferInNo;

            if (IsAutoCode)
            {
                hdr.VoucherNo = await api.GetNextMaterialTransferInCode();
                if (string.IsNullOrEmpty(hdr.VoucherNo) || hdr.VoucherNo == "0")
                {
                    var nextNo = await api.GetNewMaterialTransferInNo(AppData.BranchId);
                    hdr.VoucherNo = nextNo.ToString();
                    IsAutoCode = false;
                }
            }
            else
            {
                var nextNo = await api.GetNewMaterialTransferInNo(AppData.BranchId);
                hdr.VoucherNo = nextNo.ToString();
            }

            hdr.EnteredBy = AppData.UserName;

            // TO Branch is current (receiving branch)
            hdr.FK_ToBranchId = AppData.BranchId > 0 ? AppData.BranchId : (branches.FirstOrDefault()?.BranchId ?? 0);

            // FROM Branch defaults to another branch
            var possibleFrom = branches.FirstOrDefault(x => x.BranchId != hdr.FK_ToBranchId);
            if (possibleFrom is not null) hdr.FK_FromBranchId = possibleFrom.BranchId;

            dtls = new();

            if (Width > 600)
            {
                AddNewRow();
            }
            else
            {
                transaction = new MaterialTransferInDtls
                {
                    FK_FromStoreId = GetValidStoreId(hdr.FK_FromBranchId),
                    FK_ToStoreId = GetValidStoreId(hdr.FK_ToBranchId)
                };
            }

            DisablePrintButton = true;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    private void AddNewRow()
    {
        if (dtls.Count > 0)
        {
            var last = dtls.Last();
            if (last.FK_ItemId == 0) return;
        }

        var newRow = new MaterialTransferInDtls
        {
            SL = dtls.Count + 1,
            FK_FromStoreId = GetValidStoreId(hdr.FK_FromBranchId),
            FK_ToStoreId = GetValidStoreId(hdr.FK_ToBranchId)
        };
        dtls.Add(newRow);
        transaction = newRow;
    }

    private int GetValidStoreId(int branchId)
    {
        return stores.FirstOrDefault(x => x.FK_BranchId == branchId && x.IsDefaultStore)?.StoreId
                ?? stores.FirstOrDefault(x => x.FK_BranchId == branchId)?.StoreId
                ?? stores.FirstOrDefault(x => x.FK_BranchId == null && x.IsDefaultStore)?.StoreId
                ?? stores.FirstOrDefault(x => x.FK_BranchId == null)?.StoreId
                ?? 0;
    }

    private async Task OnFromBranchChanged(int value)
    {
        if (value == hdr.FK_ToBranchId)
        {
            snackbar.Add(Localizer("SameBranchError"), Severity.Warning);
            return;
        }
        hdr.FK_FromBranchId = value;

        // Update all detail rows' FROM stores
        foreach (var item in dtls)
        {
            item.FK_FromStoreId = GetValidStoreId(hdr.FK_FromBranchId);
            await UpdateItemStock(item);
        }
        snackbar.Add(Localizer("StoreChangedToDefault"), Severity.Success);
    }

    private async Task OnChangeFromStore(int storeId, MaterialTransferInDtls item)
    {
        item.FK_FromStoreId = storeId;
        await UpdateFromBranchStock(item);
    }

    private async Task OnChangeToStore(int storeId, MaterialTransferInDtls item)
    {
        item.FK_ToStoreId = storeId;
        await UpdateToBranchStock(item);
    }

    private async void OnChangeUnit(int unitId, MaterialTransferInDtls item)
    {
        await ChangeUnitAsync(unitId, item);
    }

    private async Task ChangeUnitAsync(int unitId, MaterialTransferInDtls item)
    {
        item.FK_UnitId = unitId;
        item.UnitName = unitlist.FirstOrDefault(x => x.Id == unitId)?.Name;

        item.QtyPerUnit = await GetQtyPerUnit(item.FK_ItemId, unitId);
        await UpdateItemStock(item);
        StateHasChanged();
    }

    private async Task BarcodeChanged(string barcode, MaterialTransferInDtls item)
    {
        // Search in FROM branch store
        int searchStoreId = item.FK_FromStoreId > 0 ? item.FK_FromStoreId : GetValidStoreId(hdr.FK_FromBranchId);

        var result = await Dialog.GetItemSearchMethod(barcode ?? string.Empty, "Product Search", searchStoreId);
        if (result is not null)
        {
            await ApplyItemFromSearch(result, item);
        }
    }

    private async Task ApplyItemFromSearch(ItemSelectionResult result, MaterialTransferInDtls targetItem)
    {
        string itemName = (PermanentSettingsData.ItemSearchMode == 2 && !string.IsNullOrEmpty(result.MultiUnitName))
                          ? result.MultiUnitName : result.ItemName;

        decimal qtyPerUnit = result.QtyPerUnit;
        if (qtyPerUnit <= 0) qtyPerUnit = 1;

        await MapItemToDetail(result.FK_ItemId, itemName, result.Barcode, result.FK_UnitId, result.UnitName,
                              qtyPerUnit, result.Rate ?? 0, targetItem.FK_FromStoreId, targetItem.FK_ToStoreId, targetItem);
    }

    private async Task MapItemToDetail(int itemId, string itemName, string barcode, int unitId, string unitName,
                                       decimal qtyPerUnit, decimal rate, int fromStoreId, int toStoreId, MaterialTransferInDtls targetItem)
    {
        targetItem.FK_ItemId = itemId;
        targetItem.ItemName = itemName;
        targetItem.Barcode = barcode;
        targetItem.FK_UnitId = unitId;
        targetItem.UnitName = unitName;
        targetItem.QtyPerUnit = qtyPerUnit;
        targetItem.Rate = rate;
        targetItem.FK_FromStoreId = fromStoreId > 0 ? fromStoreId : GetValidStoreId(hdr.FK_FromBranchId);
        targetItem.FK_ToStoreId = toStoreId > 0 ? toStoreId : GetValidStoreId(hdr.FK_ToBranchId);

        await UpdateItemStock(targetItem);

        await Task.Delay(100);
        await FocusById($"qty-{dtls.IndexOf(targetItem)}");
    }

    private async Task UpdateItemStock(MaterialTransferInDtls item)
    {
        await UpdateFromBranchStock(item);
        await UpdateToBranchStock(item);
    }

    private async Task UpdateFromBranchStock(MaterialTransferInDtls item)
    {
        if (item.FK_ItemId > 0 && item.FK_FromStoreId > 0)
        {
            var baseStock = await api.GetItemCurrentStock(item.FK_ItemId, item.FK_FromStoreId);
            if (hdr.MatTransInId > 0 && item.QtyPerUnit > 0 &&
                _originalQty.TryGetValue(item.MatTransInDtlsId, out decimal originalQty))
            {
                baseStock += (originalQty * item.QtyPerUnit);
            }
            item.FromBranchCurrentQty = (item.QtyPerUnit > 0) ? baseStock / item.QtyPerUnit : baseStock;
        }
    }

    private async Task UpdateToBranchStock(MaterialTransferInDtls item)
    {
        if (item.FK_ItemId > 0 && item.FK_ToStoreId > 0)
        {
            var baseStock = await api.GetItemCurrentStock(item.FK_ItemId, item.FK_ToStoreId);
            if (hdr.MatTransInId > 0 && item.QtyPerUnit > 0 &&
                _originalQty.TryGetValue(item.MatTransInDtlsId, out decimal originalQty))
            {
                baseStock -= (originalQty * item.QtyPerUnit);
            }
            item.ToBranchCurrentQty = (item.QtyPerUnit > 0) ? baseStock / item.QtyPerUnit : baseStock;
        }
    }

    private async Task<decimal> GetQtyPerUnit(int itemId, int unitId)
    {
        if (itemId == 0 || unitId == 0) return 1;
        var mu = await api.GetItemMultiUnit(itemId, unitId, "");
        return mu?.QTYPerUnit ?? 1;
    }

    bool ValidateDuplicateItems(MaterialTransferInDtls currentItem)
    {
        if (currentItem == null || currentItem.FK_ItemId == 0) return false;
        return dtls.Any(x => x != currentItem
                             && x.FK_ItemId == currentItem.FK_ItemId
                             && x.FK_UnitId == currentItem.FK_UnitId
                             && x.FK_FromStoreId == currentItem.FK_FromStoreId
                             && x.FK_ToStoreId == currentItem.FK_ToStoreId);
    }

    private async Task FocusById(string id)
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("eval", $"var e = document.getElementById('{id}'); if(e) {{ var inp = e.querySelector('input'); if(inp) {{ inp.focus(); if(inp.select) inp.select(); }} else {{ e.focus(); if(e.select) e.select(); }} }}");
        }
        catch { /* Ignore */ }
    }

    private async Task SaveOrUpdate()
    {
        if (_processing) return;
        if (privilege is not null && hdr.MatTransInId == 0 && !privilege.SaveData)
        {
            await Dialog.DialogMethod(Localizer("No...Privilege"));
            return;
        }
        if (privilege is not null && hdr.MatTransInId != 0 && !privilege.EditData)
        {
            await Dialog.DialogMethod(Localizer("No...Privilege"));
            return;
        }
        _processing = true;
        try
        {
            var validDetails = dtls.Where(x => !string.IsNullOrEmpty(x.ItemName) && x.Quantity > 0).ToList();
            if (!validDetails.Any())
            {
                snackbar.Add(Localizer("NoValidItemsToSave"), Severity.Warning);
                return;
            }

            // Check for Insufficient Quantity
            // Check for Insufficient Quantity
            var insufficientItems = validDetails.Where(x => !string.IsNullOrEmpty(ValidateQuantity(x.Quantity, x))).ToList();
            if (insufficientItems.Any())
            {
                bool? result2 = await DialogService.ShowMessageBox(
                    Localizer("Warning"),
                    $"{Localizer("CheckQuantity")} - {string.Join(", ", insufficientItems.Select(x => x.ItemName))}\n{Localizer("ProceedWithNegativeStock")}",
                    yesText: Localizer("Yes"), cancelText: Localizer("No"));

                if (result2 != true)
                {
                    _processing = false;
                    return;
                }
            }

            if (hdr.FK_FromBranchId == hdr.FK_ToBranchId)
            {
                snackbar.Add(Localizer("SameBranchError"), Severity.Warning);
                return;
            }

            // Validate stores exist
            var hasFromBranchStore = stores.Any(x => x.FK_BranchId == hdr.FK_FromBranchId || x.FK_BranchId == null);
            if (!hasFromBranchStore)
            {
                snackbar.Add(Localizer("NoValidStoreInSourceBranch"), Severity.Warning);
                return;
            }

            var hasToBranchStore = stores.Any(x => x.FK_BranchId == hdr.FK_ToBranchId || x.FK_BranchId == null);
            if (!hasToBranchStore)
            {
                snackbar.Add(Localizer("NoValidStoreInDestBranch"), Severity.Warning);
                return;
            }

            // Validation for Common Stores (Same Store in From/To)
            if (validDetails.Any(x => x.FK_FromStoreId == x.FK_ToStoreId))
            {
                snackbar.Add(Localizer("FromAndToStoreCannotBeSame"), Severity.Warning);
                return;
            }

            // Final Validation before Save
            if (validDetails.Any(x => ValidateDuplicateItems(x)))
            {
                snackbar.Add(Localizer("ItemAlreadyExists"), Severity.Warning);
                _processing = false;
                return;
            }

            // Clear Navigation Properties
            foreach (var d in validDetails)
            {
                d.Item = null;
                d.Unit = null;
                d.FromStore = null;
                d.ToStore = null;
                d.Header = null;
            }

            hdr.FK_UserId = AppData.UserId ?? 0;
            hdr.FK_FinancialYearId = AppData.FYearId ?? 0;
            hdr.MaterialTransferInDetails = validDetails;

            OutputResponse result;
            if (hdr.MatTransInId == 0)
            {
                result = await api.CreateMaterialTransferIn(hdr);
                if (string.IsNullOrEmpty(result.Message))
                {
                    hdr.MatTransInId = result.Id;
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
                    await Dialog.DialogMethod(result.Message);
                }
            }
            else
            {
                result = await api.UpdateMaterialTransferIn(hdr);
                if (string.IsNullOrEmpty(result.Message))
                {
                    DisablePrintButton = false;
                    snackbar.Add(Localizer("UpdateMessage"), Severity.Info);
                    await NewClick();
                }
                else
                {
                    await Dialog.DialogMethod(result.Message);
                }
            }
        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod(ex.Message);
        }
        finally
        {
            _processing = false;
        }
    }

    async Task PrintHelperMethod(bool IsPreview = false)
    {
        try
        {
            var validDetails = dtls.Where(x => !string.IsNullOrEmpty(x.ItemName)).ToList();
            if (hdr.MatTransInId == 0 || !validDetails.Any()) { return; }

            var pdto = await api.GetPrintDesignAsync(new PrintDesignReq(0, "Material_Transfer_IN"));
            if (pdto.hdr == null || !pdto.dtls.Any())
            {
                pdto = await api.GetPrintDesignAsync(new PrintDesignReq(0, "Material_Transfer_Request"));
                if (pdto.hdr == null || !pdto.dtls.Any()) { return; }
            }

            var thdr = hdr.CopyTo<TransactionHdr>();
            thdr.HdrNo = hdr.VoucherNo;
            thdr.HdrDate = hdr.VoucherDate ?? DateTime.Now;
            thdr.RefNo = hdr.ReferenceNo;
            thdr.Remarks = hdr.Remarks;
            thdr.InvoiceDate = (hdr.VoucherDate ?? DateTime.Now).ToString("dd-MM-yyyy");

            var toBranch = branches.FirstOrDefault(x => x.BranchId == hdr.FK_ToBranchId);
            var fromBranch = branches.FirstOrDefault(x => x.BranchId == hdr.FK_FromBranchId);

            if (fromBranch is not null)
            {
                thdr.FromBranch = fromBranch.Name;
            }

            thdr.TotalQuantity = hdr.TotalQty;
            thdr.GrandTotal = hdr.TotalAmount;

            var party = new Party();
            if (toBranch is not null)
            {
                party.Name = toBranch.Name;
            }

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
                td.ExRate = item.Rate;
                td.LineTotal = item.Amount;

                var fromStore = stores.FirstOrDefault(x => x.StoreId == item.FK_FromStoreId);
                var toStore = stores.FirstOrDefault(x => x.StoreId == item.FK_ToStoreId);

                td.FromStoreName = fromStore?.Name ?? "";
                td.ToStoreName = toStore?.Name ?? "";
                td.StoreName = td.FromStoreName + " - " + td.ToStoreName; // Keeping fallback logic if needed elsewhere

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

    private async Task Delete()
    {
        if (privilege is not null && !privilege.DeleteData)
        {
            await Dialog.DialogMethod(Localizer("No...Privilege"));
            return;
        }
        bool? result = await DialogService.ShowMessageBox(
           Localizer("Warning"),
           Localizer("ConfirmDelete"),
           yesText: Localizer("Yes"), cancelText: Localizer("No"));

        if (result == true)
        {
            var response = await api.DeleteMaterialTransferIn(hdr.MatTransInId);
            if (string.IsNullOrEmpty(response.Message))
            {
                snackbar.Add(Localizer("DeleteMessage"), Severity.Success);
                await NewClick();
            }
            else
            {
                await Dialog.DialogMethod(response.Message);
            }
        }
    }

    private void RowClickEvent(TableRowClickEventArgs<MaterialTransferInDtls> tableRowClickEventArgs)
    {
        transaction = tableRowClickEventArgs.Item;
    }

    private void DltTransaction(MaterialTransferInDtls item)
    {
        dtls.Remove(item);
        if (dtls.Count == 0) AddNewRow();
        TotalFindMethod();
    }

    private void TotalFindMethod()
    {
        TotalQty = dtls.Sum(x => x.Quantity);
        hdr.TotalAmount = dtls.Sum(x => x.Quantity * x.Rate);
    }

    private async Task OnBlurQtEvent(MaterialTransferInDtls item)
    {
        if (item.Quantity > 0 && item.Rate > 0)
        {
            item.Amount = item.Quantity * item.Rate;
        }
        TotalFindMethod();
    }

    private async Task TaskOnEnterMethod(KeyboardEventArgs e, int id, MaterialTransferInDtls item)
    {
        if (e.Code == "Enter" || e.Code == "NumpadEnter")
        {
            if (id == 2) // Quantity
            {
                await OnBlurQtEvent(item);
                await FocusById($"rate-{dtls.IndexOf(item)}");
            }
            else if (id == 4) // Rate
            {
                await OnBlurQtEvent(item);

                // Validation for Common Stores (Same Store in From/To)
                if (item.FK_FromStoreId == item.FK_ToStoreId)
                {
                    snackbar.Add(Localizer("FromAndToStoreCannotBeSame"), Severity.Warning);
                    return;
                }

                if (ValidateDuplicateItems(transaction))
                {
                    snackbar.Add(Localizer("ItemAlreadyExists"), Severity.Warning);
                    return;
                }
                AddNewRow();
                StateHasChanged();
                await Task.Delay(100);
                // Focus the barcode of the new row
                if (textField is not null) await textField.FocusAsync();
            }
        }
    }

    private async Task OnEnterMethod(KeyboardEventArgs e, MaterialTransferInDtls item)
    {
        if (e.Code == "Enter" || e.Code == "NumpadEnter")
        {
            await BarcodeChanged(item.Barcode, item);
        }
    }

    private void Keydown(KeyboardEventArgs e, int id)
    {
        // Placeholder for future keyboard navigation logic
    }

    private async Task SearchMethod()
    {
        await LoadValidation();
    }

    private async Task LoadValidation()
    {
        var result = await Dialog.TransactionSearchMethod("Material Transfer IN");
        if (result is not null && result.Id > 0)
        {
            var data = await api.GetMaterialTransferInById(result.Id);
            if (data is not null)
            {
                hdr = data;
                dtls = data.MaterialTransferInDetails;
                _originalQty.Clear();

                foreach (var item in dtls)
                {
                    if (item.MatTransInDtlsId > 0 && item.Quantity > 0)
                    {
                        _originalQty[item.MatTransInDtlsId] = item.Quantity;
                    }
                    if (item.FK_FromStoreId == 0) item.FK_FromStoreId = GetValidStoreId(hdr.FK_FromBranchId);
                    if (item.FK_ToStoreId == 0) item.FK_ToStoreId = GetValidStoreId(hdr.FK_ToBranchId);

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

                    if (item.QtyPerUnit <= 0) item.QtyPerUnit = await GetQtyPerUnit(item.FK_ItemId, item.FK_UnitId);
                    await UpdateItemStock(item);
                }

                transaction = dtls.FirstOrDefault() ?? new();
                if (dtls.Count == 0) AddNewRow();
                TotalFindMethod();
                DisablePrintButton = false;
            }
        }
    }

    private async Task OnBlurQtyPerUnit(MaterialTransferInDtls item)
    {
        await UpdateItemStock(item);
        StateHasChanged();
    }
    private string ValidateQuantity(decimal value, MaterialTransferInDtls item)
    {
        if (value <= 0) return Localizer("QuantityMustBeGreaterThanZero");
        if (value > item.FromBranchCurrentQty) return Localizer("InsufficientQuantity");
        return null;
    }

    // Mobile View Logic
    // Mobile View Logic
    private async Task AddLineItem()
    {
        if (string.IsNullOrEmpty(transaction.ItemName))
        {
            snackbar.Add(Localizer("Please Select Item"), Severity.Warning);
            return;
        }

        // Validation for Common Stores
        if (transaction.FK_FromStoreId == transaction.FK_ToStoreId)
        {
            snackbar.Add(Localizer("FromAndToStoreCannotBeSame"), Severity.Warning);
            return;
        }

        if (ValidateDuplicateItems(transaction))
        {
            snackbar.Add(Localizer("ItemAlreadyExists"), Severity.Warning);
            return;
        }

        // If it's a new item (not in list), add it
        if (!dtls.Contains(transaction))
        {
            transaction.SL = dtls.Any() ? dtls.Max(x => x.SL) + 1 : 1;
            dtls.Add(transaction);
        }
        // If it is already in the list (editing), changes are already reflected due to binding.

        // Reset for next entry
        transaction = new MaterialTransferInDtls
        {
            FK_FromStoreId = GetValidStoreId(hdr.FK_FromBranchId),
            FK_ToStoreId = GetValidStoreId(hdr.FK_ToBranchId)
        };

        StateHasChanged();
        await Task.Delay(100);
        // Focus logic
        await FocusById("mobile-barcode");
    }

    private void RowClicked(DataGridRowClickEventArgs<MaterialTransferInDtls> args)
    {
        transaction = args.Item;
        _activeTabIndex = 1; // Switch to Details Tab (Mobile)
        StateHasChanged();
    }

    private async Task MobileSearch()
    {
        // Use the existing search logic but triggered from mobile button
        await BarcodeChanged(transaction.Barcode, transaction);
    }

    private async Task AddNewMobileEntry()
    {
        // Standalone transaction for new entry
        transaction = new MaterialTransferInDtls
        {
            FK_FromStoreId = GetValidStoreId(hdr.FK_FromBranchId),
            FK_ToStoreId = GetValidStoreId(hdr.FK_ToBranchId)
        };

        _activeTabIndex = 1; // Switch to Details Tab
        StateHasChanged();
        await Task.Delay(100);
        await FocusById("mobile-barcode");
    }
}
