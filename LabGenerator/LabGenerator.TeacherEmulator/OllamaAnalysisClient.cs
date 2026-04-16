using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace LabGenerator.TeacherEmulator;

public sealed class OllamaAnalysisClient(HttpClient httpClient, string model, string? apiKey, LlmProvider provider = LlmProvider.Ollama)
{
    public Task<string> GenerateJsonAsync(
        string systemPrompt,
        string userPrompt,
        double temperature,
        int maxTokens,
        CancellationToken ct)
    {
        return provider == LlmProvider.OpenRouter
            ? GenerateOpenRouterAsync(systemPrompt, userPrompt, temperature, maxTokens, ct)
            : GenerateOllamaAsync(systemPrompt, userPrompt, temperature, maxTokens, ct);
    }

    private async Task<string> GenerateOllamaAsync(
        string systemPrompt,
        string userPrompt,
        double temperature,
        int maxTokens,
        CancellationToken ct)
    {
        var payload = new Dictionary<string, object?>
        {
            ["model"] = model,
            ["prompt"] = $"SYSTEM:\n{systemPrompt}\n\nUSER:\n{userPrompt}",
            ["stream"] = false,
            ["format"] = "json",
            ["options"] = new
            {
                temperature,
                num_predict = maxTokens
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, ResolveGeneratePath(httpClient.BaseAddress));
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        }

        request.Content = JsonContent.Create(payload);
        using var response = await httpClient.SendAsync(request, ct);
        var raw = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            if ((int)response.StatusCode == 401)
            {
                throw new InvalidOperationException(
                    "LLM call failed: 401 Unauthorized. " +
                    "Set OLLAMA_API_KEY (or LLM__Ollama__ApiKey) with a valid key.");
            }

            throw new InvalidOperationException(
                $"LLM call failed: {(int)response.StatusCode} {response.ReasonPhrase}. Body: {Trim(raw, 2000)}");
        }

        return ExtractTextFromOllamaResponse(raw);
    }

    private async Task<string> GenerateOpenRouterAsync(
        string systemPrompt,
        string userPrompt,
        double temperature,
        int maxTokens,
        CancellationToken ct)
    {
        var messages = new object[]
        {
            new { role = "system", content = systemPrompt },
            new { role = "user", content = userPrompt }
        };

        var payload = new Dictionary<string, object?>
        {
            ["model"] = model,
            ["messages"] = messages,
            ["temperature"] = temperature,
            ["max_tokens"] = maxTokens,
            ["response_format"] = new { type = "json_object" }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, ResolveChatCompletionsPath(httpClient.BaseAddress));
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        }

        request.Headers.TryAddWithoutValidation("X-Title", "LabGenerator TeacherEmulator");
        request.Content = JsonContent.Create(payload);
        using var response = await httpClient.SendAsync(request, ct);
        var raw = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            if ((int)response.StatusCode == 401)
            {
                throw new InvalidOperationException(
                    "LLM call failed: 401 Unauthorized. " +
                    "Set OLLAMA_API_KEY (or OPENROUTER_API_KEY) with a valid OpenRouter key.");
            }

            throw new InvalidOperationException(
                $"LLM call failed: {(int)response.StatusCode} {response.ReasonPhrase}. Body: {Trim(raw, 2000)}");
        }

        return ExtractTextFromOpenRouterResponse(raw);
    }

    private static string ResolveGeneratePath(Uri? baseAddress)
    {
        if (baseAddress is null)
        {
            return "/api/generate";
        }

        var path = baseAddress.AbsolutePath.TrimEnd('/');
        return path.EndsWith("/api", StringComparison.OrdinalIgnoreCase)
            ? "generate"
            : "api/generate";
    }

    private static string ResolveChatCompletionsPath(Uri? baseAddress)
    {
        if (baseAddress is null)
        {
            return "/api/v1/chat/completions";
        }

        var path = baseAddress.AbsolutePath.TrimEnd('/');
        if (path.EndsWith("/v1", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith("/api/v1", StringComparison.OrdinalIgnoreCase))
        {
            return "chat/completions";
        }

        if (path.EndsWith("/api", StringComparison.OrdinalIgnoreCase))
        {
            return "v1/chat/completions";
        }

        return "api/v1/chat/completions";
    }

    private static string ExtractTextFromOpenRouterResponse(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        try
        {
            using var doc = JsonDocument.Parse(raw);
            if (doc.RootElement.TryGetProperty("choices", out var choices) &&
                choices.ValueKind == JsonValueKind.Array &&
                choices.GetArrayLength() > 0 &&
                choices[0].TryGetProperty("message", out var message) &&
                message.TryGetProperty("content", out var content) &&
                content.ValueKind == JsonValueKind.String)
            {
                return content.GetString() ?? string.Empty;
            }
        }
        catch
        {
            // fall through
        }

        return raw.Trim();
    }

    private static string ExtractTextFromOllamaResponse(string raw)
    {
        var trimmed = raw.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return string.Empty;
        }

        if (trimmed.Contains('\n'))
        {
            var parts = new List<string>();
            foreach (var line in trimmed.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (!TryParseJson(line, out var doc))
                {
                    continue;
                }

                using (doc)
                {
                    if (doc.RootElement.TryGetProperty("response", out var responseEl) &&
                        responseEl.ValueKind == JsonValueKind.String)
                    {
                        parts.Add(responseEl.GetString() ?? string.Empty);
                    }
                }
            }

            if (parts.Count > 0)
            {
                return string.Concat(parts);
            }
        }

        if (TryParseJson(trimmed, out var singleDoc))
        {
            using (singleDoc)
            {
                if (singleDoc.RootElement.TryGetProperty("response", out var responseEl) &&
                    responseEl.ValueKind == JsonValueKind.String)
                {
                    return responseEl.GetString() ?? string.Empty;
                }
            }
        }

        return trimmed;
    }

    private static bool TryParseJson(string json, out JsonDocument document)
    {
        try
        {
            document = JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            document = null!;
            return false;
        }
    }

    private static string Trim(string text, int maxChars)
    {
        if (text.Length <= maxChars)
        {
            return text;
        }

        return new StringBuilder(text, 0, maxChars, maxChars + 16).Append("...").ToString();
    }
}

