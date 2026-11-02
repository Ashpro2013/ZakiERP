using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;


namespace ZatcaIntegrationSDK.BLL
{
    public class EInvoiceSigningLogic : IDisposable
    {
        bool IsDisposing = false;
        public Result SignDocument(XmlDocument xmlDoc, string certificateContent, string privateKeyContent)
        {
            Result objResult = new Result();
            objResult.Operation = "Signing E-Invoice Operation";
            objResult.IsValid = false;
            try
            {
                if (string.IsNullOrEmpty(certificateContent))
                {
                    objResult.ErrorMessage = "Invalid certificate content.";
                    return objResult;
                }
                if (string.IsNullOrEmpty(privateKeyContent))
                {
                    objResult.ErrorMessage = "Invalid private key content.";
                    return objResult;
                }

                if (string.IsNullOrEmpty(xmlDoc.InnerText))
                {
                    objResult.ErrorMessage = "Invalid invoice XML content";
                    return objResult;
                }
                objResult.lstSteps = new ResultCollection();
                Result objHashingStepResult = new Result();
                HashingValidator objHashingValidator = new HashingValidator();
                objHashingStepResult = objHashingValidator.GenerateEInvoiceHashing(xmlDoc);
                objHashingStepResult.Operation = "First Step : Generating Hashing";
                if (!objHashingStepResult.IsValid)
                {
                    objResult.lstSteps.Add(objHashingStepResult);
                    return objResult;
                }
                objResult.lstSteps.Add(objHashingStepResult);
                Result objDigitalSignatureStepResult = new Result();
                objDigitalSignatureStepResult = GetDigitalSignature(objHashingStepResult.ResultedValue, privateKeyContent);
                objDigitalSignatureStepResult.Operation = "Second Step : Generating Digital Signature";
                if (!objDigitalSignatureStepResult.IsValid)
                {
                    objResult.lstSteps.Add(objDigitalSignatureStepResult);
                    return objResult;
                }
                objResult.lstSteps.Add(objDigitalSignatureStepResult);
                Result objCertificateStepResult = new Result();
                objCertificateStepResult.IsValid = false;
                objCertificateStepResult.Operation = "Third Step : Generating Certificate";
                sbyte[] certificateBytes = (from x in Encoding.UTF8.GetBytes(certificateContent)
                                            select (sbyte)x).ToArray();
                X509Certificate2 objX509Certificate = new X509Certificate2((byte[])(object)certificateBytes);
                Org.BouncyCastle.X509.X509Certificate newCertificate = DotNetUtilities.FromX509Certificate(objX509Certificate);
                SubjectPublicKeyInfo subjectPublicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(newCertificate.GetPublicKey());
                sbyte[] publicKeySByteArr = (from x in subjectPublicKeyInfo.GetEncoded()
                                             select (sbyte)x).ToArray();
                sbyte[] serialBytes = (from x in objX509Certificate.GetSerialNumber()
                                       select (sbyte)x).ToArray();
                BigInteger serialNumber = new BigInteger((byte[])(object)serialBytes);
                if (objX509Certificate != null)
                {
                    objCertificateStepResult.IsValid = true;
                    objResult.lstSteps.Add(objCertificateStepResult);
                    Result certfHashingStepResult = new Result();
                    certfHashingStepResult.IsValid = false;
                    certfHashingStepResult.Operation = "Forth Step : Generating Certificate Hashing";
                    Result CertificateHashing = new Result();
                    try
                    {
                        certfHashingStepResult.IsValid = true;
                        certfHashingStepResult.ResultedValue = Utility.ToBase64Encode(Utility.Sha256_hashAsString(certificateContent));
                        objResult.lstSteps.Add(certfHashingStepResult);
                    }
                    catch (Exception ex2)
                    {
                        CertificateHashing.ErrorMessage = ex2.Message;
                        objResult.lstSteps.Add(certfHashingStepResult);
                        return objResult;
                    }
                    Result objTransformXMLResult = new Result();
                    objTransformXMLResult.IsValid = false;
                    objTransformXMLResult = TransformXML(xmlDoc.OuterXml);
                    objTransformXMLResult.Operation = "Fifth Step : Transform Xml Result";
                    if (!objTransformXMLResult.IsValid)
                    {
                        objResult.lstSteps.Add(objTransformXMLResult);
                        return objResult;
                    }
                    objResult.lstSteps.Add(objTransformXMLResult);
                    XmlDocument newXmlDoc = new XmlDocument();
                    newXmlDoc.PreserveWhitespace = true;
                    newXmlDoc.LoadXml(objTransformXMLResult.ResultedValue);
                    Dictionary<string, string> nameSpacesMap = getNameSpacesMap();
                    Result objSignedPropertiesHashing = new Result();
                    objSignedPropertiesHashing = PopulateSignedSignatureProperties(newXmlDoc, nameSpacesMap, certfHashingStepResult.ResultedValue, GetCurrentTimestamp(), objX509Certificate.IssuerName.Name, serialNumber.ToString());
                    objSignedPropertiesHashing.Operation = "Sixth Step : Populate Signed Signature Properties";
                    if (!objSignedPropertiesHashing.IsValid)
                    {
                        objResult.lstSteps.Add(objSignedPropertiesHashing);
                        return objResult;
                    }
                    objResult.lstSteps.Add(objSignedPropertiesHashing);
                    Result objPopulateUBLExtensionsResult = new Result();
                    objPopulateUBLExtensionsResult = PopulateUBLExtensions(newXmlDoc, objDigitalSignatureStepResult.ResultedValue, objSignedPropertiesHashing.ResultedValue, objHashingStepResult.ResultedValue, certificateContent);
                    objPopulateUBLExtensionsResult.Operation = "Seventh Step : Populate Populate UBL Extensions";
                    if (!objPopulateUBLExtensionsResult.IsValid)
                    {
                        objResult.lstSteps.Add(objPopulateUBLExtensionsResult);
                        return objResult;
                    }
                    objResult.lstSteps.Add(objPopulateUBLExtensionsResult);
                    Result objPopulateQRResult = new Result();
                    objPopulateQRResult = PopulateQRCode(newXmlDoc, publicKeySByteArr, objDigitalSignatureStepResult.ResultedValue, objHashingStepResult.ResultedValue, (from x in newCertificate.GetSignature()
                                                                                                                                                                         select (sbyte)x).ToArray());
                    objPopulateQRResult.Operation = "Eighth Step : Populate QR";
                    if (!objPopulateQRResult.IsValid)
                    {
                        objResult.lstSteps.Add(objPopulateQRResult);
                        return objResult;
                    }
                    objResult.lstSteps.Add(objPopulateQRResult);
                    objResult.IsValid = true;
                    objResult.ResultedValue = newXmlDoc.OuterXml;
                    foreach (Result currentStep in objResult.lstSteps)
                    {
                        currentStep.ResultedValue = "";
                    }

                    return objResult;
                }
                objCertificateStepResult.ErrorMessage = "Invalid Certificate";
                objResult.lstSteps.Add(objCertificateStepResult);
                return objResult;
            }
            catch (Exception ex)
            {
                objResult.ErrorMessage = ex.Message;
                return objResult;
            }
        }
        private Result PopulateUBLExtensions(XmlDocument xmlDoc, string digitalSignature, string signedPropertiesHashing, string xmlHashing, string certificate)
        {
            Result PopulateUBLExtenResult = new Result();
            try
            {
                Utility.SetNodeValue(xmlDoc, SettingsParams.SIGNATURE_XPATH, digitalSignature);
                Utility.SetNodeValue(xmlDoc, SettingsParams.CERTIFICATE_XPATH, certificate);
                Utility.SetNodeValue(xmlDoc, SettingsParams.SIGNED_Properities_DIGEST_VALUE_XPATH, signedPropertiesHashing);
                Utility.SetNodeValue(xmlDoc, SettingsParams.Hash_XPATH, xmlHashing);
                PopulateUBLExtenResult.IsValid = true;
            }
            catch (Exception ex)
            {
                PopulateUBLExtenResult.IsValid = false;
                PopulateUBLExtenResult.ErrorMessage = ex.Message;
            }
            return PopulateUBLExtenResult;
        }

