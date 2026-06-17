using DAL.DTO.DepartmentDtos;
using DAL.DTO.ShiftTypeDtos;
using Domain;

namespace DAL.Mappers;

public static class DepartmentMapper
{
    public static DalDepartment ToDal(Department d) => new()
    {
        Id = d.Id,
        Name = d.Name,
        Description = d.Description,
        Color = d.Color,
        AutorenewSchedules = d.AutorenewSchedules,
        StartTime = d.StartTime,
        EndTime = d.EndTime,
        WorkingDays = d.WorkingDays,
        DefaultSchedulePattern = d.DefaultSchedulePattern,
        ShiftTypes = d.ShiftTemplates?
            .Select(ShiftTemplateMapper.ToDal).ToList() ?? new()
    };

    public static Department ToDomain(DalDepartmentCreate dto) => new()
    {
        Name = dto.Name,
        Description = dto.Description,
        Color = dto.Color,
        AutorenewSchedules = dto.AutorenewSchedules,
        StartTime = dto.StartTime,
        EndTime = dto.EndTime,
        WorkingDays = dto.WorkingDays,
        DefaultSchedulePattern = dto.DefaultSchedulePattern,
        OrganizationId = dto.OrganizationId
    };
}
