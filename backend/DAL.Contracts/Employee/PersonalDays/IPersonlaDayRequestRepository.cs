using Domain;

namespace DAL.Contracts
{
    public interface IPersonalDayRequestRepository
    {
        Task<List<PersonalDayRequest>> GetPersonalDayRequestsAsync(int employeeId);
        Task<PersonalDayRequest?> GetPersonalDayRequestByIdAsync(int employeeId, int personalDayRequestId);
        Task<PersonalDayRequest?> GetPersonalDayRequestByIdAsync(int personalDayRequestId);
        Task<List<PersonalDayRequest>> GetPendingPersonalDayRequestsByOrganizationAsync(int organizationId);
        Task AddPersonalDayRequestAsync(PersonalDayRequest personalDayRequest);
        Task UpdatePersonalDayRequestAsync(PersonalDayRequest request);
        Task DeletePersonalDayRequestAsync(PersonalDayRequest request);
    }
}
