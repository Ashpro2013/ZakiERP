using Microsoft.AspNetCore.Components.Web;

namespace ZakiSAASERP.Shared.Pages.Masters;

public partial class FinancialYearView : ITabAware
{
    [Parameter] public int TabId { get; set; }
    #region Variables
    private FinancialYear? finYear { get; set; } = new();
    MudNumericField<int>? txtCode;
    MudDatePicker? dtName;
    MudDatePicker? dtFrom;
    MudDatePicker? dtTo;
    private MudDataGrid<FinancialYear>? grid;
    private WebUserPrivilege? privilege;
    #endregion

    async Task KeyDown(KeyboardEventArgs e, int fieldId)
    {
        if (e.Key == "Enter" || e.Key == "ArrowDown")
            await DownKeyEvent(fieldId);
        else if (e.Key == "ArrowUp")
        {
            if (fieldId == 2) await txtCode!.FocusAsync();
            else if (fieldId == 3) await dtName!.FocusAsync();
            else if (fieldId == 4) await dtFrom!.FocusAsync();
        }
    }

    private async Task DownKeyEvent(int fieldId)
    {
        if (fieldId == 1) await dtName!.FocusAsync();
        else if (fieldId == 2) await dtFrom!.FocusAsync();
        else if (fieldId == 3) await dtTo!.FocusAsync();
    }

    #region Events
    protected override async Task OnInitializedAsync()
    {
        if (AppData.privileges is not null)
            privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "FinancilaYearTitle" || x.Caption == "FinancialYear");
    }

    private async Task<GridData<FinancialYear>> LoadServerData(GridState<FinancialYear> state)
    {
        var pagination = new PaginationModel(
           page: state.Page + 1,
            pageSize: state.PageSize,
            sortColumn: state.SortDefinitions?.FirstOrDefault()?.SortBy,
            sortDirection: state.SortDefinitions?.FirstOrDefault()?.Descending == true ? "desc" : "asc",
            search: null
        );
        var result = await api.GetFinancialYearPDetails(pagination);

        return new GridData<FinancialYear>
        {
            Items = result.PFinancialYears,
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
            if (finYear?.FinancialYearId == 0)
            {
                if (privilege is not null && !privilege.SaveData) { Dialog.DialogMethod(Localizer("NoSavePrivilege")); return; }
                var result = await api.CreateFinancialYear(finYear);
                if (result.Message == null)
                {
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
                var result = await api.UpdateFinancialYear(finYear!);
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
                var result = await api.DeleteFinancialYear(finYear!.FinancialYearId);
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
        finYear = new();
        _selectedYearDate = null;
        finYear.RefNo = await api.GetMaxRefNo();
        txtCode?.FocusAsync();
        if (grid is not null) await grid.ReloadServerData();
    }
    #region Date Logic
    private DateTime? _selectedYearDate;
    private void OnYearChanged(DateTime? date)
    {
        _selectedYearDate = date;
        if (date.HasValue)
        {
            finYear!.Name = date.Value.Year.ToString();
        }
    }
    #endregion

    async Task RowClicked(DataGridRowClickEventArgs<FinancialYear> args)
    {

        finYear = args.Item;
        if (int.TryParse(finYear.Name, out int y))
        {
            _selectedYearDate = new DateTime(y, 1, 1);
        }
        else
        {
            _selectedYearDate = null;
        }
        await txtCode!.FocusAsync();
    }
    #endregion
}
