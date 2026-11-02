namespace ZakiSAASERP.Models.Masters
{
    [Table("PermanentSettings")]
    public class PermanentSettings
    {
        [Key]
        public int SettingId { get; set; }
        public string SettingKey { get; set; }
        public string? SettingValue { get; set; }
    }
}
