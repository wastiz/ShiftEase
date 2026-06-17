using DAL.Contracts;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.SickLeave;

public class SickLeaveRepository : ISickLeaveRepository
{
    private readonly AppDbContext _context;

    public SickLeaveRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<Domain.SickLeave>> GetSickLeavesAsync(int employeeId)
    {
        return await _context.SickLeaves
            .Where(s => s.EmployeeId == employeeId)
            .ToListAsync();
    }

    public async Task<Domain.SickLeave?> GetSickLeaveByIdAsync(int sickLeaveId)
    {
        return await _context.SickLeaves
            .FirstOrDefaultAsync(s => s.Id == sickLeaveId);
    }

    public async Task AddSickLeaveAsync(Domain.SickLeave sickLeave)
    {
        await _context.SickLeaves.AddAsync(sickLeave);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteSickLeaveAsync(Domain.SickLeave leave)
    {
        _context.SickLeaves.Remove(leave);
        await _context.SaveChangesAsync();
    }
}
