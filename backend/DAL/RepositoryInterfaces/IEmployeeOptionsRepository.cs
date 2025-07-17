using Domain;

namespace DAL.Repositories
{
    public interface IEmployeeOptionsRepository
{
    Task<List<VacationRequest>> GetVacationRequestsAsync(int employeeId);
    Task<VacationRequest?> GetVacationRequestByIdAsync(int employeeId, int vacationRequestId);
    Task AddVacationRequestAsync(VacationRequest vacationRequest);
    Task DeleteVacationRequestAsync(VacationRequest request);

    Task<List<SickLeave>> GetSickLeavesAsync(int employeeId);
    Task<SickLeave?> GetSickLeaveByIdAsync(int employeeId, int sickLeaveId);
    Task AddSickLeaveAsync(SickLeave sickLeave);
    Task DeleteSickLeaveAsync(SickLeave leave);

    Task<List<WeekDayPreference>> GetWeekDayPreferencesAsync(int employeeId);
    Task<List<WeekDay>> GetWeekDaysByNamesAsync(List<string> names);
    Task RemoveAllWeekDayPreferencesAsync(int employeeId);
    Task AddWeekDayPreferencesAsync(List<WeekDayPreference> preferences);

    Task<List<ShiftTypePreference>> GetShiftTypePreferencesAsync(int employeeId);
    Task RemoveShiftTypePreferencesAsync(List<ShiftTypePreference> preferences);
    Task AddShiftTypePreferencesAsync(List<ShiftTypePreference> preferences);

    Task<List<DayOffPreference>> GetDayOffPreferencesAsync(int employeeId);
    Task RemoveDayOffPreferencesAsync(List<DayOffPreference> existing);
    Task AddDayOffPreferencesAsync(List<DayOffPreference> preferences);
}

}