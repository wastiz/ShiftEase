using BLL.Mappers;
using BLL.Contracts;
using DAL.Contracts;
using Domain;
using DTOs.EmployeeOptionsDtos;

namespace BLL.Services;

public class PreferenceService : IPreferenceService
{
    private readonly IWeekDayPreferenceRepository _wkpRepository;
    private readonly IShiftTypePreferenceRepository _stpRepository;

    public PreferenceService(IWeekDayPreferenceRepository wkpRepository, IShiftTypePreferenceRepository stpRepository)
    {
        _wkpRepository = wkpRepository;
        _stpRepository = stpRepository;
    }
    
    public async Task<BllPreferenceBundle> GetPreferences(int employeeId)
    {
        var bundle = EmployeeOptionsMapper.MapToBll(await _stpRepository.GetShiftTypePreferencesAsync(employeeId), await _wkpRepository.GetEmployeePreferredDaysOfWeekAsync(employeeId));

        return bundle;
    }

    public async Task SavePreferences(int employeeId, BllPreferenceBundle dto)
    {
        // Shift type preferences
        var existingShifts = await _stpRepository.GetShiftTypePreferencesAsync(employeeId);
        await _stpRepository.RemoveShiftTypePreferencesAsync(existingShifts);

        var shiftEntities = dto.ShiftTypePreferences.Select(p => new ShiftTypePreference
        {
            EmployeeId = employeeId,
            ShiftTypeId = p
        }).ToList();
        await _stpRepository.AddShiftTypePreferencesAsync(shiftEntities);

        // WeekDay preferences
        await _wkpRepository.RemoveAllEmployeePreferredDaysOfWeekAsync(employeeId);
        await _wkpRepository.AddEmployeeDaysOfWeekPreferencesAsync(employeeId, dto.WeekDayPreferences);
    }

    public async Task<List<DayOfWeek>> GetPreferredWeekDays(int employeeId)
    {
        return await _wkpRepository.GetEmployeePreferredDaysOfWeekAsync(employeeId);
    }
}
