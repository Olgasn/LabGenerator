namespace LabGenerator.Application.Models;

public sealed record LLMCompletionResult(
    string Provider,
    string Model,
    string Text,
    int? PromptTokens,
    int? CompletionTokens,
    int? TotalTokens,
    int? LatencyMs,
    string RawResponseJson
);