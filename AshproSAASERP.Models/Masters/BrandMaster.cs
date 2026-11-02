namespace AshproSAASERP.Models.Masters
{
    [Table("BrandMaster")]
    public class BrandMaster
    {

        [Key]
        public int BrandId { get; set; }
        [Required(ErrorMessage = "Brand Code is required. *")]

        [StringLength(500)]
        public string Code { get; set; }
        [Required(ErrorMessage = "Unit Name is required. *")]
        [StringLength(500)]
        public string Name { get; set; }

        [StringLength(500)]
        public string? NameArabic { get; set; }

        [StringLength(1000)]
        public string? Remarks { get; set; }
    }
}
