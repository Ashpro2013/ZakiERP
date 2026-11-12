using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;

namespace ZakiSAASERP.Shared.Pages.Masters;
public partial class CustomerMasterView : ITabAware
{
    private WebUserPrivilege? privilege;

    #region Injections & Properties
    [Inject] IJSRuntime js { get; set; }
    [Parameter] public bool IsDialog { get; set; } = false;
    [Parameter] public int TabId { get; set; }

    private CustomerMaster customer { get; set; } = new();
    private List<CustomerMaster> customerList = new();
    private List<CustomerGroupMaster> groupList = new();
    private List<CountryMaster> countryList = new();
    private List<SalesManMaster> salesManList = new();
    private string searchText = "";
    MudTextField<string>? txtCode;
    MudTextField<string>? txtName;
    MudTextField<string>? txtNameArabic;
    MudTextField<string>? txtGroup;
    MudSelect<string>? cmbBillType;
    MudTextField<string>? txtSalesMan;
    MudTextField<string>? txtCountry;
    MudSelect<string>? cmbIdentification;
    MudTextField<string>? txtIdNo;
    MudTextField<string>? txtIdNoAr;
    MudTextField<string>? txtVatNo;
    MudTextField<string>? txtVatNoAr;
    MudTextField<string>? txtGroupVat;
    MudTextField<string>? txtBuild;
    MudTextField<string>? txtBuildAr;
    MudTextField<string>? txtStreet;
    MudTextField<string>? txtStreetAr;
    MudTextField<string>? txtDistrict;
    MudTextField<string>? txtDistrictAr;
    MudTextField<string>? txtCity;
    MudTextField<string>? txtCityAr;
    MudTextField<string>? txtPostal;
    MudTextField<string>? txtPostalAr;
    MudTextField<string>? txtAddNo;
    MudTextField<string>? txtAddNoAr;
    MudTextField<string>? txtOther;
    MudTextField<string>? txtOtherAr;
    MudTextField<string>? txtAddress;
    MudTextField<string>? txtAddressAr;
    MudTextField<string>? txtNeigh;
    MudTextField<string>? txtNeighAr;
    MudTextField<string>? txtContact;
    MudTextField<string>? txtContactAr;
    MudTextField<string>? txtTel;
    MudTextField<string>? txtTelAr;
    MudTextField<string>? txtMob;
    MudTextField<string>? txtMobAr;
    MudTextField<string>? txtEmail;
    MudTextField<string>? txtRegion;
    MudTextField<string>? txtRegionAr;
    MudNumericField<decimal?>? numLimit;
    MudNumericField<decimal?>? numDays;
    MudNumericField<decimal?>? numDiscount;
    MudTabs? tabs;
    private bool IsAutoCode => PermanentSettingsData.AutoCustomerCode;
    private bool IsZatcaPhase2 => PermanentSettingsData.ZatcaPhase == 2;

    async Task KeyDown(KeyboardEventArgs e, int fieldId)
    {
        if (e.Key == "Enter" || e.Key == "ArrowDown")
            await DownKeyEvent(fieldId);
        else if (e.Key == "ArrowUp")
        {
            if (fieldId == 2) await txtCode!.FocusAsync();
            else if (fieldId == 3) await txtName!.FocusAsync();
            else if (fieldId == 4) await txtNameArabic!.FocusAsync();
            else if (fieldId == 5) await txtGroup!.FocusAsync();
            else if (fieldId == 6) await cmbBillType!.FocusAsync();
            else if (fieldId == 8) await txtCountry!.FocusAsync();
            else if (fieldId == 9) await cmbIdentification!.FocusAsync();
            else if (fieldId == 10) await txtIdNo!.FocusAsync();
            else if (fieldId == 11) await txtIdNoAr!.FocusAsync();
            // ... Tab 3/4 Back navigation
            else if (fieldId == 35) await txtContact!.FocusAsync();
            else if (fieldId == 34) await txtContactAr!.FocusAsync();
            else if (fieldId == 36) await txtTel!.FocusAsync();
            else if (fieldId == 37) await txtTelAr!.FocusAsync();
            else if (fieldId == 38) await txtMob!.FocusAsync();
            else if (fieldId == 39) await txtMobAr!.FocusAsync();
            else if (fieldId == 40) await txtEmail!.FocusAsync();
            else if (fieldId == 41) await numLimit!.FocusAsync();
            else if (fieldId == 42) await numDays!.FocusAsync();
        }
        else if (e.Key == "Space")
        {
            if (fieldId == 4) await OpenGroupDialog();
            else if (fieldId == 6) await OpenSalesManDialog();
            else if (fieldId == 7) await OpenCountryDialog();
        }
    }

