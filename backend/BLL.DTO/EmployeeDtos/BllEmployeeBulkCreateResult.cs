namespace DTOs.EmployeeDtos;

public record BllEmployeeBulkCreateResult
{
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<BllEmployeeCreateSuccess> SuccessfulEmployees { get; set; } = new();
    public List<BllEmployeeCreateError> FailedEmployees { get; set; } = new();
}

public record BllEmployeeCreateSuccess
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
}

public record BllEmployeeCreateError
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string ErrorMessage { get; set; }
}