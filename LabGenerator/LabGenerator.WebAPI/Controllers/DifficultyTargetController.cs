using System.Text.Json;
using LabGenerator.Application.Models;
using LabGenerator.Domain.Entities;
using LabGenerator.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LabGenerator.WebAPI.Controllers;

[ApiController]
[Route("api/labs/{labId:int}/difficulty-target")]
public sealed class DifficultyTargetController(
    ApplicationDbContext db,
    IOptions<DifficultyDefaults> defaultsOptions) : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Возвращает текущую цель сложности для ЛР.
    /// IsOverridden=true — задано переопределение в профиле варьирования;
    /// IsOverridden=false — используются глобальные умолчания из difficulty_defaults.json.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<DifficultyTargetDto>> Get(int labId, CancellationToken ct)
    {
        var labExists = await db.Labs.AnyAsync(x => x.Id == labId, ct);
        if (!labExists) return NotFound($"Lab {labId} not found.");

        var profile = await db.VariationProfiles.AsNoTracking()
            .Where(x => x.LabId == labId && x.IsDefault)
            .FirstOrDefaultAsync(ct);

        if (profile?.DifficultyTargetJson is not null)
        {
            var overridden = TryParseTarget(profile.DifficultyTargetJson);
            if (overridden is not null)
                return Ok(new DifficultyTargetDto(
                    overridden.Complexity,
                    overridden.EstimatedHoursMin,
                    overridden.EstimatedHoursMax,
                    IsOverridden: true));
        }

        var d = defaultsOptions.Value;
        return Ok(new DifficultyTargetDto(d.Complexity, d.EstimatedHoursMin, d.EstimatedHoursMax, IsOverridden: false));
    }

    /// <summary>
    /// Устанавливает переопределение сложности для данной ЛР.
    /// Создаёт профиль варьирования по умолчанию, если он ещё не существует.
    /// </summary>
    [HttpPut]
    public async Task<IActionResult> Put(
        int labId,
        [FromBody] SetDifficultyTargetRequest request,
        CancellationToken ct)
    {
        if (!await db.Labs.AnyAsync(x => x.Id == labId, ct))
            return NotFound($"Lab {labId} not found.");

        if (string.IsNullOrWhiteSpace(request.Complexity))
            return BadRequest("Complexity is required.");

        if (request.EstimatedHoursMin < 0 || request.EstimatedHoursMax < request.EstimatedHoursMin)
            return BadRequest("EstimatedHoursMax must be >= EstimatedHoursMin >= 0.");

        var profile = await db.VariationProfiles
            .Where(x => x.LabId == labId && x.IsDefault)
            .FirstOrDefaultAsync(ct);

        if (profile is null)
        {
            profile = new VariationProfile
            {
                LabId = labId,
                Name = "default",
                ParametersJson = "{}",
                DifficultyRubricJson = "{}",
                IsDefault = true,
                CreatedAt = DateTimeOffset.UtcNow
            };
            db.VariationProfiles.Add(profile);
        }

        profile.DifficultyTargetJson = JsonSerializer.Serialize(new DifficultyDefaults
        {
            Complexity = request.Complexity.Trim().ToLowerInvariant(),
            EstimatedHoursMin = request.EstimatedHoursMin,
            EstimatedHoursMax = request.EstimatedHoursMax
        });

        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    /// <summary>
    /// Сбрасывает переопределение сложности — ЛР будет использовать глобальные умолчания.
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> Delete(int labId, CancellationToken ct)
    {
        var profile = await db.VariationProfiles
            .Where(x => x.LabId == labId && x.IsDefault)
            .FirstOrDefaultAsync(ct);

        if (profile is not null)
        {
            profile.DifficultyTargetJson = null;
            await db.SaveChangesAsync(ct);
        }

        return NoContent();
    }

    private static DifficultyDefaults? TryParseTarget(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<DifficultyDefaults>(json, JsonOpts);
        }
        catch
        {
            return null;
        }
    }
}

public sealed record DifficultyTargetDto(
    string Complexity,
    int EstimatedHoursMin,
    int EstimatedHoursMax,
    bool IsOverridden);

public sealed class SetDifficultyTargetRequest
{
    public string Complexity { get; set; } = "medium";
    public int EstimatedHoursMin { get; set; } = 4;
    public int EstimatedHoursMax { get; set; } = 8;
}
