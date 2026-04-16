namespace LabGenerator.Application.Models;

/// <summary>
/// Целевые параметры сложности вариантов одной лабораторной работы.
/// Глобальные значения по умолчанию задаются в difficulty_defaults.json.
/// Профиль варьирования может переопределить их через DifficultyTargetJson.
/// </summary>
public sealed class DifficultyDefaults
{
    public const string SectionName = "DifficultyDefaults";

    /// <summary>Ожидаемый уровень сложности: low | medium | high.</summary>
    public string Complexity { get; set; } = "medium";

    /// <summary>Минимально допустимая трудоёмкость варианта (часов).</summary>
    public int EstimatedHoursMin { get; set; } = 5;

    /// <summary>Максимально допустимая трудоёмкость варианта (часов).</summary>
    public int EstimatedHoursMax { get; set; } = 7;
}
