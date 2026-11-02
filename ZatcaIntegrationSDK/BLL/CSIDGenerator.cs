using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using ZatcaIntegrationSDK.APIHelper;
using ZatcaIntegrationSDK.BLL;
using ZatcaIntegrationSDK.HelperContracts;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Microsoft;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.EC;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using System.Collections;
using System.IO;
using System.Xml;
using Org.BouncyCastle.Utilities.IO.Pem;
using System.Xml.Linq;
using Org.BouncyCastle.Asn1.Crmf;
using System.Security.AccessControl;
using Org.BouncyCastle.Crypto.Generators;

namespace ZatcaIntegrationSDK
{
    public class CSIDGenerator
    {
        private bool Simplified { get; set; }
        private bool Standard { get; set; }
        private bool StandardInvoiceSent { get; set; }
        private bool StandardCreditSent { get; set; }
        private bool StandardDebitSent { get; set; }
        private bool SimplifiedInvoiceSent { get; set; }
        private bool SimplifiedCreditSent { get; set; }
        private bool SimplifiedDebitSent { get; set; }
        private Mode mode { get; set; }
        public string lang = "EN";
        public CSIDGenerator(Mode _mode = Mode.developer,string _lang="EN")
        {
            this.mode = _mode;
            this.lang= _lang.ToUpper();
        }
        private bool readytofinish()
        {
            bool show = false;
            if (this.Simplified && this.Standard)
            {
                show = (SimplifiedInvoiceSent && SimplifiedCreditSent && SimplifiedDebitSent && StandardInvoiceSent && StandardCreditSent && StandardDebitSent);
            }
            else if (Simplified)
            {
                show = (SimplifiedInvoiceSent && SimplifiedCreditSent && SimplifiedDebitSent);
            }
            else if (Standard)
            {
                show = (StandardInvoiceSent && StandardCreditSent && StandardDebitSent);
            }
            else
            {
                show = false;
            }
            return show;
        }
        public CertificateResponse GenerateCSID(CertificateRequest certrequest,Invoice inv,string Directorypath)
        {
            CertificateResponse certificateResponse = new CertificateResponse();
            certificateResponse.IsSuccess = false;
            try
            {
                CertificateResponse csrresponse = GenerateCSR(certrequest);
                 if (csrresponse.IsSuccess)
                {
                    certificateResponse.CSR = csrresponse.CSR;
                    certificateResponse.PrivateKey = csrresponse.PrivateKey.Trim().Replace("-----BEGIN EC PRIVATE KEY-----", "").Replace(Environment.NewLine, "").Replace("-----END EC PRIVATE KEY-----", "");
                    if (string.IsNullOrEmpty(certrequest.OTP))
                    {
                        certificateResponse.IsSuccess = false;
                        certificateResponse.ErrorMessage = "You must enter OTP !";
                        return certificateResponse;
                    }
                    if (string.IsNullOrEmpty(csrresponse.CSR))
                    {
                        certificateResponse.IsSuccess = false;
                        certificateResponse.ErrorMessage = "You must generate csr first !";
                        return certificateResponse;
                    }
                    try
                    {
                        ApiRequestLogic apireqlogic = new ApiRequestLogic(mode);
                        ComplianceCsrResponse tokenresponse = new ComplianceCsrResponse();
                        
                        tokenresponse = apireqlogic.GetComplianceCSIDAPI(certrequest.OTP.Trim(), csrresponse.CSR.Trim());
                        if (!string.IsNullOrEmpty(tokenresponse.ErrorMessage))
                        {
                            certificateResponse.IsSuccess = false;
                            certificateResponse.ErrorMessage = tokenresponse.ErrorMessage;
                            return certificateResponse;
                        }
                        else if (mode == Mode.developer)
                        {
                            HandleTokenResponse(tokenresponse, ref certificateResponse);
                            return certificateResponse;
                        }
                        else
                        {
                            // sign and send invoices to zatca
                            string publickey = Utility.Base64Dencode(tokenresponse.BinarySecurityToken);
                            string privatekey = csrresponse.PrivateKey.Trim().Replace("-----BEGIN EC PRIVATE KEY-----", "").Replace(Environment.NewLine, "").Replace("-----END EC PRIVATE KEY-----", "");
                            string secretkey = tokenresponse.Secret;
                            this.Simplified = IsSignSimplifiedInvoices(certrequest.InvoiceType);
                            this.Standard = IsSignStandardInvoices(certrequest.InvoiceType);
                            string errormessage = "";
                            if (this.Simplified)
                            {
                                SimplifiedInvoiceSent = GenerateAndSendDocument(inv,"0200000",388,publickey, privatekey, secretkey, "SimplifiedInvoice", Directorypath, ref errormessage);
                                SimplifiedCreditSent = GenerateAndSendDocument(inv, "0200000", 381, publickey, privatekey, secretkey, "SimplifiedCredit", Directorypath, ref errormessage);
                                SimplifiedDebitSent = GenerateAndSendDocument(inv, "0200000", 383, publickey, privatekey, secretkey, "SimplifiedDebit", Directorypath, ref errormessage);
                            }
                            if (this.Standard)
                            {
                                StandardInvoiceSent = GenerateAndSendDocument(inv, "0100000", 388, publickey, privatekey, secretkey, "StandardInvoice", Directorypath, ref errormessage);
                                StandardCreditSent = GenerateAndSendDocument(inv, "0100000", 381, publickey, privatekey, secretkey, "StandardCredit", Directorypath, ref errormessage);
                                StandardDebitSent = GenerateAndSendDocument(inv, "0100000", 383, publickey, privatekey, secretkey, "StandardDebit", Directorypath, ref errormessage);
                            }
                            bool finish = readytofinish();
                            if (finish)
                            {
                                tokenresponse = apireqlogic.GetProductionCSIDAPI(tokenresponse.RequestId, tokenresponse.BinarySecurityToken, tokenresponse.Secret);
                                if (!string.IsNullOrEmpty(tokenresponse.ErrorMessage))
                                {
                                    certificateResponse.IsSuccess = false;
                                    certificateResponse.ErrorMessage = tokenresponse.ErrorMessage;
                                    return certificateResponse;
                                }
                                else
                                {
                                    HandleTokenResponse(tokenresponse,ref certificateResponse);
                                    return certificateResponse;
                                }
                            }
                            else
                            {
                                certificateResponse.IsSuccess = false;
                                certificateResponse.ErrorMessage = errormessage;
                                return certificateResponse;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        certificateResponse.IsSuccess = false;
                        certificateResponse.ErrorMessage = ex.Message + "\n" + ex.ToString() + "\n" + ex.InnerException.ToString();
                        return certificateResponse;
                    }
                }
                else
                {
                    certificateResponse.IsSuccess = false;
                    certificateResponse.ErrorMessage = csrresponse.ErrorMessage;
                    return certificateResponse;
                }
            } 
            catch(Exception ex)
            {
                certificateResponse.IsSuccess = false;
                certificateResponse.ErrorMessage += "حدث خطأ" + "\n " + ex.InnerException.ToString();
                return certificateResponse;
            }
           
        }
        public CertificateResponse GenerateCSID(CertificateRequest certrequest,string csr,string key, Invoice inv, string Directorypath)
        {
            CertificateResponse certificateResponse = new CertificateResponse();
            certificateResponse.IsSuccess = false;
            try
            {
                CertificateResponse csrresponse = GenerateCSR(certrequest);
                if (csrresponse.IsSuccess)
                {
                    csrresponse.CSR = csr;
                    csrresponse.PrivateKey = key;
                    certificateResponse.CSR = csrresponse.CSR;
                    certificateResponse.PrivateKey = csrresponse.PrivateKey.Trim().Replace("-----BEGIN EC PRIVATE KEY-----", "").Replace(Environment.NewLine, "").Replace("-----END EC PRIVATE KEY-----", ""); ;
                    if (string.IsNullOrEmpty(certrequest.OTP))
                    {
                        certificateResponse.IsSuccess = false;
                        certificateResponse.ErrorMessage = "You must enter OTP !";
                        return certificateResponse;
                    }
                    if (string.IsNullOrEmpty(csrresponse.CSR))
                    {
                        certificateResponse.IsSuccess = false;
                        certificateResponse.ErrorMessage = "You must generate csr first !";
                        return certificateResponse;
                    }
                    try
                    {
                        ApiRequestLogic apireqlogic = new ApiRequestLogic(mode);
                        ComplianceCsrResponse tokenresponse = new ComplianceCsrResponse();

                        tokenresponse = apireqlogic.GetComplianceCSIDAPI(certrequest.OTP.Trim(), csrresponse.CSR.Trim());
                        if (!string.IsNullOrEmpty(tokenresponse.ErrorMessage))
                        {
                            certificateResponse.IsSuccess = false;
                            certificateResponse.ErrorMessage = tokenresponse.ErrorMessage;
                            return certificateResponse;
                        }
                        else if (mode == Mode.developer)
                        {
                            HandleTokenResponse(tokenresponse, ref certificateResponse);
                            return certificateResponse;
                        }
                        else
                        {
                            // sign and send invoices to zatca
                            string publickey = Utility.Base64Dencode(tokenresponse.BinarySecurityToken);
                            string privatekey = csrresponse.PrivateKey.Trim().Replace("-----BEGIN EC PRIVATE KEY-----", "").Replace(Environment.NewLine, "").Replace("-----END EC PRIVATE KEY-----", "");
                            string secretkey = tokenresponse.Secret;
                            this.Simplified = IsSignSimplifiedInvoices(certrequest.InvoiceType);
                            this.Standard = IsSignStandardInvoices(certrequest.InvoiceType);
                            string errormessage = "";
                            if (this.Simplified)
                            {
                                SimplifiedInvoiceSent = GenerateAndSendDocument(inv, "0200000", 388, publickey, privatekey, secretkey, "SimplifiedInvoice", Directorypath, ref errormessage);
                                SimplifiedCreditSent = GenerateAndSendDocument(inv, "0200000", 381, publickey, privatekey, secretkey, "SimplifiedCredit", Directorypath, ref errormessage);
                                SimplifiedDebitSent = GenerateAndSendDocument(inv, "0200000", 383, publickey, privatekey, secretkey, "SimplifiedDebit", Directorypath, ref errormessage);
                            }
                            if (this.Standard)
                            {
                                StandardInvoiceSent = GenerateAndSendDocument(inv, "0100000", 388, publickey, privatekey, secretkey, "StandardInvoice", Directorypath, ref errormessage);
                                StandardCreditSent = GenerateAndSendDocument(inv, "0100000", 381, publickey, privatekey, secretkey, "StandardCredit", Directorypath, ref errormessage);
                                StandardDebitSent = GenerateAndSendDocument(inv, "0100000", 383, publickey, privatekey, secretkey, "StandardDebit", Directorypath, ref errormessage);
                            }
                            bool finish = readytofinish();
                            if (finish)
                            {
                                tokenresponse = apireqlogic.GetProductionCSIDAPI(tokenresponse.RequestId, tokenresponse.BinarySecurityToken, tokenresponse.Secret);
                                if (!string.IsNullOrEmpty(tokenresponse.ErrorMessage))
                                {
                                    certificateResponse.IsSuccess = false;
                                    certificateResponse.ErrorMessage = tokenresponse.ErrorMessage;
                                    return certificateResponse;
                                }
                                else
                                {
                                    HandleTokenResponse(tokenresponse, ref certificateResponse);
                                    return certificateResponse;
                                }
                            }
                            else
                            {
                                certificateResponse.IsSuccess = false;
                                certificateResponse.ErrorMessage = errormessage;
                                return certificateResponse;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        certificateResponse.IsSuccess = false;
                        certificateResponse.ErrorMessage = ex.InnerException.ToString();
                        return certificateResponse;
                    }
                }
                else
                {
                    certificateResponse.IsSuccess = false;
                    certificateResponse.ErrorMessage = csrresponse.ErrorMessage;
                    return certificateResponse;
                }
            }
            catch
            {
                certificateResponse.IsSuccess = false;
                return certificateResponse;
            }

        }
        //public CertificateResponse GenerateCSR(CertificateRequest certrequest)
        //{
        //    CertificateResponse certresponse = new CertificateResponse();

        //    string errormessage = "";
        //    List<string> errormessages = new List<string>();
        //    try
        //    {
        //        if (validateCsrInputs(certrequest, ref errormessages))
        //        {
        //            AsymmetricCipherKeyPair pair;
        //            Pkcs10CertificationRequest csr;
        //            var subjectAttributes = new Dictionary<DerObjectIdentifier, string> {
        //            { X509Name.CN, certrequest.CommonName},
        //            { X509Name.O, certrequest.OrganizationName},
        //            { X509Name.OU, certrequest.OrganizationUnitName},
        //            { X509Name.C, certrequest.CountryName}};

        //            var subject = new X509Name(new ArrayList(subjectAttributes.Keys.Reverse().ToList()), subjectAttributes);

        //            DerObjectIdentifier regAddress = new DerObjectIdentifier("2.5.4.26");
        //            var subjectAlternativeNameAttributes = new Dictionary<DerObjectIdentifier, string> {
        //                                                        {X509Name.Surname, certrequest.SerialNumber},
        //                                                        {X509Name.UID, certrequest.OrganizationIdentifier},
        //                                                        {X509Name.T, certrequest.InvoiceType},
        //                                                        {regAddress, certrequest.Location},
        //                                                        {X509Name.BusinessCategory, certrequest.BusinessCategory}
        //            };

        //            var subjectAltNames = new X509Name(new ArrayList(subjectAlternativeNameAttributes.Keys), subjectAlternativeNameAttributes);
        //            var generalNames = new GeneralNames(new[] { new GeneralName(subjectAltNames) });
        //            // get template name
        //            string certificateTemplateName = "";
        //            if (mode == Mode.Simulation)
        //            {
        //                certificateTemplateName = "PREZATCA-Code-Signing";
        //            }
        //            else if (mode == Mode.Production)
        //            {
        //                certificateTemplateName = "ZATCA-Code-Signing";
        //            }
        //            else
        //            {
        //                certificateTemplateName = "TSTZATCA-Code-Signing";
        //            }

        //            var extensionsGenerator = new X509ExtensionsGenerator();
        //           // extensionsGenerator.AddExtension(MicrosoftObjectIdentifiers.MicrosoftCertTemplateV1, false, (Asn1OctetString)new DerOctetString((Asn1Encodable)new DisplayText(2, certificateTemplateName)));

        //             extensionsGenerator.AddExtension(MicrosoftObjectIdentifiers.MicrosoftCertTemplateV1, false, (Asn1Encodable)new DisplayText(2, certificateTemplateName));

        //            extensionsGenerator.AddExtension(X509Extensions.SubjectAlternativeName, false, (Asn1Encodable)generalNames);
        //            var extensions = extensionsGenerator.Generate();

        //            var gen = new ECKeyPairGenerator("ECDSA");
        //            DerObjectIdentifier oid;
        //            oid = SecObjectIdentifiers.SecP256k1;
        //            X9ECParameters ecp = CustomNamedCurves.GetByOid(oid);

        //            gen.Init(new ECKeyGenerationParameters(new ECDomainParameters(ecp.Curve, ecp.G, ecp.N, ecp.H, ecp.GetSeed()), new SecureRandom()));
        //            pair = gen.GenerateKeyPair();
        //            csr = new Pkcs10CertificationRequest("SHA256withECDSA", subject, pair.Public, new DerSet(new AttributePkcs(PkcsObjectIdentifiers.Pkcs9AtExtensionRequest, new DerSet(extensions))), pair.Private);
        //            var csrPem = new StringBuilder();
        //            var csrPemWriter = new Org.BouncyCastle.OpenSsl.PemWriter(new StringWriter(csrPem));
        //            csrPemWriter.WriteObject(csr);
        //            csrPemWriter.Writer.Flush();
        //            certresponse.CSR = csrPem.ToString();
        //            var privateKeyPem = new StringBuilder();
        //            var privateKeyPemWriter = new Org.BouncyCastle.OpenSsl.PemWriter(new StringWriter(privateKeyPem));
        //            privateKeyPemWriter.WriteObject(pair.Private);
        //            csrPemWriter.Writer.Flush();
        //            certresponse.PrivateKey = privateKeyPem.ToString();
        //            certresponse.IsSuccess = true;
        //            return certresponse;
        //        }
        //        else
        //        {
        //            certresponse.ErrorMessage = string.Join("\n", errormessages);
        //            certresponse.IsSuccess = false;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        certresponse.ErrorMessage="حدث خطأ" +"\n "+ex.InnerException.ToString();
        //        certresponse.IsSuccess = false;
        //    }
        //    return certresponse;
        //}
        public CertificateResponse GenerateCSR(CertificateRequest certrequest)
        {
            CertificateResponse certresponse = new CertificateResponse();

            List<string> errormessages = new List<string>();
            try
            {
                if (validateCsrInputs(certrequest, ref errormessages))
                {
                    AsymmetricCipherKeyPair pair;
                    Pkcs10CertificationRequest csr;
                    var subjectAttributes = new Dictionary<DerObjectIdentifier, string> {
                    { X509Name.CN, certrequest.CommonName},
                    { X509Name.O, certrequest.OrganizationName},
                    { X509Name.OU, certrequest.OrganizationUnitName},
                    { X509Name.C, certrequest.CountryName}};

                    var subject = new X509Name(new ArrayList(subjectAttributes.Keys.Reverse().ToList()), subjectAttributes);

                    DerObjectIdentifier regAddress = new DerObjectIdentifier("2.5.4.26");
                    var subjectAlternativeNameAttributes = new Dictionary<DerObjectIdentifier, string> {
                                                                {X509Name.Surname, certrequest.SerialNumber},
                                                                {X509Name.UID, certrequest.OrganizationIdentifier},
                                                                {X509Name.T, certrequest.InvoiceType},
                                                                {regAddress, certrequest.Location},
                                                                {X509Name.BusinessCategory, certrequest.BusinessCategory}
                    };

                    var subjectAltNames = new X509Name(new ArrayList(subjectAlternativeNameAttributes.Keys), subjectAlternativeNameAttributes);
                    var generalNames = new GeneralNames(new[] { new GeneralName(subjectAltNames) });
                    // get template name
                    string certificateTemplateName = "";
                    if (mode == Mode.Simulation)
                    {
                        certificateTemplateName = "PREZATCA-Code-Signing";
                    }
                    else if (mode == Mode.Production)
                    {
                        certificateTemplateName = "ZATCA-Code-Signing";
                    }
                    else
                    {
                        certificateTemplateName = "TSTZATCA-Code-Signing";
                    }

                    var extensionsGenerator = new X509ExtensionsGenerator();
                    extensionsGenerator.AddExtension(MicrosoftObjectIdentifiers.MicrosoftCertTemplateV1, false, (Asn1Encodable)new DisplayText(2, certificateTemplateName));

                    extensionsGenerator.AddExtension(X509Extensions.SubjectAlternativeName, false, (Asn1Encodable)generalNames);
                    var extensions = extensionsGenerator.Generate();

                    var gen = new ECKeyPairGenerator();
                    gen.Init(new ECKeyGenerationParameters(SecObjectIdentifiers.SecP256k1, new SecureRandom()));
                    pair = gen.GenerateKeyPair();
                    csr = new Pkcs10CertificationRequest("SHA256withECDSA", subject, pair.Public, new DerSet(new AttributePkcs(PkcsObjectIdentifiers.Pkcs9AtExtensionRequest, new DerSet(extensions))), pair.Private);
                    var csrPem = new StringBuilder();
                    var csrPemWriter = new Org.BouncyCastle.OpenSsl.PemWriter(new StringWriter(csrPem));
                    csrPemWriter.WriteObject(csr);
                    csrPemWriter.Writer.Flush();
                    certresponse.CSR = csrPem.ToString();
                    var privateKeyPem = new StringBuilder();
                    var privateKeyPemWriter = new Org.BouncyCastle.OpenSsl.PemWriter(new StringWriter(privateKeyPem));
                    privateKeyPemWriter.WriteObject(pair.Private);
                    csrPemWriter.Writer.Flush();
                    certresponse.PrivateKey = privateKeyPem.ToString();
                    certresponse.IsSuccess = true;
                    return certresponse;
                }
                else
                {
                    certresponse.ErrorMessage = string.Join("\n", errormessages);
                    certresponse.IsSuccess = false;
                }
            }
            catch (Exception ex)
            {
                certresponse.ErrorMessage = "حدث خطأ" + "\n " + ex.InnerException.ToString();
                certresponse.IsSuccess = false;
            }
            return certresponse;
        }
        private bool validateCsrInputs(CertificateRequest certrequest, ref List<string> errors)
        {
            
            if (string.IsNullOrEmpty(certrequest.CommonName))
            {
                
                errors.Add("error1:common name is mandatory field");
                
            }

            if (string.IsNullOrEmpty(certrequest.CountryName))
            {
                errors.Add("error11:Country name is a mandatory field.");
            }

            if (certrequest.CountryName.Length > 3 || certrequest.CountryName.Length < 2)
            {
                errors.Add("error12:Invalid country code name, please provide a valid country code name");
            }

            if (string.IsNullOrEmpty(certrequest.BusinessCategory))
            {
                errors.Add("error17:Industry is mandatory filed");
            }

            if (string.IsNullOrEmpty(certrequest.OrganizationUnitName))
            {
                errors.Add("error8:Organization unit name is mandatory field");
            }

            if (string.IsNullOrEmpty(certrequest.OrganizationIdentifier))
            {
                errors.Add("error4:Organization identifier is mandatory field");
            }
            else
            {
                if (certrequest.OrganizationIdentifier.Substring(10, 1) == "1" && certrequest.OrganizationUnitName.Length != 10)
                {
                    errors.Add("error9:Invalid organization unit name, please provide a valid 10 digit of your group ten number");
                }

                if (certrequest.OrganizationIdentifier.Length != 15)
                {
                    errors.Add("error5:Invalid organization identifier, please provide a valid 15 digit of your vat number");
                }

                if (certrequest.OrganizationIdentifier.Substring(0, 1) != "3")
                {
                    errors.Add("error6:Invalid organization identifier, organization identifier should be started with digit 3");
                }

                if (certrequest.OrganizationIdentifier.Substring(certrequest.OrganizationIdentifier.Length - 1, 1) != "3")
                {
                    errors.Add("error7:Invalid organization identifier, organization identifier should be end with digit 3");
                }
            }

            if (string.IsNullOrEmpty(certrequest.InvoiceType))
            {
                errors.Add("error13:Invoice type is mandatory field");
            }

            if (certrequest.InvoiceType.Length != 4)
            {
                errors.Add("error14:Invalid invoice type, please provide a valid invoice type");
            }

            if (!Regex.Match(certrequest.InvoiceType, "^[0-1]{4}$", RegexOptions.IgnoreCase).Success)
            {
                errors.Add("error15:Invalid invoice type, please provide a valid invoice type");
            }

            if (string.IsNullOrEmpty(certrequest.Location))
            {
                errors.Add("error16:Location is mandatory field");
            }

            if (string.IsNullOrEmpty(certrequest.OrganizationName))
            {
                errors.Add("error10:Organization name is mandatory field");
            }

            if (string.IsNullOrEmpty(certrequest.SerialNumber))
            {
                errors.Add("error2:Serial number is a mandatory field.");
            }

            if (!Regex.Match(certrequest.SerialNumber, "1-(.+)\\|2-(.+)\\|3-(.+)", RegexOptions.IgnoreCase).Success)
            {
                errors.Add("error3:Invalid serial number. Serial number should be in the format: 1-...|2-...|3-....");
            }
            if (Regex.Match(certrequest.SerialNumber, "[=]").Success)
            {
                errors.Add("error23:Invalid SerialNumber, The SerialNumber should only contain alphanumeric characters, and special characters ('=') are not allowed");
            }
            if (Regex.Match(certrequest.CommonName, "[!@#$%&*_]").Success)
            {
                errors.Add("error18:Invalid CommonName , The CommonName should only contain alphanumeric characters, and special characters (' ! @ # $ % & * _ ' including the symbol for 'ampersand' and 'less than' ) are not allowed");
            }

            if (Regex.Match(certrequest.OrganizationUnitName, "[!@#$%&*_]").Success)
            {
                errors.Add("error19:Invalid OrganizationUnitName. The OrganizationUnitName should only contain alphanumeric characters, and special characters (' ! @ # $ % & * _ ' including the symbol for 'ampersand' and 'less than' ) are not allowed");
            }

            if (Regex.Match(certrequest.OrganizationName, "[!@#$%&*_]").Success)
            {
                errors.Add("error20:Invalid OrganizationName. The OrganizationName should only contain alphanumeric characters, and special characters (' ! @ # $ % & * _ ' including the symbol for 'ampersand' and 'less than' ) are not allowed");
            }

            if (Regex.Match(certrequest.Location, "[!@#$%&*_]").Success)
            {
                errors.Add("error21:Invalid Location. The Location should only contain alphanumeric characters, and special characters (' ! @ # $ % & * _ ' including the symbol for 'ampersand' and 'less than' ) are not allowed");
            }

            if (Regex.Match(certrequest.BusinessCategory, "[!@#$%&*_]").Success)
            {
                errors.Add("error22:Invalid Industry. The Industry should only contain alphanumeric characters, and special characters (' ! @ # $ % & * _ ' including the symbol for 'ampersand' and 'less than' ) are not allowed");
            }

            if (errors.Count > 0)
            {
                return false;
            }

            return true;
        }
        private void HandleTokenResponse(ComplianceCsrResponse tokenresponse, ref CertificateResponse certificateResponse)
        {
            if (string.IsNullOrEmpty(tokenresponse.ErrorMessage))
            {
                certificateResponse.CSID = Utility.Base64Dencode(tokenresponse.BinarySecurityToken);
                certificateResponse.SecretKey= tokenresponse.Secret;
                certificateResponse.IsSuccess = true;
            }
            else
            {
                certificateResponse.IsSuccess = false;
                certificateResponse.ErrorMessage = tokenresponse.ErrorMessage;
            }

        }
        private bool IsSignSimplifiedInvoices(string InvoiceType)
        {
            return InvoiceType.Substring(1, 1) == "1";

        }
        private bool IsSignStandardInvoices(string InvoiceType)
        {
            return InvoiceType.Substring(0, 1) == "1";

        }
        private bool GenerateAndSendDocument(Invoice inv, string invoicetypename, int invoicetypeid, string publickey, string privatekey, string secretkey, string InvoiceTypeString, string Directorypath, ref string ErrorMessage)
        {
            UBLXML ubl = new UBLXML();
            string xmldocument = "";
            bool res = ubl.GenerateSampleXML(inv, invoicetypename, invoicetypeid, Directorypath, ref xmldocument, ref ErrorMessage);
            if (!res)
            {
                return false;
            }
            try
            {
            string xmlfilepath = Directorypath + "\\" + InvoiceTypeString + ".xml";
            string signedfilepath = Directorypath + "\\" + "signed_" + InvoiceTypeString + ".xml";
            XDocument d = XDocument.Parse(xmldocument, LoadOptions.PreserveWhitespace);
            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            doc.LoadXml("<?xml version='1.0' encoding='UTF-8'?>\r\n" + d.ToString());
            if (!string.IsNullOrEmpty(Directorypath) && Directory.Exists(Directorypath))
            {
                using (var fs = new FileStream(xmlfilepath, FileMode.Create))
                {
                    doc.Save(fs);
                }
            }
            

                EInvoiceSigningLogic logic = new EInvoiceSigningLogic();
                Result result = new Result();
                var doc1 = new XmlDocument();
                result = logic.SignDocument(doc, publickey, privatekey);

                if (result.IsValid)
                {
                    doc1.PreserveWhitespace = true;
                    doc1.LoadXml("<?xml version='1.0' encoding='UTF-8'?>" + result.ResultedValue);
                    if (!string.IsNullOrEmpty(Directorypath) && Directory.Exists(Directorypath))
                    {
                        using (var fs = new FileStream(signedfilepath, FileMode.Create))
                        {
                            doc1.Save(fs);
                        }
                    }
                    result.PIH = Utility.GetNodeInnerText(doc1, SettingsParams.PIH_XPATH);

                    //set the language of error message the default is arabic with code AR and English with code EN
                    EInvoiceValidator vali = new EInvoiceValidator(this.lang);
                    Result validationresult = new Result();
                    validationresult = vali.ValidateEInvoice(doc1, publickey, result.PIH);
                    if (!validationresult.IsValid)
                    {
                        if (!string.IsNullOrEmpty(validationresult.ErrorMessage))
                        {
                            result.ErrorMessage += validationresult.ErrorMessage + "\n";
                        }
                        if (validationresult.lstSteps != null)
                        {
                            foreach (Result r in validationresult.lstSteps)
                            {
                                if (r.IsValid == false)
                                {
                                    result.ErrorMessage += r.ErrorMessage;
                                }
                            }
                        }
                        result.IsValid = false;
                        ErrorMessage = result.ErrorMessage;
                        return false;
                    }
                    else
                    {
                        // send to zatca
                        InvoiceReportingRequest invrequestbody = new InvoiceReportingRequest();
                        invrequestbody.invoice = Utility.ToBase64Encode(doc1.OuterXml);
                        invrequestbody.uuid = Utility.GetNodeInnerText(doc1, SettingsParams.UUID_XPATH);
                        invrequestbody.invoiceHash = Utility.GetNodeInnerText(doc1, SettingsParams.Hash_XPATH);
                        ApiRequestLogic apireqlogic = new ApiRequestLogic(mode);
                        string username = Utility.ToBase64Encode(publickey);
                        ComplianceCsrResponse tokenresponse = new ComplianceCsrResponse();

                        InvoiceReportingResponse invoicereportingmodel = apireqlogic.CallComplianceInvoiceAPI(username, secretkey, invrequestbody);
                        if (invoicereportingmodel.IsSuccess)
                        {
                            return true;
                        }
                        else
                        {
                            string err = "";
                            err = invoicereportingmodel.ErrorMessage;
                            if (invoicereportingmodel.validationResults != null)
                            {
                                if (invoicereportingmodel.validationResults.ErrorMessages != null&& invoicereportingmodel.validationResults.ErrorMessages.Count>0)
                                {
                                    foreach (ErrorModel error in invoicereportingmodel.validationResults.ErrorMessages)
                                {
                                    err += error.Message + "\n";
                                }
                                }
                            }

                            ErrorMessage = err;
                            return false;
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(result.ErrorMessage))
                    {
                        result.ErrorMessage += result.ErrorMessage + "\n";
                    }
                    if (result.lstSteps != null)
                    {
                        foreach (Result r in result.lstSteps)
                        {
                            if (r.IsValid == false)
                            {
                                result.ErrorMessage += r.ErrorMessage;
                            }
                        }
                    }
                    result.IsValid = false;
                    ErrorMessage = result.ErrorMessage;
                    return false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message + "\n" + ex.ToString() + "\n" + ex.InnerException.ToString();
                return false;
            }
            
        }
        public CertificateResponse GenerateRenewalCSID(CertificateRenewalRequest certrequest, Invoice inv, string Directorypath)
        {
            CertificateResponse certificateResponse = new CertificateResponse();
            certificateResponse.IsSuccess = false;
            try
            {
               
                    certificateResponse.CSR = certrequest.OldCSR;
                    certificateResponse.PrivateKey = certrequest.PrivateKey.Trim().Replace("-----BEGIN EC PRIVATE KEY-----", "").Replace(Environment.NewLine, "").Replace("-----END EC PRIVATE KEY-----", ""); ;
                    if (string.IsNullOrEmpty(certrequest.OTP))
                    {
                        certificateResponse.IsSuccess = false;
                        certificateResponse.ErrorMessage = "You must enter OTP !";
                        return certificateResponse;
                    }
                    if (string.IsNullOrEmpty(certrequest.OldCSR))
                    {
                        certificateResponse.IsSuccess = false;
                        certificateResponse.ErrorMessage = "You must write old csr first !";
                        return certificateResponse;
                    }
                    try
                    {
                        ApiRequestLogic apireqlogic = new ApiRequestLogic(mode);
                        ComplianceCsrResponse tokenresponse = new ComplianceCsrResponse();
                    //for test
                    //tokenresponse.BinarySecurityToken = @"TUlJQ0pEQ0NBY3FnQXdJQkFnSUdBWXgyNno5ek1Bb0dDQ3FHU000OUJBTUNNQlV4RXpBUkJnTlZCQU1NQ21WSmJuWnZhV05wYm1jd0hoY05Nak14TWpFM01EZ3pOakV5V2hjTk1qZ3hNakUyTWpFd01EQXdXakJjTVFzd0NRWURWUVFHRXdKVFFURVdNQlFHQTFVRUN3d05VbWw1WVdSb0lFSnlZVzVqYURFTU1Bb0dBMVVFQ2d3RFZGTlVNU2N3SlFZRFZRUUREQjVVVTFRdE1qQTFNREF4TWpBNU5TMHpNREExT0RreU9EUTVNREF3TURNd1ZqQVFCZ2NxaGtqT1BRSUJCZ1VyZ1FRQUNnTkNBQVI1MGFEdEk2ODdKYS85WkhRdmdGM1lhY3lxVmdVaDJHNDFuSkhIN1V6THVRZmhUUG45Q01xODhmc2Y1d2Iya0RRWFVwZGIzcjFUNXg5Z2Erb29takhsbzRIQk1JRytNQXdHQTFVZEV3RUIvd1FDTUFBd2dhMEdBMVVkRVFTQnBUQ0JvcVNCbnpDQm5ERTdNRGtHQTFVRUJBd3lNUzFVVTFSOE1pMVVVMVI4TXkwM04yUTBPRGd4TlMwM056UXpMVFEwTUdVdE9UVmlNUzFsWkdZeE0yTmxaakF3TjJFeEh6QWRCZ29Ka2lhSmsvSXNaQUVCREE4ek1EQTFPRGt5T0RRNU1EQXdNRE14RFRBTEJnTlZCQXdNQkRFeE1EQXhEakFNQmdOVkJCb01CVTFoYTJ0aE1SMHdHd1lEVlFRUERCUk5aV1JwWTJGc0lFeGhZbTl5WVhSdmNtbGxjekFLQmdncWhrak9QUVFEQWdOSUFEQkZBaUExb2UrNzNKWTkzKytPZk4xVUhQOC9oM2U1Umc1ZUp0M1lLUWdPSElQNUVnSWhBUDM5S1VSeUlXckdjYk1leXFldVNPRVFjSXJzZ1AxRVpkS2RBNWJ1M3BSdg==";
                    //tokenresponse.Secret = @"bxRrBRXeTepU5g0e26eKr26dfr8CvWHD/UxOJoG/hwE=";
                    //tokenresponse.RequestId = "1702802177907";
                     tokenresponse = apireqlogic.GetProductionCSIDRenewalAPI(certrequest.OTP, certrequest.OldCSR, Utility.ToBase64Encode(certrequest.OldPublicKey.Trim()), certrequest.OldSecret.Trim());
                        if (!string.IsNullOrEmpty(tokenresponse.ErrorMessage))
                        {
                            certificateResponse.IsSuccess = false;
                            certificateResponse.ErrorMessage = tokenresponse.ErrorMessage;
                            return certificateResponse;
                        }
                        else if (mode == Mode.developer)
                        {
                            HandleTokenResponse(tokenresponse, ref certificateResponse);
                            return certificateResponse;
                        }
                        else
                        {
                            // sign and send invoices to zatca
                            string publickey = Utility.Base64Dencode(tokenresponse.BinarySecurityToken);
                            string privatekey = certrequest.PrivateKey.Trim().Replace("-----BEGIN EC PRIVATE KEY-----", "").Replace(Environment.NewLine, "").Replace("-----END EC PRIVATE KEY-----", "");
                            string secretkey = tokenresponse.Secret;
                            this.Simplified = IsSignSimplifiedInvoices(certrequest.InvoiceType);
                            this.Standard = IsSignStandardInvoices(certrequest.InvoiceType);
                            string errormessage = "";
                            if (this.Simplified)
                            {
                                SimplifiedInvoiceSent = GenerateAndSendDocument(inv, "0200000", 388, publickey, privatekey, secretkey, "SimplifiedInvoice", Directorypath, ref errormessage);
                                SimplifiedCreditSent = GenerateAndSendDocument(inv, "0200000", 381, publickey, privatekey, secretkey, "SimplifiedCredit", Directorypath, ref errormessage);
                                SimplifiedDebitSent = GenerateAndSendDocument(inv, "0200000", 383, publickey, privatekey, secretkey, "SimplifiedDebit", Directorypath, ref errormessage);
                            }
                            if (this.Standard)
                            {
                                StandardInvoiceSent = GenerateAndSendDocument(inv, "0100000", 388, publickey, privatekey, secretkey, "StandardInvoice", Directorypath, ref errormessage);
                                StandardCreditSent = GenerateAndSendDocument(inv, "0100000", 381, publickey, privatekey, secretkey, "StandardCredit", Directorypath, ref errormessage);
                                StandardDebitSent = GenerateAndSendDocument(inv, "0100000", 383, publickey, privatekey, secretkey, "StandardDebit", Directorypath, ref errormessage);
                            }
                            bool finish = readytofinish();
                            if (finish)
                            {
                                tokenresponse = apireqlogic.GetProductionCSIDAPI(tokenresponse.RequestId, tokenresponse.BinarySecurityToken, tokenresponse.Secret);
                                if (!string.IsNullOrEmpty(tokenresponse.ErrorMessage))
                                {
                                    certificateResponse.IsSuccess = false;
                                    certificateResponse.ErrorMessage = tokenresponse.ErrorMessage;
                                    return certificateResponse;
                                }
                                else
                                {
                                    HandleTokenResponse(tokenresponse, ref certificateResponse);
                                    return certificateResponse;
                                }
                            }
                            else
                            {
                                certificateResponse.IsSuccess = false;
                                certificateResponse.ErrorMessage = errormessage;
                                return certificateResponse;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        certificateResponse.IsSuccess = false;
                        certificateResponse.ErrorMessage = ex.InnerException.ToString();
                        return certificateResponse;
                    }
                
            }
            catch
            {
                certificateResponse.IsSuccess = false;
                return certificateResponse;
            }

        }
        public string GetEffectiveDate(string publickey)
        {
            if (!string.IsNullOrEmpty(publickey))
            {
                try
                {
                    sbyte[] certificateBytes = (from x in Encoding.UTF8.GetBytes(publickey)
                                                select (sbyte)x).ToArray();
                    System.Security.Cryptography.X509Certificates.X509Certificate2 cert = new System.Security.Cryptography.X509Certificates.X509Certificate2((byte[])(object)certificateBytes);
                    DateTime dt1 = cert.NotBefore;
                    return dt1.ToString("dd/MM/yyyy");
                    // return cert.GetEffectiveDateString();
                }
                catch
                {
                }
            }
            return "";

        }
        public string GetExpirationDate(string publickey)
        {
            if (!string.IsNullOrEmpty(publickey))
            {
                try
                {
                    sbyte[] certificateBytes = (from x in Encoding.UTF8.GetBytes(publickey)
                                                select (sbyte)x).ToArray();
                    System.Security.Cryptography.X509Certificates.X509Certificate2 cert = new System.Security.Cryptography.X509Certificates.X509Certificate2((byte[])(object)certificateBytes);
                    DateTime dt1 = cert.NotAfter;
                    return dt1.ToString("dd/MM/yyyy");
                    //return cert.GetExpirationDateString();
                }
                catch
                {
                }
            }
            return "";

        }

    }
}
