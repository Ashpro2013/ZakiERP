using ZakiSAASERP.Shared.Helpers;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;

namespace ZakiSAASERP.Shared.Pages.Masters;

public partial class CategoryMasterView : ITabAware
{
    #region Variables
    [Parameter] public int TabId { get; set; }
    private CategoryMaster category { get; set; } = new();
    private string? itemImagePreview;
    private string searchText = "";
    MudTextField<string>? txtCode;
    MudTextField<string>? txtName;
    MudTextField<string>? txtNameArabic;
    MudTextField<string>? txtRemarks;
    private MudDataGrid<CategoryMaster>? grid;
    WebUserPrivilege? privilege;
    /// <summary>
    /// Whether auto category code is enabled (from PermanentSettingsData - no DB call needed)
    /// </summary>
    private bool IsAutoCode => PermanentSettingsData.AutoCategoryCode;
    #endregion

    #region Lifecycle
    protected override async Task OnInitializedAsync()
    {
        if (AppData.privileges is not null)
            privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "Category Master" || x.Caption == "CategoryMaster");
        await LoadAutoCodeIfEnabled();
    }

    /// <summary>
    /// Loads auto-generated code if AutoCategoryCode setting is enabled
    /// </summary>
    private async Task LoadAutoCodeIfEnabled()
    {
        if (IsAutoCode && category.CategoryId == 0)
        {
            try
            {
                category.Code = await api.GetNextCategoryCode();
            }
            catch (Exception ex)
            {
                await Dialog.DialogMethod($"Error loading auto code: {ex.Message}");
            }
        }
    }
    #endregion

    #region Methods
    private async Task<GridData<CategoryMaster>> LoadServerData(GridState<CategoryMaster> state)
    {
        var pagination = new PaginationModel(
           page: state.Page + 1,
            pageSize: state.PageSize,
            sortColumn: state.SortDefinitions?.FirstOrDefault()?.SortBy,
            sortDirection: state.SortDefinitions?.FirstOrDefault()?.Descending == true ? "desc" : "asc",
            search: searchText
        );
        var result = await api.GetCategoryPDetails(pagination);
        return new GridData<CategoryMaster>
        {
            Items = result.PCategories,
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
            if (category.CategoryId == 0)
            {
                if (privilege is not null && !privilege.SaveData) { await Dialog.DialogMethod(Localizer("NoSavePrivilege")); return; }

                var result = await api.CreateCategory(category);
                if (result.Message == null)
                {
                    await Dialog.DialogMethod(Localizer("SaveMessage"));
                    await NewClick();
                    if (grid is not null) await grid.ReloadServerData();
                }
                else await Dialog.DialogMethod(result.Message);
            }
            else
            {
                if (privilege is not null && !privilege.EditData) { await Dialog.DialogMethod(Localizer("NoEditPrivilege")); return; }

                var result = await api.UpdateCategory(category);

                if (result.Message == null)
                {
                    await Dialog.DialogMethod(Localizer("UpdateMessage"));
                    await NewClick();
                }
                else await Dialog.DialogMethod(result.Message);
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
            if (privilege is not null && !privilege.DeleteData) { await Dialog.DialogMethod(Localizer("NoDeletePrivilege")); return; }
            if (!await Dialog.DeleteDialogMethod())
                return;

            var result = await api.DeleteCategory(category.CategoryId);
            if (result.Message == null)
            {
                await NewClick();
                StateHasChanged();
            }
            else
            {
                if (result.Message.Contains("FK_", StringComparison.OrdinalIgnoreCase) || result.Message.Contains("REFERENCE", StringComparison.OrdinalIgnoreCase) || result.Message.Contains("Constraint", StringComparison.OrdinalIgnoreCase))
                    await Dialog.DialogMethod(ArabicLocalization.GetFriendlyForeignKeyError(result.Message));
                else
                    await Dialog.DialogMethod(result.Message);
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
    async Task NewClick()
    {
        try
        {
            category = new CategoryMaster();
            itemImagePreview = null;
            await LoadAutoCodeIfEnabled();

            // Focus on Name field if auto-code, otherwise Code field
            if (IsAutoCode)
            {
                // Code is auto-filled, focus elsewhere
                StateHasChanged();
            }
            else
            {
                txtCode?.FocusAsync();
            }

            if (grid is not null) await grid.ReloadServerData();
        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod(ex.Message);
        }
    }
    async Task RowClicked(DataGridRowClickEventArgs<CategoryMaster> args)
    {

        try
        {
            category = args.Item;
            if (category.ItemImage == null)
            {
                // In case Code is not loaded in the grid, fetch full details
                var item = await api.GetCategoryById(args.Item.CategoryId);
                if (item is not null) category = item;
            }
            if (category.ItemImage is not null)
            {
                var base64 = Convert.ToBase64String(category.ItemImage);
                itemImagePreview = $"data:image/png;base64,{base64}";
            }
            else
            {
                itemImagePreview = null;
            }
        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod(ex.Message);
        }
    }
    private async Task UploadCategoryImage(IBrowserFile file)
    {
        try
        {
            // Validate file first
            var bytes = await FileHelper.ValidateAndConvertAsync(file);
            if (bytes == null)
            {
                await Dialog.DialogMethod(Localizer("MaxFileSize1MB"));
                return;
            }
            var extension = Path.GetExtension(file.Name)?.ToLower();
            if (extension is not ".png" and not ".jpg" and not ".jpeg")
            {
                await Dialog.DialogMethod(Localizer("InvalidImageFormat"));
                return;
            }
            category.ItemImage = bytes;
            var base64 = Convert.ToBase64String(bytes);
            itemImagePreview = $"data:image/{extension.TrimStart('.')};base64,{base64}";
        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod(ex.Message);
        }
    }
    async Task RemoveCategoryImage()
    {
        try
        {
            itemImagePreview = null;
            category.ItemImage = null;
        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod(ex.Message);
        }
    }
    private async Task ToArabic()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(category.Name))
            {
                await Dialog.DialogMethod(Localizer("EnterNameFirst")); // show warning
                return;
            }

            category.NameArabic = await api.GetArabicText(category.Name);
        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod(ex.Message);
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
    #endregion

}

