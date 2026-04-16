using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using LabGenerator.Application.Abstractions;
using LabGenerator.Application.Models;
using LabGenerator.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LabGenerator.Infrastructure.Llm;

public sealed class GeminiLLMClient(
    HttpClient httpClient,
    IOptions<GeminiOptions> options,
    IOptions<ApplicationSettings> appSettings,
    ILogger<GeminiLLMClient> logger) : ILLMClient
{
    private readonly GeminiOptions _options = options.Value;
    private readonly ApplicationSettings _appSettings = appSettings.Value;

    public async Task<LLMCompletionResult> GenerateTextAsync(LLMCompletionRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException("Gemini ApiKey is not configured. Set LLM:Gemini:ApiKey in appsettings.");
        }

        var model = string.IsNullOrWhiteSpace(request.Model) ? _options.Model : request.Model;
        var temperature = request.Temperature ?? _options.Temperature;
        var maxOutputTokens = request.MaxOutputTokens ?? _options.MaxOutputTokens;

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_options.ApiKey}";
        var safeUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent";

        var requireJson = request.Purpose is "variant_generation" or "uniqueness_check" or "constraints_check" or "variant_judge";

        var generationConfig = new Dictionary<string, object?>
        {
            ["temperature"] = temperature,
            ["maxOutputTokens"] = maxOutputTokens
        };

        if (requireJson)
        {
            generationConfig["responseMimeType"] = "application/json";
        }

        var payload = new
        {
            systemInstruction = new
            {
                parts = new[] { new { text = request.SystemPrompt } }
            },
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[] { new { text = request.UserPrompt } }
                }
            },
            generationConfig
        };

        var sw = Stopwatch.StartNew();

        if (_appSettings.LogLlmRequests)
        {
            logger.LogInformation(
                "LLM request start provider={Provider} purpose={Purpose} model={Model} temperature={Temperature} maxOutputTokens={MaxOutputTokens} url={Url} systemPrompt={SystemPrompt} userPrompt={UserPrompt}",
                "Gemini",
                request.Purpose,
                model,
                temperature,
                maxOutputTokens,
                safeUrl,
                Truncate(request.SystemPrompt, _appSettings.LogLlmMaxChars),
                Truncate(request.UserPrompt, _appSettings.LogLlmMaxChars));
        }

        var attempt = 0;
        var maxAttempts = Math.Max(1, _appSettings.LlmRetryCount + 1);
        HttpResponseMessage? response = null;
        string raw = "";

        while (true)
        {
            attempt++;
            response?.Dispose();
            response = await httpClient.PostAsJsonAsync(url, payload, cancellationToken);
            raw = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                break;
            }

            if ((int)response.StatusCode == 429 && attempt < maxAttempts)
            {
                var retryDelay = TryParseRetryDelay(raw);
                var delay = retryDelay ?? TimeSpan.FromSeconds(Math.Min(_appSettings.LlmRetryMaxDelaySeconds, 5 * attempt));
                if (delay.TotalSeconds > _appSettings.LlmRetryMaxDelaySeconds)
                {
                    delay = TimeSpan.FromSeconds(_appSettings.LlmRetryMaxDelaySeconds);
                }

                logger.LogWarning(
                    "LLM quota/rate limit provider={Provider} purpose={Purpose} model={Model} statusCode={StatusCode}. Waiting {DelaySeconds}s before retry {Attempt}/{MaxAttempts}.",
                    "Gemini",
                    request.Purpose,
                    model,
                    (int)response.StatusCode,
                    (int)delay.TotalSeconds,
                    attempt,
                    maxAttempts);

                await Task.Delay(delay, cancellationToken);
                continue;
            }

            break;
        }

        sw.Stop();

        if (response is null || !response.IsSuccessStatusCode)
        {
            if (_appSettings.LogLlmRequests)
            {
                logger.LogWarning(
                    "LLM request failed provider={Provider} purpose={Purpose} model={Model} statusCode={StatusCode} reason={Reason} latencyMs={LatencyMs} body={Body}",
                    "Gemini",
                    request.Purpose,
                    model,
                    response is null ? -1 : (int)response.StatusCode,
                    response?.ReasonPhrase,
                    (int)sw.ElapsedMilliseconds,
                    Truncate(raw, _appSettings.LogLlmMaxChars));
            }
            throw new InvalidOperationException($"Gemini request failed: {(int)response.StatusCode} {response.ReasonPhrase}. Body: {raw}");
        }

        string text = "";

        try
        {
            using var doc = JsonDocument.Parse(raw);
            if (doc.RootElement.TryGetProperty("candidates", out var candidates)
                && candidates.ValueKind == JsonValueKind.Array
                && candidates.GetArrayLength() > 0)
            {
                var candidate0 = candidates[0];
                if (candidate0.TryGetProperty("content", out var content)
                    && content.TryGetProperty("parts", out var parts)
                    && parts.ValueKind == JsonValueKind.Array
                    && parts.GetArrayLength() > 0
                    && parts[0].TryGetProperty("text", out var textEl))
                {
                    text = textEl.GetString() ?? "";
                }
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
                "Gemini",
                request.Purpose,
                model,
                (int)sw.ElapsedMilliseconds,
                Truncate(text, _appSettings.LogLlmMaxChars));
        }

        int? promptTokens = null;
        int? completionTokens = null;
        int? totalTokens = null;

        try
        {
            using var doc = JsonDocument.Parse(raw);
            if (doc.RootElement.TryGetProperty("usageMetadata", out var usage))
            {
                if (usage.TryGetProperty("promptTokenCount", out var pt)) promptTokens = pt.GetInt32();
                if (usage.TryGetProperty("candidatesTokenCount", out var ct)) completionTokens = ct.GetInt32();
                if (usage.TryGetProperty("totalTokenCount", out var tt)) totalTokens = tt.GetInt32();
            }
        }
        catch
        {
        }

        return new LLMCompletionResult(
            Provider: "Gemini",
            Model: model,
            Text: text,
            PromptTokens: promptTokens,
            CompletionTokens: completionTokens,
            TotalTokens: totalTokens,
            LatencyMs: (int)sw.ElapsedMilliseconds,
            RawResponseJson: raw
        );
    }

    private static TimeSpan? TryParseRetryDelay(string rawJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(rawJson);
            if (!doc.RootElement.TryGetProperty("error", out var error)) return null;
            if (!error.TryGetProperty("details", out var details) || details.ValueKind != JsonValueKind.Array) return null;

            foreach (var d in details.EnumerateArray())
            {
                if (d.ValueKind != JsonValueKind.Object) continue;
                if (!d.TryGetProperty("@type", out var typeEl)) continue;
                var type = typeEl.GetString();
                if (type is null || !type.EndsWith("RetryInfo", StringComparison.OrdinalIgnoreCase)) continue;
                if (!d.TryGetProperty("retryDelay", out var delayEl)) continue;
                var s = delayEl.GetString();
                if (string.IsNullOrWhiteSpace(s)) continue;

                // Format like "34s"
                if (s.EndsWith("s", StringComparison.OrdinalIgnoreCase)
                    && int.TryParse(s.TrimEnd('s', 'S'), out var seconds))
                {
                    return TimeSpan.FromSeconds(seconds);
                }
            }
        }
        catch
        {
        }

        return null;
    }

    private static string Truncate(string? text, int maxChars)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        if (maxChars <= 0) return string.Empty;
        if (text.Length <= maxChars) return text;
        return text.Substring(0, maxChars) + "…";
    }
}