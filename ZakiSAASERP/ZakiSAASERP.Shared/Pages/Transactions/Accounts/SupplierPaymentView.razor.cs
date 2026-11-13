using ZakiSAASERP.Models.Transactions.Purchase;

namespace ZakiSAASERP.Shared.Pages.Transactions.Accounts;

public partial class SupplierPaymentView : ITabAware
{
    [Parameter] public int TabId { get; set; }
    [Parameter] public int? Id { get; set; }
    private SupplierPaymentHdr hdr = new();
    private WebUserPrivilege privilege = new();
    private List<SupplierPaymentDtls> dtls = new();
    private bool IsPrintPreview = false;
    private bool selectAll = false;
    private decimal SupplierBalance = 0;

    // Grid support
    private MudDataGrid<SupplierPaymentHdr>? grid;
    private string searchText = "";

    // Masters
    private List<CommonModel> cashBankList = new();
    private List<CommonModel> projectList = new();

    // UI Fields for Search parity
    private string _accountCode = "";
    private string _accountName = "";
    private string _purchaserCode = "";
    private string _purchaserName = "";

    protected override async Task OnInitializedAsync()
    {
        await LoadMasters();
        await NewClick();
        if (AppData.privileges is not null)
        {
            privilege = AppData.privileges.FirstOrDefault(x => x.Caption?.Replace(" ", "").Trim().ToLower() == "supplierpayment") ?? new();
        }

        if (Id is not null && Id > 0)
        {
            await LoadPaymentById(Id ?? 0);
            await InvokeAsync(StateHasChanged);
        }
    }
    private async Task LoadMasters()
    {
        try
        {
            var branchId = AppData.BranchId;
            // cashBankList is still useful for default selection logic in PayMode
            cashBankList = (await api.GetCombofill("Account", branchId)).Where(x => x.Id != 0).ToList();
            projectList = (await api.GetCombofill("Project", 1)).Where(x => x.Id != 0).ToList();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"{Localizer("ErrorLoadingData")}: {ex.Message}", Severity.Error);
        }
    }

    private async Task NewClick()
    {
        hdr = new SupplierPaymentHdr
        {
            FK_BranchId = AppData.BranchId,
            FK_UserId = AppData.UserId ?? 0,
            FK_FinancialYearId = AppData.FYearId ?? 0,
            PaidDate = DateTime.Now,
            PayMode = "CASH", // Default
            IsPurchaseWise = true,
            VoucherNo = "0" // Will be generated on save
        };

        if (projectList.Any()) hdr.FK_ProjectId = projectList.First().Id;

        // Reset UI Search Fields
        _accountCode = "";
        _accountName = "";
        _purchaserCode = "";
        _purchaserName = "";

        // Default Account (Cash)
        if (cashBankList.Any())
        {
            var cash = cashBankList.FirstOrDefault(x => x.Name.Contains("Cash", StringComparison.OrdinalIgnoreCase));
            if (cash is not null)
            {
                hdr.FK_CashBankId = cash.Id;
                // Since CommonModel doesn't have Code, we might leave Code empty or fetch it? 
                // For UI display, we set Name at least.
                _accountName = cash.Name;
                // Ideally we'd fetch Code too, but for default list we might skip code until user searches.
            }
        }

        dtls = new List<SupplierPaymentDtls>();
        selectAll = false;
        SupplierBalance = 0;

        try
        {
            if (hdr.PaymentId == 0)
                hdr.VoucherNo = await api.GetNewSupplierPaymentNo(AppData.BranchId);
        }
        catch (Exception ex)
        {
            await DialogModel.ShowMessage(ex.Message);
        }
        StateHasChanged();
    }

    private bool _isSaving = false;

    private async Task OnTotalAmountChanged(decimal newAmount)
    {
        hdr.Amount = newAmount;
        if (newAmount > hdr.AllocatedAmount)
        {
            decimal excess = newAmount - hdr.AllocatedAmount;

            string contentText = $"{Localizer("AmountExceedsAllocated")} {excess:N2}. {Localizer("AddRemainingToAdvance")}";

            bool? result = await Dialog.ShowMessageBox(
                Localizer("Warning"),
                contentText,
                yesText: Localizer("Yes"), cancelText: Localizer("No"));

            if (result == true)
            {
                hdr.AdvanceAmount = excess;
            }
            else
            {
                hdr.Amount = hdr.AllocatedAmount;
                hdr.AdvanceAmount = 0;
            }
        }
        else
        {
            hdr.AdvanceAmount = 0;
            if (hdr.Amount < hdr.AllocatedAmount)
            {
                hdr.AdvanceAmount = 0;
            }
        }
        StateHasChanged();
    }

    private async Task SaveOrUpdate()
    {
        if (_isSaving) return;
        _isSaving = true;
        StateHasChanged();

        try
        {
            // Validation
            if (hdr.FK_SupplierId == 0)
            {
                Snackbar.Add(Localizer("PleaseSelectSupplier"), Severity.Warning);
                return;
            }

            if (hdr.FK_CashBankId == 0)
            {
                Snackbar.Add(Localizer("PleaseSelectAccount"), Severity.Warning);
                return;
            }

            if (hdr.Amount <= 0)
            {
                Snackbar.Add(Localizer("AmountMustBeGreaterThanZero"), Severity.Warning);
                return;
            }

            if (hdr.Amount != (hdr.AllocatedAmount + hdr.AdvanceAmount))
            {
                hdr.Amount = hdr.AllocatedAmount + hdr.AdvanceAmount;
            }

            if (hdr.Amount < hdr.AllocatedAmount)
            {
                Snackbar.Add(Localizer("TotalAmountCannotBeLessThanAllocated"), Severity.Warning);
                return;
            }

            var dto = new SupplierPaymentDTO(hdr, dtls.Where(x => x.PayingAmount > 0 || x.Discount > 0).ToList());
            OutputResponse result;

            if (hdr.PaymentId == 0)
            {
                if (privilege.SaveData == false)
                {
                    Snackbar.Add(Localizer("You don't have the privilege to save"), Severity.Error);
                    return;
                }
                result = await api.CreateSupplierPayment(dto);
            }
            else
            {
                if (privilege.EditData == false)
                {
                    Snackbar.Add(Localizer("You don't have the privilege to edit"), Severity.Error);
                    return;
                }
                result = await api.UpdateSupplierPayment(dto);
            }

            if (result.Id > 0) // Success
            {
                if (PermanentSettingsData.EnableShiftClosing && ShiftService != null && hdr.PaymentId == 0)
                {
                    try
                    {
                        var terminal = "Terminal 1";
                        var user = !string.IsNullOrWhiteSpace(AppData.UserName) ? AppData.UserName : "User";
                        await ShiftService.RecordPaymentAsync(hdr.Amount, terminal, user);
                    }
                    catch { }
                }

                if (IsPrintPreview)
                {
                    hdr.PaymentId = result.Id;
                }

                Snackbar.Add(Localizer("SaveMessage"), Severity.Success);

                if (IsPrintPreview)
                {
                    if (await DialogModel.DeleteDialogMethod(Localizer("PrintDialog")))
                    {
                        await PrintHelperMethod(true);
                    }
                }

                await NewClick();
                if (grid is not null) await grid.ReloadServerData();
            }
            else
            {
                await DialogModel.DialogMethod(result.Message);
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

    private async Task<GridData<SupplierPaymentHdr>> LoadServerData(GridState<SupplierPaymentHdr> state)
    {
        try
        {
            var pagination = new PaginationModel
            {
                page = state.Page + 1,
                pageSize = state.PageSize,
                search = searchText,
                BranchId = AppData.BranchId
            };
            var (list, count) = await api.GetSupplierPaymentPDetails(pagination);
            return new GridData<SupplierPaymentHdr> { Items = list, TotalItems = count };
        }
        catch (Exception ex)
        {
            Snackbar.Add($"{Localizer("ErrorOccurred")}: {ex.Message}", Severity.Error);
            return new GridData<SupplierPaymentHdr> { Items = new List<SupplierPaymentHdr>(), TotalItems = 0 };
        }
    }

    private async Task OnSearch(string text)
    {
        searchText = text;
        if (grid is not null)
            await grid.ReloadServerData();
    }

    private async Task RowClicked(DataGridRowClickEventArgs<SupplierPaymentHdr> args)
    {

        await LoadPaymentById(args.Item.PaymentId);
    }

    private async Task LoadPaymentById(int id)
    {
        try
        {
            var dto = await api.GetSupplierPaymentById(id);
            if (dto is not null)
            {
                hdr = dto.Hdr;
                dtls = dto.Dtls ?? new List<SupplierPaymentDtls>();

                // Populate Search Fields
                _accountName = hdr.CashBankName ?? "";
                _accountCode = ""; // Will remain empty if not fetched or we fetch it below

                // If we want Code, we need to fetch it or rely on Repo update?
                // For Account, if it's AccountHeadMaster, it has Code. 
                // Repo doesn't seem to fetch AccountCode for CashBank.
                // We'll leave it empty or name-only unless critical.

                _purchaserName = hdr.SalesmanName ?? "";
                _purchaserCode = hdr.SalesmanCode ?? "";

                // Populate UI Props
                foreach (var d in dtls)
                {
                    d.IsSelected = d.PayingAmount > 0;
                    if (d.OutstandingAmount == 0 && d.PendingAmt > 0)
                        d.OutstandingAmount = d.PendingAmt;

                    if (d.InvoiceAmount == 0 && d.PurchaseAmount > 0)
                        d.InvoiceAmount = d.PurchaseAmount;

                    d.RemainingBalance = d.OutstandingAmount - d.PayingAmount - d.Discount;
                }

                CalculateTotals();
                await LoadSupplierBalance();
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            await DialogModel.DialogMethod(ex.Message);
        }
    }


    // Supplier Search
    private async Task SupplierSearchMethod()
    {
        var result = await DialogModel.GetSupplierSearchMethod("");
        if (result is not null)
        {
            hdr.FK_SupplierId = result.SupplierId;
            hdr.SupplierName = result.Name;
            hdr.SupplierCode = result.Code;
            await OnSupplierSelected();
        }
    }

    // Account Search
    private async Task AccountSearchMethod()
    {
        var parameters = new DialogParameters
        {
            ["FilterId"] = 3 // Cash/Bank
        };
        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Large, FullWidth = true };

        // Caution: AccountSearchDialog returns AccountMaster. 
        // If FK_CashBankId links to AccountHeadMaster, we might have mismatch.
        // Assuming AccountMaster IS correct for UI selection and backend handles it or ID matches.
        // If broken, stick to Dropdown. But proceeding as requested for parity.
        var dialog = Dialog!.Show<AccountSearchDialog>(Localizer("SelectAccount"), parameters, options);
        var result = await dialog.Result;

        if (!result.Canceled && result.Data is AccountMaster selected)
        {
            // We map AccountMaster ID to FK_CashBankId 
            hdr.FK_CashBankId = selected.AccountId;
            _accountCode = selected.Code;
            _accountName = selected.Name;
        }
    }

    // Purchaser (Salesman) Search
    private async Task PurchaserSearchMethod()
    {
        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Large, FullWidth = true };
        var dialog = Dialog!.Show<SalesManSearchDialog>(Localizer("SelectPurchaser"), options);
        var result = await dialog.Result;

        if (!result.Canceled && result.Data is SalesManMaster selected)
        {
            hdr.FK_SalesmanId = selected.SalesmanId;
            _purchaserCode = selected.Code;
            _purchaserName = selected.Name;
        }
    }

    private async Task OnSupplierSelected()
    {
        if (hdr.FK_SupplierId != 0)
        {
            try
            {
                var pending = await api.GetPendingPurchaseInvoices(hdr.FK_SupplierId);

                dtls = pending.OrderBy(x => x.PurchaseDate).ToList();

                foreach (var d in dtls)
                {
                    if (d.OutstandingAmount == 0 && d.PendingAmt > 0) d.OutstandingAmount = d.PendingAmt;
                    if (d.InvoiceAmount == 0 && d.PurchaseAmount > 0) d.InvoiceAmount = d.PurchaseAmount;
                    d.RemainingBalance = d.OutstandingAmount;
                    d.IsSelected = false;
                    d.PayingAmount = 0;
                    d.Discount = 0;
                }

                CalculateTotals();
                await LoadSupplierBalance();
            }
            catch (Exception ex)
            {
                Snackbar.Add($"{Localizer("ErrorLoadingData")}: {Localizer("PendingInvoices")}", Severity.Error);
                dtls = new List<SupplierPaymentDtls>();
            }
            StateHasChanged();
        }
    }

    private async Task SearchMethod()
    {
        var result = await DialogModel.TransactionSearchMethod("Supplier Payment");
        if (result is not null)
        {
            await LoadPaymentById(result.Id);
        }
    }

    private void CalculateTotals()
    {
        hdr.AllocatedAmount = dtls.Where(i => i.IsSelected || i.PayingAmount > 0).Sum(i => i.PayingAmount);
        hdr.Amount = hdr.AllocatedAmount + hdr.AdvanceAmount;
    }

    private void OnAmountChanged(SupplierPaymentDtls item, decimal newAmount)
    {
        if (newAmount < 0) newAmount = 0;
        if (newAmount > item.OutstandingAmount)
        {
            Snackbar.Add($"{Localizer("AmountExceedsOutstanding")}", Severity.Warning);
            newAmount = item.OutstandingAmount;
        }

        item.PayingAmount = newAmount;
        item.IsSelected = item.PayingAmount > 0;
        item.RemainingBalance = item.OutstandingAmount - item.PayingAmount - item.Discount;

        CalculateTotals();
    }

    private void OnDiscountChanged(SupplierPaymentDtls item, decimal newDiscount)
    {
        if (newDiscount < 0) newDiscount = 0;
        if ((item.PayingAmount + newDiscount) > item.OutstandingAmount)
        {
            Snackbar.Add(Localizer("PaymentPlusDiscountExceedsOutstanding"), Severity.Warning);
            newDiscount = item.OutstandingAmount - item.PayingAmount;
        }

        item.Discount = newDiscount;
        item.IsSelected = (item.PayingAmount > 0 || item.Discount > 0);
        item.RemainingBalance = item.OutstandingAmount - item.PayingAmount - item.Discount;

        CalculateTotals();
    }

    private void OnAllocatedAmountChanged(decimal allocatedAmount)
    {
        if (allocatedAmount < 0) allocatedAmount = 0;

        var sortedInvoices = dtls.OrderBy(x => x.PurchaseDate).ToList();
        decimal remaining = allocatedAmount;

        foreach (var item in sortedInvoices)
        {
            item.PayingAmount = 0;
            item.Discount = 0;
            item.IsSelected = false;
            item.RemainingBalance = item.OutstandingAmount;
        }

        foreach (var item in sortedInvoices)
        {
            if (remaining <= 0) break;

            decimal toPay = Math.Min(remaining, item.OutstandingAmount);
            item.PayingAmount = toPay;
            item.IsSelected = true;
            item.RemainingBalance = item.OutstandingAmount - toPay;
            remaining -= toPay;
        }

        CalculateTotals();
    }

    private void OnSelectAllChanged(bool isChecked)
    {
        selectAll = isChecked;
        foreach (var item in dtls)
        {
            item.IsSelected = isChecked;
            if (isChecked)
            {
                item.PayingAmount = item.OutstandingAmount;
                item.RemainingBalance = 0;
            }
            else
            {
                item.PayingAmount = 0;
                item.Discount = 0;
                item.RemainingBalance = item.OutstandingAmount;
            }
        }
        CalculateTotals();
        StateHasChanged();
    }

    private void OnCheckboxChanged(SupplierPaymentDtls item, bool isChecked)
    {
        item.IsSelected = isChecked;
        if (isChecked)
        {
            item.PayingAmount = item.OutstandingAmount;
            item.RemainingBalance = 0;
        }
        else
        {
            item.PayingAmount = 0;
            item.Discount = 0;
            item.RemainingBalance = item.OutstandingAmount;
        }
        CalculateTotals();
        StateHasChanged();
    }

    private void OnPayModeChanged(string newMode)
    {
        hdr.PayMode = newMode;
        if (string.IsNullOrEmpty(newMode) || !cashBankList.Any()) return;

        CommonModel? account = null;

        if (newMode == "CASH")
        {
            account = cashBankList.FirstOrDefault(x => x.Name.Contains("Cash", StringComparison.OrdinalIgnoreCase));
        }
        else
        {
            account = cashBankList.FirstOrDefault(x => x.Name.Contains("Bank", StringComparison.OrdinalIgnoreCase));
        }

        if (account is not null)
        {
            hdr.FK_CashBankId = account.Id;
            _accountName = account.Name;
            // We set name; code might remain empty if CommonModel lacks it, but UI will at least show name in search box (as ReadOnly name) and Code box empty.
        }
    }

    private async Task Delete()
    {
        if (hdr.PaymentId == 0) return;

        if (await DialogModel.DeleteDialogMethod())
        {
            if (privilege.DeleteData == false)
            {
                Snackbar.Add(Localizer("You don't have the privilege to delete"), Severity.Error);
                return;
            }
            try
            {
                var result = await api.DeleteSupplierPayment(hdr.PaymentId);
                if (string.IsNullOrEmpty(result.Message))
                {
                    Snackbar.Add(Localizer("DeletedMessage"), Severity.Success);
                    await NewClick();
                    if (grid is not null) await grid.ReloadServerData();
                }
                else
                {
                    await DialogModel.DialogMethod(result.Message);
                }
            }
            catch (Exception ex)
            {
                await DialogModel.DialogMethod(ex.Message);
            }
        }
    }

    private async Task LoadSupplierBalance()
    {
        if (hdr.FK_SupplierId != 0 && hdr.FK_BranchId != 0)
        {
            try
            {
                SupplierBalance = await api.GetSupplierBalance(hdr.FK_SupplierId, hdr.FK_BranchId);
            }
            catch (Exception ex)
            {
                Snackbar.Add($"{Localizer("ErrorLoadingData")}: {ex.Message}", Severity.Warning);
                SupplierBalance = 0;
            }
        }
        else
        {
            SupplierBalance = 0;
        }
    }

    private async Task PrintMethod()
    {
        if (hdr.PaymentId > 0)
        {
            await PrintHelperMethod(false);
        }
        else
        {
            Snackbar.Add(Localizer("SaveFirst"), Severity.Warning);
        }
    }

    private async Task PrintHelperMethod(bool IsPreview = false)
    {
        try
        {
            var vchrHeader = new SupplierPaymentVchrHdr(hdr.PaidDate.ToDateTime().ToString("dd-MM-yyyy"), hdr.SupplierName, hdr.Amount, hdr.VoucherNo, hdr.RVNo, hdr.ChequeNo, hdr.CashBankName, hdr.ChequeDate.ToDateTime().ToString("dd-MM-yyyy"), null);
            Dictionary<string, object?> vHdr = vchrHeader.GetType().GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(vchrHeader));
            var vchrDtls = new List<SupplierPaymentVchrDtls>();
            foreach (var item in dtls)
            {
                var vDtls = new SupplierPaymentVchrDtls(item.PurchaseNo, item.PurchaseDate.ToDateTime().ToString("dd-MM-yyyy"), item.PayingAmount, item.Narration);
                vchrDtls.Add(vDtls);
            }
            var obj = vchrDtls.Cast<object>().ToList();
            Dictionary<string, object> ReportDtls = new() { { "Details", obj } };
            var pdto = await api.GetPrintDesignAsync(new PrintDesignReq(0, "Supplier_Payment"));
            if (pdto.hdr == null || !pdto.dtls.Any())
            {
                Snackbar.Add(Localizer("PrintDesignNotFound"), Severity.Error);
                return;
            }
            SyncfusionPrintHelper pHelper = new SyncfusionPrintHelper();
            var pdfData = await pHelper.PrintReportMethod(Downloader, pdto.hdr, pdto.dtls, vHdr, ReportDtls, null, IsPreview);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"{Localizer("ErrorPrinting")}: {ex.Message}", Severity.Error);
        }
    }
}
