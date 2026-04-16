using LabGenerator.Domain.Entities;
using LabGenerator.Infrastructure.Data;
using LabGenerator.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LabGenerator.WebAPI.Controllers;

[ApiController]
[Route("api/labs")]
public class LabsController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponse<Lab>>> GetAll([FromQuery] GetLabsRequest request, CancellationToken cancellationToken)
    {
        var query = db.Labs.AsNoTracking();

        if (request.DisciplineId is int disciplineId)
        {
            query = query.Where(x => x.DisciplineId == disciplineId);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x => x.Title.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var pageSize = request.All
            ? Math.Max(1, totalCount)
            : Math.Clamp(request.PageSize, 1, 100);
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));
        var page = request.All
            ? 1
            : Math.Clamp(request.Page, 1, totalPages);

        var sortedQuery = ApplySorting(query, request.Sort);

        var items = await sortedQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResponse<Lab>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages
        };
    }

    [HttpGet("{labId:int}")]
    public async Task<ActionResult<Lab>> GetById(int labId, CancellationToken cancellationToken)
    {
        var lab = await db.Labs.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == labId, cancellationToken);

        return lab is null ? NotFound() : lab;
    }

    [HttpPost]
    public async Task<ActionResult<Lab>> Create([FromBody] CreateLabRequest request, CancellationToken cancellationToken)
    {
        var lab = new Lab
        {
            DisciplineId = request.DisciplineId,
            OrderIndex = request.OrderIndex,
            Title = request.Title,
            InitialDescription = request.InitialDescription
        };

        db.Labs.Add(lab);
        await db.SaveChangesAsync(cancellationToken);
        return lab;
    }

    [HttpDelete("{labId:int}")]
    public async Task<IActionResult> Delete(int labId, CancellationToken cancellationToken)
    {
        var lab = await db.Labs.FirstOrDefaultAsync(x => x.Id == labId, cancellationToken);
        if (lab is null)
        {
            return NotFound();
        }

        await RemoveLabGraphAsync(new[] { labId }, cancellationToken);

        db.Labs.Remove(lab);
        await db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    internal async Task RemoveLabGraphAsync(IReadOnlyCollection<int> labIds, CancellationToken cancellationToken)
    {
        if (labIds.Count == 0)
        {
            return;
        }

        var masterIds = await db.MasterAssignments
            .Where(x => labIds.Contains(x.LabId))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var variationProfileIds = await db.VariationProfiles
            .Where(x => labIds.Contains(x.LabId))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var assignmentVariantIds = await db.AssignmentVariants
            .Where(x => labIds.Contains(x.LabId))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var generationJobs = await db.GenerationJobs
            .Where(x =>
                (x.LabId.HasValue && labIds.Contains(x.LabId.Value)) ||
                (x.MasterAssignmentId.HasValue && masterIds.Contains(x.MasterAssignmentId.Value)) ||
                (x.VariationProfileId.HasValue && variationProfileIds.Contains(x.VariationProfileId.Value)))
            .ToListAsync(cancellationToken);

        var verificationReports = await db.VerificationReports
            .Where(x => assignmentVariantIds.Contains(x.AssignmentVariantId))
            .ToListAsync(cancellationToken);

        var assignmentVariantValues = await db.Set<AssignmentVariantVariationValue>()
            .Where(x => assignmentVariantIds.Contains(x.AssignmentVariantId))
            .ToListAsync(cancellationToken);

        var supplementaryMaterials = await db.LabSupplementaryMaterials
            .Where(x => labIds.Contains(x.LabId))
            .ToListAsync(cancellationToken);

        var labVariationMethods = await db.LabVariationMethods
            .Where(x => labIds.Contains(x.LabId))
            .ToListAsync(cancellationToken);

        var masterAssignments = await db.MasterAssignments
            .Where(x => labIds.Contains(x.LabId))
            .ToListAsync(cancellationToken);

        var variationProfiles = await db.VariationProfiles
            .Where(x => labIds.Contains(x.LabId))
            .ToListAsync(cancellationToken);

        var assignmentVariants = await db.AssignmentVariants
            .Where(x => labIds.Contains(x.LabId))
            .ToListAsync(cancellationToken);

        db.GenerationJobs.RemoveRange(generationJobs);
        db.VerificationReports.RemoveRange(verificationReports);
        db.Set<AssignmentVariantVariationValue>().RemoveRange(assignmentVariantValues);
        db.LabSupplementaryMaterials.RemoveRange(supplementaryMaterials);
        db.LabVariationMethods.RemoveRange(labVariationMethods);
        db.MasterAssignments.RemoveRange(masterAssignments);
        db.VariationProfiles.RemoveRange(variationProfiles);
        db.AssignmentVariants.RemoveRange(assignmentVariants);
    }

    private static IQueryable<Lab> ApplySorting(IQueryable<Lab> query, string? sort)
    {
        return (sort ?? "desc").Trim().ToLowerInvariant() switch
        {
            "asc" => query.OrderBy(x => x.Id),
            _ => query.OrderByDescending(x => x.Id)
        };
    }
}
