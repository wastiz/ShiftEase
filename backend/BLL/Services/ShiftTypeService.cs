using BLL.Interfaces;
using DAL.Repositories;
using DAL.RepositoryInterfaces;
using Domain;
using DTOs;
using Mappers;

namespace BLL.Services;

public class ShiftTypeService : IShiftTypeService
{
    private readonly IShiftTypeRepository _repository;

    public ShiftTypeService(IShiftTypeRepository repository)
    {
        _repository = repository;
    }

    public async Task<BllShiftType?> GetByIdAsync(int id)
    {
        var shift = await _repository.GetByIdAsync(id);
        return ShiftTypeMapper.MapToBll(shift);
    }

    public async Task<List<BllShiftType>> GetByOrganizationIdAsync(int organizationId)
    {
        var shifts = await _repository.GetByOrganizationIdAsync(organizationId);
        return shifts.Select(ShiftTypeMapper.MapToBll).ToList();
    }

    public async Task<BllShiftType> CreateAsync(int orgId, BllShiftType dto)
    {
        var created = await _repository.CreateAsync(ShiftTypeMapper.MapToDal(dto , orgId));
        return ShiftTypeMapper.MapToBll(created);
    }

    public async Task<BllShiftType?> UpdateAsync(int id, BllShiftType dto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return null;

        existing.Name = dto.Name;
        existing.StartTime = dto.StartTime;
        existing.EndTime = dto.EndTime;
        existing.EmployeeNeeded = dto.EmployeeNeeded;
        existing.Color = dto.Color;

        var updated = await _repository.UpdateAsync(existing);
        return ShiftTypeMapper.MapToBll(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }
}