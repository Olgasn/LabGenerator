namespace LabGenerator.WebAPI.Models;

public sealed class DisciplineListItemResponse
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int LabsCount { get; set; }
}
