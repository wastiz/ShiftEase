using Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Contracts
{
    public interface ISickLeaveRepository
    {
        Task<List<Domain.SickLeave>> GetSickLeavesAsync(int employeeId);
        Task<Domain.SickLeave?> GetSickLeaveByIdAsync(int sickLeaveId);
        Task AddSickLeaveAsync(Domain.SickLeave sickLeave);
        Task DeleteSickLeaveAsync(Domain.SickLeave leave);
    }
}
