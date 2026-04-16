using LabGenerator.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace LabGenerator.WebAPI.Controllers;

[ApiController]
[Route("api/prompt-sections")]
public sealed class PromptCustomSectionsController(PromptCustomSectionService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PromptCustomSectionResponse>>> GetAll(CancellationToken ct)
    {
        var items = await service.GetAllAsync(ct);
        return Ok(items.Select(ToResponse).ToList());
    }

    [HttpGet("{sectionKey}")]
    public async Task<ActionResult<PromptCustomSectionResponse>> Get([FromRoute] string sectionKey, CancellationToken ct)
    {
        try
        {
            return ToResponse(await service.GetAsync(sectionKey, ct));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPut("{sectionKey}")]
    public async Task<ActionResult<PromptCustomSectionResponse>> Update(
        [FromRoute] string sectionKey,
        [FromBody] UpdatePromptCustomSectionRequest request,
        CancellationToken ct)
    {
        try
        {
            var result = await service.UpdateAsync(sectionKey, request.Content, ct);
            return ToResponse(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{sectionKey}")]
    public async Task<ActionResult<PromptCustomSectionResponse>> Reset([FromRoute] string sectionKey, CancellationToken ct)
    {
        try
        {
            return ToResponse(await service.ResetAsync(sectionKey, ct));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    private static PromptCustomSectionResponse ToResponse(PromptCustomSectionDetails d) => new()
    {
        SectionKey = d.SectionKey,
        DisplayName = d.DisplayName,
        Content = d.Content,
        DefaultContent = d.DefaultContent,
        IsCustomized = d.IsCustomized,
        UpdatedAt = d.UpdatedAt
    };
}

public sealed class PromptCustomSectionResponse
{
    public string SectionKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string DefaultContent { get; set; } = string.Empty;
    public bool IsCustomized { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public sealed class UpdatePromptCustomSectionRequest
{
    public string Content { get; set; } = string.Empty;
}
