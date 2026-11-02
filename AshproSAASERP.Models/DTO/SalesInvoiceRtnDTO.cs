using AshproSAASERP.Models.Masters;

namespace AshproSAASERP.Models.DTO;

public record SalesInvoiceRtnDTO(SalesInvoiceRtnHdr QuotationHdr, List<TransactionDtls> QuotationDtls, int ZatcaPhase = 1, CompanyDetails? CompanyDetails = null, bool IsSimplified = false);
