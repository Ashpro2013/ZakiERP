namespace ZakiSAASERP.Models.Others;

[Table("PrintDesignHdr")]
public class PrintDesignHdr
{
    [Key]
    public int PrintDesignHdrId { get; set; }
    public string DesignName { get; set; }
    public string VoucherType { get; set; }
    public bool IsDefault { get; set; }
    public int PageHeight { get; set; }
    public int PageWidth { get; set; }
    public int LinesPerPage { get; set; }
    public int SpaceBtwnLines { get; set; }
    public string? BagroundImagePath { get; set; }
    public string? PrinterName { get; set; }
    public string PrintType { get; set; }
    public string FormType { get; set; }
    public int FK_BranchId { get; set; }
}

