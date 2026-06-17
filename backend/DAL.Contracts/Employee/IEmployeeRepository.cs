using DAL.DTO.EmployeeDtos;
using Domain.Models;

namespace DAL.Contracts
{
    public interface IEmployeeRepository
    {
        Task<bool> EmployeeExistsAsync(int empId);
        Task<bool> EmployeeEmailExistsAsync(string email);
        Task<bool> EmployeePhoneExistsAsync(string phone);
        Task<int> GetEmployeesCount();
        Task<DalEmployee?> GetByIdAsync(int id);
        Task<List<DalEmployee>> GetFullDataByOrganizationIdAsync(int organizationId);
        Task<List<DalEmployee>> GetFullDataByDepartmentIdAsync(int departmentId);
        Task<List<DalEmployeeMinData>> GetMinDataByOrganizationIdAsync(int organizationId);
        Task<List<DalEmployeeMinData>> GetMinDataByDepartmentIdAsync(int departmentId);
        Task<List<DalEmployeeMinData>> GetMinDataWithoutDepartmentsAsync(int organizationId);
        Task<List<int>> GetDepartmentIdsByEmployeeIdAsync(int employeeId);
        Task<int?> GetOrgIdByEmployeeIdAsync(int employeeId);
        Task<bool> CreateAsync(DalEmployeeCreate createDto);
        Task<bool> UpdateAsync(DalEmployeeUpdate updateDto);
        Task<bool> DeleteAsync(int id);
        Task<Domain.Models.Employee?> CheckPasswordAsync(string employeeEmail, string employeePassword);
        Task UpdatePasswordByEmailAsync(string email, string newPassword);
    }
}
