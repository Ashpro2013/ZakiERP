using ZakiSAASERP.Models.Masters;
using ZakiSAASERP.Models.User;

namespace ZakiSAASERP.Models.Transactions.Sales;

[Table("RFQHdr")]
public class RFQHdr
{
    [Key]
    public int RFQId { get; set; }

    [Required(ErrorMessage = "RFQ No is required")]
    public string RFQNo { get; set; } = default!; // Our Ref No

    public string? RefNo { get; set; } // Auto generated logic might map to this or RFQNo. "Our Ref" is usually the primary ID string. 

    public string? CustomerRefNo { get; set; }

    public DateTime? CustomerRFQDate { get; set; }

    [Required(ErrorMessage = "Customer is required")]
    public int? FK_CustomerId { get; set; }

    [Required(ErrorMessage = "SalesMan is required")]
    public int? FK_SalesManId { get; set; }

    public string? Remarks { get; set; }
    public bool IsDeleted { get; set; } = false;

    public int? FK_BranchId { get; set; }
    public int? FK_UserId { get; set; }
    public int? FK_FinancialYearId { get; set; }

    public DateTime? EntryDate { get; set; } = DateTime.Now;

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

    public virtual List<RFQDtls> RFQDetails { get; set; } = new();

    [NotMapped]
    public string? CustomerName { get; set; }
}
