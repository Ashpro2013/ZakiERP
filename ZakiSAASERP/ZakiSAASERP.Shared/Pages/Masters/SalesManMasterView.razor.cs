using ZakiSAASERP.Shared.Helpers;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;

namespace ZakiSAASERP.Shared.Pages.Masters;

public partial class SalesManMasterView : ITabAware
{
    #region  variables
    [Parameter] public int TabId { get; set; }
    private SalesManMaster salesman { get; set; } = new();
    private string? salesmanImagePreview;
    private string? signaturePreview;
    private string? sealPreview;
    private string searchText = "";
    MudTextField<string>? txtCode;
    MudTextField<string>? txtName;
    MudTextField<string>? txtNameArabic;
    MudTextField<string>? txtMobile;
    MudTextField<string>? txtEmail;
    MudTextField<string>? txtDesignation;
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
            else if (fieldId == 4) await txtNameArabic!.FocusAsync();
            else if (fieldId == 5) await txtMobile!.FocusAsync();
            else if (fieldId == 6) await txtEmail!.FocusAsync();
            else if (fieldId == 7) await txtDesignation!.FocusAsync();
        }
    }

    private async Task DownKeyEvent(int fieldId)
    {
        if (fieldId == 1) await txtName!.FocusAsync();
        else if (fieldId == 2) await txtNameArabic!.FocusAsync();
        else if (fieldId == 3) await txtMobile!.FocusAsync();
        else if (fieldId == 4) await txtEmail!.FocusAsync();
        else if (fieldId == 5) await txtDesignation!.FocusAsync();
        else if (fieldId == 6) await txtRemarks!.FocusAsync();
    }
    private MudDataGrid<SalesManMaster>? grid;

    #endregion

    #region Properties
    [Parameter] public bool IsDialog { get; set; } = false;
    #endregion

    #region Events

    protected override async Task OnInitializedAsync()
    {
        if (AppData.privileges is not null)
            privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "SalesManMaster");
    }
    private async Task<GridData<SalesManMaster>> LoadServerData(GridState<SalesManMaster> state)
    {
        var pagination = new PaginationModel(
           page: state.Page + 1,
            pageSize: state.PageSize,
            sortColumn: state.SortDefinitions?.FirstOrDefault()?.SortBy,
            sortDirection: state.SortDefinitions?.FirstOrDefault()?.Descending == true ? "desc" : "asc",
            search: searchText
        );
        var result = await api.GetSalesManPDetails(pagination);
        return new GridData<SalesManMaster>
        {
            Items = result.PSalesMen,
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
            if (salesman.SalesmanId == 0)
            {
                if (privilege is not null && !privilege.SaveData) { Dialog.DialogMethod(Localizer("NoSavePrivilege")); return; }

                var result = await api.CreateSalesMan(salesman);
                if (result.Message == null)
                {
                    if (IsDialog)
                    {
                        AppData.PassingValue = new CommonModel
                        {
                            Id = result.Id,
                            Name = salesman.Name
                        };
                    }
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

                var result = await api.UpdateSalesMan(salesman);
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
            var result = await api.DeleteSalesMan(salesman.SalesmanId);
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
            salesman = new SalesManMaster();
            salesmanImagePreview = null;
            signaturePreview = null;
            sealPreview = null;
            txtCode?.FocusAsync();
            if (grid is not null) await grid.ReloadServerData();
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod(ex.Message);
        }
    }
    async Task RowClicked(DataGridRowClickEventArgs<SalesManMaster> args)
    {

        try
        {
            salesman = args.Item;

            var fullData = await api.GetSalesManById(args.Item.SalesmanId);
            salesmanImagePreview = fullData.SalesmanImage is not null
                ? "data:image/png;base64," + Convert.ToBase64String(fullData.SalesmanImage)
                : null;

            signaturePreview = fullData.SignatureImage is not null
                ? "data:image/png;base64," + Convert.ToBase64String(fullData.SignatureImage)
                : null;

            sealPreview = fullData.SealImage is not null
                ? "data:image/png;base64," + Convert.ToBase64String(fullData.SealImage)
                : null;

            salesman.SalesmanImage = fullData.SalesmanImage;
            salesman.SignatureImage = fullData.SignatureImage;
            salesman.SealImage = fullData.SealImage;
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod(ex.Message);
        }
    }
    private async Task UploadSalesmanImage(IBrowserFile file)
    {
        try
        {
            var bytes = await FileHelper.ValidateAndConvertAsync(file);

            if (bytes == null)
            {
                Dialog.DialogMethod("Maximum allowed file size is 1 MB.");
                return;
            }
            salesman.SalesmanImage = bytes;
            salesmanImagePreview = "data:image/png;base64," + Convert.ToBase64String(bytes);
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod(ex.Message);
        }
    }
    private async Task UploadSignature(IBrowserFile file)
    {
        try
        {
            var bytes = await FileHelper.ValidateAndConvertAsync(file);
            if (bytes == null)
            {
                Dialog.DialogMethod("Maximum allowed file size is 1 MB.");
                return;
            }
            salesman.SignatureImage = bytes;
            signaturePreview = "data:image/png;base64," + Convert.ToBase64String(bytes);
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod(ex.Message);
        }
    }
    private async Task UploadSeal(IBrowserFile file)
    {
        try
        {
            var bytes = await FileHelper.ValidateAndConvertAsync(file);
            if (bytes == null)
            {
                Dialog.DialogMethod("Maximum allowed file size is 1 MB.");
                return;
            }
            salesman.SealImage = bytes;
            sealPreview = "data:image/png;base64," + Convert.ToBase64String(bytes);
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod(ex.Message);
        }
    }
    void RemoveSalesmanImage()
    {
        try
        {
            salesman.SalesmanImage = null;
            salesmanImagePreview = null;
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod(ex.Message);
        }
    }

    void RemoveSignature()
    {
        try
        {
            salesman.SignatureImage = null;
            signaturePreview = null;
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod(ex.Message);
        }
    }
    void RemoveSeal()
    {
        try
        {
            salesman.SealImage = null;
            sealPreview = null;
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod(ex.Message);
        }
    }
    private async Task ShowImagePopup(string img) => await CrudHelper.ShowImagePopup(Dialog, DialogService, img);
    private async Task ToArabic()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(salesman.Name))
            {
                Dialog.DialogMethod(Localizer("EnterNameFirst")); // show warning
                return;
            }
            salesman.NameArabic = await api.GetArabicText(salesman.Name);
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod(ex.Message);
        }
    }
}
#endregion
