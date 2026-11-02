using AshproSAASERP.Models.Transactions.Inventory;
using Microsoft.AspNetCore.Components.Web;

namespace AshproSAASERP.Shared.Pages.Transactions.Inventory;

public partial class DamageStockView : ITabAware
{
    [Parameter] public int TabId { get; set; }
    [Inject] IBrowserViewportService ResizeService { get; set; }

    DamageStockHdr hdr = new();
    List<DamageStockDtls> entTransactions = new();
    private WebUserPrivilege? privilege;

    // Dropdown Lists
    List<CommonModel> accountList = new();
    List<CommonModel> projectList = new();
    List<CommonModel> unitlist = new();
    List<StoreMaster> storeList = new();
    List<CommonModel> itemUnitlist = new(); // For unit dropdowns

    // UI State
    DamageStockDtls transaction = new(); // For mobile editing / current row
    int _activeTabIndex = 0;
    bool IsEditing = false;
    public int Width { get; set; }

    // References
    MudDatePicker txtEntryDate;
    MudSelect<int?> cmbAccount;
    MudTextField<string> txtBarcode;
    MudNumericField<decimal> gridQtyField;
    MudNumericField<decimal> gridRateField;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var size = await ResizeService.GetCurrentBrowserWindowSizeAsync();
            Width = size.Width;

