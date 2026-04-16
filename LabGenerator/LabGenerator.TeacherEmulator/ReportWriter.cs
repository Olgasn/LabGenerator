using System.Text.Encodings.Web;
using System.Text;
using System.Text.Json;

namespace LabGenerator.TeacherEmulator;

public static class ReportWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static async Task<(string JsonPath, string MarkdownPath)> WriteAsync(
        TeacherEmulatorReport report,
        string outputRoot,
        CancellationToken ct)
    {
        var runDirectory = Path.Combine(outputRoot, $"run-{report.RunId}");
        Directory.CreateDirectory(runDirectory);

        var jsonPath = Path.Combine(runDirectory, "journal.json");
        var markdownPath = Path.Combine(runDirectory, "journal.md");

        var json = JsonSerializer.Serialize(report, JsonOptions);
        await File.WriteAllTextAsync(jsonPath, json, ct);

        var markdown = BuildMarkdown(report);
        await File.WriteAllTextAsync(markdownPath, markdown, ct);

        return (jsonPath, markdownPath);
    }

    private static string BuildMarkdown(TeacherEmulatorReport report)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Teacher Emulator Run");
        sb.AppendLine();
        sb.AppendLine($"- Run ID: `{report.RunId}`");
        sb.AppendLine($"- Started (UTC): `{report.StartedAtUtc:O}`");
        sb.AppendLine($"- Finished (UTC): `{report.FinishedAtUtc:O}`");
        sb.AppendLine($"- Result: **{(report.Succeeded ? "Succeeded" : "Failed")}**");
        if (!string.IsNullOrWhiteSpace(report.Error))
        {
            sb.AppendLine($"- Error: `{EscapeInline(report.Error)}`");
        }

        if (report.Discipline is not null)
        {
            sb.AppendLine();
            sb.AppendLine("## Discipline");
            sb.AppendLine();
            sb.AppendLine($"- ID: `{report.Discipline.Id}`");
            sb.AppendLine($"- Name: {EscapeInline(report.Discipline.Name)}");
            sb.AppendLine($"- Description length: `{(report.Discipline.Description ?? string.Empty).Length}`");
            sb.AppendLine();
            sb.AppendLine("### Discipline Description");
            sb.AppendLine();
            sb.AppendLine("```markdown");
            sb.AppendLine((report.Discipline.Description ?? string.Empty).Trim());
            sb.AppendLine("```");
        }

        sb.AppendLine();
        sb.AppendLine("## Labs");
        sb.AppendLine();

        foreach (var lab in report.Labs.OrderBy(x => x.LabNumber))
        {
            sb.AppendLine($"### Lab {lab.LabNumber}");
            sb.AppendLine();
            sb.AppendLine($"- Planned title: {EscapeInline(lab.Plan.Title)}");
            if (!string.IsNullOrWhiteSpace(lab.FailedStage))
            {
                sb.AppendLine($"- Failed stage: `{EscapeInline(lab.FailedStage)}`");
            }
            if (!string.IsNullOrWhiteSpace(lab.Error))
            {
                sb.AppendLine($"- Lab error: `{EscapeInline(lab.Error)}`");
            }
            sb.AppendLine($"- Created lab id: `{lab.CreatedLab?.Id}`");
            sb.AppendLine($"- Master updated: `{lab.MasterUpdated}`");
            sb.AppendLine($"- Master review comment: {EscapeInline(lab.MasterReviewComment ?? string.Empty)}");
            sb.AppendLine($"- Variants total: `{lab.Variants.Count}`");
            sb.AppendLine($"- Verification retries: `{lab.VerificationRetries}`");
            sb.AppendLine($"- Regeneration retries: `{lab.RegenerationRetries}`");
            sb.AppendLine($"- Final status: **{(lab.AllVariantsPassed ? "Passed" : "Has failures")}**");

            sb.AppendLine();
            sb.AppendLine("#### Lab Initial Description");
            sb.AppendLine();
            sb.AppendLine("```markdown");
            sb.AppendLine((lab.Plan.InitialDescription ?? string.Empty).Trim());
            sb.AppendLine("```");

            var finalMaster = (lab.MasterAfterReview?.Content ?? lab.MasterBeforeReview?.Content ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(finalMaster))
            {
                sb.AppendLine();
                sb.AppendLine("#### Master Assignment (Final)");
                sb.AppendLine();
                sb.AppendLine("```markdown");
                sb.AppendLine(finalMaster);
                sb.AppendLine("```");
            }

            if (lab.AppliedVariationMethods.Count > 0)
            {
                sb.AppendLine("- Applied variation methods:");
                foreach (var item in lab.AppliedVariationMethods)
                {
                    sb.AppendLine($"  - `{item.Code}` (id={item.VariationMethodId}, preserve={item.PreserveAcrossLabs})");
                }
            }

            if (lab.VerificationReports.Count > 0)
            {
                sb.AppendLine("- Verification summary:");
                foreach (var verification in lab.VerificationReports.OrderBy(x => x.VariantId))
                {
                    sb.AppendLine(
                        $"  - Variant `{verification.VariantId}`: passed={verification.Passed}, score={verification.OverallScore}, issues={verification.IssueCount}");
                }
            }

            if (lab.Variants.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("#### Generated Variants");
                sb.AppendLine();

                foreach (var variant in lab.Variants.OrderBy(x => x.VariantIndex))
                {
                    sb.AppendLine($"##### Variant {variant.VariantIndex} (ID: {variant.Id})");
                    sb.AppendLine();
                    sb.AppendLine($"- Title: {EscapeInline(variant.Title)}");
                    sb.AppendLine($"- Fingerprint: `{variant.Fingerprint}`");
                    sb.AppendLine();
                    sb.AppendLine("```markdown");
                    sb.AppendLine((variant.Content ?? string.Empty).Trim());
                    sb.AppendLine("```");
                    sb.AppendLine();
                    sb.AppendLine("```json");
                    sb.AppendLine(PrettyJsonOrRaw(variant.VariantParamsJson));
                    sb.AppendLine("```");
                    sb.AppendLine();
                    sb.AppendLine("```json");
                    sb.AppendLine(PrettyJsonOrRaw(variant.DifficultyProfileJson));
                    sb.AppendLine("```");
                    sb.AppendLine();
                }
            }

            sb.AppendLine();
        }

        sb.AppendLine("## Event Log");
        sb.AppendLine();
        sb.AppendLine("| UTC | Level | Stage | Message |");
        sb.AppendLine("|---|---|---|---|");
        foreach (var evt in report.Events.OrderBy(x => x.TimestampUtc))
        {
            sb.AppendLine(
                $"| `{evt.TimestampUtc:O}` | `{EscapePipe(evt.Level)}` | `{EscapePipe(evt.Stage)}` | {EscapePipe(evt.Message)} |");
        }

        return sb.ToString();
    }

    private static string EscapeInline(string value)
        => value.Replace("\r", " ").Replace("\n", " ").Trim();

    private static string EscapePipe(string value)
        => EscapeInline(value).Replace("|", "\\|");

    private static string PrettyJsonOrRaw(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return "{}";
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(doc.RootElement, JsonOptions);
        }
        catch
        {
            return json;
        }
    }
}
