using LabGenerator.Domain.Entities;
using LabGenerator.Domain.Enums;
using LabGenerator.Tests.Helpers;
using LabGenerator.WebAPI.Controllers;
using LabGenerator.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LabGenerator.Tests.Controllers;

public sealed class LabsControllerTests
{
    [Fact]
    public async Task GetAll_ReturnsPagedAndSortedLabs()
    {
        await using var db = TestDbContextFactory.Create();
        var discipline = new Discipline
        {
            Name = "Algorithms"
        };
        db.Disciplines.Add(discipline);
        await db.SaveChangesAsync();

        db.Labs.AddRange(
            new Lab
            {
                DisciplineId = discipline.Id,
                OrderIndex = 1,
                Title = "Alpha",
                InitialDescription = "First"
            },
            new Lab
            {
                DisciplineId = discipline.Id,
                OrderIndex = 2,
                Title = "Beta",
                InitialDescription = "Second"
            },
            new Lab
            {
                DisciplineId = discipline.Id,
                OrderIndex = 3,
                Title = "Gamma",
                InitialDescription = "Third"
            });
        await db.SaveChangesAsync();

        var controller = new LabsController(db);

        var result = await controller.GetAll(
            new GetLabsRequest
            {
                Sort = "asc",
                Page = 2,
                PageSize = 1
            },
            CancellationToken.None);

        var response = Assert.IsType<PagedResponse<Lab>>(result.Value);
        Assert.Equal(3, response.TotalCount);
        Assert.Equal(2, response.Page);
        Assert.Equal(1, response.PageSize);
        Assert.Equal(3, response.TotalPages);
        Assert.Single(response.Items);
        Assert.Equal("Beta", response.Items[0].Title);
    }

    [Fact]
    public async Task GetAll_FiltersByDisciplineId()
    {
        await using var db = TestDbContextFactory.Create();

        var firstDiscipline = new Discipline { Name = "Algorithms" };
        var secondDiscipline = new Discipline { Name = "Databases" };
        db.Disciplines.AddRange(firstDiscipline, secondDiscipline);
        await db.SaveChangesAsync();

        db.Labs.AddRange(
            new Lab
            {
                DisciplineId = firstDiscipline.Id,
                OrderIndex = 1,
                Title = "Alpha",
                InitialDescription = "First"
            },
            new Lab
            {
                DisciplineId = secondDiscipline.Id,
                OrderIndex = 1,
                Title = "Beta",
                InitialDescription = "Second"
            });
        await db.SaveChangesAsync();

        var controller = new LabsController(db);

        var result = await controller.GetAll(
            new GetLabsRequest
            {
                DisciplineId = secondDiscipline.Id,
                All = true
            },
            CancellationToken.None);

        var response = Assert.IsType<PagedResponse<Lab>>(result.Value);
        var item = Assert.Single(response.Items);
        Assert.Equal("Beta", item.Title);
        Assert.Equal(secondDiscipline.Id, item.DisciplineId);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenLabDoesNotExist()
    {
        await using var db = TestDbContextFactory.Create();
        var controller = new LabsController(db);

        var result = await controller.GetById(404, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Delete_RemovesLabAndRelatedEntities()
    {
        await using var db = TestDbContextFactory.Create();

        var discipline = new Discipline { Name = "Algorithms" };
        db.Disciplines.Add(discipline);
        await db.SaveChangesAsync();

        var lab = new Lab
        {
            DisciplineId = discipline.Id,
            OrderIndex = 1,
            Title = "Alpha",
            InitialDescription = "First"
        };
        db.Labs.Add(lab);
        await db.SaveChangesAsync();

        var variationMethod = new VariationMethod
        {
            Code = "subject_domain",
            Name = "Subject domain",
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.VariationMethods.Add(variationMethod);
        await db.SaveChangesAsync();

        var masterAssignment = new MasterAssignment
        {
            LabId = lab.Id,
            Version = 1,
            IsCurrent = true,
            Status = MasterAssignmentStatus.Approved,
            Content = "Master content",
            CreatedAt = DateTimeOffset.UtcNow
        };

        var variationProfile = new VariationProfile
        {
            LabId = lab.Id,
            Name = "Default",
            ParametersJson = "{}",
            DifficultyRubricJson = "{}",
            IsDefault = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var assignmentVariant = new AssignmentVariant
        {
            LabId = lab.Id,
            VariantIndex = 1,
            Title = "Variant",
            Content = "Variant content",
            VariantParamsJson = "{}",
            DifficultyProfileJson = "{}",
            Fingerprint = "fp-1",
            CreatedAt = DateTimeOffset.UtcNow
        };

        var supplementaryMaterial = new LabSupplementaryMaterial
        {
            LabId = lab.Id,
            TheoryMarkdown = "Theory",
            ControlQuestionsJson = "[]",
            SourceFingerprint = "fingerprint",
            CreatedAt = DateTimeOffset.UtcNow
        };

        var labVariationMethod = new LabVariationMethod
        {
            LabId = lab.Id,
            VariationMethodId = variationMethod.Id,
            PreserveAcrossLabs = false,
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.MasterAssignments.Add(masterAssignment);
        db.VariationProfiles.Add(variationProfile);
        db.AssignmentVariants.Add(assignmentVariant);
        db.LabSupplementaryMaterials.Add(supplementaryMaterial);
        db.LabVariationMethods.Add(labVariationMethod);
        await db.SaveChangesAsync();

        db.VerificationReports.Add(new VerificationReport
        {
            AssignmentVariantId = assignmentVariant.Id,
            Passed = true,
            JudgeScoreJson = "{}",
            IssuesJson = "[]",
            CreatedAt = DateTimeOffset.UtcNow
        });
        db.Set<AssignmentVariantVariationValue>().Add(new AssignmentVariantVariationValue
        {
            AssignmentVariantId = assignmentVariant.Id,
            VariationMethodId = variationMethod.Id,
            Value = "Retail",
            CreatedAt = DateTimeOffset.UtcNow
        });
        db.GenerationJobs.AddRange(
            new GenerationJob
            {
                Type = GenerationJobType.GenerateVariants,
                Status = GenerationJobStatus.Pending,
                LabId = lab.Id,
                VariationProfileId = variationProfile.Id,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new GenerationJob
            {
                Type = GenerationJobType.GenerateMasterAssignment,
                Status = GenerationJobStatus.Pending,
                LabId = lab.Id,
                MasterAssignmentId = masterAssignment.Id,
                CreatedAt = DateTimeOffset.UtcNow
            });
        await db.SaveChangesAsync();

        var controller = new LabsController(db);

        var result = await controller.Delete(lab.Id, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(0, await db.Labs.CountAsync());
        Assert.Equal(0, await db.MasterAssignments.CountAsync());
        Assert.Equal(0, await db.VariationProfiles.CountAsync());
        Assert.Equal(0, await db.AssignmentVariants.CountAsync());
        Assert.Equal(0, await db.LabSupplementaryMaterials.CountAsync());
        Assert.Equal(0, await db.LabVariationMethods.CountAsync());
        Assert.Equal(0, await db.VerificationReports.CountAsync());
        Assert.Equal(0, await db.Set<AssignmentVariantVariationValue>().CountAsync());
        Assert.Equal(0, await db.GenerationJobs.CountAsync());
        Assert.Equal(1, await db.Disciplines.CountAsync());
    }
}