        private Result PopulateSignedSignatureProperties(XmlDocument document, Dictionary<string, string> nameSpacesMap, string publicKeyHashing, string signatureTimestamp, string x509IssuerName, string serialNumber)
        {
            Result PopulateSignedSignature = new Result();
            try
            {
                Utility.SetNodeValue(document, SettingsParams.PUBLIC_KEY_HASHING_XPATH, publicKeyHashing);
                Utility.SetNodeValue(document, SettingsParams.SIGNING_TIME_XPATH, signatureTimestamp);
                Utility.SetNodeValue(document, SettingsParams.ISSUER_NAME_XPATH, x509IssuerName);
                Utility.SetNodeValue(document, SettingsParams.X509_SERIAL_NUMBER_XPATH, serialNumber);
                string signedSignatureElement = Utility.GetNodeInnerXML(document, SettingsParams.SIGNED_PROPERTIES_XPATH);
                signedSignatureElement = signedSignatureElement.Replace(" />", "/>");
                signedSignatureElement = signedSignatureElement.Replace("></ds:DigestMethod>", "/>");
                string dd = Utility.Sha256_hashAsString(signedSignatureElement.Replace("\r", ""));
                PopulateSignedSignature.ResultedValue = Utility.ToBase64Encode(Utility.Sha256_hashAsString(signedSignatureElement.Replace("\r", "")));
                PopulateSignedSignature.IsValid = true;
            }
            catch (Exception ex)
            {
                PopulateSignedSignature.IsValid = false;
                PopulateSignedSignature.ErrorMessage = ex.Message;
            }
            return PopulateSignedSignature;
        }

