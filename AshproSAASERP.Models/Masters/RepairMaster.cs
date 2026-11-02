namespace AshproSAASERP.Models.Masters
{
    [Table("RepairMaster")]
    public class RepairMaster
    {
        [Key]
        public int RepairId { get; set; }

        [Required(ErrorMessage = "Code is required.")]
        [StringLength(50)]
        public string Code { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }
    }
}
