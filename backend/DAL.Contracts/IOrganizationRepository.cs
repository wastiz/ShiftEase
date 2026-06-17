using DAL.DTO.OrganizationDtos;
using Domain;
using DTOs.OrganizationDtos;

namespace DAL.Contracts
{
    public interface IOrganizationRepository
    {
        Task<List<DalOrganization>> GetAllAsync();
        Task<DalOrganization?> GetByIdAsync(int id);
        Task<DalOrganization?> GetByNameAsync(string name);
        Task<int> GetEmployerIdByOrganizationIdAsync(int organizationId);
        Task<List<DalOrganization>> GetAllByEmployerIdAsync(int employerId);
        Task<List<DalHoliday>> GetHolidaysByOrganizationIdAsync(int organizationId);
        Task<List<DalWorkDay>> GetWorkScheduleByOrganizationIdAsync(int organizationId);
        Task<DalOrganizationData> GetOrganizationDataByIdAsync(int organizationId);
        Task<int> GetNewCountLastMonthAsync();
        Task<int> GetCountWithoutEmployeesAsync();
        Task<DalOrganizationEntitiesCheckResult> CheckOrganizationEntities(int orgId);
        Task<bool> IsOrganizationBelongsToEmployerAsync(int orgId, int employerId);
        Task<DalOrganization> CreateAsync(DalOrganizationCreate createDto);
        Task<DalOrganization> UpdateAsync(DalOrganizationUpdate updateDto);
        Task<bool> DeleteAsync(int id);
    }
}
