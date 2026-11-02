namespace AshproSAASERP.Models.DTO;

public record SalesOrderDTO(SalesOrderHdr OrderHdr, List<TransactionDtls> OrderDtls);
