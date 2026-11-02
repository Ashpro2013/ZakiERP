using Microsoft.AspNetCore.Components.Web;

namespace AshproSAASERP.Shared.Pages.Masters;

public partial class BranchView : ITabAware
{
    #region Variables
    private BranchMaster? branchModel { get; set; } = new();
    MudTextField<string>? txtCode;
    List<AccountMaster> accountList = new();
    string? strAccountName;
    private MudDataGrid<BranchMaster>? grid;
    private string searchText = "";
    MudTextField<string>? txtName;
    MudTextField<string>? txtRemarks;
    MudTextField<string>? txtAccount;
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
            privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "Branch" || x.Caption == "BranchTitle");
    }
    private async Task<GridData<BranchMaster>> LoadServerData(GridState<BranchMaster> state)
    {
        var pagination = new PaginationModel(
           page: state.Page + 1,
            pageSize: state.PageSize,
            sortColumn: state.SortDefinitions?.FirstOrDefault()?.SortBy,
            sortDirection: state.SortDefinitions?.FirstOrDefault()?.Descending == true ? "desc" : "asc",
            search: searchText
        );
        var result = await api.GetBranchPDetails(pagination);


        return new GridData<BranchMaster>
        {
            Items = result.PBranches,
            TotalItems = result.TotalCount
        };
    }
    private bool _isSaving = false;

    async Task SaveOrUpdate()
    {
        if (_isSaving) return;
        _isSaving = true;
        StateHasChanged();
        try
        {
            if (branchModel?.BranchId == 0)
            {
                if (privilege is not null && !privilege.SaveData) { Dialog.DialogMethod(Localizer("NoSavePrivilege")); return; }
                var branch = branchModel.CopyTo<BranchMaster>();
                var result = await api.CreateBranch(branch);
                if (result.Message == null)
                {
                    if (IsDialog)
                    {
                        AppData.PassingValue = new CommonModel
                        {
                            Id = result.Id,
                            Name = branch.Name
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
                var result = await api.UpdateBranch(branchModel!);
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
            if (privilege is not null && !privilege.DeleteData) { Dialog.DialogMethod(Localizer("NoDeletePrivilege")); return; }
            if (await Dialog.DeleteDialogMethod())
            {
                var result = await api.DeleteBranch(branchModel!.BranchId);
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
        branchModel = new();
        strAccountName = string.Empty;
        txtCode?.FocusAsync();
        if (grid is not null) await grid.ReloadServerData();
    }
    async Task RowClicked(DataGridRowClickEventArgs<BranchMaster> args)
    {

        branchModel = args.Item;
        strAccountName = accountList.FirstOrDefault(x => x.AccountId == branchModel.AccountId)?.Name;
        await txtCode.FocusAsync();
        await txtCode.SelectAsync();
    }
    async Task OpenAccountSearchDialog()
    {
        var parameters = new DialogParameters { ["accountMasters"] = accountList };
        var options = new DialogOptions() { BackdropClick = false, MaxWidth = MaxWidth.Small };
        var dialog = DialogService.Show<AccountSearchDialog>(Localizer("AccountSearchTitle"), parameters, options);
        var result = await dialog.Result;
        if (!result!.Canceled)
        {
            var account = (AccountMaster)result.Data!;
            branchModel!.AccountId = account.AccountId;
            strAccountName = account.Name;
        }
    }
    async Task KeyDown(KeyboardEventArgs e, int fieldId)
    {
        if (fieldId == 4 && e.Code == "Space")
        {
            await OpenAccountSearchDialog();
            return;
        }

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
            else if (fieldId == 4)
            {
                await txtRemarks!.FocusAsync();
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
            await txtRemarks!.FocusAsync();
        }
        else if (fieldId == 3)
        {
            await txtAccount!.FocusAsync();
        }
    }
    #endregion
}
