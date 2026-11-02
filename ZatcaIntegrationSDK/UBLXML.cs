using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using System.Xml.Linq;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Globalization;
using Org.BouncyCastle.Asn1.Ocsp;
using java.util.function;

namespace ZatcaIntegrationSDK
{
	[Guid("13FE32AD-4BF8-495f-AB4D-6C61BD463EA4")]
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class UBLXML:IDisposable
	{
		public UBLXML(string _lang = "EN") {
        this.lang= _lang.ToUpper();
        }
		//main function
		bool IsDisposing = false;
        bool SaveXML = false;
        public string lang = "";
        public Result GenerateInvoiceXML(Invoice inv, string Directorypath, bool savexml = true)
		{
            this.SaveXML = savexml;
            Result result = new Result();
			result.IsValid = false;
            if (savexml)
            {
               Utility.CreateInvoicesFolder(Directorypath);
            }
          

            if (!(inv.invoiceTypeCode.id == 381 || inv.invoiceTypeCode.id == 383 || inv.invoiceTypeCode.id == 388 || inv.invoiceTypeCode.id == 386))
			{
				result.ErrorMessage = "Invalid Invoice Type Code ! ";
				return result;
			}
			if (inv.invoiceTypeCode.Name.Length != 7)
			{
				result.ErrorMessage = "Invalid Invoice Type Code Name Example (0100000) For Simplified Invoice!";
				return result;
			}
			string invoicetype = inv.invoiceTypeCode.Name.Substring(0, 2);
			if (inv.invoiceTypeCode.id == 381) //credit note
			{
				if (invoicetype == "01")// standard
				{
					result = GenerateStandardCreditXML(inv, Directorypath);
					return result;
				}
				else if (invoicetype == "02")// simplified
				{
					result = GenerateSimplifiedCreditXML(inv, Directorypath);
					return result;

				}
				else
				{
					result.ErrorMessage = "Invalid Invoice Type Code !";
					return result;
				}
			}
			else if (inv.invoiceTypeCode.id == 383) // debit note
			{
				if (invoicetype == "01")// standard
				{
					result = GenerateStandardDebitXML(inv, Directorypath);
					return result;
				}
				else if (invoicetype == "02")// simplified
				{
					result = GenerateSimplifiedDebitXML(inv, Directorypath);
					return result;
				}
				else
				{
					result.ErrorMessage = "Invalid Invoice Type Code !";
					return result;
				}
			}
			else if (inv.invoiceTypeCode.id == 388) // invoice
			{
				if (invoicetype == "01") // standard
				{
					result = GenerateStandardInvoiceXML(inv, Directorypath);
					return result;
				}
				else if (invoicetype == "02") // simplified
				{
					result = GenerateSimplifiedInvoiceXML(inv, Directorypath);
					//result.ErrorMessage = "Test test test";
					return result;
				}
				else
				{
					result.ErrorMessage = "Invalid Invoice Type Code !";
					return result;
				}
			}
			else if (inv.invoiceTypeCode.id == 386) // Prepaid invoice
			{
				if (invoicetype == "01") // standard
				{
					result = GenerateStandardInvoiceXML(inv, Directorypath);
					return result;
				}
				else if (invoicetype == "02") // simplified
				{
					result = GenerateSimplifiedInvoiceXML(inv, Directorypath);
					//result.ErrorMessage = "Test test test";
					return result;
				}
				else
				{
					result.ErrorMessage = "Invalid Invoice Type Code !";
					return result;
				}
			}
			else
			{
				result.ErrorMessage = "Invalid Invoice Type Code !";
				return result;
			}

		}
		private Result GenerateStandardInvoiceXML(Invoice inv, string Directorypath)
		{
			Result result = new Result();
			StringBuilder sb = new StringBuilder();
			string error = "";
			GetCommonInvoiceTagElements(inv, sb);
			GetICVElement(inv.AdditionalDocumentReferenceICV.UUID, sb, ref error);
			GetPIHElement(Directorypath, inv, sb, ref error);
			GetAccountingSupplierPartyElement(inv.SupplierParty, sb);
			GetAccountingCustomerPartyElement(inv, sb, ref error);
			GetDeliveryElement(inv, sb, ref error);
			GetPaymentMeansElement(inv, sb, ref error);
			//Calculate invoicelines
			CalculateInvoiceLine(inv, ref error);
			GetallowanceChargeElement(inv, sb);
			GetDocumentTaxTotal(inv, sb, ref error);
			GetLegalMonetaryTotal(inv, sb, ref error);
			GetInvoiceLineElement(inv, sb);
			sb.Append("</Invoice>");
			if (!string.IsNullOrEmpty(error))
			{
				result.ErrorMessage = error;
				result.IsValid = false;
				return result;
			}
			string returnnormalxmldir = SettingsParams.NormalStandardInvoicePath;
			string returnsignedxmldir = SettingsParams.StandardInvoicePath;
			SignDocument(Directorypath, sb.ToString(), returnsignedxmldir, returnnormalxmldir, inv, ref result);
			return result;
		}
		private Result GenerateSimplifiedInvoiceXML(Invoice inv, string Directorypath)
		{
			Result result = new Result();
			StringBuilder sb = new StringBuilder();
			string error = "";

			GetCommonInvoiceTagElements(inv, sb);
			GetICVElement(inv.AdditionalDocumentReferenceICV.UUID, sb, ref error);
			GetPIHElement(Directorypath, inv, sb, ref error);
			GetAccountingSupplierPartyElement(inv.SupplierParty, sb);
			GetAccountingCustomerPartyElement(inv, sb, ref error);
			GetDeliveryElement(inv, sb, ref error);
			GetPaymentMeansElement(inv, sb, ref error);
			//Calculate invoicelines
			CalculateInvoiceLine(inv, ref error);
			GetallowanceChargeElement(inv, sb);
			GetDocumentTaxTotal(inv, sb, ref error);
			GetLegalMonetaryTotal(inv, sb, ref error);
			GetInvoiceLineElement(inv, sb);
			sb.Append("</Invoice>");

			if (!string.IsNullOrEmpty(error))
			{
				result.ErrorMessage = error;
				result.IsValid = false;
				return result;
			}

			string returnnormalxmldir = SettingsParams.NormalSimplifiedInvoicePath;
			string returnsignedxmldir = SettingsParams.SimplifiedInvoicePath;
			SignDocument(Directorypath, sb.ToString(), returnsignedxmldir, returnnormalxmldir, inv, ref result);

			return result;
		}
		private Result GenerateStandardCreditXML(Invoice inv, string Directorypath)
		{
			Result result = new Result();
			StringBuilder sb = new StringBuilder();
			string error = "";
			GetCommonInvoiceTagElements(inv, sb);
			GetInvoiceDocumentReferenceElement(inv.billingReference.invoiceDocumentReferences, sb);
			GetICVElement(inv.AdditionalDocumentReferenceICV.UUID, sb, ref error);
			GetPIHElement(Directorypath, inv, sb, ref error);
			GetAccountingSupplierPartyElement(inv.SupplierParty, sb);
			GetAccountingCustomerPartyElement(inv, sb, ref error);
			GetDeliveryElement(inv, sb, ref error);
			GetPaymentMeansElement(inv, sb, ref error);
			//Calculate invoicelines
			CalculateInvoiceLine(inv, ref error);
			GetallowanceChargeElement(inv, sb);
			GetDocumentTaxTotal(inv, sb, ref error);
			GetLegalMonetaryTotal(inv, sb, ref error);
			GetInvoiceLineElement(inv, sb);
			sb.Append("</Invoice>");

			if (!string.IsNullOrEmpty(error))
			{
				result.ErrorMessage = error;
				result.IsValid = false;
				return result;
			}

			string returnnormalxmldir = SettingsParams.NormalStandardCreditPath;
			string returnsignedxmldir = SettingsParams.StandardCreditPath;
			SignDocument(Directorypath, sb.ToString(), returnsignedxmldir, returnnormalxmldir, inv, ref result);
			return result;
		}
		private Result GenerateSimplifiedCreditXML(Invoice inv, string Directorypath)
		{
			Result result = new Result();
			StringBuilder sb = new StringBuilder();
			string error = "";
			GetCommonInvoiceTagElements(inv, sb);
			GetInvoiceDocumentReferenceElement(inv.billingReference.invoiceDocumentReferences, sb);
			GetICVElement(inv.AdditionalDocumentReferenceICV.UUID, sb, ref error);
			GetPIHElement(Directorypath, inv, sb, ref error);
			GetAccountingSupplierPartyElement(inv.SupplierParty, sb);
			GetAccountingCustomerPartyElement(inv, sb, ref error);
			GetDeliveryElement(inv, sb, ref error);
			GetPaymentMeansElement(inv, sb, ref error);
			//Calculate invoicelines
			CalculateInvoiceLine(inv, ref error);
			GetallowanceChargeElement(inv, sb);
			GetDocumentTaxTotal(inv, sb, ref error);
			GetLegalMonetaryTotal(inv, sb, ref error);
			GetInvoiceLineElement(inv, sb);
			sb.Append("</Invoice>");
			if (!string.IsNullOrEmpty(error))
			{
				result.ErrorMessage = error;
				result.IsValid = false;
				return result;
			}

			string returnnormalxmldir = SettingsParams.NormalSimplifiedCreditPath;
			string returnsignedxmldir = SettingsParams.SimplifiedCreditPath;
			SignDocument(Directorypath, sb.ToString(), returnsignedxmldir, returnnormalxmldir, inv, ref result);
			return result;
		}
		private Result GenerateStandardDebitXML(Invoice inv, string Directorypath)
		{
			Result result = new Result();
			StringBuilder sb = new StringBuilder();
			string error = "";
			GetCommonInvoiceTagElements(inv, sb);
			GetInvoiceDocumentReferenceElement(inv.billingReference.invoiceDocumentReferences, sb);
			GetICVElement(inv.AdditionalDocumentReferenceICV.UUID, sb, ref error);
			GetPIHElement(Directorypath, inv, sb, ref error);
			GetAccountingSupplierPartyElement(inv.SupplierParty, sb);
			GetAccountingCustomerPartyElement(inv, sb, ref error);
			GetDeliveryElement(inv, sb, ref error);
			GetPaymentMeansElement(inv, sb, ref error);
			//Calculate invoicelines
			CalculateInvoiceLine(inv, ref error);
			GetallowanceChargeElement(inv, sb);
			GetDocumentTaxTotal(inv, sb, ref error);
			GetLegalMonetaryTotal(inv, sb, ref error);
			GetInvoiceLineElement(inv, sb);
			sb.Append("</Invoice>");

			if (!string.IsNullOrEmpty(error))
			{
				result.ErrorMessage = error;
				result.IsValid = false;
				return result;
			}

			string returnnormalxmldir = SettingsParams.NormalStandardDebitPath;
			string returnsignedxmldir = SettingsParams.StandardDebitPath;
			SignDocument(Directorypath, sb.ToString(), returnsignedxmldir, returnnormalxmldir, inv, ref result);
			return result;
		}
		private Result GenerateSimplifiedDebitXML(Invoice inv, string Directorypath)
		{
			Result result = new Result();
			StringBuilder sb = new StringBuilder();
			string error = "";
			GetCommonInvoiceTagElements(inv, sb);
			GetInvoiceDocumentReferenceElement(inv.billingReference.invoiceDocumentReferences, sb);
			GetICVElement(inv.AdditionalDocumentReferenceICV.UUID, sb, ref error);
			GetPIHElement(Directorypath, inv, sb, ref error);
			GetAccountingSupplierPartyElement(inv.SupplierParty, sb);
			GetAccountingCustomerPartyElement(inv, sb, ref error);
			GetDeliveryElement(inv, sb, ref error);
			GetPaymentMeansElement(inv, sb, ref error);
			//Calculate invoicelines
			CalculateInvoiceLine(inv, ref error);
			GetallowanceChargeElement(inv, sb);
			GetDocumentTaxTotal(inv, sb, ref error);
			GetLegalMonetaryTotal(inv, sb, ref error);
			GetInvoiceLineElement(inv, sb);
			sb.Append("</Invoice>");

			if (!string.IsNullOrEmpty(error))
			{
				result.ErrorMessage = error;
				result.IsValid = false;
				return result;
			}

			string returnnormalxmldir = SettingsParams.NormalSimplifiedDebitPath;
			string returnsignedxmldir = SettingsParams.SimplifiedDebitPath;

			SignDocument(Directorypath, sb.ToString(), returnsignedxmldir, returnnormalxmldir, inv, ref result);

			return result;
		}
        public bool GenerateSampleXML(Invoice inv, string invoicetypename, int invoicetypeid, string Directorypath, ref string xmldoc, ref string ErrorMessage)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                string InvoiceTypeString = "";
                inv.UUID = Guid.NewGuid().ToString();
                inv.invoiceTypeCode.id = invoicetypeid;
                inv.invoiceTypeCode.Name = invoicetypename;
                inv.legalMonetaryTotal.PrepaidAmount = 0;
                inv.legalMonetaryTotal.PayableAmount = 0;
                string invoicetype = inv.invoiceTypeCode.Name.Substring(0, 2);
                if (inv.invoiceTypeCode.id == 381) //credit note
                {
                    if (invoicetype == "01")// standard
                    {
                        InvoiceTypeString = "StandardCredit";
                    }
                    else
                    {
                        InvoiceTypeString = "SimplifiedCredit";
                    }

                }
                else if (inv.invoiceTypeCode.id == 383) // debit note
                {
                    if (invoicetype == "01")// standard
                    {
                        InvoiceTypeString = "StandardDebit";
                    }
                    else
                    {
                        InvoiceTypeString = "SimplifiedDebit";
                    }
                }
                else if (inv.invoiceTypeCode.id == 388) // invoice
                {
                    if (invoicetype == "01")// standard
                    {
                        InvoiceTypeString = "StandardInvoice";
                    }
                    else
                    {
                        InvoiceTypeString = "SimplifiedInvoice";
                    }
                }
                GetCommonInvoiceTagElements(inv, sb);
                if (inv.invoiceTypeCode.id == 383 || inv.invoiceTypeCode.id == 381)
                {
                    GetInvoiceDocumentReferenceElement(inv.billingReference.invoiceDocumentReferences, sb);
                }
                GetICVElement(inv.AdditionalDocumentReferenceICV.UUID, sb, ref ErrorMessage);
                GetPIHElement(Directorypath, inv, sb, ref ErrorMessage);
                GetAccountingSupplierPartyElement(inv.SupplierParty, sb);
                GetAccountingCustomerPartyElement(inv, sb, ref ErrorMessage);
                GetDeliveryElement(inv, sb, ref ErrorMessage);
                GetPaymentMeansElement(inv, sb, ref ErrorMessage);
                //Calculate invoicelines
                CalculateInvoiceLine(inv, ref ErrorMessage);
                GetallowanceChargeElement(inv, sb);
                GetDocumentTaxTotal(inv, sb, ref ErrorMessage);
                GetLegalMonetaryTotal(inv, sb, ref ErrorMessage);
                GetInvoiceLineElement(inv, sb);
                sb.Append("</Invoice>");
                if (!string.IsNullOrEmpty(ErrorMessage))
                {
                    return false;
                }
                xmldoc = sb.ToString();

                return true;


            }
            catch (Exception ex)
            {
                ErrorMessage = ex.InnerException.ToString();
                return false;
            }

        }
        private void SignDocument(string Directorypath, string xmldocument, string returnsignedxmldir, string returnnormalxmldir, Invoice inv, ref Result result)
        {
            try
            {

                XDocument d = XDocument.Parse(xmldocument, LoadOptions.PreserveWhitespace);
                XmlDocument doc = new XmlDocument();
                //doc.LoadXml(xmldocument);
                //XmlDocument xmlDocument = new XmlDocument();
                doc.PreserveWhitespace = true;
                doc.LoadXml("<?xml version='1.0' encoding='UTF-8'?>\r\n" + d.ToString());
                string issuedate = "";
                string issuetime = "";
                if (!string.IsNullOrEmpty(inv.IssueDate))
                    issuedate = inv.IssueDate.Replace("-", "");
                if (!string.IsNullOrEmpty(inv.IssueTime))
                    issuetime = inv.IssueTime.Replace(":", "");
                //Save the document to a file.
                string xmlfile = inv.SupplierParty.partyTaxScheme.CompanyID + "_" + issuedate + "T" + issuetime + "_" + RemoveNonAlphanumeric(inv.ID) + ".xml";
                string xmlfilename = Directorypath + returnnormalxmldir + xmlfile;
                string signedxmlfilename = Directorypath + returnsignedxmldir + xmlfile;
                string signedxmlshortpath = returnsignedxmldir + xmlfile;
                string normalxmlfilename = Directorypath + returnnormalxmldir + xmlfile;
                string normalxmlshortpath = returnnormalxmldir + xmlfile;
                if (this.SaveXML)
                {
                    //FileMode.Create will overwrite the file.No seek and truncate is needed.
                    using (var fs = new FileStream(xmlfilename, FileMode.Create))
                    {
                        doc.Save(fs);
                    }
                }

                //doc.Save(xmlfilename);
                var doc1 = new XmlDocument();
                string cert = "";
                string privatekey = "";
                if (string.IsNullOrEmpty(inv.cSIDInfo.CertPem))
                {
                    try
                    {
                        cert = string.Join("", File.ReadAllLines(Directorypath + "\\cert\\cert.pem"));
                    }
                    catch
                    {
                        result.IsValid = false;
                        result.ErrorMessage = "Certificate Pem Doesn't exist";
                        return;
                    }

                }
                else
                {
                    cert = inv.cSIDInfo.CertPem;
                }
                if (string.IsNullOrEmpty(inv.cSIDInfo.PrivateKey))
                {
                    try
                    {
                        privatekey = string.Join("", File.ReadAllLines(Directorypath + "\\cert\\key.pem"));
                    }
                    catch
                    {
                        result.IsValid = false;
                        result.ErrorMessage = "Private Key Doesn't exist";
                        return;
                    }

                }
                else
                {
                    privatekey = inv.cSIDInfo.PrivateKey;
                }
                using (BLL.EInvoiceSigningLogic logic = new BLL.EInvoiceSigningLogic())
                {
                    result = logic.SignDocument(doc, cert, privatekey);
                }

                if (result.IsValid)
                {
                    doc1.PreserveWhitespace = true;
                    doc1.LoadXml("<?xml version='1.0' encoding='UTF-8'?>" + result.ResultedValue);
                    if (this.SaveXML)
                    {
                        using (var fs = new FileStream(signedxmlfilename, FileMode.Create))
                        {
                            doc1.Save(fs);
                        }
                    }

                    //doc1.Save(signedxmlfilename);
                    result.SingedXML = result.ResultedValue;
                    result.EncodedInvoice = Utility.ToBase64Encode(result.ResultedValue);
                    result.UUID = Utility.GetNodeInnerText(doc1, SettingsParams.UUID_XPATH);
                    result.InvoiceHash = Utility.GetNodeInnerText(doc1, SettingsParams.Hash_XPATH);
                    result.QRCode = Utility.GetNodeInnerText(doc1, SettingsParams.QR_CODE_XPATH);
                    result.PIH = Utility.GetNodeInnerText(doc1, SettingsParams.PIH_XPATH);
                    result.LineExtensionAmount = Utility.GetNodeInnerText(doc1, SettingsParams.LineExtensionAmount);
                    result.TaxExclusiveAmount = Utility.GetNodeInnerText(doc1, SettingsParams.TaxExclusiveAmount);
                    result.TaxInclusiveAmount = Utility.GetNodeInnerText(doc1, SettingsParams.TaxInclusiveAmount);
                    result.AllowanceTotalAmount = Utility.GetNodeInnerText(doc1, SettingsParams.AllowanceTotalAmount);
                    result.ChargeTotalAmount = Utility.GetNodeInnerText(doc1, SettingsParams.ChargeTotalAmount);
                    result.PayableAmount = Utility.GetNodeInnerText(doc1, SettingsParams.PayableAmount);
                    result.PrepaidAmount = Utility.GetNodeInnerText(doc1, SettingsParams.PrepaidAmount);
                    result.PayableRoundingAmount = Utility.GetNodeInnerText(doc1, SettingsParams.PayableRoundingAmount);
                    result.TaxAmount = Utility.GetNodeInnerText(doc1, SettingsParams.VAT_TOTAL_XPATH);
                    result.SingedXMLFileName = xmlfile;
                    result.SingedXMLFileNameFullPath = signedxmlfilename;
                    result.SingedXMLFileNameShortPath = signedxmlshortpath;
                    result.NormalXMLFileNameFullPath = normalxmlfilename;
                    result.NormalXMLFileNameShortPath = normalxmlshortpath;
                    //set the language of error message the default is arabic with code AR and English with code EN
                    Result validationresult = new Result();
                    result.WarningMessage = "";
                    result.ErrorMessage = "";
                    using (BLL.EInvoiceValidator vali = new BLL.EInvoiceValidator(this.lang))
                    {
                        validationresult = vali.ValidateEInvoice(doc1, cert, result.PIH);

                    }
                    if (!validationresult.IsValid)
                    {
                        if (!string.IsNullOrEmpty(validationresult.ErrorMessage))
                        {
                            result.ErrorMessage += validationresult.ErrorMessage + "\n";
                        }
                        foreach (Result r in validationresult.lstSteps)
                        {
                            if (r.IsValid == false)
                            {
                                if (!string.IsNullOrEmpty(r.ErrorMessage))
                                {
                                    result.ErrorMessage += r.ErrorMessage + "\n";
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(r.WarningMessage))
                                {
                                    if (!result.WarningMessage.Contains(r.WarningMessage))
                                        result.WarningMessage += r.WarningMessage + "\n";
                                }
                                
                            }
                        }
                        result.IsValid = false;
                    }
                    else
                    {
                        foreach (Result r in validationresult.lstSteps)
                        {
                            if (r.IsValid == true && !string.IsNullOrEmpty(r.WarningMessage))
                            {
                                if (!result.WarningMessage.Contains(r.WarningMessage))
                                    result.WarningMessage += r.WarningMessage + "\n";
                            }
                        }
                        result.IsValid = true;
                        try
                        {
                            File.WriteAllText(Directorypath + "\\PIH\\pih.txt", string.Empty);
                            File.WriteAllText(Directorypath + "\\PIH\\pih.txt", result.InvoiceHash);
                        }
                        catch
                        {

                        }
                    }


                }
                else
                {
                    if (!string.IsNullOrEmpty(result.ErrorMessage))
                    {
                        result.ErrorMessage += result.ErrorMessage + "\n";
                    }
                    foreach (Result r in result.lstSteps)
                    {
                        if (r.IsValid == false)
                        {
                            result.ErrorMessage += r.ErrorMessage + "\n";
                        }
                    }
                    result.IsValid = false;
                }
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.ErrorMessage = ex.Message + "\n" + ex.StackTrace + "\n" + ex.InnerException;
            }
        }

        #region Common Methods
        //private bool CheckTrail(Invoice inv)
        //{
        //    try
        //    {
        //        DateTime invoicedate = default(DateTime);

        //        DateTime.TryParseExact(inv.IssueDate, SettingsParams.allDatesFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out invoicedate);
        //        DateTime trailenddate = default(DateTime);

        //        DateTime.TryParseExact("2024-11-01", SettingsParams.allDatesFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out trailenddate);

        //        if (invoicedate > trailenddate)
        //            return false;
        //        else
        //            return true;
        //    }
        //    catch
        //    {
        //        return false;
        //    }

        //}
        private void GetCommonInvoiceTagElements(Invoice inv, StringBuilder sb)
		{
			inv.ProfileID = "reporting:1.0";
			if (string.IsNullOrEmpty(inv.UUID))
				inv.UUID = Guid.NewGuid().ToString();
			if (string.IsNullOrEmpty(inv.TaxCurrencyCode))
				inv.TaxCurrencyCode = "SAR";
			if (string.IsNullOrEmpty(inv.DocumentCurrencyCode))
				inv.DocumentCurrencyCode = "SAR";
			sb.Append("<?xml version='1.0' encoding='UTF-8'?>");
			sb.Append("<Invoice xmlns='urn:oasis:names:specification:ubl:schema:xsd:Invoice-2' xmlns:cac='urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2' xmlns:cbc='urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2' xmlns:ext='urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2'>");
			sb.Append("<cbc:ProfileID>" + inv.ProfileID + "</cbc:ProfileID>");
			sb.Append("<cbc:ID>" + inv.ID + "</cbc:ID>");
			sb.Append("<cbc:UUID>" + inv.UUID + "</cbc:UUID>");
			sb.Append("<cbc:IssueDate>" + inv.IssueDate + "</cbc:IssueDate>");
            //if (!string.IsNullOrEmpty(inv.IssueTime))
            //    inv.IssueTime = inv.IssueTime.Replace("Z", "") + "Z";

            sb.Append("<cbc:IssueTime>" + inv.IssueTime + "</cbc:IssueTime>");
			sb.Append("<cbc:InvoiceTypeCode name='" + inv.invoiceTypeCode.Name + "'>" + inv.invoiceTypeCode.id + "</cbc:InvoiceTypeCode>");
			sb.Append("<cbc:DocumentCurrencyCode>" + inv.DocumentCurrencyCode + "</cbc:DocumentCurrencyCode>");
			
			sb.Append("<cbc:TaxCurrencyCode>" + inv.TaxCurrencyCode + "</cbc:TaxCurrencyCode>");
			sb.Append("<cbc:LineCountNumeric>" + inv.InvoiceLines.Count + "</cbc:LineCountNumeric>");
		}
		private void GetPIHElement(string Directorypath, Invoice inv, StringBuilder sb, ref string error)
		{

			try
			{
				string pih = "";
				if (!string.IsNullOrEmpty(inv.AdditionalDocumentReferencePIH.EmbeddedDocumentBinaryObject))
				{
					pih = inv.AdditionalDocumentReferencePIH.EmbeddedDocumentBinaryObject;
				}
				else
				{
						pih = File.ReadAllText(Directorypath + "\\PIH\\pih.txt").Trim();
                        inv.AdditionalDocumentReferencePIH.EmbeddedDocumentBinaryObject = pih;
                }
				sb.Append("<cac:AdditionalDocumentReference>" +
					"<cbc:ID>PIH</cbc:ID>" +
					"<cac:Attachment>" +
					"<cbc:EmbeddedDocumentBinaryObject mimeCode='text/plain'>" + pih +
					"</cbc:EmbeddedDocumentBinaryObject>" +
					"</cac:Attachment>" +
					"</cac:AdditionalDocumentReference>");
			}
			catch
			{
				//error += ex.InnerException.ToString() + "\n";
				error += "PIH doesn't exist.";
			}

		}
		private void GetICVElement(long icv, StringBuilder sb, ref string error)
		{

			try
			{
				if (icv != 0)
				{
					sb.Append("<cac:AdditionalDocumentReference>" +
						"<cbc:ID>ICV</cbc:ID>" +
						"<cbc:UUID>" + icv + "</cbc:UUID>" +
						"</cac:AdditionalDocumentReference>");

				}
				else
				{
					error += "\n";
					error += "Error in Invoice Counter Value.";
				}
			}
			catch
			{
				error += "\n";
				error += "Error in Invoice Counter Value.";
			}


		}
		private void GetInvoiceDocumentReferenceElement(InvoiceDocumentReferenceCollection InvoiceDocumentReferences, StringBuilder sb)
		{
            //this is for debit and credit notes 
            foreach (InvoiceDocumentReference doc in InvoiceDocumentReferences)
            {
                if (!string.IsNullOrEmpty(doc.ID))
                {
                    sb.Append("<cac:BillingReference>" +
                        "<cac:InvoiceDocumentReference>" +
                        "<cbc:ID>" + doc.ID + "</cbc:ID>" +
                        "</cac:InvoiceDocumentReference>" +
                        "</cac:BillingReference>");

                }
            }
		}
		private string RemoveNonAlphanumeric(string str)
		{
            if (string.IsNullOrEmpty(str))
                return "";
			Regex rgx = new Regex("[^a-zA-Z0-9 -]");
			str = rgx.Replace(str, "");
			return str;
		}
		private void GetAccountingSupplierPartyElement(AccountingSupplierParty supplier, StringBuilder sb)
		{

			sb.Append("<cac:AccountingSupplierParty>" +
				"<cac:Party>");
			if (!string.IsNullOrEmpty(supplier.partyIdentification.ID))
			{
                if (!string.IsNullOrEmpty(supplier.partyIdentification.schemeID))
                {
                    supplier.partyIdentification.schemeID = supplier.partyIdentification.schemeID.Trim().ToUpper();

                }
                sb.Append("<cac:PartyIdentification>" +
				"<cbc:ID schemeID='" + supplier.partyIdentification.schemeID + "'>" + supplier.partyIdentification.ID + "</cbc:ID>" +
				"</cac:PartyIdentification>");
			}
			sb.Append("<cac:PostalAddress>");
			if (!string.IsNullOrEmpty(supplier.postalAddress.StreetName))
			{
				sb.Append("<cbc:StreetName>" + Utility.ReplaceXMLSpecialCharacters(supplier.postalAddress.StreetName) + "</cbc:StreetName>");
			}
			if (!string.IsNullOrEmpty(supplier.postalAddress.AdditionalStreetName))
			{
				sb.Append("<cbc:AdditionalStreetName>" + Utility.ReplaceXMLSpecialCharacters(supplier.postalAddress.AdditionalStreetName)+ "</cbc:AdditionalStreetName>");
			}
			if (!string.IsNullOrEmpty(supplier.postalAddress.BuildingNumber))
			{
				sb.Append("<cbc:BuildingNumber>" + supplier.postalAddress.BuildingNumber + "</cbc:BuildingNumber>");
			}

			if (!string.IsNullOrEmpty(supplier.postalAddress.PlotIdentification))
			{
				sb.Append("<cbc:PlotIdentification>" + supplier.postalAddress.PlotIdentification + "</cbc:PlotIdentification>");
			}
			if (!string.IsNullOrEmpty(supplier.postalAddress.CitySubdivisionName))
			{
				sb.Append("<cbc:CitySubdivisionName>" + Utility.ReplaceXMLSpecialCharacters(supplier.postalAddress.CitySubdivisionName) + "</cbc:CitySubdivisionName>");
			}
			if (!string.IsNullOrEmpty(supplier.postalAddress.CityName))
			{
				sb.Append("<cbc:CityName>" + Utility.ReplaceXMLSpecialCharacters(supplier.postalAddress.CityName) + "</cbc:CityName>");
			}
			if (!string.IsNullOrEmpty(supplier.postalAddress.PostalZone))
			{
				sb.Append("<cbc:PostalZone>" + supplier.postalAddress.PostalZone + "</cbc:PostalZone>");
			}
			if (!string.IsNullOrEmpty(supplier.postalAddress.CountrySubentity))
			{
				sb.Append("<cbc:CountrySubentity>" + Utility.ReplaceXMLSpecialCharacters(supplier.postalAddress.CountrySubentity) + "</cbc:CountrySubentity>");
			}
			if (string.IsNullOrEmpty(supplier.postalAddress.country.IdentificationCode))
			{
				supplier.postalAddress.country.IdentificationCode = "SA";
			}
			sb.Append("<cac:Country>" +
				"<cbc:IdentificationCode>" + supplier.postalAddress.country.IdentificationCode + "</cbc:IdentificationCode>" +
				"</cac:Country>");
			sb.Append("</cac:PostalAddress>");
			if (!string.IsNullOrEmpty(supplier.partyTaxScheme.CompanyID))
			{
				sb.Append("<cac:PartyTaxScheme>" +
					"<cbc:CompanyID>" + supplier.partyTaxScheme.CompanyID + "</cbc:CompanyID>" +
					"<cac:TaxScheme>" +
				"<cbc:ID>VAT</cbc:ID>" +
				"</cac:TaxScheme>" +
				"</cac:PartyTaxScheme>");
			}
			if (!string.IsNullOrEmpty(supplier.partyLegalEntity.RegistrationName))
			{
				sb.Append("<cac:PartyLegalEntity>" +
				"<cbc:RegistrationName>" + Utility.ReplaceXMLSpecialCharacters(supplier.partyLegalEntity.RegistrationName) + "</cbc:RegistrationName>" +
				"</cac:PartyLegalEntity>");
			}
				sb.Append("</cac:Party>" +
				"</cac:AccountingSupplierParty>");
		}
		private void GetAccountingCustomerPartyElement(Invoice inv, StringBuilder sb, ref string error)
		{
			AccountingCustomerParty customer = inv.CustomerParty;
			bool issimplified = inv.invoiceTypeCode.Name.Substring(0, 2) == "02";
			bool issummary = inv.invoiceTypeCode.Name.Substring(5, 1) == "1";
			//error = "";
			if (issimplified && issummary && string.IsNullOrEmpty(customer.partyLegalEntity.RegistrationName))
			{
				error += "\n";
				error += "You must Enter Customer registration name";
			}
			if (string.IsNullOrEmpty(customer.partyIdentification.ID)
				&& string.IsNullOrEmpty(customer.postalAddress.StreetName)
				&& string.IsNullOrEmpty(customer.postalAddress.AdditionalStreetName)
				&& string.IsNullOrEmpty(customer.postalAddress.BuildingNumber)
				&& string.IsNullOrEmpty(customer.postalAddress.PlotIdentification)
				&& string.IsNullOrEmpty(customer.postalAddress.CitySubdivisionName)
				&& string.IsNullOrEmpty(customer.postalAddress.CityName)
				&& string.IsNullOrEmpty(customer.postalAddress.PostalZone)
				&& string.IsNullOrEmpty(customer.partyTaxScheme.CompanyID)
				&& string.IsNullOrEmpty(customer.partyLegalEntity.RegistrationName)
				)
			{
				sb.Append("<cac:AccountingCustomerParty>" + "</cac:AccountingCustomerParty>");
				return;
			}
			sb.Append("<cac:AccountingCustomerParty>" +
				"<cac:Party>");
			if (!string.IsNullOrEmpty(customer.partyIdentification.ID))
			{
                if (!string.IsNullOrEmpty(customer.partyIdentification.schemeID))
                {
                    customer.partyIdentification.schemeID = customer.partyIdentification.schemeID.Trim().ToUpper();
                }
                sb.Append("<cac:PartyIdentification>" +
				"<cbc:ID schemeID='" + customer.partyIdentification.schemeID + "'>" + customer.partyIdentification.ID + "</cbc:ID>" +
				"</cac:PartyIdentification>");
			}
			sb.Append("<cac:PostalAddress>");
			if (!string.IsNullOrEmpty(customer.postalAddress.StreetName))
			{
				sb.Append("<cbc:StreetName>" + Utility.ReplaceXMLSpecialCharacters(customer.postalAddress.StreetName) + "</cbc:StreetName>");
			}
			if (!string.IsNullOrEmpty(customer.postalAddress.AdditionalStreetName))
			{
				sb.Append("<cbc:AdditionalStreetName>" + Utility.ReplaceXMLSpecialCharacters(customer.postalAddress.AdditionalStreetName) + "</cbc:AdditionalStreetName>");
			}
			if (!string.IsNullOrEmpty(customer.postalAddress.BuildingNumber))
			{
				sb.Append("<cbc:BuildingNumber>" + customer.postalAddress.BuildingNumber + "</cbc:BuildingNumber>");
			}
			if (!string.IsNullOrEmpty(customer.postalAddress.PlotIdentification))
			{
				sb.Append("<cbc:PlotIdentification>" + customer.postalAddress.PlotIdentification + "</cbc:PlotIdentification>");
			}
			if (!string.IsNullOrEmpty(customer.postalAddress.CitySubdivisionName))
			{
				sb.Append("<cbc:CitySubdivisionName>" + Utility.ReplaceXMLSpecialCharacters(customer.postalAddress.CitySubdivisionName) + "</cbc:CitySubdivisionName>");
			}
			if (!string.IsNullOrEmpty(customer.postalAddress.CityName))
			{
				sb.Append("<cbc:CityName>" + Utility.ReplaceXMLSpecialCharacters(customer.postalAddress.CityName) + "</cbc:CityName>");
			}
			if (!string.IsNullOrEmpty(customer.postalAddress.PostalZone))
			{
				sb.Append("<cbc:PostalZone>" + customer.postalAddress.PostalZone + "</cbc:PostalZone>");
			}
			if (!string.IsNullOrEmpty(customer.postalAddress.CountrySubentity))
			{
				sb.Append("<cbc:CountrySubentity>" + Utility.ReplaceXMLSpecialCharacters(customer.postalAddress.CountrySubentity) + "</cbc:CountrySubentity>");
			}
			if (string.IsNullOrEmpty(customer.postalAddress.country.IdentificationCode))
			{
				customer.postalAddress.country.IdentificationCode = "SA";
			}
			sb.Append("<cac:Country>" +
				"<cbc:IdentificationCode>" + customer.postalAddress.country.IdentificationCode + "</cbc:IdentificationCode>" +
				"</cac:Country>");
			sb.Append("</cac:PostalAddress>");
			if (!string.IsNullOrEmpty(customer.partyTaxScheme.CompanyID))
			{
				sb.Append("<cac:PartyTaxScheme>" +
					"<cbc:CompanyID>" + customer.partyTaxScheme.CompanyID + "</cbc:CompanyID>" +
					"<cac:TaxScheme>" +
				"<cbc:ID>VAT</cbc:ID>" +
				"</cac:TaxScheme>" +
				"</cac:PartyTaxScheme>");
			}
			if (!string.IsNullOrEmpty(customer.partyLegalEntity.RegistrationName))
			{
				sb.Append("<cac:PartyLegalEntity>" +
				"<cbc:RegistrationName>" + Utility.ReplaceXMLSpecialCharacters(customer.partyLegalEntity.RegistrationName) + "</cbc:RegistrationName>" +
				"</cac:PartyLegalEntity>");
			}
            if (!string.IsNullOrEmpty(customer.contact.Name)
                || !string.IsNullOrEmpty(customer.contact.Telephone)
                || !string.IsNullOrEmpty(customer.contact.ElectronicMail)
                || !string.IsNullOrEmpty(customer.contact.Note)
                )
            {
                sb.Append("<cac:Contact>");
                if (!string.IsNullOrEmpty(customer.contact.Name))
                {

                    sb.Append("<cbc:Name>" + Utility.ReplaceXMLSpecialCharacters(customer.contact.Name) + "</cbc:Name>");
                   
                }
                if (!string.IsNullOrEmpty(customer.contact.Telephone))
                {

                    sb.Append("<cbc:Telephone>" + Utility.ReplaceXMLSpecialCharacters(customer.contact.Telephone) + "</cbc:Telephone>");

                }
                if (!string.IsNullOrEmpty(customer.contact.ElectronicMail))
                {

                    sb.Append("<cbc:ElectronicMail>" + Utility.ReplaceXMLSpecialCharacters(customer.contact.ElectronicMail) + "</cbc:ElectronicMail>");

                }
                if (!string.IsNullOrEmpty(customer.contact.Note))
                {

                    sb.Append("<cbc:Note>" + Utility.ReplaceXMLSpecialCharacters(customer.contact.Note) + "</cbc:Note>");

                }
                sb.Append("</cac:Contact>");
            }
            sb.Append("</cac:Party>" +
				"</cac:AccountingCustomerParty>");

		}
		private void GetDeliveryElement(Invoice inv, StringBuilder sb, ref string error)
		{
			Delivery delivery = inv.delivery;

			bool issimplified = inv.invoiceTypeCode.Name.Substring(0, 2) == "02";
			bool issummary = inv.invoiceTypeCode.Name.Substring(5, 1) == "1";
			//error = "";
			if (issimplified && issummary && string.IsNullOrEmpty(delivery.ActualDeliveryDate))
			{
				error += "\n";
				error += "You must Enter start and end Delivery Date";
			}

			if (string.IsNullOrEmpty(delivery.ActualDeliveryDate) && string.IsNullOrEmpty(delivery.LatestDeliveryDate))
			{

			}
			else
			{
				sb.Append("<cac:Delivery>");
				if (!string.IsNullOrEmpty(delivery.ActualDeliveryDate))
					sb.Append("<cbc:ActualDeliveryDate>" + delivery.ActualDeliveryDate + "</cbc:ActualDeliveryDate>");
				if (!string.IsNullOrEmpty(delivery.LatestDeliveryDate))
					sb.Append("<cbc:LatestDeliveryDate>" + delivery.LatestDeliveryDate + "</cbc:LatestDeliveryDate>");
				sb.Append("</cac:Delivery>");
			}

		}
        private void CalculateInvoiceLine(Invoice inv, ref string error)
        {
            try
            {
                foreach (InvoiceLine l in inv.InvoiceLines)
                {
                    RepairTaxCategory(l);
                    //get original item price
                    if (l.price.EncludingVat)
                    {
                        //l.price.PriceAmount = Math.Round((l.price.PriceAmount / ((l.taxTotal.TaxSubtotal.taxCategory.Percent / 100) + 1)), 2, MidpointRounding.AwayFromZero);
                        l.price.PriceAmount = (l.price.PriceAmount / ((l.taxTotal.TaxSubtotal.taxCategory.Percent / 100) + 1));

                    }
                    else
                    {
                        //l.price.PriceAmount = Math.Round(l.price.PriceAmount, 2, MidpointRounding.AwayFromZero);
                        l.price.PriceAmount = l.price.PriceAmount;
                    }

                    decimal allowanceamount = 0;
                    decimal chargeamount = 0;
                    foreach (var allowancecharge in l.allowanceCharges)
                    {
                        if (allowancecharge.Amount > 0)
                        {
                            if (allowancecharge.ChargeIndicator)
                                chargeamount += allowancecharge.Amount;
                            else
                                allowanceamount += allowancecharge.Amount;
                        }
                        if (allowancecharge.MultiplierFactorNumeric > 0 && allowancecharge.BaseAmount > 0)
                        {
                            allowancecharge.Amount = (allowancecharge.MultiplierFactorNumeric / 100) * allowancecharge.BaseAmount;
                            if (allowancecharge.ChargeIndicator)
                                chargeamount += allowancecharge.Amount;
                            else
                                allowanceamount += allowancecharge.Amount;
                        }

                    }
                    if (l.price.BaseQuantity == 0)
                    {
                        l.price.BaseQuantity = 1;
                    }
                    decimal invoicelinenetamount = (l.InvoiceQuantity * (l.price.PriceAmount / l.price.BaseQuantity)) + chargeamount - allowanceamount;
                    decimal taxamount = invoicelinenetamount * (l.taxTotal.TaxSubtotal.taxCategory.Percent / 100);
                    l.LineExtensionAmount = Math.Round(invoicelinenetamount, 2, MidpointRounding.AwayFromZero);
                    //l.taxTotal.TaxAmount = Math.Round(taxamount, 2, MidpointRounding.AwayFromZero);
                    l.taxTotal.TaxAmount = taxamount;
                }
            }
            catch
            {
                error += "\n";
                error += "Error in Calculating InvoiceLine.";
            }

        }
        private void GetallowanceChargeElement(Invoice inv, StringBuilder sb)
        {
            // we change this method to allow use allowance or charge by changing ChargeIndicator true/false
            foreach (AllowanceCharge allowance in inv.allowanceCharges)
            {

                decimal allowanceamount = 0;
                bool addbaseamounttag = false;
                if (allowance.Amount > 0)
                {
                    allowanceamount = allowance.Amount;

                }
                else if (allowance.MultiplierFactorNumeric > 0 && allowance.BaseAmount > 0)
                {
                    allowanceamount = (allowance.MultiplierFactorNumeric / 100) * allowance.BaseAmount;
                    allowance.Amount = allowanceamount;
                    addbaseamounttag = true;
                }

                if (allowanceamount > 0)
                {
                    StringBuilder taxcat = new StringBuilder();
                    sb.Append("<cac:AllowanceCharge>");
                    sb.Append("<cbc:ChargeIndicator>" + allowance.ChargeIndicator.ToString().ToLower() + "</cbc:ChargeIndicator>");
                    if (!string.IsNullOrEmpty(allowance.AllowanceChargeReasonCode))
                    {
                        sb.Append("<cbc:AllowanceChargeReasonCode>" + allowance.AllowanceChargeReasonCode + "</cbc:AllowanceChargeReasonCode>");
                    }
                    if (!string.IsNullOrEmpty(allowance.AllowanceChargeReason))
                    {
                        sb.Append("<cbc:AllowanceChargeReason>" + Utility.ReplaceXMLSpecialCharacters(allowance.AllowanceChargeReason) + "</cbc:AllowanceChargeReason>");

                    }
                    GetTaxCategoryElement(allowance.taxCategory.Percent, allowance.taxCategory.ID, taxcat);
                    if (addbaseamounttag)
                    {
                        sb.Append("<cbc:MultiplierFactorNumeric>" + allowance.MultiplierFactorNumeric.ToString("0.00") + "</cbc:MultiplierFactorNumeric>");

                    }
                    sb.Append("<cbc:Amount currencyID='" + inv.DocumentCurrencyCode + "'>" + allowanceamount.ToString("0.00") + "</cbc:Amount>");
                    if (addbaseamounttag)
                    {
                        sb.Append("<cbc:BaseAmount currencyID='" + inv.DocumentCurrencyCode + "'>" + allowance.BaseAmount.ToString("0.00") + "</cbc:BaseAmount>");
                    }
                    sb.Append(taxcat);
                    sb.Append("</cac:AllowanceCharge>");
                }

            }

        }
        private void GetIvoiceLineallowanceChargeElement(InvoiceLine invline, string DocumentCurrencyCode, StringBuilder sb)
        {
            foreach (var allowancecharge in invline.allowanceCharges)
            {
                decimal allowanceamount = 0;
                bool addbaseamounttag = false;
                if (allowancecharge.MultiplierFactorNumeric > 0 && allowancecharge.BaseAmount > 0)
                {
                    allowanceamount = (allowancecharge.MultiplierFactorNumeric / 100) * allowancecharge.BaseAmount;
                    allowancecharge.Amount = allowanceamount;
                    addbaseamounttag = true;
                }
                else
                {
                    allowanceamount = allowancecharge.Amount;
                }

                if (allowanceamount > 0)
                {
                    sb.Append("<cac:AllowanceCharge>");
                    sb.Append("<cbc:ChargeIndicator>" + allowancecharge.ChargeIndicator.ToString().ToLower() + "</cbc:ChargeIndicator>");
                    if (!string.IsNullOrEmpty(allowancecharge.AllowanceChargeReasonCode))
                    {
                        sb.Append("<cbc:AllowanceChargeReasonCode>" + allowancecharge.AllowanceChargeReasonCode + "</cbc:AllowanceChargeReasonCode>");

                    }
                    if (!string.IsNullOrEmpty(allowancecharge.AllowanceChargeReason))
                    {
                        sb.Append("<cbc:AllowanceChargeReason>" + Utility.ReplaceXMLSpecialCharacters(allowancecharge.AllowanceChargeReason) + "</cbc:AllowanceChargeReason>");
                    }
                    if (addbaseamounttag)
                    {
                        sb.Append("<cbc:MultiplierFactorNumeric>" + allowancecharge.MultiplierFactorNumeric.ToString("0.00") + "</cbc:MultiplierFactorNumeric>");

                    }
                    sb.Append("<cbc:Amount currencyID='" + DocumentCurrencyCode + "'>" + Math.Round(allowanceamount, 2, MidpointRounding.AwayFromZero).ToString("0.00") + "</cbc:Amount>");
                    if (addbaseamounttag)
                    {
                        sb.Append("<cbc:BaseAmount currencyID='" + DocumentCurrencyCode + "'>" + allowancecharge.BaseAmount.ToString("0.00") + "</cbc:BaseAmount>");
                    }
                    sb.Append("</cac:AllowanceCharge>");
                }
            }

        }
        private void GetTaxCategoryElement(decimal percent, string code, StringBuilder taxcat)
        {

            taxcat.Append("<cac:TaxCategory>" +
                "<cbc:ID>" + code + "</cbc:ID>" +
                "<cbc:Percent>" + percent.ToString("0.00") + "</cbc:Percent>" +
                "<cac:TaxScheme>" +
                "<cbc:ID>VAT</cbc:ID>" +
                "</cac:TaxScheme>" +
                "</cac:TaxCategory>");
        }
        private void GetLegalMonetaryTotal(Invoice inv, StringBuilder sb, ref string error)
        {

            try
            {
                decimal lineextensionamount = 0;
                decimal invoicetotalvatamount = 0;


                Dictionary<string, TaxSubtotal> subtaxkeys = new Dictionary<string, TaxSubtotal>();

                var LinesGroupedwithvat = inv.InvoiceLines.GroupBy(i => new { ID = i.item.classifiedTaxCategory.ID.Trim(), Percent = i.item.classifiedTaxCategory.Percent })
         .Select(
             l => new
             {
                 VatCategory = l.Key.ID,
                 VatPercent = l.Key.Percent,
                 LineExtensionAmount = l.Sum(s => s.LineExtensionAmount),
                 TaxSubtotal = l.First().taxTotal.TaxSubtotal
             }).ToList();
                foreach (var grp in LinesGroupedwithvat)
                {
                    lineextensionamount += grp.LineExtensionAmount;
                    invoicetotalvatamount += (grp.LineExtensionAmount * (grp.VatPercent / 100)) ;
                  
                }



                //foreach (InvoiceLine l in inv.InvoiceLines)
                //{
                //    invoicetotalvatamount += l.taxTotal.TaxAmount;
                //    lineextensionamount += l.LineExtensionAmount;
                //}
                decimal allowancetotalamount = 0;
                decimal allowancechargetotalvat = 0;
                decimal chargetotalamount = 0;
                decimal chargetotalvat = 0;
                foreach (AllowanceCharge allowance in inv.allowanceCharges)
                {
                    if (allowance.Amount > 0)
                    {
                        allowance.Amount = Math.Round(allowance.Amount, 2, MidpointRounding.AwayFromZero);
                        if (allowance.ChargeIndicator)
                        {
                            chargetotalamount += allowance.Amount;
                            // get allowance  vat 
                            chargetotalvat += allowance.Amount * (allowance.taxCategory.Percent / 100);
                        }
                        else
                        {
                            allowancetotalamount += allowance.Amount;
                            // get allowance  vat 
                            allowancechargetotalvat += allowance.Amount * (allowance.taxCategory.Percent / 100);
                        }
                    }
                }
                decimal taxexclusiveamount = lineextensionamount + chargetotalamount - allowancetotalamount;

                decimal taxinclusiveamount = Math.Round(taxexclusiveamount + invoicetotalvatamount + chargetotalvat - allowancechargetotalvat, 2, MidpointRounding.AwayFromZero);

                inv.legalMonetaryTotal.ChargeTotalAmount = chargetotalamount;
                inv.legalMonetaryTotal.AllowanceTotalAmount = allowancetotalamount;
                inv.legalMonetaryTotal.LineExtensionAmount = Math.Round(lineextensionamount, 2, MidpointRounding.AwayFromZero);
                inv.legalMonetaryTotal.TaxExclusiveAmount = Math.Round(taxexclusiveamount, 2, MidpointRounding.AwayFromZero);
                inv.legalMonetaryTotal.TaxInclusiveAmount = taxinclusiveamount;
                inv.legalMonetaryTotal.PayableAmount= Math.Round(inv.legalMonetaryTotal.PayableAmount, 2, MidpointRounding.AwayFromZero);
                inv.legalMonetaryTotal.PrepaidAmount = Math.Round(inv.legalMonetaryTotal.PrepaidAmount, 2, MidpointRounding.AwayFromZero);
                inv.legalMonetaryTotal.PayableRoundingAmount = Math.Round(inv.legalMonetaryTotal.PayableRoundingAmount, 2, MidpointRounding.AwayFromZero);
                sb.Append("<cac:LegalMonetaryTotal>");
                sb.Append("<cbc:LineExtensionAmount currencyID='" + inv.DocumentCurrencyCode + "'>" + inv.legalMonetaryTotal.LineExtensionAmount.ToString("0.00") + "</cbc:LineExtensionAmount>");
                sb.Append("<cbc:TaxExclusiveAmount currencyID='" + inv.DocumentCurrencyCode + "'>" + inv.legalMonetaryTotal.TaxExclusiveAmount.ToString("0.00") + "</cbc:TaxExclusiveAmount>");
                sb.Append("<cbc:TaxInclusiveAmount currencyID='" + inv.DocumentCurrencyCode + "'>" + inv.legalMonetaryTotal.TaxInclusiveAmount.ToString("0.00") + "</cbc:TaxInclusiveAmount>");
                //if (allowancetotalamount > 0)
                //{
                sb.Append("<cbc:AllowanceTotalAmount currencyID='" + inv.DocumentCurrencyCode + "'>" + allowancetotalamount.ToString("0.00") + "</cbc:AllowanceTotalAmount>");
                //}
                //if (inv.legalMonetaryTotal.ChargeTotalAmount > 0)
                //{
                sb.Append("<cbc:ChargeTotalAmount currencyID='" + inv.DocumentCurrencyCode + "'>" + chargetotalamount.ToString("0.00") + "</cbc:ChargeTotalAmount>");
                //}
                if (inv.legalMonetaryTotal.PrepaidAmount > 0)
                {
                    sb.Append("<cbc:PrepaidAmount currencyID='" + inv.DocumentCurrencyCode + "'>" + Math.Round(inv.legalMonetaryTotal.PrepaidAmount, 2, MidpointRounding.AwayFromZero).ToString("0.00") + "</cbc:PrepaidAmount>");
                }
                //
                if (inv.legalMonetaryTotal.PayableAmount > 0)
                {
                  inv.legalMonetaryTotal.PayableRoundingAmount = (inv.legalMonetaryTotal.PayableAmount + inv.legalMonetaryTotal.PrepaidAmount) - taxinclusiveamount;
                    
                }
                else
                {
                   // inv.legalMonetaryTotal.PayableRoundingAmount = 0;
                }
                if (inv.legalMonetaryTotal.PayableRoundingAmount != 0)
                {
                    sb.Append("<cbc:PayableRoundingAmount currencyID='" + inv.DocumentCurrencyCode + "'>" + Math.Round(inv.legalMonetaryTotal.PayableRoundingAmount, 2, MidpointRounding.AwayFromZero).ToString("0.00") + "</cbc:PayableRoundingAmount>");

                }
                if (inv.legalMonetaryTotal.PayableAmount > 0)
                {
                    sb.Append("<cbc:PayableAmount currencyID='" + inv.DocumentCurrencyCode + "'>" + Math.Round(inv.legalMonetaryTotal.PayableAmount, 2, MidpointRounding.AwayFromZero).ToString("0.00") + "</cbc:PayableAmount>");
                }
                else
                {
                    inv.legalMonetaryTotal.PayableAmount = Math.Round(taxinclusiveamount - inv.legalMonetaryTotal.PrepaidAmount + inv.legalMonetaryTotal.PayableRoundingAmount, 2, MidpointRounding.AwayFromZero);
                    sb.Append("<cbc:PayableAmount currencyID='" + inv.DocumentCurrencyCode + "'>" + inv.legalMonetaryTotal.PayableAmount.ToString("0.00") + "</cbc:PayableAmount>");
                }

                sb.Append("</cac:LegalMonetaryTotal>");

            }
            catch
            {
                error += "\n";
                error += "Error in LegalMonetaryTotal.";
            }

        }
        private void GetDocumentTaxTotal(Invoice inv, StringBuilder sb, ref string error)
        {


            decimal taxtotalamount = 0;
            decimal totallineextensionamount = 0;

            Dictionary<string, TaxSubtotal> subtaxkeys = new Dictionary<string, TaxSubtotal>();

            StringBuilder subtotaltext = new StringBuilder();
           var LinesGroupedwithvat= inv.InvoiceLines.GroupBy(i=>new { ID=i.item.classifiedTaxCategory.ID.Trim(), Percent=i.item.classifiedTaxCategory.Percent })
    .Select(
        l => new
        {
            VatCategory = l.Key.ID,
            VatPercent = l.Key.Percent,
            LineExtensionAmount = l.Sum(s => s.LineExtensionAmount),
            TaxSubtotal=l.First().taxTotal.TaxSubtotal
        }).ToList();
            foreach (var grp in LinesGroupedwithvat)
            {
                if (subtaxkeys.ContainsKey(grp.VatPercent.ToString("0.00") + grp.VatCategory.Trim()))
                {
                    subtaxkeys[grp.VatPercent.ToString("0.00") + grp.VatCategory.Trim()].TaxableAmount += grp.LineExtensionAmount;
                    subtaxkeys[grp.VatPercent.ToString("0.00") + grp.VatCategory.Trim()].TaxAmount += grp.LineExtensionAmount * (grp.VatPercent / 100);
                }
                else
                {
                    
                    grp.TaxSubtotal.TaxableAmount = grp.LineExtensionAmount;
                    grp.TaxSubtotal.TaxAmount = grp.LineExtensionAmount * (grp.VatPercent / 100);
                    subtaxkeys.Add(grp.VatPercent.ToString("0.00") + grp.VatCategory.Trim(), grp.TaxSubtotal);
                }
            }


            //foreach (InvoiceLine l in inv.InvoiceLines)
            //{

            //    totallineextensionamount += l.LineExtensionAmount;
            //    taxtotalamount += l.taxTotal.TaxAmount;
            //    if (subtaxkeys.ContainsKey(l.item.classifiedTaxCategory.Percent.ToString("0.00") + l.item.classifiedTaxCategory.ID.Trim()))
            //    {
            //        subtaxkeys[l.item.classifiedTaxCategory.Percent.ToString("0.00") + l.item.classifiedTaxCategory.ID.Trim()].TaxableAmount += l.LineExtensionAmount;
            //        subtaxkeys[l.item.classifiedTaxCategory.Percent.ToString("0.00") + l.item.classifiedTaxCategory.ID.Trim()].TaxAmount += l.taxTotal.TaxAmount;
            //    }
            //    else
            //    {
            //        l.taxTotal.TaxSubtotal.TaxableAmount = l.LineExtensionAmount;
            //        l.taxTotal.TaxSubtotal.TaxAmount = l.taxTotal.TaxAmount;
            //        subtaxkeys.Add(l.item.classifiedTaxCategory.Percent.ToString("0.00") + l.item.classifiedTaxCategory.ID.Trim(), l.taxTotal.TaxSubtotal);
            //    }
            //}
            foreach (AllowanceCharge allowance in inv.allowanceCharges)
            {
                decimal allowanceamount = 0;
                decimal chargeamount = 0;

                if (allowance.MultiplierFactorNumeric > 0 && allowance.BaseAmount > 0)
                {
                    allowance.Amount= (allowance.MultiplierFactorNumeric / 100) * allowance.BaseAmount;
                    if (allowance.ChargeIndicator)
                        chargeamount = allowance.Amount;
                    else
                        allowanceamount = allowance.Amount;
                }
                else
                {
                    if (allowance.ChargeIndicator)
                        chargeamount = allowance.Amount;
                    else
                        allowanceamount = allowance.Amount;
                }
                if (allowanceamount > 0 && allowance.ChargeIndicator == false)
                {
                    decimal allowancevat = allowanceamount * (allowance.taxCategory.Percent / 100);
                    if (subtaxkeys.ContainsKey(allowance.taxCategory.Percent.ToString("0.00") + allowance.taxCategory.ID.Trim()))
                    {
                        subtaxkeys[allowance.taxCategory.Percent.ToString("0.00") + allowance.taxCategory.ID.Trim()].TaxableAmount = subtaxkeys[allowance.taxCategory.Percent.ToString("0.00") + allowance.taxCategory.ID.Trim()].TaxableAmount - allowanceamount;
                        subtaxkeys[allowance.taxCategory.Percent.ToString("0.00") + allowance.taxCategory.ID.Trim()].TaxAmount = subtaxkeys[allowance.taxCategory.Percent.ToString("0.00") + allowance.taxCategory.ID.Trim()].TaxAmount - allowancevat;
                    }
                }
                if (chargeamount > 0 && allowance.ChargeIndicator == true)
                {
                    decimal chargevat = chargeamount * (allowance.taxCategory.Percent / 100);
                    if (subtaxkeys.ContainsKey(allowance.taxCategory.Percent.ToString("0.00") + allowance.taxCategory.ID.Trim()))
                    {
                        subtaxkeys[allowance.taxCategory.Percent.ToString("0.00") + allowance.taxCategory.ID.Trim()].TaxableAmount = subtaxkeys[allowance.taxCategory.Percent.ToString("0.00") + allowance.taxCategory.ID.Trim()].TaxableAmount + chargeamount;
                        subtaxkeys[allowance.taxCategory.Percent.ToString("0.00") + allowance.taxCategory.ID.Trim()].TaxAmount = subtaxkeys[allowance.taxCategory.Percent.ToString("0.00") + allowance.taxCategory.ID.Trim()].TaxAmount + chargevat;
                    }
                }
            }
            foreach (var key in subtaxkeys)
            {
                TaxSubtotal sub = key.Value;
                
                subtotaltext.Append("<cac:TaxSubtotal>" +
            "<cbc:TaxableAmount currencyID='" + inv.DocumentCurrencyCode + "'>" + Math.Round(sub.TaxableAmount, 2, MidpointRounding.AwayFromZero).ToString("0.00") + "</cbc:TaxableAmount>" +
            "<cbc:TaxAmount currencyID='" + inv.DocumentCurrencyCode + "'>" + Math.Round(sub.TaxAmount, 2, MidpointRounding.AwayFromZero).ToString("0.00") + "</cbc:TaxAmount>");
                subtotaltext.Append("<cac:TaxCategory>" +
                "<cbc:ID>" + sub.taxCategory.ID + "</cbc:ID>" +
                "<cbc:Percent>" + sub.taxCategory.Percent.ToString("0.00") + "</cbc:Percent>");
                if (sub.taxCategory.ID == "O" || sub.taxCategory.ID == "E" || sub.taxCategory.ID == "Z")
                {
                    if (sub.taxCategory.ID == "Z" && string.IsNullOrEmpty(sub.taxCategory.TaxExemptionReasonCode))
                    {
                        error += "\n";
                        error += "Zero rated vat must include TaxExemptionReasonCode";
                    }
                    if (!string.IsNullOrEmpty(sub.taxCategory.TaxExemptionReasonCode))
                    {
                        subtotaltext.Append("<cbc:TaxExemptionReasonCode>" + sub.taxCategory.TaxExemptionReasonCode + "</cbc:TaxExemptionReasonCode>");

                    }
                    if (!string.IsNullOrEmpty(sub.taxCategory.TaxExemptionReason))
                    {
                        subtotaltext.Append("<cbc:TaxExemptionReason>" + Utility.ReplaceXMLSpecialCharacters(sub.taxCategory.TaxExemptionReason) + "</cbc:TaxExemptionReason>");

                    }
                    else
                    {
                        error += "\n";
                        error += "Tax must include TaxExemptionReason";
                    }
                }
                subtotaltext.Append("<cac:TaxScheme>" +
                        "<cbc:ID>VAT</cbc:ID>" +
                        "</cac:TaxScheme>" +
                        "</cac:TaxCategory>");
                subtotaltext.Append("</cac:TaxSubtotal>");
            }
            taxtotalamount = subtaxkeys.Values.Sum(x => x.TaxAmount);
            decimal currencyrate = 1;
            if (inv.DocumentCurrencyCode != "SAR")
            {
                currencyrate = inv.CurrencyRate;
            }
            sb.Append("<cac:TaxTotal>" +
            "<cbc:TaxAmount currencyID='" + inv.TaxCurrencyCode + "'>" + Math.Round(taxtotalamount * currencyrate, 2, MidpointRounding.AwayFromZero).ToString("0.00") + "</cbc:TaxAmount>" +
            "</cac:TaxTotal>");

            sb.Append("<cac:TaxTotal>");
            sb.Append("<cbc:TaxAmount currencyID='" + inv.DocumentCurrencyCode + "'>" + Math.Round(taxtotalamount, 2, MidpointRounding.AwayFromZero).ToString("0.00") + "</cbc:TaxAmount>");
            sb.Append(subtotaltext);

            sb.Append("</cac:TaxTotal>");
            

        }
        private void GetInvoiceLineElement(Invoice inv, StringBuilder sb)
        {

            int count = 0;
            foreach (InvoiceLine invline in inv.InvoiceLines)
            {
                count = count + 1;
                bool AdditionalInvoiceLine = false;
                if (inv.legalMonetaryTotal.PrepaidAmount > 0 && invline.documentReferences.Count > 0)
                {
                    AdditionalInvoiceLine = true;
                    invline.LineExtensionAmount = 0;
                    invline.taxTotal.TaxAmount = 0;
                    invline.taxTotal.RoundingAmount = 0;
                    invline.price.PriceAmount = 0;
                }
                sb.Append("<cac:InvoiceLine>");
                if (!string.IsNullOrEmpty(invline.ID))
                {
                    sb.Append("<cbc:ID>" + Utility.ReplaceXMLSpecialCharacters(invline.ID.Trim()) + "</cbc:ID>");
                }
                else
                {
                    invline.ID = count.ToString();
                    sb.Append("<cbc:ID>" + invline.ID + "</cbc:ID>");

                }
                sb.Append("<cbc:InvoicedQuantity>" + invline.InvoiceQuantity + "</cbc:InvoicedQuantity>");
                sb.Append("<cbc:LineExtensionAmount currencyID='" + inv.DocumentCurrencyCode + "'>" + invline.LineExtensionAmount.ToString("0.00") + "</cbc:LineExtensionAmount>");
                if (AdditionalInvoiceLine)
                {
                    //prepare document referance tag
                    sb.Append(GetDocumentReferences(invline.documentReferences));
                    sb.Append(GetInvoiceLineTaxTotalPrePaid(invline, inv.DocumentCurrencyCode));
                }
                else
                {
                    GetIvoiceLineallowanceChargeElement(invline, inv.DocumentCurrencyCode, sb);
                    sb.Append(GetInvoiceLineTaxTotal(invline, inv.DocumentCurrencyCode));
                }
                sb.Append(GetInvoiceLineItemElement(invline));
                sb.Append(GetInvoiceLinePriceElement(invline, inv.DocumentCurrencyCode));
                sb.Append("</cac:InvoiceLine>");
            }

        }
        private StringBuilder GetDocumentReferences(DocumentReferenceCollection documentReferences)
        {
            StringBuilder referencestag = new StringBuilder();

            foreach (DocumentReference doc in documentReferences)
            {
                referencestag.Append("<cac:DocumentReference>");
                referencestag.Append("<cbc:ID>" + doc.ID + "</cbc:ID>");
                if (!string.IsNullOrEmpty(doc.UUID))
                {
                    referencestag.Append("<cbc:UUID>" + doc.UUID + "</cbc:UUID>");
                }
                referencestag.Append("<cbc:IssueDate>" + doc.IssueDate + "</cbc:IssueDate>");
                referencestag.Append("<cbc:IssueTime>" + doc.IssueTime + "</cbc:IssueTime>");
                if (doc.DocumentTypeCode == 0)
                {
                    doc.DocumentTypeCode = 386;
                }
                referencestag.Append("<cbc:DocumentTypeCode>" + doc.DocumentTypeCode + "</cbc:DocumentTypeCode>");
                referencestag.Append("</cac:DocumentReference>");
            }

            return referencestag;
        }
        private StringBuilder GetInvoiceLineItemElement(InvoiceLine invline)
        {
            StringBuilder item = new StringBuilder();
            item.Append("<cac:Item>");
            if (!string.IsNullOrEmpty(invline.item.Name))
            {
                item.Append("<cbc:Name>" + Utility.ReplaceXMLSpecialCharacters(invline.item.Name) + "</cbc:Name>");
            }
            if (!string.IsNullOrEmpty(invline.item.BuyersItemIdentificationID))
            {
                item.Append("<cac:BuyersItemIdentification>" +
                 "<cbc:ID>" + Utility.ReplaceXMLSpecialCharacters(invline.item.BuyersItemIdentificationID) + "</cbc:ID>" +
                 "</cac:BuyersItemIdentification>");
            }
            if (!string.IsNullOrEmpty(invline.item.SellersItemIdentificationID))
            {
                item.Append("<cac:SellersItemIdentification>" +
                 "<cbc:ID>" + Utility.ReplaceXMLSpecialCharacters(invline.item.SellersItemIdentificationID) + "</cbc:ID>" +
                 "</cac:SellersItemIdentification>");
            }
            if (!string.IsNullOrEmpty(invline.item.StandardItemIdentificationID))
            {
                item.Append("<cac:StandardItemIdentification>" +
                 "<cbc:ID>" + Utility.ReplaceXMLSpecialCharacters(invline.item.StandardItemIdentificationID) + "</cbc:ID>" +
                 "</cac:StandardItemIdentification>");
            }
            item.Append("<cac:ClassifiedTaxCategory>" +
                "<cbc:ID>" + invline.item.classifiedTaxCategory.ID + "</cbc:ID>" +
                "<cbc:Percent>" + invline.item.classifiedTaxCategory.Percent.ToString("0.00") + "</cbc:Percent>" +
                "<cac:TaxScheme>" +
                "<cbc:ID>VAT</cbc:ID>" +
                "</cac:TaxScheme>" +
                "</cac:ClassifiedTaxCategory>");
            item.Append("</cac:Item>");
            return item;
        }
        private StringBuilder GetInvoiceLinePriceElement(InvoiceLine invline, string DocumentCurrencyCode)
        {
            StringBuilder pricetxt = new StringBuilder();
            pricetxt.Append("<cac:Price>");
            pricetxt.Append("<cbc:PriceAmount currencyID='" + DocumentCurrencyCode + "'>" + invline.price.PriceAmount + "</cbc:PriceAmount>");
            if (invline.price.BaseQuantity > 0)
            {
                pricetxt.Append("<cbc:BaseQuantity>" + invline.price.BaseQuantity.ToString("0.00") + "</cbc:BaseQuantity>");
            }
            if (invline.price.allowanceCharge.Amount > 0)
            {
                pricetxt.Append("<cac:AllowanceCharge>" +
                        "<cbc:ChargeIndicator>false</cbc:ChargeIndicator>");
                if (!string.IsNullOrEmpty(invline.price.allowanceCharge.AllowanceChargeReason))
                {
                    pricetxt.Append("<cbc:AllowanceChargeReason>" + Utility.ReplaceXMLSpecialCharacters(invline.price.allowanceCharge.AllowanceChargeReason) + "</cbc:AllowanceChargeReason>");
                }
                pricetxt.Append("<cbc:Amount currencyID='" + DocumentCurrencyCode + "'>" + invline.price.allowanceCharge.Amount.ToString("0.00") + "</cbc:Amount>" +
                        "</cac:AllowanceCharge>");
            }

            pricetxt.Append("</cac:Price>");
            return pricetxt;
        }
        private void GetPaymentMeansElement(Invoice inv, StringBuilder sb, ref string error)
        {
            bool issimplified = inv.invoiceTypeCode.Name.Substring(0, 2) == "02";
            foreach (var paymentmean in inv.paymentmeans)
            {
                //if (inv.invoiceTypeCode.id == 388 && (string.IsNullOrEmpty(paymentmean.PaymentMeansCode) || string.IsNullOrEmpty(paymentmean.InstructionNote)))
                //{
                //	continue; 
                //}
                if (inv.invoiceTypeCode.id == 388 && string.IsNullOrEmpty(paymentmean.PaymentMeansCode) && string.IsNullOrEmpty(paymentmean.InstructionNote))
                {
                    continue;
                }
                if ((inv.invoiceTypeCode.id == 383 || inv.invoiceTypeCode.id == 381) && string.IsNullOrEmpty(paymentmean.InstructionNote))
                {
                    error += "\n";
                    error += "Credit or Debit Note must has PaymentMeans InstructionNote.";
                }
                else
                {
                    sb.Append("<cac:PaymentMeans>");
                    if (!string.IsNullOrEmpty(paymentmean.PaymentMeansCode))
                    {
                        sb.Append("<cbc:PaymentMeansCode>" + paymentmean.PaymentMeansCode + "</cbc:PaymentMeansCode>");
                    }
                    if (!string.IsNullOrEmpty(paymentmean.InstructionNote))
                    {
                        sb.Append("<cbc:InstructionNote>" + Utility.ReplaceXMLSpecialCharacters(paymentmean.InstructionNote) + "</cbc:InstructionNote>");
                    }
                    if (!string.IsNullOrEmpty(paymentmean.payeefinancialaccount.ID) || !string.IsNullOrEmpty(paymentmean.payeefinancialaccount.paymentnote))
                    {
                        sb.Append("<cac:PayeeFinancialAccount>");
                        if (!string.IsNullOrEmpty(paymentmean.payeefinancialaccount.ID))
                            sb.Append("<cbc:ID>" + paymentmean.payeefinancialaccount.ID + "</cbc:ID>");
                        if (!string.IsNullOrEmpty(paymentmean.payeefinancialaccount.paymentnote))
                            sb.Append("<cbc:PaymentNote>" + paymentmean.payeefinancialaccount.paymentnote + "</cbc:PaymentNote>");
                        sb.Append("</cac:PayeeFinancialAccount>");
                    }
                    sb.Append("</cac:PaymentMeans>");

                }
            }




        }
        private StringBuilder GetInvoiceLineTaxTotal(InvoiceLine invline, string DocumentCurrencyCode)
        {
            StringBuilder taxtotal = new StringBuilder();
            decimal taxamount = Math.Round(invline.taxTotal.TaxAmount, 2, MidpointRounding.AwayFromZero);
            decimal roundingamount = invline.LineExtensionAmount + taxamount;
            invline.taxTotal.RoundingAmount= roundingamount;
            taxtotal.Append("<cac:TaxTotal>" +
                "<cbc:TaxAmount currencyID='" + DocumentCurrencyCode + "'>" + taxamount.ToString("0.00") + "</cbc:TaxAmount>" +
                "<cbc:RoundingAmount currencyID='" + DocumentCurrencyCode + "'>" + roundingamount.ToString("0.00") + "</cbc:RoundingAmount>" +
                "</cac:TaxTotal>");

            return taxtotal;
        }
        private StringBuilder GetInvoiceLineTaxTotalPrePaid(InvoiceLine invline, string DocumentCurrencyCode)
        {
            
            StringBuilder taxtotal = new StringBuilder();
            decimal taxamount = Math.Round(invline.taxTotal.TaxAmount, 2, MidpointRounding.AwayFromZero);
            decimal roundingamount = invline.LineExtensionAmount + taxamount;
            invline.taxTotal.RoundingAmount = roundingamount;
            taxtotal.Append("<cac:TaxTotal>");
            taxtotal.Append("<cbc:TaxAmount currencyID='" + DocumentCurrencyCode + "'>" + taxamount.ToString("0.00") + "</cbc:TaxAmount>");
            taxtotal.Append("<cbc:RoundingAmount currencyID='" + DocumentCurrencyCode + "'>" + invline.taxTotal.RoundingAmount.ToString("0.00") + "</cbc:RoundingAmount>");
            taxtotal.Append("<cac:TaxSubtotal>");
            taxtotal.Append("<cbc:TaxableAmount currencyID='" + DocumentCurrencyCode + "'>" + invline.taxTotal.TaxSubtotal.TaxableAmount.ToString("0.00") + "</cbc:TaxableAmount>");
            taxtotal.Append("<cbc:TaxAmount currencyID='" + DocumentCurrencyCode + "'>" + invline.taxTotal.TaxSubtotal.TaxAmount.ToString("0.00") + "</cbc:TaxAmount>");
            taxtotal.Append("<cac:TaxCategory>");
            taxtotal.Append("<cbc:ID>" + invline.taxTotal.TaxSubtotal.taxCategory.ID + "</cbc:ID>");
            taxtotal.Append("<cbc:Percent>" + invline.taxTotal.TaxSubtotal.taxCategory.Percent.ToString("0.00") + "</cbc:Percent>");
            taxtotal.Append("<cac:TaxScheme>");
            taxtotal.Append("<cbc:ID>VAT</cbc:ID>");
            taxtotal.Append("</cac:TaxScheme>");
            taxtotal.Append("</cac:TaxCategory>");
            taxtotal.Append("</cac:TaxSubtotal>");
            taxtotal.Append("</cac:TaxTotal>");

            return taxtotal;
        }
        public InvoiceTotal CalculateInvoiceTotal(InvoiceLineCollection invoiceLines, AllowanceChargeCollection allowanceCharges)
        {
            InvoiceTotal invoiceTotal = new InvoiceTotal();
            try
            {
                decimal totallineextensionamount = 0;
                decimal invoicetotalvatamount = 0;

                foreach (InvoiceLine l in invoiceLines)
                {
                    decimal itemprice = 0;
                    decimal lineextensionamount = 0;
                    decimal roundingamount = 0;

                    //get original item price
                    if (l.price.EncludingVat)
                    {
                        //itemprice = Math.Round((l.price.PriceAmount / ((l.taxTotal.TaxSubtotal.taxCategory.Percent / 100) + 1)), 2, MidpointRounding.AwayFromZero);
                        itemprice = (l.price.PriceAmount / ((l.taxTotal.TaxSubtotal.taxCategory.Percent / 100) + 1));
                    }
                    else
                    {
                        //itemprice = Math.Round(l.price.PriceAmount, 2, MidpointRounding.AwayFromZero);
                        itemprice = l.price.PriceAmount;
                    }

                    decimal allowanceamount = 0;
                    decimal chargeamount = 0;
                    foreach (var allowancecharge in l.allowanceCharges)
                    {
                        if (allowancecharge.Amount > 0)
                        {

                            if (allowancecharge.ChargeIndicator)
                                chargeamount += allowancecharge.Amount;
                            else
                                allowanceamount += allowancecharge.Amount;
                        }
                        else if (allowancecharge.MultiplierFactorNumeric > 0 && allowancecharge.BaseAmount > 0)
                        {
                            if (allowancecharge.ChargeIndicator)
                                chargeamount += (allowancecharge.MultiplierFactorNumeric / 100) * allowancecharge.BaseAmount;
                            else
                                allowanceamount += (allowancecharge.MultiplierFactorNumeric / 100) * allowancecharge.BaseAmount;
                        }

                    }
                    if (l.price.BaseQuantity == 0)
                    {
                        l.price.BaseQuantity = 1;
                    }
                    decimal invoicelinenetamount = (l.InvoiceQuantity * (l.price.PriceAmount / l.price.BaseQuantity)) + chargeamount - allowanceamount;
                    decimal taxamount = invoicelinenetamount * (l.taxTotal.TaxSubtotal.taxCategory.Percent / 100);
                    lineextensionamount = Math.Round(invoicelinenetamount, 2, MidpointRounding.AwayFromZero);

                    roundingamount = lineextensionamount + Math.Round(taxamount, 2, MidpointRounding.AwayFromZero);
                    invoicetotalvatamount += taxamount;
                    totallineextensionamount += lineextensionamount;
                    invoiceTotal.InvoiceLines.Add(l);
                }
                decimal allowancetotalamount = 0;
                decimal allowancechargetotalvat = 0;
                decimal chargetotalamount = 0;
                decimal chargetotalvat = 0;
                foreach (AllowanceCharge allowance in allowanceCharges)
                {
                    if (allowance.Amount > 0)
                    {
                        allowance.Amount = Math.Round(allowance.Amount, 2, MidpointRounding.AwayFromZero);
                        if (allowance.ChargeIndicator)
                        {
                            chargetotalamount += allowance.Amount;
                            // get allowance  vat 
                            chargetotalvat += allowance.Amount * (allowance.taxCategory.Percent / 100);
                        }
                        else
                        {
                            allowancetotalamount += allowance.Amount;
                            // get allowance  vat 
                            allowancechargetotalvat += allowance.Amount * (allowance.taxCategory.Percent / 100);
                        }
                    }
                }
                invoiceTotal.LineExtensionAmount = totallineextensionamount;
                invoiceTotal.AllowanceTotalAmount = allowancetotalamount;
                invoiceTotal.ChargeTotalAmount = chargetotalamount;
                decimal taxexclusiveamount = totallineextensionamount + chargetotalamount - allowancetotalamount;
                decimal taxinclusiveamount = taxexclusiveamount + invoicetotalvatamount + chargetotalvat - allowancechargetotalvat;
                invoiceTotal.TaxExclusiveAmount = Math.Round(taxexclusiveamount, 2, MidpointRounding.AwayFromZero);
                invoiceTotal.TaxInclusiveAmount = Math.Round(taxinclusiveamount, 2, MidpointRounding.AwayFromZero);
            }
            catch
            {

            }
            return invoiceTotal;
        }
        private void RepairTaxCategory(InvoiceLine invline)
        {
            try
            {
                //tax category code
                if (string.IsNullOrEmpty(invline.item.classifiedTaxCategory.ID.Trim()))
                {
                    invline.item.classifiedTaxCategory.ID = invline.taxTotal.TaxSubtotal.taxCategory.ID;
                }
                if (string.IsNullOrEmpty(invline.taxTotal.TaxSubtotal.taxCategory.ID.Trim()))
                {
                    invline.taxTotal.TaxSubtotal.taxCategory.ID = invline.item.classifiedTaxCategory.ID;
                }
                // tax percentage
                if (invline.item.classifiedTaxCategory.Percent == 0)
                {
                    invline.item.classifiedTaxCategory.Percent = invline.taxTotal.TaxSubtotal.taxCategory.Percent;
                }
                if (invline.taxTotal.TaxSubtotal.taxCategory.Percent == 0)
                {
                    invline.taxTotal.TaxSubtotal.taxCategory.Percent = invline.item.classifiedTaxCategory.Percent;
                }
                invline.taxTotal.TaxSubtotal.taxCategory.ID = invline.taxTotal.TaxSubtotal.taxCategory.ID.Trim().ToUpper();
                invline.item.classifiedTaxCategory.ID = invline.item.classifiedTaxCategory.ID.Trim().ToUpper();
            }
            catch
            {

            }

        }

        public string ConvertXMLToJSON(string xml)
        {
            string json = "";
            try
            {
                xml = Utility.ApplyXSLTPassingXML(xml, SettingsParams.Embeded_Remove_Elements_PATH);
                xml = Utility.RemoveAllNamespaces(xml);
            }
            catch (Exception)
            {
                return "Error in removing elements.";
            }
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(xml);
            }
            catch
            {
                return "Can not load XML file";
            }
             json = JsonConvert.SerializeXmlNode(doc,Newtonsoft.Json.Formatting.Indented,true);

            return json;
        }
        #endregion
        ~UBLXML()
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
