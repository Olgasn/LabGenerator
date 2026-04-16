using System.Text.Json;
using System.Text.Json.Serialization;

namespace LabGenerator.TeacherEmulator;

public sealed class BenchmarkModelEntry
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("provider")]
    public string Provider { get; set; } = "OpenRouter";

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("baseUrl")]
    public string? BaseUrl { get; set; }

    [JsonPropertyName("apiKeyEnv")]
    public string? ApiKeyEnv { get; set; }

    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; }

    [JsonPropertyName("maxOutputTokens")]
    public int? MaxOutputTokens { get; set; }
}

public static class BenchmarkModelsFile
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public static List<BenchmarkModelEntry> Load(string path)
    {
        var json = File.ReadAllText(path);
        var models = JsonSerializer.Deserialize<List<BenchmarkModelEntry>>(json, JsonOptions);

        if (models is null || models.Count == 0)
        {
            throw new InvalidOperationException($"No models found in {path}");
        }

        for (var i = 0; i < models.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(models[i].Name))
            {
                throw new InvalidOperationException($"Model at index {i} has no name.");
            }

            if (string.IsNullOrWhiteSpace(models[i].Model))
            {
                throw new InvalidOperationException($"Model '{models[i].Name}' has no model identifier.");
            }
        }

        return models;
    }
}
