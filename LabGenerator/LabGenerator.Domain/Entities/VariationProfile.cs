using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabGenerator.Domain.Entities;

public class VariationProfile
{
    [Key]
    public int Id { get; set; }

    public int LabId { get; set; }

    [ForeignKey("LabId")]
    public virtual Lab Lab { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string ParametersJson { get; set; } = "{}";

    [Required]
    public string DifficultyRubricJson { get; set; } = "{}";

    /// <summary>
    /// JSON целевых параметров сложности для данного профиля.
    /// Если null — применяются глобальные умолчания из difficulty_defaults.json.
    /// Формат: {"Complexity":"medium","EstimatedHoursMin":4,"EstimatedHoursMax":8}
    /// </summary>
    public string? DifficultyTargetJson { get; set; }

    public bool IsDefault { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
