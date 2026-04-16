using LabGenerator.Domain.Entities;
using LabGenerator.Infrastructure.Data;
using LabGenerator.Infrastructure.Services;
using LabGenerator.Infrastructure.Settings;
using LabGenerator.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LabGenerator.Tests.Services;

public sealed class DatabaseMigrationServiceTests
{
    [Fact]
    public async Task StartAsync_DoesNothing_WhenAutoMigrationsDisabled()
    {
        using var provider = new ServiceCollection().BuildServiceProvider();
        var logger = new ListLogger<DatabaseMigrationService>();
        var settings = Options.Create(new ApplicationSettings { PgAutoMigrations = false });
        var service = new DatabaseMigrationService(provider, logger, settings);

        await service.StartAsync(CancellationToken.None);

        Assert.Empty(logger.Entries);
    }

    [Fact]
    public async Task StartAsync_LogsWarningAndFallsBack_WhenMigrateFails()
    {
        using var provider = new ServiceCollection()
            .AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(Guid.NewGuid().ToString("N")))
            .BuildServiceProvider();

        var logger = new ListLogger<DatabaseMigrationService>();
        var settings = Options.Create(new ApplicationSettings { PgAutoMigrations = true });
        var service = new DatabaseMigrationService(provider, logger, settings);

        await service.StartAsync(CancellationToken.None);

        Assert.Contains(
            logger.Entries,
            entry => entry.LogLevel == LogLevel.Information
                     && entry.Message.Contains("Initializing database schema", StringComparison.Ordinal));

        Assert.Contains(
            logger.Entries,
            entry => entry.LogLevel == LogLevel.Warning
                     && entry.Message.Contains("Migrate failed", StringComparison.Ordinal)
                     && entry.Exception is not null);

        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Disciplines.Add(new Discipline { Name = "Test discipline" });
        await db.SaveChangesAsync();

        var count = await db.Disciplines.CountAsync();
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task StopAsync_CompletesImmediately()
    {
        using var provider = new ServiceCollection().BuildServiceProvider();
        var logger = new ListLogger<DatabaseMigrationService>();
        var settings = Options.Create(new ApplicationSettings());
        var service = new DatabaseMigrationService(provider, logger, settings);

        var task = service.StopAsync(CancellationToken.None);
        Assert.True(task.IsCompletedSuccessfully);

        await task;
    }
}
