using BLL.Contracts;
using DAL.Contracts;
using Domain;
using DTOs.EmployeeDtos;
using DTOs.EmployeeOptionsDtos;

namespace BLL.Services;

public class PersonalDayService : IPersonalDayService
{
    private readonly IPersonalDayRepository _personalDayRepository;
    private readonly IPersonalDayRequestRepository _personalDayRequestRepository;

    public PersonalDayService(IPersonalDayRepository personalDayRepository, IPersonalDayRequestRepository personalDayRequestRepository)
    {
        _personalDayRepository = personalDayRepository;
        _personalDayRequestRepository = personalDayRequestRepository;
    }
    
    // Personal Days (approved)
    public async Task<List<BllPersonalDayDto>> GetPersonalDays(int employeeId)
    {
        var data = await _personalDayRepository.GetPersonalDaysAsync(employeeId);
        return data.Select(p => new BllPersonalDayDto
        {
            Id = p.Id,
            EmployeeId = p.EmployeeId,
            StartDate = p.StartDate,
            EndDate = p.EndDate
        }).ToList();
    }

    public async Task<int> AddApprovedPersonalDay(int employeeId, BllPersonalDayCreateDto dto)
    {
        var entity = new PersonalDay
        {
            EmployeeId = employeeId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate
        };

        await _personalDayRepository.AddPersonalDayAsync(entity);
        return entity.Id;
    }

    public async Task<bool> DeletePersonalDay(int employeeId, int personalDayId)
    {
        var personalDay = await _personalDayRepository.GetPersonalDayByIdAsync(personalDayId);
        if (personalDay == null || personalDay.EmployeeId != employeeId) return false;

        await _personalDayRepository.DeletePersonalDayAsync(personalDay);
        return true;
    }

    // Personal Day Requests
    public async Task<List<PersonalDayRequestDto>> GetPersonalDayRequests(int employeeId)
    {
        var data = await _personalDayRequestRepository.GetPersonalDayRequestsAsync(employeeId);
        return data.Select(p => new PersonalDayRequestDto
        {
            Id = p.Id,
            EmployeeId = p.EmployeeId,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            Accepted = p.Accepted,
            Rejected = p.Rejected
        }).ToList();
    }

    public async Task<BllResult<int>> AddPersonalDayRequest(int employeeId, PersonalDayRequestCreateDto dto)
    {
        if (dto.StartDate > dto.EndDate)
            return BllResult<int>.Fail("Start date cannot be after end date");

        if (dto.StartDate < DateOnly.FromDateTime(DateTime.UtcNow))
            return BllResult<int>.Fail("Start date must be in the future");

        var existingRequests = await _personalDayRequestRepository.GetPersonalDayRequestsAsync(employeeId);
        var activeRequests = existingRequests.Where(r => !r.Rejected && !r.Accepted).ToList();

        if (activeRequests.Any())
            return BllResult<int>.Fail("You already have a pending personal day request");

        var nonRejectedRequests = existingRequests.Where(r => !r.Rejected).ToList();
        var approvedPersonalDays = await _personalDayRepository.GetPersonalDaysAsync(employeeId);

        var hasOverlap = nonRejectedRequests.Any(r =>
            dto.StartDate <= r.EndDate && dto.EndDate >= r.StartDate);
        if (!hasOverlap)
            hasOverlap = approvedPersonalDays.Any(pd =>
                dto.StartDate <= pd.EndDate && dto.EndDate >= pd.StartDate);

        if (hasOverlap)
            return BllResult<int>.Fail("The requested dates overlap with an existing personal day or request");

        var request = new PersonalDayRequest
        {
            EmployeeId = employeeId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Accepted = false,
            Rejected = false
        };

        await _personalDayRequestRepository.AddPersonalDayRequestAsync(request);
        return BllResult<int>.Ok(request.Id);
    }

    public async Task<List<PersonalDayRequestDto>> GetPendingPersonalDayRequestsByOrganization(int organizationId)
    {
        var data = await _personalDayRequestRepository.GetPendingPersonalDayRequestsByOrganizationAsync(organizationId);
        return data.Select(p => new PersonalDayRequestDto
        {
            Id = p.Id,
            EmployeeId = p.EmployeeId,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            Accepted = p.Accepted,
            Rejected = p.Rejected
        }).ToList();
    }

    public async Task<int?> ApprovePersonalDayRequest(int requestId)
    {
        var request = await _personalDayRequestRepository.GetPersonalDayRequestByIdAsync(requestId);
        if (request == null) return null;

        var personalDay = new PersonalDay
        {
            EmployeeId = request.EmployeeId,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };
        await _personalDayRepository.AddPersonalDayAsync(personalDay);

        await _personalDayRequestRepository.DeletePersonalDayRequestAsync(request);
        return request.EmployeeId;
    }

    public async Task<int?> RejectPersonalDayRequest(int requestId)
    {
        var request = await _personalDayRequestRepository.GetPersonalDayRequestByIdAsync(requestId);
        if (request == null) return null;

        request.Rejected = true;
        request.Accepted = false;
        await _personalDayRequestRepository.UpdatePersonalDayRequestAsync(request);
        return request.EmployeeId;
    }

    public async Task<bool> DeletePersonalDayRequest(int employeeId, int requestId)
    {
        var request = await _personalDayRequestRepository.GetPersonalDayRequestByIdAsync(employeeId, requestId);
        if (request == null) return false;

        await _personalDayRequestRepository.DeletePersonalDayRequestAsync(request);
        return true;
    }
}
