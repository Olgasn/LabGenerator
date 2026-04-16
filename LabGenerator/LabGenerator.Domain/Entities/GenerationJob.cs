using System.ComponentModel.DataAnnotations;
using LabGenerator.Domain.Enums;

namespace LabGenerator.Domain.Entities;

public class GenerationJob
{
    [Key]
    public int Id { get; set; }

    public GenerationJobType Type { get; set; }

    public GenerationJobStatus Status { get; set; }

    public int? DisciplineId { get; set; }

    public int? LabId { get; set; }

    public int? MasterAssignmentId { get; set; }

    public int? VariationProfileId { get; set; }

    public string? PayloadJson { get; set; }

    public string? ResultJson { get; set; }

    public string? Error { get; set; }

    public int Progress { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? StartedAt { get; set; }

    public DateTimeOffset? FinishedAt { get; set; }
}