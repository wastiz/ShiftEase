using DAL.DTO.DepartmentDtos;
using DTOs;
using DTOs.DepartmentDtos;
using Mappers;

namespace BLL.Mappers;

public static class DepartmentMapper
{
    public static BllDepartment MapToBll(DalDepartment department)
    {
        return new BllDepartment()
        {
            Id = department.Id,
            Name = department.Name,
            Description = department.Description,
            Color = department.Color,
            AutorenewSchedules = department.AutorenewSchedules,
            StartTime = department.StartTime,
            EndTime = department.EndTime,
            WorkingDays = department.WorkingDays,
            DefaultSchedulePattern = department.DefaultSchedulePattern,
            ShiftTypes = department.ShiftTypes?.Select(ShiftTypeMapper.MapToBll).ToList() ?? new()
        };
    }
    
    public static List<BllDepartment> MapToBll(List<DalDepartment> departments)
    {
        return departments.Select(MapToBll).ToList();
    }

    public static DalDepartmentCreate MapToDal(BllDepartmentCreate dto, int organizationId)
    {
        return new DalDepartmentCreate()
        {
            Name = dto.Name,
            Description = dto.Description,
            Color = dto.Color,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            WorkingDays = dto.WorkingDays,
            DefaultSchedulePattern = dto.DefaultSchedulePattern,
            OrganizationId = organizationId
        };
    }

    public static DalDepartment MapToDal(BllDepartment dto)
    {
        return new DalDepartment()
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            Color = dto.Color,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            WorkingDays = dto.WorkingDays,
            DefaultSchedulePattern = dto.DefaultSchedulePattern,
            AutorenewSchedules = dto.AutorenewSchedules
        };
    }
}
