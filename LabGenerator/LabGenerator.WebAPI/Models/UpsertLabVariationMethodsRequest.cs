namespace LabGenerator.WebAPI.Models;

public sealed class UpsertLabVariationMethodsRequest
{
    public List<LabVariationMethodItem> Items { get; set; } = new();

    public sealed class LabVariationMethodItem
    {
        public int VariationMethodId { get; set; }

        public bool PreserveAcrossLabs { get; set; }
    }
}