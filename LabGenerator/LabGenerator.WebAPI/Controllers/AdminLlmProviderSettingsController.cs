using LabGenerator.Domain.Entities;
using LabGenerator.Infrastructure.Data;
using LabGenerator.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LabGenerator.WebAPI.Controllers;

[ApiController]
[Route("api/admin/llm-provider-settings")]
public sealed class AdminLlmProviderSettingsController(ApplicationDbContext db, IConfiguration configuration) : ControllerBase
{
    [HttpGet("{provider}")]
    public async Task<ActionResult<LlmProviderSettingsResponse>> Get([FromRoute] string provider, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            return BadRequest("Provider is required.");
        }

        var normalized = provider.Trim();

        var current = await db.Set<LlmProviderSettings>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.Provider == normalized, cancellationToken);

        if (current is not null)
        {
            return ToResponse(current);
        }

        var created = new LlmProviderSettings
        {
            Provider = normalized,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        if (string.Equals(normalized, "OpenRouter", StringComparison.OrdinalIgnoreCase))
        {
            created.Model = configuration["LLM:OpenRouter:Model"] ?? configuration["LLM__OpenRouter__Model"];
            if (double.TryParse(configuration["LLM:OpenRouter:Temperature"] ?? configuration["LLM__OpenRouter__Temperature"], out var t)) created.Temperature = t;
            if (int.TryParse(configuration["LLM:OpenRouter:MaxOutputTokens"] ?? configuration["LLM__OpenRouter__MaxOutputTokens"], out var m)) created.MaxOutputTokens = m;
        }
        else
        {
            created.Model = configuration["LLM:Ollama:Model"] ?? configuration["LLM__Ollama__Model"];
            if (double.TryParse(configuration["LLM:Ollama:Temperature"] ?? configuration["LLM__Ollama__Temperature"], out var t)) created.Temperature = t;
            if (int.TryParse(configuration["LLM:Ollama:MaxOutputTokens"] ?? configuration["LLM__Ollama__MaxOutputTokens"], out var m)) created.MaxOutputTokens = m;
        }

        db.Set<LlmProviderSettings>().Add(created);
        await db.SaveChangesAsync(cancellationToken);

        return ToResponse(created);
    }

    [HttpPut("{provider}")]
    public async Task<ActionResult<LlmProviderSettingsResponse>> Upsert(
        [FromRoute] string provider,
        [FromBody] UpsertLlmProviderSettingsRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            return BadRequest("Provider is required.");
        }

        var normalized = provider.Trim();

        if (!string.Equals(normalized, request.Provider?.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Provider mismatch.");
        }

        var current = await db.Set<LlmProviderSettings>()
            .FirstOrDefaultAsync(x => x.Provider == normalized, cancellationToken);

        if (current is null)
        {
            current = new LlmProviderSettings
            {
                Provider = normalized
            };
            db.Set<LlmProviderSettings>().Add(current);
        }

        current.Model = string.IsNullOrWhiteSpace(request.Model) ? null : request.Model.Trim();
        if (request.ClearApiKey)
        {
            current.ApiKey = null;
        }
        else if (!string.IsNullOrWhiteSpace(request.ApiKey))
        {
            current.ApiKey = request.ApiKey.Trim();
        }
        current.Temperature = request.Temperature;
        current.MaxOutputTokens = request.MaxOutputTokens;
        current.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return ToResponse(current);
    }

    private static LlmProviderSettingsResponse ToResponse(LlmProviderSettings settings)
    {
        return new LlmProviderSettingsResponse
        {
            Id = settings.Id,
            Provider = settings.Provider,
            Model = settings.Model,
            HasApiKey = !string.IsNullOrWhiteSpace(settings.ApiKey),
            ApiKeyMasked = MaskApiKey(settings.ApiKey),
            Temperature = settings.Temperature,
            MaxOutputTokens = settings.MaxOutputTokens,
            UpdatedAt = settings.UpdatedAt
        };
    }

    private static string? MaskApiKey(string? apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return null;
        }

        var trimmed = apiKey.Trim();
        if (trimmed.Length <= 6)
        {
            return new string('*', trimmed.Length);
        }

        return $"{trimmed[..2]}{new string('*', Math.Max(0, trimmed.Length - 4))}{trimmed[^2..]}";
    }
}
