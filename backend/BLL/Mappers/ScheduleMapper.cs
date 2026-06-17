using BLL.DTO.ScheduleDtos;
using DAL.DTO.EmployeeDtos;
using DAL.DTO.ScheduleDtos;
using DTOs.ScheduleDtos;

namespace BLL.Mappers;

public class ScheduleMapper
{
    public static BllShift MapToBll(DalShift dal)
    {
        return new BllShift()
        {
            Id = dal.Id,
            Date = dal.Date,
            ShiftTypeId = dal.ShiftTypeId,
            ShiftTypeName = dal.ShiftTypeName,
            StartTime = dal.StartTime,
            EndTime = dal.EndTime,
            Color = dal.Color,
            MinEmployees = dal.MinEmployees,
            MaxEmployees = dal.MaxEmployees,
            BreakDuration = dal.BreakDuration,
            Employees = EmployeeMapper.MapToBll(dal.Employees)
        };
    }

    public static List<BllShift> MapToBll(List<DalShift> dal)
    {
        return dal.Select(MapToBll).ToList();
    }

    public static DalSchedulePost MapToDal(BllSchedulePost bll)
    {
        return new DalSchedulePost
        {
            DateFrom = bll.StartDate,
            DateTo = bll.EndDate,
            IsConfirmed = bll.IsConfirmed,
            Shifts = bll.Shifts.Select(s => new DalSchedulePost.ShiftCreateDto
            {
                ShiftTypeId = s.ShiftTypeId,
                Date = s.Date,
                Employees = s.Employees.Select(e => new DalSchedulePost.EmployeeInShiftDto
                {
                    EmployeeId = e.EmployeeId,
                    Note = e.Note
                }).ToList()
            }).ToList()
        };
    }
}
