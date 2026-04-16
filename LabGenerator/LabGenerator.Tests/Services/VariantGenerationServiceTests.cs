using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LabGenerator.Application.Models;
using LabGenerator.Domain.Entities;
using LabGenerator.Domain.Enums;
using LabGenerator.Infrastructure.Data;
using LabGenerator.Infrastructure.Services;
using LabGenerator.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace LabGenerator.Tests.Services;

public sealed class VariantGenerationServiceTests
{
    [Fact]
    public async Task GenerateVariantsAsync_Throws_WhenLabNotFound()
    {
        await using var db = TestDbContextFactory.Create();
        var llm = new ScriptedLlmClient();
        var service = CreateService(db, llm);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GenerateVariantsAsync(999, 1, null, CancellationToken.None));

        Assert.Equal("Lab 999 not found.", ex.Message);
    }

    [Fact]
    public async Task GenerateVariantsAsync_Throws_WhenMasterAssignmentMissing()
    {
        await using var db = TestDbContextFactory.Create();
        var discipline = await CreateDisciplineAsync(db, "Networks");
        var lab = await CreateLabAsync(db, discipline.Id, orderIndex: 1, title: "Lab");

        var llm = new ScriptedLlmClient();
        var service = CreateService(db, llm);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GenerateVariantsAsync(lab.Id, 1, null, CancellationToken.None));

        Assert.Equal("MasterAssignment is not generated.", ex.Message);
    }

    [Fact]
    public async Task GenerateVariantsAsync_Throws_WhenMasterAssignmentNotApproved()
    {
        await using var db = TestDbContextFactory.Create();
        var discipline = await CreateDisciplineAsync(db, "Operating systems");
        var lab = await CreateLabAsync(db, discipline.Id, orderIndex: 1, title: "Lab");

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

        var llm = new ScriptedLlmClient();
        var service = CreateService(db, llm);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GenerateVariantsAsync(lab.Id, 1, null, CancellationToken.None));

        Assert.Equal("MasterAssignment must be approved before generating variants.", ex.Message);
    }

    [Fact]
    public async Task GenerateVariantsAsync_RetriesRawGenerationAttempt_WhenJsonIsInvalid()
    {
        await using var db = TestDbContextFactory.Create();
        var discipline = await CreateDisciplineAsync(db, "Databases");
        var lab = await CreateLabAsync(db, discipline.Id, orderIndex: 1, title: "Lab");
        await AddApprovedMasterAsync(db, lab.Id, "Approved master");

        var llm = new ScriptedLlmClient();
        llm.Enqueue("variant_generation", "this is not valid json");
        llm.Enqueue("variant_generation", BuildVariantJson("Variant A", "Content A"));

        var service = CreateService(db, llm);

        var result = await service.GenerateVariantsAsync(lab.Id, 1, null, CancellationToken.None);

        var created = Assert.Single(result);
        Assert.Equal("Variant A", created.Title);
        Assert.Equal("Content A", created.Content);
        Assert.Equal(2, llm.Requests.Count(x => x.Purpose == "variant_generation"));
        Assert.DoesNotContain(llm.Requests, x => x.Purpose == "constraints_check");
        Assert.Equal(2, await db.LLMRuns.CountAsync());
    }

    [Fact]
    public async Task GenerateVariantsAsync_UsesExplicitVariationProfileInPrompt()
    {
        await using var db = TestDbContextFactory.Create();
        var discipline = await CreateDisciplineAsync(db, "Algorithms");
        var lab = await CreateLabAsync(db, discipline.Id, orderIndex: 1, title: "Lab");
        await AddApprovedMasterAsync(db, lab.Id, "Approved master");

        var defaultProfile = new VariationProfile
        {
            LabId = lab.Id,
            Name = "Default",
            ParametersJson = "{\"profile\":\"DEFAULT_PROFILE_MARKER\"}",
            DifficultyRubricJson = "{}",
            IsDefault = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var explicitProfile = new VariationProfile
        {
            LabId = lab.Id,
            Name = "Explicit",
            ParametersJson = "{\"profile\":\"EXPLICIT_PROFILE_MARKER\"}",
            DifficultyRubricJson = "{}",
            IsDefault = false,
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.VariationProfiles.AddRange(defaultProfile, explicitProfile);
        await db.SaveChangesAsync();

        var llm = new ScriptedLlmClient();
        llm.Enqueue("variant_generation", BuildVariantJson("Variant A", "Content A"));

        var service = CreateService(db, llm);
        var result = await service.GenerateVariantsAsync(lab.Id, 1, explicitProfile.Id, CancellationToken.None);

        Assert.Single(result);
        var generationRequest = Assert.Single(llm.Requests, x => x.Purpose == "variant_generation");
        Assert.Contains("EXPLICIT_PROFILE_MARKER", generationRequest.UserPrompt, StringComparison.Ordinal);
        Assert.DoesNotContain("DEFAULT_PROFILE_MARKER", generationRequest.UserPrompt, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GenerateVariantsAsync_IncludesMethodDescriptionInPrompt()
    {
        await using var db = TestDbContextFactory.Create();
        var discipline = await CreateDisciplineAsync(db, "Software testing");
        var lab = await CreateLabAsync(db, discipline.Id, orderIndex: 1, title: "Lab");
        await AddApprovedMasterAsync(db, lab.Id, "Approved master");

        var method = new VariationMethod
        {
            Code = "subject_domain",
            Name = "Subject domain",
            Description = "Pick a concrete business domain and keep it explicit in variant_params.",
            IsSystem = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.VariationMethods.Add(method);
        await db.SaveChangesAsync();

        db.LabVariationMethods.Add(new LabVariationMethod
        {
            LabId = lab.Id,
            VariationMethodId = method.Id,
            PreserveAcrossLabs = false,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        var llm = new ScriptedLlmClient();
        llm.Enqueue(
            "variant_generation",
            BuildVariantJson(
                "Variant A",
                "Content A",
                new Dictionary<string, string> { ["subject_domain"] = "Healthcare" }));

        var service = CreateService(db, llm);
        var result = await service.GenerateVariantsAsync(lab.Id, 1, null, CancellationToken.None);

        Assert.Single(result);
        var generationRequest = Assert.Single(llm.Requests, x => x.Purpose == "variant_generation");
        Assert.Contains(
            "Pick a concrete business domain and keep it explicit in variant_params.",
            generationRequest.UserPrompt,
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task GenerateVariantsAsync_Retries_WhenRequiredVariantParamIsMissing()
    {
        await using var db = TestDbContextFactory.Create();
        var discipline = await CreateDisciplineAsync(db, "Architecture");
        var lab = await CreateLabAsync(db, discipline.Id, orderIndex: 1, title: "Lab");
        await AddApprovedMasterAsync(db, lab.Id, "Approved master");

        var method = new VariationMethod
        {
            Code = "subject_domain",
            Name = "Subject domain",
            IsSystem = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.VariationMethods.Add(method);
        await db.SaveChangesAsync();

        db.LabVariationMethods.Add(new LabVariationMethod
        {
            LabId = lab.Id,
            VariationMethodId = method.Id,
            PreserveAcrossLabs = false,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        var llm = new ScriptedLlmClient();
        llm.Enqueue(
            "variant_generation",
            BuildVariantJson(
                "Bad",
                "Bad content",
                new Dictionary<string, string> { ["matrix_type"] = "sparse" }));
        llm.Enqueue(
            "variant_generation",
            BuildVariantJson(
                "Good",
                "Good content",
                new Dictionary<string, string> { ["subject_domain"] = "Retail" }));

        var service = CreateService(db, llm);
        var result = await service.GenerateVariantsAsync(lab.Id, 1, null, CancellationToken.None);

        var created = Assert.Single(result);
        Assert.Equal("Retail", ReadVariantParam(created.VariantParamsJson, "subject_domain"));
        Assert.Equal(2, llm.Requests.Count(x => x.Purpose == "variant_generation"));

        var retryPrompt = llm.Requests
            .Where(x => x.Purpose == "variant_generation")
            .Skip(1)
            .Single()
            .UserPrompt;
        Assert.Contains("variant_params", retryPrompt, StringComparison.Ordinal);
        Assert.Contains("subject_domain", retryPrompt, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GenerateVariantsAsync_Retries_WhenConstraintsCheckFlagsDifficultyMismatch()
    {
        await using var db = TestDbContextFactory.Create();
        var discipline = await CreateDisciplineAsync(db, "Algorithms");
        var lab = await CreateLabAsync(db, discipline.Id, orderIndex: 1, title: "Lab");
        await AddApprovedMasterAsync(db, lab.Id, """
            # Assignment
            Implement Dijkstra shortest path algorithm for a weighted graph.

            # Deliverables
            Provide the source code and a short explanation of the chosen data structures.
            """);

        db.AssignmentVariants.Add(CreateVariant(
            lab.Id,
            variantIndex: 1,
            title: "Existing variant",
            content: "Existing content",
            variantParamsJson: "{}",
            fingerprint: "existing-fp"));
        await db.SaveChangesAsync();

        var llm = new ScriptedLlmClient();
        llm.Enqueue(
            "variant_generation",
            BuildVariantJson(
                "Too strict",
                """
                # Assignment
                Implement Dijkstra shortest path algorithm for a weighted graph.

                # Deliverables
                Provide the source code and a short explanation of the chosen data structures.
                Students must achieve coverage of at least 95 percent.
                """));
        llm.Enqueue(
            "constraints_check",
            BuildConstraintsCheckJson(
                isUnique: true,
                difficultyConsistent: false,
                difficultyReason: "Coverage requirement makes the task too large."));
        llm.Enqueue(
            "variant_generation",
            BuildVariantJson(
                "Valid",
                """
                # Assignment
                Implement Dijkstra shortest path algorithm for a weighted graph.

                # Deliverables
                Provide the source code and a short explanation of the chosen data structures.
                Add one custom scenario that highlights path reconstruction.
                """));
        llm.Enqueue("constraints_check", BuildConstraintsCheckJson());

        var service = CreateService(db, llm);
        var result = await service.GenerateVariantsAsync(lab.Id, 1, null, CancellationToken.None);

        var created = Assert.Single(result);
        Assert.Equal("Valid", created.Title);
        Assert.Equal(2, llm.Requests.Count(x => x.Purpose == "variant_generation"));
        Assert.Equal(2, llm.Requests.Count(x => x.Purpose == "constraints_check"));

        var retryPrompt = llm.Requests
            .Where(x => x.Purpose == "variant_generation")
            .Skip(1)
            .Single()
            .UserPrompt;
        Assert.Contains("complexity", retryPrompt, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("coverage", retryPrompt, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GenerateVariantsAsync_Retries_WhenPreservedValueIsViolated()
    {
        await using var db = TestDbContextFactory.Create();
        var discipline = await CreateDisciplineAsync(db, "Math");
        var previousLab = await CreateLabAsync(db, discipline.Id, orderIndex: 1, title: "Previous lab");
        var currentLab = await CreateLabAsync(db, discipline.Id, orderIndex: 2, title: "Current lab");
        await AddApprovedMasterAsync(db, currentLab.Id, "Approved master");

        db.AssignmentVariants.Add(CreateVariant(
            previousLab.Id,
            variantIndex: 1,
            title: "Prev variant",
            content: "Previous content",
            variantParamsJson: "{\"subject_domain\":\"FixedDomain\"}",
            fingerprint: "prev-fp"));

        var method = new VariationMethod
        {
            Code = "subject_domain",
            Name = "Subject domain",
            IsSystem = false,
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.VariationMethods.Add(method);
        await db.SaveChangesAsync();

        db.LabVariationMethods.Add(new LabVariationMethod
        {
            LabId = currentLab.Id,
            VariationMethodId = method.Id,
            PreserveAcrossLabs = true,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        var llm = new ScriptedLlmClient();
        llm.Enqueue(
            "variant_generation",
            BuildVariantJson(
                "Wrong",
                "Wrong content",
                new Dictionary<string, string> { ["subject_domain"] = "WrongDomain" }));
        llm.Enqueue(
            "variant_generation",
            BuildVariantJson(
                "Correct",
                "Correct content",
                new Dictionary<string, string> { ["subject_domain"] = "FixedDomain" }));

        var service = CreateService(db, llm);

        var result = await service.GenerateVariantsAsync(currentLab.Id, 1, null, CancellationToken.None);
        var created = Assert.Single(result);

        Assert.Equal("FixedDomain", ReadVariantParam(created.VariantParamsJson, "subject_domain"));
        Assert.Equal(2, llm.Requests.Count(x => x.Purpose == "variant_generation"));
        Assert.DoesNotContain(llm.Requests, x => x.Purpose == "constraints_check");
    }

    [Fact]
    public async Task GenerateVariantsAsync_Retries_WhenValueAlreadyUsedWithinLab()
    {
        await using var db = TestDbContextFactory.Create();
        var discipline = await CreateDisciplineAsync(db, "Compilers");
        var lab = await CreateLabAsync(db, discipline.Id, orderIndex: 1, title: "Lab");
        await AddApprovedMasterAsync(db, lab.Id, "Approved master");

        db.AssignmentVariants.Add(CreateVariant(
            lab.Id,
            variantIndex: 1,
            title: "Existing variant",
            content: "Existing content",
            variantParamsJson: "{\"subject_domain\":\"UsedDomain\"}",
            fingerprint: "existing-fp"));

        var method = new VariationMethod
        {
            Code = "subject_domain",
            Name = "Subject domain",
            IsSystem = false,
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.VariationMethods.Add(method);
        await db.SaveChangesAsync();

        db.LabVariationMethods.Add(new LabVariationMethod
        {
            LabId = lab.Id,
            VariationMethodId = method.Id,
            PreserveAcrossLabs = false,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        var llm = new ScriptedLlmClient();
        llm.Enqueue(
            "variant_generation",
            BuildVariantJson(
                "Bad",
                "Bad content",
                new Dictionary<string, string> { ["subject_domain"] = "UsedDomain" }));
        llm.Enqueue(
            "variant_generation",
            BuildVariantJson(
                "Good",
                "Good content",
                new Dictionary<string, string> { ["subject_domain"] = "FreshDomain" }));
        llm.Enqueue("constraints_check", BuildConstraintsCheckJson());

        var service = CreateService(db, llm);
        var result = await service.GenerateVariantsAsync(lab.Id, 1, null, CancellationToken.None);

        var created = Assert.Single(result);
        Assert.Equal(2, created.VariantIndex);
        Assert.Equal("FreshDomain", ReadVariantParam(created.VariantParamsJson, "subject_domain"));
        Assert.Equal(2, llm.Requests.Count(x => x.Purpose == "variant_generation"));
        Assert.Equal(1, llm.Requests.Count(x => x.Purpose == "constraints_check"));
    }

    [Fact]
    public async Task GenerateVariantsAsync_Retries_WhenUniquenessCheckFails()
    {
        await using var db = TestDbContextFactory.Create();
        var discipline = await CreateDisciplineAsync(db, "AI");
        var lab = await CreateLabAsync(db, discipline.Id, orderIndex: 1, title: "Lab");
        await AddApprovedMasterAsync(db, lab.Id, "Approved master");

        db.AssignmentVariants.Add(CreateVariant(
            lab.Id,
            variantIndex: 1,
            title: "Existing variant",
            content: "Existing content",
            variantParamsJson: "{}",
            fingerprint: "existing-fp"));
        await db.SaveChangesAsync();

        var llm = new ScriptedLlmClient();
        llm.Enqueue("variant_generation", BuildVariantJson("Candidate 1", "First candidate"));
        llm.Enqueue(
            "constraints_check",
            BuildConstraintsCheckJson(
                isUnique: false,
                mostSimilarTo: "Existing variant",
                similarityReason: "Too similar"));
        llm.Enqueue("variant_generation", BuildVariantJson("Candidate 2", "Second candidate"));
        llm.Enqueue("constraints_check", BuildConstraintsCheckJson());

        var service = CreateService(db, llm);
        var result = await service.GenerateVariantsAsync(lab.Id, 1, null, CancellationToken.None);

        var created = Assert.Single(result);
        Assert.Equal("Second candidate", created.Content);
        Assert.Equal(2, llm.Requests.Count(x => x.Purpose == "variant_generation"));
        Assert.Equal(2, llm.Requests.Count(x => x.Purpose == "constraints_check"));
    }

    [Fact]
    public async Task GenerateVariantsAsync_Retries_WhenConstraintsCheckJsonIsInvalid()
    {
        await using var db = TestDbContextFactory.Create();
        var discipline = await CreateDisciplineAsync(db, "ML");
        var lab = await CreateLabAsync(db, discipline.Id, orderIndex: 1, title: "Lab");
        await AddApprovedMasterAsync(db, lab.Id, "Approved master");

        db.AssignmentVariants.Add(CreateVariant(
            lab.Id,
            variantIndex: 1,
            title: "Existing variant",
            content: "Existing content",
            variantParamsJson: "{}",
            fingerprint: "existing-fp"));
        await db.SaveChangesAsync();

        var llm = new ScriptedLlmClient();
        llm.Enqueue("variant_generation", BuildVariantJson("Candidate 1", "First candidate"));
        llm.Enqueue("constraints_check", "not valid json");
        llm.Enqueue("variant_generation", BuildVariantJson("Candidate 2", "Second candidate"));
        llm.Enqueue("constraints_check", BuildConstraintsCheckJson());

        var service = CreateService(db, llm);
        var result = await service.GenerateVariantsAsync(lab.Id, 1, null, CancellationToken.None);

        var created = Assert.Single(result);
        Assert.Equal("Second candidate", created.Content);
        Assert.Equal(2, llm.Requests.Count(x => x.Purpose == "variant_generation"));
        Assert.Equal(2, llm.Requests.Count(x => x.Purpose == "constraints_check"));
    }

    [Fact]
    public async Task GenerateVariantsAsync_ThrowsAfterFiveRounds_WhenAllAttemptsInvalid()
    {
        await using var db = TestDbContextFactory.Create();
        var discipline = await CreateDisciplineAsync(db, "Distributed systems");
        var lab = await CreateLabAsync(db, discipline.Id, orderIndex: 1, title: "Lab");
        await AddApprovedMasterAsync(db, lab.Id, "Approved master");

        var llm = new ScriptedLlmClient();
        for (var i = 0; i < 15; i++)
        {
            llm.Enqueue("variant_generation", "invalid response");
        }

        var service = CreateService(db, llm);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GenerateVariantsAsync(lab.Id, 1, null, CancellationToken.None));

        Assert.Contains("after 5 rounds", ex.Message, StringComparison.Ordinal);
        Assert.Empty(await db.AssignmentVariants.ToListAsync());
        Assert.Equal(15, llm.Requests.Count(x => x.Purpose == "variant_generation"));
    }

    [Fact]
    public async Task GenerateVariantsAsync_RepairsJsonWithTrailingComma()
    {
        await using var db = TestDbContextFactory.Create();
        var discipline = await CreateDisciplineAsync(db, "DevOps");
        var lab = await CreateLabAsync(db, discipline.Id, orderIndex: 1, title: "Lab");
        await AddApprovedMasterAsync(db, lab.Id, "Approved master");

        var llm = new ScriptedLlmClient();
        llm.Enqueue("variant_generation",
            "{\"title\":\"Variant A\",\"content_markdown\":\"Content A\",\"variant_params\":{},\"difficulty_profile\":{\"complexity\":\"medium\",\"estimated_hours\":6},}");

        var service = CreateService(db, llm);
        var result = await service.GenerateVariantsAsync(lab.Id, 1, null, CancellationToken.None);

        var created = Assert.Single(result);
        Assert.Equal("Variant A", created.Title);
        Assert.Equal("Content A", created.Content);
        Assert.Equal(1, llm.Requests.Count(x => x.Purpose == "variant_generation"));
    }

    [Fact]
    public async Task GenerateVariantsAsync_RepairsJsonWithUnescapedNewlines()
    {
        await using var db = TestDbContextFactory.Create();
        var discipline = await CreateDisciplineAsync(db, "Web Development");
        var lab = await CreateLabAsync(db, discipline.Id, orderIndex: 1, title: "Lab");
        await AddApprovedMasterAsync(db, lab.Id, "Approved master");

        var llm = new ScriptedLlmClient();
        // content_markdown contains literal newlines that are not JSON-escaped
        llm.Enqueue("variant_generation",
            "{\"title\":\"Variant A\",\"content_markdown\":\"# Heading\nParagraph text\nMore text\",\"variant_params\":{},\"difficulty_profile\":{\"complexity\":\"medium\",\"estimated_hours\":6}}");

        var service = CreateService(db, llm);
        var result = await service.GenerateVariantsAsync(lab.Id, 1, null, CancellationToken.None);

        var created = Assert.Single(result);
        Assert.Equal("Variant A", created.Title);
        Assert.Contains("Heading", created.Content);
        Assert.Contains("Paragraph text", created.Content);
        Assert.Equal(1, llm.Requests.Count(x => x.Purpose == "variant_generation"));
    }

    [Fact]
    public async Task GenerateVariantsAsync_IncludesSpecificParseErrorInRetryPrompt()
    {
        await using var db = TestDbContextFactory.Create();
        var discipline = await CreateDisciplineAsync(db, "Data Science");
        var lab = await CreateLabAsync(db, discipline.Id, orderIndex: 1, title: "Lab");
        await AddApprovedMasterAsync(db, lab.Id, "Approved master");

        var llm = new ScriptedLlmClient();
        llm.Enqueue("variant_generation", "no json here at all");
        llm.Enqueue("variant_generation", BuildVariantJson("OK", "Content"));

        var service = CreateService(db, llm);
        var result = await service.GenerateVariantsAsync(lab.Id, 1, null, CancellationToken.None);

        Assert.Single(result);
        var retryRequest = llm.Requests.Where(x => x.Purpose == "variant_generation").Skip(1).Single();
        Assert.Contains("content_markdown", retryRequest.UserPrompt, StringComparison.Ordinal);
        Assert.Contains("не удалось разобрать", retryRequest.UserPrompt, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GenerateVariantsAsync_AddsFingerprintSuffix_WhenBaseFingerprintCollides()
    {
        await using var db = TestDbContextFactory.Create();
        var discipline = await CreateDisciplineAsync(db, "Security");
        var lab = await CreateLabAsync(db, discipline.Id, orderIndex: 1, title: "Lab");
        await AddApprovedMasterAsync(db, lab.Id, "Approved master");

        const string generatedContent = "Collision target content";
        var collisionBase = $"lab{lab.Id}-v2-{ComputeSha256Short(generatedContent)}";

        db.AssignmentVariants.Add(CreateVariant(
            lab.Id,
            variantIndex: 1,
            title: "Existing variant",
            content: "Existing content",
            variantParamsJson: "{}",
            fingerprint: collisionBase));
        await db.SaveChangesAsync();

        var llm = new ScriptedLlmClient();
        llm.Enqueue("variant_generation", BuildVariantJson("Candidate", generatedContent));
        llm.Enqueue("constraints_check", BuildConstraintsCheckJson());

        var service = CreateService(db, llm);
        var result = await service.GenerateVariantsAsync(lab.Id, 1, null, CancellationToken.None);

        var created = Assert.Single(result);
        Assert.Equal($"{collisionBase}-1", created.Fingerprint);
    }

    private static VariantGenerationService CreateService(ApplicationDbContext db, ScriptedLlmClient llm)
    {
        return new VariantGenerationService(
            db,
            llm,
            new LlmPromptTemplateService(),
            new ListLogger<VariantGenerationService>(),
            Microsoft.Extensions.Options.Options.Create(new DifficultyDefaults()));
    }

    private static async Task<Discipline> CreateDisciplineAsync(ApplicationDbContext db, string name)
    {
        var discipline = new Discipline
        {
            Name = name,
            Description = $"{name} description"
        };

        db.Disciplines.Add(discipline);
        await db.SaveChangesAsync();
        return discipline;
    }

    private static async Task<Lab> CreateLabAsync(ApplicationDbContext db, int disciplineId, int orderIndex, string title)
    {
        var lab = new Lab
        {
            DisciplineId = disciplineId,
            OrderIndex = orderIndex,
            Title = title,
            InitialDescription = $"Initial description for {title}"
        };

        db.Labs.Add(lab);
        await db.SaveChangesAsync();
        return lab;
    }

    private static async Task AddApprovedMasterAsync(ApplicationDbContext db, int labId, string content)
    {
        db.MasterAssignments.Add(new MasterAssignment
        {
            LabId = labId,
            Version = 1,
            IsCurrent = true,
            Status = MasterAssignmentStatus.Approved,
            Content = content,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await db.SaveChangesAsync();
    }

    private static AssignmentVariant CreateVariant(
        int labId,
        int variantIndex,
        string title,
        string content,
        string variantParamsJson,
        string fingerprint)
    {
        return new AssignmentVariant
        {
            LabId = labId,
            VariantIndex = variantIndex,
            Title = title,
            Content = content,
            VariantParamsJson = variantParamsJson,
            DifficultyProfileJson = "{}",
            Fingerprint = fingerprint,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    private static string BuildVariantJson(
        string title,
        string content,
        object? variantParams = null,
        object? difficultyProfile = null)
    {
        var payload = new Dictionary<string, object?>
        {
            ["title"] = title,
            ["content_markdown"] = content,
            ["variant_params"] = variantParams ?? new { },
            ["difficulty_profile"] = difficultyProfile ?? new { complexity = "medium", estimated_hours = 6 }
        };

        return JsonSerializer.Serialize(payload);
    }

    private static string BuildConstraintsCheckJson(
        bool isUnique = true,
        string? mostSimilarTo = null,
        string? similarityReason = null,
        bool difficultyConsistent = true,
        string? difficultyReason = null)
    {
        var payload = new Dictionary<string, object?>
        {
            ["is_unique"] = isUnique,
            ["most_similar_to"] = mostSimilarTo,
            ["similarity_reason"] = similarityReason,
            ["difficulty_consistent"] = difficultyConsistent,
            ["difficulty_reason"] = difficultyReason
        };

        return JsonSerializer.Serialize(payload);
    }

    private static string ReadVariantParam(string variantParamsJson, string key)
    {
        using var doc = JsonDocument.Parse(variantParamsJson);
        return doc.RootElement.GetProperty(key).GetString() ?? string.Empty;
    }

    private static string ComputeSha256Short(string input)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash)[..12].ToLowerInvariant();
    }
}
