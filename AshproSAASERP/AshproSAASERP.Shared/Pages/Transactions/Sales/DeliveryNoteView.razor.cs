using AshproSAASERP.Models.Settings;
using AshproSAASERP.Shared.Pages.Masters;
using Microsoft.AspNetCore.Components.Web;

namespace AshproSAASERP.Shared.Pages.Transactions.Sales;

public partial class DeliveryNoteView : ITabAware
{
    #region Variables
    [Parameter] public int TabId { get; set; }
    [Inject] IBrowserViewportService ResizeService { get; set; }
    List<TransactionDtls> entTransactions = new();
    private string _searchString;
    DeliveryNoteHdr deliveryNote = new();
    TransactionDtls transaction = new();
    List<ExMasters> products = new();
    List<CommonModel> customerList = new();
    List<CommonModel> unitlist = new();
    List<CommonModel> itemUnitlist = new();
    List<CommonModel> salesmanList = new();
    List<CommonModel> projectList = new();
    List<StoreMaster> storeList = new();
    Party party = new();
    private WebUserPrivilege? privilege;
    bool IsEditing = false;
    bool DisablePrintButton = false;
    private bool IsAutoCode => PermanentSettingsData.AutoDeliveryNoteNo;
    MudTextField<string> txtDocNo;
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
    string sSalesInvoiceNo;
    #endregion

