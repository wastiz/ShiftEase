using DAL.Contracts;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.Employee.Preferences;

public class ShiftTypePreferenceRepository : IShiftTypePreferenceRepository
{
    private readonly AppDbContext _context;

    public ShiftTypePreferenceRepository(AppDbContext context)
    {
        _context = context;
    }
    
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
}
