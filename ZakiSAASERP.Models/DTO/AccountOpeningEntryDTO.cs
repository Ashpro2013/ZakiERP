using ZakiSAASERP.Models.Masters;

namespace ZakiSAASERP.Models.DTO;

public class AccountOpeningEntryDTO
{
    public AccountOpeningEntryHdr Header { get; set; } = new();
    public List<AccountOpeningEntryDtls> Details { get; set; } = new();
    public string? Message { get; set; }
    public int Id { get; set; }

    public AccountOpeningEntryDTO() { }

    public AccountOpeningEntryDTO(AccountOpeningEntryHdr header, List<AccountOpeningEntryDtls> details)
    {
        Header = header;
        Details = details;
    }
}
