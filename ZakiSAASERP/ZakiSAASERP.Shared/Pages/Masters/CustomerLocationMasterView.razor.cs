using Microsoft.AspNetCore.Components.Web;
namespace ZakiSAASERP.Shared.Pages.Masters;

public partial class CustomerLocationMasterView : ITabAware
{
    [Parameter] public int TabId { get; set; }
    private CustomerLocationMaster location { get; set; } = new();
    private string searchText = "";
    MudTextField<string>? txtCode;
    MudTextField<string>? txtName;
    MudTextField<string>? txtRemarks;
    private MudDataGrid<CustomerLocationMaster>? grid;
    private WebUserPrivilege? privilege;

    protected override async Task OnInitializedAsync()
    {
        if (AppData.privileges is not null)
            privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "CustomerLocation");
    }

    private async Task<GridData<CustomerLocationMaster>> LoadServerData(GridState<CustomerLocationMaster> state)
    {
        var pagination = new PaginationModel(
           page: state.Page + 1,
            pageSize: state.PageSize,
            sortColumn: state.SortDefinitions?.FirstOrDefault()?.SortBy,
            sortDirection: state.SortDefinitions?.FirstOrDefault()?.Descending == true ? "desc" : "asc",
            search: searchText
        );
        var result = await api.GetCustomerLocationPDetails(pagination);
        return new GridData<CustomerLocationMaster>
        {
            Items = result.PCustomerLocations,
            TotalItems = result.TotalCount
        };
    }

    private async Task OnSearch(string text)
    {
        searchText = text;
        await grid.ReloadServerData();
    }
    private bool _isSaving = false;

    async Task SaveOrUpdate()
    {
        if (_isSaving) return;
        _isSaving = true;
        StateHasChanged();
        try
        {
            if (location.LocationId == 0)
            {
                if (privilege is not null && !privilege.SaveData) { await Dialog.DialogMethod(Localizer("NoSavePrivilege")); return; }
                var result = await api.CreateCustomerLocation(location);
                if (result.Message == null)
                {
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
                var result = await api.UpdateCustomerLocation(location);
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
            if (!await Dialog.DeleteDialogMethod())
                return;

            if (privilege is not null && !privilege.DeleteData) { await Dialog.DialogMethod(Localizer("NoDeletePrivilege")); return; }

            var result = await api.DeleteCustomerLocation(location.LocationId);
            if (result.Message == null)
            {
                await NewClick();
                StateHasChanged();
            }
            else
            {
                if (result.Message.Contains("FK_", StringComparison.OrdinalIgnoreCase) || result.Message.Contains("REFERENCE", StringComparison.OrdinalIgnoreCase) || result.Message.Contains("Constraint", StringComparison.OrdinalIgnoreCase))
                    await Dialog.DialogMethod(ArabicLocalization.GetFriendlyForeignKeyError(result.Message));
                else
                    await Dialog.DialogMethod(result.Message);
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
        try
        {
            location = new CustomerLocationMaster();
            txtCode?.FocusAsync();
            if (grid is not null) await grid.ReloadServerData();
        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod(ex.Message);
        }
    }
    async Task RowClicked(DataGridRowClickEventArgs<CustomerLocationMaster> args)
    {

        try
        {
            location = args.Item;
            await txtCode!.FocusAsync();
            await txtCode.SelectAsync();
        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod(ex.Message);
        }
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
    }
}
