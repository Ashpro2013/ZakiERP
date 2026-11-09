using System;
using System.Collections.Generic;
using ZakiSAASERP.Models.Settings;

namespace ZakiSAASERP.Models.DTO;

public record ShiftClosingReportRequest(
    int BranchId,
    DateTime FromDate,
    DateTime ToDate,
    string? Terminal = null,
    string? UserName = null
);

public record ShiftClosingReportResponse(
    List<ShiftClosingRecord> Reports,
    int TotalShifts,
    decimal TotalNetSales,
    decimal TotalCashSales,
    decimal TotalCardSales,
    decimal TotalCreditSales,
    decimal TotalReturns,
    decimal TotalActualCash,
    decimal TotalExpectedCash,
    decimal TotalDifference
);

