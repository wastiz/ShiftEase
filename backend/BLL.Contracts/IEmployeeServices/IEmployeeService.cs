using BLL.DTO.ScheduleDtos;
using DTOs.EmployeeDtos;

namespace BLL.Contracts;

public interface IEmployeeService
{
    Task<BllEmployee?> GetByIdAsync(int id);
    Task<List<BllEmployee>> GetFullDataByOrganizationIdAsync(int organizationId);
    Task<List<BllEmployee>> GetFullDataByDepartmentIdAsync(int departmentId);
    Task<List<BllEmployeeMinData>> GetMinDataByOrganizationIdAsync(int organizationId);
    Task<List<BllEmployeeMinData>> GetMinDataByDepartmentIdAsync(int departmentId);
    Task<List<BllEmployeeMinData>> GetMinDataWithoutDepartmentsAsync(int organizationId);
    Task<List<BllEmployeeTimeOff>> GetEmployeeTimeOffAsync(int employeeId);
    Task<List<BllEmployeeTimeOff>> GetEmployeesTimeOffAsync(IEnumerable<int> employeeIds, DateOnly start, DateOnly end);
    Task<List<BllEmployeeTimeOff>> GetEmployeesTimeOffByFilterAsync(int organizationId, List<int>? employeeIds, int? year, int? month);
    Task<BllResult<bool>> CreateAsync(BllEmployeeCreate dto);
    Task<BllResult<BllEmployeeBulkCreateResult>> CreateBulkAsync(List<BllEmployeeCreate> employees, int organizationId);
    Task<BllResult<bool>> UpdateAsync(int id, BllEmployeeUpdate update);
    Task<BllResult<bool>> DeleteAsync(int id);
    Task<int?> GetOrgIdByEmployeeIdAsync(int employeeId);
    Task<bool> BelongsToOrganizationAsync(int employeeId, int orgId);
}
