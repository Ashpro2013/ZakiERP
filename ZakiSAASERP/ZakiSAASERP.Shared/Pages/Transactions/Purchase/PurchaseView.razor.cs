using ZakiSAASERP.Models.Transactions.Purchase;
using ZakiSAASERP.Shared.Pages.Masters;
using ZakiSAASERP.Shared.SubWindows;
using ZakiSAASERP.Models.DTO;
using Microsoft.AspNetCore.Components.Web;

namespace ZakiSAASERP.Shared.Pages.Transactions.Purchase;

public partial class PurchaseView : ComponentBase, ITabAware
{

    #region Variables
    [Inject] IBrowserViewportService ResizeService { get; set; }
    [Inject] ISnackbar Snackbar { get; set; }
    [Inject] IJSRuntime JSRuntime { get; set; }

    List<TransactionDtls> entTransactions = new();
    private WebUserPrivilege? privilege;
    private string _searchString;
    PurchaseHdr purchase = new();
    TransactionDtls transaction = new();
    List<ExMasters> products = new();
    List<CommonModel> supplierList = new();
    List<CommonModel> unitlist = new();
    List<CommonModel> itemUnitlist = new();
    List<CommonModel> purchaserList = new();
    List<CommonModel> projectList = new();
    List<VatMaster> vatList = new();
    decimal defaultVatPerc = 0;

    bool IsEditing = false;
    private bool IsAutoCode => PermanentSettingsData.AutoPurchaseNo;
    private bool IsUnitSelectionDisabled => PermanentSettingsData.ItemSearchMode == 2;

    MudTextField<string> txtPurchaseNo;
    MudDatePicker txtPurchaseDate;
    MudTextField<string> txtRefNo;
    MudTextField<string> txtInvoiceNo;
    MudTextField<string> txtRemarks;
    MudNumericField<decimal?>? txtAdditional;
    MudNumericField<decimal?>? txtAdditionalDisc;
    MudSelect<int?> cmbProject;
    MudSelect<int?> cmbPurchaser;
    MudNumericField<decimal> txtQuantity;
    MudTextField<string> textField;
    MudTextField<string> txtBarcode;
    MudTextField<string> txtSupplierName;
    MudTextField<string> txtVatNo;
    MudSelect<string> cmbBillType;

    MudNumericField<decimal> txtRate;
    MudNumericField<decimal> txtDiscount;
    private int CashSupplierId = 0;
    private bool IsCashSupplier => purchase.FK_SupplierId == CashSupplierId && CashSupplierId > 0;
    MudNumericField<decimal> txtDiscPerc;
    MudNumericField<decimal> txtTax;
    MudNumericField<decimal> txtNet;
    private int _activeTabIndex = 0;
    string strQuantityFormat => PermanentSettingsData.QuantityFormat;
    string strCurencyFormat => PermanentSettingsData.CurrencyFormat;
    TransactionGridSettings gridSettings => PermanentSettingsData.PurchaseSettings;
    public int Width { get; set; }


    private const int MOBILE_BREAKPOINT = 600;
    private const decimal DEFAULT_QUANTITY = 1;

    private const decimal DEFAULT_QTY_PER_UNIT = 1;
    private const decimal DEFAULT_DISCOUNT_PERC = 0;
    #endregion

    #region Properties
    [Parameter] public int TabId { get; set; }
    [Parameter] public int? Id { get; set; }
    #endregion

    List<StoreMaster> storeList = new();

