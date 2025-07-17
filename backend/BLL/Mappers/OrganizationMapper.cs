using DAL.DTO.OrganizationDtos;
using DTOs.OrganizationDtos;

namespace BLL.Mappers;

public static class OrganizationMapper
{
    public static BllOrganizationInfo MapToBll(DalOrganizationInfo dal)
    {
        return new BllOrganizationInfo
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
            EmployeeCount = dal.EmployeeCount
        };
    }
    
    public static BllHoliday MapToBll(DalHoliday dal)
    {
        return new BllHoliday()
        {
            HolidayName = dal.HolidayName,
            Month = dal.Month,
            Day = dal.Day
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
            WeekDayName = dal.WeekDayName,
            From = dal.From,
            To = dal.To,
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
            OrganizationHolidays = bll.OrganizationHolidays.Select(MapToDal).ToList(),
            OrganizationWorkDays = bll.OrganizationWorkDays.Select(MapToDal).ToList(),
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
            OrganizationId = bll.OrganizationId,
            OrganizationHolidays = bll.OrganizationHolidays.Select(MapToDal).ToList(),
            OrganizationWorkDays = bll.OrganizationWorkDays.Select(MapToDal).ToList(),
        };
    }
    
    public static DalHoliday MapToDal(BllHoliday bll)
    {
        return new DalHoliday
        {
            HolidayName = bll.HolidayName,
            Month = bll.Month,
            Day = bll.Day
        };
    }
    
    public static DalWorkDay MapToDal(BllWorkDay bll)
    {
        return new DalWorkDay
        {
            WeekDayName = bll.WeekDayName,
            From = bll.From,
            To = bll.To
        };
    }
    
    public static List<BllOrganizationInfo> MapToBll(IEnumerable<DalOrganizationInfo> dalList)
    {
        return dalList.Select(MapToBll).ToList();
    }
}