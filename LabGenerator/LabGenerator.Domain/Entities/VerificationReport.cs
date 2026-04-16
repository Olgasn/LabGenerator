using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabGenerator.Domain.Entities;

public class VerificationReport
{
    [Key]
    public int Id { get; set; }

    public int AssignmentVariantId { get; set; }

    [ForeignKey("AssignmentVariantId")]
    public virtual AssignmentVariant AssignmentVariant { get; set; } = null!;

    public bool Passed { get; set; }

    [Required]
    public string JudgeScoreJson { get; set; } = "{}";

    [Required]
    public string IssuesJson { get; set; } = "[]";

    public int? JudgeRunId { get; set; }

    public int? SolverRunId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
