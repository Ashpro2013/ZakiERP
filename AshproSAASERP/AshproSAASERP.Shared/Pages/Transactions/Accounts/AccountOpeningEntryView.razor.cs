using Microsoft.AspNetCore.Components.Web;

namespace AshproSAASERP.Shared.Pages.Transactions.Accounts;

public partial class AccountOpeningEntryView : ITabAware
{
    [Parameter] public int TabId { get; set; }
    [Inject] IViewService ViewService { get; set; } = default!;
    [Inject] ISnackbar Snackbar { get; set; } = default!;
    [Inject] IJSRuntime js { get; set; } = default!;
    [Inject] IDialogService DialogService { get; set; } = default!;
    [Inject] AppData AppData { get; set; } = default!;

    private AccountOpeningEntryDTO entryDto = new();
    private WebUserPrivilege privilege = new();
    private DateTime? entryDate = DateTime.Now;
    private bool _isLoading = false;
    private bool _isDialogOpen = false;
    private MudDataGrid<AccountOpeningEntryHdr>? grid;
    private string searchText = "";
    private AccountMaster? selectedCapitalAccount;
    private List<AccountMaster> _allAccountsCache = new(); // Cache for search

    // Quick Add Fields
    private AccountMaster? quickAddAccount;
    private decimal? quickAddDebit;
    private decimal? quickAddCredit;


