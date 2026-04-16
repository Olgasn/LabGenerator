using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using LabGenerator.Application.Models;
using LabGenerator.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TextGeneration;

namespace LabGenerator.Infrastructure.Llm;

public sealed class OllamaTextGenerationService(
    HttpClient httpClient,
    IOptions<OllamaOptions> options,
    IOptions<ApplicationSettings> appSettings,
    ILogger<OllamaTextGenerationService> logger) : ITextGenerationService
{
    private readonly OllamaOptions _options = options.Value;
    private readonly ApplicationSettings _appSettings = appSettings.Value;

    public async Task<IReadOnlyList<TextContent>> GetTextContentsAsync(
        string prompt,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        var model = _options.Model;

        var temperature = _options.Temperature;
        var maxTokens = _options.MaxOutputTokens;
        string? responseFormat = null;

        if (executionSettings is not null)
        {
            if (executionSettings.ExtensionData is not null)
            {
                if (executionSettings.ExtensionData.TryGetValue("temperature", out var tObj)
                    && tObj is not null
                    && double.TryParse(tObj.ToString(), out var tParsed))
                {
                    temperature = tParsed;
                }

                if (executionSettings.ExtensionData.TryGetValue("max_tokens", out var mtObj)
                    && mtObj is not null
                    && int.TryParse(mtObj.ToString(), out var mtParsed))
                {
                    maxTokens = mtParsed;
                }

                if (executionSettings.ExtensionData.TryGetValue("format", out var fmtObj)
                    && fmtObj is not null)
                {
                    var fmt = fmtObj.ToString();
                    if (!string.IsNullOrWhiteSpace(fmt)) responseFormat = fmt;
                }
            }

            if (!string.IsNullOrWhiteSpace(executionSettings.ModelId))
            {
                model = executionSettings.ModelId;
            }
        }

        var baseUrl = _options.BaseUrl?.TrimEnd('/') ?? "https://ollama.com";
        var url = baseUrl.EndsWith("/api", StringComparison.OrdinalIgnoreCase)
            ? baseUrl + "/generate"
            : baseUrl + "/api/generate";

        var payload = new Dictionary<string, object?>
        {
            ["model"] = model,
            ["prompt"] = prompt,
            ["stream"] = false,
            ["options"] = new
            {
                temperature,
                num_predict = maxTokens
            }
        };

        if (!string.IsNullOrWhiteSpace(responseFormat))
        {
            payload["format"] = responseFormat;
        }

        var sw = Stopwatch.StartNew();
        var maxAttempts = Math.Max(1, _appSettings.LlmRetryCount + 1);
        var attempt = 0;
        string raw;
        string? contentType;

        if (_appSettings.LogLlmRequests)
        {
            logger.LogInformation(
                "LLM request start provider={Provider} model={Model} url={Url} temperature={Temperature} maxTokens={MaxTokens} prompt={Prompt}",
                "Ollama",
                model,
                url,
                temperature,
                maxTokens,
                Truncate(prompt, _appSettings.LogLlmMaxChars));
        }

        while (true)
        {
            attempt++;

            try
            {
                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);

                if (!string.IsNullOrWhiteSpace(_options.ApiKey))
                {
                    httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiKey);
                }

                httpRequest.Content = JsonContent.Create(payload);

                using var response = await httpClient.SendAsync(httpRequest, cancellationToken);
                raw = await response.Content.ReadAsStringAsync(cancellationToken);
                contentType = response.Content.Headers.ContentType?.ToString() ?? string.Empty;

                if (!response.IsSuccessStatusCode)
                {
                    if (attempt < maxAttempts && IsTransientStatusCode(response.StatusCode))
                    {
                        var delay = CalculateRetryDelay(attempt);

                        logger.LogWarning(
                            "LLM request transient failure provider={Provider} model={Model} attempt={Attempt}/{MaxAttempts} statusCode={StatusCode}. Retrying in {DelaySeconds}s. BodyPreview={BodyPreview}",
                            "Ollama",
                            model,
                            attempt,
                            maxAttempts,
                            (int)response.StatusCode,
                            (int)delay.TotalSeconds,
                            Truncate(EscapeForSingleLine(raw), _appSettings.LogLlmMaxChars));

                        await Task.Delay(delay, cancellationToken);
                        continue;
                    }

                    if (_appSettings.LogLlmRequests)
                    {
                        logger.LogWarning(
                            "LLM request failed provider={Provider} model={Model} statusCode={StatusCode} reason={Reason} latencyMs={LatencyMs} bodyPreview={BodyPreview}",
                            "Ollama",
                            model,
                            (int)response.StatusCode,
                            response.ReasonPhrase,
                            (int)sw.ElapsedMilliseconds,
                            Truncate(EscapeForSingleLine(raw), _appSettings.LogLlmMaxChars));
                    }

                    throw new InvalidOperationException($"Ollama request failed: {(int)response.StatusCode} {response.ReasonPhrase}. Body: {raw}");
                }

                sw.Stop();

                var (text, error) = ExtractResponseAndError(raw);

                if (!string.IsNullOrWhiteSpace(error))
                {
                    throw new InvalidOperationException($"Ollama error: {error}");
                }

                if (string.IsNullOrWhiteSpace(text) && _appSettings.LogLlmRequests)
                {
                    logger.LogWarning(
                        "LLM request success but empty responseText provider={Provider} model={Model} latencyMs={LatencyMs} contentType={ContentType} rawPreview={RawPreview}",
                        "Ollama",
                        model,
                        (int)sw.ElapsedMilliseconds,
                        contentType,
                        Truncate(EscapeForSingleLine(raw), _appSettings.LogLlmMaxChars));
                }

                if (_appSettings.LogLlmRequests)
                {
                    logger.LogInformation(
                        "LLM request success provider={Provider} model={Model} latencyMs={LatencyMs} responseText={ResponseText}",
                        "Ollama",
                        model,
                        (int)sw.ElapsedMilliseconds,
                        Truncate(text, _appSettings.LogLlmMaxChars));
                }

                return new[]
                {
                    new TextContent(text)
                };
            }
            catch (HttpRequestException ex) when (attempt < maxAttempts)
            {
                var delay = CalculateRetryDelay(attempt);

                logger.LogWarning(
                    ex,
                    "LLM transport error provider={Provider} model={Model} attempt={Attempt}/{MaxAttempts}. Retrying in {DelaySeconds}s.",
                    "Ollama",
                    model,
                    attempt,
                    maxAttempts,
                    (int)delay.TotalSeconds);

                await Task.Delay(delay, cancellationToken);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested && attempt < maxAttempts)
            {
                var delay = CalculateRetryDelay(attempt);

                logger.LogWarning(
                    "LLM request timeout provider={Provider} model={Model} attempt={Attempt}/{MaxAttempts}. Retrying in {DelaySeconds}s.",
                    "Ollama",
                    model,
                    attempt,
                    maxAttempts,
                    (int)delay.TotalSeconds);

                await Task.Delay(delay, cancellationToken);
            }
        }

    }

    private TimeSpan CalculateRetryDelay(int attempt)
    {
        var maxDelay = Math.Max(1, _appSettings.LlmRetryMaxDelaySeconds);
        var seconds = Math.Min(maxDelay, Math.Max(1, attempt * 2));
        return TimeSpan.FromSeconds(seconds);
    }

    private static bool IsTransientStatusCode(HttpStatusCode statusCode)
    {
        var code = (int)statusCode;
        return code is 408 or 429 or 500 or 502 or 503 or 504;
    }

    public async IAsyncEnumerable<StreamingTextContent> GetStreamingTextContentsAsync(
        string prompt,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var contents = await GetTextContentsAsync(prompt, executionSettings, kernel, cancellationToken);
        var text = contents.FirstOrDefault()?.Text ?? string.Empty;
        yield return new StreamingTextContent(text);
    }

    public IReadOnlyDictionary<string, object?> Attributes { get; } = new Dictionary<string, object?>();

    private static string EscapeForSingleLine(string? text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        return text
            .Replace("\\", "\\\\")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n")
            .Replace("\t", "\\t");
    }

    private static (string Text, string Error) ExtractResponseAndError(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return (string.Empty, string.Empty);

        var trimmed = raw.Trim();

        if (trimmed.Contains("\n") &&
            (trimmed.Contains("\n{") ||
             trimmed.StartsWith("{\"model\"", StringComparison.Ordinal)))
        {
            return ExtractFromNdjson(trimmed);
        }

        try
        {
            using var doc = JsonDocument.Parse(trimmed);
            var root = doc.RootElement;

            if (root.TryGetProperty("error", out var errEl) &&
                errEl.ValueKind == JsonValueKind.String)
            {
                var e = errEl.GetString() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(e))
                    return (string.Empty, e);
            }

            var response = GetStringProperty(root, "response");

            if (string.IsNullOrWhiteSpace(response))
            {
                var thinking = GetStringProperty(root, "thinking");
                if (!string.IsNullOrWhiteSpace(thinking))
                {
                    var extracted = TryExtractJsonFromThinking(thinking);
                    if (!string.IsNullOrWhiteSpace(extracted))
                        return (extracted, string.Empty);
                }
            }

            if (!string.IsNullOrWhiteSpace(response))
                return (response, string.Empty);

            return (string.Empty, string.Empty);
        }
        catch
        {
            return (string.Empty, string.Empty);
        }
    }
    
    private static string GetStringProperty(JsonElement root, string name)
    {
        return root.TryGetProperty(name, out var el) &&
               el.ValueKind == JsonValueKind.String
            ? el.GetString() ?? string.Empty
            : string.Empty;
    }

    private static (string Text, string Error) ExtractFromNdjson(string trimmed)
    {
        var parts = new List<string>();
        var thinkingParts = new List<string>();

        foreach (var line in trimmed.Split('\n',
                     StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            try
            {
                using var doc = JsonDocument.Parse(line);

                if (doc.RootElement.TryGetProperty("error", out var errEl) &&
                    errEl.ValueKind == JsonValueKind.String)
                {
                    var e = errEl.GetString() ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(e))
                        return (string.Empty, e);
                }

                var resp = GetStringProperty(doc.RootElement, "response");
                if (!string.IsNullOrEmpty(resp))
                    parts.Add(resp);

                var think = GetStringProperty(doc.RootElement, "thinking");
                if (!string.IsNullOrEmpty(think))
                    thinkingParts.Add(think);

                if (doc.RootElement.TryGetProperty("done", out var doneEl) &&
                    doneEl.ValueKind == JsonValueKind.True)
                    break;
            }
            catch { continue; }
        }

        var responseText = string.Join("", parts);
        if (!string.IsNullOrWhiteSpace(responseText))
            return (responseText, string.Empty);

        var fullThinking = string.Join("", thinkingParts);
        if (!string.IsNullOrWhiteSpace(fullThinking))
        {
            var extracted = TryExtractJsonFromThinking(fullThinking);
            if (!string.IsNullOrWhiteSpace(extracted))
                return (extracted, string.Empty);
        }

        return (string.Empty, string.Empty);
    }
    
    private static string TryExtractJsonFromThinking(string thinking)
    {
        if (string.IsNullOrWhiteSpace(thinking))
            return string.Empty;

        var unescaped = thinking
            .Replace("\\n", "\n")
            .Replace("\\t", "\t")
            .Replace("\\\"", "\"");

        var candidates = new List<string>();
        var depth = 0;
        var start = -1;

        for (var i = 0; i < unescaped.Length; i++)
        {
            switch (unescaped[i])
            {
                case '{':
                    if (depth == 0) start = i;
                    depth++;
                    break;
                case '}' when depth > 0:
                    depth--;
                    if (depth == 0 && start >= 0)
                    {
                        var candidate = unescaped[start..(i + 1)];
                        try
                        {
                            using var doc = JsonDocument.Parse(candidate);
                            if (doc.RootElement.ValueKind == JsonValueKind.Object)
                                candidates.Add(candidate);
                        }
                        catch
                        {
                        }
                    }
                    break;
            }
        }

        if (candidates.Count == 0)
            return string.Empty;

        static bool HasVariantShape(JsonElement root)
            => root.TryGetProperty("content_markdown", out _);

        static bool HasJudgeShape(JsonElement root)
            => root.TryGetProperty("passed", out _)
               && root.TryGetProperty("score", out _)
               && root.TryGetProperty("issues", out _);

        static bool HasUniquenessShape(JsonElement root)
            => root.TryGetProperty("is_unique", out _);

        string? FirstMatch(Func<JsonElement, bool> predicate)
        {
            foreach (var c in candidates)
            {
                try
                {
                    using var doc = JsonDocument.Parse(c);
                    if (doc.RootElement.ValueKind != JsonValueKind.Object) continue;
                    if (predicate(doc.RootElement)) return c;
                }
                catch
                {
                }
            }

            return null;
        }
        
        var best =
            FirstMatch(HasVariantShape)
            ?? FirstMatch(HasJudgeShape)
            ?? FirstMatch(HasUniquenessShape);

        if (best is not null)
            return best;

        return candidates.OrderByDescending(c => c.Length).First();
    }

    private static string Truncate(string? text, int maxChars)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        if (maxChars <= 0) return string.Empty;
        if (text.Length <= maxChars) return text;
        return text.Substring(0, maxChars) + "…";
    }
}
