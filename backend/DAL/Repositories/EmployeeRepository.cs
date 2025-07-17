using DAL.DTO.EmployeeDtos;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher<Employee> _passwordHasher;

        public EmployeeRepository(AppDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<Employee>();
        }
        
        //Check if Employee Exists
        public async Task<bool> EmployeeExistsAsync(int id)
        {
            return await _context.Employees.AnyAsync(e => e.Id == id);
        }
        
        //Check if employee with this email already exists
        public async Task<bool> EmployeeEmailExistsAsync(string email)
        {
            return await _context.Employees.AnyAsync(e => e.Email == email);
        }
        
        //Check id employee with this phone already exists
        public async Task<bool> EmployeePhoneExistsAsync(string phone)
        {
            return await _context.Employees.AnyAsync(e => e.Phone == phone);
        }
        
        //Get Employee count
        public async Task<int> GetEmployeesCount()
        {
            return await _context.Employees.CountAsync();
        }
        
        //Get employee full info by hid id
        public async Task<DalEmployeeFullData?> GetByIdAsync(int id)
        {
            return await _context.Employees
                .Where(e => e.Id == id)
                .Select(e => new DalEmployeeFullData()
                {
                    Id = e.Id,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Email = e.Email,
                    Password = e.Password,
                    Phone = e.Phone,
                    Position = e.Position,
                    Workload = e.Workload,
                    Salary = e.Salary,
                    SalaryInHour = e.SalaryInHour,
                    Priority = e.Priority,
                    OnVacation = e.OnVacation,
                    OnSickLeave = e.OnSickLeave,
                    OnWork = e.OnWork,
                    GroupId = e.GroupId,
                    GroupName = e.Group.Name
                })
                .FirstOrDefaultAsync();
        }
        
        //Get all organization's employees by org id
        public async Task<List<DalEmployeeFullData>> GetFullDataByOrganizationIdAsync(int organizationId)
        {
            return await _context.Employees
                .Where(e => e.OrganizationId == organizationId)
                .Select(e => new DalEmployeeFullData
                {
                    Id = e.Id,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Email = e.Email,
                    Password = e.Password,
                    Phone = e.Phone,
                    Position = e.Position,
                    Workload = e.Workload,
                    Salary = e.Salary,
                    SalaryInHour = e.SalaryInHour,
                    Priority = e.Priority,
                    OnVacation = e.OnVacation,
                    OnSickLeave = e.OnSickLeave,
                    OnWork = e.OnWork,
                    GroupId = e.GroupId,
                    GroupName = e.Group.Name
                })
                .ToListAsync();
        }
        
        //Get organization's employees minimum data (for display) byu org id
        public async Task<List<DalEmployeeMinData>> GetMinDataByOrganizationIdAsync(int organizationId)
        {
            return await _context.Employees
                .Where(e => e.OrganizationId == organizationId)
                .Select(e => new DalEmployeeMinData()
                {
                    Id = e.Id,
                    Name = e.FirstName + " " + e.LastName,
                    GroupName = e.Group.Name
                })
                .ToListAsync();
        }
        
        //Get employee minimum info for disaply by his id
        public async Task<List<DalEmployeeMinData>> GetMinDataByGroupIdAsync(int groupId)
        {
            return await _context.Employees
                .Where(e => e.GroupId == groupId)
                .Select(e => new DalEmployeeMinData()
                {
                    Id = e.Id,
                    Name = e.FirstName + " " + e.LastName,
                })
                .ToListAsync();
        }
        
        //Create new employee
        public async Task<bool> CreateAsync(DalEmployeeCreate dto)
        {
            var employee = new Employee
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Phone = dto.Phone,
                Email = dto.Email,
                Password = dto.Password,
                Position = dto.Position,
                Workload = dto.Workload,
                Salary = dto.Salary,
                SalaryInHour = dto.SalaryInHour,
                Priority = dto.Priority,
                GroupId = dto.GroupId,
                OrganizationId = dto.OrganizationId
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            return true;
        }
        
        public async Task<bool> UpdateAsync(DalEmployeeUpdate updateDto)
        {
            var employee = await _context.Employees.FindAsync(updateDto.Id);
            if (employee == null) return false;
            employee.FirstName = updateDto.FirstName;
            employee.LastName = updateDto.LastName;
            employee.Email = updateDto.Email;
            employee.Password = updateDto.Password;
            employee.Phone = updateDto.Phone;
            employee.Position = updateDto.Position;
            employee.Workload = updateDto.Workload;
            employee.Salary = updateDto.Salary;
            employee.SalaryInHour = updateDto.SalaryInHour;
            employee.Priority = updateDto.Priority;
            employee.GroupId = updateDto.GroupId;
            
            _context.Entry(employee).State = EntityState.Modified;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return false;

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            return true;
        }
        
        //Function for authenticating employee when loging
        public async Task<Employee> CheckPasswordAsync(string employeeEmail, string employeePassword)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == employeeEmail);

            if (employee == null || employee.Password != employeePassword)
            {
                return null;
            }

            return employee;
        }


    }
}
