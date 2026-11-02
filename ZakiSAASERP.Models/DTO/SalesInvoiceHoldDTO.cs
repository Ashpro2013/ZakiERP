using ZakiSAASERP.Models.Transactions.Sales;
using ZakiSAASERP.Models.ViewModel;

namespace ZakiSAASERP.Models.DTO;

public record SalesInvoiceHoldDTO(SalesInvoiceHoldHdr HoldHdr, List<TransactionDtls> HoldDtls);

