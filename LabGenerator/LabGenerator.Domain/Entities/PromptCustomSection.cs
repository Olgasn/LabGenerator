using System.ComponentModel.DataAnnotations;

namespace LabGenerator.Domain.Entities;

public sealed class PromptCustomSection
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string SectionKey { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
