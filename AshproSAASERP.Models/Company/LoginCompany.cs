namespace AshproSAASERP.Models.Company;

public class LoginCompany
{
    [Key]
    public int Id { get; set; }
    [Required(ErrorMessage = "Company Name is required")]
    public string CompanyName { get; set; } = default!;
    [Required(ErrorMessage = "Company Code is required")]
    public string CompanyCode { get; set; } = default!;
    [Required(ErrorMessage = "Subscription Type is required")]
    public string SubscriptionType { get; set; } = default!;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    [Required(ErrorMessage = "Subscription Started At is required")]
    public DateTime? SubscriptionStartedAt { get; set; }
    [Required(ErrorMessage = "Subscription Ending Date is required")]
    public DateTime? SubscriptionEndingDate { get; set; }
    public bool IsExpired { get; set; } = false;
    public string? ContactNo { get; set; } = null;
    public string? Email { get; set; } = null;
    public string? CompanyAddress { get; set; } = null;
    public string? ConnectionString { get; set; }
    public int DBMainVersion { get; set; }
    public int DBSubVersion { get; set; }
}
