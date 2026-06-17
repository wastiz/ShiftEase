using System.Security.Claims;
using BLL.DTO.IdentityDtos;
using Domain.Enums;
using DTOs.EmployeeDtos;

namespace BLL.Contracts;

public interface IIdentityService
{
    Task<BllResult<bool>> RegisterEmployerAsync(BllRegister dto);
    Task<BllResult<BllAuthResult>> LoginEmployerAsync(BllLogin dto);
    Task<BllResult<BllEmployeeAuthResult>> LoginEmployeeAsync(BllLogin dto);
    Task<BllResult<BllUser>> GetUserMeDataAsync(int userId, UserRole userRole);
    Task<BllResult<BllAuthResult>> RefreshAsync(string refreshToken);
    Task<bool> LogoutAsync(string refreshToken);
    Task<BllResult<bool>> ForgotPasswordAsync(BllForgotPassword dto);
    Task<BllResult<bool>> ResetPasswordAsync(BllResetPassword dto);
    Task<BllResult<bool>> DeleteAccountAsync(int employerId, string password);
}
