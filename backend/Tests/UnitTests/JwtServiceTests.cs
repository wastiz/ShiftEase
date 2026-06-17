using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BLL.Contracts;
using DAL.Contracts;
using Domain;
using Domain.Enums;
using DTOs.EmployeeDtos;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

public class JwtServiceTests
{
    private readonly JwtService _service;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepoMock = new();

    private const string ValidSecret = "supersecretkey_longenough_1234567890"; // ≥ 32 chars for HMAC-SHA256

    public JwtServiceTests()
    {
        var options = Options.Create(new JwtOptions
        {
            SecretKey                   = ValidSecret,
            Issuer                      = "ShiftEase",
            Audience                    = "ShiftEase",
            AccessTokenExpirationMinutes = 60
        });

        _refreshTokenRepoMock
            .Setup(r => r.SaveAsync(It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);

        _service = new JwtService(options, _refreshTokenRepoMock.Object);
    }

    // ── GenerateAccessToken ───────────────────────────────────────────────────

    [Fact]
    public void GenerateAccessToken_ReturnsNonEmptyString()
    {
        var token = _service.GenerateAccessToken("42", UserRole.Employer, "John Doe");

        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public void GenerateAccessToken_ContainsUserIdClaim()
    {
        var token     = _service.GenerateAccessToken("42", UserRole.Employer, "John Doe");
        var principal = _service.ValidateToken(token);

        Assert.NotNull(principal);
        var userId = _service.GetUserIdFromClaims(principal!);
        Assert.Equal("42", userId);
    }

    [Fact]
    public void GenerateAccessToken_ContainsRoleClaim()
    {
        var token     = _service.GenerateAccessToken("1", UserRole.Employee, "Jane Smith");
        var principal = _service.ValidateToken(token);

        Assert.NotNull(principal);
        var role = _service.GetUserRoleFromClaims(principal!);
        Assert.Equal(UserRole.Employee.ToString(), role);
    }

    [Theory]
    [InlineData("1",  UserRole.Employer, "Alice")]
    [InlineData("99", UserRole.Employee, "Bob")]
    public void GenerateAccessToken_ClaimsRoundTrip(string userId, UserRole role, string name)
    {
        var token     = _service.GenerateAccessToken(userId, role, name);
        var principal = _service.ValidateToken(token);

        Assert.NotNull(principal);
        Assert.Equal(userId,              _service.GetUserIdFromClaims(principal!));
        Assert.Equal(role.ToString(),     _service.GetUserRoleFromClaims(principal!));
        Assert.Equal(name, principal!.FindFirst(ClaimTypes.Name)?.Value);
    }

    // ── ValidateToken ─────────────────────────────────────────────────────────

    [Fact]
    public void ValidateToken_ValidToken_ReturnsPrincipal()
    {
        var token  = _service.GenerateAccessToken("1", UserRole.Employer, "Test");
        var result = _service.ValidateToken(token);

        Assert.NotNull(result);
    }

    [Fact]
    public void ValidateToken_TamperedToken_ReturnsNull()
    {
        var token     = _service.GenerateAccessToken("1", UserRole.Employer, "Test");
        var tampered  = token[..^5] + "XXXXX"; // corrupt last 5 chars

        var result = _service.ValidateToken(tampered);

        Assert.Null(result);
    }

    [Fact]
    public void ValidateToken_CompletelyInvalidString_ReturnsNull()
    {
        var result = _service.ValidateToken("this.is.not.a.jwt");

        Assert.Null(result);
    }

    [Fact]
    public void ValidateToken_ExpiredToken_WithLifetimeCheck_ReturnsNull()
    {
        // Build a token that expired 1 hour ago using a JwtSecurityToken directly
        var handler = new JwtSecurityTokenHandler();
        var key     = System.Text.Encoding.UTF8.GetBytes(ValidSecret);
        var creds   = new Microsoft.IdentityModel.Tokens.SigningCredentials(
            new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
            Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

        var expiredToken = new JwtSecurityToken(
            issuer:            "ShiftEase",
            audience:          "ShiftEase",
            claims:            new[] { new Claim(ClaimTypes.NameIdentifier, "1") },
            notBefore:         DateTime.UtcNow.AddHours(-2),
            expires:           DateTime.UtcNow.AddHours(-1),
            signingCredentials: creds);

        var tokenString = handler.WriteToken(expiredToken);

        var result = _service.ValidateToken(tokenString, validateLifetime: true);

        Assert.Null(result);
    }

    [Fact]
    public void ValidateToken_ExpiredToken_WithoutLifetimeCheck_ReturnsPrincipal()
    {
        var handler = new JwtSecurityTokenHandler();
        var key     = System.Text.Encoding.UTF8.GetBytes(ValidSecret);
        var creds   = new Microsoft.IdentityModel.Tokens.SigningCredentials(
            new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
            Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

        var expiredToken = new JwtSecurityToken(
            issuer:            "ShiftEase",
            audience:          "ShiftEase",
            claims:            new[] { new Claim(ClaimTypes.NameIdentifier, "1") },
            notBefore:         DateTime.UtcNow.AddHours(-2),
            expires:           DateTime.UtcNow.AddHours(-1),
            signingCredentials: creds);

        var tokenString = handler.WriteToken(expiredToken);

        var result = _service.ValidateToken(tokenString, validateLifetime: false);

        Assert.NotNull(result);
    }

    // ── GenerateRefreshToken ──────────────────────────────────────────────────

    [Fact]
    public async Task GenerateRefreshToken_ReturnsDifferentTokenEachTime()
    {
        var token1 = await _service.GenerateRefreshToken("1", UserRole.Employer);
        var token2 = await _service.GenerateRefreshToken("1", UserRole.Employer);

        Assert.NotEqual(token1, token2);
    }

    [Fact]
    public async Task GenerateRefreshToken_SavesHashedTokenToRepository()
    {
        var rawToken = await _service.GenerateRefreshToken("1", UserRole.Employer);

        // The raw token is returned; the hashed version is stored in the repo.
        // Verify that SaveAsync was called and the stored token is NOT the raw one.
        _refreshTokenRepoMock.Verify(r => r.SaveAsync(
            It.Is<RefreshToken>(t => t.Token != rawToken && !string.IsNullOrWhiteSpace(t.Token))),
            Times.Once);
    }

    // ── GetUserIdFromClaims / GetUserRoleFromClaims ───────────────────────────

    [Fact]
    public void GetUserIdFromClaims_NoNameIdentifierClaim_ReturnsNull()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "Employer")
        }));

        var result = _service.GetUserIdFromClaims(principal);

        Assert.Null(result);
    }

    [Fact]
    public void GetUserRoleFromClaims_NoRoleClaim_ReturnsNull()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "42")
        }));

        var result = _service.GetUserRoleFromClaims(principal);

        Assert.Null(result);
    }
}
