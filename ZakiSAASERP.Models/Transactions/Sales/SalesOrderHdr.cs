using ZakiSAASERP.Models.Masters;
using ZakiSAASERP.Models.User;

namespace ZakiSAASERP.Models.Transactions.Sales;

[Table("SalesOrderHdr")]
public class SalesOrderHdr
{
    [Key]
    public int SalesOrderId { get; set; }
    [Required(ErrorMessage = "Sales Order No is required")]
    public string SalesOrderNo { get; set; } = default!;
    public string? RefNo { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal LineDsicount { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal NetAmount { get; set; }
    public decimal? AdditionalAmount { get; set; }
    public decimal? Freight { get; set; }
    public decimal? AdditionalDiscPertge { get; set; }
    public decimal? AdditionalDiscAmnt { get; set; }
    public int AdditionalDiscountType { get; set; } = 1;
    public decimal GrandTotal { get; set; }
    public string? Remarks { get; set; }
    public bool IsDeleted { get; set; } = false;
    public int FK_BranchId { get; set; }
    public int FK_CustomerId { get; set; }
    public string CustomerName { get; set; }
    public int FK_SalesManId { get; set; }
    public int FK_UserId { get; set; }
    public int FK_FinancialYearId { get; set; }
    public int? FK_ProjectId { get; set; }
    public DateTime? SalesOrderDate { get; set; }
    public DateTime? EntryDate { get; set; } = DateTime.Now;
    public int? SalesQuotationId { get; set; }
    [NotMapped]
    public string? QuotationNo { get; set; }
    [ForeignKey("FK_BranchId")]
    public virtual BranchMaster? Branch { get; set; }
    [ForeignKey("FK_CustomerId")]
    public virtual CustomerMaster? Customer { get; set; }
    [ForeignKey("FK_SalesManId")]
    public virtual SalesManMaster? SalesMan { get; set; }
    [ForeignKey("FK_UserId")]
    public virtual UserDetails? User { get; set; }
    [ForeignKey("FK_FinancialYearId")]
    public virtual FinancialYear? FinancialYear { get; set; }
    [ForeignKey("FK_ProjectId")]
    public virtual ProjectMaster? Project { get; set; }

    public List<SalesOrderDtls> SalesOrderDtls { get; set; } = new();
}
