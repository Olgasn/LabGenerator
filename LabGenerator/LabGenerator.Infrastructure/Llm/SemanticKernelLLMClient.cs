using System.Diagnostics;
using LabGenerator.Application.Abstractions;
using LabGenerator.Application.Models;
using LabGenerator.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TextGeneration;

namespace LabGenerator.Infrastructure.Llm;

public sealed class SemanticKernelLLMClient(
    ITextGenerationService textGeneration,
    IOptions<OllamaOptions> options,
    IOptions<ApplicationSettings> appSettings,
    ILogger<SemanticKernelLLMClient> logger) : ILLMClient
{
    private readonly OllamaOptions _options = options.Value;
    private readonly ApplicationSettings _appSettings = appSettings.Value;

    public async Task<LLMCompletionResult> GenerateTextAsync(LLMCompletionRequest request, CancellationToken cancellationToken)
    {
        return await GenerateTextAsync(
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
        var model = string.IsNullOrWhiteSpace(request.Model)
            ? (string.IsNullOrWhiteSpace(modelOverride) ? _options.Model : modelOverride)
            : request.Model;

        var temperature = request.Temperature
                          ?? temperatureOverride
                          ?? _options.Temperature;

        var maxOutputTokens = request.MaxOutputTokens
                             ?? maxOutputTokensOverride
                             ?? _options.MaxOutputTokens;

        var requireJson = request.Purpose is "variant_generation" or "uniqueness_check" or "constraints_check" or "variant_judge";

        var prompt = $"""
SYSTEM:
{request.SystemPrompt}

USER:
{request.UserPrompt}
""";

        var settings = new PromptExecutionSettings
        {
            ModelId = model,
            ExtensionData = new Dictionary<string, object?>
            {
                ["temperature"] = temperature,
                ["max_tokens"] = maxOutputTokens,
                ["format"] = requireJson ? "json" : null,
                ["base_url"] = string.IsNullOrWhiteSpace(baseUrlOverride) ? null : baseUrlOverride,
                ["api_key"] = string.IsNullOrWhiteSpace(apiKeyOverride) ? null : apiKeyOverride
            }
        };

        var sw = Stopwatch.StartNew();
        var text = string.Empty;

        try
        {
            var res = await textGeneration.GetTextContentsAsync(
                prompt,
                settings,
                kernel: null,
                cancellationToken);

            text = res.FirstOrDefault()?.Text ?? string.Empty;
        }
        finally
        {
            sw.Stop();
        }

        if (_appSettings.LogLlmRequests)
        {
            logger.LogInformation(
                "LLM request success provider={Provider} purpose={Purpose} model={Model} latencyMs={LatencyMs} responseText={ResponseText}",
                "Ollama",
                request.Purpose,
                model,
                (int)sw.ElapsedMilliseconds,
                Truncate(text, _appSettings.LogLlmMaxChars));
        }

        return new LLMCompletionResult(
            Provider: "Ollama",
            Model: model,
            Text: text,
            PromptTokens: null,
            CompletionTokens: null,
            TotalTokens: null,
            LatencyMs: (int)sw.ElapsedMilliseconds,
            RawResponseJson: string.Empty);
    }

    private static string Truncate(string? text, int maxChars)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        if (maxChars <= 0) return string.Empty;
        if (text.Length <= maxChars) return text;
        return text.Substring(0, maxChars) + "…";
    }
}
