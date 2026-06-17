using DTOs.EmployeeDtos;
using DTOs.EmployeeOptionsDtos;

namespace BLL.Contracts;

public interface IPersonalDayService
{
    // Personal Days (approved)
    Task<List<BllPersonalDayDto>> GetPersonalDays(int employeeId);
    Task<int> AddApprovedPersonalDay(int employeeId, BllPersonalDayCreateDto dto);
    Task<bool> DeletePersonalDay(int employeeId, int personalDayId);

    // Personal Day Requests
    Task<List<PersonalDayRequestDto>> GetPersonalDayRequests(int employeeId);
    Task<List<PersonalDayRequestDto>> GetPendingPersonalDayRequestsByOrganization(int organizationId);
    Task<BllResult<int>> AddPersonalDayRequest(int employeeId, PersonalDayRequestCreateDto dto);
    Task<int?> ApprovePersonalDayRequest(int requestId);
    Task<int?> RejectPersonalDayRequest(int requestId);
    Task<bool> DeletePersonalDayRequest(int employeeId, int requestId);
}
