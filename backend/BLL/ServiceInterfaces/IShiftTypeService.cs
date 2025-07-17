using DTOs;

namespace BLL.Interfaces;

public interface IShiftTypeService
{
    Task<BllShiftType?> GetByIdAsync(int id);
    Task<List<BllShiftType>> GetByOrganizationIdAsync(int organizationId);
    Task<BllShiftType> CreateAsync(int organizationId, BllShiftType dto);
    Task<BllShiftType?> UpdateAsync(int id, BllShiftType dto);
    Task<bool> DeleteAsync(int id);
}