using Domain;

namespace DAL.Contracts
{
    public interface IPersonalDayRepository
    {
        Task<List<PersonalDay>> GetPersonalDaysAsync(int employeeId);
        Task<PersonalDay?> GetPersonalDayByIdAsync(int personalDayId);
        Task AddPersonalDayAsync(PersonalDay personalDay);
        Task DeletePersonalDayAsync(PersonalDay personalDay);
    }
}
