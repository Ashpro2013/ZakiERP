using Microsoft.AspNetCore.Components.Web;

namespace ZakiSAASERP.Shared.Pages.Masters
{
    public partial class OpeningStockEntryView : ITabAware
    {
        [Inject] IViewService? api { get; set; }
        [Inject] ISnackbar? Snackbar { get; set; }
        [Inject] NavigationManager? NavManager { get; set; }
        [Inject] IDialogService? DialogService { get; set; }
        [Inject] ModelService? ModelDialog { get; set; }
        [Inject] IJSRuntime JSRuntime { get; set; }
        [Parameter] public int TabId { get; set; }

        [Parameter] public int Id { get; set; }

        private int ActiveTabIndex { get; set; } = 0;

        private OpeningStockEntryDTO entryDto = new();
        private List<BranchMaster> branches = new();
        private List<UnitMaster> units = new();
        private List<StoreMaster> stores = new();
        WebUserPrivilege? privilege;
        private DateTime? date = DateTime.Now;
        private bool _isLoading = false;
        private string searchText = "";
        private MudDataGrid<OpeningStockEntry>? grid;

        // Mobile Entry
        private OpeningStockEntryItem mobileItem = new();
        private OpeningStockEntryItem? editingItem = null;

        public decimal TotalAmount => entryDto.EntryDetails.Sum(x => x.Amount);

        private async Task OnBranchChanged(int value)
        {
            if (entryDto.FK_BranchId == value) return;

            if (entryDto.EntryDetails.Any(x => x.FK_ItemId != 0))
            {
                bool? result = await DialogService.ShowMessageBox(
                    Localizer("Warning"),
                    Localizer("ChangeBranchWarning"),
                    yesText: Localizer("Yes"), cancelText: Localizer("No"));

                if (result != true)
                {
                    // Reset to old value (force UI update if needed, but binding might be tricky without StateHasChanged loop)
                    await InvokeAsync(StateHasChanged);
                    return;
                }
            }

            entryDto.FK_BranchId = value;

            // Retain items but update their Store to the new Branch's default store
            // User said: "just change to login branch default store" -> likely means "change to new branch's default store".
            // "login branch default store" is a bit ambiguous if I just selected a DIFFERENT branch.
            // Context: "change branches... change to login branch default store". 
            // If I change selected branch to B, I probably want stores from B.
            // "Default store" for a branch is where `IsDefaultStore == true`.

            var newDefaultStore = stores.FirstOrDefault(x => x.FK_BranchId == value && x.IsDefaultStore)?.StoreId
                                  ?? stores.FirstOrDefault(x => x.FK_BranchId == value)?.StoreId
                                  ?? stores.FirstOrDefault(x => x.FK_BranchId == null)?.StoreId
                                  ?? 0;

            foreach (var item in entryDto.EntryDetails)
            {
                item.FK_StoreId = newDefaultStore;
            }
            await Task.Delay(100); // Allow dialog to close and UI thread to catch up
            await InvokeAsync(StateHasChanged);
        }

        // For displaying existing stock details for selected item
        private List<OpeningStockEntryItemForUpdate> ExistingItemDetails = new();
        private string SelectedItemDisplayName = "";

        protected override async Task OnInitializedAsync()
        {
            try
            {
                _isLoading = true;
                branches = await api!.GetAllBranches();
                units = await api!.GetAllUnits();
                stores = await api!.GetAllStores();

                if (AppData.privileges is not null)
                    privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "OpeningStockEntry");

                if (Id > 0)
                {
                    entryDto = await api.GetOpeningStockEntryFull(Id);
                    date = entryDto.EntryDate;
                }
                else
                {
                    await LoadNewDefaults();
                }
            }
            catch (Exception ex)
            {
                ModelDialog?.DialogMethod(ex.Message);
            }
            finally
            {
                _isLoading = false;
            }
        }

        private bool _focusOnRender = false;