    #region Initialize
    protected async override Task OnInitializedAsync()
    {
        try
        {
            var size = await ResizeService.GetCurrentBrowserWindowSizeAsync();
            Width = size.Width;
            if (AppData.privileges is not null)
            {
                privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "PurchaseTitle");
            }
            supplierList = await api.GetCombofill("Suppliers", AppData.BranchId);

            unitlist = await api.GetCombofill("Unit", 1);
            purchaserList = await api.GetCombofill("Purchaser", 1);
            projectList = await api.GetCombofill("Project", 1);
            vatList = await api.GetAllVat();
            storeList = await api.GetAllStores();

            itemUnitlist = unitlist;
            products = await api.GetMasters("product", 1);

            var defaultVat = vatList.FirstOrDefault(v => v.IsDefault);
            defaultVatPerc = defaultVat?.Percentage ?? 0;

            if (Width > 600)
            {
                AddNewRow();
            }

            // Find Cash Supplier Id
            var cashSupplier = supplierList.FirstOrDefault(s => s.Code == "S1000");
            if (cashSupplier is not null) CashSupplierId = cashSupplier.Id;

            // Start new entry
            if (Id is not null && Id > 0)
            {
                await LoadPurchase(Id.Value);
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

    #region New/Cancel/Save
    async Task NewClick()
    {
        try
        {
            entTransactions.Clear();
            purchase = new PurchaseHdr
            {
                PurchaseDate = DateTime.Now.Date,
                FK_BranchId = AppData.BranchId,
                FK_UserId = AppData.UserId ?? 0,
                FK_FinancialYearId = AppData.FYearId ?? 0,
                BillType = "Cash",
                AdditionalDiscountType = 1
            };

            // Load auto code if enabled
            await LoadAutoCodeIfEnabled();

            // Default to Cash Supplier (S1000)
            var cashSupplier = supplierList.FirstOrDefault(s => s.Code == "S1000");
            if (cashSupplier is not null)
            {
                CashSupplierId = cashSupplier.Id;
                purchase.FK_SupplierId = cashSupplier.Id;
                purchase.SupplierName = cashSupplier.Name;
                await AutoFillFromSupplier(cashSupplier.Id);
            }

            // Default Project
            if (projectList is not null && projectList.Any())
            {
                purchase.FK_ProjectId = projectList.First().Id;
            }

            IsEditing = false;
            _activeTabIndex = 0;
            DisablePrintButton = true;
            party = new Party();
            if (cashSupplier is not null)
            {
                // Init party with default supplier
                party.Name = cashSupplier.Name;
                party.Address = ""; // Fill if available
            }

            transaction = new TransactionDtls();
            SetDefaultStore(transaction);

            if (Width > MOBILE_BREAKPOINT)
            {
                AddNewRow();
                await Task.Delay(100);
                if (textField is not null)
                    await textField.FocusAsync();
            }
        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod(ex.Message);
        }
    }
    async Task Cancel()
    {
        var existingId = purchase.PurchaseId;
        if (existingId > 0)
        {
            if (privilege is not null && !privilege.DeleteData)
            {
                await Dialog.DialogMethod(Localizer("No...Privilege"));
                return;
            }
            try
            {
                if (await Dialog.DeleteDialogMethod(Localizer("CancelDialog")))
                {
                    var result = await api.DeletePurchase(existingId);
                    if (result.Id > 0)
                    {
                        Snackbar.Add(Localizer("DeletedSuccessfully"), Severity.Success);
                        await NewClick();
                    }
                    else
                    {
                        Snackbar.Add(result.Message, Severity.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add(ex.Message, Severity.Error);
            }
        }
    }


    private bool _isSaving = false;

    async Task SaveOrUpdate()
    {
        if (_isSaving) return;
        if (_isSaving) return;
        if (privilege is not null && purchase.PurchaseId == 0 && !privilege.SaveData)
        {
            await Dialog.DialogMethod(Localizer("No...Privilege"));
            return;
        }
        if (privilege is not null && purchase.PurchaseId != 0 && !privilege.EditData)
        {
            await Dialog.DialogMethod(Localizer("No...Privilege"));
            return;
        }
        _isSaving = true;
        StateHasChanged();

        try
        {
            if (!ValidateHeaderDetails())
                return;

            // Filter out rows with no selected item
            var validTransactions = entTransactions.Where(x => !string.IsNullOrEmpty(x.ItemName) && x.FK_ItemId > 0).ToList();

            if (validTransactions == null || validTransactions.Count == 0)
            {
                Snackbar.Add(Localizer("PleaseAddAtLeastOneItem"), Severity.Error);
                return;
            }

            var dto = new PurchaseDTO(purchase, validTransactions);
            OutputResponse result;

            bool isNew = purchase.PurchaseId == 0;
            if (isNew)
            {
                result = await api.CreatePurchase(dto);
            }
            else
            {
                result = await api.UpdatePurchase(dto);
            }

            if (result.Id > 0)
            {
                if (PermanentSettingsData.EnableShiftClosing && ShiftService != null && isNew)
                {
                    try
                    {
                        var terminal = "Terminal 1";
                        var user = !string.IsNullOrWhiteSpace(AppData.UserName) ? AppData.UserName : "User";
                        await ShiftService.RecordPurchaseAsync(purchase.GrandTotal, purchase.BillType, false, terminal, user);
                    }
                    catch { }
                }

                Snackbar.Add(Localizer(isNew ? "SavedSuccessfully" : "UpdatedSuccessfully"), Severity.Success);

                // Clear form and prepare for next entry
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
                    Snackbar.Add(result.Message ?? Localizer("ErrorOccurred"), Severity.Error);
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

    bool ValidateHeaderDetails()
    {
        if (string.IsNullOrEmpty(purchase.SupplierName))
        {
            Snackbar.Add(Localizer("SupplierRequired"), Severity.Error);
            return false;
        }

        if (purchase.PurchaseDate == null)
        {
            Snackbar.Add(Localizer("PurchaseDateRequired"), Severity.Error);
            return false;
        }

        if (string.IsNullOrEmpty(purchase.PurchaseNo))
        {
            Snackbar.Add(Localizer("PurchaseNoRequired"), Severity.Error);
            return false;
        }

        if (string.IsNullOrEmpty(purchase.InvoiceNo))
        {
            Snackbar.Add(Localizer("SupplierInvoiceNoRequired"), Severity.Error);
            return false;
        }

        if (!string.IsNullOrEmpty(purchase.SupplierVatNo) && purchase.SupplierVatNo.Length != 15)
        {
            Snackbar.Add(Localizer("VatNoMustBe15Digits"), Severity.Error);
            return false;
        }

        return true;
    }
    #endregion

    #region Supplier Selection
    async Task HandleSupplierSearchKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            if (!string.IsNullOrWhiteSpace(purchase.SupplierName))
            {
                // If supplier name is already filled, just move focus
                StateHasChanged();
            }
            else
            {
                await SupplierSearchMethod();
            }
        }
    }

    async Task SupplierSearchMethod()
    {
        var selectedSupplier = await Dialog.GetSupplierSearchMethod("");
        if (selectedSupplier is not null)
        {
            purchase.FK_SupplierId = selectedSupplier.SupplierId;
            purchase.SupplierName = selectedSupplier.Name;
            await AutoFillFromSupplier(selectedSupplier.SupplierId);
            StateHasChanged();
        }
    }

    async Task AutoFillFromSupplier(int supplierId)
    {
        if (supplierId == 0) // Cash Supplier
        {
            purchase.BillType = "Cash";
            purchase.SupplierVatNo = "";
            return;
        }

        try
        {
            var supplier = await api.GetSupplierById(supplierId);
            if (supplier is not null)
            {
                purchase.SupplierVatNo = supplier.VatNo;

                // Auto-determine BillType based on supplier settings
                if (!string.IsNullOrEmpty(supplier.SuppBillType))
                {
                    purchase.BillType = supplier.SuppBillType;
                }
                else if (supplier.CreditDays > 0 || supplier.CreditLimit > 0)
                {
                    purchase.BillType = "Credit";
                }
                else
                {
                    purchase.BillType = "Cash";
                }
            }
        }
        catch (Exception ex)
        {
            // Supplier details not critical for save
            Console.WriteLine($"Error loading supplier details: {ex.Message}");
        }
    }

    async Task CreateSupplierMethod()
    {
        var result = await Dialog.MasterCreationDialogMethod<SupplierMasterView>();
        if (!result!.Canceled && result.Data is not null)
        {
            supplierList.Add(AppData.PassingValue);
            purchase.FK_SupplierId = AppData.PassingValue.Id;
            purchase.SupplierName = AppData.PassingValue.Name;

            // Load supplier details
            await AutoFillFromSupplier(AppData.PassingValue.Id);
            StateHasChanged();
        }
    }

    async Task OnBillTypeChanged(string billType)
    {
        purchase.BillType = billType;
        StateHasChanged();
    }

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
    }

    private async Task DownFocusMethod(int fieldId)
    {
        if (fieldId == 5) // Additional Disc Percentage
        {
            if (txtAdditionalDisc is not null) await txtAdditionalDisc.FocusAsync();
        }
        else if (fieldId == 6) // Additional Disc Amount
        {
            if (cmbProject is not null) await cmbProject.FocusAsync();
        }
    }
    #endregion

    #region Item Grid
    void AddNewRow()
    {
        var lastSL = entTransactions.Any() ? entTransactions.Max(t => t.SL) : 0;

        // Find default store
        // Fix: Only select stores that are visible in the dropdown (Current Branch or Global)
        var allowedStores = storeList.Where(x => x.FK_BranchId == AppData.BranchId || x.FK_BranchId == null).ToList();

        var defaultStore = allowedStores.FirstOrDefault(x => x.IsDefaultStore)?.StoreId
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

        var newTransaction = new TransactionDtls
        {
            SL = lastSL + 1,
            Quantity = DEFAULT_QUANTITY,
            QtyPerUnit = DEFAULT_QTY_PER_UNIT,
            DiscountPerc = DEFAULT_DISCOUNT_PERC,
            TaxPerc = defaultVatPerc,
            AvailableUnits = new List<CommonModel>(),
            FK_StoreId = defaultStore // Set Default Store
        };
        entTransactions.Add(newTransaction);
        StateHasChanged();
    }

    void DltTransaction(TransactionDtls context)
    {
        entTransactions.Remove(context);

        // If no rows left, add an empty row
        if (!entTransactions.Any())
        {
            AddNewRow();
        }

        TotalFindMethod();
        StateHasChanged();
    }

    void RowClickEvent(TableRowClickEventArgs<TransactionDtls> args)
    {
        // Handle row click if needed
    }

    async Task BarcodeChanged(string barcode, TransactionDtls context)
    {
        var selectedProduct = await Dialog.GetItemSearchMethod(barcode, IsSales: false);
        if (selectedProduct is not null)
        {
            IsEditing = true;
            context.FK_ItemId = selectedProduct.FK_ItemId;
            context.ItemName = selectedProduct.MultiUnitName;
            context.Barcode = selectedProduct.Barcode;
            context.FK_UnitId = selectedProduct.FK_UnitId;
            context.UnitName = unitlist.FirstOrDefault(x => x.Id == selectedProduct.FK_UnitId)?.Name;
            context.QtyPerUnit = selectedProduct.QTYPerUnit ?? 0;
            context.ExRate = (selectedProduct.Rate > 0 ? selectedProduct.Rate : selectedProduct.Cost) ?? 0;
            context.InRate = selectedProduct.IncRate ?? context.ExRate;
            context.TaxPerc = await api.GetValueMethod("vatFromItemId", selectedProduct.FK_ItemId);
            context.DiscountPerc = 0;

            // Populate AvailableUnits for this specific row
            context.AvailableUnits = selectedProduct.AvailableUnits ?? new List<CommonModel>();

            context = TransactionHelper.GetTransactionAfterEdit(context);
            TotalFindMethod();
            StateHasChanged();

            if (txtQuantity is not null)
            {
                await txtQuantity.FocusAsync();
                await txtQuantity.SelectAsync();
            }
        }
    }

    async Task OnBarcodeBlurEvent(FocusEventArgs e, TransactionDtls context)
    {
        if (!string.IsNullOrWhiteSpace(context.Barcode) && !IsEditing)
        {
            var multiUnit = await api.GetItemMultiUnit(context.FK_ItemId, context.FK_UnitId, context.Barcode);
            if (multiUnit is not null && multiUnit.FK_ItemId > 0)
            {
                // Item found, populate directly without dialog
                var allUnits = await api.GetCombofill("munit", multiUnit.FK_ItemId);
                context.AvailableUnits = allUnits ?? new List<CommonModel>();
                context.FK_ItemId = multiUnit.FK_ItemId;
                context.ItemName = multiUnit.MultiUnitName;
                context.Barcode = multiUnit.Barcode;
                context.FK_UnitId = multiUnit.FK_UnitId;
                context.UnitName = unitlist.FirstOrDefault(x => x.Id == context.FK_UnitId)?.Name;
                context.QtyPerUnit = (multiUnit.QTYPerUnit ?? 0) > 0 ? multiUnit.QTYPerUnit.Value : 1;
                context.ExRate = (multiUnit.Rate > 0 ? multiUnit.Rate : multiUnit.Cost) ?? 0;
                context.InRate = multiUnit.IncRate ?? context.ExRate;
                context.TaxPerc = await api.GetValueMethod("vatFromItemId", multiUnit.FK_ItemId);
                context.DiscountPerc = 0;
                context.Quantity = 1;

                context = TransactionHelper.GetTransactionAfterEdit(context);
                TotalFindMethod();
                StateHasChanged();
            }
            else
            {
                await BarcodeChanged(context.Barcode, context);
            }
        }
    }

    async Task OnChangeUnit(int unitId, TransactionDtls context)
    {
        if (context.FK_ItemId == 0)
            return;

        try
        {
            var itemMultiUnit = await api.GetItemMultiUnit(context.FK_ItemId, unitId, "");
            if (itemMultiUnit is not null)
            {
                context.FK_UnitId = unitId;
                context.UnitName = itemMultiUnit.Unit?.Name;
                context.QtyPerUnit = itemMultiUnit.QTYPerUnit ?? 1;
                context.ExRate = itemMultiUnit.Cost ?? 0;
                context.InRate = itemMultiUnit.IncRate ?? 0;
                RecalculateLineTotals(context);
                TotalFindMethod();
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add(ex.Message, Severity.Error);
        }
    }
    #endregion

    #region Search
    async Task HandlePurchaseNoKeyDown(KeyboardEventArgs e)
    {
        if (e.Code == "Enter" || e.Code == "NumpadEnter")
        {
            await txtPurchaseDate.FocusAsync();
        }
    }

    async Task SearchClickAsync()
    {
        var result = await Dialog.TransactionSearchMethod("Purchase");
        if (result is not null && result.Id > 0)
        {
            await LoadPurchase(result.Id);
        }
    }

    async Task LoadPurchase(int id)
    {
        try
        {
            var dto = await api.GetPurchaseById(id);
            if (dto is not null)
            {
                purchase = dto.Hdr;
                entTransactions = dto.Dtls;
                entTransactions.ForEach(x => x.SL = entTransactions.IndexOf(x) + 1);
                AddNewRow();
                DisablePrintButton = false;

                IsEditing = true;

                foreach (var item in entTransactions)
                {
                    RecalculateLineTotals(item);
                }

                TotalFindMethod();
                if (purchase.AdditionalDiscountType != 2)
                {
                    int iType = purchase.AdditionalDiscAmnt > 0 ? 2 : 1;
                    await DiscountMethod(new FocusEventArgs(), iType);
                }
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add(ex.Message, Severity.Error);
        }
    }
    #endregion

    #region Calculations
    async Task OnLineQuantityChanged(FocusEventArgs e, TransactionDtls context)
    {
        RecalculateLineTotals(context);
        TotalFindMethod();
    }

    async Task OnTaxBlur(FocusEventArgs e, TransactionDtls context)
    {
        RecalculateLineTotals(context);
        TotalFindMethod();

        // Add new row if this is the last row and has valid item
        if (entTransactions.LastOrDefault() == context && !string.IsNullOrEmpty(context.ItemName))
        {
            AddNewRow();
            StateHasChanged();
            await Task.Delay(100);
            if (textField is not null)
            {
                await textField.FocusAsync();
            }
        }
    }

    void OnDiscountPercChanged(TransactionDtls context)
    {
        RecalculateLineTotals(context);
        TotalFindMethod();
    }

    void OnDiscountAmountChanged(TransactionDtls context)
    {
        if (context.GrossAmount > 0)
        {
            context.DiscountPerc = (context.DiscountAmnt / context.GrossAmount) * 100;
        }
        RecalculateLineTotals(context);
        TotalFindMethod();
    }

    void OnVatPercentageChanged(TransactionDtls context, decimal vatPerc)
    {
        context.TaxPerc = vatPerc;
        RecalculateLineTotals(context);
        TotalFindMethod();
    }

    void RecalculateLineTotals(TransactionDtls line)
    {
        // Gross = Quantity * Rate
        line.GrossAmount = line.Quantity * line.ExRate;

        // Discount Amount
        line.DiscountAmnt = (line.GrossAmount * line.DiscountPerc) / 100;

        // Taxable = Gross - Discount
        line.TaxableAmnt = line.GrossAmount - line.DiscountAmnt;

        // Tax Amount
        line.TaxAmount = (line.TaxableAmnt * line.TaxPerc) / 100;

        // Line Total = Taxable + Tax
        line.LineTotal = line.TaxableAmnt + line.TaxAmount;
    }

    void TotalFindMethod()
    {
        purchase.GrossAmount = entTransactions.Sum(t => t.GrossAmount);
        purchase.LineDsicount = entTransactions.Sum(t => t.DiscountAmnt);
        purchase.TaxableAmount = entTransactions.Sum(t => t.TaxableAmnt);
        purchase.TaxTotal = entTransactions.Sum(t => t.TaxAmount);
        purchase.NetAmount = entTransactions.Sum(t => t.LineTotal);
        purchase.GrandTotal = purchase.NetAmount;
    }

    async Task DiscountMethod(FocusEventArgs e, int iType)
    {
        if (purchase.AdditionalDiscountType == 2)
        {
            var result = await TransactionHelper.AdditionalDiscountMethod(entTransactions, iType, purchase.AdditionalDiscPertge, purchase.AdditionalDiscAmnt);
            purchase.AdditionalDiscAmnt = result.Item2 ?? 0;
            TotalFindMethod();

        }
        else if (purchase.AdditionalDiscountType == 1)
        {
            var result = await TransactionHelper.AddDiscType2Method(entTransactions, iType, purchase.AdditionalDiscPertge, purchase.AdditionalDiscAmnt, purchase.GrandTotal);
            purchase.AdditionalDiscPertge = result.Item1 ?? 0;
            purchase.AdditionalDiscAmnt = result.Item2 ?? 0;
            TotalFindMethod();
        }
        else
        {
            decimal baseTotal = purchase.NetAmount + (purchase.AdditionalAmount ?? 0);
            if ((purchase.AdditionalDiscAmnt ?? 0) == 0 && (purchase.AdditionalDiscPertge ?? 0) == 0)
            {
                purchase.GrandTotal = Math.Round(baseTotal);
                purchase.AdditionalDiscAmnt = baseTotal - purchase.GrandTotal;
                purchase.AdditionalDiscPertge = (purchase.GrandTotal == 0) ? 0 : Math.Round((100 * (purchase.AdditionalDiscAmnt ?? 0)) / purchase.GrandTotal, 2);
            }
            else
            {
                var result = TransactionHelper.AddDiscType3Method(iType, purchase.AdditionalDiscPertge, purchase.AdditionalDiscAmnt, baseTotal);
                purchase.AdditionalDiscPertge = result.Item1 ?? 0;
                purchase.AdditionalDiscAmnt = result.Item2 ?? 0;
                purchase.GrandTotal = result.Item3 ?? 0;
            }
        }
        StateHasChanged();
    }

    bool DisablePrintButton { get; set; } = true;
    Party party = new();

    async Task PrintHelperMethod(bool IsPreview = false)
    {
        try
        {
            var transctions = entTransactions.Where(x => !string.IsNullOrEmpty(x.ItemName)).ToList();
            if (purchase.PurchaseId == 0 || !entTransactions.Any()) { return; }

            var pdto = await api.GetPrintDesignAsync(new PrintDesignReq(0, "Purchase_Invoice"));
            if (pdto.hdr == null || !pdto.dtls.Any()) { return; }

            var hdr = purchase.CopyTo<TransactionHdr>();
            hdr.HdrNo = purchase.PurchaseNo;
            hdr.InvoiceDate = (purchase.PurchaseDate ?? DateTime.Now).ToString("dd-MM-yyyy");

            party.Name = purchase.SupplierName;
            party.VatNo = purchase.SupplierVatNo;

            PrintParameterDTO dto = new PrintParameterDTO(pdto.hdr, pdto.dtls, hdr, transctions, party, AppData.CompanyDetails!, null);
            await CrudHelper.PrintInvoiceMethodLocal(Downloader, dto, Dialog, IsPreview);
        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod(ex.Message);
        }
    }
    async Task PrintPrviewMethod() => await PrintHelperMethod(true);
    async Task PrintMethod() => await PrintHelperMethod(false);
    #endregion

    #region Mobile
    void SetDefaultStore(TransactionDtls trans)
    {
        if (storeList is not null && storeList.Any())
        {
            var allowedStores = storeList.Where(x => x.FK_BranchId == AppData.BranchId || x.FK_BranchId == null).ToList();
            var defaultStore = allowedStores.FirstOrDefault(x => x.IsDefaultStore)?.StoreId
                               ?? allowedStores.FirstOrDefault()?.StoreId
                               ?? 0;
            trans.FK_StoreId = defaultStore;
        }
    }

    async Task AddLineItem()
    {
        if (string.IsNullOrEmpty(transaction.ItemName)) { return; }

        // If it's a new item (not in list), add it
        if (!entTransactions.Contains(transaction))
        {
            transaction.SL = entTransactions.Any() ? entTransactions.Max(x => x.SL) + 1 : 1;
            entTransactions.Add(transaction);
        }

        // Reset transaction object for next entry
        transaction = new TransactionDtls();
        SetDefaultStore(transaction);

        // Reset available units to default list
        transaction.AvailableUnits = new List<CommonModel>();

        IsEditing = false;
        TotalFindMethod();

        // Focus back to barcode
        if (txtBarcode is not null) await txtBarcode.FocusAsync();

        StateHasChanged();
    }

    private void RowClicked(DataGridRowClickEventArgs<TransactionDtls> args)
    {
        transaction = args.Item;
        IsEditing = true;
        StateHasChanged();
    }
    #endregion

    // ... (Skipping Search/Load Purchase regions) ...

    async Task FocusMethod()
    {
        if (!entTransactions.Any() || !string.IsNullOrEmpty(entTransactions.LastOrDefault()!.ItemName))
        {
            AddNewRow();
            StateHasChanged();
            await Task.Delay(300);
        }
        IsEditing = false;
        if (textField is not null)
        {
            await textField.FocusAsync();
        }
    }

    async Task TaskOnEnterMethod(KeyboardEventArgs e, int index)
    {
        if (e.Key == "Enter")
        {
            switch (index)
            {
                case 2: // Quantity -> Rate
                    if (txtRate is not null) { await txtRate.FocusAsync(); await txtRate.SelectAsync(); }
                    break;
                case 4: // Rate -> Disc %
                    if (txtDiscPerc is not null) { await txtDiscPerc.FocusAsync(); await txtDiscPerc.SelectAsync(); }
                    break;
                case 5: // Disc % -> Disc Amt
                    if (txtDiscount is not null) { await txtDiscount.FocusAsync(); await txtDiscount.SelectAsync(); }
                    break;
                case 6: // Disc Amt -> Tax
                    if (txtTax is not null) { await txtTax.FocusAsync(); await txtTax.SelectAsync(); }
                    break;
                case 7: // Tax -> Net Amount
                    if (txtNet is not null) await txtNet.FocusAsync();
                    break;
                case 8: // TaxAmount -> NetAmount
                    if (txtNet is not null) await txtNet.FocusAsync();
                    break;
                case 9: // Line Total -> New Row
                    await FocusMethod();
                    break;
                default:
                    break;
            }
            IsEditing = true;
            TotalFindMethod();
        }
    }

    async Task OnEnterMethod(KeyboardEventArgs e, TransactionDtls context)
    {
        if (e.Key == "Enter")
        {
            if (string.IsNullOrEmpty(context.Barcode))
            {
                await BarcodeChanged("", context);
            }
            else
            {
                var multiUnit = await api.GetItemMultiUnit(1, 1, context.Barcode);
                if (multiUnit is not null && multiUnit.FK_ItemId > 0)
                {
                    IsEditing = true;
                    var allUnits = await api.GetCombofill("munit", multiUnit.FK_ItemId);
                    context.AvailableUnits = allUnits ?? new List<CommonModel>();
                    context.FK_ItemId = multiUnit.FK_ItemId;
                    context.ItemName = multiUnit.MultiUnitName;
                    context.Barcode = multiUnit.Barcode;
                    context.FK_UnitId = multiUnit.FK_UnitId;
                    context.UnitName = unitlist.FirstOrDefault(x => x.Id == context.FK_UnitId)?.Name;
                    context.QtyPerUnit = (multiUnit.QTYPerUnit ?? 0) > 0 ? multiUnit.QTYPerUnit.Value : 1;
                    context.ExRate = (multiUnit.Rate > 0 ? multiUnit.Rate : multiUnit.Cost) ?? 0;
                    context.InRate = multiUnit.IncRate ?? context.ExRate;
                    context.TaxPerc = await api.GetValueMethod("vatFromItemId", multiUnit.FK_ItemId);
                    context.DiscountPerc = 0;
                    context.Quantity = 1;

                    context = TransactionHelper.GetTransactionAfterEdit(context);
                    TotalFindMethod();

                    if (txtQuantity is not null) await txtQuantity.FocusAsync();
                    StateHasChanged();
                }
                else
                {
                    await BarcodeChanged(context.Barcode, context);
                }
            }
        }
    }

    private async Task LoadAutoCodeIfEnabled()
    {
        try
        {
            if (purchase.PurchaseId == 0)
            {
                if (IsAutoCode)
                {
                    purchase.PurchaseNo = await api.GetNextPurchaseNo();
                }
                else
                {
                    purchase.PurchaseNo = (await api.GetPurchaseNextVchrNo(AppData.BranchId)).ToString();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading Purchase No: {ex.Message}");
            Snackbar.Add($"Error loading Purchase No: {ex.Message}", Severity.Error);
        }
    }

    #region AI Invoice OCR
    async Task OpenAiInvoiceOcrDialog()
    {
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            MaxWidth = MaxWidth.Large,
            FullWidth = true
        };

        var parameters = new DialogParameters
        {
            ["SupplierList"] = supplierList,
            ["Products"] = products,
            ["UnitList"] = unitlist,
            ["StoreList"] = storeList
        };

        var dialog = await DialogService.ShowAsync<PurchaseOcrDialog>("AI Purchase Invoice / Receipt Scanner", parameters, options);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled && result.Data is PurchaseInvoiceOcrResult ocrData)
        {
            await ApplyOcrResultToPurchase(ocrData);
        }
    }

    async Task ApplyOcrResultToPurchase(PurchaseInvoiceOcrResult ocrData)
    {
        try
        {
            // 1. Supplier Auto-fill
            if (ocrData.MatchedSupplierId.HasValue && ocrData.MatchedSupplierId.Value > 0)
            {
                purchase.FK_SupplierId = ocrData.MatchedSupplierId.Value;
                purchase.SupplierName = ocrData.MatchedSupplierName ?? ocrData.SupplierName;
                await AutoFillFromSupplier(ocrData.MatchedSupplierId.Value);
            }
            else if (!string.IsNullOrWhiteSpace(ocrData.SupplierName))
            {
                purchase.SupplierName = ocrData.SupplierName;
                if (CashSupplierId > 0 && (purchase.FK_SupplierId == 0 || purchase.FK_SupplierId == null))
                {
                    purchase.FK_SupplierId = CashSupplierId;
                }
            }

            if (!string.IsNullOrWhiteSpace(ocrData.SupplierVatNo))
            {
                purchase.SupplierVatNo = ocrData.SupplierVatNo;
            }

            // 2. Invoice Meta Auto-fill
            if (!string.IsNullOrWhiteSpace(ocrData.InvoiceNo))
            {
                purchase.InvoiceNo = ocrData.InvoiceNo;
            }

            if (ocrData.InvoiceDate.HasValue)
            {
                purchase.PurchaseDate = ocrData.InvoiceDate.Value;
            }

            if (!string.IsNullOrWhiteSpace(ocrData.BillType))
            {
                purchase.BillType = ocrData.BillType;
            }

            if (!string.IsNullOrWhiteSpace(ocrData.Remarks))
            {
                purchase.Remarks = ocrData.Remarks;
            }

            // 3. Grid Line Items Auto-fill
            // Remove any trailing empty placeholder rows
            entTransactions.RemoveAll(x => x.FK_ItemId == 0 && string.IsNullOrWhiteSpace(x.ItemName));

            int defaultStoreId = storeList.FirstOrDefault(s => s.FK_BranchId == AppData.BranchId)?.StoreId 
                               ?? (storeList.FirstOrDefault()?.StoreId ?? 1);
            int defaultUnitId = unitlist.FirstOrDefault()?.Id ?? 1;
            string defaultUnitName = unitlist.FirstOrDefault()?.Name ?? "PCS";

            foreach (var line in ocrData.LineItems.Where(x => x.IsSelected))
            {
                int unitId = line.MatchedUnitId ?? defaultUnitId;
                string uName = line.MatchedUnitName ?? unitlist.FirstOrDefault(u => u.Id == unitId)?.Name ?? defaultUnitName;

                var newTx = new TransactionDtls
                {
                    SL = entTransactions.Count + 1,
                    FK_ItemId = line.MatchedItemId ?? 0,
                    ItemName = !string.IsNullOrWhiteSpace(line.MatchedItemName) ? line.MatchedItemName : line.ItemName,
                    ItemArabicName = line.ItemArabicName,
                    Barcode = line.MatchedBarcode ?? line.Barcode ?? string.Empty,
                    Quantity = line.Quantity > 0 ? line.Quantity : 1,
                    FK_UnitId = unitId,
                    UnitName = uName,
                    FK_StoreId = line.MatchedStoreId ?? defaultStoreId,
                    ExRate = line.UnitPrice,
                    DiscountPerc = line.DiscountPerc,
                    DiscountAmnt = line.DiscountAmount,
                    TaxPerc = line.TaxPerc > 0 ? line.TaxPerc : defaultVatPerc,
                    QtyPerUnit = 1
                };

                // Fetch multi-units if matched to system product
                if (newTx.FK_ItemId > 0)
                {
                    try
                    {
                        var allUnits = await api.GetCombofill("munit", newTx.FK_ItemId);
                        newTx.AvailableUnits = allUnits ?? new List<CommonModel>();
                    }
                    catch { }
                }

                RecalculateLineTotals(newTx);
                entTransactions.Add(newTx);
            }

            // Always ensure an empty row at the bottom for easy manual addition
            AddNewRow();

            TotalFindMethod();
            IsEditing = true;
            StateHasChanged();

            Snackbar.Add($"Imported {ocrData.LineItems.Count(x => x.IsSelected)} line items from AI OCR successfully!", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Failed to apply OCR data: {ex.Message}", Severity.Error);
        }
    }
    #endregion

}
