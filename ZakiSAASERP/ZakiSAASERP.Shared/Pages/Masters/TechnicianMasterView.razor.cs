using Microsoft.AspNetCore.Components.Forms;

namespace ZakiSAASERP.Shared.Pages.Masters
{
    public partial class TechnicianMasterView : ITabAware
    {
        [Parameter] public int TabId { get; set; }
        [Inject] ISnackbar Snackbar { get; set; } = null!;
        private TechnicianMaster technician = new();
        private MudDataGrid<TechnicianMaster>? grid;
        private string searchText = "";
        private EditForm? form;
        private InfoTextField<string>? _codeField;
        private WebUserPrivilege? privilege;

        protected override async Task OnInitializedAsync()
        {
            if (AppData.privileges is not null)
                privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "TechnicianMaster");
            await LoadNewDefaults();
        }

        private async Task LoadNewDefaults()
        {
            technician = new TechnicianMaster();
            // Optional: Fetch next code
            try
            {
                // var nextCode = await api.GetTechnicianNextCode();
                // technician.Code = nextCode; // If applicable
            }
            catch { }
            await Task.CompletedTask;
        }

        private async Task NewClick()
        {
            await LoadNewDefaults();
            StateHasChanged();
            if (_codeField is not null)
            {
                await _codeField.FocusAsync();
            }
        }

        private async Task Delete()
        {
            if (technician.TechnicianId == 0) return;

            if (privilege is not null && !privilege.DeleteData) { Snackbar.Add(Localizer("NoDeletePrivilege"), Severity.Warning); return; }

            if (await Dialog.DeleteDialogMethod())
            {
                var res = await api.DeleteTechnician(technician.TechnicianId);
                if (res.Id > 0)
                {
                    Snackbar.Add(Localizer("TechnicianDeletedSuccess"), Severity.Success);
                    await NewClick();
                    await grid!.ReloadServerData();
                }
                else
                {
                    await Dialog.DialogMethod(Localizer(res.Message!));
                }
            }
        }

        private bool _isSaving = false;

        private async Task SaveOrUpdate()
        {
            if (_isSaving) return;

            _isSaving = true;
            StateHasChanged();

            try
            {
                if (technician.TechnicianId == 0)
                {
                    if (privilege is not null && !privilege.SaveData) { Snackbar.Add(Localizer("NoSavePrivilege"), Severity.Warning); return; }
                    var res = await api.CreateTechnician(technician);
                    if (res.Id > 0)
                    {
                        Snackbar.Add(Localizer("TechnicianCreatedSuccess"), Severity.Success);
                        await NewClick();
                        await grid!.ReloadServerData();
                    }
                    else
                    {
                        await Dialog.DialogMethod(Localizer(res.Message!));
                    }
                }
                else
                {
                    if (privilege is not null && !privilege.EditData) { Snackbar.Add(Localizer("NoEditPrivilege"), Severity.Warning); return; }
                    var res = await api.UpdateTechnician(technician);
                    if (res.Id > 0)
                    {
                        Snackbar.Add(Localizer("TechnicianUpdatedSuccess"), Severity.Success);
                        await NewClick();
                        await grid!.ReloadServerData();
                    }
                    else
                    {
                        await Dialog.DialogMethod(Localizer(res.Message!));
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

        private void RowClicked(DataGridRowClickEventArgs<TechnicianMaster> args)
        {
            technician = new TechnicianMaster
            {
                TechnicianId = args.Item.TechnicianId,
                Code = args.Item.Code,
                Name = args.Item.Name,
                Position = args.Item.Position,
                Mobile = args.Item.Mobile,
                Remarks = args.Item.Remarks
            };
        }

        private async Task<GridData<TechnicianMaster>> LoadServerData(GridState<TechnicianMaster> state)
        {
            var pagination = new PaginationModel
            {
                page = state.Page + 1,
                pageSize = state.PageSize,
                search = searchText,
                sortColumn = state.SortDefinitions.FirstOrDefault()?.SortBy ?? "",
                sortDirection = state.SortDefinitions.FirstOrDefault()?.Descending == true ? "desc" : "asc"
            };

            var data = await api.GetTechnicianPDetails(pagination);
            return new GridData<TechnicianMaster>
            {
                TotalItems = data.TotalCount,
                Items = data.Items
            };
        }
    }
}
