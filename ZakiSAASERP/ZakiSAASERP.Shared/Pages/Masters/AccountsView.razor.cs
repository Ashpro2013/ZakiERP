using Microsoft.AspNetCore.Components.Web;
namespace ZakiSAASERP.Shared.Pages.Masters;

public partial class AccountsView : ITabAware
{
    #region Variables
    private AccountMaster accountMaster { get; set; } = new();
    MudTextField<string>? txtCode;
    private MudDataGrid<AccountMaster>? grid;

    // Lists for Dropdowns
    private List<AccountGroup> accountGroupList = new();
    private List<AccountGroup> filteredAccountGroupList = new();
    private List<AccountSubGroup> accountSubGroupList = new();
    private List<AccountSubGroup> filteredSubGroupList = new();
    private List<AccountHeadMaster> accountHeadList = new();
    private List<AccountHeadMaster> filteredAccountHeadList = new();
    private List<AccountType> accountTypeList = new();

    // List for DbCr
    private List<string> dbCrList = new() { "Debit", "Credit" };

    // Search and filter
    private string searchText = "";
    private WebUserPrivilege? privilege;

    #endregion

    #region Properties
    [Parameter] public bool IsDialog { get; set; } = false;
    [Parameter] public int TabId { get; set; }
    #endregion

    #region Events
    protected override async Task OnInitializedAsync()
    {
        if (AppData.privileges is not null)
            privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "Accounts" || x.Caption == "AccountsView");
        // Load dropdown data
        accountGroupList = await api.GetAllAccountGroups();
        accountSubGroupList = await api.GetAllAccountSubGroups();
        accountHeadList = await api.GetAllAccountHeads();
        accountTypeList = await api.GetAllAccountTypes();

        filteredAccountGroupList = new List<AccountGroup>(accountGroupList);
        filteredSubGroupList = new List<AccountSubGroup>(accountSubGroupList);
        filteredAccountHeadList = new List<AccountHeadMaster>(accountHeadList);

