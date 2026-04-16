using System.Text.Json;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using LabGenerator.Application.Abstractions;
using LabGenerator.Domain.Entities;
using LabGenerator.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LabGenerator.Infrastructure.Services;

public sealed partial class DocxExportService(ApplicationDbContext db) : IDocxExportService
{
    private const int TitleFontSize = 32;
    private const int HeadingFontSize = 30;
    private const int BodyFontSize = 28;
    private const int CodeFontSize = 24;
    private const string MainFont = "Times New Roman";
    private const string CodeFont = "Courier New";

    public async Task<byte[]> ExportLabAsync(int labId, CancellationToken cancellationToken)
    {
        var lab = await db.Labs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == labId, cancellationToken);
        if (lab is null)
        {
            throw new InvalidOperationException($"Lab {labId} not found.");
        }

        var master = await db.MasterAssignments.AsNoTracking()
            .Where(x => x.LabId == labId && x.IsCurrent)
            .FirstOrDefaultAsync(cancellationToken);

        var variants = await db.AssignmentVariants.AsNoTracking()
            .Where(x => x.LabId == labId)
            .OrderBy(x => x.VariantIndex)
            .ToListAsync(cancellationToken);

        var supplementaryMaterial = await db.LabSupplementaryMaterials.AsNoTracking()
            .FirstOrDefaultAsync(x => x.LabId == labId, cancellationToken);

        var masterSections = ParseMasterSections(master?.Content);
        var goalText = FirstNotEmpty(masterSections.Goal, lab.InitialDescription);
        var taskText = masterSections.Task;
        var theoryText = supplementaryMaterial?.TheoryMarkdown;
        var controlQuestions = ParseQuestions(supplementaryMaterial?.ControlQuestionsJson);

