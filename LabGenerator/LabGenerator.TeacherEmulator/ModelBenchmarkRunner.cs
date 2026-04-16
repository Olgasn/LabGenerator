using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LabGenerator.TeacherEmulator;

public sealed class ModelBenchmarkRunner(
    ModelBenchmarkOptions benchmarkOptions,
    TestPlanOptions testPlanOptions,
    TeacherEmulatorOptions emulatorOptions,
    LgApiClient lgApi,
    JournalAnalysisOptions? analysisOptionsTemplate)
{
    private static readonly JsonSerializerOptions JsonWriteOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public async Task<ModelBenchmarkSummary> RunAsync(CancellationToken ct)
    {
        var models = BenchmarkModelsFile.Load(benchmarkOptions.ModelsFilePath);

        var startedAt = DateTimeOffset.UtcNow;
        var sessionId = startedAt.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
        var sessionRoot = Path.Combine(benchmarkOptions.OutputRoot, $"run-{sessionId}");
        Directory.CreateDirectory(sessionRoot);

        Log($"Benchmark session started. Models: {models.Count}. Output: {sessionRoot}");

        var originalSettings = await lgApi.GetLlmSettingsAsync(ct);
        Log($"Original LLM settings: provider={originalSettings.Provider}, model={originalSettings.Model}");

        var summary = new ModelBenchmarkSummary
        {
            StartedAtUtc = startedAt,
            ModelsFilePath = benchmarkOptions.ModelsFilePath,
            TestPlanCsvPath = testPlanOptions.CsvPath,
            OutputRoot = sessionRoot,
            VariantCount = emulatorOptions.VariantCountPerLab,
        };

        IReadOnlyDictionary<string, string>? seedMasterTexts = null;

        try
        {
            for (var i = 0; i < models.Count; i++)
            {
                ct.ThrowIfCancellationRequested();

                var model = models[i];
                var isSeedModel = i == 0;

                Log(isSeedModel
                    ? $"[{i + 1}/{models.Count}] SEED model: {model.Name} ({model.Provider}/{model.Model}) — will generate master assignments"
                    : $"[{i + 1}/{models.Count}] Starting model: {model.Name} ({model.Provider}/{model.Model}) — using seed masters");

                var result = await RunForModelAsync(model, sessionRoot, isSeedModel ? null : seedMasterTexts, ct);
                summary.Models.Add(result);

                if (isSeedModel && result.CollectedMasterTexts is not null)
                {
                    seedMasterTexts = result.CollectedMasterTexts;
                    Log($"[{i + 1}/{models.Count}] Seed masters collected: {seedMasterTexts.Count} entries.");
                }

                Log($"[{i + 1}/{models.Count}] Completed: {model.Name} — {(result.Succeeded ? "OK" : "FAILED")} ({result.SucceededCases}/{result.TotalCases} cases)");
            }
        }
        finally
        {
            Log("Restoring original LLM settings...");
            try
            {
                await lgApi.UpdateLlmSettingsAsync(new UpdateLlmSettingsRequest
                {
                    Provider = originalSettings.Provider,
                    Model = originalSettings.Model
                }, CancellationToken.None);
                Log($"Restored: provider={originalSettings.Provider}, model={originalSettings.Model}");
            }
            catch (Exception ex)
            {
                Log($"WARNING: Failed to restore LLM settings: {ex.Message}");
            }

            summary.FinishedAtUtc = DateTimeOffset.UtcNow;
            await WriteSummaryAsync(summary, sessionRoot);
        }

        return summary;
    }

    private async Task<ModelBenchmarkResult> RunForModelAsync(
        BenchmarkModelEntry model,
        string sessionRoot,
        IReadOnlyDictionary<string, string>? masterTextOverrides,
        CancellationToken ct)
    {
        var result = new ModelBenchmarkResult
        {
            ModelName = model.Name,
            Provider = model.Provider,
            Model = model.Model,
            StartedAtUtc = DateTimeOffset.UtcNow,
        };

        var modelOutputRoot = Path.Combine(sessionRoot, SanitizeDirectoryName(model.Name));

        try
        {
            await SwitchModelAsync(model, ct);

            var modelPlanOptions = new TestPlanOptions
            {
                CsvPath = testPlanOptions.CsvPath,
                OutputRoot = modelOutputRoot
            };

            var runner = new TestPlanRunner(emulatorOptions, lgApi, modelPlanOptions, masterTextOverrides);
            var planSummary = await runner.RunAsync(ct);

            result.CollectedMasterTexts = runner.CollectedMasterTexts.Count > 0
                ? new Dictionary<string, string>(runner.CollectedMasterTexts)
                : null;

            result.Succeeded = planSummary.Succeeded;
            result.TotalCases = planSummary.Cases.Count;
            result.SucceededCases = planSummary.Cases.Count(c => c.Succeeded);
            result.FailedCases = planSummary.Cases.Count(c => !c.Succeeded);
            result.TestPlanOutputRoot = planSummary.OutputRoot;

            if (benchmarkOptions.RunAnalysis && analysisOptionsTemplate is not null)
            {
                try
                {
                    var analysisResult = await RunAnalysisAsync(model, planSummary.OutputRoot, sessionRoot, ct);
                    result.AnalysisOutputRoot = analysisResult.outputRoot;
                    result.Quality = analysisResult.quality;
                }
                catch (Exception ex)
                {
                    Log($"WARNING: Analysis failed for {model.Name}: {ex.Message}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            result.Succeeded = false;
            result.Error = "Canceled.";
            throw;
        }
        catch (Exception ex)
        {
            result.Succeeded = false;
            result.Error = ex.Message;
            Log($"ERROR: Model {model.Name} failed: {ex.Message}");
        }
        finally
        {
            result.FinishedAtUtc = DateTimeOffset.UtcNow;
        }

        return result;
    }

    private async Task SwitchModelAsync(BenchmarkModelEntry model, CancellationToken ct)
    {
        var provider = model.Provider.Trim();

        await lgApi.UpdateLlmSettingsAsync(new UpdateLlmSettingsRequest
        {
            Provider = provider,
            Model = model.Model
        }, ct);

        Log($"  LLM settings switched: provider={provider}, model={model.Model}");

        var providerSettingsRequest = new UpdateLlmProviderSettingsRequest
        {
            Provider = provider,
            Model = model.Model,
            Temperature = model.Temperature,
            MaxOutputTokens = model.MaxOutputTokens
        };

        await lgApi.UpdateLlmProviderSettingsAsync(provider, providerSettingsRequest, ct);
        Log($"  Provider settings updated: temp={model.Temperature}, maxTokens={model.MaxOutputTokens}");
    }

    private async Task<(string outputRoot, BenchmarkQualityAggregates? quality)> RunAnalysisAsync(
        BenchmarkModelEntry model,
        string testPlanOutputRoot,
        string sessionRoot,
        CancellationToken ct)
    {
        Log($"  Running analysis for {model.Name}...");

        var analysisOutputRoot = Path.Combine(sessionRoot, "analysis", SanitizeDirectoryName(model.Name));
        Directory.CreateDirectory(analysisOutputRoot);

        var analysisOptions = new JournalAnalysisOptions
        {
            InputDirectory = testPlanOutputRoot,
            OutputDirectory = analysisOutputRoot,
            OllamaBaseUri = analysisOptionsTemplate!.OllamaBaseUri,
            Model = analysisOptionsTemplate.Model,
            ApiKey = analysisOptionsTemplate.ApiKey,
            RequestTimeout = analysisOptionsTemplate.RequestTimeout,
            CriteriaPath = analysisOptionsTemplate.CriteriaPath,
            LlmProvider = analysisOptionsTemplate.LlmProvider
        };

        using var analysisHttp = new HttpClient
        {
            BaseAddress = analysisOptions.OllamaBaseUri,
            Timeout = analysisOptions.RequestTimeout
        };

        var analysisClient = new OllamaAnalysisClient(analysisHttp, analysisOptions.Model, analysisOptions.ApiKey, analysisOptions.LlmProvider);
        var analysisRunner = new JournalAnalysisRunner(analysisOptions, analysisClient);
        var analysisSummary = await analysisRunner.RunAsync(ct);

        var quality = AggregateQuality(analysisSummary);

        Log($"  Analysis complete for {model.Name}: {analysisSummary.Results.Count} journals analyzed.");

        return (analysisOutputRoot, quality);
    }

    private static BenchmarkQualityAggregates? AggregateQuality(JournalAnalysisSummary summary)
    {
        if (summary.Results.Count == 0)
        {
            return null;
        }

        var allQuality = summary.Results.SelectMany(r => r.Quality).ToList();
        var allLabs = summary.Results.SelectMany(r => r.Labs).ToList();

        if (allQuality.Count == 0)
        {
            return null;
        }

        var matchCount = 0;
        var differCount = 0;
        var logicalCount = 0;

        foreach (var result in summary.Results)
        {
            if (result.Discipline.AssignmentsMatchDiscipline) matchCount++;
            if (result.Discipline.LabsDiffer) differCount++;
            if (result.Discipline.SequenceLogical) logicalCount++;
        }

        return new BenchmarkQualityAggregates
        {
            AvgCorrectness = allQuality.Average(q => q.Correctness),
            AvgQuality = allQuality.Average(q => q.Quality),
            AvgCompleteness = allQuality.Average(q => q.Completeness),
            AvgClarity = allQuality.Average(q => q.Clarity),
            AssignmentsMatchDisciplineCount = matchCount,
            LabsDifferCount = differCount,
            SequenceLogicalCount = logicalCount,
            VariantsDifferCount = allLabs.Count(l => l.VariantsDiffer),
            VariantsSameDifficultyCount = allLabs.Count(l => l.VariantsSameDifficulty),
            TotalLabsAnalyzed = allLabs.Count
        };
    }

    private async Task WriteSummaryAsync(ModelBenchmarkSummary summary, string sessionRoot)
    {
        var jsonPath = Path.Combine(sessionRoot, "benchmark-report.json");
        var mdPath = Path.Combine(sessionRoot, "benchmark-report.md");

        var json = JsonSerializer.Serialize(summary, JsonWriteOptions);
        await File.WriteAllTextAsync(jsonPath, json, CancellationToken.None);

        var markdown = BuildMarkdown(summary);
        await File.WriteAllTextAsync(mdPath, markdown, CancellationToken.None);

        Log($"Benchmark report JSON: {jsonPath}");
        Log($"Benchmark report Markdown: {mdPath}");
    }

    private static string BuildMarkdown(ModelBenchmarkSummary summary)
    {
        var sb = new StringBuilder();

        sb.AppendLine("# Model Benchmark Report");
        sb.AppendLine();
        sb.AppendLine($"- Started (UTC): `{summary.StartedAtUtc:O}`");
        sb.AppendLine($"- Finished (UTC): `{summary.FinishedAtUtc:O}`");
        sb.AppendLine($"- Models file: `{summary.ModelsFilePath}`");
        sb.AppendLine($"- Test plan CSV: `{summary.TestPlanCsvPath}`");
        sb.AppendLine($"- Variants per lab: `{summary.VariantCount}`");
        sb.AppendLine($"- Models tested: `{summary.Models.Count}`");
        if (summary.Models.Count > 0)
        {
            sb.AppendLine($"- Seed model (master assignments): `{summary.Models[0].ModelName}`");
        }
        sb.AppendLine();

        sb.AppendLine("## Сводная таблица генерации");
        sb.AppendLine();
        sb.AppendLine("| Модель | Провайдер | Идентификатор | Роль | Сценариев | Успешных | Неудачных | Успех % | Время |");
        sb.AppendLine("|--------|-----------|---------------|------|:---------:|:--------:|:---------:|:-------:|------:|");

        for (var mi = 0; mi < summary.Models.Count; mi++)
        {
            var m = summary.Models[mi];
            var role = mi == 0 ? "seed + test" : "test";
            var pct = m.TotalCases > 0 ? (100.0 * m.SucceededCases / m.TotalCases).ToString("F1", CultureInfo.InvariantCulture) : "—";
            var duration = m.FinishedAtUtc.HasValue
                ? (m.FinishedAtUtc.Value - m.StartedAtUtc).ToString(@"hh\:mm\:ss")
                : "—";
            sb.AppendLine($"| {Esc(m.ModelName)} | {Esc(m.Provider)} | `{Esc(m.Model)}` | {role} | {m.TotalCases} | {m.SucceededCases} | {m.FailedCases} | {pct}% | {duration} |");
        }

        sb.AppendLine();

        var modelsWithQuality = summary.Models.Where(m => m.Quality is not null).ToList();
        if (modelsWithQuality.Count > 0)
        {
            sb.AppendLine("## Сводная таблица качества");
            sb.AppendLine();
            sb.AppendLine("| Модель | Корректность | Качество | Полнота | Ясность | Соотв. дисц. | ЛР отличаются | Логичность | Варианты отлич. | Ед. сложность | ЛР проанализ. |");
            sb.AppendLine("|--------|:------------:|:--------:|:-------:|:-------:|:------------:|:-------------:|:----------:|:---------------:|:-------------:|:-------------:|");

            foreach (var m in modelsWithQuality)
            {
                var q = m.Quality!;
                var total = q.TotalLabsAnalyzed > 0 ? q.TotalLabsAnalyzed : 1;
                sb.AppendLine(string.Format(
                    CultureInfo.InvariantCulture,
                    "| {0} | {1:F2} | {2:F2} | {3:F2} | {4:F2} | {5} | {6} | {7} | {8}/{9} ({10:F0}%) | {11}/{12} ({13:F0}%) | {14} |",
                    Esc(m.ModelName),
                    q.AvgCorrectness,
                    q.AvgQuality,
                    q.AvgCompleteness,
                    q.AvgClarity,
                    q.AssignmentsMatchDisciplineCount,
                    q.LabsDifferCount,
                    q.SequenceLogicalCount,
                    q.VariantsDifferCount, total, 100.0 * q.VariantsDifferCount / total,
                    q.VariantsSameDifficultyCount, total, 100.0 * q.VariantsSameDifficultyCount / total,
                    q.TotalLabsAnalyzed));
            }

            sb.AppendLine();
        }

        var modelsWithErrors = summary.Models.Where(m => !string.IsNullOrWhiteSpace(m.Error)).ToList();
        if (modelsWithErrors.Count > 0)
        {
            sb.AppendLine("## Ошибки");
            sb.AppendLine();
            foreach (var m in modelsWithErrors)
            {
                sb.AppendLine($"### {Esc(m.ModelName)}");
                sb.AppendLine();
                sb.AppendLine($"```\n{m.Error}\n```");
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    private static string Esc(string value)
        => (value ?? string.Empty).Replace("\r", " ").Replace("\n", " ").Replace("|", "\\|").Trim();

    private static string SanitizeDirectoryName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var sb = new StringBuilder(name.Length);
        foreach (var ch in name)
        {
            sb.Append(invalid.Contains(ch) ? '_' : ch);
        }

        return sb.ToString();
    }

    private static void Log(string message)
        => Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] [benchmark] {message}");
}
