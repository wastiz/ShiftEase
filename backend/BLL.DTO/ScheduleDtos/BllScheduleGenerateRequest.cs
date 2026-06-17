namespace BLL.DTO.ScheduleDtos;

public class BllScheduleGenerateRequest
{
    public int Id { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    /// <summary>
    /// Total working-hours pool distributed across all employees and shifts.
    /// Break time from shift templates is excluded from working hours.
    /// Null means no budget constraint — shifts are filled up to MaxEmployees.
    /// </summary>
    public double? TotalHours { get; set; }

    /// <summary>
    /// When true (default) the budget is a hard limit: no assignment that would
    /// cause total used hours to exceed <see cref="TotalHours"/> is made.
    /// When false it is a soft constraint: minimum shift coverage may cause the
    /// budget to be exceeded, but a BudgetExhausted warning is raised.
    /// </summary>
    public bool HardTotalHours { get; set; } = true;
}
