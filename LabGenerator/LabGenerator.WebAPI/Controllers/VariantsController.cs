using System.Text.Json;
using LabGenerator.Domain.Entities;
using LabGenerator.Domain.Enums;
using LabGenerator.Infrastructure.Data;
using LabGenerator.Infrastructure.Services;
using LabGenerator.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LabGenerator.WebAPI.Controllers;

[ApiController]
[Route("api/labs/{labId:int}/variants")]
public class VariantsController(ApplicationDbContext db, LlmAccessGuardService llmAccessGuard) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponse<AssignmentVariant>>> GetAll(
        int labId,
        [FromQuery] GetVariantsRequest request,
        CancellationToken cancellationToken)
    {
        var query = db.AssignmentVariants.AsNoTracking()
            .Where(x => x.LabId == labId);

        var totalCount = await query.CountAsync(cancellationToken);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));
        var page = Math.Clamp(request.Page, 1, totalPages);
        var sortedQuery = query.ApplySorting(request.Sort);

        var items = await sortedQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResponse<AssignmentVariant>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages
        };
    }

    [HttpPost("generate")]
    public async Task<ActionResult<GenerationJob>> Generate(int labId, [FromBody] GenerateVariantsRequest request, CancellationToken cancellationToken)
    {
        var master = await db.MasterAssignments.AsNoTracking()
            .FirstOrDefaultAsync(x => x.LabId == labId && x.IsCurrent, cancellationToken);

        if (master is null)
        {
            return BadRequest("MasterAssignment is not generated.");
        }

        if (master.Status != MasterAssignmentStatus.Approved)
        {
            return BadRequest("MasterAssignment must be approved before generating variants.");
        }

        var llmStatus = await llmAccessGuard.GetStatusAsync(cancellationToken);
        if (!llmStatus.HasApiKey)
        {
            return BadRequest(llmStatus.Message);
        }

        var job = new GenerationJob
        {
            Type = GenerationJobType.GenerateVariants,
            Status = GenerationJobStatus.Pending,
            LabId = labId,
            MasterAssignmentId = master.Id,
            VariationProfileId = request.VariationProfileId,
            PayloadJson = JsonSerializer.Serialize(new { count = request.Count, variationProfileId = request.VariationProfileId }),
            CreatedAt = DateTimeOffset.UtcNow,
            Progress = 0
        };

        db.GenerationJobs.Add(job);
        await db.SaveChangesAsync(cancellationToken);

        return job;
    }
}

file static class AssignmentVariantQueryExtensions
{
    public static IQueryable<AssignmentVariant> ApplySorting(this IQueryable<AssignmentVariant> query, string? sort)
    {
        return (sort ?? "asc").Trim().ToLowerInvariant() switch
        {
            "desc" => query.OrderByDescending(x => x.VariantIndex).ThenByDescending(x => x.Id),
            _ => query.OrderBy(x => x.VariantIndex).ThenBy(x => x.Id)
        };
    }
}
