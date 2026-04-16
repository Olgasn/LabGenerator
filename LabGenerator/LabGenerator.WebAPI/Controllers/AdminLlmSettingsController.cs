using LabGenerator.Domain.Entities;
using LabGenerator.Infrastructure.Data;
using LabGenerator.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LabGenerator.WebAPI.Controllers;

[ApiController]
[Route("api/admin/llm-settings")]
public sealed class AdminLlmSettingsController(ApplicationDbContext db, IConfiguration configuration) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<LlmSettings>> Get(CancellationToken cancellationToken)
    {
        var current = await db.Set<LlmSettings>().AsNoTracking()
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (current is not null)
        {
            return current;
        }

        var provider = configuration["LLM:Provider"]
                       ?? configuration["LLM__Provider"]
                       ?? "Ollama";

        var created = new LlmSettings
        {
            Provider = provider,
            Model = string.Empty,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        db.Set<LlmSettings>().Add(created);
        await db.SaveChangesAsync(cancellationToken);

        return created;
    }

    [HttpPut]
    public async Task<ActionResult<LlmSettings>> Upsert([FromBody] UpdateLlmSettingsRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Provider))
        {
            return BadRequest("Provider is required.");
        }

        var provider = request.Provider.Trim();
        var model = request.Model?.Trim() ?? string.Empty;

        var current = await db.Set<LlmSettings>()
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (current is null)
        {
            current = new LlmSettings
            {
                Provider = provider,
                Model = model,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            db.Set<LlmSettings>().Add(current);
        }
        else
        {
            current.Provider = provider;
            current.Model = model;
            current.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);

        return current;
    }
}
