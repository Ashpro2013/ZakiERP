namespace AshproSAASERP.Models.Masters
{
    [Table("CustomerLocationMaster")]
    public class CustomerLocationMaster
    {
        [Key]
        public int LocationId { get; set; }

        [Required(ErrorMessage = "Location code is required.")]
        [StringLength(100)]
        public string Code { get; set; }

        [Required(ErrorMessage = "Location name is required.")]
        [StringLength(500)]
        public string Name { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }
    }
}
