namespace AshproSAASERP.Models.Masters
{
    /// <summary>
    /// Tracks the last used number for each code type (Item, Customer, Supplier, Category, SubCategory)
    /// </summary>
    [Table("CodeSequences")]
    public class CodeSequence
    {
        [Key]
        [Required]
        [MaxLength(100)]
        public string CodeKey { get; set; } = string.Empty;

        [Required]
        public int LastNumber { get; set; }
    }
}
