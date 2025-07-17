namespace BLL.ServiceInterfaces;
using DTOs.EmployeeOptionsDtos;

public interface IEmployeeOptionsService
{
    Task<List<VacationRequestDto>> GetVacationRequests(int employeeId);
    
    Task<int> AddVacationRequest(int employeeId, VacationRequestDto dto);
    
    Task<bool> DeleteVacationRequest(int employeeId, int vacationId);
    
    Task<List<SickLeaveDto>> GetSickLeaves(int employeeId);
    
    Task<int> AddSickLeave(int employeeId, SickLeaveDto dto);
    
    Task<bool> DeleteSickLeave(int employeeId, int id);
    
    Task<BllPreferenceBundle> GetPreferences(int employeeId);
    
    Task SavePreferences(int employeeId, BllPreferenceBundle dto);
    
    Task<List<WeekDayPreferenceDto>> GetPreferredWeekDays(int employeeId);
}
