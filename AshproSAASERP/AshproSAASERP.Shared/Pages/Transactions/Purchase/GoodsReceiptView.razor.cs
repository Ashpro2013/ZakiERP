using AshproSAASERP.Models.Transactions.Purchase;
using AshproSAASERP.Models.Settings;
using FileDownloader;
using Microsoft.AspNetCore.Components.Web;

namespace AshproSAASERP.Shared.Pages.Transactions.Purchase;

public partial class GoodsReceiptView : ComponentBase, ITabAware
{
    #region Variables
    [Parameter] public int TabId { get; set; }
    [Inject] IBrowserViewportService ResizeService { get; set; }
    [Inject] NavigationManager NavManager { get; set; }
    [Inject] IJSRuntime JSRuntime { get; set; }
    [Inject] public Downloader Downloader { get; set; }

    List<GoodsReceiptDtls> entTransactions = new();
    private string _searchString;
    GoodsReceiptHdr goodsReceipt = new() { Supplier = new SupplierMaster() };
    private WebUserPrivilege? privilege;
    GoodsReceiptDtls transaction = new();
    List<ExMasters> products = new();
    List<CommonModel> purchaserList = new();
    List<StoreMaster> storeList = new();
    List<CommonModel> unitlist = new();
    List<CommonModel> itemUnitlist = new();

    bool IsEditing = false;
    bool DisablePrintButton = false;

    MudDatePicker txtEntryDate;
    MudTextField<string> txtGoodsReceiptNo;
    MudTextField<string> txtRefNo;
    MudSelect<int?> cmbPurchaser;
    MudTextField<string> txtRemarks;
    MudTextField<string> txtBarcode;
    MudTextField<string> textField;
    MudNumericField<decimal> txtQuantity;
    MudNumericField<decimal> txtRate;
    MudNumericField<decimal> txtDiscount;
    MudNumericField<decimal> txtDiscAmt;
    MudNumericField<decimal> txtNet;

    private int _activeTabIndex = 0;
    string strQuantityFormat => PermanentSettingsData.QuantityFormat;
    string strCurencyFormat => PermanentSettingsData.CurrencyFormat;
    public int Width { get; set; }
    PrintDesignDTO pdto;

