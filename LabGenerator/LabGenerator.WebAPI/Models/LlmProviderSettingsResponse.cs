namespace LabGenerator.WebAPI.Models;

public sealed class LlmProviderSettingsResponse
{
    public int Id { get; set; }

    public string Provider { get; set; } = string.Empty;

    public string? Model { get; set; }

    public bool HasApiKey { get; set; }

    public string? ApiKeyMasked { get; set; }

    public double? Temperature { get; set; }

    public int? MaxOutputTokens { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
