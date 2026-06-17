using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace BLL.Helpers;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var claimValue = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claimValue, out var id) ? id : 0;
    }

    public static UserRole GetRole(this ClaimsPrincipal user)
    {
        var claimValue = user.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrWhiteSpace(claimValue))
            return UserRole.Unknown;
        
        if (Enum.TryParse<UserRole>(claimValue, ignoreCase: true, out var role))
            return role;

        return UserRole.Unknown;
    }

    public static string GetName(this ClaimsPrincipal user)
    {
        var claimValue = user.FindFirstValue(ClaimTypes.Name);
        return claimValue is not null ? claimValue : string.Empty;
    }
}