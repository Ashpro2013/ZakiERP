using ZakiSAASERP.Models.Settings;
using ZakiSAASERP.Shared.Pages.Masters;
using Microsoft.AspNetCore.Components.Web;

namespace ZakiSAASERP.Shared.Pages.Transactions.Sales;

public partial class DeliveryNoteRtnView : ITabAware
{
    #region Variables
    [Parameter] public int TabId { get; set; }
    [Inject] IBrowserViewportService ResizeService { get; set; }
    List<TransactionDtls> entTransactions = new();
    private string _searchString;
    DeliveryNoteRtnHdr dnRturn = new();
    TransactionDtls transaction = new();
    List<ExMasters> products = new();
    List<CommonModel> customerList = new();
    List<CommonModel> unitlist = new();
    List<CommonModel> itemUnitlist = new();
    List<CommonModel> salesmanList = new();
    List<CommonModel> projectList = new();
    List<StoreMaster> storeList = new();
    Party party = new();
    bool IsEditing = false;
    bool DisablePrintButton = false;
    MudNumericField<string> txtDocNo;
    private WebUserPrivilege? privilege;
    MudDatePicker txtDocDate;
    MudTextField<string> txtCustomer;
    MudTextField<string> txtRefNo;
    MudSelect<int> cmbSalesMan;
    MudTextField<string> txtRemarks;
    MudNumericField<decimal?>? txtAdditional;
    MudNumericField<decimal?>? txtFreight;
    MudNumericField<decimal?>? txtAdditionalDisc;
    MudSelect<int> cmbProject;
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
    string sDeliveryNoteNo;
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
                privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "Delivery Note Return");
            }
            customerList = await api.GetCombofill("Customers", AppData.BranchId);
            unitlist = await api.GetCombofill("Unit", 1);
            salesmanList = await api.GetCombofill("SalesMan", 1);
            projectList = await api.GetCombofill("Project", 1);
            storeList = await api.GetAllStores();
            itemUnitlist = unitlist;
            if (Width > 600)
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
    async Task FocusMethod()
    {
        if (!entTransactions.Any() || !string.IsNullOrEmpty(entTransactions.LastOrDefault()!.ItemName))
            AddNewRow();
        StateHasChanged();
        IsEditing = false;
    }
    async Task CreatePartyMethod()
    {
        var result = await Dialog.MasterCreationDialogMethod<CustomerMasterView>();
        if (!result!.Canceled && result.Data is not null)
        {
            customerList.Add(AppData.PassingValue);
            dnRturn.FK_CustomerId = AppData.PassingValue.Id;
            dnRturn.CustomerName = AppData.PassingValue.Name;
            var customer = await api.GetCustomerById(AppData.PassingValue.Id);
            party = customer.CopyTo<Party>();
            StateHasChanged();
        }
    }
    async Task CreateSManMethod()
    {
        var result = await Dialog.MasterCreationDialogMethod<SalesManMasterView>();
        if (!result!.Canceled && result.Data is not null)
        {
            salesmanList.Add(AppData.PassingValue);
            dnRturn.FK_SalesManId = AppData.PassingValue.Id;
            dnRturn.CustomerName = AppData.PassingValue.Name;
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
        var dialog = await DialogService.ShowAsync<CustomerSearchDialog>(Localizer("CustomerSearch"), options);
        var result = await dialog.Result;
        if (!result!.Canceled && result.Data is CustomerMaster selectedCustomer)
        {
            dnRturn.FK_CustomerId = selectedCustomer.CustomerId;
            dnRturn.CustomerName = selectedCustomer.Name;
            var customer = await api.GetCustomerById(selectedCustomer.CustomerId);
            party = customer.CopyTo<Party>();
            StateHasChanged();
        }
    }
    TransactionGridSettings gridSettings => PermanentSettingsData.DeliveryNoteReturnSettings;

    void TotalFindMethod()
    {
        foreach (var item in entTransactions)
        {
            if (item.InRate == 0 && item.ExRate > 0)
            {
                item.InRate = Math.Round(item.ExRate * (1 + (item.TaxPerc / 100m)), 2, MidpointRounding.AwayFromZero);
            }
        }
        dnRturn.GrossAmount = entTransactions.Sum(x => x.GrossAmount);
        dnRturn.LineDsicount = entTransactions.Sum(x => x.DiscountAmnt);
        dnRturn.TaxableAmount = entTransactions.Sum(x => x.TaxableAmnt);
        dnRturn.TaxTotal = entTransactions.Sum(x => x.TaxAmount);
        dnRturn.NetAmount = entTransactions.Sum(x => x.LineTotal);
        dnRturn.GrandTotal = dnRturn.NetAmount;
    }

    private void OnExRateChanged(TransactionDtls row, decimal newExRate)
    {
        row.ExRate = newExRate;
        row.InRate = Math.Round(newExRate * (1 + (row.TaxPerc / 100m)), 2, MidpointRounding.AwayFromZero);
        TransactionHelper.GetTransactionAfterEdit(row, false);
        TotalFindMethod();
        StateHasChanged();
    }

    private void OnInRateChanged(TransactionDtls row, decimal newInRate)
    {
        row.InRate = newInRate;
        decimal taxMultiplier = 1 + (row.TaxPerc / 100m);
        row.ExRate = taxMultiplier != 0 ? Math.Round(newInRate / taxMultiplier, 4, MidpointRounding.AwayFromZero) : newInRate;
        TransactionHelper.GetTransactionAfterEdit(row, true);
        TotalFindMethod();
        StateHasChanged();
    }

    private void OnDiscPerChanged(TransactionDtls row, decimal newDiscPer)
    {
        row.DiscountPerc = newDiscPer;
        TransactionHelper.GetTransactionAfterEdit(row, gridSettings.ShowIncRate);
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
        TransactionHelper.GetTransactionAfterEdit(row, gridSettings.ShowIncRate, true);
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
            await DownFocusMethod(fieldId);
        else if (e.Key == "ArrowDown")
            await DownFocusMethod(fieldId);
        else if (e.Key == "ArrowUp")
        {
            if (fieldId == 2)
                await txtDocDate.FocusAsync();
            else if (fieldId == 3)
                await cmbSalesMan.FocusAsync();
            else if (fieldId == 4)
            {
                _activeTabIndex = 0;
                await Task.Delay(100);
                await txtRemarks.FocusAsync();
            }
            else if (fieldId == 5)
                await txtAdditional.FocusAsync();
            else if (fieldId == 6)
                await txtFreight.FocusAsync();
        }
    }

    private async Task DownFocusMethod(int fieldId)
    {
        if (fieldId == 1)
            await txtDocDate.FocusAsync();
        else if (fieldId == 2)
            await cmbSalesMan.FocusAsync();
        else if (fieldId == 3)
        {
            _activeTabIndex = 1;
            await Task.Delay(100);
            txtAdditional?.FocusAsync();
        }
        else if (fieldId == 4)
            await txtFreight!.FocusAsync();
        else if (fieldId == 5)
            await txtAdditionalDisc!.FocusAsync();
        else if (fieldId == 6)
            await cmbProject.FocusAsync();
    }
    #endregion

    #region Common
    async Task SearchDeliveryNoteAsync()
    {
        var result = await Dialog.TransactionSearchMethod("Delivery Note", sDeliveryNoteNo);
        if (result is not null)
        {
            var dto = await api.GetDeliveryNoteById(result.Id);
            entTransactions = dto.DeliveryNoteDtls;
            AddNewRow();
            int isl = 1;
            entTransactions.ForEach(x => isl = isl + 1);
            dnRturn = dto.DeliveryNoteHdr.CopyTo<DeliveryNoteRtnHdr>();
            dnRturn.DeliveryNoteId = dto.DeliveryNoteHdr.DeliveryNoteId;
            sDeliveryNoteNo = dto.DeliveryNoteHdr.DeliveryNoteNo;
            dnRturn.DeliveryNoteRtnNo = (await api.GetNewDeliveryNoteRtnDocNo(AppData.BranchId)).ToString();
            dnRturn.DeliveryNoteRtnDate = DateTime.Now;
            dnRturn.EntryDate = DateTime.Now;
            var customer = await api.GetCustomerById(dnRturn.FK_CustomerId);
            party = customer.CopyTo<Party>();
            TotalFindMethod();

            // Logic aligned with DeliveryNoteView: Skip recalculation for Type 2 (Inclusive)
            // because loaded line totals already reflect the discount.
            if (dnRturn.AdditionalDiscountType != 2)
            {
                int iType = dnRturn.AdditionalDiscAmnt > 0 ? 2 : 1;
                await DiscountMethod(new FocusEventArgs(), iType);
            }
            StateHasChanged();
        }
    }
    private async Task DltTransaction(TransactionDtls item)
    {
        entTransactions.Remove(item);
        if (!entTransactions.Any())
            await FocusMethod();
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
        transaction = TransactionHelper.GetTransactionAfterEdit(transaction, gridSettings.ShowIncRate);
        TotalFindMethod();
        StateHasChanged();
    }
    private void OnBlurQtEvent(FocusEventArgs arg)
    {
        if (transaction.FK_ItemId == 0) { return; }
        transaction = TransactionHelper.GetTransactionAfterEdit(transaction, gridSettings.ShowIncRate);
        TotalFindMethod();
        StateHasChanged();
    }
    async Task DiscountMethod(FocusEventArgs e, int iType)
    {
        if (dnRturn.AdditionalDiscountType == 2)
        {
            var result = await TransactionHelper.AdditionalDiscountMethod(entTransactions, iType, dnRturn.AdditionalDiscPertge, dnRturn.AdditionalDiscAmnt);
            dnRturn.AdditionalDiscPertge = result.Item1 ?? 0;
            dnRturn.AdditionalDiscAmnt = result.Item2 ?? 0;
            TotalFindMethod();

        }
        else if (dnRturn.AdditionalDiscountType == 1)
        {
            var result = await TransactionHelper.AddDiscType2Method(entTransactions, iType, dnRturn.AdditionalDiscPertge, dnRturn.AdditionalDiscAmnt, dnRturn.GrandTotal);
            dnRturn.AdditionalDiscPertge = result.Item1 ?? 0;
            dnRturn.AdditionalDiscAmnt = result.Item2 ?? 0;
            TotalFindMethod();
        }

        else
        {
            decimal baseTotal = dnRturn.NetAmount + (dnRturn.AdditionalAmount ?? 0) + (dnRturn.Freight ?? 0);
            var result = TransactionHelper.AddDiscType3Method(iType, dnRturn.AdditionalDiscPertge, dnRturn.AdditionalDiscAmnt, baseTotal);
            dnRturn.AdditionalDiscPertge = result.Item1 ?? 0;
            dnRturn.AdditionalDiscAmnt = result.Item2 ?? 0;
            dnRturn.GrandTotal = result.Item3 ?? 0;
        }

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
                case 5:
                    await txtTax.FocusAsync();
                    await txtTax.SelectAsync();
                    break;
                case 6:
                    await txtNet.FocusAsync();
                    break;
                case 7:
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
                    transaction = TransactionHelper.GetTransactionAfterEdit(transaction, gridSettings.ShowIncRate);
                }
                else
                    await BarcodeChanged(transaction.Barcode);
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
        transaction = TransactionHelper.GetTransactionAfterEdit(transaction, gridSettings.ShowIncRate);
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
        var selectedProduct = await Dialog.GetItemSearchMethod(sVal);
        if (selectedProduct is not null)
        {
            IsEditing = true;
            transaction.FK_ItemId = selectedProduct.FK_ItemId;
            transaction.ItemName = selectedProduct.MultiUnitName;
            transaction.Barcode = selectedProduct.Barcode;
            transaction.FK_UnitId = selectedProduct.FK_UnitId;
            transaction.UnitName = unitlist.FirstOrDefault(x => x.Id == selectedProduct.FK_UnitId)?.Name;
            transaction.Quantity = 1;
            transaction.QtyPerUnit = selectedProduct.QtyPerUnit > 0 ? selectedProduct.QtyPerUnit : 1;
            transaction.ExRate = selectedProduct.Rate ?? 0;
            transaction.InRate = selectedProduct.IncRate ?? 0;
            transaction.TaxPerc = await api.GetValueMethod("vatFromItemId", selectedProduct.FK_ItemId);
            transaction.DiscountPerc = 0;
            transaction = TransactionHelper.GetTransactionAfterEdit(transaction, gridSettings.ShowIncRate);

            // Use pre-populated units from ItemSelectionResult (respects ItemSearchMode)
            unitlist.Clear();
            unitlist = selectedProduct.AvailableUnits ?? new List<CommonModel>();
            itemUnitlist = unitlist.ToList();

            TotalFindMethod();
        }
    }

    private bool _isSaving = false;

    async Task SaveOrUpdate()
    {
        if (_isSaving) return;
        if (privilege is not null && dnRturn.DeliveryNoteRtnId == 0 && !privilege.SaveData)
        {
            Dialog.DialogMethod(Localizer("No...Privilege"));
            return;
        }
        if (privilege is not null && dnRturn.DeliveryNoteRtnId != 0 && !privilege.EditData)
        {
            Dialog.DialogMethod(Localizer("No...Privilege"));
            return;
        }
        _isSaving = true;
        StateHasChanged();

        try
        {
            entTransactions = entTransactions.Where(x => !string.IsNullOrEmpty(x.ItemName)).ToList();
            if (!entTransactions.Any()) { return; }
            dnRturn.FK_BranchId = AppData.BranchId;
            dnRturn.FK_UserId = AppData.UserId ?? 0;
            dnRturn.FK_FinancialYearId = AppData.FYearId ?? 0;
            var selectedCustomer = customerList.FirstOrDefault(x => x.Name == dnRturn.CustomerName);
            if (selectedCustomer is not null)
            {
                dnRturn.FK_CustomerId = selectedCustomer.Id;
            }
            else
            {
                dnRturn.FK_CustomerId = customerList.FirstOrDefault(x => x.Name == "Cash Customer")!.Id;
            }
            if (dnRturn.DeliveryNoteRtnId == 0)
            {
                dnRturn.EntryDate = DateTime.Now;
                var result = await api.CreateDeliveryNoteRtn(new DeliveryNoteRtnDTO(dnRturn, entTransactions));
                if (string.IsNullOrEmpty(result.Message))
                {
                    dnRturn.DeliveryNoteRtnId = result.Id;
                    if (await Dialog.DeleteDialogMethod(Localizer("PrintDialog")))
                    {
                        await PrintMethod();
                    }
                    DisablePrintButton = false;
                    Dialog.DialogMethod(Localizer("SaveMessage"));
                    await NewClick();
                }
                else
                    Dialog.DialogMethod(Localizer(result.Message));
            }
            else
            {
                var result = await api.UpdateDeliveryNoteRtn(new DeliveryNoteRtnDTO(dnRturn, entTransactions));
                if (!string.IsNullOrEmpty(result.Message))
                    Dialog.DialogMethod(Localizer(result.Message));
                else
                {
                    Dialog.DialogMethod(Localizer("UpdateMessage"));
                    await NewClick();
                }
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add(ex.Message, Severity.Error);
        }
        finally
        {
            _isSaving = false;
            StateHasChanged();
        }
    }
    async Task Cancel()
    {
        if (dnRturn.DeliveryNoteRtnId == 0 || !entTransactions.Any()) { return; }
        if (await Dialog.DeleteDialogMethod(Localizer("CancelDialog")))
        {
            dnRturn.IsDeleted = true;
            entTransactions.ForEach(x => x.IsDeleted = true);
            var result = await api.UpdateDeliveryNoteRtn(new DeliveryNoteRtnDTO(dnRturn, entTransactions));
            if (result.Id > 0)
            {
                if (!string.IsNullOrEmpty(result.Message))
                    Dialog.DialogMethod(Localizer(result.Message));

                await NewClick();
            }
            else
            {
                Dialog.DialogMethod(Localizer(result.Message));
            }
        }
    }

    async Task Delete()
    {
        if (dnRturn.DeliveryNoteRtnId == 0 || !entTransactions.Any()) { return; }
        if (privilege is not null && !privilege.DeleteData)
        {
            Dialog.DialogMethod(Localizer("No...Privilege"));
            return;
        }
        if (await Dialog.DeleteDialogMethod())
        {
            var result = await api.DeleteDeliveryNote(dnRturn.DeliveryNoteRtnId);
            if (string.IsNullOrEmpty(result.Message))
                await NewClick();
            else
                Dialog.DialogMethod(Localizer(result.Message));
        }
    }
    async Task NewClick()
    {
        dnRturn = new();
        sDeliveryNoteNo = null;
        dnRturn.DeliveryNoteRtnNo = (await api.GetNewDeliveryNoteRtnDocNo(AppData.BranchId)).ToString();
        dnRturn.DeliveryNoteRtnDate = DateTime.Now.Date;
        dnRturn.FK_CustomerId = customerList.FirstOrDefault()!.Id;
        dnRturn.CustomerName = customerList.FirstOrDefault().Name;
        dnRturn.FK_SalesManId = salesmanList.FirstOrDefault()!.Id;
        dnRturn.FK_ProjectId = projectList.FirstOrDefault()!.Id;
        dnRturn.AdditionalAmount = 0;
        dnRturn.Freight = 0;
        dnRturn.AdditionalDiscAmnt = 0;
        dnRturn.AdditionalDiscPertge = 0;
        dnRturn.AdditionalDiscountType = 1;
        dnRturn.IsDeleted = false;
        entTransactions = new();
        party = new Party();
        party.Name = "Cash Customer";
        party.Code = customerList.FirstOrDefault(x => x.Name == party.Name)!.Code;
        AddNewRow();
    }
    async Task PrintHelperMethod(bool IsPreview = false)
    {
        try
        {
            var transctions = entTransactions.Where(x => !string.IsNullOrEmpty(x.ItemName)).ToList();
            if (dnRturn.DeliveryNoteRtnId == 0 || !entTransactions.Any()) { return; }
            var pdto = await api.GetPrintDesignAsync(new PrintDesignReq(0, "Delivery_Note_Return"));
            if (pdto == null || pdto.hdr == null || !pdto.dtls.Any()) { return; }
            var hdr = dnRturn.CopyTo<TransactionHdr>();
            hdr.HdrNo = dnRturn.DeliveryNoteRtnNo;
            hdr.InvoiceDate = (dnRturn.DeliveryNoteRtnDate ?? DateTime.Now).ToString("dd-MM-yyyy");
            PrintParameterDTO dto = new PrintParameterDTO(pdto.hdr, pdto.dtls, hdr, transctions, party, AppData.CompanyDetails!, null);
            await CrudHelper.PrintInvoiceMethodLocal(Downloader, dto, Dialog, IsPreview);
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod(ex.Message);
        }
    }
    async Task PrintPrviewMethod() => await PrintHelperMethod(true);
    async Task PrintMethod() => await PrintHelperMethod(false);
    async Task SearchMethod()
    {
        var result = await Dialog.TransactionSearchMethod("Delivery Note Return");
        if (result is not null)
        {
            var dto = await api.GetDeliveryNoteRtnById(result.Id);
            if (dto.DeliveryNoteRtnHdr == null || !dto.DeliveryNoteRtnDtls.Any()) { return; }
            entTransactions = dto.DeliveryNoteRtnDtls;
            AddNewRow();
            int isl = 1;
            entTransactions.ForEach(x => { x.SL = isl; isl++; });
            dnRturn = dto.DeliveryNoteRtnHdr;
            sDeliveryNoteNo = dnRturn.DeliveryNoteNo;
            var customer = await api.GetCustomerById(dnRturn.FK_CustomerId);
            party = customer.CopyTo<Party>();
            DisablePrintButton = false;
            TotalFindMethod();

            // Logic aligned with DeliveryNoteView: Skip recalculation for Type 2 (Inclusive)
            // because loaded line totals already reflect the discount.
            if (dnRturn.AdditionalDiscountType != 2)
            {
                int iType = dnRturn.AdditionalDiscAmnt > 0 ? 2 : 1;
                await DiscountMethod(new FocusEventArgs(), iType);
            }
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
        transaction.UnitName = unitlist.FirstOrDefault()!.Name;
        transaction.FK_UnitId = unitlist.FirstOrDefault()!.Id;
        entTransactions.Add(transaction);
    }
    #endregion
}
