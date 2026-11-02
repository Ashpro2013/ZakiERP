using AshproStringExtension;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using static ZakiSAASERP.Desk.Services.AppUtility;

namespace ZakiSAASERP.Desk.Services;

public class POSActivation
{
    public static async Task<bool> POSActivationMethod()
    {
        try
        {
            uMacId = ANIHelper.GetMaccID();
            if (!string.IsNullOrEmpty(ActivationKey) || !string.IsNullOrEmpty(Activationlicense))
            {
                string license = Activationlicense;
                string Start = ActivationStartDate;
                string End = ActivationEndDate;
                string MacID = ActivationMacID;
                if (!AppUtility.uMacId.Contains(MacID))
                {
                    return await ActivationDialog("This Software is not registered. Do you want to register now?");
                }
                else
                {
                    System.Globalization.CultureInfo enCul = new System.Globalization.CultureInfo("en-US");
                    string date1 = DateTime.UtcNow.ToString("yyyy-MM-dd", enCul);
                    if (license == "demo")
                    {
                        RegistryHelper.SetData("IsActivated", false.ToString());
                        isActivated = false;
                        DateTime End1 = End.toDateTime();
                        DateTime date = date1.toDateTime();
                        uLicenceNumber = ActivationKey;
                        return false;
                    }
                    else
                    {
                        if (ActivationEndDate == "0")
                        {
                            isActivated = true;
                            string enddate = DateTime.UtcNow.AddDays(90).ToString("yyyy-MM-dd", enCul);
                            RegistryHelper.SetData("ActivationStartDate", date1);
                            RegistryHelper.SetData("ActivationEndDate", enddate);
                            return await FormLoadMethod();
                        }
                        else
                        {
                            DateTime End1 = ActivationEndDate.toDateTime();
                            DateTime date = date1.toDateTime();
                            int iDiff = ANIHelper.GetDateDifferences(End1, date);
                            if (iDiff <= 0)
                            {
                                if (await ActivationCheckingMethod(ActivationKey))
                                {
                                    isActivated = true;
                                    return await FormLoadMethod();
                                }
                                else
                                {
                                    return await ActivationDialog("This is not a valid key. Do you want to register with another key?");
                                }
                            }
                            else
                            {
                                isActivated = true;
                                return await FormLoadMethod()!;
                            }
                        }
                    }
                }
            }
            else
            {
                return await ActivationDialog("This Software is not registered. Do you want to register now?");
            }
        }
        catch (Exception ex) { await Application.Current.MainPage.DisplayAlertAsync("Error", ex.Message + ex.InnerException, "OK"); return false; }
    }
    public static async Task<bool> FormLoadMethod()
    {
        try
        {
            RegistryHelper.SetData("IsActivated", true.ToString());
            AppUtility.uLicenceNumber = ActivationKey;
            return true;
        }
        catch (Exception ex) { await Application.Current.MainPage.DisplayAlertAsync("Error", ex.Message + ex.InnerException, "OK"); return false; }
    }

