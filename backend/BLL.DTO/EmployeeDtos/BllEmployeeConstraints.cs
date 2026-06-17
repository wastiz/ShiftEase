using DTOs.EmployeeOptionsDtos;

namespace DTOs.EmployeeDtos;

public class BllEmployeeConstraints
{
    public int EmployeeId { get; set; }
    public List<BllTimeOffRequest> TimeOffRequests { get; set; }
    public List<BllUnavailableDay> UnavailableDays { get; set; }
    public List<BllPreferredShift> PreferredShifts { get; set; }
    public int MaxShiftsPerWeek { get; set; }
    public int MaxConsecutiveShifts { get; set; }
    public int MinRestHoursBetweenShifts { get; set; }
    public List<int> QualifiedShiftTypeIds { get; set; }
}
