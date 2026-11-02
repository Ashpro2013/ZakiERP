namespace AshproSAASERP.Shared.Pages.Masters;

using AshproSAASERP.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using System.Linq;
public partial class UnitMasterView : ITabAware
{
    #region Variables
    private UnitMaster? unitMaster { get; set; } = new();
    MudTextField<string>? txtCode;
    MudTextField<string>? txtName;
    MudTextField<string>? txtRemarks;
    private MudDataGrid<UnitMaster>? grid;
    WebUserPrivilege? privilege;
    private bool _isSaving = false;
    #endregion

    #region Pproperties
    [Parameter]
    public int TabId { get; set; }
    #endregion

    #region initialization
    protected override async Task OnInitializedAsync()
    {
        if (AppData.privileges is not null)
            privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "Unit Master")!;
        await base.OnInitializedAsync();
    }

    async Task KeyDown(KeyboardEventArgs e, int fieldId)
    {
        if (e.Key == "Enter" || e.Key == "ArrowDown")
            await DownKeyEvent(fieldId);
        else if (e.Key == "ArrowUp")
        {
            if (fieldId == 2) await txtCode!.FocusAsync();
            else if (fieldId == 3) await txtName!.FocusAsync();
        }
    }

    private async Task DownKeyEvent(int fieldId)
    {
        if (fieldId == 1) await txtName!.FocusAsync();
        else if (fieldId == 2) await txtRemarks!.FocusAsync();
    }
    #endregion

    #region Events
    private async Task<GridData<UnitMaster>> LoadServerData(GridState<UnitMaster> state)
    {
        var pagination = new PaginationModel(
           page: state.Page + 1,
            pageSize: state.PageSize,
            sortColumn: state.SortDefinitions?.FirstOrDefault()?.SortBy,
            sortDirection: state.SortDefinitions?.FirstOrDefault()?.Descending == true ? "desc" : "asc",
            search: null
        );
        var result = await api.GetUnitPDetails(pagination);
        return new GridData<UnitMaster>
        {
            Items = result.PUnits,
            TotalItems = result.TotalCount
        };
    }

    async Task SaveOrUpdate()
    {
        if (_isSaving) return;
        _isSaving = true;
        StateHasChanged();
        try
        {
            if (unitMaster?.UnitId == 0)
            {
                if (privilege is not null && !privilege.SaveData) { await Dialog.DialogMethod(Localizer("NoSavePrivilege")); return; }

                var result = await api.CreateUnit(unitMaster);
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

                var result = await api.UpdateUnit(unitMaster!);
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
            await Dialog.DialogMethod(ex.Message);
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
            if (await Dialog.DeleteDialogMethod())
            {
                var result = await api.DeleteUnit(unitMaster!.UnitId);
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
        await grid!.ReloadServerData();
        unitMaster = new UnitMaster();
        txtCode?.FocusAsync();
    }
    async Task RowClicked(DataGridRowClickEventArgs<UnitMaster> args)
    {

        unitMaster = args.Item;
        await txtCode!.FocusAsync();
        await txtCode!.SelectAsync();
    }
    #endregion
}
