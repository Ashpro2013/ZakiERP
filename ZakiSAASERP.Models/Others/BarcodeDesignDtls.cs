namespace ZakiSAASERP.Models.Others;

[Table("BarcodeDesignDtls")]
public class BarcodeDesignDtls
{
    [Key]
    public int BarcodeDesignDtlsId { get; set; }
    public int FK_BarcodeDesignHdrId { get; set; }
    public string? Name { get; set; }
    public string? Text { get; set; }
    public int Position_x { get; set; }
    public int Position_y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string? Alignment { get; set; }
    public string? RotationPoint { get; set; }
    public string? fontFamily { get; set; }
    public int fontSize { get; set; }
    public string? fStyle { get; set; }
    public string Format { get; set; }
    public string? bAlignment { get; set; }
    public string? Type { get; set; }
    public string? Prefix { get; set; }
}
