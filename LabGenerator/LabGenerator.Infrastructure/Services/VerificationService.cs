using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LabGenerator.Application.Abstractions;
using LabGenerator.Application.Models;
using LabGenerator.Domain.Entities;
using LabGenerator.Infrastructure.Data;
using LabGenerator.Infrastructure.Jobs;
using Microsoft.EntityFrameworkCore;

namespace LabGenerator.Infrastructure.Services;

public sealed class VerificationService(
    ApplicationDbContext db,
    ILLMClient llmClient,
    LlmPromptTemplateService promptTemplates) : IVerificationService
{
    public async Task<VerificationReport> VerifyVariantAsync(int variantId, CancellationToken cancellationToken)
    {
        var variant = await db.AssignmentVariants.FirstOrDefaultAsync(x => x.Id == variantId, cancellationToken);
        if (variant is null)
        {
            throw new InvalidOperationException($"Variant {variantId} not found.");
        }

        var master = await db.MasterAssignments.AsNoTracking()
            .Where(x => x.LabId == variant.LabId && x.IsCurrent)
            .FirstOrDefaultAsync(cancellationToken);

        if (master is null)
        {
            throw new InvalidOperationException("MasterAssignment not found.");
        }

        var first = await JudgeAsync(master.Content, variant, cancellationToken);

        if (!first.Parsed)
        {
            var invalidIssuesJson = JsonSerializer.Serialize(new[]
            {
                new
                {
                    code = "LLM_OUTPUT_INVALID",
                    message = "Не удалось получить валидный JSON-ответ от LLM (response пустой/обрезан). Повторите верификацию.",
                    severity = "high"
                }
            });

            first = first with
            {
                Passed = false,
                JudgeScoreJson = "{}",
                IssuesJson = invalidIssuesJson
            };
        }
        else if (!first.Passed)
        {
            var repaired = await RepairAsync(master.Content, variant, first.IssuesJson, cancellationToken);
            variant.Content = repaired;
            await db.SaveChangesAsync(cancellationToken);

            first = await JudgeAsync(master.Content, variant, cancellationToken);
        }

        var existing = await db.VerificationReports.FirstOrDefaultAsync(x => x.AssignmentVariantId == variantId, cancellationToken);

        if (existing is null)
        {
            existing = new VerificationReport
            {
                AssignmentVariantId = variantId,
                CreatedAt = DateTimeOffset.UtcNow
            };
            db.VerificationReports.Add(existing);
        }

        existing.Passed = first.Passed;
        existing.JudgeScoreJson = first.JudgeScoreJson;
        existing.IssuesJson = first.IssuesJson;
        existing.JudgeRunId = first.JudgeRunId;
        existing.SolverRunId = null;
        existing.CreatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return existing;
    }

    private async Task<JudgeResult> JudgeAsync(string masterMarkdown, AssignmentVariant variant, CancellationToken ct)
    {
        // var masterShort = TruncateForJudge(SanitizeForJudge(masterMarkdown), 1200);
        // var variantShort = TruncateForJudge(SanitizeForJudge(variant.Content), 1200);
        var masterShort = masterMarkdown;
        var variantShort = variant.Content;
        var prompt = promptTemplates.Render(
            "variant_judge",
            new Dictionary<string, string?>
            {
                ["master_assignment_markdown"] = masterShort,
                ["variant_markdown"] = variantShort
            });

        var systemPrompt = prompt.SystemPrompt;
        var userPrompt = prompt.UserPrompt;

        await JobExecutionContext.AppendLlmPromptAsync(
            db,
            purpose: "variant_judge",
            systemPrompt: systemPrompt,
            userPrompt: userPrompt,
            attempt: 1,
            temperature: 0.0,
            maxOutputTokens: 1024,
            ct: ct);

        var llm = await llmClient.GenerateTextAsync(
            new LLMCompletionRequest(
                Purpose: "variant_judge",
                SystemPrompt: systemPrompt,
                UserPrompt: userPrompt,
                Model: null,
                Temperature: 0.0,
                MaxOutputTokens: 1024),
            ct);

        var run = new LLMRun
        {
            Provider = llm.Provider,
            Model = llm.Model,
            Purpose = "variant_judge",
            RequestJson = JsonSerializer.Serialize(new { variantId = variant.Id, systemPrompt, userPrompt }),
            ResponseText = llm.Text,
            PromptTokens = llm.PromptTokens,
            CompletionTokens = llm.CompletionTokens,
            TotalTokens = llm.TotalTokens,
            LatencyMs = llm.LatencyMs,
            Status = "Succeeded",
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.LLMRuns.Add(run);
        await db.SaveChangesAsync(ct);

        var parsed = TryParseJudge(llm.Text);

        if (!parsed.Parsed)
        {
            var retryUserPrompt = string.IsNullOrWhiteSpace(prompt.RetryUserPromptSuffix)
                ? userPrompt
                : userPrompt + "\n\n" + prompt.RetryUserPromptSuffix;

            await JobExecutionContext.AppendLlmPromptAsync(
                db,
                purpose: "variant_judge",
                systemPrompt: systemPrompt,
                userPrompt: retryUserPrompt,
                attempt: 2,
                temperature: 0.0,
                maxOutputTokens: 512,
                ct: ct);

            var llmRetry = await llmClient.GenerateTextAsync(
                new LLMCompletionRequest(
                    Purpose: "variant_judge",
                    SystemPrompt: systemPrompt,
                    UserPrompt: retryUserPrompt,
                    Temperature: 0.0,
                    Model: null,
                    MaxOutputTokens: 512),
                ct);

            var retryRun = new LLMRun
            {
                Provider = llmRetry.Provider,
                Model = llmRetry.Model,
                Purpose = "variant_judge",
                RequestJson = JsonSerializer.Serialize(new { variantId = variant.Id, systemPrompt, userPrompt = retryUserPrompt }),
                ResponseText = llmRetry.Text,
                PromptTokens = llmRetry.PromptTokens,
                CompletionTokens = llmRetry.CompletionTokens,
                TotalTokens = llmRetry.TotalTokens,
                LatencyMs = llmRetry.LatencyMs,
                Status = "Succeeded",
                CreatedAt = DateTimeOffset.UtcNow
            };

            db.LLMRuns.Add(retryRun);
            await db.SaveChangesAsync(ct);

            parsed = TryParseJudge(llmRetry.Text);
            return new JudgeResult(parsed.Passed, parsed.ScoreJson, parsed.IssuesJson, retryRun.Id, parsed.Parsed);
        }

        return new JudgeResult(parsed.Passed, parsed.ScoreJson, parsed.IssuesJson, run.Id, parsed.Parsed);
    }

    private async Task<string> RepairAsync(string masterMarkdown, AssignmentVariant variant, string issuesJson, CancellationToken ct)
    {
        var prompt = promptTemplates.Render(
            "variant_repair",
            new Dictionary<string, string?>
            {
                ["master_assignment_markdown"] = masterMarkdown,
                ["issues_json"] = issuesJson,
                ["variant_markdown"] = variant.Content
            });

        var systemPrompt = prompt.SystemPrompt;
        var userPrompt = prompt.UserPrompt;

        await JobExecutionContext.AppendLlmPromptAsync(
            db,
            purpose: "variant_repair",
            systemPrompt: systemPrompt,
            userPrompt: userPrompt,
            attempt: 1,
            temperature: 0.2,
            maxOutputTokens: 4096,
            ct: ct);

        var llm = await llmClient.GenerateTextAsync(
            new LLMCompletionRequest(
                Purpose: "variant_repair",
                SystemPrompt: systemPrompt,
                UserPrompt: userPrompt,
                Model: null,
                Temperature: 0.2,
                MaxOutputTokens: 4096
            ),
            ct);

        var run = new LLMRun
        {
            Provider = llm.Provider,
            Model = llm.Model,
            Purpose = "variant_repair",
            RequestJson = JsonSerializer.Serialize(new { variantId = variant.Id, systemPrompt, userPrompt }),
            ResponseText = llm.Text,
            PromptTokens = llm.PromptTokens,
            CompletionTokens = llm.CompletionTokens,
            TotalTokens = llm.TotalTokens,
            LatencyMs = llm.LatencyMs,
            Status = "Succeeded",
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.LLMRuns.Add(run);
        await db.SaveChangesAsync(ct);

        return llm.Text;
    }

    private static JudgeParsed TryParseJudge(string text)
    {
        try
        {
            var json = ExtractJsonObject(text);
            if (string.IsNullOrWhiteSpace(json)) return JudgeParsed.Fallback();

            using var doc = JsonDocument.Parse(json);

            var root = doc.RootElement;
            var passed = root.TryGetProperty("passed", out var p) && p.ValueKind == JsonValueKind.True;

            var scoreJson = root.TryGetProperty("score", out var s) ? s.GetRawText() : "{}";
            var issuesJson = root.TryGetProperty("issues", out var i) ? i.GetRawText() : "[]";

            return new JudgeParsed(true, passed, scoreJson, issuesJson);
        }
        catch
        {
            return JudgeParsed.Fallback();
        }
    }

    private static string ExtractJsonObject(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        var trimmed = text.Trim();

        if (trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            var firstNewline = trimmed.IndexOf('\n');
            if (firstNewline >= 0)
            {
                trimmed = trimmed.Substring(firstNewline + 1);
            }

            var fenceEnd = trimmed.LastIndexOf("```", StringComparison.Ordinal);
            if (fenceEnd >= 0)
            {
                trimmed = trimmed.Substring(0, fenceEnd);
            }

            trimmed = trimmed.Trim();
        }

        var start = -1;
        var depth = 0;
        for (var i = 0; i < trimmed.Length; i++)
        {
            var ch = trimmed[i];
            if (ch == '{')
            {
                if (depth == 0) start = i;
                depth++;
                continue;
            }

            if (ch == '}')
            {
                if (depth == 0) continue;
                depth--;
                if (depth != 0 || start < 0) continue;

                var candidate = trimmed.Substring(start, i - start + 1).Trim();
                try
                {
                    using var doc = JsonDocument.Parse(candidate);
                    if (doc.RootElement.ValueKind != JsonValueKind.Object) continue;
                    return candidate;
                }
                catch
                {
                    continue;
                }
            }
        }

        return string.Empty;
    }

    private sealed record JudgeParsed(bool Parsed, bool Passed, string ScoreJson, string IssuesJson)
    {
        public static JudgeParsed Fallback() => new(false, false, "{}", "[]");
    }

    private sealed record JudgeResult(bool Passed, string JudgeScoreJson, string IssuesJson, int JudgeRunId, bool Parsed)
    {
        public static JudgeResult Ok(bool passed, string scoreJson, string issuesJson, int judgeRunId)
            => new(passed, scoreJson, issuesJson, judgeRunId, true);
    }
}
