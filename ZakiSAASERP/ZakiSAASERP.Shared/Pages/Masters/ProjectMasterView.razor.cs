using Microsoft.AspNetCore.Components.Web;

namespace ZakiSAASERP.Shared.Pages.Masters
{
    public partial class ProjectMasterView : ITabAware
    {
        [Parameter] public int TabId { get; set; }
        #region Variables

        private ProjectMaster project { get; set; } = new();
        private List<ProjectMaster> projectList = new();

        private List<PurchaserMaster> purchaserList = new();
        private List<CustomerMaster> customerList = new();
        private List<SalesManMaster> salesmanList = new();

        private string? strPurchaserName;
        private string? strCustomerName;
        private string? strCustomerCode;

        private string? strSalesManName;

        private string? strSalesManCode;
        private string? strPurchaserCode;

        private string searchText = "";
        MudTextField<string>? txtCode;
        MudTextField<string>? txtName;
        MudNumericField<decimal?>? numValue;
        MudDatePicker? dtExStart;
        MudDatePicker? dtExEnd;
        MudDatePicker? dtAcStart;
        MudDatePicker? dtAcEnd;
        MudTextField<string>? txtCustName;
        MudTextField<string>? txtPurchName;
        MudTextField<string>? txtSalesName;
        MudTextField<string>? txtStatus;
        MudTextField<string>? txtRemarks;
        private WebUserPrivilege? privilege;

        async Task KeyDown(KeyboardEventArgs e, int fieldId)
        {
            if (e.Key == "Enter" || e.Key == "ArrowDown")
                await DownKeyEvent(fieldId);
            else if (e.Key == "ArrowUp")
            {
                if (fieldId == 2) await txtCode!.FocusAsync();
                else if (fieldId == 3) await txtName!.FocusAsync();
                else if (fieldId == 4) await numValue!.FocusAsync();
                else if (fieldId == 5) await dtExStart!.FocusAsync();
                else if (fieldId == 6) await dtExEnd!.FocusAsync();
                else if (fieldId == 7) await dtAcStart!.FocusAsync();
                else if (fieldId == 8) await dtAcEnd!.FocusAsync();
                else if (fieldId == 9) await txtCustName!.FocusAsync();
                else if (fieldId == 10) await txtPurchName!.FocusAsync();
                else if (fieldId == 11) await txtSalesName!.FocusAsync();
                else if (fieldId == 12) await txtStatus!.FocusAsync();
            }
            else if (e.Key == "Space")
            {
                if (fieldId == 8) await OpenCustomerSearch();
                else if (fieldId == 9) await OpenPurchaserSearch();
                else if (fieldId == 10) await OpenSalesManSearch();
            }
        }

        private async Task DownKeyEvent(int fieldId)
        {
            if (fieldId == 1) await txtName!.FocusAsync();
            else if (fieldId == 2) await numValue!.FocusAsync();
            else if (fieldId == 3) await dtExStart!.FocusAsync();
            else if (fieldId == 4) await dtExEnd!.FocusAsync();
            else if (fieldId == 5) await dtAcStart!.FocusAsync();
            else if (fieldId == 6) await dtAcEnd!.FocusAsync();
            else if (fieldId == 7) await txtCustName!.FocusAsync();
            else if (fieldId == 8) await txtPurchName!.FocusAsync();
            else if (fieldId == 9) await txtSalesName!.FocusAsync();
            else if (fieldId == 10) await txtStatus!.FocusAsync();
            else if (fieldId == 11) await txtRemarks!.FocusAsync();
        }

        #endregion

        #region Init
        protected override async Task OnInitializedAsync()
        {
            try
            {
                if (AppData.privileges is not null)
                    privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "ProjectMaster");

                projectList = await api.GetAllProject();

                // Use IndexedDB caching for faster dropdown loading
                salesmanList = await api.GetAllSalesMan();
                purchaserList = await api.GetAllPurchaser();
                customerList = await api.GetAllCustomers();

                NewClick();
            }
            catch (Exception ex)
            {
                Dialog.DialogMethod(ex.Message);
            }
        }

