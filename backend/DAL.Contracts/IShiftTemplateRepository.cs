using DAL.DTO.ShiftTypeDtos;

namespace DAL.Contracts
{
    public interface IShiftTemplateRepository
    {
        Task<DalShiftTemplate?> GetByIdAsync(int id);
        Task<List<DalShiftTemplate>> GetByDepartmentIdAsync(int departmentId);
        Task<List<DalShiftTemplate>> GetByOrganizationIdAsync(int organizationId);
        Task<DalShiftTemplate> CreateAsync(DalShiftTemplateCreate createDto);
        Task<DalShiftTemplate?> UpdateAsync(DalShiftTemplateUpdate updateDto);
        Task<bool> DeleteAsync(int id);
    }
}
