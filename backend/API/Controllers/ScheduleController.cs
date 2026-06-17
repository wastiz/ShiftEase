using BLL.Helpers;
using BLL.Contracts;
using DTOs.ScheduleDtos;
using Microsoft.AspNetCore.Mvc;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/organizations/{orgId}/schedules")]
[Authorize(Roles = "Employer")]
[ServiceFilter(typeof(OrganizationOwnerFilter))]
public class ScheduleEmployerController : ControllerBase
{
    private readonly IScheduleService _scheduleService;
    private readonly IScheduleExportService _exportService;
    private readonly INotificationService _notificationService;

    public ScheduleEmployerController(
        IScheduleService scheduleService,
        IScheduleExportService exportService,
        INotificationService notificationService)
    {
        _scheduleService = scheduleService;
        _exportService = exportService;
        _notificationService = notificationService;
    }

    [HttpGet("schedule-summaries")]
    public async Task<IActionResult> GetScheduleSummaries(int orgId)
    {
        var result = await _scheduleService.GetScheduleSummaryAsync(orgId);
        return Ok(result);
    }

    [HttpGet("schedule-management")]
    public async Task<IActionResult> GetScheduleManagement(int orgId, [FromQuery] int month, [FromQuery] int year)
    {
        try
        {
            var data = await _scheduleService.GetScheduleEditorDataAsync(orgId, month, year);
            return Ok(data);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", ex.Message));
        }
    }

    [HttpGet("schedule")]
    public async Task<IActionResult> GetSchedule(int orgId, [FromQuery] int month, [FromQuery] int year, [FromQuery] int? departmentId = null)
    {
        try
        {
            var data = await _scheduleService.GetScheduleByMonthAsync(orgId, month, year, departmentId: departmentId);
            return Ok(data);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", ex.Message));
        }
    }

    [HttpGet("confirmed")]
    public async Task<IActionResult> GetConfirmedSchedule(int orgId, [FromQuery] int month, [FromQuery] int year, [FromQuery] int? departmentId = null)
    {
        try
        {
            var data = await _scheduleService.GetScheduleByMonthAsync(orgId, month, year, onlyConfirmed: true, departmentId: departmentId);
            return Ok(data);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", ex.Message));
        }
    }

    [HttpPost("create-empty-schedule")]
    public async Task<IActionResult> CreateEmptySchedule(int orgId)
    {
        var result = await _scheduleService.CreateEmptyScheduleAsync(orgId);
        if (!result.Success)
            return BadRequest(new ErrorResponse("BAD_REQUEST", result.Message));

        return Ok(new { message = result.Message });
    }

    [HttpPost("update-schedule")]
    public async Task<IActionResult> UpsertSchedule(int orgId, [FromBody] BllSchedulePost dto)
    {
        var result = await _scheduleService.UpsertScheduleAsync(orgId, dto);
        if (!result.Success)
            return BadRequest(new ErrorResponse("BAD_REQUEST", result.Message));

        if (dto.IsConfirmed)
        {
            var employeeIds = dto.Shifts
                .SelectMany(s => s.Employees)
                .Select(e => e.EmployeeId)
                .Distinct();

            foreach (var employeeId in employeeIds)
            {
                await _notificationService.CreateAsync(
                    employeeId,
                    UserRole.Employee,
                    NotificationTypes.ScheduleUpdate,
                    "Your schedule has been updated",
                    "Schedule Updated",
                    new { startDate = dto.StartDate.ToString(), endDate = dto.EndDate.ToString() }
                );
            }
        }

        return Ok(new { message = result.Message });
    }

    [HttpPost("{scheduleId}/unconfirm")]
    public async Task<IActionResult> UnconfirmSchedule(int orgId, int scheduleId)
    {
        if (!await _scheduleService.ScheduleBelongsToOrgAsync(scheduleId, orgId))
            return NotFound(new ErrorResponse("NOT_FOUND", "Schedule not found."));

        var success = await _scheduleService.UnconfirmScheduleAsync(scheduleId);
        if (!success)
            return NotFound(new ErrorResponse("NOT_FOUND", "Schedule not found."));

        return Ok(new { message = "Schedule unconfirmed successfully." });
    }

    [HttpGet("{id}/export")]
    public async Task<IActionResult> ExportSchedule(int orgId, int id)
    {
        if (!await _scheduleService.ScheduleBelongsToOrgAsync(id, orgId))
            return NotFound(new ErrorResponse("NOT_FOUND", "Schedule not found."));

        try
        {
            var fileBytes = await _exportService.ExportScheduleToExcelAsync(id);
            var fileName = $"Schedule_{id}_{DateTime.Now:yyyy-MM-dd}.xlsx";
            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ErrorResponse("NOT_FOUND", ex.Message));
        }
        catch (Exception)
        {
            return StatusCode(500, new ErrorResponse("INTERNAL_ERROR", "Failed to export schedule."));
        }
    }
}


[ApiController]
[Route("api/schedules")]
[Authorize(Roles = "Employee")]
public class ScheduleEmployeeController : ControllerBase
{
    private readonly IScheduleService _scheduleService;

    public ScheduleEmployeeController(IScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    [HttpGet("confirmed/my-shifts")]
    public async Task<IActionResult> GetMyConfirmedShifts([FromQuery] int month, [FromQuery] int year)
    {
        int employeeId = User.GetUserId();
        try
        {
            var schedule = await _scheduleService.GetMyScheduleByMonthAsync(employeeId, month, year);
            return Ok(schedule);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", ex.Message));
        }
    }
}
