using LabGenerator.Application.Models;

namespace LabGenerator.Application.Abstractions;

public interface ILLMClient
{
    Task<LLMCompletionResult> GenerateTextAsync(LLMCompletionRequest request, CancellationToken cancellationToken);
}