using DAL;
using Microsoft.AspNetCore.Mvc;
using Domain;
using DAL.Repositories;
using DTOs.GroupDtos;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupController : ControllerBase
    {
        private readonly IGroupService _service;

        public GroupController(IGroupService service)
        {
            _service = service;
        }
        
        //Get Group by its id
        [HttpGet("{id:int}")]
        public async Task<ActionResult<BllGroup>> GetGroupById(int id)
        {
            var group = await _service.GetByIdAsync(id);
            if (group == null)
            {
                return BadRequest("Group not found.");
            }

            return Ok(group);
        }
        
        //Get all organization groups by organization id
        [HttpGet("by-organization-id")]
        public async Task<ActionResult<Group>> GetGroupByOrgId()
        {
            int orgId = int.Parse(Request.Headers["X-Organization-Id"]);
            
            var groups = await _service.GetAllByOrganizationIdAsync(orgId);

            if (groups == null)
            {
                return NotFound("Group not found.");
            }

            return Ok(groups);
        }

        
        //Post Group to specific organization
        [HttpPost]
        public async Task<ActionResult<Group>> CreateGroup([FromBody] BllGroupCreate createDto)
        {
            int orgId = int.Parse(Request.Headers["X-Organization-Id"]);
            var createdGroup = await _service.CreateAsync(createDto, orgId);
            return CreatedAtAction(nameof(GetGroupById), new { id = createdGroup.Id }, createdGroup);
        }
        
        //Update Group by its id
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateGroup(int id, [FromBody] BllGroup updateDto)
        {
            if (updateDto == null || string.IsNullOrWhiteSpace(updateDto.Name))
            {
                return BadRequest("Invalid group data.");
            }

            var updatedGroup = await _service.UpdateAsync(updateDto);
            return Ok(updatedGroup);
        }
        
        //Delete Group by its id
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteGroup(int id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result)
            {
                return NotFound("Group not found.");
            }

            return Ok();
        }
    }
}