        private IEnumerable<ProjectMaster> filteredList =>
            string.IsNullOrWhiteSpace(searchText)
                ? projectList
                : projectList.Where(x =>
                    (x.Code?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (x.Name?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false));

        #endregion

        #region CRUD

        private bool _isSaving = false;

        async Task SaveOrUpdate()
        {
            if (_isSaving) return;
            _isSaving = true;
            StateHasChanged();
            try
            {
                bool exists = projectList.Any(x =>
                    x.Code.Equals(project.Code, StringComparison.OrdinalIgnoreCase) &&
                    x.ProjectId != project.ProjectId);

                if (exists)
                {
                    Dialog.DialogMethod(Localizer("ProjectCodeExists"));
                    return;
                }

                if (project.ProjectId == 0)
                {
                    if (privilege is not null && !privilege.SaveData) { Dialog.DialogMethod(Localizer("NoSavePrivilege")); return; }
                    var response = await api.CreateProject(project);

                    if (response.Message == null)
                    {
                        project.ProjectId = response.Id;
                        projectList.Add(project);

                        Dialog.DialogMethod(Localizer("SaveMessage"));
                        NewClick();
                    }
                    else
                        Dialog.DialogMethod(response.Message);
                }
                else
                {
                    if (privilege is not null && !privilege.EditData) { Dialog.DialogMethod(Localizer("NoEditPrivilege")); return; }
                    var response = await api.UpdateProject(project);

                    if (response.Message == null)
                    {
                        var index = projectList.FindIndex(x => x.ProjectId == project.ProjectId);
                        if (index >= 0) projectList[index] = project;

                        Dialog.DialogMethod(Localizer("UpdateMessage"));
                        NewClick();
                    }
                    else
                        Dialog.DialogMethod(response.Message);
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

                var response = await api.DeleteProject(project.ProjectId);

                if (response.Message == null)
                {
                    projectList.RemoveAll(x => x.ProjectId == project.ProjectId);
                    NewClick();
                }
                else
                {
                    if (response.Message.Contains("FK_", StringComparison.OrdinalIgnoreCase) || response.Message.Contains("REFERENCE", StringComparison.OrdinalIgnoreCase) || response.Message.Contains("Constraint", StringComparison.OrdinalIgnoreCase))
                        Dialog.DialogMethod(ArabicLocalization.GetFriendlyForeignKeyError(response.Message));
                    else
                        Dialog.DialogMethod(response.Message);
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

        void NewClick()
        {
            project = new ProjectMaster();
            strPurchaserName = "";
            strCustomerName = "";
            strSalesManName = "";

            txtCode?.FocusAsync();
        }

        #endregion

        #region Grid Click

        async Task RowClicked(DataGridRowClickEventArgs<ProjectMaster> args)
        {

            try
            {
                project = args.Item;

                project = await api.GetProjectById(project.ProjectId);

                if (project == null) return;

                strPurchaserName = purchaserList
                    .FirstOrDefault(x => x.PurchaserId == project.FK_PurchaserId)?.Name;

                strSalesManName = salesmanList
                    .FirstOrDefault(x => x.SalesmanId == project.FK_SalesmanId)?.Name;

                StateHasChanged();
            }
            catch (Exception ex)
            {
                Dialog.DialogMethod(ex.Message);
            }
        }

        #endregion

        #region Purchaser Search

        async Task OpenPurchaserSearch()
        {
            try
            {
                var parameters = new DialogParameters
                {
                    ["PurchaserList"] = purchaserList
                };

                var options = new DialogOptions
                {
                    BackdropClick = false,
                    MaxWidth = MaxWidth.Small
                };

                var dialog = DialogService.Show<PurchaserSearchDialog>("Purchaser Search", parameters, options);
                var result = await dialog.Result;

                if (!result.Canceled)
                {
                    var pur = (PurchaserMaster)result.Data;
                    project.FK_PurchaserId = pur.PurchaserId;
                    strPurchaserName = pur.Name;
                    strPurchaserCode = pur.Code;
                }
            }
            catch (Exception ex)
            {
                Dialog.DialogMethod(ex.Message);
            }
        }

        async Task OnPurchaserKeyDown(KeyboardEventArgs e)
        {
            if (e.Code == "Space")
                await OpenPurchaserSearch();
        }

        #endregion

        #region Salesman Search

        async Task OpenSalesManSearch()
        {
            try
            {
                var parameters = new DialogParameters
                {
                    ["SalesManList"] = salesmanList
                };

                var options = new DialogOptions
                {
                    BackdropClick = false,
                    MaxWidth = MaxWidth.Small
                };

                var dialog = DialogService.Show<SalesManSearchDialog>("Salesman Search", parameters, options);
                var result = await dialog.Result;

                if (!result.Canceled)
                {
                    var sales = (SalesManMaster)result.Data;

                    project.FK_SalesmanId = sales.SalesmanId;
                    strSalesManName = sales.Name;
                    strSalesManCode = sales.Code;
                }
            }
            catch (Exception ex)
            {
                Dialog.DialogMethod(ex.Message);
            }
        }

        async Task OnSalesManKeyDown(KeyboardEventArgs e)
        {
            if (e.Code == "Space")
                await OpenSalesManSearch();
        }

        #endregion

        #region Customer Search (If you want later)

        async Task OpenCustomerSearch()
        {
            try
            {
                var parameters = new DialogParameters
                {
                    ["CustomerList"] = customerList
                };

                var options = new DialogOptions
                {
                    BackdropClick = false,
                    MaxWidth = MaxWidth.Small
                };

                var dialog = DialogService.Show<CustomerSearchDialog>("Customer Search", parameters, options);
                var result = await dialog.Result;

                if (!result.Canceled)
                {
                    var customer = (CustomerMaster)result.Data;

                    project.FK_CustomerId = customer.CustomerId;
                    strCustomerName = customer.Name;
                    strCustomerCode = customer.Code;
                }
            }
            catch (Exception ex)
            {
                Dialog.DialogMethod(ex.Message);
            }


        }

        async Task OnCustomerKeyDown(KeyboardEventArgs e)
        {
            if (e.Code == "Space")
                await OpenCustomerSearch();
        }

        #endregion
    }
}
