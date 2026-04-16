using LabGenerator.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LabGenerator.Tests.Helpers;

internal static class TestDbContextFactory
{
    public static ApplicationDbContext Create(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString("N"))
            .Options;

        return new ApplicationDbContext(options);
    }
}
