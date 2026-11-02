using Microsoft.AspNetCore.Components.Web;

namespace AshproSAASERP.Shared.Pages.Masters
{
    public partial class StoreMasterView : ITabAware
    {
        [Parameter] public int TabId { get; set; }
        [Inject] IViewService? api { get; set; }
        [Inject] ISnackbar? SnackBar { get; set; }
        [Inject] IDialogService? Dialog { get; set; }
        [Inject] ModelService? ModelDialog { get; set; }

        private bool IsLoading = false;
        private string searchText = "";
        private StoreMaster store = new();
        private MudDataGrid<StoreMaster>? grid;
        private bool isDefaultCheckboxReadOnly = false;
        private WebUserPrivilege? privilege;

        private List<BranchMaster> branches = new();

        MudTextField<string>? txtCode;
        MudTextField<string>? txtName;
        MudSelect<int?>? ddlBranch;
        MudCheckBox<bool>? chkDefault;
        MudTextField<string>? txtRemarks;

        async Task KeyDown(KeyboardEventArgs e, int fieldId)
        {
            if (e.Key == "Enter" || e.Key == "ArrowDown")
                await DownKeyEvent(fieldId);
            else if (e.Key == "ArrowUp")
            {
                if (fieldId == 2) await txtCode!.FocusAsync();
                else if (fieldId == 3) await txtName!.FocusAsync();
                else if (fieldId == 4) await ddlBranch!.FocusAsync();
                else if (fieldId == 5) await txtRemarks!.FocusAsync();
            }
        }

        private async Task DownKeyEvent(int fieldId)
        {
            if (fieldId == 1) await txtName!.FocusAsync();
            else if (fieldId == 2)
            {
                if (branches.Count > 0) await ddlBranch!.FocusAsync();
                else await txtRemarks!.FocusAsync(); // Skip checkbox
            }
            else if (fieldId == 3) await txtRemarks!.FocusAsync(); // Skip checkbox
            else if (fieldId == 4) await txtRemarks!.FocusAsync();
        }

        protected override async Task OnInitializedAsync()
        {
            if (AppData.privileges is not null)
                privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "Store Master" || x.Caption == "StoreMaster");
            branches = await api!.GetAllBranches();
            await LoadData();
            if (store.StoreId == 0)
            {
                store.FK_BranchId = AppData.BranchId;
            }
            await CheckDefaultStoreStatus();
        }

        private async Task CheckDefaultStoreStatus()
        {
            try
            {
                if (store.StoreId == 0)
                {
                    isDefaultCheckboxReadOnly = await api!.CheckIfDefaultStoreExists(0, store.FK_BranchId);
                }
                else
                {

                    if (store.IsDefaultStore)
                    {
                        isDefaultCheckboxReadOnly = false;
                    }
                    else
                    {
                        // Check others
                        isDefaultCheckboxReadOnly = await api!.CheckIfDefaultStoreExists(store.StoreId, store.FK_BranchId);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async Task OnBranchChanged(int? value)
        {
            store.FK_BranchId = value;
            await CheckDefaultStoreStatus();
        }


        private bool _isSaving = false;

        private async Task Save()
        {
            if (_isSaving) return;
            _isSaving = true;
            StateHasChanged();
            try
            {
                IsLoading = true;
                if (store.StoreId == 0)
                {
                    if (privilege is not null && !privilege.SaveData) { ModelDialog?.DialogMethod(Localizer("NoSavePrivilege")); return; }
                    var res = await api!.CreateStore(store);
                    if (res.Id > 0)
                    {
                        ModelDialog?.DialogMethod(Localizer("SavedSuccessfully"));
                        await Clear();
                        await LoadData();
                    }
                    else
                    {
                        ModelDialog?.DialogMethod(Localizer(res.Message!));
                    }
                }
                else
                {
                    if (privilege is not null && !privilege.EditData) { ModelDialog?.DialogMethod(Localizer("NoEditPrivilege")); return; }
                    var res = await api!.UpdateStore(store);
                    if (res.Id > 0)
                    {
                        ModelDialog?.DialogMethod(Localizer("UpdatedSuccessfully"));
                        await Clear();
                        await LoadData();
                    }
                    else
                    {
                        ModelDialog?.DialogMethod(Localizer(res.Message!));
                    }
                }
            }
            catch (Exception ex)
            {
                snackbar.Add(ex.Message, Severity.Error);
            }
            finally
            {
                IsLoading = false;
                _isSaving = false;
                StateHasChanged();
            }
        }

        private async Task Clear()
        {
            store = new StoreMaster();
            store.FK_BranchId = AppData.BranchId;
            await CheckDefaultStoreStatus();
        }

        private List<StoreMaster> storeList = new();

        private async Task LoadData()
        {
            storeList = await api!.GetAllStores();
        }

        private async Task RowClicked(DataGridRowClickEventArgs<StoreMaster> args)
        {

            store = await api!.GetStoreById(args.Item.StoreId);
            await CheckDefaultStoreStatus();
        }

        private async Task Delete()
        {
            try
            {
                if (privilege is not null && !privilege.DeleteData) { ModelDialog?.DialogMethod(Localizer("NoDeletePrivilege")); return; }
                if (!await ModelDialog!.DeleteDialogMethod())
                    return;
                var result = await api!.DeleteStore(store.StoreId);
                if (result.Message == null)
                {
                    await Clear();
                    await LoadData();
                    StateHasChanged();
                }
                else
                {
                    if (result.Message.Contains("FK_", StringComparison.OrdinalIgnoreCase) || result.Message.Contains("REFERENCE", StringComparison.OrdinalIgnoreCase) || result.Message.Contains("Constraint", StringComparison.OrdinalIgnoreCase))
                        ModelDialog?.DialogMethod(ArabicLocalization.GetFriendlyForeignKeyError(result.Message));
                    else
                        ModelDialog?.DialogMethod(result.Message);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("FK_", StringComparison.OrdinalIgnoreCase) || ex.Message.Contains("REFERENCE", StringComparison.OrdinalIgnoreCase) || ex.Message.Contains("Constraint", StringComparison.OrdinalIgnoreCase))
                    ModelDialog?.DialogMethod(ArabicLocalization.GetFriendlyForeignKeyError(ex.Message));
                else
                    ModelDialog?.DialogMethod(ex.Message);
            }
        }

        private bool FilterFunc(StoreMaster element)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return true;
            if (element.Code?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true)
                return true;
            if (element.Name?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true)
                return true;
            if (element.Branch?.Name?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true)
                return true;
            return false;
        }
    }
}
