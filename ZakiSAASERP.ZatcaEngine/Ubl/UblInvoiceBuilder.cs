using ZakiSAASERP.ZatcaEngine.Core;
using ZakiSAASERP.ZatcaEngine.Enums;
using ZakiSAASERP.ZatcaEngine.Models;
using System.Xml.Linq;

namespace ZakiSAASERP.ZatcaEngine.Ubl;

public class UblInvoiceBuilder
{
    public string BuildInvoice(Invoice invoice, InvoiceType type)
    {
        var cac = UblNamespaces.cac;
        var cbc = UblNamespaces.cbc;
        var ext = UblNamespaces.ext;

        string invoiceTypeCode =
            InvoiceTypeMapper.GetUblCode(type);

        bool isStandard =
            InvoiceTypeMapper.RequiresClearance(type);

        var issueDate = invoice.IssueDate.Year >= 2000 ? invoice.IssueDate : DateTime.Now;

        decimal lineExtensionTotal = invoice.Lines?.Sum(l => l.LineTotal) ?? 0;
        decimal allowanceTotal = invoice.Allowances?.Where(a => !a.ChargeIndicator).Sum(a => a.Amount) ?? 0;
        decimal chargeTotal = invoice.Allowances?.Where(a => a.ChargeIndicator).Sum(a => a.Amount) ?? 0;
        decimal taxExclusiveAmount = lineExtensionTotal - allowanceTotal + chargeTotal;
        decimal taxInclusiveAmount = taxExclusiveAmount + invoice.VatAmount;

        var doc = new XDocument(
            new XElement(UblNamespaces.invoice + "Invoice",
                new XAttribute(XNamespace.Xmlns + "cac", cac),
                new XAttribute(XNamespace.Xmlns + "cbc", cbc),
                new XAttribute(XNamespace.Xmlns + "ext", ext),

                // =============================
                // UBLExtensions
                // =============================
                new XElement(ext + "UBLExtensions",
                    new XElement(ext + "UBLExtension",
                        new XElement(ext + "ExtensionURI", "urn:oasis:names:specification:ubl:dsig:enveloped:xades"),
                        new XElement(ext + "ExtensionContent"))),

                new XElement(cbc + "ProfileID", "reporting:1.0"),
                new XElement(cbc + "ID", invoice.InvoiceNumber),
                new XElement(cbc + "UUID", string.IsNullOrWhiteSpace(invoice.Uuid) ? Guid.NewGuid().ToString() : invoice.Uuid),
                new XElement(cbc + "IssueDate",
                    issueDate.ToString("yyyy-MM-dd")),
                new XElement(cbc + "IssueTime",
                    issueDate.ToString("HH:mm:ss")),
                new XElement(cbc + "InvoiceTypeCode",
                    new XAttribute("name", !string.IsNullOrEmpty(invoice.InvoiceSubType) ? invoice.InvoiceSubType : (isStandard ? "0100000" : "0200000")),
                    invoiceTypeCode),
                new XElement(cbc + "DocumentCurrencyCode", "SAR"),
                new XElement(cbc + "TaxCurrencyCode", "SAR"),
                new XElement(cbc + "LineCountNumeric", invoice.Lines?.Count ?? 0),

                // =============================
                // Billing Reference (Mandatory for Credit Note 381 & Debit Note 383 per BR-KSA-56)
                // =============================
                !string.IsNullOrWhiteSpace(invoice.BillingReferenceId) ?
                    new XElement(cac + "BillingReference",
                        new XElement(cac + "InvoiceDocumentReference",
                            new XElement(cbc + "ID", invoice.BillingReferenceId.Trim()))) : null,

                // =============================
                // Additional Document Reference (ICV)
                // =============================
                new XElement(cac + "AdditionalDocumentReference",
                    new XElement(cbc + "ID", "ICV"),
                    new XElement(cbc + "UUID",
                        invoice.Icv.ToString())),

                // =============================
                // Previous Invoice Hash (PIH)
                // =============================
                new XElement(cac + "AdditionalDocumentReference",
                    new XElement(cbc + "ID", "PIH"),
                    new XElement(cac + "Attachment",
                        new XElement(cbc + "EmbeddedDocumentBinaryObject",
                            new XAttribute("mimeCode", "text/plain"),
                            invoice.PreviousInvoiceHash ?? string.Empty))),

                // =============================
                // Document Signature Reference (KSA-15)
                // =============================
                new XElement(cac + "Signature",
                    new XElement(cbc + "ID", "urn:oasis:names:specification:ubl:signature:Invoice"),
                    new XElement(cbc + "SignatureMethod", "urn:oasis:names:specification:ubl:dsig:enveloped:xades")),

                // =============================
                // Supplier
                // =============================
                BuildParty(invoice.Seller, true),

                // =============================
                // Customer (Mandatory in UBL 2.1 schema)
                // =============================
                BuildCustomer(invoice.Buyer, isStandard),

                // =============================
                // Payment Means
                // =============================
                new XElement(cac + "PaymentMeans",
                    new XElement(cbc + "PaymentMeansCode", invoice.PaymentMeansCode),
                    new XElement(cbc + "InstructionNote", invoice.PaymentInstruction)),

                // =============================
                // Allowances
                // =============================
                invoice.Allowances.Select(allowance =>
                    new XElement(cac + "AllowanceCharge",
                        new XElement(cbc + "ChargeIndicator", allowance.ChargeIndicator ? "true" : "false"),
                        new XElement(cbc + "AllowanceChargeReason", allowance.Reason),
                        new XElement(cbc + "Amount",
                            new XAttribute("currencyID", "SAR"),
                            allowance.Amount.ToString("0.00")),
                         new XElement(cac + "TaxCategory",
                                new XElement(cbc + "ID", allowance.TaxCategory),
                                new XElement(cbc + "Percent", allowance.TaxRate.ToString("0.00")),
                                new XElement(cac + "TaxScheme",
                                    new XElement(cbc + "ID", "VAT")))
                    )),

                // =============================
                // Tax Total (1. In Accounting Currency SAR without subtotals - EN16931-09)
                // =============================
                new XElement(cac + "TaxTotal",
                    new XElement(cbc + "TaxAmount",
                        new XAttribute("currencyID", "SAR"),
                        invoice.VatAmount.ToString("0.00"))),

                // =============================
                // Tax Total (2. In Document Currency with subtotals)
                // =============================
                new XElement(cac + "TaxTotal",
                    new XElement(cbc + "TaxAmount",
                        new XAttribute("currencyID", "SAR"),
                        invoice.VatAmount.ToString("0.00")),
                    new XElement(cac + "TaxSubtotal",
                        new XElement(cbc + "TaxableAmount",
                            new XAttribute("currencyID", "SAR"),
                            taxExclusiveAmount.ToString("0.00")),
                        new XElement(cbc + "TaxAmount",
                            new XAttribute("currencyID", "SAR"),
                            invoice.VatAmount.ToString("0.00")),
                        new XElement(cac + "TaxCategory",
                            new XElement(cbc + "ID", "S"),
                            new XElement(cbc + "Percent", "15.00"),
                            new XElement(cac + "TaxScheme",
                                new XElement(cbc + "ID", "VAT"))))),

                // =============================
                // Legal Monetary Total
                // =============================
                new XElement(cac + "LegalMonetaryTotal",
                    new XElement(cbc + "LineExtensionAmount",
                        new XAttribute("currencyID", "SAR"),
                        lineExtensionTotal.ToString("0.00")),
                    new XElement(cbc + "TaxExclusiveAmount",
                        new XAttribute("currencyID", "SAR"),
                        taxExclusiveAmount.ToString("0.00")),
                    new XElement(cbc + "TaxInclusiveAmount",
                        new XAttribute("currencyID", "SAR"),
                        taxInclusiveAmount.ToString("0.00")),
                    allowanceTotal > 0 ? new XElement(cbc + "AllowanceTotalAmount",
                        new XAttribute("currencyID", "SAR"),
                        allowanceTotal.ToString("0.00")) : null,
                    chargeTotal > 0 ? new XElement(cbc + "ChargeTotalAmount",
                        new XAttribute("currencyID", "SAR"),
                        chargeTotal.ToString("0.00")) : null,
                    new XElement(cbc + "PayableAmount",
                        new XAttribute("currencyID", "SAR"),
                        taxInclusiveAmount.ToString("0.00"))),

                // =============================
                // Invoice Lines
                // =============================
                invoice.Lines.Select((line, index) =>
                    new XElement(cac + "InvoiceLine",
                        new XElement(cbc + "ID", index + 1),
                        new XElement(cbc + "InvoicedQuantity", line.Quantity),
                        new XElement(cbc + "LineExtensionAmount",
                            new XAttribute("currencyID", "SAR"),
                            line.LineTotal.ToString("0.00")),

                       // Discount at line level
                       line.DiscountAmount > 0 ?
                       new XElement(cac + "AllowanceCharge",
                           new XElement(cbc + "ChargeIndicator", "false"),
                           new XElement(cbc + "AllowanceChargeReason", "Discount"),
                           new XElement(cbc + "Amount",
                               new XAttribute("currencyID", "SAR"),
                               line.DiscountAmount.ToString("0.00"))) : null,

                        new XElement(cac + "TaxTotal",
                            new XElement(cbc + "TaxAmount",
                                new XAttribute("currencyID", "SAR"),
                                line.VatAmount.ToString("0.00")),
                             new XElement(cbc + "RoundingAmount",
                                new XAttribute("currencyID", "SAR"),
                                (line.LineTotal + line.VatAmount).ToString("0.00"))),

                        new XElement(cac + "Item",
                            new XElement(cbc + "Name", line.Description),
                            new XElement(cac + "ClassifiedTaxCategory",
                                new XElement(cbc + "ID", line.TaxCategory),
                                new XElement(cbc + "Percent", line.VatRate),
                                !string.IsNullOrEmpty(line.TaxExemptionReasonCode) ? new XElement(cbc + "TaxExemptionReasonCode", line.TaxExemptionReasonCode) : null,
                                !string.IsNullOrEmpty(line.TaxExemptionReason) ? new XElement(cbc + "TaxExemptionReason", line.TaxExemptionReason) : null,
                                new XElement(cac + "TaxScheme",
                                    new XElement(cbc + "ID", "VAT")))),

                        new XElement(cac + "Price",
                            new XElement(cbc + "PriceAmount",
                                new XAttribute("currencyID", "SAR"),
                                line.UnitPrice.ToString("0.00"))))))
        );

        return doc.ToString(SaveOptions.DisableFormatting);
    }

