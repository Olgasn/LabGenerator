using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LabGenerator.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LabGenerator.Infrastructure.Jobs;

public static class JobExecutionContext
{
    private static readonly AsyncLocal<int?> CurrentJobIdHolder = new();

    public static int? CurrentJobId
    {
        get => CurrentJobIdHolder.Value;
        set => CurrentJobIdHolder.Value = value;
    }

    public static async Task AppendLlmPromptAsync(
        ApplicationDbContext db,
        string purpose,
        string systemPrompt,
        string userPrompt,
        int? attempt,
        double? temperature,
        int? maxOutputTokens,
        CancellationToken ct)
    {
        if (CurrentJobId is null) return;

        var job = await db.GenerationJobs.FirstOrDefaultAsync(x => x.Id == CurrentJobId.Value, ct);
        if (job is null) return;

        var existing = new Dictionary<string, object?>();

        if (!string.IsNullOrWhiteSpace(job.PayloadJson))
        {
            try
            {
                existing = JsonSerializer.Deserialize<Dictionary<string, object?>>(job.PayloadJson) ?? new Dictionary<string, object?>();
            }
            catch
            {
                existing = new Dictionary<string, object?>();
            }
        }

        var item = new Dictionary<string, object?>
        {
            ["purpose"] = purpose,
            ["attempt"] = attempt,
            ["systemPrompt"] = Truncate(systemPrompt, 20000),
            ["userPrompt"] = Truncate(userPrompt, 20000),
            ["temperature"] = temperature,
            ["maxOutputTokens"] = maxOutputTokens
        };

        if (existing.TryGetValue("llmPrompts", out var raw) && raw is JsonElement je && je.ValueKind == JsonValueKind.Array)
        {
            var list = new List<Dictionary<string, object?>>();
            foreach (var el in je.EnumerateArray())
            {
                if (el.ValueKind != JsonValueKind.Object) continue;
                try
                {
                    var dict = JsonSerializer.Deserialize<Dictionary<string, object?>>(el.GetRawText());
                    if (dict is not null) list.Add(dict);
                }
                catch
                {
                }
            }

            list.Add(item);
            existing["llmPrompts"] = list;
        }
        else
        {
            existing["llmPrompts"] = new[] { item };
        }

        job.PayloadJson = JsonSerializer.Serialize(existing);
        await db.SaveChangesAsync(ct);
    }

    private static string Truncate(string text, int maxChars)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        if (maxChars <= 0) return string.Empty;
        return text.Length <= maxChars ? text : text.Substring(0, maxChars) + "…";
    }
}
