using Microsoft.AspNetCore.Components.Web;

namespace AshproSAASERP.Shared.Pages.Masters
{
    public partial class CountryMasterView : ITabAware
    {
        [Parameter] public int TabId { get; set; }
        private CountryMaster country { get; set; } = new();
        private string searchText = "";
        MudTextField<string>? txtCode;
        MudTextField<string>? txtName;
        MudTextField<string>? txtNameArabic;
        private MudDataGrid<CountryMaster>? grid;
        private WebUserPrivilege? privilege;

        protected override async Task OnInitializedAsync()
        {
            if (AppData.privileges is not null)
                privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "CountryMaster");
        }
        private async Task<GridData<CountryMaster>> ServerData(GridState<CountryMaster> state) =>
            await LoadServerData(state);

        private async Task<GridData<CountryMaster>> LoadServerData(GridState<CountryMaster> state)
        {
            var pagination = new PaginationModel(
                page: state.Page + 1,
                pageSize: state.PageSize,
                sortColumn: state.SortDefinitions?.FirstOrDefault()?.SortBy,
                sortDirection: state.SortDefinitions?.FirstOrDefault()?.Descending == true ? "desc" : "asc",
                search: searchText
            );
            var result = await api.GetCountryPDetails(pagination);
            return new GridData<CountryMaster>
            {
                Items = result.PCountries,
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
                if (country.CountryId == 0)
                {
                    if (privilege is not null && !privilege.SaveData) { Dialog.DialogMethod(Localizer("NoSavePrivilege")); return; }
                    var result = await api.CreateCountry(country);

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
                    var result = await api.UpdateCountry(country);
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
                if (!await Dialog.DeleteDialogMethod())
                    return;
                var result = await api.DeleteCountry(country.CountryId);
                if (result.Message == null)
                {
                    await NewClick();
                    StateHasChanged();
                }
                else
                {
                    if (result.Message.Contains("FK_", StringComparison.OrdinalIgnoreCase) || result.Message.Contains("REFERENCE", StringComparison.OrdinalIgnoreCase) || result.Message.Contains("Constraint", StringComparison.OrdinalIgnoreCase))
                        Dialog.DialogMethod(ArabicLocalization.GetFriendlyForeignKeyError(result.Message));
                    else
                        Dialog.DialogMethod(result.Message);
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
            try
            {
                country = new CountryMaster();
                txtCode?.FocusAsync();
                if (grid is not null) await grid.ReloadServerData();
            }
            catch (Exception ex)
            {
                Dialog.DialogMethod(ex.Message);
            }
        }
        async Task RowClicked(DataGridRowClickEventArgs<CountryMaster> args)
        {

            try
            {
                country = args.Item;
                await txtCode!.FocusAsync();
                await txtCode.SelectAsync();
            }
            catch (Exception ex)
            {
                Dialog.DialogMethod(ex.Message);
            }
        }
        private async Task ToArabic()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(country.Name))
                {
                    Dialog.DialogMethod(Localizer("EnterNameFirst"));
                    return;
                }

                country.NameArabic = await api.GetArabicText(country.Name);
            }
            catch (Exception ex)
            {
                Dialog.DialogMethod(ex.Message);
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
                await txtNameArabic!.FocusAsync();
            }
        }
    }
}
