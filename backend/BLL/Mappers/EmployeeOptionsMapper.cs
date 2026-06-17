using Domain;
using DTOs.EmployeeOptionsDtos;

namespace BLL.Mappers;

public class EmployeeOptionsMapper
{
    public static BllPreferenceBundle MapToBll(
        List<ShiftTypePreference> shiftTypes,
        List<DayOfWeek> weekDays)
    {
        return new BllPreferenceBundle
        {
            ShiftTypePreferences = shiftTypes.Select(st => st.ShiftTypeId).ToList(),
            WeekDayPreferences = weekDays
        };
    }

}