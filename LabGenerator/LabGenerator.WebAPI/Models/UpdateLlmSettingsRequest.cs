namespace LabGenerator.WebAPI.Models;

public sealed class UpdateLlmSettingsRequest
{
    public string Provider { get; set; } = "Ollama";

    public string Model { get; set; } = string.Empty;
}
