using System.ComponentModel.DataAnnotations;

namespace LabGenerator.Domain.Entities;

public class Discipline
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public virtual ICollection<Lab> Labs { get; set; } = new List<Lab>();
}