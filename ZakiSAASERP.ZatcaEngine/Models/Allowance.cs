namespace ZakiSAASERP.ZatcaEngine.Models;

public class Allowance
{
    public bool ChargeIndicator { get; set; } // false for discount
    public string Reason { get; set; } = "Discount";
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
    public string TaxCategory { get; set; } = "S";
    public decimal TaxRate { get; set; } = 15;
}
