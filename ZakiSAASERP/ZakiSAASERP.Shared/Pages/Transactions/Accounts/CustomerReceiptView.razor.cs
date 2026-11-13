using ZakiSAASERP.Models.Transactions.Accounts;

namespace ZakiSAASERP.Shared.Pages.Transactions.Accounts
{
    public partial class CustomerReceiptView : ITabAware
    {
        [Parameter] public int TabId { get; set; }
        [Inject] IViewService? api { get; set; }
        [Inject] ISnackbar? SnackBar { get; set; }
        [Inject] IDialogService? Dialog { get; set; }
        [Inject] ModelService? ModelDialog { get; set; }
        [Inject] ShiftService? ShiftService { get; set; }
        [Parameter] public int Id { get; set; } = 0;

        private bool _isLoading = false;
        private MudDataGrid<CustomerReceiptHdr>? grid;
        private string searchText = "";

        private CustomerReceiptDTO receiptDto = new();
        private WebUserPrivilege privilege = new();
        private DateTime? receiptDate = DateTime.Now;
        private DateTime? chequeDate;
        private List<CustomerMaster>? customers;
        private List<ProjectMaster>? projects;
        private List<CustomerReceiptDtls> invoicePayments = new();
        private string _customerCode = "";
        private string _customerName = "";
        private MudTextField<string>? txtCustomerSearch;

        // Account Selection
        private List<AccountMaster> cashBankAccounts = new();
        private string _accountCode = "";
        private string _accountName = "";
        private int? defaultCashAccountId;
        private int? defaultBankAccountId;

        // Salesman Selection
        private string _salesManCode = "";
        private string _salesManName = "";

        // Select All
        private bool selectAll = false;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                await LoadDropdownData();
                if (AppData.privileges is not null)
                {
                    privilege = AppData.privileges.FirstOrDefault(x => x.Caption?.Replace(" ", "").Trim().ToLower() == "customerreceipt") ?? new();
                }
                if (Id > 0)
                {
                    await LoadReceiptById(Id);
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
        }

        private async Task LoadDropdownData()
        {
            try
            {
                customers = await api!.GetAllCustomer();
                projects = await api!.GetAllProject();

                // Load Cash & Bank accounts
                cashBankAccounts = await api!.GetCashAndBankAccounts();

                // Load default account IDs from settings
                var settings = await api!.GetAllPermanentSettings();
                defaultCashAccountId = int.TryParse(
                    settings.FirstOrDefault(s => s.SettingKey == "DefaultCashAccount")?.SettingValue,
                    out var cashId) ? cashId : null;
                defaultBankAccountId = int.TryParse(
                    settings.FirstOrDefault(s => s.SettingKey == "DefaultBankAccount")?.SettingValue,
                    out var bankId) ? bankId : null;
            }
            catch (Exception ex)
            {
                SnackBar?.Add($"{Localizer("ErrorLoadingData")}: {ex.Message}", Severity.Warning);
            }
        }
        private async Task PrintMethod() => await PrintHelperMethod();
        private async Task PrintHelperMethod(bool IsPreview = false)
        {
            try
            {
                try
                {
                    var vchrHeader = new CustomerReceiptVchrHdr(receiptDto.ReceiptDate.ToString("dd-MM-yyyy"), receiptDto.CustomerName, receiptDto.TotalAmount, receiptDto.ReceiptNo, receiptDto.RvNumber, receiptDto.ChequeNo, receiptDto.BankName, receiptDto.ChequeDate.ToDateTime().ToString("dd-MM-yyyy"), null);
                    Dictionary<string, object?> vHdr = vchrHeader.GetType().GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(vchrHeader));
                    var vchrDtls = new List<CustomerReceiptVchrDtls>();
                    foreach (var item in invoicePayments)
                    {
                        var vDtls = new CustomerReceiptVchrDtls(item.InvoiceNo, item.InvoiceDate.ToDateTime().ToString("dd-MM-yyyy"), item.PaidAmount, item.Narration);
                        vchrDtls.Add(vDtls);
                    }
                    var obj = vchrDtls.Cast<object>().ToList();
                    Dictionary<string, object> ReportDtls = new() { { "Details", obj } };
                    var pdto = await api.GetPrintDesignAsync(new PrintDesignReq(0, "Customer_Receipt"));
                    if (pdto.hdr == null || !pdto.dtls.Any())
                    {
                        await ModelDialog.DialogMethod("Print design for 'Customer Receipt' not found.");
                        return;
                    }
                    SyncfusionPrintHelper pHelper = new SyncfusionPrintHelper();
                    var pdfData = await pHelper.PrintReportMethod(downloader, pdto.hdr, pdto.dtls, vHdr, ReportDtls, null, IsPreview);
                }
                catch (Exception ex)
                {
                    await ModelDialog.DialogMethod(ex.Message);
                }
            }
            catch (Exception ex)
            {
                await ModelDialog.DialogMethod(ex.Message);
            }
        }
        private async Task LoadNewDefaults()
        {
            try
            {
                var nextNo = await api!.GetNextCustomerReceiptNo();
                receiptDto = new CustomerReceiptDTO
                {
                    ReceiptNo = nextNo ?? "CR000001",
                    ReceiptDate = DateTime.Now,
                    FK_BranchId = AppData.BranchId,
                    FK_UserId = AppData.UserId,
                    PaymentMode = "Cash"
                };
                receiptDate = DateTime.Now;
                invoicePayments.Clear();

                // Set default account based on payment mode
                SetDefaultAccount();
            }
            catch (Exception ex)
            {
                SnackBar?.Add($"{Localizer("ErrorOccurred")}: {ex.Message}", Severity.Warning);
            }
        }

