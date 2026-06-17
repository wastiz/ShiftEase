namespace BLL.DTO.ScheduleDtos;

public class BllAcoScheduleGenerateRequest
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    /// <summary>
    /// Total working-hours pool distributed across all employees and shifts.
    /// Break time from shift templates is excluded from working hours.
    /// Null means no budget constraint.
    /// </summary>
    public double? TotalHours { get; set; }

    /// <summary>Hard vs soft budget constraint. See BllScheduleGenerateRequest.</summary>
    public bool HardTotalHours { get; set; } = true;

    /// <summary>Number of ants per iteration. Defaults to 20.</summary>
    public int NumAnts { get; set; } = 20;

    /// <summary>Number of ACO iterations. Defaults to 50.</summary>
    public int NumIterations { get; set; } = 50;
}