        private async Task LoadNewDefaults()
        {
            _isLoading = true;
            try
            {
                entryDto = new OpeningStockEntryDTO();
                entryDto.DocumentNo = await api!.GetNewOpeningStockEntryDocNo();
                entryDto.FK_BranchId = AppData.BranchId > 0 ? AppData.BranchId : (branches.FirstOrDefault()?.BranchId ?? 0);
                //entryDto.EnteredBy = AppData.UserId?.ToString(); // Default to logged in user
                entryDto.FK_UserId = AppData.UserId ?? 0;
                date = DateTime.Now;
                entryDto.EntryDetails = new List<OpeningStockEntryItem>();
                AddNewRow();
            }
            finally
            {
                _isLoading = false;
                _focusOnRender = true;
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (_focusOnRender)
            {
                _focusOnRender = false;
                await Task.Delay(300);
                await FocusById("barcode-0");
            }
        }

        private void AddNewRow()
        {
            if (entryDto.EntryDetails.Count > 0)
            {
                var lastItem = entryDto.EntryDetails.Last();
                if (!CheckRowValidation(lastItem)) return;
            }

            // Fix: Default store based on SELECTED branch if available, else logged-in branch
            int targetBranch = entryDto.FK_BranchId > 0 ? entryDto.FK_BranchId : AppData.BranchId;

            // Refined Default Store Selection:
            // 1. Try getting default store for target branch.
            // 2. Try getting ANY store for target branch.
            // 3. Fallback to Common Store (FK_BranchId is null).
            // 4. Fallback to first available store.
            int defaultStore = stores.FirstOrDefault(x => x.FK_BranchId == targetBranch && x.IsDefaultStore)?.StoreId
                               ?? stores.FirstOrDefault(x => x.FK_BranchId == targetBranch)?.StoreId
                               ?? stores.FirstOrDefault(x => x.FK_BranchId == null)?.StoreId
                               ?? stores.FirstOrDefault()?.StoreId ?? 0;

            int defaultUnit = units.FirstOrDefault()?.UnitId ?? 0;

            // Clear Existing Items Table as requested
            ExistingItemDetails.Clear();

            entryDto.EntryDetails.Add(new OpeningStockEntryItem
            {
                Qty = 1,
                Rate = 0,
                Amount = 0,
                FK_StoreId = defaultStore,
                FK_UnitId = defaultUnit,
                SlNo = entryDto.EntryDetails.Count + 1
            });
        }

        private bool CheckRowValidation(OpeningStockEntryItem item)
        {
            if (item.FK_ItemId == 0)
            {
                // New Validation: If user typed Code/Name but didn't select from dialog
                if (!string.IsNullOrWhiteSpace(item.ItemCode) || !string.IsNullOrWhiteSpace(item.ItemName))
                {
                    ModelDialog?.DialogMethod(Localizer("PleaseSelectItemViaSearch"));
                    return false;
                }

                // User requested explicit message for empty rows instead of silent failure
                ModelDialog?.DialogMethod(Localizer("RowEmptyError"));
                return false;
            }

            // Fix: Duplicate validation (Same Item + Unit + Store)
            if (entryDto.EntryDetails.Any(x => x != item && x.FK_ItemId == item.FK_ItemId && x.FK_UnitId == item.FK_UnitId && x.FK_StoreId == item.FK_StoreId))
            {
                ModelDialog?.DialogMethod(Localizer("DuplicateItemStoreUnit"));
                return false;
            }

            // Ensure Quantity is valid
            if (item.Qty <= 0)
            {
                ModelDialog?.DialogMethod(Localizer("QuantityMustBeGreaterThanZero"));
                return false;
            }

            return true;
        }

        private void RemoveRow(OpeningStockEntryItem item)
        {
            entryDto.EntryDetails.Remove(item);
            if (entryDto.EntryDetails.Count == 0) AddNewRow();
        }

        private void CalculateAmount(OpeningStockEntryItem item)
        {
            item.Amount = item.Qty * item.Rate;
        }

        private async Task SetSelectedItem(OpeningStockEntryItem item, ItemMaster selectedItem, ItemSelectionResult? selectionResult = null)
        {
            item.FK_ItemId = selectedItem.ItemId;
            item.Item = selectedItem;
            item.ItemCode = selectedItem.ItemCode;
            item.LocName = selectedItem.Location; // Auto-fill Location default

            SelectedItemDisplayName = $"{selectedItem.ItemCode} - {selectedItem.ItemName}";

            // Initialize lists
            item.AvailableUnits = new List<UnitMaster>();
            item.MultiUnitDetails = new List<ItemMultiUnit>();

            // Check ItemSearchMode to determine unit loading behavior
            if (PermanentSettingsData.ItemSearchMode == 2 && selectionResult?.AvailableUnits is not null)
            {
                // ProductSearchDialog (Mode 2): Use data from ItemSelectionResult
                // Use MultiUnit name and barcode, not the ItemMaster defaults
                item.ItemName = selectionResult.ItemName; // MultiUnitName
                item.ArabicName = selectionResult.ItemNameArabic; // MultiUnitArabicName
                item.Barcode = selectionResult.Barcode; // MultiUnit barcode

                item.AvailableUnits = selectionResult.AvailableUnits
                    .Select(u => new UnitMaster { UnitId = u.Id, Name = u.Name })
                    .ToList();
                item.MultiUnitDetails = selectionResult.MultiUnitDetails ?? new List<ItemMultiUnit>();

                // Set the selected unit
                var defaultUnit = selectionResult.FK_UnitId > 0
                    ? selectionResult.FK_UnitId
                    : (item.AvailableUnits.FirstOrDefault()?.UnitId ?? 0);
                await OnUnitChanged(item, defaultUnit);
            }
            else
            {
                // ItemSearchDialog (Mode 1): Use ItemMaster defaults
                item.ItemName = selectedItem.ItemName;
                item.ArabicName = selectedItem.ItemNameArabic;
                item.Barcode = selectedItem.Barcode;

                // Load ALL units
                var fullItem = await api!.GetItemById(selectedItem.ItemId);

                if (fullItem is not null)
                {
                    item.MultiUnitDetails = fullItem.MultiUnits?.ToList() ?? new List<ItemMultiUnit>();

                    // Add Base Unit
                    if (fullItem.Unit is not null)
                    {
                        item.AvailableUnits.Add(fullItem.Unit);
                    }

                    // Add MultiUnits
                    if (fullItem.MultiUnits is not null)
                    {
                        foreach (var mu in fullItem.MultiUnits)
                        {
                            if (mu.Unit is not null && !item.AvailableUnits.Any(x => x.UnitId == mu.Unit.UnitId))
                            {
                                item.AvailableUnits.Add(mu.Unit);
                            }
                        }
                    }

                    // Default to Base Unit
                    var defaultUnit = fullItem.FK_UnitId > 0 ? fullItem.FK_UnitId.Value : (units.FirstOrDefault()?.UnitId ?? 0);
                    await OnUnitChanged(item, defaultUnit);
                }
                else
                {
                    // Fallback if fetch fails
                    var tempUnit = selectedItem.FK_UnitId > 0 ? selectedItem.FK_UnitId.Value : (units.FirstOrDefault()?.UnitId ?? 0);
                    item.AvailableUnits = units.ToList(); // Fallback to all units
                    await OnUnitChanged(item, tempUnit);
                }
            }

            // Ensure UI updates to show specific units in dropdown
            StateHasChanged();

            // Trigger lookup for existing entries
            _ = LoadExistingItemDetails(selectedItem.ItemId);
        }

        private async Task OnUnitChanged(OpeningStockEntryItem item, int unitId)
        {
            item.FK_UnitId = unitId;

            // Logic to update QtyPerUnit and Rate/Cost
            // Priority: Check MultiUnit definitions first
            var mu = item.MultiUnitDetails.FirstOrDefault(x => x.FK_UnitId == unitId);

            if (mu is not null)
            {
                // Found explicit MultiUnit definition
                item.QtyPerUnit = mu.QTYPerUnit ?? 1;

                // Rate Logic: Cost > Rate > Default
                item.Rate = (mu.Cost > 0 ? mu.Cost : (mu.Rate > 0 ? mu.Rate : 0)) ?? 0;
                if (item.Rate == 0 && (item.Item?.StandardCost ?? 0) > 0)
                {
                    // Fallback to Standard Cost * QtyPerUnit if specific MultiUnit cost is missing
                    // Assumption: StandardCost is for Base Unit (1).
                    item.Rate = (item.Item!.StandardCost!.Value) * item.QtyPerUnit;
                }
            }
            else
            {
                // Not in MultiUnits, check if Base Unit
                bool isBase = item.Item is not null && item.Item.FK_UnitId == unitId;
                if (isBase)
                {
                    item.QtyPerUnit = 1;
                    item.Rate = item.Item?.StandardCost ?? 0;
                }
                else
                {
                    // Unknown Unit (shouldn't happen if filtered correctly)
                    item.QtyPerUnit = 1;
                    item.Rate = 0;
                }
            }

            CalculateAmount(item);
            await InvokeAsync(StateHasChanged);
        }

        private bool _isDialogOpen = false;

        private async Task OpenItemDialog(OpeningStockEntryItem item, string searchText = "")
        {
            if (_isDialogOpen) return;
            _isDialogOpen = true;
            try
            {
                // Fix: Ensure Branch is selected first
                if (entryDto.FK_BranchId == 0)
                {
                    ModelDialog?.DialogMethod(Localizer("SelectBranchFirst"));
                    return;
                }

                // Use centralized ModelService item search (respects ItemSearchMode setting)
                var selectedMultiUnit = await ModelDialog!.GetItemSearchMethod(searchText, Localizer("SearchItem"));

                if (selectedMultiUnit is not null && selectedMultiUnit.FK_ItemId > 0)
                {
                    // Fetch full ItemMaster by ID for SetSelectedItem
                    var selectedItem = await api!.GetItemById(selectedMultiUnit.FK_ItemId);
                    if (selectedItem is not null)
                    {
                        // Pass the ItemSelectionResult so we can use its AvailableUnits
                        await SetSelectedItem(item, selectedItem, selectedMultiUnit);

                        // Focus next field (Qty)
                        int index = entryDto.EntryDetails.IndexOf(item);
                        // Delay to allow dialog to fully close and prevent focus fighting
                        await Task.Delay(300);
                        await FocusById($"qty-{index}");
                    }
                }
            }
            finally
            {
                // Delay releasing lock to prevent 'Enter' key bounce from re-triggering click
                await Task.Delay(500);
                _isDialogOpen = false;
            }
        }

        private async Task LoadExistingItemDetails(int itemId)
        {
            try
            {
                ExistingItemDetails = await api!.GetOpeningStockEntryExistingStock(itemId);
                StateHasChanged();
            }
            catch (Exception ex)
            {
                // Ignore or log
                Console.WriteLine(ex.Message);
                ExistingItemDetails.Clear();
            }
        }

        private bool _isSearching = false;
        private bool _dialogJustClosed = false;

        private async Task HandleFieldEnter(KeyboardEventArgs e, OpeningStockEntryItem item, string field)
        {
            if (_dialogJustClosed) return; // Debounce check
            var index = entryDto.EntryDetails.IndexOf(item);

            if (e.Key == "ArrowUp")
            {
                if (index > 0)
                {
                    await FocusById($"{field}-{index - 1}");
                }
            }
            else if (e.Key == "ArrowDown")
            {
                if (index < entryDto.EntryDetails.Count - 1)
                {
                    await FocusById($"{field}-{index + 1}");
                }
            }
            else if (e.Key == "Enter" || e.Key == "NumpadEnter")
            {
                if (_isSearching) return;
                _isSearching = true;
                try
                {
                    switch (field)
                    {
                        case "barcode":
                            string barcodeQuery = item.Barcode;

                            if (item.FK_ItemId != 0 && item.Barcode == barcodeQuery)
                            {
                                await FocusById($"qty-{index}");
                                return;
                            }

                            if (!string.IsNullOrWhiteSpace(barcodeQuery))
                            {
                                var pagination = new PaginationModel(1, 2, "ItemCode", "asc", barcodeQuery);
                                var result = await api!.GetItemPDetails(pagination);

                                if (result.TotalCount == 1 && result.PItems.Count == 1)
                                {
                                    await SetSelectedItem(item, result.PItems[0]);
                                    await FocusById($"qty-{index}");
                                }
                                else
                                {
                                    await OpenItemDialog(item, barcodeQuery);
                                }
                            }
                            else
                            {
                                await FocusById($"itemcode-{index}");
                            }
                            break;

                        case "itemcode":
                        case "itemname":
                            // Smart Search Logic
                            string searchQuery = field == "itemcode" ? item.ItemCode : item.ItemName;

                            // Optimization: If we already have a selected item and the text matches, don't re-search
                            if (item.FK_ItemId != 0 && (
                                (field == "itemcode" && item.ItemCode == searchQuery) ||
                                (field == "itemname" && item.ItemName == searchQuery)
                            ))
                            {
                                await FocusById($"qty-{index}");
                                return;
                            }

                            if (!string.IsNullOrWhiteSpace(searchQuery))
                            {
                                var pagination = new PaginationModel(1, 2, "ItemCode", "asc", searchQuery);
                                var result = await api!.GetItemPDetails(pagination);

                                if (result.TotalCount == 1 && result.PItems.Count == 1)
                                {
                                    // Exact/Single Match -> Auto Select
                                    await SetSelectedItem(item, result.PItems[0]);
                                    // Focus Qty
                                    await FocusById($"qty-{index}");
                                }
                                else
                                {
                                    // 0 or Multiple matches -> Open Dialog
                                    await OpenItemDialog(item, searchQuery);
                                }
                            }
                            else
                            {
                                // Empty text -> Open Dialog (show all)
                                await OpenItemDialog(item);
                            }
                            break;

                        case "unit":
                            await FocusById($"qtyperunit-{index}");
                            break;
                        case "qtyperunit":
                            await FocusById($"store-{index}");
                            break;
                        case "store":
                            await FocusById($"qty-{index}");
                            break;
                        case "qty":
                            await FocusById($"rate-{index}");
                            break;
                        case "rate":
                            await FocusById($"loc-{index}");
                            break;
                        case "loc":
                            if (index == entryDto.EntryDetails.Count - 1)
                            {
                                AddNewRow();
                                await Task.Delay(50);
                                await FocusById($"barcode-{index + 1}");
                            }
                            else
                            {
                                await FocusById($"barcode-{index + 1}");
                            }
                            break;
                    }
                }
                finally
                {
                    _isSearching = false;
                }
            }
        }

        private async Task FocusById(string id)
        {
            try
            {
                // Robust focus: Try element itself, then try finding an input inside it (for MudBlazor wrappers)
                await JSRuntime!.InvokeVoidAsync("eval", $"var e = document.getElementById('{id}'); if(e) {{ var inp = e.querySelector('input'); if(inp) {{ inp.focus(); if(inp.select) inp.select(); }} else {{ e.focus(); if(e.select) e.select(); }} }}");
            }
            catch { /* Ignore if element not found */ }
        }

        private bool _isSaving = false;

        private async Task Save(bool isMobile = false)
        {
            if (_isSaving) return;
            _isSaving = true;
            StateHasChanged();
            try
            {
                entryDto.EntryDate = date ?? DateTime.Now;
                if (entryDto.FK_BranchId == 0)
                {
                    entryDto.FK_BranchId = AppData.BranchId;
                }
                entryDto.FK_UserId = AppData.UserId ?? 0;

                // Mobile: Remove empty rows FIRST to avoid validation errors on ghost rows
                if (isMobile)
                {
                    entryDto.EntryDetails.RemoveAll(x => x.FK_ItemId == 0 && string.IsNullOrWhiteSpace(x.ItemCode) && string.IsNullOrWhiteSpace(x.ItemName));
                }

                if (entryDto.EntryDetails.Count == 0 || (entryDto.EntryDetails.Count == 1 && entryDto.EntryDetails[0].FK_ItemId == 0))
                {
                    ModelDialog?.DialogMethod(Localizer("NoDetails"));
                    return;
                }

                // Validate rows
                foreach (var item in entryDto.EntryDetails.ToList())
                {
                    if (!CheckRowValidation(item)) return;
                }

                // Desktop: Remove empty rows AFTER validation (if any exist but somehow passed validation? usually validation catches them)
                // Actually, if validation passes, it means rows are valid. But if we skipped empty rows above for Mobile, we don't need to do it again here.
                // But for Desktop, we didn't remove them yet. So if validation passed (maybe logic allows empty rows? No, CheckRowValidation returns false for ItemId=0).
                // So on Desktop, validation WILL fail if there is an empty row. This matches user request.
                // However, we should still clean up any potential garbage before sending to API if validation passed.
                if (!isMobile)
                {
                    entryDto.EntryDetails.RemoveAll(x => x.FK_ItemId == 0 && string.IsNullOrWhiteSpace(x.ItemCode) && string.IsNullOrWhiteSpace(x.ItemName));
                }

                if (entryDto.OpeningStockEntryId == 0)
                {
                    if (privilege is not null && !privilege.SaveData) { ModelDialog?.DialogMethod(Localizer("NoSavePrivilege")); return; }

                    var res = await api!.CreateOpeningStockEntry(entryDto);
                    if (res.Id > 0)
                    {
                        await DialogService.ShowMessageBox(Localizer("Save"), Localizer("SavedSuccessfully"), yesText: Localizer("Ok"));
                        await Clear(true);
                        if (grid is not null) await grid.ReloadServerData();
                    }
                    else
                    {
                        ModelDialog?.DialogMethod(res.Message);
                    }
                }
                else
                {
                    if (privilege is not null && !privilege.EditData) { ModelDialog?.DialogMethod(Localizer("NoEditPrivilege")); return; }

                    var res = await api!.UpdateOpeningStockEntry(entryDto);
                    if (res.Id > 0)
                    {
                        await DialogService.ShowMessageBox(Localizer("Update"), Localizer("UpdatedSuccessfully"), yesText: Localizer("Ok"));
                        await Clear(true);
                        if (grid is not null) await grid.ReloadServerData();
                    }
                    else
                    {
                        ModelDialog?.DialogMethod(res.Message);
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

        private async Task Clear(bool fullReset)
        {
            // Fix: Only warn if in New Mode (Id == 0) and has data (as requested)
            if (Id == 0 && !fullReset && entryDto.EntryDetails.Any(x => x.FK_ItemId != 0))
            {
                bool? result = await DialogService.ShowMessageBox(
                    Localizer("Warning"),
                    Localizer("UnsavedChanges"),
                    yesText: Localizer("Yes"), cancelText: Localizer("No"));
                if (result != true) return;
            }

            Id = 0;
            await LoadNewDefaults();
            ExistingItemDetails.Clear();
            if (grid is not null) await grid.ReloadServerData();
            ActiveTabIndex = 0; // Ensure we go back to Entry tab on New/Clear
        }

        private async Task<GridData<OpeningStockEntry>> LoadServerData(GridState<OpeningStockEntry> state)
        {
            var pagination = new PaginationModel(state.Page + 1, state.PageSize, state.SortDefinitions.FirstOrDefault()?.SortBy, state.SortDefinitions.FirstOrDefault()?.Descending == true ? "desc" : "asc", searchText);
            var result = await api!.GetOpeningStockEntryPDetails(pagination);
            return new GridData<OpeningStockEntry> { Items = result.POpeningStockEntries, TotalItems = result.TotalCount };
        }

        private void OnSearch(string text)
        {
            searchText = text;
            grid?.ReloadServerData();
        }

        private async Task RowClicked(DataGridRowClickEventArgs<OpeningStockEntry> args)
        {

            // Allow switching to any row immediately without warning (as requested)
            Id = args.Item.OpeningStockEntryId;
            _isLoading = true;
            entryDto = await api!.GetOpeningStockEntryFull(Id);
            date = entryDto.EntryDate;

            // Populate MultiUnits for existing items
            foreach (var item in entryDto.EntryDetails)
            {
                if (item.FK_ItemId > 0)
                {
                    var fullItem = await api.GetItemById(item.FK_ItemId);
                    if (fullItem is not null)
                    {
                        item.MultiUnitDetails = fullItem.MultiUnits?.ToList() ?? new List<ItemMultiUnit>();
                        item.AvailableUnits = new List<UnitMaster>();
                        if (fullItem.Unit is not null) item.AvailableUnits.Add(fullItem.Unit);
                        if (fullItem.MultiUnits is not null)
                        {
                            foreach (var mu in fullItem.MultiUnits)
                            {
                                if (mu.Unit is not null && !item.AvailableUnits.Any(x => x.UnitId == mu.Unit.UnitId))
                                {
                                    item.AvailableUnits.Add(mu.Unit);
                                }
                            }
                        }
                    }
                }
            }

            _isLoading = false;
            ActiveTabIndex = 0; // Switch to Entry Tab
            StateHasChanged();
        }
        async Task Delete()
        {
            try
            {
                if (entryDto.OpeningStockEntryId == 0) return;

                if (privilege is not null && !privilege.DeleteData) { ModelDialog?.DialogMethod(Localizer("NoDeletePrivilege")); return; }

                if (!await ModelDialog!.DeleteDialogMethod())
                    return;

                var result = await api!.DeleteOpeningStockEntry(entryDto.OpeningStockEntryId);
                if (result.Message == null)
                {
                    await Clear(true);
                    if (grid is not null) await grid.ReloadServerData();
                }
                else
                {
                    if (result.Message.Contains("FK_", StringComparison.OrdinalIgnoreCase) || result.Message.Contains("REFERENCE", StringComparison.OrdinalIgnoreCase) || result.Message.Contains("Constraint", StringComparison.OrdinalIgnoreCase))
                        ModelDialog?.DialogMethod(ArabicLocalization.GetFriendlyForeignKeyError(result.Message));
                    else
                        ModelDialog?.DialogMethod(result.Message);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("FK_", StringComparison.OrdinalIgnoreCase) || ex.Message.Contains("REFERENCE", StringComparison.OrdinalIgnoreCase) || ex.Message.Contains("Constraint", StringComparison.OrdinalIgnoreCase))
                    ModelDialog?.DialogMethod(ArabicLocalization.GetFriendlyForeignKeyError(ex.Message));
                else
                    ModelDialog?.DialogMethod(ex.Message);
            }
        }

        // --- Mobile Methods ---

        private void MobileAddClick()
        {
            mobileItem = new OpeningStockEntryItem { Qty = 1 };
            editingItem = null;

            // Set Default Store
            int targetBranch = entryDto.FK_BranchId > 0 ? entryDto.FK_BranchId : AppData.BranchId;
            mobileItem.FK_StoreId = stores.FirstOrDefault(x => x.FK_BranchId == targetBranch && x.IsDefaultStore)?.StoreId
                                   ?? stores.FirstOrDefault(x => x.FK_BranchId == targetBranch)?.StoreId
                                   ?? stores.FirstOrDefault(x => x.FK_BranchId == null)?.StoreId
                                   ?? stores.FirstOrDefault()?.StoreId ?? 0;

            mobileItem.FK_UnitId = units.FirstOrDefault()?.UnitId ?? 0;

            ActiveTabIndex = 1; // Switch to Items tab
            StateHasChanged();
        }

        private void AddMobileRow()
        {
            if (mobileItem.FK_ItemId == 0 && string.IsNullOrEmpty(mobileItem.ItemName))
            {
                Snackbar!.Add(Localizer("PleaseSelectAnItem"), Severity.Warning);
                return;
            }

            if (mobileItem.Qty <= 0)
            {
                Snackbar!.Add(Localizer("QuantityMustBeGreaterThanZero"), Severity.Warning);
                return;
            }

            if (editingItem is not null)
            {
                // Update Mode
                editingItem.FK_ItemId = mobileItem.FK_ItemId;
                editingItem.ItemName = mobileItem.ItemName;
                editingItem.ItemCode = mobileItem.ItemCode; // Ensure code is copied
                editingItem.Barcode = mobileItem.Barcode;
                editingItem.FK_UnitId = mobileItem.FK_UnitId;
                editingItem.UnitName = units.FirstOrDefault(u => u.UnitId == mobileItem.FK_UnitId)?.Name; // Map Unit Name
                editingItem.FK_StoreId = mobileItem.FK_StoreId;
                editingItem.Store = stores.FirstOrDefault(s => s.StoreId == mobileItem.FK_StoreId); // Map Store Object if needed
                editingItem.Qty = mobileItem.Qty;
                editingItem.Rate = mobileItem.Rate;
                editingItem.QtyPerUnit = mobileItem.QtyPerUnit;
                editingItem.LocName = mobileItem.LocName;

                CalculateAmount(editingItem);

                Snackbar!.Add(Localizer("ItemUpdated"), Severity.Success);
                MobileAddClick();
            }
            else
            {
                // Add Mode
                // Check Duplicates
                if (entryDto.EntryDetails.Any(x => x.FK_ItemId == mobileItem.FK_ItemId && x.FK_UnitId == mobileItem.FK_UnitId && x.FK_StoreId == mobileItem.FK_StoreId))
                {
                    Snackbar!.Add(Localizer("DuplicateItemStoreUnit"), Severity.Warning);
                    return;
                }

                mobileItem.SlNo = entryDto.EntryDetails.Count + 1;
                mobileItem.UnitName = units.FirstOrDefault(u => u.UnitId == mobileItem.FK_UnitId)?.Name;
                mobileItem.Store = stores.FirstOrDefault(s => s.StoreId == mobileItem.FK_StoreId);

                CalculateAmount(mobileItem);
                entryDto.EntryDetails.Add(mobileItem);

                Snackbar!.Add(Localizer("ItemAdded"), Severity.Success);
                MobileAddClick();
            }
        }

        private void CalculateMobileItem()
        {
            mobileItem.Amount = mobileItem.Qty * mobileItem.Rate;
        }

        private void MobileRowClicked(DataGridRowClickEventArgs<OpeningStockEntryItem> args)
        {
            editingItem = args.Item;

            // Clone to mobileItem
            mobileItem = new OpeningStockEntryItem
            {
                OpeningStockEntryItemId = args.Item.OpeningStockEntryItemId,
                FK_OpeningStockEntryId = args.Item.FK_OpeningStockEntryId,
                SlNo = args.Item.SlNo,
                FK_ItemId = args.Item.FK_ItemId,
                ItemName = args.Item.ItemName,
                ItemCode = args.Item.ItemCode,
                Barcode = args.Item.Barcode,
                FK_UnitId = args.Item.FK_UnitId,
                UnitName = args.Item.UnitName,
                FK_StoreId = args.Item.FK_StoreId,
                Store = args.Item.Store, // Keep ref
                Qty = args.Item.Qty,
                Rate = args.Item.Rate,
                Amount = args.Item.Amount,
                QtyPerUnit = args.Item.QtyPerUnit,
                LocName = args.Item.LocName,
                // Copy lists for helper methods
                AvailableUnits = args.Item.AvailableUnits,
                MultiUnitDetails = args.Item.MultiUnitDetails
            };

            ActiveTabIndex = 1;
        }

        private async Task MobileItemSearch()
        {
            await OpenItemDialog(mobileItem);
        }

        private async Task OnMobileUnitChanged(int unitId)
        {
            await OnUnitChanged(mobileItem, unitId);
        }

        private async Task OnListRowClicked(DataGridRowClickEventArgs<OpeningStockEntry> args)
        {
            await LoadOpeningStockEntry(args.Item.OpeningStockEntryId);
            ActiveTabIndex = 0;
        }

        private async Task LoadOpeningStockEntry(int id)
        {
            try
            {
                _isLoading = true;
                var result = await api!.GetOpeningStockEntryFull(id);
                if (result is not null)
                {
                    entryDto = result;
                    date = entryDto.EntryDate;

                    // Populate MultiUnits for existing items (copied from RowClicked logic)
                    foreach (var item in entryDto.EntryDetails)
                    {
                        if (item.FK_ItemId > 0)
                        {
                            var fullItem = await api.GetItemById(item.FK_ItemId);
                            if (fullItem is not null)
                            {
                                item.MultiUnitDetails = fullItem.MultiUnits?.ToList() ?? new List<ItemMultiUnit>();
                                item.AvailableUnits = new List<UnitMaster>();
                                if (fullItem.Unit is not null) item.AvailableUnits.Add(fullItem.Unit);
                                if (fullItem.MultiUnits is not null)
                                {
                                    foreach (var mu in fullItem.MultiUnits)
                                    {
                                        if (mu.Unit is not null && !item.AvailableUnits.Any(x => x.UnitId == mu.Unit.UnitId))
                                        {
                                            item.AvailableUnits.Add(mu.Unit);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Reset Mobile inputs
                    MobileAddClick();
                }
            }
            catch (Exception ex)
            {
                ModelDialog?.DialogMethod(ex.Message);
            }
            finally
            {
                _isLoading = false;
            }
        }
    }
}
