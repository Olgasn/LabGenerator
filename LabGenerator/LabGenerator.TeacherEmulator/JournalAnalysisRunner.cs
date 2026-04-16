using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LabGenerator.TeacherEmulator;

public sealed class JournalAnalysisRunner(
    JournalAnalysisOptions options,
    OllamaAnalysisClient llmClient)
{
    private const string AnalysisSchema = """
{
  "runId": "string",
  "discipline": {
    "name": "string",
    "assignmentsMatchDiscipline": true/false,
    "assignmentsMatchReason": "string",
    "labsDiffer": true/false,
    "labsDifferReason": "string",
    "sequenceLogical": true/false,
    "sequenceReason": "string"
  },
  "labs": [
    {
      "labNumber": 1,
      "assignmentTitle": "string",
      "variantsDiffer": true/false,
      "variantsDifferences": "string",
      "variantsSameDifficulty": true/false,
      "variantsDifficultyReason": "string",
      "missingGenerationReason": "string"
    }
  ],
  "quality": [
    {
      "labNumber": 1,
      "assignmentTitle": "string",
      "correctness": 0-5,
      "quality": 0-5,
      "completeness": 0-5,
      "clarity": 0-5,
      "justification": "string"
    }
  ]
}
""";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly JsonSerializerOptions OutputJsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public async Task<JournalAnalysisSummary> RunAsync(CancellationToken ct)
    {
        var journalFiles = Directory.GetFiles(options.InputDirectory, "journal.json", SearchOption.AllDirectories)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (journalFiles.Count == 0)
        {
            throw new InvalidOperationException($"No journal.json files found in {options.InputDirectory}");
        }

        var startedAt = DateTimeOffset.UtcNow;
        var runDirectory = Path.Combine(options.OutputDirectory, $"run-{startedAt:yyyyMMdd-HHmmss}");
        Directory.CreateDirectory(runDirectory);

        Log($"Analysis start. Input: {options.InputDirectory}");
        Log($"Journals found: {journalFiles.Count}");
        Log($"Output directory: {runDirectory}");
        if (!string.IsNullOrWhiteSpace(options.CriteriaPath))
        {
            Log($"Criteria path: {options.CriteriaPath}");
        }

        var summary = new JournalAnalysisSummary
        {
            StartedAtUtc = startedAt,
            InputDirectory = options.InputDirectory,
            OutputDirectory = runDirectory,
            CriteriaPath = options.CriteriaPath
        };

        var criteriaText = LoadCriteriaText(options.CriteriaPath);

        var index = 0;
        var errorDirectory = Path.Combine(runDirectory, "errors");
        Directory.CreateDirectory(errorDirectory);

        foreach (var jsonPath in journalFiles)
        {
            ct.ThrowIfCancellationRequested();
            index++;

            Log($"[{index}/{journalFiles.Count}] Analyzing {jsonPath}");

            var mdPath = Path.Combine(Path.GetDirectoryName(jsonPath) ?? string.Empty, "journal.md");
            var jsonText = File.ReadAllText(jsonPath);

            var snapshot = BuildSnapshot(jsonText);

            var result = await AnalyzeSingleAsync(
                snapshot,
                jsonPath,
                File.Exists(mdPath) ? mdPath : null,
                criteriaText,
                jsonText,
                errorDirectory,
                ct);

            summary.Results.Add(result);

            Log($"[{index}/{journalFiles.Count}] Done. RunId={result.RunId}, Discipline={result.Discipline.Name}, Labs={result.Labs.Count}");
        }

        summary.FinishedAtUtc = DateTimeOffset.UtcNow;

        var jsonOutPath = Path.Combine(runDirectory, "analysis.json");
        var mdOutPath = Path.Combine(runDirectory, "analysis.md");

        var jsonOut = JsonSerializer.Serialize(summary, OutputJsonOptions);
        await File.WriteAllTextAsync(jsonOutPath, jsonOut, ct);

        var markdown = BuildMarkdown(summary);
        await File.WriteAllTextAsync(mdOutPath, markdown, ct);

        Console.WriteLine($"Analysis JSON: {jsonOutPath}");
        Console.WriteLine($"Analysis Markdown: {mdOutPath}");

        return summary;
    }

    private async Task<JournalAnalysisResult> AnalyzeSingleAsync(
        JournalSnapshot snapshot,
        string jsonPath,
        string? mdPath,
        string criteriaText,
        string jsonContent,
        string errorDirectory,
        CancellationToken ct)
    {
        var systemPrompt = "You are an expert reviewer of university lab assignments. Respond with one JSON object only.";
        var userPrompt = BuildAnalysisPrompt(snapshot, criteriaText, jsonContent);

        var attempts = new List<(string Label, string Raw)>();

        JournalAnalysisResult? lastValid = null;

        var raw = await llmClient.GenerateJsonAsync(systemPrompt, userPrompt, temperature: 0.1, maxTokens: 4096, ct);
        attempts.Add(("attempt-1", raw));

        if (TryParseResultWithRepair(raw, out var parsed, out var repaired))
        {
            if (!string.IsNullOrWhiteSpace(repaired))
            {
                attempts.Add(("attempt-1-repaired", repaired));
            }

            lastValid = parsed;
        }
        else
        {
            Log($"LLM response parse failed for {jsonPath}. Retrying with stricter prompt.");
            var retryPrompt = userPrompt + "\n\nAnswer ONLY valid JSON according to the schema. Do not add any extra text.";
            var retryRaw = await llmClient.GenerateJsonAsync(systemPrompt, retryPrompt, temperature: 0.05, maxTokens: 4096, ct);
            attempts.Add(("attempt-2", retryRaw));

            if (TryParseResultWithRepair(retryRaw, out parsed, out repaired))
            {
                if (!string.IsNullOrWhiteSpace(repaired))
                {
                    attempts.Add(("attempt-2-repaired", repaired));
                }

                lastValid = parsed;
            }
        }

        if (lastValid is null)
        {
            var repairSource = attempts
                .Select(x => x.Raw)
                .LastOrDefault(x => !string.IsNullOrWhiteSpace(x));

            if (!string.IsNullOrWhiteSpace(repairSource))
            {
                Log($"LLM response parse failed for {jsonPath}. Attempting JSON repair via LLM.");
                var repairPrompt = BuildRepairPrompt(repairSource);
                var repairRaw = await llmClient.GenerateJsonAsync(
                    systemPrompt,
                    repairPrompt,
                    temperature: 0.0,
                    maxTokens: 2048,
                    ct);
                attempts.Add(("repair-llm", repairRaw));

                if (TryParseResultWithRepair(repairRaw, out parsed, out repaired))
                {
                    if (!string.IsNullOrWhiteSpace(repaired))
                    {
                        attempts.Add(("repair-llm-repaired", repaired));
                    }

                    lastValid = parsed;
                }
            }
        }

        if (lastValid is null)
        {
            var errorFile = SaveRawResponses(errorDirectory, jsonPath, attempts);
            Log($"LLM response parse failed for {jsonPath}. Saved raw response to {errorFile}.");
            var errorMessage = $"LLM response is not valid JSON. See {errorFile}.";
            return BuildFallbackResult(snapshot, jsonPath, mdPath, errorMessage);
        }

        lastValid.RunId = string.IsNullOrWhiteSpace(lastValid.RunId) ? snapshot.RunId : lastValid.RunId;
        lastValid.SourceJsonPath = jsonPath;
        lastValid.SourceMarkdownPath = mdPath;
        lastValid.Discipline ??= new DisciplineAnalysis();
        lastValid.Labs ??= new List<LabComparisonAnalysis>();
        lastValid.Quality ??= new List<LabQualityAnalysis>();

        EnrichWithRetries(lastValid, snapshot);

        return lastValid;
    }

    private static string BuildAnalysisPrompt(
        JournalSnapshot snapshot,
        string criteriaText,
        string jsonContent)
    {
        var sb = new StringBuilder();

        sb.AppendLine("Analyze the journal files for one test run.");
        sb.AppendLine("Answer in Russian. Use the evaluation criteria below.");
        sb.AppendLine();

        if (!string.IsNullOrWhiteSpace(criteriaText))
        {
            sb.AppendLine("Evaluation criteria (verbatim):");
            sb.AppendLine(criteriaText.Trim());
            sb.AppendLine();
        }

        sb.AppendLine("JSON output schema:");
        sb.AppendLine(AnalysisSchema);
        sb.AppendLine();
        sb.AppendLine("Rules:");
        sb.AppendLine("- If assignments or variants are missing, set missingGenerationReason with the cause.");
        sb.AppendLine("- If there are no variants, set variantsDiffer=false and explain why.");
        sb.AppendLine("- Assess clarity, correctness, completeness, quality for each lab and variants.");
        sb.AppendLine("- Use the Labs array in the journal JSON as the source of truth for lab count. Ignore Options.LabCount if present.");
        sb.AppendLine("- Keep every reason/explanation field concise (1-2 sentences).");
        sb.AppendLine();

        sb.AppendLine("Parsed snapshot:");
        sb.AppendLine($"RunId: {snapshot.RunId}");
        sb.AppendLine($"Discipline: {snapshot.DisciplineName}");
        sb.AppendLine($"Labs: {snapshot.LabsCount}");
        if (!string.IsNullOrWhiteSpace(snapshot.Error))
        {
            sb.AppendLine($"Error: {snapshot.Error}");
        }
        sb.AppendLine();

        sb.AppendLine("Journal JSON content:");
        sb.AppendLine("```json");
        sb.AppendLine(jsonContent.Trim());
        sb.AppendLine("```");

        return sb.ToString();
    }

    private static string BuildRepairPrompt(string raw)
    {
        var sb = new StringBuilder();
        sb.AppendLine("You are a JSON repair tool. Return ONLY a valid JSON object.");
        sb.AppendLine("Schema:");
        sb.AppendLine(AnalysisSchema);
        sb.AppendLine();
        sb.AppendLine("Rules:");
        sb.AppendLine("- Preserve existing values when possible.");
        sb.AppendLine("- If fields are missing or truncated, fill them with empty strings, false, 0, or empty arrays as appropriate.");
        sb.AppendLine("- Do not add labs beyond those already present in the input.");
        sb.AppendLine("- Keep every reason/explanation field concise (1-2 sentences).");
        sb.AppendLine();
        sb.AppendLine("Broken JSON:");
        sb.AppendLine("```json");
        sb.AppendLine(TrimForPrompt(raw, 6000));
        sb.AppendLine("```");
        return sb.ToString();
    }

    private static JournalSnapshot BuildSnapshot(string jsonText)
    {
        try
        {
            using var doc = JsonDocument.Parse(jsonText);
            var root = doc.RootElement;

            var runId = root.TryGetProperty("RunId", out var runIdEl) ? runIdEl.GetString() ?? string.Empty : string.Empty;
            var discipline = string.Empty;
            if (root.TryGetProperty("Discipline", out var disciplineEl) &&
                disciplineEl.ValueKind == JsonValueKind.Object &&
                disciplineEl.TryGetProperty("Name", out var nameEl))
            {
                discipline = nameEl.GetString() ?? string.Empty;
            }

            var labsCount = 0;
            var labRetries = new Dictionary<int, (int VerificationRetries, int RegenerationRetries)>();

            if (root.TryGetProperty("Labs", out var labsEl) && labsEl.ValueKind == JsonValueKind.Array)
            {
                labsCount = labsEl.GetArrayLength();

                foreach (var labEl in labsEl.EnumerateArray())
                {
                    var labNumber = labEl.TryGetProperty("LabNumber", out var lnEl) ? lnEl.GetInt32() : 0;
                    var verRetries = labEl.TryGetProperty("VerificationRetries", out var vrEl) ? vrEl.GetInt32() : 0;
                    var regenRetries = labEl.TryGetProperty("RegenerationRetries", out var rrEl) ? rrEl.GetInt32() : 0;
                    labRetries[labNumber] = (verRetries, regenRetries);
                }
            }

            var error = root.TryGetProperty("Error", out var errorEl) ? errorEl.GetString() ?? string.Empty : string.Empty;

            return new JournalSnapshot(runId, discipline, labsCount, error, labRetries);
        }
        catch
        {
            return new JournalSnapshot(string.Empty, string.Empty, 0, string.Empty, new());
        }
    }

    private static void EnrichWithRetries(JournalAnalysisResult result, JournalSnapshot snapshot)
    {
        foreach (var lab in result.Labs)
        {
            if (snapshot.LabRetries.TryGetValue(lab.LabNumber, out var retries))
            {
                lab.VerificationRetries = retries.VerificationRetries;
                lab.RegenerationRetries = retries.RegenerationRetries;
            }
        }
    }

    private static string LoadCriteriaText(string? criteriaPath)
    {
        if (string.IsNullOrWhiteSpace(criteriaPath))
        {
            return string.Empty;
        }

        return File.Exists(criteriaPath) ? File.ReadAllText(criteriaPath) : string.Empty;
    }

    private static string BuildMarkdown(JournalAnalysisSummary summary)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Journal Analysis Report");
        sb.AppendLine();
        sb.AppendLine($"- Started (UTC): `{summary.StartedAtUtc:O}`");
        sb.AppendLine($"- Finished (UTC): `{summary.FinishedAtUtc:O}`");
        sb.AppendLine($"- Input directory: `{summary.InputDirectory}`");
        sb.AppendLine($"- Output directory: `{summary.OutputDirectory}`");
        if (!string.IsNullOrWhiteSpace(summary.CriteriaPath))
        {
            sb.AppendLine($"- Criteria path: `{summary.CriteriaPath}`");
        }

        sb.AppendLine();
        sb.AppendLine("## Сравнение");
        sb.AppendLine();
        sb.AppendLine("| Дисциплина | ЛР | Соответствует дисциплине | Отличается от других ЛР | Логичность последовательности | Варианты отличаются | Отличия вариантов | Одинаковая сложность | Повторы верификации | Повторы генерации | Причина отсутствия |");
        sb.AppendLine("|---|---:|---|---|---|---|---|---|---:|---:|---|");

        foreach (var result in summary.Results)
        {
            var discipline = EscapePipe(result.Discipline.Name);
            foreach (var lab in result.Labs.OrderBy(x => x.LabNumber))
            {
                sb.AppendLine($"| {discipline} | {lab.LabNumber} | {Bool(result.Discipline.AssignmentsMatchDiscipline)} | {Bool(result.Discipline.LabsDiffer)} | {Bool(result.Discipline.SequenceLogical)} | {Bool(lab.VariantsDiffer)} | {EscapePipe(lab.VariantsDifferences)} | {Bool(lab.VariantsSameDifficulty)} | {lab.VerificationRetries} | {lab.RegenerationRetries} | {EscapePipe(lab.MissingGenerationReason)} |");
            }
        }

        sb.AppendLine();
        sb.AppendLine("## Оценка качества");
        sb.AppendLine();
        sb.AppendLine("| Дисциплина | ЛР | Задание | Корректность | Качество | Полнота | Ясность | Обоснование |");
        sb.AppendLine("|---|---:|---|---:|---:|---:|---:|---|");

        foreach (var result in summary.Results)
        {
            var discipline = EscapePipe(result.Discipline.Name);
            foreach (var lab in result.Quality.OrderBy(x => x.LabNumber))
            {
                sb.AppendLine($"| {discipline} | {lab.LabNumber} | {EscapePipe(lab.AssignmentTitle)} | {lab.Correctness} | {lab.Quality} | {lab.Completeness} | {lab.Clarity} | {EscapePipe(lab.Justification)} |");
            }
        }

        return sb.ToString();
    }

    private static string EscapePipe(string value)
        => (value ?? string.Empty).Replace("\r", " ").Replace("\n", " ").Replace("|", "\\|").Trim();

    private static string Bool(bool value) => value ? "Да" : "Нет";

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

    private static bool TryParseResultFromRaw(string raw, out JournalAnalysisResult? result)
    {
        result = null;

        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        var candidates = new List<string>();

        var fenced = ExtractFirstCodeFence(raw);
        if (!string.IsNullOrWhiteSpace(fenced))
        {
            candidates.Add(fenced);
        }

        var extracted = ExtractJsonObject(raw);
        if (!string.IsNullOrWhiteSpace(extracted))
        {
            candidates.Add(extracted);
        }

        candidates.Add(raw.Trim());

        foreach (var candidate in candidates.Distinct())
        {
            if (string.IsNullOrWhiteSpace(candidate))
            {
                continue;
            }

            if (TryDeserialize(candidate, out result) && result is not null)
            {
                return true;
            }

            if (TryExtractObjectFromJson(candidate, out var objJson) &&
                TryDeserialize(objJson, out result) &&
                result is not null)
            {
                return true;
            }
        }

        return false;
    }

    private static bool TryParseResultWithRepair(string raw, out JournalAnalysisResult? result, out string? repaired)
    {
        repaired = null;

        if (TryParseResultFromRaw(raw, out result) && result is not null)
        {
            return true;
        }

        if (TryRepairJson(raw, out var repairedJson))
        {
            repaired = repairedJson;
            if (TryParseResultFromRaw(repairedJson, out result) && result is not null)
            {
                return true;
            }
        }

        return false;
    }

    private static bool TryRepairJson(string raw, out string repaired)
    {
        repaired = string.Empty;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        var start = raw.IndexOf('{');
        if (start < 0)
        {
            return false;
        }

        var candidate = raw[start..].Trim();
        candidate = StripTrailingCodeFence(candidate);
        if (string.IsNullOrWhiteSpace(candidate))
        {
            return false;
        }

        var inString = false;
        var escape = false;
        var stack = new Stack<char>();

        foreach (var ch in candidate)
        {
            if (inString)
            {
                if (escape)
                {
                    escape = false;
                    continue;
                }

                if (ch == '\\')
                {
                    escape = true;
                    continue;
                }

                if (ch == '"')
                {
                    inString = false;
                }

                continue;
            }

            if (ch == '"')
            {
                inString = true;
                continue;
            }

            if (ch == '{' || ch == '[')
            {
                stack.Push(ch);
                continue;
            }

            if (ch == '}' || ch == ']')
            {
                if (stack.Count == 0)
                {
                    continue;
                }

                var open = stack.Peek();
                if ((open == '{' && ch == '}') || (open == '[' && ch == ']'))
                {
                    stack.Pop();
                }
            }
        }

        var sb = new StringBuilder(candidate.Length + stack.Count + 4);
        sb.Append(candidate);

        if (inString)
        {
            sb.Append('"');
        }

        while (stack.Count > 0)
        {
            var open = stack.Pop();
            sb.Append(open == '{' ? '}' : ']');
        }

        var normalized = RemoveTrailingCommas(sb.ToString());
        repaired = normalized;
        return true;
    }

    private static string RemoveTrailingCommas(string text)
    {
        var sb = new StringBuilder(text.Length);
        var inString = false;
        var escape = false;

        for (var i = 0; i < text.Length; i++)
        {
            var ch = text[i];

            if (inString)
            {
                sb.Append(ch);

                if (escape)
                {
                    escape = false;
                    continue;
                }

                if (ch == '\\')
                {
                    escape = true;
                    continue;
                }

                if (ch == '"')
                {
                    inString = false;
                }

                continue;
            }

            if (ch == '"')
            {
                inString = true;
                sb.Append(ch);
                continue;
            }

            if (ch == ',')
            {
                var j = i + 1;
                while (j < text.Length && char.IsWhiteSpace(text[j]))
                {
                    j++;
                }

                if (j < text.Length && (text[j] == '}' || text[j] == ']'))
                {
                    continue;
                }
            }

            sb.Append(ch);
        }

        return sb.ToString();
    }

    private static string StripTrailingCodeFence(string text)
    {
        var trimmed = text.Trim();
        var fenceIndex = trimmed.LastIndexOf("```", StringComparison.Ordinal);
        if (fenceIndex >= 0 && fenceIndex >= trimmed.Length - 4)
        {
            return trimmed[..fenceIndex].TrimEnd();
        }

        return trimmed;
    }

    private static string TrimForPrompt(string text, int maxChars)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length <= maxChars)
        {
            return text;
        }

        return new StringBuilder(text, 0, maxChars, maxChars + 16).Append("...").ToString();
    }

    private static bool TryExtractObjectFromJson(string json, out string objJson)
    {
        objJson = string.Empty;

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind == JsonValueKind.Object)
            {
                objJson = doc.RootElement.GetRawText();
                return true;
            }

            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var el in doc.RootElement.EnumerateArray())
                {
                    if (el.ValueKind == JsonValueKind.Object)
                    {
                        objJson = el.GetRawText();
                        return true;
                    }
                }
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    private static string ExtractFirstCodeFence(string text)
    {
        var start = text.IndexOf("```", StringComparison.Ordinal);
        if (start < 0)
        {
            return string.Empty;
        }

        var fenceEnd = text.IndexOf('\n', start);
        if (fenceEnd < 0)
        {
            return string.Empty;
        }

        var end = text.IndexOf("```", fenceEnd + 1, StringComparison.Ordinal);
        if (end < 0)
        {
            return string.Empty;
        }

        return text[(fenceEnd + 1)..end].Trim();
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
                    return trimmed[start..(i + 1)];
                }
            }
        }

        return string.Empty;
    }

    private static JournalAnalysisResult BuildFallbackResult(
        JournalSnapshot snapshot,
        string jsonPath,
        string? mdPath,
        string errorMessage)
    {
        var result = new JournalAnalysisResult
        {
            RunId = snapshot.RunId,
            SourceJsonPath = jsonPath,
            SourceMarkdownPath = mdPath,
            Discipline = new DisciplineAnalysis
            {
                Name = snapshot.DisciplineName,
                AssignmentsMatchDiscipline = false,
                AssignmentsMatchReason = errorMessage,
                LabsDiffer = false,
                LabsDifferReason = errorMessage,
                SequenceLogical = false,
                SequenceReason = errorMessage
            }
        };

        var labCount = snapshot.LabsCount > 0 ? snapshot.LabsCount : 1;
        for (var i = 0; i < labCount; i++)
        {
            var number = snapshot.LabsCount > 0 ? i + 1 : 0;
            var title = snapshot.LabsCount > 0 ? "<unknown>" : "<missing labs>";

            snapshot.LabRetries.TryGetValue(number, out var retries);

            result.Labs.Add(new LabComparisonAnalysis
            {
                LabNumber = number,
                AssignmentTitle = title,
                VariantsDiffer = false,
                VariantsDifferences = errorMessage,
                VariantsSameDifficulty = false,
                VariantsDifficultyReason = errorMessage,
                MissingGenerationReason = errorMessage,
                VerificationRetries = retries.VerificationRetries,
                RegenerationRetries = retries.RegenerationRetries
            });

            result.Quality.Add(new LabQualityAnalysis
            {
                LabNumber = number,
                AssignmentTitle = title,
                Correctness = 0,
                Quality = 0,
                Completeness = 0,
                Clarity = 0,
                Justification = errorMessage
            });
        }

        return result;
    }

    private static string SaveRawResponses(string errorDirectory, string jsonPath, List<(string Label, string Raw)> attempts)
    {
        var runFolder = Path.GetFileName(Path.GetDirectoryName(jsonPath) ?? string.Empty);
        if (string.IsNullOrWhiteSpace(runFolder))
        {
            runFolder = "unknown-run";
        }

        var fileName = $"analysis-error-{runFolder}.txt";
        var path = Path.Combine(errorDirectory, fileName);

        var sb = new StringBuilder();
        sb.AppendLine($"Source: {jsonPath}");
        sb.AppendLine();
        foreach (var attempt in attempts)
        {
            sb.AppendLine($"--- {attempt.Label} ---");
            sb.AppendLine(attempt.Raw);
            sb.AppendLine();
        }

        File.WriteAllText(path, sb.ToString());
        return path;
    }

    private static void Log(string message)
        => Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] [analysis] {message}");

    private readonly record struct JournalSnapshot(
        string RunId,
        string DisciplineName,
        int LabsCount,
        string Error,
        Dictionary<int, (int VerificationRetries, int RegenerationRetries)> LabRetries);
}
