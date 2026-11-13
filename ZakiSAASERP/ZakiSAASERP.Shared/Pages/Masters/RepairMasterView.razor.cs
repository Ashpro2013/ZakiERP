using ZakiSAASERP.Shared.UserControl;
using Microsoft.AspNetCore.Components.Forms;

namespace ZakiSAASERP.Shared.Pages.Masters
{
    public partial class RepairMasterView : ITabAware
    {
        [Parameter] public int TabId { get; set; }
        [Inject] ISnackbar Snackbar { get; set; } = null!;
        private RepairMaster repair = new();
        private MudDataGrid<RepairMaster> grid;
        private string searchText = "";
        private EditForm form;
        private InfoTextField<string> _codeField;
        private WebUserPrivilege? privilege;

        protected override async Task OnInitializedAsync()
        {
            if (AppData.privileges is not null)
                privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "RepairMaster");
            await LoadNewDefaults();
        }

        private async Task LoadNewDefaults()
        {
            repair = new RepairMaster();
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
            if (repair.RepairId == 0) return;

            if (privilege is not null && !privilege.DeleteData) { await Dialog.DialogMethod(Localizer("NoDeletePrivilege")); return; }

            var confirm = await DialogService.ShowMessageBox(
                Localizer("Warning"),
                Localizer("AreYouSureDelete"),
                yesText: Localizer("Delete"), cancelText: Localizer("Cancel"));

            if (confirm == true)
            {
                var res = await api.DeleteRepair(repair.RepairId);
                if (res.Id > 0)
                {
                    Snackbar.Add(Localizer("RepairDeletedSuccess"), Severity.Success);
                    await NewClick();
                    await grid.ReloadServerData();
                }
                else
                {
                    Snackbar.Add(Localizer(res.Message), Severity.Error);
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
                if (repair.RepairId == 0)
                {
                    if (privilege is not null && !privilege.SaveData) { await Dialog.DialogMethod(Localizer("NoSavePrivilege")); return; }
                    var res = await api.CreateRepair(repair);
                    if (res.Id > 0)
                    {
                        Snackbar.Add(Localizer("RepairCreatedSuccess"), Severity.Success);
                        await NewClick();
                        await grid.ReloadServerData();
                    }
                    else
                    {
                        Snackbar.Add(Localizer(res.Message), Severity.Error);
                    }
                }
                else
                {
                    if (privilege is not null && !privilege.EditData) { await Dialog.DialogMethod(Localizer("NoEditPrivilege")); return; }
                    var res = await api.UpdateRepair(repair);
                    if (res.Id > 0)
                    {
                        Snackbar.Add(Localizer("RepairUpdatedSuccess"), Severity.Success);
                        await NewClick();
                        await grid.ReloadServerData();
                    }
                    else
                    {
                        Snackbar.Add(Localizer(res.Message), Severity.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add(ex.Message, Severity.Error);
            }
            finally
            {
                _isSaving = false;
                StateHasChanged();
            }
        }

        private void RowClicked(DataGridRowClickEventArgs<RepairMaster> args)
        {
            repair = new RepairMaster
            {
                RepairId = args.Item.RepairId,
                Code = args.Item.Code,
                Name = args.Item.Name,
                Remarks = args.Item.Remarks
            };
        }

        private async Task<GridData<RepairMaster>> LoadServerData(GridState<RepairMaster> state)
        {
            var pagination = new PaginationModel
            {
                page = state.Page + 1,
                pageSize = state.PageSize,
                search = searchText,
                sortColumn = state.SortDefinitions.FirstOrDefault()?.SortBy ?? "",
                sortDirection = state.SortDefinitions.FirstOrDefault()?.Descending == true ? "desc" : "asc"
            };

            var data = await api.GetRepairPDetails(pagination);
            return new GridData<RepairMaster>
            {
                TotalItems = data.TotalCount,
                Items = data.Items
            };
        }
    }
}
