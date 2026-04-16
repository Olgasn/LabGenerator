using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabGenerator.Domain.Entities;

public class LabVariationMethod
{
    [Key]
    public int Id { get; set; }

    public int LabId { get; set; }

    [ForeignKey("LabId")]
    public virtual Lab Lab { get; set; } = null!;

    public int VariationMethodId { get; set; }

    [ForeignKey("VariationMethodId")]
    public virtual VariationMethod VariationMethod { get; set; } = null!;

    public bool PreserveAcrossLabs { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}