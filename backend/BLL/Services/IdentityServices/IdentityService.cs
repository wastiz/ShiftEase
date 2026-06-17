using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using BLL.DTO.IdentityDtos;
using BLL.Contracts;
using BLL.Mappers;
using DAL.Contracts;
using Domain.Enums;
using DTOs.EmployeeDtos;
using Microsoft.AspNetCore.Identity;

namespace BLL.Services;

public class IdentityService : IIdentityService
{
    private readonly IEmployerRepository _employerRepo;
    private readonly IEmployeeRepository _employeeRepo;
    private readonly IRefreshTokenRepository _refreshTokenRepo;
    private readonly IPasswordHasher<Employer> _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IMailService _mailService;
    private readonly IPasswordResetRepository _passwordResetRepo;
    public IdentityService(IEmployerRepository employerRepo,
                           IEmployeeRepository employeeRepo,
                           IRefreshTokenRepository refreshTokenRepo,
                           IPasswordHasher<Employer> passwordHasher,
                           IJwtService jwtService,
                           IMailService mailService,
                           IPasswordResetRepository passwordResetRepo)
    {
        _employerRepo = employerRepo;
        _employeeRepo = employeeRepo;
        _refreshTokenRepo = refreshTokenRepo;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _mailService = mailService;
        _passwordResetRepo = passwordResetRepo;
    }

