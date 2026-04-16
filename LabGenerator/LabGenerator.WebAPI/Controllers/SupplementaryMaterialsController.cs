using System.Text.Json;
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
[Route("api/labs/{labId:int}/supplementary-material")]
public class SupplementaryMaterialsController(
    ApplicationDbContext db,
    ILabSupplementaryMaterialService supplementaryMaterialService,
    LlmAccessGuardService llmAccessGuard) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<LabSupplementaryMaterialResponse>> GetCurrent(int labId, CancellationToken cancellationToken)
    {
        var material = await supplementaryMaterialService.GetCurrentAsync(labId, cancellationToken);
        if (material is null)
        {
            return NotFound();
        }

        return LabSupplementaryMaterialResponse.FromEntity(material);
    }

    [HttpPost("generate")]
    public async Task<ActionResult<GenerationJob>> Generate(
        int labId,
        [FromBody] GenerateSupplementaryMaterialRequest? request,
        CancellationToken cancellationToken)
    {
        var exists = await db.Labs.AnyAsync(x => x.Id == labId, cancellationToken);
        if (!exists)
        {
            return NotFound($"Lab {labId} not found.");
        }

        var llmStatus = await llmAccessGuard.GetStatusAsync(cancellationToken);
        if (!llmStatus.HasApiKey)
        {
            return BadRequest(llmStatus.Message);
        }

        var job = new GenerationJob
        {
            Type = GenerationJobType.GenerateTheory,
            Status = GenerationJobStatus.Pending,
            LabId = labId,
            PayloadJson = JsonSerializer.Serialize(new { force = request?.Force ?? false }),
            CreatedAt = DateTimeOffset.UtcNow,
            Progress = 0
        };

        db.GenerationJobs.Add(job);
        await db.SaveChangesAsync(cancellationToken);

        return job;
    }
}
