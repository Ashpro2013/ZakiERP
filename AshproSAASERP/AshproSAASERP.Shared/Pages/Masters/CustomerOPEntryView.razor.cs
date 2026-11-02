using Microsoft.AspNetCore.Components.Web;

namespace AshproSAASERP.Shared.Pages.Masters
{
    public partial class CustomerOPEntryView : ComponentBase, ITabAware
    {
        [Parameter] public int TabId { get; set; }
        [Inject] IViewService? api { get; set; }
        [Inject] ISnackbar? SnackBar { get; set; }
        [Inject] NavigationManager? nav { get; set; }
        [Inject] IDialogService? Dialog { get; set; }
        [Inject] ModelService? ModelDialog { get; set; }
        [Inject] IJSRuntime? js { get; set; }

        [Parameter] public int Id { get; set; }

        private CustomerOPEntryDTO entryDto = new();
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

                if (AppData.privileges is not null)
                    privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "CustomerOpeningEntry");

                if (Id > 0)
                {
                    _isLoading = true;
                    entryDto = await api.GetCustomerOPEntryById(Id);
                    date = entryDto.EntryDate;
                }
                // Removed explicit 'else' call to LoadNewDefaults here to avoid double loading if not needed.
                // But logic requires default state if Id=0.
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

        private async Task LoadNewDefaults()
        {
            _isLoading = true;
            try
            {
                entryDto = new CustomerOPEntryDTO();
                entryDto.DocNo = await api!.GetNewCustomerOPEntryDocNo();
                date = DateTime.Now;
                entryDto.EntryDetails = new List<CustomerOPEntryDetails>();
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
                await FocusById("custcode-0");
            }
        }

        private void AddNewRow()
        {
            if (entryDto.EntryDetails.Count > 0)
            {
                var lastItem = entryDto.EntryDetails.Last();
                if (!CheckRowValidation(lastItem)) return;
            }

            entryDto.EntryDetails.Add(new CustomerOPEntryDetails
            {
                InvoiceDate = DateTime.Now,
                InvoiceAmt = 0,
                ReceivedAmt = 0,
                DiscountAmt = 0,
                ReturnedAmt = 0,
                BalanceAmt = 0,
                FK_BranchId = AppData.BranchId
            });
        }

        private bool CheckRowValidation(CustomerOPEntryDetails item)
        {
            if (item.FK_CustomerId is null or 0)
            {
                // Check if user manually typed something without selecting
                if (!string.IsNullOrWhiteSpace(item.CustomerCode) || !string.IsNullOrWhiteSpace(item.CustomerName))
                {
                    ModelDialog?.DialogMethod(Localizer("PleaseSelectCustomerViaSearch"));
                    return false;
                }

                ModelDialog?.DialogMethod(Localizer("CustomerRequired"));
                return false;
            }
            if (string.IsNullOrWhiteSpace(item.InvoiceNo))
            {
                ModelDialog?.DialogMethod(Localizer("InvoiceNoRequired"));
                return false;
            }
            if ((item.InvoiceAmt ?? 0) <= 0)
            {
                ModelDialog?.DialogMethod(Localizer("AmountRequired"));
                return false;
            }
            return true;
        }

        private void RemoveRow(CustomerOPEntryDetails item)
        {
            entryDto.EntryDetails.Remove(item);
        }

        private void ItemHasBeenCommitted(object element)
        {
            if (element is CustomerOPEntryDetails detail)
            {
                CalculateBalance(detail);
            }
        }

        private void CalculateBalance(CustomerOPEntryDetails detail)
        {
            detail.BalanceAmt = (detail.InvoiceAmt ?? 0) - (detail.ReceivedAmt ?? 0) - (detail.DiscountAmt ?? 0) - (detail.ReturnedAmt ?? 0);
            StateHasChanged();
        }

