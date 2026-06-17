using BLL.DTO.ScheduleDtos;
using BLL.Helpers;
using BLL.Contracts;
using BLL.Mappers;
using DAL.DTO.ScheduleDtos;
using DAL.Contracts;
using DTOs.EmployeeDtos;
using DTOs.ScheduleDtos;

namespace BLL.Services;

public class ScheduleService : IScheduleService
{
    private readonly IScheduleRepository _scheduleRepo;
    private readonly IEmployeeService _employeeService;
    private readonly IShiftTemplateService _shiftTemplateService;
    private readonly IDepartmentService _departmentService;
    private readonly IOrganizationService _orgService;
    public ScheduleService(
        IScheduleRepository scheduleRepo,
        IEmployeeService employeeService,
        IShiftTemplateService shiftTemplateService,
        IDepartmentService departmentService,
        IOrganizationService orgService)
    {
        _scheduleRepo = scheduleRepo;
        _employeeService = employeeService;
        _shiftTemplateService = shiftTemplateService;
        _departmentService = departmentService;
        _orgService = orgService;
    }

    public async Task<BllScheduleSummary> GetScheduleSummaryAsync(int orgId)
    {
        var dalSummary = await _scheduleRepo.GetScheduleSummaryAsync(orgId);

        var shiftTypes = await _shiftTemplateService.GetByOrganizationIdAsync(orgId);
        var departments     = await _departmentService.GetAllByOrganizationIdAsync(orgId);
        var holidays   = await _orgService.GetOrganizationHolidaysByIdAsync(orgId);
        var orgSchedule = await _orgService.GetOrganizationWorkDaysByIdAsync(orgId);

        async Task<BllScheduleItem> BuildItemAsync(DalScheduleItem dal)
        {
            var dalShifts = await _scheduleRepo.GetScheduleShiftsByScheduleIdAsync(dal.Id);

            var shiftsForCoverage = dalShifts
                .Select(s => new BllShiftForCoverage
                {
                    Date = s.Date,
                    ShiftTypeId = s.ShiftTypeId,
                    EmployeeCount = s.Employees.Count
                })
                .ToList();

            var coverage = CoverageHelper.Check(
                dal.StartDate,
                dal.EndDate,
                shiftsForCoverage,
                shiftTypes,
                departments,
                holidays,
                orgSchedule);

            return new BllScheduleItem
            {
                Id          = dal.Id,
                Month       = dal.StartDate.ToString("MMMM"),
                Year        = dal.StartDate.Year.ToString(),
                Coverage    = coverage.CoverageStatus,
                Warnings    = coverage.Warnings,
                TotalShifts = dal.TotalShifts,
                TotalHours  = dal.TotalMinutes / 60
            };
        }

        var confirmedItems = new List<BllScheduleItem>();
        foreach (var dal in dalSummary.ConfirmedSchedules)
        {
            confirmedItems.Add(await BuildItemAsync(dal));
        }

        var unconfirmedItems = new List<BllScheduleItem>();
        foreach (var dal in dalSummary.UnconfirmedSchedules)
        {
            unconfirmedItems.Add(await BuildItemAsync(dal));
        }

        return new BllScheduleSummary
        {
            ConfirmedSchedules   = confirmedItems,
            UnconfirmedSchedules = unconfirmedItems
        };
    }

    public async Task<BllScheduleEditorData> GetScheduleEditorDataAsync(int orgId, int month, int year)
    {
        var start = new DateOnly(year, month, 1);
        var end = start.AddMonths(1).AddDays(-1);

        var employees = await _employeeService.GetMinDataByOrganizationIdAsync(orgId);
        var employeeIds = employees.Select(e => e.Id).ToList();

        var schedule = await GetScheduleByMonthAsync(orgId, month, year);

        return new BllScheduleEditorData
        {
            Employees = employees,
            Departments = await _departmentService.GetAllByOrganizationIdAsync(orgId),
            ShiftTypes = await _shiftTemplateService.GetByOrganizationIdAsync(orgId),
            OrganizationHolidays = await _orgService.GetOrganizationHolidaysByIdAsync(orgId),
            OrganizationSchedule = await _orgService.GetOrganizationWorkDaysByIdAsync(orgId),
            EmployeeTimeOffs = await _employeeService.GetEmployeesTimeOffAsync(employeeIds, start, end),
            Schedule = schedule
        };
    }

