using BLL.DTO.IdentityDtos;
using BLL.Contracts;
using BLL.Services;
using DAL.DTO.EmployerDtos;
using DAL.Contracts;
using Domain;
using Domain.Enums;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

public class IdentityServiceTest
{
    private readonly Mock<IEmployerRepository> _employerRepoMock;
    private readonly Mock<IEmployeeRepository> _employeeRepoMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepoMock;
    private readonly Mock<IPasswordHasher<Employer>> _passwordHasherMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IMailService> _mailServiceMock;
    private readonly Mock<IPasswordResetRepository> _passwordResetRepoMock;
    private readonly IdentityService _service;

    public IdentityServiceTest()
    {
        _employerRepoMock           = new Mock<IEmployerRepository>();
        _employeeRepoMock           = new Mock<IEmployeeRepository>();
        _refreshTokenRepoMock       = new Mock<IRefreshTokenRepository>();
        _passwordHasherMock         = new Mock<IPasswordHasher<Employer>>();
        _jwtServiceMock             = new Mock<IJwtService>();
        _mailServiceMock            = new Mock<IMailService>();
        _passwordResetRepoMock      = new Mock<IPasswordResetRepository>();

        _jwtServiceMock
            .Setup(j => j.GenerateAccessToken(It.IsAny<string>(), It.IsAny<UserRole>(), It.IsAny<string>()))
            .Returns("access_token");
        _jwtServiceMock
            .Setup(j => j.GenerateRefreshToken(It.IsAny<string>(), It.IsAny<UserRole>()))
            .ReturnsAsync("refresh_token");

        _service = new IdentityService(
            _employerRepoMock.Object,
            _employeeRepoMock.Object,
            _refreshTokenRepoMock.Object,
            _passwordHasherMock.Object,
            _jwtServiceMock.Object,
            _mailServiceMock.Object,
            _passwordResetRepoMock.Object
        );
    }

    // ── Register ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task RegisterEmployerAsync_Success_ReturnsSuccess()
    {
        var dto = new BllRegister
        {
            Email     = "test@example.com",
            Password  = "Password123!",
            FirstName = "John",
            LastName  = "Doe"
        };

        _employerRepoMock.Setup(r => r.GetByEmailAsync(dto.Email))
            .ReturnsAsync((DalEmployerFullData)null);
        _employerRepoMock.Setup(r => r.CreateAsync(It.IsAny<DalEmployerCreate>()))
            .ReturnsAsync(new Employer { Id = 1, FirstName = dto.FirstName, LastName = dto.LastName });

        var result = await _service.RegisterEmployerAsync(dto);

        Assert.True(result.Success);
        Assert.Equal("Registration successful.", result.Message);
    }

    [Fact]
    public async Task RegisterEmployerAsync_EmailAlreadyExists_ReturnsFailure()
    {
        var dto = new BllRegister
        {
            Email     = "test@example.com",
            Password  = "Password123!",
            FirstName = "John",
            LastName  = "Doe"
        };

        _employerRepoMock.Setup(r => r.GetByEmailAsync(dto.Email))
            .ReturnsAsync(new DalEmployerFullData { Id = 1, Email = dto.Email });

        var result = await _service.RegisterEmployerAsync(dto);

        Assert.False(result.Success);
        Assert.Equal("User with this email is already registered.", result.Message);
    }

