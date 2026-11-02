using AshproSAASERP.Models.Transactions.Inventory;
using FileDownloader;

namespace AshproSAASERP.Shared.Pages.Transactions.Inventory;

public partial class MaterialTransferApprovalDetails
{
    [CascadingParameter] IMudDialogInstance MudDialog { get; set; }
    [Parameter] public int id { get; set; } // Kept as lowercase 'id' to match previous parameter name usage if any

    [Inject] ModelService Dialog { get; set; }
    [Inject] Downloader Downloader { get; set; }

    private bool isLoading = true;
    private MaterialTransferRequestHdr? request;
    private List<StoreMaster> allStores = new();
    private List<BranchMaster> allBranches = new();

    // Mobile State
    private MaterialTransferRequestDtls? selectedMobileItem;
    private bool isMobileEditing => selectedMobileItem is not null;

    private void SelectMobileItem(MaterialTransferRequestDtls item)
    {
        selectedMobileItem = item;
    }

    private void ClearMobileSelection()
    {
        selectedMobileItem = null;
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadData();
        await LoadItemStocks();
    }

    private async Task LoadData()
    {
        try
        {
            isLoading = true;
            allStores = await api.GetAllStores();
            allBranches = await api.GetAllBranches();
            request = await api.GetMaterialTransferRequestById(id);

            if (request is not null)
            {
                // Apply ItemSearchMode Logic for ALL viewing scenarios (Client-Side)
                foreach (var item in request.MaterialTransferRequestDetails)
                {
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
                }

                if (request.Status == "Opened")
                {
                    // Initialize ApprovedQuantity with RequestedQuantity if 0
                    foreach (var item in request.MaterialTransferRequestDetails)
                    {
                        if (item.ApprovedQuantity == 0)
                        {
                            item.ApprovedQuantity = item.Quantity;
                            item.ApprovedAmount = item.Quantity * item.Rate;
                        }

                        // Always default to the Branch's Default Store (or first available)
                        var stores = GetStoresForBranch(request.FK_ToBranchId);
                        var defaultStore = stores.FirstOrDefault(s => s.IsDefaultStore) ?? stores.FirstOrDefault();

                        if (defaultStore is not null)
                        {
                            // If user explicitly wants "Always select", we overwrite even if something was there (or if it was 0/null)
                            // This ensures consistent behavior for Approver.
                            if (item.FK_ToStoreId == null || item.FK_ToStoreId == 0)
                            {
                                item.FK_ToStoreId = defaultStore.StoreId;
                            }

                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            snackbar.Add(ArabicLocalization.Localizer("ErrorLoadingData") + ": " + ex.Message, Severity.Error);
        }
        finally
        {
            isLoading = false;
        }
    }

    private string GetBranchName(int branchId) => allBranches.FirstOrDefault(b => b.BranchId == branchId)?.Name ?? "";
    private string GetStoreName(int storeId) => allStores.FirstOrDefault(s => s.StoreId == storeId)?.Name ?? "";

    private IEnumerable<StoreMaster> GetStoresForBranch(int branchId)
    {
        return allStores.Where(s => s.FK_BranchId == branchId || s.FK_BranchId == null);
    }

    private Dictionary<(int ItemId, int StoreId), decimal> itemStocks = new();

    private decimal GetCurrentStock(int itemId, int? storeId, decimal qtyPerUnit = 1)
    {
        if (storeId == null) return 0;
        if (!itemStocks.TryGetValue((itemId, storeId.Value), out var baseStock)) return 0;

        // Calculate display stock based on QtyPerUnit (same as Stock Adjustment)
        return (qtyPerUnit > 0) ? baseStock / qtyPerUnit : baseStock;
    }

    private async Task LoadItemStocks()
    {
        if (request == null) return;
        if (request == null) return;

        // Load stock from ToStore (destination store) 
        foreach (var x in request.MaterialTransferRequestDetails.Where(x => x.FK_ToStoreId is not null))
        {
            var qty = await api.GetItemCurrentStock(x.FK_ItemId, x.FK_ToStoreId.Value);
            itemStocks[(x.FK_ItemId, x.FK_ToStoreId.Value)] = qty;
        }

        // Load stock from FromStore (source store) for validation
        foreach (var x in request.MaterialTransferRequestDetails.Where(x => x.FK_FromStoreId > 0))
        {
            var qty = await api.GetItemCurrentStock(x.FK_ItemId, x.FK_FromStoreId);
            itemStocks[(x.FK_ItemId, x.FK_FromStoreId)] = qty;
        }
    }

    private void Close()
    {
        MudDialog.Cancel();
    }


    private bool _isSaving = false;

    private async Task ApproveRequest()
    {
        if (request == null) return;
        if (_isSaving) return;
        _isSaving = true;
        StateHasChanged();

        try
        {

            // Validation: Check if all items have ToStore
            if (request.MaterialTransferRequestDetails.Any(x => x.FK_ToStoreId == null || x.FK_ToStoreId == 0))
            {
                snackbar.Add(ArabicLocalization.Localizer("ToStoreRequired"), Severity.Warning);
                snackbar.Add(ArabicLocalization.Localizer("ToStoreRequired"), Severity.Warning);
                return;
            }

            // Validation: Check if FromStore and ToStore are the same
            if (request.MaterialTransferRequestDetails.Any(x => x.FK_ToStoreId == x.FK_FromStoreId))
            {
                snackbar.Add(ArabicLocalization.Localizer("SameStoreError"), Severity.Warning);
                return;
            }

            // Validation: Check if any item exceeds stock in the Supplying Store (ToStore)
            var stockErrors = request.MaterialTransferRequestDetails
                .Where(x => x.FK_ToStoreId.HasValue && x.ApprovedQuantity > GetCurrentStock(x.FK_ItemId, x.FK_ToStoreId, x.QtyPerUnit))
                .ToList();

            if (stockErrors.Any())
            {
                var confirm = await DialogService.ShowMessageBox(
                    ArabicLocalization.Localizer("StockWarningTitle"),
                    ArabicLocalization.Localizer("StockWarningMessage"),
                    yesText: ArabicLocalization.Localizer("Yes"),
                    cancelText: ArabicLocalization.Localizer("No"));

                if (confirm != true) return;
            }


            var userId = AppData.UserId ?? 0;
            // Map details to a list to send
            var detailsToSend = request.MaterialTransferRequestDetails.Select(x => new MaterialTransferRequestDtls
            {
                MatTransReqDtlsId = x.MatTransReqDtlsId,
                ApprovedQuantity = x.ApprovedQuantity,
                FK_ToStoreId = x.FK_ToStoreId,
                ApproverComment = x.ApproverComment
            }).ToList();

            var response = await api.ApproveMaterialTransferRequest(request.MatTransReqId, userId, detailsToSend);

            if (string.IsNullOrEmpty(response.Message))
            {
                snackbar.Add(ArabicLocalization.Localizer("ApprovedSuccessfully"), Severity.Success);

                // Ask if user wants to print
                if (await Dialog.DeleteDialogMethod(ArabicLocalization.Localizer("PrintDialog")))
                {
                    await PrintMethod();
                }

                MudDialog.Close(DialogResult.Ok(true));
            }
            else
            {
                snackbar.Add(response.Message, Severity.Error);
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

    private async Task RejectRequest()
    {
        if (request == null) return;
        if (_isSaving) return;
        _isSaving = true;
        StateHasChanged();

        try
        {

            var parameters = new DialogParameters();
            parameters.Add("FormTitle", ArabicLocalization.Localizer("EnterRejectionReason").ToString());
            parameters.Add("ShowCancel", true);

            var dialog = await DialogService.ShowAsync<AshproSAASERP.Shared.SubWindows.InputDialog>(ArabicLocalization.Localizer("RejectRequest"), parameters);
            var result = await dialog.Result;

            if (!result.Canceled && result.Data is string reason && !string.IsNullOrWhiteSpace(reason))
            {

                var userId = AppData.UserId ?? 0;
                var response = await api.RejectMaterialTransferRequest(request.MatTransReqId, userId, reason);

                if (string.IsNullOrEmpty(response.Message))
                {
                    snackbar.Add(ArabicLocalization.Localizer("RejectedSuccessfully"), Severity.Success);

                    // Ask if user wants to print
                    if (await Dialog.DeleteDialogMethod(ArabicLocalization.Localizer("PrintDialog")))
                    {
                        await PrintMethod();
                    }

                    MudDialog.Close(DialogResult.Ok(true));
                }
                else
                {
                    snackbar.Add(response.Message, Severity.Error);
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

    [Inject] IJSRuntime js { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && request is not null && request.MaterialTransferRequestDetails.Count > 0)
        {
            await Task.Delay(500);
            await FocusElement("qty-0");
        }
    }

    private async Task HandleKeyDown(Microsoft.AspNetCore.Components.Web.KeyboardEventArgs e, int index)
    {
        if (e.Key == "Enter" || e.Key == "NumpadEnter")
        {
            var nextIndex = index + 1;
            if (nextIndex < request.MaterialTransferRequestDetails.Count)
            {
                await FocusElement($"qty-{nextIndex}");
            }
            else
            {
                await FocusElement("btn-approve");
            }
        }
    }

    private async Task OnToStoreChanged(MaterialTransferRequestDtls detail, int? newStoreId)
    {
        detail.FK_ToStoreId = newStoreId;

        if (newStoreId.HasValue)
        {
            // Update stock for this item from the destination store
            var qty = await api.GetItemCurrentStock(detail.FK_ItemId, newStoreId.Value);
            itemStocks[(detail.FK_ItemId, newStoreId.Value)] = qty;
        }

        StateHasChanged();
    }

    private void OnApprovedQuantityChanged(MaterialTransferRequestDtls detail, decimal newQuantity)
    {
        detail.ApprovedQuantity = newQuantity;
        detail.ApprovedAmount = newQuantity * detail.Rate;
        StateHasChanged();
    }

    private async Task FocusElement(string elementId)
    {
        try
        {
            await js.InvokeVoidAsync("eval", $"document.getElementById('{elementId}').select()");
        }
        catch
        {
            try
            {
                await js.InvokeVoidAsync("eval", $"document.getElementById('{elementId}').focus()");
            }
            catch { /* Ignore if element not found */ }
        }
    }

    private async Task PrintHelperMethod(bool IsPreview = false)
    {
        try
        {
            if (request == null || request.MatTransReqId == 0) return;

            var validDetails = request.MaterialTransferRequestDetails.Where(x => !string.IsNullOrEmpty(x.ItemName)).ToList();
            if (!validDetails.Any()) return;

            // Use "Material_Transfer_Approval" key for approval-specific template
            var pdto = await api.GetPrintDesignAsync(new PrintDesignReq(0, "Material_Transfer_Approval"));
            if (pdto.hdr == null || !pdto.dtls.Any())
            {
                // Fallback to Material_Transfer_Request if approval template doesn't exist
                pdto = await api.GetPrintDesignAsync(new PrintDesignReq(0, "Material_Transfer_Request"));
                if (pdto.hdr == null || !pdto.dtls.Any()) return;
            }

            var thdr = request.CopyTo<TransactionHdr>();
            thdr.HdrNo = request.VoucherNo;
            thdr.HdrDate = request.VoucherDate ?? DateTime.Now;
            thdr.RefNo = request.ReferenceNo;
            thdr.Remarks = request.Remarks;
            thdr.InvoiceDate = (request.VoucherDate ?? DateTime.Now).ToString("dd-MM-yyyy");

            // Map From/To Branch Names
            var toBranch = allBranches.FirstOrDefault(x => x.BranchId == request.FK_ToBranchId);
            var fromBranch = allBranches.FirstOrDefault(x => x.BranchId == request.FK_FromBranchId);

            if (fromBranch is not null)
            {
                thdr.FromBranch = fromBranch.Name;
            }
            else if (request.FK_FromBranchId == AppData.BranchId)
            {
                thdr.FromBranch = allBranches.FirstOrDefault(x => x.BranchId == AppData.BranchId)?.Name;
            }

            // Dynamic header based on status
            if (request.Status == "Approved")
            {
                // Set Status in Remarks - simple status only
                thdr.Remarks = "Approved";

                // Set fields - using generic "Processed By" approach
                if (request.ApproverUser is not null)
                {
                    thdr.ApprovedBy = request.ApproverUser.UserName;
                }
                if (request.ApprovedDate.HasValue)
                {
                    thdr.ApprovedDate = request.ApprovedDate.Value.ToString("dd-MM-yyyy");
                }

                // Set totals for approved requests
                thdr.TotalQuantity = request.MaterialTransferRequestDetails.Sum(x => x.ApprovedQuantity);
                thdr.GrandTotal = request.MaterialTransferRequestDetails.Sum(x => x.ApprovedAmount);
            }
            else if (request.Status == "Rejected")
            {
                // Set Status in Remarks - include rejection reason here
                thdr.Remarks = "Rejected";
                if (!string.IsNullOrEmpty(request.RejectionReason))
                {
                    thdr.Remarks += $"\nReason: {request.RejectionReason}";
                }

                // Map rejected info to the same fields as approved (for "Processed By" generic label)
                if (request.RejectorUser is not null)
                {
                    thdr.ApprovedBy = request.RejectorUser.UserName;  // Map to same field
                }
                if (request.RejectedDate.HasValue)
                {
                    thdr.ApprovedDate = request.RejectedDate.Value.ToString("dd-MM-yyyy");  // Map to same field
                }

                // No totals for rejected requests
                thdr.TotalQuantity = 0;
                thdr.GrandTotal = 0;
            }

            // Using Party Name for To Branch (Destination)
            var party = new Party();
            if (toBranch is not null)
            {
                party.Name = toBranch.Name;
            }

            // Map Details manually
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
                td.QtyPerUnit = item.QtyPerUnit;
                td.ExRate = item.Rate;

                // Always set RequestedQty for the print report
                td.RequestedQty = item.Quantity;

                // Always take approved qty/amount from transaction details
                // This ensures that if it's rejected (and ApprovedQuantity is 0), it shows 0
                td.Quantity = item.ApprovedQuantity;
                td.LineTotal = item.ApprovedAmount;

                // Map Store Names
                var fromStore = allStores.FirstOrDefault(x => x.StoreId == item.FK_FromStoreId);
                td.StoreName = fromStore?.Name ?? "";

                var toStore = allStores.FirstOrDefault(x => x.StoreId == item.FK_ToStoreId);
                if (toStore is not null)
                {
                    td.StoreName += " → " + toStore.Name;
                }

                tdtls.Add(td);
            }

            PrintParameterDTO dto = new PrintParameterDTO(pdto.hdr, pdto.dtls, thdr, tdtls, party, AppData.CompanyDetails!, null);
            await CrudHelper.PrintInvoiceMethodLocal(Downloader, dto, Dialog, IsPreview);
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod(ex.Message);
        }
    }

    private async Task PrintPrviewMethod() => await PrintHelperMethod(true);
    private async Task PrintMethod() => await PrintHelperMethod(false);
}
