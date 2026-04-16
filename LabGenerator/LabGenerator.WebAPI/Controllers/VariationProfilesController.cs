using LabGenerator.Domain.Entities;
using LabGenerator.Infrastructure.Data;
using LabGenerator.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LabGenerator.WebAPI.Controllers;

[ApiController]
[Route("api/labs/{labId:int}/variation-profiles")]
public sealed class VariationProfilesController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<VariationProfile>>> GetAll(int labId, CancellationToken cancellationToken)
    {
        var exists = await db.Labs.AnyAsync(x => x.Id == labId, cancellationToken);
        if (!exists) return NotFound($"Lab {labId} not found.");

        var items = await db.VariationProfiles.AsNoTracking()
            .Where(x => x.LabId == labId)
            .OrderByDescending(x => x.IsDefault)
            .ThenByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return items;
    }

    [HttpPost]
    public async Task<ActionResult<VariationProfile>> Create(
        int labId,
        [FromBody] UpsertVariationProfileRequest request,
        CancellationToken cancellationToken)
    {
        var lab = await db.Labs.FirstOrDefaultAsync(x => x.Id == labId, cancellationToken);
        if (lab is null) return NotFound($"Lab {labId} not found.");

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("Name is required.");
        }

        if (request.IsDefault)
        {
            var currentDefaults = await db.VariationProfiles
                .Where(x => x.LabId == labId && x.IsDefault)
                .ToListAsync(cancellationToken);

            foreach (var d in currentDefaults)
            {
                d.IsDefault = false;
            }
        }

        var entity = new VariationProfile
        {
            LabId = labId,
            Name = request.Name.Trim(),
            ParametersJson = string.IsNullOrWhiteSpace(request.ParametersJson) ? "{}" : request.ParametersJson,
            DifficultyRubricJson = string.IsNullOrWhiteSpace(request.DifficultyRubricJson) ? "{}" : request.DifficultyRubricJson,
            DifficultyTargetJson = string.IsNullOrWhiteSpace(request.DifficultyTargetJson) ? null : request.DifficultyTargetJson,
            IsDefault = request.IsDefault,
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.VariationProfiles.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        return entity;
    }
}
