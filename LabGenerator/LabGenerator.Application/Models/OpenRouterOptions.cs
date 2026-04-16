namespace LabGenerator.Application.Models;

public sealed class OpenRouterOptions
{
    public const string SectionName = "LLM:OpenRouter";

    public string BaseUrl { get; set; } = "https://openrouter.ai/api/v1";

    public string ApiKey { get; set; } = string.Empty;

    public string Model { get; set; } = "openai/gpt-4o-mini";

    public double Temperature { get; set; } = 0.2;

    public int MaxOutputTokens { get; set; } = 4096;
}
