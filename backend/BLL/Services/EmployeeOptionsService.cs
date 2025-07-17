using BLL.Mappers;
using BLL.ServiceInterfaces;
using DAL.Repositories;
using Domain;
using DTOs.EmployeeOptionsDtos;

public class EmployeeOptionsService : IEmployeeOptionsService
{
    private readonly IEmployeeOptionsRepository _repository;

    public EmployeeOptionsService(IEmployeeOptionsRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<VacationRequestDto>> GetVacationRequests(int employeeId)
    {
        var data = await _repository.GetVacationRequestsAsync(employeeId);
        return data.Select(v => new VacationRequestDto
        {
            Id = v.Id,
            StartDate = v.StartDate,
            EndDate = v.EndDate,
            Accepted = v.Accepted,
            Rejected = v.Rejected
        }).ToList();
    }

    public async Task<int> AddVacationRequest(int employeeId, VacationRequestDto dto)
    {
        var entity = new VacationRequest
        {
            EmployeeId = employeeId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Accepted = false,
            Rejected = false
        };

        await _repository.AddVacationRequestAsync(entity);
        return entity.Id;
    }

    public async Task<bool> DeleteVacationRequest(int employeeId, int vacationId)
    {
        var request = await _repository.GetVacationRequestByIdAsync(employeeId, vacationId);
        if (request == null) return false;

        await _repository.DeleteVacationRequestAsync(request);
        return true;
    }

    public async Task<List<SickLeaveDto>> GetSickLeaves(int employeeId)
    {
        var list = await _repository.GetSickLeavesAsync(employeeId);
        return list.Select(s => new SickLeaveDto
        {
            Id = s.Id,
            StartDate = s.StartDate,
            EndDate = s.EndDate
        }).ToList();
    }

    public async Task<int> AddSickLeave(int employeeId, SickLeaveDto dto)
    {
        var leave = new SickLeave
        {
            EmployeeId = employeeId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate
        };

        await _repository.AddSickLeaveAsync(leave);
        return leave.Id;
    }

    public async Task<bool> DeleteSickLeave(int employeeId, int id)
    {
        var leave = await _repository.GetSickLeaveByIdAsync(employeeId, id);
        if (leave == null) return false;

        await _repository.DeleteSickLeaveAsync(leave);
        return true;
    }

    public async Task<BllPreferenceBundle> GetPreferences(int employeeId)
    {
        var bundle = EmployeeOptionsMapper.MapToBll(await _repository.GetShiftTypePreferencesAsync(employeeId),
            await _repository.GetWeekDayPreferencesAsync(employeeId),
            await _repository.GetDayOffPreferencesAsync(employeeId));

        return bundle;
    }

    public async Task SavePreferences(int employeeId, BllPreferenceBundle dto)
    {
        // Shift type preferences
        var existingShifts = await _repository.GetShiftTypePreferencesAsync(employeeId);
        await _repository.RemoveShiftTypePreferencesAsync(existingShifts);

        var shiftEntities = dto.ShiftTypePreferences.Select(p => new ShiftTypePreference
        {
            EmployeeId = employeeId,
            ShiftTypeId = p
        }).ToList();
        await _repository.AddShiftTypePreferencesAsync(shiftEntities);

        // WeekDay preferences
        await _repository.RemoveAllWeekDayPreferencesAsync(employeeId);
        var weekDays = await _repository.GetWeekDaysByNamesAsync(dto.WeekDayPreferences);
        var weekDayEntities = weekDays.Select(wd => new WeekDayPreference
        {
            EmployeeId = employeeId,
            WeekDayId = wd.Id
        }).ToList();
        await _repository.AddWeekDayPreferencesAsync(weekDayEntities);

        // DayOff preferences
        var existingDaysOff = await _repository.GetDayOffPreferencesAsync(employeeId);
        await _repository.RemoveDayOffPreferencesAsync(existingDaysOff);
        var newDaysOff = dto.DayOffPreferences.Select(p => new DayOffPreference
        {
            EmployeeId = employeeId,
            Date = p
        }).ToList();
        await _repository.AddDayOffPreferencesAsync(newDaysOff);
    }

    public async Task<List<WeekDayPreferenceDto>> GetPreferredWeekDays(int employeeId)
    {
        var preferences = await _repository.GetWeekDayPreferencesAsync(employeeId);
        return preferences.Select(p => new WeekDayPreferenceDto
        {
            WeekDayName = p.WeekDay.Name
        }).ToList();
    }
}
