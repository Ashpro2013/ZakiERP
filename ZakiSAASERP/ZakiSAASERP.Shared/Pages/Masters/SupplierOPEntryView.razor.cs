using Microsoft.AspNetCore.Components.Web;

namespace ZakiSAASERP.Shared.Pages.Masters
{
    public partial class SupplierOPEntryView : ITabAware
    {
        [Parameter] public int TabId { get; set; }
        [Inject] IViewService? api { get; set; }
        [Inject] ISnackbar? SnackBar { get; set; }
        [Inject] NavigationManager? nav { get; set; }
        [Inject] IDialogService? Dialog { get; set; }
        [Inject] ModelService? ModelDialog { get; set; }
        [Inject] IJSRuntime? js { get; set; }

        [Parameter] public int Id { get; set; }

        private SupplierOPEntryDTO entryDto = new();
        private DateTime? date = DateTime.Now;
        private bool _isLoading = false;
        private bool _focusOnRender = false;
        private bool _isSearching = false;

        private List<BranchMaster> branches = new();
        WebUserPrivilege? privilege;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                branches = await api!.GetAllBranches();

                if (Id > 0)
                {
                    _isLoading = true;
                    entryDto = await api.GetSupplierOPEntryById(Id);
                    date = entryDto.EntryDate;
                }
                else
                {
                    await LoadNewDefaults();
                }
                if (AppData.privileges is not null)
                    privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "SupplierOpeningEntry");
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

        private async Task LoadNewDefaults()
        {
            _isLoading = true;
            try
            {
                entryDto = new SupplierOPEntryDTO();
                entryDto.DocNo = await api!.GetNewSupplierOPEntryDocNo();
                date = DateTime.Now;
                entryDto.EntryDetails = new List<SupplierOPEntryDetails>();
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
                await FocusById("suppcode-0");
            }
        }

        private void AddNewRow()
        {
            if (entryDto.EntryDetails.Count > 0)
            {
                var lastItem = entryDto.EntryDetails.Last();
                if (!CheckRowValidation(lastItem)) return;
            }

            entryDto.EntryDetails.Add(new SupplierOPEntryDetails
            {
                PurchaseDate = DateTime.Now,
                PurchaseAmt = 0,
                ReceivedAmt = 0,
                DiscountAmt = 0,
                ReturnedAmt = 0,
                BalanceAmt = 0,
                FK_BranchId = AppData.BranchId
            });
        }

        private bool CheckRowValidation(SupplierOPEntryDetails item)
        {
            if (item.FK_SupplierId is null or 0)
            {
                // Check if user manually typed something without selecting
                if (!string.IsNullOrWhiteSpace(item.SupplierCode) || !string.IsNullOrWhiteSpace(item.SupplierName))
                {
                    ModelDialog?.DialogMethod(Localizer("PleaseSelectSupplierViaSearch"));
                    return false;
                }

                ModelDialog?.DialogMethod(Localizer("SupplierRequired"));
                return false;
            }
            if (string.IsNullOrWhiteSpace(item.PurchaseNo))
            {
                ModelDialog?.DialogMethod(Localizer("PurchaseNoRequired"));
                return false;
            }
            if ((item.PurchaseAmt ?? 0) <= 0)
            {
                ModelDialog?.DialogMethod(Localizer("AmountRequired"));
                return false;
            }
            return true;
        }

        private void RemoveRow(SupplierOPEntryDetails item)
        {
            entryDto.EntryDetails.Remove(item);
        }

        private void ItemHasBeenCommitted(object element)
        {
            if (element is SupplierOPEntryDetails detail)
            {
                CalculateBalance(detail);
            }
        }

        private void CalculateBalance(SupplierOPEntryDetails detail)
        {
            detail.BalanceAmt = (detail.PurchaseAmt ?? 0) - (detail.ReceivedAmt ?? 0) - (detail.DiscountAmt ?? 0) - (detail.ReturnedAmt ?? 0);
            StateHasChanged();
        }

