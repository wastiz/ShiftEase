using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BLL.Contracts;
using DAL.Contracts;
using Domain;
using Domain.Enums;
using DTOs.EmployeeDtos;

public class JwtService : IJwtService
{
    private readonly JwtOptions _jwtOptions;
    private readonly byte[] _key;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public JwtService(IOptions<JwtOptions> jwtOptions, IRefreshTokenRepository refreshTokenRepository)
    {
        _jwtOptions = jwtOptions.Value;
        _key = Encoding.UTF8.GetBytes(_jwtOptions.SecretKey);
        _refreshTokenRepository = refreshTokenRepository;
    }

    public string GenerateAccessToken(string userId, UserRole role, string name)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, name ?? ""),
            new Claim(ClaimTypes.Role, role.ToString())
        };

        var creds = new SigningCredentials(new SymmetricSecurityKey(_key), SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpirationMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<string> GenerateRefreshToken(string userId, UserRole role)
    {
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var hashedToken = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));

        var refreshToken = new RefreshToken
        {
            Token = hashedToken,
            Expires = DateTime.UtcNow.AddDays(7),
            UserId = userId,
            UserRole = role
        };

        await _refreshTokenRepository.SaveAsync(refreshToken);
        return rawToken;
    }

    public ClaimsPrincipal? ValidateToken(string token, bool validateLifetime = true)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            return tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(_key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = validateLifetime,
                ClockSkew = TimeSpan.Zero
            }, out _);
        }
        catch
        {
            return null;
        }
    }

    public string? GetUserIdFromClaims(ClaimsPrincipal principal)
        => principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public string? GetUserRoleFromClaims(ClaimsPrincipal principal)
        => principal.FindFirst(ClaimTypes.Role)?.Value;
}
