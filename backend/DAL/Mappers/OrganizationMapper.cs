using DAL.DTO.OrganizationDtos;
using Domain;

namespace DAL.Mappers;

public static class OrganizationMapper
{
    public static DalOrganization ToDal(Organization o) => new()
    {
        Id = o.Id,
        Name = o.Name,
        Description = o.Description,
        Address = o.Address,
        Phone = o.Phone,
        Website = o.Website,
        OrganizationType = o.OrganizationType,
        IsOpen24_7 = o.IsOpen24_7,
        NightShiftBonus = o.NightShiftBonus,
        HolidayBonus = o.HolidayBonus,
        PhotoUrl = o.PhotoUrl,
        EmployeeCount = o.EmployeeCount,
        Holidays = o.OrganizationHolidays?
            .Select(h => new DalHoliday
            {
                Name = h.Name ?? string.Empty,
                Month = h.Month,
                Day = h.Day,
                IsShortenedDay = h.IsShortenedDay,
                StartTime = h.StartTime,
                EndTime = h.EndTime
            }).ToList() ?? new(),
        WorkDays = o.OrganizationWorkDays?
            .Select(w => new DalWorkDay
            {
                DayOfWeek = w.DayOfWeek,
                StartTime = w.StartTime.ToString(@"hh\:mm"),
                EndTime = w.EndTime.ToString(@"hh\:mm")
            }).ToList() ?? new()
    };

    public static Organization ToDomain(DalOrganizationCreate dto) => new()
    {
        Name = dto.Name,
        Description = dto.Description,
        Address = dto.Address,
        Phone = dto.Phone,
        Website = dto.Website,
        OrganizationType = dto.OrganizationType,
        IsOpen24_7 = dto.IsOpen24_7,
        NightShiftBonus = dto.NightShiftBonus,
        HolidayBonus = dto.HolidayBonus,
        PhotoUrl = dto.PhotoUrl,
        EmployeeCount = dto.EmployeeCount,
        EmployerId = dto.EmployerId
    };
}
