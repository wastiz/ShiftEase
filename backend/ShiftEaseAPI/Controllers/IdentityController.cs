using BLL.DTO.IdentityDtos;
using BLL.Interfaces;
using DTOs.IdentityDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ShiftEaseAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IdentityController : ControllerBase
{
    private readonly IIdentityService _identityService;

    public IdentityController(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    // 👤 Employer Registration
    [HttpPost("register/employer")]
    public async Task<IActionResult> RegisterEmployer([FromBody] BllEmployerRegister model)
    {
        if (string.IsNullOrWhiteSpace(model.FirstName) || string.IsNullOrWhiteSpace(model.LastName) ||
            string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
            return BadRequest("All fields are required.");

        var result = await _identityService.RegisterEmployerAsync(model);
        if (!result.Success) return BadRequest(result);

        return Ok(new
        {
            accessToken = result.AccessToken,
            refreshToken = result.RefreshToken,
            message = result.Message
        });
    }

    // 🔑 Employer Login
    [HttpPost("login/employer")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginEmployer([FromBody] BllLoginDto model)
    {
        try
        {
            var result = await _identityService.LoginEmployerAsync(model);
            return Ok(new
            {
                accessToken = result.AccessToken,
                refreshToken = result.RefreshToken,
                message = result.Message
            });
        }
        catch (Exception ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    // 🔑 Employee Login
    [HttpPost("login/employee")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginEmployee([FromBody] BllLoginDto model)
    {
        try
        {
            var result = await _identityService.LoginEmployeeAsync(model);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    // 🔁 Refresh token
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] string refreshToken)
    {
        var result = await _identityService.RefreshAsync(refreshToken);
        if (!result.Success)
            return Unauthorized(result.Message);

        return Ok(result);
    }

    // 🚪 Logout
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] string refreshToken)
    {
        await _identityService.LogoutAsync(refreshToken);
        return Ok("Logged out");
    }
}
