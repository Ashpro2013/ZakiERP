using ZakiSAASERP.Models.User;

namespace ZakiSAASERP.Shared.Pages.Users;

public partial class UserPrevilageView : ITabAware
{
    #region Variables
    [Parameter] public int TabId { get; set; }
    UserPrivilegeHdr privilegeHdr = new();
    MudTextField<string>? textField;
    List<ExUserPrivilege> userPrivilages = new();
    List<ExFuncDetails> exFuncDetails = new();
    List<ExMasters> privileges = new();
    // WebUserPrivilege? privilege; // Removed
    #endregion

    #region Events
    protected async override Task OnInitializedAsync()
    {
        try
        {
            privilegeHdr = new();
            userPrivilages = new();
            exFuncDetails = new();
            privileges = new();
            var dto = await api.GetUserprivilege(0);
            privileges = await api.GetMasters("privileges", AppData.BranchId);
            if (dto is not null)
            {
                userPrivilages = dto.entUserPrivilegeDtls;
                exFuncDetails = dto.entFunctionalPrivilege;
            }
            StateHasChanged();
        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod(ex.Message);
        }
    }
    private async Task RowClicked(DataGridRowClickEventArgs<ExUserPrivilege> args)
    {
        try
        {
            var item = args.Item;
            item = await Dialog.PrivilegeDialog(item);
            var data = userPrivilages.FirstOrDefault(x => x.MenuId == item.MenuId);
            data!.Visible = item.Visible;
            data.SaveData = item.SaveData;
            data.EditData = item.EditData;
            data.DeleteData = item.DeleteData;
            StateHasChanged();
        }
        catch (Exception ex) { await Dialog.DialogMethod(ex.Message); }
    }
    private async Task FuncRowClicked(DataGridRowClickEventArgs<ExFuncDetails> args)
    {
        try
        {
            var item = args.Item;
            item = await Dialog.FuncPrivilegeDialog(item);
            var data = exFuncDetails.FirstOrDefault(x => x.EventCaption == item.EventCaption);
            data!.Enable = item.Enable;
            StateHasChanged();
        }
        catch (Exception ex) { await Dialog.DialogMethod(ex.Message); }
    }
    private void EnableChecked(bool isChecked)
    {
        try
        {
            userPrivilages.ForEach(x => { x.Visible = isChecked; });
            StateHasChanged();
        }
        catch (Exception)
        {

            throw;
        }
    }
    private void SaveChecked(bool isChecked)
    {
        try
        {
            userPrivilages.ForEach(x => { x.SaveData = isChecked; });
            StateHasChanged();
        }
        catch (Exception)
        {

            throw;
        }
    }
    private void EditChecked(bool isChecked)
    {
        try
        {
            userPrivilages.ForEach(x => { x.EditData = isChecked; });
            StateHasChanged();
        }
        catch (Exception)
        {

            throw;
        }
    }
    private void DeleteChecked(bool isChecked)
    {
        try
        {
            userPrivilages.ForEach(x => { x.DeleteData = isChecked; });
            StateHasChanged();
        }
        catch (Exception)
        {

            throw;
        }
    }
    private void FunEnableChecked(bool isChecked)
    {
        try
        {
            exFuncDetails.ForEach(x => { x.Enable = isChecked; });
            StateHasChanged();
        }
        catch (Exception)
        {

            throw;
        }
    }
    async Task SaveOrUpdate()
    {
        try
        {
            if (string.IsNullOrEmpty(privilegeHdr.Name)) { await Dialog.DialogMethod("Please enter name and try again!"); return; }
            userPrivilages.ForEach(x => { x.FK_MenuDetailsId = x.MenuId; x.FK_UserPrivilegeHdrId = privilegeHdr.Id; x.Visible = x.Visible; });
            privilegeHdr.FK_BranchId = AppData.BranchId;
            var dto = new UserPrivilegeDTO(privilegeHdr, userPrivilages, exFuncDetails);

            if (privilegeHdr.Id == 0)
            {
                var output = await api.NewUserprivilege(dto);
                if (string.IsNullOrEmpty(output.Message))
                {
                    Snackbar.Add(Localizer("SaveMessage"), Severity.Success);
                    await NewClick();
                }
                else
                {
                    await Dialog.DialogMethod(output.Message!);
                }
            }
            else
            {
                var output = await api.UpdateUserprivilege(dto);
                if (string.IsNullOrEmpty(output.Message))
                {
                    Snackbar.Add(Localizer("UpdateMessage"), Severity.Success);
                    await NewClick();
                }
                else
                {
                    await Dialog.DialogMethod(output.Message!);
                }
            }
        }
        catch (Exception ex) { await Dialog.DialogMethod(ex.Message); }
    }
    async Task Delete()
    {
        try
        {
            if (privilegeHdr.Id == 0) { return; }
            if (await Dialog.DeleteDialogMethod())
            {
                var output = await api.DeleteUserprivilege(privilegeHdr.Id);
                if (string.IsNullOrEmpty(output.Message))
                {
                    Snackbar.Add(Localizer("DeleteMessage"), Severity.Success);
                    await NewClick();
                }
                else
                {
                    await Dialog.DialogMethod(output.Message!);
                }
            }
        }
        catch (Exception)
        {

            throw;
        }
    }
    async Task NewClick()
    {
        try
        {
            privilegeHdr = new();
            userPrivilages = new();
            exFuncDetails = new();
            var dto = await api.GetUserprivilege(0);
            privileges = await api.GetMasters("privileges", AppData.BranchId);
            if (dto is not null)
            {
                userPrivilages = dto.entUserPrivilegeDtls;
                exFuncDetails = dto.entFunctionalPrivilege;
            }
            await textField!.FocusAsync();
            StateHasChanged();
        }
        catch (Exception)
        {

            throw;
        }
    }
    async Task SearchMethod()
    {
        try
        {
            var masters = await Dialog.TransactionSearchMethod("Privileges");
            if (masters is not null)
            {
                var dto = await api.GetUserprivilege(masters.Id);
                if (dto is not null)
                {
                    privilegeHdr = dto.entUserPrivilegeHdr;
                    userPrivilages = dto.entUserPrivilegeDtls;
                    exFuncDetails = dto.entFunctionalPrivilege;
                    StateHasChanged();
                }

            }
        }
        catch (Exception)
        {

            throw;
        }
    }
    #endregion
}
