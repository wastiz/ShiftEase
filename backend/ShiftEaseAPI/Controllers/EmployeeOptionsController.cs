using BLL.ServiceInterfaces;
using DAL.Repositories;
using Domain;
using DTOs.EmployeeOptionsDtos;
using Microsoft.AspNetCore.Mvc;
using ShiftEaseAPI.Middlewares;

namespace ShiftEaseAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowEmployeeAccess]
    public class EmployeeOptionsController : ControllerBase
    {
        private readonly IEmployeeOptionsService _service;

        public EmployeeOptionsController(IEmployeeOptionsService service)
        {
            _service = service;
        }

        private int GetEmployeeId()
        {
            var id = User?.FindFirst("UserId")?.Value;
            if (id == null) throw new UnauthorizedAccessException();
            return int.Parse(id);
        }

        [HttpGet("get-vacations")]
        public async Task<ActionResult<List<VacationRequestDto>>> GetVacations()
        {
            return Ok(await _service.GetVacationRequests(GetEmployeeId()));
        }

        [HttpPost("add-vacation-request")]
        public async Task<ActionResult> AddVacation([FromBody] VacationRequestDto dto)
        {
            var id = await _service.AddVacationRequest(GetEmployeeId(), dto);
            return Ok(new { id });
        }

        [HttpDelete("delete-vacation/{id}")]
        public async Task<ActionResult> DeleteVacation(int id)
        {
            var success = await _service.DeleteVacationRequest(GetEmployeeId(), id);
            if (!success) return NotFound();
            return Ok();
        }

        [HttpGet("get-sick-leaves")]
        public async Task<ActionResult<List<SickLeaveDto>>> GetSickLeaves()
        {
            return Ok(await _service.GetSickLeaves(GetEmployeeId()));
        }

        [HttpPost("add-sick-leave")]
        public async Task<ActionResult> AddSickLeave([FromBody] SickLeaveDto dto)
        {
            var id = await _service.AddSickLeave(GetEmployeeId(), dto);
            return Ok(new { id });
        }

        [HttpDelete("sick-leave/{id}")]
        public async Task<ActionResult> DeleteSickLeave(int id)
        {
            var success = await _service.DeleteSickLeave(GetEmployeeId(), id);
            if (!success) return NotFound();
            return Ok();
        }
        
        [HttpGet("preferences")]
        public async Task<ActionResult> GetPreferences()
        {
            var result = await _service.GetPreferences(GetEmployeeId());
            return Ok(result);
        }

        [HttpPost("preferences")]
        public async Task<ActionResult> SavePreferences([FromBody] BllPreferenceBundle dto)
        {
            await _service.SavePreferences(GetEmployeeId(), dto);
            return Ok();
        }

        [HttpGet("week-days")]
        public async Task<ActionResult<List<WeekDayPreferenceDto>>> GetWeekDays()
        {
            return Ok(await _service.GetPreferredWeekDays(GetEmployeeId()));
        }
    }

}
