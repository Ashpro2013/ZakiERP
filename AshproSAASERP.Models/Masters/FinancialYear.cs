namespace AshproSAASERP.Models.Masters;

[Table("FinancialYear")]
public class FinancialYear
{
    [Key]
    public int FinancialYearId { get; set; }
    [Required(ErrorMessage = "Financial Year Code is required. *")]
    public int RefNo { get; set; }
    [Required(ErrorMessage = "Financial Year From Date is required. *")]
    public DateTime? FromDate { get; set; }
    [Required(ErrorMessage = "Financial Year To Date is required. *")]
    public DateTime? ToDate { get; set; }
    [Required(ErrorMessage = "Financial Year Name is required. *")]
    public string Name { get; set; }
    public bool IsClosed { get; set; } = false;
}
