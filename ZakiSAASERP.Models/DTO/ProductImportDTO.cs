namespace ZakiSAASERP.Models.DTO;

public class ProductImportRowDTO
{
    public int RowNumber { get; set; }
    public string? ProductName { get; set; }
    public string? ArabicName { get; set; }
    public string? Barcode { get; set; }
    public string? Category { get; set; }
    public string? SubCategory { get; set; }
    public string? Brand { get; set; }
    public string? Unit { get; set; }
    public string? VatCategory { get; set; }
    public decimal? SalesRate { get; set; }
    public decimal? SalesInclusiveRate { get; set; }
    public decimal? PurchaseRate { get; set; }
    public string? Status { get; set; }
    public bool IsValid { get; set; } = true;
}

public class ProductImportResultDTO
{
    public int TotalRows { get; set; }
    public int ImportedCount { get; set; }
    public int SkippedCount { get; set; }
    public int ErrorCount { get; set; }
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public List<string> Messages { get; set; } = new();
}
