using Microsoft.AspNetCore.Mvc;
using BLL.Interfaces;
using DTOs;
using ShiftEaseAPI.Middlewares;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ShiftTypeController : ControllerBase
{
    private readonly IShiftTypeService _service;

    public ShiftTypeController(IShiftTypeService service)
    {
        _service = service;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var shift = await _service.GetByIdAsync(id);
        if (shift == null) return NotFound();
        return Ok(shift);
    }

    [HttpGet("organization")]
    [AllowEmployeeAccess]
    public async Task<IActionResult> GetByOrganizationId()
    {
        int orgId = int.Parse(Request.Headers["X-Organization-Id"]);
        var shifts = await _service.GetByOrganizationIdAsync(orgId);
        return Ok(shifts);
    }

    [HttpPost("organization")]
    public async Task<IActionResult> Create([FromBody] BllShiftType dto)
    {
        int orgId = int.Parse(Request.Headers["X-Organization-Id"]);
        var created = await _service.CreateAsync(orgId, dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] BllShiftType dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}