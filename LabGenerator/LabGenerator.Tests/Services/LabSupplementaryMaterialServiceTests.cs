using System.Text.Json;
using LabGenerator.Domain.Entities;
using LabGenerator.Domain.Enums;
using LabGenerator.Infrastructure.Data;
using LabGenerator.Infrastructure.Services;
using LabGenerator.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace LabGenerator.Tests.Services;

public sealed class LabSupplementaryMaterialServiceTests
{
    [Fact]
    public async Task GenerateAsync_CreatesTheoryAndControlQuestions()
    {
        await using var db = TestDbContextFactory.Create();
        var lab = await SeedLabGraphAsync(db);

        var llm = new ScriptedLlmClient();
        llm.Enqueue(
            "supplementary_material",
            """
            {
              "theory_markdown": "# Теория\n## Ключевые понятия\n- Граф\n- Вершина",
              "control_questions": [
                "Что такое ориентированный граф?",
                "Как оценивается сложность алгоритма поиска?"
              ]
            }
            """);

        var service = new LabSupplementaryMaterialService(db, llm, new LlmPromptTemplateService(), new PromptCustomSectionService(db));
        var material = await service.GenerateAsync(lab.Id, force: false, CancellationToken.None);

        Assert.True(material.Id > 0);
        Assert.Contains("Ключевые понятия", material.TheoryMarkdown, StringComparison.Ordinal);

        using var doc = JsonDocument.Parse(material.ControlQuestionsJson);
        Assert.Equal(2, doc.RootElement.GetArrayLength());
        Assert.Equal(1, llm.Requests.Count(x => x.Purpose == "supplementary_material"));
        Assert.Equal(1, await db.LabSupplementaryMaterials.CountAsync());
    }

    [Fact]
    public async Task GenerateAsync_ReusesExistingMaterial_WhenFingerprintIsCurrent()
    {
        await using var db = TestDbContextFactory.Create();
        var lab = await SeedLabGraphAsync(db);

        var llm = new ScriptedLlmClient();
        llm.Enqueue(
            "supplementary_material",
            """
            {
              "theory_markdown": "# Теория\nТекст",
              "control_questions": ["Вопрос 1", "Вопрос 2"]
            }
            """);

        var service = new LabSupplementaryMaterialService(db, llm, new LlmPromptTemplateService(), new PromptCustomSectionService(db));
        var first = await service.GenerateAsync(lab.Id, force: false, CancellationToken.None);
        var second = await service.GenerateAsync(lab.Id, force: false, CancellationToken.None);

        Assert.Equal(first.Id, second.Id);
        Assert.Equal(1, llm.Requests.Count(x => x.Purpose == "supplementary_material"));
    }

    private static async Task<Lab> SeedLabGraphAsync(ApplicationDbContext db)
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
            Title = "Поиск в графе",
            InitialDescription = "Нужно реализовать алгоритм обхода графа."
        };
        db.Labs.Add(lab);
        await db.SaveChangesAsync();

        db.MasterAssignments.Add(new MasterAssignment
        {
            LabId = lab.Id,
            Version = 1,
            IsCurrent = true,
            Status = MasterAssignmentStatus.Approved,
            Content = """
                # Цель работы
                Изучить представление графов и алгоритмы поиска.

                # Требования
                Реализовать обход графа и провести анализ сложности.
                """,
            CreatedAt = DateTimeOffset.UtcNow
        });

        db.AssignmentVariants.AddRange(
            new AssignmentVariant
            {
                LabId = lab.Id,
                VariantIndex = 1,
                Title = "Маршрутизация",
                Content = "Реализовать поиск пути для транспортной сети.",
                VariantParamsJson = "{\"domain\":\"transport\"}",
                DifficultyProfileJson = "{\"complexity\":\"medium\"}",
                Fingerprint = "variant-1",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new AssignmentVariant
            {
                LabId = lab.Id,
                VariantIndex = 2,
                Title = "Социальный граф",
                Content = "Реализовать поиск связей в социальной сети.",
                VariantParamsJson = "{\"domain\":\"social\"}",
                DifficultyProfileJson = "{\"complexity\":\"medium\"}",
                Fingerprint = "variant-2",
                CreatedAt = DateTimeOffset.UtcNow
            });

        await db.SaveChangesAsync();
        return lab;
    }
}
