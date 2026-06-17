namespace DTOs.EmployeeDtos;

public class BllResult<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = "No message";
    public T? Data { get; set; }

    public static BllResult<T> Ok(T data, string message = "Success") =>
        new() { Success = true, Message = message, Data = data };

    public static BllResult<T> Fail(string message = "Failure") =>
        new() { Success = false, Message = message };
}
