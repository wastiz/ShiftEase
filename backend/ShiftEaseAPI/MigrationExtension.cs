using DAL;
using Microsoft.EntityFrameworkCore;

public static class MigrationExtension
{
    public static async Task ApplyMigrationsAsync(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();
        
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        try
        {
            logger.LogInformation("Starting database migration...");
            
            using AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            
            logger.LogInformation("Testing database connection with retry logic...");
            await WaitForDatabaseAsync(dbContext, logger);
            
            logger.LogInformation("Database connection successful");
            
            var pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync()).ToList();
            
            if (pendingMigrations.Any())
            {
                logger.LogInformation($"Found {pendingMigrations.Count} pending migrations:");
                foreach (var migration in pendingMigrations)
                {
                    logger.LogInformation($"- {migration}");
                }
                logger.LogInformation("Applying migrations...");
                await dbContext.Database.MigrateAsync();
                
                logger.LogInformation("Migrations applied successfully!");
            }
            else
            {
                logger.LogInformation("No pending migrations found");
            }

            var appliedMigrations = (await dbContext.Database.GetAppliedMigrationsAsync()).ToList();
            logger.LogInformation($"Total applied migrations: {appliedMigrations.Count}");
            
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Migration failed: {Message}", ex.Message);
            throw;
        }
    }
    
    private static async Task WaitForDatabaseAsync(AppDbContext context, ILogger logger, int maxRetries = 30)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                await context.Database.OpenConnectionAsync();
                await context.Database.CloseConnectionAsync();
                logger.LogInformation($"Database is ready after {i + 1} attempts");
                return;
            }
            catch (Exception ex)
            {
                logger.LogWarning($"Database not ready (attempt {i + 1}/{maxRetries}): {ex.Message}");
                await Task.Delay(2000);
            }
        }
        
        throw new Exception("Database is not available after maximum retries");
    }
}