namespace API.Models;

public record ErrorResponse(string ErrorCode, string Message, object? Errors = null);