    private bool IsAutoCode => PermanentSettingsData.AutoGoodsReceiptNo;

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
                privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "GoodsReceipt");
            }

            // Use IndexedDB caching for faster dropdown loading
            purchaserList = await indexdbHelper.LoadComboFromIndexedDB("Purchaser");
            unitlist = await indexdbHelper.LoadComboFromIndexedDB("Unit");
            storeList = await api.GetAllStores();
            itemUnitlist = unitlist;
            products = await api.GetMasters("product", 1);
            pdto = await indexdbHelper.LoadPrintDesignFromIndexedDB("Goods_Receipts");

            if (Width > 600)
            {
                AddNewRow();
            }
            await NewClick();
            goodsReceipt.Supplier = new SupplierMaster();
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod(ex.Message);
        }
    }
    #endregion

    #region Methods

    async Task OpenSupplierSearch()
    {
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            MaxWidth = MaxWidth.Large
        };
        var dialog = await DialogService.ShowAsync<SupplierSearchDialog>(Localizer("SupplierSearch"), options);
        var result = await dialog.Result;
        if (!result.Canceled && result.Data is SupplierMaster selectedSupplier)
        {
            goodsReceipt.FK_SupplierId = selectedSupplier.SupplierId;
            goodsReceipt.Supplier = selectedSupplier; // For display
            StateHasChanged();
        }
    }

    async Task OpenSupplierCreate()
    {
        // User requested URL to open SupplierMaster
        await JSRuntime.InvokeVoidAsync("open", "/suppliermaster", "_blank");
    }

    async Task OpenPOSearch()
    {
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            MaxWidth = MaxWidth.Large
        };
        var dialog = await DialogService.ShowAsync<PurchaseOrderSearchDialog>(Localizer("PurchaseOrderSearch"), options);
        var result = await dialog.Result;
        if (!result.Canceled && result.Data is List<ExMasters> selectedPOs)
        {
            if (selectedPOs.Any())
            {
                goodsReceipt.OurPORefNo = string.Join("-", selectedPOs.Select(x => x.Code));

                // Remove empty row if it exists
                if (entTransactions.Count == 1 && string.IsNullOrEmpty(entTransactions.First().ItemName))
                {
                    entTransactions.Clear();
                }

                foreach (var po in selectedPOs)
                {
                    var poDto = await api.GetPurchaseOrderById(po.Id);
                    if (poDto is not null && poDto.PurchaseOdrDtls is not null)
                    {
                        foreach (var item in poDto.PurchaseOdrDtls)
                        {
                            // Create new item using mapping/copy
                            var newItem = item.CopyTo<GoodsReceiptDtls>();
                            newItem.SL = entTransactions.Count + 1;
                            entTransactions.Add(newItem);
                        }

                    }
                }
                AddNewRow();
                TotalFindMethod();
            }
            StateHasChanged();
        }
    }

    TransactionGridSettings gridSettings => PermanentSettingsData.GoodsReceiptSettings;

    void TotalFindMethod()
    {
        foreach (var item in entTransactions)
        {
            if (item.InRate == 0 && item.ExRate > 0)
            {
                item.InRate = Math.Round(item.ExRate * (1 + (item.TaxPerc / 100m)), 2, MidpointRounding.AwayFromZero);
            }
            item.GrossAmount = item.Quantity * item.ExRate;
            item.DiscountAmnt = (item.GrossAmount * item.DiscountPerc) / 100;
            item.TaxableAmnt = item.GrossAmount - item.DiscountAmnt;
            item.TaxAmount = (item.TaxableAmnt * item.TaxPerc) / 100;
            item.LineTotal = item.TaxableAmnt + item.TaxAmount;
        }

        goodsReceipt.GrossAmount = entTransactions.Sum(x => x.GrossAmount);
        goodsReceipt.LineDsicount = entTransactions.Sum(x => x.DiscountAmnt);
        goodsReceipt.TaxableAmount = entTransactions.Sum(x => x.TaxableAmnt);
        goodsReceipt.TaxTotal = entTransactions.Sum(x => x.TaxAmount);
        goodsReceipt.NetAmount = entTransactions.Sum(x => x.LineTotal);

        goodsReceipt.GrandTotal = (goodsReceipt.NetAmount + (goodsReceipt.AdditionalAmount ?? 0) + (goodsReceipt.Freight ?? 0)) - (goodsReceipt.AdditionalDiscAmnt ?? 0);
    }

    private void OnExRateChanged(GoodsReceiptDtls row, decimal newExRate)
    {
        row.ExRate = newExRate;
        row.InRate = Math.Round(newExRate * (1 + (row.TaxPerc / 100m)), 2, MidpointRounding.AwayFromZero);
        TransactionHelper.GetTransactionAfterEdit(row);
        TotalFindMethod();
        StateHasChanged();
    }

    private void OnInRateChanged(GoodsReceiptDtls row, decimal newInRate)
    {
        row.InRate = newInRate;
        decimal taxMultiplier = 1 + (row.TaxPerc / 100m);
        row.ExRate = taxMultiplier != 0 ? Math.Round(newInRate / taxMultiplier, 4, MidpointRounding.AwayFromZero) : newInRate;
        TransactionHelper.GetTransactionAfterEdit(row);
        TotalFindMethod();
        StateHasChanged();
    }

    private void OnDiscPerChanged(GoodsReceiptDtls row, decimal newDiscPer)
    {
        row.DiscountPerc = newDiscPer;
        TransactionHelper.GetTransactionAfterEdit(row);
        TotalFindMethod();
        StateHasChanged();
    }

    private void OnDiscAmountChanged(GoodsReceiptDtls row, decimal newDiscAmount)
    {
        row.DiscountAmnt = newDiscAmount;
        if (row.GrossAmount > 0)
        {
            row.DiscountPerc = Math.Round((newDiscAmount / row.GrossAmount) * 100m, 4, MidpointRounding.AwayFromZero);
        }
        TransactionHelper.GetTransactionAfterEdit(row);
        TotalFindMethod();
        StateHasChanged();
    }

    async Task Delete()
    {
        if (goodsReceipt.GoodsReceiptId == 0) return;
        if (privilege is not null && !privilege.DeleteData)
        {
            Dialog.DialogMethod(Localizer("No...Privilege"));
            return;
        }
        if (await Dialog.DeleteDialogMethod())
        {
            var result = await api.DeleteGoodsReceipt(goodsReceipt.GoodsReceiptId);
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

    async Task NewClick()
    {
        goodsReceipt = new();
        goodsReceipt.Supplier = new SupplierMaster();
        goodsReceipt.GoodsReceiptDate = DateTime.Now;
        goodsReceipt.EntryDate = DateTime.Now;
        goodsReceipt.FK_PurchaserId = purchaserList.FirstOrDefault()?.Id;
        await LoadAutoCodeIfEnabled();
        entTransactions = new();
        AddNewRow();
        StateHasChanged();
    }

    async Task LoadAutoCodeIfEnabled()
    {
        try
        {
            if (goodsReceipt.GoodsReceiptId == 0)
            {
                if (IsAutoCode)
                {
                    goodsReceipt.GoodsReceiptNo = await api.GetNextGoodsReceiptNo();
                }
                else
                {
                    goodsReceipt.GoodsReceiptNo = await api.GetNewGoodsReceiptDocNo(AppData.BranchId);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading Goods Receipt No: {ex.Message}");
            Dialog.DialogMethod(ex.Message);
        }
    }


    private bool _isSaving = false;

    async Task SaveOrUpdate()
    {
        if (_isSaving) return;
        if (privilege is not null && goodsReceipt.GoodsReceiptId == 0 && !privilege.SaveData)
        {
            Dialog.DialogMethod(Localizer("No...Privilege"));
            return;
        }
        if (privilege is not null && goodsReceipt.GoodsReceiptId != 0 && !privilege.EditData)
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
            goodsReceipt.FK_BranchId = AppData.BranchId;
            goodsReceipt.FK_UserId = AppData.UserId ?? 0;
            goodsReceipt.FK_FinancialYearId = AppData.FYearId ?? 0;

            // Validate supplier
            if (goodsReceipt.FK_SupplierId == 0)
            {
                Dialog.DialogMethod("Please select a Supplier.");
                return;
            }

            if (goodsReceipt.GoodsReceiptId == 0)
            {
                goodsReceipt.EntryDate = DateTime.Now;
                var result = await api.CreateGoodsReceipt(new GoodsReceiptDTO(goodsReceipt, entTransactions));
                if (string.IsNullOrEmpty(result.Message))
                {
                    goodsReceipt.GoodsReceiptId = result.Id;
                    Dialog.DialogMethod(Localizer("SaveMessage"));
                    await NewClick();
                }
                else
                {
                    Dialog.DialogMethod(result.Message);
                }
            }
            else
            {
                var result = await api.UpdateGoodsReceipt(new GoodsReceiptDTO(goodsReceipt, entTransactions));
                if (string.IsNullOrEmpty(result.Message))
                {
                    Dialog.DialogMethod(Localizer("UpdateMessage"));
                    await NewClick();
                }
                else
                {
                    Dialog.DialogMethod(result.Message);
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
        await NewClick();
    }

    async Task PrintHelperMethod(bool IsPreview = false)
    {
        try
        {
            if (goodsReceipt.GoodsReceiptId == 0 || !entTransactions.Any()) { return; }

            var validTransactions = entTransactions.Where(x => !string.IsNullOrEmpty(x.ItemName) && x.FK_ItemId > 0).ToList();
            if (!validTransactions.Any())
            {
                Dialog.DialogMethod(Localizer("NoItemsMessage"));
                return;
            }

            // Use cached print design, fallback to API if not cached
            if (pdto == null || pdto.hdr == null || !pdto.dtls.Any())
            {
                pdto = await api.GetPrintDesignAsync(new PrintDesignReq(0, "Goods_Receipts"));
            }

            if (pdto == null || pdto.hdr == null || !pdto.dtls.Any())
            {
                Dialog.DialogMethod("Print design 'Goods_Receipts' not found.");
                return;
            }

            var hdr = goodsReceipt.CopyTo<TransactionHdr>();
            hdr.HdrNo = goodsReceipt.GoodsReceiptNo;
            hdr.InvoiceDate = (goodsReceipt.GoodsReceiptDate ?? DateTime.Now).ToString("dd-MM-yyyy");

            // Map GoodsReceiptDtls to TransactionDtls
            var printDetails = new List<TransactionDtls>();
            foreach (var item in validTransactions)
            {
                printDetails.Add(item.CopyTo<TransactionDtls>());
            }

            // Construct Party object
            // purchaserList contains CommonModel, we need Party
            Party party = null;
            if (goodsReceipt.FK_SupplierId > 0)
            {
                var sup = await api.GetSupplierById(goodsReceipt.FK_SupplierId);
                if (sup is not null)
                {
                    party = sup.CopyTo<Party>();
                }
            }

            if (party == null)
            {
                party = new Party
                {
                    Name = goodsReceipt.Supplier?.Name ?? "",
                    Code = "",
                    VatNo = goodsReceipt.Supplier?.VatNo ?? ""
                };
            }

            PrintParameterDTO dto = new PrintParameterDTO(pdto.hdr, pdto.dtls, hdr, printDetails, party, AppData.CompanyDetails, null);
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
        var result = await Dialog.TransactionSearchMethod("Goods Receipt");
        if (result is not null)
        {
            var dto = await api.GetGoodsReceiptById(result.Id);
            if (dto is not null && dto.Header is not null)
            {
                goodsReceipt = dto.Header;
                entTransactions = dto.Details;
                entTransactions.ForEach(x => x.SL = entTransactions.IndexOf(x) + 1);
                AddNewRow();
                TotalFindMethod();
                StateHasChanged();
            }
            else
            {
                Dialog.DialogMethod("Failed to load document.");
            }
        }
    }

    private void AddNewRow()
    {
        transaction = new();
        transaction.SL = entTransactions.Count + 1;
        if (unitlist.Any())
        {
            transaction.UnitName = unitlist.FirstOrDefault().Name;
            transaction.FK_UnitId = unitlist.FirstOrDefault().Id;
        }
        entTransactions.Add(transaction);
    }

    #endregion

    #region Grid Events
    async Task OnBarcodBlurEvent(FocusEventArgs arg, GoodsReceiptDtls? row = null)
    {
        var currentItem = row ?? transaction;
        var multiUnit = await api.GetItemMultiUnit(currentItem.FK_ItemId, currentItem.FK_UnitId, currentItem.Barcode ?? "");
        if (multiUnit == null) { return; }

        currentItem.QtyPerUnit = (multiUnit.QTYPerUnit ?? 0) > 0 ? multiUnit.QTYPerUnit.Value : 1;
        currentItem.ExRate = multiUnit.Rate ?? 0;
        currentItem.ItemName = multiUnit.MultiUnitName;
        currentItem.UnitName = unitlist.FirstOrDefault(x => x.Id == multiUnit.FK_UnitId)?.Name;
        currentItem.Barcode = multiUnit.Barcode;
        currentItem.ItemArabicName = multiUnit.MultiUnitArabicName;
        currentItem.FK_ItemId = multiUnit.FK_ItemId;
        currentItem.Quantity = 1;
        currentItem.FK_UnitId = multiUnit.FK_UnitId;
        currentItem.TaxPerc = await api.GetValueMethod("vatFromItemId", multiUnit.FK_ItemId);
        currentItem.DiscountPerc = 0;
        currentItem = TransactionHelper.GetTransactionAfterEdit(currentItem);
        var allUnits = await api.GetCombofill("munit", multiUnit.FK_ItemId);
        unitlist.Clear();
        unitlist = allUnits ?? new List<CommonModel>();

        // Also update itemUnitlist for mobile view
        itemUnitlist = unitlist.ToList();
        TotalFindMethod();
        StateHasChanged();
    }

    async Task BarcodeChanged(string sVal, GoodsReceiptDtls? row = null)
    {
        var currentItem = row ?? transaction;
        var selectedProduct = await Dialog.GetItemSearchMethod(sVal, IsSales: false);

        if (selectedProduct is not null)
        {
            currentItem.FK_ItemId = selectedProduct.FK_ItemId;
            currentItem.ItemName = selectedProduct.ItemName; // Use ItemName from result
            currentItem.ItemArabicName = selectedProduct.ItemNameArabic;
            currentItem.Barcode = selectedProduct.Barcode;
            currentItem.FK_UnitId = selectedProduct.FK_UnitId;
            currentItem.UnitName = selectedProduct.UnitName;
            currentItem.Quantity = 1;
            currentItem.QtyPerUnit = selectedProduct.QtyPerUnit;
            currentItem.ExRate = (selectedProduct.Rate > 0 ? selectedProduct.Rate : selectedProduct.Cost) ?? 0;
            currentItem.InRate = selectedProduct.IncRate ?? currentItem.ExRate;
            currentItem.TaxPerc = await api.GetValueMethod("vatFromItemId", selectedProduct.FK_ItemId);
            currentItem.DiscountPerc = 0;

            // Sync unit lists
            unitlist.Clear();
            unitlist = selectedProduct.AvailableUnits ?? new List<CommonModel>();
            itemUnitlist = unitlist.ToList();

            TotalFindMethod();
            StateHasChanged();
            await Task.Delay(150);
            if (txtQuantity is not null)
            {
                await txtQuantity.FocusAsync();
                await txtQuantity.SelectAsync();
            }
        }
    }

    async Task OnBlurQtEvent(FocusEventArgs arg) { TotalFindMethod(); }

    async Task TaskOnEnterMethod(KeyboardEventArgs e, int index)
    {
        if (e.Key == "Enter")
        {
            if (index == 2 && txtRate is not null) { await txtRate.FocusAsync(); await txtRate.SelectAsync(); }
            else if (index == 4 && txtDiscount is not null) { await txtDiscount.FocusAsync(); await txtDiscount.SelectAsync(); }
            else if (index == 5 && txtDiscAmt is not null) { await txtDiscAmt.FocusAsync(); await txtDiscAmt.SelectAsync(); }
            else if (index == 8) await FocusMethod();

            TotalFindMethod();
            StateHasChanged();
        }
    }

    async Task FocusMethod()
    {
        if (!entTransactions.Any() || entTransactions.LastOrDefault()?.FK_ItemId > 0)
        {
            AddNewRow();
            StateHasChanged();
            await Task.Delay(200);
        }
        if (textField is not null)
        {
            await textField.FocusAsync();
            await textField.SelectAsync();
        }
    }

    async Task OnEnterMethod(KeyboardEventArgs e, GoodsReceiptDtls? row = null)
    {
        if (e.Key == "Enter")
        {
            var currentItem = row ?? transaction;

            if (string.IsNullOrEmpty(currentItem.Barcode))
            {
                await BarcodeChanged("", currentItem);
            }
            else
            {
                // Fast fetch logic
                var multiUnit = await api.GetItemMultiUnit(0, 0, currentItem.Barcode);
                if (multiUnit is not null)
                {
                    currentItem.ItemName = multiUnit.MultiUnitName;
                    currentItem.Barcode = multiUnit.Barcode;
                    currentItem.ItemArabicName = multiUnit.MultiUnitArabicName;
                    currentItem.QtyPerUnit = multiUnit.QTYPerUnit ?? 0;
                    currentItem.FK_UnitId = multiUnit.FK_UnitId;
                    currentItem.FK_ItemId = multiUnit.FK_ItemId;
                    currentItem.UnitName = unitlist.FirstOrDefault(x => x.Id == currentItem.FK_UnitId)?.Name;
                    currentItem.ExRate = multiUnit.Rate ?? 0;
                    currentItem.InRate = multiUnit.IncRate ?? 0;
                    currentItem.TaxPerc = await api.GetValueMethod("vatFromItemId", multiUnit.FK_ItemId);
                    currentItem.Quantity = 1;

                    // Update unit list for this item
                    var units = await api.GetCombofill("munit", currentItem.FK_ItemId);
                    unitlist = units;
                    itemUnitlist = units.ToList();
                }
                else
                {
                    await BarcodeChanged(currentItem.Barcode, currentItem);
                }
            }

            if (txtQuantity is not null)
            {
                await txtQuantity.FocusAsync();
                await txtQuantity.SelectAsync();
            }
            TotalFindMethod();
            StateHasChanged();
        }
    }

    private void RowClickEvent(TableRowClickEventArgs<GoodsReceiptDtls> args)
    {
        transaction = args.Item;
        IsEditing = true;
    }

    async Task DltTransaction(GoodsReceiptDtls item)
    {
        entTransactions.Remove(item);
        TotalFindMethod();
    }

    async Task OnChangeUnit(int iUnitId)
    {
        // Unit change logic
        transaction.FK_UnitId = iUnitId;
        // Fetch new rate/barcode for unit...
    }

    async Task Keydown(KeyboardEventArgs e, int fieldId)
    {
        // Navigation logic for Header fields
    }

    async Task MobileBarcodeChanged(string sVal) { }

    #endregion
}
