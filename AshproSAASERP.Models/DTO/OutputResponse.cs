namespace AshproSAASERP.Models.DTO;

public record OutputResponse(int Id, string? Message, object? obj = null,bool IsSuccess = false);
