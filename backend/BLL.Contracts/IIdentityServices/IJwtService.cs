using Domain.Enums;

namespace BLL.Contracts;

using System.Security.Claims;

public interface IJwtService
{
    string GenerateAccessToken(string userId, UserRole role, string name);
    Task<string> GenerateRefreshToken(string userId, UserRole role);
    ClaimsPrincipal? ValidateToken(string token, bool validateLifetime = true);
    string? GetUserIdFromClaims(ClaimsPrincipal principal);
    string? GetUserRoleFromClaims(ClaimsPrincipal principal);
}
