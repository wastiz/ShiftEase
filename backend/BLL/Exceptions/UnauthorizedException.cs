namespace BLL.Exceptions;

public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message) : base("UNAUTHORIZED", message) { }
}
