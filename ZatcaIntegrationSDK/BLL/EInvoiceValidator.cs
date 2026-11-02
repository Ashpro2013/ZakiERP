using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using org.apache.xerces.dom;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using Saxon.Api;
using ZatcaIntegrationSDK.GeneralLogic;

namespace ZatcaIntegrationSDK.BLL
{

    public class EInvoiceValidator : IDisposable
    {
        bool IsDisposing = false;
        Dictionary<string, ZatcaErrorMessage> ZatcaErrorMessages = new Dictionary<string, ZatcaErrorMessage>();
        string Lang = "";
        public EInvoiceValidator(string lang = "EN")
        {
            ZatcaErrorMessages = ErrorMessageHelper.ErrorMessage();
            Lang = lang;
        }
        public Result ValidateEInvoice(XmlDocument xmlDoc, string certificateContent, string pihContent)
        {
            Result objResult = new Result();
            objResult.Operation = "Validating E-Invoice";
            objResult.IsValid = false;
            objResult.lstSteps = new ResultCollection();
            try
            {
                if (string.IsNullOrEmpty(certificateContent))
                {
                    objResult.ErrorMessage = "Invalid certificate content.";
                    return objResult;
                }
                if (string.IsNullOrEmpty(pihContent))
                {
                    objResult.ErrorMessage = "Invalid PIH file content.";
                    return objResult;
                }

                if (string.IsNullOrEmpty(xmlDoc.InnerText))
                {
                    objResult.ErrorMessage = "Invalid invoice XML content";
                    return objResult;
                }
                Result objFirstStepResult = new Result();
                objFirstStepResult.Operation = "First Step : XSD Validation";
                objFirstStepResult.IsValid = ValidateXSD(xmlDoc);
                if (!objFirstStepResult.IsValid)
                {
                    objFirstStepResult.ErrorMessage = "Schema validation failed; XML does not comply with UBL 2.1 standards";
                    objResult.lstSteps.Add(objFirstStepResult);
                    return objResult;
                }
                objResult.lstSteps.Add(objFirstStepResult);
                string errorMessage = "";
                
                errorMessage = "";
                
                string invoiceType = Utility.GetInvoiceType(xmlDoc);
                if (invoiceType == "Simplified")
                {
                    objResult.Operation += " : ( Simplified )";
                    Result objForthStepResult = new Result();
                    QRValidator objQRValidator = new QRValidator();
                    objForthStepResult = objQRValidator.ValidateEInvoiceQRCode(xmlDoc);
                    objForthStepResult.Operation = "Forth Step : QR Validation";
                    if (!objForthStepResult.IsValid)
                    {
                        objResult.lstSteps.Add(objForthStepResult);
                        return objResult;
                    }
                    objResult.lstSteps.Add(objForthStepResult);
                    errorMessage = "";
                    Result objFifthStepResult = new Result();
                    objFifthStepResult.IsValid = ValidateSignature(xmlDoc, ref errorMessage, certificateContent);
                    objFifthStepResult.Operation = "Fifth Step : Signature Validation";
                    if (!objFifthStepResult.IsValid)
                    {
                        objFifthStepResult.ErrorMessage = errorMessage;
                        objResult.lstSteps.Add(objFifthStepResult);
                        return objResult;
                    }
                    objResult.lstSteps.Add(objFifthStepResult);
                }
                else
                {
                    objResult.Operation += " : ( Standard )";
                }
                errorMessage = "";
                Result objSixthStepResult = new Result();
                objSixthStepResult.IsValid = ValidatePIH(xmlDoc, ref errorMessage, pihContent);
                objSixthStepResult.Operation = "Sixth Step : PIH Validation";
                if (!objSixthStepResult.IsValid)
                {
                    objSixthStepResult.ErrorMessage = errorMessage;
                    objResult.lstSteps.Add(objSixthStepResult);
                    return objResult;
                }
                objResult.lstSteps.Add(objSixthStepResult);
                objResult.IsValid = true;
                return objResult;
            }
            catch (Exception ex)
            {
                objResult.ErrorMessage = ex.Message;
                return objResult;
            }
        }
        private bool ValidateXSD(XmlDocument xmlDoc)
        {
            try
            {

                XmlReaderSettings objXmlReaderSettings = new XmlReaderSettings();
                using (XmlReader x1 = XmlReader.Create(Utility.ReadInternalEmbededResourceStream(SettingsParams.Embeded_UBL_XSD_PATH), objXmlReaderSettings))
                {
                    objXmlReaderSettings.Schemas.Add("urn:oasis:names:specification:ubl:schema:xsd:Invoice-2", x1);
                }
                using (XmlReader x7 = XmlReader.Create(Utility.ReadInternalEmbededResourceStream(SettingsParams.Embeded_CommonExtensionComponents_XSD_PATH), objXmlReaderSettings))
                {
                    objXmlReaderSettings.Schemas.Add("urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2", x7);
                }
                using (XmlReader x8 = XmlReader.Create(Utility.ReadInternalEmbededResourceStream(SettingsParams.Embeded_CommonBasicComponents_XSD_PATH), objXmlReaderSettings))
                {
                    objXmlReaderSettings.Schemas.Add("urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2", x8);
                }
                using (XmlReader x9 = XmlReader.Create(Utility.ReadInternalEmbededResourceStream(SettingsParams.Embeded_CommonAggregateComponents_XSD_PATH), objXmlReaderSettings))
                {
                    objXmlReaderSettings.Schemas.Add("urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2", x9);
                }
                using (XmlReader x10 = XmlReader.Create(Utility.ReadInternalEmbededResourceStream(SettingsParams.Embeded_CommonSignatureComponents_XSD_PATH)))
                {
                    objXmlReaderSettings.Schemas.Add("urn:oasis:names:specification:ubl:schema:xsd:CommonSignatureComponents-2", x10);

                }
                using (XmlReader x11 = XmlReader.Create(Utility.ReadInternalEmbededResourceStream(SettingsParams.Embeded_SignatureAggregateComponents_XSD_PATH)))
                {
                    objXmlReaderSettings.Schemas.Add("urn:oasis:names:specification:ubl:schema:xsd:SignatureAggregateComponents-2", x11);
                }
                using (XmlReader x12 = XmlReader.Create(Utility.ReadInternalEmbededResourceStream(SettingsParams.Embeded_SignatureBasicComponents_XSD_PATH)))
                {
                    objXmlReaderSettings.Schemas.Add("urn:oasis:names:specification:ubl:schema:xsd:SignatureBasicComponents-2", x12);
                }
                using (XmlReader x13 = XmlReader.Create(Utility.ReadInternalEmbededResourceStream(SettingsParams.Embeded_UBL_UnqualifiedDataTypes_XSD_PATH)))
                {
                    objXmlReaderSettings.Schemas.Add("urn:oasis:names:specification:ubl:schema:xsd:UnqualifiedDataTypes-2", x13);
                }
                using (XmlReader x14 = XmlReader.Create(Utility.ReadInternalEmbededResourceStream(SettingsParams.Embeded_CoreComponentTypeSchemaModule_XSD_PATH)))
                {
                    objXmlReaderSettings.Schemas.Add("urn:un:unece:uncefact:documentation:2", x14);
                }
                using (XmlReader x2 = XmlReader.Create(Utility.ReadInternalEmbededResourceStream(SettingsParams.Embeded_ExtensionContentDataType_XSD_PATH)))
                {
                    objXmlReaderSettings.Schemas.Add("urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2", x2);
                }
                using (XmlReader x3 = XmlReader.Create(Utility.ReadInternalEmbededResourceStream(SettingsParams.Embeded_QualifiedDataTypes_XSD_PATH)))
                {
                    objXmlReaderSettings.Schemas.Add("urn:oasis:names:specification:ubl:schema:xsd:QualifiedDataTypes-2", x3);

                }
                using (XmlReader x4 = XmlReader.Create(Utility.ReadInternalEmbededResourceStream(SettingsParams.Embeded_CCTS_CCT_SchemaModule_XSD_PATH)))
                {
                    objXmlReaderSettings.Schemas.Add("urn:un:unece:uncefact:data:specification:CoreComponentTypeSchemaModule:2", x4);
                }
                using (XmlReader x5 = XmlReader.Create(Utility.ReadInternalEmbededResourceStream(SettingsParams.Embeded_UBL_XAdESv_XSD_PATH)))
                {
                    objXmlReaderSettings.Schemas.Add("http://uri.etsi.org/01903/v1.4.1#", x5);
                }
                using (XmlReader x6 = XmlReader.Create(Utility.ReadInternalEmbededResourceStream(SettingsParams.Embeded_UBL_Xmldsig_Core_XSD_PATH)))
                {
                    objXmlReaderSettings.Schemas.Add("http://www.w3.org/2000/09/xmldsig#", x6);
                }
                objXmlReaderSettings.DtdProcessing = DtdProcessing.Parse;
                objXmlReaderSettings.ValidationType = ValidationType.DTD;
                objXmlReaderSettings.ValidationEventHandler += DocumentValidationHandler;
                MemoryStream xmlStream = new MemoryStream();
                xmlDoc.Save(xmlStream);

                xmlStream.Flush();//Adjust this if you want read your data 
                xmlStream.Position = 0;

                //Define here your reading
                using (XmlReader books = XmlReader.Create(xmlStream, objXmlReaderSettings))
                {
                    while (books.Read())
                    {
                    }
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        private static void DocumentValidationHandler(object sender, ValidationEventArgs e)
        {
            if (e.Severity == XmlSeverityType.Error)
            {
                throw new Exception(e.Message);
            }
        }
        private SchematronResult ValidateSchematrons(XmlDocument xmlDoc, string xslPath, ref string nodeErrors)
        {
            SchematronResult schematronResult = new SchematronResult();
            List<string> Errors = new List<string>();

            List<string> Warnings = new List<string>();
            try
            {

                Lang = Lang.ToUpper();
                Processor processor = new Processor();
                XsltCompiler compiler = processor.NewXsltCompiler();
                XdmNode input = processor.NewDocumentBuilder().Build(xmlDoc);
                XsltExecutable executable = compiler.Compile(Utility.ReadInternalEmbededResourceStream(xslPath));
                XsltTransformer transformer = executable.Load();
                transformer.InitialContextNode = input;
                XdmDestination chainResult = new XdmDestination();
                MemoryStream xmlStream = new MemoryStream();
                xmlDoc.Save(xmlStream);
                xmlStream.Flush();
                xmlStream.Position = 0L;
                transformer.SetInputStream(xmlStream, new Uri(Path.GetTempPath()));
                transformer.Run(chainResult);
                MemoryStream results = new MemoryStream();
                StreamWriter stream = new StreamWriter(results);
                XdmNode rootnode = chainResult.XdmNode;
                foreach (XdmNode node in rootnode.Children())
                {
                    foreach (XdmNode childNode in node.Children())
                    {
                        if (childNode.NodeName != null && "failed-assert".Equals(childNode.NodeName.LocalName))
                        {
                            string attributeValue = childNode.GetAttributeValue("flag");
                            ZatcaErrorMessage error;
                            ZatcaErrorMessage warning;
                            if (attributeValue == null || attributeValue.ToLower().Trim().Equals("error"))
                            {

                                if (!ZatcaErrorMessages.TryGetValue(childNode.GetAttributeValue("id"), out error))
                                {
                                    if (!Errors.Any(x => x.Contains(childNode.StringValue)))
                                    {
                                        Errors.Add(string.Concat(new string[] { "[", attributeValue, "] CODE: ", childNode.GetAttributeValue("id"), ", MESSAGE: ", childNode.StringValue }));
                                    }
                                }
                                else
                                {
                                    string errormessage = Lang == "AR" ? error.MessageAR : error.MessageEN;
                                    if (!Errors.Any(x => x.Contains(errormessage)))
                                    {
                                        Errors.Add(string.Concat(new string[] { "[", attributeValue, "] CODE: ", childNode.GetAttributeValue("id"), ", MESSAGE: ", errormessage }));
                                    }
                                }

                            }
                            else
                            {
                                if (!ZatcaErrorMessages.TryGetValue(childNode.GetAttributeValue("id"), out warning))
                                {
                                    if (!Warnings.Any(x => x.Contains(childNode.StringValue)))
                                    {
                                        Warnings.Add(string.Concat(new string[] { "[", attributeValue, "] CODE: ", childNode.GetAttributeValue("id"), ", MESSAGE: ", childNode.StringValue }));
                                    }
                                }
                                else
                                {
                                    string warningmessage = Lang == "AR" ? warning.MessageAR : warning.MessageEN;
                                    if (!Warnings.Any(x => x.Contains(warningmessage)))
                                    {
                                        Warnings.Add(string.Concat(new string[] { "[", attributeValue, "] CODE: ", childNode.GetAttributeValue("id"), ", MESSAGE: ", warningmessage }));
                                    }
                                }
                            }

                        }
                    }
                }
                schematronResult.Errors = Errors;
                schematronResult.Warnings = Warnings;
                return schematronResult;

            }
            catch (Exception e)
            {
                Errors.Add(e.GetType().ToString() + " : " + e.Message);
                schematronResult.Errors = Errors;
                return schematronResult;

            }
        }

        private bool ValidatePIH(XmlDocument xmlDoc, ref string errorMessage, string newPIH)
        {
            try
            {
                if (string.IsNullOrEmpty(newPIH))
                {
                    errorMessage = "Empty PIH file.";
                    return false;
                }
                string existPIH = Utility.GetNodeInnerText(xmlDoc, SettingsParams.PIH_XPATH);
                if (string.IsNullOrEmpty(existPIH))
                {
                    errorMessage = "Empty PIH node value.";
                    return false;
                }
                if (newPIH != existPIH)
                {
                    errorMessage = "PIH is inValid";
                    return false;
                }
                return true;
            }
            catch (IOException)
            {
                errorMessage = "PIH file not found.";
                return false;
            }
            catch (Exception)
            {
                errorMessage = "Error Occurred in validating PIH.";
                return false;
            }
        }

        private bool ValidateSignature(XmlDocument xmlDoc, ref string errorMessage, string certificateContent)
        {
            try
            {
                string CERTIFICATE = Utility.GetNodeInnerText(xmlDoc, SettingsParams.CERTIFICATE_XPATH).Trim();
                if (string.IsNullOrEmpty(CERTIFICATE))
                {
                    errorMessage = "Unable to get CERTIFICATE value from E-invoice XML";
                    return false;
                }
                sbyte[] arrCertificate = (from x in Utility.ToBase64DecodeAsBinary(CERTIFICATE)
                                          select (sbyte)x).ToArray();
                if (arrCertificate != null && arrCertificate.Length == 0)
                {
                    errorMessage = "Invalid CERTIFICATE";
                    return false;
                }
                HashingValidator objHashingValidator = new HashingValidator();
                Result objHashingResult = objHashingValidator.GenerateEInvoiceHashing(xmlDoc);
                if (!objHashingResult.IsValid)
                {
                    errorMessage = "Invalid Hashing Generation";
                    return false;
                }
                Result objValidateHashingResult = objHashingValidator.ValidateEInvoiceHashing(xmlDoc);
                if (!objValidateHashingResult.IsValid)
                {
                    errorMessage = objValidateHashingResult.ErrorMessage;
                    return false;
                }
                sbyte[] publicKeySByteArr = null;
                try
                {
                    X509Certificate2 objX509Certificate = new X509Certificate2((byte[])(object)arrCertificate);
                    Org.BouncyCastle.X509.X509Certificate newCertificate = DotNetUtilities.FromX509Certificate(objX509Certificate);
                    SubjectPublicKeyInfo subjectPublicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(newCertificate.GetPublicKey());
                    publicKeySByteArr = (from x in subjectPublicKeyInfo.GetEncoded()
                                         select (sbyte)x).ToArray();
                }
                catch
                {
                    errorMessage = "Invalid CERTIFICATE";
                    return false;
                }
                string existSignature = Utility.GetNodeInnerText(xmlDoc, SettingsParams.SIGNATURE_XPATH);
                if (string.IsNullOrEmpty(existSignature))
                {
                    errorMessage = "Unable to get Signature value in E-invoice XML.";
                    return false;
                }
                sbyte[] xmlHashingBytes = (from x in Utility.ToBase64DecodeAsBinary(objHashingResult.ResultedValue)
                                           select (sbyte)x).ToArray();
                AsymmetricKeyParameter publicKey = PublicKeyFactory.CreateKey((byte[])(object)publicKeySByteArr);
                ISigner signer = SignerUtilities.GetSigner("SHA-256withECDSA");
                signer.Init(forSigning: false, publicKey);
                signer.BlockUpdate((byte[])(object)xmlHashingBytes, 0, xmlHashingBytes.Length);
                sbyte[] sigBytes = (from x in Convert.FromBase64String(existSignature)
                                    select (sbyte)x).ToArray();
                bool result = signer.VerifySignature((byte[])(object)sigBytes);
                if (!result)
                {
                    errorMessage = "Wrong signature value.";
                    return false;
                }
                string SignedPropertiesXML = Utility.GetNodeInnerXML(xmlDoc, SettingsParams.SIGNED_PROPERTIES_XPATH);
                SignedPropertiesXML = SignedPropertiesXML.Replace(" />", "/>");
                SignedPropertiesXML = SignedPropertiesXML.Replace("></ds:DigestMethod>", "/>");
                string signedPropertiesDigestValueExist = Utility.GetNodeInnerText(xmlDoc, SettingsParams.SIGNED_Properities_DIGEST_VALUE_XPATH);
                string signedPropertiesDigestValueCalculated = Utility.ToBase64Encode(Utility.Sha256_hashAsString(SignedPropertiesXML.Replace("\r", "")));
                if (signedPropertiesDigestValueCalculated != signedPropertiesDigestValueExist)
                {
                    errorMessage = "Wrong signing properties digest value.";
                    return false;
                }
                string signingCertificateDigestValueCalculated = Utility.ToBase64Encode(Utility.Sha256_hashAsString(CERTIFICATE));
                string signingCertificateDigestValueExist = Utility.GetNodeInnerText(xmlDoc, SettingsParams.SIGNED_Certificate_DIGEST_VALUE_XPATH);
                if (signingCertificateDigestValueCalculated != signingCertificateDigestValueExist)
                {
                    errorMessage = "Wrong signing certificate digest value.";
                    return false;
                }
                sbyte[] certificateArr = (from x in Encoding.UTF8.GetBytes(certificateContent)
                                          select (sbyte)x).ToArray();
                X509Certificate2 certificate = new X509Certificate2((byte[])(object)certificateArr);
                string certificateSerialNumber = Utility.GetNodeInnerText(xmlDoc, SettingsParams.X509_SERIAL_NUMBER_XPATH);
                sbyte[] serialBytes = (from x in certificate.GetSerialNumber()
                                       select (sbyte)x).ToArray();
                if (certificateSerialNumber != new BigInteger((byte[])(object)serialBytes).ToString())
                {
                    errorMessage = "Invalid certificate serial number.";
                    return false;
                }
                string certificateIssuerName = Utility.GetNodeInnerText(xmlDoc, SettingsParams.ISSUER_NAME_XPATH);
                if (certificateIssuerName != certificate.IssuerName.Name)
                {
                    errorMessage = "Invalid certificate issuer name.";
                    return false;
                }
                return result;
            }
            catch (Exception)
            {
                errorMessage = "Error occurred in validating signature.";
                return false;
            }
        }

        ~EInvoiceValidator()
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