using ZakiSAASERP.Models.Masters;
using System.Text.Json.Serialization;

namespace ZakiSAASERP.Models.Transactions.Workshop
{
    [Table("JobCardDtls")]
    public class JobCardDtls
    {
        [Key]
        public int JobCardDtlsId { get; set; }

        [NotMapped]
        public int SL { get; set; }

        public int FK_JobCardId { get; set; }

        public int FK_ItemId { get; set; }
        public int FK_UnitId { get; set; }

        [StringLength(50)]
        public string? ItemCode { get; set; }

        [StringLength(200)]
        public string? ItemName { get; set; }

        [NotMapped]
        public string? ItemArabicName { get; set; }

        [StringLength(50)]
        public string? Barcode { get; set; }

        [Column(TypeName = "decimal(18, 3)")]
        public decimal QtyPerUnit { get; set; } = 1;

        [Column(TypeName = "decimal(18, 3)")]
        public decimal Qty { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Rate { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal IncVat { get; set; } // Rate Includes Vat? or Rate + Vat? Usually Rate field is Excl Vat. This might be gross or simply display. Sticking to standards.

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Value { get; set; } // Gross Amount (Qty * Rate)

        [Column(TypeName = "decimal(18, 2)")]
        public decimal DiscountPercent { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal DiscountAmount { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Total { get; set; } // Value - Discount

        [Column(TypeName = "decimal(18, 2)")]
        public decimal VatPercent { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal VatAmount { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal LineTotal { get; set; } // Total + Vat

        public string? Remarks { get; set; }

        [NotMapped]
        public string? UnitName { get; set; }

        [NotMapped]
        [JsonIgnore]
        public List<ItemMultiUnit>? MultiUnits { get; set; } = new();

        [ForeignKey("FK_JobCardId")]
        [JsonIgnore]
        public virtual JobCardHdr? JobCardHdr { get; set; }

        [ForeignKey("FK_ItemId")]
        [JsonIgnore]
        public virtual ItemMaster? Item { get; set; }

        [ForeignKey("FK_UnitId")]
        [JsonIgnore]
        public virtual UnitMaster? Unit { get; set; }

        public int? FK_StoreId { get; set; }
        [ForeignKey("FK_StoreId")]
        [JsonIgnore]
        public virtual StoreMaster? Store { get; set; }
    }
}
