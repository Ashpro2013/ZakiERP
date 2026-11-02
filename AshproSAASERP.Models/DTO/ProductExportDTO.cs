namespace AshproSAASERP.Models.DTO;

public class ProductExportRowDTO
{
    public int SL { get; set; }
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
}
