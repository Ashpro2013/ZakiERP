namespace ZakiSAASERP.Models.Masters
{
    [Table("StoreMaster")]
    public class StoreMaster
    {
        [Key]
        public int StoreId { get; set; }

        [Required(ErrorMessage = "Code is required")]
        [StringLength(50)]
        public string Code { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        public bool IsDefaultStore { get; set; } = false;

        public int? FK_BranchId { get; set; }

        [ForeignKey(nameof(FK_BranchId))]
        public BranchMaster? Branch { get; set; }
    }
}
