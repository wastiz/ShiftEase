using DAL.Contracts;
using DAL.DTO.EmployeeDtos;
using DAL.Mappers;
using Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher<Domain.Models.Employee> _passwordHasher;

        public EmployeeRepository(AppDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<Domain.Models.Employee>();
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
        public async Task<DalEmployee?> GetByIdAsync(int id)
        {
            return await _context.Employees
                .Where(e => e.Id == id)
                .Select(e => new DalEmployee()
                {
                    Id = e.Id,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Email = e.Email,
                    Phone = e.Phone,
                    Position = e.Position,
                    HourlyRate = e.HourlyRate,
                    EmploymentRate = e.EmploymentRate,
                    Priority = e.Priority,
                    OnVacation = e.OnVacation,
                    OnSickLeave = e.OnSickLeave,
                    OnWork = e.OnWork,
                    DepartmentIds = e.EmployeeDepartments.Select(eg => eg.DepartmentId).ToList(),
                    DepartmentNames = e.EmployeeDepartments.Select(eg => eg.Department.Name).ToList(),
                    PrimaryDepartmentId = e.EmployeeDepartments.Where(eg => eg.IsPrimary).Select(eg => eg.DepartmentId).FirstOrDefault(),
                    OrganizationId = e.OrganizationId,
                    OrganizationName = e.Organization.Name
                })
                .FirstOrDefaultAsync();
        }
        
        //Get all organization's employees by org id
        public async Task<List<DalEmployee>> GetFullDataByOrganizationIdAsync(int organizationId)
        {
            return await _context.Employees
                .Where(e => e.OrganizationId == organizationId)
                .Select(e => new DalEmployee
                {
                    Id = e.Id,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Email = e.Email,
                    Phone = e.Phone,
                    Position = e.Position,
                    HourlyRate = e.HourlyRate,
                    EmploymentRate = e.EmploymentRate,
                    Priority = e.Priority,
                    OnVacation = e.OnVacation,
                    OnSickLeave = e.OnSickLeave,
                    OnWork = e.OnWork,
                    DepartmentIds = e.EmployeeDepartments.Select(eg => eg.DepartmentId).ToList(),
                    DepartmentNames = e.EmployeeDepartments.Select(eg => eg.Department.Name).ToList(),
                    PrimaryDepartmentId = e.EmployeeDepartments
                        .Where(eg => eg.IsPrimary)
                        .Select(eg => (int?)eg.DepartmentId)
                        .FirstOrDefault()
                })
                .ToListAsync();
        }

        public async Task<List<DalEmployee>> GetFullDataByDepartmentIdAsync(int departmentId)
        {
            return await _context.Employees
                .Where(e => e.EmployeeDepartments.Any(eg => eg.DepartmentId == departmentId))
                .Select(e => new DalEmployee
                {
                    Id = e.Id,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Email = e.Email,
                    Phone = e.Phone,
                    Position = e.Position,
                    HourlyRate = e.HourlyRate,
                    EmploymentRate = e.EmploymentRate,
                    Priority = e.Priority,
                    OnVacation = e.OnVacation,
                    OnSickLeave = e.OnSickLeave,
                    OnWork = e.OnWork,
                    DepartmentIds = e.EmployeeDepartments.Select(eg => eg.DepartmentId).ToList(),
                    DepartmentNames = e.EmployeeDepartments.Select(eg => eg.Department.Name).ToList(),
                    PrimaryDepartmentId = e.EmployeeDepartments
                        .Where(eg => eg.IsPrimary)
                        .Select(eg => (int?)eg.DepartmentId)
                        .FirstOrDefault()
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
                    DepartmentName = string.Join(", ", e.EmployeeDepartments.Select(eg => eg.Department.Name))
                })
                .ToListAsync();
        }
        
        public async Task<List<DalEmployeeMinData>> GetMinDataByDepartmentIdAsync(int departmentId)
        {
            return await _context.Employees
                .Where(e => e.EmployeeDepartments.Any(eg => eg.DepartmentId == departmentId))
                .Select(e => new DalEmployeeMinData()
                {
                    Id = e.Id,
                    Name = e.FirstName + " " + e.LastName,
                    DepartmentName = string.Join(", ", e.EmployeeDepartments.Select(eg => eg.Department.Name))
                })
                .ToListAsync();
        }
        
        public async Task<List<DalEmployeeMinData>> GetMinDataWithoutDepartmentsAsync(int organizationId)
        {
            return await _context.Employees
                .Where(e => e.OrganizationId == organizationId && !e.EmployeeDepartments.Any())
                .Select(e => new DalEmployeeMinData()
                {
                    Id = e.Id,
                    Name = e.FirstName + " " + e.LastName,
                    DepartmentName = null
                })
                .ToListAsync();
        }

        public async Task<List<int>> GetDepartmentIdsByEmployeeIdAsync(int employeeId)
        {
            return await _context.EmployeeInDepartments
                .Where(eg => eg.EmployeeId == employeeId)
                .Select(eg => eg.DepartmentId)
                .ToListAsync();
        }
        
        public async Task<int?> GetOrgIdByEmployeeIdAsync(int employeeId)
        {
            var employee = await _context.Employees
                .Where(e => e.Id == employeeId)
                .Select(e => new { e.OrganizationId })
                .FirstOrDefaultAsync();

            return employee?.OrganizationId;
        }
        
        //Create new employee
        public async Task<bool> CreateAsync(DalEmployeeCreate dto)
        {
            var employee = EmployeeMapper.ToDomain(dto);

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            if (dto.DepartmentIds != null && dto.DepartmentIds.Count > 0)
            {
                // Validate: PrimaryDepartmentId must be in DepartmentIds if provided
                if (dto.PrimaryDepartmentId.HasValue && !dto.DepartmentIds.Contains(dto.PrimaryDepartmentId.Value))
                    throw new InvalidOperationException("PrimaryDepartmentId must be one of the assigned DepartmentIds.");

                bool primaryAssigned = false;
                foreach (var departmentId in dto.DepartmentIds)
                {
                    // If PrimaryDepartmentId is specified, use it; otherwise default first department to primary
                    bool isPrimary = dto.PrimaryDepartmentId.HasValue
                        ? dto.PrimaryDepartmentId.Value == departmentId
                        : !primaryAssigned;

                    var employeeInDepartment = new Domain.EmployeeInDepartment
                    {
                        EmployeeId = employee.Id,
                        DepartmentId = departmentId,
                        IsPrimary = isPrimary
                    };
                    _context.EmployeeInDepartments.Add(employeeInDepartment);

                    if (isPrimary) primaryAssigned = true;
                }
                await _context.SaveChangesAsync();
            }

            return true;
        }
        
        //Update Employee
        public async Task<bool> UpdateAsync(DalEmployeeUpdate updateDto)
        {
            var employee = await _context.Employees.FindAsync(updateDto.Id);
            if (employee == null) return false;

            employee.FirstName = updateDto.FirstName;
            employee.LastName = updateDto.LastName;
            employee.Email = updateDto.Email;
            employee.Phone = updateDto.Phone;
            employee.Position = updateDto.Position;
            employee.HourlyRate = updateDto.HourlyRate;
            employee.EmploymentRate = updateDto.EmploymentRate;
            employee.Priority = updateDto.Priority;
            employee.OnVacation = updateDto.OnVacation;
            employee.OnSickLeave = updateDto.OnSickLeave;
            employee.OnWork = updateDto.OnWork;

            _context.Entry(employee).State = EntityState.Modified;

            var existingDepartmentIds = await _context.EmployeeInDepartments
                .Where(eg => eg.EmployeeId == updateDto.Id)
                .ToListAsync();

            _context.EmployeeInDepartments.RemoveRange(existingDepartmentIds);

            if (updateDto.DepartmentIds != null && updateDto.DepartmentIds.Count > 0)
            {
                // Validate: PrimaryDepartmentId must be in DepartmentIds if provided
                if (updateDto.PrimaryDepartmentId.HasValue && !updateDto.DepartmentIds.Contains(updateDto.PrimaryDepartmentId.Value))
                    throw new InvalidOperationException("PrimaryDepartmentId must be one of the assigned DepartmentIds.");

                bool primaryAssigned = false;
                foreach (var departmentId in updateDto.DepartmentIds)
                {
                    bool isPrimary = updateDto.PrimaryDepartmentId.HasValue
                        ? updateDto.PrimaryDepartmentId.Value == departmentId
                        : !primaryAssigned;

                    var employeeInDepartment = new Domain.EmployeeInDepartment
                    {
                        EmployeeId = updateDto.Id,
                        DepartmentId = departmentId,
                        IsPrimary = isPrimary
                    };
                    _context.EmployeeInDepartments.Add(employeeInDepartment);

                    if (isPrimary) primaryAssigned = true;
                }
            }

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
        
        //Function for authenticating employee when logging
        public async Task<Domain.Models.Employee> CheckPasswordAsync(string employeeEmail, string employeePassword)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == employeeEmail);

            if (employee == null || employee.Password != employeePassword)
            {
                return null;
            }

            return employee;
        }

        public async Task UpdatePasswordByEmailAsync(string email, string newPassword)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == email);
            if (employee != null)
            {
                employee.Password = newPassword;
                await _context.SaveChangesAsync();
            }
        }
    }
}
