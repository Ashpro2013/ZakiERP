namespace AshproSAASERP.Models.Transactions.Inventory;

public class MTRApprovalFilter
{
    public int? BranchId { get; set; }
    public string? Status { get; set; } // "Pending", "Approved", "Rejected", "All"
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? SearchTerm { get; set; } // For searching VoucherNo, Ref, etc.
}
