using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BLL.Contracts;
using DTOs;

namespace API.Controllers;

[ApiController]
[Route("api/organizations/{orgId}/shift-templates")]
[Authorize(Roles = "Employer")]
[ServiceFilter(typeof(OrganizationOwnerFilter))]
public class ShiftTemplateController : ControllerBase
{
    private readonly IShiftTemplateService _service;

    public ShiftTemplateController(IShiftTemplateService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetByOrganizationId(int orgId)
    {
        var shifts = await _service.GetByOrganizationIdAsync(orgId);
        return Ok(shifts);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int orgId, int id)
    {
        var shift = await _service.GetByIdAsync(id);
        if (shift == null || shift.OrganizationId != orgId) return NotFound();
        return Ok(shift);
    }

    [HttpGet("by-department/{departmentId}")]
    public async Task<IActionResult> GetByDepartmentId(int orgId, int departmentId)
    {
        var shifts = await _service.GetByDepartmentIdAsync(departmentId);
        return Ok(shifts);
    }

    [HttpPost]
    public async Task<IActionResult> Create(int orgId, [FromBody] BllShiftTemplateCreate dto)
    {
        dto = dto with { OrganizationId = orgId };
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { orgId, id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int orgId, int id, [FromBody] BllShiftTemplateUpdate dto)
    {
        var existing = await _service.GetByIdAsync(id);
        if (existing == null || existing.OrganizationId != orgId) return NotFound();
        dto = dto with { Id = id };
        var updated = await _service.UpdateAsync(dto);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int orgId, int id)
    {
        var existing = await _service.GetByIdAsync(id);
        if (existing == null || existing.OrganizationId != orgId) return NotFound();
        var deleted = await _service.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}