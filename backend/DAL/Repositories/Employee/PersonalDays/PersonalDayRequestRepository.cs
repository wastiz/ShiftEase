using DAL.Contracts;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.PersonalDays;

public class PersonalDayRequestRepository : IPersonalDayRequestRepository
{
    private readonly AppDbContext _context;

    public PersonalDayRequestRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<PersonalDayRequest>> GetPersonalDayRequestsAsync(int employeeId)
    {
        return await _context.PersonalDayRequests
            .Where(p => p.EmployeeId == employeeId)
            .ToListAsync();
    }

    public async Task<PersonalDayRequest?> GetPersonalDayRequestByIdAsync(int employeeId, int personalDayRequestId)
    {
        return await _context.PersonalDayRequests
            .FirstOrDefaultAsync(p => p.Id == personalDayRequestId && p.EmployeeId == employeeId);
    }

    public async Task<PersonalDayRequest?> GetPersonalDayRequestByIdAsync(int personalDayRequestId)
    {
        return await _context.PersonalDayRequests
            .FirstOrDefaultAsync(p => p.Id == personalDayRequestId);
    }

    public async Task<List<PersonalDayRequest>> GetPendingPersonalDayRequestsByOrganizationAsync(int organizationId)
    {
        return await _context.PersonalDayRequests
            .Where(p => !p.Accepted && !p.Rejected && _context.Employees.Any(e => e.Id == p.EmployeeId && e.OrganizationId == organizationId))
            .ToListAsync();
    }

    public async Task AddPersonalDayRequestAsync(PersonalDayRequest personalDayRequest)
    {
        await _context.PersonalDayRequests.AddAsync(personalDayRequest);
        await _context.SaveChangesAsync();
    }

    public async Task UpdatePersonalDayRequestAsync(PersonalDayRequest request)
    {
        _context.PersonalDayRequests.Update(request);
        await _context.SaveChangesAsync();
    }

    public async Task DeletePersonalDayRequestAsync(PersonalDayRequest request)
    {
        _context.PersonalDayRequests.Remove(request);
        await _context.SaveChangesAsync();
    }
}
