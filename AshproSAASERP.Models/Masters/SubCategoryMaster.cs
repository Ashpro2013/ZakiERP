namespace AshproSAASERP.Models.Masters
{
    [Table("SubCategoryMaster")]
    public class SubCategoryMaster
    {
        [Key]
        [Column("SubCategoryId")]
        public int SubCategoryId { get; set; }

        [Required(ErrorMessage = "Sub Category Code is required. *")]
        [StringLength(50)]
        public string? Code { get; set; }

        [Required(ErrorMessage = "Sub Category Name is required. *")]
        [StringLength(500)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? NameArabic { get; set; }

        [Required(ErrorMessage = "Category is required. *")]
        public int? FK_CategoryId { get; set; }

        [StringLength(1000)]
        public string? Remarks { get; set; }

        [StringLength(500)]
        public string? ItemImageName { get; set; }

        public byte[]? ItemImage { get; set; }

        public bool? IsFixedAsset { get; set; }

        [ForeignKey(nameof(FK_CategoryId))]
        public virtual CategoryMaster? Category { get; set; }
    }
}
