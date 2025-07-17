using DAL.DTO.ScheduleDtos;
using Domain;

namespace DAL.Repositories;

public interface IScheduleRepository
{
    Task<List<Schedule>> GetSchedulesByGroupIdAsync(int groupId);
    Task<List<string>> GetUnconfirmedMonthsByGroupIdAsync(int groupId);
    Task<List<string>> GetConfirmedMonthsByGroupIdAsync(int groupId);
    Task<int?> GetScheduleIdByGroupAndDateRange(int orgId, int groupId, DateOnly monthStart, DateOnly monthEnd, bool showOnlyConfirmed = false);
    Task<DalScheduleInfo?> GetScheduleInfoByIdAsync(int scheduleId);
    Task<List<DalShift>?> GetScheduleShiftsByIdAsync(int scheduleId);
    Task<List<DalFullShift>?> GetScheduleFullShiftsByIdAsync(int scheduleId);
    Task<bool> CreateScheduleAsync(DalSchedulePost dto, int organizationId);
    Task<bool> UpdateScheduleAsync(int orgId, int scheduleId, DalSchedulePost updateDto);
    Task<bool> UnconfirmSchedule(int scheduleId);
}