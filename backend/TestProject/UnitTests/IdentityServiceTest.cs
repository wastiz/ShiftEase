using System;
using System.Threading.Tasks;
using BLL.DTO.IdentityDtos;
using BLL.Interfaces;
using BLL.Services;
using DAL.DTO.EmployerDtos;
using DAL.Repositories;
using DAL.RepositoryInterfaces;
using Domain;
using Domain.Models;
using DTOs.IdentityDtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

public class IdentityServiceTest
{
    private readonly Mock<IConfiguration> _configMock;
    private readonly Mock<IEmployerRepository> _employerRepoMock;
    private readonly Mock<IEmployeeRepository> _employeeRepoMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepoMock;
    private readonly Mock<IPasswordHasher<Employer>> _passwordHasherMock;
    private readonly IdentityService _service;

    public IdentityServiceTest()
    {
        _configMock = new Mock<IConfiguration>();
        _employerRepoMock = new Mock<IEmployerRepository>();
        _employeeRepoMock = new Mock<IEmployeeRepository>();
        _refreshTokenRepoMock = new Mock<IRefreshTokenRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher<Employer>>();

        _configMock.Setup(c => c["Jwt:SecretKey"]).Returns("themostlongestsuperdupersecretkey12345");
        _configMock.Setup(c => c["Jwt:Issuer"]).Returns("ShiftEase");
        _configMock.Setup(c => c["Jwt:Audience"]).Returns("ShiftEase");

        _service = new IdentityService(
            _configMock.Object,
            _employerRepoMock.Object,
            _employeeRepoMock.Object,
            _refreshTokenRepoMock.Object,
            _passwordHasherMock.Object
        );
    }

    [Fact]
    public async Task RegisterEmployerAsync_Success_ReturnsTokens()
    {
        // Arrange
        var dto = new BllEmployerRegister
        {
            Email = "test@example.com",
            Password = "Password123!",
            FirstName = "John",
            LastName = "Doe"
        };

        _employerRepoMock.Setup(r => r.GetByEmailAsync(dto.Email))
            .ReturnsAsync((DalEmployerFullData)null);

        _employerRepoMock.Setup(r => r.CreateAsync(It.IsAny<DalEmployerCreate>()))
            .ReturnsAsync(new Employer { Id = 1, FirstName = dto.FirstName, LastName = dto.LastName });

        _refreshTokenRepoMock.Setup(r => r.SaveAsync(It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.RegisterEmployerAsync(dto);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.AccessToken);
        Assert.NotNull(result.RefreshToken);
        Assert.Equal("Employer Registered Successfully", result.Message);
    }

    [Fact]
    public async Task RegisterEmployerAsync_EmailAlreadyExists_ReturnsFailure()
    {
        // Arrange
        var dto = new BllEmployerRegister
        {
            Email = "test@example.com",
            Password = "Password123!",
            FirstName = "John",
            LastName = "Doe"
        };

        _employerRepoMock.Setup(r => r.GetByEmailAsync(dto.Email))
            .ReturnsAsync(new DalEmployerFullData() { Id = 1, Email = dto.Email });

        // Act
        var result = await _service.RegisterEmployerAsync(dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Employer with this email is already registered.", result.Message);
    }

    [Fact]
    public async Task LoginEmployerAsync_Success_ReturnsTokens()
    {
        // Arrange
        var dto = new BllLoginDto
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        _employerRepoMock.Setup(r => r.CheckPasswordAsync(dto.Email, dto.Password))
            .ReturnsAsync(new Employer { Id = 1, FirstName = "John", LastName = "Doe" });

        _refreshTokenRepoMock.Setup(r => r.SaveAsync(It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.LoginEmployerAsync(dto);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.AccessToken);
        Assert.NotNull(result.RefreshToken);
        Assert.Equal("Login success", result.Message);
    }

    [Fact]
    public async Task LoginEmployeeAsync_Success_ReturnsTokensAndIds()
    {
        // Arrange
        var dto = new BllLoginDto
        {
            Email = "employee@example.com",
            Password = "Password123!"
        };

        var employee = new Employee
        {
            Id = 10,
            FirstName = "Jane",
            LastName = "Smith",
            OrganizationId = 2,
            GroupId = 3
        };

        _employeeRepoMock.Setup(r => r.CheckPasswordAsync(dto.Email, dto.Password))
            .ReturnsAsync(employee);

        _refreshTokenRepoMock.Setup(r => r.SaveAsync(It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.LoginEmployeeAsync(dto);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.AccessToken);
        Assert.NotNull(result.RefreshToken);
        Assert.Equal(employee.Id, result.EmployeeId);
        Assert.Equal(employee.OrganizationId, result.OrganizationId);
        Assert.Equal(employee.GroupId, result.GroupId);
        Assert.Equal("Login success", result.Message);
    }

    [Fact]
    public async Task RefreshAsync_ValidToken_ReturnsNewTokens()
    {
        // Arrange
        var oldToken = "old_refresh_token";
        var storedToken = new RefreshToken
        {
            Token = oldToken,
            Expires = DateTime.UtcNow.AddHours(1),
            IsUsed = false,
            IsRevoked = false,
            UserId = "1",
            UserRole = "Employer"
        };

        _refreshTokenRepoMock.Setup(r => r.GetByTokenAsync(oldToken))
            .ReturnsAsync(storedToken);

        _refreshTokenRepoMock.Setup(r => r.InvalidateAsync(It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);

        _refreshTokenRepoMock.Setup(r => r.SaveAsync(It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.RefreshAsync(oldToken);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.AccessToken);
        Assert.NotNull(result.RefreshToken);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task RefreshAsync_InvalidOrEmptyToken_ReturnsFailure(string token)
    {
        // Arrange
        _refreshTokenRepoMock.Setup(r => r.GetByTokenAsync(token))
            .ReturnsAsync((RefreshToken)null);

        // Act
        var result = await _service.RefreshAsync(token);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Invalid or expired refresh token", result.Message);
    }

    [Fact]
    public async Task RefreshAsync_ExpiredToken_ReturnsFailure()
    {
        // Arrange
        var expiredToken = new RefreshToken
        {
            Token = "expired",
            Expires = DateTime.UtcNow.AddDays(-1),
            IsUsed = false,
            IsRevoked = false,
            UserId = "1",
            UserRole = "Employer"
        };

        _refreshTokenRepoMock.Setup(r => r.GetByTokenAsync(expiredToken.Token))
            .ReturnsAsync(expiredToken);

        // Act
        var result = await _service.RefreshAsync(expiredToken.Token);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Invalid or expired refresh token", result.Message);
    }

    [Fact]
    public async Task LogoutAsync_ValidToken_RevokesToken()
    {
        // Arrange
        var token = new RefreshToken
        {
            Token = "token123",
            IsRevoked = false,
        };

        _refreshTokenRepoMock.Setup(r => r.GetByTokenAsync(token.Token))
            .ReturnsAsync(token);

        _refreshTokenRepoMock.Setup(r => r.InvalidateAsync(token))
            .Returns(Task.CompletedTask)
            .Verifiable();

        // Act
        await _service.LogoutAsync(token.Token);

        // Assert
        Assert.True(token.IsRevoked);
        _refreshTokenRepoMock.Verify(r => r.InvalidateAsync(token), Times.Once);
    }
}
