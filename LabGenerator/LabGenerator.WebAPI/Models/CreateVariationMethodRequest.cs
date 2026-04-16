namespace LabGenerator.WebAPI.Models;

public sealed class CreateVariationMethodRequest
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}