    private async Task DownKeyEvent(int fieldId)
    {
        if (fieldId == 1) await txtName!.FocusAsync();
        else if (fieldId == 2) await txtNameArabic!.FocusAsync();
        else if (fieldId == 3) await txtGroup!.FocusAsync();
        else if (fieldId == 4) await cmbBillType!.FocusAsync();
        else if (fieldId == 5) await txtSalesMan!.FocusAsync();
        else if (fieldId == 6)
        {
            // Switch to Tab 2
            if (tabs is not null) tabs.ActivatePanel(1);
            await Task.Delay(100);
            await txtCountry!.FocusAsync();
        }
        else if (fieldId == 7) await cmbIdentification!.FocusAsync();
        else if (fieldId == 8) await txtIdNo!.FocusAsync();
        else if (fieldId == 9) await txtIdNoAr!.FocusAsync();
        else if (fieldId == 10) await txtVatNo!.FocusAsync();
        else if (fieldId == 11) await txtVatNoAr!.FocusAsync();
        else if (fieldId == 12) await txtGroupVat!.FocusAsync();
        else if (fieldId == 13) await txtBuild!.FocusAsync();
        else if (fieldId == 14) await txtBuildAr!.FocusAsync();
        else if (fieldId == 15) await txtStreet!.FocusAsync();
        else if (fieldId == 16) await txtStreetAr!.FocusAsync();
        else if (fieldId == 17) await txtDistrict!.FocusAsync();
        else if (fieldId == 18) await txtDistrictAr!.FocusAsync();
        else if (fieldId == 19) await txtCity!.FocusAsync();
        else if (fieldId == 20) await txtCityAr!.FocusAsync();
        else if (fieldId == 21) await txtPostal!.FocusAsync();
        else if (fieldId == 22) await txtPostalAr!.FocusAsync();
        else if (fieldId == 23) await txtAddNo!.FocusAsync();
        else if (fieldId == 24) await txtAddNoAr!.FocusAsync();
        else if (fieldId == 25) await txtOther!.FocusAsync();
        else if (fieldId == 26) await txtOtherAr!.FocusAsync();
        else if (fieldId == 27) await txtRegion!.FocusAsync();
        else if (fieldId == 28) await txtRegionAr!.FocusAsync();
        else if (fieldId == 29) await txtAddress!.FocusAsync();
        else if (fieldId == 30) await txtAddressAr!.FocusAsync();
        else if (fieldId == 31)
        {
            // Switch to Tab 3
            if (tabs is not null) tabs.ActivatePanel(2);
            await Task.Delay(100);
            await txtContact!.FocusAsync();
        }
        else if (fieldId == 32) await txtContactAr!.FocusAsync();
        else if (fieldId == 33) await txtTel!.FocusAsync();
        else if (fieldId == 34) await txtTelAr!.FocusAsync();
        else if (fieldId == 35) await txtMob!.FocusAsync();
        else if (fieldId == 36) await txtMobAr!.FocusAsync();
        else if (fieldId == 37) await txtEmail!.FocusAsync();
        else if (fieldId == 38)
        {
            // Switch to Tab 4
            if (tabs is not null) tabs.ActivatePanel(3);
            await Task.Delay(100);
            await numLimit!.FocusAsync();
        }
        else if (fieldId == 39) await numDays!.FocusAsync();
        else if (fieldId == 40) await numDiscount!.FocusAsync();
    }

    // Display names for selected FKs
    private string? strGroupName;
    private string? strCountryName;
    private string? strSalesManName;
    private MudDataGrid<CustomerMaster>? grid;

    private EditContext? editContext;
    #endregion