    public async Task<BllSchedule> GetScheduleByMonthAsync(int orgId, int month, int year, bool onlyConfirmed = false, int? departmentId = null)
    {
        if (year < 1 || year > 9999 || month < 1 || month > 12)
            throw new ArgumentException($"Invalid Year ({year}) or Month ({month}) value.");

        var start = new DateOnly(year, month, 1);
        var end = start.AddMonths(1).AddDays(-1);

        var scheduleId = await _scheduleRepo.GetScheduleIdByDateRange(orgId, start, end, onlyConfirmed);

        if (scheduleId == null)
        {
            return new BllSchedule
            {
                Id = null,
                StartDate = start,
                EndDate = end,
                IsConfirmed = false,
                Shifts = new()
            };
        }

        var scheduleInfo = await _scheduleRepo.GetScheduleByIdAsync(scheduleId.Value);

        List<BllShift> shifts;
        if (departmentId.HasValue)
        {
            shifts = ScheduleMapper.MapToBll(
                await _scheduleRepo.GetDepartmentShiftsByScheduleIdAsync(scheduleId.Value, departmentId.Value));
        }
        else
        {
            shifts = ScheduleMapper.MapToBll(
                await _scheduleRepo.GetScheduleShiftsByScheduleIdAsync(scheduleId.Value));
        }

        return new BllSchedule
        {
            Id = scheduleInfo.Id,
            StartDate = scheduleInfo.StartDate,
            EndDate = scheduleInfo.EndDate,
            IsConfirmed = scheduleInfo.IsConfirmed,
            Shifts = shifts
        };
    }
    
    public async Task<BllSchedule> GetScheduleByIdAsync(int scheduleId)
    {
        var scheduleInfo = await _scheduleRepo.GetScheduleByIdAsync(scheduleId);
        if (scheduleInfo == null)
            throw new KeyNotFoundException($"Schedule {scheduleId} not found");

        return new BllSchedule
        {
            Id = scheduleInfo.Id,
            StartDate = scheduleInfo.StartDate,
            EndDate = scheduleInfo.EndDate,
            IsConfirmed = scheduleInfo.IsConfirmed,
            Shifts = ScheduleMapper.MapToBll(
                await _scheduleRepo.GetScheduleShiftsByScheduleIdAsync(scheduleId))
        };
    }

    public async Task<BllResult<bool>> UpsertScheduleAsync(int orgId, BllSchedulePost post)
    {
        var existingScheduleId = await _scheduleRepo.GetScheduleIdByDateRange(
            orgId, post.StartDate, post.EndDate);

        if (existingScheduleId.HasValue)
        {
            var updated = await _scheduleRepo.UpdateScheduleAsync(
                orgId, existingScheduleId.Value, ScheduleMapper.MapToDal(post));

            if (!updated)
                return new BllResult<bool>
                {
                    Success = false,
                    Message = "Schedule with this id does not exist or doesn't belong to organization"
                };

            return new BllResult<bool> { Success = true, Message = "Schedule updated" };
        }

        var created = await _scheduleRepo.CreateScheduleAsync(ScheduleMapper.MapToDal(post), orgId);

        if (!created)
            return new BllResult<bool> { Success = false, Message = "Failed to create schedule" };

        return new BllResult<bool> { Success = true, Message = "Schedule created" };
    }
    
    public async Task<BllResult<bool>> CreateEmptyScheduleAsync(int orgId)
    {
        var today = DateTime.Today;
        var firstDayOfMonth = new DateOnly(today.Year, today.Month, 1);
        var lastDayOfMonth = new DateOnly(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));

        var created = await _scheduleRepo.CreateScheduleAsync(new DalSchedulePost
        {
            DateFrom = firstDayOfMonth,
            DateTo = lastDayOfMonth,
            IsConfirmed = false,
        }, orgId);

        if (!created)
            return new BllResult<bool> { Success = false, Message = "Failed to create empty schedule" };

        return new BllResult<bool> { Success = true, Message = "Empty schedule created" };
    }

    public async Task<bool> UnconfirmScheduleAsync(int scheduleId)
        => await _scheduleRepo.UnconfirmSchedule(scheduleId);

    public async Task<BllSchedule> GetMyScheduleByMonthAsync(int employeeId, int month, int year)
    {
        var orgId = await _employeeService.GetOrgIdByEmployeeIdAsync(employeeId);
        if (orgId == null)
            throw new ArgumentException("Employee not found or not assigned to an organization.");

        var schedule = await GetScheduleByMonthAsync(orgId.Value, month, year, onlyConfirmed: true);
        return schedule with
        {
            Shifts = schedule.Shifts
                .Where(s => s.Employees.Any(e => e.Id == employeeId))
                .ToList()
        };
    }

    public async Task<bool> ScheduleBelongsToOrgAsync(int scheduleId, int orgId)
    {
        var schedule = await _scheduleRepo.GetScheduleByIdAsync(scheduleId);
        return schedule != null && schedule.OrganizationId == orgId;
    }
}
