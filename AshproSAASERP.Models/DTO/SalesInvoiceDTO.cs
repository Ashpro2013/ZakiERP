using AshproSAASERP.Models.Masters;

namespace AshproSAASERP.Models.DTO;

public record SalesInvoiceDTO(SalesInvoiceHdr InvoiceHdr, List<TransactionDtls> InvoiceDtls, CompanyDetails CompanyDetails, int ZatcaPhase = 1, bool IsSimplified = true, bool IsUpload = true);