    private XElement BuildParty(Party party, bool isSupplier)
    {
        var cac = UblNamespaces.cac;
        var cbc = UblNamespaces.cbc;

        var partyElement = new XElement(cac + "Party");

        // 1. PartyIdentification (optional)
        if (!string.IsNullOrWhiteSpace(party.IdentificationNumber))
        {
            partyElement.Add(
                new XElement(cac + "PartyIdentification",
                    new XElement(cbc + "ID",
                        new XAttribute("schemeID", string.IsNullOrWhiteSpace(party.IdentificationScheme) ? "CRN" : party.IdentificationScheme.Trim()),
                        party.IdentificationNumber.Trim())));
        }

        // 2. PartyName (optional)
        if (!string.IsNullOrWhiteSpace(party.Name))
        {
            partyElement.Add(
                new XElement(cac + "PartyName",
                    new XElement(cbc + "Name", party.Name.Trim())));
        }

        // 3. PostalAddress (UBL 2.1 schema sequence)
        partyElement.Add(
            new XElement(cac + "PostalAddress",
                new XElement(cbc + "StreetName", party.Street?.Trim() ?? string.Empty),
                new XElement(cbc + "BuildingNumber", party.BuildingNumber?.Trim() ?? string.Empty),
                new XElement(cbc + "CitySubdivisionName", party.District?.Trim() ?? string.Empty),
                new XElement(cbc + "CityName", party.City?.Trim() ?? string.Empty),
                new XElement(cbc + "PostalZone", party.PostalCode?.Trim() ?? string.Empty),
                !string.IsNullOrWhiteSpace(party.Subdivision) ? new XElement(cbc + "CountrySubentity", party.Subdivision.Trim()) : null,
                new XElement(cac + "Country",
                    new XElement(cbc + "IdentificationCode", string.IsNullOrWhiteSpace(party.CountryCode) ? "SA" : party.CountryCode.Trim()))));

        // 4. PartyTaxScheme (Only if VatNumber is present)
        if (!string.IsNullOrWhiteSpace(party.VatNumber))
        {
            partyElement.Add(
                new XElement(cac + "PartyTaxScheme",
                    new XElement(cbc + "CompanyID", party.VatNumber.Trim()),
                    new XElement(cac + "TaxScheme",
                        new XElement(cbc + "ID", "VAT"))));
        }

        // 5. PartyLegalEntity (BT-27 for Seller, BT-44 for Buyer) - Mandatory for BR-06 and ZATCA QR Validation
        if (!string.IsNullOrWhiteSpace(party.Name))
        {
            partyElement.Add(
                new XElement(cac + "PartyLegalEntity",
                    new XElement(cbc + "RegistrationName", party.Name.Trim())));
        }

        return new XElement(
            isSupplier ? cac + "AccountingSupplierParty" : cac + "AccountingCustomerParty",
            partyElement);
    }

    private XElement BuildCustomer(Party? party, bool isStandard)
    {
        var cac = UblNamespaces.cac;

        // In Simplified Tax Invoice (B2C):
        // If buyer is anonymous or default consumer ("مستهلك نهائي" or empty),
        // emit an empty AccountingCustomerParty (strictly compliant with UBL 2.1 & ZATCA Simplified rules).
        if (!isStandard && (party == null ||
                            string.IsNullOrWhiteSpace(party.Name) ||
                            party.Name.Contains("مستهلك") ||
                            party.Name.Contains("Consumer", StringComparison.OrdinalIgnoreCase)))
        {
            return new XElement(cac + "AccountingCustomerParty");
        }

        if (party == null)
        {
            return new XElement(cac + "AccountingCustomerParty");
        }

        return BuildParty(party, isSupplier: false);
    }
}
