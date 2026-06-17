using BLL.Contracts;
using DAL.Contracts;
using Domain;
using DTOs.EmployeeDtos;
using DTOs.EmployeeOptionsDtos;

namespace BLL.Services;

public class VacationService : IVacationService
{
    private readonly IVacationRepository _vacationRepository;
    private readonly IVacationRequestRepository _vacationRequestRepository;

    public VacationService(IVacationRepository vacationRepository, IVacationRequestRepository vacationRequestRepository)
    {
        _vacationRepository = vacationRepository;
        _vacationRequestRepository = vacationRequestRepository;
    }
    
    //Vacations
    public async Task<List<BllVacationDto>> GetVacations(int employeeId)
    {
        var data = await _vacationRepository.GetVacationsAsync(employeeId);
        return data.Select(v => new BllVacationDto
        {
            Id = v.Id,
            EmployeeId = v.EmployeeId,
            StartDate = v.StartDate,
            EndDate = v.EndDate
        }).ToList();
    }

    public async Task<int> AddApprovedVacation(int employeeId, BllVacationCreateDto dto)
    {
        var entity = new Vacation
        {
            EmployeeId = employeeId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate
        };

        await _vacationRepository.AddVacationAsync(entity);
        return entity.Id;
    }

    public async Task<bool> DeleteVacation(int employeeId, int vacationId)
    {
        var vacation = await _vacationRepository.GetVacationByIdAsync(vacationId);
        if (vacation == null || vacation.EmployeeId != employeeId) return false;

        await _vacationRepository.DeleteVacationAsync(vacation);
        return true;
    }

    // Vacation Requests
    public async Task<List<VacationRequestDto>> GetVacationRequests(int employeeId)
    {
        var data = await _vacationRequestRepository.GetVacationRequestsAsync(employeeId);
        return data.Select(v => new VacationRequestDto
        {
            Id = v.Id,
            StartDate = v.StartDate,
            EndDate = v.EndDate,
            Accepted = v.Accepted,
            Rejected = v.Rejected
        }).ToList();
    }

    public async Task<BllResult<int>> AddVacationRequest(int employeeId, VacationRequestCreateDto dto)
    {
        if (dto.StartDate > dto.EndDate)
            return BllResult<int>.Fail("Start date cannot be after end date");

        if (dto.StartDate < DateOnly.FromDateTime(DateTime.UtcNow))
            return BllResult<int>.Fail("Start date must be in the future");

        var existingRequests = await _vacationRequestRepository.GetVacationRequestsAsync(employeeId);
        var activeRequests = existingRequests.Where(r => !r.Rejected && !r.Accepted).ToList();

        if (activeRequests.Any())
            return BllResult<int>.Fail("You already have a pending vacation request");

        var nonRejectedRequests = existingRequests.Where(r => !r.Rejected).ToList();
        var approvedVacations = await _vacationRepository.GetVacationsAsync(employeeId);

        var hasOverlap = nonRejectedRequests.Any(r =>
            dto.StartDate <= r.EndDate && dto.EndDate >= r.StartDate);
        if (!hasOverlap)
            hasOverlap = approvedVacations.Any(v =>
                dto.StartDate <= v.EndDate && dto.EndDate >= v.StartDate);

        if (hasOverlap)
            return BllResult<int>.Fail("The requested dates overlap with an existing vacation or request");

        var allVacationDates = nonRejectedRequests
            .Select(r => (r.StartDate, r.EndDate))
            .Concat(approvedVacations.Select(v => (v.StartDate, v.EndDate)))
            .ToList();

        foreach (var (start, end) in allVacationDates)
        {
            var daysAfterExisting = dto.StartDate.DayNumber - end.DayNumber;
            var daysBeforeExisting = start.DayNumber - dto.EndDate.DayNumber;

            if ((daysAfterExisting > 0 && daysAfterExisting < 14) ||
                (daysBeforeExisting > 0 && daysBeforeExisting < 14))
                return BllResult<int>.Fail("There must be at least 14 days between vacations");
        }

        var entity = new VacationRequest
        {
            EmployeeId = employeeId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Accepted = false,
            Rejected = false
        };

        await _vacationRequestRepository.AddVacationRequestAsync(entity);
        return BllResult<int>.Ok(entity.Id);
    }

    public async Task<List<VacationRequestDto>> GetPendingVacationRequestsByOrganization(int organizationId)
    {
        var data = await _vacationRequestRepository.GetPendingVacationRequestsByOrganizationAsync(organizationId);
        return data.Select(v => new VacationRequestDto
        {
            Id = v.Id,
            StartDate = v.StartDate,
            EndDate = v.EndDate,
            Accepted = v.Accepted,
            Rejected = v.Rejected
        }).ToList();
    }

    public async Task<int?> ApproveVacationRequest(int requestId)
    {
        var request = await _vacationRequestRepository.GetVacationRequestByIdAsync(requestId);
        if (request == null) return null;

        var vacation = new Vacation
        {
            EmployeeId = request.EmployeeId,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };
        await _vacationRepository.AddVacationAsync(vacation);

        await _vacationRequestRepository.DeleteVacationRequestAsync(request);
        return request.EmployeeId;
    }

    public async Task<int?> RejectVacationRequest(int requestId)
    {
        var request = await _vacationRequestRepository.GetVacationRequestByIdAsync(requestId);
        if (request == null) return null;

        request.Rejected = true;
        request.Accepted = false;
        await _vacationRequestRepository.UpdateVacationRequestAsync(request);
        return request.EmployeeId;
    }

    public async Task<bool> DeleteVacationRequest(int employeeId, int vacationId)
    {
        var request = await _vacationRequestRepository.GetVacationRequestByIdAsync(employeeId, vacationId);
        if (request == null) return false;

        await _vacationRequestRepository.DeleteVacationRequestAsync(request);
        return true;
    }
}
