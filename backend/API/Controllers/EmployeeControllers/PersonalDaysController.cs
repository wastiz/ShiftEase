using BLL.Helpers;
using BLL.Contracts;
using Domain.Enums;
using DTOs.EmployeeDtos;
using DTOs.EmployeeOptionsDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/organizations/{orgId}/personal-days")]
[Authorize(Roles = "Employer")]
[ServiceFilter(typeof(OrganizationOwnerFilter))]
public class EmployerPersonalDaysController : ControllerBase
{
    private readonly IPersonalDayService _service;
    private readonly IEmployeeService _employeeService;
    private readonly INotificationService _notificationService;

    public EmployerPersonalDaysController(
        IPersonalDayService service,
        IEmployeeService employeeService,
        INotificationService notificationService)
    {
        _service = service;
        _employeeService = employeeService;
        _notificationService = notificationService;
    }

    [HttpGet("{employeeId}")]
    public async Task<ActionResult<List<BllPersonalDayDto>>> GetPersonalDays(int orgId, int employeeId)
    {
        if (!await _employeeService.BelongsToOrganizationAsync(employeeId, orgId))
            return NotFound(new ErrorResponse("NOT_FOUND", "Employee not found."));

        return Ok(await _service.GetPersonalDays(employeeId));
    }

    [HttpDelete("{employeeId}/{id}")]
    public async Task<ActionResult> DeletePersonalDay(int orgId, int employeeId, int id)
    {
        if (!await _employeeService.BelongsToOrganizationAsync(employeeId, orgId))
            return NotFound(new ErrorResponse("NOT_FOUND", "Employee not found."));

        var success = await _service.DeletePersonalDay(employeeId, id);
        if (!success)
            return NotFound(new ErrorResponse("NOT_FOUND", "Personal day not found."));

        return Ok();
    }

    [HttpGet("requests/{employeeId}")]
    public async Task<ActionResult<List<PersonalDayRequestDto>>> GetPersonalDayRequests(int orgId, int employeeId)
    {
        if (!await _employeeService.BelongsToOrganizationAsync(employeeId, orgId))
            return NotFound(new ErrorResponse("NOT_FOUND", "Employee not found."));

        return Ok(await _service.GetPersonalDayRequests(employeeId));
    }

    [HttpGet("pending-requests")]
    public async Task<ActionResult<List<PersonalDayRequestDto>>> GetPendingRequests(int orgId)
    {
        return Ok(await _service.GetPendingPersonalDayRequestsByOrganization(orgId));
    }

    [HttpPost("requests/{requestId}/approve")]
    public async Task<ActionResult> ApprovePersonalDayRequest(int orgId, int requestId)
    {
        var pending = await _service.GetPendingPersonalDayRequestsByOrganization(orgId);
        if (!pending.Any(r => r.Id == requestId))
            return NotFound(new ErrorResponse("NOT_FOUND", "Request not found in this organization."));

        var employeeId = await _service.ApprovePersonalDayRequest(requestId);
        if (employeeId == null)
            return NotFound(new ErrorResponse("NOT_FOUND", "Request not found."));

        await _notificationService.CreateAsync(
            employeeId.Value,
            UserRole.Employee,
            NotificationTypes.PersonalDayApproved,
            "Your personal day request has been approved",
            "Personal Day Approved"
        );
        return Ok();
    }

    [HttpPost("requests/{requestId}/reject")]
    public async Task<ActionResult> RejectPersonalDayRequest(int orgId, int requestId)
    {
        var pending = await _service.GetPendingPersonalDayRequestsByOrganization(orgId);
        if (!pending.Any(r => r.Id == requestId))
            return NotFound(new ErrorResponse("NOT_FOUND", "Request not found in this organization."));

        var employeeId = await _service.RejectPersonalDayRequest(requestId);
        if (employeeId == null)
            return NotFound(new ErrorResponse("NOT_FOUND", "Request not found."));

        await _notificationService.CreateAsync(
            employeeId.Value,
            UserRole.Employee,
            NotificationTypes.PersonalDayRejected,
            "Your personal day request has been rejected",
            "Personal Day Rejected"
        );
        return Ok();
    }

    [HttpPost("{employeeId}")]
    public async Task<ActionResult> AddApprovedPersonalDay(int orgId, int employeeId, [FromBody] BllPersonalDayCreateDto dto)
    {
        if (!await _employeeService.BelongsToOrganizationAsync(employeeId, orgId))
            return NotFound(new ErrorResponse("NOT_FOUND", "Employee not found."));

        var id = await _service.AddApprovedPersonalDay(employeeId, dto);
        return Ok(new { id });
    }
}


[ApiController]
[Route("api/personal-days")]
[Authorize(Roles = "Employee")]
public class EmployeePersonalDaysController : ControllerBase
{
    private readonly IPersonalDayService _service;
    private readonly IEmployeeService _employeeService;
    private readonly IOrganizationService _organizationService;
    private readonly INotificationService _notificationService;

    public EmployeePersonalDaysController(
        IPersonalDayService service,
        IEmployeeService employeeService,
        IOrganizationService organizationService,
        INotificationService notificationService)
    {
        _service = service;
        _employeeService = employeeService;
        _organizationService = organizationService;
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult<List<BllPersonalDayDto>>> GetMyPersonalDays()
    {
        var employeeId = User.GetUserId();
        return Ok(await _service.GetPersonalDays(employeeId));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMyPersonalDay(int id)
    {
        var employeeId = User.GetUserId();
        var success = await _service.DeletePersonalDay(employeeId, id);
        if (!success)
            return NotFound(new ErrorResponse("NOT_FOUND", "Personal day not found."));

        return Ok();
    }

    [HttpGet("requests")]
    public async Task<ActionResult<List<PersonalDayRequestDto>>> GetMyRequests()
    {
        var employeeId = User.GetUserId();
        return Ok(await _service.GetPersonalDayRequests(employeeId));
    }

    [HttpPost("requests")]
    public async Task<ActionResult> AddMyRequest([FromBody] PersonalDayRequestCreateDto dto)
    {
        var employeeId = User.GetUserId();
        var result = await _service.AddPersonalDayRequest(employeeId, dto);
        if (!result.Success)
            return BadRequest(new ErrorResponse("BAD_REQUEST", result.Message));

        var orgId = await _employeeService.GetOrgIdByEmployeeIdAsync(employeeId);
        if (orgId != null)
        {
            var employerId = await _organizationService.GetEmployerIdByOrganizationIdAsync(orgId.Value);
            var employee = await _employeeService.GetByIdAsync(employeeId);
            if (employee != null)
            {
                await _notificationService.CreateAsync(
                    employerId,
                    UserRole.Employer,
                    NotificationTypes.PersonalDayRequest,
                    $"{employee.FirstName} {employee.LastName} requested personal day {dto.StartDate} - {dto.EndDate}",
                    "Personal Day Request",
                    new { employeeId, startDate = dto.StartDate.ToString(), endDate = dto.EndDate.ToString() }
                );
            }
        }

        return Ok(new { id = result.Data });
    }

    [HttpDelete("requests/{id}")]
    public async Task<ActionResult> DeleteMyRequest(int id)
    {
        var employeeId = User.GetUserId();
        var success = await _service.DeletePersonalDayRequest(employeeId, id);
        if (!success)
            return NotFound(new ErrorResponse("NOT_FOUND", "Personal day request not found."));

        return Ok();
    }
}
