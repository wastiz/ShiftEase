using DAL;
using DAL.Repositories;
using Domain;
using Microsoft.EntityFrameworkCore;

public class EmployeeOptionsRepository : IEmployeeOptionsRepository
{
    private readonly AppDbContext _context;

    public EmployeeOptionsRepository(AppDbContext context)
    {
        _context = context;
    }

    // Vacation
    public async Task<List<VacationRequest>> GetVacationRequestsAsync(int employeeId)
    {
        return await _context.VacationRequests
            .Where(v => v.EmployeeId == employeeId)
            .ToListAsync();
    }

    public async Task<VacationRequest?> GetVacationRequestByIdAsync(int employeeId, int vacationRequestId)
    {
        return await _context.VacationRequests
            .FirstOrDefaultAsync(v => v.Id == vacationRequestId && v.EmployeeId == employeeId);
    }

    public async Task AddVacationRequestAsync(VacationRequest vacationRequest)
    {
        await _context.VacationRequests.AddAsync(vacationRequest);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteVacationRequestAsync(VacationRequest request)
    {
        _context.VacationRequests.Remove(request);
        await _context.SaveChangesAsync();
    }

    // Sick Leaves
    public async Task<List<SickLeave>> GetSickLeavesAsync(int employeeId)
    {
        return await _context.SickLeaves
            .Where(s => s.EmployeeId == employeeId)
            .ToListAsync();
    }

    public async Task<SickLeave?> GetSickLeaveByIdAsync(int employeeId, int sickLeaveId)
    {
        return await _context.SickLeaves
            .FirstOrDefaultAsync(s => s.Id == sickLeaveId && s.EmployeeId == employeeId);
    }

    public async Task AddSickLeaveAsync(SickLeave sickLeave)
    {
        await _context.SickLeaves.AddAsync(sickLeave);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteSickLeaveAsync(SickLeave leave)
    {
        _context.SickLeaves.Remove(leave);
        await _context.SaveChangesAsync();
    }

    // Week Day Preferences
    public async Task<List<WeekDayPreference>> GetWeekDayPreferencesAsync(int employeeId)
    {
        return await _context.WeekDayPreferences
            .Include(p => p.WeekDay)
            .Where(p => p.EmployeeId == employeeId)
            .ToListAsync();
    }

    public async Task<List<WeekDay>> GetWeekDaysByNamesAsync(List<string> names)
    {
        return await _context.WeekDays
            .Where(wd => names.Contains(wd.Name))
            .ToListAsync();
    }

    public async Task RemoveAllWeekDayPreferencesAsync(int employeeId)
    {
        var existing = await _context.WeekDayPreferences
            .Where(p => p.EmployeeId == employeeId)
            .ToListAsync();

        _context.WeekDayPreferences.RemoveRange(existing);
        await _context.SaveChangesAsync();
    }

    public async Task AddWeekDayPreferencesAsync(List<WeekDayPreference> preferences)
    {
        await _context.WeekDayPreferences.AddRangeAsync(preferences);
        await _context.SaveChangesAsync();
    }

    // Shift Type Preferences
    public async Task<List<ShiftTypePreference>> GetShiftTypePreferencesAsync(int employeeId)
    {
        return await _context.ShiftTypePreferences.Where(p => p.EmployeeId == employeeId).ToListAsync();
    }

    public async Task RemoveShiftTypePreferencesAsync(List<ShiftTypePreference> preferences)
    {
        _context.ShiftTypePreferences.RemoveRange(preferences);
        await _context.SaveChangesAsync();
    }

    public async Task AddShiftTypePreferencesAsync(List<ShiftTypePreference> preferences)
    {
        await _context.ShiftTypePreferences.AddRangeAsync(preferences);
        await _context.SaveChangesAsync();
    }

    // Day Off Preferences
    public async Task<List<DayOffPreference>> GetDayOffPreferencesAsync(int employeeId)
    {
        return await _context.DayOffPreferences.Where(p => p.EmployeeId == employeeId).ToListAsync();
    }

    public async Task RemoveDayOffPreferencesAsync(List<DayOffPreference> preferences)
    {
        _context.DayOffPreferences.RemoveRange(preferences);
        await _context.SaveChangesAsync();
    }

    public async Task AddDayOffPreferencesAsync(List<DayOffPreference> preferences)
    {
        await _context.DayOffPreferences.AddRangeAsync(preferences);
        await _context.SaveChangesAsync();
    }
}
