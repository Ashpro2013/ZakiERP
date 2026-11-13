using ZakiSAASERP.Shared.Pages.Masters;
using Microsoft.AspNetCore.Components.Web;
using ZakiSAASERP.Models.Settings;

namespace ZakiSAASERP.Shared.Pages.Transactions.Sales;

public partial class ProformaInvoiceView : ITabAware
{

    #region Variables
    [Parameter] public int TabId { get; set; }
    [Inject] IBrowserViewportService ResizeService { get; set; }
    [Inject] ISnackbar Snackbar { get; set; }
    [Inject] IJSRuntime JSRuntime { get; set; }
    List<TransactionDtls> entTransactions = new();
    private string _searchString;
    ProformaInvoiceHdr proformaInvoice = new();
    TransactionDtls transaction = new();
    List<ExMasters> products = new();
    List<CommonModel>? customerList = new();
    List<CommonModel> unitlist = new();
    List<CommonModel> itemUnitlist = new();
    List<CommonModel> salesmanList = new();
    List<AccountMaster> accountList = new();
    List<CommonModel> projectList = new();
    List<VatMaster> vatList = new();
    decimal defaultVatPerc = 0;
    List<StoreMaster> storeList = new();

    string quotationNo = "";
    bool IsEditing = false;
    bool DisablePrintButton = true;
    private bool IsAutoCode => PermanentSettingsData.AutoProformaInvoiceNo;
    private bool IsUnitSelectionDisabled => PermanentSettingsData.ItemSearchMode == 2; // Disable when MultiUnit mode
    MudTextField<string> txtInvoiceNo;
    MudDatePicker txtInvoiceDate;
    MudTextField<string> txtRefNo;
    MudTextField<string> txtCustomer;
    MudSelect<int> cmbSalesMan;
    MudTextField<string> txtRemarks;
    MudNumericField<decimal?>? txtAdditional;
    MudNumericField<decimal?>? txtFreight;
    MudNumericField<decimal?>? txtAdditionalDisc;
    MudSelect<int?> cmbProject;
    MudNumericField<decimal> txtQuantity;
    MudTextField<string> txtBarcode;
    MudTextField<string> textField;
    MudTextField<string> txtCustomerName;
    MudTextField<string> txtCustomerSearch;
    MudTextField<string> txtVatNo;
    MudSelect<string> cmbBillType;
    MudTextField<string> txtAccountNo;
    MudTextField<string> txtAccountDesc;
    MudTextField<string> txtAttention;
    MudTextField<string> txtDeliveryTo;
    MudTextField<string> txtSectionNo;

    MudNumericField<decimal> txtRate;
    MudNumericField<decimal> txtDiscount;
    private string _customerCode = string.Empty;
    private bool IsCashCustomer => _customerCode?.Equals("C1000", StringComparison.OrdinalIgnoreCase) ?? false;
    MudNumericField<decimal> txtDiscPerc;
    MudNumericField<decimal> txtTax;
    MudNumericField<decimal> txtNet;
    private int _activeTabIndex = 0;
    string strQuantityFormat => PermanentSettingsData.QuantityFormat;
    string strCurencyFormat => PermanentSettingsData.CurrencyFormat;
    public int Width { get; set; }
    public decimal Gross { get; set; } = 0;


    // Constants
    private const int MOBILE_BREAKPOINT = 600;
    private const decimal DEFAULT_QUANTITY = 1;
    private const decimal DEFAULT_QTY_PER_UNIT = 1;
    private const decimal DEFAULT_DISCOUNT_PERC = 0;

