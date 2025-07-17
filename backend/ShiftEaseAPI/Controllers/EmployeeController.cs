using BLL.Interfaces;
using DTOs.EmployeeDtos;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeeController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet("{employeeId}")]
    public async Task<IActionResult> GetEmployee(int id)
    {
        var employee = await _employeeService.GetByIdAsync(id);
        if (employee == null) return NotFound("Employee not found.");
        return Ok(employee);
    }

    [HttpGet("by-organization-id/{orgId}")]
    public async Task<IActionResult> GetEmployeesByOrganizationId(int orgId)
    {
        var employees = await _employeeService.GetFullDataByOrganizationIdAsync(orgId);
        return Ok(employees);
    }

    [HttpPost]
    public async Task<IActionResult> CreateEmployee([FromBody] BllEmployeeCreate dto)
    {
        int orgId = int.Parse(Request.Headers["X-Organization-Id"]);
        var result = await _employeeService.CreateAsync(dto, orgId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEmployee(int id, [FromBody] BllEmployeeUpdate dto)
    {
        var result = await _employeeService.UpdateAsync(id, dto);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        var result = await _employeeService.DeleteAsync(id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}