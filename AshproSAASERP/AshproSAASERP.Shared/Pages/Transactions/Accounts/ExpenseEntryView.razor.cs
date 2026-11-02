using AshproSAASERP.Models.Transactions.Accounts;
using Microsoft.AspNetCore.Components.Web;

namespace AshproSAASERP.Shared.Pages.Transactions.Accounts
{
    public partial class ExpenseEntryView : ComponentBase, ITabAware
    {
        #region Variables
        [Inject] ISnackbar? SnackBar { get; set; }
        [Inject] NavigationManager? nav { get; set; }
        [Inject] IDialogService? Dialog { get; set; }
        [Inject] ModelService? ModelDialog { get; set; }
        [Inject] IJSRuntime? js { get; set; }
        [Inject] IViewService? api { get; set; }
        private bool IsAutoCode => PermanentSettingsData.AutoExpenseEntryNo;
        private ExpenseEntryDTO expenseDto = new() { ExpenseEntryDtls = new List<ExpenseEntryDtls>() };
        private WebUserPrivilege privilege = new();
        private DateTime? entryDate = DateTime.Now;
        private bool _isLoading = false;
        private bool _isDialogOpen = false;
        private List<ProjectMaster> projects = new();
        private List<SalesManMaster> salesmen = new();
        private List<VatMaster> vats = new();
        #endregion

        #region Properties
        [Parameter] public int TabId { get; set; }
        [Parameter] public int Id { get; set; }
        #endregion

        protected override async Task OnInitializedAsync()
        {
            try
            {
                // Load dropdown data
                await LoadDropdownData();
                if (AppData.privileges is not null)
                {
                    privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "ExpenseEntry") ?? new();
                }

                if (Id > 0)
                {
                    _isLoading = true;
                    var loaded = await api!.GetExpenseEntryById(Id);
                    if (loaded is not null)
                    {
                        expenseDto = loaded;
                        if (expenseDto.ExpenseEntryDtls == null) expenseDto.ExpenseEntryDtls = new List<ExpenseEntryDtls>();
                        entryDate = expenseDto.EntryDate;
                    }
                }
                else
                {
                    await LoadNewDefaults();
                }
            }
            catch (Exception ex)
            {
                SnackBar?.Add($"{Localizer("ErrorOccurred")}: {ex.Message}", Severity.Error);
            }
            finally
            {
                _isLoading = false;
            }
        }

        private async Task LoadDropdownData()
        {
            try
            {
                projects = await api!.GetAllProject() ?? new();
                salesmen = await api!.GetAllSalesMan() ?? new();
                vats = await api!.GetAllVat() ?? new();

                // Optimized: Do not load all accounts on init. Search used for defaults and selection.
            }
            catch (Exception ex)
            {
                SnackBar?.Add($"{Localizer("ErrorLoadingData")}: {ex.Message}", Severity.Warning);
            }
        }

        private async Task LoadNewDefaults()
        {
            _isLoading = true;
            try
            {
                expenseDto = new ExpenseEntryDTO();
                expenseDto.ExpenseEntryDtls = new List<ExpenseEntryDtls>(); // Initialize immediately before any await

                // Get next expense number
                if (IsAutoCode)
                {
                    await LoadAutoCodeIfEnabled();
                }
                else
                {
                    expenseDto.ExpenseNo = "EXP0001";
                }

                expenseDto.EntryDate = DateTime.Now;
                entryDate = DateTime.Now;
                // Default Payment Account (Cash) - Optimized with Type Check
                try
                {
                    // 1. Find "Cash" Account Type ID
                    var accountTypes = await api!.GetAllAccountTypes();
                    var cashType = accountTypes?.FirstOrDefault(x => x.AcTypeName.Contains("Cash", StringComparison.OrdinalIgnoreCase));

                    int? filterId = cashType?.AcTypeId;

                    // 2. Search for accounts (FilterId = AccountTypeId if found)
                    var pagination = new PaginationModel(1, 1, null, null, "Cash", AppData.BranchId) { FilterId = filterId };
                    var accountResult = await api!.GetAccountPDetails(pagination);

                    if (accountResult is not null && accountResult.PAccounts.Any())
                    {
                        var cashAcc = accountResult.PAccounts.FirstOrDefault();
                        if (cashAcc is not null)
                        {
                            expenseDto.FK_AccountId = cashAcc.AccountId;
                            expenseDto.PaymentAccountName = cashAcc.Name;
                        }
                    }
                }
                catch
                {
                    // Ignore if default fetch fails
                }

                // Default Supplier removed as per requirement


                expenseDto.ExpenseEntryDtls = new List<ExpenseEntryDtls>();
                AddNewRow();
            }
            catch (Exception ex)
            {
                ModelDialog?.DialogMethod($"Error initializing: {ex.Message}");
                expenseDto = new ExpenseEntryDTO();
                expenseDto.ExpenseNo = "EXP0001";
                expenseDto.EntryDate = DateTime.Now;
                entryDate = DateTime.Now;
                expenseDto.FK_BranchId = AppData.BranchId;
                expenseDto.FK_UserId = AppData.UserId;
                expenseDto.BillType = "Cash";

                try
                {
                    var pagination = new PaginationModel(1, 1, null, null, "Cash", AppData.BranchId) { FilterId = 0 };
                    var accountResult = await api!.GetAccountPDetails(pagination);
                    if (accountResult is not null && accountResult.PAccounts.Any())
                    {
                        var cashAcc = accountResult.PAccounts.FirstOrDefault();
                        if (cashAcc is not null)
                        {
                            expenseDto.FK_AccountId = cashAcc.AccountId;
                            expenseDto.PaymentAccountName = cashAcc.Name;
                        }
                    }
                }
                catch { }

                expenseDto.ExpenseEntryDtls = new List<ExpenseEntryDtls>();
                AddNewRow();
            }
            finally
            {
                _isLoading = false;
            }
        }

