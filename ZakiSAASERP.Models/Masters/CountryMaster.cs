namespace ZakiSAASERP.Models.Masters
{
    [Table("CountryMaster")]
    public class CountryMaster
    {
        [Key]
        public int CountryId { get; set; }

        [Required(ErrorMessage = "Country Code is required. *")]
        [StringLength(100)]
        public string? Code { get; set; }

        [Required(ErrorMessage = "Country Name is required. *")]
        [StringLength(500)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? NameArabic { get; set; }
    }
}
