using Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Contracts
{
    public interface ISickLeaveRequestRepository
    {
        Task<List<SickLeaveRequest>> GetSickLeaveRequestsAsync(int employeeId);
        Task<SickLeaveRequest?> GetSickLeaveRequestByIdAsync(int employeeId, int sickLeaveRequestId);
        Task<SickLeaveRequest?> GetSickLeaveRequestByIdAsync(int sickLeaveRequestId);
        Task<List<SickLeaveRequest>> GetPendingSickLeaveRequestsByOrganizationAsync(int organizationId);
        Task AddSickLeaveRequestAsync(SickLeaveRequest sickLeaveRequest);
        Task UpdateSickLeaveRequestAsync(SickLeaveRequest request);
        Task DeleteSickLeaveRequestAsync(SickLeaveRequest request);
    }
}
