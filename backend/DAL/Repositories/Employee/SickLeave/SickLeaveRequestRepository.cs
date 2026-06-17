using DAL.Contracts;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.SickLeave;

public class SickLeaveRequestRepository : ISickLeaveRequestRepository
{
    private readonly AppDbContext _context;

    public SickLeaveRequestRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<SickLeaveRequest>> GetSickLeaveRequestsAsync(int employeeId)
    {
        return await _context.SickLeaveRequests
            .Where(s => s.EmployeeId == employeeId)
            .ToListAsync();
    }

    public async Task<SickLeaveRequest?> GetSickLeaveRequestByIdAsync(int employeeId, int sickLeaveRequestId)
    {
        return await _context.SickLeaveRequests
            .FirstOrDefaultAsync(s => s.Id == sickLeaveRequestId && s.EmployeeId == employeeId);
    }

    public async Task<SickLeaveRequest?> GetSickLeaveRequestByIdAsync(int sickLeaveRequestId)
    {
        return await _context.SickLeaveRequests
            .FirstOrDefaultAsync(s => s.Id == sickLeaveRequestId);
    }

    public async Task<List<SickLeaveRequest>> GetPendingSickLeaveRequestsByOrganizationAsync(int organizationId)
    {
        return await _context.SickLeaveRequests
            .Where(s => !s.Accepted && !s.Rejected && _context.Employees.Any(e => e.Id == s.EmployeeId && e.OrganizationId == organizationId))
            .ToListAsync();
    }

    public async Task AddSickLeaveRequestAsync(SickLeaveRequest sickLeaveRequest)
    {
        await _context.SickLeaveRequests.AddAsync(sickLeaveRequest);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateSickLeaveRequestAsync(SickLeaveRequest request)
    {
        _context.SickLeaveRequests.Update(request);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteSickLeaveRequestAsync(SickLeaveRequest request)
    {
        _context.SickLeaveRequests.Remove(request);
        await _context.SaveChangesAsync();
    }

}
