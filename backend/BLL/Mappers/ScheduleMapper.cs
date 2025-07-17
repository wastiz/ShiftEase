using BLL.DTO.ScheduleDtos;
using DAL.DTO.EmployeeDtos;
using DAL.DTO.ScheduleDtos;
using DTOs.ScheduleDtos;
using DTOs.Shifts;

namespace BLL.Mappers;

public class ScheduleMapper
{
    public static BllScheduleInfo MapToBll(DalScheduleInfo dal)
    {
        return new BllScheduleInfo()
        {
            Id = dal.Id,
            StartDate = dal.StartDate,
            EndDate = dal.EndDate,
            GroupId = dal.GroupId,
            IsConfirmed = dal.IsConfirmed,
        };
    }

    public static BllShift MapToBll(DalShift dal)
    {
        return new BllShift()
        {
            Id = dal.Id,
            Date = dal.Date,
            ShiftTypeId = dal.ShiftTypeId,
            Employees = EmployeeMapper.MapToBll(dal.Employees)
        };
    }

    public static List<BllShift> MapToBll(List<DalShift> dal)
    {
        return dal.Select(MapToBll).ToList();
    }

    public static BllFullShift MapToBll(DalFullShift dal)
    {
        return new BllFullShift()
        {
            Id = dal.Id,
            Date = dal.Date,
            ShiftTypeName = dal.ShiftTypeName,
            From = dal.From,
            To = dal.To,
            Color = dal.Color,
            EmployeeNeeded = dal.EmployeeNeeded,
            Employees = EmployeeMapper.MapToBll(dal.Employees)
        };
    }

    public static List<BllFullShift> MapToBll(List<DalFullShift> dal)
    {
        return dal.Select(MapToBll).ToList();
    }
    
    public static DalSchedulePost MapToDal(BllSchedulePost bll)
    {
        return new DalSchedulePost
        {
            GroupId = bll.GroupId,
            DateFrom = bll.DateFrom,
            DateTo = bll.DateTo,
            Autorenewal = bll.Autorenewal,
            IsConfirmed = bll.IsConfirmed,
            Shifts = bll.Shifts.Select(s => new DalSchedulePost.ShiftCreateDto
            {
                ShiftTypeId = s.ShiftTypeId,
                Date = s.Date,
                EmployeeIds = s.EmployeeIds.ToList()
            }).ToList()
        };
    }
}