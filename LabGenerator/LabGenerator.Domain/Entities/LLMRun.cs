using System.ComponentModel.DataAnnotations;

namespace LabGenerator.Domain.Entities;

public class LLMRun
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Provider { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Model { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Purpose { get; set; } = string.Empty;

    [Required]
    public string RequestJson { get; set; } = "{}";

    [Required]
    public string ResponseText { get; set; } = string.Empty;

    public int? PromptTokens { get; set; }

    public int? CompletionTokens { get; set; }

    public int? TotalTokens { get; set; }

    public int? LatencyMs { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Succeeded";

    public DateTimeOffset CreatedAt { get; set; }
}