    private WebUserPrivilege? privilege;
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
                privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "ProformaInvoiceTitle");
            }
            customerList = await api.GetCombofill("Customers", AppData.BranchId);
            unitlist = await api.GetCombofill("Unit", 1);
            salesmanList = await api.GetCombofill("SalesMan", 1);
            projectList = await api.GetCombofill("Project", 1);
            storeList = await api.GetAllStores();
            vatList = await api.GetAllVat();
            itemUnitlist = unitlist;
            products = await api.GetMasters("product", 1);

            var defaultVat = vatList.FirstOrDefault(v => v.IsDefault);
            defaultVatPerc = defaultVat?.Percentage ?? 0;

            if (Width > 600)
            {
                AddNewRow();
            }
            if (Id is not null && Id > 0)
            {
                await LoadProformaInvoice(Id ?? 0);
            }
            else
            {
                await NewClick();
            }
            StateHasChanged();
        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod(ex.Message);
        }
    }
    #endregion

    #region Methods
    async Task LoadFromQuotationAsync()
    {
        try
        {
            int? quotationId = null;


            var parameters = new DialogParameters { ["InitialSearch"] = quotationNo };
            var options = new DialogOptions
            {
                CloseOnEscapeKey = true,
                MaxWidth = MaxWidth.ExtraLarge,
                FullWidth = true
            };

            var dialog = await DialogService.ShowAsync<QuotationSearchDialog>(Localizer("SelectQuotation"), parameters, options);
            var result = await dialog.Result;

            if (!result.Canceled && result.Data is not null)
            {
                var selectedQuotation = (ExMasters)result.Data;
                quotationId = selectedQuotation.Id;
                quotationNo = selectedQuotation.Code; // Update the UI field
                // Move focus to next field to prevent accidental re-triggering of search dialog on Enter key
                await txtRefNo.FocusAsync();
            }

            if (quotationId.HasValue)
            {
                // Load the quotation and convert to proforma invoice
                var proformaDto = await api.CreateProformaFromQuotationAsync(quotationId.Value);

                if (proformaDto is not null)
                {
                    // Clear existing data
                    entTransactions.Clear();

                    // Load header
                    var newHdr = proformaDto.Hdr;

                    // Keep the current Invoice No (since we are creating a NEW proforma)
                    newHdr.ProformaInvoiceNo = proformaInvoice.ProformaInvoiceNo;
                    newHdr.ProformaInvoiceId = 0; // Ensure it's treated as new
                    newHdr.FK_BranchId = AppData.BranchId;
                    newHdr.FK_UserId = AppData.UserId ?? 0;
                    newHdr.FK_FinancialYearId = AppData.FYearId ?? 0;
                    newHdr.EntryDate = DateTime.Now;
                    newHdr.ProformaInvoiceDate = DateTime.Now.Date;

                    proformaInvoice = newHdr;
                    _customerCode = customerList.FirstOrDefault(x => x.Id == proformaInvoice.FK_CustomerId)?.Code ?? "";

                    // Load details
                    entTransactions = proformaDto.Dtls;

                    // Renumber SL
                    int sl = 1;
                    entTransactions.ForEach(x =>
                    {
                        x.SL = sl++;
                    });

                    // Add empty row if desktop
                    if (Width > 600)
                    {
                        AddNewRow();
                    }

                    // Recalculate totals to handle any differences between Quotation and Proforma logic
                    TotalFindMethod();

                    // Ensure Store logic
                    var defaultStore = storeList.FirstOrDefault(x => x.IsDefaultStore) ?? storeList.FirstOrDefault();
                    entTransactions.ForEach(x =>
                    {
                        if (x.FK_StoreId == 0 && defaultStore is not null) x.FK_StoreId = defaultStore.StoreId;
                    });

                    StateHasChanged();
                    await Dialog.DialogMethod(Localizer("QuotationLoadedSuccessfully"));

                    // Recalculate based on existing discount type in the quotation
                    // Logic aligned with SalesQuotationView/SalesInvoiceView
                    if (proformaInvoice.AdditionalDiscountType != 2)
                    {
                        int iType = proformaInvoice.AdditionalDiscAmnt > 0 ? 2 : 1;
                        await DiscountMethod(new FocusEventArgs(), iType);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod($"Error loading quotation: {ex.Message}");
        }
    }

    async Task NewClick()
    {
        proformaInvoice = new ProformaInvoiceHdr
        {
            ProformaInvoiceDate = DateTime.Now.Date,
            HijriDate = CrudHelper.ConvertToHijri(DateTime.Now.Date), // Initialize Hijri date
            FK_BranchId = AppData.BranchId,
            FK_UserId = AppData.UserId ?? 0,
            FK_FinancialYearId = AppData.FYearId ?? 0,
            BillType = "Cash",  // Default
            AdditionalDiscountType = 1,
        };
        quotationNo = ""; // Clear Quotation No

        entTransactions.Clear();

        if (IsAutoCode)
        {
            proformaInvoice.ProformaInvoiceNo = await api.GetNextProformaInvoiceCode();
        }
        else
        {
            proformaInvoice.ProformaInvoiceNo = (await api.GetNewProformaInvoiceNo(AppData.BranchId)).ToString();
        }

        if (Width > 600)
        {
            AddNewRow();
        }

        // Set Default Salesman
        if (salesmanList.Any())
        {
            var defaultSalesman = salesmanList.FirstOrDefault();
            proformaInvoice.FK_SalesManId = defaultSalesman!.Id;
        }

        // Set Default Customer (Cash Customer or First)
        if (customerList is not null && customerList.Any())
        {
            var cashCustomer = customerList.FirstOrDefault(x => x.Name.Contains("Cash Customer", StringComparison.OrdinalIgnoreCase) || x.Code.Contains("CASH", StringComparison.OrdinalIgnoreCase));
            if (cashCustomer is not null)
            {
                proformaInvoice.FK_CustomerId = cashCustomer.Id;
                proformaInvoice.CustomerName = cashCustomer.Name;
                _customerCode = cashCustomer.Code;
                proformaInvoice.BillType = "Cash";
            }
            else
            {
                proformaInvoice.FK_CustomerId = customerList.First().Id;
                proformaInvoice.CustomerName = customerList.First().Name;
                _customerCode = customerList.First().Code;
            }
        }
        DisablePrintButton = true;
        StateHasChanged();
    }

    async Task DeleteClickAsync()
    {
        try
        {
            if (privilege is not null && !privilege.DeleteData)
            {
                await Dialog.DialogMethod(Localizer("No...Privilege"));
                return;
            }
            var result = await Dialog.DeleteDialogMethod();
            if (result == true)  // DeleteDialogMethod returns bool
            {
                var response = await api.DeleteProformaInvoice(proformaInvoice.ProformaInvoiceId);
                if (response.Id > 0)
                {
                    await Dialog.DialogMethod(Localizer("DeletedSuccessfully"));
                    await NewClick();
                }
                else
                {
                    await Dialog.DialogMethod(response.Message ?? Localizer("DeleteFailed"));
                }
            }
        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod($"Error: {ex.Message}");
        }
    }
    async Task LoadProformaInvoice(int id)
    {
        try
        {
            var dto = await api.GetProformaInvoiceById(id);
            if (dto is not null)
            {
                proformaInvoice = dto.Hdr;
                if (proformaInvoice.SourceDocType == "Quotation")
                {
                    quotationNo = proformaInvoice.SourceDocNo;
                }
                else
                {
                    quotationNo = "";
                }
                _customerCode = customerList.FirstOrDefault(x => x.Id == proformaInvoice.FK_CustomerId)?.Code ?? "";
                entTransactions = dto.Dtls;

                int sl = 1;
                entTransactions.ForEach(x => x.SL = sl++);

                if (Width > 600 && (!entTransactions.Any() || !string.IsNullOrEmpty(entTransactions.LastOrDefault()?.ItemName)))
                {
                    AddNewRow();
                }
                DisablePrintButton = false;
                TotalFindMethod();
                // Logic aligned with SalesQuotationView/SalesInvoiceView
                if (proformaInvoice.AdditionalDiscountType != 2)
                {
                    int iType = proformaInvoice.AdditionalDiscAmnt > 0 ? 2 : 1;
                    await DiscountMethod(new FocusEventArgs(), iType);
                }
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod($"Error: {ex.Message}");
        }
    }
    async Task SearchClickAsync()
    {
        try
        {
            var result = await Dialog.TransactionSearchMethod("Proforma Invoice");
            if (result is not null && result is ExMasters selected)
            {
                var dto = await api.GetProformaInvoiceById(selected.Id);
                if (dto is not null)
                {
                    proformaInvoice = dto.Hdr;
                    if (proformaInvoice.SourceDocType == "Quotation")
                    {
                        quotationNo = proformaInvoice.SourceDocNo;
                    }
                    else
                    {
                        quotationNo = "";
                    }
                    _customerCode = customerList.FirstOrDefault(x => x.Id == proformaInvoice.FK_CustomerId)?.Code ?? "";
                    entTransactions = dto.Dtls;

                    int sl = 1;
                    entTransactions.ForEach(x => x.SL = sl++);

                    if (Width > 600 && (!entTransactions.Any() || !string.IsNullOrEmpty(entTransactions.LastOrDefault()?.ItemName)))
                    {
                        AddNewRow();
                    }
                    DisablePrintButton = false;
                    TotalFindMethod();
                    // Logic aligned with SalesQuotationView/SalesInvoiceView
                    if (proformaInvoice.AdditionalDiscountType != 2)
                    {
                        int iType = proformaInvoice.AdditionalDiscAmnt > 0 ? 2 : 1;
                        await DiscountMethod(new FocusEventArgs(), iType);
                    }
                    StateHasChanged();
                }
            }
        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod($"Error: {ex.Message}");
        }
    }

    async Task FocusMethod()
    {
        if (!entTransactions.Any() || !string.IsNullOrEmpty(entTransactions.LastOrDefault()!.ItemName))
        {
            AddNewRow();
        }
        StateHasChanged();
        IsEditing = false;
        if (Width > 600)
        {
            await textField.FocusAsync();
        }
        else
        {
            await txtBarcode.FocusAsync();
        }
    }
    async Task CreatePartyMethod()
    {
        var result = await Dialog.MasterCreationDialogMethod<CustomerMasterView>();
        if (!result!.Canceled && result.Data is not null)
        {
            customerList.Add(AppData.PassingValue);
            proformaInvoice.FK_CustomerId = AppData.PassingValue.Id;
            proformaInvoice.CustomerName = AppData.PassingValue.Name;
            StateHasChanged();
        }
    }
    async Task CreateSManMethod()
    {
        var result = await Dialog.MasterCreationDialogMethod<SalesManMasterView>();
        if (!result!.Canceled && result.Data is not null)
        {
            salesmanList.Add(AppData.PassingValue);
            proformaInvoice.FK_SalesManId = AppData.PassingValue.Id;
            StateHasChanged();
        }
    }
    async Task PartySearchMethod()
    {
        var parameters = new DialogParameters();
        var dialog = DialogService.Show<CustomerSearchDialog>("Customer Search", parameters);
        var result = await dialog.Result;
        if (!result.Canceled)
        {
            var customer = (CustomerMaster)result.Data;
            _customerCode = customer.Code;
            proformaInvoice.FK_CustomerId = customer.CustomerId;
            proformaInvoice.CustomerName = customer.Name;
            proformaInvoice.CustomerVatNo = customer.VatNo;

            // Auto-fill Logic
            proformaInvoice.CreditDays = (int?)customer.CreditDays;

            // Auto-fill Bill Type
            if (!string.IsNullOrEmpty(customer.CusBillType))
            {
                proformaInvoice.BillType = customer.CusBillType;
            }
            else if (IsCashCustomer)
            {
                proformaInvoice.BillType = "Cash";
            }
            else
            {
                proformaInvoice.BillType = customer.CreditDays > 0 ? "Credit" : "Cash";
            }

            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task HandleCustomerSearchKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            if (string.IsNullOrWhiteSpace(_customerCode))
            {
                await PartySearchMethod();
            }
            else
            {
                // Smart Search Logic on customerList (CommonModel)
                var matches = customerList.Where(x =>
                    (x.Code?.Contains(_customerCode, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (x.Name?.Contains(_customerCode, StringComparison.OrdinalIgnoreCase) ?? false)
                ).ToList();

                if (matches.Count == 1)
                {
                    var match = matches.First();
                    // Fetch full details
                    var customer = await api.GetCustomerById(match.Id);

                    _customerCode = customer.Code;
                    proformaInvoice.FK_CustomerId = customer.CustomerId;
                    proformaInvoice.CustomerName = customer.Name;
                    proformaInvoice.CustomerVatNo = customer.VatNo;

                    proformaInvoice.CreditDays = (int?)customer.CreditDays;

                    if (!string.IsNullOrEmpty(customer.CusBillType))
                    {
                        proformaInvoice.BillType = customer.CusBillType;
                    }
                    else if (IsCashCustomer) proformaInvoice.BillType = "Cash";
                    else proformaInvoice.BillType = customer.CreditDays > 0 ? "Credit" : "Cash";

                    await InvokeAsync(StateHasChanged);
                    await txtCustomerName.FocusAsync(); // Move focus
                }
                else
                {
                    // 0 or Multiple matches -> Open Dialog
                    await PartySearchMethod();
                }
            }
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
        proformaInvoice.GrossAmount = entTransactions.Sum(x => x.GrossAmount);
        proformaInvoice.LineDsicount = entTransactions.Sum(x => x.DiscountAmnt);
        proformaInvoice.TaxableAmount = entTransactions.Sum(x => x.TaxableAmnt);
        proformaInvoice.TaxTotal = entTransactions.Sum(x => x.TaxAmount);
        proformaInvoice.NetAmount = entTransactions.Sum(x => x.LineTotal);
        proformaInvoice.GrandTotal = proformaInvoice.NetAmount;
    }

    async Task DiscountMethod(FocusEventArgs e, int inputType)
    {
        // inputType: 1=Percentage, 2=Amount

        if (proformaInvoice.AdditionalDiscountType == 2)
        {
            var result = await TransactionHelper.AdditionalDiscountMethod(entTransactions, inputType, proformaInvoice.AdditionalDiscPertge, proformaInvoice.AdditionalDiscAmnt);
            proformaInvoice.AdditionalDiscPertge = result.Item1 ?? 0;
            proformaInvoice.AdditionalDiscAmnt = result.Item2 ?? 0;
            TotalFindMethod();
        }
        else if (proformaInvoice.AdditionalDiscountType == 1)
        {
            var result = await TransactionHelper.AddDiscType2Method(entTransactions, inputType, proformaInvoice.AdditionalDiscPertge, proformaInvoice.AdditionalDiscAmnt, proformaInvoice.GrandTotal);
            proformaInvoice.AdditionalDiscPertge = result.Item1 ?? 0;
            proformaInvoice.AdditionalDiscAmnt = result.Item2 ?? 0;
            TotalFindMethod();
        }
        else
        {
            // For Type 3 (Net Amount / Round)
            decimal baseTotal = proformaInvoice.NetAmount + (proformaInvoice.AdditionalAmount ?? 0);
            var result = TransactionHelper.AddDiscType3Method(inputType, proformaInvoice.AdditionalDiscPertge, proformaInvoice.AdditionalDiscAmnt, baseTotal);
            proformaInvoice.AdditionalDiscPertge = result.Item1 ?? 0;
            proformaInvoice.AdditionalDiscAmnt = result.Item2 ?? 0;
            proformaInvoice.GrandTotal = result.Item3 ?? 0;
        }
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
    private void OnBlurQtEvent(FocusEventArgs arg)
    {
        if (transaction.FK_ItemId == 0) { return; }
        transaction = TransactionHelper.GetTransactionAfterEdit(transaction, gridSettings.ShowIncRate);
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

            // Conditional item name based on ItemSearchMode
            if (PermanentSettingsData.ItemSearchMode == 2)
            {
                // MultiUnit mode - use MultiUnitName
                transaction.ItemName = selectedProduct.MultiUnitName;
                transaction.ItemArabicName = selectedProduct.ItemNameArabic;
            }
            else
            {
                // ItemCode mode - fetch ItemMaster to get ItemName
                var itemMaster = await api.GetItemById(selectedProduct.FK_ItemId);
                transaction.ItemName = itemMaster?.ItemName ?? selectedProduct.MultiUnitName;
                transaction.ItemArabicName = itemMaster?.ItemNameArabic ?? selectedProduct.ItemNameArabic;
            }

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

            // Update itemUnitlist for mobile view
            itemUnitlist = selectedProduct.AvailableUnits ?? unitlist.ToList();

            TotalFindMethod();
        }
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
        var selectedProduct = await Dialog.GetItemSearchMethod(sVal);
        if (selectedProduct is not null)
        {
            IsEditing = true;
            transaction.FK_ItemId = selectedProduct.FK_ItemId;

            // Conditional item name based on ItemSearchMode
            if (PermanentSettingsData.ItemSearchMode == 2)
            {
                // MultiUnit mode - use MultiUnitName
                transaction.ItemName = selectedProduct.MultiUnitName;
                transaction.ItemArabicName = selectedProduct.ItemNameArabic;
            }
            else
            {
                // ItemCode mode - fetch ItemMaster to get ItemName
                var itemMaster = await api.GetItemById(selectedProduct.FK_ItemId);
                transaction.ItemName = itemMaster?.ItemName ?? selectedProduct.MultiUnitName;
                transaction.ItemArabicName = itemMaster?.ItemNameArabic ?? selectedProduct.ItemNameArabic;
            }

            transaction.Barcode = selectedProduct.Barcode;
            transaction.FK_UnitId = selectedProduct.FK_UnitId;
            transaction.UnitName = unitlist.FirstOrDefault(x => x.Id == selectedProduct.FK_UnitId)?.Name;
            transaction.Quantity = 1;
            transaction.QtyPerUnit = 1;
            transaction.ExRate = selectedProduct.Rate ?? 0;
            transaction.InRate = selectedProduct.IncRate ?? 0;
            transaction.TaxPerc = await api.GetValueMethod("vatFromItemId", selectedProduct.FK_ItemId);
            transaction.DiscountPerc = 0;
            transaction.DiscountPerc = 0;
            transaction = TransactionHelper.GetTransactionAfterEdit(transaction, gridSettings.ShowIncRate);

            // Update itemUnitlist for mobile view only
            itemUnitlist = selectedProduct.AvailableUnits ?? unitlist.ToList();

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
                await txtInvoiceDate.FocusAsync();
            }
            else if (fieldId == 3)
            {
                await txtCustomer.FocusAsync();
            }
            else if (fieldId == 4)
            {
                await cmbSalesMan.FocusAsync();
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
            await txtInvoiceDate.FocusAsync();
        }
        else if (fieldId == 2)
        {
            await cmbSalesMan.FocusAsync();
        }
        else if (fieldId == 3)
        {

            cmbSalesMan?.FocusAsync();
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

    async Task OnChangeUnit(int iUnitId, TransactionDtls item)
    {
        if (item.FK_ItemId == 0) { return; }
        // Prevent unnecessary updates on load or if ID is invalid
        if (iUnitId <= 0 || item.FK_UnitId == iUnitId) { return; }
        item.FK_UnitId = iUnitId;
        item.UnitName = unitlist.FirstOrDefault(x => x.Id == iUnitId)?.Name;
        var multiUnit = await api.GetItemMultiUnit(item.FK_ItemId, iUnitId, "");
        if (multiUnit is not null)
        {
            item.QtyPerUnit = multiUnit.QTYPerUnit ?? 0;
            item.ExRate = multiUnit.Rate ?? 0;

            // Conditional item name and barcode based on ItemSearchMode
            if (PermanentSettingsData.ItemSearchMode == 2)
            {
                // MultiUnit mode - update MultiUnitName and barcode
                item.ItemName = multiUnit.MultiUnitName;
                item.ItemArabicName = multiUnit.MultiUnitArabicName;
                item.Barcode = multiUnit.Barcode; // Only change barcode in MultiUnit mode
            }
            // In ItemCode mode, keep the original ItemName and Barcode (don't change them)
        }
        TransactionHelper.GetTransactionAfterEdit(item, gridSettings.ShowIncRate);
        TotalFindMethod();
        StateHasChanged();
    }

    private async Task OnLineQuantityChanged(FocusEventArgs arg, TransactionDtls line)
    {
        if (line.FK_ItemId == 0) return;
        TransactionHelper.GetTransactionAfterEdit(line, gridSettings.ShowIncRate);
        TotalFindMethod();
        StateHasChanged();
    }
    async Task TaskOnEnterMethod(KeyboardEventArgs e, int index)
    {
        if (e.Key == "Enter")
        {
            switch (index)
            {
                case 2: // Quantity
                    await txtRate.FocusAsync();
                    await txtRate.SelectAsync();
                    break;
                case 4: // Rate
                    await txtDiscPerc.FocusAsync();
                    await txtDiscPerc.SelectAsync();
                    break;
                case 5: // Disc%
                    await txtDiscount.FocusAsync();
                    await txtDiscount.SelectAsync();
                    break;
                case 6: // DiscAmt
                    // Skip VAT and Net Amount (Read Only)
                    // Trigger AddNewRow directly
                    AddNewRow();
                    await Task.Delay(100);
                    // Focus on the new row's barcode field
                    await textField.FocusAsync();
                    break;
                case 7: // Net Amount
                    // Fallback if somehow Net gets focus
                    AddNewRow();
                    await Task.Delay(100);
                    await textField.FocusAsync();
                    break;
                default:
                    break;
            }
            IsEditing = true;
            TotalFindMethod();
        }
    }


    async Task OnEnterMethod(KeyboardEventArgs e, TransactionDtls item)
    {
        if (e.Key == "Enter")
        {
            if (string.IsNullOrEmpty(item.Barcode))
            {
                await BarcodeChanged("", item);
            }
            else
            {
                var multiUnit = await api.GetItemMultiUnit(item.FK_ItemId, item.FK_UnitId, item.Barcode);
                if (multiUnit is not null)
                {
                    item.ItemName = multiUnit.MultiUnitName;
                    item.Barcode = multiUnit.Barcode;
                    item.ItemArabicName = multiUnit.MultiUnitArabicName;
                    item.QtyPerUnit = (multiUnit.QTYPerUnit ?? 0) > 0 ? multiUnit.QTYPerUnit.Value : 1;
                    item.FK_UnitId = multiUnit.FK_UnitId;
                    item.FK_ItemId = multiUnit.FK_ItemId;
                    item.UnitName = unitlist.FirstOrDefault(x => x.Id == item.FK_UnitId)?.Name;
                    item.ExRate = multiUnit.Rate ?? 0;
                    item.InRate = multiUnit.IncRate ?? 0;
                    item.TaxPerc = await api.GetValueMethod("vatFromItemId", multiUnit.FK_ItemId);
                    item.DiscountPerc = 0;
                    item.Quantity = 1;
                    item = TransactionHelper.GetTransactionAfterEdit(item, gridSettings.ShowIncRate);
                    var allUnits = await api.GetCombofill("munit", multiUnit.FK_ItemId);
                    item.AvailableUnits = allUnits ?? new List<CommonModel>();
                    itemUnitlist = item.AvailableUnits.ToList();
                }
                else
                {
                    await BarcodeChanged(item.Barcode, item);
                }
            }
            await txtQuantity.FocusAsync();
            StateHasChanged();
        }
    }
    async Task OnBarcodBlurEvent(FocusEventArgs arg, TransactionDtls item)
    {
        if (string.IsNullOrWhiteSpace(item.Barcode) || item.FK_ItemId > 0) { return; }
        var multiUnit = await api.GetItemMultiUnit(item.FK_ItemId, item.FK_UnitId, item.Barcode);
        if (multiUnit == null) { return; }
        item.QtyPerUnit = (multiUnit.QTYPerUnit ?? 0) > 0 ? multiUnit.QTYPerUnit.Value : 1;
        item.ExRate = multiUnit.Rate ?? 0;
        item.ItemName = multiUnit.MultiUnitName;
        item.UnitName = unitlist.FirstOrDefault(x => x.Id == multiUnit.FK_UnitId)?.Name;
        item.Barcode = multiUnit.Barcode;
        item.ItemArabicName = multiUnit.MultiUnitArabicName;
        item.FK_ItemId = multiUnit.FK_ItemId;
        item.Quantity = 1;
        item.FK_UnitId = multiUnit.FK_UnitId;
        item.TaxPerc = await api.GetValueMethod("vatFromItemId", multiUnit.FK_ItemId);
        item.DiscountPerc = 0;
        item = TransactionHelper.GetTransactionAfterEdit(item, gridSettings.ShowIncRate);
        var allUnits = await api.GetCombofill("munit", multiUnit.FK_ItemId);
        item.AvailableUnits = allUnits ?? new List<CommonModel>();
        itemUnitlist = item.AvailableUnits.ToList();
        TotalFindMethod();
        StateHasChanged();
    }

    #region Item Validation Methods

    /// <summary>
    /// Validates all items in the grid have valid item IDs
    /// </summary>
    private bool ValidateAllItemsExist()
    {
        var invalidItems = entTransactions.Where(x =>
            !string.IsNullOrEmpty(x.ItemName) && x.FK_ItemId == 0).ToList();

        if (invalidItems.Any())
        {
            var itemNames = string.Join(", ", invalidItems.Select(x => x.Barcode ?? x.ItemName));
            Snackbar.Add($"{Localizer("InvalidItemsFound")}: {itemNames}", Severity.Error);
            return false;
        }

        return true;
    }

    #endregion
    async Task BarcodeChanged(string sVal, TransactionDtls item)
    {
        if (string.IsNullOrEmpty(sVal) && IsEditing) { return; }

        // Smart Search: Only for ItemCode mode (mode 1)
        // In MultiUnit mode (mode 2), always open dialog
        if (PermanentSettingsData.ItemSearchMode == 1 && !string.IsNullOrWhiteSpace(sVal))
        {
            var pagination = new PaginationModel(1, 2, "ItemCode", "asc", sVal);
            var searchResult = await api.GetItemPDetails(pagination);

            if (searchResult.TotalCount == 1 && searchResult.PItems.Count == 1)
            {
                // Single match found - auto select
                var selectedItem = searchResult.PItems[0];
                IsEditing = true;
                item.FK_ItemId = selectedItem.ItemId;

                // ItemCode mode - use ItemName
                item.ItemName = selectedItem.ItemName;
                item.ItemArabicName = selectedItem.ItemNameArabic;

                item.Barcode = selectedItem.Barcode;

                // Load available units for this item
                var fullItem = await api.GetItemById(selectedItem.ItemId);
                if (fullItem is not null)
                {
                    item.AvailableUnits = new List<CommonModel>();

                    // Add base unit
                    if (fullItem.Unit is not null)
                    {
                        item.AvailableUnits.Add(new CommonModel
                        {
                            Id = fullItem.Unit.UnitId,
                            Name = fullItem.Unit.Name
                        });
                    }

                    // Add multi units
                    if (fullItem.MultiUnits is not null)
                    {
                        foreach (var mu in fullItem.MultiUnits)
                        {
                            if (mu.Unit is not null && !item.AvailableUnits.Any(x => x.Id == mu.Unit.UnitId))
                            {
                                item.AvailableUnits.Add(new CommonModel
                                {
                                    Id = mu.Unit.UnitId,
                                    Name = mu.Unit.Name
                                });
                            }
                        }
                    }

                    // Set default unit to first available
                    var defaultUnit = item.AvailableUnits.FirstOrDefault();
                    item.FK_UnitId = defaultUnit?.Id ?? unitlist.FirstOrDefault()?.Id ?? 0;
                    item.UnitName = defaultUnit?.Name ?? unitlist.FirstOrDefault(x => x.Id == item.FK_UnitId)?.Name;
                }
                else
                {
                    // Fallback to global unitlist
                    item.AvailableUnits = unitlist.ToList();
                    item.FK_UnitId = selectedItem.FK_UnitId ?? unitlist.FirstOrDefault()?.Id ?? 0;
                    item.UnitName = unitlist.FirstOrDefault(x => x.Id == item.FK_UnitId)?.Name;
                }

                item.Quantity = DEFAULT_QUANTITY;
                item.QtyPerUnit = DEFAULT_QTY_PER_UNIT;
                item.ExRate = selectedItem.PurchaseRate ?? 0;
                item.InRate = selectedItem.InclusiveRate ?? 0;
                item.TaxPerc = await api.GetValueMethod("vatFromItemId", selectedItem.ItemId);
                item.DiscountPerc = DEFAULT_DISCOUNT_PERC;
                item = TransactionHelper.GetTransactionAfterEdit(item, gridSettings.ShowIncRate);

                // Update itemUnitlist for mobile view only (desktop uses global unitlist)
                itemUnitlist = item.AvailableUnits.ToList();

                TotalFindMethod();

                // Auto-focus on quantity field
                await Task.Delay(100);
                await txtQuantity.FocusAsync();
                return;
            }
        }

        // Multiple matches, no exact match, or MultiUnit mode - open dialog
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            MaxWidth = MaxWidth.Large
        };
        var selectedProduct = await Dialog.GetItemSearchMethod(sVal);
        if (selectedProduct is not null)
        {
            IsEditing = true;
            item.FK_ItemId = selectedProduct.FK_ItemId;

            // Conditional item name based on ItemSearchMode
            if (PermanentSettingsData.ItemSearchMode == 2)
            {
                // MultiUnit mode - use MultiUnitName from selection result
                item.ItemName = selectedProduct.MultiUnitName;
                item.ItemArabicName = selectedProduct.ItemNameArabic;
            }
            else
            {
                // ItemCode mode - fetch ItemMaster to get ItemName
                var itemMaster = await api.GetItemById(selectedProduct.FK_ItemId);
                item.ItemName = itemMaster?.ItemName ?? selectedProduct.MultiUnitName;
                item.ItemArabicName = itemMaster?.ItemNameArabic ?? selectedProduct.ItemNameArabic;
            }

            item.Barcode = selectedProduct.Barcode;
            item.FK_UnitId = selectedProduct.FK_UnitId;
            item.UnitName = unitlist.FirstOrDefault(x => x.Id == selectedProduct.FK_UnitId)?.Name;

            // Load available units for this item (This block is removed as per instruction)
            // The instruction implies a simpler assignment for itemUnitlist and doesn't explicitly mention item.AvailableUnits here.
            // However, item.AvailableUnits is used later for itemUnitlist.
            // Let's re-add the AvailableUnits logic for 'item' to ensure consistency,
            // but use the new itemUnitlist assignment.

            var fullItem = await api.GetItemById(selectedProduct.FK_ItemId);
            if (fullItem is not null)
            {
                item.AvailableUnits = new List<CommonModel>();

                // Add base unit
                if (fullItem.Unit is not null)
                {
                    item.AvailableUnits.Add(new CommonModel
                    {
                        Id = fullItem.Unit.UnitId,
                        Name = fullItem.Unit.Name
                    });
                }

                // Add multi units
                if (fullItem.MultiUnits is not null)
                {
                    foreach (var mu in fullItem.MultiUnits)
                    {
                        if (mu.Unit is not null && !item.AvailableUnits.Any(x => x.Id == mu.Unit.UnitId))
                        {
                            item.AvailableUnits.Add(new CommonModel
                            {
                                Id = mu.Unit.UnitId,
                                Name = mu.Unit.Name
                            });
                        }
                    }
                }
            }
            else
            {
                // Fallback: use AvailableUnits from selection result or global unitlist
                if (selectedProduct.AvailableUnits is not null && selectedProduct.AvailableUnits.Any())
                {
                    item.AvailableUnits = selectedProduct.AvailableUnits.ToList();
                }
                else
                {
                    item.AvailableUnits = unitlist.ToList();
                }
            }

            item.Quantity = DEFAULT_QUANTITY;
            item.QtyPerUnit = DEFAULT_QTY_PER_UNIT;

            // Get QtyPerUnit from multiUnit data
            var multiUnitData = await api.GetItemMultiUnit(selectedProduct.FK_ItemId, selectedProduct.FK_UnitId, "");
            if (multiUnitData is not null)
            {
                item.QtyPerUnit = multiUnitData.QTYPerUnit ?? DEFAULT_QTY_PER_UNIT;
            }

            item.ExRate = selectedProduct.Rate ?? 0;
            item.InRate = selectedProduct.IncRate ?? 0;
            item.TaxPerc = await api.GetValueMethod("vatFromItemId", selectedProduct.FK_ItemId);
            item.DiscountPerc = DEFAULT_DISCOUNT_PERC;
            item = TransactionHelper.GetTransactionAfterEdit(item, gridSettings.ShowIncRate);


            // Update itemUnitlist for mobile view only (desktop uses per-item AvailableUnits)
            itemUnitlist = item.AvailableUnits.ToList();

            TotalFindMethod();

            // Auto-focus on quantity field
            await Task.Delay(300);
            await txtQuantity.FocusAsync();
        }
    }

    private bool _isSaving = false;

    async Task SaveOrUpdate()
    {
        if (_isSaving) return;
        _isSaving = true;
        StateHasChanged();
        if (privilege is not null && proformaInvoice.ProformaInvoiceId == 0 && !privilege.SaveData)
        {
            await Dialog.DialogMethod(Localizer("No...Privilege"));
            _isSaving = false;
            return;
        }
        if (privilege is not null && proformaInvoice.ProformaInvoiceId != 0 && !privilege.EditData)
        {
            await Dialog.DialogMethod(Localizer("No...Privilege"));
            _isSaving = false;
            return;
        }

        try
        {
            // Run all validations
            if (!ValidateHeaderDetails()) return;
            if (!ValidateItemDetails()) return;
            if (!ValidateAllItemsExist()) return; // Validate all items exist
            if (!await CheckRateZeroConfirmation()) return;

            proformaInvoice.FK_BranchId = AppData.BranchId;
            proformaInvoice.FK_UserId = AppData.UserId ?? 0;
            proformaInvoice.FK_FinancialYearId = AppData.FYearId ?? 0;

            // Customer lookup is handled in ValidateHeaderDetails or PartySearchMethod, 
            // but for safety we ensure FK is set if name exists.
            var selectedCustomer = customerList.FirstOrDefault(x => x.Name == proformaInvoice.CustomerName);
            if (selectedCustomer is not null)
            {
                proformaInvoice.FK_CustomerId = selectedCustomer.Id;
            }

            if (proformaInvoice.ProformaInvoiceId == 0)
            {
                proformaInvoice.EntryDate = DateTime.Now;
                var result = await api.CreateProformaInvoice(new ProformaInvoiceDTO(proformaInvoice, entTransactions));
                if (string.IsNullOrEmpty(result.Message))
                {
                    proformaInvoice.ProformaInvoiceId = result.Id;
                    DisablePrintButton = false;

                    if (await Dialog.DeleteDialogMethod(Localizer("PrintDialog")))
                    {
                        await PrintMethod();
                    }

                    Snackbar.Add(Localizer("SaveMessage"), Severity.Success);
                    await NewClick();
                }
                else
                {
                    if (result.Message == "NoAlreadyExists")
                    {
                        Snackbar.Add(Localizer("NoAlreadyExists"), Severity.Error);
                    }
                    else
                    {
                        await Dialog.DialogMethod(result.Message);
                    }
                }
            }
            else
            {
                var result = await api.UpdateProformaInvoice(new ProformaInvoiceDTO(proformaInvoice, entTransactions));
                if (!string.IsNullOrEmpty(result.Message))
                {
                    if (result.Message == "NoAlreadyExists")
                    {
                        Snackbar.Add(Localizer("NoAlreadyExists"), Severity.Error);
                    }
                    else
                    {
                        await Dialog.DialogMethod(result.Message);
                    }
                }
                else
                {
                    if (await Dialog.DeleteDialogMethod(Localizer("PrintDialog")))
                    {
                        await PrintMethod();
                    }

                    Snackbar.Add(Localizer("UpdateMessage"), Severity.Info);
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
        if (proformaInvoice.FK_CustomerId == 0 && string.IsNullOrEmpty(proformaInvoice.CustomerName))
        {
            Snackbar.Add(Localizer("CustomerRequired"), Severity.Error);
            return false;
        }

        // Ensure FK is set if using Cash Customer fallback
        if (proformaInvoice.FK_CustomerId == 0)
        {
            var cashCustomer = customerList.FirstOrDefault(x => x.Name == "Cash Customer");
            if (cashCustomer is not null)
            {
                proformaInvoice.FK_CustomerId = cashCustomer.Id;
                proformaInvoice.CustomerName = cashCustomer.Name;
            }
            else
            {
                Snackbar.Add(Localizer("CustomerRequired"), Severity.Error);
                return false;
            }
        }

        if (proformaInvoice.FK_SalesManId == 0)
        {
            Snackbar.Add(Localizer("SalesManRequired"), Severity.Error);
            return false;
        }

        // VAT Validation
        if (!string.IsNullOrEmpty(proformaInvoice.CustomerVatNo))
        {
            if (proformaInvoice.CustomerVatNo.Length != 15 || !long.TryParse(proformaInvoice.CustomerVatNo, out _))
            {
                Snackbar.Add(Localizer("VATNoValidation"), Severity.Error);
                return false;
            }
        }

        return true;
    }

    bool ValidateItemDetails()
    {
        var validItems = entTransactions.Where(x => !string.IsNullOrEmpty(x.ItemName)).ToList();

        if (!validItems.Any())
        {
            Snackbar.Add(Localizer("NoItemsMessage"), Severity.Error);
            return false;
        }

        entTransactions = validItems;

        var zeroQtyItems = entTransactions.Where(x => x.Quantity == 0).ToList();
        if (zeroQtyItems.Any())
        {
            Snackbar.Add(Localizer("QtyCannotBeZero"), Severity.Error);
            return false;
        }

        if (proformaInvoice.GrossAmount == 0)
        {
            Snackbar.Add(Localizer("GrossAmountCannotBeZero"), Severity.Error);
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
        if (proformaInvoice.ProformaInvoiceId == 0 || !entTransactions.Any()) { return; }
        if (await Dialog.DeleteDialogMethod(Localizer("CancelDialog")))
        {
            proformaInvoice.IsDeleted = true;
            entTransactions.ForEach(x => x.IsDeleted = true);
            var result = await api.UpdateProformaInvoice(new ProformaInvoiceDTO(proformaInvoice, entTransactions));
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
        if (privilege is not null && !privilege.DeleteData)
        {
            await Dialog.DialogMethod(Localizer("No...Privilege"));
            return;
        }
        if (proformaInvoice.ProformaInvoiceId == 0 || !entTransactions.Any()) { return; }
        if (await Dialog.DeleteDialogMethod())
        {
            var result = await api.DeleteProformaInvoice(proformaInvoice.ProformaInvoiceId);
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
    async Task PrintHelperMethod(bool IsPreview = false)
    {
        try
        {
            if (proformaInvoice.ProformaInvoiceId == 0 || !entTransactions.Any()) { return; }

            var validTransactions = entTransactions.Where(x => !string.IsNullOrEmpty(x.ItemName) && x.FK_ItemId > 0).ToList();
            if (!validTransactions.Any())
            {
                Snackbar.Add(Localizer("NoItemsMessage"), Severity.Warning);
                return;
            }

            var pdto = await api.GetPrintDesignAsync(new PrintDesignReq(0, "Proforma_Invoice"));
            if (pdto.hdr == null || !pdto.dtls.Any())
            {
                await Dialog.DialogMethod(Localizer("PrintDesignNotFound"));
                return;
            }

            var hdr = proformaInvoice.CopyTo<TransactionHdr>();
            hdr.HdrNo = proformaInvoice.ProformaInvoiceNo;
            hdr.InvoiceDate = (proformaInvoice.ProformaInvoiceDate ?? DateTime.Now).ToString("dd-MM-yyyy");

            var selectedCustomer = customerList.FirstOrDefault(x => x.Id == proformaInvoice.FK_CustomerId);
            var party = new Party
            {
                CustomerId = selectedCustomer?.Id ?? 0,
                Name = proformaInvoice.CustomerName,
                Code = selectedCustomer?.Code ?? "",
                VatNo = proformaInvoice.CustomerVatNo
            };

            PrintParameterDTO dto = new PrintParameterDTO(pdto.hdr, pdto.dtls, hdr, validTransactions, party, AppData.CompanyDetails!, null);
            await CrudHelper.PrintInvoiceMethodLocal(Downloader, dto, Dialog, IsPreview);
        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod(ex.Message);
        }
    }
    async Task PrintPrviewMethod() => await PrintHelperMethod(true);
    async Task PrintMethod() => await PrintHelperMethod(false);
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
        transaction.SL = entTransactions.Count + 1;
        transaction.TaxPerc = defaultVatPerc;
        transaction.UnitName = unitlist.FirstOrDefault()!.Name;
        transaction.FK_UnitId = unitlist.FirstOrDefault()!.Id;
        transaction.FK_StoreId = defaultStoreId;
        entTransactions.Add(transaction);
    }



    // Calculation methods for discount and VAT
    private void OnDiscountPercChanged(TransactionDtls item)
    {
        if (item.DiscountPerc < 0 || item.DiscountPerc > 100)
        {
            item.DiscountPerc = 0;
            return;
        }

        TransactionHelper.GetTransactionAfterEdit(item, gridSettings.ShowIncRate);
        TotalFindMethod();
    }

    private void OnVatPercentageChanged(TransactionDtls item, decimal vatPerc)
    {
        item.TaxPerc = vatPerc;
        TransactionHelper.GetTransactionAfterEdit(item, gridSettings.ShowIncRate);
        TotalFindMethod();
    }

    private void OnDiscountAmountChanged(TransactionDtls item)
    {
        if (item.DiscountAmnt < 0 || item.DiscountAmnt > item.GrossAmount)
        {
            item.DiscountAmnt = 0;
            return;
        }

        // Calculate percentage from amount manually first to set the percentage, 
        // then let Helper handle the full recalculation
        if (item.GrossAmount > 0)
        {
            item.DiscountPerc = (item.DiscountAmnt / item.GrossAmount) * 100;
        }
        else
        {
            item.DiscountPerc = 0;
        }

        TransactionHelper.GetTransactionAfterEdit(item, gridSettings.ShowIncRate, true);
        TotalFindMethod();
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

            // Conditional item name and barcode based on ItemSearchMode
            if (PermanentSettingsData.ItemSearchMode == 2)
            {
                // MultiUnit mode - update MultiUnitName and barcode
                transaction.ItemName = multiUnit.MultiUnitName;
                transaction.ItemArabicName = multiUnit.MultiUnitArabicName;
                transaction.Barcode = multiUnit.Barcode;
            }
            // In ItemCode mode, keep the original ItemName and Barcode
        }
        transaction = TransactionHelper.GetTransactionAfterEdit(transaction, gridSettings.ShowIncRate);
        TotalFindMethod();
        StateHasChanged();
    }
    #endregion

    #region Focus Management
    private async Task FocusById(string id)
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("eval", $"var e = document.getElementById('{id}'); if(e) {{ var inp = e.querySelector('input'); if(inp) {{ inp.focus(); if(inp.select) inp.select(); }} else {{ e.focus(); if(e.select) e.select(); }} }}");
        }
        catch { /* Ignore if element not found */ }
    }
    #endregion

    #region UI Optimization Methods



    /// <summary>
    /// Handler for date changed - auto-convert to Hijri
    /// </summary>
    private async Task OnDateChanged(DateTime? newDate)
    {
        proformaInvoice.ProformaInvoiceDate = newDate;
        proformaInvoice.HijriDate = CrudHelper.ConvertToHijri(newDate);
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Handle Bill Type Change
    /// </summary>
    private async Task OnBillTypeChanged(string value)
    {
        if (value == "Credit")
        {
            if (proformaInvoice.FK_CustomerId <= 0)
            {
                Snackbar.Add("Please select a customer first to enable Credit.", Severity.Warning);
                // Reset to Cash if it was changing to Credit, or keep previous. 
                // Since bind is one-way + event, we just don't update if valid.
                // But we must force UI update to revert check.
                proformaInvoice.BillType = "Cash";
                await InvokeAsync(StateHasChanged);
                return;
            }

            var customer = customerList.FirstOrDefault(x => x.Id == proformaInvoice.FK_CustomerId);
            // Check for default customer (assuming Name contains "Cash Customer" or code "C1000")
            if (customer is not null && (customer.Name.Contains("Cash Customer", StringComparison.OrdinalIgnoreCase) || customer.Code == "C1000"))
            {
                Snackbar.Add("Credit bill type is not allowed for Cash Customer.", Severity.Warning);
                proformaInvoice.BillType = "Cash";
                await InvokeAsync(StateHasChanged);
                return;
            }
        }

        proformaInvoice.BillType = value;
        await InvokeAsync(StateHasChanged);
    }






    /// <summary>
    /// Account search method - Search from Account Master
    /// </summary>
    private async Task AccountSearchMethod()
    {
        try
        {
            var options = new DialogOptions
            {
                CloseOnEscapeKey = true,
                MaxWidth = MaxWidth.Large,
                BackdropClick = false
            };
            // No parameters needed as AccountSearchDialog now loads data internally
            var dialog = DialogService.Show<AccountSearchDialog>(Localizer("AccountSearchTitle"), options);
            var result = await dialog.Result;

            if (!result!.Canceled && result.Data is AccountMaster selectedAccount)
            {
                if (selectedAccount.AccountId > 0 || !string.IsNullOrEmpty(selectedAccount.Code))
                {
                    proformaInvoice.FK_AccountId = selectedAccount.AccountId;
                    proformaInvoice.AccountNo = selectedAccount.Code;
                    proformaInvoice.AccountName = selectedAccount.Name;
                    await InvokeAsync(StateHasChanged);
                }
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"{Localizer("ErrorSearchingAccount")}: {ex.Message}", Severity.Error);
        }
    }

    #endregion

    #region Transaction Grid Settings & Calculation Helpers
    private TransactionGridSettings gridSettings => PermanentSettingsData.GetTransactionSettings("ProformaInvoice");

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
        if (row.InRate == 0 && row.ExRate > 0) UpdateInRate(row);
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