        await NewClick();
    }

    private async Task<GridData<AccountMaster>> LoadServerData(GridState<AccountMaster> state)
    {
        try
        {
            var pagination = new PaginationModel(
                page: state.Page + 1,
                pageSize: state.PageSize,
                sortColumn: state.SortDefinitions?.FirstOrDefault()?.SortBy ?? "Code",
                sortDirection: state.SortDefinitions?.FirstOrDefault()?.Descending == true ? "desc" : "asc",
                search: searchText
            );

            var result = await api.GetAccountPDetails(pagination);

            return new GridData<AccountMaster>
            {
                Items = result.PAccounts,
                TotalItems = result.TotalCount
            };
        }
        catch (Exception ex)
        {
            snackbar.Add($"Error loading data: {ex.Message}", Severity.Error);
            return new GridData<AccountMaster>
            {
                Items = new List<AccountMaster>(),
                TotalItems = 0
            };
        }
    }

    private bool _isSaving = false;

    async Task SaveOrUpdate()
    {
        if (_isSaving) return;

        // Ensure manual mapping if needed, but model binding handles it.
        // Just verify nullable logic if needed.

        if (accountMaster.AccountId == 0)
        {
            if (!accountMaster.FK_AcTypeId.HasValue)
            {
                snackbar.Add(Localizer("AccountTypeRequired"), Severity.Error);
                return;
            }
            if (string.IsNullOrEmpty(accountMaster.DbCr))
            {
                snackbar.Add(Localizer("EffectRequired"), Severity.Error);
                return;
            }

            if (privilege is not null && !privilege.SaveData) { Dialog.DialogMethod(Localizer("NoSavePrivilege")); return; }

            _isSaving = true;
            StateHasChanged();
            try
            {
                var result = await api.CreateAccountMaster(accountMaster);
                if (result.Message == null)
                {
                    if (IsDialog)
                    {
                        AppData.PassingValue = new CommonModel
                        {
                            Id = result.Id,
                            Name = accountMaster.Name
                        };
                    }
                    Dialog.DialogMethod(Localizer("SaveMessage"));
                    if (grid is not null) await grid.ReloadServerData();
                    await NewClick();
                }
                else
                {
                    Dialog.DialogMethod(result.Message);
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
        else
        {
            if (!accountMaster.FK_AcTypeId.HasValue)
            {
                snackbar.Add(Localizer("AccountTypeRequired"), Severity.Error);
                return;
            }
            if (string.IsNullOrEmpty(accountMaster.DbCr))
            {
                snackbar.Add(Localizer("EffectRequired"), Severity.Error);
                return;
            }

            if (privilege is not null && !privilege.EditData) { Dialog.DialogMethod(Localizer("NoEditPrivilege")); return; }

            _isSaving = true;
            StateHasChanged();
            try
            {
                var result = await api.UpdateAccountMaster(accountMaster);
                if (result.Message == null)
                {
                    Dialog.DialogMethod(Localizer("UpdateMessage"));
                    if (grid is not null) await grid.ReloadServerData();
                    await NewClick();
                }
                else
                {
                    Dialog.DialogMethod(result.Message);
                }
            }
            finally
            {
                _isSaving = false;
                StateHasChanged();
            }
        }
    }

    async Task Delete()
    {
        try
        {
            if (accountMaster.IsReserved)
            {
                Dialog.DialogMethod(Localizer("ReservedDeleteError"));
                return;
            }

            if (privilege is not null && !privilege.DeleteData) { Dialog.DialogMethod(Localizer("NoDeletePrivilege")); return; }

            if (await Dialog.DeleteDialogMethod())
            {
                var result = await api.DeleteAccountMaster(accountMaster.AccountId);
                if (result.Message == null)
                {
                    await NewClick();
                }
                else
                {
                    if (result.Message.Contains("FK_", StringComparison.OrdinalIgnoreCase) || result.Message.Contains("REFERENCE", StringComparison.OrdinalIgnoreCase) || result.Message.Contains("Constraint", StringComparison.OrdinalIgnoreCase))
                        Dialog.DialogMethod(ArabicLocalization.GetFriendlyForeignKeyError(result.Message));
                    else
                        Dialog.DialogMethod(result.Message);
                }
            }
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("FK_", StringComparison.OrdinalIgnoreCase) || ex.Message.Contains("REFERENCE", StringComparison.OrdinalIgnoreCase) || ex.Message.Contains("Constraint", StringComparison.OrdinalIgnoreCase))
                Dialog.DialogMethod(ArabicLocalization.GetFriendlyForeignKeyError(ex.Message));
            else
                Dialog.DialogMethod(ex.Message);
        }
    }

    async Task NewClick()
    {
        if (grid is not null) await grid.ReloadServerData();

        accountMaster = new AccountMaster();

        filteredAccountGroupList = new List<AccountGroup>(accountGroupList);
        filteredSubGroupList = new List<AccountSubGroup>(accountSubGroupList);
        filteredAccountHeadList = new List<AccountHeadMaster>(accountHeadList);

        accountMaster.DbCr = "Debit";
        accountMaster.Flag = "Acc";

        // Default to Asset type
        var assetType = accountTypeList.FirstOrDefault(x => x.AcTypeName.Contains("Asset", StringComparison.OrdinalIgnoreCase));
        if (assetType is not null)
        {
            OnTypeChanged(assetType.AcTypeId);
        }
        else
        {
            OnTypeChanged(null);
        }

        if (txtCode is not null) await txtCode.FocusAsync();
    }

    async Task RowClicked(DataGridRowClickEventArgs<AccountMaster> args)
    {

        accountMaster = args.Item;

        // Filter AccountHead based on Type
        if (accountMaster.FK_AcTypeId.HasValue)
        {
            var type = accountTypeList.FirstOrDefault(x => x.AcTypeId == accountMaster.FK_AcTypeId.Value);
            if (type is not null)
            {
                filteredAccountHeadList = accountHeadList
                    .Where(x => x.FK_AcTypeId == type.AcTypeId)
                    .ToList();
            }
            else
            {
                filteredAccountHeadList = new List<AccountHeadMaster>(accountHeadList);
            }
        }
        else
        {
            filteredAccountHeadList = new List<AccountHeadMaster>(accountHeadList);
        }

        // Restore cascaded lists based on current values
        if (accountMaster.FK_AccountHeadId > 0)
        {
            filteredAccountGroupList = accountGroupList.Where(x => x.FK_AcHeadId == accountMaster.FK_AccountHeadId).ToList();
        }
        else
        {
            filteredAccountGroupList = new List<AccountGroup>(accountGroupList);
        }

        if (accountMaster.FK_AcGroupId > 0)
        {
            filteredSubGroupList = accountSubGroupList.Where(x => x.FK_AcGroupId == accountMaster.FK_AcGroupId).ToList();
        }
        else
        {
            filteredSubGroupList = new List<AccountSubGroup>(accountSubGroupList);
        }

        if (txtCode is not null)
        {
            await txtCode.FocusAsync();
            await txtCode.SelectAsync();
        }
    }

    private void OnTypeChanged(int? typeId)
    {
        accountMaster.FK_AcTypeId = typeId;

        if (typeId.HasValue)
        {
            var type = accountTypeList.FirstOrDefault(x => x.AcTypeId == typeId.Value);
            if (type is not null)
            {
                filteredAccountHeadList = accountHeadList
                    .Where(x => x.FK_AcTypeId == type.AcTypeId)
                    .ToList();
            }
            else
            {
                filteredAccountHeadList = new List<AccountHeadMaster>(accountHeadList);
            }
        }
        else
        {
            filteredAccountHeadList = new List<AccountHeadMaster>(accountHeadList);
        }

        // Reset dependent fields
        accountMaster.FK_AccountHeadId = 0;
        OnHeadChanged(0);
    }

    private async Task ToArabic()
    {
        if (!string.IsNullOrEmpty(accountMaster.Name))
        {
            accountMaster.NameArabic = await api.GetArabicText(accountMaster.Name);
        }
    }

    private void OnHeadChanged(int headId)
    {
        accountMaster.FK_AccountHeadId = headId;
        if (headId > 0)
        {
            filteredAccountGroupList = accountGroupList.Where(x => x.FK_AcHeadId == headId).ToList();
        }
        else
        {
            filteredAccountGroupList = new List<AccountGroup>(accountGroupList);
        }

        // Reset dependent fields
        accountMaster.FK_AcGroupId = 0;
        OnGroupChanged(0);
    }

    private void OnGroupChanged(int groupId)
    {
        accountMaster.FK_AcGroupId = groupId;
        if (groupId > 0)
        {
            filteredSubGroupList = accountSubGroupList.Where(x => x.FK_AcGroupId == groupId).ToList();
        }
        else
        {
            filteredSubGroupList = new List<AccountSubGroup>(accountSubGroupList);
        }

        // Reset dependent fields
        accountMaster.FK_AccountSubGroupId = 0;
        OnSubGroupChanged(0);
    }

    private async Task OnSubGroupChanged(int subGroupId)
    {
        accountMaster.FK_AccountSubGroupId = subGroupId;
        if (subGroupId > 0)
        {
            var res = await api.GetAccountMasterNextCode(subGroupId);
            if (res.Id == 1) accountMaster.Code = res.Message;
        }
    }

    MudTextField<string>? txtName;
    MudTextField<string>? txtNameArabic;
    MudSelect<string>? cmbDbCr;

    async Task KeyDown(KeyboardEventArgs e, int fieldId)
    {
        if (e.Key == "Enter" || e.Key == "ArrowDown")
            await DownKeyEvent(fieldId);
        else if (e.Key == "ArrowUp")
        {
            if (fieldId == 1)
            {
                // await txtCode.FocusAsync();
            }
            else if (fieldId == 2)
            {
                await txtCode!.FocusAsync();
            }
            else if (fieldId == 3)
            {
                await txtName!.FocusAsync();
            }
        }
    }

    private async Task DownKeyEvent(int fieldId)
    {
        if (fieldId == 1)
        {
            await txtName!.FocusAsync();
        }
        else if (fieldId == 2)
        {
            await txtNameArabic!.FocusAsync();
        }
        else if (fieldId == 3)
        {
            await cmbDbCr!.FocusAsync();
        }
    }

    #endregion
}
