using DTOs;

namespace BLL.Contracts;

public interface IShiftTemplateService
{
    Task<BllShiftTemplate?> GetByIdAsync(int id);
    Task<List<BllShiftTemplate>> GetByDepartmentIdAsync(int departmentId);
    Task<List<BllShiftTemplate>> GetByOrganizationIdAsync(int organizationId);
    Task<BllShiftTemplate> CreateAsync(BllShiftTemplateCreate dto);
    Task<BllShiftTemplate?> UpdateAsync(BllShiftTemplateUpdate dto);
    Task<bool> DeleteAsync(int id);
}