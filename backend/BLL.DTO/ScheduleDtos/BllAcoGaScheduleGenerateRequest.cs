namespace BLL.DTO.ScheduleDtos;

public class BllAcoGaScheduleGenerateRequest
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

    /// <summary>Number of ants per ACO iteration. Defaults to 20.</summary>
    public int NumAnts { get; set; } = 20;

    /// <summary>Number of ACO iterations (Phase 1). Defaults to 30.</summary>
    public int NumAcoIterations { get; set; } = 30;

    /// <summary>GA population size (Phase 2). Defaults to 50.</summary>
    public int PopulationSize { get; set; } = 50;

    /// <summary>Number of GA generations (Phase 2). Defaults to 80.</summary>
    public int NumGaGenerations { get; set; } = 80;
}
