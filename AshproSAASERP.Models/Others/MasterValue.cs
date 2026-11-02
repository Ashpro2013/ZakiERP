namespace AshproSAASERP.Models.Others;

[Table("MasterValue")]
public class MasterValue
{
    [Key]
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? MasterType { get; set; }
    public string? FieldOne { get; set; }
    public string? FieldTwo { get; set; }
    public string? FieldThree { get; set; }
    public string? FieldFour { get; set; }
    public int? BranchId { get; set; }
}
