using ZakiSAASERP.Models.Masters;

namespace ZakiSAASERP.Models.DTO;

public record CustomerStatementRequest(
    DateTime FromDate,
    DateTime ToDate,
    int? CustomerId,
    int BranchId
);

public record CustomerStatementReportDtls(
   int SL,
   string Date,
   string VoucherNo,
   string Type,
   string Description,
   decimal? Debit,
   decimal? Credit,
   decimal? Balance,
   string RefNo = "",
   int VoucherId = 0
);

public record CustomerStatementResponse(
    List<CustomerStatementReportDtls> Details,
    decimal OpeningBalance,
    decimal TotalDebit,
    decimal TotalCredit,
    decimal ClosingBalance,
    CustomerMaster? Customer = null
);
