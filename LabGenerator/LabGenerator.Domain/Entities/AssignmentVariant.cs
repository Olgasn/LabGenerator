using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabGenerator.Domain.Entities;

public class AssignmentVariant
{
    [Key]
    public int Id { get; set; }

    public int LabId { get; set; }

    [ForeignKey("LabId")]
    public virtual Lab Lab { get; set; } = null!;

    public int VariantIndex { get; set; }

    [Required]
    [MaxLength(250)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    [Required]
    public string VariantParamsJson { get; set; } = "{}";

    [Required]
    public string DifficultyProfileJson { get; set; } = "{}";

    [Required]
    [MaxLength(200)]
    public string Fingerprint { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}