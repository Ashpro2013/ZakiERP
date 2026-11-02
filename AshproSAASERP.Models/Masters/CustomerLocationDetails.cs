namespace AshproSAASERP.Models.Masters
{
    [Table("CustomerLocationDetails")]
    public class CustomerLocationDetails
    {
        [Key]
        public int CustomerLocationDetailsId { get; set; }

        public int? FK_CustomerId { get; set; }

        public int? FK_CustomerLocationId { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        [NotMapped]
        public string? LocationName { get; set; }
    }
}
