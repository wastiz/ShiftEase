namespace DTOs.EmployeeOptionsDtos;

public record BllPreferenceBundle
{
    public List<int> ShiftTypePreferences { get; set; } //Shift Type ids
    public List<string> WeekDayPreferences { get; set; }
    public List<DateOnly> DayOffPreferences { get; set; }
}