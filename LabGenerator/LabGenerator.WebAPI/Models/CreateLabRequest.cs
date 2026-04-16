namespace LabGenerator.WebAPI.Models;

public sealed class CreateLabRequest
{
    public int DisciplineId { get; set; }

    public int OrderIndex { get; set; }

    public string Title { get; set; } = string.Empty;

    public string InitialDescription { get; set; } = string.Empty;
}