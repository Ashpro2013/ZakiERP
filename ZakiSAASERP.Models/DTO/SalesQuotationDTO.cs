
namespace ZakiSAASERP.Models.DTO;

public record SalesQuotationDTO(SalesQuotationHdr QuotationHdr, List<TransactionDtls> QuotationDtls);
