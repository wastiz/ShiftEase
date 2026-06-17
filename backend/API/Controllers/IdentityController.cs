using BLL.DTO.IdentityDtos;
using BLL.Helpers;
using BLL.Contracts;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Route("api/auth")]
public class IdentityController : ControllerBase
{
    private readonly IIdentityService _identityService;

    public IdentityController(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    [HttpPost("employer/register")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-register")]
    public async Task<IActionResult> EmployerRegister([FromBody] BllRegister model)
    {
        var result = await _identityService.RegisterEmployerAsync(model);

        if (!result.Success)
            return BadRequest(new ErrorResponse("REGISTRATION_FAILED", result.Message));

        return Ok(new { message = result.Message });
    }

    [HttpPost("employer/login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-login")]
    public async Task<IActionResult> EmployerLogin([FromBody] BllLogin model)
    {
        var result = await _identityService.LoginEmployerAsync(model);

        if (!result.Success)
            return Unauthorized(new ErrorResponse("INVALID_CREDENTIALS", result.Message));

        Response.Cookies.Append("accessToken", result.Data.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddMinutes(15)
        });

        Response.Cookies.Append("refreshToken", result.Data.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddDays(7)
        });

        return Ok(new { message = result.Message });
    }

    [HttpPost("employee/login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-login")]
    public async Task<IActionResult> EmployeeLogin([FromBody] BllLogin model)
    {
        var result = await _identityService.LoginEmployeeAsync(model);

        if (!result.Success)
            return Unauthorized(new ErrorResponse("INVALID_CREDENTIALS", result.Message));

        Response.Cookies.Append("accessToken", result.Data.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddMinutes(15)
        });

        Response.Cookies.Append("refreshToken", result.Data.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddDays(7)
        });

        return Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies["refreshToken"];

        if (!string.IsNullOrEmpty(refreshToken))
        {
            await _identityService.LogoutAsync(refreshToken);
        }

        Response.Cookies.Delete("accessToken");
        Response.Cookies.Delete("refreshToken");

        return Ok(new { message = "Logged out successfully" });
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = User.GetUserId();
        var userRole = User.GetRole();

        Console.WriteLine($"User ID: {userId}, Role: {userRole}");

        var result = await _identityService.GetUserMeDataAsync(userId, userRole);

        if (!result.Success) 
            return BadRequest(new ErrorResponse("BAD_REQUEST", result.Message));

        return Ok(result.Data);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "Refresh token is missing."));

        var result = await _identityService.RefreshAsync(refreshToken);

        if (!result.Success)
            return Unauthorized(new ErrorResponse("TOKEN_REFRESH_FAILED", result.Message));

        Response.Cookies.Append("accessToken", result.Data.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddMinutes(15)
        });

        Response.Cookies.Append("refreshToken", result.Data.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddDays(7)
        });

        return Ok(new { message = "Tokens refreshed successfully" });
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] BllForgotPassword model)
    {
        var result = await _identityService.ForgotPasswordAsync(model);

        if (!result.Success)
            return BadRequest(new ErrorResponse("BAD_REQUEST", result.Message));

        return Ok(new { message = result.Message });
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] BllResetPassword model)
    {
        var result = await _identityService.ResetPasswordAsync(model);

        if (!result.Success)
            return BadRequest(new ErrorResponse("PASSWORD_RESET_FAILED", result.Message));

        return Ok(new { message = result.Message });
    }

    [HttpDelete("account")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> DeleteAccount([FromBody] BllDeleteConfirmation dto)
    {
        var employerId = User.GetUserId();

        var result = await _identityService.DeleteAccountAsync(employerId, dto.Password);

        if (!result.Success)
            return BadRequest(new ErrorResponse("ACCOUNT_DELETE_FAILED", result.Message));

        Response.Cookies.Delete("accessToken");
        Response.Cookies.Delete("refreshToken");

        return Ok(new { message = result.Message });
    }
}
