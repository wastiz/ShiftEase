using DAL.DTO.ScheduleDtos;
using Domain;
using Domain.Models;

namespace DAL.Mappers;

public static class ScheduleMapper
{
    public static DalSchedule ToDal(Schedule s) => new()
    {
        Id = s.Id,
        StartDate = s.StartDate,
        EndDate = s.EndDate,
        IsConfirmed = s.IsConfirmed,
        OrganizationId = s.OrganizationId
    };

    public static Schedule ToDomain(DalSchedulePost dto, int organizationId) => new()
    {
        StartDate = dto.DateFrom,
        EndDate = dto.DateTo,
        IsConfirmed = dto.IsConfirmed,
        OrganizationId = organizationId
    };

    public static Shift ShiftToDomain(DalSchedulePost.ShiftCreateDto dto, int scheduleId) => new()
    {
        Date = dto.Date,
        ShiftTemplateId = dto.ShiftTypeId,
        ScheduleId = scheduleId,
        EmployeeInShifts = dto.Employees.Select(e => new EmployeeInShift
        {
            EmployeeId = e.EmployeeId,
            Note = e.Note
        }).ToList()
    };
}
