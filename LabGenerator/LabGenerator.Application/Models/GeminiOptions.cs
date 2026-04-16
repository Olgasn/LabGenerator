namespace LabGenerator.Application.Models;

public sealed class GeminiOptions
{
    public const string SectionName = "LLM:Gemini";

    public string ApiKey { get; set; } = string.Empty;

    public string Model { get; set; } = "gemini-2.0-pro";

    public double Temperature { get; set; } = 0.2;

    public int MaxOutputTokens { get; set; } = 4096;
}