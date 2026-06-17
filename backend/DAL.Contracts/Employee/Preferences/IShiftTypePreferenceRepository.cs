using Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Contracts
{
    public interface IShiftTypePreferenceRepository
    {
        Task<List<ShiftTypePreference>> GetShiftTypePreferencesAsync(int employeeId);
        Task RemoveShiftTypePreferencesAsync(List<ShiftTypePreference> preferences);
        Task AddShiftTypePreferencesAsync(List<ShiftTypePreference> preferences);
    }
}
