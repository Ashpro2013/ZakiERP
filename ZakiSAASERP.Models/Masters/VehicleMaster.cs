using ZakiSAASERP.Models.Enums;

namespace ZakiSAASERP.Models.Masters
{
    [Table("VehicleMaster")]
    public class VehicleMaster
    {
        [Key]
        public int VehicleId { get; set; }

        [Required(ErrorMessage = "Plate No is required.")]
        [StringLength(50)]
        public string PlateNo { get; set; }

        [Required(ErrorMessage = "Chassis No is required.")]
        [StringLength(100)]
        public string ChassisNo { get; set; }

        [StringLength(100)]
        public string Model { get; set; }

        [Range(1000, 9999, ErrorMessage = "Year must be 4 digits")]
        public int Year { get; set; }

        public int FK_CustomerId { get; set; }

        [StringLength(200)]
        public string? CustomerName { get; set; }

        [StringLength(500)]
        public string? ItemImageName { get; set; }

        /// <summary>
        /// Relative path to image: {ClientFolder}/{EntityType}/{EntityId}/{filename}
        /// Combined with BasePath at runtime to get full path/URL
        /// </summary>
        [StringLength(500)]
        public string? ImagePath { get; set; }

        /// <summary>
        /// Storage type: 0=Local, 1=AWS
        /// Determines which service to use for image operations
        /// </summary>
        public ImageStorageType StorageType { get; set; } = ImageStorageType.Local;
    }
}
