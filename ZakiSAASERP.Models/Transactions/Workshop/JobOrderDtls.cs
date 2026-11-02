using ZakiSAASERP.Models.Masters;
using System.Text.Json.Serialization;

namespace ZakiSAASERP.Models.Transactions.Workshop
{
    [Table("JobOrderDtls")]
    public class JobOrderDtls
    {
        [Key]
        public int JobOrderDtlsId { get; set; }

        public int FK_JobOrderId { get; set; }

        public int FK_RepairId { get; set; }

        [NotMapped]
        public string? RepairCode { get; set; }

        [NotMapped]
        public string? RepairName { get; set; }

        public int? FK_TechnicianId { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Qty { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Rate { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Total { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal VatPercent { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal VatAmount { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal LineTotal { get; set; }

        [NotMapped]
        public string? TechnicianName { get; set; }

        [ForeignKey("FK_JobOrderId")]
        [JsonIgnore]
        public virtual JobOrderHdr? JobOrderHdr { get; set; }

        [ForeignKey("FK_RepairId")]
        [JsonIgnore]
        public virtual RepairMaster? Repair { get; set; }

        [ForeignKey("FK_TechnicianId")]
        [JsonIgnore]
        public virtual TechnicianMaster? Technician { get; set; }
    }
}
