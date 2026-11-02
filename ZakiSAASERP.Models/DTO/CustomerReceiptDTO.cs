namespace ZakiSAASERP.Models.Transactions.Accounts;

public class CustomerReceiptDTO
{
    public int CustomerReceiptId { get; set; }
    public string ReceiptNo { get; set; } = string.Empty;
    public DateTime ReceiptDate { get; set; } = DateTime.Now;
    public int FK_CustomerId { get; set; }
    public int FK_BranchId { get; set; }
    public int? FK_UserId { get; set; }
    public int? FK_ProjectId { get; set; }
    public string PaymentMode { get; set; } = "Cash";
    public string? BankName { get; set; }
    public string? ChequeNo { get; set; }
    public DateTime? ChequeDate { get; set; }
    public string? BankAccountNo { get; set; }
    public string? RvNumber { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AdvanceAmount { get; set; }
    public decimal AllocatedAmount { get; set; }
    public string? Remarks { get; set; }
    public int? FK_AccountId { get; set; }
    public int? FK_SalesManId { get; set; }

    public List<CustomerReceiptDtls> Details { get; set; } = new();

    // Additional properties for UI
    public string? CustomerName { get; set; }
    public decimal CustomerBalance { get; set; }
    public string? AccountCode { get; set; }
    public string? AccountName { get; set; }
    public string? SalesManName { get; set; }
}