        await using var ms = new MemoryStream();
        using (var doc = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document, true))
        {
            var mainPart = doc.AddMainDocumentPart();
            mainPart.Document = new Document(new Body());

            var body = mainPart.Document.Body!;

            body.Append(CreateTitleParagraph(BuildLabTitle(lab)));

            if (!string.IsNullOrWhiteSpace(goalText))
            {
                body.Append(CreateLabeledParagraph("Цель работы: ", goalText!));
            }

            if (!string.IsNullOrWhiteSpace(taskText))
            {
                AppendSectionHeading(body, "ЗАДАНИЕ");
                AppendMarkdown(body, taskText!);
            }

            if (variants.Count > 0)
            {
                AppendSectionHeading(body, "ВАРИАНТЫ ЗАДАНИЙ");

                foreach (var variant in variants)
                {
                    body.Append(CreateVariantTitleParagraph($"Вариант {variant.VariantIndex}: {variant.Title}"));
                    AppendMarkdown(body, ExtractVariantContentForExport(variant.Content));
                }
            }

            if (!string.IsNullOrWhiteSpace(theoryText))
            {
                AppendSectionHeading(body, "КРАТКИЕ ТЕОРЕТИЧЕСКИЕ СВЕДЕНИЯ");
                AppendMarkdown(body, theoryText!);
            }

            if (controlQuestions.Count > 0)
            {
                AppendSectionHeading(body, "КОНТРОЛЬНЫЕ ВОПРОСЫ");

                for (var i = 0; i < controlQuestions.Count; i++)
                {
                    body.Append(CreateNumberedParagraph(i + 1, controlQuestions[i]));
                }
            }

            body.Append(CreateSectionProperties());
            mainPart.Document.Save();
        }

        return ms.ToArray();
    }

    private static string BuildLabTitle(Lab lab)
        => $"Лабораторная работа № {lab.OrderIndex}: {lab.Title}";

    private static string ExtractVariantContentForExport(string? markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return string.Empty;
        }

        var filtered = new List<string>();
        var skipGoalSection = false;

        foreach (var line in SplitLines(markdown))
        {
            if (ShouldSkipVariantLine(line, ref skipGoalSection))
            {
                continue;
            }

            filtered.Add(line);
        }

        return string.Join(Environment.NewLine, filtered).Trim();
    }

    private static bool ShouldSkipVariantLine(string line, ref bool skipGoalSection)
    {
        var trimmed = line.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return skipGoalSection;
        }

        var normalizedHeading = TryGetVariantHeading(trimmed);

        if (skipGoalSection)
        {
            if (normalizedHeading is null)
            {
                return true;
            }

            skipGoalSection = false;
        }

        if (trimmed.StartsWith("Fingerprint:", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (trimmed.StartsWith("Лабораторная работа:", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (normalizedHeading is null)
        {
            return false;
        }

        if (normalizedHeading.StartsWith("лабораторная работа", StringComparison.Ordinal))
        {
            return true;
        }

        if (IsGoalHeading(normalizedHeading))
        {
            skipGoalSection = true;
            return true;
        }

        return false;
    }

    private static string? TryGetVariantHeading(string line)
    {
        var markdownHeading = TryExtractHeading(line);
        if (markdownHeading is not null)
        {
            return NormalizeHeading(markdownHeading);
        }

        var normalized = NormalizeHeading(line);
        return IsVariantSectionHeading(normalized) ? normalized : null;
    }

    private static bool IsVariantSectionHeading(string heading)
        => heading is "цель" or "цель работы"
            or "постановка задачи"
            or "задание"
            or "задания"
            or "требуемые sql-запросы"
            or "требования к реализации"
            or "ожидаемый результат"
            or "контракт ввода-вывода"
            or "входные данные"
            or "выходные данные"
            or "ограничения"
            or "критерии оценки"
            or "требуемая функциональность";

    private static MasterSections ParseMasterSections(string? markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return MasterSections.Empty;
        }

        string? goal = null;
        string? task = null;

        foreach (var section in ParseStructuredSections(markdown))
        {
            CaptureSection(section.Heading, section.Content, ref goal, ref task);
        }

        return new MasterSections(goal, task);
    }

    private static IReadOnlyList<StructuredSection> ParseStructuredSections(string markdown)
    {
        string? currentHeading = null;
        var currentContent = new List<string>();
        var sections = new List<StructuredSection>();

        foreach (var line in SplitLines(markdown))
        {
            var heading = TryExtractHeading(line);
            if (heading is not null)
            {
                AddStructuredSection(sections, currentHeading, currentContent);
                currentHeading = heading;
                currentContent.Clear();
                continue;
            }

            currentContent.Add(line);
        }

        AddStructuredSection(sections, currentHeading, currentContent);
        return sections;
    }

    private static void AddStructuredSection(
        ICollection<StructuredSection> sections,
        string? heading,
        IReadOnlyCollection<string> content)
    {
        if (string.IsNullOrWhiteSpace(heading))
        {
            return;
        }

        var text = string.Join(Environment.NewLine, content).Trim();
        sections.Add(new StructuredSection(heading, text));
    }

    private static void CaptureSection(
        string? heading,
        string content,
        ref string? goal,
        ref string? task)
    {
        if (string.IsNullOrWhiteSpace(heading) || string.IsNullOrWhiteSpace(content))
        {
            return;
        }

        var normalizedHeading = NormalizeHeading(heading);
        if (goal is null && IsGoalHeading(normalizedHeading))
        {
            goal = content;
            return;
        }

        if (task is null && IsTaskHeading(normalizedHeading))
        {
            task = content;
        }
    }

    private static bool IsGoalHeading(string heading)
        => heading is "цель" or "цель работы";

    private static bool IsTaskHeading(string heading)
        => heading is "задание" or "задания" or "постановка задачи" or "практическое задание";

    private static string NormalizeHeading(string heading)
        => Regex.Replace(heading.Trim().TrimEnd(':').ToLowerInvariant(), @"\s+", " ");

    private static string? TryExtractHeading(string line)
    {
        var trimmed = line.Trim();
        if (!trimmed.StartsWith('#'))
        {
            return null;
        }

        var heading = trimmed.TrimStart('#').Trim();
        return string.IsNullOrWhiteSpace(heading) ? null : heading;
    }

    private static void AppendSectionHeading(Body body, string text)
    {
        body.Append(CreateSectionHeadingParagraph(text));
    }

    private static Paragraph CreateTitleParagraph(string text)
    {
        return CreateParagraph(
            [new InlineSegment(text, IsBold: true, UseMonospace: false)],
            justification: JustificationValues.Center,
            fontSize: TitleFontSize,
            spacingAfter: 240);
    }

    private static Paragraph CreateSectionHeadingParagraph(string text)
    {
        return CreateParagraph(
            [new InlineSegment(text, IsBold: true, UseMonospace: false)],
            justification: JustificationValues.Center,
            fontSize: HeadingFontSize,
            spacingBefore: 240,
            spacingAfter: 120);
    }

    private static Paragraph CreateVariantTitleParagraph(string text)
    {
        return CreateParagraph(
            [new InlineSegment(text, IsBold: true, UseMonospace: false)],
            fontSize: BodyFontSize,
            spacingBefore: 120,
            spacingAfter: 60,
            leftIndent: 600);
    }

    private static Paragraph CreateLabeledParagraph(string label, string value)
    {
        var segments = new List<InlineSegment>
        {
            new(label, IsBold: true, UseMonospace: false)
        };
        segments.AddRange(ParseInlineSegments(value));

        return CreateParagraph(
            segments,
            fontSize: BodyFontSize,
            spacingAfter: 120);
    }

    private static Paragraph CreateNumberedParagraph(int number, string text)
    {
        var segments = new List<InlineSegment>
        {
            new($"{number}. ", IsBold: false, UseMonospace: false)
        };
        segments.AddRange(ParseInlineSegments(text));

        return CreateParagraph(
            segments,
            fontSize: BodyFontSize,
            spacingAfter: 60);
    }

    private static Paragraph CreateBodyParagraph(string text, int spacingAfter = 60, int leftIndent = 0)
    {
        return CreateParagraph(
            ParseInlineSegments(text),
            fontSize: BodyFontSize,
            spacingAfter: spacingAfter,
            leftIndent: leftIndent);
    }

    private static Paragraph CreateCodeParagraph(string text)
    {
        return CreateParagraph(
            [new InlineSegment(text, IsBold: false, UseMonospace: true)],
            justification: JustificationValues.Left,
            fontSize: CodeFontSize,
            spacingAfter: 0,
            leftIndent: 600);
    }

    private static Paragraph CreateParagraph(
        IReadOnlyCollection<InlineSegment> segments,
        JustificationValues? justification = null,
        int fontSize = BodyFontSize,
        int spacingBefore = 0,
        int spacingAfter = 0,
        int leftIndent = 0)
    {
        var paragraph = new Paragraph(
            new ParagraphProperties(
                new Justification { Val = justification ?? JustificationValues.Both },
                new SpacingBetweenLines
                {
                    Before = spacingBefore.ToString(),
                    After = spacingAfter.ToString()
                },
                new Indentation
                {
                    Left = leftIndent > 0 ? leftIndent.ToString() : null
                }));

        foreach (var segment in segments)
        {
            if (string.IsNullOrEmpty(segment.Text))
            {
                continue;
            }

            paragraph.Append(new Run(
                CreateRunProperties(fontSize, segment.IsBold, segment.UseMonospace),
                new Text(segment.Text)
                {
                    Space = SpaceProcessingModeValues.Preserve
                }));
        }

        if (!paragraph.Elements<Run>().Any())
        {
            paragraph.Append(new Run(new Text(string.Empty)));
        }

        return paragraph;
    }

    private static RunProperties CreateRunProperties(int fontSize, bool isBold, bool useMonospace)
    {
        var fontFamily = useMonospace ? CodeFont : MainFont;
        var runProperties = new RunProperties(
            new RunFonts
            {
                Ascii = fontFamily,
                HighAnsi = fontFamily,
                ComplexScript = fontFamily
            },
            new FontSize { Val = fontSize.ToString() },
            new FontSizeComplexScript { Val = fontSize.ToString() });

        if (isBold)
        {
            runProperties.Append(new Bold());
            runProperties.Append(new BoldComplexScript());
        }

        return runProperties;
    }

    private static void AppendMarkdown(Body body, string markdown)
    {
        var lines = SplitLines(markdown).ToArray();
        var inCodeBlock = false;

        foreach (var rawLine in lines)
        {
            var line = rawLine.TrimEnd();
            var trimmed = line.Trim();

            if (trimmed.StartsWith("```", StringComparison.Ordinal))
            {
                inCodeBlock = !inCodeBlock;
                continue;
            }

            if (string.IsNullOrWhiteSpace(trimmed))
            {
                continue;
            }

            if (inCodeBlock)
            {
                body.Append(CreateCodeParagraph(line));
                continue;
            }

            var headingLevel = GetHeadingLevel(trimmed);
            if (headingLevel > 0)
            {
                var headingText = trimmed[(headingLevel + 1)..].Trim();
                body.Append(CreateSectionHeadingParagraph(headingText));
                continue;
            }

            var unorderedMatch = BulletPrefixRegex().Match(trimmed);
            if (unorderedMatch.Success)
            {
                var segments = new List<InlineSegment>
                {
                    new("• ", IsBold: false, UseMonospace: false)
                };
                segments.AddRange(ParseInlineSegments(trimmed[unorderedMatch.Length..]));
                body.Append(CreateParagraph(segments, fontSize: BodyFontSize, spacingAfter: 60));
                continue;
            }

            body.Append(CreateBodyParagraph(line));
        }
    }

    private static int GetHeadingLevel(string line)
    {
        var level = 0;
        while (level < line.Length && line[level] == '#')
        {
            level++;
        }

        return level > 0 && level < line.Length && line[level] == ' ' ? level : 0;
    }

    private static IReadOnlyList<InlineSegment> ParseInlineSegments(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return [];
        }

        var segments = new List<InlineSegment>();
        var lastIndex = 0;

        foreach (Match match in InlineTokenRegex().Matches(text))
        {
            if (match.Index > lastIndex)
            {
                segments.Add(new InlineSegment(text[lastIndex..match.Index], IsBold: false, UseMonospace: false));
            }

            var value = match.Value;
            if (value.StartsWith("**", StringComparison.Ordinal) && value.EndsWith("**", StringComparison.Ordinal))
            {
                segments.Add(new InlineSegment(value[2..^2], IsBold: true, UseMonospace: false));
            }
            else if (value.StartsWith('`') && value.EndsWith('`'))
            {
                segments.Add(new InlineSegment(value[1..^1], IsBold: false, UseMonospace: true));
            }

            lastIndex = match.Index + match.Length;
        }

        if (lastIndex < text.Length)
        {
            segments.Add(new InlineSegment(text[lastIndex..], IsBold: false, UseMonospace: false));
        }

        return segments;
    }

    private static IReadOnlyList<string> ParseQuestions(string? controlQuestionsJson)
    {
        if (string.IsNullOrWhiteSpace(controlQuestionsJson))
        {
            return [];
        }

        try
        {
            using var doc = JsonDocument.Parse(controlQuestionsJson);
            if (doc.RootElement.ValueKind != JsonValueKind.Array)
            {
                return [];
            }

            return doc.RootElement.EnumerateArray()
                .Where(x => x.ValueKind == JsonValueKind.String)
                .Select(x => x.GetString()?.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Cast<string>()
                .ToArray();
        }
        catch
        {
            return [];
        }
    }

    private static IEnumerable<string> SplitLines(string text)
    {
        using var reader = new StringReader(text.Replace("\r\n", "\n"));
        while (reader.ReadLine() is { } line)
        {
            yield return line;
        }
    }

    private static string? FirstNotEmpty(params string?[] values)
        => values.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x))?.Trim();

    private static SectionProperties CreateSectionProperties()
    {
        return new SectionProperties(
            new PageSize { Width = 11906, Height = 16838 },
            new PageMargin
            {
                Top = 1134,
                Right = 851,
                Bottom = 1134,
                Left = 1701,
                Header = 709,
                Footer = 709,
                Gutter = 0
            });
    }

    [GeneratedRegex(@"(\*\*.+?\*\*|`[^`\r\n]+`)", RegexOptions.Compiled)]
    private static partial Regex InlineTokenRegex();

    [GeneratedRegex(@"^[-*]\s+", RegexOptions.Compiled)]
    private static partial Regex BulletPrefixRegex();

    private sealed record MasterSections(string? Goal, string? Task)
    {
        public static MasterSections Empty { get; } = new(null, null);
    }

    private sealed record StructuredSection(string Heading, string Content);

    private sealed record InlineSegment(string Text, bool IsBold, bool UseMonospace);
}
