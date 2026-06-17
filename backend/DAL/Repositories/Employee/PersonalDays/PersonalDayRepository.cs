using DAL.Contracts;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.PersonalDays;

public class PersonalDayRepository : IPersonalDayRepository
{
    private readonly AppDbContext _context;

    public PersonalDayRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<PersonalDay>> GetPersonalDaysAsync(int employeeId)
    {
        return await _context.PersonalDays
            .Where(p => p.EmployeeId == employeeId)
            .ToListAsync();
    }

    public async Task<PersonalDay?> GetPersonalDayByIdAsync(int personalDayId)
    {
        return await _context.PersonalDays
            .FirstOrDefaultAsync(p => p.Id == personalDayId);
    }

    public async Task AddPersonalDayAsync(PersonalDay personalDay)
    {
        await _context.PersonalDays.AddAsync(personalDay);
        await _context.SaveChangesAsync();
    }

    public async Task DeletePersonalDayAsync(PersonalDay personalDay)
    {
        _context.PersonalDays.Remove(personalDay);
        await _context.SaveChangesAsync();
    }
}
