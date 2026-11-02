namespace ZatcaIntegrationSDK
{


	public static class SettingsParams
	{
		public static string SELLER_NAME_XPATH = "/*[local-name() = 'Invoice']/*[local-name() = 'AccountingSupplierParty']/*[local-name() = 'Party']/*[local-name() = 'PartyLegalEntity']//*[local-name() = 'RegistrationName']";

		public static string VAT_REGISTERATION_XPATH = "/*[local-name() = 'Invoice']/*[local-name() = 'AccountingSupplierParty']/*[local-name() = 'Party']/*[local-name() = 'PartyTaxScheme']/*[local-name() = 'CompanyID']";

		public static string ISSUE_DATE_XPATH = "/*[local-name() = 'Invoice']/*[local-name() = 'IssueDate']";

		public static string ISSUE_TIME_XPATH = "/*[local-name() = 'Invoice']/*[local-name() = 'IssueTime']";

		public static string INVOICE_TOTAL_XPATH = "/*[local-name() = 'Invoice']/*[local-name() = 'LegalMonetaryTotal']/*[local-name() = 'TaxInclusiveAmount']";

		public static string VAT_TOTAL_XPATH = "/*[local-name() = 'Invoice']/*[local-name() = 'TaxTotal']/*[local-name() = 'TaxAmount']";

		public static string SIGNATURE_XPATH = "/*[local-name() = 'Invoice']/*[local-name() = 'UBLExtensions']/*[local-name() = 'UBLExtension']/*[local-name() = 'ExtensionContent']/*[local-name() = 'UBLDocumentSignatures']/*[local-name() = 'SignatureInformation']/*[local-name() = 'Signature']/*[local-name() = 'SignatureValue']";

		public static string CERTIFICATE_XPATH = "/*[local-name() = 'Invoice']/*[local-name() = 'UBLExtensions']/*[local-name() = 'UBLExtension']/*[local-name() = 'ExtensionContent']/*[local-name() = 'UBLDocumentSignatures']/*[local-name() = 'SignatureInformation']/*[local-name() = 'Signature']/*[local-name() = 'KeyInfo']/*[local-name() = 'X509Data']/*[local-name() = 'X509Certificate']";

		public static string QR_CODE_XPATH = "/*[local-name() = 'Invoice']/*[local-name() = 'AdditionalDocumentReference' and *[local-name()='ID' and .='QR']]/*[local-name() = 'Attachment']/*[local-name() = 'EmbeddedDocumentBinaryObject']";

		public static string Hash_XPATH = "/*[local-name() = 'Invoice']/*[local-name() = 'UBLExtensions']/*[local-name() = 'UBLExtension']/*[local-name() = 'ExtensionContent']/*[local-name() = 'UBLDocumentSignatures']/*[local-name() = 'SignatureInformation']/*[local-name() = 'Signature']/*[local-name() = 'SignedInfo']/*[local-name() = 'Reference' and @Id='invoiceSignedData']/*[local-name() = 'DigestValue']";

		public static string Invoice_Type_XPATH = "//*[local-name()='Invoice']//*[local-name()='InvoiceTypeCode']";

		public static string PIH_XPATH = "/*[local-name() = 'Invoice']/*[local-name() = 'AdditionalDocumentReference' and *[local-name()='ID' and .='PIH']]/*[local-name() = 'Attachment']/*[local-name() = 'EmbeddedDocumentBinaryObject']";

		public static string SIGNED_PROPERTIES_XPATH = "//*[local-name()='Invoice']//*[local-name()='UBLExtensions']//*[local-name()='ExtensionContent']//*[local-name()='UBLDocumentSignatures']//*[local-name()='SignatureInformation']//*[local-name()='Signature']//*[local-name()='QualifyingProperties']//*[local-name()='SignedProperties']";

		public static string SIGNED_Properities_DIGEST_VALUE_XPATH = "//*[local-name()='Invoice']//*[local-name()='UBLExtensions']//*[local-name()='ExtensionContent']//*[local-name()='UBLDocumentSignatures']//*[local-name()='SignatureInformation']//*[local-name()='Signature']//*[local-name()='SignedInfo']//*[local-name()='Reference'][2]//*[local-name()='DigestValue']";

		public static string SIGNED_Certificate_DIGEST_VALUE_XPATH = "//*[local-name()='Invoice']//*[local-name()='UBLExtensions']//*[local-name()='UBLExtension']//*[local-name()='ExtensionContent']//*[local-name()='UBLDocumentSignatures']//*[local-name()='SignatureInformation']//*[local-name()='Signature']//*[local-name()='Object']//*[local-name()='QualifyingProperties']//*[local-name()='SignedProperties']//*[local-name()='SignedSignatureProperties']//*[local-name()='SigningCertificate']//*[local-name()='Cert']//*[local-name()='CertDigest']//*[local-name()='DigestValue']";

		public static string X509_SERIAL_NUMBER_XPATH = "//*[local-name()='Invoice']//*[local-name()='UBLExtensions']//*[local-name()='UBLExtension']//*[local-name()='ExtensionContent']//*[local-name()='UBLDocumentSignatures']//*[local-name()='SignatureInformation']//*[local-name()='Signature']//*[local-name()='Object']//*[local-name()='QualifyingProperties']//*[local-name()='SignedProperties']//*[local-name()='SignedSignatureProperties']//*[local-name()='SigningCertificate']//*[local-name()='Cert']//*[local-name()='IssuerSerial']//*[local-name()='X509SerialNumber']";

		public static string ISSUER_NAME_XPATH = "//*[local-name()='Invoice']//*[local-name()='UBLExtensions']//*[local-name()='UBLExtension']//*[local-name()='ExtensionContent']//*[local-name()='UBLDocumentSignatures']//*[local-name()='SignatureInformation']//*[local-name()='Signature']//*[local-name()='Object']//*[local-name()='QualifyingProperties']//*[local-name()='SignedProperties']//*[local-name()='SignedSignatureProperties']//*[local-name()='SigningCertificate']//*[local-name()='Cert']//*[local-name()='IssuerSerial']//*[local-name()='X509IssuerName']";

		public static string PUBLIC_KEY_HASHING_XPATH = "//*[local-name()='Invoice']//*[local-name()='UBLExtensions']//*[local-name()='ExtensionContent']//*[local-name()='UBLDocumentSignatures']//*[local-name()='SignatureInformation']//*[local-name()='Signature']//*[local-name()='QualifyingProperties']//*[local-name()='SignedProperties']//*[local-name()='SignedSignatureProperties']//*[local-name()='SigningCertificate']//*[local-name()='Cert']//*[local-name()='CertDigest']//*[local-name()='DigestValue']";

		public static string SIGNING_TIME_XPATH = "//*[local-name()='Invoice']//*[local-name()='UBLExtensions']//*[local-name()='ExtensionContent']//*[local-name()='UBLDocumentSignatures']//*[local-name()='SignatureInformation']//*[local-name()='Signature']//*[local-name()='QualifyingProperties']//*[local-name()='SignedProperties']//*[local-name()='SignedSignatureProperties']//*[local-name()='SigningTime']";

		public static string UUID_XPATH = "/*[local-name() = 'Invoice']/*[local-name() = 'UUID']";
        public static string INVOICE_ID_XPATH = "/*[local-name() = 'Invoice']/*[local-name() = 'ID']";

        public static string[] allDatesFormats = new string[19]
		{
		"yyyy-MM-dd", "yyyy/MM/dd", "yyyy/M/d", "dd/MM/yyyy", "d/M/yyyy", "dd/M/yyyy", "d/MM/yyyy", "yyyy-MM-dd", "yyyy-M-d", "dd-MM-yyyy",
		"d-M-yyyy", "dd-M-yyyy", "d-MM-yyyy", "yyyy MM dd", "yyyy M d", "dd MM yyyy", "d M yyyy", "dd M yyyy", "d MM yyyy"
		};

		public static string Embeded_InvoiceXSLFileForFormatting = "ZatcaIntegrationSDK.Data.format.xsl";

		public static string Embeded_InvoiceXSLFileForHashing = "ZatcaIntegrationSDK.Data.invoice.xsl";

		public static string Embeded_EN_Schematrons_PATH = "ZatcaIntegrationSDK.Data.CEN-EN16931-UBL.xsl";

		public static string Embeded_KSA_Schematrons_PATH = "ZatcaIntegrationSDK.Data.ZATCA_Validation_Rules.xsl";
		//public static string Embeded_EN_Schematrons_PATH
		//{
		//	get
		//	{
		//		string ENSchematronsPath = @"Data\CEN-EN16931-UBL.xsl";

		//		return ENSchematronsPath;
		//	}
		//}

		//public static string Embeded_KSA_Schematrons_PATH
		//{
		//	get
		//	{
		//		string KSASchematronsPath = @"Data\ZATCA_Validation_Rules.xsl";

		//		return KSASchematronsPath;
		//	}
		//}

		public static string Embeded_Remove_Elements_PATH = "ZatcaIntegrationSDK.Data.removeElements.xsl";

		public static string Embeded_Add_QR_Element_PATH = "ZatcaIntegrationSDK.Data.addQRElement.xsl";

		public static string Embeded_Add_Signature_Element_PATH = "ZatcaIntegrationSDK.Data.addSignatureElement.xsl";

		public static string Embeded_Add_UBL_Element_PATH = "ZatcaIntegrationSDK.Data.addUBLElement.xsl";

		public static string Embeded_QR_XML_File_PATH = "ZatcaIntegrationSDK.Data.qr.xml";

		public static string Embeded_Signature_File_PATH = "ZatcaIntegrationSDK.Data.signature.xml";

		public static string Embeded_UBL_File_PATH = "ZatcaIntegrationSDK.Data.ubl.xml";

		public static string Embeded_License_Config_File_PATH = "ZatcaIntegrationSDK.Data.config.xsd";

		public static string Embeded_UBL_XSD_PATH = "ZatcaIntegrationSDK.Data.xsd.maindoc.UBL-Invoice-2.1.xsd";

		public static string Embeded_CommonAggregateComponents_XSD_PATH = "ZatcaIntegrationSDK.Data.xsd.common.UBL-CommonAggregateComponents-2.1.xsd";

		public static string Embeded_CommonBasicComponents_XSD_PATH = "ZatcaIntegrationSDK.Data.xsd.common.UBL-CommonBasicComponents-2.1.xsd";

		public static string Embeded_CommonExtensionComponents_XSD_PATH = "ZatcaIntegrationSDK.Data.xsd.common.UBL-CommonExtensionComponents-2.1.xsd";

		public static string Embeded_CommonSignatureComponents_XSD_PATH = "ZatcaIntegrationSDK.Data.xsd.common.UBL-CommonSignatureComponents-2.1.xsd";

		public static string Embeded_SignatureAggregateComponents_XSD_PATH = "ZatcaIntegrationSDK.Data.xsd.common.UBL-SignatureAggregateComponents-2.1.xsd";

		public static string Embeded_SignatureBasicComponents_XSD_PATH = "ZatcaIntegrationSDK.Data.xsd.common.UBL-SignatureBasicComponents-2.1.xsd";

		public static string Embeded_UBL_UnqualifiedDataTypes_XSD_PATH = "ZatcaIntegrationSDK.Data.xsd.common.UBL-UnqualifiedDataTypes-2.1.xsd";

		public static string Embeded_CoreComponentTypeSchemaModule_XSD_PATH = "ZatcaIntegrationSDK.Data.xsd.common.UBL-CoreComponentParameters-2.1.xsd";

		public static string Embeded_ExtensionContentDataType_XSD_PATH = "ZatcaIntegrationSDK.Data.xsd.common.UBL-ExtensionContentDataType-2.1.xsd";

		public static string Embeded_QualifiedDataTypes_XSD_PATH = "ZatcaIntegrationSDK.Data.xsd.common.UBL-QualifiedDataTypes-2.1.xsd";

		public static string Embeded_CCTS_CCT_SchemaModule_XSD_PATH = "ZatcaIntegrationSDK.Data.xsd.common.CCTS_CCT_SchemaModule-2.1.xsd";

		public static string Embeded_UBL_XAdESv_XSD_PATH = "ZatcaIntegrationSDK.Data.xsd.common.UBL-XAdESv141-2.1.xsd";

		public static string Embeded_UBL_Xmldsig_Core_XSD_PATH = "ZatcaIntegrationSDK.Data.xsd.common.UBL-xmldsig-core-schema-2.1.xsd";
		public static string LineExtensionAmount = "/*[local-name() = 'Invoice']/*[local-name() = 'LegalMonetaryTotal']/*[local-name() = 'LineExtensionAmount']";
		public static string TaxExclusiveAmount = "/*[local-name() = 'Invoice']/*[local-name() = 'LegalMonetaryTotal']/*[local-name() = 'TaxExclusiveAmount']";
		public static string TaxInclusiveAmount = "/*[local-name() = 'Invoice']/*[local-name() = 'LegalMonetaryTotal']/*[local-name() = 'TaxInclusiveAmount']";
		public static string AllowanceTotalAmount = "/*[local-name() = 'Invoice']/*[local-name() = 'LegalMonetaryTotal']/*[local-name() = 'AllowanceTotalAmount']";
		public static string ChargeTotalAmount = "/*[local-name() = 'Invoice']/*[local-name() = 'LegalMonetaryTotal']/*[local-name() = 'ChargeTotalAmount']";
		public static string PayableAmount = "/*[local-name() = 'Invoice']/*[local-name() = 'LegalMonetaryTotal']/*[local-name() = 'PayableAmount']";
		public static string PrepaidAmount = "/*[local-name() = 'Invoice']/*[local-name() = 'LegalMonetaryTotal']/*[local-name() = 'PrepaidAmount']";
        public static string PayableRoundingAmount = "/*[local-name() = 'Invoice']/*[local-name() = 'LegalMonetaryTotal']/*[local-name() = 'PayableRoundingAmount']";
        public static string NormalStandardInvoicePath = "\\Invoices\\Normal\\Standard\\Invoices\\";
		public static string NormalStandardDebitPath = "\\Invoices\\Normal\\Standard\\Debit\\";
		public static string NormalStandardCreditPath = "\\Invoices\\Normal\\Standard\\Credit\\";
		public static string NormalSimplifiedInvoicePath = "\\Invoices\\Normal\\Simplified\\Invoices\\";
		public static string NormalSimplifiedDebitPath = "\\Invoices\\Normal\\Simplified\\Debit\\";
		public static string NormalSimplifiedCreditPath = "\\Invoices\\Normal\\Simplified\\Credit\\";

		public static string StandardInvoicePath = "\\Invoices\\Signed\\Standard\\Invoices\\";
		public static string StandardDebitPath = "\\Invoices\\Signed\\Standard\\Debit\\";
		public static string StandardCreditPath = "\\Invoices\\Signed\\Standard\\Credit\\";
		public static string SimplifiedInvoicePath = "\\Invoices\\Signed\\Simplified\\Invoices\\";
		public static string SimplifiedDebitPath = "\\Invoices\\Signed\\Simplified\\Debit\\";
		public static string SimplifiedCreditPath = "\\Invoices\\Signed\\Simplified\\Credit\\";

        public static string StandardInvoiceFromZatcaPath = "\\Invoices\\Cleared\\Standard\\Invoices\\";
        public static string StandardDebitFromZatcaPath = "\\Invoices\\Cleared\\Standard\\Debit\\";
        public static string StandardCreditFromZatcaPath = "\\Invoices\\Cleared\\Standard\\Credit\\";
        public static string SimplifiedInvoiceFromZatcaPath = "\\Invoices\\Reported\\Simplified\\Invoices\\";
        public static string SimplifiedDebitFromZatcaPath = "\\Invoices\\Reported\\Simplified\\Debit\\";
        public static string SimplifiedCreditFromZatcaPath = "\\Invoices\\Reported\\Simplified\\Credit\\";
    }

}