namespace DTOs.EmployeeOptionsDtos;

public record BllPreferenceBundle
{
    public List<int> ShiftTypePreferences { get; set; } //Shift Type ids
    public List<DayOfWeek> WeekDayPreferences { get; set; }
}