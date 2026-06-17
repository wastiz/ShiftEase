namespace BLL.DTO.ScheduleDtos;

public class BllGaScheduleGenerateRequest
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

    /// <summary>Number of individuals in the population. Defaults to 50.</summary>
    public int PopulationSize { get; set; } = 50;

    /// <summary>Number of GA generations. Defaults to 100.</summary>
    public int NumGenerations { get; set; } = 100;
}
