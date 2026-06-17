using DAL.DTO.EmployeeDtos;
using Domain.Models;

namespace DAL.Mappers;

public static class EmployeeMapper
{
    public static DalEmployee ToDal(Employee e) => new()
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
        OrganizationId = e.OrganizationId,
        OrganizationName = e.Organization?.Name,
        DepartmentIds = e.EmployeeDepartments?
            .Select(eg => eg.DepartmentId).ToList() ?? new(),
        DepartmentNames = e.EmployeeDepartments?
            .Select(eg => eg.Department?.Name ?? string.Empty).ToList() ?? new(),
        PrimaryDepartmentId = e.EmployeeDepartments?
            .Where(eg => eg.IsPrimary)
            .Select(eg => (int?)eg.DepartmentId)
            .FirstOrDefault()
    };

    public static Employee ToDomain(DalEmployeeCreate dto) => new()
    {
        FirstName = dto.FirstName,
        LastName = dto.LastName,
        Email = dto.Email,
        Phone = dto.Phone,
        Password = dto.Password,
        Position = dto.Position,
        HourlyRate = dto.HourlyRate,
        EmploymentRate = dto.EmploymentRate,
        Priority = dto.Priority,
        OrganizationId = dto.OrganizationId
    };
}
