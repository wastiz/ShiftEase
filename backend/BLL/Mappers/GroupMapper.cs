using DAL.DTO.EmployeeDtos;
using DAL.DTO.GroupDtos;
using Domain;
using DTOs.EmployeeDtos;
using DTOs.GroupDtos;

namespace BLL.Mappers;

public static class GroupMapper
{
    public static BllGroup MapToBll(DalGroup group)
    {
        return new BllGroup()
        {
            Id = group.Id,
            Name = group.Name,
            Description = group.Description,
            Color = group.Color,
            AutorenewSchedules = group.AutorenewSchedules
        };
    }
    
    public static List<BllGroup> MapToBll(List<DalGroup> groups)
    {
        return groups.Select(MapToBll).ToList();
    }

    public static DalGroupCreate MapToDal(BllGroupCreate dto, int organizationId)
    {
        return new DalGroupCreate()
        {
            Name = dto.Name,
            Description = dto.Description,
            Color = dto.Color,
            OrganizationId = organizationId
        };
    }

    public static DalGroup MapToDal(BllGroup dto)
    {
        return new DalGroup()
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            Color = dto.Color,
            AutorenewSchedules = dto.AutorenewSchedules
        };
    }
}