        private async Task OnCustomerChanged(int customerId)
        {
            receiptDto.FK_CustomerId = customerId;
            if (customerId > 0)
            {
                await LoadCustomerBalance();
                await LoadCustomerInvoices(); // Auto-load invoices
            }
        }

        private async Task PartySearchMethod()
        {
            var parameters = new DialogParameters();
            var options = new DialogOptions()
            {
                MaxWidth = MaxWidth.Large,
                FullWidth = true
            };
            var dialog = Dialog!.Show<CustomerSearchDialog>("Customer Search", parameters, options);
            var result = await dialog.Result;
            if (!result.Canceled)
            {
                var customer = (CustomerMaster)result.Data;
                _customerCode = customer.Code;
                _customerName = customer.Name;
                receiptDto.FK_CustomerId = customer.CustomerId;
                await LoadCustomerBalance();
                await LoadCustomerInvoices(); // Auto-load invoices
                await InvokeAsync(StateHasChanged);
            }
        }

        private void OnAmountChanged(CustomerReceiptDtls item, decimal newAmount)
        {
            // Validate negative amounts
            if (newAmount < 0)
            {
                SnackBar?.Add(Localizer("AmountCannotBeNegative"), Severity.Warning);
                item.PaymentAmount = 0;
                item.IsSelected = false;
                CalculateTotals();
                return;
            }

            // Validate against outstanding
            if (newAmount > item.OutstandingAmount)
            {
                SnackBar?.Add($"{Localizer("AmountExceedsOutstanding")}: {item.OutstandingAmount:N3}", Severity.Warning);
                item.PaymentAmount = item.OutstandingAmount;
            }
            else
            {
                item.PaymentAmount = newAmount;
            }

            // Auto-check/uncheck based on amount
            item.IsSelected = item.PaymentAmount > 0;

            CalculateTotals();
        }

        private void OnDiscountChanged(CustomerReceiptDtls item, decimal newDiscount)
        {
            // Validate negative discount
            if (newDiscount < 0)
            {
                SnackBar?.Add(Localizer("DiscountCannotBeNegative"), Severity.Warning);
                item.Discount = 0;
                CalculateTotals();
                return;
            }

            // Validate discount doesn't exceed outstanding
            if (newDiscount > item.OutstandingAmount)
            {
                SnackBar?.Add($"{Localizer("DiscountExceedsOutstanding")}: {item.OutstandingAmount:N3}", Severity.Warning);
                item.Discount = 0;
                CalculateTotals();
                return;
            }

            // Validate total (payment + discount) doesn't exceed outstanding
            if (item.PaymentAmount + newDiscount > item.OutstandingAmount)
            {
                SnackBar?.Add(Localizer("PaymentPlusDiscountExceedsOutstanding"), Severity.Warning);
                var maxDiscount = item.OutstandingAmount - item.PaymentAmount;
                item.Discount = maxDiscount > 0 ? maxDiscount : 0;
            }
            else
            {
                item.Discount = newDiscount;
            }

            // Auto-check if discount is entered
            if (item.Discount > 0)
                item.IsSelected = true;

            CalculateTotals();
        }

