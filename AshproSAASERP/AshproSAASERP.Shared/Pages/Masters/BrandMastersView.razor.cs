using Microsoft.AspNetCore.Components.Web;

namespace AshproSAASERP.Shared.Pages.Masters
{
    public partial class BrandMastersView : ITabAware
    {
        #region Variables
        private BrandMaster? brandMaster { get; set; } = new();
        MudTextField<string>? txtCode;
        private MudDataGrid<BrandMaster>? grid;
        MudTextField<string>? txtName;
        MudTextField<string>? txtNameArabic;
        MudTextField<string>? txtRemarks;
        #endregion

        #region Properties
        [Parameter] public bool IsDialog { get; set; } = false;
        [Parameter] public int TabId { get; set; }
        #endregion

        #region Events
        WebUserPrivilege? privilege;
        protected override async Task OnInitializedAsync()
        {
            if (AppData.privileges is not null)
                privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "Brand Master" || x.Caption == "BrandMaster");
            await base.OnInitializedAsync();
        }
        private async Task<GridData<BrandMaster>> LoadServerData(GridState<BrandMaster> state)
        {
            var pagination = new PaginationModel(
               page: state.Page + 1,
                pageSize: state.PageSize,
                sortColumn: state.SortDefinitions?.FirstOrDefault()?.SortBy,
                sortDirection: state.SortDefinitions?.FirstOrDefault()?.Descending == true ? "desc" : "asc",
                search: null
            );
            var result = await api.GetBrandPDetails(pagination);

            return new GridData<BrandMaster>
            {
                Items = result.PBrands,
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
                if (brandMaster?.BrandId == 0)
                {
                    if (privilege is not null && !privilege.SaveData) { Dialog.DialogMethod(Localizer("NoSavePrivilege")); return; }
                    var result = await api.CreateBrand(brandMaster);
                    if (result.Message == null)
                    {
                        if (IsDialog)
                        {
                            AppData.PassingValue = new CommonModel
                            {
                                Id = result.Id,
                                Name = brandMaster.Name
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
                    var result = await api.UpdateBrand(brandMaster!);
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
                if (await Dialog.DeleteDialogMethod())
                {
                    var result = await api.DeleteBrand(brandMaster!.BrandId);
                    if (result.Message == null)
                    {
                        await NewClick();
                    }
                    else
                    {
                        if (result.Message.Contains("FK_", StringComparison.OrdinalIgnoreCase) || result.Message.Contains("REFERENCE", StringComparison.OrdinalIgnoreCase) || result.Message.Contains("Constraint", StringComparison.OrdinalIgnoreCase))
                            Dialog.DialogMethod(ArabicLocalization.GetFriendlyForeignKeyError(result.Message));
                        else
                            Dialog.DialogMethod(result.Message);
                    }
                }
                else
                {
                    await NewClick();
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
            brandMaster = new BrandMaster();
            txtCode?.FocusAsync();
            if (grid is not null) await grid.ReloadServerData();
        }

        async Task RowClicked(DataGridRowClickEventArgs<BrandMaster> args)
        {
            brandMaster = args.Item;
            await txtCode!.FocusAsync();
            await txtCode!.SelectAsync();
        }

        private async Task ToArabic()
        {
            brandMaster!.NameArabic = await api.GetArabicText(brandMaster.Name);
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
}
