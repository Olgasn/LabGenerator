using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using LabGenerator.Application.Abstractions;
using LabGenerator.Application.Models;
using LabGenerator.Domain.Entities;
using LabGenerator.Domain.Enums;
using LabGenerator.Infrastructure.Data;
using LabGenerator.Infrastructure.Jobs;
using Microsoft.EntityFrameworkCore;

namespace LabGenerator.Infrastructure.Services;

public sealed class LabSupplementaryMaterialService(
    ApplicationDbContext db,
    ILLMClient llmClient,
    LlmPromptTemplateService promptTemplates,
    PromptCustomSectionService promptCustomSections)
    : ILabSupplementaryMaterialService
{
    public async Task<LabSupplementaryMaterial?> GetCurrentAsync(int labId, CancellationToken cancellationToken)
    {
        return await db.LabSupplementaryMaterials.AsNoTracking()
            .FirstOrDefaultAsync(x => x.LabId == labId, cancellationToken);
    }

    public async Task<LabSupplementaryMaterial> GenerateAsync(int labId, bool force, CancellationToken cancellationToken)
    {
        var lab = await db.Labs.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == labId, cancellationToken)
            ?? throw new InvalidOperationException($"Lab {labId} not found.");

        var master = await db.MasterAssignments.AsNoTracking()
            .Where(x => x.LabId == labId && x.IsCurrent)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("MasterAssignment is not generated.");

        if (master.Status != MasterAssignmentStatus.Approved)
        {
            throw new InvalidOperationException("MasterAssignment must be approved before generating supplementary materials.");
        }

        var variants = await db.AssignmentVariants.AsNoTracking()
            .Where(x => x.LabId == labId)
            .OrderBy(x => x.VariantIndex)
            .Select(x => new VariantSummary(x.VariantIndex, x.Title, x.VariantParamsJson, x.Fingerprint))
            .ToListAsync(cancellationToken);

        if (variants.Count == 0)
        {
            throw new InvalidOperationException("Variants must be generated before supplementary materials.");
        }

        var sourceFingerprint = BuildSourceFingerprint(master.Content, variants.Select(x => x.Fingerprint).ToArray());

        var existing = await db.LabSupplementaryMaterials
            .FirstOrDefaultAsync(x => x.LabId == labId, cancellationToken);

        if (!force && existing is not null && IsUpToDate(existing, sourceFingerprint))
        {
            return existing;
        }

        var materialRequirements = await promptCustomSections.GetContentAsync(
            PromptCustomSectionService.MaterialRequirements, cancellationToken);

        var promptVars = BuildPromptVariables(lab, master.Content, variants);
        promptVars["material_requirements_block"] = materialRequirements;

        var prompt = promptTemplates.Render(
            "supplementary_material",
            promptVars);

        var systemPrompt = prompt.SystemPrompt;
        var userPrompt = prompt.UserPrompt;

        await JobExecutionContext.AppendLlmPromptAsync(
            db,
            purpose: "supplementary_material",
            systemPrompt: systemPrompt,
            userPrompt: userPrompt,
            attempt: 1,
            temperature: 0.2,
            maxOutputTokens: 8192,
            ct: cancellationToken);

        var result = await llmClient.GenerateTextAsync(
            new LLMCompletionRequest(
                Purpose: "supplementary_material",
                SystemPrompt: systemPrompt,
                UserPrompt: userPrompt,
                Model: null,
                Temperature: 0.2,
                MaxOutputTokens: 8192),
            cancellationToken);

        db.LLMRuns.Add(new LLMRun
        {
            Provider = result.Provider,
            Model = result.Model,
            Purpose = "supplementary_material",
            RequestJson = JsonSerializer.Serialize(new
            {
                labId,
                force,
                sourceFingerprint
            }),
            ResponseText = result.Text,
            PromptTokens = result.PromptTokens,
            CompletionTokens = result.CompletionTokens,
            TotalTokens = result.TotalTokens,
            LatencyMs = result.LatencyMs,
            Status = "Succeeded",
            CreatedAt = DateTimeOffset.UtcNow
        });

        var parsed = Parse(result.Text);

        if (existing is null)
        {
            existing = new LabSupplementaryMaterial
            {
                LabId = labId,
                CreatedAt = DateTimeOffset.UtcNow
            };
            db.LabSupplementaryMaterials.Add(existing);
        }

        existing.TheoryMarkdown = parsed.TheoryMarkdown;
        existing.ControlQuestionsJson = JsonSerializer.Serialize(parsed.ControlQuestions);
        existing.SourceFingerprint = sourceFingerprint;
        existing.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return existing;
    }

    public bool IsUpToDate(LabSupplementaryMaterial material, string sourceFingerprint)
        => string.Equals(material.SourceFingerprint, sourceFingerprint, StringComparison.Ordinal)
           && !string.IsNullOrWhiteSpace(material.TheoryMarkdown)
           && !string.IsNullOrWhiteSpace(material.ControlQuestionsJson);

    public string BuildSourceFingerprint(string masterContent, IReadOnlyCollection<string> variantFingerprints)
    {
        var normalized = new StringBuilder(masterContent.Length + (variantFingerprints.Count * 32));
        normalized.Append(masterContent.Trim());

        foreach (var fingerprint in variantFingerprints.OrderBy(x => x, StringComparer.Ordinal))
        {
            normalized.Append('|');
            normalized.Append(fingerprint.Trim());
        }

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(normalized.ToString()));
        return Convert.ToHexString(hash)[..32].ToLowerInvariant();
    }

    private static Dictionary<string, string?> BuildPromptVariables(
        Lab lab,
        string masterContent,
        IReadOnlyList<VariantSummary> variants)
    {
        var variantsSummary = new StringBuilder();

        foreach (var variant in variants.Take(8))
        {
            variantsSummary.AppendLine($"Вариант {variant.VariantIndex}: {variant.Title}");
            if (!string.IsNullOrWhiteSpace(variant.VariantParamsJson) && variant.VariantParamsJson != "{}")
            {
                variantsSummary.AppendLine($"Параметры варианта: {TrimForPrompt(variant.VariantParamsJson, 400)}");
            }
        }

        return new Dictionary<string, string?>
        {
            ["lab_title"] = lab.Title,
            ["lab_initial_description"] = lab.InitialDescription,
            ["master_assignment_excerpt"] = TrimForPrompt(masterContent, 5000),
            ["variants_count"] = variants.Count.ToString(),
            ["variants_summary"] = variantsSummary.ToString().TrimEnd()
        };
    }

    private static SupplementaryMaterialParsed Parse(string text)
    {
        var json = ExtractJsonObject(text);
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new InvalidOperationException("LLM did not return valid supplementary materials JSON.");
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var theory = root.TryGetProperty("theory_markdown", out var theoryNode)
                         && theoryNode.ValueKind == JsonValueKind.String
                ? theoryNode.GetString()?.Trim()
                : null;

            if (string.IsNullOrWhiteSpace(theory))
            {
                throw new InvalidOperationException("Supplementary materials must include theory_markdown.");
            }

            var questions = new List<string>();
            if (root.TryGetProperty("control_questions", out var questionNode)
                && questionNode.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in questionNode.EnumerateArray())
                {
                    if (item.ValueKind != JsonValueKind.String)
                    {
                        continue;
                    }

                    var value = item.GetString()?.Trim();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        questions.Add(value);
                    }
                }
            }

            if (questions.Count == 0)
            {
                throw new InvalidOperationException("Supplementary materials must include control questions.");
            }

            return new SupplementaryMaterialParsed(theory, questions);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("LLM returned malformed supplementary materials JSON.", ex);
        }
    }

    private static string ExtractJsonObject(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var trimmed = text.Trim();
        var fenceStart = trimmed.IndexOf("```json", StringComparison.OrdinalIgnoreCase);
        if (fenceStart >= 0)
        {
            var contentStart = trimmed.IndexOf('\n', fenceStart);
            if (contentStart >= 0)
            {
                var fenceEnd = trimmed.IndexOf("```", contentStart + 1, StringComparison.Ordinal);
                if (fenceEnd > contentStart)
                {
                    var inner = trimmed[(contentStart + 1)..fenceEnd].Trim();
                    if (LooksLikeObject(inner))
                    {
                        return inner;
                    }
                }
            }
        }

        var depth = 0;
        var start = -1;
        for (var i = 0; i < trimmed.Length; i++)
        {
            if (trimmed[i] == '{')
            {
                if (depth == 0)
                {
                    start = i;
                }

                depth++;
            }
            else if (trimmed[i] == '}' && depth > 0)
            {
                depth--;
                if (depth == 0 && start >= 0)
                {
                    var candidate = trimmed[start..(i + 1)];
                    if (LooksLikeObject(candidate))
                    {
                        return candidate;
                    }
                }
            }
        }

        return string.Empty;
    }

    private static bool LooksLikeObject(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.ValueKind == JsonValueKind.Object;
        }
        catch
        {
            return false;
        }
    }

    private static string TrimForPrompt(string text, int maxChars)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var normalized = text.Replace("\r\n", "\n").Trim();
        return normalized.Length <= maxChars
            ? normalized
            : normalized[..maxChars] + "\n[...обрезано...]";
    }

    private sealed record VariantSummary(int VariantIndex, string Title, string VariantParamsJson, string Fingerprint);

    private sealed record SupplementaryMaterialParsed(string TheoryMarkdown, IReadOnlyList<string> ControlQuestions);
}
