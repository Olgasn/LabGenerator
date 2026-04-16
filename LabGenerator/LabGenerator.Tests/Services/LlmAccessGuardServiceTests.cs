using LabGenerator.Domain.Entities;
using LabGenerator.Infrastructure.Services;
using LabGenerator.Tests.Helpers;

namespace LabGenerator.Tests.Services;

public sealed class LlmAccessGuardServiceTests
{
    [Fact]
    public async Task GetStatusAsync_ReturnsHasApiKeyFalse_WhenNoProviderSettings()
    {
        await using var db = TestDbContextFactory.Create();
        var svc = new LlmAccessGuardService(db);

        var status = await svc.GetStatusAsync(CancellationToken.None);

        Assert.False(status.HasApiKey);
        Assert.Equal("Ollama", status.Provider);
        Assert.NotNull(status.Message);
    }

    [Fact]
    public async Task GetStatusAsync_ReturnsHasApiKeyFalse_WhenProviderSettingsExistButKeyEmpty()
    {
        await using var db = TestDbContextFactory.Create();
        db.Set<LlmProviderSettings>().Add(new LlmProviderSettings
        {
            Provider = "Ollama",
            Model = "llama3",
            ApiKey = null
        });
        await db.SaveChangesAsync();

        var svc = new LlmAccessGuardService(db);
        var status = await svc.GetStatusAsync(CancellationToken.None);

        Assert.False(status.HasApiKey);
    }

    [Fact]
    public async Task GetStatusAsync_ReturnsHasApiKeyTrue_WhenApiKeyConfigured()
    {
        await using var db = TestDbContextFactory.Create();
        db.Set<LlmProviderSettings>().Add(new LlmProviderSettings
        {
            Provider = "Ollama",
            Model = "llama3",
            ApiKey = "sk-test-key-123"
        });
        await db.SaveChangesAsync();

        var svc = new LlmAccessGuardService(db);
        var status = await svc.GetStatusAsync(CancellationToken.None);

        Assert.True(status.HasApiKey);
        Assert.Null(status.Message);
    }

    [Fact]
    public async Task GetStatusAsync_UsesActiveProvider_FromLlmSettings()
    {
        await using var db = TestDbContextFactory.Create();

        db.LlmSettings.Add(new LlmSettings
        {
            Provider = "OpenRouter",
            Model = "deepseek-v3"
        });
        // Ollama has key, but OpenRouter does not
        db.Set<LlmProviderSettings>().Add(new LlmProviderSettings
        {
            Provider = "Ollama",
            ApiKey = "sk-ollama-key"
        });
        await db.SaveChangesAsync();

        var svc = new LlmAccessGuardService(db);
        var status = await svc.GetStatusAsync(CancellationToken.None);

        Assert.Equal("OpenRouter", status.Provider);
        Assert.False(status.HasApiKey);
    }

    [Fact]
    public async Task GetStatusAsync_FindsKeyForActiveProvider()
    {
        await using var db = TestDbContextFactory.Create();

        db.LlmSettings.Add(new LlmSettings
        {
            Provider = "OpenRouter",
            Model = "deepseek-v3"
        });
        db.Set<LlmProviderSettings>().Add(new LlmProviderSettings
        {
            Provider = "OpenRouter",
            ApiKey = "sk-or-key"
        });
        await db.SaveChangesAsync();

        var svc = new LlmAccessGuardService(db);
        var status = await svc.GetStatusAsync(CancellationToken.None);

        Assert.Equal("OpenRouter", status.Provider);
        Assert.True(status.HasApiKey);
    }

    [Fact]
    public async Task GetStatusAsync_DefaultsToOllama_WhenNoLlmSettings()
    {
        await using var db = TestDbContextFactory.Create();

        db.Set<LlmProviderSettings>().Add(new LlmProviderSettings
        {
            Provider = "Ollama",
            ApiKey = "key"
        });
        await db.SaveChangesAsync();

        var svc = new LlmAccessGuardService(db);
        var status = await svc.GetStatusAsync(CancellationToken.None);

        Assert.Equal("Ollama", status.Provider);
        Assert.True(status.HasApiKey);
    }

    [Fact]
    public async Task EnsureConfiguredAsync_DoesNotThrow_WhenApiKeyPresent()
    {
        await using var db = TestDbContextFactory.Create();
        db.Set<LlmProviderSettings>().Add(new LlmProviderSettings
        {
            Provider = "Ollama",
            ApiKey = "sk-key"
        });
        await db.SaveChangesAsync();

        var svc = new LlmAccessGuardService(db);

        await svc.EnsureConfiguredAsync(CancellationToken.None);
    }

    [Fact]
    public async Task EnsureConfiguredAsync_Throws_WhenApiKeyMissing()
    {
        await using var db = TestDbContextFactory.Create();
        var svc = new LlmAccessGuardService(db);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => svc.EnsureConfiguredAsync(CancellationToken.None));

        Assert.Contains("API key", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
