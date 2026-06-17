namespace DAL.DTO.ScheduleDtos;

public class DalScheduleSummary
{
    public List<DalScheduleItem> ConfirmedSchedules { get; set; } = new();
    public List<DalScheduleItem> UnconfirmedSchedules { get; set; } = new();
}

public class DalScheduleItem
{
    public int Id { get; set; }
    public string Month { get; set; } = default!;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int TotalShifts { get; set; }
    public int TotalMinutes { get; set; }
    public bool IsConfirmed { get; set; }
}