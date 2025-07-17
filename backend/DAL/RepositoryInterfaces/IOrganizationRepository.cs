using DAL.DTO.OrganizationDtos;
using Domain;
using DTOs.OrganizationDtos;

namespace DAL.RepositoryInterfaces
{
    public interface IOrganizationRepository
    {
        Task<List<DalOrganizationInfo>> GetAllAsync();
        Task<DalOrganizationInfo?> GetByIdAsync(int id);
        Task<Organization?> GetByNameAsync(string name);
        Task<int> GetEmployerIdByOrganizationIdAsync(int organizationId);
        Task<List<DalOrganizationInfo>> GetAllByEmployerIdAsync(int employerId);
        Task<List<DalHoliday>> GetHolidaysByOrganizationIdAsync(int organizationId);
        Task<List<DalWorkDay>> GetWorkScheduleByOrganizationIdAsync(int organizationId);
        Task<int> GetNewCountLastMonthAsync();
        Task<int> GetCountWithoutEmployeesAsync();
        Task<DalOrganizationEntitiesCheckResult> CheckOrganizationEntities(int orgId);
        Task<bool> IsOrganizationOwnedByEmployerAsync(int orgId, int employerId);
        Task<bool> CreateAsync(DalOrganizationCreate createDto);
        Task<bool> UpdateAsync(DalOrganizationUpdate updateDto);
        Task<bool> DeleteAsync(int id);
    }
}