using DTOs.EmployeeDtos;

namespace BLL.Interfaces;

public interface IEmployeeService
{
    Task<BllEmployeeFullData?> GetByIdAsync(int id);
    Task<List<BllEmployeeFullData>> GetFullDataByOrganizationIdAsync(int organizationId);
    Task<List<BllEmployeeMinData>> GetMinDataByOrganizationIdAsync(int organizationId);
    Task<List<BllEmployeeMinData>> GetMinDataByGroupIdAsync(int groupId);
    Task<BllRequestResult> CreateAsync(BllEmployeeCreate dto, int organizationId);
    Task<BllRequestResult> UpdateAsync(int id, BllEmployeeUpdate update);
    Task<BllRequestResult> DeleteAsync(int id);
}
