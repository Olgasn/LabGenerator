using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LabGenerator.Domain.Enums;

namespace LabGenerator.Domain.Entities;

public class MasterAssignment
{
    [Key]
    public int Id { get; set; }

    public int LabId { get; set; }

    [ForeignKey("LabId")]
    public virtual Lab Lab { get; set; } = null!;

    public int Version { get; set; }

    public bool IsCurrent { get; set; }

    public MasterAssignmentStatus Status { get; set; }

    [Required]
    public string Content { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}