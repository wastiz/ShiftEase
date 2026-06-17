using DTOs.EmployeeOptionsDtos;

namespace BLL.Contracts;

public interface IPreferenceService
{
    Task<BllPreferenceBundle> GetPreferences(int employeeId);
    Task SavePreferences(int employeeId, BllPreferenceBundle dto);
    Task<List<DayOfWeek>> GetPreferredWeekDays(int employeeId);
}
