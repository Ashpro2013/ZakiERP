using ZakiSAASERP.Models.Transactions.Inventory;
using Microsoft.AspNetCore.Components.Web;

namespace ZakiSAASERP.Shared.Pages.Transactions.Inventory;

public partial class MaterialTransferRequestView : ComponentBase, ITabAware
{
    [Parameter] public int TabId { get; set; }
    // Variables
    MaterialTransferRequestHdr hdr = new();
    List<MaterialTransferRequestDtls> dtls = new();
    MaterialTransferRequestDtls transaction = new();
    private WebUserPrivilege? privilege;

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
    private MudTextField<string>? txtDescription;
    private MudTextField<string>? textField;
    private MudTextField<string>? txtBarcode;

    private string strCurencyFormat => PermanentSettingsData.CurrencyFormat;
    private string strQuantityFormat => PermanentSettingsData.QuantityFormat;
    public bool IsAutoCode { get; set; } = PermanentSettingsData.AutoMaterialTransferRequestNo;
    public decimal Width { get; set; }
    public decimal TotalQty { get; set; }
    public bool IsLocked => !string.IsNullOrEmpty(hdr.Status) && (hdr.Status == "Approved" || hdr.Status == "Rejected");

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
                privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "MaterialTransferRequest");
            }
            branches = await api.GetAllBranches();
            stores = await api.GetAllStores();
            unitlist = await api.GetCombofill("Unit", 1);
            itemUnitlist = unitlist.ToList();

            // Default Row always
            AddNewRow();

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
        hdr = new();
        hdr.MatTransReqId = 0;
        hdr.EntryDate = DateTime.Now;
        hdr.VoucherDate = DateTime.Now;

        IsAutoCode = PermanentSettingsData.AutoMaterialTransferRequestNo;

        if (IsAutoCode)
        {
            hdr.VoucherNo = await api.GetNextMaterialTransferRequestCode();
            if (string.IsNullOrEmpty(hdr.VoucherNo) || hdr.VoucherNo == "0")
            {
                var nextNo = await api.GetNewMaterialTransferRequestNo(AppData.BranchId);
                hdr.VoucherNo = nextNo.ToString();
                IsAutoCode = false;
            }
        }
        else
        {
            var nextNo = await api.GetNewMaterialTransferRequestNo(AppData.BranchId);
            hdr.VoucherNo = nextNo.ToString();
        }

        hdr.EnteredBy = AppData.UserName;

        // Default Branch
        hdr.FK_FromBranchId = AppData.BranchId > 0 ? AppData.BranchId : (branches.FirstOrDefault()?.BranchId ?? 0);

        var possibleTo = branches.FirstOrDefault(x => x.BranchId != hdr.FK_FromBranchId);
        if (possibleTo is not null) hdr.FK_ToBranchId = possibleTo.BranchId;

        dtls = new();
        AddNewRow(); // Adds blank row for Desktop

        // For Mobile, we start with a fresh standalone transaction
        transaction = new MaterialTransferRequestDtls()
        {
            FK_FromStoreId = GetValidStoreId(hdr.FK_FromBranchId)
        };
        DisablePrintButton = true;
    }
    private void AddNewRow()
    {
        if (dtls.Count > 0)
        {
            var last = dtls.Last();
            if (last.FK_ItemId == 0) return;
        }

        var newRow = new MaterialTransferRequestDtls
        {
            SL = dtls.Count + 1,
            FK_FromStoreId = GetValidStoreId(hdr.FK_FromBranchId)
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

    private async Task OnToBranchChanged(int value)
    {
        if (value == hdr.FK_FromBranchId)
        {
            snackbar.Add(Localizer("SameBranchError"), Severity.Warning);
            return;
        }
        hdr.FK_ToBranchId = value;
    }

    private async Task OnChangeStore(int storeId, MaterialTransferRequestDtls item)
    {
        item.FK_FromStoreId = storeId;
        await UpdateItemStock(item);
    }

    private async void OnChangeUnit(int unitId, MaterialTransferRequestDtls item)
    {
        await ChangeUnitAsync(unitId, item);
    }

    private async Task ChangeUnitAsync(int unitId, MaterialTransferRequestDtls item)
    {
        item.FK_UnitId = unitId;
        item.UnitName = unitlist.FirstOrDefault(x => x.Id == unitId)?.Name;

        item.QtyPerUnit = await GetQtyPerUnit(item.FK_ItemId, unitId);
        await UpdateItemStock(item);
        StateHasChanged();
    }

    private async Task BarcodeChanged(string barcode, MaterialTransferRequestDtls item)
    {
        // Determine Store for search (From Store of current line or Default)
        int searchStoreId = item.FK_FromStoreId > 0 ? item.FK_FromStoreId : GetValidStoreId(hdr.FK_FromBranchId);

        // Usage: GetItemSearchMethod(searchText, Title, StoreId)
        var result = await Dialog.GetItemSearchMethod(barcode ?? string.Empty, "Product Search", searchStoreId);
        if (result is not null)
        {
            await ApplyItemFromSearch(result, item);
        }
    }

    private async Task ApplyItemFromSearch(ItemSelectionResult result, MaterialTransferRequestDtls targetItem)
    {
        string itemName = (PermanentSettingsData.ItemSearchMode == 2 && !string.IsNullOrEmpty(result.MultiUnitName))
                          ? result.MultiUnitName : result.ItemName;

        decimal qtyPerUnit = result.QtyPerUnit;
        if (qtyPerUnit <= 0) qtyPerUnit = 1;

        await MapItemToDetail(result.FK_ItemId, itemName, result.Barcode, result.FK_UnitId, result.UnitName,
                              qtyPerUnit, result.Rate ?? 0, targetItem.FK_FromStoreId, targetItem);
    }

    private async Task MapItemToDetail(int itemId, string itemName, string barcode, int unitId, string unitName,
                                       decimal qtyPerUnit, decimal rate, int storeId, MaterialTransferRequestDtls targetItem)
    {
        targetItem.FK_ItemId = itemId;
        targetItem.ItemName = itemName;
        targetItem.Barcode = barcode;
        targetItem.FK_UnitId = unitId;
        targetItem.UnitName = unitName;
        targetItem.QtyPerUnit = qtyPerUnit;
        targetItem.Rate = rate;
        targetItem.FK_FromStoreId = storeId > 0 ? storeId : GetValidStoreId(hdr.FK_FromBranchId);

        await UpdateItemStock(targetItem);

        await Task.Delay(100);
        await Task.Delay(100);
        if (Width < 600) await FocusById("mobile-qty");
        else await FocusById($"qty-{dtls.IndexOf(targetItem)}");
    }

    private async Task UpdateItemStock(MaterialTransferRequestDtls item)
    {
        if (item.FK_ItemId > 0)
        {
            var baseStock = await api.GetItemCurrentStock(item.FK_ItemId, item.FK_FromStoreId);
            item.CurrentQty = (item.QtyPerUnit > 0) ? baseStock / item.QtyPerUnit : baseStock;
        }
    }

    private async Task<decimal> GetQtyPerUnit(int itemId, int unitId)
    {
        if (itemId == 0 || unitId == 0) return 1;
        var mu = await api.GetItemMultiUnit(itemId, unitId, "");
        return mu?.QTYPerUnit ?? 1;
    }

    bool ValidateDuplicateItems(MaterialTransferRequestDtls currentItem)
    {
        if (currentItem == null || currentItem.FK_ItemId == 0) return false;
        // Logic: Exclude current item instance, check for same Item + Unit + Store
        return dtls.Any(x => x != currentItem
                             && x.FK_ItemId == currentItem.FK_ItemId
                             && x.FK_UnitId == currentItem.FK_UnitId
                             && x.FK_FromStoreId == currentItem.FK_FromStoreId);
    }

    private async Task FocusById(string id)
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("eval", $"var e = document.getElementById('{id}'); if(e) {{ var inp = e.querySelector('input'); if(inp) {{ inp.focus(); if(inp.select) inp.select(); }} else {{ e.focus(); if(e.select) e.select(); }} }}");
        }
        catch { /* Ignore */ }
    }

    private async Task OnBarcodBlurEvent()
    {
        if (transaction.FK_ItemId == 0 && !string.IsNullOrEmpty(transaction.Barcode))
        {
            await BarcodeChanged(transaction.Barcode, transaction);
        }
    }

    private async Task SaveOrUpdate()
    {
        if (_processing) return;
        if (privilege is not null && hdr.MatTransReqId == 0 && !privilege.SaveData)
        {
            await Dialog.DialogMethod(Localizer("No...Privilege"));
            return;
        }
        if (privilege is not null && hdr.MatTransReqId != 0 && !privilege.EditData)
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

            if (hdr.FK_FromBranchId == hdr.FK_ToBranchId)
            {
                snackbar.Add(Localizer("SameBranchError"), Severity.Warning);
                return;
            }

            var hasToBranchStore = stores.Any(x => x.FK_BranchId == hdr.FK_ToBranchId || x.FK_BranchId == null);
            if (!hasToBranchStore)
            {
                snackbar.Add(Localizer("NoValidStoreInDestBranch"), Severity.Warning);
                return;
            }

            foreach (var item in validDetails)
            {
                if (ValidateDuplicateItems(item))
                {
                    snackbar.Add(Localizer("ItemAlreadyExists"), Severity.Warning);
                    _processing = false;
                    return;
                }
            }

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
            hdr.MaterialTransferRequestDetails = validDetails;

            OutputResponse result;
            if (hdr.MatTransReqId == 0)
            {
                result = await api.CreateMaterialTransferRequest(hdr);
                if (string.IsNullOrEmpty(result.Message))
                {
                    hdr.MatTransReqId = result.Id;
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
                result = await api.UpdateMaterialTransferRequest(hdr);
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
            var validDetails = dtls.Where(x => !string.IsNullOrEmpty(x.ItemName) && x.Quantity > 0).ToList();
            if (hdr.MatTransReqId == 0 || !validDetails.Any()) { return; }

            var pdto = await api.GetPrintDesignAsync(new PrintDesignReq(0, "Material_Transfer_Request"));
            if (pdto.hdr == null || !pdto.dtls.Any())
            {
                snackbar.Add("Print design 'Material_Transfer_Request' not found.", Severity.Warning);
                return;
            }

            var toBranch = branches.FirstOrDefault(x => x.BranchId == hdr.FK_ToBranchId);
            var fromBranch = branches.FirstOrDefault(x => x.BranchId == hdr.FK_FromBranchId);
            var fromBranchName = fromBranch?.Name ?? (hdr.FK_FromBranchId == AppData.BranchId ? branches.FirstOrDefault(x => x.BranchId == AppData.BranchId)?.Name : "");

            var headerDict = new Dictionary<string, object>
            {
                { "HdrNo", hdr.VoucherNo },
                { "InvoiceDate", (hdr.VoucherDate ?? DateTime.Now).ToString("dd-MM-yyyy") },
                { "FromBranch", fromBranchName ?? "" },
                { "ToBranch", toBranch?.Name ?? "" },
                { "GrandTotal", hdr.TotalAmount.ToString(strCurencyFormat) }
            };

            var detailList = new List<PrintDetail>();
            int sl = 1;
            foreach (var item in validDetails)
            {
                var store = stores.FirstOrDefault(x => x.StoreId == item.FK_FromStoreId);

                detailList.Add(new PrintDetail(
                    SL: sl++.ToString(),
                    ItemName: item.ItemName,
                    StoreName: store?.Name ?? "",
                    Quantity: item.Quantity.ToString(strQuantityFormat),
                    UnitName: item.UnitName,
                    QtyPerUnit: item.QtyPerUnit,
                    LineTotal: item.Amount.ToString(strCurencyFormat)
                ));
            }

            detailList.Add(new PrintDetail(
                SL: "",
                ItemName: "TOTAL",
                StoreName: "",
                Quantity: validDetails.Sum(x => x.Quantity).ToString(strQuantityFormat),
                UnitName: "",
                QtyPerUnit: null,
                LineTotal: validDetails.Sum(x => x.Amount).ToString(strCurencyFormat)
            ));

            var ReportDtls = new Dictionary<string, object> { { "Details", detailList.Cast<object>().ToList() } };

            var printHelper = new SyncfusionPrintHelper();
            byte[]? bytes = await printHelper.PrintReportMethodNew(Downloader, pdto.hdr, pdto.dtls, headerDict, ReportDtls, null, IsPreview);

            if (IsPreview && bytes is not null)
            {
                await Dialog.InvoicePreviewDialog(bytes);
            }
        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod(ex.Message);
        }
    }

    private record PrintDetail(string SL, string ItemName, string StoreName, string Quantity, string UnitName, decimal? QtyPerUnit, string LineTotal);
    async Task PrintPrviewMethod() => await PrintHelperMethod(true);
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
           Localizer("DeleteMessage"),
           yesText: Localizer("Yes"), cancelText: Localizer("No"));

        if (result == true)
        {
            var response = await api.DeleteMaterialTransferRequest(hdr.MatTransReqId);
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

    private void RowClickEvent(TableRowClickEventArgs<MaterialTransferRequestDtls> tableRowClickEventArgs)
    {
        transaction = tableRowClickEventArgs.Item;
    }

    private void DltTransaction(MaterialTransferRequestDtls item)
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

    private async Task OnBlurQtEvent(MaterialTransferRequestDtls item)
    {
        if (item.Quantity > 0 && item.Rate > 0)
        {
            item.Amount = item.Quantity * item.Rate;
        }
        TotalFindMethod();
    }

    private async Task TaskOnEnterMethod(KeyboardEventArgs e, int id, MaterialTransferRequestDtls item)
    {
        if (e.Code == "Enter" || e.Code == "NumpadEnter")
        {
            if (id == 2) // Quantity
            {
                await OnBlurQtEvent(item);
                if (txtRate is not null) await txtRate.FocusAsync();
            }
            else if (id == 4) // Rate
            {
                await OnBlurQtEvent(item);
                if (txtDescription is not null) await txtDescription.FocusAsync();
            }
            else if (id == 5) // Description
            {
                var isDuplicate = ValidateDuplicateItems(item);
                if (isDuplicate)
                {
                    snackbar.Add(Localizer("ItemAlreadyExists"), Severity.Warning);
                    return;
                }
                AddNewRow();
                StateHasChanged();
                await Task.Delay(100);
                if (textField is not null) await textField.FocusAsync();
            }
        }
    }

    private async Task OnEnterMethod(KeyboardEventArgs e, MaterialTransferRequestDtls item)
    {
        if (e.Code == "Enter" || e.Code == "NumpadEnter")
        {
            await BarcodeChanged(item.Barcode, item);
        }
    }

    private async Task Keydown(KeyboardEventArgs e, int id)
    {
        if (e.Code == "Enter" || e.Code == "NumpadEnter")
        {
            if (id == 1) // Voucher No
            {
                if (txtVoucherDate is not null) await txtVoucherDate.FocusAsync();
            }
        }
    }

    private async Task SearchMethod()
    {
        await LoadValidation();
    }

    private async Task LoadValidation()
    {
        var result = await Dialog.TransactionSearchMethod("Material Transfer Request");
        if (result is not null && result.Id > 0)
        {
            var data = await api.GetMaterialTransferRequestById(result.Id);
            if (data is not null)
            {
                hdr = data;
                dtls = data.MaterialTransferRequestDetails;
                dtls.ForEach(x => x.SL = dtls.IndexOf(x) + 1);

                foreach (var item in dtls)
                {
                    if (item.FK_FromStoreId == 0) item.FK_FromStoreId = GetValidStoreId(hdr.FK_FromBranchId);

                    if (string.IsNullOrEmpty(item.ItemName) && item.Item is not null) item.ItemName = item.Item.ItemName;
                    if (string.IsNullOrEmpty(item.Barcode) && item.Item is not null) item.Barcode = item.Item.Barcode;
                    if (string.IsNullOrEmpty(item.UnitName) && item.Unit is not null) item.UnitName = item.Unit.Name;

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

                if (dtls.Count == 0)
                {
                    AddNewRow();
                    IsEditing = false;
                }
                else
                {
                    transaction = dtls.FirstOrDefault();
                    IsEditing = true;
                }
                TotalFindMethod();
                DisablePrintButton = false;
            }
        }
    }
    private async Task OnBlurQtyPerUnit(MaterialTransferRequestDtls item)
    {
        await UpdateItemStock(item);
        StateHasChanged();
    }

    // --- Mobile Methods ---



    private bool IsEditing = false;

    private async Task AddLineItem()
    {
        if (string.IsNullOrEmpty(transaction.ItemName))
        {
            snackbar.Add(Localizer("Please Select Item"), Severity.Warning);
            return;
        }

        if (dtls.Contains(transaction))
        {
            // Already in list (Edit Mode) -> End Edit
            transaction = new MaterialTransferRequestDtls()
            {
                FK_FromStoreId = GetValidStoreId(hdr.FK_FromBranchId)
            };
        }
        else
        {
            // Standalone -> Add to list
            var last = dtls.LastOrDefault();
            if (last is not null && string.IsNullOrEmpty(last.ItemName))
            {
                dtls.Insert(dtls.Count - 1, transaction);
            }
            else
            {
                dtls.Add(transaction);
            }

            // Prepare next staging
            transaction = new MaterialTransferRequestDtls()
            {
                FK_FromStoreId = GetValidStoreId(hdr.FK_FromBranchId)
            };
        }
        IsEditing = false;
        TotalFindMethod();
    }

    private void RowClicked(DataGridRowClickEventArgs<MaterialTransferRequestDtls> args)
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
        // Determine Store for search (From Store of current line or Default)
        int searchStoreId = transaction.FK_FromStoreId > 0 ? transaction.FK_FromStoreId : GetValidStoreId(hdr.FK_FromBranchId);

        var result = await Dialog.GetItemSearchMethod(sVal, "Product Search", searchStoreId);
        if (result is not null)
        {
            await ApplyItemFromSearch(result, transaction);
        }
    }
}
