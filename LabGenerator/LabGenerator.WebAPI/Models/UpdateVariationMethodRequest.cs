namespace LabGenerator.WebAPI.Models;

public sealed class UpdateVariationMethodRequest
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}
