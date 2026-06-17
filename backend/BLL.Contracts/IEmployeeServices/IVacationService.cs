using DTOs.EmployeeDtos;
using DTOs.EmployeeOptionsDtos;

namespace BLL.Contracts;

public interface IVacationService
{
    // Vacations
    Task<List<BllVacationDto>> GetVacations(int employeeId);
    Task<int> AddApprovedVacation(int employeeId, BllVacationCreateDto dto);
    Task<bool> DeleteVacation(int employeeId, int vacationId);

    // Vacation Requests
    Task<List<VacationRequestDto>> GetVacationRequests(int employeeId);
    Task<List<VacationRequestDto>> GetPendingVacationRequestsByOrganization(int organizationId);
    Task<BllResult<int>> AddVacationRequest(int employeeId, VacationRequestCreateDto dto);
    Task<int?> ApproveVacationRequest(int requestId);
    Task<int?> RejectVacationRequest(int requestId);
    Task<bool> DeleteVacationRequest(int employeeId, int vacationId);
}
