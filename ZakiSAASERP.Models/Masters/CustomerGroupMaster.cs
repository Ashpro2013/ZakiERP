namespace ZakiSAASERP.Models.Masters
{
    [Table("CustomerGroupMaster")]
    public class CustomerGroupMaster
    {
        [Key]
        public int CustomerGroupId { get; set; }
        [Required(ErrorMessage = "Customer Group Code is required. *")]
        [StringLength(30)]
        public string? Code { get; set; }
        [Required(ErrorMessage = "Customer Group Name is required. *")]
        [StringLength(150)]
        public string? Name { get; set; }
        [StringLength(150)]
        public string? NameArabic { get; set; }
        [StringLength(1000)]
        public string? Remarks { get; set; }
    }
}
