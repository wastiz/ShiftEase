using Domain;
using System.Collections.Generic;
using System.Threading.Tasks;
using DAL.DTO.DepartmentDtos;

namespace DAL.Contracts
{
    public interface IDepartmentRepository
    {
        Task<DalDepartment> GetByIdAsync(int departmentId);
        Task<List<DalDepartment>> GetAllByOrganizationIdAsync(int organizationId);
        Task<DalDepartment> CreateAsync(DalDepartmentCreate department);
        Task<bool> UpdateAutorenewalAsync(int departmentId, bool autorenewal);
        Task<DalDepartment> UpdateAsync(DalDepartment updatedDepartment);
        Task<bool> DeleteAsync(int departmentId);
        Task<bool> BelongsToOrganizationAsync(int departmentId, int orgId);
    }
}