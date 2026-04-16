namespace LabGenerator.WebAPI.Models;

public sealed class GetVariantsRequest
{
    public string? Sort { get; set; } = "asc";

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}
