namespace LabGenerator.Application.Models;

public sealed record LLMCompletionRequest(
    string Purpose,
    string SystemPrompt,
    string UserPrompt,
    string? Model,
    double? Temperature,
    int? MaxOutputTokens
);