using Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Contracts
{
    public interface IVacationRepository
    {
        Task<List<Vacation>> GetVacationsAsync(int employeeId);
        Task<Vacation?> GetVacationByIdAsync(int vacationId);
        Task AddVacationAsync(Vacation vacation);
        Task DeleteVacationAsync(Vacation vacation);
    }
}
