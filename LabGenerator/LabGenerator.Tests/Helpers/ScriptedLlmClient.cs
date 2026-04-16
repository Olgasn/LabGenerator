using LabGenerator.Application.Abstractions;
using LabGenerator.Application.Models;

namespace LabGenerator.Tests.Helpers;

internal sealed class ScriptedLlmClient : ILLMClient
{
    private readonly Dictionary<string, Queue<LLMCompletionResult>> _responses = new(StringComparer.Ordinal);

    public List<LLMCompletionRequest> Requests { get; } = [];

    public void Enqueue(
        string purpose,
        string text,
        string provider = "Stub",
        string model = "stub-model")
    {
        if (!_responses.TryGetValue(purpose, out var queue))
        {
            queue = new Queue<LLMCompletionResult>();
            _responses[purpose] = queue;
        }

        queue.Enqueue(new LLMCompletionResult(
            Provider: provider,
            Model: model,
            Text: text,
            PromptTokens: null,
            CompletionTokens: null,
            TotalTokens: null,
            LatencyMs: 0,
            RawResponseJson: "{}"));
    }

    public Task<LLMCompletionResult> GenerateTextAsync(LLMCompletionRequest request, CancellationToken cancellationToken)
    {
        Requests.Add(request);

        if (!_responses.TryGetValue(request.Purpose, out var queue) || queue.Count == 0)
        {
            throw new InvalidOperationException($"No scripted LLM response for purpose '{request.Purpose}'.");
        }

        return Task.FromResult(queue.Dequeue());
    }
}
