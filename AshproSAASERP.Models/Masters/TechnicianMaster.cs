namespace AshproSAASERP.Models.Masters
{
    [Table("TechnicianMaster")]
    public class TechnicianMaster
    {
        [Key]
        public int TechnicianId { get; set; }

        [Required(ErrorMessage = "Code is required.")]
        [StringLength(50)]
        public string Code { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(100)]
        public string? Position { get; set; }

        [StringLength(20)]
        public string? Mobile { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }
    }
}
