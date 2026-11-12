using ZakiSAASERP.Shared.Helpers;
using Microsoft.AspNetCore.Components.Forms;

namespace ZakiSAASERP.Shared.Pages.Settings;

public partial class CompanyDetailsView : ITabAware
{
    #region Variables
    [Inject] private IViewService viewService { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private AppData AppData { get; set; } = default!;

    [Parameter] public int TabId { get; set; }

    private CompanyDetails companyDetail = new() { Name = "" };
    private CompanyDetails originalDetail = new() { Name = "" };
    private List<BranchMaster> branches = new();
    private List<CountryMaster> countries = new();
    private int selectedBranchId;
    private bool isEditMode = false;
    private bool dataExists = false;
    private bool canChangeBranch = false; // Set based on user permissions
    private bool isLoading = false;
    private WebUserPrivilege? _privilege = null;
    private string? headerImagePreview;
    private string? footerImagePreview;
    #endregion

    #region Initialization
    protected override async Task OnInitializedAsync()
    {
        isLoading = true;
        StateHasChanged();

        try
        {
            if (AppData.privileges is not null && AppData.privileges.Any())
            {
                _privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "Company Details") ?? new();
            }
            // Load branches
            var branchList = await viewService.GetAllBranches();
            branches = branchList ?? new List<BranchMaster>();

            // Load countries
            var countryList = await viewService.GetAllCountry();
            countries = countryList ?? new List<CountryMaster>();

            // Set initial branch to logged-in user's branch
            selectedBranchId = AppData.BranchId;

            // TODO: Check user permissions to determine if they can change branches
            // For now, allowing branch change - adjust based on your permission logic
            canChangeBranch = true;

            // Trigger state change to ensure collections are ready before loading data
            StateHasChanged();

            // Load data for the logged-in branch
            await LoadDataForBranch(selectedBranchId);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error loading data: {ex.Message}", Severity.Error);
            // Ensure collections are initialized even on error
            branches ??= new List<BranchMaster>();
            countries ??= new List<CountryMaster>();
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }
    #endregion

    #region Methods
    private async Task OnBranchChanged()
    {
        isLoading = true;
        StateHasChanged();

        try
        {
            await LoadDataForBranch(selectedBranchId);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error loading branch data: {ex.Message}", Severity.Error);
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }
    private async Task OnBranchChangedHandler(int newBranchId)
    {
        if (selectedBranchId == newBranchId)
            return; // Avoid unnecessary reloads

        selectedBranchId = newBranchId;
        await OnBranchChanged();
    }
    private async Task LoadDataForBranch(int branchId)
    {
        // Check if data exists for this branch
        dataExists = await viewService.CheckCompanyDetailsExists(branchId);

        if (dataExists)
        {
            // Load existing data
            var existing = await viewService.GetCompanyDetailsByBranch(branchId);
            if (existing is not null)
            {
                companyDetail = existing;
                originalDetail = CloneCompanyDetails(existing);
                if (companyDetail.HeaderImage is not null)
                {
                    headerImagePreview = "data:image/png;base64," + Convert.ToBase64String(companyDetail.HeaderImage);
                }
                else
                {
                    headerImagePreview = null;
                }
                if (companyDetail.FooterImage is not null)
                {
                    footerImagePreview = "data:image/png;base64," + Convert.ToBase64String(companyDetail.FooterImage);
                }
                else
                {
                    footerImagePreview = null;
                }
            }
            else
            {
                // Fallback if null returned
                companyDetail = new CompanyDetails
                {
                    Name = "",
                    FK_BranchId = branchId
                };
                originalDetail = CloneCompanyDetails(companyDetail);
            }

            // Disable edit mode if data exists
            isEditMode = false;
        }
        else
        {
            // No data for this branch - create new instance and enable edit mode
            companyDetail = new CompanyDetails
            {
                Name = "",
                FK_BranchId = branchId
            };

            // Set default country - prioritize 'SA' (Saudi Arabia), otherwise first country
            if (countries is not null && countries.Any())
            {
                var saCountry = countries.FirstOrDefault(c => c.Code?.Equals("SA", StringComparison.OrdinalIgnoreCase) == true);
                companyDetail.FK_CountryId = saCountry?.CountryId ?? countries.First().CountryId;
            }

            originalDetail = CloneCompanyDetails(companyDetail);
            isEditMode = true;
        }

        StateHasChanged();
    }
    private void ToggleEditMode()
    {
        if (_privilege is not null && !_privilege.EditData)
        {
            Snackbar.Add("You have no privilege to Edit Company Details!", Severity.Error);
            return;
        }
        isEditMode = true;
    }
    private void CancelEdit()
    {
        // Revert changes
        companyDetail = CloneCompanyDetails(originalDetail);
        isEditMode = false;
        StateHasChanged();
    }
    private async Task SaveAsync()
    {
        if (dataExists && _privilege is not null && !_privilege.EditData)
        {
            Snackbar.Add("You have no privilege to Update Company Details!", Severity.Error);
            return;
        }
        if (!dataExists && _privilege is not null && !_privilege.SaveData)
        {
            Snackbar.Add("You have no privilege to Save Company Details!", Severity.Error);
            return;
        }
        try
        {
            // Set the branch ID
            companyDetail.FK_BranchId = selectedBranchId;

            // Save or update based on whether data exists
            var response = dataExists
                ? await viewService.UpdateCompanyDetails(companyDetail)
                : await viewService.CreateCompanyDetails(companyDetail);

            if (response.Id == 1)
            {
                Snackbar.Add(response.Message ?? "Operation completed successfully", Severity.Success);
                // Update state
                dataExists = true;
                isEditMode = false;
                originalDetail = CloneCompanyDetails(companyDetail);
                AppData.CompanyDetails = originalDetail;
            }
            else
            {
                Snackbar.Add(response.Message ?? "Operation failed", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error saving company details: {ex.Message}", Severity.Error);
        }
    }
    private CompanyDetails CloneCompanyDetails(CompanyDetails source)
    {
        return new CompanyDetails
        {
            CompanyDetailId = source.CompanyDetailId,
            FK_BranchId = source.FK_BranchId,
            Name = source.Name,
            NameArabic = source.NameArabic,
            Contact = source.Contact,
            ContactArabic = source.ContactArabic,
            Telephone = source.Telephone,
            TelephoneArabic = source.TelephoneArabic,
            Fax = source.Fax,
            FaxArabic = source.FaxArabic,
            Mobile = source.Mobile,
            MobileArabic = source.MobileArabic,
            Email = source.Email,
            VatNo = source.VatNo,
            VatNoArabic = source.VatNoArabic,
            BuildingNo = source.BuildingNo,
            BuildingNoArabic = source.BuildingNoArabic,
            StreetName = source.StreetName,
            StreetNameArabic = source.StreetNameArabic,
            District = source.District,
            DistrictArabic = source.DistrictArabic,
            City = source.City,
            CityArabic = source.CityArabic,
            PostalCode = source.PostalCode,
            PostalCodeArabic = source.PostalCodeArabic,
            AdditionalNo = source.AdditionalNo,
            AdditionalNoArabic = source.AdditionalNoArabic,
            FK_CountryId = source.FK_CountryId,
            OtherSellerId = source.OtherSellerId,
            OtherSellerIdArabic = source.OtherSellerIdArabic,
            IdentificationType = source.IdentificationType,
            IdentificationNo = source.IdentificationNo,
            IdentificationNoArabic = source.IdentificationNoArabic,
            GroupVat = source.GroupVat
        };
    }
    private async Task TranslateToArabic(string fieldName)
    {
        try
        {
            string? sourceText = fieldName switch
            {
                nameof(companyDetail.Name) => companyDetail.Name,
                nameof(companyDetail.Contact) => companyDetail.Contact,
                nameof(companyDetail.Telephone) => companyDetail.Telephone,
                nameof(companyDetail.Fax) => companyDetail.Fax,
                nameof(companyDetail.Mobile) => companyDetail.Mobile,
                nameof(companyDetail.VatNo) => companyDetail.VatNo,
                nameof(companyDetail.BuildingNo) => companyDetail.BuildingNo,
                nameof(companyDetail.StreetName) => companyDetail.StreetName,
                nameof(companyDetail.District) => companyDetail.District,
                nameof(companyDetail.City) => companyDetail.City,
                nameof(companyDetail.PostalCode) => companyDetail.PostalCode,
                nameof(companyDetail.AdditionalNo) => companyDetail.AdditionalNo,
                nameof(companyDetail.OtherSellerId) => companyDetail.OtherSellerId,
                nameof(companyDetail.IdentificationNo) => companyDetail.IdentificationNo,
                _ => null
            };

            if (string.IsNullOrWhiteSpace(sourceText))
                return;

            // Call the API to translate
            var arabicText = await viewService.GetArabicText(sourceText);

            if (!string.IsNullOrWhiteSpace(arabicText))
            {
                // Convert numbers to Arabic numerals
                arabicText = ArabicLocalization.ConvertToArabicNumbers(arabicText);

                // Update the corresponding Arabic field
                switch (fieldName)
                {
                    case nameof(companyDetail.Name):
                        companyDetail.NameArabic = arabicText;
                        break;
                    case nameof(companyDetail.Contact):
                        companyDetail.ContactArabic = arabicText;
                        break;
                    case nameof(companyDetail.Telephone):
                        companyDetail.TelephoneArabic = arabicText;
                        break;
                    case nameof(companyDetail.Fax):
                        companyDetail.FaxArabic = arabicText;
                        break;
                    case nameof(companyDetail.Mobile):
                        companyDetail.MobileArabic = arabicText;
                        break;
                    case nameof(companyDetail.VatNo):
                        companyDetail.VatNoArabic = arabicText;
                        break;
                    case nameof(companyDetail.BuildingNo):
                        companyDetail.BuildingNoArabic = arabicText;
                        break;
                    case nameof(companyDetail.StreetName):
                        companyDetail.StreetNameArabic = arabicText;
                        break;
                    case nameof(companyDetail.District):
                        companyDetail.DistrictArabic = arabicText;
                        break;
                    case nameof(companyDetail.City):
                        companyDetail.CityArabic = arabicText;
                        break;
                    case nameof(companyDetail.PostalCode):
                        companyDetail.PostalCodeArabic = arabicText;
                        break;
                    case nameof(companyDetail.AdditionalNo):
                        companyDetail.AdditionalNoArabic = arabicText;
                        break;
                    case nameof(companyDetail.OtherSellerId):
                        companyDetail.OtherSellerIdArabic = arabicText;
                        break;
                    case nameof(companyDetail.IdentificationNo):
                        companyDetail.IdentificationNoArabic = arabicText;
                        break;
                }

                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Translation failed: {ex.Message}", Severity.Error);
        }
    }
    #endregion

    #region ImageHandling
    private async Task Uploadheader(IBrowserFile file)
    {
        try
        {
            if (file == null) return;
            var bytes = await FileHelper.ValidateAndConvertAsync(file);

            if (bytes == null)
            {
                await Dialog.DialogMethod("Maximum allowed file size is 1 MB.");
                return;
            }
            companyDetail.HeaderImage = bytes;
            headerImagePreview = "data:image/png;base64," + Convert.ToBase64String(bytes);
        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod(ex.Message);
        }
    }
    async Task RemoveHeaderImage()
    {
        try
        {
            headerImagePreview = null;
        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod(ex.Message);
        }
    }
    private async Task Uploadfooter(IBrowserFile file)
    {
        try
        {
            if (file == null) return;
            var bytes = await FileHelper.ValidateAndConvertAsync(file);

            if (bytes == null)
            {
                await Dialog.DialogMethod("Maximum allowed file size is 1 MB.");
                return;
            }
            companyDetail.FooterImage = bytes;
            footerImagePreview = "data:image/png;base64," + Convert.ToBase64String(bytes);
        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod(ex.Message);
        }
    }
    async Task RemoveFooterImage()
    {
        try
        {
            footerImagePreview = null;
        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod(ex.Message);
        }
    }

    #endregion
}
