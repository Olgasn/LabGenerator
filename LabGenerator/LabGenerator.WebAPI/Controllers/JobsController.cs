using LabGenerator.Domain.Entities;
using LabGenerator.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LabGenerator.WebAPI.Controllers;

[ApiController]
[Route("api/jobs")]
public class JobsController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<ActionResult<GenerationJob>> Get(int id, CancellationToken cancellationToken)
    {
        var job = await db.GenerationJobs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (job is null) return NotFound();
        return job;
    }
}
