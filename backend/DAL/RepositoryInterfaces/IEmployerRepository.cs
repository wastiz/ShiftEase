using DAL.DTO.EmployerDtos;

public interface IEmployerRepository
{
    Task<int> GetNewCountLastWeekAsync();
    Task<List<DalEmployerFullData>> GetAllAsync();
    Task<DalEmployerFullData?> GetByIdAsync(int id);
    Task<DalEmployerFullData?> GetByEmailAsync(string email);
    Task<Employer> CreateAsync(DalEmployerCreate dto);
    Task<bool> UpdateEmployerInfoAsync(DalEmployerUpdate dto);
    Task<bool> UpdateEmployerPasswordAsync(int employerId, string newPassword);
    Task DeleteAsync(int id);
    Task<Employer> CheckPasswordAsync(string email, string password);
}