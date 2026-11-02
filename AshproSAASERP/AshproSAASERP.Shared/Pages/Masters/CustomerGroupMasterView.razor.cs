using Microsoft.AspNetCore.Components.Web;
namespace AshproSAASERP.Shared.Pages.Masters
{
    public partial class CustomerGroupMasterView : ITabAware
    {
        [Parameter] public int TabId { get; set; }
        private CustomerGroupMaster customerGroup { get; set; } = new();
        private string searchText = "";
        MudTextField<string>? txtCode;
        MudTextField<string>? txtName;
        MudTextField<string>? txtNameArabic;
        MudTextField<string>? txtRemarks;
        private MudDataGrid<CustomerGroupMaster>? grid;
        private WebUserPrivilege? privilege;

        protected override async Task OnInitializedAsync()
        {
            if (AppData.privileges is not null)
                privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "CustomerGroupMaster");
        }

        private async Task<GridData<CustomerGroupMaster>> LoadServerData(GridState<CustomerGroupMaster> state)
        {
            try
            {
                var pagination = new PaginationModel(
                   page: state.Page + 1,
                    pageSize: state.PageSize,
                    sortColumn: state.SortDefinitions?.FirstOrDefault()?.SortBy,
                    sortDirection: state.SortDefinitions?.FirstOrDefault()?.Descending == true ? "desc" : "asc",
                    search: searchText
                );
                var result = await api.GetCustomerGroupPDetails(pagination);
                return new GridData<CustomerGroupMaster>
                {
                    Items = result?.PCustomerGroups ?? new List<CustomerGroupMaster>(),
                    TotalItems = result?.TotalCount ?? 0
                };
            }
            catch (Exception ex)
            {
                Dialog.DialogMethod(ex.Message);
                return new GridData<CustomerGroupMaster>();
            }
        }

        private async Task OnSearch(string text)
        {
            searchText = text;
            if (grid is not null)
                await grid.ReloadServerData();
            StateHasChanged();
        }
        private bool _isSaving = false;

        async Task SaveOrUpdate()
        {
            if (_isSaving) return;
            _isSaving = true;
            StateHasChanged();
            try
            {
                if (customerGroup.CustomerGroupId == 0)
                {
                    if (privilege is not null && !privilege.SaveData) { Dialog.DialogMethod(Localizer("NoSavePrivilege")); return; }
                    var result = await api.CreateCustomerGroup(customerGroup);
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
                    var result = await api.UpdateCustomerGroup(customerGroup);
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
                if (!await Dialog.DeleteDialogMethod())
                    return;

                if (privilege is not null && !privilege.DeleteData) { Dialog.DialogMethod(Localizer("NoDeletePrivilege")); return; }

                var result = await api.DeleteCustomerGroup(customerGroup.CustomerGroupId);
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
                customerGroup = new CustomerGroupMaster();
                txtCode?.FocusAsync();
                if (grid is not null) await grid.ReloadServerData();
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Dialog.DialogMethod(ex.Message);
            }
        }
        async Task RowClicked(DataGridRowClickEventArgs<CustomerGroupMaster> args)
        {

            try
            {
                customerGroup = args.Item;
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
                if (string.IsNullOrWhiteSpace(customerGroup.Name))
                {
                    Dialog.DialogMethod(Localizer("EnterNameFirst"));
                    return;
                }
                customerGroup.NameArabic = await api.GetArabicText(customerGroup.Name);
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
                else if (fieldId == 4)
                {
                    await txtNameArabic!.FocusAsync();
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
            else if (fieldId == 3)
            {
                await txtRemarks!.FocusAsync();
            }
        }
    }
}
