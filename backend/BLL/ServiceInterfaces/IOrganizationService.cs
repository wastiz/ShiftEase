using DAL.DTO.OrganizationDtos;
using DTOs.OrganizationDtos;

namespace BLL.ServiceInterfaces;

public interface IOrganizationService
{
    Task<List<DalOrganizationInfo>> GetAllByEmployerIdAsync(int employerId);
    
    Task<BllOrganizationWithWorkDays> GetOrganizationFullInfoByIdAsync(int orgId);
    
    Task<bool> CreateAsync(BllOrganizationCreate dto);
    
    Task<bool> UpdateAsync(BllOrganizationUpdate dto, int employerId);
    
    Task<DalOrganizationEntitiesCheckResult> CheckOrganizationEntities(int orgId);
    
    Task<List<DalHoliday>> GetOrganizationHolidaysByIdAsync(int orgId);
    
    Task<List<DalWorkDay>> GetOrganizationWorkDaysByIdAsync(int orgId);
    
    Task<bool> DeleteAsync(int id);
}