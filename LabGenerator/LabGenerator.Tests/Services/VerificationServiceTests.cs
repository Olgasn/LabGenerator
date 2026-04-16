using LabGenerator.Domain.Entities;
using LabGenerator.Domain.Enums;
using LabGenerator.Infrastructure.Data;
using LabGenerator.Infrastructure.Services;
using LabGenerator.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace LabGenerator.Tests.Services;

public sealed class VerificationServiceTests
{
    [Fact]
    public async Task VerifyVariantAsync_CreatesPassedReport_WhenJudgePasses()
    {
        await using var db = TestDbContextFactory.Create();
        var (lab, variant) = await SeedLabWithVariantAsync(db);

        var llm = new ScriptedLlmClient();
        llm.Enqueue("variant_judge", """{"passed":true,"score":{"overall":9},"issues":[]}""");

        var svc = new VerificationService(db, llm, new LlmPromptTemplateService());
        var report = await svc.VerifyVariantAsync(variant.Id, CancellationToken.None);

        Assert.True(report.Passed);
        Assert.Contains("9", report.JudgeScoreJson);
        Assert.True(report.JudgeRunId > 0);
        Assert.Single(await db.VerificationReports.ToListAsync());
    }

    [Fact]
    public async Task VerifyVariantAsync_RunsRepairAndReJudge_WhenJudgeFails()
    {
        await using var db = TestDbContextFactory.Create();
        var (lab, variant) = await SeedLabWithVariantAsync(db);

        var llm = new ScriptedLlmClient();
        // First judge: fails
        llm.Enqueue("variant_judge",
            """{"passed":false,"score":{"overall":3},"issues":[{"code":"INCOMPLETE","message":"Задание неполное","severity":"high"}]}""");
        // Repair: returns fixed content
        llm.Enqueue("variant_repair", "Fixed variant content after repair");
        // Second judge after repair: passes
        llm.Enqueue("variant_judge", """{"passed":true,"score":{"overall":8},"issues":[]}""");

        var svc = new VerificationService(db, llm, new LlmPromptTemplateService());
        var report = await svc.VerifyVariantAsync(variant.Id, CancellationToken.None);

        Assert.True(report.Passed);

        // Variant content should be updated by repair
        var updatedVariant = await db.AssignmentVariants.FirstAsync(v => v.Id == variant.Id);
        Assert.Equal("Fixed variant content after repair", updatedVariant.Content);

        // Should have 3 LLM calls: judge, repair, judge
        Assert.Equal(3, llm.Requests.Count);
        Assert.Equal("variant_judge", llm.Requests[0].Purpose);
        Assert.Equal("variant_repair", llm.Requests[1].Purpose);
        Assert.Equal("variant_judge", llm.Requests[2].Purpose);
    }

    [Fact]
    public async Task VerifyVariantAsync_RetriesJudge_WhenFirstResponseInvalid()
    {
        await using var db = TestDbContextFactory.Create();
        var (lab, variant) = await SeedLabWithVariantAsync(db);

        var llm = new ScriptedLlmClient();
        // First judge attempt within JudgeAsync: invalid response
        llm.Enqueue("variant_judge", "this is not valid json at all");
        // Retry within JudgeAsync
        llm.Enqueue("variant_judge", """{"passed":true,"score":{"overall":7},"issues":[]}""");

        var svc = new VerificationService(db, llm, new LlmPromptTemplateService());
        var report = await svc.VerifyVariantAsync(variant.Id, CancellationToken.None);

        Assert.True(report.Passed);
        Assert.Equal(2, llm.Requests.Count(r => r.Purpose == "variant_judge"));
    }

    [Fact]
    public async Task VerifyVariantAsync_CreatesFailedReport_WhenBothJudgeAttemptsInvalid()
    {
        await using var db = TestDbContextFactory.Create();
        var (lab, variant) = await SeedLabWithVariantAsync(db);

        var llm = new ScriptedLlmClient();
        // Both judge attempts return garbage
        llm.Enqueue("variant_judge", "garbage");
        llm.Enqueue("variant_judge", "still garbage");

        var svc = new VerificationService(db, llm, new LlmPromptTemplateService());
        var report = await svc.VerifyVariantAsync(variant.Id, CancellationToken.None);

        Assert.False(report.Passed);
        Assert.Contains("LLM_OUTPUT_INVALID", report.IssuesJson);
    }

    [Fact]
    public async Task VerifyVariantAsync_StillFailedAfterRepair()
    {
        await using var db = TestDbContextFactory.Create();
        var (lab, variant) = await SeedLabWithVariantAsync(db);

        var llm = new ScriptedLlmClient();
        // First judge: fails
        llm.Enqueue("variant_judge",
            """{"passed":false,"score":{"overall":2},"issues":[{"code":"BAD","message":"Плохо","severity":"high"}]}""");
        // Repair
        llm.Enqueue("variant_repair", "Attempted fix");
        // Second judge after repair: still fails
        llm.Enqueue("variant_judge",
            """{"passed":false,"score":{"overall":4},"issues":[{"code":"STILL_BAD","message":"Всё ещё плохо","severity":"medium"}]}""");

        var svc = new VerificationService(db, llm, new LlmPromptTemplateService());
        var report = await svc.VerifyVariantAsync(variant.Id, CancellationToken.None);

        Assert.False(report.Passed);
        Assert.Contains("STILL_BAD", report.IssuesJson);
    }

