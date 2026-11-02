namespace ZakiSAASERP.Models.DTO;

public record OutputResponse(int Id, string? Message, object? obj = null,bool IsSuccess = false);
