using Microsoft.AspNetCore.Components.Web;

namespace ZakiSAASERP.Shared.Pages.Masters;

public partial class AccountHeadView : ITabAware
{
    #region Variables
    private AccountHeadMaster? accountHead { get; set; } = new();
    MudTextField<string>? txtCode;
    private MudDataGrid<AccountHeadMaster>? grid;
    private List<AccountType> accountTypeList = new();
    private string searchText = "";
    MudTextField<string>? txtName;
    MudTextField<string>? txtNameArabic;
    // MudSelect<string>? cmbGroup; // Removed as part of refactoring, but keydown logic referenced it.
    // We should fix KeyDown logic instead of restoring this if it's not used in UI.

    #endregion

    #region Properties
    [Parameter] public bool IsDialog { get; set; } = false;
    [Parameter] public int TabId { get; set; }
    #endregion

    WebUserPrivilege? privilege;

    protected override async Task OnInitializedAsync()
    {
        accountTypeList = await api.GetAllAccountTypes();
        await NewClick();
        if (AppData.privileges is not null)
            privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "Account Head");
    }

    private async Task<GridData<AccountHeadMaster>> LoadServerData(GridState<AccountHeadMaster> state)
    {
        var pagination = new PaginationModel(
           page: state.Page + 1,
            pageSize: state.PageSize,
            sortColumn: state.SortDefinitions?.FirstOrDefault()?.SortBy,
            sortDirection: state.SortDefinitions?.FirstOrDefault()?.Descending == true ? "desc" : "asc",
            search: searchText
        );
        var result = await api.GetAccountHeadPDetails(pagination);

        return new GridData<AccountHeadMaster>
        {
            Items = result.PAccountHeads,
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
                // await cmbGroup.FocusAsync();
            }
            else if (fieldId == 2)
            {
                await txtCode.FocusAsync();
            }
            else if (fieldId == 3)
            {
                await txtName.FocusAsync();
            }
        }
    }

    private async Task DownKeyEvent(int fieldId)
    {
        if (fieldId == 1)
        {
            await txtName.FocusAsync();
        }
        else if (fieldId == 2)
        {
            await txtNameArabic.FocusAsync();
        }
        else if (fieldId == 3)
        {
            // await cmbGroup.FocusAsync();
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
            if (accountHead?.AccountHeadId == 0)
            {
                if (privilege is not null && !privilege.SaveData) { Dialog.DialogMethod(Localizer("NoSavePrivilege")); return; }
                var result = await api.CreateAccountHead(accountHead);
                if (result.Message == null)
                {
                    if (IsDialog)
                    {
                        AppData.PassingValue = new CommonModel
                        {
                            Id = result.Id,
                            Name = accountHead.Name
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
                var result = await api.UpdateAccountHead(accountHead!);
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
        if (privilege is not null && !privilege.DeleteData) { Dialog.DialogMethod(Localizer("NoDeletePrivilege")); return; }
        if (accountHead!.IsReserved)
        {
            Dialog.DialogMethod(Localizer("Cannot delete this record because it is reserved."));
            return;
        }

        if (await Dialog.DeleteDialogMethod())
        {
            var result = await api.DeleteAccountHead(accountHead!.AccountHeadId);
            if (result.Message == null)
            {
                await NewClick();
            }
            else
            {
                Dialog.DialogMethod(result.Message);
            }
        }
        else
        {
            await NewClick();
        }
    }

    async Task NewClick()
    {
        accountHead = new AccountHeadMaster();

        // Default to first type if available
        if (accountTypeList.Any())
        {
            accountHead.FK_AcTypeId = accountTypeList.First().AcTypeId;
            // Fetch Code
            var res = await api.GetAccountHeadNextCode(accountHead.FK_AcTypeId);
            if (res.Id == 1) accountHead.Code = res.Message;
        }

        if (txtCode is not null) await txtCode.FocusAsync();
        if (grid is not null) await grid.ReloadServerData();
    }

    private async Task OnTypeChanged(IEnumerable<int> values)
    {
        var typeId = values.FirstOrDefault();
        if (typeId > 0)
        {
            accountHead!.FK_AcTypeId = typeId;

            // Fetch Code using TypeId
            var res = await api.GetAccountHeadNextCode(typeId);
            if (res.Id == 1) accountHead.Code = res.Message;
        }
    }

    async Task RowClicked(DataGridRowClickEventArgs<AccountHeadMaster> args)
    {

        accountHead = args.Item;
        await txtCode!.FocusAsync();
        await txtCode!.SelectAsync();
    }

    private async Task ToArabic()
    {
        accountHead?.NameArabic = await api.GetArabicText(accountHead.Name);
    }
}
