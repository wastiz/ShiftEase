using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BLL.DTO.IdentityDtos;
using BLL.Interfaces;
using BLL.Mappers;
using DAL.Repositories;
using DAL.RepositoryInterfaces;
using Domain;
using DTOs.EmployeeDtos;
using DTOs.IdentityDtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BLL.Services;

public class IdentityService : IIdentityService
{
    private readonly IConfiguration _config;
    private readonly IEmployerRepository _employerRepo;
    private readonly IEmployeeRepository _employeeRepo;
    private readonly IRefreshTokenRepository _refreshTokenRepo;
    private readonly IPasswordHasher<Employer> _passwordHasher;

    public IdentityService(IConfiguration config,
                           IEmployerRepository employerRepo,
                           IEmployeeRepository employeeRepo,
                           IRefreshTokenRepository refreshTokenRepo,
                           IPasswordHasher<Employer> passwordHasher)
    {
        _config = config;
        _employerRepo = employerRepo;
        _employeeRepo = employeeRepo;
        _refreshTokenRepo = refreshTokenRepo;
        _passwordHasher = passwordHasher;
    }

    public async Task<BllEmployerAuthResult> RegisterEmployerAsync(BllEmployerRegister registerDto)
    {
        if (await _employerRepo.GetByEmailAsync(registerDto.Email) != null)
        {
            return new BllEmployerAuthResult() { Success = false, Message = "Employer with this email is already registered." };
        }

        var createdEmployer = await _employerRepo.CreateAsync(IdentityMapper.MapToDal(registerDto));

        var accessToken = GenerateJwtToken(createdEmployer.Id.ToString(), "Employer", createdEmployer.FirstName + " " + createdEmployer.LastName);
        var refreshToken = await GenerateRefreshToken(createdEmployer.Id.ToString(), "Employer");

        return new BllEmployerAuthResult() { Success = true, Message = "Employer Registered Successfully", AccessToken = accessToken, RefreshToken = refreshToken };
    }

    public async Task<BllEmployerAuthResult> LoginEmployerAsync(BllLoginDto loginDto)
    {
        var employer = await _employerRepo.CheckPasswordAsync(loginDto.Email, loginDto.Password);
        var accessToken = GenerateJwtToken(employer.Id.ToString(), "Employer", employer.FirstName + " " + employer.LastName);
        var refreshToken = await GenerateRefreshToken(employer.Id.ToString(), "Employer");

        return new BllEmployerAuthResult() { Success = true, Message = "Login success", AccessToken = accessToken, RefreshToken = refreshToken };
    }

    public async Task<BllEmployeeAuthResult> LoginEmployeeAsync(BllLoginDto loginDto)
    {
        var employee = await _employeeRepo.CheckPasswordAsync(loginDto.Email, loginDto.Password);
        var accessToken = GenerateJwtToken(employee.Id.ToString(), "Employee", $"{employee.FirstName} {employee.LastName}");
        var refreshToken = await GenerateRefreshToken(employee.Id.ToString(), "Employee");

        return new BllEmployeeAuthResult()
        {
            Success = true,
            Message = "Login success",
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            EmployeeId = employee.Id,
            OrganizationId = employee.OrganizationId,
            GroupId = employee.GroupId
        };
    }

    public async Task<BllEmployerAuthResult> RefreshAsync(string refreshToken)
    {
        var token = await _refreshTokenRepo.GetByTokenAsync(refreshToken);

        if (token == null || token.Expires < DateTime.UtcNow || token.IsUsed || token.IsRevoked)
            return new BllEmployerAuthResult() { Success = false, Message = "Invalid or expired refresh token" };

        token.IsUsed = true;
        await _refreshTokenRepo.InvalidateAsync(token);

        var accessToken = GenerateJwtToken(token.UserId, token.UserRole, "");
        var newRefreshToken = await GenerateRefreshToken(token.UserId, token.UserRole);

        return new BllEmployerAuthResult() { Success = true, AccessToken = accessToken, RefreshToken = newRefreshToken };
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var token = await _refreshTokenRepo.GetByTokenAsync(refreshToken);
        if (token != null)
        {
            token.IsRevoked = true;
            await _refreshTokenRepo.InvalidateAsync(token);
        }
    }

    // Helpers
    private string GenerateJwtToken(string userId, string role, string name)
    {
        var claims = new List<Claim>
        {
            new Claim("UserId", userId),
            new Claim(ClaimTypes.Role, role),
            new Claim("Name", name ?? "")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<string> GenerateRefreshToken(string userId, string role)
    {
        var refreshToken = new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            Expires = DateTime.UtcNow.AddDays(7),
            UserId = userId,
            UserRole = role
        };

        await _refreshTokenRepo.SaveAsync(refreshToken);
        return refreshToken.Token;
    }
}
