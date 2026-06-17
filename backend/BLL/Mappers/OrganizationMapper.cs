using DAL.DTO.OrganizationDtos;
using BLL.DTO.ScheduleDtos;
using DTOs.OrganizationDtos;

namespace BLL.Mappers;

public static class OrganizationMapper
{
    public static BllOrganization MapToBll(DalOrganization dal)
    {
        return new BllOrganization
        {
            Id = dal.Id,
            Name = dal.Name,
            Description = dal.Description,
            Address = dal.Address,
            Phone = dal.Phone,
            Website = dal.Website,
            OrganizationType = dal.OrganizationType,
            IsOpen24_7 = dal.IsOpen24_7,
            NightShiftBonus = dal.NightShiftBonus,
            HolidayBonus = dal.HolidayBonus,
            PhotoUrl = dal.PhotoUrl,
            EmployeeCount = dal.EmployeeCount,
            Holidays = MapToBll(dal.Holidays),
            WorkDays = MapToBll(dal.WorkDays),
        };
    }
    
    public static List<BllOrganization> MapToBll(IEnumerable<DalOrganization> dalList)
    {
        return dalList.Select(MapToBll).ToList();
    }
    
    public static BllHoliday MapToBll(DalHoliday dal)
    {
        return new BllHoliday()
        {
            HolidayName = dal.Name,
            Month = dal.Month,
            Day = dal.Day,
            IsShortenedDay = dal.IsShortenedDay,
            StartTime = dal.StartTime,
            EndTime = dal.EndTime
        };
    }
    
    public static List<BllHoliday> MapToBll(List<DalHoliday> holidays)
    {
        return holidays.Select(MapToBll).ToList();
    }
    
    public static BllWorkDay MapToBll(DalWorkDay dal)
    {
        return new BllWorkDay()
        {
            DayOfWeek = dal.DayOfWeek,
            StartTime = dal.StartTime,
            EndTime = dal.EndTime,
        };
    }

    public static List<BllWorkDay> MapToBll(List<DalWorkDay> workDays)
    {
        return workDays.Select(MapToBll).ToList();
    }
    
    public static DalOrganizationCreate MapToDal(BllOrganizationCreate bll)
    {
        return new DalOrganizationCreate()
        {
            Name = bll.Name,
            Description = bll.Description,
            Address = bll.Address,
            Phone = bll.Phone,
            Website = bll.Website,
            OrganizationType = bll.OrganizationType,
            IsOpen24_7 = bll.IsOpen24_7,
            NightShiftBonus = bll.NightShiftBonus,
            HolidayBonus = bll.HolidayBonus,
            PhotoUrl = bll.PhotoUrl,
            EmployeeCount = bll.EmployeeCount,
            EmployerId = bll.EmployerId,
            OrganizationHolidays = bll.Holidays.Select(MapToDal).ToList(),
            OrganizationWorkDays = bll.WorkDays.Select(MapToDal).ToList(),
        };
    }
    
    public static DalOrganizationUpdate MapToDal(BllOrganizationUpdate bll)
    {
        return new DalOrganizationUpdate()
        {
            Name = bll.Name,
            Description = bll.Description,
            Address = bll.Address,
            Phone = bll.Phone,
            Website = bll.Website,
            OrganizationType = bll.OrganizationType,
            IsOpen24_7 = bll.IsOpen24_7,
            NightShiftBonus = bll.NightShiftBonus,
            HolidayBonus = bll.HolidayBonus,
            PhotoUrl = bll.PhotoUrl,
            EmployeeCount = bll.EmployeeCount,
            OrganizationId = bll.Id,
            OrganizationHolidays = bll.Holidays.Select(MapToDal).ToList(),
            OrganizationWorkDays = bll.WorkDays.Select(MapToDal).ToList(),
        };
    }
    
    public static DalHoliday MapToDal(BllHoliday bll)
    {
        return new DalHoliday
        {
            Name = bll.HolidayName,
            Month = bll.Month,
            Day = bll.Day,
            IsShortenedDay = bll.IsShortenedDay,
            StartTime = bll.StartTime,
            EndTime = bll.EndTime
        };
    }
    
    public static DalWorkDay MapToDal(BllWorkDay bll)
    {
        return new DalWorkDay
        {
            DayOfWeek = bll.DayOfWeek,
            StartTime = bll.StartTime,
            EndTime = bll.EndTime
        };
    }

    public static BllOrganizationDashboardData MapToBll(DalOrganizationData dal)
    {
        return new BllOrganizationDashboardData()
        {
            Id = dal.Id,
            Name = dal.Name ?? string.Empty,
            DepartmentCount = dal.DepartmentCount,
            ShiftTypeCount = dal.ShiftTypeCount,
            EmployeeCount = dal.EmployeeCount,
            ScheduleCount = dal.ScheduleCount,
            ScheduleSummary = new()
        };
    }
}
