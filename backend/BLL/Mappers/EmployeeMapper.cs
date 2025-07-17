using DAL.DTO.EmployeeDtos;
using Domain.Models;
using DTOs.EmployeeDtos;

namespace BLL.Mappers;

public class EmployeeMapper
{
    public static BllEmployeeFullData MapToBll(DalEmployeeFullData e)
    {
        return new BllEmployeeFullData
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
            GroupName = e.GroupName
        };
    }

    public static BllEmployeeMinData MapToBll(DalEmployeeMinData e)
    {
        return new BllEmployeeMinData
        {
            Id = e.Id,
            Name = e.Name,
            GroupName = e.GroupName ?? ""
        };
    }
    
    public static List<BllEmployeeMinData> MapToBll(List<DalEmployeeMinData> employees)
    {
        return employees.Select(MapToBll).ToList();
    }

    public static DalEmployeeCreate MapToDal(BllEmployeeCreate e, int organizationId)
    {
        return new DalEmployeeCreate()
        {
            FirstName = e.FirstName,
            LastName = e.LastName,
            Email = e.Email,
            Phone = e.Phone,
            Password = e.Password,
            Position = e.Position,
            Workload = e.Workload,
            Salary = e.Salary,
            SalaryInHour = e.SalaryInHour,
            Priority = e.Priority,
            GroupId = e.GroupId,
            OrganizationId = organizationId,
        };
    }

    public static DalEmployeeUpdate MapToDal(BllEmployeeUpdate e, int employeeId)
    {
        return new DalEmployeeUpdate()
        {
            Id = employeeId,
            FirstName = e.FirstName,
            LastName = e.LastName,
            Email = e.Email,
            Phone = e.Phone,
            Password = e.Password,
            Position = e.Position,
            Workload = e.Workload,
            Salary = e.Salary,
            SalaryInHour = e.SalaryInHour,
            Priority = e.Priority,
            GroupId = e.GroupId
        };
    }
}
