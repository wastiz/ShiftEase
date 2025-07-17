using DAL.DTO.ShiftTypeDtos;
using Domain;

namespace DAL.RepositoryInterfaces
{
    public interface IShiftTypeRepository
    {
        Task<DalShiftType?> GetByIdAsync(int id);
        Task<List<DalShiftType>> GetByOrganizationIdAsync(int organizationId);
        Task<DalShiftType> CreateAsync(DalShiftType createDto);
        Task<DalShiftType?> UpdateAsync(DalShiftType updateDto);
        Task<bool> DeleteAsync(int id);
    }
}