        private async void AddNewRow()
        {
            // ERP Standard: Validate last row before adding new
            if (expenseDto.ExpenseEntryDtls.Count > 0)
            {
                var lastRow = expenseDto.ExpenseEntryDtls.LastOrDefault();
                if (lastRow is not null && lastRow.FK_AccountId == 0)
                {
                    SnackBar?.Add(Localizer("PleaseSelectAccountFirst"), Severity.Warning);
                    return;
                }
            }

            expenseDto.ExpenseEntryDtls.Add(new ExpenseEntryDtls
            {
                FK_ExpenseEntryId = expenseDto.ExpenseId,
                Amount = 0,
                VatPer = 0,
                VatAmt = 0,
                IncAmt = 0,
                Narration = "",
                VatId = null,
                // Default Supplier from Header
                FK_SupplierId = expenseDto.FK_SupplierId,
                SupplierName = expenseDto.SupplierName
            });

            // Focus on the new row's first field after DOM update
            await Task.Delay(100);
            await js!.InvokeVoidAsync("focusElement", $"exp_acc-{expenseDto.ExpenseEntryDtls.Count - 1}");
        }

        private void CalculateVat(ExpenseEntryDtls row)
        {
            // If VAT is selected
            if (row.VatId > 0 && vats is not null)
            {
                var vat = vats.FirstOrDefault(x => x.VatId == row.VatId);
                if (vat is not null)
                {
                    row.VatPer = vat.Percentage ?? 0;
                    row.VatAmt = Math.Round(row.Amount * (row.VatPer / 100), 2);
                }
            }
            else
            {
                row.VatAmt = 0;
                row.VatPer = 0;
            }

            // Calculate inclusive amount
            row.IncAmt = row.Amount + row.VatAmt;
            CalculateTotals();
        }

        private async Task HandleKeyPress(KeyboardEventArgs e, ExpenseEntryDtls row, string fieldName)
        {
            if (e.Key == "Enter")
            {
                var index = expenseDto.ExpenseEntryDtls.IndexOf(row);

                // Define field order: exp_acc -> amount -> invno -> narr -> next row
                switch (fieldName)
                {
                    case "exp_acc":
                        // Open account dialog if account field is focused
                        await OpenAccountDialog(row);
                        break;

                    case "amount":
                        // From amount, move to InvoiceNo
                        await js!.InvokeVoidAsync("focusElement", $"invno-{index}");
                        break;

                    case "invno":
                        // From InvoiceNo, move to Narration
                        await js!.InvokeVoidAsync("focusElement", $"narr-{index}");
                        break;

                    case "narr":
                        // Move to next row or add new row
                        if (index == expenseDto.ExpenseEntryDtls.Count - 1)
                        {
                            AddNewRow();
                            // Focus handled inside AddNewRow
                        }
                        else
                        {
                            await js!.InvokeVoidAsync("focusElement", $"exp_acc-{index + 1}");
                        }
                        break;
                }
            }
        }

        private void RemoveRow(ExpenseEntryDtls item)
        {
            if (expenseDto.ExpenseEntryDtls.Count > 1)
            {
                expenseDto.ExpenseEntryDtls.Remove(item);
                CalculateTotals();
            }
            else
            {
                ModelDialog?.DialogMethod(Localizer("AtLeastOneLineRequired"));
            }
        }

