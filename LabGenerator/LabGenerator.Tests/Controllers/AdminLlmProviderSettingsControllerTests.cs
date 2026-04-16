using LabGenerator.Domain.Entities;
using LabGenerator.Infrastructure.Data;
using LabGenerator.Tests.Helpers;
using LabGenerator.WebAPI.Controllers;
using LabGenerator.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace LabGenerator.Tests.Controllers;

public sealed class AdminLlmProviderSettingsControllerTests
{
    [Fact]
    public async Task Get_ReturnsMaskedKeyAndPresenceFlag()
    {
        await using var db = TestDbContextFactory.Create();
        db.LlmProviderSettings.Add(new LlmProviderSettings
        {
            Provider = "OpenRouter",
            Model = "model-a",
            ApiKey = "secret-123456",
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        var controller = new AdminLlmProviderSettingsController(db, CreateConfiguration());
        var result = await controller.Get("OpenRouter", CancellationToken.None);

        var response = Assert.IsType<LlmProviderSettingsResponse>(result.Value);
        Assert.True(response.HasApiKey);
        Assert.NotNull(response.ApiKeyMasked);
        Assert.DoesNotContain("secret-123456", response.ApiKeyMasked!, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Upsert_SavesApiKeyWithoutReturningRawValue()
    {
        await using var db = TestDbContextFactory.Create();
        var controller = new AdminLlmProviderSettingsController(db, CreateConfiguration());

        var result = await controller.Upsert(
            "Ollama",
            new UpsertLlmProviderSettingsRequest
            {
                Provider = "Ollama",
                Model = "model-a",
                ApiKey = "very-secret-key",
                Temperature = 0.2,
                MaxOutputTokens = 1024
            },
            CancellationToken.None);

        var response = Assert.IsType<LlmProviderSettingsResponse>(result.Value);
        Assert.True(response.HasApiKey);
        Assert.NotEqual("very-secret-key", response.ApiKeyMasked);

        var saved = Assert.Single(db.LlmProviderSettings);
        Assert.Equal("very-secret-key", saved.ApiKey);
    }

    private static IConfiguration CreateConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();
    }
}
