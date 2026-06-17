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
    [Route("api/organizations/{orgId}/sick-leaves")]
    [Authorize(Roles = "Employer")]
    [ServiceFilter(typeof(OrganizationOwnerFilter))]
    public class EmployerSickLeavesController : ControllerBase
    {
        private readonly ISickLeaveService _service;
        private readonly IEmployeeService _employeeService;
        private readonly INotificationService _notificationService;

        public EmployerSickLeavesController(
            ISickLeaveService service,
            IEmployeeService employeeService,
            INotificationService notificationService)
        {
            _service = service;
            _employeeService = employeeService;
            _notificationService = notificationService;
        }

        [HttpGet("{employeeId}")]
        public async Task<ActionResult<List<BllBllSickLeaveDto>>> GetSickLeaves(int orgId, int employeeId)
        {
            if (!await _employeeService.BelongsToOrganizationAsync(employeeId, orgId))
                return NotFound(new ErrorResponse("NOT_FOUND", "Employee not found."));

            return Ok(await _service.GetSickLeaves(employeeId));
        }

        [HttpDelete("{employeeId}/{id}")]
        public async Task<ActionResult> DeleteSickLeave(int orgId, int employeeId, int id)
        {
            if (!await _employeeService.BelongsToOrganizationAsync(employeeId, orgId))
                return NotFound(new ErrorResponse("NOT_FOUND", "Employee not found."));

            var success = await _service.DeleteSickLeave(employeeId, id);
            if (!success)
                return NotFound(new ErrorResponse("NOT_FOUND", "Sick leave not found."));

            return Ok();
        }

        [HttpGet("requests/{employeeId}")]
        public async Task<ActionResult<List<SickLeaveRequestDto>>> GetSickLeaveRequests(int orgId, int employeeId)
        {
            if (!await _employeeService.BelongsToOrganizationAsync(employeeId, orgId))
                return NotFound(new ErrorResponse("NOT_FOUND", "Employee not found."));

            return Ok(await _service.GetSickLeaveRequests(employeeId));
        }

        [HttpGet("pending-requests")]
        public async Task<ActionResult<List<SickLeaveRequestDto>>> GetPendingRequests(int orgId)
        {
            return Ok(await _service.GetPendingSickLeaveRequestsByOrganization(orgId));
        }

        [HttpPost("requests/{requestId}/approve")]
        public async Task<ActionResult> ApproveSickLeaveRequest(int orgId, int requestId)
        {
            var pending = await _service.GetPendingSickLeaveRequestsByOrganization(orgId);
            if (!pending.Any(r => r.Id == requestId))
                return NotFound(new ErrorResponse("NOT_FOUND", "Request not found in this organization."));

            var employeeId = await _service.ApproveSickLeaveRequest(requestId);
            if (employeeId == null)
                return NotFound(new ErrorResponse("NOT_FOUND", "Request not found."));

            await _notificationService.CreateAsync(
                employeeId.Value,
                UserRole.Employee,
                NotificationTypes.SickLeaveApproved,
                "Your sick leave request has been approved",
                "Sick Leave Approved"
            );
            return Ok();
        }

        [HttpPost("requests/{requestId}/reject")]
        public async Task<ActionResult> RejectSickLeaveRequest(int orgId, int requestId)
        {
            var pending = await _service.GetPendingSickLeaveRequestsByOrganization(orgId);
            if (!pending.Any(r => r.Id == requestId))
                return NotFound(new ErrorResponse("NOT_FOUND", "Request not found in this organization."));

            var employeeId = await _service.RejectSickLeaveRequest(requestId);
            if (employeeId == null)
                return NotFound(new ErrorResponse("NOT_FOUND", "Request not found."));

            await _notificationService.CreateAsync(
                employeeId.Value,
                UserRole.Employee,
                NotificationTypes.SickLeaveRejected,
                "Your sick leave request has been rejected",
                "Sick Leave Rejected"
            );
            return Ok();
        }

        [HttpPost("{employeeId}")]
        public async Task<ActionResult> AddApprovedSickLeave(int orgId, int employeeId, [FromBody] BllSickLeaveCreateDto dto)
        {
            if (!await _employeeService.BelongsToOrganizationAsync(employeeId, orgId))
                return NotFound(new ErrorResponse("NOT_FOUND", "Employee not found."));

            var id = await _service.AddApprovedSickLeave(employeeId, dto);
            return Ok(new { id });
        }
    }


    [ApiController]
    [Route("api/sick-leaves")]
    [Authorize(Roles = "Employee")]
    public class EmployeeSickLeavesController : ControllerBase
    {
        private readonly ISickLeaveService _service;
        private readonly IEmployeeService _employeeService;
        private readonly IOrganizationService _organizationService;
        private readonly INotificationService _notificationService;

        public EmployeeSickLeavesController(
            ISickLeaveService service,
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
        public async Task<ActionResult<List<BllBllSickLeaveDto>>> GetMySickLeaves()
        {
            var employeeId = User.GetUserId();
            return Ok(await _service.GetSickLeaves(employeeId));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMySickLeave(int id)
        {
            var employeeId = User.GetUserId();
            var success = await _service.DeleteSickLeave(employeeId, id);
            if (!success)
                return NotFound(new ErrorResponse("NOT_FOUND", "Sick leave not found."));

            return Ok();
        }

        [HttpGet("requests")]
        public async Task<ActionResult<List<SickLeaveRequestDto>>> GetMyRequests()
        {
            var employeeId = User.GetUserId();
            return Ok(await _service.GetSickLeaveRequests(employeeId));
        }

        [HttpPost("requests")]
        public async Task<ActionResult> AddMyRequest([FromBody] SickLeaveRequestCreateDto dto)
        {
            var employeeId = User.GetUserId();
            var result = await _service.AddSickLeaveRequest(employeeId, dto);
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
                        NotificationTypes.SickLeaveRequest,
                        $"{employee.FirstName} {employee.LastName} requested sick leave {dto.StartDate} - {dto.EndDate}",
                        "Sick Leave Request",
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
            var success = await _service.DeleteSickLeaveRequest(employeeId, id);
            if (!success)
                return NotFound(new ErrorResponse("NOT_FOUND", "Sick leave request not found."));

            return Ok();
        }
    }
}