    public async Task<BllResult<bool>> RegisterEmployerAsync(BllRegister dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FirstName) ||
            string.IsNullOrWhiteSpace(dto.LastName) ||
            string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.Password))
        {
            return BllResult<bool>.Fail("Please fill all required fields");
        }

        if (await _employerRepo.GetByEmailAsync(dto.Email) != null)
        {
            return BllResult<bool>.Fail("User with this email is already registered.");
        }

        await _employerRepo.CreateAsync(IdentityMapper.MapToDal(dto));

        return BllResult<bool>.Ok(true, "Registration successful.");
    }

    public async Task<BllResult<BllAuthResult>> LoginEmployerAsync(BllLogin login)
    {
        var employer = await _employerRepo.CheckPasswordAsync(login.Email, login.Password);

        if (employer == null)
        {
            return BllResult<BllAuthResult>.Fail("Incorrect email or password");
        }

        var accessToken = _jwtService.GenerateAccessToken(employer.Id.ToString(), UserRole.Employer, $"{employer.FirstName} {employer.LastName}");
        var refreshToken = await _jwtService.GenerateRefreshToken(employer.Id.ToString(), UserRole.Employer);

        return BllResult<BllAuthResult>.Ok(new BllAuthResult()
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        }, "User login successfully");
    }

    public async Task<BllResult<BllEmployeeAuthResult>> LoginEmployeeAsync(BllLogin login)
    {
        var employee = await _employeeRepo.CheckPasswordAsync(login.Email, login.Password);
        
        if (employee == null)
        {
            return BllResult<BllEmployeeAuthResult>.Fail("Incorrect email or password");
        }
        
        var accessToken = _jwtService.GenerateAccessToken(employee.Id.ToString(), UserRole.Employee, $"{employee.FirstName} {employee.LastName}");
        var refreshToken = await _jwtService.GenerateRefreshToken(employee.Id.ToString(), UserRole.Employee);
        
        var departmentIds = await _employeeRepo.GetDepartmentIdsByEmployeeIdAsync(employee.Id);

        return BllResult<BllEmployeeAuthResult>.Ok(new BllEmployeeAuthResult()
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            EmployeeId = employee.Id,
            OrganizationId = employee.OrganizationId,
            DepartmentIds = departmentIds
        }, "User login successfully");
    }
    
    public async Task<BllResult<BllUser>> GetUserMeDataAsync(int userId, UserRole userRole)
    {
        if (userRole == UserRole.Employer)
        {
            var employer = await _employerRepo.GetByIdAsync(userId);
        
            if (employer == null) return BllResult<BllUser>.Fail("Employer not found");

            return BllResult<BllUser>.Ok(new BllEmployerMeData
            {
                Id = employer.Id,
                Role = UserRole.Employer,
                FullName = employer.FirstName + " " + employer.LastName,
                Email = employer.Email,
            });
        }

        if (userRole == UserRole.Employee)
        {
            var employee = await _employeeRepo.GetByIdAsync(userId);
        
            if (employee == null) return BllResult<BllUser>.Fail("Employee not found");
        
            return BllResult<BllUser>.Ok(new BllEmployeeMeData()
            {
                Id = employee.Id,
                Role =  UserRole.Employee,
                FullName = employee.FirstName + " " + employee.LastName,
                Email = employee.Email,
                DepartmentIds = employee.DepartmentIds,
                DepartmentNames = employee.DepartmentNames,
                OrganizationId = employee.OrganizationId,
                OrganizationName = employee.OrganizationName,
                Position = employee.Position,
            });
        }

        return BllResult<BllUser>.Fail("Unknown role");
    }

    public async Task<BllResult<BllAuthResult>> RefreshAsync(string refreshToken)
    {
        var hashed = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));

        var token = await _refreshTokenRepo.GetByTokenAsync(hashed);

        if (token == null || token.Expires < DateTime.UtcNow || token.IsUsed || token.IsRevoked)
            return BllResult<BllAuthResult>.Fail("Refresh token is expired");

        token.IsUsed = true;
        await _refreshTokenRepo.InvalidateAsync(token);

        var accessToken = _jwtService.GenerateAccessToken(token.UserId, token.UserRole, "");
        var newRefreshToken = await _jwtService.GenerateRefreshToken(token.UserId, token.UserRole);

        return BllResult<BllAuthResult>.Ok(new BllAuthResult()
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
        }, "User refresh successfully");
    }

    public async Task<bool> LogoutAsync(string refreshToken)
    {
        var token = await _refreshTokenRepo.GetByTokenAsync(refreshToken);
        if (token == null) return false;
        token.IsRevoked = true;
        await _refreshTokenRepo.InvalidateAsync(token);

        return true;
    }

    public async Task<BllResult<bool>> ForgotPasswordAsync(BllForgotPassword dto)
    {
        // Check if employer or employee exists
        var employerExists = await _employerRepo.GetByEmailAsync(dto.Email) != null;
        var employeeExists = await _employeeRepo.EmployeeEmailExistsAsync(dto.Email);

        if (!employerExists && !employeeExists)
        {
            // Return success even if user not found (security best practice)
            return BllResult<bool>.Ok(true, "If an account with that email exists, a password reset link has been sent.");
        }

        // Generate reset token
        var resetToken = Guid.NewGuid().ToString("N");
        var expiresAt = DateTime.UtcNow.AddHours(1);

        // Save token to database
        await _passwordResetRepo.CreateTokenAsync(dto.Email, resetToken, expiresAt);

        // Send email
        try
        {
            await _mailService.SendPasswordResetEmailAsync(dto.Email, resetToken);
        }
        catch (Exception ex)
        {
            return BllResult<bool>.Fail($"Failed to send email: {ex.Message}");
        }

        return BllResult<bool>.Ok(true, "If an account with that email exists, a password reset link has been sent.");
    }

    public async Task<BllResult<bool>> ResetPasswordAsync(BllResetPassword dto)
    {
        // Validate token
        var resetToken = await _passwordResetRepo.GetByTokenAsync(dto.Token);

        if (resetToken == null || resetToken.IsUsed || resetToken.ExpiresAt < DateTime.UtcNow)
        {
            return BllResult<bool>.Fail("Invalid or expired reset token.");
        }

        // Update password via repositories
        var employer = await _employerRepo.GetByEmailAsync(resetToken.Email);

        if (employer != null)
        {
            await _employerRepo.UpdateEmployerPasswordAsync(employer.Id, dto.NewPassword);
        }
        else
        {
            // Check if employee exists and update password
            var employeeExists = await _employeeRepo.EmployeeEmailExistsAsync(resetToken.Email);
            if (employeeExists)
            {
                await _employeeRepo.UpdatePasswordByEmailAsync(resetToken.Email, dto.NewPassword);
            }
            else
            {
                return BllResult<bool>.Fail("User not found.");
            }
        }

        // Mark token as used
        await _passwordResetRepo.MarkAsUsedAsync(dto.Token);

        return BllResult<bool>.Ok(true, "Password has been reset successfully.");
    }

    public async Task<BllResult<bool>> DeleteAccountAsync(int employerId, string password)
    {
        var employer = await _employerRepo.GetEmployerEntityByIdAsync(employerId);
        if (employer == null)
            return BllResult<bool>.Fail("Account not found.");

        if (employer.Password == null)
            return BllResult<bool>.Fail("This account does not have a password set. Please contact support.");

        if (!await _employerRepo.VerifyPasswordByIdAsync(employerId, password))
            return BllResult<bool>.Fail("Incorrect password.");

        // Delete non-cascading auth records
        await _refreshTokenRepo.DeleteByUserIdAsync(employerId.ToString());
        await _passwordResetRepo.DeleteByEmailAsync(employer.Email);

        // Delete employer — cascades to all organizations and their data via DB
        await _employerRepo.DeleteAsync(employerId);

        return BllResult<bool>.Ok(true, "Account and all associated data have been permanently deleted.");
    }
}
