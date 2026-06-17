using BLL.DTO.ScheduleDtos;
using DTOs.EmployeeDtos;
using DTOs.ScheduleDtos;

namespace BLL.Contracts;

public interface IScheduleService
{
    Task<BllScheduleSummary> GetScheduleSummaryAsync(int orgId);
    Task<BllScheduleEditorData> GetScheduleEditorDataAsync(int orgId, int month, int year);
    Task<BllSchedule> GetScheduleByMonthAsync(int orgId, int month, int year, bool onlyConfirmed = false, int? departmentId = null);
    Task<BllSchedule> GetScheduleByIdAsync(int scheduleId);
    Task<BllResult<bool>> UpsertScheduleAsync(int orgId, BllSchedulePost post);
    Task<bool> UnconfirmScheduleAsync(int scheduleId);
    Task<BllResult<bool>> CreateEmptyScheduleAsync(int orgId);
    Task<BllSchedule> GetMyScheduleByMonthAsync(int employeeId, int month, int year);
    Task<bool> ScheduleBelongsToOrgAsync(int scheduleId, int orgId);
}
