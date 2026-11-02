namespace AshproSAASERP.Models.Masters
{
    [Table("CategoryMaster")]
    public class CategoryMaster
    {
        [Key]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Category Code is required. *")]
        [StringLength(50)]
        public string Code { get; set; }

        [Required(ErrorMessage = "Category Name is required. *")]

        [StringLength(300)]
        public string Name { get; set; }

        [StringLength(300)]
        public string? NameArabic { get; set; }

        [StringLength(1000)]
        public string? Remarks { get; set; }
        public byte[]? ItemImage { get; set; }
        public bool IsFixedAsset { get; set; }
    }
}
