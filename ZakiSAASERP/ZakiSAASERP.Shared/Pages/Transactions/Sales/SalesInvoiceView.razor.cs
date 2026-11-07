using ZakiSAASERP.Models.Settings;
using ZakiSAASERP.Shared.Pages.Masters;
using Microsoft.AspNetCore.Components.Web;
using System.Text.Json;
using static MudBlazor.CategoryTypes;

namespace ZakiSAASERP.Shared.Pages.Transactions.Sales;

public partial class SalesInvoiceView : ITabAware
{

    #region Variables
    [Parameter] public int TabId { get; set; }
    [Inject] IBrowserViewportService ResizeService { get; set; }
    List<TransactionDtls> entTransactions = new();
    private string _searchString;
    SalesInvoiceHdr salesInvoice = new();
    TransactionDtls transaction = new();
    List<ExMasters> products = new();
    List<CommonModel>? customerList = new();
    List<CommonModel> unitlist = new();
    List<CommonModel> itemUnitlist = new();
    List<CommonModel> salesmanList = new();
    List<CommonModel> projectList = new();
    List<StoreMaster> storeList = new();
    Party party = new();
    private WebUserPrivilege? privilege;
    bool IsEditing = false;
    bool DisablePrintButton = false;
    private bool IsAutoCode => PermanentSettingsData.AutoSalesInvoiceNo;
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
    MudNumericField<decimal> txtRate;
    MudNumericField<decimal> txtDiscount;
    MudNumericField<decimal> txtTax;
    MudNumericField<decimal> txtNet;
    private int _activeTabIndex = 0;
    string strQuantityFormat => PermanentSettingsData.QuantityFormat;
    string strCurencyFormat => PermanentSettingsData.CurrencyFormat;
    public int Width { get; set; }
    public decimal Gross { get; set; } = 0;
    PrintDesignDTO? pdto;
    string sQuotationNo;
    string sDeliveryNoteNo;
    bool IsSymplified = true;
    HashSet<int> _selectedDeliveryNoteIds = new();
    private int heldInvoicesCount = 0;
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
                privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "SalesInvoiceTitle");
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
            if (Id is not null && Id > 0)
            {
                await LoadInvoice(Id ?? 0);
            }
            else
            {
                await NewClick();
            }
            await RefreshHeldInvoicesCountAsync();
            pdto = await indexdbHelper.LoadPrintDesignFromIndexedDB("Sales_Invoice");
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
        txtBarcode?.FocusAsync();
    }
    async Task CreatePartyMethod()
    {
        var result = await Dialog.MasterCreationDialogMethod<CustomerMasterView>();
        if (!result!.Canceled && result.Data is not null)
        {
            customerList.Add(AppData.PassingValue);
            salesInvoice.FK_CustomerId = AppData.PassingValue.Id;
            salesInvoice.CustomerName = AppData.PassingValue.Name;
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
            salesInvoice.FK_SalesManId = AppData.PassingValue.Id;
            salesInvoice.CustomerName = AppData.PassingValue.Name;
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
            salesInvoice.FK_CustomerId = selectedCustomer.CustomerId;
            salesInvoice.CustomerName = selectedCustomer.Name;
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
        salesInvoice.GrossAmount = entTransactions.Sum(x => x.GrossAmount);
        salesInvoice.LineDsicount = entTransactions.Sum(x => x.DiscountAmnt);
        salesInvoice.TaxableAmount = entTransactions.Sum(x => x.TaxableAmnt);
        salesInvoice.TaxTotal = entTransactions.Sum(x => x.TaxAmount);
        salesInvoice.NetAmount = entTransactions.Sum(x => x.LineTotal);
        salesInvoice.GrandTotal = salesInvoice.NetAmount;
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
            salesInvoice = dto.QuotationHdr.CopyTo<SalesInvoiceHdr>();
            salesInvoice.QuotationNo = dto.QuotationHdr.SalesQuotationNo;
            salesInvoice.QuotationId = dto.QuotationHdr.SalesQuotationId;
            salesInvoice.SalesInvoiceDate = DateTime.Now;
            salesInvoice.EntryDate = DateTime.Now;
            sQuotationNo = dto.QuotationHdr.SalesQuotationNo;
            if (IsAutoCode)
            {
                await LoadAutoCodeIfEnabled();
            }
            else
            {
                salesInvoice.SalesInvoiceNo = (await api.GetNewSalesInvoiceDocNo(AppData.BranchId)).ToString();
            }

            // Reset lines to pre-additional-discount state to avoid double counting
            TotalFindMethod();

            // Logic aligned with SalesQuotationView
            if (salesInvoice.AdditionalDiscountType != 2)
            {
                int iType = salesInvoice.AdditionalDiscAmnt > 0 ? 2 : 1;
                await DiscountMethod(new FocusEventArgs(), iType);
                // DEBUG
                // snackbar.Add($"Loaded Quotation. Type: {salesInvoice.AdditionalDiscountType}, Amt: {salesInvoice.AdditionalDiscAmnt}, GT: {salesInvoice.GrandTotal}", Severity.Info);
            }
            StateHasChanged();
        }
    }
    async Task SearchDeliveryNotesAsync()
    {
        if (salesInvoice.FK_CustomerId == 0)
        {
            snackbar.Add(Localizer("PleaseSelectCustomer"), Severity.Warning);
            return;
        }

        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            MaxWidth = MaxWidth.Large
        };
        var parameters = new DialogParameters { ["CustomerId"] = salesInvoice.FK_CustomerId, ["PreSelectedIds"] = _selectedDeliveryNoteIds };
        var dialog = await DialogService.ShowAsync<DeliveryNoteSelectionDialog>(Localizer("Pending Delivery Notes"), parameters, options);
        var result = await dialog.Result;

        if (!result.Canceled && result.Data is HashSet<PendingDeliveryNoteDTO> selectedNotes)
        {
            _selectedDeliveryNoteIds = selectedNotes.Select(x => x.DeliveryNoteId).ToHashSet();

            // Collect all valid DtlsIds from the currently selected notes
            var validDtlsIds = selectedNotes.SelectMany(x => x.Items).Select(i => i.DtlsId).ToHashSet();

            // 1. Remove items that are no longer selected
            // We remove items that have a link (FK_DeliveryNoteDtlsId > 0) BUT are not in the valid list.
            var itemsToRemove = entTransactions.Where(x => x.FK_DeliveryNoteDtlsId > 0 && !validDtlsIds.Contains(x.FK_DeliveryNoteDtlsId.Value)).ToList();
            foreach (var item in itemsToRemove)
            {
                entTransactions.Remove(item);
            }

            if (entTransactions.Count == 1 && string.IsNullOrEmpty(entTransactions.First().ItemName))
            {
                // Handle empty row logic if needed, but usually redundant if we just cleared or removed.
                // If we removed all, we might want to ensure a blank row exists? 
                // We'll leave that to the end or AddItems loop.
            }
            // If grid became empty after removal, ensure it's clean for adding
            if (!entTransactions.Any())
            {
                // entTransactions.Clear(); // Already empty
            }
            else if (entTransactions.Count == 1 && string.IsNullOrEmpty(entTransactions.First().ItemName))
            {
                entTransactions.Clear(); // Clear the placeholder
            }

            // 2. Add new items
            foreach (var note in selectedNotes)
            {
                foreach (var item in note.Items)
                {
                    // Check if item already exists
                    if (entTransactions.Any(x => x.FK_DeliveryNoteDtlsId == item.DtlsId)) continue;

                    // Convert DTO to TransactionDtls
                    var trans = item.CopyTo<TransactionDtls>();
                    trans.SL = entTransactions.Count + 1;
                    trans.Quantity = item.PendingQty; // Ensure we take pending
                    trans.FK_DeliveryNoteDtlsId = item.DtlsId; // IMPORTANT: Link
                                                               // trans.RefType = "DeliveryNote"; // If needed
                    entTransactions.Add(trans);
                }
            }

            // Re-sequence SL
            int sl = 1;
            entTransactions.ForEach(x => x.SL = sl++);

            sDeliveryNoteNo = string.Join(", ", selectedNotes.Select(x => x.DeliveryNoteNo));
            if (!entTransactions.Any()) AddNewRow(); // Ensure at least one row if everything removed

            // Apply discount/freight from the last (highest ID) Delivery Note
            // Calculate totals first to ensure GrandTotal is available for discount calculation
            TotalFindMethod();

            var lastNote = selectedNotes.OrderByDescending(x => x.DeliveryNoteId).FirstOrDefault();
            if (lastNote is not null)
            {
                var fullNoteDto = await api.GetDeliveryNoteById(lastNote.DeliveryNoteId);
                if (fullNoteDto?.DeliveryNoteHdr is not null)
                {
                    salesInvoice.AdditionalDiscountType = fullNoteDto.DeliveryNoteHdr.AdditionalDiscountType;
                    salesInvoice.AdditionalDiscPertge = fullNoteDto.DeliveryNoteHdr.AdditionalDiscPertge;
                    salesInvoice.AdditionalDiscAmnt = fullNoteDto.DeliveryNoteHdr.AdditionalDiscAmnt;
                    salesInvoice.Freight = fullNoteDto.DeliveryNoteHdr.Freight;
                    salesInvoice.AdditionalAmount = fullNoteDto.DeliveryNoteHdr.AdditionalAmount;

                    // Fix for Inclusive Discount (Type 2): Prevent value drift.
                    // Standardized Safe Logic: Trust the loaded values for Inclusive(2)
                    if (salesInvoice.AdditionalDiscountType != 2)
                    {
                        int iType = (salesInvoice.AdditionalDiscAmnt ?? 0) > 0 ? 2 : 1;
                        await DiscountMethod(new FocusEventArgs(), iType);
                    }
                }
                else
                {
                    TotalFindMethod();
                }
            }
            else
            {
                TotalFindMethod();
            }

            StateHasChanged();
        }
    }
    async Task DiscountMethod(FocusEventArgs e, int iType)
    {
        if (salesInvoice.AdditionalDiscountType == 2)
        {
            var result = await TransactionHelper.AdditionalDiscountMethod(entTransactions, iType, salesInvoice.AdditionalDiscPertge, salesInvoice.AdditionalDiscAmnt,strCurencyFormat);
            salesInvoice.AdditionalDiscPertge = result.Item1 ?? 0;
            salesInvoice.AdditionalDiscAmnt = result.Item2 ?? 0;
            TotalFindMethod();

        }
        else if (salesInvoice.AdditionalDiscountType == 1)
        {
            var result = await TransactionHelper.AddDiscType2Method(entTransactions, iType, salesInvoice.AdditionalDiscPertge, salesInvoice.AdditionalDiscAmnt, salesInvoice.GrandTotal);
            salesInvoice.AdditionalDiscPertge = result.Item1 ?? 0;
            salesInvoice.AdditionalDiscAmnt = result.Item2 ?? 0;
            TotalFindMethod();
        }
        else
        {
            decimal baseTotal = salesInvoice.NetAmount + (salesInvoice.AdditionalAmount ?? 0) + (salesInvoice.Freight ?? 0);
            var result = TransactionHelper.AddDiscType3Method(iType, salesInvoice.AdditionalDiscPertge, salesInvoice.AdditionalDiscAmnt, baseTotal);
            salesInvoice.AdditionalDiscPertge = result.Item1 ?? 0;
            salesInvoice.AdditionalDiscAmnt = result.Item2 ?? 0;
            salesInvoice.GrandTotal = result.Item3 ?? 0;
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
        transaction =TransactionHelper.GetTransactionAfterEdit(transaction, gridSettings.ShowIncRate);;
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
                    if(txtRate != null)
                    {
                        await txtRate.FocusAsync();
                        await txtRate.SelectAsync();
                    }
                    break;
                case 4:
                    if(txtDiscount != null)
                    {
                        await txtDiscount.FocusAsync();
                        await txtDiscount.SelectAsync();
                    }
                    else
                    {
                        if (txtTax != null)
                        {
                            await txtTax.FocusAsync();
                            await txtTax.SelectAsync();
                        }
                    }
                    break;
                case 6:
                case 5:
                    if(txtTax != null)
                    {
                        await txtTax.FocusAsync();
                        await txtTax.SelectAsync();
                    }
                    break;
                case 7:
                case 8:
                    if(txtNet != null)
                    {
                        await txtNet.FocusAsync();
                    }
                    break;
                case 9:
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
                    transaction.QtyPerUnit = (multiUnit.QTYPerUnit ?? 0) > 0 ? multiUnit.QTYPerUnit.Value : 1;
                    transaction.FK_UnitId = multiUnit.FK_UnitId;
                    transaction.FK_ItemId = multiUnit.FK_ItemId;
                    transaction.UnitName = unitlist.FirstOrDefault(x => x.Id == transaction.FK_UnitId).Name;
                    transaction.ExRate = multiUnit.Rate ?? 0;
                    transaction.InRate = multiUnit.IncRate ?? 0;
                    transaction.TaxPerc = await api.GetValueMethod("vatFromItemId", multiUnit.FK_ItemId);
                    transaction.Quantity = 1;
                    transaction =TransactionHelper.GetTransactionAfterEdit(transaction, gridSettings.ShowIncRate);;
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
        transaction =TransactionHelper.GetTransactionAfterEdit(transaction, gridSettings.ShowIncRate);;
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
            transaction.ItemArabicName = selectedProduct.MultiUnitArabicName;
            transaction.Barcode = selectedProduct.Barcode;
            transaction.FK_UnitId = selectedProduct.FK_UnitId;
            transaction.UnitName = unitlist.FirstOrDefault(x => x.Id == selectedProduct.FK_UnitId)?.Name;
            transaction.Quantity = 1;

            transaction.QtyPerUnit = selectedProduct.QtyPerUnit > 0 ? selectedProduct.QtyPerUnit : 1;

            transaction.ExRate = selectedProduct.Rate ?? 0;
            transaction.InRate = selectedProduct.IncRate ?? 0;
            transaction.TaxPerc = await api.GetValueMethod("vatFromItemId", selectedProduct.FK_ItemId);
            transaction.DiscountPerc = 0;
            transaction =TransactionHelper.GetTransactionAfterEdit(transaction, gridSettings.ShowIncRate);

            // Use pre-populated units from ItemSelectionResult (respects ItemSearchMode)
            unitlist.Clear();
            unitlist = selectedProduct.AvailableUnits ?? new List<CommonModel>();

            // Also update itemUnitlist for mobile view
            itemUnitlist = unitlist.ToList();

            TotalFindMethod();
        }
    }
    private bool _isSaving = false;

    async Task SaveOrUpdate()
    {
        if (_isSaving) return;
        if (privilege is not null && salesInvoice.SalesInvoiceId == 0 && !privilege.SaveData)
        {
            await Dialog.DialogMethod(Localizer("NoSavePrivilege"));
            return;
        }
        if (privilege is not null && salesInvoice.SalesInvoiceId != 0 && !privilege.EditData)
        {
            await Dialog.DialogMethod(Localizer("NoEditPrivilege"));
            return;
        }
        if (salesInvoice.SalesInvoiceId != 0 && !gridSettings.EnableUpdate)
        {
            await Dialog.DialogMethod(Localizer("UpdateDisabledInSettings"));
            return;
        }
        _isSaving = true;
        StateHasChanged();

        try
        {
            entTransactions = entTransactions.Where(x => !string.IsNullOrEmpty(x.ItemName)).ToList();
            if (!entTransactions.Any()) { await NewClick(); return; }
            salesInvoice.FK_BranchId = AppData.BranchId;
            salesInvoice.FK_UserId = AppData.UserId ?? 0;
            salesInvoice.FK_FinancialYearId = AppData.FYearId ?? 0;
            var selectedCustomer = customerList.FirstOrDefault(x => x.Name == salesInvoice.CustomerName);
            if (selectedCustomer is not null)
            {
                salesInvoice.FK_CustomerId = selectedCustomer.Id;
            }
            else
            {
                salesInvoice.FK_CustomerId = customerList.FirstOrDefault(x => x.Name == "Cash Customer")!.Id;
            }
            if (salesInvoice.SalesInvoiceId == 0)
            {
                // --- Credit Days Block Check ---
                if (PermanentSettingsData.BlockInvoiceOnCreditDaysExceeded
                    && string.Equals(salesInvoice.BillType, "Credit", StringComparison.OrdinalIgnoreCase)
                    && salesInvoice.FK_CustomerId > 0
                    && (party?.CreditDays ?? 0) > 0)
                {
                    var oldestUnpaidDate = await api.GetOldestUnpaidInvoiceDate(salesInvoice.FK_CustomerId);
                    if (oldestUnpaidDate.HasValue)
                    {
                        var daysPending = (DateTime.Now.Date - oldestUnpaidDate.Value.Date).Days;
                        if (daysPending > (int)(party!.CreditDays ?? 0))
                        {
                            var msg = string.Format(Localizer("CreditBlockedMessage"), daysPending, party.CreditDays);
                            snackbar.Add(msg, Severity.Warning);
                            return;
                        }
                    }
                }
                // --- End Credit Days Check ---
                // --- Credit Limit Block Check ---
                if (PermanentSettingsData.BlockInvoiceOnCreditLimitExceeded
                    && string.Equals(salesInvoice.BillType, "Credit", StringComparison.OrdinalIgnoreCase)
                    && salesInvoice.FK_CustomerId > 0
                    && (party?.CreditLimit ?? 0) > 0)
                {
                    var outstandingBalance = await api.GetCustomerOutstandingBalanceAsync(salesInvoice.FK_CustomerId);
                    if (outstandingBalance + salesInvoice.GrandTotal > (party!.CreditLimit ?? 0))
                    {
                        var msg = string.Format(Localizer("CreditLimitBlockedMessage"), (outstandingBalance + salesInvoice.GrandTotal), party.CreditLimit);
                        snackbar.Add(msg, Severity.Error);
                        _isSaving = false;
                        StateHasChanged();
                        return;
                    }
                }
                // --- End Credit Limit Check ---

                salesInvoice.EntryDate = DateTime.Now;
                var tran = salesInvoice.CopyTo<TransactionHdr>();
                tran.HdrNo = salesInvoice.SalesInvoiceNo;
                var result = await api.CreateSalesInvoice(new SalesInvoiceDTO(salesInvoice, entTransactions, AppData.CompanyDetails!, PermanentSettingsData.ZatcaPhase, IsSymplified, PermanentSettingsData.UploadSimplifiedXMLRealtime));

                if (result.IsSuccess)
                {
                    if (PermanentSettingsData.EnableShiftClosing && ShiftService != null)
                    {
                        try
                        {
                            var terminal = "Terminal 1";
                            var user = !string.IsNullOrWhiteSpace(AppData.UserName) ? AppData.UserName : "User";
                            bool isReturn = salesInvoice.GrandTotal < 0 || entTransactions.All(x => x.Quantity < 0);
                            await ShiftService.RecordSaleAsync(salesInvoice.GrandTotal, salesInvoice.BillType ?? "Cash", isReturn, terminal, user);
                        }
                        catch { }
                    }

                    if (result.obj is JsonElement jsonElement)
                    {
                        var element = (JsonElement)result.obj!;
                        salesInvoice = JsonSerializer.Deserialize<SalesInvoiceHdr>(element.GetRawText())!;
                    }
                    else
                    {
                        salesInvoice = result.obj as SalesInvoiceHdr;
                    }
                    if (await Dialog.DeleteDialogMethod(Localizer("PrintDialog")))
                    {
                        await PrintMethod();
                    }
                    DisablePrintButton = false;
                    if(string.IsNullOrEmpty(result.Message))
                    {
                        snackbar.Add(Localizer("SavedMessage"), Severity.Success);
                    }
                    else
                    {
                        snackbar.Add(result.Message, Severity.Success);
                    }
                    await NewClick();
                }
                else
                {
                   await Dialog.DialogMethod(result.Message!);
                }
            }
            else
            {
                var result = await api.UpdateSalesInvoice(new SalesInvoiceDTO(salesInvoice, entTransactions, AppData.CompanyDetails!));
                if (result.IsSuccess)
                {
                    Dialog.DialogMethod(Localizer(result.Message));
                }
                else
                {
                    snackbar.Add(Localizer("UpdateMessage"), Severity.Info);
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
        if (!gridSettings.EnableCancel)
        {
            await Dialog.DialogMethod(Localizer("CancelDisabledInSettings"));
            return;
        }
        if (salesInvoice.SalesInvoiceId == 0 || !entTransactions.Any()) { return; }
        if (await Dialog.DeleteDialogMethod(Localizer("CancelDialog")))
        {
            salesInvoice.IsDeleted = true;
            entTransactions.ForEach(x => x.IsDeleted = true);
            var result = await api.UpdateSalesInvoice(new SalesInvoiceDTO(salesInvoice, entTransactions, null));
            if (result.Id > 0)
            {
                if (!string.IsNullOrEmpty(result.Message))
                    snackbar.Add(Localizer(result.Message), Severity.Success);
                else
                    snackbar.Add(Localizer("CancelledMessage"), Severity.Success);

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
        if (salesInvoice.SalesInvoiceId == 0 || !entTransactions.Any()) { return; }
        if (await Dialog.DeleteDialogMethod())
        {
            var result = await api.DeleteSalesInvoice(salesInvoice.SalesInvoiceId);
            if (string.IsNullOrEmpty(result.Message))
            {
                snackbar.Add(Localizer("DeletedMessage"), Severity.Error);
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
        IsSymplified = true;
        salesInvoice = new();
        sQuotationNo = string.Empty;
        sDeliveryNoteNo = string.Empty;
        _selectedDeliveryNoteIds.Clear();
        if (IsAutoCode)
        {
            await LoadAutoCodeIfEnabled();
        }
        else
        {
            salesInvoice.SalesInvoiceNo = (await api.GetNewSalesInvoiceDocNo(AppData.BranchId)).ToString();
        }
        salesInvoice.SalesInvoiceDate = DateTime.Now.Date;
        salesInvoice.FK_CustomerId = customerList.FirstOrDefault().Id;
        salesInvoice.CustomerName = customerList.FirstOrDefault().Name;
        salesInvoice.FK_SalesManId = salesmanList.FirstOrDefault()!.Id;
        salesInvoice.FK_ProjectId = projectList.FirstOrDefault()!.Id;
        salesInvoice.AdditionalAmount = 0;
        salesInvoice.Freight = 0;
        salesInvoice.AdditionalDiscAmnt = 0;
        salesInvoice.AdditionalDiscPertge = 0;
        salesInvoice.AdditionalDiscountType = 1;
        salesInvoice.IsDeleted = false;
        entTransactions = new();
        party = new Party();
        party.Name = "Cash Customer";
        party.Code = customerList.FirstOrDefault(x => x.Name == party.Name)!.Code;
        AddNewRow();
        if (PermanentSettingsData.ZatcaPhase == 2)
        {
            appBar.SetCount(await api.GetFailedCount());
        }
    }
    async Task PrintHelperMethod(bool IsPreview = false, bool IsPdf = false)
    {
        try
        {
            if (pdto == null || pdto.hdr == null || pdto.dtls.Any() == false)
            {
                await RefreshHeldInvoicesCountAsync();
            pdto = await indexdbHelper.LoadPrintDesignFromIndexedDB("Sales_Invoice");
            }
            var transctions = entTransactions.Where(x => !string.IsNullOrEmpty(x.ItemName)).ToList();
            if (salesInvoice.SalesInvoiceId == 0 || !entTransactions.Any()) { await Dialog.DialogMethod("No transaction found"); return; }
            if (pdto == null || pdto.hdr == null || pdto.dtls.Any() == false) { await Dialog.DialogMethod("No print design found"); return; }
            var hdr = salesInvoice.CopyTo<TransactionHdr>();
            hdr.HdrNo = salesInvoice.SalesInvoiceNo;
            hdr.InvoiceDate = (salesInvoice.SalesInvoiceDate ?? DateTime.Now).ToString("dd-MM-yyyy");
            hdr.QRCode = salesInvoice.InvoiceQRCode;
            hdr.BillType = salesInvoice.BillType;
            PrintParameterDTO dto = new PrintParameterDTO(pdto.hdr, pdto.dtls, hdr, transctions, party, AppData.CompanyDetails!, null);
            if (!IsPdf && !IsPreview && !AppData.IsMaui)
            {
                await CrudHelper.PrintInvoiceMethod(dto, js);
            }
            else if (!IsPdf && !IsPreview && AppData.IsWindows)
            {
                FormFactor.DirectPrinting(dto);
            }
            else
            {
                await CrudHelper.PrintInvoiceMethodLocal(Downloader, dto, Dialog, IsPreview);
            }
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod(ex.Message);
        }
    }
    async Task PrintPrviewMethod() => await PrintHelperMethod(true);
    async Task PrintMethod() => await PrintHelperMethod();
    async Task PDFMethod() => await PrintHelperMethod(false, true);
    async Task SearchMethod()
    {
        var result = await Dialog.TransactionSearchMethod("Sales Invoice");
        if (result is not null)
        {
            var dto = await api.GetSalesInvoiceById(result.Id);
            entTransactions = dto.InvoiceDtls;
            entTransactions.ForEach(x => x.SL = entTransactions.IndexOf(x) + 1);
            AddNewRow();
            salesInvoice = dto.InvoiceHdr;
            // Populate Quotation No if ID exists
            sQuotationNo = salesInvoice.QuotationNo!;

            // Populate Delivery Note field if IDs exist
            if (!string.IsNullOrEmpty(salesInvoice.DeliveryNoteIds))
            {
                var dnIds = salesInvoice.DeliveryNoteIds.Split(',', StringSplitOptions.RemoveEmptyEntries);
                var validDnNos = new List<string>();
                var validDnIds = new HashSet<int>();

                foreach (var idStr in dnIds)
                {
                    if (int.TryParse(idStr, out int dnId))
                    {
                        try
                        {
                            var dn = await api.GetDeliveryNoteById(dnId);
                            if (dn is not null)
                            {
                                validDnNos.Add(dn.DeliveryNoteHdr.DeliveryNoteNo);
                                validDnIds.Add(dnId);
                            }
                        }
                        catch { /* Ignore if DN not found */ }
                    }
                }

                if (validDnNos.Any())
                {
                    sDeliveryNoteNo = string.Join(", ", validDnNos);
                    // Also ensure selectedDeliveryNotes set is updated if needed for the dialog
                    _selectedDeliveryNoteIds = validDnIds; // Using the correct member variable
                }
            }
            else
            {
                sDeliveryNoteNo = string.Empty;
                _selectedDeliveryNoteIds = new HashSet<int>();
            }

            var customer = await api.GetCustomerById(salesInvoice.FK_CustomerId);
            party = customer.CopyTo<Party>();
            DisablePrintButton = false;
            TotalFindMethod();
            if (salesInvoice.AdditionalDiscountType != 2)
            {
                int iType = salesInvoice.AdditionalDiscAmnt > 0 ? 2 : 1;
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

    async Task LoadAutoCodeIfEnabled()
    {
        if (IsAutoCode && salesInvoice.SalesInvoiceId == 0)
        {
            salesInvoice.SalesInvoiceNo = await api.GetNextSalesInvoiceCode();
        }
    }

    async Task LoadInvoice(int id)
    {
        var dto = await api.GetSalesInvoiceById(id);
        entTransactions = dto.InvoiceDtls;
        AddNewRow();
        salesInvoice = dto.InvoiceHdr;

        // Populate Delivery Note field if IDs exist
        if (!string.IsNullOrEmpty(salesInvoice.DeliveryNoteIds))
        {
            var dnIds = salesInvoice.DeliveryNoteIds.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var validDnNos = new List<string>();
            var validDnIds = new HashSet<int>();

            foreach (var idStr in dnIds)
            {
                if (int.TryParse(idStr, out int dnId))
                {
                    try
                    {
                        var dn = await api.GetDeliveryNoteById(dnId);
                        if (dn is not null)
                        {
                            validDnNos.Add(dn.DeliveryNoteHdr.DeliveryNoteNo);
                            validDnIds.Add(dnId);
                        }
                    }
                    catch { /* Ignore if DN not found */ }
                }
            }

            if (validDnNos.Any())
            {
                sDeliveryNoteNo = string.Join(", ", validDnNos);
                _selectedDeliveryNoteIds = validDnIds; // Using the correct member variable
            }
        }
        else
        {
            sDeliveryNoteNo = string.Empty;
            _selectedDeliveryNoteIds = new HashSet<int>();
        }

        var customer = await api.GetCustomerById(salesInvoice.FK_CustomerId);
        party = customer.CopyTo<Party>();
        DisablePrintButton = false;

        TotalFindMethod();

        // Logic aligned with SalesQuotationView
        if (salesInvoice.AdditionalDiscountType != 2)
        {
            int iType = salesInvoice.AdditionalDiscAmnt > 0 ? 2 : 1;
            await DiscountMethod(new FocusEventArgs(), iType);
        }
        StateHasChanged();
    }
    #endregion

    #region Transaction Grid Settings & Calculation Helpers
    private TransactionGridSettings gridSettings => PermanentSettingsData.GetTransactionSettings("SalesInvoice");

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
        TransactionHelper.GetTransactionAfterEdit(row,false);
        TotalFindMethod();
        StateHasChanged();
    }

    private void OnRowIncRateChanged(TransactionDtls row)
    {
        if (row == null || row.FK_ItemId == 0) return;
        UpdateExRate(row);
        TransactionHelper.GetTransactionAfterEdit(row,true);
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

    #region Hold and Retrieve Actions
    private async Task HoldBillClick()
    {
        var validItems = entTransactions.Where(x => !string.IsNullOrEmpty(x.ItemName) || x.FK_ItemId > 0).ToList();
        if (!validItems.Any())
        {
            snackbar.Add(Localizer("NoItemsToHold"), Severity.Warning);
            return;
        }

        try
        {
            salesInvoice.FK_BranchId = salesInvoice.FK_BranchId > 0 ? salesInvoice.FK_BranchId : AppData.BranchId;
            salesInvoice.FK_UserId = salesInvoice.FK_UserId > 0 ? salesInvoice.FK_UserId : (AppData.UserId ?? 0);
            salesInvoice.FK_FinancialYearId = salesInvoice.FK_FinancialYearId > 0 ? salesInvoice.FK_FinancialYearId : (AppData.FYearId ?? 0);
            if (salesInvoice.FK_CustomerId == 0)
            {
                var selectedCustomer = customerList.FirstOrDefault(x => x.Name == salesInvoice.CustomerName)
                                       ?? customerList.FirstOrDefault(x => x.Name == "Cash Customer")
                                       ?? customerList.FirstOrDefault();
                if (selectedCustomer != null)
                {
                    salesInvoice.FK_CustomerId = selectedCustomer.Id;
                }
            }

            var holdHdr = salesInvoice.CopyTo<SalesInvoiceHoldHdr>();
            holdHdr.SalesInvoiceHoldHdrId = 0;
            if (string.IsNullOrWhiteSpace(holdHdr.SalesInvoiceNo))
                holdHdr.SalesInvoiceNo = "HOLD-" + DateTime.Now.ToString("yyMMddHHmmss");
            holdHdr.RefNo = AppData.Terminal;

            var holdDto = new SalesInvoiceHoldDTO(holdHdr, validItems);
            var res = await api.CreateSalesInvoiceHold(holdDto);
            if (res.Id > 0)
            {
                snackbar.Add(Localizer("TransactionPlacedOnHold"), Severity.Info);
                await NewClick();
                await RefreshHeldInvoicesCountAsync();
            }
            else
            {
                snackbar.Add($"Failed to hold transaction: {res.Message}", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            snackbar.Add($"Error holding transaction: {ex.Message}", Severity.Error);
        }
    }

    private async Task RetrieveBillClick()
    {
        try
        {
            var branchId = salesInvoice.FK_BranchId > 0 ? salesInvoice.FK_BranchId : AppData.BranchId;
            var heldList = await api.GetAllSalesInvoiceHold(branchId, AppData.Terminal);
            if (heldList == null || !heldList.Any())
            {
                snackbar.Add(Localizer("NoHeldTransactions"), Severity.Warning);
                return;
            }

            int selectedHoldId = 0;
            if (heldList.Count == 1)
            {
                selectedHoldId = heldList.First().SalesInvoiceHoldHdrId;
            }
            else
            {
                var parameters = new DialogParameters { ["BranchId"] = branchId, ["Terminal"] = AppData.Terminal };
                var dialog = await DialogService.ShowAsync<SalesInvoiceHoldSearchDialog>(Localizer("HeldTransactions"), parameters, new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true });
                var result = await dialog.Result;
                if (!result.Canceled && result.Data is int holdId)
                {
                    selectedHoldId = holdId;
                }
                else
                {
                    return;
                }
            }

            if (selectedHoldId > 0)
            {
                var heldDto = await api.GetSalesInvoiceHoldById(selectedHoldId);
                if (heldDto != null && heldDto.HoldHdr != null)
                {
                    salesInvoice = heldDto.HoldHdr.CopyTo<SalesInvoiceHdr>();
                    salesInvoice.SalesInvoiceId = 0;
                    entTransactions = (heldDto.HoldDtls ?? new List<TransactionDtls>()).Select(x => x.CopyTo<TransactionDtls>()).ToList();
                    entTransactions.ForEach(x => x.SL = entTransactions.IndexOf(x) + 1);
                    AddNewRow();

        sQuotationNo = salesInvoice.QuotationNo ?? string.Empty;
        if (!string.IsNullOrEmpty(salesInvoice.DeliveryNoteIds))
        {
            var dnIds = salesInvoice.DeliveryNoteIds.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var validDnNos = new List<string>();
            var validDnIds = new HashSet<int>();
            foreach (var idStr in dnIds)
            {
                if (int.TryParse(idStr, out int dnId))
                {
                    try
                    {
                        var dn = await api.GetDeliveryNoteById(dnId);
                        if (dn is not null)
                        {
                            validDnNos.Add(dn.DeliveryNoteHdr.DeliveryNoteNo);
                            validDnIds.Add(dnId);
                        }
                    }
                    catch { }
                }
            }
            if (validDnNos.Any())
            {
                sDeliveryNoteNo = string.Join(", ", validDnNos);
                _selectedDeliveryNoteIds = validDnIds;
            }
            else
            {
                sDeliveryNoteNo = string.Empty;
                _selectedDeliveryNoteIds = new HashSet<int>();
            }
        }
        else
        {
            sDeliveryNoteNo = string.Empty;
            _selectedDeliveryNoteIds = new HashSet<int>();
        }

        if (salesInvoice.FK_CustomerId > 0)
        {
            try
            {
                var customer = await api.GetCustomerById(salesInvoice.FK_CustomerId);
                if (customer is not null)
                {
                    party = customer.CopyTo<Party>();
                }
            }
            catch { }
        }

        TotalFindMethod();
        if (salesInvoice.AdditionalDiscountType != 2)
        {
            int iType = salesInvoice.AdditionalDiscAmnt > 0 ? 2 : 1;
            await DiscountMethod(new FocusEventArgs(), iType);
        }

        await api.DeleteSalesInvoiceHold(selectedHoldId);
                    DisablePrintButton = false;
                    snackbar.Add(Localizer("HeldRetrievedSuccess"), Severity.Success);
                    await FocusMethod();
                }
            }
        }
        catch (Exception ex)
        {
            snackbar.Add($"Error retrieving held transaction: {ex.Message}", Severity.Error);
        }
        finally
        {
            await RefreshHeldInvoicesCountAsync();
        }
    }

    private async Task RefreshHeldInvoicesCountAsync()
    {
        try
        {
            var branchId = salesInvoice.FK_BranchId > 0 ? salesInvoice.FK_BranchId : AppData.BranchId;
            var heldList = await api.GetAllSalesInvoiceHold(branchId, AppData.Terminal);
            heldInvoicesCount = heldList?.Count ?? 0;
            await InvokeAsync(StateHasChanged);
        }
        catch
        {
            heldInvoicesCount = 0;
        }
    }
    #endregion
}
