using LabGenerator.Domain.Entities;
using LabGenerator.Domain.Enums;
using LabGenerator.Infrastructure.Services;
using LabGenerator.Tests.Helpers;

namespace LabGenerator.Tests.Services;

public sealed class MasterAssignmentServiceTests
{
    [Fact]
    public async Task GenerateDraftAsync_CreatesCurrentDraftAndLlmRun()
    {
        await using var db = TestDbContextFactory.Create();
        var lab = await SeedLabAsync(db);
        var llm = new StubLlmClient("Generated master content");
        var service = new MasterAssignmentService(db, llm, new LlmPromptTemplateService(), new PromptCustomSectionService(db));

        var result = await service.GenerateDraftAsync(lab.Id, CancellationToken.None);

        Assert.Equal(1, result.Version);
        Assert.True(result.IsCurrent);
        Assert.Equal(MasterAssignmentStatus.Draft, result.Status);
        Assert.Equal("Generated master content", result.Content);
        Assert.Single(db.MasterAssignments);
        Assert.Single(db.LLMRuns);
        Assert.Equal(1, llm.CallCount);
    }

    [Fact]
    public async Task GenerateDraftAsync_ReplacesCurrentVersion()
    {
        await using var db = TestDbContextFactory.Create();
        var lab = await SeedLabAsync(db);

        var previous = new MasterAssignment
        {
            LabId = lab.Id,
            Version = 1,
            IsCurrent = true,
            Status = MasterAssignmentStatus.Draft,
            Content = "Previous content",
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.MasterAssignments.Add(previous);
        await db.SaveChangesAsync();

        var service = new MasterAssignmentService(db, new StubLlmClient("New content"), new LlmPromptTemplateService(), new PromptCustomSectionService(db));

        var result = await service.GenerateDraftAsync(lab.Id, CancellationToken.None);
        var all = db.MasterAssignments.OrderBy(x => x.Version).ToList();

        Assert.Equal(2, all.Count);
        Assert.False(previous.IsCurrent);
        Assert.NotNull(previous.UpdatedAt);
        Assert.Equal(2, result.Version);
        Assert.True(result.IsCurrent);
        Assert.Equal("New content", result.Content);
        Assert.Equal(1, all.Count(x => x.IsCurrent));
    }

    [Fact]
    public async Task UpdateContentAsync_CreatesNewDraftVersion_ForApprovedMaster()
    {
        await using var db = TestDbContextFactory.Create();
        var lab = await SeedLabAsync(db);

        var approvedMaster = new MasterAssignment
        {
            LabId = lab.Id,
            Version = 1,
            IsCurrent = true,
            Status = MasterAssignmentStatus.Approved,
            Content = "Approved content",
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.MasterAssignments.Add(approvedMaster);
        await db.SaveChangesAsync();

        var service = new MasterAssignmentService(db, new StubLlmClient("Unused"), new LlmPromptTemplateService(), new PromptCustomSectionService(db));

        var updated = await service.UpdateContentAsync(approvedMaster.Id, "New content", CancellationToken.None);
        var all = db.MasterAssignments.OrderBy(x => x.Version).ToList();

        Assert.Equal(2, all.Count);
        Assert.Equal(2, updated.Version);
        Assert.True(updated.IsCurrent);
        Assert.Equal(MasterAssignmentStatus.Draft, updated.Status);
        Assert.Equal("New content", updated.Content);

        Assert.False(approvedMaster.IsCurrent);
        Assert.Equal(MasterAssignmentStatus.Approved, approvedMaster.Status);
    }

    [Fact]
    public async Task UpdateContentAsync_UpdatesDraftInPlace()
    {
        await using var db = TestDbContextFactory.Create();
        var lab = await SeedLabAsync(db);

        var draftMaster = new MasterAssignment
        {
            LabId = lab.Id,
            Version = 1,
            IsCurrent = true,
            Status = MasterAssignmentStatus.Draft,
            Content = "Old content",
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.MasterAssignments.Add(draftMaster);
        await db.SaveChangesAsync();

        var service = new MasterAssignmentService(db, new StubLlmClient("Unused"), new LlmPromptTemplateService(), new PromptCustomSectionService(db));
        var updated = await service.UpdateContentAsync(draftMaster.Id, "Updated content", CancellationToken.None);

        Assert.Equal(draftMaster.Id, updated.Id);
        Assert.Equal("Updated content", updated.Content);
        Assert.True(updated.IsCurrent);
        Assert.Equal(MasterAssignmentStatus.Draft, updated.Status);
        Assert.NotNull(updated.UpdatedAt);
        Assert.Single(db.MasterAssignments);
    }

    private static async Task<Lab> SeedLabAsync(LabGenerator.Infrastructure.Data.ApplicationDbContext db)
    {
        var discipline = new Discipline
        {
            Name = "Programming",
            Description = "Base discipline"
        };

        db.Disciplines.Add(discipline);
        await db.SaveChangesAsync();

        var lab = new Lab
        {
            DisciplineId = discipline.Id,
            OrderIndex = 1,
            Title = "Lab 1",
            InitialDescription = "Create a console app"
        };

        db.Labs.Add(lab);
        await db.SaveChangesAsync();
        return lab;
    }
}
