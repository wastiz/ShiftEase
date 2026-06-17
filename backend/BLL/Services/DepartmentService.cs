using BLL.Contracts;
using BLL.Mappers;
using DAL.Contracts;
using DTOs.DepartmentDtos;

namespace BLL.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _repository;

    public DepartmentService(IDepartmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<BllDepartment?> GetByIdAsync(int departmentId)
    {
        var department = await _repository.GetByIdAsync(departmentId);
        return department != null ? DepartmentMapper.MapToBll(department) : null;
    }

    public async Task<List<BllDepartment>> GetAllByOrganizationIdAsync(int organizationId)
    {
        return DepartmentMapper.MapToBll(await _repository.GetAllByOrganizationIdAsync(organizationId));
    }

    public async Task<BllDepartment> CreateAsync(BllDepartmentCreate dto, int organizationId)
    {
        return DepartmentMapper.MapToBll(await _repository.CreateAsync(DepartmentMapper.MapToDal(dto, organizationId)));
        
    }

    public async Task<BllDepartment?> UpdateAsync(BllDepartment updateDto)
    {
        var updatedDepartment = await _repository.UpdateAsync(DepartmentMapper.MapToDal(updateDto));
        if (updatedDepartment == null)  return null;
        return DepartmentMapper.MapToBll(updatedDepartment);
    }


    public async Task<bool> UpdateAutorenewStatusAsync(int departmentId, bool newStatus)
    {
        return await _repository.UpdateAutorenewalAsync(departmentId, newStatus);
    }

    public async Task<bool> DeleteAsync(int departmentId)
    {
        return await _repository.DeleteAsync(departmentId);
    }

    public async Task<bool> BelongsToOrganizationAsync(int departmentId, int orgId)
        => await _repository.BelongsToOrganizationAsync(departmentId, orgId);
}