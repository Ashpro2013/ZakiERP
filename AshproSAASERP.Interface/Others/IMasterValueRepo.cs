using AshproSAASERP.Models.Others;

namespace AshproSAASERP.Interface.Others;

public interface IMasterValueRepo
{
    Task<MasterValue> GetAsync(string key, int? BranchId);
    Task<OutputResponse> AddAsync(MasterValue value);
    Task UpdatePreviousHashAsync(string hash, int BranchId);
}
