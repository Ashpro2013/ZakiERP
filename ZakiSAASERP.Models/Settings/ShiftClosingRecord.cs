using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZakiSAASERP.Models.Settings;

public class ShiftClosingRecord
{
    [Key]
    public int ShiftClosingRecordId { get; set; }

    [MaxLength(50)]
    public string ShiftCode { get; set; } = string.Empty;

    [MaxLength(100)]
    public string UserName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Terminal { get; set; } = string.Empty;

    public int? FK_BranchId { get; set; }

    public DateTime? DateFrom { get; set; }

    public DateTime? DateTo { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal OpeningBalance { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalSale { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalSalesReturn { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal NetSales { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCashSales { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCardSales { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCreditSales { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPayment { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCashReceipt { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCardReceipt { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ExpectedCash { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ActualCash { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Difference { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal NewOpeningBalance { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Balanced";

    [MaxLength(500)]
    public string? Remarks { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