    #region Properties
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
                privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "DeliveryNoteTitle");
            }
            customerList = await api.GetCombofill("Customers", AppData.BranchId);
            unitlist = await api.GetCombofill("Unit", 1);
            salesmanList = await api.GetCombofill("SalesMan", 1);
            projectList = await api.GetCombofill("Project", 1);
            storeList = await api.GetAllStores();
            itemUnitlist = unitlist;
            products = await api.GetMasters("product", 1);
            if (Width > 600)
            {
                AddNewRow();
            }
            if (Id is not null)
            {
                await LoadDeliveryNote(Id ?? 0);
            }
            else
            {
                await NewClick();
            }
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
        {
            AddNewRow();
        }
        StateHasChanged();
        IsEditing = false;
    }
    async Task CreatePartyMethod()
    {
        var result = await Dialog.MasterCreationDialogMethod<CustomerMasterView>();
        if (!result!.Canceled && result.Data is not null)
        {
            customerList.Add(AppData.PassingValue);
            deliveryNote.FK_CustomerId = AppData.PassingValue.Id;
            deliveryNote.CustomerName = AppData.PassingValue.Name;
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
            deliveryNote.FK_SalesManId = AppData.PassingValue.Id;
            deliveryNote.CustomerName = AppData.PassingValue.Name;
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
            deliveryNote.FK_CustomerId = selectedCustomer.CustomerId;
            deliveryNote.CustomerName = selectedCustomer.Name;
            var customer = await api.GetCustomerById(selectedCustomer.CustomerId);
            party = customer.CopyTo<Party>();
            StateHasChanged();
        }
    }
    TransactionGridSettings gridSettings => PermanentSettingsData.DeliveryNoteSettings;

    void TotalFindMethod()
    {
        foreach (var item in entTransactions)
        {
            if (item.InRate == 0 && item.ExRate > 0)
            {
                item.InRate = Math.Round(item.ExRate * (1 + (item.TaxPerc / 100m)), 2, MidpointRounding.AwayFromZero);
            }
        }
        deliveryNote.GrossAmount = entTransactions.Sum(x => x.GrossAmount);
        deliveryNote.LineDsicount = entTransactions.Sum(x => x.DiscountAmnt);
        deliveryNote.TaxableAmount = entTransactions.Sum(x => x.TaxableAmnt);
        deliveryNote.TaxTotal = entTransactions.Sum(x => x.TaxAmount);
        deliveryNote.NetAmount = entTransactions.Sum(x => x.LineTotal);
        deliveryNote.GrandTotal = deliveryNote.NetAmount;
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
                await txtDocDate.FocusAsync();
            }
            else if (fieldId == 3)
            {
                await cmbSalesMan.FocusAsync();
            }
            else if (fieldId == 4)
            {
                _activeTabIndex = 0;
                await Task.Delay(100);
                await txtRemarks.FocusAsync();
            }
            else if (fieldId == 5)
            {
                await txtAdditional.FocusAsync();
            }
            else if (fieldId == 6)
            {
                await txtFreight.FocusAsync();
            }
        }
    }

    private async Task DownFocusMethod(int fieldId)
    {
        if (fieldId == 1)
        {
            await txtDocDate.FocusAsync();
        }
        else if (fieldId == 2)
        {
            await cmbSalesMan.FocusAsync();
        }
        else if (fieldId == 3)
        {
            _activeTabIndex = 1;
            await Task.Delay(100);
            txtAdditional?.FocusAsync();
        }
        else if (fieldId == 4)
        {
            await txtFreight!.FocusAsync();
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
    async Task SearchSalesInvoiceAsync()
    {
        var result = await Dialog.TransactionSearchMethod("Sales Invoice", sSalesInvoiceNo);
        if (result is not null)
        {
            var currentProjectId = deliveryNote.FK_ProjectId; // Capture default project
            var dto = await api.GetSalesInvoiceById(result.Id);
            entTransactions = dto.InvoiceDtls;
            entTransactions.ForEach(x =>
            {
                x.FK_SalesInvoiceDtlsId = x.DtlsId;
                x.RefDtlsId = x.DtlsId;
                x.RefType = "SalesInvoice";
                x.DtlsId = 0;
            });
            AddNewRow();
            int isl = 1;
            entTransactions.ForEach(x => isl = isl + 1);
            deliveryNote = dto.InvoiceHdr.CopyTo<DeliveryNoteHdr>();

            // Restore default project if Invoice has no valid project
            if (deliveryNote.FK_ProjectId == 0 && currentProjectId != 0)
            {
                deliveryNote.FK_ProjectId = currentProjectId;
            }

            deliveryNote.SalesInvoiceId = dto.InvoiceHdr.SalesInvoiceId;
            if (IsAutoCode)
            {
                await LoadAutoCodeIfEnabled();
            }
            else
            {
                deliveryNote.DeliveryNoteNo = (await api.GetNewDeliveryNoteDocNo(AppData.BranchId)).ToString();
            }
            sSalesInvoiceNo = dto.InvoiceHdr.SalesInvoiceNo;
            var customer = await api.GetCustomerById(dto.InvoiceHdr.FK_CustomerId);
            party = customer.CopyTo<Party>();
            deliveryNote.DeliveryNoteDate = DateTime.Now;
            deliveryNote.EntryDate = DateTime.Now;
            TotalFindMethod();
            // Logic aligned with SalesQuotationView/SalesInvoiceView
            if (deliveryNote.AdditionalDiscountType != 2)
            {
                int iType = deliveryNote.AdditionalDiscAmnt > 0 ? 2 : 1;
                await DiscountMethod(new FocusEventArgs(), iType);
            }
            StateHasChanged();
        }
    }
    async Task DiscountMethod(FocusEventArgs e, int iType)
    {
        if (deliveryNote.AdditionalDiscountType == 2)
        {
            var result = await TransactionHelper.AdditionalDiscountMethod(entTransactions, iType, deliveryNote.AdditionalDiscPertge, deliveryNote.AdditionalDiscAmnt, deliveryNote.GrandTotal);
            deliveryNote.AdditionalDiscPertge = result.Item1 ?? 0;
            deliveryNote.AdditionalDiscAmnt = result.Item2 ?? 0;
            TotalFindMethod();

        }
        else if (deliveryNote.AdditionalDiscountType == 1)
        {
            var result = await TransactionHelper.AddDiscType2Method(entTransactions, iType, deliveryNote.AdditionalDiscPertge, deliveryNote.AdditionalDiscAmnt, deliveryNote.GrandTotal);
            deliveryNote.AdditionalDiscPertge = result.Item1 ?? 0;
            deliveryNote.AdditionalDiscAmnt = result.Item2 ?? 0;
            TotalFindMethod();
        }

        else
        {
            decimal baseTotal = deliveryNote.NetAmount + (deliveryNote.AdditionalAmount ?? 0) + (deliveryNote.Freight ?? 0);
            var result = TransactionHelper.AddDiscType3Method(iType, deliveryNote.AdditionalDiscPertge, deliveryNote.AdditionalDiscAmnt, baseTotal);
            deliveryNote.AdditionalDiscPertge = result.Item1 ?? 0;
            deliveryNote.AdditionalDiscAmnt = result.Item2 ?? 0;
            deliveryNote.GrandTotal = result.Item3 ?? 0;
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
        if (privilege is not null && deliveryNote.DeliveryNoteId == 0 && !privilege.SaveData)
        {
            Dialog.DialogMethod(Localizer("No...Privilege"));
            return;
        }
        if (privilege is not null && deliveryNote.DeliveryNoteId != 0 && !privilege.EditData)
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

            if (deliveryNote.FK_ProjectId == 0)
            {
                Dialog.DialogMethod(Localizer("ProjectIdRequired")); // Or use a hardcoded message if localizer key missing
                return;
            }

            deliveryNote.FK_BranchId = AppData.BranchId;
            deliveryNote.FK_UserId = AppData.UserId ?? 0;
            deliveryNote.FK_FinancialYearId = AppData.FYearId ?? 0;
            var selectedCustomer = customerList.FirstOrDefault(x => x.Name == deliveryNote.CustomerName);
            if (selectedCustomer is not null)
            {
                deliveryNote.FK_CustomerId = selectedCustomer.Id;
            }
            else
            {
                deliveryNote.FK_CustomerId = customerList.FirstOrDefault(x => x.Name == "Cash Customer")!.Id;
            }
            if (deliveryNote.DeliveryNoteId == 0)
            {
                deliveryNote.EntryDate = DateTime.Now;
                var result = await api.CreateDeliveryNote(new DeliveryNoteDTO(deliveryNote, entTransactions));
                if (string.IsNullOrEmpty(result.Message))
                {
                    deliveryNote.DeliveryNoteId = result.Id;
                    if (await Dialog.DeleteDialogMethod(Localizer("PrintDialog")))
                    {
                        await PrintMethod();
                    }
                    DisablePrintButton = false;
                    Dialog.DialogMethod(Localizer("SaveMessage"));
                    await NewClick();
                }
                else
                {
                    Dialog.DialogMethod(Localizer(result.Message));
                }
            }
            else
            {
                var result = await api.UpdateDeliveryNote(new DeliveryNoteDTO(deliveryNote, entTransactions));
                if (!string.IsNullOrEmpty(result.Message))
                {
                    Dialog.DialogMethod(Localizer(result.Message));
                }
                else
                {
                    Dialog.DialogMethod(Localizer("UpdateMessage"));
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
        if (deliveryNote.DeliveryNoteId == 0 || !entTransactions.Any()) { return; }
        if (await Dialog.DeleteDialogMethod(Localizer("CancelDialog")))
        {
            deliveryNote.IsDeleted = true;
            entTransactions.ForEach(x => x.IsDeleted = true);
            var result = await api.UpdateDeliveryNote(new DeliveryNoteDTO(deliveryNote, entTransactions));
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
        if (privilege is not null && !privilege.DeleteData)
        {
            Dialog.DialogMethod(Localizer("No...Privilege"));
            return;
        }
        if (deliveryNote.DeliveryNoteId == 0 || !entTransactions.Any()) { return; }
        if (await Dialog.DeleteDialogMethod())
        {
            var result = await api.DeleteDeliveryNote(deliveryNote.DeliveryNoteId);
            if (string.IsNullOrEmpty(result.Message))
            {
                await NewClick();
            }
            else
            {
                Dialog.DialogMethod(Localizer(result.Message));
            }
        }
    }
    async Task NewClick()
    {
        deliveryNote = new();
        sSalesInvoiceNo = string.Empty;
        if (IsAutoCode)
        {
            await LoadAutoCodeIfEnabled();
        }
        else
        {
            deliveryNote.DeliveryNoteNo = (await api.GetNewDeliveryNoteDocNo(AppData.BranchId)).ToString();
        }
        deliveryNote.DeliveryNoteDate = DateTime.Now.Date;
        deliveryNote.FK_CustomerId = customerList.FirstOrDefault()!.Id;
        deliveryNote.CustomerName = customerList.FirstOrDefault()!.Name;
        deliveryNote.FK_SalesManId = salesmanList.FirstOrDefault()!.Id;
        deliveryNote.FK_ProjectId = projectList.FirstOrDefault()!.Id;
        deliveryNote.AdditionalAmount = 0;
        deliveryNote.Freight = 0;
        deliveryNote.AdditionalDiscAmnt = 0;
        deliveryNote.AdditionalDiscPertge = 0;
        deliveryNote.AdditionalDiscountType = 1;
        deliveryNote.IsDeleted = false;
        deliveryNote.IsDeleted = false;
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
            if (deliveryNote.DeliveryNoteId == 0 || !entTransactions.Any()) { return; }
            var pdto = await api.GetPrintDesignAsync(new PrintDesignReq(0, "Delivery_Note"));
            if (pdto.hdr == null || !pdto.dtls.Any()) { return; }
            var hdr = deliveryNote.CopyTo<TransactionHdr>();
            hdr.HdrNo = deliveryNote.DeliveryNoteNo;
            hdr.InvoiceDate = (deliveryNote.DeliveryNoteDate ?? DateTime.Now).ToString("dd-MM-yyyy");
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
        var result = await Dialog.TransactionSearchMethod("Delivery Note");
        if (result is not null)
        {
            var dto = await api.GetDeliveryNoteById(result.Id);
            entTransactions = dto.DeliveryNoteDtls;
            entTransactions.ForEach(x => x.SL = entTransactions.IndexOf(x) + 1);
            AddNewRow();
            deliveryNote = dto.DeliveryNoteHdr;
            if (deliveryNote.SalesInvoiceId.HasValue && deliveryNote.SalesInvoiceId > 0)
            {
                try
                {
                    var invDto = await api.GetSalesInvoiceById(deliveryNote.SalesInvoiceId.Value);
                    sSalesInvoiceNo = invDto?.InvoiceHdr?.SalesInvoiceNo;
                }
                catch (Exception ex)
                {
                    // Log error to snackbar to debug
                    snackbar.Add($"Error fetching Invoice {deliveryNote.SalesInvoiceId}: {ex.Message}", Severity.Warning);
                }
            }
            // else snackbar.Add($"No Sales Invoice Linked (Id: {deliveryNote.SalesInvoiceId})", Severity.Info);

            var customer = await api.GetCustomerById(deliveryNote.FK_CustomerId);
            party = customer.CopyTo<Party>();
            DisablePrintButton = false;
            TotalFindMethod();

            // Logic aligned with SalesQuotationView/SalesInvoiceView
            if (deliveryNote.AdditionalDiscountType != 2)
            {
                int iType = deliveryNote.AdditionalDiscAmnt > 0 ? 2 : 1;
                await DiscountMethod(new FocusEventArgs(), iType);
            }
            StateHasChanged();
        }
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

    private async Task LoadAutoCodeIfEnabled()
    {
        if (IsAutoCode && deliveryNote.DeliveryNoteId == 0)
        {
            deliveryNote.DeliveryNoteNo = await api.GetNextDeliveryNoteCode();
        }
    }
    private async Task LoadDeliveryNote(int id)
    {
        // Removed TransactionSearchModel usage
        if (id > 0)
        {
            var dto = await api.GetDeliveryNoteById(id);
            entTransactions = dto.DeliveryNoteDtls;
            AddNewRow();
            int isl = 1;
            entTransactions.ForEach(x => isl = isl + 1);
            deliveryNote = dto.DeliveryNoteHdr;

            if (deliveryNote.SalesInvoiceId.HasValue && deliveryNote.SalesInvoiceId > 0)
            {
                try
                {
                    var invDto = await api.GetSalesInvoiceById(deliveryNote.SalesInvoiceId.Value);
                    sSalesInvoiceNo = invDto?.InvoiceHdr?.SalesInvoiceNo;
                }
                catch (Exception ex)
                {
                    // Log error to snackbar to debug
                    snackbar.Add($"Error fetching Invoice {deliveryNote.SalesInvoiceId}: {ex.Message}", Severity.Warning);
                }
            }

            var customer = await api.GetCustomerById(deliveryNote.FK_CustomerId);
            party = customer.CopyTo<Party>();
            DisablePrintButton = false;
            TotalFindMethod();
            // Logic aligned with SalesQuotationView/SalesInvoiceView
            if (deliveryNote.AdditionalDiscountType != 2)
            {
                int iType = deliveryNote.AdditionalDiscAmnt > 0 ? 2 : 1;
                await DiscountMethod(new FocusEventArgs(), iType);
            }
            StateHasChanged();
        }
    }
    #endregion
}
