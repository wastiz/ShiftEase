using BLL.Interfaces;
using DTOs.ScheduleDtos;
using Microsoft.AspNetCore.Mvc;
using ShiftEaseAPI.Middlewares;

[ApiController]
[Route("api/[controller]")]
public class ScheduleController : ControllerBase
{
    private readonly IScheduleService _scheduleService;

    public ScheduleController(IScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }
    
    //Get Schedule Summaries (for each group unconfirmed, confirmed months)
    [HttpGet("schedule-summaries")]
    public async Task<IActionResult> GetScheduleSummariesData()
    {
        int orgId = int.Parse(Request.Headers["X-Organization-Id"]);
        var result = await _scheduleService.GetGroupScheduleSummariesAsync(orgId);
        if (result == null || !result.Any())
            return NotFound("No schedules found for this organization");

        return Ok(result);
    }
    
    //Get data for schedule managing (groups, employees, shiftTypes)
    [HttpGet("schedule-data-for-managing")]
    public async Task<IActionResult> GetScheduleDataForManaging()
    {
        int orgId = int.Parse(Request.Headers["X-Organization-Id"]);
        var data = await _scheduleService.GetScheduleDataForManagingAsync(orgId);
        return Ok(data);
    }
    
    //Get schedule full data (schedule info and it shifts with employee infos)
    [HttpGet("schedule-info-with-shifts")]
    public async Task<IActionResult> GetScheduleInfoWithShifts([FromQuery] BllScheduleQuery queryDto)
    {
        int orgId = int.Parse(Request.Headers["X-Organization-Id"]);
        var data = await _scheduleService.GetScheduleInfoWithShiftsAsync(orgId, queryDto);
        return Ok(data);
    }
    
    //GET: Get schedule for view only
    [HttpGet("get-schedule-for-view")]
    [AllowEmployeeAccess]
    public async Task<IActionResult> GetScheduleForView([FromQuery] BllScheduleQuery query)
    {
        int orgId = int.Parse(Request.Headers["X-Organization-Id"]);
        var schedule = await _scheduleService.GetScheduleForViewAsync(orgId, query);
        return schedule == null ? NotFound("Schedule not found for this group and month.") : Ok(schedule);
    }

    [HttpPost("update-schedule")]
    public async Task<IActionResult> UpsertSchedule([FromBody] BllSchedulePost dto)
    {
        int orgId = int.Parse(Request.Headers["X-Organization-Id"]);

        try
        {
            var message = await _scheduleService.UpsertScheduleAsync(orgId, dto);
            return Ok(message);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("unconfirm/{scheduleId}")]
    public async Task<IActionResult> UnconfirmSchedule(int scheduleId)
    {
        var success = await _scheduleService.UnconfirmScheduleAsync(scheduleId);
        return success
            ? Ok("Schedule unconfirmed successfully.")
            : NotFound("Schedule not found.");
    }
}
