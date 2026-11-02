namespace AshproSAASERP.Models.DTO;

public class StockMovementDto
{
    public int ItemId { get; set; }
    public int BranchId { get; set; }
    public int? StoreId { get; set; }

    public int VoucherId { get; set; }

    public string VoucherNo { get; set; } = string.Empty;

    public DateTime VoucherDate { get; set; }

    public string VoucherType { get; set; } = string.Empty;

    public string FormType { get; set; } = string.Empty;

    public int? AccountId { get; set; }

    public bool IsInward { get; set; }

    public decimal Quantity { get; set; }

    public int UnitId { get; set; }

    public int ActualUnitId { get; set; }

    public decimal ActualQuantity { get; set; }

    public decimal Rate { get; set; }

    public decimal ActualRate { get; set; }

    public string? Narration { get; set; }

    public int? UserId { get; set; }
}
