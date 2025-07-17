using BLL.Mappers;
using DAL.Repositories;
using Domain;
using DTOs.GroupDtos;

namespace BLL.Services;

public class GroupService : IGroupService
{
    private readonly IGroupRepository _repository;

    public GroupService(IGroupRepository repository)
    {
        _repository = repository;
    }

    public async Task<BllGroup?> GetByIdAsync(int groupId)
    {
        var group = await _repository.GetByIdAsync(groupId);
        return group != null ? GroupMapper.MapToBll(group) : null;
    }

    public async Task<List<BllGroup>> GetAllByOrganizationIdAsync(int organizationId)
    {
        var groups = await _repository.GetAllByOrganizationIdAsync(organizationId);
        return groups.Select(g => new BllGroup
        {
            Id = g.Id,
            Name = g.Name,
            Description = g.Description,
            Color = g.Color,
        }).ToList();
    }

    public async Task<BllGroup> CreateAsync(BllGroupCreate dto, int organizationId)
    {
        var group = GroupMapper.MapToDal(dto, organizationId);
        var created = await _repository.CreateAsync(GroupMapper.MapToDal(dto, organizationId));
        return new BllGroup
        {
            Id = created.Id,
            Name = created.Name,
            Description = created.Description,
            Color = created.Color
        };
    }

    public async Task<BllGroup?> UpdateAsync(BllGroup updateDto)
    {
        var updated = await _repository.UpdateAsync(GroupMapper.MapToDal(updateDto));

        return GroupMapper.MapToBll(updated);
    }

    public async Task<bool> UpdateAutorenewStatusAsync(int groupId, bool newStatus)
    {
        return await _repository.UpdateAutorenewalAsync(groupId, newStatus);
    }

    public async Task<bool> DeleteAsync(int groupId)
    {
        return await _repository.DeleteAsync(groupId);
    }
}