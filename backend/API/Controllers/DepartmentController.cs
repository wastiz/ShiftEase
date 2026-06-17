using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BLL.Contracts;
using DTOs.DepartmentDtos;

namespace API.Controllers
{
    [Route("api/organizations/{orgId}/departments")]
    [ApiController]
    [Authorize(Roles = "Employer")]
    [ServiceFilter(typeof(OrganizationOwnerFilter))]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentService _service;

        public DepartmentController(IDepartmentService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<BllDepartment>>> GetDepartments(int orgId)
        {
            var departments = await _service.GetAllByOrganizationIdAsync(orgId);
            return Ok(departments);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<BllDepartment>> GetDepartmentById(int orgId, int id)
        {
            if (!await _service.BelongsToOrganizationAsync(id, orgId))
                return NotFound(new ErrorResponse("NOT_FOUND", "Department not found."));

            var department = await _service.GetByIdAsync(id);
            if (department == null)
                return NotFound(new ErrorResponse("NOT_FOUND", "Department not found."));

            return Ok(department);
        }

        [HttpPost]
        public async Task<ActionResult<BllDepartment>> CreateDepartment(int orgId, [FromBody] BllDepartmentCreate createDto)
        {
            var created = await _service.CreateAsync(createDto, orgId);
            return CreatedAtAction(nameof(GetDepartmentById), new { orgId, id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateDepartment(int orgId, int id, [FromBody] BllDepartment updateDto)
        {
            if (updateDto == null || string.IsNullOrWhiteSpace(updateDto.Name))
                return BadRequest(new ErrorResponse("VALIDATION_ERROR", "Invalid department data."));

            if (!await _service.BelongsToOrganizationAsync(id, orgId))
                return NotFound(new ErrorResponse("NOT_FOUND", $"Department with id {id} not found."));

            updateDto.Id = id;
            var updated = await _service.UpdateAsync(updateDto);
            if (updated == null)
                return NotFound(new ErrorResponse("NOT_FOUND", $"Department with id {id} not found."));

            return Ok(updated);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteDepartment(int orgId, int id)
        {
            if (!await _service.BelongsToOrganizationAsync(id, orgId))
                return NotFound(new ErrorResponse("NOT_FOUND", "Department not found."));

            var result = await _service.DeleteAsync(id);
            if (!result)
                return NotFound(new ErrorResponse("NOT_FOUND", "Department not found."));

            return Ok();
        }
    }
}
