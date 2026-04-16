using LabGenerator.Domain.Entities;
using LabGenerator.Infrastructure.Data;
using LabGenerator.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LabGenerator.WebAPI.Controllers;

[ApiController]
[Route("api/labs/{labId:int}/variation-methods")]
public class LabVariationMethodsController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<LabVariationMethod>>> GetAll(int labId, CancellationToken cancellationToken)
    {
        var exists = await db.Labs.AnyAsync(x => x.Id == labId, cancellationToken);
        if (!exists) return NotFound($"Lab {labId} not found.");

        var items = await db.LabVariationMethods.AsNoTracking()
            .Include(x => x.VariationMethod)
            .Where(x => x.LabId == labId)
            .OrderBy(x => x.Id)
            .ToListAsync(cancellationToken);

        return items;
    }

    [HttpPut]
    public async Task<ActionResult<List<LabVariationMethod>>> Upsert(
        int labId,
        [FromBody] UpsertLabVariationMethodsRequest request,
        CancellationToken cancellationToken)
    {
        var lab = await db.Labs.FirstOrDefaultAsync(x => x.Id == labId, cancellationToken);
        if (lab is null) return NotFound($"Lab {labId} not found.");

        var methodIds = request.Items.Select(x => x.VariationMethodId).Distinct().ToList();
        var validCount = await db.VariationMethods.CountAsync(x => methodIds.Contains(x.Id), cancellationToken);
        if (validCount != methodIds.Count)
        {
            return BadRequest("Some VariationMethodId values are invalid.");
        }

        var existing = await db.LabVariationMethods
            .Where(x => x.LabId == labId)
            .ToListAsync(cancellationToken);

        db.LabVariationMethods.RemoveRange(existing);

        var now = DateTimeOffset.UtcNow;
        var newItems = request.Items
            .Select(x => new LabVariationMethod
            {
                LabId = labId,
                VariationMethodId = x.VariationMethodId,
                PreserveAcrossLabs = x.PreserveAcrossLabs,
                CreatedAt = now
            })
            .ToList();

        db.LabVariationMethods.AddRange(newItems);
        await db.SaveChangesAsync(cancellationToken);

        var result = await db.LabVariationMethods.AsNoTracking()
            .Include(x => x.VariationMethod)
            .Where(x => x.LabId == labId)
            .OrderBy(x => x.Id)
            .ToListAsync(cancellationToken);

        return result;
    }
}
