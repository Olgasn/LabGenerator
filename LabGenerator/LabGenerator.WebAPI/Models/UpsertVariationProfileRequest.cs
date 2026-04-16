namespace LabGenerator.WebAPI.Models;

public sealed class UpsertVariationProfileRequest
{
    public string Name { get; set; } = string.Empty;

    public string ParametersJson { get; set; } = "{}";

    public string DifficultyRubricJson { get; set; } = "{}";

    /// <summary>
    /// Переопределение целевых параметров сложности для данного профиля.
    /// null означает использование глобальных умолчаний из difficulty_defaults.json.
    /// Формат: {"Complexity":"medium","EstimatedHoursMin":5,"EstimatedHoursMax":7}
    /// </summary>
    public string? DifficultyTargetJson { get; set; }

    public bool IsDefault { get; set; }
}
