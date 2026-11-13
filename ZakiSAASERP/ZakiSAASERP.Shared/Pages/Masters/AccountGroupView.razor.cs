using Microsoft.AspNetCore.Components.Web;

namespace ZakiSAASERP.Shared.Pages.Masters;

public partial class AccountGroupView : ITabAware
{
    #region Variables
    private AccountGroup? accountGroup { get; set; } = new();
    MudTextField<string>? txtCode;
    private MudDataGrid<AccountGroup>? grid;
    private List<AccountHeadMaster> accountHeadList = new();
    MudTextField<string>? txtName;
    MudTextField<string>? txtNameArabic;
    MudSelect<int?>? cmbAccountHead;
    MudTextField<string>? txtRemarks;
    private string searchText = "";
    WebUserPrivilege? privilege;
    #endregion

    #region Properties
    [Parameter]
    public bool IsDialog { get; set; }
    [Parameter] public int TabId { get; set; }
    #endregion

    #region Events

    protected override async Task OnInitializedAsync()
    {
        accountHeadList = await api.GetAllAccountHeads();
        if (AppData.privileges is not null)
            privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "Account Group");
    }

    private async Task<GridData<AccountGroup>> LoadServerData(GridState<AccountGroup> state)
    {
        var pagination = new PaginationModel(
           page: state.Page + 1,
            pageSize: state.PageSize,
            sortColumn: state.SortDefinitions?.FirstOrDefault()?.SortBy,
            sortDirection: state.SortDefinitions?.FirstOrDefault()?.Descending == true ? "desc" : "asc",
            search: searchText
        );
        var result = await api.GetAccountGroupPDetails(pagination);

        return new GridData<AccountGroup>
        {
            Items = result.PAccountGroups,
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
            else if (fieldId == 2)
            {
                await txtCode!.FocusAsync();
            }
            else if (fieldId == 3)
            {
                await txtName!.FocusAsync();
            }
            else if (fieldId == 5)
            {
                await cmbAccountHead!.FocusAsync();
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
            await cmbAccountHead!.FocusAsync();
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
            if (accountGroup?.AccountGroupId == 0)
            {
                if (privilege is not null && !privilege.SaveData) { await Dialog.DialogMethod(Localizer("NoSavePrivilege")); return; }
                var result = await api.CreateAccountGroup(accountGroup);
                if (result.Message == null)
                {
                    if (IsDialog)
                    {
                        AppData.PassingValue = new CommonModel
                        {
                            Id = result.Id,
                            Name = accountGroup.Name
                        };
                    }
                    await Dialog.DialogMethod(Localizer("SaveMessage"));
                    await NewClick();
                }
                else
                {
                    await Dialog.DialogMethod(result.Message);
                }
            }
            else
            {
                if (privilege is not null && !privilege.EditData) { await Dialog.DialogMethod(Localizer("NoEditPrivilege")); return; }
                var result = await api.UpdateAccountGroup(accountGroup!);
                if (result.Message == null)
                {
                    await Dialog.DialogMethod(Localizer("UpdateMessage"));
                    await NewClick();
                }
                else
                {
                    await Dialog.DialogMethod(result.Message);
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

    async Task Delete()
    {
        try
        {
            if (privilege is not null && !privilege.DeleteData) { await Dialog.DialogMethod(Localizer("NoDeletePrivilege")); return; }
            if (accountGroup!.IsReserved)
            {
                await Dialog.DialogMethod(Localizer("Cannot delete this record because it is reserved."));
                return;
            }

            if (await Dialog.DeleteDialogMethod())
            {
                var result = await api.DeleteAccountGroup(accountGroup!.AccountGroupId);
                if (result.Message == null)
                {
                    await NewClick();
                }
                else
                {
                    if (result.Message.Contains("FK_", StringComparison.OrdinalIgnoreCase) || result.Message.Contains("REFERENCE", StringComparison.OrdinalIgnoreCase) || result.Message.Contains("Constraint", StringComparison.OrdinalIgnoreCase))
                        await Dialog.DialogMethod(ArabicLocalization.GetFriendlyForeignKeyError(result.Message));
                    else
                        await Dialog.DialogMethod(result.Message);
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
                await Dialog.DialogMethod(ArabicLocalization.GetFriendlyForeignKeyError(ex.Message));
            else
                await Dialog.DialogMethod(ex.Message);
        }
    }

    async Task NewClick()
    {
        accountGroup = new AccountGroup();
        if (accountHeadList.Count > 0)
        {
            // Optional: Default selection
        }
        txtCode?.FocusAsync();
        if (grid is not null) await grid.ReloadServerData();
    }

    async Task RowClicked(DataGridRowClickEventArgs<AccountGroup> args)
    {

        accountGroup = args.Item;
        // Ensure the FK object is loaded mostly handled by Include in repo
        // But if grid item doesn't have FK set properly, we might need to rely on ID.
        // Usually args.Item comes from GetAllWithPagination which includes AccountHead.
        await txtCode!.FocusAsync();
        await txtCode!.SelectAsync();
    }

    private async Task ToArabic()
    {
        if (!string.IsNullOrEmpty(accountGroup?.Name))
        {
            accountGroup.NameArabic = await api.GetArabicText(accountGroup.Name);
        }
    }
    private async Task OnAccountHeadChanged(int? value)
    {
        accountGroup.FK_AcHeadId = value;
        if (value > 0)
        {
            var res = await api.GetAccountGroupNextCode(value.Value);
            if (res.Id == 1) accountGroup.Code = res.Message;
        }
    }
    #endregion
}
