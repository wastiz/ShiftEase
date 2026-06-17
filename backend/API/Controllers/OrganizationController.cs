using BLL.DTO.IdentityDtos;
using BLL.Helpers;
using BLL.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Domain;
using DTOs.OrganizationDtos;

namespace API.Controllers
{
    [ApiController]
    [Route("api/organizations")]
    [Authorize(Roles = "Employer")]
    public class OrganizationController : ControllerBase
    {
        private readonly IOrganizationService _service;
        private readonly ILogger<OrganizationController> _logger;

        public OrganizationController(IOrganizationService organizationService, ILogger<OrganizationController> logger)
        {
            _service = organizationService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Organization>>> GetOrganizationsByEmployerId()
        {
            var employerId = User.GetUserId();
            var organizations = await _service.GetAllByEmployerIdAsync(employerId);
            if (organizations == null || !organizations.Any())
                return Ok(new { message = "No organizations found for this employer." });
            return Ok(organizations);
        }

        [HttpGet("{orgId}")]
        [ServiceFilter(typeof(OrganizationOwnerFilter))]
        public async Task<IActionResult> GetOrganizationById(int orgId)
        {
            var organization = await _service.GetOrganizationByIdAsync(orgId);
            if (organization == null)
                return NotFound(new ErrorResponse("NOT_FOUND", $"Organization with id {orgId} not found."));
            return Ok(organization);
        }

        [HttpGet("data/{orgId}")]
        [ServiceFilter(typeof(OrganizationOwnerFilter))]
        public async Task<ActionResult> GetOrganizationData(int orgId)
        {
            var organizationData = await _service.GetOrganizationDataByIdAsync(orgId);
            if (organizationData == null)
                return NotFound(new ErrorResponse("NOT_FOUND", $"Organization with id {orgId} not found."));
            return Ok(organizationData);
        }

        [HttpGet("{orgId}/check-entities")]
        [ServiceFilter(typeof(OrganizationOwnerFilter))]
        public async Task<IActionResult> GetCheckEntities(int orgId)
        {
            var result = await _service.CheckOrganizationEntities(orgId);
            return Ok(result);
        }

        [HttpGet("{orgId}/holidays")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<BllHoliday>>> GetHolidaysByOrganizationId(int orgId)
        {
            var holidays = await _service.GetOrganizationHolidaysByIdAsync(orgId);
            return Ok(holidays);
        }

        [HttpGet("{orgId}/work-schedule")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<object>>> GetWorkScheduleByOrganizationId(int orgId)
        {
            var workSchedule = await _service.GetOrganizationWorkDaysByIdAsync(orgId);
            return Ok(workSchedule);
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] BllOrganizationCreate createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse("VALIDATION_ERROR", "Invalid data for creating organization."));

            var employerId = User.GetUserId();
            createDto.EmployerId = employerId;

            var result = await _service.CreateAsync(createDto);
            return Ok(result);
        }

        [HttpPut("{orgId}")]
        [ServiceFilter(typeof(OrganizationOwnerFilter))]
        public async Task<IActionResult> Update(int orgId, [FromBody] BllOrganizationUpdate updateDto)
        {
            if (!ModelState.IsValid)
            {
                var fieldErrors = ModelState
                    .Where(x => x.Value!.Errors.Any())
                    .ToDictionary(
                        x => x.Key,
                        x => x.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                return BadRequest(new ErrorResponse("VALIDATION_ERROR", "Invalid input.", fieldErrors));
            }

            var employerId = User.GetUserId();
            updateDto.Id = orgId;
            var result = await _service.UpdateAsync(updateDto, employerId);
            return Ok(result);
        }

        [HttpDelete("{orgId}")]
        [ServiceFilter(typeof(OrganizationOwnerFilter))]
        public async Task<IActionResult> Delete(int orgId, [FromBody] BllDeleteConfirmation dto)
        {
            var employerId = User.GetUserId();
            var result = await _service.DeleteWithPasswordAsync(orgId, employerId, dto.Password);
            if (!result.Success)
                return BadRequest(new ErrorResponse("ORG_DELETE_FAILED", result.Message));
            return Ok(new { message = result.Message });
        }
    }
}
