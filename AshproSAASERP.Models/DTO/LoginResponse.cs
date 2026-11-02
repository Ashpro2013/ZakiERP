namespace AshproSAASERP.Models.DTO;

public record LoginResponse(int UserId, string tocken, bool IsLogin = false, bool IsAdmin = false);

