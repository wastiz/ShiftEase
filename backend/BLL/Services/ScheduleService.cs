using BLL.DTO.ScheduleDtos;
using BLL.Interfaces;
using BLL.Mappers;
using DAL;
using DAL.Repositories;
using DAL.RepositoryInterfaces;
using Domain;
using DTOs.EmployeeDtos;
using DTOs.ScheduleDtos;
using DTOs.Shifts;
using Mappers;

namespace BLL.Services;

public class ScheduleService : IScheduleService
{
    private readonly IScheduleRepository _scheduleRepo;
    private readonly IEmployeeRepository _employeeRepo;
    private readonly IShiftTypeRepository _shiftTypeRepo;
    private readonly IGroupRepository _groupRepo;
    private readonly IOrganizationRepository _orgRepo;

    public ScheduleService(
        IScheduleRepository scheduleRepo,
        IEmployeeRepository employeeRepo,
        IShiftTypeRepository shiftTypeRepo,
        IGroupRepository groupRepo,
        IOrganizationRepository orgRepo)
    {
        _scheduleRepo = scheduleRepo;
        _employeeRepo = employeeRepo;
        _shiftTypeRepo = shiftTypeRepo;
        _groupRepo = groupRepo;
        _orgRepo = orgRepo;
    }

    public async Task<List<BllScheduleSummary>> GetGroupScheduleSummariesAsync(int orgId)
    {
        var groups = await _groupRepo.GetAllByOrganizationIdAsync(orgId);

        var result = new List<BllScheduleSummary>();

        foreach (var group in groups)
        {
            var confirmed = await _scheduleRepo.GetConfirmedMonthsByGroupIdAsync(group.Id);
            var unconfirmed = await _scheduleRepo.GetUnconfirmedMonthsByGroupIdAsync(group.Id);

            result.Add(new BllScheduleSummary
            {
                GroupId = group.Id,
                GroupName = group.Name,
                ConfirmedMonths = confirmed,
                UnconfirmedMonths = unconfirmed,
                Autorenewal = group.AutorenewSchedules
            });
        }

        return result;
    }


    public async Task<BllScheduleDataForManaging> GetScheduleDataForManagingAsync(int orgId)
    {
        return new BllScheduleDataForManaging
        {
            Employees = EmployeeMapper.MapToBll(await _employeeRepo.GetMinDataByOrganizationIdAsync(orgId)),
            ShiftTypes = ShiftTypeMapper.MapToBll(await _shiftTypeRepo.GetByOrganizationIdAsync(orgId)) ,
            Groups = GroupMapper.MapToBll(await _groupRepo.GetAllByOrganizationIdAsync(orgId)),
            OrganizationHolidays = OrganizationMapper.MapToBll(await _orgRepo.GetHolidaysByOrganizationIdAsync(orgId)),
            OrganizationWorkSchedule = OrganizationMapper.MapToBll(await _orgRepo.GetWorkScheduleByOrganizationIdAsync(orgId))
        };
    }

    public async Task<BllScheduleInfoWithShifts> GetScheduleInfoWithShiftsAsync(int orgId, BllScheduleQuery queryDto)
    {
        var start = new DateOnly(queryDto.Year, queryDto.Month, 1);
        var end = start.AddMonths(1).AddDays(-1);

        var scheduleId = await _scheduleRepo.GetScheduleIdByGroupAndDateRange(
            orgId,
            queryDto.GroupId,
            start,
            end,
            queryDto.ShowOnlyConfirmed
        );

        if (scheduleId == null)
            return null;

        return new BllScheduleInfoWithShifts
        {
            ScheduleInfo = ScheduleMapper.MapToBll(await _scheduleRepo.GetScheduleInfoByIdAsync(scheduleId.Value)),
            Shifts = ScheduleMapper.MapToBll(await _scheduleRepo.GetScheduleShiftsByIdAsync(scheduleId.Value))
        };
    }
    
    public async Task<BllScheduleInfoWithFullShifts> GetScheduleInfoWithFullShiftsAsync(int orgId, BllScheduleQuery queryDto)
    {
        var start = new DateOnly(queryDto.Year, queryDto.Month, 1);
        var end = start.AddMonths(1).AddDays(-1);

        var scheduleId = await _scheduleRepo.GetScheduleIdByGroupAndDateRange(
            orgId,
            queryDto.GroupId,
            start,
            end,
            queryDto.ShowOnlyConfirmed
        );

        if (scheduleId == null)
            return null;

        return new BllScheduleInfoWithFullShifts
        {
            ScheduleInfo = ScheduleMapper.MapToBll(await _scheduleRepo.GetScheduleInfoByIdAsync(scheduleId.Value)),
            Shifts = ScheduleMapper.MapToBll(await _scheduleRepo.GetScheduleFullShiftsByIdAsync(scheduleId.Value))
        };
    }

    public async Task<BllScheduleForView> GetScheduleForViewAsync(int orgId, BllScheduleQuery queryDto)
    {
        var holidays = await _orgRepo.GetHolidaysByOrganizationIdAsync(orgId);
        var workDays = await _orgRepo.GetWorkScheduleByOrganizationIdAsync(orgId);

        return new BllScheduleForView
        {
            Holidays = OrganizationMapper.MapToBll(holidays),
            WorkDays = OrganizationMapper.MapToBll(workDays),
            ScheduleInfoWithShifts = await GetScheduleInfoWithFullShiftsAsync(orgId, queryDto)
        };
    }

    public async Task<BllRequestResult> UpsertScheduleAsync(int orgId, BllSchedulePost post)
    {
        var start = new DateOnly(post.DateFrom.Year, post.DateFrom.Month, 1);
        var end = start.AddMonths(1).AddDays(-1);

        var autoRenewalUpdated = await _groupRepo.UpdateAutorenewalAsync(post.GroupId, post.Autorenewal);
        if (!autoRenewalUpdated) return new BllRequestResult() { Success = false, Message = "Group Id not found" };

        var existingScheduleId = await _scheduleRepo.GetScheduleIdByGroupAndDateRange(orgId, post.GroupId, start, end);

        if (existingScheduleId.HasValue)
        {
            var updated = await _scheduleRepo.UpdateScheduleAsync(orgId, existingScheduleId.Value, ScheduleMapper.MapToDal(post));
            if (!updated) return new BllRequestResult() { Success = false, Message = "Schedule with this id does not exist or doesn't belong to organization" };

            return new BllRequestResult() { Success = true, Message = "Schedule updated" };
        }

        var created = await _scheduleRepo.CreateScheduleAsync(ScheduleMapper.MapToDal(post), orgId);
        
        if (!created) return new BllRequestResult() { Success = false, Message = "Failed to create schedule" };
        
        return new BllRequestResult() { Success = true, Message = "Schedule created" };
    }

    public async Task<bool> UnconfirmScheduleAsync(int scheduleId)
        => await _scheduleRepo.UnconfirmSchedule(scheduleId);
}
