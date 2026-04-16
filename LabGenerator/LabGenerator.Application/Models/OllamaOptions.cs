namespace LabGenerator.Application.Models;

public sealed class OllamaOptions
{
    public const string SectionName = "LLM:Ollama";

    public string BaseUrl { get; set; } = "https://ollama.com";

    public string ApiKey { get; set; } = string.Empty;

    public string Model { get; set; } = "deepseek-v3.2:cloud";

    public double Temperature { get; set; } = 0.2;

    public int MaxOutputTokens { get; set; } = 4096;
}