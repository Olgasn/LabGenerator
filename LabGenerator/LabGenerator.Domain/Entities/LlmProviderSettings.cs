using System.ComponentModel.DataAnnotations;

namespace LabGenerator.Domain.Entities;

public sealed class LlmProviderSettings
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Provider { get; set; } = "Ollama";

    [MaxLength(200)]
    public string? Model { get; set; }

    [MaxLength(1000)]
    public string? ApiKey { get; set; }

    public double? Temperature { get; set; }

    public int? MaxOutputTokens { get; set; }

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
