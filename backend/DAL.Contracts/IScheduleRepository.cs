using DAL.DTO.ScheduleDtos;
using Domain;

namespace DAL.Contracts;

public interface IScheduleRepository
{
    Task<DalScheduleSummary> GetScheduleSummaryAsync(int orgId);
    Task<int?> GetScheduleIdByDateRange(int orgId, DateOnly monthStart, DateOnly monthEnd, bool showOnlyConfirmed = false);
    Task<DalSchedule?> GetScheduleByIdAsync(int scheduleId);
    Task<List<DalShift>> GetScheduleShiftsByScheduleIdAsync(int scheduleId);
    Task<List<DalShift>> GetDepartmentShiftsByScheduleIdAsync(int scheduleId, int departmentId);
    Task<bool> CreateScheduleAsync(DalSchedulePost dto, int organizationId);
    Task<bool> UpdateScheduleAsync(int orgId, int scheduleId, DalSchedulePost updateDto);
    Task<bool> UnconfirmSchedule(int scheduleId);
}