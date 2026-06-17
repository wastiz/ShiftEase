using Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Contracts
{
    public interface IVacationRequestRepository
    {
        Task<List<VacationRequest>> GetVacationRequestsAsync(int employeeId);
        Task<VacationRequest?> GetVacationRequestByIdAsync(int employeeId, int vacationRequestId);
        Task<VacationRequest?> GetVacationRequestByIdAsync(int vacationRequestId);
        Task<List<VacationRequest>> GetPendingVacationRequestsByOrganizationAsync(int organizationId);
        Task AddVacationRequestAsync(VacationRequest vacationRequest);
        Task UpdateVacationRequestAsync(VacationRequest request);
        Task DeleteVacationRequestAsync(VacationRequest request);
    }
}
