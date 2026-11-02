namespace AshproSAASERP.Interface.Others;

public interface IServerRepo
{
    Task<PrintDesignDTO> GetPDesignFromFileAsync(string designName);
    Task<List<PrintDesignDTO>> GetFullPDesignFromFileAsync();
    Task SavePDesignToFileAsync(PrintDesignDTO dto);

}
