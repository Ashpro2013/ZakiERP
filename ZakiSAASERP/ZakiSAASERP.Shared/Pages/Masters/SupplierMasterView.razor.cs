using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;

namespace ZakiSAASERP.Shared.Pages.Masters
{
    public partial class SupplierMasterView : ITabAware
    {
        [Parameter] public int TabId { get; set; }
        private WebUserPrivilege? privilege;
        #region Injections & Properties
        [Parameter] public bool IsDialog { get; set; } = false;
        private SupplierMaster supplier = new SupplierMaster();
        private MudDataGrid<SupplierMaster> grid;
        private string searchText = "";

        // UI Helpers
        private string strCountryName = "";
        private string strPurchaserName = "";

        private MudTextField<string> txtCode;
        private MudTextField<string> txtName;
        private bool IsAutoCode => PermanentSettingsData.AutoSupplierCode;
        private List<CountryMaster> countryList = new();

        private EditContext? editContext;
        #endregion

        #region Initialization
        protected override async Task OnInitializedAsync()
        {
            try
            {
                if (AppData.privileges is not null)
                    privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "SupplierMaster");
                supplier = new SupplierMaster();
                editContext = new EditContext(supplier);

                if (string.IsNullOrEmpty(supplier.SuppBillType)) supplier.SuppBillType = "Cash";
                countryList = await api.GetAllCountry();

                if (supplier.SupplierId == 0)
                {
                    var defaultCountry = countryList.FirstOrDefault(c => c.Code == "SA");
                    if (defaultCountry is not null)
                    {
                        supplier.FK_CountryId = defaultCountry.CountryId;
                        strCountryName = defaultCountry.Name;
                    }
                }

                await LoadServerData(new GridState<SupplierMaster>());
                await NewClick();
            }
            catch (Exception ex)
            {
                Dialog.DialogMethod(ex.Message);
            }
        }
        #endregion

        #region Grid & Search
        private async Task<GridData<SupplierMaster>> LoadServerData(GridState<SupplierMaster> state)
        {
            var pagination = new PaginationModel(
                page: state.Page + 1,
                pageSize: state.PageSize,
                sortColumn: state.SortDefinitions?.FirstOrDefault()?.SortBy ?? "SupplierId",
                sortDirection: state.SortDefinitions?.FirstOrDefault()?.Descending == true ? "desc" : "asc",
                search: searchText
            );

            var result = await api.GetSupplierPDetails(pagination);

            return new GridData<SupplierMaster>
            {
                Items = result.PSuppliers,
                TotalItems = result.TotalCount
            };
        }
        #endregion

        #region CRUD Operations
        private async Task RowClicked(DataGridRowClickEventArgs<SupplierMaster> args)
        {
            // Handle double click only
            try
            {
                var id = args.Item.SupplierId;
                supplier = await api.GetSupplierById(id);
                editContext = new EditContext(supplier);

                // Load related names for UI
                if (supplier.FK_CountryId > 0)
                {
                    var country = await api.GetCountryById(supplier.FK_CountryId.Value);
                    strCountryName = country?.Name ?? "";
                }
                else strCountryName = "";

                if (supplier.FK_PurchaserId > 0)
                {
                    var purchaser = await api.GetPurchaserById(supplier.FK_PurchaserId.Value);
                    strPurchaserName = purchaser?.Name ?? "";
                }
                else strPurchaserName = "";
            }
            catch (Exception ex)
            {
                Dialog.DialogMethod(ex.Message);
            }
        }

        async Task NewClick()
        {
            supplier = new SupplierMaster();
            editContext = new EditContext(supplier);
            supplier.IsActive = true;
            supplier.Code = "";
            supplier.Name = "";

            strCountryName = "";
            strPurchaserName = "";

            var defaultCountry = countryList.FirstOrDefault(c => c.Code == "SA");
            if (string.IsNullOrEmpty(supplier.SuppBillType)) supplier.SuppBillType = "Cash";
            if (defaultCountry is not null)
            {
                supplier.FK_CountryId = defaultCountry.CountryId;
                strCountryName = defaultCountry.Name;
            }

            await LoadAutoCodeIfEnabled();

        }