    #region Initialization
    protected override async Task OnInitializedAsync()
    {
        if (AppData.privileges is not null)
            privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "CustomerMaster");

        try
        {
            customer = new CustomerMaster();
            editContext = new EditContext(customer);

            if (string.IsNullOrEmpty(customer.CusBillType)) customer.CusBillType = "Cash";
            groupList = await api.GetAllCustomerGroup();
            countryList = await api.GetAllCountry();
            salesManList = await api.GetAllSalesMan();

            if (customer.CustomerId == 0)
            {
                SetDefaults();
            }
            await NewClick();
        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod(ex.Message);
        }
    }
    #endregion

    #region Grid & Search
    private async Task<GridData<CustomerMaster>> LoadServerData(GridState<CustomerMaster> state)
    {
        var pagination = new PaginationModel(
           page: state.Page + 1,
            pageSize: state.PageSize,
            sortColumn: state.SortDefinitions?.FirstOrDefault()?.SortBy,
            sortDirection: state.SortDefinitions?.FirstOrDefault()?.Descending == true ? "desc" : "asc",
            search: searchText
        );
        var result = await api.GetCustomerPDetails(pagination);

        return new GridData<CustomerMaster>
        {
            Items = result.PCustomers,
            TotalItems = result.TotalCount
        };
    }
    #endregion

    #region CRUD Operations
    private bool _isSaving = false;

    async Task SaveOrUpdate()
    {
        if (_isSaving) return;
        if (!await ValidateZatcaCustomerFieldsAsync())
        {
            return;
        }
        if (editContext is not null && !editContext.Validate())
        {
            snackbar.Add(Localizer("CheckValidationErrors"), Severity.Error);
            return;
        }

        _isSaving = true;
        StateHasChanged();

        try
        {
            // --- Create ---
            if (customer.CustomerId == 0)
            {
                if (privilege is not null && !privilege.SaveData) { Dialog.DialogMethod(Localizer("NoSavePrivilege")); return; }
                var result = await api.CreateCustomer(customer);

                if (result.Message == null)
                {
                    if (IsDialog)
                    {
                        AppData.PassingValue = new CommonModel
                        {
                            Id = result.Id,
                            Name = customer.Name
                        };
                    }
                    customer.CustomerId = result.Id;
                    Dialog.DialogMethod(Localizer("SaveMessage"));
                    await NewClick();
                }
                else
                    Dialog.DialogMethod(result.Message);
            }
            else
            {
                // --- Update ---
                if (privilege is not null && !privilege.EditData) { Dialog.DialogMethod(Localizer("NoEditPrivilege")); return; }
                var result = await api.UpdateCustomer(customer);

                if (result.Message == null)
                {
                    Dialog.DialogMethod(Localizer("UpdateMessage"));
                    await NewClick();
                }
                else
                    Dialog.DialogMethod(result.Message);
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

            var result = await api.DeleteCustomer(customer.CustomerId);

            if (result.Message == null)
            {
                NewClick();
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
            customer = new CustomerMaster();
            editContext = new EditContext(customer);

            // Clear display strings
            strGroupName = null;
            strCountryName = null;
            strSalesManName = null;

            SetDefaults();
            await LoadAutoCodeIfEnabled();

            if (grid is not null) await grid.ReloadServerData();
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod(ex.Message);
        }
    }

    async Task RowClicked(DataGridRowClickEventArgs<CustomerMaster> args)
    {
        // Handle double click only
        try
        {
            customer = args.Item; // Optimistic first
            editContext = new EditContext(customer);

            var fullData = await api.GetCustomerById(args.Item.CustomerId);
            customer = fullData; // Replace with full data
            editContext = new EditContext(customer);

            // Populate display strings
            strGroupName = groupList.FirstOrDefault(x => x.CustomerGroupId == customer.FK_CustomerGroupId)?.Name;
            strCountryName = countryList.FirstOrDefault(x => x.CountryId == customer.FK_CountryId)?.Name;

            if (customer.FK_SalesmanId is not null && customer.FK_SalesmanId != 0)
            {
                var s = await api.GetSalesManById(customer.FK_SalesmanId.Value);
                strSalesManName = s?.Name;
            }
            else strSalesManName = null;
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod(ex.Message);
        }
    }
    #endregion

    #region Helpers
    /// <summary>
    /// Validates each ZATCA Phase 2 required customer field individually, prompting and focusing the first missing input.
    /// </summary>
    private async Task<bool> ValidateZatcaCustomerFieldsAsync()
    {
        if (!IsZatcaPhase2)
            return true;

        (bool isMissing, string promptKey, string fieldKey, Func<Task> focusField)[] requiredFields =
        [
            (string.IsNullOrWhiteSpace(customer.VatNo), "PleaseEnter", "VatNo", async () => { if (txtVatNo is not null) await txtVatNo.FocusAsync(); }),
            (string.IsNullOrWhiteSpace(customer.IdentificationNo), "PleaseEnter", "IdentificationNo", async () => { if (txtIdNo is not null) await txtIdNo.FocusAsync(); }),
            (string.IsNullOrWhiteSpace(customer.Identification), "PleaseSelect", "Identification", async () => { if (cmbIdentification is not null) await cmbIdentification.FocusAsync(); }),
            (string.IsNullOrWhiteSpace(customer.StreetName), "PleaseEnter", "StreetName", async () => { if (txtStreet is not null) await txtStreet.FocusAsync(); }),
            (string.IsNullOrWhiteSpace(customer.BuildingNo), "PleaseEnter", "BuildingNo", async () => { if (txtBuild is not null) await txtBuild.FocusAsync(); }),
            (string.IsNullOrWhiteSpace(customer.City), "PleaseEnter", "City", async () => { if (txtCity is not null) await txtCity.FocusAsync(); }),
            (string.IsNullOrWhiteSpace(customer.PostalCode), "PleaseEnter", "PostalCode", async () => { if (txtPostal is not null) await txtPostal.FocusAsync(); }),
            (string.IsNullOrWhiteSpace(customer.District), "PleaseEnter", "District", async () => { if (txtDistrict is not null) await txtDistrict.FocusAsync(); }),
            (string.IsNullOrWhiteSpace(customer.CountrySubentity), "PleaseEnter", "Region", async () => { if (txtRegion is not null) await txtRegion.FocusAsync(); }),
            ((!customer.FK_CountryId.HasValue || customer.FK_CountryId.Value <= 0) && customer.Country is null, "PleaseSelect", "Country", async () => { if (txtCountry is not null) await txtCountry.FocusAsync(); })
        ];

        foreach (var (isMissing, promptKey, fieldKey, focusField) in requiredFields)
        {
            if (!isMissing) continue;

            await Dialog.ShowMessage($"{Localizer(promptKey)} {Localizer(fieldKey)}");

            if (tabs is not null && tabs.ActivePanelIndex != 1)
            {
                await tabs.ActivatePanelAsync(1);
                await InvokeAsync(StateHasChanged);
                await Task.Delay(100);
            }
            else
            {
                await Task.Delay(50);
            }

            await focusField();
            return false;
        }

        return true;
    }

    private void SetDefaults()
    {
        // Default Bill Type
        if (string.IsNullOrEmpty(customer.CusBillType)) customer.CusBillType = "Cash";

        // Default Country: SA
        var defaultCountry = countryList.FirstOrDefault(c => c.Code == "SA");
        if (defaultCountry is not null)
        {
            customer.FK_CountryId = defaultCountry.CountryId;
            strCountryName = defaultCountry.Name;
        }

        // Default Group: General
        var defaultGroup = groupList.FirstOrDefault(c => c.Name.Equals("General", StringComparison.OrdinalIgnoreCase));
        if (defaultGroup is not null)
        {
            customer.FK_CustomerGroupId = defaultGroup.CustomerGroupId;
            strGroupName = defaultGroup.Name;
        }

        AddNewLocationRow();
    }

    private void HandleInvalidSubmit(EditContext context)
    {
        snackbar.Add(Localizer("CheckValidationErrors"), Severity.Error);
    }

    private async Task Translate(string fieldType)
    {
        try
        {
            string? sourceText = fieldType switch
            {
                "Name" => customer.Name,
                "Address" => customer.Address,
                "Contact" => customer.Contact,
                "Telephone" => customer.Telephone,
                "Mobile" => customer.Mobile,
                "BuildingNo" => customer.BuildingNo,
                "StreetName" => customer.StreetName,
                "District" => customer.District,
                "City" => customer.City,
                "PostalCode" => customer.PostalCode,
                "AdditionalNo" => customer.AdditionalNo,
                "OtherSellerNo" => customer.OtherSellerNo,
                "Neighbourhood" => customer.Neighbourhood,
                "IdentificationNo" => customer.IdentificationNo,
                "VatNo" => customer.VatNo,
                _ => null
            };

            if (string.IsNullOrWhiteSpace(sourceText))
            {
                // Optionally alert user to enter text first
                return;
            }

            var translated = await api.GetArabicText(sourceText);

            switch (fieldType)
            {
                case "Name": customer.NameArabic = translated; break;
                case "Address": customer.AddressArabic = translated; break;
                case "Contact": customer.ContactArabic = translated; break;
                case "Telephone": customer.PhoneArabic = ArabicLocalization.ConvertToArabicNumbers(translated); break;
                case "Mobile": customer.MobileArabic = ArabicLocalization.ConvertToArabicNumbers(translated); break;
                case "BuildingNo": customer.BuildingNoArabic = ArabicLocalization.ConvertToArabicNumbers(translated); break;
                case "StreetName": customer.StreetNameArabic = translated; break;
                case "District": customer.DistrictArabic = translated; break;
                case "City": customer.CityArabic = translated; break;
                case "PostalCode": customer.PostalCodeArabic = ArabicLocalization.ConvertToArabicNumbers(translated); break;
                case "AdditionalNo": customer.AdditionalNoArabic = ArabicLocalization.ConvertToArabicNumbers(translated); break;
                case "OtherSellerNo": customer.OtherSellerNoArabic = ArabicLocalization.ConvertToArabicNumbers(translated); break;
                case "Neighbourhood": customer.NeighbourhoodArabic = translated; break;
                case "IdentificationNo": customer.IdentificationNoArabic = ArabicLocalization.ConvertToArabicNumbers(translated); break;
                case "VatNo": customer.VatNoArabic = ArabicLocalization.ConvertToArabicNumbers(translated); break;
            }
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod(ex.Message);
        }
    }


    private async Task LoadAutoCodeIfEnabled()
    {
        if (IsAutoCode && customer.CustomerId == 0)
        {
            customer.Code = await api.GetNextCustomerCode();
        }

        if (IsAutoCode)
            await txtName!.FocusAsync();
        else
            await txtCode!.FocusAsync();
    }
    #endregion

    #region Dialogs
    async Task OpenGroupDialog()
    {
        try
        {
            var parameters = new DialogParameters { ["groupList"] = groupList };
            var options = new DialogOptions() { BackdropClick = false, MaxWidth = MaxWidth.Medium };

            var dialog = DialogService.Show<CustomerGroupSearchDialog>(Localizer("CustomerGroupSearch"), parameters, options);
            var result = await dialog.Result;

            if (!result.Canceled && result.Data is CustomerGroupMaster item)
            {
                if (item.CustomerGroupId != 0)
                {
                    customer.FK_CustomerGroupId = item.CustomerGroupId;
                    strGroupName = item.Name;
                }
                else
                {
                    customer.FK_CustomerGroupId = null;
                    strGroupName = null;
                }
            }
        }
        catch (Exception ex) { Dialog.DialogMethod(ex.Message); }
    }

    async Task OpenCountryDialog()
    {
        try
        {
            var parameters = new DialogParameters { ["countryList"] = countryList };
            var options = new DialogOptions() { BackdropClick = false, MaxWidth = MaxWidth.Medium };

            var dialog = DialogService.Show<CountrySearchDialog>(Localizer("CountrySearch"), parameters, options);
            var result = await dialog.Result;

            if (!result.Canceled && result.Data is CountryMaster item)
            {
                if (item.CountryId != 0)
                {
                    customer.FK_CountryId = item.CountryId;
                    strCountryName = item.Name;
                }
                else
                {
                    customer.FK_CountryId = null;
                    strCountryName = null;
                }
            }
        }
        catch (Exception ex) { Dialog.DialogMethod(ex.Message); }
    }

    async Task OpenSalesManDialog()
    {
        try
        {
            var parameters = new DialogParameters { ["salesManList"] = salesManList };
            var options = new DialogOptions() { BackdropClick = false, MaxWidth = MaxWidth.Medium };

            var dialog = DialogService.Show<SalesManSearchDialog>(Localizer("SalesManSearch"), parameters, options);
            var result = await dialog.Result;

            if (!result.Canceled && result.Data is SalesManMaster item)
            {
                if (item.SalesmanId != 0)
                {
                    customer.FK_SalesmanId = item.SalesmanId;
                    strSalesManName = item.Name;
                }
                else
                {
                    customer.FK_SalesmanId = null;
                    strSalesManName = null;
                }
            }
        }
        catch (Exception ex) { Dialog.DialogMethod(ex.Message); }
    }
    #endregion

    #region Location Management
    bool AddNewLocationRow()
    {
        if (customer.LocationDetails == null) customer.LocationDetails = new();

        if (customer.LocationDetails.Count > 0)
        {
            var lastItem = customer.LocationDetails.Last();
            if (string.IsNullOrWhiteSpace(lastItem.LocationName))
            {
                snackbar.Add(Localizer("FillDetailsFirst"), Severity.Warning);
                return false;
            }
        }

        customer.LocationDetails.Add(new CustomerLocationDetails());
        return true;
    }

    void RemoveLocation(CustomerLocationDetails item)
    {
        customer.LocationDetails.Remove(item);
    }

    async Task OpenLocationDialog(CustomerLocationDetails item)
    {
        try
        {
            var options = new DialogOptions() { BackdropClick = false, MaxWidth = MaxWidth.Medium };

            var dialog = DialogService.Show<CustomerLocationSearchDialog>(Localizer("CustomerLocationSearch"), options);
            var result = await dialog.Result;

            if (!result.Canceled && result.Data is CustomerLocationMaster resItem)
            {
                if (resItem.LocationId != 0)
                {
                    if (customer.LocationDetails.Any(x => x != item && x.FK_CustomerLocationId == resItem.LocationId))
                    {
                        Dialog.DialogMethod(Localizer("LocationAlreadyAdded"));
                        return;
                    }
                    item.FK_CustomerLocationId = resItem.LocationId;
                    item.LocationName = resItem.Name;
                }
            }
        }
        catch (Exception ex) { Dialog.DialogMethod(ex.Message); }
    }
    #endregion

    #region Navigation
    // Key down handlers
    // async Task OnGroupKeyDown(KeyboardEventArgs e) { if (e.Code == "Space" || e.Code == "Enter") await OpenGroupDialog(); }
    async Task OnCountryKeyDown(KeyboardEventArgs e) { if (e.Code == "Space" || e.Code == "Enter") await OpenCountryDialog(); }
    // async Task OnSalesManKeyDown(KeyboardEventArgs e) { if (e.Code == "Space" || e.Code == "Enter") await OpenSalesManDialog(); }

    // Grid Navigation
    private async Task HandleFieldEnter(KeyboardEventArgs e, CustomerLocationDetails item, string fieldName)
    {
        if (e.Code == "Enter" || e.Code == "NumpadEnter")
        {
            var index = customer.LocationDetails.IndexOf(item);
            string nextId = "";

            switch (fieldName)
            {
                case "loc":
                    nextId = $"rem-{index}";
                    break;
                case "rem":
                    // checks if it is the last item
                    if (index == customer.LocationDetails.Count - 1)
                    {
                        if (AddNewLocationRow())
                        {
                            // wait for render
                            await Task.Delay(50);
                            nextId = $"loc-{index + 1}";
                        }
                    }
                    else
                    {
                        nextId = $"loc-{index + 1}";
                    }
                    break;
            }

            if (!string.IsNullOrEmpty(nextId))
            {
                await js.InvokeVoidAsync("focusElementById", nextId);
            }
        }
    }
    #endregion
}
