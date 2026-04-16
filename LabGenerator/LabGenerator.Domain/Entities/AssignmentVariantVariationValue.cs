using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabGenerator.Domain.Entities;

public class AssignmentVariantVariationValue
{
    [Key]
    public int Id { get; set; }

    public int AssignmentVariantId { get; set; }

    [ForeignKey("AssignmentVariantId")]
    public virtual AssignmentVariant AssignmentVariant { get; set; } = null!;

    public int VariationMethodId { get; set; }

    [ForeignKey("VariationMethodId")]
    public virtual VariationMethod VariationMethod { get; set; } = null!;

    public string Value { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}