using System.ComponentModel.DataAnnotations;

namespace LabGenerator.Domain.Entities;

public sealed class LlmSettings
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Provider { get; set; } = "Ollama";

    [Required]
    [MaxLength(200)]
    public string Model { get; set; } = string.Empty;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