            accountList = await api.GetCombofill("Account", AppData.BranchId);
            projectList = await api.GetCombofill("Project", 1);
            if (AppData.privileges is not null)
            {
                privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "Damage Stock");
            }
            unitlist = await api.GetCombofill("Unit", 1);
            storeList = await api.GetAllStores();
            itemUnitlist = unitlist; // Initialize

            await NewClick();
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod(ex.Message);
        }
    }

    async Task NewClick()
    {
        hdr = new();
        var docNo = await api.GetNewDamageStockDocNo(AppData.BranchId);
        if (string.IsNullOrWhiteSpace(docNo) || docNo.TrimStart().StartsWith("<"))
        {
            docNo = "1001";
        }
        hdr.DamageStockNo = docNo;
        hdr.EntryDate = DateTime.Now;
        hdr.FK_BranchId = AppData.BranchId;
        hdr.FK_UserId = AppData.UserId ?? 0;
        hdr.FK_FinancialYearId = AppData.FYearId ?? 0;
        entTransactions = new();
        // Default Account/Project if any (optional)
        if (accountList.Any()) hdr.FK_AccountId = accountList.First().Id;

        if (Width > 600)
        {
            AddNewRow();
        }
        else
        {
            transaction = InitNewRow();
            // Ensure transaction is NOT in entTransactions
        }
    }

    DamageStockDtls InitNewRow()
    {
        // Find default store
        var allowedStores = storeList.Where(x => x.FK_BranchId == AppData.BranchId || x.FK_BranchId == null).ToList();
        var defaultStoreId = allowedStores.FirstOrDefault(x => x.IsDefaultStore)?.StoreId
                           ?? allowedStores.FirstOrDefault()?.StoreId
                           ?? 0;

        return new DamageStockDtls
        {
            FK_UnitId = unitlist.Any() ? unitlist.First().Id : 0,
            UnitName = unitlist.Any() ? unitlist.First().Name : "",
            FK_StoreId = defaultStoreId,
            Quantity = 0,
            QtyPerUnit = 1,
            Cost = 0,
            Rate = 0,
            Value = 0
        };
    }

    void AddNewRow()
    {
        var newRow = InitNewRow();
        newRow.SL = entTransactions.Count + 1;
        entTransactions.Add(newRow);
        transaction = newRow;
    }
    async Task DltTransaction(DamageStockDtls item)
    {
        entTransactions.Remove(item);
        if (!entTransactions.Any()) AddNewRow();
        TotalFindMethod();
    }

    void TotalFindMethod()
    {
        foreach (var item in entTransactions)
        {
            item.Value = item.Quantity * item.Rate;
        }
        hdr.TotalValue = entTransactions.Sum(x => x.Value);
    }

    // --- Desktop Grid Methods ---

    async Task BarcodeChanged(DamageStockDtls item)
    {
        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Large };
        var selectedProduct = await Dialog.GetItemSearchMethod(item.Barcode);

        if (selectedProduct is not null)
        {
            item.FK_ItemId = selectedProduct.FK_ItemId;
            item.ItemName = selectedProduct.MultiUnitName;
            item.Barcode = selectedProduct.Barcode;
            item.ItemArabicName = selectedProduct.MultiUnitArabicName;
            item.FK_UnitId = selectedProduct.FK_UnitId;
            item.UnitName = selectedProduct.UnitName; // Ensure UnitName is set
            item.QtyPerUnit = selectedProduct.QtyPerUnit > 0 ? selectedProduct.QtyPerUnit : 1;
            item.Rate = selectedProduct.Rate ?? 0;
            item.Cost = selectedProduct.Rate ?? 0; // Default Cost to Rate? Or 0?
            item.Quantity = 1;

            // Update item unit list if needed (for that row? Current grid design uses shared unitlist... 
            // PurchaseReturn uses `itemUnitlist` for `transaction` (mobile) but for desktop likely uses shared unless it handles row-specific units.
            // My Razor uses `unitlist` (shared) for the dropdown. This assumes all items share units OR we load all.
            // Usually `unitlist` contains basic units. `GetItemSearchMethod` returns `AvailableUnits`.
            // If I want row-specific units, I need a property on Row or a smarter dropdown.
            // `PurchaseReturnView` desktop table uses `itemUnitlist` which is `unitlist`.
            // So it only shows global units.
            // We'll stick to that for now.

            TotalFindMethod();
        }
    }

    public async Task OnBarcodBlurEvent(FocusEventArgs e, DamageStockDtls item)
    {
        if (item.FK_ItemId > 0 && !string.IsNullOrEmpty(item.ItemName)) return; // Already loaded
        if (string.IsNullOrEmpty(item.Barcode)) return;

        // Simple fetch if barcode exists
        var multiUnit = await api.GetItemMultiUnit(1, 1, item.Barcode); // 1,1 are SearchMode/Unit? Check API.
        // api.GetItemMultiUnit(int searchMode, int unitId, string barcode)
        if (multiUnit is not null)
        {
            item.FK_ItemId = multiUnit.FK_ItemId;
            item.ItemName = multiUnit.MultiUnitName;
            item.Barcode = multiUnit.Barcode;
            item.ItemArabicName = multiUnit.MultiUnitArabicName;
            item.FK_UnitId = multiUnit.FK_UnitId;
            // item.UnitName ?
            item.QtyPerUnit = multiUnit.QTYPerUnit ?? 1;
            item.Rate = multiUnit.Rate ?? 0;
            item.Cost = multiUnit.Cost ?? 0;
            item.Quantity = 1;
            TotalFindMethod();
        }
    }

    async Task OnEnterMethod(KeyboardEventArgs e, DamageStockDtls item)
    {
        if (e.Key == "Enter")
        {
            await BarcodeChanged(item);
            // Focus Qty?
            // Not easy to focus specific row control without unique ref.
            // But existing behavior uses Enter to open Search.
        }
    }

    async Task TaskOnEnterMethod(KeyboardEventArgs e, int index)
    {
        if (e.Key == "Enter")
        {
            if (index == 2) // Qty
            {
                // Focus Rate?
                // await gridRateField.FocusAsync(); // This focuses the LAST rendered one.
            }
            else if (index == 4) // Rate
            {
                // Add new row?
                AddNewRow();
            }
        }
    }

    void RowClickEvent(TableRowClickEventArgs<DamageStockDtls> args)
    {
        // item = args.Item;
    }

    async Task OnChangeUnit(int unitId, DamageStockDtls item)
    {
        item.FK_UnitId = unitId;
        // Fetch Rate for this unit
        if (item.FK_ItemId > 0)
        {
            var multiUnit = await api.GetItemMultiUnit(item.FK_ItemId, unitId, "");
            if (multiUnit is not null)
            {
                item.Rate = multiUnit.Rate ?? 0;
                // Cost?
            }
        }
        TotalFindMethod();
    }

    // --- Mobile Methods ---

    async Task AddLineItem()
    {
        if (string.IsNullOrEmpty(transaction.ItemName)) { return; }
        entTransactions.Add(transaction);
        transaction = InitNewRow();
        TotalFindMethod();
    }

    async Task RowClicked(DataGridRowClickEventArgs<DamageStockDtls> args)
    {
        try
        {
            transaction = args.Item;
            IsEditing = true;
        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod(ex.Message);
        }
    }
    async Task MobileSearch()
    {
        await MobileSearchMethod(transaction.Barcode ?? "");
    }

    async Task MobileSearchMethod(string sVal)
    {
        var selectedProduct = await Dialog.GetItemSearchMethod(sVal);
        if (selectedProduct is not null)
        {
            // Populate transaction details for editing
            transaction.FK_ItemId = selectedProduct.FK_ItemId;
            transaction.ItemName = selectedProduct.MultiUnitName;
            transaction.Barcode = selectedProduct.Barcode;
            transaction.FK_UnitId = selectedProduct.FK_UnitId;
            transaction.UnitName = selectedProduct.UnitName;
            transaction.QtyPerUnit = selectedProduct.QtyPerUnit > 0 ? selectedProduct.QtyPerUnit : 1;
            transaction.Rate = selectedProduct.Rate ?? 0;
            transaction.Cost = selectedProduct.Rate ?? 0;
            transaction.Quantity = 1;

            TotalFindMethod();
            StateHasChanged();
        }
    }

    // --- CRUD & Actions ---

    private bool _isSaving = false;

    async Task SaveOrUpdate()
    {
        if (_isSaving) return;
        if (privilege is not null && hdr.DamageStockId == 0 && !privilege.SaveData)
        {
            Dialog.DialogMethod(Localizer("No...Privilege"));
            return;
        }
        if (privilege is not null && hdr.DamageStockId != 0 && !privilege.EditData)
        {
            Dialog.DialogMethod(Localizer("No...Privilege"));
            return;
        }
        _isSaving = true;
        StateHasChanged();

        try
        {
            var validItems = entTransactions.Where(x => x.Quantity > 0 && x.FK_ItemId > 0).ToList();
            if (!validItems.Any())
            {
                snackbar.Add("Please add at least one valid item", Severity.Warning);
                return;
            }

            var dto = new DamageStockDTO { Header = hdr, Details = validItems };

            if (hdr.DamageStockId == 0)
            {
                var res = await api.CreateDamageStock(dto);
                if (string.IsNullOrEmpty(res.Message))
                {
                    snackbar.Add("Saved Successfully", Severity.Success);
                    hdr.DamageStockId = res.Id;
                    if (await Dialog.DeleteDialogMethod(Localizer("PrintDialog")))
                    {
                        await PrintMethod(false);
                    }
                    await NewClick();
                }
                else Dialog.DialogMethod(res.Message);
            }
            else
            {
                var res = await api.UpdateDamageStock(dto);
                if (string.IsNullOrEmpty(res.Message))
                {
                    snackbar.Add("Updated Successfully", Severity.Success);
                    await NewClick();
                }
                else Dialog.DialogMethod(res.Message);
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

    async Task Delete()
    {
        if (hdr.DamageStockId == 0) return;
        if (privilege is not null && !privilege.DeleteData)
        {
            Dialog.DialogMethod(Localizer("No...Privilege"));
            return;
        }
        if (await Dialog.DeleteDialogMethod())
        {
            var res = await api.DeleteDamageStock(hdr.DamageStockId);
            if (res.Id > 0)
            {
                if (!string.IsNullOrEmpty(res.Message))
                    snackbar.Add(res.Message, Severity.Success);
                else
                    snackbar.Add("Deleted Successfully", Severity.Success);

                await NewClick();
            }
            else Dialog.DialogMethod(res.Message);
        }
    }

    async Task SearchMethod()
    {
        try
        {
            var result = await Dialog.TransactionSearchMethod("Damage Stock");
            if (result is not null)
            {
                var dto = await api.GetDamageStockById(result.Id);
                hdr = dto.Header;
                entTransactions = dto.Details;
                entTransactions.ForEach(x => x.SL = entTransactions.IndexOf(x) + 1);
                AddNewRow();
                TotalFindMethod();
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod(ex.Message);
        }
    }

    async Task PrintMethod(bool IsPreview = false)
    {
        if (hdr.DamageStockId == 0) return;

        // Fetch Print Design
        var pdto = await api.GetPrintDesignAsync(new PrintDesignReq(0, "Damage_Stock"));

        if (pdto.hdr == null || !pdto.dtls.Any())
        {
            snackbar.Add("Print Design 'Damage_Stock' not found", Severity.Warning);
            return;
        }

        var transHdr = new TransactionHdr
        {
            HdrId = hdr.DamageStockId,
            HdrNo = hdr.DamageStockNo,
            InvoiceDate = (hdr.EntryDate ?? DateTime.Now).ToString("dd-MM-yyyy"),
            HdrDate = hdr.EntryDate ?? DateTime.Now,
            GrandTotal = hdr.TotalValue,
            NetAmount = hdr.TotalValue
        };

        var party = new Party();
        party.Name = accountList.FirstOrDefault(x => x.Id == hdr.FK_AccountId)?.Name ?? "";

        // Map Details
        var transDetails = entTransactions.Select(d => new TransactionDtls
        {
            ItemName = d.ItemName,
            Barcode = d.Barcode,
            Quantity = d.Quantity,
            ExRate = d.Rate,
            LineTotal = d.Value,
            UnitName = unitlist.FirstOrDefault(u => u.Id == d.FK_UnitId)?.Name ?? d.UnitName,
            SL = entTransactions.IndexOf(d) + 1
        }).ToList();

        PrintParameterDTO dto = new PrintParameterDTO(pdto.hdr, pdto.dtls, transHdr, transDetails, party, AppData.CompanyDetails, null);
        await CrudHelper.PrintInvoiceMethodLocal(Downloader, dto, Dialog, IsPreview);
    }
}