        private async Task HandleFieldEnter(KeyboardEventArgs e, SupplierOPEntryDetails detail, string fieldName)
        {
            if (e.Key == "Enter" || e.Key == "NumpadEnter")
            {
                if (_isSearching || _isDialogOpen) return;
                _isSearching = true;
                try
                {
                    var index = entryDto.EntryDetails.IndexOf(detail);
                    switch (fieldName)
                    {
                        case "suppcode":
                        case "suppname":
                            // Smart Search Logic
                            string searchQuery = fieldName == "suppcode" ? detail.SupplierCode : detail.SupplierName;

                            // Optimization: Check for existing valid selection
                            if ((detail.FK_SupplierId ?? 0) != 0 && (
                                (fieldName == "suppcode" && detail.SupplierCode == searchQuery) ||
                                (fieldName == "suppname" && detail.SupplierName == searchQuery)
                            ))
                            {
                                await FocusById($"purno-{index}");
                                return;
                            }

                            if (!string.IsNullOrWhiteSpace(searchQuery))
                            {
                                var searchCol = fieldName == "suppcode" ? "Code" : "Name";
                                var pagination = new PaginationModel(1, 2, searchCol, "asc", searchQuery);
                                var result = await api!.GetSupplierPDetails(pagination);

                                if (result.TotalCount == 1 && result.PSuppliers.Count == 1)
                                {
                                    SetSelectedSupplier(detail, result.PSuppliers[0]);
                                    StateHasChanged();
                                    await FocusById($"purno-{index}");
                                }
                                else
                                {
                                    await OpenSupplierDialog(detail, searchQuery);
                                }
                            }
                            else
                            {
                                await OpenSupplierDialog(detail);
                            }
                            break;

                        case "purno":
                            await Task.Delay(300);
                            await FocusById($"purdate-{index}");
                            break;
                        case "puramt":
                            await Task.Delay(300);
                            await FocusById($"recamt-{index}");
                            break;
                        case "recamt":
                            await Task.Delay(300);
                            await FocusById($"discamt-{index}");
                            break;
                        case "discamt":
                            await Task.Delay(300);
                            await FocusById($"retamt-{index}");
                            break;
                        case "retamt":
                            await Task.Delay(300);
                            if (index == entryDto.EntryDetails.Count - 1)
                            {
                                AddNewRow();
                                await Task.Delay(300);
                                await FocusById($"suppcode-{index + 1}");
                            }
                            else
                            {
                                await FocusById($"suppcode-{index + 1}");
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    ModelDialog?.DialogMethod(ex.Message);
                }
                finally
                {
                    _isSearching = false;
                }
            }
        }

        private bool _isDialogOpen = false;

        private async Task OpenSupplierDialog(SupplierOPEntryDetails row, string searchText = "")
        {
            if (_isDialogOpen) return;
            _isDialogOpen = true;
            try
            {
                await Task.Delay(100); // Prevent Enter key bounce
                var parameters = new DialogParameters();
                var options = new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true, CloseButton = true };
                if (!string.IsNullOrEmpty(searchText))
                {
                    parameters.Add("InitialSearch", searchText);
                }
                var dialog = Dialog!.Show<SupplierSearchDialog>(Localizer("SupplierSearch"), parameters, options);
                var result = await dialog.Result;

                if (!result.Canceled && result.Data is SupplierMaster selectedSupplier)
                {
                    SetSelectedSupplier(row, selectedSupplier);
                    var index = entryDto.EntryDetails.IndexOf(row);
                    await Task.Delay(100);
                    await FocusById($"purno-{index}");
                }
            }
            finally
            {
                await Task.Delay(500);
                _isDialogOpen = false;
            }
        }

        private void SetSelectedSupplier(SupplierOPEntryDetails row, SupplierMaster selectedSupplier)
        {
            row.FK_SupplierId = selectedSupplier.SupplierId;
            row.Supplier = selectedSupplier;
            row.SupplierCode = selectedSupplier.Code;
            row.SupplierName = selectedSupplier.Name;
            StateHasChanged();
        }

        private bool _isSaving = false;

        private async Task Save()
        {
            if (_isSaving) return;
            _isSaving = true;
            StateHasChanged();
            try
            {
                if (privilege is not null && !privilege.SaveData) { ModelDialog?.DialogMethod(Localizer("NoSavePrivilege")); _isSaving = false; return; }
                entryDto.EntryDate = date ?? DateTime.Now;

                // Assign BranchId from AppData
                entryDto.FK_BranchId = AppData.BranchId;
                //entryDto.EnteredBy = AppData.UserId?.ToString(); // Set User ID
                entryDto.FK_UserId = AppData.UserId ?? 0;

                // Calculate balances before save to be sure
                foreach (var d in entryDto.EntryDetails)
                {
                    d.BalanceAmt = (d.PurchaseAmt ?? 0) - (d.ReceivedAmt ?? 0) - (d.DiscountAmt ?? 0) - (d.ReturnedAmt ?? 0);

                    // Ensure mandatory fields
                    if (d.FK_SupplierId == null || d.FK_SupplierId == 0)
                    {
                        ModelDialog?.DialogMethod(Localizer("SupplierRequired"));
                        _isSaving = false;
                        StateHasChanged();
                        return;
                    }
                }

                if (Id > 0)
                {
                    ModelDialog?.DialogMethod(Localizer("ModificationNotAllowed"));
                }
                else
                {
                    var res = await api!.CreateSupplierOPEntry(entryDto);
                    if (res.Id > 0)
                    {
                        ModelDialog?.DialogMethod(Localizer("SavedSuccessfully"));
                        await Clear(true);

                        // Reload Grid
                        if (grid is not null) await grid.ReloadServerData();
                        StateHasChanged();
                    }
                    else
                    {
                        ModelDialog?.DialogMethod(res.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                ModelDialog?.DialogMethod(ex.Message);
            }
            finally
            {
                _isSaving = false;
                StateHasChanged();
            }
        }

        private async Task Clear(bool fromSave = false)
        {
            // Check if there is unsaved data (Dirty Check)
            bool isDirty = false;
            if (!fromSave && Id == 0 && entryDto.EntryDetails is not null)
            {
                if (entryDto.EntryDetails.Count > 1) isDirty = true;
                else if (entryDto.EntryDetails.Count == 1)
                {
                    var first = entryDto.EntryDetails.First();
                    if (first.FK_SupplierId > 0 || !string.IsNullOrWhiteSpace(first.PurchaseNo) || (first.PurchaseAmt ?? 0) > 0)
                        isDirty = true;
                }
            }

            if (isDirty)
            {
                bool? result = await Dialog!.ShowMessageBox(
                    Localizer("Warning"),
                    Localizer("UnsavedChangesWarning"),
                    yesText: Localizer("Yes"), cancelText: Localizer("No"));

                if (result != true) return;
            }

            await LoadNewDefaults();
            Id = 0;
        }

        private MudDataGrid<SupplierOPEntry>? grid;
        private string searchText = "";

        private async Task<GridData<SupplierOPEntry>> LoadServerData(GridState<SupplierOPEntry> state)
        {
            var pagination = new PaginationModel(
               page: state.Page + 1,
                pageSize: state.PageSize,
                sortColumn: state.SortDefinitions?.FirstOrDefault()?.SortBy,
                sortDirection: state.SortDefinitions?.FirstOrDefault()?.Descending == true ? "desc" : "asc",
                search: searchText
            );

            var result = await api!.GetSupplierOPEntryPDetails(pagination);

            return new GridData<SupplierOPEntry>
            {
                Items = result?.PSupplierOpEntries ?? new List<SupplierOPEntry>(),
                TotalItems = result?.TotalCount ?? 0
            };
        }

        private async Task OnSearch(string text)
        {
            searchText = text;
            if (grid is not null)
                await grid.ReloadServerData();
        }

        private async Task RowClicked(DataGridRowClickEventArgs<SupplierOPEntry> args)
        {

            try
            {
                _isLoading = true;
                Id = args.Item.SupplierOPEntryId;
                entryDto = await api!.GetSupplierOPEntryById(Id);
                date = entryDto.EntryDate;
                StateHasChanged();
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
        private async Task FocusById(string id)
        {
            try
            {
                // Robust focus: Try element itself, then try finding an input inside it (for MudBlazor wrappers)
                await js!.InvokeVoidAsync("eval", $"var e = document.getElementById('{id}'); if(e) {{ var inp = e.querySelector('input'); if(inp) {{ inp.focus(); if(inp.select) inp.select(); }} else {{ e.focus(); if(e.select) e.select(); }} }}");
            }
            catch { /* Ignore if element not found */ }
        }
    }
}