    [Fact]
    public async Task VerifyVariantAsync_Throws_WhenVariantNotFound()
    {
        await using var db = TestDbContextFactory.Create();
        var llm = new StubLlmClient("unused");
        var svc = new VerificationService(db, llm, new LlmPromptTemplateService());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => svc.VerifyVariantAsync(999, CancellationToken.None));
    }

    [Fact]
    public async Task VerifyVariantAsync_Throws_WhenMasterMissing()
    {
        await using var db = TestDbContextFactory.Create();
        var discipline = new Discipline { Name = "D", Description = "D" };
        db.Disciplines.Add(discipline);
        await db.SaveChangesAsync();

        var lab = new Lab
        {
            DisciplineId = discipline.Id, OrderIndex = 1,
            Title = "L", InitialDescription = "desc"
        };
        db.Labs.Add(lab);
        await db.SaveChangesAsync();

        var variant = new AssignmentVariant
        {
            LabId = lab.Id, VariantIndex = 1, Title = "V",
            Content = "content", Fingerprint = "fp",
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.AssignmentVariants.Add(variant);
        await db.SaveChangesAsync();

        var llm = new StubLlmClient("unused");
        var svc = new VerificationService(db, llm, new LlmPromptTemplateService());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => svc.VerifyVariantAsync(variant.Id, CancellationToken.None));
    }

    [Fact]
    public async Task VerifyVariantAsync_UpdatesExistingReport()
    {
        await using var db = TestDbContextFactory.Create();
        var (lab, variant) = await SeedLabWithVariantAsync(db);

        var llm = new ScriptedLlmClient();
        llm.Enqueue("variant_judge", """{"passed":false,"score":{"overall":3},"issues":[{"code":"A","message":"issue","severity":"low"}]}""");
        // Repair + re-judge for first run
        llm.Enqueue("variant_repair", "repaired");
        llm.Enqueue("variant_judge", """{"passed":false,"score":{"overall":4},"issues":[{"code":"B","message":"still","severity":"low"}]}""");

        var svc = new VerificationService(db, llm, new LlmPromptTemplateService());
        var first = await svc.VerifyVariantAsync(variant.Id, CancellationToken.None);

        // Second run — now passes
        llm.Enqueue("variant_judge", """{"passed":true,"score":{"overall":10},"issues":[]}""");
        var second = await svc.VerifyVariantAsync(variant.Id, CancellationToken.None);

        Assert.Equal(first.Id, second.Id);
        Assert.True(second.Passed);
        Assert.Equal(1, await db.VerificationReports.CountAsync());
    }

    [Fact]
    public async Task VerifyVariantAsync_SavesLlmRuns()
    {
        await using var db = TestDbContextFactory.Create();
        var (lab, variant) = await SeedLabWithVariantAsync(db);

        var llm = new ScriptedLlmClient();
        llm.Enqueue("variant_judge", """{"passed":true,"score":{"overall":9},"issues":[]}""");

        var svc = new VerificationService(db, llm, new LlmPromptTemplateService());
        await svc.VerifyVariantAsync(variant.Id, CancellationToken.None);

        var runs = await db.LLMRuns.ToListAsync();
        Assert.Single(runs);
        Assert.Equal("variant_judge", runs[0].Purpose);
        Assert.Equal("Succeeded", runs[0].Status);
    }

    [Fact]
    public async Task VerifyVariantAsync_HandlesJsonInCodeFence()
    {
        await using var db = TestDbContextFactory.Create();
        var (lab, variant) = await SeedLabWithVariantAsync(db);

        var llm = new ScriptedLlmClient();
        llm.Enqueue("variant_judge", """
            ```json
            {"passed":true,"score":{"overall":8},"issues":[]}
            ```
            """);

        var svc = new VerificationService(db, llm, new LlmPromptTemplateService());
        var report = await svc.VerifyVariantAsync(variant.Id, CancellationToken.None);

        Assert.True(report.Passed);
    }

    private static async Task<(Lab lab, AssignmentVariant variant)> SeedLabWithVariantAsync(ApplicationDbContext db)
    {
        var discipline = new Discipline { Name = "Testing", Description = "Test discipline" };
        db.Disciplines.Add(discipline);
        await db.SaveChangesAsync();

        var lab = new Lab
        {
            DisciplineId = discipline.Id,
            OrderIndex = 1,
            Title = "Lab Verify",
            InitialDescription = "Verify lab"
        };
        db.Labs.Add(lab);
        await db.SaveChangesAsync();

        db.MasterAssignments.Add(new MasterAssignment
        {
            LabId = lab.Id,
            Version = 1,
            IsCurrent = true,
            Status = MasterAssignmentStatus.Approved,
            Content = "# Master\nThis is the master assignment content.",
            CreatedAt = DateTimeOffset.UtcNow
        });

        var variant = new AssignmentVariant
        {
            LabId = lab.Id,
            VariantIndex = 1,
            Title = "Variant 1",
            Content = "Variant content to verify",
            VariantParamsJson = """{"param":"value"}""",
            DifficultyProfileJson = """{"complexity":"medium"}""",
            Fingerprint = "test-fp-1",
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.AssignmentVariants.Add(variant);
        await db.SaveChangesAsync();

        return (lab, variant);
    }
}
