namespace ZakiSAASERP.Shared.Pages.Settings
{
    public partial class ZatcaCSRView : ITabAware
    {
        #region Variables
        [Parameter] public int TabId { get; set; }
        private CSRRequest? csrModel = new();
        private CSRResponse? csrResponse = new();
        private MudTextField<string>? txtOTP;
        #endregion

        #region Lifecycle Methods
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            PopulateDefaultModel();
            // Load any existing configuration if needed
            await LoadExistingConfiguration();
        }
        #endregion

        #region Methods
        private void PopulateDefaultModel()
        {
            var company = AppData.CompanyDetails;
            csrModel = new CSRRequest
            {
                CountryCode = "SA",
                CSRGenerationType = PermanentSettingsData.ZatcaConnection,
                OTP = "123456",
                CommonName = $"{AppData.CompanyDetails?.Name ?? "TST"}-{AppData.CompanyDetails?.IdentificationNo ?? "2050012095"}-{AppData.CompanyDetails?.VatNo ?? "300000134310003"}",
                Organization = AppData.CompanyDetails?.Name ?? "TST",
                UnitName = AppData.CompanyDetails?.IdentificationNo ?? "2050012095",
                SerialNumber = $"1-{AppData.CompanyDetails?.Name ?? "TST"}|2-{AppData.CompanyDetails?.IdentificationNo ?? "2050012095"}|3-f60f4518-72dc-45a1-a053-3899788b42d0",
                OrganizationIdentifier = AppData.CompanyDetails?.VatNo ?? "300000134310003",
                Location = AppData.CompanyDetails?.City ?? "Riyadh",
                BusinessCategory =  "Supply chain",
                InvoiceType = "1100"
            };
        }

        private async Task LoadExistingConfiguration()
        {
            try
            {
                var existing = await api.GetCsr(AppData.BranchId);
                if (existing != null)
                {
                    csrResponse = existing;
                    if (!string.IsNullOrWhiteSpace(existing.OTP) && csrModel != null)
                    {
                        csrModel.OTP = existing.OTP;
                    }
                }
                StateHasChanged();
            }
            catch (Exception) { }
        }

        private async Task GenerateCSR()
        {
            try
            {
                if (csrModel != null)
                {
                    csrModel.company = AppData.CompanyDetails;
                    csrResponse = await api.GenerateCSR(csrModel);
                    Dialog.DialogMethod(Localizer("CSR Generated Successfully"));
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                Dialog.DialogMethod($"Error generating CSR: {ex.Message}");
            }
        }

        private async Task Cancel()
        {
            PopulateDefaultModel();
            if (txtOTP != null)
            {
                await txtOTP.FocusAsync();
            }
            StateHasChanged();
        }

        private async Task SaveDetails()
        {
            try
            {
                if (csrResponse != null)
                {
                    if (csrResponse.BranchId == 0)
                    {
                        csrResponse.BranchId = AppData.BranchId;
                    }
                    if (string.IsNullOrWhiteSpace(csrResponse.OTP) && csrModel != null)
                    {
                        csrResponse.OTP = csrModel.OTP;
                    }
                }

                var response = await api.AddNewCSR(csrResponse);
                if (string.IsNullOrEmpty(response?.Message))
                {
                    Dialog.DialogMethod(Localizer("SaveMessage"));
                }
                else
                    Dialog.DialogMethod(response.Message);
            }
            catch (Exception ex)
            {
                Dialog.DialogMethod($"Error saving details: {ex.Message}");
            }
        }

        private async Task CancelResponse()
        {
            await Cancel();
        }
        #endregion
    }
}
