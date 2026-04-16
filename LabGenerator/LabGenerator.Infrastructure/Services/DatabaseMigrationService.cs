using LabGenerator.Infrastructure.Settings;
using LabGenerator.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LabGenerator.Infrastructure.Services;

public class DatabaseMigrationService(
    IServiceProvider serviceProvider,
    ILogger<DatabaseMigrationService> logger,
    IOptions<ApplicationSettings> applicationSettings
) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!applicationSettings.Value.PgAutoMigrations) return;

        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            logger.LogInformation("Initializing database schema (Migrate)...");
            await db.Database.MigrateAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Migrate failed. Falling back to EnsureCreated (no migrations or migrations unavailable).");
            await db.Database.EnsureCreatedAsync(cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}