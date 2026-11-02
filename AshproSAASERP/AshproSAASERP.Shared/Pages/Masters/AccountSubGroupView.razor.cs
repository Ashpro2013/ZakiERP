using Microsoft.AspNetCore.Components.Web;
namespace AshproSAASERP.Shared.Pages.Masters;

public partial class AccountSubGroupView : ITabAware
{
    #region Variables
    private AccountSubGroup? accountSubGroup { get; set; } = new();
    MudTextField<string>? txtCode;
    private MudDataGrid<AccountSubGroup>? grid;
    private List<AccountGroup> accountGroupList = new();
    MudSelect<int?>? cmbAccountGroup;
    MudTextField<string>? txtName;
    MudTextField<string>? txtNameArabic;
    MudTextField<string>? txtRemarks;
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
            privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "Account Sub Group" || x.Caption == "AccountSubGroup");
        accountGroupList = await api.GetAllAccountGroups();
    }

    private async Task<GridData<AccountSubGroup>> LoadServerData(GridState<AccountSubGroup> state)
    {
        var pagination = new PaginationModel(
           page: state.Page + 1,
            pageSize: state.PageSize,
            sortColumn: state.SortDefinitions?.FirstOrDefault()?.SortBy,
            sortDirection: state.SortDefinitions?.FirstOrDefault()?.Descending == true ? "desc" : "asc",
            search: searchText
        );
        var result = await api.GetAccountSubGroupPDetails(pagination);

        return new GridData<AccountSubGroup>
        {
            Items = result.PAccountSubGroups,
            TotalItems = result.TotalCount
        };
    }











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
            else if (fieldId == 3)
            {
                await cmbAccountGroup!.FocusAsync();
            }
            else if (fieldId == 4)
            {
                await txtName!.FocusAsync();
            }
            else if (fieldId == 5)
            {
                await txtNameArabic!.FocusAsync();
            }
        }
    }

    private async Task DownKeyEvent(int fieldId)
    {
        if (fieldId == 1)
        {
            await cmbAccountGroup!.FocusAsync();
        }
        else if (fieldId == 3)
        {
            await txtNameArabic!.FocusAsync();
        }
        else if (fieldId == 4)
        {
            await txtRemarks!.FocusAsync();
        }
        else if (fieldId == 5)
        {
            // End of form
        }
    }



    private bool _isSaving = false;

    async Task SaveOrUpdate()
    {
        if (_isSaving) return;
        _isSaving = true;
        StateHasChanged();
        try
        {
            // Auto-set AccountHead based on selected AccountGroup
            if (accountSubGroup?.FK_AcGroupId > 0)
            {
                var selectedGroup = accountGroupList.FirstOrDefault(x => x.AccountGroupId == accountSubGroup.FK_AcGroupId);
                if (selectedGroup?.FK_AcHeadId is not null)
                {
                    accountSubGroup.FK_AcHeadId = selectedGroup.FK_AcHeadId.Value;
                }
            }

            if (accountSubGroup?.SubGroupId == 0)
            {
                if (privilege is not null && !privilege.SaveData) { Dialog.DialogMethod(Localizer("NoSavePrivilege")); return; }
                var result = await api.CreateAccountSubGroup(accountSubGroup);
                if (result.Message == null)
                {
                    if (IsDialog)
                    {
                        AppData.PassingValue = new CommonModel
                        {
                            Id = result.Id,
                            Name = accountSubGroup.Name
                        };
                    }
                    Dialog.DialogMethod(Localizer("SaveMessage"));
                    await NewClick();
                }
                else
                {
                    Dialog.DialogMethod(result.Message);
                }
            }
            else
            {
                if (privilege is not null && !privilege.EditData) { Dialog.DialogMethod(Localizer("NoEditPrivilege")); return; }
                var result = await api.UpdateAccountSubGroup(accountSubGroup!);
                if (result.Message == null)
                {
                    Dialog.DialogMethod(Localizer("UpdateMessage"));
                    await NewClick();
                }
                else
                {
                    Dialog.DialogMethod(result.Message);
                }
            }
        }
        finally
        {
            _isSaving = false;
            StateHasChanged();
        }
    }

    async Task Delete()
    {
        try
        {
            if (accountSubGroup!.IsReserved)
            {
                Dialog.DialogMethod(Localizer("Cannot delete this record because it is reserved."));
                return;
            }

            if (privilege is not null && !privilege.DeleteData) { Dialog.DialogMethod(Localizer("NoDeletePrivilege")); return; }

            if (await Dialog.DeleteDialogMethod())
            {
                var result = await api.DeleteAccountSubGroup(accountSubGroup!.SubGroupId);
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
            else
            {
                await NewClick();
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
        accountSubGroup = new AccountSubGroup();
        if (accountGroupList.Count > 0)
        {
            // Optional: Default selection
        }
        txtCode?.FocusAsync();
        if (grid is not null) await grid.ReloadServerData();
    }

    async Task RowClicked(DataGridRowClickEventArgs<AccountSubGroup> args)
    {

        accountSubGroup = args.Item;
        await txtCode!.FocusAsync();
        await txtCode!.SelectAsync();
    }

    private async Task ToArabic()
    {
        if (!string.IsNullOrEmpty(accountSubGroup?.Name))
        {
            accountSubGroup.NameArabic = await api.GetArabicText(accountSubGroup.Name);
        }
    }
    private async Task OnAccountGroupChanged(int? value)
    {
        accountSubGroup.FK_AcGroupId = value;
        if (value > 0)
        {
            var res = await api.GetAccountSubGroupNextCode(value.Value);
            if (res.Id == 1) accountSubGroup.Code = res.Message;

            var selectedGroup = accountGroupList.FirstOrDefault(x => x.AccountGroupId == value);
            if (selectedGroup?.FK_AcHeadId is not null)
            {
                accountSubGroup.FK_AcHeadId = selectedGroup.FK_AcHeadId.Value;
            }
        }
    }
    #endregion
}
