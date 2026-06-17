using BLL.Mappers;
using BLL.Contracts;
using BLL.Exceptions;
using DAL.Contracts;
using DTOs.EmployeeDtos;
using DTOs.OrganizationDtos;

namespace BLL.Services;

public class OrganizationService : IOrganizationService
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IEmployerRepository _employerRepository;
    public OrganizationService(IOrganizationRepository repository, IEmployerRepository employerRepository)
    {
        _organizationRepository = repository;
        _employerRepository = employerRepository;
    }

    public async Task<List<BllOrganization>> GetAllByEmployerIdAsync(int employerId)
    {
        return OrganizationMapper.MapToBll(await _organizationRepository.GetAllByEmployerIdAsync(employerId));
    }
    
    public async Task<BllOrganization> GetOrganizationByIdAsync(int orgId)
    {
        return OrganizationMapper.MapToBll(await _organizationRepository.GetByIdAsync(orgId));
    }

    public async Task<BllOrganizationDashboardData> GetOrganizationDataByIdAsync(int organizationId)
    {
        return OrganizationMapper.MapToBll(await _organizationRepository.GetOrganizationDataByIdAsync(organizationId));
    }

    public async Task<BllOrganization> CreateAsync(BllOrganizationCreate dto)
    {
        var org = OrganizationMapper.MapToBll(await _organizationRepository.CreateAsync(OrganizationMapper.MapToDal(dto)));

        return org;
    }

    public async Task<BllOrganization> UpdateAsync(BllOrganizationUpdate dto, int employerId)
    {
        var belongs = await _organizationRepository.IsOrganizationBelongsToEmployerAsync(dto.Id, employerId);
        if (!belongs) throw new ForbiddenException("You do not have access to this organization");
        
        var updated = await _organizationRepository.UpdateAsync(OrganizationMapper.MapToDal(dto));
        return OrganizationMapper.MapToBll(updated);
    }

    public async Task<DalOrganizationEntitiesCheckResult> CheckOrganizationEntities(int orgId)
    {
        return await _organizationRepository.CheckOrganizationEntities(orgId);
    }

    public async Task<List<BllHoliday>> GetOrganizationHolidaysByIdAsync(int orgId)
    {
        return OrganizationMapper.MapToBll(await _organizationRepository.GetHolidaysByOrganizationIdAsync(orgId));
    }

    public async Task<List<BllWorkDay>> GetOrganizationWorkDaysByIdAsync(int orgId)
    {
        return OrganizationMapper.MapToBll(await _organizationRepository.GetWorkScheduleByOrganizationIdAsync(orgId));
    }

    public async Task<int> GetEmployerIdByOrganizationIdAsync(int orgId)
    {
        return await _organizationRepository.GetEmployerIdByOrganizationIdAsync(orgId);
    }

    public async Task<bool> IsOrganizationBelongsToEmployerAsync(int orgId, int employerId)
    {
        return await _organizationRepository.IsOrganizationBelongsToEmployerAsync(orgId, employerId);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _organizationRepository.DeleteAsync(id);
    }

    public async Task<BllResult<bool>> DeleteWithPasswordAsync(int orgId, int employerId, string password)
    {
        var employer = await _employerRepository.GetEmployerEntityByIdAsync(employerId);
        if (employer == null)
            return BllResult<bool>.Fail("Employer not found.");

        if (employer.Password == null)
            return BllResult<bool>.Fail("This account does not have a password set. Please contact support.");

        if (!await _employerRepository.VerifyPasswordByIdAsync(employerId, password))
            return BllResult<bool>.Fail("Incorrect password.");

        var deleted = await _organizationRepository.DeleteAsync(orgId);
        if (!deleted)
            return BllResult<bool>.Fail("Organization not found.");

        return BllResult<bool>.Ok(true, "Organization and all its data have been permanently deleted.");
    }
}
