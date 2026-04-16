using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabGenerator.Domain.Entities;

public class LabSupplementaryMaterial
{
    [Key]
    public int Id { get; set; }

    public int LabId { get; set; }

    [ForeignKey("LabId")]
    public virtual Lab Lab { get; set; } = null!;

    [Required]
    public string TheoryMarkdown { get; set; } = string.Empty;

    [Required]
    public string ControlQuestionsJson { get; set; } = "[]";

    [Required]
    [MaxLength(128)]
    public string SourceFingerprint { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}
