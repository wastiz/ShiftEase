namespace BLL.Exceptions;

public class NotFoundException : AppException
{
    public NotFoundException(string message) : base("NOT_FOUND", message) { }
}
