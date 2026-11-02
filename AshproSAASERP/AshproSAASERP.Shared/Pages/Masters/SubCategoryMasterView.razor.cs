using AshproSAASERP.Shared.Helpers;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;

namespace AshproSAASERP.Shared.Pages.Masters;

public partial class SubCategoryMasterView : ITabAware
{
    [Parameter] public int TabId { get; set; }

    #region Injections & Properties
    [Inject] IJSRuntime JSRuntime { get; set; }

    private SubCategoryMaster subcat { get; set; } = new();
    private List<CategoryMaster> categoryList = new();
    private string? imagePreview;
    private string searchText = "";
    MudTextField<string>? txtCode;
    private string? strCategoryName;
    private MudDataGrid<SubCategoryMaster>? grid;
    private EditContext? editContext;
    private bool _dialogJustClosed = false;
    private WebUserPrivilege? privilege;

    /// <summary>
    /// Checks if auto subcategory code generation is enabled from PermanentSettings
    /// </summary>
    private bool IsAutoCode => PermanentSettingsData.AutoSubCategoryCode;
    #endregion

    #region Initialization
    protected override async Task OnInitializedAsync()
    {
        if (AppData.privileges is not null)
            privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "Sub Category Master" || x.Caption == "SubCategoryMaster");
        await NewClick();
    }
    #endregion

    #region Grid & Search
    private async Task<GridData<SubCategoryMaster>> LoadServerData(GridState<SubCategoryMaster> state)
    {
        var pagination = new PaginationModel(
            page: state.Page + 1,
            pageSize: state.PageSize,
            sortColumn: state.SortDefinitions?.FirstOrDefault()?.SortBy,
            sortDirection: state.SortDefinitions?.FirstOrDefault()?.Descending == true ? "desc" : "asc",
            search: searchText
        );
        var result = await api.GetSubCategoryPDetails(pagination);
        return new GridData<SubCategoryMaster>
        {
            Items = result.PSubCategories,
            TotalItems = result.TotalCount
        };
    }

    private async Task OnSearch(string text)
    {
        searchText = text;
        await grid.ReloadServerData();
    }
    #endregion

    #region CRUD Operations
    private bool _isSaving = false;

    async Task SaveOrUpdate()
    {
        if (_isSaving) return;
        _isSaving = true;
        StateHasChanged();

        if (editContext is not null && !editContext.Validate())
        {
            _isSaving = false;
            StateHasChanged();
            return;
        }

        var tempCategory = subcat.Category;
        try
        {
            subcat.Category = null;
            if (subcat.SubCategoryId == 0)
            {
                if (privilege is not null && !privilege.SaveData) { Dialog.DialogMethod(Localizer("NoSavePrivilege")); return; }
                var result = await api.CreateSubCategory(subcat);
                if (result.Message == null)
                {
                    Dialog.DialogMethod(Localizer("SaveMessage"));
                    await NewClick();
                }
                else
                {
                    subcat.Category = tempCategory;
                    Dialog.DialogMethod(result.Message);
                }
            }
            else
            {
                if (privilege is not null && !privilege.EditData) { Dialog.DialogMethod(Localizer("NoEditPrivilege")); return; }
                var result = await api.UpdateSubCategory(subcat);
                if (result.Message == null)
                {
                    Dialog.DialogMethod(Localizer("UpdateMessage"));
                    await NewClick();
                }
                else
                {
                    subcat.Category = tempCategory;
                    Dialog.DialogMethod(result.Message);
                }
            }
        }
        catch (Exception ex)
        {
            subcat.Category = tempCategory;
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
            var result = await api.DeleteSubCategory(subcat.SubCategoryId);
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
            subcat = new SubCategoryMaster();
            subcat.IsFixedAsset = false;
            editContext = new EditContext(subcat);
            imagePreview = null;
            strCategoryName = null;
            if (txtCode is not null) await txtCode.FocusAsync();
            await LoadDefaultCategory();
            if (grid is not null) await grid.ReloadServerData();
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod(ex.Message);
        }
    }

    async Task RowClicked(DataGridRowClickEventArgs<SubCategoryMaster> args)
    {

        try
        {
            subcat = args.Item;
            editContext = new EditContext(subcat);
            var fullData = await api.GetSubCategoryById(args.Item.SubCategoryId);
            strCategoryName = subcat.Category?.Name;
            if (subcat.Category == null && args.Item.Category is not null)
            {
                subcat.Category = args.Item.Category;
            }
            if (fullData.ItemImage is not null)
                imagePreview = "data:image/png;base64," + Convert.ToBase64String(fullData.ItemImage);
            else
                imagePreview = null;
            subcat.ItemImage = fullData.ItemImage;
            subcat.ItemImageName = fullData.ItemImageName;
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod(ex.Message);
        }
    }
    #endregion

    #region Image Handling
    private async Task UploadImage(IBrowserFile file)
    {
        try
        {
            var bytes = await FileHelper.ValidateAndConvertAsync(file);
            if (bytes == null)
            {
                Dialog.DialogMethod(Localizer("MaxFileSize1MB"));
                return;
            }
            var extension = Path.GetExtension(file.Name)?.ToLower();
            if (extension is not ".png" and not ".jpg" and not ".jpeg")
            {
                Dialog.DialogMethod(Localizer("InvalidImageFormat"));
                return;
            }
            subcat.ItemImageName = file.Name;
            subcat.ItemImage = bytes;
            imagePreview = "data:image/png;base64," + Convert.ToBase64String(bytes);
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod(ex.Message);
        }
    }

    void RemoveImage()
    {
        try
        {
            subcat.ItemImage = null;
            subcat.ItemImageName = null;
            imagePreview = null;
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod(ex.Message);
        }
    }

    private void ShowImagePopup(string img)
    {
        try
        {
            var parameters = new DialogParameters
            {
                ["ImageUrl"] = img
            };
            DialogService.Show<ImagePreviewDialog>(Localizer("Preview"), parameters);
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod(ex.Message);
        }
    }
    #endregion

    #region Helpers
    private async Task ToArabic()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(subcat.Name))
            {
                Dialog.DialogMethod(Localizer("EnterNameFirst"));
                return;
            }
            subcat.NameArabic = await api.GetArabicText(subcat.Name);
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod(ex.Message);
        }
    }

    private async Task LoadDefaultCategory()
    {
        try
        {
            var pagination = new PaginationModel(1, 1, "Name", "asc", "General");
            var result = await api.GetCategoryPDetails(pagination);
            if (result.TotalCount > 0 && result.PCategories.Any())
            {
                var cat = result.PCategories.FirstOrDefault(x => x.Name.Equals("General", StringComparison.OrdinalIgnoreCase));
                if (cat is not null)
                {
                    subcat.FK_CategoryId = cat.CategoryId;
                    subcat.Category = cat;
                    strCategoryName = cat.Name;
                    if (cat.IsFixedAsset) subcat.IsFixedAsset = true;

                    // Load auto-code after category is set
                    await LoadAutoCodeIfEnabled();
                }
            }
        }
        catch { }
    }

    /// <summary>
    /// Loads auto-generated subcategory code if enabled and category is selected
    /// </summary>
    private async Task LoadAutoCodeIfEnabled()
    {
        if (IsAutoCode && subcat.SubCategoryId == 0 && subcat.FK_CategoryId.HasValue)
        {
            try
            {
                // Get category code from the Category navigation property
                string? categoryCode = subcat.Category?.Code;
                subcat.Code = await api.GetNextSubCategoryCode(subcat.FK_CategoryId.Value, categoryCode);
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Dialog.DialogMethod($"Error loading auto code: {ex.Message}");
            }
        }
    }

    private async Task FocusById(string id)
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("eval", $"var e = document.getElementById('{id}'); if(e) {{ var inp = e.querySelector('input'); if(inp) {{ inp.focus(); if(inp.select) inp.select(); }} else {{ e.focus(); if(e.select) e.select(); }} }}");
        }
        catch { }
    }
    #endregion

    #region Navigation & Smart Search
    private async Task HandleFormKeyDown(KeyboardEventArgs e, string field)
    {
        if (_dialogJustClosed) return;

        if (e.Key == "ArrowUp")
        {
            switch (field)
            {
                case "name": await FocusById("sc-code"); break;
                case "arabic": await FocusById("sc-name"); break;
                case "category": await FocusById("sc-arabic"); break;
                case "remarks": await FocusById("sc-category"); break;
            }
        }
        else if (e.Key == "ArrowDown" || e.Key == "Enter" || e.Key == "NumpadEnter")
        {
            switch (field)
            {
                case "code": await FocusById("sc-name"); break;
                case "name": await FocusById("sc-arabic"); break;
                case "arabic": await FocusById("sc-category"); break;

                case "category":
                    if (e.Key == "Enter" || e.Key == "NumpadEnter")
                        await HandleCategoryEnter();
                    else
                        await FocusById("sc-remarks");
                    break;

                case "remarks": await FocusById("btn-save"); break;
            }
        }
    }

    private async Task HandleCategoryEnter()
    {
        if (_dialogJustClosed) return;

        try
        {
            if (string.IsNullOrWhiteSpace(strCategoryName))
            {
                await OpenCategoryDialog();
                return;
            }

            // Search to see if there is an exact or single match
            var pagination = new PaginationModel(1, 10, "Name", "asc", strCategoryName);
            var result = await api.GetCategoryPDetails(pagination);

            if (result is not null && result.PCategories is not null)
            {
                // Check for exact match first (Name or Code)
                var match = result.PCategories.FirstOrDefault(c =>
                    c.Name.Equals(strCategoryName, StringComparison.OrdinalIgnoreCase) ||
                    c.Code.Equals(strCategoryName, StringComparison.OrdinalIgnoreCase));

                // If no exact match but exactly one result found
                if (match == null && result.TotalCount == 1 && result.PCategories.Count == 1)
                {
                    match = result.PCategories.First();
                }

                if (match is not null)
                {
                    subcat.FK_CategoryId = match.CategoryId;
                    subcat.Category = match;
                    strCategoryName = match.Name;
                    if (match.IsFixedAsset) subcat.IsFixedAsset = true;

                    // Load auto-code for the matched category
                    await LoadAutoCodeIfEnabled();

                    await FocusById("sc-remarks");
                    return;
                }
            }

            // If we reach here: No match, or multiple matches -> Open Dialog
            await OpenCategoryDialog(strCategoryName);
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod(ex.Message);
        }
    }

    async Task OpenCategoryDialog(string searchText = "")
    {
        if (_dialogJustClosed) return;
        try
        {
            var parameters = new DialogParameters();
            if (!string.IsNullOrEmpty(searchText)) parameters.Add("InitialSearch", searchText);

            var options = new DialogOptions()
            {
                BackdropClick = false,
                MaxWidth = MaxWidth.Medium
            };
            var dialog = DialogService.Show<CategorySearchDialog>(Localizer("CategorySearchTitle"), parameters, options);
            var result = await dialog.Result;
            if (!result!.Canceled)
            {
                _dialogJustClosed = true; // Use simple flag to just block immediate re-entry
                var cat = (CategoryMaster)result.Data!;
                subcat.FK_CategoryId = cat.CategoryId;
                subcat.Category = cat;
                strCategoryName = cat.Name;

                if (cat.IsFixedAsset) subcat.IsFixedAsset = true;

                // Load auto-code for the newly selected category
                await LoadAutoCodeIfEnabled();

                await Task.Delay(100);
                await FocusById("sc-remarks");

                // Keep flag true a bit longer to swallow any pending Enter keys
                await Task.Delay(200);
            }
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod(ex.Message);
        }
        finally
        {
            _dialogJustClosed = false;
        }
    }
    #endregion
}
