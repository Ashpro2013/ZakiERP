namespace AshproSAASERP.Interface.Others;

public interface IUserRepo : ICommonInterface<UserDetails>
{
    Task<UserDetails> GetForLogin(string userName, string password);
    Task<List<AshproSAASERP.Models.Masters.BranchMaster>> GetBranchesForUser(string userName);
    Task<AshproSAASERP.Models.DTO.OutputResponse> SaveUserBranches(List<UserBranch> userBranches);
}
