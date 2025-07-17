namespace DAL.DTO.ShiftTypeDtos;

public class DalShiftType
{
    public int Id { get; set; }
    public string Name { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int EmployeeNeeded { get; set; }
    public string Color { get; set; }
    public int OrganizationId { get; set; } = 0;
}