using BLL.Contracts;
using BLL.DTO.ScheduleDtos;
using BLL.Helpers;
using DTOs.EmployeeDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/organizations/{orgId}/employees")]
[Authorize(Roles = "Employer")]
[ServiceFilter(typeof(OrganizationOwnerFilter))]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeeController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetEmployees(int orgId)
    {
        var employees = await _employeeService.GetFullDataByOrganizationIdAsync(orgId);
        return Ok(employees);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetEmployee(int orgId, int id)
    {
        if (!await _employeeService.BelongsToOrganizationAsync(id, orgId))
            return StatusCode(403, new ErrorResponse("FORBIDDEN", "Employee does not belong to this organization."));

        var employee = await _employeeService.GetByIdAsync(id);
        if (employee == null)
            return NotFound(new ErrorResponse("NOT_FOUND", "Employee not found."));

        return Ok(employee);
    }

    [HttpPost]
    public async Task<IActionResult> CreateEmployee(int orgId, [FromBody] BllEmployeeCreate dto)
    {
        dto.OrganizationId = orgId;
        var result = await _employeeService.CreateAsync(dto);
        if (!result.Success)
            return BadRequest(new ErrorResponse("BAD_REQUEST", result.Message));
        return Ok(result);
    }

    [HttpPost("bulk")]
    public async Task<IActionResult> CreateEmployeesBulk(int orgId, [FromBody] List<BllEmployeeCreate> employees)
    {
        if (employees == null || employees.Count == 0)
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "Employee list cannot be empty."));

        var result = await _employeeService.CreateBulkAsync(employees, orgId);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEmployee(int orgId, int id, [FromBody] BllEmployeeUpdate dto)
    {
        if (!await _employeeService.BelongsToOrganizationAsync(id, orgId))
            return NotFound(new ErrorResponse("NOT_FOUND", "Employee not found."));

        dto.Id = id;
        var result = await _employeeService.UpdateAsync(id, dto);
        if (!result.Success)
            return BadRequest(new ErrorResponse("BAD_REQUEST", result.Message));

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEmployee(int orgId, int id)
    {
        if (!await _employeeService.BelongsToOrganizationAsync(id, orgId))
            return NotFound(new ErrorResponse("NOT_FOUND", "Employee not found."));

        var result = await _employeeService.DeleteAsync(id);
        if (!result.Success)
            return BadRequest(new ErrorResponse("BAD_REQUEST", result.Message));

        return Ok(result);
    }

    [HttpPost("time-offs")]
    public async Task<IActionResult> GetEmployeesTimeOffs(int orgId, [FromBody] BllEmployeeTimeOffRequest request)
    {
        if (request.Month.HasValue && !request.Year.HasValue)
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "Year is required when month is specified."));

        var result = await _employeeService.GetEmployeesTimeOffByFilterAsync(orgId, request.EmployeeIds, request.Year, request.Month);
        return Ok(result);
    }
}
