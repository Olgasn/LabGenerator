using LabGenerator.Domain.Entities;
using LabGenerator.Infrastructure.Data;
using LabGenerator.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace LabGenerator.WebAPI.Controllers;

[ApiController]
[Route("api/variation-methods")]
public class VariationMethodsController(ApplicationDbContext db) : ControllerBase
{
    private static string Slugify(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        var s = input.Trim().ToLowerInvariant();
        var sb = new StringBuilder(s.Length);
        var prevUnderscore = false;

        foreach (var ch in s)
        {
            var isAsciiLetter = ch is >= 'a' and <= 'z';
            var isDigit = ch is >= '0' and <= '9';

            if (isAsciiLetter || isDigit)
            {
                sb.Append(ch);
                prevUnderscore = false;
                continue;
            }

            if (!prevUnderscore)
            {
                sb.Append('_');
                prevUnderscore = true;
            }
        }

        return sb.ToString().Trim('_');
    }

    private async Task<string> GenerateUniqueCodeAsync(string baseCode, CancellationToken cancellationToken)
    {
        var code = string.IsNullOrWhiteSpace(baseCode) ? "param" : baseCode;

        for (var i = 0; i < 200; i++)
        {
            var candidate = i == 0 ? code : $"{code}_{i + 1}";
            var exists = await db.VariationMethods.AnyAsync(x => x.Code == candidate, cancellationToken);
            if (!exists) return candidate;
        }

        return $"{code}_{Guid.NewGuid():N}"[..Math.Min(50, code.Length + 33)];
    }

    [HttpGet]
    public Task<List<VariationMethod>> GetAll(CancellationToken cancellationToken)
        => db.VariationMethods.AsNoTracking()
            .OrderBy(x => x.Id)
            .ToListAsync(cancellationToken);

    [HttpPost]
    public async Task<ActionResult<VariationMethod>> Create(
        [FromBody] CreateVariationMethodRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest("Name is required");

        var provided = request.Code?.Trim();
        var baseCode = !string.IsNullOrWhiteSpace(provided)
            ? provided
            : Slugify(request.Name);

        if (!string.IsNullOrWhiteSpace(provided))
        {
            var exists = await db.VariationMethods.AnyAsync(x => x.Code == provided, cancellationToken);
            if (exists) return Conflict("VariationMethod with such code already exists");
        }

        var code = await GenerateUniqueCodeAsync(baseCode, cancellationToken);

        var entity = new VariationMethod
        {
            Code = code,
            Name = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            IsSystem = false,
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.VariationMethods.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<VariationMethod>> Update(
        int id,
        [FromBody] UpdateVariationMethodRequest request,
        CancellationToken cancellationToken)
    {
        var entity = await db.VariationMethods.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return NotFound();

        if (entity.IsSystem)
        {
            entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

            await db.SaveChangesAsync(cancellationToken);
            return entity;
        }

        if (string.IsNullOrWhiteSpace(request.Code)) return BadRequest("Code is required");
        if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest("Name is required");

        var code = request.Code.Trim();
        var codeTaken = await db.VariationMethods.AnyAsync(x => x.Id != id && x.Code == code, cancellationToken);
        if (codeTaken) return Conflict("VariationMethod with such code already exists");

        entity.Code = code;
        entity.Name = request.Name.Trim();
        entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

        await db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var entity = await db.VariationMethods.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return NotFound();

        if (entity.IsSystem) return BadRequest("System variation methods cannot be deleted.");

        db.VariationMethods.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}