        private bool _isSaving = false;

        async Task SaveOrUpdate()
        {
            if (_isSaving) return;
            _isSaving = true;
            StateHasChanged();
            try
            {
                if (editContext is not null && !editContext.Validate())
                {
                    Dialog.DialogMethod(Localizer("FillRequired"));
                    _isSaving = false;
                    StateHasChanged();
                    return;
                }

                // Create
                if (supplier.SupplierId == 0)
                {
                    if (privilege is not null && !privilege.SaveData) { Dialog.DialogMethod(Localizer("NoSavePrivilege")); return; }
                    var result = await api.CreateSupplier(supplier);

                    if (result.Message == null)
                    {
                        if (IsDialog)
                        {
                            AppData.PassingValue = new CommonModel
                            {
                                Id = result.Id,
                                Name = supplier.Name,
                                Code = supplier.Code
                            };
                        }
                        supplier.SupplierId = result.Id;
                        Dialog.DialogMethod(Localizer("SaveMessage"));
                        await NewClick();
                        await grid.ReloadServerData();
                    }
                    else
                    {
                        Dialog.DialogMethod(result.Message);
                    }
                }
                // Update
                else
                {
                    if (privilege is not null && !privilege.EditData) { Dialog.DialogMethod(Localizer("NoEditPrivilege")); return; }
                    var result = await api.UpdateSupplier(supplier);

                    if (result.Message == null)
                    {
                        Dialog.DialogMethod(Localizer("UpdateMessage"));
                        await NewClick();
                        await grid.ReloadServerData();
                    }
                    else
                    {
                        Dialog.DialogMethod(result.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Dialog.DialogMethod(ex.Message);
            }
            finally
            {
                _isSaving = false;
                StateHasChanged();
            }
        }

        async Task Delete()
        {
            if (supplier.SupplierId == 0)
            {
                Dialog.DialogMethod(Localizer("SelectRecordDelete"));
                return;
            }

            try
            {
                if (!await Dialog.DeleteDialogMethod())
                    return;

                if (privilege is not null && !privilege.DeleteData) { Dialog.DialogMethod(Localizer("NoDeletePrivilege")); return; }

                var result = await api.DeleteSupplier(supplier.SupplierId);

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
        #endregion

        #region Helpers
        private void HandleInvalidSubmit()
        {
            Dialog.DialogMethod(Localizer("FillRequired"));
            // Optionally focus the first invalid field or just let Blazor handle it
        }

        private async Task Translate(string fieldName)
        {
            string source = "";
            switch (fieldName)
            {
                case "Name": source = supplier.Name; break;
                case "Address": source = supplier.Address; break;
                case "Contact": source = supplier.Contact; break;
                case "Telephone": source = supplier.Telephone; break;
                case "Mobile": source = supplier.Mobile; break;
                case "VatNo": source = supplier.VatNo; break;
                case "IdentificationNo": source = supplier.IdentificationNo; break;

                // ZATCA Address
                case "BuildingNo": source = supplier.BuildingNo; break;
                case "StreetName": source = supplier.StreetName; break;
                case "District": source = supplier.District; break;
                case "City": source = supplier.City; break;
                case "PostalCode": source = supplier.PostalCode; break;
                case "AdditionalNo": source = supplier.AdditionalNo; break;
                case "OtherSellerNo": source = supplier.OtherSellerNo; break;
                case "Neighbourhood": source = supplier.Neighbourhood; break;
            }

            if (string.IsNullOrWhiteSpace(source)) return;

            var arabic = await api.GetArabicText(source);

            switch (fieldName)
            {
                case "Name": supplier.NameArabic = arabic; break;
                case "Address": supplier.AddressArabic = arabic; break;
                case "Contact": supplier.ContactArabic = arabic; break;
                case "Telephone": supplier.PhoneArabic = ArabicLocalization.ConvertToArabicNumbers(arabic); break;
                case "Mobile": supplier.MobileArabic = ArabicLocalization.ConvertToArabicNumbers(arabic); break;
                case "VatNo": supplier.VatNoArabic = ArabicLocalization.ConvertToArabicNumbers(arabic); break;
                case "IdentificationNo": supplier.IdentificationNoArabic = ArabicLocalization.ConvertToArabicNumbers(arabic); break;

                // ZATCA Address
                case "BuildingNo": supplier.BuildingNoArabic = ArabicLocalization.ConvertToArabicNumbers(arabic); break;
                case "StreetName": supplier.StreetNameArabic = arabic; break;
                case "District": supplier.DistrictArabic = arabic; break;
                case "City": supplier.CityArabic = arabic; break;
                case "PostalCode": supplier.PostalCodeArabic = ArabicLocalization.ConvertToArabicNumbers(arabic); break;
                case "AdditionalNo": supplier.AdditionalNoArabic = ArabicLocalization.ConvertToArabicNumbers(arabic); break;
                case "OtherSellerNo": supplier.OtherSellerNoArabic = ArabicLocalization.ConvertToArabicNumbers(arabic); break;
                case "Neighbourhood": supplier.NeighbourhoodArabic = arabic; break;
            }
        }


        private async Task LoadAutoCodeIfEnabled()
        {
            if (IsAutoCode && supplier.SupplierId == 0)
            {
                supplier.Code = await api.GetNextSupplierCode();
            }
            if (IsAutoCode)
                await txtName!.FocusAsync();
            else
                await txtCode!.FocusAsync();
        }
        #endregion

        #region Dialogs
        async Task OpenCountryDialog()
        {
            try
            {
                var parameters = new DialogParameters { ["countryList"] = countryList };
                var options = new DialogOptions() { BackdropClick = false, MaxWidth = MaxWidth.Small };

                var dialog = DialogService.Show<CountrySearchDialog>(Localizer("CountrySearch"), parameters, options);
                var result = await dialog.Result;

                if (!result.Canceled && result.Data is CountryMaster item)
                {
                    if (item.CountryId != 0)
                    {
                        supplier.FK_CountryId = item.CountryId;
                        strCountryName = item.Name;
                    }
                    else
                    {
                        supplier.FK_CountryId = null;
                        strCountryName = null;
                    }
                }
            }
            catch (Exception ex) { Dialog.DialogMethod(ex.Message); }
        }


        private async Task OpenPurchaserDialog()
        {
            var options = new MudBlazor.DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
            var dialog = await DialogService.ShowAsync<PurchaserSearchDialog>(Localizer("SelectPurchaser"), options);
            var result = await dialog.Result;

            if (!result.Canceled && result.Data is PurchaserMaster item)
            {
                supplier.FK_PurchaserId = item.PurchaserId;
                strPurchaserName = item.Name;
            }
        }
        #endregion

        #region Navigation
        // --- Keyboard Events for Search fields ---

        private async Task OnCountryKeyDown(KeyboardEventArgs e)
        {
            if (e.Code == "Space")
            {
                await OpenCountryDialog();
            }
            else if (e.Key == "Delete" || e.Key == "Backspace")
            {
                supplier.FK_CountryId = null;
                strCountryName = "";
            }
        }

        private async Task OnPurchaserKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter" || e.Key == "NumpadEnter" || e.Key == "F2")
            {
                await OpenPurchaserDialog();
            }
            else if (e.Key == "Delete" || e.Key == "Backspace")
            {
                supplier.FK_PurchaserId = null;
                strPurchaserName = "";
            }
        }
        #endregion
    }
}
