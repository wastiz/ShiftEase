using Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Contracts
{
    public interface IWeekDayPreferenceRepository
    {
        Task<List<DayOfWeek>> GetEmployeePreferredDaysOfWeekAsync(int employeeId);
        Task RemoveAllEmployeePreferredDaysOfWeekAsync(int employeeId);
        Task AddEmployeeDaysOfWeekPreferencesAsync(int employeeId, List<DayOfWeek> preferredDays);
    }
}
