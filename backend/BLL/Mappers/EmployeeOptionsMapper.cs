using Domain;
using DTOs.EmployeeOptionsDtos;

namespace BLL.Mappers;

public class EmployeeOptionsMapper
{
    public static BllPreferenceBundle MapToBll(
        List<ShiftTypePreference> shiftTypes,
        List<WeekDayPreference> weekDays,
        List<DayOffPreference> dayOffs)
    {
        return new BllPreferenceBundle
        {
            ShiftTypePreferences = shiftTypes.Select(st => st.ShiftTypeId).ToList(),
            WeekDayPreferences = weekDays.Select(wd => wd.WeekDay.Name).ToList(),
            DayOffPreferences = dayOffs.Select(doff => doff.Date).ToList()
        };
    }

}