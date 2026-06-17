using BLL.Contracts;
using DAL.Contracts;
using Domain;
using DTOs.EmployeeDtos;
using DTOs.EmployeeOptionsDtos;

namespace BLL.Services;

public class SickLeaveService : ISickLeaveService
{
    private readonly ISickLeaveRepository _sickLeaveRepository;
    private readonly ISickLeaveRequestRepository _sickLeaveRequestRepository;

    public SickLeaveService(ISickLeaveRepository sickLeaveRepository, ISickLeaveRequestRepository sickLeaveRequestRepository)
    {
        _sickLeaveRepository = sickLeaveRepository;
        _sickLeaveRequestRepository = sickLeaveRequestRepository;
    }
    
    // Sick Leaves
    public async Task<List<BllBllSickLeaveDto>> GetSickLeaves(int employeeId)
    {
        var list = await _sickLeaveRepository.GetSickLeavesAsync(employeeId);
        return list.Select(s => new BllBllSickLeaveDto
        {
            Id = s.Id,
            EmployeeId = s.EmployeeId,
            StartDate = s.StartDate,
            EndDate = s.EndDate
        }).ToList();
    }

    public async Task<int> AddApprovedSickLeave(int employeeId, BllSickLeaveCreateDto dto)
    {
        var leave = new SickLeave
        {
            EmployeeId = employeeId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate
        };

        await _sickLeaveRepository.AddSickLeaveAsync(leave);
        return leave.Id;
    }

    public async Task<bool> DeleteSickLeave(int employeeId, int sickLeaveId)
    {
        var leave = await _sickLeaveRepository.GetSickLeaveByIdAsync(sickLeaveId);
        if (leave == null || leave.EmployeeId != employeeId) return false;

        await _sickLeaveRepository.DeleteSickLeaveAsync(leave);
        return true;
    }

    // Sick Leave Requests
    public async Task<List<SickLeaveRequestDto>> GetSickLeaveRequests(int employeeId)
    {
        var data = await _sickLeaveRequestRepository.GetSickLeaveRequestsAsync(employeeId);
        return data.Select(s => new SickLeaveRequestDto
        {
            Id = s.Id,
            EmployeeId = s.EmployeeId,
            StartDate = s.StartDate,
            EndDate = s.EndDate,
            Accepted = s.Accepted,
            Rejected = s.Rejected
        }).ToList();
    }

    public async Task<BllResult<int>> AddSickLeaveRequest(int employeeId, SickLeaveRequestCreateDto dto)
    {
        if (dto.StartDate > dto.EndDate)
            return BllResult<int>.Fail("Start date cannot be after end date");

        if (dto.StartDate < DateOnly.FromDateTime(DateTime.UtcNow))
            return BllResult<int>.Fail("Start date must be in the future");

        var existingRequests = await _sickLeaveRequestRepository.GetSickLeaveRequestsAsync(employeeId);
        var activeRequests = existingRequests.Where(r => !r.Rejected && !r.Accepted).ToList();

        if (activeRequests.Any())
            return BllResult<int>.Fail("You already have a pending sick leave request");

        var nonRejectedRequests = existingRequests.Where(r => !r.Rejected).ToList();
        var approvedSickLeaves = await _sickLeaveRepository.GetSickLeavesAsync(employeeId);

        var hasOverlap = nonRejectedRequests.Any(r =>
            dto.StartDate <= r.EndDate && dto.EndDate >= r.StartDate);
        if (!hasOverlap)
            hasOverlap = approvedSickLeaves.Any(sl =>
                dto.StartDate <= sl.EndDate && dto.EndDate >= sl.StartDate);

        if (hasOverlap)
            return BllResult<int>.Fail("The requested dates overlap with an existing sick leave or request");

        var request = new SickLeaveRequest
        {
            EmployeeId = employeeId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Accepted = false,
            Rejected = false
        };

        await _sickLeaveRequestRepository.AddSickLeaveRequestAsync(request);
        return BllResult<int>.Ok(request.Id);
    }

    public async Task<List<SickLeaveRequestDto>> GetPendingSickLeaveRequestsByOrganization(int organizationId)
    {
        var data = await _sickLeaveRequestRepository.GetPendingSickLeaveRequestsByOrganizationAsync(organizationId);
        return data.Select(s => new SickLeaveRequestDto
        {
            Id = s.Id,
            EmployeeId = s.EmployeeId,
            StartDate = s.StartDate,
            EndDate = s.EndDate,
            Accepted = s.Accepted,
            Rejected = s.Rejected
        }).ToList();
    }

    public async Task<int?> ApproveSickLeaveRequest(int requestId)
    {
        var request = await _sickLeaveRequestRepository.GetSickLeaveRequestByIdAsync(requestId);
        if (request == null) return null;

        var sickLeave = new SickLeave
        {
            EmployeeId = request.EmployeeId,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };
        await _sickLeaveRepository.AddSickLeaveAsync(sickLeave);

        await _sickLeaveRequestRepository.DeleteSickLeaveRequestAsync(request);
        return request.EmployeeId;
    }

    public async Task<int?> RejectSickLeaveRequest(int requestId)
    {
        var request = await _sickLeaveRequestRepository.GetSickLeaveRequestByIdAsync(requestId);
        if (request == null) return null;

        request.Rejected = true;
        request.Accepted = false;
        await _sickLeaveRequestRepository.UpdateSickLeaveRequestAsync(request);
        return request.EmployeeId;
    }

    public async Task<bool> DeleteSickLeaveRequest(int employeeId, int requestId)
    {
        var request = await _sickLeaveRequestRepository.GetSickLeaveRequestByIdAsync(employeeId, requestId);
        if (request == null) return false;

        await _sickLeaveRequestRepository.DeleteSickLeaveRequestAsync(request);
        return true;
    }
}
