using LabGenerator.Domain.Entities;
using LabGenerator.Domain.Enums;
using LabGenerator.Infrastructure.Services;
using LabGenerator.Tests.Helpers;
using LabGenerator.WebAPI.Controllers;
using LabGenerator.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace LabGenerator.Tests.Controllers;

public sealed class VariantsControllerTests
{
    [Fact]
    public async Task GetAll_ReturnsPagedVariants()
    {
        await using var db = TestDbContextFactory.Create();
        var lab = await SeedLabAsync(db);
        db.AssignmentVariants.AddRange(
            new AssignmentVariant
            {
                LabId = lab.Id,
                VariantIndex = 1,
                Title = "Variant 1",
                Content = "Content 1",
                VariantParamsJson = "{}",
                DifficultyProfileJson = "{}",
                Fingerprint = "fp-1",
                CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-2)
            },
            new AssignmentVariant
            {
                LabId = lab.Id,
                VariantIndex = 2,
                Title = "Variant 2",
                Content = "Content 2",
                VariantParamsJson = "{}",
                DifficultyProfileJson = "{}",
                Fingerprint = "fp-2",
                CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-1)
            },
            new AssignmentVariant
            {
                LabId = lab.Id,
                VariantIndex = 3,
                Title = "Variant 3",
                Content = "Content 3",
                VariantParamsJson = "{}",
                DifficultyProfileJson = "{}",
                Fingerprint = "fp-3",
                CreatedAt = DateTimeOffset.UtcNow
            });
        await db.SaveChangesAsync();

        var controller = new VariantsController(db, new LlmAccessGuardService(db));

        var result = await controller.GetAll(
            lab.Id,
            new GetVariantsRequest
            {
                Sort = "desc",
                Page = 2,
                PageSize = 1
            },
            CancellationToken.None);

        var response = Assert.IsType<PagedResponse<AssignmentVariant>>(result.Value);
        Assert.Equal(3, response.TotalCount);
        Assert.Equal(2, response.Page);
        Assert.Equal(1, response.PageSize);
        Assert.Equal(3, response.TotalPages);
        Assert.Single(response.Items);
        Assert.Equal(2, response.Items[0].VariantIndex);
    }

    [Fact]
    public async Task Generate_ReturnsBadRequest_WhenMasterIsMissing()
    {
        await using var db = TestDbContextFactory.Create();
        var lab = await SeedLabAsync(db);
        var controller = new VariantsController(db, new LlmAccessGuardService(db));

        var result = await controller.Generate(
            lab.Id,
            new GenerateVariantsRequest { Count = 3 },
            CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("MasterAssignment is not generated.", badRequest.Value);
    }

    [Fact]
    public async Task Generate_ReturnsBadRequest_WhenMasterIsNotApproved()
    {
        await using var db = TestDbContextFactory.Create();
        var lab = await SeedLabAsync(db);
        db.MasterAssignments.Add(new MasterAssignment
        {
            LabId = lab.Id,
            Version = 1,
            IsCurrent = true,
            Status = MasterAssignmentStatus.Draft,
            Content = "Draft master",
            CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        var controller = new VariantsController(db, new LlmAccessGuardService(db));
        var result = await controller.Generate(
            lab.Id,
            new GenerateVariantsRequest { Count = 2 },
            CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("MasterAssignment must be approved before generating variants.", badRequest.Value);
    }

    [Fact]
    public async Task Generate_CreatesPendingJob_WhenMasterApproved()
    {
        await using var db = TestDbContextFactory.Create();
        var lab = await SeedLabAsync(db);
        db.LlmSettings.Add(new LlmSettings
        {
            Provider = "Ollama",
            Model = string.Empty,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        db.LlmProviderSettings.Add(new LlmProviderSettings
        {
            Provider = "Ollama",
            ApiKey = "test-api-key",
            UpdatedAt = DateTimeOffset.UtcNow
        });
        db.MasterAssignments.Add(new MasterAssignment
        {
            LabId = lab.Id,
            Version = 1,
            IsCurrent = true,
            Status = MasterAssignmentStatus.Approved,
            Content = "Approved master",
            CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        var controller = new VariantsController(db, new LlmAccessGuardService(db));
        var result = await controller.Generate(
            lab.Id,
            new GenerateVariantsRequest { Count = 4 },
            CancellationToken.None);

        var createdJob = Assert.IsType<GenerationJob>(result.Value);
        Assert.True(createdJob.Id > 0);
        Assert.Equal(GenerationJobType.GenerateVariants, createdJob.Type);
        Assert.Equal(GenerationJobStatus.Pending, createdJob.Status);
        Assert.Equal(lab.Id, createdJob.LabId);
        Assert.Equal(1, db.GenerationJobs.Count());
        Assert.Contains("\"count\":4", createdJob.PayloadJson ?? string.Empty, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Generate_ReturnsBadRequest_WhenApiKeyMissing()
    {
        await using var db = TestDbContextFactory.Create();
        var lab = await SeedLabAsync(db);
        db.LlmSettings.Add(new LlmSettings
        {
            Provider = "OpenRouter",
            Model = string.Empty,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        db.MasterAssignments.Add(new MasterAssignment
        {
            LabId = lab.Id,
            Version = 1,
            IsCurrent = true,
            Status = MasterAssignmentStatus.Approved,
            Content = "Approved master",
            CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        var controller = new VariantsController(db, new LlmAccessGuardService(db));
        var result = await controller.Generate(
            lab.Id,
            new GenerateVariantsRequest { Count = 2 },
            CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("API key for provider 'OpenRouter' is not configured in settings.", badRequest.Value);
    }

    private static async Task<Lab> SeedLabAsync(LabGenerator.Infrastructure.Data.ApplicationDbContext db)
    {
        var discipline = new Discipline
        {
            Name = "Algorithms",
            Description = "Algorithms discipline"
        };
        db.Disciplines.Add(discipline);
        await db.SaveChangesAsync();

        var lab = new Lab
        {
            DisciplineId = discipline.Id,
            OrderIndex = 1,
            Title = "Lab",
            InitialDescription = "Implement graph search"
        };
        db.Labs.Add(lab);
        await db.SaveChangesAsync();

        return lab;
    }
}
