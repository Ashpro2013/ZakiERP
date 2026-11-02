using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Xml;
using javax.xml.transform;
using Newtonsoft.Json;
using ZatcaIntegrationSDK.APIHelper;
using ZatcaIntegrationSDK.HelperContracts;

namespace ZatcaIntegrationSDK.BLL
{
    public class ApiRequestLogic
    {
        private Mode mode { get; set; }
        private bool SaveXML = false;
        private string Directorypath = "";
        //public ApiRequestLogic(Mode _mode = Mode.developer)
        //{
        //    this.mode = _mode;
        //}
        public ApiRequestLogic(Mode _mode = Mode.developer, string _Directorypath = "", bool savexml = false)
        {
            this.mode = _mode;
            this.SaveXML = savexml;
            this.Directorypath = _Directorypath;

        }
        public ComplianceCsrResponse GetComplianceCSIDAPI(string OTP, string CSR, string Directorypath)
        {
            ComplianceCsrResponse response = new ComplianceCsrResponse();
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(GlobalVariables.BaseUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("OTP", OTP);
                    client.DefaultRequestHeaders.Add("Accept-Version", "V2");
                    //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072 | SecurityProtocolType.Tls;
                    //POST Method
                    if (string.IsNullOrEmpty(CSR))
                        CSR = File.ReadAllText(Directorypath + "\\cert\\csr.csr");
                    var data = new StringContent(JsonConvert.SerializeObject(new { csr = Utility.ToBase64Encode(CSR) }), Encoding.UTF8, "application/json");
                    client.Timeout = TimeSpan.FromSeconds(90);
                    HttpResponseMessage responsePost = client.PostAsync(GlobalVariables.ComplianceCsidEndpoint(mode), data).Result;
                    var reponsestr = responsePost.Content.ReadAsStringAsync().Result;
                    response.StatusCode = (int)responsePost.StatusCode;
                    if (responsePost.IsSuccessStatusCode)
                    {
                        
                        response = JsonConvert.DeserializeObject<ComplianceCsrResponse>(reponsestr);
                        return response;
                    }
                    response.ErrorMessage = "Error StatusCode : " + responsePost.StatusCode + "  \n\r";
                    if (responsePost.StatusCode == HttpStatusCode.BadRequest)
                    {
                        ComplianceCsrResponse error = new ComplianceCsrResponse();
                        try
                        {
                            error = JsonConvert.DeserializeObject<ComplianceCsrResponse>(reponsestr);
                            response.ErrorMessage += error.DispositionMessage + " : " + string.Join("\n", error.Errors);
                        }
                        catch
                        {
                            response.ErrorMessage += reponsestr;
                        }
                    }
                    else if (responsePost.StatusCode == HttpStatusCode.NotAcceptable)
                    {

                        response.ErrorMessage += "This Version is not supported or not provided in the header.";

                    }
                    else if (responsePost.StatusCode == HttpStatusCode.InternalServerError)
                    {
                        ErrorModel error = new ErrorModel();
                        try
                        {
                            error = JsonConvert.DeserializeObject<ErrorModel>(reponsestr);
                            response.ErrorMessage += error.Code + " : " + error.Message;
                        }
                        catch
                        {
                            response.ErrorMessage += reponsestr;
                        }
                    }
                    else
                    {
                        response.ErrorMessage += "Error in ComplianceCsr API";
                    }

                }
            }
            catch (Exception ex)
            {
                response.ErrorMessage += ex.Message;

            }
            return response;
        }
        public ComplianceCsrResponse GetComplianceCSIDAPI(string OTP, string CSR)
        {
            ComplianceCsrResponse response = new ComplianceCsrResponse();
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(GlobalVariables.BaseUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("OTP", OTP);
                    client.DefaultRequestHeaders.Add("Accept-Version", "V2");
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072 | SecurityProtocolType.Tls;
                    //POST Method
                    if (string.IsNullOrEmpty(CSR))
                    {
                        response.ErrorMessage = "Please Enter csr";
                        return response;
                    }
                    string sending_exception = "";
                    int LoopCount = 3;
                    HttpResponseMessage responsePost = null;

                    for (int i = 0; i <= LoopCount; i++)
                    {

                        try
                        {
                            var data = new StringContent(JsonConvert.SerializeObject(new { csr = Utility.ToBase64Encode(CSR) }), Encoding.UTF8, "application/json");
                            responsePost = client.PostAsync(GlobalVariables.ComplianceCsidEndpoint(mode), data).Result;
                            break;
                        }
                        catch (Exception ex)
                        {
                            if (i == LoopCount)
                            {
                                if (ex.InnerException != null)
                                {
                                    sending_exception = ex.InnerException.Message;
                                }
                               
                            }
                        }
                    }
                    if (responsePost == null)
                    {
                        response.ErrorMessage = sending_exception;
                        return response;
                    }
                    var reponsestr = responsePost.Content.ReadAsStringAsync().Result;
                    response.StatusCode = (int)responsePost.StatusCode;
                    if (responsePost.IsSuccessStatusCode)
                    {
                        response = JsonConvert.DeserializeObject<ComplianceCsrResponse>(reponsestr);
                        return response;
                    }
                    response.ErrorMessage = "Error StatusCode : " + responsePost.StatusCode + "  \n\r";

                    if (responsePost.StatusCode == HttpStatusCode.BadRequest)
                    {

                        ComplianceCsrResponse error = new ComplianceCsrResponse();
                        try
                        {
                            error = JsonConvert.DeserializeObject<ComplianceCsrResponse>(reponsestr);
                            response.ErrorMessage += error.DispositionMessage + " : " + string.Join("\n",error.Errors);
                        }
                        catch
                        {
                            response.ErrorMessage += reponsestr;
                        }
                    }
                    else if (responsePost.StatusCode == HttpStatusCode.NotAcceptable)
                    {
                        //ErrorModel error = new ErrorModel();
                        //error = JsonConvert.DeserializeObject<ErrorModel>(reponsestr);
                        response.ErrorMessage += "This Version is not supported or not provided in the header.";

                    }
                    else if (responsePost.StatusCode == HttpStatusCode.InternalServerError)
                    {
                        ErrorModel error = new ErrorModel();
                        error = JsonConvert.DeserializeObject<ErrorModel>(reponsestr);
                        response.ErrorMessage += error.Code + " : " + error.Message;
                    }
                    else
                    {
                        response.ErrorMessage += "Error in ComplianceCsr API";
                    }

                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException!=null)
                {
                    response.ErrorMessage += ex.InnerException.Message;
                }
                //response.ErrorMessage += ex.Message + "\n" + ex?.ToString() + "\n" + ex.InnerException?.ToString();
            }
            return response;
        }
        public ComplianceCsrResponse GetProductionCSIDAPI(string Compliance_request_id, string username, string password)
        {
            ComplianceCsrResponse response = new ComplianceCsrResponse();
            try
            {
                using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(GlobalVariables.BaseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Accept-Version", "V2");
                var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Basic {credentials}");
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072 | SecurityProtocolType.Tls;

                //POST Method
                    string sending_exception = "";
                    int LoopCount = 3;
                    HttpResponseMessage responsePost = null;

                    for (int i = 0; i <= LoopCount; i++)
                    {

                        try
                        {
                            var data = new StringContent(JsonConvert.SerializeObject(new { compliance_request_id = Compliance_request_id }), Encoding.UTF8, "application/json");
                            responsePost = client.PostAsync(GlobalVariables.ProdCsidEndpoint(mode), data).Result;
                            break;
                        }
                        catch (Exception ex)
                        {
                            if (i == LoopCount)
                            {
                                sending_exception = ex.Message + "\n" + ex?.ToString() + "\n" + ex.InnerException?.ToString();
                            }
                        }
                    }
                    if (responsePost == null)
                    {
                        response.ErrorMessage = sending_exception;
                        return response;
                    }

                   
                var reponsestr = responsePost.Content.ReadAsStringAsync().Result;
                response.StatusCode = (int)responsePost.StatusCode;
                if (responsePost.IsSuccessStatusCode)
                {
                        try
                        {
                            response = JsonConvert.DeserializeObject<ComplianceCsrResponse>(reponsestr);
                        }
                        catch
                        {

                        }
                    return response;
                }
                response.ErrorMessage = "Error StatusCode : " + responsePost.StatusCode + "  \n\r";

                if (responsePost.StatusCode == HttpStatusCode.BadRequest)
                {
                    ErrorModel error = new ErrorModel();
                    try
                    {
                        error = JsonConvert.DeserializeObject<ErrorModel>(reponsestr);
                        response.ErrorMessage += error.Code + " : " + error.Message;
                    }
                    catch
                    {
                        response.ErrorMessage += reponsestr;
                    }

                }
                else if (responsePost.StatusCode == HttpStatusCode.Unauthorized)
                {

                    response.ErrorMessage += "Unauthorized Production CSID API";
                }
                else if (responsePost.StatusCode == HttpStatusCode.NotAcceptable)
                {

                    response.ErrorMessage += "This Version is not supported or not provided in the header.";

                }
                else if (responsePost.StatusCode == HttpStatusCode.InternalServerError)
                {
                    ErrorModel error = new ErrorModel();
                    try
                    {
                        error = JsonConvert.DeserializeObject<ErrorModel>(reponsestr);
                        response.ErrorMessage += error.Code + " : " + error.Message;
                    }
                    catch
                    {
                        response.ErrorMessage += reponsestr;
                    }
                }
                else
                {
                    response.ErrorMessage += "Error in ProductionCSID API";
                }
                return response;

            }
            }
            catch (Exception ex)
            {
                response.ErrorMessage += ex.Message + "\n" + ex?.ToString() + "\n" + ex.InnerException?.ToString();

            }
            return response;
        }
        public InvoiceReportingResponse CallReportingAPI(string userName, string password, InvoiceReportingRequest inv)
        {
            InvoiceReportingResponse result = new InvoiceReportingResponse();
            try
            {
                using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(GlobalVariables.BaseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Add("accept-language", "en");
                client.DefaultRequestHeaders.Add("Clearance-Status", "0");
                client.DefaultRequestHeaders.Add("Accept-Version", "V2");
                var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(userName + ":" + password));
                //for live
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Basic {credentials}");
                //for test 
                //client.DefaultRequestHeaders.Add("Authorization", "Basic VFVsSlJEbHFRME5CTlhWblFYZEpRa0ZuU1ZSaWQwRkJaVU41T1dGTFkweEJPVGxJY2tGQlFrRkJRalJNUkVGTFFtZG5jV2hyYWs5UVVWRkVRV3BDYWsxU1ZYZEZkMWxMUTFwSmJXbGFVSGxNUjFGQ1IxSlpSbUpIT1dwWlYzZDRSWHBCVWtKbmIwcHJhV0ZLYXk5SmMxcEJSVnBHWjA1dVlqTlplRVo2UVZaQ1oyOUthMmxoU21zdlNYTmFRVVZhUm1ka2JHVklVbTVaV0hBd1RWSjNkMGRuV1VSV1VWRkVSWGhPVlZVeGNFWlRWVFZYVkRCc1JGSlRNVlJrVjBwRVVWTXdlRTFDTkZoRVZFbDVUVVJSZUU5VVNYZE9SR3QzVDFadldFUlVTVEJOUkZGNFQwUkpkMDVFYTNkUFZtOTNWMVJGVEUxQmEwZEJNVlZGUW1oTlExVXdSWGhGZWtGU1FtZE9Wa0pCYjFSRGFrMTRUV3BOTUU1VVdUTlBSR3Q0UkVSQlMwSm5UbFpDUVhOVVFURlNWRlpFUlc1TlExVkhRVEZWUlVGNFRXVldSazVWVEZNd05VNTZRVEZPYWtGM1RrUkJkRTE2UlhsTmVsRXhUbXBqTkU5VVFYZE5SRUY2VFVaWmQwVkJXVWhMYjFwSmVtb3dRMEZSV1VaTE5FVkZRVUZ2UkZGblFVVlpXVTFOYjA5aFJsbEJhRTFQTDNOMFpXOTBabHA1WVhaeU5uQXhNVk5UYkhkelN6bGhlbTF6VEZrM1lqRmlLMFpNYUhGTlFYSm9RakprY1VoTFltOTRjVXRPWm5aclMwUmxVR2h3Y1dwMWFUVm9ZMjR3WVU5RFFXcHJkMmRuU1RGTlNVZGhRbWRPVmtoU1JVVm5Xa2wzWjFrcmEyZFpkM2RuV1d0NFQzcEJOVUpuVGxaQ1FWRk5UV3BGZEZaR1RsVm1SRWwwVmtaT1ZXWkVUWFJPUkdSdFRWUmFhazFxV1hSUFJFRXlXV2t3TUZwVVJURk1WMGw1VG1wcmRFNHlSVFJOUkUwMFQwUlNhVnBVYkdwTlVqaDNTRkZaUzBOYVNXMXBXbEI1VEVkUlFrRlJkMUJOZWtWNVRYcFJNVTVxWXpSUFZFRjNUVVJCZWsxUk1IZERkMWxFVmxGUlRVUkJVWGhOVkVGM1RWRjNkME5uV1VSV1VWRmhSRUZPVlZVeFVYaEVSRUZMUW1kT1ZrSkJPRTFCTVZKVVZrUkJaRUpuVGxaSVVUUkZSbWRSVlU4MVdtbFZOMDVoYTFVelpXVnFWbUV6U1RKVE1VSXljMFIzYTNkSWQxbEVWbEl3YWtKQ1ozZEdiMEZWWkcxRFRTdDNZV2R5UjJSWVRsb3pVRzF4ZVc1TE5Xc3hkRk00ZDFSbldVUldVakJtUWtWamQxSlVRa1J2UlVkblVEUlpPV0ZJVWpCalJHOTJURE5TZW1SSFRubGlRelUyV1ZoU2FsbFROVzVpTTFsMVl6SkZkbEV5Vm5sa1JWWjFZMjA1YzJKRE9WVlZNWEJHVTFVMVYxUXdiRVJTVXpGVVpGZEtSRkZUTUhoTWJVNTVZa1JEUW5KUldVbExkMWxDUWxGVlNFRlJSVVZuWVVGM1oxb3dkMkpuV1VsTGQxbENRbEZWU0UxQlIwZFpiV2d3WkVoQk5reDVPVEJqTTFKcVkyMTNkV1Z0UmpCWk1rVjFXakk1TWt4dVRtaE1NRTVzWTI1U1JtSnVTblppUjNkMlZrWk9ZVkpYYkhWa2JUbHdXVEpXVkZFd1JYaE1iVlkwWkVka2FHVnVVWFZhTWpreVRHMTRkbGt5Um5OWU1WSlVWMnRXU2xSc1dsQlRWVTVHVEZaT01WbHJUa0pNVkVWdlRWTnJkVmt6U2pCTlEzTkhRME56UjBGUlZVWkNla0ZDYUdnNWIyUklVbmRQYVRoMlpFaE9NRmt6U25OTWJuQm9aRWRPYUV4dFpIWmthVFY2V1ZNNWRsa3pUbmROUVRSSFFURlZaRVIzUlVJdmQxRkZRWGRKU0dkRVFXUkNaMDVXU0ZOVlJVWnFRVlZDWjJkeVFtZEZSa0pSWTBSQloxbEpTM2RaUWtKUlZVaEJkMDEzU25kWlNrdDNXVUpDUVVkRFRuaFZTMEpDYjNkSFJFRkxRbWRuY2tKblJVWkNVV05FUVdwQlMwSm5aM0pDWjBWR1FsRmpSRUY2UVV0Q1oyZHhhR3RxVDFCUlVVUkJaMDVLUVVSQ1IwRnBSVUUzYlVoVU5ubG5PRFZxZEZGSFYzQXpUVGQwVUZRM1Ntc3lLM3B6ZGxaSVIzTXpZbFUxV2pkWlJUWTRRMGxSUkRZd1pXSlJZVzFaYWxsMlpHVmlia1pxVG1aNE5GZzBaRzl3TjB4elJVSkdRMDVUYzB4Wk1FbEdZVkU5UFE9PTo1UURiSDRGdEZCZXNYWWlicXI3UlVJZnkzNUNEVkl6Ulgza3E1MlliV2ZNPQ==");
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072 | SecurityProtocolType.Tls;

                //POST Method
                
                    string sending_exception = "";
                    int LoopCount = 3;
                    HttpResponseMessage responsePost = null;

                    for (int i = 0; i <= LoopCount; i++)
                    {

                        try
                        {
                            var data = new StringContent(JsonConvert.SerializeObject(inv), Encoding.UTF8, "application/json"); 
                            responsePost = client.PostAsync(GlobalVariables.InvoiceReportingEndPoint(mode), data).Result;
                            break;
                        }
                        catch (Exception ex)
                        {
                            if (i == LoopCount)
                            {
                                sending_exception = ex.Message + "\n" + ex?.ToString() + "\n" + ex.InnerException?.ToString();
                            }
                        }
                    }
                    if (responsePost == null)
                    {
                        result.ErrorMessage = sending_exception;
                        return result;
                    }

                    
                string reponsestr = responsePost.Content.ReadAsStringAsync().Result;
               
                
               
                if (responsePost.IsSuccessStatusCode)
                {
                    try
                    {
                        result = JsonConvert.DeserializeObject<InvoiceReportingResponse>(reponsestr);
                    }
                    catch
                    {
                       
                    }
                    result.IsSuccess = true;
                    result.StatusCode = (int)responsePost.StatusCode;
                    return result;
                }
                if (responsePost.StatusCode == HttpStatusCode.Accepted)
                {

                    try
                    {
                        result = JsonConvert.DeserializeObject<InvoiceReportingResponse>(reponsestr);

                        //foreach (var warning in result.validationResults.WarningMessages)
                        //{
                        //    result.WarningMessage = warning.Code + " : " + warning.Message + "\n";
                        //}
                       
                    }
                    catch
                    {
                    }
                    result.IsSuccess = true;
                    result.StatusCode = (int)responsePost.StatusCode;
                    return result;
                }
                result.ErrorMessage = "Error StatusCode : " + responsePost.StatusCode + "  \n\r";

                if (responsePost.StatusCode == HttpStatusCode.BadRequest)
                {

                    try
                    {
                        result = JsonConvert.DeserializeObject<InvoiceReportingResponse>(reponsestr);
                        if (result.validationResults.ErrorMessages != null && result.validationResults.ErrorMessages.Count > 0)
                        {
                            foreach (var error in result.validationResults.ErrorMessages)
                            {
                                result.ErrorMessage += error.Message + "\n";
                            }
                        }
                    }
                    catch
                    {
                        result.ErrorMessage += reponsestr;
                    }


                }
                if (responsePost.StatusCode == HttpStatusCode.Unauthorized)
                {
                    result = new InvoiceReportingResponse();
                    result.ErrorMessage += "NotAcceptable";
                    result.ReportingStatus = "Unauthorized";
                }
                if (responsePost.StatusCode == HttpStatusCode.NotAcceptable)
                {
                    result = new InvoiceReportingResponse();
                    result.ErrorMessage += "NotAcceptable";
                    result.ReportingStatus = "NotAcceptable";
                }
                if (responsePost.StatusCode == HttpStatusCode.InternalServerError)
                {
                    result = new InvoiceReportingResponse();
                    ErrorModel error = new ErrorModel();
                    try
                    {
                        error = JsonConvert.DeserializeObject<ErrorModel>(reponsestr);
                        result.ErrorMessage += "InternalServerError " + error.Code + " : " + error.Message;
                    }
                    catch
                    {
                        result.ErrorMessage += "InternalServerError " + reponsestr;
                    }
                    result.ReportingStatus = "InternalServerError";
                }
                if (responsePost.StatusCode == HttpStatusCode.NotFound)
                {
                    result = new InvoiceReportingResponse();
                    result.ErrorMessage += "NotFound";
                    result.ReportingStatus = "NotFound";
                }
                if (responsePost.StatusCode == HttpStatusCode.RequestEntityTooLarge)
                {
                    result = new InvoiceReportingResponse();
                    result.ErrorMessage += "Payload Too Large, invoice not received";
                    result.ReportingStatus = "XML is Too Large";
                }
                if ((int)responsePost.StatusCode == 429)
                {
                    result = new InvoiceReportingResponse();
                    result.ErrorMessage += "a user has sent too many requests within a short period of time";
                    result.ReportingStatus = "a user has sent too many requests within a short period of time";
                }
                if (responsePost.StatusCode == HttpStatusCode.GatewayTimeout)
                {
                    result = new InvoiceReportingResponse();
                    result.ErrorMessage += "Gateway Timeout";
                    result.ReportingStatus = "Gateway Timeout";
                }
                if (responsePost.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    result = new InvoiceReportingResponse();
                    result.ErrorMessage += "Service Unavailable";
                    result.ReportingStatus = "Service Unavailable";
                }
                result.StatusCode = (int)responsePost.StatusCode;
                return result;

            }
            }
            catch (Exception ex)
            {
                result.ErrorMessage += ex.Message + "\n" + ex?.ToString() + "\n" + ex.InnerException?.ToString();
                return result;
            }
        }
        public InvoiceClearanceResponse CallClearanceAPI(string userName, string password, InvoiceReportingRequest inv)
        {
            InvoiceClearanceResponse result = new InvoiceClearanceResponse();
            try
            {
                using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(GlobalVariables.BaseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Add("accept-language", "en");
                client.DefaultRequestHeaders.Add("Clearance-Status", "1");
                client.DefaultRequestHeaders.Add("Accept-Version", "V2");
                var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(userName + ":" + password));
                //for live
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Basic {credentials}");
                //for test 
                //client.DefaultRequestHeaders.Add("Authorization", "Basic VFVsSlJERkVRME5CTTIxblFYZEpRa0ZuU1ZSaWQwRkJaVE5WUVZsV1ZUTTBTUzhyTlZGQlFrRkJRamRrVkVGTFFtZG5jV2hyYWs5UVVWRkVRV3BDYWsxU1ZYZEZkMWxMUTFwSmJXbGFVSGxNUjFGQ1IxSlpSbUpIT1dwWlYzZDRSWHBCVWtKbmIwcHJhV0ZLYXk5SmMxcEJSVnBHWjA1dVlqTlplRVo2UVZaQ1oyOUthMmxoU21zdlNYTmFRVVZhUm1ka2JHVklVbTVaV0hBd1RWSjNkMGRuV1VSV1VWRkVSWGhPVlZVeGNFWlRWVFZYVkRCc1JGSlRNVlJrVjBwRVVWTXdlRTFDTkZoRVZFbDVUVVJaZUUxcVJUTk9SRUV4VFd4dldFUlVTVEJOUkZsNFRWUkZNMDVFUVRGTmJHOTNVMVJGVEUxQmEwZEJNVlZGUW1oTlExVXdSWGhFYWtGTlFtZE9Wa0pCYjFSQ1YwWnVZVmQ0YkUxU1dYZEdRVmxFVmxGUlRFVjNNVzlaV0d4b1NVaHNhRm95YUhSaU0xWjVUVkpKZDBWQldVUldVVkZFUlhkcmVFMXFZM1ZOUXpSM1RHcEZkMVpxUVZGQ1oyTnhhR3RxVDFCUlNVSkNaMVZ5WjFGUlFVTm5Ua05CUVZSVVFVczViSEpVVm10dk9YSnJjVFphV1dOak9VaEVVbHBRTkdJNVV6UjZRVFJMYlRkWldFb3JjMjVVVm1oTWEzcFZNRWh6YlZOWU9WVnVPR3BFYUZKVVQwaEVTMkZtZERoREwzVjFWVms1TXpSMmRVMU9ielJKUTBwNlEwTkJhVTEzWjFsblIwRXhWV1JGVVZOQ1owUkNLM0JJZDNkbGFrVmlUVUpyUjBFeFZVVkNRWGRUVFZNeGIxbFliR2htUkVsMFRXcE5NR1pFVFhSTlZFVjVUVkk0ZDBoUldVdERXa2x0YVZwUWVVeEhVVUpCVVhkUVRYcEJkMDFFWXpGT1ZHYzBUbnBCZDAxRVFYcE5VVEIzUTNkWlJGWlJVVTFFUVZGNFRWUkJkMDFTUlhkRWQxbEVWbEZSWVVSQmFHRlpXRkpxV1ZOQmVFMXFSVmxOUWxsSFFURlZSVVIzZDFCU2JUbDJXa05DUTJSWVRucGhWelZzWXpOTmVrMUNNRWRCTVZWa1JHZFJWMEpDVTJkdFNWZEVObUpRWm1KaVMydHRWSGRQU2xKWWRrbGlTRGxJYWtGbVFtZE9Wa2hUVFVWSFJFRlhaMEpTTWxsSmVqZENjVU56V2pGak1XNWpLMkZ5UzJOeWJWUlhNVXg2UWs5Q1owNVdTRkk0UlZKNlFrWk5SVTluVVdGQkwyaHFNVzlrU0ZKM1QyazRkbVJJVGpCWk0wcHpURzV3YUdSSFRtaE1iV1IyWkdrMWVsbFRPVVJhV0Vvd1VsYzFlV0l5ZUhOTU1WSlVWMnRXU2xSc1dsQlRWVTVHVEZaT01WbHJUa0pNVkVWMVdUTktjMDFKUjNSQ1oyZHlRbWRGUmtKUlkwSkJVVk5DYjBSRFFtNVVRblZDWjJkeVFtZEZSa0pSWTNkQldWcHBZVWhTTUdORWIzWk1NMUo2WkVkT2VXSkROVFpaV0ZKcVdWTTFibUl6V1hWak1rVjJVVEpXZVdSRlZuVmpiVGx6WWtNNVZWVXhjRVpoVnpVeVlqSnNhbHBXVGtSUlZFVjFXbGhvTUZveVJqWmtRelZ1WWpOWmRXSkhPV3BaVjNobVZrWk9ZVkpWYkU5V2F6bEtVVEJWZEZVelZtbFJNRVYwVFZObmVFdFROV3BqYmxGM1MzZFpTVXQzV1VKQ1VWVklUVUZIUjBneWFEQmtTRUUyVEhrNU1HTXpVbXBqYlhkMVpXMUdNRmt5UlhWYU1qa3lURzVPYUV3eU9XcGpNMEYzUkdkWlJGWlNNRkJCVVVndlFrRlJSRUZuWlVGTlFqQkhRVEZWWkVwUlVWZE5RbEZIUTBOelIwRlJWVVpDZDAxRFFtZG5ja0puUlVaQ1VXTkVRWHBCYmtKbmEzSkNaMFZGUVZsSk0wWlJiMFZIYWtGWlRVRnZSME5EYzBkQlVWVkdRbmROUTAxQmIwZERRM05IUVZGVlJrSjNUVVJOUVc5SFEwTnhSMU5OTkRsQ1FVMURRVEJyUVUxRldVTkpVVU5XZDBSTlkzRTJVRThyVFdOdGMwSllWWG92ZGpGSFpHaEhjRGR5Y1ZOaE1rRjRWRXRUZGpnek9FbEJTV2hCVDBKT1JFSjBPU3N6UkZOc2FXcHZWbVo0ZW5Ka1JHZzFNamhYUXpNM2MyMUZaRzlIVjFaeVUzQkhNUT09OlhsajE1THlNQ2dTQzY2T2JuRU8vcVZQZmhTYnMza0RUalduR2hlWWhmU3M9");
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072 | SecurityProtocolType.Tls;

                //POST Method
                
                    string sending_exception = "";
                    int LoopCount = 3;
                    HttpResponseMessage responsePost = null;

                    for (int i = 0; i <= LoopCount; i++)
                    {

                        try
                        {
                            var data = new StringContent(JsonConvert.SerializeObject(inv), Encoding.UTF8, "application/json");
                            responsePost = client.PostAsync(GlobalVariables.InvoiceClearanceEndPoint(mode), data).Result;
                            break;
                        }
                        catch (Exception ex)
                        {
                            if (i == LoopCount)
                            {
                                sending_exception = ex.Message + "\n" + ex?.ToString() + "\n" + ex.InnerException?.ToString();
                            }
                        }
                    }
                    if (responsePost == null)
                    {
                        result.ErrorMessage = sending_exception;
                        return result;
                    }
                   
                string reponsestr = responsePost.Content.ReadAsStringAsync().Result;
                
                if (responsePost.IsSuccessStatusCode)
                {
                    try
                    {
                        result = JsonConvert.DeserializeObject<InvoiceClearanceResponse>(reponsestr);
                        var doc = new XmlDocument();
                        if (!string.IsNullOrEmpty(result.ClearedInvoice))
                        {
                            doc.PreserveWhitespace = true;
                            result.SingedXML = Utility.Base64Dencode(result.ClearedInvoice);
                            doc.LoadXml(result.SingedXML);
                            result.QRCode = Utility.GetNodeInnerText(doc, SettingsParams.QR_CODE_XPATH);
                            //here to save xml file
                            if (this.SaveXML&&!string.IsNullOrEmpty(this.Directorypath))
                            {
                                string clearedxmldir = "";
                                string clearedxmlfilename = "";
                                SaveClearedXmlFile(doc, this.Directorypath, ref clearedxmldir, ref clearedxmlfilename);
                                result.ClearedXMLFileName = clearedxmlfilename;
                                result.ClearedXMLFileNameShortPath = clearedxmldir + clearedxmlfilename;
                                result.ClearedXMLFileNameFullPath = this.Directorypath + clearedxmldir + clearedxmlfilename;
                            }
                        }
                        result.IsSuccess = true;
                    }
                    catch
                    {

                    }
                    result.StatusCode = (int)responsePost.StatusCode;
                    return result;
                }
                if (responsePost.StatusCode == HttpStatusCode.Accepted)
                {
                    
                    try
                    {
                        result = JsonConvert.DeserializeObject<InvoiceClearanceResponse>(reponsestr);
                        //foreach (var warning in result.validationResults.WarningMessages)
                        //{
                        //    result.WarningMessage = warning.Code + " : " + warning.Message + "\n";
                        //}
                        var doc = new XmlDocument();
                        if (!string.IsNullOrEmpty(result.ClearedInvoice))
                        {
                            doc.PreserveWhitespace = true;
                            result.SingedXML = Utility.Base64Dencode(result.ClearedInvoice);
                            doc.LoadXml(result.SingedXML);
                            result.QRCode = Utility.GetNodeInnerText(doc, SettingsParams.QR_CODE_XPATH);
                            //here to save xml file
                            if (this.SaveXML && !string.IsNullOrEmpty(this.Directorypath))
                            {
                                string clearedxmldir = "";
                                string clearedxmlfilename = "";
                                SaveClearedXmlFile(doc, this.Directorypath,ref clearedxmldir, ref clearedxmlfilename);
                                result.ClearedXMLFileName= clearedxmlfilename;
                                result.ClearedXMLFileNameShortPath = clearedxmldir+clearedxmlfilename;
                                result.ClearedXMLFileNameFullPath = this.Directorypath+ clearedxmldir + clearedxmlfilename;

                            }
                        }
                    }
                    catch
                    {

                    }
                    result.IsSuccess = true;
                    result.StatusCode = (int)responsePost.StatusCode;
                    return result;
                }
                result.ErrorMessage = "Error StatusCode : " + responsePost.StatusCode + "  \n\r";
                if (responsePost.StatusCode == HttpStatusCode.BadRequest)
                {
                    try
                    {
                        result = JsonConvert.DeserializeObject<InvoiceClearanceResponse>(reponsestr);
                        if (result.validationResults.ErrorMessages != null && result.validationResults.ErrorMessages.Count > 0)
                        {
                            foreach (var error in result.validationResults.ErrorMessages)
                            {
                                result.ErrorMessage += error.Message + "\n";
                            }
                        }

                    }
                    catch
                    {
                    }
                   
                }
                if (responsePost.StatusCode == HttpStatusCode.Unauthorized)
                {
                    result = new InvoiceClearanceResponse();
                    result.ErrorMessage += "Unauthorized in Clearance API";
                    result.ClearanceStatus = "Unauthorized";
                }
                if (responsePost.StatusCode == HttpStatusCode.NotAcceptable)
                {
                    result = new InvoiceClearanceResponse();
                    result.ErrorMessage += "This Version is not supported or not provided in the header.";
                    result.ClearanceStatus = "NotAcceptable";
                }
                if (responsePost.StatusCode == HttpStatusCode.InternalServerError)
                {
                    result = new InvoiceClearanceResponse();
                    ErrorModel error = new ErrorModel();
                    try
                    {
                        error = JsonConvert.DeserializeObject<ErrorModel>(reponsestr);
                        result.ErrorMessage += "InternalServerError "+error.Code + " : " + error.Message;
                    }
                    catch
                    {
                        result.ErrorMessage += "InternalServerError "+ reponsestr;
                    }
                    result.ClearanceStatus = "InternalServerError";
                }
                if (responsePost.StatusCode == HttpStatusCode.NotFound)
                {
                    result = new InvoiceClearanceResponse();
                    result.ErrorMessage += "NotFound";
                    result.ClearanceStatus = "NotFound";
                }
                if (responsePost.StatusCode == HttpStatusCode.RequestEntityTooLarge)
                {
                    result = new InvoiceClearanceResponse();
                    result.ErrorMessage += "Payload Too Large, invoice not received";
                    result.ClearanceStatus = "XML is Too Large";
                }
                if ((int)responsePost.StatusCode == 429)
                {
                    result = new InvoiceClearanceResponse();
                    result.ErrorMessage += "a user has sent too many requests within a short period of time";
                    result.ClearanceStatus = "a user has sent too many requests within a short period of time";
                }
                if (responsePost.StatusCode == HttpStatusCode.GatewayTimeout)
                {
                    result = new InvoiceClearanceResponse();
                    result.ErrorMessage += "Gateway Timeout";
                    result.ClearanceStatus = "Gateway Timeout";
                }
                if (responsePost.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    result = new InvoiceClearanceResponse();
                    result.ErrorMessage += "Service Unavailable";
                    result.ClearanceStatus = "Service Unavailable";
                }
                result.StatusCode = (int)responsePost.StatusCode;
                return result;

            }
            }
            catch (Exception ex)
            {
                result.ErrorMessage += ex.Message + "\n" + ex?.ToString() + "\n" + ex.InnerException?.ToString();
                return result;
            }
        }
        public InvoiceReportingResponse CallComplianceInvoiceAPI(string userName, string password, InvoiceReportingRequest inv)
        {
            InvoiceReportingResponse response = new InvoiceReportingResponse();
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(GlobalVariables.BaseUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    client.DefaultRequestHeaders.Add("accept-language", "en");
                    client.DefaultRequestHeaders.Add("Accept-Version", "V2");
                    var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(userName + ":" + password));
                    //for live
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Basic {credentials}");
                    //for test 
                    //client.DefaultRequestHeaders.Add("Authorization", "Basic VFVsSlJERkVRME5CTTIxblFYZEpRa0ZuU1ZSaWQwRkJaVE5WUVZsV1ZUTTBTUzhyTlZGQlFrRkJRamRrVkVGTFFtZG5jV2hyYWs5UVVWRkVRV3BDYWsxU1ZYZEZkMWxMUTFwSmJXbGFVSGxNUjFGQ1IxSlpSbUpIT1dwWlYzZDRSWHBCVWtKbmIwcHJhV0ZLYXk5SmMxcEJSVnBHWjA1dVlqTlplRVo2UVZaQ1oyOUthMmxoU21zdlNYTmFRVVZhUm1ka2JHVklVbTVaV0hBd1RWSjNkMGRuV1VSV1VWRkVSWGhPVlZVeGNFWlRWVFZYVkRCc1JGSlRNVlJrVjBwRVVWTXdlRTFDTkZoRVZFbDVUVVJaZUUxcVJUTk9SRUV4VFd4dldFUlVTVEJOUkZsNFRWUkZNMDVFUVRGTmJHOTNVMVJGVEUxQmEwZEJNVlZGUW1oTlExVXdSWGhFYWtGTlFtZE9Wa0pCYjFSQ1YwWnVZVmQ0YkUxU1dYZEdRVmxFVmxGUlRFVjNNVzlaV0d4b1NVaHNhRm95YUhSaU0xWjVUVkpKZDBWQldVUldVVkZFUlhkcmVFMXFZM1ZOUXpSM1RHcEZkMVpxUVZGQ1oyTnhhR3RxVDFCUlNVSkNaMVZ5WjFGUlFVTm5Ua05CUVZSVVFVczViSEpVVm10dk9YSnJjVFphV1dOak9VaEVVbHBRTkdJNVV6UjZRVFJMYlRkWldFb3JjMjVVVm1oTWEzcFZNRWh6YlZOWU9WVnVPR3BFYUZKVVQwaEVTMkZtZERoREwzVjFWVms1TXpSMmRVMU9ielJKUTBwNlEwTkJhVTEzWjFsblIwRXhWV1JGVVZOQ1owUkNLM0JJZDNkbGFrVmlUVUpyUjBFeFZVVkNRWGRUVFZNeGIxbFliR2htUkVsMFRXcE5NR1pFVFhSTlZFVjVUVkk0ZDBoUldVdERXa2x0YVZwUWVVeEhVVUpCVVhkUVRYcEJkMDFFWXpGT1ZHYzBUbnBCZDAxRVFYcE5VVEIzUTNkWlJGWlJVVTFFUVZGNFRWUkJkMDFTUlhkRWQxbEVWbEZSWVVSQmFHRlpXRkpxV1ZOQmVFMXFSVmxOUWxsSFFURlZSVVIzZDFCU2JUbDJXa05DUTJSWVRucGhWelZzWXpOTmVrMUNNRWRCTVZWa1JHZFJWMEpDVTJkdFNWZEVObUpRWm1KaVMydHRWSGRQU2xKWWRrbGlTRGxJYWtGbVFtZE9Wa2hUVFVWSFJFRlhaMEpTTWxsSmVqZENjVU56V2pGak1XNWpLMkZ5UzJOeWJWUlhNVXg2UWs5Q1owNVdTRkk0UlZKNlFrWk5SVTluVVdGQkwyaHFNVzlrU0ZKM1QyazRkbVJJVGpCWk0wcHpURzV3YUdSSFRtaE1iV1IyWkdrMWVsbFRPVVJhV0Vvd1VsYzFlV0l5ZUhOTU1WSlVWMnRXU2xSc1dsQlRWVTVHVEZaT01WbHJUa0pNVkVWMVdUTktjMDFKUjNSQ1oyZHlRbWRGUmtKUlkwSkJVVk5DYjBSRFFtNVVRblZDWjJkeVFtZEZSa0pSWTNkQldWcHBZVWhTTUdORWIzWk1NMUo2WkVkT2VXSkROVFpaV0ZKcVdWTTFibUl6V1hWak1rVjJVVEpXZVdSRlZuVmpiVGx6WWtNNVZWVXhjRVpoVnpVeVlqSnNhbHBXVGtSUlZFVjFXbGhvTUZveVJqWmtRelZ1WWpOWmRXSkhPV3BaVjNobVZrWk9ZVkpWYkU5V2F6bEtVVEJWZEZVelZtbFJNRVYwVFZObmVFdFROV3BqYmxGM1MzZFpTVXQzV1VKQ1VWVklUVUZIUjBneWFEQmtTRUUyVEhrNU1HTXpVbXBqYlhkMVpXMUdNRmt5UlhWYU1qa3lURzVPYUV3eU9XcGpNMEYzUkdkWlJGWlNNRkJCVVVndlFrRlJSRUZuWlVGTlFqQkhRVEZWWkVwUlVWZE5RbEZIUTBOelIwRlJWVVpDZDAxRFFtZG5ja0puUlVaQ1VXTkVRWHBCYmtKbmEzSkNaMFZGUVZsSk0wWlJiMFZIYWtGWlRVRnZSME5EYzBkQlVWVkdRbmROUTAxQmIwZERRM05IUVZGVlJrSjNUVVJOUVc5SFEwTnhSMU5OTkRsQ1FVMURRVEJyUVUxRldVTkpVVU5XZDBSTlkzRTJVRThyVFdOdGMwSllWWG92ZGpGSFpHaEhjRGR5Y1ZOaE1rRjRWRXRUZGpnek9FbEJTV2hCVDBKT1JFSjBPU3N6UkZOc2FXcHZWbVo0ZW5Ka1JHZzFNamhYUXpNM2MyMUZaRzlIVjFaeVUzQkhNUT09OlhsajE1THlNQ2dTQzY2T2JuRU8vcVZQZmhTYnMza0RUalduR2hlWWhmU3M9");
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072 | SecurityProtocolType.Tls;

                    //POST Method

                    string sending_exception = "";
                    int LoopCount = 3;
                    HttpResponseMessage responsePost = null;

                    for (int i = 0; i <= LoopCount; i++)
                    {

                        try
                        {
                            var data = new StringContent(JsonConvert.SerializeObject(inv), Encoding.UTF8, "application/json");
                            responsePost = client.PostAsync(GlobalVariables.ComplianceInvoiceEndPoint(mode), data).Result;
                            break;
                        }
                        catch (Exception ex)
                        {
                            if (i == LoopCount)
                            {
                                sending_exception = ex.Message + "\n" + ex?.ToString() + "\n" + ex.InnerException?.ToString();
                            }
                        }
                    }
                    if (responsePost == null)
                    {
                        response.ErrorMessage = sending_exception;
                        return response;
                    }
                   
                    string reponsestr = responsePost.Content.ReadAsStringAsync().Result;
                    response.IsSuccess = false;
                    if (responsePost.IsSuccessStatusCode)
                    {
                        try
                        {
                            response = JsonConvert.DeserializeObject<InvoiceReportingResponse>(reponsestr);
                        }
                        catch
                        {

                        }
                        response.IsSuccess = true;
                        response.StatusCode = (int)responsePost.StatusCode;
                        return response;
                    }
                    if (responsePost.StatusCode == HttpStatusCode.Accepted)
                    {
                        try
                        {
                            response = JsonConvert.DeserializeObject<InvoiceReportingResponse>(reponsestr);

                            //foreach (var warning in response.validationResults.WarningMessages)
                            //{
                            //    response.WarningMessage = warning.Code + " : " + warning.Message + "\n";
                            //}

                        }
                        catch
                        {
                        }
                        response.IsSuccess = true;
                        response.StatusCode = (int)responsePost.StatusCode;
                        return response;
                    }
                    response.ErrorMessage = "Error StatusCode : " + responsePost.StatusCode + "  \n\r";
                    if (responsePost.StatusCode == HttpStatusCode.BadRequest)
                    {
                        try
                        {
                            response = JsonConvert.DeserializeObject<InvoiceReportingResponse>(reponsestr);
                            if (response.validationResults.ErrorMessages != null && response.validationResults.ErrorMessages.Count > 0)
                            {
                                foreach (var error in response.validationResults.ErrorMessages)
                                {
                                    response.ErrorMessage += error.Message + "\n";
                                }
                            }

                        }
                        catch
                        {
                        }
                    }
                    else if (responsePost.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        response.ReportingStatus = "Unauthorized";
                        response.ErrorMessage += "Unauthorized in Compliance Invoice API";
                    }
                    else if (responsePost.StatusCode == HttpStatusCode.NotAcceptable)
                    {
                        response.ReportingStatus = "NotAcceptable";
                        response.ErrorMessage += "This Version is not supported or not provided in the header.";
                    }
                    else if (responsePost.StatusCode == HttpStatusCode.InternalServerError)
                    {
                        ErrorModel error = new ErrorModel();
                        try
                        {
                            error = JsonConvert.DeserializeObject<ErrorModel>(reponsestr);
                            response.ErrorMessage += "InternalServerError " + error.Code + " : " + error.Message;
                        }
                        catch
                        {
                            response.ErrorMessage += "InternalServerError " + reponsestr;
                        }
                        response.ReportingStatus = "InternalServerError";
                    }
                    else if (responsePost.StatusCode == HttpStatusCode.RequestEntityTooLarge)
                    {
                        response = new InvoiceReportingResponse();
                        response.ErrorMessage += "Payload Too Large, invoice not received";
                        response.ReportingStatus = "XML is Too Large";
                    }
                    else if ((int)responsePost.StatusCode == 429)
                    {
                        response = new InvoiceReportingResponse();
                        response.ErrorMessage += "a user has sent too many requests within a short period of time";
                        response.ReportingStatus = "a user has sent too many requests within a short period of time";
                    }
                    else if (responsePost.StatusCode == HttpStatusCode.GatewayTimeout)
                    {
                        response = new InvoiceReportingResponse();
                        response.ErrorMessage += "Gateway Timeout";
                        response.ReportingStatus = "Gateway Timeout";
                    }
                    else if (responsePost.StatusCode == HttpStatusCode.ServiceUnavailable)
                    {
                        response = new InvoiceReportingResponse();
                        response.ErrorMessage += "Service Unavailable";
                        response.ReportingStatus = "Service Unavailable";
                    }
                    else
                    {
                        response.ErrorMessage += "Error in Compliance Invoice API";
                    }
                    response.StatusCode = (int)responsePost.StatusCode;
                }
               
            }
            catch (Exception ex)
            {

                response.ErrorMessage += ex.Message + "\n" + ex?.ToString() + "\n" + ex.InnerException?.ToString();
            }
           
            return response;
        }
        public ComplianceCsrResponse GetProductionCSIDRenewalAPI(string OTP, string CSR, string username, string password)
        {
            ComplianceCsrResponse response = new ComplianceCsrResponse();
            try { 
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(GlobalVariables.BaseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("OTP", OTP);
                client.DefaultRequestHeaders.Add("accept-language", "en");
                client.DefaultRequestHeaders.Add("Accept-Version", "V2");


                var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));
                //string credentials = @"VFVsSlJERkVRME5CTTIxblFYZEpRa0ZuU1ZSaWQwRkJaVE5WUVZsV1ZUTTBTUzhyTlZGQlFrRkJRamRrVkVGTFFtZG5jV2hyYWs5UVVWRkVRV3BDYWsxU1ZYZEZkMWxMUTFwSmJXbGFVSGxNUjFGQ1IxSlpSbUpIT1dwWlYzZDRSWHBCVWtKbmIwcHJhV0ZLYXk5SmMxcEJSVnBHWjA1dVlqTlplRVo2UVZaQ1oyOUthMmxoU21zdlNYTmFRVVZhUm1ka2JHVklVbTVaV0hBd1RWSjNkMGRuV1VSV1VWRkVSWGhPVlZVeGNFWlRWVFZYVkRCc1JGSlRNVlJrVjBwRVVWTXdlRTFDTkZoRVZFbDVUVVJaZUUxcVJUTk9SRUV4VFd4dldFUlVTVEJOUkZsNFRWUkZNMDVFUVRGTmJHOTNVMVJGVEUxQmEwZEJNVlZGUW1oTlExVXdSWGhFYWtGTlFtZE9Wa0pCYjFSQ1YwWnVZVmQ0YkUxU1dYZEdRVmxFVmxGUlRFVjNNVzlaV0d4b1NVaHNhRm95YUhSaU0xWjVUVkpKZDBWQldVUldVVkZFUlhkcmVFMXFZM1ZOUXpSM1RHcEZkMVpxUVZGQ1oyTnhhR3RxVDFCUlNVSkNaMVZ5WjFGUlFVTm5Ua05CUVZSVVFVczViSEpVVm10dk9YSnJjVFphV1dOak9VaEVVbHBRTkdJNVV6UjZRVFJMYlRkWldFb3JjMjVVVm1oTWEzcFZNRWh6YlZOWU9WVnVPR3BFYUZKVVQwaEVTMkZtZERoREwzVjFWVms1TXpSMmRVMU9ielJKUTBwNlEwTkJhVTEzWjFsblIwRXhWV1JGVVZOQ1owUkNLM0JJZDNkbGFrVmlUVUpyUjBFeFZVVkNRWGRUVFZNeGIxbFliR2htUkVsMFRXcE5NR1pFVFhSTlZFVjVUVkk0ZDBoUldVdERXa2x0YVZwUWVVeEhVVUpCVVhkUVRYcEJkMDFFWXpGT1ZHYzBUbnBCZDAxRVFYcE5VVEIzUTNkWlJGWlJVVTFFUVZGNFRWUkJkMDFTUlhkRWQxbEVWbEZSWVVSQmFHRlpXRkpxV1ZOQmVFMXFSVmxOUWxsSFFURlZSVVIzZDFCU2JUbDJXa05DUTJSWVRucGhWelZzWXpOTmVrMUNNRWRCTVZWa1JHZFJWMEpDVTJkdFNWZEVObUpRWm1KaVMydHRWSGRQU2xKWWRrbGlTRGxJYWtGbVFtZE9Wa2hUVFVWSFJFRlhaMEpTTWxsSmVqZENjVU56V2pGak1XNWpLMkZ5UzJOeWJWUlhNVXg2UWs5Q1owNVdTRkk0UlZKNlFrWk5SVTluVVdGQkwyaHFNVzlrU0ZKM1QyazRkbVJJVGpCWk0wcHpURzV3YUdSSFRtaE1iV1IyWkdrMWVsbFRPVVJhV0Vvd1VsYzFlV0l5ZUhOTU1WSlVWMnRXU2xSc1dsQlRWVTVHVEZaT01WbHJUa0pNVkVWMVdUTktjMDFKUjNSQ1oyZHlRbWRGUmtKUlkwSkJVVk5DYjBSRFFtNVVRblZDWjJkeVFtZEZSa0pSWTNkQldWcHBZVWhTTUdORWIzWk1NMUo2WkVkT2VXSkROVFpaV0ZKcVdWTTFibUl6V1hWak1rVjJVVEpXZVdSRlZuVmpiVGx6WWtNNVZWVXhjRVpoVnpVeVlqSnNhbHBXVGtSUlZFVjFXbGhvTUZveVJqWmtRelZ1WWpOWmRXSkhPV3BaVjNobVZrWk9ZVkpWYkU5V2F6bEtVVEJWZEZVelZtbFJNRVYwVFZObmVFdFROV3BqYmxGM1MzZFpTVXQzV1VKQ1VWVklUVUZIUjBneWFEQmtTRUUyVEhrNU1HTXpVbXBqYlhkMVpXMUdNRmt5UlhWYU1qa3lURzVPYUV3eU9XcGpNMEYzUkdkWlJGWlNNRkJCVVVndlFrRlJSRUZuWlVGTlFqQkhRVEZWWkVwUlVWZE5RbEZIUTBOelIwRlJWVVpDZDAxRFFtZG5ja0puUlVaQ1VXTkVRWHBCYmtKbmEzSkNaMFZGUVZsSk0wWlJiMFZIYWtGWlRVRnZSME5EYzBkQlVWVkdRbmROUTAxQmIwZERRM05IUVZGVlJrSjNUVVJOUVc5SFEwTnhSMU5OTkRsQ1FVMURRVEJyUVUxRldVTkpVVU5XZDBSTlkzRTJVRThyVFdOdGMwSllWWG92ZGpGSFpHaEhjRGR5Y1ZOaE1rRjRWRXRUZGpnek9FbEJTV2hCVDBKT1JFSjBPU3N6UkZOc2FXcHZWbVo0ZW5Ka1JHZzFNamhYUXpNM2MyMUZaRzlIVjFaeVUzQkhNUT09OlhsajE1THlNQ2dTQzY2T2JuRU8vcVZQZmhTYnMza0RUalduR2hlWWhmU3M9";
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Basic {credentials}");
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072 | SecurityProtocolType.Tls;

                //POST Method
                if (string.IsNullOrEmpty(CSR))
                {
                    response.ErrorMessage = "Please Enter csr";
                    return response;
                }
                //string ttt = @"LS0tLS1CRUdJTiBDRVJUSUZJQ0FURSBSRVFVRVNULS0tLS0NCk1JSUNEakNDQWJVQ0FRQXdkekVMTUFrR0ExVUVCaE1DVTBFeEh6QWRCZ05WQkFzTUZrRmpiV1VnVjJsa1oyVjANCnc2TENnTUtaY3lCTVZFUXhIekFkQmdOVkJBb01Ga0ZqYldVZ1YybGtaMlYwdzZMQ2dNS1pjeUJNVkVReEpqQWsNCkJnTlZCQU1NSFZSVFZDMDRPRFkwTXpFeE5EWXRNekF3TVRjeE5UWTVPVEF3TURBek1GWXdFQVlIS29aSXpqMEMNCkFRWUZLNEVFQUFvRFFnQUVid0NMcHBaNi9ta0dnazNONUlRRWt3am5vRG5UaUQzUitjMkh0bjQxRFJqdGZaekENCkpFaDFIdjBOTHY1QnBRUUZJZ21oV2NUeGZWU3Q1Tm9oZ2pnN2g2Q0IzakNCMndZSktvWklodmNOQVFrT01ZSE4NCk1JSEtNQ0VHQ1NzR0FRUUJnamNVQWdRVURCSmFRVlJEUVMxRGIyUmxMVk5wWjI1cGJtY3dnYVFHQTFVZEVRU0INCm5EQ0JtYVNCbGpDQmt6RTdNRGtHQTFVRUJBd3lNUzFVVTFSOE1pMVVVMVI4TXkxbFpESXlaakZrT0MxbE5tRXkNCkxURXhNVGt0T1dJMU9DMWtPV0U0WmpFeFpUUTBOV1l4SHpBZEJnb0praWFKay9Jc1pBRUJEQTh6TVRBd09UUXcNCk1UQXpNREF3TURNeERUQUxCZ05WQkF3TUJERXhNREF4RGpBTUJnTlZCQm9NQlZKNVlXUm9NUlF3RWdZRFZRUVANCkRBdElaV0ZzZEdnZ1EyRnlaVEFLQmdncWhrak9QUVFEQWdOSEFEQkVBaUJuUEI3cDJBc1dubjJlZ1I2R2tvRW0NClNBWE5TeWUxZEo2YjQxb0h5UVBwWGdJZ1R0K0cyRjYxMzV4cnlMRm05b0dEbnA2bW56U1dyN3Nmb2UwNXl0R1UNCnJ5TT0NCi0tLS0tRU5EIENFUlRJRklDQVRFIFJFUVVFU1QtLS0tLQ0K";
                var data = new StringContent(JsonConvert.SerializeObject(new { csr = Utility.ToBase64Encode(CSR) }), Encoding.UTF8, "application/json");
                var request = new HttpRequestMessage(new HttpMethod("PATCH"), GlobalVariables.ProdCsidEndpoint(mode));
                request.Content = data;
                client.Timeout = TimeSpan.FromSeconds(90);
                HttpResponseMessage responsePost = client.SendAsync(request).Result;

                var reponsestr = responsePost.Content.ReadAsStringAsync().Result;

                if (responsePost.IsSuccessStatusCode)
                {
                    response = JsonConvert.DeserializeObject<ComplianceCsrResponse>(reponsestr);
                    return response;
                }
                if ((int)responsePost.StatusCode == 428)
                {
                    response = JsonConvert.DeserializeObject<ComplianceCsrResponse>(reponsestr);
                    return response;
                }
                response.ErrorMessage = "Error StatusCode : " + responsePost.StatusCode + "  \n\r";

                if (responsePost.StatusCode == HttpStatusCode.BadRequest)
                {

                    ErrorModel error = new ErrorModel();
                    try
                    {
                        error = JsonConvert.DeserializeObject<ErrorModel>(reponsestr);
                        response.ErrorMessage += error.Code + " : " + error.Message;
                    }
                    catch
                    {
                        response.ErrorMessage += reponsestr;
                    }
                }
                else if (responsePost.StatusCode == HttpStatusCode.Unauthorized)
                {

                    response.ErrorMessage += "Unauthorized Production CSID API";
                }
                else if (responsePost.StatusCode == HttpStatusCode.NotAcceptable)
                {

                    response.ErrorMessage += "This Version is not supported or not provided in the header.";

                }
                else if (responsePost.StatusCode == HttpStatusCode.InternalServerError)
                {
                    ErrorModel error = new ErrorModel();
                    try
                    {
                        error = JsonConvert.DeserializeObject<ErrorModel>(reponsestr);
                        response.ErrorMessage += error.Code + " : " + error.Message;
                    }
                    catch
                    {
                        response.ErrorMessage += reponsestr;
                    }
                }
                else
                {
                    response.ErrorMessage += "Error in ProductionCSID API";
                }
                return response;

            }
            }
            catch (Exception ex)
            {
                response.ErrorMessage += ex.Message + "\n" + ex?.ToString() + "\n" + ex.InnerException?.ToString();

            }
            return response;
        }

        private void SaveClearedXmlFile(XmlDocument doc, string Directorypath, ref string clearedxmldir, ref string clearedxmlfilename)
        {
           
            GetNormalXMLPath(doc, ref clearedxmldir, ref clearedxmlfilename);
            string signedxmlfilename = Directorypath + clearedxmldir+ clearedxmlfilename;
            if (this.SaveXML)
            {
                Utility.CreateClearedInvoicesFolder(Directorypath);
                //FileMode.Create will overwrite the file.No seek and truncate is needed.
                using (var fs = new FileStream(signedxmlfilename, FileMode.Create))
                {
                    doc.Save(fs);
                }
            }
        }
        private void GetNormalXMLPath(XmlDocument doc, ref string returnsignedxmldir,ref string xmlfilename)
        {
            try
            {
                string invoicetype = Utility.GetInvoiceType(doc);
                string invoicetypecode = Utility.GetNodeInnerText(doc, SettingsParams.Invoice_Type_XPATH);
                string VAT_REGISTERATION = Utility.GetNodeInnerText(doc, SettingsParams.VAT_REGISTERATION_XPATH);
                string ISSUE_DATE = Utility.GetNodeInnerText(doc, SettingsParams.ISSUE_DATE_XPATH);
                string ISSUE_TIME = Utility.GetNodeInnerText(doc, SettingsParams.ISSUE_TIME_XPATH);
                string INVOICE_ID = Utility.GetNodeInnerText(doc, SettingsParams.INVOICE_ID_XPATH);

                string xmlfile = VAT_REGISTERATION + "_" + ISSUE_DATE.Replace("-", "") + "T" + ISSUE_TIME.Replace(":", "") + "_" + Utility.RemoveNonAlphanumeric(INVOICE_ID) + ".xml";

                if (invoicetype == "Standard")
                {
                    if (invoicetypecode == "388")
                    {
                        returnsignedxmldir = SettingsParams.StandardInvoiceFromZatcaPath;
                    }
                    else if (invoicetypecode == "381")
                    {
                        returnsignedxmldir = SettingsParams.StandardCreditFromZatcaPath;

                    }
                    else if (invoicetypecode == "383")
                    {
                        returnsignedxmldir = SettingsParams.StandardDebitFromZatcaPath;
                    }
                    else
                    {
                        returnsignedxmldir = SettingsParams.StandardInvoiceFromZatcaPath;
                    }

                }
                else
                {
                    if (invoicetypecode == "388")
                    {
                        returnsignedxmldir = SettingsParams.SimplifiedInvoiceFromZatcaPath;
                    }
                    else if (invoicetypecode == "381")
                    {
                        returnsignedxmldir = SettingsParams.SimplifiedCreditFromZatcaPath;

                    }
                    else if (invoicetypecode == "383")
                    {

                        returnsignedxmldir = SettingsParams.SimplifiedDebitFromZatcaPath;
                    }
                    else
                    {
                        returnsignedxmldir = SettingsParams.SimplifiedInvoiceFromZatcaPath;
                    }


                }
               
                xmlfilename= xmlfile;
            }
            catch
            {

            }

        }

    }
}