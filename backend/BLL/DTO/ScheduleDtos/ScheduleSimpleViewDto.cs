namespace DTOs.ScheduleDtos;

public record ScheduleSimpleViewDto
{
    public int Id { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    
    public List<EmployeeSchedule> EmployeeWithShifts { get; set; }
    
    public record EmployeeSchedule
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; init; }
        public Dictionary<DateOnly, ShiftTime> Shifts { get; init; }
        
        public record ShiftTime
        {
            public string StartTime { get; init; }
            public string EndTime { get; init; }
        }
    }
}