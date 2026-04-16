using LabGenerator.Domain.Entities;
using LabGenerator.Infrastructure.Data;
using LabGenerator.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LabGenerator.WebAPI.Controllers;

[ApiController]
[Route("api/disciplines")]
public class DisciplinesController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public Task<List<DisciplineListItemResponse>> GetAll(CancellationToken cancellationToken)
        => db.Disciplines.AsNoTracking()
            .Select(x => new DisciplineListItemResponse
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                LabsCount = x.Labs.Count
            })
            .ToListAsync(cancellationToken);

    [HttpPost]
    public async Task<ActionResult<Discipline>> Create([FromBody] CreateDisciplineRequest request, CancellationToken cancellationToken)
    {
        var discipline = new Discipline
        {
            Name = request.Name,
            Description = request.Description
        };

        db.Disciplines.Add(discipline);
        await db.SaveChangesAsync(cancellationToken);
        return discipline;
    }

    [HttpDelete("{disciplineId:int}")]
    public async Task<IActionResult> Delete(int disciplineId, CancellationToken cancellationToken)
    {
        var discipline = await db.Disciplines.FirstOrDefaultAsync(x => x.Id == disciplineId, cancellationToken);
        if (discipline is null)
        {
            return NotFound();
        }

        var labIds = await db.Labs
            .Where(x => x.DisciplineId == disciplineId)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        if (labIds.Count > 0)
        {
            var labsController = new LabsController(db);
            await labsController.RemoveLabGraphAsync(labIds, cancellationToken);

            var labs = await db.Labs
                .Where(x => labIds.Contains(x.Id))
                .ToListAsync(cancellationToken);

            db.Labs.RemoveRange(labs);
        }

        var generationJobs = await db.GenerationJobs
            .Where(x => x.DisciplineId == disciplineId)
            .ToListAsync(cancellationToken);

        db.GenerationJobs.RemoveRange(generationJobs);
        db.Disciplines.Remove(discipline);

        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
