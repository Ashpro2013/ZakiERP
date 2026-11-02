namespace AshproSAASERP.Models.Others;

[Table("BarcodeDesignHdr")]
public class BarcodeDesignHdr
{
    [Key]
    public int BarcodeDesignHdrId { get; set; }
    public string DesignName { get; set; }
    public int? FirstMargin { get; set; }
    public int? TopMargin { get; set; }
    public int? RightMargin { get; set; }
    public int? BottomMargin { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public int? Accross { get; set; }
    public int? Down { get; set; }
    public int? TotalPerPage { get; set; }
    public string? PrinterName { get; set; } = null;
    public string? PaperName { get; set; } = null;
    public int? PSWidth { get; set; }
    public int? PSHieght { get; set; }
    public bool IsBorderIncluded { get; set; } = false;
    public bool IsBarcode { get; set; } = true;
    public bool GenerateLabel { get; set; } = true;
    public bool IsDefault { get; set; } = true;
    public int FK_BranchId { get; set; }
}