        private async Task LoadCustomerBalance()
        {
            try
            {
                var balance = await api!.GetCustomerBalance(receiptDto.FK_CustomerId, AppData.BranchId);
                receiptDto.CustomerBalance = balance;
            }
            catch (Exception ex)
            {
                SnackBar?.Add($"{Localizer("ErrorLoadingData")}: {ex.Message}", Severity.Warning);
            }
        }

        private async Task LoadCustomerInvoices()
        {
            if (receiptDto.FK_CustomerId == 0)
            {
                SnackBar?.Add(Localizer("PleaseSelectCustomerFirst"), Severity.Warning);
                return;
            }

            try
            {
                _isLoading = true;
                StateHasChanged();

                var outstandingInvoices = await api!.GetCustomerOutstandingInvoices(
                    receiptDto.FK_CustomerId, AppData.BranchId);

                invoicePayments.Clear();
                if (outstandingInvoices is not null && outstandingInvoices.Any())
                {
                    foreach (var invoice in outstandingInvoices)
                    {
                        invoicePayments.Add(new CustomerReceiptDtls
                        {
                            FK_InvoiceId = invoice.InvoiceId,
                            InvoiceNo = invoice.InvoiceNo,
                            InvoiceDate = invoice.InvoiceDate,
                            InvoiceAmount = invoice.TotalAmount,
                            PaidAmount = invoice.PaidAmount,
                            ReturnAmount = invoice.ReturnAmount,
                            OutstandingAmount = invoice.OutstandingAmount,
                            PaymentAmount = 0,
                            Discount = 0,
                            Narration = "",
                            IsSelected = false,
                            IsOpeningEntry = invoice.IsOpeningEntry,
                            CustomerName = invoice.CustomerName
                        });
                    }
                }
                else
                {
                    SnackBar?.Add(Localizer("NoPendingInvoices"), Severity.Info);
                }
            }
            catch (Exception ex)
            {
                SnackBar?.Add($"{Localizer("ErrorOccurred")}: {ex.Message}", Severity.Error);
            }
            finally
            {
                _isLoading = false;
                StateHasChanged();
            }
        }

        private void CalculateTotals()
        {
            receiptDto.AllocatedAmount = invoicePayments
                .Where(i => i.IsSelected || i.PaymentAmount > 0)
                .Sum(i => i.PaymentAmount - i.Discount);
            receiptDto.TotalAmount = receiptDto.AllocatedAmount + receiptDto.AdvanceAmount;
        }


        private bool _isSaving = false;

