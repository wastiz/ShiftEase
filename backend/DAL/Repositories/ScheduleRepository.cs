using DAL.Contracts;
using DAL.DTO.EmployeeDtos;
using DAL.DTO.ScheduleDtos;
using DAL.Mappers;
using Domain;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class ScheduleRepository : IScheduleRepository
{
    private readonly AppDbContext _context;

    public ScheduleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DalScheduleSummary> GetScheduleSummaryAsync(int organizationId)
    {
        var schedules = await _context.Schedules
            .Where(s => s.OrganizationId == organizationId)
            .OrderBy(s => s.StartDate)
            .Select(s => new DalScheduleItem
            {
                Id = s.Id,
                Month = s.StartDate.ToString("yyyy-MM"),
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                TotalShifts = s.Shifts.Count,
                TotalMinutes = s.Shifts
                    .Sum(sh => sh.EmployeeInShifts.Count *
                        (int)(sh.ShiftTemplate.EndTime - sh.ShiftTemplate.StartTime - (sh.ShiftTemplate.BreakDuration ?? TimeSpan.Zero)).TotalMinutes),
                IsConfirmed = s.IsConfirmed
            })
            .ToListAsync();

        return new DalScheduleSummary
        {
            ConfirmedSchedules = schedules.Where(s => s.IsConfirmed).ToList(),
            UnconfirmedSchedules = schedules.Where(s => !s.IsConfirmed).ToList()
        };
    }

    public async Task<int?> GetScheduleIdByDateRange(int orgId, DateOnly monthStart, DateOnly monthEnd, bool showOnlyConfirmed = false)
    {
        var query = _context.Schedules
            .Where(s =>
                s.OrganizationId == orgId &&
                s.StartDate <= monthEnd &&
                s.EndDate >= monthStart
            );

        if (showOnlyConfirmed)
        {
            query = query.Where(s => s.IsConfirmed);
        }

        var schedule = await query.FirstOrDefaultAsync();
        return schedule?.Id;
    }
    
    public async Task<DalSchedule?> GetScheduleByIdAsync(int scheduleId)
    {
        var schedule = await _context.Schedules.FirstOrDefaultAsync(s => s.Id == scheduleId);
        return schedule == null ? null : ScheduleMapper.ToDal(schedule);
    }

    public async Task<List<DalShift>> GetScheduleShiftsByScheduleIdAsync(int scheduleId)
    {
        return await _context.Shifts
            .Where(sh => sh.ScheduleId == scheduleId)
            .Select(sh => new DalShift
            {
                Id = sh.Id,
                Date = sh.Date,
                ShiftTypeId = sh.ShiftTemplateId,
                ShiftTypeName = sh.ShiftTemplate.Name,
                StartTime = sh.ShiftTemplate.StartTime,
                EndTime = sh.ShiftTemplate.EndTime,
                Color = sh.ShiftTemplate.Color,
                MinEmployees = sh.ShiftTemplate.MinEmployees,
                MaxEmployees = sh.ShiftTemplate.MaxEmployees,
                BreakDuration = sh.ShiftTemplate.BreakDuration,
                Employees = sh.EmployeeInShifts
                    .Select(es => new DalEmployeeMinData
                    {
                        Id = es.Employee.Id,
                        Name = es.Employee.FirstName + " " + es.Employee.LastName,
                        DepartmentName = string.Join(", ", es.Employee.EmployeeDepartments.Select(eg => eg.Department.Name)),
                        Note = es.Note
                    })
                    .ToList()
            })
            .ToListAsync();
    }

    public async Task<List<DalShift>> GetDepartmentShiftsByScheduleIdAsync(int scheduleId, int departmentId)
    {
        return await _context.Shifts
            .Where(sh => sh.ScheduleId == scheduleId)
            .Where(sh => sh.EmployeeInShifts.Any(eis =>
                eis.Employee.EmployeeDepartments.Any(eg => eg.DepartmentId == departmentId)))
            .Select(sh => new DalShift
            {
                Id = sh.Id,
                Date = sh.Date,
                ShiftTypeId = sh.ShiftTemplateId,
                ShiftTypeName = sh.ShiftTemplate.Name,
                StartTime = sh.ShiftTemplate.StartTime,
                EndTime = sh.ShiftTemplate.EndTime,
                Color = sh.ShiftTemplate.Color,
                MinEmployees = sh.ShiftTemplate.MinEmployees,
                MaxEmployees = sh.ShiftTemplate.MaxEmployees,
                BreakDuration = sh.ShiftTemplate.BreakDuration,
                Employees = sh.EmployeeInShifts
                    .Select(es => new DalEmployeeMinData
                    {
                        Id = es.Employee.Id,
                        Name = es.Employee.FirstName + " " + es.Employee.LastName,
                        DepartmentName = string.Join(", ", es.Employee.EmployeeDepartments.Select(eg => eg.Department.Name)),
                        Note = es.Note
                    })
                    .ToList()
            })
            .ToListAsync();
    }

    public async Task<bool> CreateScheduleAsync(DalSchedulePost dto, int organizationId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var schedule = ScheduleMapper.ToDomain(dto, organizationId);

            await _context.Schedules.AddAsync(schedule);
            await _context.SaveChangesAsync();

            foreach (var shiftDto in dto.Shifts)
            {
                var shift = ScheduleMapper.ShiftToDomain(shiftDto, schedule.Id);
                await _context.Shifts.AddAsync(shift);
                await _context.SaveChangesAsync();
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> UpdateScheduleAsync(int orgId, int scheduleId, DalSchedulePost updateDto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var schedule = await _context.Schedules
                .Include(s => s.Shifts)
                .FirstOrDefaultAsync(s =>
                    s.Id == scheduleId &&
                    s.OrganizationId == orgId);

            if (schedule == null)
                return false;

            schedule.StartDate = updateDto.DateFrom;
            schedule.EndDate = updateDto.DateTo;
            schedule.IsConfirmed = updateDto.IsConfirmed;

            _context.Shifts.RemoveRange(schedule.Shifts);

            var newShifts = updateDto.Shifts.Select(s => ScheduleMapper.ShiftToDomain(s, scheduleId));

            await _context.Shifts.AddRangeAsync(newShifts);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> UnconfirmSchedule(int scheduleId)
    {
        var schedule = await _context.Schedules.FirstOrDefaultAsync(s => s.Id == scheduleId);
        if (schedule == null)
            return false;

        schedule.IsConfirmed = false;
        await _context.SaveChangesAsync();
        return true;
    }
}
