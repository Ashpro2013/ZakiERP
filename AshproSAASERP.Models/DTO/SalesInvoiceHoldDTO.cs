using AshproSAASERP.Models.Transactions.Sales;
using AshproSAASERP.Models.ViewModel;

namespace AshproSAASERP.Models.DTO;

public record SalesInvoiceHoldDTO(SalesInvoiceHoldHdr HoldHdr, List<TransactionDtls> HoldDtls);

