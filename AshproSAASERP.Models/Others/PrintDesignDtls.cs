namespace AshproSAASERP.Models.Others;

[Table("PrintDesignDtls")]
public class PrintDesignDtls
{
    [Key]
    public int PrintDesignDtlsId { get; set; }
    public int FK_PrintDesignHdrId { get; set; }
    public string ControlName { get; set; }
    public string ControlType { get; set; }
    public string? FieldText { get; set; }
    public int PositionX { get; set; }
    public int PositionY { get; set; }
    public int FontSize { get; set; }
    public string? Font { get; set; }
    public bool IsBold { get; set; }
    public int Width { get; set; }
    public string? Alignment { get; set; }
    public string Location { get; set; }
    public int PIndex { get; set; }
    public int Height { get; set; }
    public string? Spec { get; set; }
    public string? FStyle { get; set; }
}

