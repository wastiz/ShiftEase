using DAL;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using BLL.Contracts;
using Moq;

namespace ShiftEase.Tests.Security;

/// <summary>
/// Isolated factory for security tests.
/// Uses a per-test InMemory database and a known JWT secret so tokens
/// can be minted manually inside tests.
/// </summary>
public class SecurityWebApplicationFactory : WebApplicationFactory<Program>
{
    /// <summary>Known secret used for minting test JWTs.</summary>
    public const string JwtSecret  = "security_tests_secret_key_min32chars!!";
    public const string JwtIssuer  = "ShiftEase";
    public const string JwtAudience = "ShiftEase";

    // Unique DB per factory instance → no shared state between test classes.
    private readonly string _dbName = $"SecurityDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Override JWT config so manually-minted tokens are accepted.
        builder.ConfigureAppConfiguration((_, cfg) =>
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"]                     = JwtSecret,
                ["Jwt:Issuer"]                        = JwtIssuer,
                ["Jwt:Audience"]                      = JwtAudience,
                ["Jwt:AccessTokenExpirationMinutes"]  = "60",
                // Prevent real SMTP calls
                ["MailSettings:SmtpServer"]           = "localhost",
                ["MailSettings:SmtpPort"]             = "25",
            }));

        builder.ConfigureServices(services =>
        {
            // Replace PostgreSQL with InMemory
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>(o =>
                o.UseInMemoryDatabase(_dbName));

            // Remove background token-cleanup service (not needed in tests)
            var hostedDescriptors = services
                .Where(d => d.ServiceType == typeof(IHostedService))
                .ToList();
            foreach (var d in hostedDescriptors)
                services.Remove(d);

            // Stub the mail service so ForgotPassword does not throw
            services.RemoveAll<IMailService>();
            var mailMock = new Mock<IMailService>();
            mailMock.Setup(m => m.SendPasswordResetEmailAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(Task.CompletedTask);
            services.AddSingleton(mailMock.Object);

            // Ensure DB is created
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
        });
    }
}
