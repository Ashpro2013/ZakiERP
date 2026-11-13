using ZakiSAASERP.Models.Transactions.Purchase;
using ZakiSAASERP.Shared.Pages.Masters;
using Microsoft.AspNetCore.Components.Web;

namespace ZakiSAASERP.Shared.Pages.Transactions.Purchase;

public partial class PurchaseOrderView : ComponentBase, ITabAware
{
    #region Variables
    [Parameter] public int TabId { get; set; }
    [Inject] IBrowserViewportService ResizeService { get; set; }
    [Inject] ISnackbar Snackbar { get; set; }
    private WebUserPrivilege? privilege;
    List<TransactionDtls> entTransactions = new();
    private string _searchString;
    PurchaseOrderHdr purchaseOrder = new();
    TransactionDtls transaction = new();
    List<ExMasters> products = new();
    List<CommonModel> supplierList = new();
    List<CommonModel> unitlist = new();
    List<CommonModel> itemUnitlist = new();
    bool IsEditing = false;
    bool DisablePrintButton = false;
    private bool IsAutoCode => PermanentSettingsData.AutoPurchaseOrderNo;
    MudTextField<string> txtOrderNo;
    MudNumericField<decimal> txtQuantity;
    MudTextField<string> txtBarcode;
    MudTextField<string> textField;
    MudNumericField<decimal> txtRate;
    MudNumericField<decimal> txtDiscPerc;
    MudNumericField<decimal> txtDiscount;
    MudNumericField<decimal> txtTax;
    MudNumericField<decimal> txtNet;
    List<ExMasters> entPOrders = new();
    string strQuantityFormat => PermanentSettingsData.QuantityFormat;
    string strCurencyFormat => PermanentSettingsData.CurrencyFormat;
    public int Width { get; set; }
    public decimal Gross { get; set; } = 0;
    string supplierCode = "";
    string supplierName = "";
    string supplierVatNo = "";
    string supplierAddress = "";
    List<CommonModel> purchaserList = new();
    string purchaserName = "";
    List<VatMaster> vatList = new();
    List<StoreMaster> storeList = new();
    decimal defaultVatPerc = 0;
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
                privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "PurchaseOrderTitle");
            }
            supplierList = await api.GetCombofill("Suppliers", AppData.BranchId);
            unitlist = await api.GetCombofill("Unit", 1);
            purchaserList = await api.GetCombofill("Purchaser", 1);
            itemUnitlist = unitlist;
            entPOrders = await api.GetMasters("purchaseorder", AppData.BranchId);
            products = await api.GetMasters("product", 1);
            vatList = await api.GetAllVat();
            storeList = await api.GetAllStores();
            var defaultVat = vatList.FirstOrDefault(v => v.IsDefault);
            defaultVatPerc = defaultVat?.Percentage ?? 0;
            if (Width > 600)
            {
                AddNewRow();
            }
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
        // Validate current row before adding new one
        if (entTransactions.Any() && !string.IsNullOrEmpty(entTransactions.LastOrDefault()!.ItemName))
        {
            var lastItem = entTransactions.LastOrDefault();
            if (lastItem is not null)
            {
                // Qty validation
                if (lastItem.Quantity == 0)
                {
                    await Dialog.DialogMethod(Localizer("QtyCannotBeZero"));
                    return;
                }

                // Rate zero confirmation
                if (lastItem.ExRate == 0)
                {
                    bool? confirmed = await DialogService.ShowMessageBox(
                        Localizer("Warning"),
                        Localizer("RateIsZeroConfirm"),
                        yesText: Localizer("Yes"),
                        cancelText: Localizer("No"));
                    if (confirmed != true)
                    {
                        return;
                    }
                }
            }
        }

        if (!entTransactions.Any() || !string.IsNullOrEmpty(entTransactions.LastOrDefault()!.ItemName))
        {
            AddNewRow();
        }
        StateHasChanged();
        IsEditing = false;
        if (txtBarcode is not null)
        {
            await txtBarcode.FocusAsync();
        }
    }
    async Task CreateSupplierMethod()
    {
        var result = await Dialog.MasterCreationDialogMethod<SupplierMasterView>();
        if (!result!.Canceled && result.Data is not null)
        {
            supplierList.Add(AppData.PassingValue);
            purchaseOrder.FK_SupplierId = AppData.PassingValue.Id;
            await UpdateSupplierDisplayAsync();
            StateHasChanged();
        }
    }
    async Task SupplierSearchMethod()
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
            purchaseOrder.FK_SupplierId = selectedSupplier.SupplierId;
            supplierCode = selectedSupplier.Code ?? "";
            supplierName = selectedSupplier.Name ?? "";
            supplierVatNo = selectedSupplier.VatNo ?? "";
            supplierAddress = selectedSupplier.Address ?? "";
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
            purchaseOrder.FK_PurchaserId = selectedPurchaser.PurchaserId;
            purchaserName = selectedPurchaser.Name ?? "";
            StateHasChanged();
        }
    }
    async Task UpdateSupplierDisplayAsync()
    {
        if (purchaseOrder.FK_SupplierId > 0)
        {
            var supplier = await api.GetSupplierById(purchaseOrder.FK_SupplierId);
            if (supplier is not null)
            {
                supplierCode = supplier.Code ?? "";
                supplierName = supplier.Name ?? "";
                supplierVatNo = supplier.VatNo ?? "";
                supplierAddress = supplier.Address ?? "";
            }
            else
            {
                supplierCode = "";
                supplierName = "";
                supplierVatNo = "";
                supplierAddress = "";
            }
        }
        else
        {
            supplierCode = "";
            supplierName = "";
            supplierVatNo = "";
            supplierAddress = "";
        }
    }
    void UpdateSupplierDisplay()
    {
        var supplier = supplierList.FirstOrDefault(s => s.Id == purchaseOrder.FK_SupplierId);
        if (supplier is not null)
        {
            supplierCode = supplier.Code ?? "";
            supplierName = supplier.Name ?? "";
            // Note: VAT and Address not available from supplierList, would need API call
            supplierVatNo = "";
            supplierAddress = "";
        }
        else
        {
            supplierCode = "";
            supplierName = "";
            supplierVatNo = "";
            supplierAddress = "";
        }
    }
    void TotalFindMethod()
    {
        purchaseOrder.GrossAmount = entTransactions.Sum(x => x.GrossAmount);
        purchaseOrder.LineDsicount = entTransactions.Sum(x => x.DiscountAmnt);
        purchaseOrder.TaxableAmount = entTransactions.Sum(x => x.TaxableAmnt);
        purchaseOrder.TaxTotal = entTransactions.Sum(x => x.TaxAmount);
        purchaseOrder.NetAmount = entTransactions.Sum(x => x.LineTotal);
        purchaseOrder.GrandTotal = purchaseOrder.NetAmount;
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
    #endregion

    #region Desktop
    private void RowClickEvent(TableRowClickEventArgs<TransactionDtls> args)
    {
        transaction = args.Item!;
        IsEditing = false;
    }
    async Task OnBlurEvent(FocusEventArgs arg) => await FocusMethod();
    #endregion

    #region Common
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
        transaction = TransactionHelper.GetTransactionAfterEdit(transaction);
        TotalFindMethod();
        StateHasChanged();
    }
    private void OnVatPercentageChanged(TransactionDtls item, decimal vatPerc)
    {
        item.TaxPerc = vatPerc;
        item.TaxableAmnt = item.GrossAmount - item.DiscountAmnt;
        item.TaxAmount = Math.Round(item.TaxableAmnt * vatPerc / 100, 2);
        item.LineTotal = item.TaxableAmnt + item.TaxAmount;
        TotalFindMethod();
        StateHasChanged();
    }
    private void OnDiscountPercChanged(TransactionDtls item)
    {
        item.DiscountAmnt = Math.Round(item.GrossAmount * item.DiscountPerc / 100, 2);
        item.TaxableAmnt = item.GrossAmount - item.DiscountAmnt;
        item.TaxAmount = Math.Round(item.TaxableAmnt * item.TaxPerc / 100, 2);
        item.LineTotal = item.TaxableAmnt + item.TaxAmount;
        TotalFindMethod();
        StateHasChanged();
    }
    private void OnBlurAditional(FocusEventArgs arg)
    {
        if (entTransactions.Any())
        {
            TotalFindMethod();
            StateHasChanged();
        }
    }
    async Task TaskOnEnterMethod(KeyboardEventArgs e, int index)
    {
        if (e.Key == "Enter")
        {
            switch (index)
            {
                case 2: // Qty -> Rate
                    await txtRate.FocusAsync();
                    await txtRate.SelectAsync();
                    break;
                case 4: // Rate -> Disc%
                    await txtDiscPerc.FocusAsync();
                    await txtDiscPerc.SelectAsync();
                    break;
                case 5: // Disc% -> DiscAmt
                    await txtDiscount.FocusAsync();
                    await txtDiscount.SelectAsync();
                    break;
                case 6: // DiscAmt -> Add new row
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
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            MaxWidth = MaxWidth.Large
        };
        var selectedProduct = await Dialog.GetItemSearchMethod(sVal, IsSales: false);
        if (selectedProduct is not null)
        {
            unitlist.Clear();
            unitlist = selectedProduct.AvailableUnits ?? new List<CommonModel>();
            itemUnitlist = unitlist.ToList();

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

            TotalFindMethod();
            StateHasChanged();
            await txtQuantity.FocusAsync();
            await txtQuantity.SelectAsync();
        }
    }

    private bool _isSaving = false;

    async Task SaveOrUpdate()
    {
        if (_isSaving) return;
        if (_isSaving) return;
        if (privilege is not null && purchaseOrder.PurchaseOrderId == 0 && !privilege.SaveData)
        {
            await Dialog.DialogMethod(Localizer("No...Privilege"));
            return;
        }
        if (privilege is not null && purchaseOrder.PurchaseOrderId != 0 && !privilege.EditData)
        {
            await Dialog.DialogMethod(Localizer("No...Privilege"));
            return;
        }
        _isSaving = true;
        StateHasChanged();


        try
        {
            // Run all validations
            if (!ValidateHeaderDetails()) return;
            if (!ValidateItemDetails()) return;
            if (!await CheckRateZeroConfirmation()) return;

            purchaseOrder.FK_BranchId = AppData.BranchId;
            purchaseOrder.FK_UserId = AppData.UserId ?? 0;
            purchaseOrder.FK_FinancialYearId = AppData.FYearId ?? 0;
            if (purchaseOrder.PurchaseOrderId == 0)
            {
                purchaseOrder.EntryDate = DateTime.Now;
                var result = await api.CreatePurchaseOrder(new PurchaseOrderDTO(purchaseOrder, entTransactions));
                if (string.IsNullOrEmpty(result.Message))
                {
                    purchaseOrder.PurchaseOrderId = result.Id;
                    DisablePrintButton = false;
                    await Dialog.DialogMethod(Localizer("SaveMessage"));
                    await NewClick();
                }
                else
                {
                    await Dialog.DialogMethod(result.Message);
                }
            }
            else
            {
                var result = await api.UpdatePurchaseOrder(new PurchaseOrderDTO(purchaseOrder, entTransactions));
                if (!string.IsNullOrEmpty(result.Message))
                {
                    await Dialog.DialogMethod(result.Message);
                }
                else
                {
                    await Dialog.DialogMethod(Localizer("UpdateMessage"));
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

    #region Validation Methods

    bool ValidateHeaderDetails()
    {
        // Supplier validation
        if (purchaseOrder.FK_SupplierId == 0)
        {
            Snackbar.Add(Localizer("SupplierRequired"), Severity.Error);
            return false;
        }

        // VAT No validation
        if (!string.IsNullOrEmpty(supplierVatNo))
        {
            if (supplierVatNo.Length != 15 || !supplierVatNo.StartsWith("3") || !supplierVatNo.EndsWith("3"))
            {
                Snackbar.Add(Localizer("InvalidVatNo"), Severity.Error);
                return false;
            }
        }
        return true;
    }

    bool ValidateItemDetails()
    {
        var validItems = entTransactions.Where(x => !string.IsNullOrEmpty(x.ItemName)).ToList();

        // No items validation
        if (!validItems.Any())
        {
            Snackbar.Add(Localizer("NoItemsMessage"), Severity.Error);
            return false;
        }

        entTransactions = validItems;

        // Zero Qty validation
        var zeroQtyItems = entTransactions.Where(x => x.Quantity == 0).ToList();
        if (zeroQtyItems.Any())
        {
            Snackbar.Add(Localizer("QtyCannotBeZero"), Severity.Error);
            return false;
        }

        // Amounts validation
        if (purchaseOrder.GrossAmount == 0)
        {
            Snackbar.Add(Localizer("GrossAmountCannotBeZero"), Severity.Error);
            return false;
        }

        if (purchaseOrder.NetAmount == 0)
        {
            Snackbar.Add(Localizer("NetAmountCannotBeZero"), Severity.Error);
            return false;
        }

        return true;
    }

    async Task<bool> CheckRateZeroConfirmation()
    {
        var zeroRateItems = entTransactions.Where(x => x.ExRate == 0).ToList();
        if (zeroRateItems.Any())
        {
            bool? confirmed = await DialogService.ShowMessageBox(
                Localizer("Warning"),
                Localizer("RateIsZeroConfirm"),
                yesText: Localizer("Yes"),
                cancelText: Localizer("No"));
            if (confirmed != true)
            {
                return false;
            }
        }
        return true;
    }
    #endregion
    async Task Cancel()
    {
        var validTransactions = entTransactions.Where(x => !string.IsNullOrEmpty(x.ItemName)).ToList();

        if (purchaseOrder.PurchaseOrderId == 0 || !validTransactions.Any()) { return; }
        if (privilege is not null && !privilege.DeleteData)
        {
            await Dialog.DialogMethod(Localizer("No...Privilege"));
            return;
        }
        if (await Dialog.DeleteDialogMethod(Localizer("CancelDialog")))
        {
            purchaseOrder.IsDeleted = true;
            validTransactions.ForEach(x => x.IsDeleted = true);
            var result = await api.UpdatePurchaseOrder(new PurchaseOrderDTO(purchaseOrder, validTransactions));
            if (result.Id > 0)
            {
                if (!string.IsNullOrEmpty(result.Message))
                    await Dialog.DialogMethod(result.Message);

                await NewClick();
            }
            else
            {
                await Dialog.DialogMethod(result.Message);
            }
        }
    }

    async Task Delete()
    {
        if (purchaseOrder.PurchaseOrderId == 0 || !entTransactions.Any()) { return; }
        if (privilege is not null && !privilege.DeleteData)
        {
            await Dialog.DialogMethod(Localizer("No...Privilege"));
            return;
        }
        if (await Dialog.DeleteDialogMethod())
        {
            var result = await api.DeletePurchaseOrder(purchaseOrder.PurchaseOrderId);
            if (string.IsNullOrEmpty(result.Message))
            {
                await NewClick();
            }
            else
            {
                await Dialog.DialogMethod(result.Message);
            }
        }
    }
    async Task NewClick()
    {
        purchaseOrder = new();
        if (IsAutoCode)
        {
            await LoadAutoCodeIfEnabled();
        }
        else
        {
            purchaseOrder.PurchaseOrderNo = (await api.GetNewPurchaseOrderDocNo(AppData.BranchId)).ToString();
        }
        purchaseOrder.PurchaseOrderDate = DateTime.Now.Date;
        if (supplierList.Any())
        {
            purchaseOrder.FK_SupplierId = supplierList.FirstOrDefault()!.Id;
            await UpdateSupplierDisplayAsync();
        }
        purchaseOrder.IsDeleted = false;
        entTransactions = new();
        AddNewRow();
    }
    async Task PrintMethod()
    {

    }
    async Task SearchMethod()
    {
        var result = await Dialog.TransactionSearchMethod("Purchase Order");
        if (result is not null)
        {
            var dto = await api.GetPurchaseOrderById(result.Id);
            entTransactions = dto.PurchaseOdrDtls;
            entTransactions.ForEach(x => x.SL = entTransactions.IndexOf(x) + 1);
            AddNewRow();
            purchaseOrder = dto.PurchaseOdrHdr;
            await UpdateSupplierDisplayAsync();
            DisablePrintButton = false;
            TotalFindMethod();
            StateHasChanged();
        }
    }

    private void AddNewRow()
    {
        var allowedStores = storeList.Where(x => x.FK_BranchId == AppData.BranchId || x.FK_BranchId == null).ToList();
        var defaultStoreId = allowedStores.FirstOrDefault(x => x.IsDefaultStore)?.StoreId
                           ?? allowedStores.FirstOrDefault()?.StoreId
                           ?? 0;
        var lastItem = entTransactions.LastOrDefault();
        if (lastItem is not null)
        {
            if (string.IsNullOrEmpty(lastItem.ItemName))
            {
                Snackbar.Add(Localizer("ItemNameRequired"), Severity.Warning);
                return;
            }
            if (lastItem.Quantity <= 0)
            {
                Snackbar.Add(Localizer("QtyCannotBeZero"), Severity.Warning);
                return;
            }
            // Validate item exists
            if (lastItem.FK_ItemId == 0)
            {
                Snackbar.Add($"{Localizer("InvalidItem")}: {lastItem.Barcode ?? lastItem.ItemName}", Severity.Error);
                return;
            }
        }

        transaction = new();
        transaction.FK_StoreId = defaultStoreId;
        transaction.SL = entTransactions.Count + 1;
        transaction.TaxPerc = defaultVatPerc;
        if (unitlist.Any())
        {
            transaction.UnitName = unitlist.FirstOrDefault()!.Name;
            transaction.FK_UnitId = unitlist.FirstOrDefault()!.Id;
        }
        entTransactions.Add(transaction);
    }

    private async Task LoadAutoCodeIfEnabled()
    {
        if (IsAutoCode && purchaseOrder.PurchaseOrderId == 0)
        {
            purchaseOrder.PurchaseOrderNo = await api.GetNextPurchaseOrderCode();
            StateHasChanged();
        }
    }

    #endregion
}
