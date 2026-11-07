using ZakiSAASERP.Shared.Pages.Masters;
using Microsoft.AspNetCore.Components.Web;
using ZakiSAASERP.Models.Settings;

namespace ZakiSAASERP.Shared.Pages.Transactions.Sales;

public partial class SalesOrderView : ITabAware
{

    #region Variables
    [Parameter] public int TabId { get; set; }
    [Inject] IBrowserViewportService ResizeService { get; set; }
    List<TransactionDtls> entTransactions = new();
    private string _searchString;
    SalesOrderHdr salesOrder = new();
    TransactionDtls transaction = new();
    List<CommonModel> customerList = new();
    List<CommonModel> unitlist = new();
    List<CommonModel> itemUnitlist = new();
    List<CommonModel> salesmanList = new();
    List<CommonModel> projectList = new();
    List<StoreMaster> storeList = new();
    Party party = new();
    private WebUserPrivilege? privilege;
    bool DisablePrintButton = false;
    private bool IsAutoCode => PermanentSettingsData.AutoSalesOrderNo;
    MudNumericField<decimal> txtQuantity;
    MudTextField<string> txtBarcode;
    MudNumericField<decimal> txtRate;
    MudNumericField<decimal> txtDiscount;
    MudNumericField<decimal> txtTax;
    MudNumericField<decimal> txtNet;
    MudTextField<string> txtOrderNo;
    MudDatePicker txtOrderDate;
    MudTextField<string> txtCustomer;
    MudTextField<string> txtRefNo;
    MudSelect<int> cmbSalesMan;
    MudTextField<string> txtRemarks;
    MudNumericField<decimal?>? txtAdditional;
    MudNumericField<decimal?>? txtFreight;
    MudNumericField<decimal?>? txtAdditionalDisc;
    MudSelect<int?> cmbProject;
    MudTextField<string> txtProName;
    private int _activeTabIndex = 0;
    string strQuantityFormat => PermanentSettingsData.QuantityFormat;
    string strCurencyFormat => PermanentSettingsData.CurrencyFormat;
    public int Width { get; set; }
    public decimal Gross { get; set; } = 0;
    string sQuotationNo;
    private bool _isSaving = false;
    PrintDesignDTO pdto;
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
                privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "SalesOrderTitle");
            }
            customerList = await indexdbHelper.LoadComboFromIndexedDB("Customers");
            unitlist = await indexdbHelper.LoadComboFromIndexedDB("Unit");
            salesmanList = await indexdbHelper.LoadComboFromIndexedDB("SalesMan");
            projectList = await indexdbHelper.LoadComboFromIndexedDB("Project");
            pdto = await indexdbHelper.LoadPrintDesignFromIndexedDB("Sales_Order");
            itemUnitlist = unitlist;
            storeList = await api.GetAllStores();
            if (Width > 600)
            {
                AddNewRow();
            }
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
        {
            AddNewRow();
        }
        StateHasChanged();
    }
    async Task CreatePartyMethod()
    {
        var result = await Dialog.MasterCreationDialogMethod<CustomerMasterView>();
        if (!result!.Canceled && result.Data is not null)
        {
            customerList.Add(AppData.PassingValue);
            salesOrder.FK_CustomerId = AppData.PassingValue.Id;
            salesOrder.CustomerName = AppData.PassingValue.Name;
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
            salesOrder.FK_SalesManId = AppData.PassingValue.Id;
            salesOrder.CustomerName = AppData.PassingValue.Name;
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
            salesOrder.FK_CustomerId = selectedCustomer.CustomerId;
            salesOrder.CustomerName = selectedCustomer.Name;
            var customer = await api.GetCustomerById(selectedCustomer.CustomerId);
            party = customer.CopyTo<Party>();
            StateHasChanged();
        }
    }
    void TotalFindMethod()
    {
        foreach (var item in entTransactions)
        {
            if (item.InRate == 0 && item.ExRate > 0)
            {
                UpdateInRate(item);
            }
        }
        salesOrder.GrossAmount = entTransactions.Sum(x => x.GrossAmount);
        salesOrder.LineDsicount = entTransactions.Sum(x => x.DiscountAmnt);
        salesOrder.TaxableAmount = entTransactions.Sum(x => x.TaxableAmnt);
        salesOrder.TaxTotal = entTransactions.Sum(x => x.TaxAmount);
        salesOrder.NetAmount = entTransactions.Sum(x => x.LineTotal);
        salesOrder.GrandTotal = salesOrder.NetAmount;
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
        TotalFindMethod();
        await txtBarcode.FocusAsync();
        StateHasChanged();
    }
    private void RowClicked(DataGridRowClickEventArgs<TransactionDtls> args)
    {
        transaction = args.Item;
    }
    #endregion

    #region Desktop
    private void RowClickEvent(TableRowClickEventArgs<TransactionDtls> args)
    {
        transaction = args.Item!;
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
                await txtOrderDate.FocusAsync();
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
            await txtOrderDate.FocusAsync();
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
    async Task SearchSalesQuotationAsync()
    {
        var result = await Dialog.TransactionSearchMethod("Sales Quotation", sQuotationNo);
        if (result is not null)
        {
            var dto = await api.GetSalesQuotationById(result.Id);
            entTransactions = dto.QuotationDtls;
            AddNewRow();
            int isl = 1;
            entTransactions.ForEach(x => isl = isl + 1);
            salesOrder = dto.QuotationHdr.CopyTo<SalesOrderHdr>();
            salesOrder.SalesQuotationId = dto.QuotationHdr.SalesQuotationId;
            salesOrder.SalesOrderDate = DateTime.Now;
            salesOrder.EntryDate = DateTime.Now;
            sQuotationNo = dto.QuotationHdr.SalesQuotationNo;
            if (IsAutoCode)
            {
                await LoadAutoCodeIfEnabled();
            }
            else
            {
                salesOrder.SalesOrderNo = (await api.GetNewSalesOrderDocNo(AppData.BranchId)).ToString();
            }
            TotalFindMethod();
            // Logic aligned with SalesQuotationView/SalesInvoiceView
            if (salesOrder.AdditionalDiscountType != 2)
            {
                int iType = salesOrder.AdditionalDiscAmnt > 0 ? 2 : 1;
                await DiscountMethod(new FocusEventArgs(), iType);
            }
            StateHasChanged();
        }
    }

    async Task DiscountMethod(FocusEventArgs e, int iType)
    {
        if (salesOrder.AdditionalDiscountType == 2)
        {
            var result = await TransactionHelper.AdditionalDiscountMethod(entTransactions, iType, salesOrder.AdditionalDiscPertge, salesOrder.AdditionalDiscAmnt);
            salesOrder.AdditionalDiscPertge = result.Item1 ?? 0;
            salesOrder.AdditionalDiscAmnt = result.Item2 ?? 0;
            TotalFindMethod();
        }
        else if (salesOrder.AdditionalDiscountType == 1)
        {
            var result = await TransactionHelper.AddDiscType2Method(entTransactions, iType, salesOrder.AdditionalDiscPertge, salesOrder.AdditionalDiscAmnt, salesOrder.GrandTotal);
            salesOrder.AdditionalDiscPertge = result.Item1 ?? 0;
            salesOrder.AdditionalDiscAmnt = result.Item2 ?? 0;
            TotalFindMethod();
        }
        else
        {
            decimal baseTotal = salesOrder.NetAmount + (salesOrder.AdditionalAmount ?? 0) + (salesOrder.Freight ?? 0);
            var result = TransactionHelper.AddDiscType3Method(iType, salesOrder.AdditionalDiscPertge, salesOrder.AdditionalDiscAmnt, baseTotal);
            salesOrder.AdditionalDiscPertge = result.Item1 ?? 0;
            salesOrder.AdditionalDiscAmnt = result.Item2 ?? 0;
            salesOrder.GrandTotal = result.Item3 ?? 0;
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
        if (transaction.FK_ItemId == 0) { return; }
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
                case 1:
                    await txtQuantity.FocusAsync();
                    await txtQuantity.SelectAsync();
                    break;
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
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            MaxWidth = MaxWidth.Large
        };
        var selectedProduct = await Dialog.GetItemSearchMethod(sVal);
        if (selectedProduct is not null)
        {
            transaction.FK_ItemId = selectedProduct.FK_ItemId;
            transaction.ItemName = selectedProduct.MultiUnitName;
            transaction.Barcode = selectedProduct.Barcode;
            transaction.FK_UnitId = selectedProduct.FK_UnitId;
            transaction.UnitName = unitlist.FirstOrDefault(x => x.Id == selectedProduct.FK_UnitId)?.Name;
            transaction.Quantity = 1;
            transaction.QtyPerUnit = 1;
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


    async Task SaveOrUpdate()
    {
        if (_isSaving) return;
        if (privilege is not null && salesOrder.SalesOrderId == 0 && !privilege.SaveData)
        {
            Dialog.DialogMethod(Localizer("No...Privilege"));
            return;
        }
        if (privilege is not null && salesOrder.SalesOrderId != 0 && !privilege.EditData)
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
            salesOrder.FK_BranchId = AppData.BranchId;
            salesOrder.FK_UserId = AppData.UserId ?? 0;
            salesOrder.FK_FinancialYearId = AppData.FYearId ?? 0;
            var selectedCustomer = customerList.FirstOrDefault(x => x.Name == salesOrder.CustomerName);
            if (selectedCustomer is not null)
            {
                salesOrder.FK_CustomerId = selectedCustomer.Id;
            }
            else
            {
                salesOrder.FK_CustomerId = customerList.FirstOrDefault(x => x.Name == "Cash Customer")!.Id;
            }
            if (salesOrder.SalesOrderId == 0)
            {
                salesOrder.EntryDate = DateTime.Now;
                var result = await api.CreateSalesOrder(new SalesOrderDTO(salesOrder, entTransactions));
                if (string.IsNullOrEmpty(result.Message))
                {
                    salesOrder.SalesOrderId = result.Id;
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
                    Dialog.DialogMethod(result.Message);
                }
            }
            else
            {
                var result = await api.UpdateSalesOrder(new SalesOrderDTO(salesOrder, entTransactions));
                if (!string.IsNullOrEmpty(result.Message))
                {
                    Dialog.DialogMethod(result.Message);
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
        if (salesOrder.SalesOrderId == 0 || !entTransactions.Any()) { return; }
        if (await Dialog.DeleteDialogMethod(Localizer("CancelDialog")))
        {
            salesOrder.IsDeleted = true;
            entTransactions.ForEach(x => x.IsDeleted = true);
            var result = await api.UpdateSalesOrder(new SalesOrderDTO(salesOrder, entTransactions));
            if (result.Id > 0)
            {
                if (!string.IsNullOrEmpty(result.Message))
                    Dialog.DialogMethod(result.Message);

                await NewClick();
            }
            else
            {
                Dialog.DialogMethod(result.Message);
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
        if (salesOrder.SalesOrderId == 0 || !entTransactions.Any()) { return; }
        if (await Dialog.DeleteDialogMethod())
        {
            var result = await api.DeleteSalesOrder(salesOrder.SalesOrderId);
            if (string.IsNullOrEmpty(result.Message))
            {
                await NewClick();
            }
            else
            {
                Dialog.DialogMethod(result.Message);
            }
        }
    }
    async Task NewClick()
    {
        salesOrder = new();
        sQuotationNo = string.Empty;
        if (IsAutoCode)
        {
            await LoadAutoCodeIfEnabled();
        }
        else
        {
            salesOrder.SalesOrderNo = (await api.GetNewSalesOrderDocNo(AppData.BranchId)).ToString();
        }
        salesOrder.SalesOrderDate = DateTime.Now.Date;
        salesOrder.FK_CustomerId = customerList.FirstOrDefault()!.Id;
        salesOrder.CustomerName = customerList.FirstOrDefault()!.Name;
        salesOrder.FK_SalesManId = salesmanList.FirstOrDefault()!.Id;
        if (projectList.Any())
        {
            salesOrder.FK_ProjectId = projectList.FirstOrDefault()!.Id;
        }
        salesOrder.AdditionalAmount = 0;
        salesOrder.Freight = 0;
        salesOrder.AdditionalDiscAmnt = 0;
        salesOrder.AdditionalDiscPertge = 0;
        salesOrder.AdditionalDiscAmnt = 0;
        salesOrder.AdditionalDiscPertge = 0;
        salesOrder.AdditionalDiscountType = 1;
        salesOrder.IsDeleted = false;
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
            if (salesOrder.SalesOrderId == 0 || !entTransactions.Any()) { return; }
            if (pdto.hdr == null || !pdto.dtls.Any()) { return; }
            var hdr = salesOrder.CopyTo<TransactionHdr>();
            hdr.HdrNo = salesOrder.SalesOrderNo;
            hdr.InvoiceDate = (salesOrder.SalesOrderDate ?? DateTime.Now).ToString("dd-MM-yyyy");
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
        var result = await Dialog.TransactionSearchMethod("Sales Order");
        if (result is not null)
        {
            var dto = await api.GetSalesOrderById(result.Id);
            entTransactions = dto.OrderDtls;
            entTransactions.ForEach(x => x.SL = entTransactions.IndexOf(x) + 1);
            AddNewRow();
            salesOrder = dto.OrderHdr;
            var customer = await api.GetCustomerById(salesOrder.FK_CustomerId);
            party = customer.CopyTo<Party>();
            DisablePrintButton = false;
            if (salesOrder.SalesQuotationId > 0)
            {
                sQuotationNo = salesOrder.QuotationNo ?? "";
            }
            TotalFindMethod();
            // Logic aligned with SalesQuotationView/SalesInvoiceView
            if (salesOrder.AdditionalDiscountType != 2)
            {
                int iType = salesOrder.AdditionalDiscAmnt > 0 ? 2 : 1;
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
        if (IsAutoCode && salesOrder.SalesOrderId == 0)
        {
            salesOrder.SalesOrderNo = await api.GetNextSalesOrderCode();
            StateHasChanged();
        }
    }
    #endregion

    #region Transaction Grid Settings & Calculation Helpers
    private TransactionGridSettings gridSettings => PermanentSettingsData.GetTransactionSettings("SalesOrder");

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

    private void OnRowRateChanged(TransactionDtls row)
    {
        if (row == null || row.FK_ItemId == 0) return;
        UpdateInRate(row);
        TransactionHelper.GetTransactionAfterEdit(row, false);
        TotalFindMethod();
        StateHasChanged();
    }

    private void OnRowIncRateChanged(TransactionDtls row)
    {
        if (row == null || row.FK_ItemId == 0) return;
        UpdateExRate(row);
        TransactionHelper.GetTransactionAfterEdit(row, true);
        TotalFindMethod();
        StateHasChanged();
    }

    private void OnRowQuantityChanged(TransactionDtls row)
    {
        if (row == null || row.FK_ItemId == 0) return;
        if (row.InRate == 0 && row.ExRate > 0)
        {
            UpdateInRate(row);
        }
        TransactionHelper.GetTransactionAfterEdit(row, gridSettings.ShowIncRate);
        TotalFindMethod();
        StateHasChanged();
    }

    private void OnRowDiscountPercChanged(TransactionDtls row)
    {
        if (row == null || row.FK_ItemId == 0) return;
        TransactionHelper.GetTransactionAfterEdit(row, gridSettings.ShowIncRate);
        TotalFindMethod();
        StateHasChanged();
    }

    private void OnRowDiscountAmountChanged(TransactionDtls row)
    {
        if (row == null || row.FK_ItemId == 0) return;
        if (row.GrossAmount > 0)
        {
            row.DiscountPerc = Math.Round((row.DiscountAmnt / row.GrossAmount) * 100m, 2, MidpointRounding.AwayFromZero);
        }
        TransactionHelper.GetTransactionAfterEdit(row, gridSettings.ShowIncRate, true);
        TotalFindMethod();
        StateHasChanged();
    }
    #endregion
}
