namespace BLL.DTO.ScheduleDtos;

public class BllScheduleGenerateResult
{
    public GenerateStatus Status { get; init; }
    public GenerateErrorCode? Error { get; init; }
    public IReadOnlyList<GenerateWarningCode> Warnings { get; init; } = [];
    public IReadOnlyList<BllShift> Shifts { get; init; } = [];

    public static BllScheduleGenerateResult Success(List<BllShift> shifts) => new()
    {
        Status = GenerateStatus.Success,
        Shifts = shifts,
        Warnings = []
    };

    public static BllScheduleGenerateResult WithWarnings(List<BllShift> shifts, IReadOnlyList<GenerateWarningCode> warnings) => new()
    {
        Status = GenerateStatus.Warning,
        Shifts = shifts,
        Warnings = warnings
    };

    public static BllScheduleGenerateResult Fail(GenerateErrorCode error) => new()
    {
        Status = GenerateStatus.Error,
        Error = error,
        Warnings = []
    };
}

public enum GenerateStatus
{
    Success,
    Warning,
    Error
}

public enum GenerateErrorCode
{
    DepartmentNotFound,
    OrganizationNotFound,
    NoEmployeesInDepartment,
    NoShiftTypes,
    SelectedShiftTypesNotFound,
    NoWorkDaysConfigured,
    InvalidDateRange,
    AllDaysAreHolidays,
    AllDaysAreNonWorking,
    ShiftTypesDontFitSchedule,
    ScheduleNotFound
}

public enum GenerateWarningCode
{
    NoSuitableShiftTypes,
    NotEnoughEmployeesForMinimum,
    EmployeesAssignedWithConstraintViolations,
    AllEmployeesOnTimeOff,
    HighWorkloadDetected,
    SomeDaysWithoutShifts,
    BudgetExhausted
}
