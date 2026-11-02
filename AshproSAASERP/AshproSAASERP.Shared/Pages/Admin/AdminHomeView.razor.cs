using Microsoft.AspNetCore.Components.Web;
namespace AshproSAASERP.Shared.Pages.Admin;

public partial class AdminHomeView
{
    #region Variables
    private ExLCompany? lCompany { get; set; } = new();
    List<LoginCompany> companyList = new();
    MudTextField<string>? txtCode;
    MudTextField<string>? txtName;
    MudTextField<string>? txtAddress;
    MudTextField<string>? txtContactNo;
    MudTextField<string>? txtEmail;
    string[] grouplist = { "Trial", "Standard", "Pro", "Premium" };
    private MudDataGrid<LoginCompany>? grid;
    string PasswordInputIcon = Icons.Material.Filled.VisibilityOff;
    bool isShow;
    InputType PasswordInput = InputType.Password;
    #endregion

    #region Events
    void ButtonTestclick()
    {
        if (isShow)
        {
            isShow = false;
            PasswordInputIcon = Icons.Material.Filled.VisibilityOff;
            PasswordInput = InputType.Password;
        }
        else
        {
            isShow = true;
            PasswordInputIcon = Icons.Material.Filled.Visibility;
            PasswordInput = InputType.Text;
        }
    }
    private async Task<GridData<LoginCompany>> LoadServerData(GridState<LoginCompany> state)
    {
        var pagination = new PaginationModel(
           page: state.Page + 1,
            pageSize: state.PageSize,
            sortColumn: state.SortDefinitions?.FirstOrDefault()?.SortBy,
            sortDirection: state.SortDefinitions?.FirstOrDefault()?.Descending == true ? "desc" : "asc",
            search: null
        );
        var result = await api.GetPAdminCompanies(pagination);
        return new GridData<LoginCompany>
        {
            Items = result.PLCompanies,
            TotalItems = result.TotalCount
        };
    }
    async Task SaveOrUpdate()
    {
        if (lCompany?.Id == 0)
        {
            try
            {
                var result = await api.CreateAdminCompany(lCompany);
                if (result is not null)
                {
                    companyList.Add(result);
                    await Dialog.DialogMethod(Localizer("SaveMessage"));
                    await NewClick();
                }
                else
                {
                    await Dialog.DialogMethod("Error");
                }
            }
            catch (Exception ex)
            {
                await Dialog.DialogMethod(ex.Message);
            }
        }
        else
        {
            try
            {
                var result = await api.UpdateAdminCompany(lCompany!);
                if (result is not null)
                {
                    var index = companyList.FindIndex(e => e.Id == lCompany?.Id);
                    companyList[index] = lCompany!;
                    await Dialog.DialogMethod(Localizer("UpdateMessage"));
                    await NewClick();
                }
                else
                {
                    await Dialog.DialogMethod("Error");
                }
            }
            catch (Exception ex)
            {
                await Dialog.DialogMethod(ex.Message);
            }
        }
    }
    async Task Delete()
    {
        if (await Dialog.DeleteDialogMethod())
        {
            var result = await api.DeleteAdminCompany(lCompany!.Id);
            if (result is not null)
            {
                var index = companyList.FindIndex(e => e.Id == lCompany?.Id);
                companyList.RemoveAt(index);
                await NewClick();
            }
            else
            {
                await Dialog.DialogMethod("Error");
            }
        }
        else
        {
            await NewClick();
        }
    }
    async Task NewClick()
    {
        lCompany = new();
        txtCode?.FocusAsync();
        await grid!.ReloadServerData();
    }
    async Task RowClicked(DataGridRowClickEventArgs<LoginCompany> args)
    {
        lCompany = args.Item.CopyTo<ExLCompany>();
        await txtCode!.FocusAsync();
    }
    async Task KeyDown(KeyboardEventArgs e, int fieldId)
    {
        if (e.Key == "Enter" || e.Key == "ArrowDown")
            await DownKeyEvent(fieldId);
        else if (e.Key == "ArrowUp")
        {
            if (fieldId == 2) await txtCode!.FocusAsync();
            else if (fieldId == 3) await txtName!.FocusAsync();
            else if (fieldId == 4) await txtAddress!.FocusAsync();
            else if (fieldId == 5) await txtContactNo!.FocusAsync();
        }
    }
    private async Task DownKeyEvent(int fieldId)
    {
        if (fieldId == 1) await txtName!.FocusAsync();
        else if (fieldId == 2) await txtAddress!.FocusAsync();
        else if (fieldId == 3) await txtContactNo!.FocusAsync();
        else if (fieldId == 4) await txtEmail!.FocusAsync();
    }
    #endregion
}