    [Theory]
    [InlineData("", "John", "Doe", "pass")]
    [InlineData("t@t.com", "", "Doe", "pass")]
    [InlineData("t@t.com", "John", "", "pass")]
    [InlineData("t@t.com", "John", "Doe", "")]
    public async Task RegisterEmployerAsync_MissingFields_ReturnsFailure(
        string email, string firstName, string lastName, string password)
    {
        var dto = new BllRegister
        {
            Email     = email,
            FirstName = firstName,
            LastName  = lastName,
            Password  = password
        };

        var result = await _service.RegisterEmployerAsync(dto);

        Assert.False(result.Success);
        Assert.Equal("Please fill all required fields", result.Message);
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task LoginEmployerAsync_Success_ReturnsTokens()
    {
        var dto = new BllLogin { Email = "test@example.com", Password = "Password123!" };

        _employerRepoMock.Setup(r => r.CheckPasswordAsync(dto.Email, dto.Password))
            .ReturnsAsync(new Employer { Id = 1, FirstName = "John", LastName = "Doe" });

        var result = await _service.LoginEmployerAsync(dto);

        Assert.True(result.Success);
        Assert.NotNull(result.Data!.AccessToken);
        Assert.NotNull(result.Data!.RefreshToken);
        Assert.Equal("User login successfully", result.Message);
    }

    [Fact]
    public async Task LoginEmployerAsync_WrongCredentials_ReturnsFailure()
    {
        var dto = new BllLogin { Email = "test@example.com", Password = "WrongPassword" };

        _employerRepoMock.Setup(r => r.CheckPasswordAsync(dto.Email, dto.Password))
            .ReturnsAsync((Employer)null);

        var result = await _service.LoginEmployerAsync(dto);

        Assert.False(result.Success);
        Assert.Equal("Incorrect email or password", result.Message);
    }

    [Fact]
    public async Task LoginEmployeeAsync_Success_ReturnsTokensAndIds()
    {
        var dto = new BllLogin { Email = "employee@example.com", Password = "Password123!" };

        var employee = new Employee
        {
            Id             = 10,
            FirstName      = "Jane",
            LastName       = "Smith",
            OrganizationId = 2
        };

        _employeeRepoMock.Setup(r => r.CheckPasswordAsync(dto.Email, dto.Password))
            .ReturnsAsync(employee);
        _employeeRepoMock.Setup(r => r.GetDepartmentIdsByEmployeeIdAsync(employee.Id))
            .ReturnsAsync(new List<int> { 3, 4 });

        var result = await _service.LoginEmployeeAsync(dto);

        Assert.True(result.Success);
        Assert.NotNull(result.Data!.AccessToken);
        Assert.NotNull(result.Data!.RefreshToken);
        Assert.Equal(employee.Id, result.Data.EmployeeId);
        Assert.Equal(employee.OrganizationId, result.Data.OrganizationId);
        Assert.Contains(3, result.Data.DepartmentIds);
        Assert.Equal("User login successfully", result.Message);
    }

    // ── Refresh ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task RefreshAsync_ValidToken_ReturnsNewTokens()
    {
        // The service SHA-256-hashes the raw token before calling GetByTokenAsync,
        // so we match any string in the mock.
        var storedToken = new RefreshToken
        {
            Token     = "hashed_token",
            Expires   = DateTime.UtcNow.AddHours(1),
            IsUsed    = false,
            IsRevoked = false,
            UserId    = "1",
            UserRole  = UserRole.Employer
        };

        _refreshTokenRepoMock.Setup(r => r.GetByTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(storedToken);
        _refreshTokenRepoMock.Setup(r => r.InvalidateAsync(It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.RefreshAsync("raw_refresh_token");

        Assert.True(result.Success);
        Assert.NotNull(result.Data!.AccessToken);
        Assert.NotNull(result.Data!.RefreshToken);
    }

    [Fact]
    public async Task RefreshAsync_TokenNotFound_ReturnsFailure()
    {
        _refreshTokenRepoMock.Setup(r => r.GetByTokenAsync(It.IsAny<string>()))
            .ReturnsAsync((RefreshToken)null);

        var result = await _service.RefreshAsync("nonexistent_raw_token");

        Assert.False(result.Success);
        Assert.Equal("Refresh token is expired", result.Message);
    }

    [Fact]
    public async Task RefreshAsync_ExpiredToken_ReturnsFailure()
    {
        var expiredToken = new RefreshToken
        {
            Token     = "hashed",
            Expires   = DateTime.UtcNow.AddDays(-1),
            IsUsed    = false,
            IsRevoked = false,
            UserId    = "1",
            UserRole  = UserRole.Employer
        };

        _refreshTokenRepoMock.Setup(r => r.GetByTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(expiredToken);

        var result = await _service.RefreshAsync("expired_raw_token");

        Assert.False(result.Success);
        Assert.Equal("Refresh token is expired", result.Message);
    }

    [Fact]
    public async Task RefreshAsync_UsedToken_ReturnsFailure()
    {
        var usedToken = new RefreshToken
        {
            Token     = "hashed",
            Expires   = DateTime.UtcNow.AddHours(1),
            IsUsed    = true,
            IsRevoked = false,
            UserId    = "1",
            UserRole  = UserRole.Employer
        };

        _refreshTokenRepoMock.Setup(r => r.GetByTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(usedToken);

        var result = await _service.RefreshAsync("used_raw_token");

        Assert.False(result.Success);
        Assert.Equal("Refresh token is expired", result.Message);
    }

    [Fact]
    public async Task RefreshAsync_RevokedToken_ReturnsFailure()
    {
        var revokedToken = new RefreshToken
        {
            Token     = "hashed",
            Expires   = DateTime.UtcNow.AddHours(1),
            IsUsed    = false,
            IsRevoked = true,
            UserId    = "1",
            UserRole  = UserRole.Employer
        };

        _refreshTokenRepoMock.Setup(r => r.GetByTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(revokedToken);

        var result = await _service.RefreshAsync("revoked_raw_token");

        Assert.False(result.Success);
        Assert.Equal("Refresh token is expired", result.Message);
    }

    // ── Logout ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task LogoutAsync_ValidToken_RevokesTokenAndReturnsTrue()
    {
        var token = new RefreshToken { Token = "token123", IsRevoked = false };

        _refreshTokenRepoMock.Setup(r => r.GetByTokenAsync(token.Token))
            .ReturnsAsync(token);
        _refreshTokenRepoMock.Setup(r => r.InvalidateAsync(token))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var result = await _service.LogoutAsync(token.Token);

        Assert.True(result);
        Assert.True(token.IsRevoked);
        _refreshTokenRepoMock.Verify(r => r.InvalidateAsync(token), Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_TokenNotFound_ReturnsFalse()
    {
        _refreshTokenRepoMock.Setup(r => r.GetByTokenAsync(It.IsAny<string>()))
            .ReturnsAsync((RefreshToken)null);

        var result = await _service.LogoutAsync("nonexistent_token");

        Assert.False(result);
    }

    // ── ForgotPassword ────────────────────────────────────────────────────────

    [Fact]
    public async Task ForgotPasswordAsync_UserNotFound_ReturnsSuccessAnyway()
    {
        // Security best practice: don't reveal whether email exists.
        _employerRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((DalEmployerFullData)null);
        _employeeRepoMock.Setup(r => r.EmployeeEmailExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        var result = await _service.ForgotPasswordAsync(new BllForgotPassword { Email = "nobody@test.com" });

        Assert.True(result.Success);
    }

    [Fact]
    public async Task ForgotPasswordAsync_EmployerExists_SendsEmailAndReturnsSuccess()
    {
        const string email = "employer@test.com";

        _employerRepoMock.Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync(new DalEmployerFullData { Id = 1, Email = email });
        _passwordResetRepoMock
            .Setup(r => r.CreateTokenAsync(email, It.IsAny<string>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new PasswordResetToken { Email = email, Token = "tok", ExpiresAt = DateTime.UtcNow.AddHours(1) });
        _mailServiceMock
            .Setup(m => m.SendPasswordResetEmailAsync(email, It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var result = await _service.ForgotPasswordAsync(new BllForgotPassword { Email = email });

        Assert.True(result.Success);
        _mailServiceMock.Verify(m => m.SendPasswordResetEmailAsync(email, It.IsAny<string>()), Times.Once);
    }

    // ── ResetPassword ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ResetPasswordAsync_ExpiredToken_ReturnsFailure()
    {
        _passwordResetRepoMock.Setup(r => r.GetByTokenAsync("expired_token"))
            .ReturnsAsync(new PasswordResetToken
            {
                Email     = "test@test.com",
                Token     = "expired_token",
                ExpiresAt = DateTime.UtcNow.AddHours(-1),
                IsUsed    = false
            });

        var result = await _service.ResetPasswordAsync(
            new BllResetPassword { Token = "expired_token", NewPassword = "newpass123" });

        Assert.False(result.Success);
        Assert.Equal("Invalid or expired reset token.", result.Message);
    }

    [Fact]
    public async Task ResetPasswordAsync_UsedToken_ReturnsFailure()
    {
        _passwordResetRepoMock.Setup(r => r.GetByTokenAsync("used_token"))
            .ReturnsAsync(new PasswordResetToken
            {
                Email     = "test@test.com",
                Token     = "used_token",
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                IsUsed    = true
            });

        var result = await _service.ResetPasswordAsync(
            new BllResetPassword { Token = "used_token", NewPassword = "newpass123" });

        Assert.False(result.Success);
        Assert.Equal("Invalid or expired reset token.", result.Message);
    }

    [Fact]
    public async Task ResetPasswordAsync_ValidToken_EmployerExists_ReturnsSuccess()
    {
        const string email = "employer@test.com";

        _passwordResetRepoMock.Setup(r => r.GetByTokenAsync("valid_token"))
            .ReturnsAsync(new PasswordResetToken
            {
                Email     = email,
                Token     = "valid_token",
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                IsUsed    = false
            });
        _employerRepoMock.Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync(new DalEmployerFullData { Id = 1, Email = email });
        _employerRepoMock.Setup(r => r.UpdateEmployerPasswordAsync(1, "newpass123"))
            .ReturnsAsync(true);
        _passwordResetRepoMock.Setup(r => r.MarkAsUsedAsync("valid_token"))
            .Returns(Task.CompletedTask);

        var result = await _service.ResetPasswordAsync(
            new BllResetPassword { Token = "valid_token", NewPassword = "newpass123" });

        Assert.True(result.Success);
        _passwordResetRepoMock.Verify(r => r.MarkAsUsedAsync("valid_token"), Times.Once);
    }
}
