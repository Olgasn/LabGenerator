using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabGenerator.Domain.Entities;

public class Lab
{
    [Key]
    public int Id { get; set; }

    public int OrderIndex { get; set; }

    [Required]
    [MaxLength(250)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string InitialDescription { get; set; } = string.Empty;

    public int DisciplineId { get; set; }

    [ForeignKey("DisciplineId")]
    public virtual Discipline Discipline { get; set; } = null!;

    public virtual ICollection<MasterAssignment> MasterAssignments { get; set; } = new List<MasterAssignment>();

    public virtual ICollection<VariationProfile> VariationProfiles { get; set; } = new List<VariationProfile>();

    public virtual ICollection<AssignmentVariant> AssignmentVariants { get; set; } = new List<AssignmentVariant>();

    public virtual ICollection<LabSupplementaryMaterial> SupplementaryMaterials { get; set; } = new List<LabSupplementaryMaterial>();
}
