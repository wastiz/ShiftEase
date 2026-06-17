using DAL.DTO.ShiftTypeDtos;
using Domain;
using DTOs;

namespace Mappers;

public static class ShiftTypeMapper
{
    public static BllShiftTemplate MapToBll(DalShiftTemplate shiftTemplate)
    {
        return new BllShiftTemplate
        {
            Id = shiftTemplate.Id,
            DepartmentId = shiftTemplate.DepartmentId,
            OrganizationId = shiftTemplate.OrganizationId,
            Name = shiftTemplate.Name,
            StartTime = shiftTemplate.StartTime,
            EndTime = shiftTemplate.EndTime,
            MinEmployees = shiftTemplate.MinEmployees,
            MaxEmployees = shiftTemplate.MaxEmployees,
            Color = shiftTemplate.Color,
            BreakDuration = shiftTemplate.BreakDuration
        };
    }

    public static List<BllShiftTemplate> MapToBll(List<DalShiftTemplate> shiftTypes)
    {
        return shiftTypes.Select(MapToBll).ToList();
    }

    public static DalShiftTemplateCreate MapToDal(BllShiftTemplateCreate bll)
    {
        return new DalShiftTemplateCreate()
        {
            Name = bll.Name,
            StartTime = bll.StartTime,
            EndTime = bll.EndTime,
            MinEmployees = bll.MinEmployees,
            MaxEmployees = bll.MaxEmployees,
            Color = bll.Color,
            BreakDuration = bll.BreakDuration,
            DepartmentId = bll.DepartmentId,
            OrganizationId = bll.OrganizationId
        };
    }

    public static DalShiftTemplateUpdate MapToDal(BllShiftTemplateUpdate bll)
    {
        return new DalShiftTemplateUpdate()
        {
            Id = bll.Id,
            Name = bll.Name,
            StartTime = bll.StartTime,
            EndTime = bll.EndTime,
            MinEmployees = bll.MinEmployees,
            MaxEmployees = bll.MaxEmployees,
            Color = bll.Color,
            BreakDuration = bll.BreakDuration
        };
    }
}