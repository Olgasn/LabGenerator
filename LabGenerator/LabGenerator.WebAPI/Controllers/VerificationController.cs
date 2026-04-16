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
public class VerificationController(ApplicationDbContext db, LlmAccessGuardService llmAccessGuard) : ControllerBase
{
    [HttpPost("api/labs/{labId:int}/verify")]
    public async Task<ActionResult<GenerationJob>> VerifyLab(int labId, [FromBody] VerifyVariantsRequest request, CancellationToken cancellationToken)
    {
        if (request.VariantId is not null)
        {
            var variantExists = await db.AssignmentVariants.AnyAsync(x => x.Id == request.VariantId.Value, cancellationToken);
            if (!variantExists) return NotFound($"Variant {request.VariantId.Value} not found.");
        }

        var llmStatus = await llmAccessGuard.GetStatusAsync(cancellationToken);
        if (!llmStatus.HasApiKey)
        {
            return BadRequest(llmStatus.Message);
        }

        var job = new GenerationJob
        {
            Type = GenerationJobType.VerifyVariants,
            Status = GenerationJobStatus.Pending,
            LabId = labId,
            PayloadJson = JsonSerializer.Serialize(new { variantId = request.VariantId }),
            CreatedAt = DateTimeOffset.UtcNow,
            Progress = 0
        };

        db.GenerationJobs.Add(job);
        await db.SaveChangesAsync(cancellationToken);

        return job;
    }

    [HttpGet("api/variants/{variantId:int}/verification")]
    public async Task<ActionResult<VerificationReport>> GetReport(int variantId, CancellationToken cancellationToken)
    {
        var report = await db.VerificationReports.AsNoTracking()
            .FirstOrDefaultAsync(x => x.AssignmentVariantId == variantId, cancellationToken);

        if (report is null) return NotFound();
        return report;
    }

    [HttpGet("api/labs/{labId:int}/verification-reports")]
    public async Task<ActionResult<List<VerificationReport>>> GetLabReports(int labId, CancellationToken cancellationToken)
    {
        var reports = await (
                from r in db.VerificationReports.AsNoTracking()
                join v in db.AssignmentVariants.AsNoTracking() on r.AssignmentVariantId equals v.Id
                where v.LabId == labId
                select r)
            .ToListAsync(cancellationToken);

        return reports;
    }
}
