using ZakiSAASERP.Models.Masters;

namespace ZakiSAASERP.Models.DTO;

public record SalesInvoiceRtnDTO(SalesInvoiceRtnHdr QuotationHdr, List<TransactionDtls> QuotationDtls, int ZatcaPhase = 1, CompanyDetails? CompanyDetails = null, bool IsSimplified = false);
