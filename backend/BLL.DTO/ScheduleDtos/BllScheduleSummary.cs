namespace BLL.DTO.ScheduleDtos;

public class BllScheduleSummary
{
    public List<BllScheduleItem> ConfirmedSchedules { get; set; } = new();
    public List<BllScheduleItem> UnconfirmedSchedules { get; set; } = new();
}

public class BllScheduleItem
{
    public int Id { get; set; }
    public string Month { get; set; } = default!;
    public string Year { get; set; } = default!;
    public string Coverage { get; set; } = default!;
    public List<BllDayIssue> Warnings { get; set; } = new();
    public int TotalHours { get; set; }
    public int TotalShifts { get; set; }
}

public class BllDayIssue
{
    public string IsoDate { get; set; } = default!;
    public string Label { get; set; } = default!;
    public int Assigned { get; set; }
    public int Required { get; set; }
    public string Severity { get; set; } = default!;
}

