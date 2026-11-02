using Microsoft.AspNetCore.Components.Web;

namespace AshproSAASERP.Shared.Pages.Masters
{
    public partial class VatMasterView : ITabAware
    {
        [Parameter] public int TabId { get; set; }
        #region Variables
        private VatMaster? vatMaster { get; set; } = new();
        List<VatMaster> vatList = new();
        MudTextField<string>? txtCode;
        MudTextField<string>? txtName;
        MudNumericField<decimal?>? numPercentage;
        private string searchTerm = string.Empty;
        private MudDataGrid<VatMaster>? grid;
        private WebUserPrivilege? privilege;
        private bool _isSaving = false;

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
            else if (fieldId == 2) await numPercentage!.FocusAsync();
        }
        #endregion

        #region Events
        protected override async Task OnInitializedAsync()
        {
            if (AppData.privileges is not null)
                privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "VatMaster");
        }
        private async Task<GridData<VatMaster>> LoadServerData(GridState<VatMaster> state)
        {
            var pagination = new PaginationModel(
               page: state.Page + 1,
                pageSize: state.PageSize,
                sortColumn: state.SortDefinitions?.FirstOrDefault()?.SortBy,
                sortDirection: state.SortDefinitions?.FirstOrDefault()?.Descending == true ? "desc" : "asc",
                search: null
            );
            var result = await api.GetVatPDetails(pagination);

            return new GridData<VatMaster>
            {
                Items = result.PVats,
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
                if (vatMaster == null)
                {
                    _isSaving = false;
                    StateHasChanged();
                    return;
                }

                if (vatMaster.IsDefault)
                {
                    var otherDefault = vatList.FirstOrDefault(x => x.IsDefault && x.VatId != vatMaster.VatId);

                    if (otherDefault is not null)
                    {
                        snackbar.Add(Localizer("DefaultVatExistsMessage"), Severity.Warning);
                        _isSaving = false;
                        StateHasChanged();
                        return;
                    }
                }


                if (vatMaster.VatId == 0)
                {

                    if (privilege is not null && !privilege.SaveData) { snackbar.Add(Localizer("NoSavePrivilege"), Severity.Warning); return; }
                    var result = await api.CreateVat(vatMaster);

                    if (result.Message == null)
                    {
                        vatMaster.VatId = result.Id;
                        vatList.Add(vatMaster);
                        snackbar.Add(Localizer("SaveMessage"));
                        await NewClick();
                    }
                    else
                    {
                        await Dialog.DialogMethod(result.Message);
                    }
                }
                else
                {
                    if (privilege is not null && !privilege.EditData) { snackbar.Add(Localizer("NoEditPrivilege"), Severity.Warning); return; }
                    var result = await api.UpdateVat(vatMaster!);

                    if (result.Message == null)
                    {
                        var index = vatList.FindIndex(x => x.VatId == vatMaster?.VatId);
                        if (index >= 0)
                            vatList[index] = vatMaster!;
                        snackbar.Add(Localizer("UpdateMessage"), Severity.Success);
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
                if (privilege is not null && !privilege.DeleteData) { snackbar.Add(Localizer("NoDeletePrivilege"), Severity.Warning); return; }
                if (await Dialog.DeleteDialogMethod())
                {
                    var result = await api.DeleteVat(vatMaster!.VatId);
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
            vatMaster = new VatMaster();
            txtCode?.FocusAsync();
            if (grid is not null) await grid.ReloadServerData();
        }

        async Task RowClicked(DataGridRowClickEventArgs<VatMaster> args)
        {
            if (args.MouseEventArgs.Detail != 2) return;
            vatMaster = args.Item;
            await txtCode!.FocusAsync();
            await txtCode!.SelectAsync();
            await InvokeAsync(StateHasChanged);
        }

        #endregion
    }
}