        private async Task HandleFieldEnter(KeyboardEventArgs e, CustomerOPEntryDetails detail, string fieldName)
        {
            var index = entryDto.EntryDetails.IndexOf(detail);

            if (e.Key == "ArrowUp")
            {
                if (index > 0)
                {
                    await FocusById($"{fieldName}-{index - 1}");
                }
            }
            else if (e.Key == "ArrowDown")
            {
                if (index < entryDto.EntryDetails.Count - 1)
                {
                    await FocusById($"{fieldName}-{index + 1}");
                }
            }
            else if (e.Key == "Enter" || e.Key == "NumpadEnter")
            {
                if (_isSearching || _isDialogOpen) return;
                _isSearching = true;
                try
                {
                    switch (fieldName)
                    {
                        case "custcode":
                        case "custname":
                            // Smart Search Logic
                            string searchQuery = fieldName == "custcode" ? detail.CustomerCode : detail.CustomerName;

                            // Optimization: If we already have a selected customer and the text matches, don't re-search
                            // Check for non-null and non-zero ID to ensure it's a valid selection
                            if ((detail.FK_CustomerId ?? 0) != 0 && (
                                (fieldName == "custcode" && detail.CustomerCode == searchQuery) ||
                                (fieldName == "custname" && detail.CustomerName == searchQuery)
                            ))
                            {
                                await FocusById($"invno-{index}");
                                return;
                            }

                            if (!string.IsNullOrWhiteSpace(searchQuery))
                            {
                                var searchCol = fieldName == "custcode" ? "Code" : "Name";
                                // Fetch a few more items to ensure exact match is captured if sort order varies
                                var pagination = new PaginationModel(1, 2, searchCol, "asc", searchQuery);
                                var result = await api!.GetCustomerPDetails(pagination);

                                if (result.TotalCount == 1 && result.PCustomers.Count == 1)
                                {
                                    SetSelectedCustomer(detail, result.PCustomers[0]);
                                    StateHasChanged();
                                    await FocusById($"invno-{index}");
                                }
                                else
                                {
                                    await OpenCustomerDialog(detail, searchQuery);
                                }
                            }
                            else
                            {
                                // Empty text -> Open Dialog
                                await OpenCustomerDialog(detail);
                            }
                            break;

                        case "invno":
                            await Task.Delay(100);
                            await FocusById($"invdate-{index}");
                            break;
                        case "invamt":
                            await Task.Delay(100);
                            await FocusById($"recamt-{index}");
                            break;
                        case "recamt":
                            await Task.Delay(100);
                            await FocusById($"discamt-{index}");
                            break;
                        case "discamt":
                            await Task.Delay(100);
                            await FocusById($"retamt-{index}");
                            break;
                        case "retamt":
                            await Task.Delay(100);
                            if (index == entryDto.EntryDetails.Count - 1)
                            {
                                AddNewRow();
                                await Task.Delay(100);
                                await FocusById($"custcode-{index + 1}");
                            }
                            else
                            {
                                await FocusById($"custcode-{index + 1}");
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

        private async Task OpenCustomerDialog(CustomerOPEntryDetails row, string searchText = "")
        {
            if (_isDialogOpen) return;
            _isDialogOpen = true;
            try
            {
                var parameters = new DialogParameters();
                var options = new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true, CloseButton = true };
                if (!string.IsNullOrEmpty(searchText))
                {
                    parameters.Add("InitialSearch", searchText);
                }
                var dialog = Dialog!.Show<CustomerSearchDialog>(Localizer("CustomerSearch"), parameters, options);
                var result = await dialog.Result;

                if (!result.Canceled && result.Data is CustomerMaster selectedCustomer)
                {
                    SetSelectedCustomer(row, selectedCustomer);
                    var index = entryDto.EntryDetails.IndexOf(row);
                    await Task.Delay(100);
                    await FocusById($"invno-{index}");
                }
            }
            finally
            {
                await Task.Delay(500); // Prevent 'Enter' bounce
                _isDialogOpen = false;
            }
        }

        private void SetSelectedCustomer(CustomerOPEntryDetails row, CustomerMaster selectedCustomer)
        {
            row.FK_CustomerId = selectedCustomer.CustomerId;
            row.Customer = selectedCustomer;
            row.CustomerCode = selectedCustomer.Code; // Backing field update or property update
            row.CustomerName = selectedCustomer.Name;
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
                    d.BalanceAmt = (d.InvoiceAmt ?? 0) - (d.ReceivedAmt ?? 0) - (d.DiscountAmt ?? 0) - (d.ReturnedAmt ?? 0);

                    // Ensure mandatory fields
                    if (d.FK_CustomerId == null || d.FK_CustomerId == 0)
                    {
                        ModelDialog?.DialogMethod(Localizer("CustomerRequired"));
                        return;
                    }
                }

                if (Id > 0)
                {
                    ModelDialog?.DialogMethod(Localizer("ModificationNotAllowed")); // Should not happen due to UI hide, but safeguard
                }
                else
                {
                    var res = await api!.CreateCustomerOPEntry(entryDto);
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
                snackbar.Add(ex.Message, Severity.Error);
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
            // Logic: If Id is 0 (New Mode) AND (More than 1 row OR First row has data)
            // We assume 'Dirty' if CustomerId is selected or Amount > 0
            bool isDirty = false;
            if (!fromSave && Id == 0 && entryDto.EntryDetails is not null)
            {
                if (entryDto.EntryDetails.Count > 1) isDirty = true;
                else if (entryDto.EntryDetails.Count == 1)
                {
                    var first = entryDto.EntryDetails.First();
                    if (first.FK_CustomerId > 0 || !string.IsNullOrWhiteSpace(first.InvoiceNo) || (first.InvoiceAmt ?? 0) > 0)
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

        private MudDataGrid<CustomerOPEntry>? grid;
        private string searchText = "";

        private async Task<GridData<CustomerOPEntry>> LoadServerData(GridState<CustomerOPEntry> state)
        {
            var pagination = new PaginationModel(
               page: state.Page + 1,
                pageSize: state.PageSize,
                sortColumn: state.SortDefinitions?.FirstOrDefault()?.SortBy,
                sortDirection: state.SortDefinitions?.FirstOrDefault()?.Descending == true ? "desc" : "asc",
                search: searchText
            );

            var result = await api!.GetCustomerOPEntryPDetails(pagination);

            return new GridData<CustomerOPEntry>
            {
                Items = result?.PCustomerOpEntries ?? new List<CustomerOPEntry>(),
                TotalItems = result?.TotalCount ?? 0
            };
        }

        private async Task OnSearch(string text)
        {
            searchText = text;
            if (grid is not null)
                await grid.ReloadServerData();
        }

        private async Task RowClicked(DataGridRowClickEventArgs<CustomerOPEntry> args)
        {

            try
            {
                _isLoading = true;
                Id = args.Item.CustomerOPEntryId;
                entryDto = await api!.GetCustomerOPEntryById(Id);
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
