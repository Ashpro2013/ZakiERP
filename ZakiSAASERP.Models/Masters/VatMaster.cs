namespace ZakiSAASERP.Models.Masters
{
    [Table("VatMaster")]
    public class VatMaster
    {
        [Key]
        public int VatId { get; set; }
        [Required(ErrorMessage = "Vat Code is required. *")]
        public string Code { get; set; }
        [Required(ErrorMessage = "Vat Name is required. *")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Vat Percentage is required. *")]
        public decimal? Percentage { get; set; }
        public bool IsDefault { get; set; } = false;

    }
}
