using DAL.DTO.EmployeeDtos;
using DAL.DTO.ScheduleDtos;
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
    
    //Get All Schedules by Group Id
    public async Task<List<Schedule>> GetSchedulesByGroupIdAsync(int groupId)
    {
        return await _context.Schedules.Where(s => s.GroupId == groupId).ToListAsync();
    }
    
    //Get months, where schedules are unconfirmed
    public async Task<List<string>> GetUnconfirmedMonthsByGroupIdAsync(int groupId)
    {
        var schedules = await GetSchedulesByGroupIdAsync(groupId);
        return schedules
            .Where(s => !s.IsConfirmed)
            .Select(s => new DateTime(s.StartDate.Year, s.StartDate.Month, 1).ToString("MMMM yyyy"))
            .Distinct()
            .ToList();
    }
    
    //Get months, where schedules are confirmed
    public async Task<List<string>> GetConfirmedMonthsByGroupIdAsync(int groupId)
    {
        var schedules = await GetSchedulesByGroupIdAsync(groupId);
        return schedules
            .Where(s => s.IsConfirmed)
            .Select(s => new DateTime(s.StartDate.Year, s.StartDate.Month, 1).ToString("MMMM yyyy"))
            .Distinct()
            .ToList();

    }
    
    //Get schedule id for specific group and month
    public async Task<int?> GetScheduleIdByGroupAndDateRange(int orgId, int groupId, DateOnly monthStart, DateOnly monthEnd, bool showOnlyConfirmed = false)
    {
        var query = _context.Schedules
            .Where(s =>
                s.OrganizationId == orgId &&
                s.GroupId == groupId &&
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
    
    //Get schedule basic info (from date, to date, confirmed?, group id)
    public async Task<DalScheduleInfo?> GetScheduleInfoByIdAsync(int scheduleId)
    {
        return await _context.Schedules
            .Where(s => s.Id == scheduleId)
            .Select(s => new DalScheduleInfo
            {
                Id = s.Id,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                IsConfirmed = s.IsConfirmed,
                GroupId = s.GroupId
            })
            .FirstOrDefaultAsync();
    }
    
    //Get schedule min shifts
    public async Task<List<DalShift>?> GetScheduleShiftsByIdAsync(int scheduleId)
    {
        return await _context.Shifts
            .Where(sh => sh.ScheduleId == scheduleId)
            .Select(sh => new DalShift
            {
                Id = sh.Id,
                Date = sh.Date,
                ShiftTypeId = sh.ShiftTypeId,
                Employees = sh.EmployeeInShifts
                    .Select(es => new DalEmployeeMinData
                    {
                        Id = es.Employee.Id,
                        Name = es.Employee.FirstName + " " + es.Employee.LastName,
                        GroupName = es.Employee.Group.Name,
                    })
                    .ToList()
            })
            .ToListAsync();
    }
    
    //Get Schedule Full Shifts
    public async Task<List<DalFullShift>?> GetScheduleFullShiftsByIdAsync(int scheduleId)
    {
        return await _context.Shifts
            .Where(sh => sh.ScheduleId == scheduleId)
            .Select(sh => new DalFullShift
            {
                Id = sh.Id,
                Date = sh.Date,
                ShiftTypeName = sh.ShiftType.Name,
                From = sh.ShiftType.StartTime,
                To = sh.ShiftType.EndTime,
                Color = sh.ShiftType.Color,
                EmployeeNeeded = sh.ShiftType.EmployeeNeeded,
                Employees = sh.EmployeeInShifts
                    .Select(es => new DalEmployeeMinData
                    {
                        Id = es.Employee.Id,
                        Name = es.Employee.FirstName + " " + es.Employee.LastName,
                        GroupName = es.Employee.Group.Name,
                    })
                    .ToList()
            })
            .ToListAsync();
    }
    
    //Adding new schedule
    public async Task<bool> CreateScheduleAsync(DalSchedulePost dto, int organizationId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var schedule = new Schedule
            {
                StartDate = dto.DateFrom,
                EndDate = dto.DateTo,
                OrganizationId = organizationId,
                GroupId = dto.GroupId,
                IsConfirmed = dto.IsConfirmed
            };

            await _context.Schedules.AddAsync(schedule);
            await _context.SaveChangesAsync();

            foreach (var shiftDto in dto.Shifts)
            {
                var shift = new Shift
                {
                    Date = shiftDto.Date,
                    ShiftTypeId = shiftDto.ShiftTypeId,
                    ScheduleId = schedule.Id
                };

                await _context.Shifts.AddAsync(shift);
                await _context.SaveChangesAsync();

                foreach (var employeeId in shiftDto.EmployeeIds)
                {
                    var employeeInShift = new EmployeeInShift
                    {
                        EmployeeId = employeeId,
                        ShiftId = shift.Id
                    };

                    await _context.EmployeeInShifts.AddAsync(employeeInShift);
                }
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
    
    //Editing Schedule by its ID
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
            {
                return false;
            }

            schedule.GroupId = updateDto.GroupId;
            schedule.StartDate = updateDto.DateFrom;
            schedule.EndDate = updateDto.DateTo;
            schedule.IsConfirmed = updateDto.IsConfirmed;

            _context.Shifts.RemoveRange(schedule.Shifts);

            var newShifts = updateDto.Shifts.Select(shift => new Shift
            {
                ScheduleId = scheduleId,
                ShiftTypeId = shift.ShiftTypeId,
                Date = shift.Date,
                EmployeeInShifts = shift.EmployeeIds.Select(empId => new EmployeeInShift
                {
                    EmployeeId = empId
                }).ToList()
            });

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
    
    //Unconfirm Schedule by its id
    public async Task<bool> UnconfirmSchedule(int scheduleId)
    {
        var schedule = await _context.Schedules.FirstOrDefaultAsync(s => s.Id == scheduleId);
        if (schedule == null)
        {
            return false;
        }

        schedule.IsConfirmed = false;
        await _context.SaveChangesAsync();
        return true;
    }

}