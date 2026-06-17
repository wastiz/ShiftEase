using DAL.Contracts;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class VacationRequestRepository : IVacationRequestRepository
{
    private readonly AppDbContext _context;

    public VacationRequestRepository(AppDbContext context)
    {
        _context = context;
    }
    
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

    public async Task<VacationRequest?> GetVacationRequestByIdAsync(int vacationRequestId)
    {
        return await _context.VacationRequests
            .FirstOrDefaultAsync(v => v.Id == vacationRequestId);
    }

    public async Task<List<VacationRequest>> GetPendingVacationRequestsByOrganizationAsync(int organizationId)
    {
        return await _context.VacationRequests
            .Where(v => !v.Accepted && !v.Rejected && _context.Employees.Any(e => e.Id == v.EmployeeId && e.OrganizationId == organizationId))
            .ToListAsync();
    }

    public async Task AddVacationRequestAsync(VacationRequest vacationRequest)
    {
        await _context.VacationRequests.AddAsync(vacationRequest);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateVacationRequestAsync(VacationRequest request)
    {
        _context.VacationRequests.Update(request);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteVacationRequestAsync(VacationRequest request)
    {
        _context.VacationRequests.Remove(request);
        await _context.SaveChangesAsync();
    }
}
