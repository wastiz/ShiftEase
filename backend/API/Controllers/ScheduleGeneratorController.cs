using BLL.DTO.ScheduleDtos;
using BLL.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/organizations/{orgId}/schedule-generator")]
[Authorize(Roles = "Employer")]
[ServiceFilter(typeof(OrganizationOwnerFilter))]
public class ScheduleGeneratorController : ControllerBase
{
    private readonly IScheduleGeneratorService      _scheduleGeneratorService;
    private readonly IAcoScheduleGeneratorService   _acoScheduleGeneratorService;
    private readonly IGaScheduleGeneratorService    _gaScheduleGeneratorService;
    private readonly IAcoGaScheduleGeneratorService _acoGaScheduleGeneratorService;

    public ScheduleGeneratorController(
        IScheduleGeneratorService      scheduleService,
        IAcoScheduleGeneratorService   acoScheduleGeneratorService,
        IGaScheduleGeneratorService    gaScheduleGeneratorService,
        IAcoGaScheduleGeneratorService acoGaScheduleGeneratorService)
    {
        _scheduleGeneratorService      = scheduleService;
        _acoScheduleGeneratorService   = acoScheduleGeneratorService;
        _gaScheduleGeneratorService    = gaScheduleGeneratorService;
        _acoGaScheduleGeneratorService = acoGaScheduleGeneratorService;
    }

    [HttpPost("generate-greedy")]
    public async Task<IActionResult> GenerateGreedySchedule(int orgId, [FromBody] BllScheduleGenerateRequest request)
    {
        var result = await _scheduleGeneratorService.GenerateGreedyScheduleAsync(orgId, request);
        if (result.Status == GenerateStatus.Error)
            return BadRequest(new ErrorResponse("SCHEDULE_GENERATION_ERROR", result.Error?.ToString() ?? "Schedule generation failed."));
        return Ok(result);
    }

    [HttpPost("generate-aco")]
    public async Task<IActionResult> GenerateAcoSchedule(int orgId, [FromBody] BllAcoScheduleGenerateRequest request)
    {
        var result = await _acoScheduleGeneratorService.GenerateAcoScheduleAsync(orgId, request);
        if (result.Status == GenerateStatus.Error)
            return BadRequest(new ErrorResponse("SCHEDULE_GENERATION_ERROR", result.Error?.ToString() ?? "Schedule generation failed."));
        return Ok(result);
    }

    [HttpPost("generate-ga")]
    public async Task<IActionResult> GenerateGaSchedule(int orgId, [FromBody] BllGaScheduleGenerateRequest request)
    {
        var result = await _gaScheduleGeneratorService.GenerateGaScheduleAsync(orgId, request);
        if (result.Status == GenerateStatus.Error)
            return BadRequest(new ErrorResponse("SCHEDULE_GENERATION_ERROR", result.Error?.ToString() ?? "Schedule generation failed."));
        return Ok(result);
    }

    [HttpPost("generate-aco-ga")]
    public async Task<IActionResult> GenerateAcoGaSchedule(int orgId, [FromBody] BllAcoGaScheduleGenerateRequest request)
    {
        var result = await _acoGaScheduleGeneratorService.GenerateAcoGaScheduleAsync(orgId, request);
        if (result.Status == GenerateStatus.Error)
            return BadRequest(new ErrorResponse("SCHEDULE_GENERATION_ERROR", result.Error?.ToString() ?? "Schedule generation failed."));
        return Ok(result);
    }
}
