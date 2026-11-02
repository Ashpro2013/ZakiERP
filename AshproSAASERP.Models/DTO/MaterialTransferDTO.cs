using AshproSAASERP.Models.Transactions.Inventory;

namespace AshproSAASERP.Models.DTO
{

    public class ApproveMTRRequest
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public List<MaterialTransferRequestDtls> Details { get; set; } = new();
    }

    public class RejectMTRRequest
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? RejectionReason { get; set; }
    }

}