        private void CalculateTotals()
        {
            expenseDto.Gross = expenseDto.ExpenseEntryDtls.Sum(x => x.Amount);
            expenseDto.VatTotal = expenseDto.ExpenseEntryDtls.Sum(x => x.VatAmt);
            expenseDto.NetTotal = expenseDto.Gross + expenseDto.VatTotal;
            StateHasChanged();
        }

        private async Task OpenPaymentAccountDialog()
        {
            if (_isDialogOpen) return;
            _isDialogOpen = true;
            try
            {
                var options = new DialogOptions
                {
                    CloseOnEscapeKey = true,
                    MaxWidth = MaxWidth.Large,
                    FullWidth = true
                };

                var parameters = new DialogParameters { ["FilterId"] = 0 }; // 0 for all accounts
                var dialog = Dialog!.Show<AccountSearchDialog>(Localizer("AccountSearch"), parameters, options);
                var result = await dialog.Result;

                if (!result.Canceled && result.Data is AccountMaster selectedAccount)
                {
                    expenseDto.FK_AccountId = selectedAccount.AccountId;
                    expenseDto.PaymentAccountName = selectedAccount.Name;
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                SnackBar?.Add($"{Localizer("ErrorOccurred")}: {ex.Message}", Severity.Error);
            }
            finally
            {
                _isDialogOpen = false;
            }
        }

        private async Task OpenAccountDialog(ExpenseEntryDtls row)
        {
            if (_isDialogOpen) return;
            _isDialogOpen = true;
            try
            {
                var options = new DialogOptions
                {
                    CloseOnEscapeKey = true,
                    MaxWidth = MaxWidth.Large,
                    FullWidth = true
                };

                var parameters = new DialogParameters { ["FilterId"] = row.FilterAccountTypeId };
                var dialog = Dialog!.Show<AccountSearchDialog>(Localizer("AccountSearch"), parameters, options);
                var result = await dialog.Result;

                if (!result.Canceled && result.Data is AccountMaster selectedAccount)
                {
                    row.FK_AccountId = selectedAccount.AccountId;
                    row.AccountCode = selectedAccount.Code;
                    row.AccountName = selectedAccount.Name;

                    StateHasChanged();

                    // Focus Amount Field
                    await Task.Delay(100);
                    var index = expenseDto.ExpenseEntryDtls.IndexOf(row);
                    await js!.InvokeVoidAsync("focusElement", $"amount-{index}");
                }
            }
            catch (Exception ex)
            {
                SnackBar?.Add($"{Localizer("ErrorOccurred")}: {ex.Message}", Severity.Error);
            }
            finally
            {
                _isDialogOpen = false;
            }
        }

        private async Task OpenSupplierDialog()
        {
            if (_isDialogOpen) return;
            _isDialogOpen = true;
            try
            {
                var options = new DialogOptions
                {
                    CloseOnEscapeKey = true,
                    MaxWidth = MaxWidth.Medium,
                    FullWidth = true
                };

                var dialog = Dialog!.Show<SupplierSearchDialog>(Localizer("SupplierSearch"), options);
                var result = await dialog.Result;

                if (!result.Canceled && result.Data is SupplierMaster selectedSupplier)
                {
                    expenseDto.FK_SupplierId = selectedSupplier.SupplierId;
                    expenseDto.SupplierName = selectedSupplier.Name;
                    PropagateHeaderSupplierToGrid();
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                SnackBar?.Add($"{Localizer("ErrorOccurred")}: {ex.Message}", Severity.Error);
            }
            finally
            {
                _isDialogOpen = false;
            }
        }

        private async Task OpenSupplierRowDialog(ExpenseEntryDtls row)
        {
            if (_isDialogOpen) return;
            _isDialogOpen = true;
            try
            {
                var options = new DialogOptions
                {
                    CloseOnEscapeKey = true,
                    MaxWidth = MaxWidth.Medium,
                    FullWidth = true
                };

                var dialog = Dialog!.Show<SupplierSearchDialog>(Localizer("SupplierSearch"), options);
                var result = await dialog.Result;

                if (!result.Canceled && result.Data is SupplierMaster selectedSupplier)
                {
                    row.FK_SupplierId = selectedSupplier.SupplierId;
                    row.SupplierName = selectedSupplier.Name;
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                SnackBar?.Add($"{Localizer("ErrorOccurred")}: {ex.Message}", Severity.Error);
            }
            finally
            {
                _isDialogOpen = false;
            }
        }


        private bool _isSaving = false;

        private async Task Save()
        {
            if (_isSaving) return;
            _isSaving = true;
            StateHasChanged();

            try
            {
                // ERP Standard: Only save rows with accounts selected
                expenseDto.ExpenseEntryDtls = expenseDto.ExpenseEntryDtls.Where(x => x.FK_AccountId > 0).ToList();

                if (privilege.SaveData == false)
                {
                    ModelDialog?.DialogMethod(Localizer("You don't have the privilege to save"));
                    return;
                }

                // Validate
                if (!ValidateEntry())
                {
                    _isSaving = false;
                    StateHasChanged();
                    return;
                }

                expenseDto.EntryDate = entryDate ?? DateTime.Now;
                expenseDto.FK_BranchId = AppData.BranchId;
                expenseDto.FK_UserId = AppData.UserId;

                var res = await api!.CreateExpenseEntry(expenseDto);
                if (res.Id > 0)
                {
                    ModelDialog?.DialogMethod(Localizer("SavedSuccessfully"));
                    await Clear(true);
                    if (grid is not null) await grid.ReloadServerData();
                    StateHasChanged();
                }
                else
                {
                    ModelDialog?.DialogMethod(Localizer(res.Message));
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

        private async Task Update()
        {
            if (_isSaving) return;
            _isSaving = true;
            StateHasChanged();

            try
            {
                // ERP Standard: Only save rows with accounts selected
                expenseDto.ExpenseEntryDtls = expenseDto.ExpenseEntryDtls.Where(x => x.FK_AccountId > 0).ToList();

                if (privilege.EditData == false)
                {
                    ModelDialog?.DialogMethod(Localizer("You don't have the privilege to edit"));
                    return;
                }

                // Validate
                if (!ValidateEntry())
                {
                    _isSaving = false;
                    StateHasChanged();
                    return;
                }

                expenseDto.EntryDate = entryDate ?? DateTime.Now;

                var res = await api!.UpdateExpenseEntry(expenseDto);
                if (res.Id > 0)
                {
                    ModelDialog?.DialogMethod(Localizer("UpdatedSuccessfully"));
                    await Clear(true);
                    if (grid is not null) await grid.ReloadServerData();
                    StateHasChanged();
                }
                else
                {
                    ModelDialog?.DialogMethod(Localizer(res.Message));
                }
            }
            finally
            {
                _isSaving = false;
                StateHasChanged();
            }
        }

        private async Task Delete()
        {
            try
            {
                bool? confirm = await Dialog!.ShowMessageBox(
                    Localizer("Warning"),
                    Localizer("DeleteConfirmation"),
                    yesText: Localizer("Yes"), cancelText: Localizer("No"));

                if (confirm == true)
                {
                    if (privilege.DeleteData == false)
                    {
                        ModelDialog?.DialogMethod(Localizer("You don't have the privilege to delete"));
                        return;
                    }
                    var res = await api!.DeleteExpenseEntry(Id);
                    if (res.Id > 0)
                    {
                        ModelDialog?.DialogMethod(Localizer("DeletedSuccessfully"));
                        await Clear(true);
                        if (grid is not null) await grid.ReloadServerData();
                    }
                    else
                    {
                        ModelDialog?.DialogMethod(Localizer(res.Message));
                    }
                }
            }
            catch (Exception ex)
            {
                ModelDialog?.DialogMethod(ex.Message);
            }
        }

        private bool ValidateEntry()
        {
            // Remove empty rows
            expenseDto.ExpenseEntryDtls.RemoveAll(x => x.FK_AccountId == 0 && x.Amount == 0);

            // Check minimum 1 line
            if (expenseDto.ExpenseEntryDtls.Count < 1)
            {
                ModelDialog?.DialogMethod(Localizer("AtLeastOneLineRequired"));
                return false;
            }

            // Check payment account
            if (expenseDto.FK_AccountId == 0)
            {
                ModelDialog?.DialogMethod(Localizer("PaymentAccountRequired"));
                return false;
            }

            // Check each line has account and amount
            foreach (var line in expenseDto.ExpenseEntryDtls)
            {
                if (line.FK_AccountId == 0)
                {
                    ModelDialog?.DialogMethod(Localizer("AccountRequired"));
                    return false;
                }

                if (line.Amount == 0)
                {
                    ModelDialog?.DialogMethod(Localizer("AmountRequired"));
                    return false;
                }
            }

            // Calculate totals
            CalculateTotals();

            return true;
        }

        private async Task Clear(bool fromSave = false)
        {
            // Check for unsaved changes
            bool isDirty = false;
            if (!fromSave && Id == 0 && expenseDto.ExpenseEntryDtls is not null)
            {
                isDirty = expenseDto.ExpenseEntryDtls.Any(x => x.FK_AccountId > 0 || x.Amount > 0);
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

        private MudDataGrid<ExpenseEntryHdr>? grid;
        private string searchText = "";

        private async Task<GridData<ExpenseEntryHdr>> LoadServerData(GridState<ExpenseEntryHdr> state)
        {
            try
            {
                // PaginationModel is a record with positional parameters
                var pagination = new PaginationModel(
                    page: state.Page + 1, // MudBlazor is 0-indexed, API expects 1-indexed
                    pageSize: state.PageSize,
                    sortColumn: null,
                    sortDirection: null,
                    search: searchText,
                    BranchId: AppData.BranchId
                )
                { FilterId = null };

                var result = await api!.GetExpenseEntryPDetails(pagination);

                // Null safety check
                if (result == null || result.PTransactions == null)
                {
                    return new GridData<ExpenseEntryHdr>
                    {
                        Items = new List<ExpenseEntryHdr>(),
                        TotalItems = 0
                    };
                }

                // Convert ExMasters to ExpenseEntryHdr for grid display
                var items = result.PTransactions.Select(x => new ExpenseEntryHdr
                {
                    ExpenseEntryId = x.Id,
                    ExpenseNo = x.Code ?? "",
                    NetTotal = x.Amount,
                    EntryDate = DateTime.TryParse(x.Description, out var date) ? date : DateTime.Now,
                    Narration = "" // Narration not included in pagination result
                }).ToList();

                return new GridData<ExpenseEntryHdr>
                {
                    Items = items,
                    TotalItems = result.TotalCount
                };
            }
            catch (Exception ex)
            {
                SnackBar?.Add($"{Localizer("ErrorOccurred")}: {ex.Message}", Severity.Error);
                return new GridData<ExpenseEntryHdr>
                {
                    Items = new List<ExpenseEntryHdr>(),
                    TotalItems = 0
                };
            }
        }

        private async Task OnSearch(string text)
        {
            searchText = text;
            if (grid is not null)
                await grid.ReloadServerData();
        }

        private async Task RowClicked(DataGridRowClickEventArgs<ExpenseEntryHdr> args)
        {

            try
            {
                _isLoading = true;
                Id = args.Item.ExpenseEntryId;
                var loaded = await api!.GetExpenseEntryById(Id);
                if (loaded is not null)
                {
                    expenseDto = loaded;
                    entryDate = expenseDto.EntryDate;
                }
                StateHasChanged();
            }
            catch (Exception ex)
            {
                SnackBar?.Add($"{Localizer("ErrorOccurred")}: {ex.Message}", Severity.Error);
            }
            finally
            {
                _isLoading = false;
            }
        }

        private async Task LoadAutoCodeIfEnabled()
        {
            if (IsAutoCode && expenseDto.ExpenseId == 0)
            {
                expenseDto.ExpenseNo = await api!.GetNextExpenseNo();
            }
        }

        private void OnHeaderSupplierChanged(string? value)
        {
            expenseDto.SupplierName = value;
            if (string.IsNullOrEmpty(value))
            {
                ClearHeaderSupplier();
            }
        }

        private void OnGridSupplierChanged(string? value, ExpenseEntryDtls row)
        {
            row.SupplierName = value;
            if (string.IsNullOrEmpty(value))
            {
                ClearGridSupplier(row);
            }
        }

        private void ClearHeaderSupplier()
        {
            expenseDto.FK_SupplierId = null;
            expenseDto.SupplierName = string.Empty;
            PropagateHeaderSupplierToGrid();
        }

        private void ClearGridSupplier(ExpenseEntryDtls row)
        {
            row.FK_SupplierId = null;
            row.SupplierName = string.Empty;
        }

        private void PropagateHeaderSupplierToGrid()
        {
            if (expenseDto.ExpenseEntryDtls == null) return;

            foreach (var row in expenseDto.ExpenseEntryDtls)
            {
                // Only propagate to rows where Account is NOT selected
                // "if account is not selected... else if account selected then no need apply"
                if (row.FK_AccountId == 0)
                {
                    row.FK_SupplierId = expenseDto.FK_SupplierId;
                    row.SupplierName = expenseDto.SupplierName;
                }
            }
        }
    }
}
