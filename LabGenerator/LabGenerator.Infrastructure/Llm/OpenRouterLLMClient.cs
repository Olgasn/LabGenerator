using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Net.Http.Json;
using LabGenerator.Application.Abstractions;
using LabGenerator.Application.Models;
using LabGenerator.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LabGenerator.Infrastructure.Llm;

public sealed class OpenRouterLLMClient(
    HttpClient httpClient,
    IOptions<OpenRouterOptions> options,
    IOptions<ApplicationSettings> appSettings,
    ILogger<OpenRouterLLMClient> logger) : ILLMClient
{
    private readonly OpenRouterOptions _options = options.Value;
    private readonly ApplicationSettings _appSettings = appSettings.Value;

    public async Task<LLMCompletionResult> GenerateTextAsync(LLMCompletionRequest request, CancellationToken cancellationToken)
    {
        return await GenerateTextInternalAsync(
            request,
            baseUrlOverride: null,
            apiKeyOverride: null,
            modelOverride: null,
            temperatureOverride: null,
            maxOutputTokensOverride: null,
            cancellationToken);
    }

    public async Task<LLMCompletionResult> GenerateTextAsync(
        LLMCompletionRequest request,
        string? baseUrlOverride,
        string? apiKeyOverride,
        string? modelOverride,
        double? temperatureOverride,
        int? maxOutputTokensOverride,
        CancellationToken cancellationToken)
    {
        return await GenerateTextInternalAsync(
            request,
            baseUrlOverride,
            apiKeyOverride,
            modelOverride,
            temperatureOverride,
            maxOutputTokensOverride,
            cancellationToken);
    }

    private async Task<LLMCompletionResult> GenerateTextInternalAsync(
        LLMCompletionRequest request,
        string? baseUrlOverride,
        string? apiKeyOverride,
        string? modelOverride,
        double? temperatureOverride,
        int? maxOutputTokensOverride,
        CancellationToken cancellationToken)
    {
        var apiKey = string.IsNullOrWhiteSpace(apiKeyOverride) ? _options.ApiKey : apiKeyOverride;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("OpenRouter ApiKey is not configured. Set LLM:OpenRouter:ApiKey in appsettings/env.");
        }

        var model = string.IsNullOrWhiteSpace(request.Model)
            ? (string.IsNullOrWhiteSpace(modelOverride) ? _options.Model : modelOverride)
            : request.Model;

        var temperature = request.Temperature
                          ?? temperatureOverride
                          ?? _options.Temperature;

        var maxOutputTokens = request.MaxOutputTokens
                             ?? maxOutputTokensOverride
                             ?? _options.MaxOutputTokens;

        var baseUrl = (string.IsNullOrWhiteSpace(baseUrlOverride) ? (_options.BaseUrl ?? "https://openrouter.ai/api/v1") : baseUrlOverride)
            .TrimEnd('/');
        var url = baseUrl + "/chat/completions";

        var requireJson = request.Purpose is "variant_generation" or "uniqueness_check" or "constraints_check" or "variant_judge";

        var payload = new Dictionary<string, object?>
        {
            ["model"] = model,
            ["temperature"] = temperature,
            ["max_tokens"] = maxOutputTokens,
            ["messages"] = new object[]
            {
                new { role = "system", content = request.SystemPrompt },
                new { role = "user", content = request.UserPrompt }
            }
        };

        if (requireJson)
        {
            payload["response_format"] = new { type = "json_object" };
        }

        var safeUrl = baseUrl + "/chat/completions";

        var sw = Stopwatch.StartNew();

        if (_appSettings.LogLlmRequests)
        {
            logger.LogInformation(
                "LLM request start provider={Provider} purpose={Purpose} model={Model} temperature={Temperature} maxOutputTokens={MaxOutputTokens} url={Url} systemPrompt={SystemPrompt} userPrompt={UserPrompt}",
                "OpenRouter",
                request.Purpose,
                model,
                temperature,
                maxOutputTokens,
                safeUrl,
                Truncate(request.SystemPrompt, _appSettings.LogLlmMaxChars),
                Truncate(request.UserPrompt, _appSettings.LogLlmMaxChars));
        }

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        httpRequest.Content = JsonContent.Create(payload);

        using var response = await httpClient.SendAsync(httpRequest, cancellationToken);
        var raw = await response.Content.ReadAsStringAsync(cancellationToken);

        sw.Stop();

        if (!response.IsSuccessStatusCode)
        {
            if (_appSettings.LogLlmRequests)
            {
                logger.LogWarning(
                    "LLM request failed provider={Provider} purpose={Purpose} model={Model} statusCode={StatusCode} reason={Reason} latencyMs={LatencyMs} body={Body}",
                    "OpenRouter",
                    request.Purpose,
                    model,
                    (int)response.StatusCode,
                    response.ReasonPhrase,
                    (int)sw.ElapsedMilliseconds,
                    Truncate(raw, _appSettings.LogLlmMaxChars));
            }

            throw new InvalidOperationException($"OpenRouter request failed: {(int)response.StatusCode} {response.ReasonPhrase}. Body: {raw}");
        }

        string text = "";
        int? promptTokens = null;
        int? completionTokens = null;
        int? totalTokens = null;

        try
        {
            using var doc = JsonDocument.Parse(raw);
            if (doc.RootElement.TryGetProperty("choices", out var choices)
                && choices.ValueKind == JsonValueKind.Array
                && choices.GetArrayLength() > 0)
            {
                var c0 = choices[0];
                if (c0.TryGetProperty("message", out var msg)
                    && msg.TryGetProperty("content", out var contentEl)
                    && contentEl.ValueKind == JsonValueKind.String)
                {
                    text = contentEl.GetString() ?? "";
                }
            }

            if (doc.RootElement.TryGetProperty("usage", out var usage) && usage.ValueKind == JsonValueKind.Object)
            {
                if (usage.TryGetProperty("prompt_tokens", out var pt) && pt.ValueKind == JsonValueKind.Number) promptTokens = pt.GetInt32();
                if (usage.TryGetProperty("completion_tokens", out var ct) && ct.ValueKind == JsonValueKind.Number) completionTokens = ct.GetInt32();
                if (usage.TryGetProperty("total_tokens", out var tt) && tt.ValueKind == JsonValueKind.Number) totalTokens = tt.GetInt32();
            }
        }
        catch
        {
            text = "";
        }

        if (_appSettings.LogLlmRequests)
        {
            logger.LogInformation(
                "LLM request success provider={Provider} purpose={Purpose} model={Model} latencyMs={LatencyMs} responseText={ResponseText}",
                "OpenRouter",
                request.Purpose,
                model,
                (int)sw.ElapsedMilliseconds,
                Truncate(text, _appSettings.LogLlmMaxChars));
        }

        return new LLMCompletionResult(
            Provider: "OpenRouter",
            Model: model,
            Text: text,
            PromptTokens: promptTokens,
            CompletionTokens: completionTokens,
            TotalTokens: totalTokens,
            LatencyMs: (int)sw.ElapsedMilliseconds,
            RawResponseJson: raw);
    }

    private static string Truncate(string? text, int maxChars)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        if (maxChars <= 0) return string.Empty;
        if (text.Length <= maxChars) return text;
        return text.Substring(0, maxChars) + "…";
    }
}
