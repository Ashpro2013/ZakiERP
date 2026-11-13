using Microsoft.AspNetCore.Components.Web;
namespace ZakiSAASERP.Shared.Pages.Masters;

public partial class CurrencyMasterView : ITabAware
{
    [Parameter] public int TabId { get; set; }
    #region Variables
    private CurrencyMaster? currencyMaster { get; set; } = new();
    MudTextField<string>? txtCode;
    MudTextField<string>? txtName;
    MudTextField<string>? txtRate;
    private MudDataGrid<CurrencyMaster>? grid;
    private WebUserPrivilege? privilege;
    #endregion

    #region Events
    protected override async Task OnInitializedAsync()
    {
        if (AppData.privileges is not null)
            privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "CurrencyMaster");
    }
    private async Task<GridData<CurrencyMaster>> LoadServerData(GridState<CurrencyMaster> state)
    {
        var pagination = new PaginationModel(
           page: state.Page + 1,
            pageSize: state.PageSize,
            sortColumn: state.SortDefinitions?.FirstOrDefault()?.SortBy,
            sortDirection: state.SortDefinitions?.FirstOrDefault()?.Descending == true ? "desc" : "asc",
            search: null
        );
        var result = await api.GetCurrencyPDetails(pagination);
        return new GridData<CurrencyMaster>
        {
            Items = result.PCurrencies,
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
            if (currencyMaster?.CurrencyId == 0)
            {
                if (privilege is not null && !privilege.SaveData) { await Dialog.DialogMethod(Localizer("NoSavePrivilege")); return; }
                var result = await api.CreateCurrency(currencyMaster);
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
                var result = await api.UpdateCurrency(currencyMaster!);
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
            if (await Dialog.DeleteDialogMethod())
            {
                var result = await api.DeleteCurrency(currencyMaster!.CurrencyId);
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
        currencyMaster = new CurrencyMaster();
        rateText = string.Empty;   // <-- CLEAR RATE TEXTBOX
        StateHasChanged();         // <-- FORCE UI REFRESH
        txtCode?.FocusAsync();
        if (grid is not null) await grid.ReloadServerData();
    }
    async Task RowClicked(DataGridRowClickEventArgs<CurrencyMaster> args)
    {

        currencyMaster = args.Item;
        await txtCode!.FocusAsync();
        await txtCode!.SelectAsync();
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
            await txtRate!.FocusAsync();
        }
    }
    #endregion
}
