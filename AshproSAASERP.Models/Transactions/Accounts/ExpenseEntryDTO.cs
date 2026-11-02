namespace AshproSAASERP.Models.Transactions.Accounts;

public class ExpenseEntryDTO
{
    public int ExpenseId { get; set; }
    public string ExpenseNo { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; } = DateTime.Now;

    [Range(1, int.MaxValue, ErrorMessage = "Payment Account is required. *")]
    public int FK_AccountId { get; set; }

    public int FK_BranchId { get; set; }
    public int? FK_UserId { get; set; }
    public int? FK_ProjectId { get; set; }
    public int? FK_SalesmanId { get; set; }
    public int? FK_SupplierId { get; set; }
    public decimal Gross { get; set; }
    public decimal VatTotal { get; set; }
    public decimal NetTotal { get; set; }
    public string? Narration { get; set; }
    public string? ReferenceNo { get; set; }
    public string? BillType { get; set; }
    public string? VatNo { get; set; }

    public List<ExpenseEntryDtls> ExpenseEntryDtls { get; set; } = new();

    // NotMapped properties for UI
    [Required(ErrorMessage = "Payment Account is required. *")]
    public string? PaymentAccountName { get; set; }
    public string? SupplierName { get; set; }
}
