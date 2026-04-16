using LabGenerator.Application.Abstractions;
using LabGenerator.Domain.Entities;
using LabGenerator.Domain.Enums;
using LabGenerator.Infrastructure.Data;
using LabGenerator.Infrastructure.Services;
using LabGenerator.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LabGenerator.WebAPI.Controllers;

[ApiController]
[Route("api/labs/{labId:int}/master")]
public class MasterAssignmentsController(
    ApplicationDbContext db,
    IMasterAssignmentService masterService,
    LlmAccessGuardService llmAccessGuard) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<MasterAssignment>> GetCurrent(int labId, CancellationToken cancellationToken)
    {
        var master = await masterService.GetCurrentAsync(labId, cancellationToken);
        if (master is null) return NotFound();
        return master;
    }

    [HttpPost("generate")]
    public async Task<ActionResult<GenerationJob>> GenerateDraft(int labId, CancellationToken cancellationToken)
    {
        var exists = await db.Labs.AnyAsync(x => x.Id == labId, cancellationToken);
        if (!exists) return NotFound($"Lab {labId} not found.");

        var llmStatus = await llmAccessGuard.GetStatusAsync(cancellationToken);
        if (!llmStatus.HasApiKey)
        {
            return BadRequest(llmStatus.Message);
        }

        var job = new GenerationJob
        {
            Type = GenerationJobType.GenerateMasterAssignment,
            Status = GenerationJobStatus.Pending,
            LabId = labId,
            CreatedAt = DateTimeOffset.UtcNow,
            Progress = 0
        };

        db.GenerationJobs.Add(job);
        await db.SaveChangesAsync(cancellationToken);

        return job;
    }

    [HttpPut("{masterAssignmentId:int}")]
    public async Task<ActionResult<MasterAssignment>> Update(
        int labId,
        int masterAssignmentId,
        [FromBody] UpdateMasterAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        var master = await db.MasterAssignments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == masterAssignmentId, cancellationToken);
        if (master is null) return NotFound($"MasterAssignment {masterAssignmentId} not found.");
        if (master.LabId != labId) return BadRequest("masterAssignmentId does not belong to labId");

        var updated = await masterService.UpdateContentAsync(masterAssignmentId, request.Content, cancellationToken);
        return updated;
    }

    [HttpPost("{masterAssignmentId:int}/approve")]
    public async Task<ActionResult<MasterAssignment>> Approve(int labId, int masterAssignmentId, CancellationToken cancellationToken)
    {
        var master = await db.MasterAssignments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == masterAssignmentId, cancellationToken);
        if (master is null) return NotFound($"MasterAssignment {masterAssignmentId} not found.");
        if (master.LabId != labId) return BadRequest("masterAssignmentId does not belong to labId");

        var approved = await masterService.ApproveAsync(masterAssignmentId, cancellationToken);
        return approved;
    }
}
