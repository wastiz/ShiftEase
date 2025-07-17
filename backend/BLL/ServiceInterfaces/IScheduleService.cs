using BLL.DTO.ScheduleDtos;
using DTOs.EmployeeDtos;
using DTOs.ScheduleDtos;
using DTOs.Shifts;

namespace BLL.Interfaces;

public interface IScheduleService
{
    Task<List<BllScheduleSummary>> GetGroupScheduleSummariesAsync(int orgId);
    Task<BllScheduleDataForManaging> GetScheduleDataForManagingAsync(int orgId);
    Task<BllScheduleInfoWithShifts?> GetScheduleInfoWithShiftsAsync(int orgId, BllScheduleQuery queryDto);
    Task<BllScheduleInfoWithFullShifts> GetScheduleInfoWithFullShiftsAsync(int orgId, BllScheduleQuery queryDto);
    Task<BllScheduleForView> GetScheduleForViewAsync(int orgId, BllScheduleQuery queryDto);
    Task<BllRequestResult> UpsertScheduleAsync(int orgId, BllSchedulePost post);
    Task<bool> UnconfirmScheduleAsync(int scheduleId);
}