using DAL.DTO.ShiftTypeDtos;
using Domain;
using DTOs;

namespace Mappers;

public static class ShiftTypeMapper
{
    public static BllShiftType MapToBll(DalShiftType shiftType)
    {
        return new BllShiftType
        {
            Id = shiftType.Id,
            Name = shiftType.Name,
            StartTime = shiftType.StartTime,
            EndTime = shiftType.EndTime,
            EmployeeNeeded = shiftType.EmployeeNeeded,
            Color = shiftType.Color
        };
    }
    
    public static List<BllShiftType> MapToBll(List<DalShiftType> shiftTypes)
    {
        return shiftTypes.Select(MapToBll).ToList();
    }

    public static DalShiftType MapToDal(BllShiftType bll, int orgId)
    {
        return new DalShiftType()
        {
            Name = bll.Name,
            StartTime = bll.StartTime,
            EndTime = bll.EndTime,
            EmployeeNeeded = bll.EmployeeNeeded,
            Color = bll.Color,
            OrganizationId = orgId
        };
    }
}