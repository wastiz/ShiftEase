using BLL.Helpers;
using BLL.Contracts;
using Domain.Enums;
using DTOs.EmployeeDtos;
using DTOs.EmployeeOptionsDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/organizations/{orgId}/vacations")]
    [Authorize(Roles = "Employer")]
    [ServiceFilter(typeof(OrganizationOwnerFilter))]
    public class EmployerVacationsController : ControllerBase
    {
        private readonly IVacationService _service;
        private readonly IEmployeeService _employeeService;
        private readonly INotificationService _notificationService;

        public EmployerVacationsController(
            IVacationService service,
            IEmployeeService employeeService,
            INotificationService notificationService)
        {
            _service = service;
            _employeeService = employeeService;
            _notificationService = notificationService;
        }

        [HttpGet("{employeeId}")]
        public async Task<ActionResult<List<BllVacationDto>>> GetVacations(int orgId, int employeeId)
        {
            if (!await _employeeService.BelongsToOrganizationAsync(employeeId, orgId))
                return NotFound(new ErrorResponse("NOT_FOUND", "Employee not found."));

            return Ok(await _service.GetVacations(employeeId));
        }

        [HttpDelete("{employeeId}/{id}")]
        public async Task<ActionResult> DeleteVacation(int orgId, int employeeId, int id)
        {
            if (!await _employeeService.BelongsToOrganizationAsync(employeeId, orgId))
                return NotFound(new ErrorResponse("NOT_FOUND", "Employee not found."));

            var success = await _service.DeleteVacation(employeeId, id);
            if (!success)
                return NotFound(new ErrorResponse("NOT_FOUND", "Vacation not found."));

            return Ok();
        }

        [HttpGet("requests/{employeeId}")]
        public async Task<ActionResult<List<VacationRequestDto>>> GetVacationRequests(int orgId, int employeeId)
        {
            if (!await _employeeService.BelongsToOrganizationAsync(employeeId, orgId))
                return NotFound(new ErrorResponse("NOT_FOUND", "Employee not found."));

            return Ok(await _service.GetVacationRequests(employeeId));
        }

        [HttpGet("pending-requests")]
        public async Task<ActionResult<List<VacationRequestDto>>> GetPendingRequests(int orgId)
        {
            return Ok(await _service.GetPendingVacationRequestsByOrganization(orgId));
        }

        [HttpPost("requests/{requestId}/approve")]
        public async Task<ActionResult> ApproveVacationRequest(int orgId, int requestId)
        {
            var pending = await _service.GetPendingVacationRequestsByOrganization(orgId);
            if (!pending.Any(r => r.Id == requestId))
                return NotFound(new ErrorResponse("NOT_FOUND", "Request not found in this organization."));

            var employeeId = await _service.ApproveVacationRequest(requestId);
            if (employeeId == null)
                return NotFound(new ErrorResponse("NOT_FOUND", "Request not found."));

            await _notificationService.CreateAsync(
                employeeId.Value,
                UserRole.Employee,
                NotificationTypes.VacationApproved,
                "Your vacation request has been approved",
                "Vacation Approved"
            );
            return Ok();
        }

        [HttpPost("requests/{requestId}/reject")]
        public async Task<ActionResult> RejectVacationRequest(int orgId, int requestId)
        {
            var pending = await _service.GetPendingVacationRequestsByOrganization(orgId);
            if (!pending.Any(r => r.Id == requestId))
                return NotFound(new ErrorResponse("NOT_FOUND", "Request not found in this organization."));

            var employeeId = await _service.RejectVacationRequest(requestId);
            if (employeeId == null)
                return NotFound(new ErrorResponse("NOT_FOUND", "Request not found."));

            await _notificationService.CreateAsync(
                employeeId.Value,
                UserRole.Employee,
                NotificationTypes.VacationRejected,
                "Your vacation request has been rejected",
                "Vacation Rejected"
            );
            return Ok();
        }

        [HttpPost("{employeeId}")]
        public async Task<ActionResult> AddApprovedVacation(int orgId, int employeeId, [FromBody] BllVacationCreateDto dto)
        {
            if (!await _employeeService.BelongsToOrganizationAsync(employeeId, orgId))
                return NotFound(new ErrorResponse("NOT_FOUND", "Employee not found."));

            var id = await _service.AddApprovedVacation(employeeId, dto);
            return Ok(new { id });
        }
    }


    [ApiController]
    [Route("api/vacations")]
    [Authorize(Roles = "Employee")]
    public class EmployeeVacationsController : ControllerBase
    {
        private readonly IVacationService _service;
        private readonly IEmployeeService _employeeService;
        private readonly IOrganizationService _organizationService;
        private readonly INotificationService _notificationService;

        public EmployeeVacationsController(
            IVacationService service,
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
        public async Task<ActionResult<List<BllVacationDto>>> GetMyVacations()
        {
            var employeeId = User.GetUserId();
            return Ok(await _service.GetVacations(employeeId));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMyVacation(int id)
        {
            var employeeId = User.GetUserId();
            var success = await _service.DeleteVacation(employeeId, id);
            if (!success)
                return NotFound(new ErrorResponse("NOT_FOUND", "Vacation not found."));

            return Ok();
        }

        [HttpGet("requests")]
        public async Task<ActionResult<List<VacationRequestDto>>> GetMyRequests()
        {
            var employeeId = User.GetUserId();
            return Ok(await _service.GetVacationRequests(employeeId));
        }

        [HttpPost("requests")]
        public async Task<ActionResult> AddMyRequest([FromBody] VacationRequestCreateDto dto)
        {
            var employeeId = User.GetUserId();
            var result = await _service.AddVacationRequest(employeeId, dto);
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
                        NotificationTypes.VacationRequest,
                        $"{employee.FirstName} {employee.LastName} requested vacation {dto.StartDate} - {dto.EndDate}",
                        "Vacation Request",
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
            var success = await _service.DeleteVacationRequest(employeeId, id);
            if (!success)
                return NotFound(new ErrorResponse("NOT_FOUND", "Vacation request not found."));

            return Ok();
        }
    }
}
