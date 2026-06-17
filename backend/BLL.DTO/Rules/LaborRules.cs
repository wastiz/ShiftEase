namespace BLL.Rules;

public class LaborRules
{
    public string Country { get; set; }

    public StandardWorkRules StandardWork { get; set; }
    public MaxLimitsRules MaxLimits { get; set; }
    public RestPeriodsRules RestPeriods { get; set; }
    public OvertimeRules Overtime { get; set; }
    public SummarizedWorkTimeRules SummarizedWorkTime { get; set; }
    public NightWorkRules NightWork { get; set; }

    public Dictionary<int, Dictionary<int, int>> MonthlyNormHours { get; set; }
}

public class StandardWorkRules
{
    public int HoursPerDay { get; set; } = 8;
    public int HoursPerWeek { get; set; } = 40;
}

public class MaxLimitsRules
{
    public int MaxHoursPerWeekAverage { get; set; } = 48;
    public int MaxHoursWithAgreement { get; set; } = 52;
    public bool RequiresAgreementForExtendedHours { get; set; } = false;
    public int MaxShiftLengthHours { get; set; } = 13;
}

public class RestPeriodsRules
{
    public int MinDailyRestHours { get; set; } = 11;
    public int MinWeeklyRestHours { get; set; } = 48;
    public int MinWeeklyRestHoursSummarized { get; set; } = 36;
    public int BreakAfterHours { get; set; } = 6;
    public int MinBreakDurationMinutes { get; set; } = 30;
    public bool BreakCountsAsWorkTime { get; set; } = false;
}

public class OvertimeRules
{
    public bool Allowed { get; set; } = true;
    public bool RequiresAgreement { get; set; } = false;
}

public class SummarizedWorkTimeRules
{
    public bool Enabled { get; set; } = true;
    public int MaxPeriodMonths { get; set; } = 4;
    public bool OvertimeCalculatedAtEnd { get; set; } = true;
}

public class NightWorkRules
{
    public TimeSpan Start { get; set; } = TimeSpan.FromHours(22);
    public TimeSpan End { get; set; } =  TimeSpan.FromHours(6);
}