using System.Text.Json;
using LabGenerator.Domain.Entities;

namespace LabGenerator.WebAPI.Models;

public sealed class LabSupplementaryMaterialResponse
{
    public int Id { get; init; }

    public int LabId { get; init; }

    public string TheoryMarkdown { get; init; } = string.Empty;

    public IReadOnlyList<string> ControlQuestions { get; init; } = [];

    public string SourceFingerprint { get; init; } = string.Empty;

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public static LabSupplementaryMaterialResponse FromEntity(LabSupplementaryMaterial material)
    {
        return new LabSupplementaryMaterialResponse
        {
            Id = material.Id,
            LabId = material.LabId,
            TheoryMarkdown = material.TheoryMarkdown,
            ControlQuestions = ParseQuestions(material.ControlQuestionsJson),
            SourceFingerprint = material.SourceFingerprint,
            CreatedAt = material.CreatedAt,
            UpdatedAt = material.UpdatedAt
        };
    }

    private static IReadOnlyList<string> ParseQuestions(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Array)
            {
                return [];
            }

            return doc.RootElement.EnumerateArray()
                .Where(x => x.ValueKind == JsonValueKind.String)
                .Select(x => x.GetString()?.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Cast<string>()
                .ToArray();
        }
        catch
        {
            return [];
        }
    }
}
