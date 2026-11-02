using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;


namespace ZatcaIntegrationSDK.BLL
{
    public class QRValidator
    {
        public Result GenerateEInvoiceQRCode(XmlDocument xmlDoc)
        {
            Result objResult = new Result();
            objResult.Operation = "Generate Invoice QR";
            objResult.IsValid = false;
            try
            {
                if (string.IsNullOrEmpty(xmlDoc.InnerText))
                {
                    objResult.ErrorMessage = "Invalid invoice XML content";
                    return objResult;
                }
                string SELLER_NAME = Utility.GetNodeInnerText(xmlDoc, SettingsParams.SELLER_NAME_XPATH);
                if (string.IsNullOrEmpty(SELLER_NAME))
                {
                    objResult.ErrorMessage = "Unable to get SELLER_NAME value";
                    return objResult;
                }
                string VAT_REGISTERATION = Utility.GetNodeInnerText(xmlDoc, SettingsParams.VAT_REGISTERATION_XPATH);
                if (string.IsNullOrEmpty(VAT_REGISTERATION))
                {
                    objResult.ErrorMessage = "Unable to get VAT_REGISTERATION value";
                    return objResult;
                }
                string ISSUE_DATE = Utility.GetNodeInnerText(xmlDoc, SettingsParams.ISSUE_DATE_XPATH);
                if (string.IsNullOrEmpty(ISSUE_DATE))
                {
                    objResult.ErrorMessage = "Unable to get ISSUE_DATE value";
                    return objResult;
                }
                string ISSUE_TIME = Utility.GetNodeInnerText(xmlDoc, SettingsParams.ISSUE_TIME_XPATH);
                if (string.IsNullOrEmpty(ISSUE_TIME))
                {
                    objResult.ErrorMessage = "Unable to get ISSUE_TIME value";
                    return objResult;
                }
                string INVOICE_TOTAL = Utility.GetNodeInnerText(xmlDoc, SettingsParams.PayableAmount);
                if (string.IsNullOrEmpty(INVOICE_TOTAL))
                {
                    objResult.ErrorMessage = "Unable to get INVOICE_TOTAL value";
                    return objResult;
                }
                string VAT_TOTAL = Utility.GetNodeInnerText(xmlDoc, SettingsParams.VAT_TOTAL_XPATH);
                if (string.IsNullOrEmpty(VAT_TOTAL))
                {
                    objResult.ErrorMessage = "Unable to get VAT_TOTAL value";
                    return objResult;
                }
                string SIGNATURE = Utility.GetNodeInnerText(xmlDoc, SettingsParams.SIGNATURE_XPATH);
                if (string.IsNullOrEmpty(SIGNATURE))
                {
                    objResult.ErrorMessage = "Unable to get SIGNATURE value";
                    return objResult;
                }
                string CERTIFICATE = Utility.GetNodeInnerText(xmlDoc, SettingsParams.CERTIFICATE_XPATH);
                if (string.IsNullOrEmpty(CERTIFICATE))
                {
                    objResult.ErrorMessage = "Unable to get CERTIFICATE value";
                    return objResult;
                }
                //new
                string issueDateTimeStr = "";
                if (!ISSUE_TIME.EndsWith("Z"))
                {
                    DateTime dateTime = DateTime.ParseExact(string.Concat(ISSUE_DATE, "T", ISSUE_TIME), "yyyy-MM-ddTHH:mm:ss", null);
                    issueDateTimeStr = dateTime.ToString("yyyy-MM-ddTHH:mm:ss");
                }
                else
                {
                    DateTime dateTime1 = TimeZoneInfo.ConvertTimeFromUtc(DateTime.ParseExact(string.Concat(ISSUE_DATE, "T", ISSUE_TIME), "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal), TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time"));
                    issueDateTimeStr = dateTime1.ToString("yyyy-MM-ddTHH:mm:ss");
                }
                //old
                //DateTime issueDateTime = default(DateTime);
                //string issueFullTimeSpan = ISSUE_DATE + " " + ISSUE_TIME;
                //ISSUE_TIME = ISSUE_TIME.Replace("Z", "");
                //DateTime.TryParseExact(ISSUE_DATE, SettingsParams.allDatesFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out issueDateTime);
                //string[] arrTimeParts = ISSUE_TIME.Split(':');
                //int hours = 0;
                //int minutes = 0;
                //int seconds = 0;
                //if (!string.IsNullOrEmpty(arrTimeParts[0]) && int.TryParse(arrTimeParts[0], out hours))
                //{
                //    issueDateTime = issueDateTime.AddHours(hours);
                //}
                //if (arrTimeParts.Length > 1 && !string.IsNullOrEmpty(arrTimeParts[1]) && int.TryParse(arrTimeParts[1], out minutes))
                //{
                //    issueDateTime = issueDateTime.AddMinutes(minutes);
                //}
                //if (arrTimeParts.Length > 2 && !string.IsNullOrEmpty(arrTimeParts[2]) && int.TryParse(arrTimeParts[2], out seconds))
                //{
                //    issueDateTime = issueDateTime.AddSeconds(seconds);
                //}
                //string issueDateTimeStr = issueDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", System.Globalization.CultureInfo.GetCultureInfo("en-us", "en"));

                sbyte[] arrCertificate = (from x in Utility.ToBase64DecodeAsBinary(CERTIFICATE)
                                          select (sbyte)x).ToArray();
                if (arrCertificate != null && arrCertificate.Length == 0)
                {
                    objResult.ErrorMessage = "Invalid CERTIFICATE";
                    return objResult;
                }
                sbyte[] publicKeySByteArr = null;
                Org.BouncyCastle.X509.X509Certificate newCertificate;
                try
                {
                    X509Certificate2 objX509Certificate = new X509Certificate2((byte[])(object)arrCertificate);
                    newCertificate = DotNetUtilities.FromX509Certificate(objX509Certificate);
                    SubjectPublicKeyInfo subjectPublicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(newCertificate.GetPublicKey());
                    publicKeySByteArr = (from x in subjectPublicKeyInfo.GetEncoded()
                                         select (sbyte)x).ToArray();
                }
                catch
                {
                    objResult.ErrorMessage = "Invalid CERTIFICATE";
                    return objResult;
                }
                HashingValidator objHashingValidator = new HashingValidator();
                Result objHashingResult = objHashingValidator.GenerateEInvoiceHashing(xmlDoc);
                if (!objHashingResult.IsValid)
                {
                    objResult.ErrorMessage = "Problem in generating hash step - " + objHashingResult.ErrorMessage;
                    return objResult;
                }
                bool isSimplified = false;
                string invoiceType = Utility.GetInvoiceType(xmlDoc);
                if (invoiceType == "Simplified")
                {
                    isSimplified = true;
                }
                objResult.ResultedValue = GenerateQrCodeFromValues(SELLER_NAME, VAT_REGISTERATION, issueDateTimeStr, INVOICE_TOTAL, VAT_TOTAL, objHashingResult.ResultedValue, publicKeySByteArr, SIGNATURE, isSimplified, (from x in newCertificate.GetSignature()
                                                                                                                                                                                                                            select (sbyte)x).ToArray());
                objResult.IsValid = true;
                return objResult;
            }
            catch (Exception ex)
            {
                objResult.ErrorMessage = ex.Message;
                return objResult;
            }
        }
        public string GenerateQrCodeFromValues(string sellerName, string vatRegistrationNumber, string timeStamp, string invoiceTotal, string vatTotal, string hashedXml, sbyte[] publicKey, string digitalSignature, bool isSimplified, sbyte[] certificateSignature)
        {
            sbyte[] signatureBytes = (from x in Encoding.UTF8.GetBytes(digitalSignature)
                                      select (sbyte)x).ToArray();
            byte[] qrData = Utility.WriteTlv(1u, Encoding.UTF8.GetBytes(sellerName)).ToArray().Concat(Utility.WriteTlv(2u, Encoding.UTF8.GetBytes(vatRegistrationNumber)).ToArray())
                .Concat(Utility.WriteTlv(3u, Encoding.UTF8.GetBytes(timeStamp)).ToArray())
                .Concat(Utility.WriteTlv(4u, Encoding.UTF8.GetBytes(invoiceTotal)).ToArray())
                .Concat(Utility.WriteTlv(5u, Encoding.UTF8.GetBytes(vatTotal)).ToArray())
                .Concat(Utility.WriteTlv(6u, Encoding.UTF8.GetBytes(hashedXml)).ToArray())
                .Concat(Utility.WriteTlv(7u, (byte[])(object)signatureBytes).ToArray())
                .Concat(Utility.WriteTlv(8u, (byte[])(object)publicKey).ToArray())
                .ToArray();
            if (isSimplified)
            {
                qrData = qrData.Concat(Utility.WriteTlv(9u, (byte[])(object)certificateSignature).ToArray()).ToArray();
            }
            return Utility.ToBase64Encode(qrData);
        }
        public Result ValidateEInvoiceQRCode(XmlDocument xmlDoc)
        {
            Result objResult = new Result();
            objResult.Operation = "Validating QR Code";
            objResult.IsValid = false;
            string existQRNodeValue = Utility.GetNodeInnerText(xmlDoc, SettingsParams.QR_CODE_XPATH);
            if (string.IsNullOrEmpty(existQRNodeValue))
            {
                objResult.ErrorMessage = "There is no QR node value in this XML file";
                return objResult;
            }
            Result objQRResult = GenerateEInvoiceQRCode(xmlDoc);
            if (!objQRResult.IsValid)
            {
                objResult.ErrorMessage = objQRResult.ErrorMessage;
                return objResult;
            }
            if (existQRNodeValue != objQRResult.ResultedValue)
            {
                objResult.ErrorMessage = "The generated QR code is different of the one exists in the XML file.";
                return objResult;
            }
            objResult.IsValid = true;
            return objResult;
        }

    }

}