using DAL.DTO.EmployeeDtos;
using Domain.Models;
using DTOs.EmployeeDtos;

namespace BLL.Mappers;

public class EmployeeMapper
{
    public static BllEmployee MapToBll(DalEmployee e)
    {
        return new BllEmployee
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
            DepartmentIds = e.DepartmentIds,
            DepartmentNames = e.DepartmentNames,
            PrimaryDepartmentId = e.PrimaryDepartmentId
        };
    }

    public static BllEmployeeMinData MapToBll(DalEmployeeMinData e)
    {
        return new BllEmployeeMinData
        {
            Id = e.Id,
            Name = e.Name,
            Note = e.Note,
            DepartmentNames = string.IsNullOrEmpty(e.DepartmentName)
                ? new List<string>()
                : e.DepartmentName.Split(", ").ToList()
        };
    }
    
    public static List<BllEmployeeMinData> MapToBll(List<DalEmployeeMinData> employees)
    {
        return employees.Select(MapToBll).ToList();
    }

    public static DalEmployeeCreate MapToDal(BllEmployeeCreate e)
    {
        return new DalEmployeeCreate()
        {
            FirstName = e.FirstName,
            LastName = e.LastName,
            Email = e.Email,
            Phone = e.Phone,
            Position = e.Position,
            HourlyRate = e.HourlyRate,
            EmploymentRate = e.EmploymentRate,
            Priority = e.Priority,
            DepartmentIds = e.DepartmentIds,
            OrganizationId = e.OrganizationId,
            PrimaryDepartmentId = e.PrimaryDepartmentId,
        };
    }

    public static DalEmployeeUpdate MapToDal(BllEmployeeUpdate e)
    {
        return new DalEmployeeUpdate()
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
            DepartmentIds = e.DepartmentIds,
            PrimaryDepartmentId = e.PrimaryDepartmentId
        };
    }
}
