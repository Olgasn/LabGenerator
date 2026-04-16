namespace LabGenerator.WebAPI.Models;

public sealed class GenerateVariantsRequest
{
    public int Count { get; set; } = 10;

    public int? VariationProfileId { get; set; }
}