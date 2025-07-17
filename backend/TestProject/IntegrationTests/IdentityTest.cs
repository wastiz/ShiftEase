using System.Net;
using System.Net.Http.Json;
using BLL.DTO.IdentityDtos;
using DTOs.IdentityDtos;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Xunit;
using Xunit.Extensions.Ordering;

namespace ShiftEase.Tests.Integration.Api;

[Collection("Sequential")]
public class IdentityTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public IdentityTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }
    
    public class AuthResponseDto
    {
        public string AccessToken { get; set; } = default!;
        public string RefreshToken { get; set; } = default!;
        public string? Message { get; set; }
    }

    [Fact, Order(1)]
    public async Task Register_Employer_Success()
    {
        var registerDto = new BllEmployerRegister
        {
            FirstName = "Test",
            LastName = "Employer",
            Email = "employer@test.com",
            Password = "Test.Password.123"
        };

        var response = await _client.PostAsJsonAsync("/api/identity/register/employer", registerDto);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
    }

    [Fact, Order(2)]
    public async Task Login_Employer_Success()
    {
        var loginDto = new BllLoginDto
        {
            Email = "employer@test.com",
            Password = "Test.Password.123"
        };

        var response = await _client.PostAsJsonAsync("/api/identity/login/employer", loginDto);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
    }

    [Fact, Order(3)]
    public async Task Login_Employer_Invalid_Credentials_Returns_Unauthorized()
    {
        var loginDto = new BllLoginDto
        {
            Email = "employer@test.com",
            Password = "WrongPassword123"
        };

        var response = await _client.PostAsJsonAsync("/api/identity/login/employer", loginDto);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact, Order(4)]
    public async Task Refresh_Token_Success()
    {
        var loginDto = new BllLoginDto
        {
            Email = "employer@test.com",
            Password = "Test.Password.123"
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/identity/login/employer", loginDto);
        loginResponse.EnsureSuccessStatusCode();
        var authData = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        var response = await _client.PostAsJsonAsync("/api/identity/refresh", authData.RefreshToken);

        response.EnsureSuccessStatusCode();

        var refreshResult = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(refreshResult);
        Assert.False(string.IsNullOrWhiteSpace(refreshResult.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(refreshResult.RefreshToken));
    }

    [Fact, Order(5)]
    public async Task Logout_Success()
    {
        var loginDto = new BllLoginDto
        {
            Email = "employer@test.com",
            Password = "Test.Password.123"
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/identity/login/employer", loginDto);
        loginResponse.EnsureSuccessStatusCode();
        var authData = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        var response = await _client.PostAsJsonAsync("/api/identity/logout", authData.RefreshToken);

        response.EnsureSuccessStatusCode();

        var message = await response.Content.ReadAsStringAsync();
        Assert.Equal("\"Logged out\"", message);
    }
}
