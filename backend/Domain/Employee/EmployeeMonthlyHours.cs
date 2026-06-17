namespace Domain;

public class EmployeeMonthlyHours
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalHours { get; set; }
    public DateTime UpdatedAt { get; set; }
}