    protected override async Task OnInitializedAsync()
    {
        await LoadNew();
        if (AppData.privileges is not null)
        {
            privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "AccountOpeningEntry") ?? new();
        }
    }

    private async Task LoadNew()
    {
        var branchId = AppData.BranchId;
        entryDto = new AccountOpeningEntryDTO();
        entryDto.Header.FK_BranchId = branchId;

        if (entryDto.Header.FK_BranchId > 0)
        {
            var next = await ViewService.GetAccountOpeningEntryNewDocNo(entryDto.Header.FK_BranchId ?? 0);
            entryDto.Header.VoucherNo = next;
        }

        entryDto.Header.EntryDate = DateTime.Now;
        entryDate = entryDto.Header.EntryDate;

        // Load ALL Accounts (FilterId=3 for General/Acc)
        _isLoading = true;
        try
        {
            // 1. Fetch All Accounts
            var pagination = new PaginationModel(
                page: 1,
                pageSize: 10000, // Load all for inputs
                sortColumn: "Code",
                sortDirection: "asc",
                search: "",
                BranchId: AppData.BranchId
            )
            { FilterId = 3 };

            var accountResult = await ViewService.GetAccountPDetails(pagination);

            // 2. Fetch Existing Balances
            var existingBalances = await ViewService.GetExistingOpeningBalances(AppData.BranchId);

            if (accountResult.PAccounts is not null)
            {
                entryDto.Details = accountResult.PAccounts.Select(acc =>
                {
                    var existing = existingBalances.FirstOrDefault(x => x.FK_AccountId == acc.AccountId);
                    return new AccountOpeningEntryDtls
                    {
                        FK_AccountId = acc.AccountId,
                        Account = new AccountMaster
                        {
                            AccountId = acc.AccountId,
                            Name = acc.Name,
                            Code = acc.Code
                        },
                        Debit = existing?.Debit ?? 0,
                        Credit = existing?.Credit ?? 0
                    };
                }).ToList();

                // Populate Cache for Autocomplete
                _allAccountsCache = entryDto.Details.Select(x => x.Account).ToList();
                CalculateTotals();
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error loading data: {ex.Message}", Severity.Error);
        }
        finally
        {
            _isLoading = false;
        }

        StateHasChanged();
        await FocusFirst();
    }

    private async Task OpenCapitalAccountDialog()
    {
        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Large, FullWidth = true };
        var parameters = new DialogParameters { ["FilterId"] = 3 }; // FilterId 3 = 'Acc' flag
        var dialog = DialogService.Show<AccountSearchDialog>(Localizer("Select Capital Account"), parameters, options);
        var result = await dialog.Result;

        if (!result.Canceled && result.Data is AccountMaster selected)
        {
            selectedCapitalAccount = selected;
        }
    }

    private async Task OpenQuickAddAccountDialog()
    {
        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Large, FullWidth = true };
        var parameters = new DialogParameters { ["FilterId"] = 3 }; // FilterId 3 = 'Acc' flag
        var dialog = DialogService.Show<AccountSearchDialog>(Localizer("Select Account"), parameters, options);
        var result = await dialog.Result;

        if (!result.Canceled && result.Data is AccountMaster selected)
        {
            quickAddAccount = selected;
        }
    }

    private void AdjustCapitalDifference()
    {
        if (selectedCapitalAccount == null) return;

        // Find the row for the selected Capital Account
        var capitalRow = entryDto.Details.FirstOrDefault(x => x.FK_AccountId == selectedCapitalAccount.AccountId);

        if (capitalRow is not null)
        {
            // Use Delta Logic: Adjust the existing account balance by the Global Difference
            // NewBalance = OldBalance - GlobalDifference
            // Example: Account=1000Dr, Diff=1000Dr. New = 1000 - 1000 = 0.
            // Example: Account=1000Dr, Diff=2000Dr. New = 1000 - 2000 = -1000 (1000Cr).

            var totalDebit = entryDto.Header.DebitTotal ?? 0;
            var totalCredit = entryDto.Header.CreditTotal ?? 0;
            var globalImbalance = totalDebit - totalCredit;

            if (Math.Abs(globalImbalance) < 0.001m)
            {
                Snackbar.Add(Localizer("Entries are already balanced!"), Severity.Info);
                return;
            }

            var currentNet = (capitalRow.Debit ?? 0) - (capitalRow.Credit ?? 0);
            var newNet = currentNet - globalImbalance;

            if (newNet > 0)
            {
                capitalRow.Debit = newNet;
                capitalRow.Credit = 0;
            }
            else
            {
                capitalRow.Debit = 0;
                capitalRow.Credit = Math.Abs(newNet);
            }

            capitalRow.IsChanged = true; // Mark as changed
            CalculateTotals();
            Snackbar.Add(Localizer("Difference adjusted to Capital Account"), Severity.Success);
        }
        else
        {
            Snackbar.Add(Localizer("Selected Capital Account not found in the list"), Severity.Warning);
        }
    }



    private void QuickAddEntry()
    {
        if (quickAddAccount == null)
        {
            Snackbar.Add(Localizer("Please select an account"), Severity.Warning);
            return;
        }

        if ((quickAddDebit ?? 0) == 0 && (quickAddCredit ?? 0) == 0)
        {
            Snackbar.Add(Localizer("Please enter a Debit or Credit amount"), Severity.Warning);
            return;
        }

        var match = entryDto.Details.FirstOrDefault(x => x.FK_AccountId == quickAddAccount.AccountId);
        if (match is not null)
        {
            match.Debit = quickAddDebit ?? 0;
            match.Credit = quickAddCredit ?? 0;
            match.IsChanged = true; // Mark as changed
            CalculateTotals();

            // Clear inputs
            quickAddAccount = null;
            quickAddDebit = null;
            quickAddCredit = null;
            Snackbar.Add(Localizer("Entry Updated"), Severity.Success);
        }
        else
        {
            Snackbar.Add(Localizer("Account not found in list"), Severity.Error);
        }
    }


    private void CalculateTotals()
    {
        entryDto.Header.DebitTotal = entryDto.Details.Sum(x => x.Debit ?? 0);
        entryDto.Header.CreditTotal = entryDto.Details.Sum(x => x.Credit ?? 0);
        StateHasChanged();
    }

    private async Task OpenAccountDialog(AccountOpeningEntryDtls row)
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

            var parameters = new DialogParameters();
            var dialog = DialogService.Show<AccountSearchDialog>(Localizer("SelectAccount"), parameters, options);
            var result = await dialog.Result;

            if (!result.Canceled && result.Data is AccountMaster selectedAccount)
            {
                row.FK_AccountId = selectedAccount.AccountId;
                row.Account = selectedAccount;
                row.IsChanged = true; // Mark as changed if account changes (not typical in bulk mode, but safe)
                StateHasChanged();
            }
        }
        finally
        {
            _isDialogOpen = false;
        }
    }

    private async Task HandleKeyPress(KeyboardEventArgs e, AccountOpeningEntryDtls row, string fieldType)
    {
        if (e.Key == "Enter" || e.Key == "NumpadEnter")
        {
            var index = entryDto.Details.IndexOf(row);

            switch (fieldType)
            {
                case "debit":
                    // Fetch the actual value from DOM because binding might not have happened yet (Immediate=false)
                    string rawVal = "0";
                    try
                    {
                        rawVal = await js.InvokeAsync<string>("getElementValue", $"debit-{index}");
                    }
                    catch { /* ignore */ }

                    decimal val = 0;
                    decimal.TryParse(rawVal, out val);

                    // If Debit is entered (> 0), jump to next row's Debit
                    // If Debit is 0 (unchanged/skipped), force focus to Credit on same row
                    if (val > 0)
                    {
                        // Move to next row
                        if (index < entryDto.Details.Count - 1)
                        {
                            await FocusElement($"debit-{index + 1}");
                        }
                    }
                    else
                    {
                        await FocusElement($"credit-{index}");
                    }
                    break;

                case "credit":
                    // From Credit, always move to Next Row Debit
                    if (index < entryDto.Details.Count - 1)
                    {
                        await FocusElement($"debit-{index + 1}");
                    }
                    break;
            }
        }
    }

    private async Task FocusFirst()
    {
        try
        {
            await Task.Delay(200); // Wait for grid render
            if (entryDto.Details.Count > 0)
            {
                await FocusElement("debit-0");
            }
        }
        catch { /* ignore */ }
    }

    private void OnQuickDebitChanged(decimal value)
    {
        quickAddDebit = value;
        if (value > 0)
        {
            quickAddCredit = 0;
        }
    }

    private void OnQuickCreditChanged(decimal value)
    {
        quickAddCredit = value;
        if (value > 0)
        {
            quickAddDebit = 0;
        }
    }

    private void OnDebitChanged(AccountOpeningEntryDtls item, decimal value)
    {
        bool wasCredit = (item.Credit ?? 0) > 0;
        item.Debit = value;
        item.IsChanged = true; // Mark as changed

        if (value > 0)
        {
            if (wasCredit)
            {
                item.Credit = 0;
                Snackbar.Add($"{Localizer("Credit Cleared for")} {item.Account.Name}", Severity.Info);
            }
        }
        CalculateTotals();
    }

    private void OnCreditChanged(AccountOpeningEntryDtls item, decimal value)
    {
        bool wasDebit = (item.Debit ?? 0) > 0;
        item.Credit = value;
        item.IsChanged = true; // Mark as changed

        if (value > 0)
        {
            if (wasDebit)
            {
                item.Debit = 0;
                Snackbar.Add($"{Localizer("Debit Cleared for")} {item.Account.Name}", Severity.Info);
            }
        }
        CalculateTotals();
    }

    private async Task FocusElement(string id)
    {
        try
        {
            await js.InvokeVoidAsync("focusElement", id);
        }
        catch (Exception)
        {
            // Ignore focus errors
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
            if (privilege.SaveData == false)
            {
                Snackbar.Add(Localizer("You don't have the privilege to save"), Severity.Error);
                return;
            }
            // Bulk Mode: No need to check if accounts are selected, they are pre-loaded.
            // We filter out 0 entries.



            _isLoading = true;

            // Create a SAFE copy for saving (Projecting to new objects)
            // Optimization: Only send CHANGED rows.
            var detailsToSave = entryDto.Details
                .Where(x => x.IsChanged)
                .Select(x => new AccountOpeningEntryDtls
                {
                    AccountOpeningEntryDetailsId = x.AccountOpeningEntryDetailsId,
                    FK_AccountOpeningEntryId = x.FK_AccountOpeningEntryId,
                    FK_AccountId = x.FK_AccountId,
                    Debit = x.Debit,
                    Credit = x.Credit,
                    Narration = x.Narration,
                    IsDeleted = x.IsDeleted,
                    Account = null // Explicitly null for server validation
                })
                .ToList();

            if (!detailsToSave.Any())
            {
                Snackbar.Add(Localizer("No changes to save"), Severity.Info);
                _isLoading = false;
                return;
            }

            var saveDto = new AccountOpeningEntryDTO
            {
                Header = new AccountOpeningEntryHdr
                {
                    AccountOpeningEntryId = entryDto.Header.AccountOpeningEntryId,
                    VoucherNo = entryDto.Header.VoucherNo,
                    EntryDate = entryDate,
                    VoucherDate = entryDate,
                    FK_BranchId = entryDto.Header.FK_BranchId,
                    FK_UserId = AppData.UserId,
                    FK_FinancialYearId = AppData.FYearId,
                    DebitTotal = entryDto.Header.DebitTotal,
                    CreditTotal = entryDto.Header.CreditTotal,
                    Remarks = entryDto.Header.Remarks
                },
                Details = detailsToSave
            };

            var res = await ViewService.CreateAccountOpeningEntry(saveDto);
            _isLoading = false;

            if (res is not null && string.IsNullOrEmpty(res.Message))
            {
                Snackbar.Add(Localizer("Saved Successfully"), Severity.Success);
                await LoadNew();
                if (grid is not null)
                {
                    await grid.ReloadServerData();
                }
            }
            else
            {
                Snackbar.Add(res?.Message ?? Localizer("Error Saving"), Severity.Error);
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

    private async Task Update()
    {
        if (_isSaving) return;
        _isSaving = true;
        StateHasChanged();

        try
        {
            if (privilege.EditData == false)
            {
                Snackbar.Add(Localizer("You don't have the privilege to edit"), Severity.Error);
                return;
            }
            // Check if any rows have values
            /*
            if (!entryDto.Details.Any(x => (x.Debit ?? 0) > 0 || (x.Credit ?? 0) > 0))
            {
                 // If all are 0, it's effectively deleting all entries.
            }
            */

            _isLoading = true;

            // Project to new objects for API call
            // Optimization: Only send CHANGED rows.
            var detailsToSave = entryDto.Details
                .Where(x => x.IsChanged)
                .Select(x => new AccountOpeningEntryDtls
                {
                    AccountOpeningEntryDetailsId = x.AccountOpeningEntryDetailsId,
                    FK_AccountOpeningEntryId = x.FK_AccountOpeningEntryId,
                    FK_AccountId = x.FK_AccountId,
                    Debit = x.Debit,
                    Credit = x.Credit,
                    Narration = x.Narration,
                    IsDeleted = x.IsDeleted,
                    Account = null
                })
                .ToList();

            if (!detailsToSave.Any())
            {
                Snackbar.Add(Localizer("No changes to update"), Severity.Info);
                _isLoading = false;
                return;
            }

            var updateDto = new AccountOpeningEntryDTO
            {
                Header = new AccountOpeningEntryHdr
                {
                    AccountOpeningEntryId = entryDto.Header.AccountOpeningEntryId,
                    VoucherNo = entryDto.Header.VoucherNo,
                    EntryDate = entryDate,
                    VoucherDate = entryDate,
                    FK_BranchId = entryDto.Header.FK_BranchId,
                    FK_UserId = AppData.UserId,
                    FK_FinancialYearId = AppData.FYearId,
                    DebitTotal = entryDto.Header.DebitTotal,
                    CreditTotal = entryDto.Header.CreditTotal,
                    Remarks = entryDto.Header.Remarks
                },
                Details = detailsToSave
            };

            var res = await ViewService.UpdateAccountOpeningEntry(updateDto);
            _isLoading = false;

            if (res is not null && string.IsNullOrEmpty(res.Message))
            {
                Snackbar.Add(Localizer("Updated Successfully"), Severity.Success);
                await LoadNew();
                if (grid is not null)
                {
                    await grid.ReloadServerData();
                }
            }
            else
            {
                Snackbar.Add(res?.Message ?? Localizer("Error Updating"), Severity.Error);
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
        bool? confirm = await DialogService.ShowMessageBox(
            Localizer("Confirm Delete"),
            Localizer("Are you sure you want to delete this entry?"),
            yesText: Localizer("Delete"), cancelText: Localizer("Cancel"));

        if (confirm == true)
        {
            if (privilege.DeleteData == false)
            {
                Snackbar.Add(Localizer("You don't have the privilege to delete"), Severity.Error);
                return;
            }
            _isLoading = true;
            var res = await ViewService.DeleteAccountOpeningEntry(entryDto.Header.AccountOpeningEntryId);
            _isLoading = false;

            if (res is not null && string.IsNullOrEmpty(res.Message))
            {
                Snackbar.Add(Localizer("Deleted Successfully"), Severity.Success);
                await LoadNew();
                if (grid is not null)
                {
                    await grid.ReloadServerData();
                }
            }
            else
            {
                Snackbar.Add(res?.Message ?? Localizer("Error Deleting"), Severity.Error);
            }
        }
    }

    private async Task Clear(bool refresh)
    {
        await LoadNew();
        if (refresh && grid is not null)
        {
            await grid.ReloadServerData();
        }
    }

    private async Task<GridData<AccountOpeningEntryHdr>> LoadServerData(GridState<AccountOpeningEntryHdr> state)
    {
        try
        {
            var pagination = new PaginationModel(
                page: state.Page + 1,
                pageSize: state.PageSize,
                sortColumn: null,
                sortDirection: null,
                search: searchText,
                BranchId: AppData.BranchId
            );

            var result = await ViewService.GetAccountOpeningEntryPDetails(pagination);

            return new GridData<AccountOpeningEntryHdr>
            {
                Items = result.PTransactions.Select(x => new AccountOpeningEntryHdr
                {
                    AccountOpeningEntryId = x.Id,
                    VoucherNo = x.Code,
                    DebitTotal = x.Amount,
                    EntryDate = DateTime.TryParse(x.Description, out var date) ? date : null
                }).ToList(),
                TotalItems = result.TotalCount
            };
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error loading data: {ex.Message}", Severity.Error);
            return new GridData<AccountOpeningEntryHdr> { Items = new List<AccountOpeningEntryHdr>(), TotalItems = 0 };
        }
    }

    private async Task OnSearch(string text)
    {
        searchText = text;
        if (grid is not null)
        {
            await grid.ReloadServerData();
        }
    }

    private async Task RowClicked(DataGridRowClickEventArgs<AccountOpeningEntryHdr> args)
    {
        _isLoading = true;
        try
        {
            var loaded = await ViewService.GetAccountOpeningEntryById(args.Item.AccountOpeningEntryId);
            if (loaded is not null && loaded.Header is not null)
            {
                // Start with Header
                entryDto.Header = loaded.Header;
                entryDate = entryDto.Header.EntryDate;

                // Load ONLY saved details (Read-Only Snapshot Mode)
                if (loaded.Details is not null)
                {
                    entryDto.Details = loaded.Details;
                    CalculateTotals();
                    await FocusFirst();
                }
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error loading entry: {ex.Message}", Severity.Error);
        }
        finally
        {
            _isLoading = false;
        }
    }
}
