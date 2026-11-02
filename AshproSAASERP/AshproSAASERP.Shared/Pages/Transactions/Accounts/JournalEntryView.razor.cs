using AshproSAASERP.Models.Transactions.Accounts;
using Microsoft.AspNetCore.Components.Web;

namespace AshproSAASERP.Shared.Pages.Transactions.Accounts
{
    public partial class JournalEntryView : ComponentBase, ITabAware
    {
        [Parameter] public int TabId { get; set; }
        [Inject] IViewService? api { get; set; }
        [Inject] ISnackbar? SnackBar { get; set; }
        [Inject] NavigationManager? nav { get; set; }
        [Inject] IDialogService? Dialog { get; set; }
        [Inject] ModelService? ModelDialog { get; set; }
        [Inject] IJSRuntime? js { get; set; }

        [Parameter] public int Id { get; set; }
        private WebUserPrivilege privilege = new();
        private JournalEntryDTO journalDto = new();
        private DateTime? journalDate = DateTime.Now;
        private bool _isLoading = false;
        private bool _isDialogOpen = false;

        // Dropdown lists
        private List<ProjectMaster>? projects;
        private List<SalesManMaster>? salesmen;
        private List<VatMaster>? vats;
        private int? selectedAccountType = 0; // 0 for All

        protected override async Task OnInitializedAsync()
        {
            try
            {
                // Load dropdown data
                await LoadDropdownData();
                if (AppData.privileges is not null)
                {
                    privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "JournalEntry") ?? new();
                }

                if (Id > 0)
                {
                    _isLoading = true;
                    var loaded = await api!.GetJournalEntryById(Id);
                    if (loaded is not null)
                    {
                        journalDto = loaded;
                        journalDate = journalDto.JournalDate;
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
                projects = await api!.GetAllProject();
                salesmen = await api!.GetAllSalesMan();
                vats = await api!.GetAllVat();
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
                journalDto = new JournalEntryDTO();

                // Get next journal number with fallback
                var nextNo = await api!.GetNextJournalNo();
                journalDto.JournalNo = string.IsNullOrEmpty(nextNo) ? "JV0001" : nextNo;

                journalDto.JournalDate = DateTime.Now;
                journalDate = DateTime.Now;
                journalDto.FK_BranchId = AppData.BranchId;
                journalDto.FK_UserId = AppData.UserId;
                journalDto.Status = "Posted"; // Default to Posted since only Posted entries affect TblAccounting
                journalDto.Details = new List<JournalEntryDtls>();
                AddNewRow();
                AddNewRow(); // Start with 2 rows (minimum for double-entry)
            }
            catch (Exception ex)
            {
                ModelDialog?.DialogMethod($"Error initializing: {ex.Message}");
                journalDto = new JournalEntryDTO();
                journalDto.JournalNo = "JV0001";
                journalDto.JournalDate = DateTime.Now;
                journalDate = DateTime.Now;
                journalDto.FK_BranchId = AppData.BranchId;
                journalDto.FK_UserId = AppData.UserId;
                journalDto.Status = "Posted";
                journalDto.Details = new List<JournalEntryDtls>();
                AddNewRow();
                AddNewRow();
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void AddNewRow()
        {
            // ERP Standard: Allow first 2 rows (double-entry minimum) without validation
            // Only validate when adding 3rd+ row
            if (journalDto.Details.Count >= 2)
            {
                var lastRow = journalDto.Details.LastOrDefault();
                if (lastRow is not null && lastRow.FK_AccountId == 0)
                {
                    SnackBar?.Add(Localizer("PleaseSelectAccountFirst"), Severity.Warning);
                    return;
                }
            }

            journalDto.Details.Add(new JournalEntryDtls
            {
                FK_JournalId = journalDto.JournalId,
                DebitAmount = 0,
                CreditAmount = 0,
                Narration = "",
                VatId = null // Default VAT to null (None)
            });
        }

        private void CalculateVat(JournalEntryDtls row)
        {
            // If VAT is selected
            if (row.VatId > 0 && vats is not null)
            {
                var vat = vats.FirstOrDefault(x => x.VatId == row.VatId);
                if (vat is not null)
                {
                    row.VatPer = vat.Percentage ?? 0;

                    // Calculate VAT based on entered amount (Base Amount)
                    // If Debit > 0, calculate Debit VAT
                    if (row.DebitAmount > 0)
                    {
                        row.DebitVat = Math.Round(row.DebitAmount * (row.VatPer / 100), 2);
                        row.CreditVat = 0;
                    }
                    // If Credit > 0, calculate Credit VAT
                    else if (row.CreditAmount > 0)
                    {
                        row.DebitVat = 0;
                        row.CreditVat = Math.Round(row.CreditAmount * (row.VatPer / 100), 2);
                    }
                }
            }
            else
            {
                row.DebitVat = 0;
                row.CreditVat = 0;
                row.VatPer = 0;
            }

            // Start with base amount
            row.TotalDebit = row.DebitAmount + row.DebitVat;
            row.TotalCredit = row.CreditAmount + row.CreditVat;
            CalculateTotals();
        }

        private async Task HandleKeyPress(KeyboardEventArgs e, JournalEntryDtls row, string fieldName)
        {
            if (e.Key == "Enter")
            {
                var index = journalDto.Details.IndexOf(row);

                // Define field order: comp_acc -> narration -> debit -> credit -> next row
                switch (fieldName)
                {
                    case "comp_acc":
                        // Open account dialog if account field is focused
                        await OpenAccountDialog(row);
                        break;

                    case "narr":
                        // From narration, move to debit
                        await js!.InvokeVoidAsync("focusElement", $"debit-{index}");
                        break;

                    case "debit":
                        // From debit, move to credit
                        await js!.InvokeVoidAsync("focusElement", $"credit-{index}");
                        break;

                    case "credit":
                        // Move to next row or add new row
                        if (index == journalDto.Details.Count - 1)
                        {
                            AddNewRow();
                            await Task.Delay(100); // Wait for DOM update
                            await js!.InvokeVoidAsync("focusElement", $"comp_acc-{journalDto.Details.Count - 1}");
                        }
                        else
                        {
                            await js!.InvokeVoidAsync("focusElement", $"comp_acc-{index + 1}");
                        }
                        break;
                }
            }
        }

        private void RemoveRow(JournalEntryDtls item)
        {
            if (journalDto.Details.Count > 2) // Keep minimum 2 rows
            {
                journalDto.Details.Remove(item);
                CalculateTotals();
            }
            else
            {
                ModelDialog?.DialogMethod(Localizer("AtLeastTwoLinesRequired"));
            }
        }

        private void CalculateTotals()
        {
            journalDto.TotalDebit = journalDto.Details.Sum(x => x.TotalDebit);
            journalDto.TotalCredit = journalDto.Details.Sum(x => x.TotalCredit);
            StateHasChanged();
        }

        private async Task OpenAccountDialog(JournalEntryDtls row)
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

                var parameters = new DialogParameters { ["FilterId"] = row.FilterAccountTypeId }; // Pass row-specific filter
                var dialog = Dialog!.Show<AccountSearchDialog>(Localizer("AccountSearch"), parameters, options);
                var result = await dialog.Result;

                if (!result.Canceled && result.Data is AccountMaster selectedAccount)
                {
                    row.FK_AccountId = selectedAccount.AccountId;
                    row.AccountCode = selectedAccount.Code;
                    row.AccountName = selectedAccount.Name;

                    // Set default VAT if applicable (optional logic)
                    // If account has default VAT, set it (needs logic if exists in AccountMaster)

                    StateHasChanged();

                    // Focus Debit Field
                    await Task.Delay(100);
                    var index = journalDto.Details.IndexOf(row);
                    await js!.InvokeVoidAsync("focusElement", $"debit-{index}");
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

        private async Task OpenInvoiceDialog(JournalEntryDtls row)
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

                // Use SalesInvoiceSearchDialog (Assuming we created it or reusing similar logic)
                var dialog = Dialog!.Show<SalesInvoiceSearchDialog>(Localizer("InvoiceSearch"), options);
                var result = await dialog.Result;

                if (!result.Canceled && result.Data is AshproSAASERP.Models.ViewModel.ExMasters selectedInvoice)
                {
                    row.InvoiceNo = selectedInvoice.Code;
                    row.InvoiceAmt = selectedInvoice.Amount;
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
                if (privilege.SaveData == false)
                {
                    SnackBar?.Add(Localizer("You don't have the privilege to save"), Severity.Error);
                    return;
                }
                // ERP Standard: Only save rows with accounts selected
                journalDto.Details = journalDto.Details.Where(x => x.FK_AccountId > 0).ToList();

                // Validate
                if (!ValidateEntry())
                {
                    _isSaving = false;
                    StateHasChanged();
                    return;
                }

                journalDto.JournalDate = journalDate ?? DateTime.Now;
                journalDto.FK_BranchId = AppData.BranchId;
                journalDto.FK_UserId = AppData.UserId;

                var res = await api!.CreateJournalEntry(journalDto);
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
                if (privilege.EditData == false)
                {
                    SnackBar?.Add(Localizer("You don't have the privilege to edit"), Severity.Error);
                    return;
                }
                // ERP Standard: Only save rows with accounts selected
                journalDto.Details = journalDto.Details.Where(x => x.FK_AccountId > 0).ToList();

                // Validate
                if (!ValidateEntry())
                {
                    _isSaving = false;
                    StateHasChanged();
                    return;
                }

                journalDto.JournalDate = journalDate ?? DateTime.Now;

                var res = await api!.UpdateJournalEntry(journalDto);
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
                        SnackBar?.Add(Localizer("You don't have the privilege to delete"), Severity.Error);
                        return;
                    }
                    var res = await api!.DeleteJournalEntry(Id);
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
            // Remove empty rows (rows with no account and no amounts)
            journalDto.Details.RemoveAll(x => x.FK_AccountId == 0 && x.DebitAmount == 0 && x.CreditAmount == 0);

            // Check minimum 2 lines
            if (journalDto.Details.Count < 2)
            {
                ModelDialog?.DialogMethod(Localizer("AtLeastTwoLinesRequired"));
                return false;
            }

            // Check each line has account
            foreach (var line in journalDto.Details)
            {
                if (line.FK_AccountId == 0)
                {
                    ModelDialog?.DialogMethod(Localizer("AccountRequired"));
                    return false;
                }

                // Check line has either debit or credit (not both, not neither)
                if (line.DebitAmount > 0 && line.CreditAmount > 0)
                {
                    ModelDialog?.DialogMethod(Localizer("LineCannotHaveBothDebitAndCredit"));
                    return false;
                }

                if (line.DebitAmount == 0 && line.CreditAmount == 0)
                {
                    ModelDialog?.DialogMethod(Localizer("LineMusHaveDebitOrCredit"));
                    return false;
                }
            }

            // Calculate totals
            CalculateTotals();

            // Check balance
            if (Math.Abs(journalDto.TotalDebit - journalDto.TotalCredit) > 0.001m)
            {
                ModelDialog?.DialogMethod(Localizer("DebitCreditMustBalance"));
                return false;
            }

            return true;
        }

        private async Task Clear(bool fromSave = false)
        {
            // Check for unsaved changes
            bool isDirty = false;
            if (!fromSave && Id == 0 && journalDto.Details is not null)
            {
                isDirty = journalDto.Details.Any(x => x.FK_AccountId > 0 || x.DebitAmount > 0 || x.CreditAmount > 0);
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

        private MudDataGrid<JournalEntryHdr>? grid;
        private string searchText = "";

        private async Task<GridData<JournalEntryHdr>> LoadServerData(GridState<JournalEntryHdr> state)
        {
            var items = await api!.GetAllJournalEntries();

            // Apply search
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                items = items.Where(x =>
                    (x.JournalNo?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (x.Narration?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false)
                ).ToList();
            }

            // Apply sorting
            if (state.SortDefinitions.Any())
            {
                var sortDef = state.SortDefinitions.First();
                items = sortDef.Descending
                    ? items.OrderByDescending(x => x.JournalDate).ToList()
                    : items.OrderBy(x => x.JournalDate).ToList();
            }
            else
            {
                items = items.OrderByDescending(x => x.JournalDate).ToList();
            }

            // Apply paging
            var totalCount = items.Count;
            items = items.Skip(state.Page * state.PageSize).Take(state.PageSize).ToList();

            return new GridData<JournalEntryHdr>
            {
                Items = items,
                TotalItems = totalCount
            };
        }

        private async Task OnSearch(string text)
        {
            searchText = text;
            if (grid is not null)
                await grid.ReloadServerData();
        }

        private async Task RowClicked(DataGridRowClickEventArgs<JournalEntryHdr> args)
        {

            try
            {
                _isLoading = true;
                Id = args.Item.JournalId;
                var loaded = await api!.GetJournalEntryById(Id);
                if (loaded is not null)
                {
                    journalDto = loaded;
                    journalDate = journalDto.JournalDate;
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
    }
}
