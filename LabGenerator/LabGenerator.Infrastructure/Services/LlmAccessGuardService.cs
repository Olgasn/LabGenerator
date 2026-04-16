using LabGenerator.Domain.Entities;
using LabGenerator.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LabGenerator.Infrastructure.Services;

public sealed class LlmAccessGuardService(ApplicationDbContext db)
{
    public async Task<LlmAccessStatus> GetStatusAsync(CancellationToken cancellationToken)
    {
        var settings = await db.LlmSettings.AsNoTracking()
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var provider = string.IsNullOrWhiteSpace(settings?.Provider)
            ? "Ollama"
            : settings.Provider.Trim();

        var providerSettings = await db.LlmProviderSettings.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Provider == provider, cancellationToken);

        var hasApiKey = !string.IsNullOrWhiteSpace(providerSettings?.ApiKey);

        return new LlmAccessStatus(
            provider,
            hasApiKey,
            hasApiKey
                ? null
                : $"API key for provider '{provider}' is not configured in settings.");
    }

    public async Task EnsureConfiguredAsync(CancellationToken cancellationToken)
    {
        var status = await GetStatusAsync(cancellationToken);
        if (!status.HasApiKey)
        {
            throw new InvalidOperationException(status.Message ?? "LLM API key is not configured.");
        }
    }
}

public sealed record LlmAccessStatus(
    string Provider,
    bool HasApiKey,
    string? Message);
