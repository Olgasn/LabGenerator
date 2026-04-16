using LabGenerator.Application.Abstractions;
using LabGenerator.Application.Models;
using LabGenerator.Domain.Entities;
using LabGenerator.Infrastructure.Data;
using LabGenerator.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace LabGenerator.Infrastructure.Llm;

public sealed class RoutingLLMClient(
    ApplicationDbContext db,
    LlmAccessGuardService llmAccessGuard,
    SemanticKernelLLMClient ollamaClient,
    OpenRouterLLMClient openRouterClient) : ILLMClient
{
    public async Task<LLMCompletionResult> GenerateTextAsync(LLMCompletionRequest request, CancellationToken cancellationToken)
    {
        await llmAccessGuard.EnsureConfiguredAsync(cancellationToken);

        var (provider, activeModelOverride) = await GetActiveSettingsAsync(cancellationToken);
        var providerSettings = await GetProviderSettingsAsync(provider, cancellationToken);

        var modelOverride = string.IsNullOrWhiteSpace(activeModelOverride)
            ? providerSettings?.Model
            : activeModelOverride;

        var effectiveRequest = request;
        if (!string.IsNullOrWhiteSpace(modelOverride) && string.IsNullOrWhiteSpace(request.Model))
        {
            effectiveRequest = request with { Model = modelOverride };
        }

        var temperatureOverride = providerSettings?.Temperature;
        var maxOutputTokensOverride = providerSettings?.MaxOutputTokens;

        return provider.ToUpperInvariant() switch
        {
            "OPENROUTER" => await openRouterClient.GenerateTextAsync(
                effectiveRequest,
                baseUrlOverride: null,
                apiKeyOverride: providerSettings?.ApiKey,
                modelOverride: null,
                temperatureOverride,
                maxOutputTokensOverride,
                cancellationToken),
            _ => await ollamaClient.GenerateTextAsync(
                effectiveRequest,
                baseUrlOverride: null,
                apiKeyOverride: providerSettings?.ApiKey,
                modelOverride: null,
                temperatureOverride,
                maxOutputTokensOverride,
                cancellationToken)
        };
    }
    private async Task<(string Provider, string? Model)> GetActiveSettingsAsync(CancellationToken ct)
    {
        var settings = await db.LlmSettings.AsNoTracking()
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync(ct);

        if (settings is not null)
        {
            return (settings.Provider?.Trim() ?? "Ollama", string.IsNullOrWhiteSpace(settings.Model) ? null : settings.Model.Trim());
        }
        
        return ("Ollama", null);
    }

    private async Task<LlmProviderSettings?> GetProviderSettingsAsync(string provider, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            provider = "Ollama";
        }

        var normalized = provider.Trim();

        return await db.LlmProviderSettings.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Provider == normalized, ct);
    }
}
