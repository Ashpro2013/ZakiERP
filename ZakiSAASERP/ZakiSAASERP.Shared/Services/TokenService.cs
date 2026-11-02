namespace ZakiSAASERP.Shared.Services;

public class TokenService(JSService js)
{
    public async Task SaveToken(string token)
    {
        await js.SetLocal(token, "jwt");
    }
    public async Task<string?> GetToken()
    {
        return await js.GetLocal("jwt");
    }
}
