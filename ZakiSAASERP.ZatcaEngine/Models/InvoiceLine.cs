namespace ZakiSAASERP.ZatcaEngine.Models;

public class InvoiceLine
{
    public string Description { get; set; } = "";
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }

    public decimal DiscountAmount { get; set; }
    public decimal VatAmount { get; set; }
    public decimal VatRate { get; set; }
    public string TaxCategory { get; set; } = "S";
    public string TaxExemptionReasonCode { get; set; } = "";
    public string TaxExemptionReason { get; set; } = "";
}

