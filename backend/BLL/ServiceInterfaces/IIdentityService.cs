using BLL.DTO.IdentityDtos;
using DTOs.IdentityDtos;

namespace BLL.Interfaces;

public interface IIdentityService
{
    Task<BllEmployerAuthResult> RegisterEmployerAsync(BllEmployerRegister dto);
    Task<BllEmployerAuthResult> LoginEmployerAsync(BllLoginDto dto);
    Task<BllEmployeeAuthResult> LoginEmployeeAsync(BllLoginDto dto);
    Task<BllEmployerAuthResult> RefreshAsync(string refreshToken);
    Task LogoutAsync(string refreshToken);
}
