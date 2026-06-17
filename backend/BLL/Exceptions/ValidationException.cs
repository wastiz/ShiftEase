namespace BLL.Exceptions;

public class ValidationException : AppException
{
    public Dictionary<string, string[]>? Errors { get; }

    public ValidationException(string message, Dictionary<string, string[]>? errors = null)
        : base("VALIDATION_ERROR", message) => Errors = errors;
}
