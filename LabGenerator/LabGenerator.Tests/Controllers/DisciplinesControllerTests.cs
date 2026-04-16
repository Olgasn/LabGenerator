using LabGenerator.Domain.Entities;
using LabGenerator.Domain.Enums;
using LabGenerator.Tests.Helpers;
using LabGenerator.WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LabGenerator.Tests.Controllers;

public sealed class DisciplinesControllerTests
{
    [Fact]
    public async Task Delete_RemovesDisciplineLabsAndRelatedJobs()
    {
        await using var db = TestDbContextFactory.Create();

        var discipline = new Discipline { Name = "Algorithms" };
        db.Disciplines.Add(discipline);
        await db.SaveChangesAsync();

        var lab = new Lab
        {
            DisciplineId = discipline.Id,
            OrderIndex = 1,
            Title = "Lab 1",
            InitialDescription = "First"
        };
        db.Labs.Add(lab);
        await db.SaveChangesAsync();

        db.GenerationJobs.AddRange(
            new GenerationJob
            {
                Type = GenerationJobType.GenerateMasterAssignment,
                Status = GenerationJobStatus.Pending,
                DisciplineId = discipline.Id,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new GenerationJob
            {
                Type = GenerationJobType.GenerateVariants,
                Status = GenerationJobStatus.Pending,
                LabId = lab.Id,
                CreatedAt = DateTimeOffset.UtcNow
            });

        db.MasterAssignments.Add(new MasterAssignment
        {
            LabId = lab.Id,
            Version = 1,
            IsCurrent = true,
            Status = MasterAssignmentStatus.Approved,
            Content = "Master content",
            CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        var controller = new DisciplinesController(db);

        var result = await controller.Delete(discipline.Id, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(0, await db.Disciplines.CountAsync());
        Assert.Equal(0, await db.Labs.CountAsync());
        Assert.Equal(0, await db.MasterAssignments.CountAsync());
        Assert.Equal(0, await db.GenerationJobs.CountAsync());
    }
}
