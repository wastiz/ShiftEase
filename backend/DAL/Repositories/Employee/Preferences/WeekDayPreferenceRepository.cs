using DAL.Contracts;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.Employee.Preferences;

public class WeekDayPreferenceRepository : IWeekDayPreferenceRepository
{
    private readonly AppDbContext _context;

    public WeekDayPreferenceRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<DayOfWeek>> GetEmployeePreferredDaysOfWeekAsync(int employeeId)
    {
        return await _context.WeekDayPreferences
            .Where(p => p.EmployeeId == employeeId)
            .Select(p => p.DayOfWeek)
            .ToListAsync();
    }

    public async Task RemoveAllEmployeePreferredDaysOfWeekAsync(int employeeId)
    {
        await _context.WeekDayPreferences
            .Where(p => p.EmployeeId == employeeId)
            .ExecuteDeleteAsync();
    }

    public async Task AddEmployeeDaysOfWeekPreferencesAsync(int employeeId, List<DayOfWeek> preferredDays)
    {
        var preferences = preferredDays.Select(day => new WeekDayPreference
        {
            EmployeeId = employeeId,
            DayOfWeek = day
        });

        await _context.WeekDayPreferences.AddRangeAsync(preferences);
        await _context.SaveChangesAsync();
    }
}
