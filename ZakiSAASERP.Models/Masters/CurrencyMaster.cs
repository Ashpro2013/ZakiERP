namespace ZakiSAASERP.Models.Masters
{
    [Table("CurrencyMaster")]
    public class CurrencyMaster
    {
        [Key]
        public int CurrencyId { get; set; }

        [Required(ErrorMessage = "Currency Code is required. *")]
        [StringLength(100)]
        public string Code { get; set; }

        [StringLength(500)]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Rate is required. *")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal Rate { get; set; }
    }
}
