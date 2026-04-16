namespace LabGenerator.WebAPI.Models;

public sealed class GetLabsRequest
{
    public int? DisciplineId { get; set; }

    public string? Search { get; set; }

    public string? Sort { get; set; } = "desc";

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public bool All { get; set; }
}
