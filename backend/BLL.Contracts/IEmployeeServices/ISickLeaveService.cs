using DTOs.EmployeeDtos;
using DTOs.EmployeeOptionsDtos;

namespace BLL.Contracts;

public interface ISickLeaveService
{
    // Sick Leaves (approved)
    Task<List<BllBllSickLeaveDto>> GetSickLeaves(int employeeId);
    Task<int> AddApprovedSickLeave(int employeeId, BllSickLeaveCreateDto dto);
    Task<bool> DeleteSickLeave(int employeeId, int sickLeaveId);

    // Sick Leave Requests
    Task<List<SickLeaveRequestDto>> GetSickLeaveRequests(int employeeId);
    Task<List<SickLeaveRequestDto>> GetPendingSickLeaveRequestsByOrganization(int organizationId);
    Task<BllResult<int>> AddSickLeaveRequest(int employeeId, SickLeaveRequestCreateDto dto);
    Task<int?> ApproveSickLeaveRequest(int requestId);
    Task<int?> RejectSickLeaveRequest(int requestId);
    Task<bool> DeleteSickLeaveRequest(int employeeId, int requestId);
}
