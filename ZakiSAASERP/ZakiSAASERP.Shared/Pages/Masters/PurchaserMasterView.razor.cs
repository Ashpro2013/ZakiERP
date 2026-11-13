using Microsoft.AspNetCore.Components.Web;

namespace ZakiSAASERP.Shared.Pages.Masters;

public partial class PurchaserMasterView : ITabAware
{
    [Parameter] public int TabId { get; set; }
    private PurchaserMaster purchaserMaster { get; set; } = new();
    private List<PurchaserMaster> purchaserList = new();
    private MudDataGrid<PurchaserMaster>? grid;
    private MudTextField<string>? txtCode;
    private MudTextField<string>? txtName;
    private MudTextField<string>? txtNameArabic;
    private MudTextField<string>? txtRemarks;
    private MudTextField<string>? txtCreditLimit;
    private MudTextField<string>? txtDescription;

    private string? creditLimitText;
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
            else if (fieldId == 5) await txtRemarks!.FocusAsync();
            else if (fieldId == 6) await txtCreditLimit!.FocusAsync();
        }
    }

    private async Task DownKeyEvent(int fieldId)
    {
        if (fieldId == 1) await txtName!.FocusAsync();
        else if (fieldId == 2) await txtNameArabic!.FocusAsync();
        else if (fieldId == 3) await txtRemarks!.FocusAsync();
        else if (fieldId == 4) await txtCreditLimit!.FocusAsync();
        else if (fieldId == 5) await txtDescription!.FocusAsync();
    }

    private string searchText = "";

    protected override async Task OnInitializedAsync()
    {
        if (AppData.privileges is not null)
            privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "PurchaserMaster");
        try
        {
            // purchaserList is no longer needed for initial load with ServerData
            NewClick();
        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod(ex.Message);
        }
    }

    private async Task<GridData<PurchaserMaster>> LoadServerData(GridState<PurchaserMaster> state)
    {
        var pagination = new PaginationModel(
            page: state.Page + 1,
            pageSize: state.PageSize,
            sortColumn: state.SortDefinitions?.FirstOrDefault()?.SortBy,
            sortDirection: state.SortDefinitions?.FirstOrDefault()?.Descending == true ? "desc" : "asc",
            search: searchText
        );
        var result = await api.GetPurchaserPDetails(pagination);
        return new GridData<PurchaserMaster>
        {
            Items = result.PPurchasers,
            TotalItems = result.TotalCount
        };
    }

    // -------------------------------------------------------------
    // SAVE / UPDATE
    // -------------------------------------------------------------
    private bool _isSaving = false;

    private async Task SaveOrUpdate()
    {
        if (_isSaving) return;
        _isSaving = true;
        StateHasChanged();
        try
        {
            if (purchaserMaster.PurchaserId == 0)
            {
                if (privilege is not null && !privilege.SaveData) { await Dialog.DialogMethod(Localizer("NoSavePrivilege")); return; }
                // Server-side handles duplicate check now
                var result = await api.CreatePurchaser(purchaserMaster);

                if (result.Message == null)
                {
                    await Dialog.DialogMethod("Saved successfully");
                    NewClick();
                    if (grid is not null) await grid.ReloadServerData();
                }
                else
                    await Dialog.DialogMethod(result.Message);
            }
            else
            {
                if (privilege is not null && !privilege.EditData) { await Dialog.DialogMethod(Localizer("NoEditPrivilege")); return; }
                var result = await api.UpdatePurchaser(purchaserMaster);

                if (result.Message == null)
                {
                    await Dialog.DialogMethod("Updated successfully");
                    NewClick();
                    if (grid is not null) await grid.ReloadServerData();
                }
                else
                    await Dialog.DialogMethod(result.Message);
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

    // -------------------------------------------------------------
    // DELETE
    // -------------------------------------------------------------
    private async Task Delete()
    {
        try
        {
            if (privilege is not null && !privilege.DeleteData) { await Dialog.DialogMethod(Localizer("NoDeletePrivilege")); return; }
            if (await Dialog.DeleteDialogMethod())
            {
                var result = await api.DeletePurchaser(purchaserMaster.PurchaserId);
                if (result.Message == null)
                {
                    NewClick();
                    if (grid is not null) await grid.ReloadServerData();
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
                NewClick();
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

    // -------------------------------------------------------------
    // NEW CLICK
    // -------------------------------------------------------------
    private void NewClick()
    {
        purchaserMaster = new PurchaserMaster();
        creditLimitText = string.Empty;

        StateHasChanged();
        txtCode?.FocusAsync();
    }

    // -------------------------------------------------------------
    // PARSE CREDIT LIMIT
    // -------------------------------------------------------------
    private void ParseCreditLimit()
    {
        if (decimal.TryParse(creditLimitText, out var val))
            purchaserMaster.CreditLimit = val;
        else
            creditLimitText = purchaserMaster.CreditLimit.ToString();
    }

    // -------------------------------------------------------------
    // ROW CLICKED
    // -------------------------------------------------------------
    private async Task RowClicked(DataGridRowClickEventArgs<PurchaserMaster> args)
    {

        purchaserMaster = args.Item;
        creditLimitText = purchaserMaster.CreditLimit.ToString();

        await txtCode!.FocusAsync();
        await txtCode!.SelectAsync();
    }

    private async Task ToArabic()
    {
        if (!string.IsNullOrEmpty(purchaserMaster?.Name))
        {
            purchaserMaster.NameArabic = await api.GetArabicText(purchaserMaster.Name);
        }
    }

}
