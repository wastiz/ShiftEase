using BLL.Helpers;
using BLL.Contracts;
using DTOs.EmployeeOptionsDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/organizations/{orgId}/preferences")]
    [Authorize(Roles = "Employer")]
    [ServiceFilter(typeof(OrganizationOwnerFilter))]
    public class EmployerPreferencesController : ControllerBase
    {
        private readonly IPreferenceService _service;
        private readonly IEmployeeService _employeeService;

        public EmployerPreferencesController(IPreferenceService service, IEmployeeService employeeService)
        {
            _service = service;
            _employeeService = employeeService;
        }

        [HttpGet("{employeeId}")]
        public async Task<ActionResult> GetPreferences(int orgId, int employeeId)
        {
            if (!await _employeeService.BelongsToOrganizationAsync(employeeId, orgId))
                return NotFound("Employee not found.");
            var result = await _service.GetPreferences(employeeId);
            return Ok(result);
        }

        [HttpPost("{employeeId}")]
        public async Task<ActionResult> SavePreferences(int orgId, int employeeId, [FromBody] BllPreferenceBundle dto)
        {
            if (!await _employeeService.BelongsToOrganizationAsync(employeeId, orgId))
                return NotFound("Employee not found.");
            await _service.SavePreferences(employeeId, dto);
            return Ok();
        }

        [HttpGet("{employeeId}/week-days")]
        public async Task<ActionResult<List<WeekDayPreferenceDto>>> GetWeekDays(int orgId, int employeeId)
        {
            if (!await _employeeService.BelongsToOrganizationAsync(employeeId, orgId))
                return NotFound("Employee not found.");
            return Ok(await _service.GetPreferredWeekDays(employeeId));
        }
    }


    [ApiController]
    [Route("api/preferences")]
    [Authorize(Roles = "Employee")]
    public class EmployeePreferencesController : ControllerBase
    {
        private readonly IPreferenceService _service;

        public EmployeePreferencesController(IPreferenceService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult> GetMyPreferences()
        {
            var employeeId = User.GetUserId();
            return Ok(await _service.GetPreferences(employeeId));
        }

        [HttpPost]
        public async Task<ActionResult> SaveMyPreferences([FromBody] BllPreferenceBundle dto)
        {
            var employeeId = User.GetUserId();
            await _service.SavePreferences(employeeId, dto);
            return Ok();
        }

        [HttpGet("week-days")]
        public async Task<ActionResult<List<WeekDayPreferenceDto>>> GetMyWeekDays()
        {
            var employeeId = User.GetUserId();
            return Ok(await _service.GetPreferredWeekDays(employeeId));
        }
    }
}
