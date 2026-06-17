using DTOs.DepartmentDtos;

namespace BLL.Contracts;

public interface IDepartmentService
{
    Task<BllDepartment?> GetByIdAsync(int departmentId);
    Task<List<BllDepartment>> GetAllByOrganizationIdAsync(int organizationId);
    Task<BllDepartment> CreateAsync(BllDepartmentCreate dto, int organizationId);
    Task<BllDepartment?> UpdateAsync(BllDepartment updateDto);
    Task<bool> UpdateAutorenewStatusAsync(int departmentId, bool newStatus);
    Task<bool> DeleteAsync(int departmentId);
    Task<bool> BelongsToOrganizationAsync(int departmentId, int orgId);
}
