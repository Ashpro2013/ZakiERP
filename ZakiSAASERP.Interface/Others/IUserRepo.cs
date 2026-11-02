namespace ZakiSAASERP.Interface.Others;

public interface IUserRepo : ICommonInterface<UserDetails>
{
    Task<UserDetails> GetForLogin(string userName, string password);
    Task<List<ZakiSAASERP.Models.Masters.BranchMaster>> GetBranchesForUser(string userName);
    Task<ZakiSAASERP.Models.DTO.OutputResponse> SaveUserBranches(List<UserBranch> userBranches);
}
