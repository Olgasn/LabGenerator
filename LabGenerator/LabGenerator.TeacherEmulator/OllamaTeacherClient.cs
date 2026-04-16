using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace LabGenerator.TeacherEmulator;

public sealed class OllamaTeacherClient(HttpClient httpClient, string model, string? apiKey, LlmProvider provider = LlmProvider.Ollama)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<DisciplinePlan> BuildDisciplinePlanAsync(int labCount, string seedTopic, CancellationToken ct)
        => await BuildDisciplinePlanAsync(labCount, seedTopic, curriculumOverride: null, ct);

    public async Task<DisciplinePlan> BuildDisciplinePlanAsync(
        int labCount,
        string seedTopic,
        CurriculumDisciplineOverride? curriculumOverride,
        CancellationToken ct)
    {
        var systemPrompt = "You are a university professor. Respond with one JSON object only.";
        var userPrompt = BuildDisciplinePlanPrompt(labCount, seedTopic, curriculumOverride);

        var raw = await GenerateAsync(systemPrompt, userPrompt, requireJson: true, temperature: 0.3, maxTokens: 1800, ct);
        var json = ExtractJsonObject(raw);
        if (TryDeserialize<DisciplinePlan>(json, out var parsed) &&
            parsed is not null &&
            parsed.Labs.Count >= labCount &&
            !string.IsNullOrWhiteSpace(parsed.Name))
        {
            parsed.Labs = parsed.Labs
                .OrderBy(x => x.OrderIndex)
                .Take(labCount)
                .Select((lab, index) => new LabPlanItem
                {
                    OrderIndex = index + 1,
                    Title = NormalizeOrFallback(lab.Title, $"Laboratory work #{index + 1}"),
                    InitialDescription = NormalizeOrFallback(
                        lab.InitialDescription,
                        $"Prepare the assignment and implementation for laboratory work #{index + 1}.")
                })
                .ToList();

            parsed.Name = NormalizeOrFallback(parsed.Name, "Software engineering discipline");
            parsed.Description = NormalizeOrFallback(parsed.Description, "Discipline for integration testing of lab assignment generation.");
            return parsed;
        }

        return BuildFallbackPlan(labCount, seedTopic);
    }

    private static string BuildDisciplinePlanPrompt(
        int labCount,
        string seedTopic,
        CurriculumDisciplineOverride? curriculumOverride)
    {
        var curriculumSection = curriculumOverride is null
            ? string.Empty
            : $$"""

Use the following curriculum document as the source material for planning.
- align the laboratory works with the curriculum topics, goals and thematic sections;
- prefer the discipline title from the curriculum;
- base the discipline description on the curriculum content;
- keep lab titles and lab descriptions concise.

Curriculum document (verbatim):
{{curriculumOverride.Description}}
""";

        return $$"""
Create one discipline and exactly {{labCount}} lab works for integration testing.
Output JSON:
{
  "name": "discipline title in Russian",
  "description": "discipline description in Russian, 2-4 sentences",
  "labs": [
    {
      "orderIndex": 1,
      "title": "lab title in Russian",
      "initialDescription": "short assignment description in Russian, 2-5 sentences"
    }
  ]
}
Rules:
- return exactly {{labCount}} labs in "labs";
- orderIndex must be 1..{{labCount}} with no gaps;
- all fields are required;
- topic area: {{seedTopic}}.
{{curriculumSection}}
""";
    }

    public async Task<MasterReviewDecision> ReviewMasterAsync(
        LabPlanItem labPlan,
        string masterContent,
        CancellationToken ct)
    {
        var shortenedMaster = Trim(masterContent, 6000);
        var systemPrompt = "You are a strict reviewer of university assignments. Respond with one JSON object only.";
        var userPrompt = $$"""
Review the master assignment for this lab and decide whether it needs correction.
Lab:
- order index: {{labPlan.OrderIndex}}
- title: {{labPlan.Title}}
- initial description: {{labPlan.InitialDescription}}

Master assignment markdown:
{{shortenedMaster}}

Output JSON:
{
  "needsUpdate": true or false,
  "updatedContent": "full corrected markdown if needsUpdate=true; otherwise empty string",
  "comment": "brief reason in Russian"
}
Review criteria:
- must include clear goal;
- must include concrete task statement;
- must include input/output contract;
- must include requirements list;
- must include evaluation criteria.
If content is already good, set needsUpdate=false.
""";

        var raw = await GenerateAsync(systemPrompt, userPrompt, requireJson: true, temperature: 0.1, maxTokens: 4200, ct);
        var json = ExtractJsonObject(raw);
        if (TryDeserialize<MasterReviewDecision>(json, out var parsed) && parsed is not null)
        {
            parsed.Comment = NormalizeOrFallback(parsed.Comment, "Reviewed by teacher emulator.");
            if (parsed.NeedsUpdate && string.IsNullOrWhiteSpace(parsed.UpdatedContent))
            {
                parsed.UpdatedContent = BuildMasterFallbackRevision(masterContent);
                parsed.Comment += " (fallback revision applied)";
            }

            return parsed;
        }

        var needsUpdateFallback = NeedsMasterFixByHeuristics(masterContent);
        return new MasterReviewDecision
        {
            NeedsUpdate = needsUpdateFallback,
            UpdatedContent = needsUpdateFallback ? BuildMasterFallbackRevision(masterContent) : string.Empty,
            Comment = needsUpdateFallback
                ? "Fallback heuristics decided to improve missing structure."
                : "Fallback heuristics accepted generated content."
        };
    }

    public async Task<VariationSelectionDecision> SelectVariationMethodsAsync(
        int labNumber,
        IReadOnlyList<VariationMethodDto> availableMethods,
        CancellationToken ct)
    {
        var methodsJson = JsonSerializer.Serialize(
            availableMethods.Select(x => new { x.Id, x.Code, x.Name }),
            JsonOptions);

        var systemPrompt = "You are a professor configuring variation settings. Respond with one JSON object only.";
        var userPrompt = $$"""
Select 2-3 variation methods for laboratory work #{{labNumber}}.
At least one selected method must have preserveAcrossLabs=true.
Available methods:
{{methodsJson}}

Output JSON:
{
  "items": [
    {
      "code": "variation method code from the list",
      "preserveAcrossLabs": true or false
    }
  ]
}
Rules:
- include only codes from the list;
- do not repeat codes;
- return 2 or 3 items.
""";

        var raw = await GenerateAsync(systemPrompt, userPrompt, requireJson: true, temperature: 0.2, maxTokens: 800, ct);
        var json = ExtractJsonObject(raw);
        if (TryDeserialize<VariationSelectionDecision>(json, out var parsed) &&
            parsed is not null &&
            parsed.Items.Count > 0)
        {
            parsed.Items = parsed.Items
                .Where(x => !string.IsNullOrWhiteSpace(x.Code))
                .GroupBy(x => x.Code.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(g => new VariationSelectionItemDecision
                {
                    Code = g.First().Code.Trim(),
                    PreserveAcrossLabs = g.First().PreserveAcrossLabs
                })
                .ToList();

            if (parsed.Items.Count > 0)
            {
                return parsed;
            }
        }

        return BuildFallbackSelection(availableMethods);
    }

    private async Task<string> GenerateAsync(
        string systemPrompt,
        string userPrompt,
        bool requireJson,
        double temperature,
        int maxTokens,
        CancellationToken ct)
    {
        return provider == LlmProvider.OpenRouter
            ? await GenerateOpenRouterAsync(systemPrompt, userPrompt, requireJson, temperature, maxTokens, ct)
            : await GenerateOllamaAsync(systemPrompt, userPrompt, requireJson, temperature, maxTokens, ct);
    }

    private async Task<string> GenerateOllamaAsync(
        string systemPrompt,
        string userPrompt,
        bool requireJson,
        double temperature,
        int maxTokens,
        CancellationToken ct)
    {
        var payload = new Dictionary<string, object?>
        {
            ["model"] = model,
            ["prompt"] = $"SYSTEM:\n{systemPrompt}\n\nUSER:\n{userPrompt}",
            ["stream"] = false,
            ["options"] = new
            {
                temperature,
                num_predict = maxTokens
            }
        };

        if (requireJson)
        {
            payload["format"] = "json";
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, ResolveGeneratePath(httpClient.BaseAddress));
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        }

        request.Content = JsonContent.Create(payload);
        using var response = await httpClient.SendAsync(request, ct);
        var raw = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            if ((int)response.StatusCode == 401)
            {
                throw new InvalidOperationException(
                    "Teacher LLM call failed: 401 Unauthorized. " +
                    "Set LG_EMULATOR_OLLAMA_API_KEY (or LLM__Ollama__ApiKey) with a valid key.");
            }

            throw new InvalidOperationException(
                $"Teacher LLM call failed: {(int)response.StatusCode} {response.ReasonPhrase}. Body: {Trim(raw, 2000)}");
        }

        return ExtractTextFromOllamaResponse(raw);
    }

    private async Task<string> GenerateOpenRouterAsync(
        string systemPrompt,
        string userPrompt,
        bool requireJson,
        double temperature,
        int maxTokens,
        CancellationToken ct)
    {
        var messages = new object[]
        {
            new { role = "system", content = systemPrompt },
            new { role = "user", content = userPrompt }
        };

        var payload = new Dictionary<string, object?>
        {
            ["model"] = model,
            ["messages"] = messages,
            ["temperature"] = temperature,
            ["max_tokens"] = maxTokens
        };

        if (requireJson)
        {
            payload["response_format"] = new { type = "json_object" };
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, ResolveChatCompletionsPath(httpClient.BaseAddress));
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        }

        request.Headers.TryAddWithoutValidation("X-Title", "LabGenerator TeacherEmulator");
        request.Content = JsonContent.Create(payload);
        using var response = await httpClient.SendAsync(request, ct);
        var raw = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            if ((int)response.StatusCode == 401)
            {
                throw new InvalidOperationException(
                    "Teacher LLM call failed: 401 Unauthorized. " +
                    "Set LG_EMULATOR_OLLAMA_API_KEY (or OPENROUTER_API_KEY) with a valid OpenRouter key.");
            }

            throw new InvalidOperationException(
                $"Teacher LLM call failed: {(int)response.StatusCode} {response.ReasonPhrase}. Body: {Trim(raw, 2000)}");
        }

        return ExtractTextFromOpenRouterResponse(raw);
    }

    private static string ResolveGeneratePath(Uri? baseAddress)
    {
        if (baseAddress is null)
        {
            return "/api/generate";
        }

        var path = baseAddress.AbsolutePath.TrimEnd('/');
        return path.EndsWith("/api", StringComparison.OrdinalIgnoreCase)
            ? "generate"
            : "api/generate";
    }

    private static string ResolveChatCompletionsPath(Uri? baseAddress)
    {
        if (baseAddress is null)
        {
            return "/api/v1/chat/completions";
        }

        var path = baseAddress.AbsolutePath.TrimEnd('/');
        if (path.EndsWith("/v1", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith("/api/v1", StringComparison.OrdinalIgnoreCase))
        {
            return "chat/completions";
        }

        if (path.EndsWith("/api", StringComparison.OrdinalIgnoreCase))
        {
            return "v1/chat/completions";
        }

        return "api/v1/chat/completions";
    }

    private static string ExtractTextFromOpenRouterResponse(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        try
        {
            using var doc = JsonDocument.Parse(raw);
            if (doc.RootElement.TryGetProperty("choices", out var choices) &&
                choices.ValueKind == JsonValueKind.Array &&
                choices.GetArrayLength() > 0 &&
                choices[0].TryGetProperty("message", out var message) &&
                message.TryGetProperty("content", out var content) &&
                content.ValueKind == JsonValueKind.String)
            {
                return content.GetString() ?? string.Empty;
            }
        }
        catch
        {
            // fall through
        }

        return raw.Trim();
    }

    private static string ExtractTextFromOllamaResponse(string raw)
    {
        var trimmed = raw.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return string.Empty;
        }

        if (trimmed.Contains('\n'))
        {
            var parts = new List<string>();
            foreach (var line in trimmed.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (!TryParseJson(line, out var doc))
                {
                    continue;
                }

                using (doc)
                {
                    if (doc.RootElement.TryGetProperty("response", out var responseEl) &&
                        responseEl.ValueKind == JsonValueKind.String)
                    {
                        parts.Add(responseEl.GetString() ?? string.Empty);
                    }
                }
            }

            if (parts.Count > 0)
            {
                return string.Concat(parts);
            }
        }

        if (TryParseJson(trimmed, out var singleDoc))
        {
            using (singleDoc)
            {
                if (singleDoc.RootElement.TryGetProperty("response", out var responseEl) &&
                    responseEl.ValueKind == JsonValueKind.String)
                {
                    return responseEl.GetString() ?? string.Empty;
                }
            }
        }

        return trimmed;
    }

    private static bool TryParseJson(string json, out JsonDocument document)
    {
        try
        {
            document = JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            document = null!;
            return false;
        }
    }

    private static bool TryDeserialize<T>(string json, out T? value) where T : class
    {
        try
        {
            value = JsonSerializer.Deserialize<T>(json, JsonOptions);
            return value is not null;
        }
        catch
        {
            value = null;
            return false;
        }
    }

    private static string ExtractJsonObject(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var trimmed = text.Trim();
        if (trimmed.StartsWith('{') && trimmed.EndsWith('}'))
        {
            return trimmed;
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
                continue;
            }

            if (trimmed[i] == '}')
            {
                if (depth == 0)
                {
                    continue;
                }

                depth--;
                if (depth == 0 && start >= 0)
                {
                    var candidate = trimmed[start..(i + 1)];
                    if (TryParseJson(candidate, out _))
                    {
                        return candidate;
                    }
                }
            }
        }

        return string.Empty;
    }

    private static string BuildMasterFallbackRevision(string content)
    {
        var source = string.IsNullOrWhiteSpace(content)
            ? "Master assignment draft was empty."
            : content.Trim();

        var sb = new StringBuilder();
        sb.AppendLine(source);
        sb.AppendLine();
        sb.AppendLine("## Goal");
        sb.AppendLine("Implement and test a complete solution according to the assignment requirements.");
        sb.AppendLine();
        sb.AppendLine("## Task Statement");
        sb.AppendLine("Develop the requested functionality, provide reproducible examples, and explain key design choices.");
        sb.AppendLine();
        sb.AppendLine("## Input/Output Contract");
        sb.AppendLine("- Describe all required inputs and constraints.");
        sb.AppendLine("- Describe expected outputs and validation rules.");
        sb.AppendLine();
        sb.AppendLine("## Requirements");
        sb.AppendLine("- Follow coding style and error-handling requirements.");
        sb.AppendLine("- Provide test cases for normal and edge scenarios.");
        sb.AppendLine("- Include a short report with implementation notes.");
        sb.AppendLine();
        sb.AppendLine("## Evaluation Criteria");
        sb.AppendLine("- Correctness and completeness.");
        sb.AppendLine("- Quality of code and tests.");
        sb.AppendLine("- Quality of explanation and report.");

        return sb.ToString().Trim();
    }

    private static bool NeedsMasterFixByHeuristics(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return true;
        }

        var normalized = content.ToLowerInvariant();
        var hasGoal = normalized.Contains("goal") || normalized.Contains("цель");
        var hasTask = normalized.Contains("task") || normalized.Contains("задание");
        var hasIo = normalized.Contains("input") || normalized.Contains("output") || normalized.Contains("вход") || normalized.Contains("выход");
        var hasRequirements = normalized.Contains("requirement") || normalized.Contains("требован");
        var hasCriteria = normalized.Contains("criteria") || normalized.Contains("критер");

        var structureScore = (hasGoal ? 1 : 0) + (hasTask ? 1 : 0) + (hasIo ? 1 : 0) + (hasRequirements ? 1 : 0) + (hasCriteria ? 1 : 0);
        return content.Length < 400 || structureScore < 4;
    }

    private static VariationSelectionDecision BuildFallbackSelection(IReadOnlyList<VariationMethodDto> methods)
    {
        var items = new List<VariationSelectionItemDecision>();

        var preferred = new[] { "subject_domain", "input_data_sets", "algorithmic_requirements" };
        foreach (var code in preferred)
        {
            var method = methods.FirstOrDefault(x => x.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            if (method is null)
            {
                continue;
            }

            items.Add(new VariationSelectionItemDecision
            {
                Code = method.Code,
                PreserveAcrossLabs = method.Code.Equals("subject_domain", StringComparison.OrdinalIgnoreCase)
            });
        }

        if (items.Count == 0)
        {
            foreach (var method in methods.Take(2))
            {
                items.Add(new VariationSelectionItemDecision
                {
                    Code = method.Code,
                    PreserveAcrossLabs = items.Count == 0
                });
            }
        }

        return new VariationSelectionDecision { Items = items };
    }

    private static DisciplinePlan BuildFallbackPlan(int labCount, string seedTopic)
    {
        var plan = new DisciplinePlan
        {
            Name = $"Lab assignments for {seedTopic}",
            Description = "Discipline generated by teacher emulator for integration testing.",
            Labs = new List<LabPlanItem>()
        };

        for (var i = 1; i <= labCount; i++)
        {
            plan.Labs.Add(new LabPlanItem
            {
                OrderIndex = i,
                Title = $"Laboratory work #{i}",
                InitialDescription = $"Build, test and document laboratory work #{i} for the selected discipline topic."
            });
        }

        return plan;
    }

    private static string NormalizeOrFallback(string value, string fallback)
        => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    private static string Trim(string value, int maxChars)
        => value.Length <= maxChars ? value : value[..maxChars] + "...";
}
