namespace LabGenerator.WebAPI.Models;

public sealed class CreateDisciplineRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}