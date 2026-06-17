using BLL.Contracts;
using DAL.Contracts;
using Domain;
using DTOs;
using Mappers;

namespace BLL.Services;

public class ShiftTemplateService : IShiftTemplateService
{
    private readonly IShiftTemplateRepository _repository;

    public ShiftTemplateService(IShiftTemplateRepository repository)
    {
        _repository = repository;
    }

    public async Task<BllShiftTemplate?> GetByIdAsync(int id)
    {
        var shift = await _repository.GetByIdAsync(id);
        return shift == null ? null : ShiftTypeMapper.MapToBll(shift);
    }

    public async Task<List<BllShiftTemplate>> GetByDepartmentIdAsync(int departmentId)
    {
        var shifts = await _repository.GetByDepartmentIdAsync(departmentId);
        return shifts.Select(ShiftTypeMapper.MapToBll).ToList();
    }

    public async Task<List<BllShiftTemplate>> GetByOrganizationIdAsync(int organizationId)
    {
        var shifts = await _repository.GetByOrganizationIdAsync(organizationId);
        return shifts.Select(ShiftTypeMapper.MapToBll).ToList();
    }

    public async Task<BllShiftTemplate> CreateAsync(BllShiftTemplateCreate dto)
    {
        var created = await _repository.CreateAsync(ShiftTypeMapper.MapToDal(dto));
        return ShiftTypeMapper.MapToBll(created);
    }

    public async Task<BllShiftTemplate?> UpdateAsync(BllShiftTemplateUpdate dto)
    {
        var updated = await _repository.UpdateAsync(ShiftTypeMapper.MapToDal(dto));
        return updated == null ? null : ShiftTypeMapper.MapToBll(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }
}