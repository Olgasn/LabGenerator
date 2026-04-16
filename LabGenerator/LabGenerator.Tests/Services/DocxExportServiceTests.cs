using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using LabGenerator.Domain.Entities;
using LabGenerator.Domain.Enums;
using LabGenerator.Infrastructure.Data;
using LabGenerator.Infrastructure.Services;
using LabGenerator.Tests.Helpers;

namespace LabGenerator.Tests.Services;

public sealed class DocxExportServiceTests
{
    [Fact]
    public async Task ExportLabAsync_Throws_WhenLabDoesNotExist()
    {
        await using var db = TestDbContextFactory.Create();
        var service = new DocxExportService(db);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ExportLabAsync(404, CancellationToken.None));

        Assert.Equal("Lab 404 not found.", ex.Message);
    }

    [Fact]
    public async Task ExportLabAsync_KeepsVariantDetailsButRemovesDuplicateGoal()
    {
        await using var db = TestDbContextFactory.Create();
        var lab = await SeedLabAsync(db, includeSupplementaryMaterial: true);
        var service = new DocxExportService(db);

        var bytes = await service.ExportLabAsync(lab.Id, CancellationToken.None);

        Assert.NotEmpty(bytes);

        using var stream = new MemoryStream(bytes);
        using var document = WordprocessingDocument.Open(stream, false);
        var mainPart = document.MainDocumentPart;
        Assert.NotNull(mainPart);
        Assert.NotNull(mainPart.Document);
        Assert.NotNull(mainPart.Document.Body);

        var paragraphs = mainPart.Document.Body
            .Descendants<Paragraph>()
            .Select(x => x.InnerText.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToArray();

        Assert.Contains(paragraphs, x => x.Contains("Лабораторная работа № 1: Lab title", StringComparison.Ordinal));
        Assert.Contains(paragraphs, x => x.Contains("Цель работы: Освоить экспорт лабораторных работ.", StringComparison.Ordinal));
        Assert.Contains(paragraphs, x => x.Contains("ЗАДАНИЕ", StringComparison.Ordinal));
        Assert.Contains(paragraphs, x => x.Contains("ВАРИАНТЫ ЗАДАНИЙ", StringComparison.Ordinal));
        Assert.Contains(paragraphs, x => x.Contains("Вариант 1: Variant title 1", StringComparison.Ordinal));
        Assert.Contains(paragraphs, x => x.Contains("Постановка задачи", StringComparison.Ordinal));
        Assert.Contains(paragraphs, x => x.Contains("Спроектировать базу данных, которая будет хранить информацию о:", StringComparison.Ordinal));
        Assert.Contains(paragraphs, x => x.Contains("Требуемые SQL-запросы:", StringComparison.Ordinal));
        Assert.Contains(paragraphs, x => x.Contains("Требования к реализации:", StringComparison.Ordinal));
        Assert.Contains(paragraphs, x => x.Contains("Ожидаемый результат", StringComparison.Ordinal));
        Assert.Contains(paragraphs, x => x.Contains("SQL-запросы согласно требованиям", StringComparison.Ordinal));
        Assert.Contains(paragraphs, x => x.Contains("КРАТКИЕ ТЕОРЕТИЧЕСКИЕ СВЕДЕНИЯ", StringComparison.Ordinal));
        Assert.Contains(paragraphs, x => x.Contains("КОНТРОЛЬНЫЕ ВОПРОСЫ", StringComparison.Ordinal));

        Assert.DoesNotContain(paragraphs, x => x.Contains("Fingerprint:", StringComparison.Ordinal));
        Assert.DoesNotContain(paragraphs, x => x.Contains("Лабораторная работа: Проектирование базы данных библиотеки", StringComparison.Ordinal));
        Assert.DoesNotContain(paragraphs, x => x.Contains("Разработать реляционную базу данных для управления библиотекой, включая учет книг, читателей и выдачи литературы.", StringComparison.Ordinal));
        Assert.DoesNotContain(paragraphs, x => x.Contains("PASSED", StringComparison.Ordinal));
        Assert.DoesNotContain(paragraphs, x => x.Contains("FAILED", StringComparison.Ordinal));
        Assert.DoesNotContain(paragraphs, x => x.Contains("**", StringComparison.Ordinal));

        Assert.True(IndexOf(paragraphs, "ВАРИАНТЫ ЗАДАНИЙ")
                    < IndexOf(paragraphs, "Вариант 1: Variant title 1"));
        Assert.True(IndexOf(paragraphs, "Вариант 1: Variant title 1")
                    < IndexOf(paragraphs, "Постановка задачи"));
        Assert.True(IndexOf(paragraphs, "Постановка задачи")
                    < IndexOf(paragraphs, "Требуемые SQL-запросы:"));
        Assert.True(IndexOf(paragraphs, "Требуемые SQL-запросы:")
                    < IndexOf(paragraphs, "Требования к реализации:"));
        Assert.True(IndexOf(paragraphs, "Требования к реализации:")
                    < IndexOf(paragraphs, "Ожидаемый результат"));
    }

    [Fact]
    public async Task ExportLabAsync_WorksWithoutCurrentMasterAssignment()
    {
        await using var db = TestDbContextFactory.Create();
        var lab = await SeedLabAsync(db, includeMaster: false, includeReports: false);
        var service = new DocxExportService(db);

        var bytes = await service.ExportLabAsync(lab.Id, CancellationToken.None);

        using var stream = new MemoryStream(bytes);
        using var document = WordprocessingDocument.Open(stream, false);
        var mainPart = document.MainDocumentPart;
        Assert.NotNull(mainPart);
        Assert.NotNull(mainPart.Document);
        Assert.NotNull(mainPart.Document.Body);

        var paragraphs = mainPart.Document.Body
            .Descendants<Paragraph>()
            .Select(x => x.InnerText.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToArray();

        Assert.Contains(paragraphs, x => x.Contains("Лабораторная работа № 1: Lab title", StringComparison.Ordinal));
        Assert.Contains(paragraphs, x => x.Contains("Цель работы: Initial lab description", StringComparison.Ordinal));
        Assert.Contains(paragraphs, x => x.Contains("Вариант 1: Variant title 1", StringComparison.Ordinal));
        Assert.DoesNotContain(paragraphs, x => x.Contains("ЗАДАНИЕ", StringComparison.Ordinal));
    }

    private static int IndexOf(IEnumerable<string> values, string expected)
        => values.Select((value, index) => (value, index))
            .First(x => x.value.Contains(expected, StringComparison.Ordinal)).index;

    private static async Task<Lab> SeedLabAsync(
        ApplicationDbContext db,
        bool includeMaster = true,
        bool includeReports = true,
        bool includeSupplementaryMaterial = false)
    {
        var discipline = new Discipline
        {
            Name = "Software Engineering",
            Description = "SE course"
        };
        db.Disciplines.Add(discipline);
        await db.SaveChangesAsync();

        var lab = new Lab
        {
            DisciplineId = discipline.Id,
            OrderIndex = 1,
            Title = "Lab title",
            InitialDescription = "Initial lab description"
        };
        db.Labs.Add(lab);
        await db.SaveChangesAsync();

        if (includeMaster)
        {
            db.MasterAssignments.Add(new MasterAssignment
            {
                LabId = lab.Id,
                Version = 1,
                IsCurrent = true,
                Status = MasterAssignmentStatus.Approved,
                Content = """
                    # Лабораторная работа

                    ## Цель работы
                    Освоить экспорт лабораторных работ.

                    ## Постановка задачи
                    Подготовить документ в формате DOCX.

                    ## Контракт ввода-вывода
                    Эти данные в экспорт попадать не должны.

                    ## Требования к реализации
                    И этот раздел тоже не должен экспортироваться.
                    """,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        var variant1 = new AssignmentVariant
        {
            LabId = lab.Id,
            VariantIndex = 1,
            Title = "Variant title 1",
            Content = """
                Fingerprint: lab1-v1-1f84ffdf7e55
                Лабораторная работа: Проектирование базы данных библиотеки
                Цель работы
                Разработать реляционную базу данных для управления библиотекой, включая учет книг, читателей и выдачи литературы.

                Постановка задачи
                Спроектировать базу данных, которая будет хранить информацию о:
                - Книгах (название, автор, год издания, ISBN, жанр, количество экземпляров)
                - Читателях (ФИО, контактные данные, дата регистрации)
                - Выдаче книг (книга, читатель, дата выдачи, срок возврата, фактическая дата возврата)

                Требуемые SQL-запросы:
                - Вывести список книг определенного жанра, изданных после 2010 года
                - Найти читателей, которые не вернули книги в срок

                Требования к реализации:
                - Создать минимум 5 нормализованных таблиц
                - Определить все необходимые связи между таблицами

                Ожидаемый результат
                - ER-диаграмма базы данных
                - SQL-запросы согласно требованиям

                ```sql
                SELECT 1;
                ```
                """,
            VariantParamsJson = "{\"topic\":\"A\"}",
            DifficultyProfileJson = "{\"complexity\":\"low\"}",
            Fingerprint = "fp-1",
            CreatedAt = DateTimeOffset.UtcNow
        };

        var variant2 = new AssignmentVariant
        {
            LabId = lab.Id,
            VariantIndex = 2,
            Title = "Variant title 2",
            Content = "Variant two line 1",
            VariantParamsJson = "{\"topic\":\"B\"}",
            DifficultyProfileJson = "{\"complexity\":\"medium\"}",
            Fingerprint = "fp-2",
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.AssignmentVariants.AddRange(variant1, variant2);
        await db.SaveChangesAsync();

        if (includeReports)
        {
            db.VerificationReports.Add(new VerificationReport
            {
                AssignmentVariantId = variant1.Id,
                Passed = true,
                JudgeScoreJson = "{\"overall\":10}",
                IssuesJson = "[]",
                CreatedAt = DateTimeOffset.UtcNow
            });

            db.VerificationReports.Add(new VerificationReport
            {
                AssignmentVariantId = variant2.Id,
                Passed = false,
                JudgeScoreJson = "{\"overall\":4}",
                IssuesJson = "[{\"code\":\"FORMAT\"}]",
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        if (includeSupplementaryMaterial)
        {
            db.LabSupplementaryMaterials.Add(new LabSupplementaryMaterial
            {
                LabId = lab.Id,
                TheoryMarkdown = """
                    ## Основные понятия
                    **Нормализация** - устранение избыточности.
                    - **Ключ** - средство идентификации записи.
                    """,
                ControlQuestionsJson = "[\"Что такое нормализация?\",\"Чем первичный ключ отличается от внешнего?\"]",
                SourceFingerprint = "supplementary-fp",
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        await db.SaveChangesAsync();
        return lab;
    }
}
