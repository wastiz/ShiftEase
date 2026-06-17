using DAL.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class RefreshTokenCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RefreshTokenCleanupService> _logger;

    public RefreshTokenCleanupService(
        IServiceScopeFactory scopeFactory,
        ILogger<RefreshTokenCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await CleanupAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
        }
    }

    private async Task CleanupAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();

        var deleted = await repo.DeleteExpiredAndInvalidAsync();

        if (deleted > 0)
        {
            _logger.LogInformation(
                "RefreshToken cleanup: {Count} tokens deleted",
                deleted);
        }
    }
}
