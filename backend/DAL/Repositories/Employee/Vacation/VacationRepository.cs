using DAL.Contracts;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class VacationRepository : IVacationRepository
{
    private readonly AppDbContext _context;

    public VacationRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<Vacation>> GetVacationsAsync(int employeeId)
    {
        return await _context.Vacations
            .Where(v => v.EmployeeId == employeeId)
            .ToListAsync();
    }

    public async Task<Vacation?> GetVacationByIdAsync(int vacationId)
    {
        return await _context.Vacations
            .FirstOrDefaultAsync(v => v.Id == vacationId);
    }

    public async Task AddVacationAsync(Vacation vacation)
    {
        await _context.Vacations.AddAsync(vacation);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteVacationAsync(Vacation vacation)
    {
        _context.Vacations.Remove(vacation);
        await _context.SaveChangesAsync();
    }
}
