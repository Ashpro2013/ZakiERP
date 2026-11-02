namespace ZakiSAASERP.Models.Masters
{
    [Table("VATCategoryDetails")]
    public class VATCategoryDetails
    {
        [Key]
        public int VATCategoryId { get; set; }
        public string? Code { get; set; }
        public string? DescriptionEN { get; set; }
        public string? DescriptionAR { get; set; }
        public string? ExemptionCode { get; set; }
        public string? ExemptionTextEN { get; set; }
        public string? ExemptionTextAR { get; set; }
    }
}