        private async Task Save()
        {
            if (_isSaving) return;
            _isSaving = true;
            StateHasChanged();

            try
            {
                await ValidateAndPromptAdvance();
                if (privilege.SaveData == false)
                {
                    ModelDialog?.DialogMethod(Localizer("You don't have the privilege to save"));
                    return;
                }

                if (!ValidateReceipt())
                {
                    _isSaving = false;
                    StateHasChanged();
                    return;
                }

                _isLoading = true;
                receiptDto.ReceiptDate = receiptDate ?? DateTime.Now;
                receiptDto.ChequeDate = chequeDate;
                receiptDto.Details = invoicePayments
                    .Where(i => i.IsSelected && i.PaymentAmount > 0)
                    .ToList();

                var result = await api!.CreateCustomerReceipt(receiptDto);

                if (result is not null && result.Id > 0)
                {
                    if (PermanentSettingsData.EnableShiftClosing && ShiftService != null)
                    {
                        try
                        {
                            var terminal = "Terminal 1";
                            var user = !string.IsNullOrWhiteSpace(AppData.UserName) ? AppData.UserName : "User";
                            await ShiftService.RecordReceiptAsync(receiptDto.TotalAmount, receiptDto.PaymentMode ?? "Cash", terminal, user);
                        }
                        catch { }
                    }

                    ModelDialog?.DialogMethod(Localizer("SavedSuccessfully"));
                    await Clear(true);
                    if (grid is not null) await grid.ReloadServerData();
                }
                else
                {
                    ModelDialog?.DialogMethod(result?.Message ?? Localizer("ErrorOccurred"));
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

        private async Task Update()
        {
            if (_isSaving) return;
            _isSaving = true;
            StateHasChanged();

            try
            {
                if (!ValidateReceipt())
                {
                    _isSaving = false;
                    StateHasChanged();
                    return;
                }

                if (privilege.EditData == false)
                {
                    ModelDialog?.DialogMethod(Localizer("You don't have the privilege to edit"));
                    return;
                }

                _isLoading = true;
                receiptDto.ReceiptDate = receiptDate ?? DateTime.Now;
                receiptDto.ChequeDate = chequeDate;
                receiptDto.Details = invoicePayments.Where(i => i.PaymentAmount > 0).ToList();

                var result = await api!.UpdateCustomerReceipt(receiptDto);

                if (result is not null && result.Id > 0)
                {
                    ModelDialog?.DialogMethod(Localizer("UpdatedSuccessfully"));
                    await Clear(true);
                    if (grid is not null) await grid.ReloadServerData();
                }
                else
                {
                    ModelDialog?.DialogMethod(result?.Message ?? Localizer("ErrorOccurred"));
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
                    var result = await api!.DeleteCustomerReceipt(Id);
                    if (result is not null && result.Id > 0)
                    {
                        ModelDialog?.DialogMethod(Localizer("DeletedSuccessfully"));
                        await Clear(true);
                        if (grid is not null) await grid.ReloadServerData();
                    }
                    else
                    {
                        ModelDialog?.DialogMethod(result?.Message ?? Localizer("ErrorOccurred"));
                    }
                }
            }
            catch (Exception ex)
            {
                ModelDialog?.DialogMethod(ex.Message);
            }
        }

        private bool ValidateReceipt()
        {
            // 0. Receipt number validation
            if (string.IsNullOrWhiteSpace(receiptDto.ReceiptNo))
            {
                ModelDialog?.DialogMethod(Localizer("ReceiptNumberRequired"));
                return false;
            }

            // 1. Account validation
            if (receiptDto.FK_AccountId == null || receiptDto.FK_AccountId == 0)
            {
                ModelDialog?.DialogMethod(Localizer("PleaseSelectAccount"));
                return false;
            }

            // 2. Customer validation
            if (receiptDto.FK_CustomerId == 0)
            {
                ModelDialog?.DialogMethod(Localizer("PleaseSelectCustomer"));
                return false;
            }

            // 3. Receipt date validation
            if (receiptDate == null)
            {
                ModelDialog?.DialogMethod(Localizer("ReceiptDateRequired"));
                return false;
            }

            // 4. At least one invoice or advance amount
            if (receiptDto.AllocatedAmount <= 0 && receiptDto.AdvanceAmount <= 0)
            {
                ModelDialog?.DialogMethod(Localizer("PleaseSelectInvoiceOrEnterAdvance"));
                return false;
            }

            // 5. Total amount validation
            if (receiptDto.TotalAmount <= 0)
            {
                ModelDialog?.DialogMethod(Localizer("TotalAmountMustBeGreaterThanZero"));
                return false;
            }

            // 6. Total reconciliation
            if (Math.Abs(receiptDto.TotalAmount - (receiptDto.AllocatedAmount + receiptDto.AdvanceAmount)) > 0.01m)
            {
                ModelDialog?.DialogMethod(Localizer("TotalMustEqualAllocatedPlusAdvance"));
                return false;
            }

            // 7. Cheque validation
            if (receiptDto.PaymentMode == "Cheque" && string.IsNullOrWhiteSpace(receiptDto.ChequeNo))
            {
                ModelDialog?.DialogMethod(Localizer("ChequeNumberRequired"));
                return false;
            }

            return true;
        }

        private async Task ValidateAndPromptAdvance()
        {
            decimal totalAllocated = invoicePayments
                .Where(i => i.IsSelected || i.PaymentAmount > 0)
                .Sum(i => i.PaymentAmount - i.Discount);

            decimal excess = receiptDto.TotalAmount - totalAllocated - receiptDto.AdvanceAmount;

            if (excess > 0.01m)
            {
                bool? confirm = await Dialog!.ShowMessageBox(
                    Localizer("AllocateToAdvance"),
                    $"Do you want to allocate the excess amount of {excess:N3} to advance?",
                    yesText: Localizer("Yes"),
                    cancelText: Localizer("No"));

                if (confirm == true)
                {
                    receiptDto.AdvanceAmount = excess;
                    CalculateTotals();
                }
            }
        }

        private async Task Clear(bool fromSave = false)
        {
            Id = 0;
            invoicePayments.Clear();

            // Reset customer fields
            _customerCode = "";
            _customerName = "";

            // Reset salesman fields
            _salesManCode = "";
            _salesManName = "";

            // Reset account fields (will be set by LoadNewDefaults)
            _accountCode = "";
            _accountName = "";

            await LoadNewDefaults();
        }

        private async Task<GridData<CustomerReceiptHdr>> LoadServerData(GridState<CustomerReceiptHdr> state)
        {
            try
            {
                var receipts = await api!.GetAllCustomerReceipts();

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    receipts = receipts.Where(r =>
                        (r.ReceiptNo?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true) ||
                        (r.Customer?.Name?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true)
                    ).ToList();
                }

                return new GridData<CustomerReceiptHdr>
                {
                    Items = receipts,
                    TotalItems = receipts.Count
                };
            }
            catch (Exception ex)
            {
                SnackBar?.Add($"{Localizer("ErrorOccurred")}: {ex.Message}", Severity.Error);
                return new GridData<CustomerReceiptHdr> { Items = new List<CustomerReceiptHdr>(), TotalItems = 0 };
            }
        }

        private async Task OnSearch(string text)
        {
            searchText = text;
            if (grid is not null)
                await grid.ReloadServerData();
        }

        private async Task RowClicked(DataGridRowClickEventArgs<CustomerReceiptHdr> args)
        {
            await LoadReceiptById(args.Item.CustomerReceiptId);
        }

        private async Task LoadReceiptById(int receiptId)
        {
            try
            {
                _isLoading = true;
                var receipt = await api!.GetCustomerReceiptById(receiptId);

                if (receipt is not null)
                {
                    receiptDto = receipt;
                    Id = receipt.CustomerReceiptId;
                    receiptDate = receipt.ReceiptDate;
                    chequeDate = receipt.ChequeDate;

                    // Load and populate customer fields
                    if (receipt.FK_CustomerId > 0 && customers is not null)
                    {
                        var customer = customers.FirstOrDefault(c => c.CustomerId == receipt.FK_CustomerId);
                        if (customer is not null)
                        {
                            _customerCode = customer.Code;
                            _customerName = customer.Name;
                        }
                    }

                    // Load and populate account fields
                    if (receipt.FK_AccountId.HasValue && receipt.FK_AccountId > 0 && cashBankAccounts is not null)
                    {
                        var account = cashBankAccounts.FirstOrDefault(a => a.AccountId == receipt.FK_AccountId.Value);
                        if (account is not null)
                        {
                            _accountCode = account.Code;
                            _accountName = account.Name;
                        }
                    }

                    // Load and populate salesman fields if available
                    if (receipt.FK_SalesManId.HasValue && receipt.FK_SalesManId > 0)
                    {
                        var salesmen = await api!.GetAllSalesMan();
                        var salesman = salesmen?.FirstOrDefault(s => s.SalesmanId == receipt.FK_SalesManId.Value);
                        if (salesman is not null)
                        {
                            _salesManCode = salesman.Code;
                            _salesManName = salesman.Name;
                        }
                    }

                    // Load customer balance
                    await LoadCustomerBalance();

                    // Load invoice payments
                    invoicePayments.Clear();
                    if (receipt.Details is not null && receipt.Details.Any())
                    {
                        invoicePayments = receipt.Details.ToList();
                    }
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

        private async Task AccountSearchMethod()
        {
            var parameters = new DialogParameters
            {
                ["FilterId"] = 3 // General accounts
            };

            var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Large, FullWidth = true };
            var dialog = Dialog!.Show<AccountSearchDialog>(Localizer("SelectAccount"), parameters, options);
            var result = await dialog.Result;

            if (!result.Canceled && result.Data is AccountMaster selectedAccount)
            {
                // Check if it's a Cash & Bank account
                if (cashBankAccounts.Any(a => a.AccountId == selectedAccount.AccountId))
                {
                    receiptDto.FK_AccountId = selectedAccount.AccountId;
                    _accountCode = selectedAccount.Code;
                    _accountName = selectedAccount.Name;
                }
                else
                {
                    SnackBar?.Add(Localizer("PleaseSelectCashBankAccount"), Severity.Warning);
                }
            }
        }

        private async Task SalesManSearchMethod()
        {
            var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Large, FullWidth = true };
            var dialog = Dialog!.Show<SalesManSearchDialog>(Localizer("SelectSalesMan"), options);
            var result = await dialog.Result;

            if (!result.Canceled && result.Data is SalesManMaster selectedSalesMan)
            {
                receiptDto.FK_SalesManId = selectedSalesMan.SalesmanId;
                _salesManCode = selectedSalesMan.Code;
                _salesManName = selectedSalesMan.Name;
            }
        }

        private void SetDefaultAccount()
        {
            var accountId = receiptDto.PaymentMode == "Cash" ? defaultCashAccountId : defaultBankAccountId;
            if (accountId.HasValue)
            {
                var account = cashBankAccounts.FirstOrDefault(a => a.AccountId == accountId.Value);
                if (account is not null)
                {
                    receiptDto.FK_AccountId = account.AccountId;
                    _accountCode = account.Code;
                    _accountName = account.Name;
                }
            }
        }

        private void OnSelectAllChanged(bool isChecked)
        {
            selectAll = isChecked;
            foreach (var invoice in invoicePayments)
            {
                invoice.IsSelected = isChecked;
                if (isChecked)
                {
                    invoice.PaymentAmount = invoice.OutstandingAmount;
                }
                else
                {
                    invoice.PaymentAmount = 0;
                    invoice.Discount = 0;
                }
            }
            CalculateTotals();
            StateHasChanged();
        }

        private void OnCheckboxChanged(CustomerReceiptDtls detail, bool isChecked)
        {
            detail.IsSelected = isChecked;

            if (isChecked)
            {
                // Auto-fill payment amount with outstanding amount
                detail.PaymentAmount = detail.OutstandingAmount;
            }
            else
            {
                // Clear amounts when unchecked
                detail.PaymentAmount = 0;
                detail.Discount = 0;
            }

            CalculateTotals();
            StateHasChanged();
        }

        private async void OnAllocatedAmountChanged(decimal allocatedAmount)
        {
            if (allocatedAmount < 0)
            {
                allocatedAmount = 0;
            }

            // Sort invoices by date (oldest first)
            var sortedInvoices = invoicePayments.OrderBy(i => i.InvoiceDate).ToList();

            decimal remainingAllocation = allocatedAmount;

            // Clear all current payments first
            foreach (var invoice in sortedInvoices)
            {
                invoice.PaymentAmount = 0;
                invoice.Discount = 0;
                invoice.IsSelected = false;
            }

            // Auto-allocate to invoices sequentially
            foreach (var invoice in sortedInvoices)
            {
                if (remainingAllocation <= 0) break;

                if (remainingAllocation >= invoice.OutstandingAmount)
                {
                    // Pay full amount
                    invoice.PaymentAmount = invoice.OutstandingAmount;
                    invoice.IsSelected = true;
                    remainingAllocation -= invoice.OutstandingAmount;
                }
                else
                {
                    // Pay partial amount
                    invoice.PaymentAmount = remainingAllocation;
                    invoice.IsSelected = true;
                    remainingAllocation = 0;
                }
            }

            // If excess amount remains, prompt for advance
            if (remainingAllocation > 0.01m)
            {
                bool? assignToAdvance = await Dialog!.ShowMessageBox(
                    Localizer("ExcessAmount"),
                    $"{Localizer("ExcessAmountMessage")} {remainingAllocation:N3}. {Localizer("AssignToAdvance")}?",
                    yesText: Localizer("Yes"),
                    cancelText: Localizer("No"));

                if (assignToAdvance == true)
                {
                    receiptDto.AdvanceAmount = remainingAllocation;
                }
                else
                {
                    receiptDto.AdvanceAmount = 0;
                }
            }
            else
            {
                receiptDto.AdvanceAmount = 0;
            }

            // Recalculate totals
            CalculateTotals();
            StateHasChanged();
        }

        private void OnPaymentModeChanged(string mode)
        {
            receiptDto.PaymentMode = mode;
            SetDefaultAccount(); // Auto-select default account
        }
    }
}
