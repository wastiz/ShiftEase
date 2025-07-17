using BLL.Mappers;
using BLL.ServiceInterfaces;
using DAL;
using DAL.DTO.OrganizationDtos;
using DAL.RepositoryInterfaces;
using Domain;
using DTOs.OrganizationDtos;

namespace BLL.Services;

public class OrganizationService : IOrganizationService
{
    private readonly IOrganizationRepository _organizationRepository;

    public OrganizationService(IOrganizationRepository repository)
    {
        _organizationRepository = repository;
    }

    public async Task<List<DalOrganizationInfo>> GetAllByEmployerIdAsync(int employerId)
    {
        var organizations = await _organizationRepository.GetAllByEmployerIdAsync(employerId);
        return organizations;
    }
    
    public async Task<BllOrganizationWithWorkDays> GetOrganizationFullInfoByIdAsync(int orgId)
    {
        var dalOrganizationInfo = await _organizationRepository.GetByIdAsync(orgId);
        var dalHolidays = await _organizationRepository.GetHolidaysByOrganizationIdAsync(orgId);
        var dalWorkDays = await _organizationRepository.GetWorkScheduleByOrganizationIdAsync(orgId);
        
        var organization = new BllOrganizationWithWorkDays
        {
            OrganizationInfo = OrganizationMapper.MapToBll(dalOrganizationInfo),
            OrganizationHolidays = OrganizationMapper.MapToBll(dalHolidays),
            OrganizationWorkDays = OrganizationMapper.MapToBll(dalWorkDays)
        };
        
        return organization;
    }

    public async Task<bool> CreateAsync(BllOrganizationCreate dto)
    {
        return await _organizationRepository.CreateAsync(OrganizationMapper.MapToDal(dto));
    }
    
    public async Task<bool> UpdateAsync(BllOrganizationUpdate dto, int employerId)
    {
        var belongs = await _organizationRepository.IsOrganizationOwnedByEmployerAsync(dto.OrganizationId, employerId);
        if (belongs) return await _organizationRepository.UpdateAsync(OrganizationMapper.MapToDal(dto));
        return false;
    }

    public async Task<DalOrganizationEntitiesCheckResult> CheckOrganizationEntities(int orgId)
    {
        return await _organizationRepository.CheckOrganizationEntities(orgId);
    }

    public async Task<List<DalHoliday>> GetOrganizationHolidaysByIdAsync(int orgId)
    {
        return await _organizationRepository.GetHolidaysByOrganizationIdAsync(orgId);
    }

    public async Task<List<DalWorkDay>> GetOrganizationWorkDaysByIdAsync(int orgId)
    {
        return await _organizationRepository.GetWorkScheduleByOrganizationIdAsync(orgId);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }
}
