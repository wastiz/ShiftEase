using DAL.DTO.OrganizationDtos;
using DTOs.EmployeeDtos;
using DTOs.OrganizationDtos;

namespace BLL.Contracts;

public interface IOrganizationService
{
    Task<List<BllOrganization>> GetAllByEmployerIdAsync(int employerId);
    
    Task<BllOrganization> GetOrganizationByIdAsync(int orgId);
    Task<BllOrganizationDashboardData> GetOrganizationDataByIdAsync(int orgId);
    
    Task<BllOrganization> CreateAsync(BllOrganizationCreate dto);
    
    Task<BllOrganization> UpdateAsync(BllOrganizationUpdate dto, int employerId);
    
    Task<DalOrganizationEntitiesCheckResult> CheckOrganizationEntities(int orgId);
    
    Task<List<BllHoliday>> GetOrganizationHolidaysByIdAsync(int orgId);
    
    Task<List<BllWorkDay>> GetOrganizationWorkDaysByIdAsync(int orgId);
    
    Task<int> GetEmployerIdByOrganizationIdAsync(int orgId);
    Task<bool> IsOrganizationBelongsToEmployerAsync(int orgId, int employerId);

    Task<bool> DeleteAsync(int id);
    Task<BllResult<bool>> DeleteWithPasswordAsync(int orgId, int employerId, string password);
}
