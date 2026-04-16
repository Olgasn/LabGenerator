namespace LabGenerator.WebAPI.Models;

public sealed class UpsertLlmProviderSettingsRequest
{
    public string Provider { get; set; } = "Ollama";

    public string? Model { get; set; }

    public string? ApiKey { get; set; }

    public bool ClearApiKey { get; set; }

    public double? Temperature { get; set; }

    public int? MaxOutputTokens { get; set; }
}
