using DAL.DTO.EmployeeDtos;
using Domain.Models;

namespace DAL.Repositories
{
    public interface IEmployeeRepository
    {
        Task<bool> EmployeeExistsAsync(int empId);
        Task<bool> EmployeeEmailExistsAsync(string email);
        Task<bool> EmployeePhoneExistsAsync(string phone);
        Task<int> GetEmployeesCount();
        Task<DalEmployeeFullData?> GetByIdAsync(int id);
        Task<List<DalEmployeeFullData>> GetFullDataByOrganizationIdAsync(int organizationId);
        Task<List<DalEmployeeMinData>> GetMinDataByOrganizationIdAsync(int organizationId);
        Task<List<DalEmployeeMinData>> GetMinDataByGroupIdAsync(int groupId);
        Task<bool> CreateAsync(DalEmployeeCreate createDto);
        Task<bool> UpdateAsync(DalEmployeeUpdate updateDto);
        Task<bool> DeleteAsync(int id);
        Task<Employee> CheckPasswordAsync(string employeeEmail, string employeePassword);
    }
}