        private string GetCurrentTimestamp()
        {
            //return DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", System.Globalization.CultureInfo.GetCultureInfo("en-us", "en"));
            return DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss", System.Globalization.CultureInfo.GetCultureInfo("en-us", "en"));
            //return DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
        }

        private Dictionary<string, string> getNameSpacesMap()
        {
            Dictionary<string, string> nameSpaces = new Dictionary<string, string>();
            nameSpaces.Add("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
            nameSpaces.Add("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            nameSpaces.Add("ext", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");
            nameSpaces.Add("sig", "urn:oasis:names:specification:ubl:schema:xsd:CommonSignatureComponents-2");
            nameSpaces.Add("sac", "urn:oasis:names:specification:ubl:schema:xsd:SignatureAggregateComponents-2");
            nameSpaces.Add("sbc", "urn:oasis:names:specification:ubl:schema:xsd:SignatureBasicComponents-2");
            nameSpaces.Add("ds", "http://www.w3.org/2000/09/xmldsig#");
            nameSpaces.Add("xades", "http://uri.etsi.org/01903/v1.3.2#");
            return nameSpaces;
        }

        public Result GetDigitalSignature(string xmlHashing, string privateKeyContent)
        {
            Result objResult = new Result();
            try
            {
                sbyte[] xmlHashingBytes = (from x in Utility.ToBase64DecodeAsBinary(xmlHashing)
                                           select (sbyte)x).ToArray();
                if (!privateKeyContent.Contains("-----BEGIN EC PRIVATE KEY-----") && !privateKeyContent.Contains("-----END EC PRIVATE KEY-----"))
                {
                    privateKeyContent = "-----BEGIN EC PRIVATE KEY-----\n" + privateKeyContent + "\n-----END EC PRIVATE KEY-----";
                }
                byte[] digitalSignatureBytes;
                using (TextReader text_reader = new StringReader(privateKeyContent))
                {
                    PemReader pemReader = new PemReader(text_reader);
                    object pemObject = pemReader.ReadObject();
                    AsymmetricKeyParameter result = ((AsymmetricCipherKeyPair)pemObject).Private;
                    ISigner signer = SignerUtilities.GetSigner("SHA-256withECDSA");
                    signer.Init(forSigning: true, result);
                    signer.BlockUpdate((byte[])(object)xmlHashingBytes, 0, xmlHashingBytes.Length);
                    digitalSignatureBytes = signer.GenerateSignature();
                }
                objResult.IsValid = true;
                objResult.ResultedValue = Convert.ToBase64String(digitalSignatureBytes);
            }
            catch
            {
                objResult.IsValid = false;
            }
            return objResult;
        }

        public Result TransformXML(string xmlContent)
        {
            string resultXML = "";
            Result objResult = new Result();
            objResult.IsValid = false;
            try
            {
                resultXML = Utility.ApplyXSLTPassingXML(xmlContent, SettingsParams.Embeded_Remove_Elements_PATH);
            }
            catch (Exception)
            {
                objResult.ErrorMessage = "Error in removing elements.";
            }
            try
            {
                resultXML = Utility.ApplyXSLTPassingXML(resultXML, SettingsParams.Embeded_Add_UBL_Element_PATH);
            }
            catch (Exception)
            {
                objResult.ErrorMessage = "Error in adding UBL elements.";
            }
            try
            {
                resultXML = resultXML.Replace("UBL-TO-BE-REPLACED", new StreamReader(Utility.ReadInternalEmbededResourceStream(SettingsParams.Embeded_UBL_File_PATH)).ReadToEnd());
            }
            catch (Exception)
            {
                objResult.ErrorMessage = "Error in replacing UBL elements.";
            }
            try
            {
                resultXML = Utility.ApplyXSLTPassingXML(resultXML, SettingsParams.Embeded_Add_QR_Element_PATH);
            }
            catch (Exception)
            {
                objResult.ErrorMessage = "Error in adding QR elements.";
            }
            try
            {
                resultXML = resultXML.Replace("QR-TO-BE-REPLACED", new StreamReader(Utility.ReadInternalEmbededResourceStream(SettingsParams.Embeded_QR_XML_File_PATH)).ReadToEnd());
            }
            catch (Exception)
            {
                objResult.ErrorMessage = "Error in replacing QR elements.";
            }
            try
            {
                resultXML = Utility.ApplyXSLTPassingXML(resultXML, SettingsParams.Embeded_Add_Signature_Element_PATH);
            }
            catch (Exception)
            {
                objResult.ErrorMessage = "Error in adding signature elements.";
            }
            try
            {
                resultXML = resultXML.Replace("SIGN-TO-BE-REPLACED", new StreamReader(Utility.ReadInternalEmbededResourceStream(SettingsParams.Embeded_Signature_File_PATH)).ReadToEnd());
            }
            catch (Exception)
            {
                objResult.ErrorMessage = "Error in replacing signature elements.";
            }
            if (resultXML != null)
            {
                objResult.ResultedValue = resultXML;
                objResult.IsValid = true;
            }
            return objResult;
        }

        private Result PopulateQRCode(XmlDocument xmlDoc, sbyte[] publicKeyArr, string signature, string hashedXml, sbyte[] certificateSignatureBytes)
        {
            Result objResult = new Result();
            objResult.IsValid = false;
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
            //string QR_CODE = Utility.GetNodeInnerText(xmlDoc, SettingsParams.QR_CODE_XPATH);
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
            //DateTime.TryParseExact(ISSUE_DATE, SettingsParams.allDatesFormats, System.Globalization.CultureInfo.GetCultureInfo("en-us", "en"), DateTimeStyles.None, out issueDateTime);
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

            bool isSimplified = false;
            string invoiceType = Utility.GetInvoiceType(xmlDoc);
            if (invoiceType == "Simplified")
            {
                isSimplified = true;
            }
            QRValidator objQRValidator = new QRValidator();
            string qrCodeValue = objQRValidator.GenerateQrCodeFromValues(SELLER_NAME, VAT_REGISTERATION, issueDateTimeStr, INVOICE_TOTAL, VAT_TOTAL, hashedXml, publicKeyArr, signature, isSimplified, certificateSignatureBytes);
            try
            {
                Utility.SetNodeValue(xmlDoc, SettingsParams.QR_CODE_XPATH, qrCodeValue);
            }
            catch
            {
                objResult.ErrorMessage = "There is no node for QR in XML file.";
                objResult.IsValid = false;
            }
            objResult.IsValid = true;
            return objResult;
        }

        ~EInvoiceSigningLogic()
        {
            Dispose();
        }
        public void Dispose()
        {
            if (!IsDisposing)
            {
                IsDisposing = true;
            }
        }



    }
}