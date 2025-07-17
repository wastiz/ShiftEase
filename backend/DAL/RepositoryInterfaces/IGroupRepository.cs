using Domain;
using System.Collections.Generic;
using System.Threading.Tasks;
using DAL.DTO.GroupDtos;

namespace DAL.Repositories
{
    public interface IGroupRepository
    {
        Task<DalGroup> GetByIdAsync(int groupId);
        Task<List<DalGroup>> GetAllByOrganizationIdAsync(int organizationId);
        Task<DalGroup> CreateAsync(DalGroupCreate group);
        Task<bool> UpdateAutorenewalAsync(int groupId, bool autorenewal);
        Task<DalGroup> UpdateAsync(DalGroup updatedGroup);
        Task<bool> DeleteAsync(int groupId);
    }
}