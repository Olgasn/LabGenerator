using LabGenerator.Application.Abstractions;
using LabGenerator.Application.Models;

namespace LabGenerator.Tests.Helpers;

internal sealed class StubLlmClient(string text) : ILLMClient
{
    public int CallCount { get; private set; }
    public LLMCompletionRequest? LastRequest { get; private set; }

    public Task<LLMCompletionResult> GenerateTextAsync(LLMCompletionRequest request, CancellationToken cancellationToken)
    {
        CallCount++;
        LastRequest = request;

        return Task.FromResult(new LLMCompletionResult(
            Provider: "Stub",
            Model: "stub-model",
            Text: text,
            PromptTokens: null,
            CompletionTokens: null,
            TotalTokens: null,
            LatencyMs: 0,
            RawResponseJson: "{}"));
    }
}
