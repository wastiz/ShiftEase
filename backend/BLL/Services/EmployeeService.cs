using BLL.Interfaces;
using BLL.Mappers;
using DAL.Repositories;
using DTOs.EmployeeDtos;

namespace BLL.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;

    public EmployeeService(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    public async Task<BllEmployeeFullData?> GetByIdAsync(int id)
    {
        var employee = await _employeeRepository.GetByIdAsync(id);
        return employee == null ? null : EmployeeMapper.MapToBll(employee);
    }

    public async Task<List<BllEmployeeFullData>> GetFullDataByOrganizationIdAsync(int organizationId)
    {
        var employees = await _employeeRepository.GetFullDataByOrganizationIdAsync(organizationId);
        return employees.Select(EmployeeMapper.MapToBll).ToList();
    }

    public async Task<List<BllEmployeeMinData>> GetMinDataByOrganizationIdAsync(int organizationId)
    {
        var employees = await _employeeRepository.GetMinDataByOrganizationIdAsync(organizationId);
        return employees.Select(EmployeeMapper.MapToBll).ToList();
    }

    public async Task<List<BllEmployeeMinData>> GetMinDataByGroupIdAsync(int groupId)
    {
        var employees = await _employeeRepository.GetMinDataByGroupIdAsync(groupId);
        return employees.Select(EmployeeMapper.MapToBll).ToList();
    }

    public async Task<BllRequestResult> CreateAsync(BllEmployeeCreate createDto, int organizationId)
    {
        if (await _employeeRepository.EmployeeEmailExistsAsync(createDto.Email))
        {
            return new BllRequestResult
            {
                Success = false,
                Message = "Employee with this email already exists."
            };
        }

        if (await _employeeRepository.EmployeePhoneExistsAsync(createDto.Phone))
        {
            return new BllRequestResult
            {
                Success = false,
                Message = "Employee with this phone number already exists."
            };
        }
        bool created = await _employeeRepository.CreateAsync(EmployeeMapper.MapToDal(createDto, organizationId));

        if (!created)
        {
            return new BllRequestResult()
            {
                Success = false,
                Message = "Failed to create employee"
            };
        }
        
        return new BllRequestResult() { Success = true, Message = "Employee created successfully" };
    }

    public async Task<BllRequestResult> UpdateAsync(int employeeId, BllEmployeeUpdate updateDto)
    {
        var currentEmployee = await _employeeRepository.GetByIdAsync(employeeId);
        if (currentEmployee == null)
        {
            return new BllRequestResult { Success = false, Message = "Employee not found." };
        }
        
        if (!string.Equals(updateDto.Email, currentEmployee.Email, StringComparison.OrdinalIgnoreCase))
        {
            if (await _employeeRepository.EmployeeEmailExistsAsync(updateDto.Email))
            {
                return new BllRequestResult { Success = false, Message = "Employee with this email already exists." };
            }
        }
        
        if (!string.Equals(updateDto.Phone, currentEmployee.Phone, StringComparison.OrdinalIgnoreCase))
        {
            if (await _employeeRepository.EmployeePhoneExistsAsync(updateDto.Phone))
            {
                return new BllRequestResult { Success = false, Message = "Employee with this phone number already exists." };
            }
        }

        bool updated = await _employeeRepository.UpdateAsync(EmployeeMapper.MapToDal(updateDto, employeeId));

        if (!updated)
        {
            return new BllRequestResult() { Success = false, Message = "Failed to update employee" };
        }

        return new BllRequestResult() { Success = true, Message = "Employee updated successfully" };
    }


    public async Task<BllRequestResult> DeleteAsync(int id)
    {
        bool deleted = await _employeeRepository.DeleteAsync(id);
        
        if (!deleted) return new BllRequestResult() { Success = false, Message = "Failed to delete employee" };
        
        return new BllRequestResult() { Success = true, Message = "Employee deleted successfully" };
    }
}
