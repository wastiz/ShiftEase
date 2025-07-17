using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using BLL.ServiceInterfaces;
using BLL.Services;
using Microsoft.AspNetCore.Mvc;
using Domain;
using DAL;
using DTOs.OrganizationDtos;
using ShiftEaseAPI.Helpers;
using ShiftEaseAPI.Middlewares;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationController : ControllerBase
    {
        private readonly IOrganizationService _service;
        private readonly ILogger<OrganizationController> _logger;

        public OrganizationController(IOrganizationService organizationService, ILogger<OrganizationController> logger)
        {
            _service = organizationService;
            _logger = logger;
        }
        
        //Check Organization entities (Groups, Employees, ShiftTypes, Scchedules)
        [HttpGet("check-entities")]
        public async Task<IActionResult> GetCheckEntities()
        {
            int orgId = int.Parse(Request.Headers["X-Organization-Id"]);
            var result = await _service.CheckOrganizationEntities(orgId);
            return Ok(result);
        }
        
        //Get Organization Holidays by org Id
        [HttpGet("holidays")]
        [AllowEmployeeAccess]
        public async Task<ActionResult<IEnumerable<BllHoliday>>> GetHolidaysByOrganizationId()
        {
            int orgId = int.Parse(Request.Headers["X-Organization-Id"]);
            var holidays = await _service.GetOrganizationHolidaysByIdAsync(orgId);
            return Ok(holidays);
        }
        
        //Get Organization Work Days by org Id
        [HttpGet("work-schedule")]
        [AllowEmployeeAccess]
        public async Task<ActionResult<IEnumerable<object>>> GetWorkScheduleByOrganizationId()
        {
            int orgId = int.Parse(Request.Headers["X-Organization-Id"]);
            var workSchedule = await _service.GetOrganizationWorkDaysByIdAsync(orgId);
            return Ok(workSchedule);
        }
        
        //Get All Employer's Organizations by Employer ID
        [HttpGet("by-employer-id")]
        public async Task<ActionResult<IEnumerable<Organization>>> GetOrganizationsByEmployerId()
        {
            try
            {
                int employerId = JwtHelper.GetUserIdFromRequest(Request);
                if (employerId == 0) return Unauthorized("Invalid or missing token.");

                var organizations = await _service.GetAllByEmployerIdAsync(employerId);

                if (organizations == null || !organizations.Any())
                {
                    return Ok(new { message = "No organizations found for this employer." });
                }

                return Ok(organizations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching organizations for employer.");
                return StatusCode(500, "An error occurred while fetching organizations.");
            }
        }
        
        //Get organization by its id
        [HttpGet("{orgId}")]
        public async Task<IActionResult> GetOrganizationById(int orgId)
        {
            var organization = await _service.GetOrganizationFullInfoByIdAsync(orgId);
    
            if (organization == null)
            {
                return NotFound($"Organization with id {orgId} not found.");
            }

            return Ok(organization);
        }
        
        //Post: Create Organization
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] BllOrganizationCreate createDto)
        {
            if (!ModelState.IsValid) return BadRequest("Not valid data for creating organization");

            try
            {
                int employerId = JwtHelper.GetUserIdFromRequest(Request);
                createDto.EmployerId = employerId;
                var success = await _service.CreateAsync(createDto);

                return success ? Ok(new { message = "Organization created successfully" }) : StatusCode(500, new { message = "Failed to create organization" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating organization");
                return StatusCode(500, new { error = "An internal server error occurred", details = ex.Message });
            }
        }

        
        //Update Organization by its id
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] BllOrganizationUpdate updateDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            try
            {
                int employerId = JwtHelper.GetUserIdFromRequest(Request);

                var result = await _service.UpdateAsync(updateDto, employerId);
                
                if (!result) return NotFound(new {message = "Failed to update Organization" });

                return Ok(new {message = "Organization updated successfully"});
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating organization with ID {OrganizationId}", id);
                return StatusCode(500, "An error occurred while updating the organization.");
            }
        }

        
        //Delete Organization by its ID
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            bool result = await _service.DeleteAsync(id);
            if (result)
            {
                return Ok();
            }
            return NotFound(new { message = "Organization not found." });
        }

    }
}