    public static async Task<bool> ActivationDialog(string Dialog)
    {
        bool result = await Application.Current.MainPage.DisplayAlertAsync("Confirm", Dialog, "Yes", "No");

        if (result)
        {
            string dialogresult = await Application.Current.MainPage.DisplayPromptAsync("Activation Key", "Key:", "OK", "Cancel", placeholder: "Enter your activation key");
            if (!string.IsNullOrEmpty(dialogresult))
            {
                result = await ActivationMethod(dialogresult);
            }
            if (result)
            {
                return await FormLoadMethod();
            }
        }
        return false;
    }
    public static async Task<bool> ActivationCheckingMethod(string ActivationKey)
    {
        if (ANIHelper.CheckInternet())
        {
            if (await ActivationMethod(ActivationKey))
            {
                return true;
            }
            else
            {
                RegistryHelper.SetData("Activationlicense", string.Empty);
                RegistryHelper.SetData("ActivationStartDate", string.Empty);
                RegistryHelper.SetData("ActivationEndDate", string.Empty);
                RegistryHelper.SetData("ActivationMacID", string.Empty);
                RegistryHelper.SetData("ActivationKey", string.Empty);
                return false;
            }
        }
        else
        {
            RegistryHelper.SetData("IsActivated", true.ToString());
            AppUtility.uLicenceNumber = ActivationKey;
            return true;
        }
    }
    public static async Task<bool> ActivationMethod(string licenceKey, string Remarks = null)
    {
        string startdate, enddate, license, result = string.Empty;
        try
        {
            if (!ANIHelper.CheckInternet())
            {
                await Application.Current.MainPage.DisplayAlertAsync("Error", "Active internet connection not found!", "OK");
                return false;
            }
            var url = "http://ashprotechnology.com/api/v2/activation";
            result = GetLicenseKey(licenceKey, url);
            var cdetails = JsonConvert.DeserializeObject<List<Customerdetails>>('[' + result + ']');
            if (cdetails.Count > 0)
            {
                license = cdetails[0].licenseType;
                if (license == null)
                {
                    string id = null;
                    foreach (var item in AppUtility.uMacId)
                    {
                        id = id + " " + item;
                    }
                    await Application.Current.MainPage.DisplayAlertAsync("Error", $"The Registration is filed for the ID {id}. Please contact the dealer.", "OK");
                    return false;
                }
                else
                {
                    if (license == "demo")
                    {
                        startdate = cdetails[0].startDate;
                        enddate = cdetails[0].expiryDate;
                    }
                    else if (license == "Suspended")
                    {
                        string id = null;
                        foreach (var item in AppUtility.uMacId)
                        {
                            id = id + " " + item;
                        }
                        await Application.Current.MainPage.DisplayAlertAsync("Error", $"The Registration is suspended for the ID {id}. Please contact the dealer.", "OK");
                        return false;
                    }
                    else
                    {
                        startdate = "0";
                        enddate = "0";
                    }
                    AppUtility.uLicenceNumber = licenceKey;
                    RegistryHelper.SetData("Activationlicense", license);
                    RegistryHelper.SetData("ActivationStartDate", startdate);
                    RegistryHelper.SetData("ActivationEndDate", enddate);
                    RegistryHelper.SetData("ActivationMacID", AppUtility.uSpecMacId);
                    RegistryHelper.SetData("ActivationKey", licenceKey);
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception)
        {
            await Application.Current.MainPage.DisplayAlertAsync("Error", "Server Error. Please try again!", "OK");
            return false;
        }
    }
    private static string GetLicenseKey(string licenceKey, string url)
    {
        HttpMessageHandler handler = new HttpClientHandler() { };
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(url),
            Timeout = new TimeSpan(0, 2, 0)
        };
        httpClient.DefaultRequestHeaders.Add("ContentType", "application/json");
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes("riolab123456");
        string val = System.Convert.ToBase64String(plainTextBytes);
        httpClient.DefaultRequestHeaders.Add("Authorization", val);
        string result = string.Empty;
        string wResult = string.Empty;
        foreach (var item in AppUtility.uMacId)
        {
            var contentObject = new { Terminal =Environment.MachineName , licenseKey = licenceKey, DeviceId = item, Remarks = ANIHelper.GetIpAddress(), ProductCode = 16 };
            var content = new StringContent(JsonConvert.SerializeObject(contentObject), Encoding.UTF8, "application/json");
            HttpResponseMessage response = httpClient.PostAsync(url, content).Result;
            result = response.Content.ReadAsStringAsync().Result;
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                wResult = wResult + result + "\n";
                continue;
            }
            else
            {
                AppUtility.uSpecMacId = item;
                break;
            }
        }
        if (result.Contains("Message"))
            return wResult;
        else
            return result;
    }

}
