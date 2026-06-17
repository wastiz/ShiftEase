using BLL.DTO.ScheduleDtos;
using BLL.Helpers;
using BLL.Contracts;
using BLL.Mappers;
using DAL.DTO.EmployeeDtos;
using DAL.Contracts;
using DTOs.EmployeeDtos;

namespace BLL.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IPersonalDayService _personalDayService;
    private readonly IVacationService _vacationService;
    private readonly ISickLeaveService _sickLeaveService;
    public EmployeeService(IEmployeeRepository employeeRepository, IVacationService vacationService, ISickLeaveService sickLeaveService, IPersonalDayService personalDayService)
    {
        _employeeRepository = employeeRepository;
        _vacationService = vacationService;
        _personalDayService = personalDayService;
        _sickLeaveService = sickLeaveService;
    }

    public async Task<BllEmployee?> GetByIdAsync(int id)
    {
        var employee = await _employeeRepository.GetByIdAsync(id);
        return employee == null ? null : EmployeeMapper.MapToBll(employee);
    }

    public async Task<List<BllEmployee>> GetFullDataByOrganizationIdAsync(int organizationId)
    {
        var employees = await _employeeRepository.GetFullDataByOrganizationIdAsync(organizationId);
        return employees.Select(EmployeeMapper.MapToBll).ToList();
    }
    
    public async Task<List<BllEmployee>> GetFullDataByDepartmentIdAsync(int departmentId)
    {
        var employees = await _employeeRepository.GetFullDataByDepartmentIdAsync(departmentId);
        return employees.Select(EmployeeMapper.MapToBll).ToList();
    }

    public async Task<List<BllEmployeeMinData>> GetMinDataByOrganizationIdAsync(int organizationId)
    {
        var employees = await _employeeRepository.GetMinDataByOrganizationIdAsync(organizationId);
        return employees.Select(EmployeeMapper.MapToBll).ToList();
    }

    public async Task<List<BllEmployeeMinData>> GetMinDataByDepartmentIdAsync(int departmentId)
    {
        var employees = await _employeeRepository.GetMinDataByDepartmentIdAsync(departmentId);
        return employees.Select(EmployeeMapper.MapToBll).ToList();
    }

    public async Task<List<BllEmployeeMinData>> GetMinDataWithoutDepartmentsAsync(int organizationId)
    {
        var employees = await _employeeRepository.GetMinDataWithoutDepartmentsAsync(organizationId);
        return employees.Select(EmployeeMapper.MapToBll).ToList();
    }

    public async Task<List<BllEmployeeTimeOff>> GetEmployeeTimeOffAsync(int employeeId)
    {
        var vacations = await _vacationService.GetVacations(employeeId);
        var sickLeaves = await _sickLeaveService.GetSickLeaves(employeeId);
        var personalDays = await _personalDayService.GetPersonalDays(employeeId);

        var timeOffs = new List<BllEmployeeTimeOff>();

        timeOffs.AddRange(vacations.Select(v => new BllEmployeeTimeOff
        {
            Id = v.Id,
            EmployeeId = v.EmployeeId,
            StartDate = v.StartDate,
            EndDate = v.EndDate,
            Type = TimeOffType.Vacation
        }));

        timeOffs.AddRange(sickLeaves.Select(s => new BllEmployeeTimeOff
        {
            Id = s.Id,
            EmployeeId = s.EmployeeId,
            StartDate = s.StartDate,
            EndDate = s.EndDate,
            Type = TimeOffType.SickLeave
        }));

        timeOffs.AddRange(personalDays.Select(p => new BllEmployeeTimeOff
        {
            Id = p.Id,
            EmployeeId = p.EmployeeId,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            Type = TimeOffType.PersonalDay
        }));

        return timeOffs
            .OrderBy(t => t.StartDate)
            .ToList();
    }
    
    public async Task<List<BllEmployeeTimeOff>> GetEmployeesTimeOffAsync(IEnumerable<int> employeeIds, DateOnly start, DateOnly end)
    {
        var result = new List<BllEmployeeTimeOff>();

        var uniqueEmployeeIds = employeeIds
            .Distinct()
            .ToList();

        if (!uniqueEmployeeIds.Any())
            return result;

        static bool Intersects(
            DateOnly start1,
            DateOnly end1,
            DateOnly start2,
            DateOnly end2)
            => start1 <= end2 && end1 >= start2;

        foreach (var employeeId in uniqueEmployeeIds)
        {
            var vacations = await _vacationService.GetVacations(employeeId);
            var sickLeaves = await _sickLeaveService.GetSickLeaves(employeeId);
            var personalDays = await _personalDayService.GetPersonalDays(employeeId);

            result.AddRange(
                vacations
                    .Where(v => Intersects(v.StartDate, v.EndDate, start, end))
                    .Select(v => new BllEmployeeTimeOff
                    {
                        Id = v.Id,
                        EmployeeId = v.EmployeeId,
                        StartDate = v.StartDate,
                        EndDate = v.EndDate,
                        Type = TimeOffType.Vacation
                    })
            );

            result.AddRange(
                sickLeaves
                    .Where(s => Intersects(s.StartDate, s.EndDate, start, end))
                    .Select(s => new BllEmployeeTimeOff
                    {
                        Id = s.Id,
                        EmployeeId = s.EmployeeId,
                        StartDate = s.StartDate,
                        EndDate = s.EndDate,
                        Type = TimeOffType.SickLeave
                    })
            );

            result.AddRange(
                personalDays
                    .Where(p => Intersects(p.StartDate, p.EndDate, start, end))
                    .Select(p => new BllEmployeeTimeOff
                    {
                        Id = p.Id,
                        EmployeeId = p.EmployeeId,
                        StartDate = p.StartDate,
                        EndDate = p.EndDate,
                        Type = TimeOffType.PersonalDay
                    })
            );
        }
        Console.WriteLine(result);
        return result;
    }



    public async Task<List<BllEmployeeTimeOff>> GetEmployeesTimeOffByFilterAsync(int organizationId, List<int>? employeeIds, int? year, int? month)
    {
        var allEmployees = await _employeeRepository.GetMinDataByOrganizationIdAsync(organizationId);
        var employeeNameMap = allEmployees.ToDictionary(e => e.Id, e => e.Name);

        IEnumerable<int> targetEmployeeIds;

        if (employeeIds == null || employeeIds.Count == 0)
        {
            targetEmployeeIds = employeeNameMap.Keys;
        }
        else
        {
            targetEmployeeIds = employeeIds.Distinct();
        }

        DateOnly? periodStart = null;
        DateOnly? periodEnd = null;

        if (year.HasValue && month.HasValue)
        {
            periodStart = new DateOnly(year.Value, month.Value, 1);
            periodEnd = new DateOnly(year.Value, month.Value, DateTime.DaysInMonth(year.Value, month.Value));
        }
        else if (year.HasValue)
        {
            periodStart = new DateOnly(year.Value, 1, 1);
            periodEnd = new DateOnly(year.Value, 12, 31);
        }

        static bool Intersects(DateOnly s1, DateOnly e1, DateOnly s2, DateOnly e2) => s1 <= e2 && e1 >= s2;

        var result = new List<BllEmployeeTimeOff>();

        foreach (var employeeId in targetEmployeeIds)
        {
            var vacations = await _vacationService.GetVacations(employeeId);
            var sickLeaves = await _sickLeaveService.GetSickLeaves(employeeId);
            var personalDays = await _personalDayService.GetPersonalDays(employeeId);

            var name = employeeNameMap.GetValueOrDefault(employeeId, string.Empty);

            result.AddRange(
                vacations
                    .Where(v => !periodStart.HasValue || Intersects(v.StartDate, v.EndDate, periodStart.Value, periodEnd!.Value))
                    .Select(v => new BllEmployeeTimeOff
                    {
                        Id = v.Id,
                        EmployeeId = v.EmployeeId,
                        EmployeeName = name,
                        StartDate = v.StartDate,
                        EndDate = v.EndDate,
                        Type = TimeOffType.Vacation
                    })
            );

            result.AddRange(
                sickLeaves
                    .Where(s => !periodStart.HasValue || Intersects(s.StartDate, s.EndDate, periodStart.Value, periodEnd!.Value))
                    .Select(s => new BllEmployeeTimeOff
                    {
                        Id = s.Id,
                        EmployeeId = s.EmployeeId,
                        EmployeeName = name,
                        StartDate = s.StartDate,
                        EndDate = s.EndDate,
                        Type = TimeOffType.SickLeave
                    })
            );

            result.AddRange(
                personalDays
                    .Where(p => !periodStart.HasValue || Intersects(p.StartDate, p.EndDate, periodStart.Value, periodEnd!.Value))
                    .Select(p => new BllEmployeeTimeOff
                    {
                        Id = p.Id,
                        EmployeeId = p.EmployeeId,
                        EmployeeName = name,
                        StartDate = p.StartDate,
                        EndDate = p.EndDate,
                        Type = TimeOffType.PersonalDay
                    })
            );
        }

        return result.OrderBy(t => t.StartDate).ToList();
    }

    public async Task<BllResult<bool>> CreateAsync(BllEmployeeCreate createDto)
    {
        if (await _employeeRepository.EmployeeEmailExistsAsync(createDto.Email))
        {
            return BllResult<bool>.Fail("This email is already assigned to other employee");
        }

        if (await _employeeRepository.EmployeePhoneExistsAsync(createDto.Phone))
        {
            return BllResult<bool>.Fail("This phone is already assigned to other employee");
        }

        DalEmployeeCreate dalDto = EmployeeMapper.MapToDal(createDto);
        dalDto.Password = Helper.GenerateRandomPassword();
        
        bool created = await _employeeRepository.CreateAsync(dalDto);

        if (!created)
        {
            return BllResult<bool>.Fail("Failed to create employee.");
        }

        return BllResult<bool>.Ok(created, "Employee created.");
    }

    public async Task<BllResult<BllEmployeeBulkCreateResult>> CreateBulkAsync(List<BllEmployeeCreate> employees, int organizationId)
    {
        var result = new BllEmployeeBulkCreateResult();
        var processedEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var processedPhones = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var employee in employees)
        {
            string errorMessage = null;

            if (processedEmails.Contains(employee.Email))
            {
                errorMessage = "Duplicate email in the batch";
            }
            else if (processedPhones.Contains(employee.Phone))
            {
                errorMessage = "Duplicate phone in the batch";
            }
            else if (await _employeeRepository.EmployeeEmailExistsAsync(employee.Email))
            {
                errorMessage = "Email already exists in the system";
            }
            else if (await _employeeRepository.EmployeePhoneExistsAsync(employee.Phone))
            {
                errorMessage = "Phone already exists in the system";
            }

            if (errorMessage != null)
            {
                result.FailedEmployees.Add(new BllEmployeeCreateError
                {
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    Email = employee.Email,
                    ErrorMessage = errorMessage
                });
                result.FailedCount++;
                continue;
            }

            try
            {
                DalEmployeeCreate dalDto = EmployeeMapper.MapToDal(employee);
                dalDto.OrganizationId = organizationId;
                dalDto.Password = Helper.GenerateRandomPassword();

                bool created = await _employeeRepository.CreateAsync(dalDto);

                if (created)
                {
                    result.SuccessfulEmployees.Add(new BllEmployeeCreateSuccess
                    {
                        FirstName = employee.FirstName,
                        LastName = employee.LastName,
                        Email = employee.Email
                    });
                    result.SuccessCount++;
                    processedEmails.Add(employee.Email);
                    processedPhones.Add(employee.Phone);
                }
                else
                {
                    result.FailedEmployees.Add(new BllEmployeeCreateError
                    {
                        FirstName = employee.FirstName,
                        LastName = employee.LastName,
                        Email = employee.Email,
                        ErrorMessage = "Failed to create employee"
                    });
                    result.FailedCount++;
                }
            }
            catch (Exception ex)
            {
                result.FailedEmployees.Add(new BllEmployeeCreateError
                {
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    Email = employee.Email,
                    ErrorMessage = $"Unexpected error: {ex.Message}"
                });
                result.FailedCount++;
            }
        }

        string message = result.SuccessCount > 0
            ? $"Successfully created {result.SuccessCount} employee(s). Failed: {result.FailedCount}."
            : "Failed to create any employees.";

        return BllResult<BllEmployeeBulkCreateResult>.Ok(result, message);
    }

    public async Task<BllResult<bool>> UpdateAsync(int employeeId, BllEmployeeUpdate updateDto)
    {
        var currentEmployee = await _employeeRepository.GetByIdAsync(employeeId);
        if (currentEmployee == null)
        {
            return BllResult<bool>.Fail("Employee not found");
        }
        
        if (!string.Equals(updateDto.Email, currentEmployee.Email, StringComparison.OrdinalIgnoreCase))
        {
            if (await _employeeRepository.EmployeeEmailExistsAsync(updateDto.Email))
            {
                return BllResult<bool>.Fail("This email is already assigned to other employee");
            }
        }
        
        if (!string.Equals(updateDto.Phone, currentEmployee.Phone, StringComparison.OrdinalIgnoreCase))
        {
            if (await _employeeRepository.EmployeePhoneExistsAsync(updateDto.Phone))
            {
                return BllResult<bool>.Fail("This phone is already assigned to other employee");
            }
        }

        bool updated = await _employeeRepository.UpdateAsync(EmployeeMapper.MapToDal(updateDto));

        if (!updated)
        {
            return BllResult<bool>.Fail("Failed to update employee.");
        }

        return BllResult<bool>.Ok(updated, "Employee updated successfully");
    }


    public async Task<BllResult<bool>> DeleteAsync(int id)
    {
        var orgId = await _employeeRepository.GetOrgIdByEmployeeIdAsync(id);
        bool deleted = await _employeeRepository.DeleteAsync(id);

        if (!deleted) return BllResult<bool>.Fail("Failed to delete employee");

        return BllResult<bool>.Ok(deleted, "Employee deleted successfully");
    }

    public async Task<int?> GetOrgIdByEmployeeIdAsync(int employeeId)
        => await _employeeRepository.GetOrgIdByEmployeeIdAsync(employeeId);

    public async Task<bool> BelongsToOrganizationAsync(int employeeId, int orgId)
    {
        var empOrgId = await _employeeRepository.GetOrgIdByEmployeeIdAsync(employeeId);
        return empOrgId == orgId;
    }
}
