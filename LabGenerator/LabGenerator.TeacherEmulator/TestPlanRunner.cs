using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LabGenerator.TeacherEmulator;

public sealed class TestPlanRunner(
    TeacherEmulatorOptions options,
    LgApiClient lgApi,
    TestPlanOptions planOptions,
    IReadOnlyDictionary<string, string>? masterTextOverrides = null)
{
    private const string PartialLabReportDataKey = "__partial_lab_report";
    private const int JobStatusSucceeded = 2;
    private const int JobStatusFailed = 3;
    private const int JobStatusCanceled = 4;

    private readonly Dictionary<string, string> _collectedMasterTexts = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Master texts collected during this run, keyed by "DisciplineName|LabOrderIndex".
    /// Populated after each lab's master is approved.
    /// </summary>
    public IReadOnlyDictionary<string, string> CollectedMasterTexts => _collectedMasterTexts;

    public static string BuildMasterKey(string disciplineName, int labOrderIndex)
        => $"{disciplineName}|{labOrderIndex}";

    private static readonly JsonSerializerOptions SummaryJsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public async Task<TestPlanSummary> RunAsync(CancellationToken ct)
    {
        var cases = TestPlanCsvReader.Load(planOptions.CsvPath);
        var methods = await lgApi.GetVariationMethodsAsync(ct);
        var methodsById = methods.ToDictionary(x => x.Id);

        var startedAt = DateTimeOffset.UtcNow;
        var sessionId = startedAt.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
        var sessionRoot = Path.Combine(planOptions.OutputRoot, $"run-{sessionId}");
        Directory.CreateDirectory(sessionRoot);

        var summary = new TestPlanSummary
        {
            CsvPath = planOptions.CsvPath,
            OutputRoot = sessionRoot,
            StartedAtUtc = startedAt
        };

        try
        {
            foreach (var testCase in cases)
            {
                var report = await RunCaseAsync(testCase, methodsById, sessionRoot, ct);

                // Используем CancellationToken.None: запись результатов должна выполняться
                // даже если ct уже отменён (например, по Ctrl+C).
                var paths = await ReportWriter.WriteAsync(report, sessionRoot, CancellationToken.None);

                summary.Cases.Add(new TestPlanCaseResult
                {
                    TestNumber = testCase.TestNumber,
                    Discipline = testCase.DisciplineName,
                    LabNumber = testCase.LabNumber,
                    Succeeded = report.Succeeded,
                    JsonPath = paths.JsonPath,
                    MarkdownPath = paths.MarkdownPath
                });

                if (ct.IsCancellationRequested)
                {
                    Console.WriteLine("Execution canceled.");
                    break;
                }
            }
        }
        finally
        {
            summary.FinishedAtUtc = DateTimeOffset.UtcNow;
            summary.Succeeded = summary.Cases.All(x => x.Succeeded);

            var summaryPath = Path.Combine(sessionRoot, "summary.json");
            var json = JsonSerializer.Serialize(summary, SummaryJsonOptions);
            await File.WriteAllTextAsync(summaryPath, json, CancellationToken.None);

            Console.WriteLine($"Test plan summary: {summaryPath}");
            Console.WriteLine(summary.Succeeded
                ? "Test plan completed successfully."
                : "Test plan completed with failures.");
        }

        return summary;
    }

    private async Task<TeacherEmulatorReport> RunCaseAsync(
        TestPlanCase testCase,
        IReadOnlyDictionary<int, VariationMethodDto> methodsById,
        string sessionRoot,
        CancellationToken ct)
    {
        var report = new TeacherEmulatorReport
        {
            RunId = $"test-{testCase.TestNumber:000}",
            StartedAtUtc = DateTimeOffset.UtcNow,
            Options = BuildSnapshot(sessionRoot)
        };

        try
        {
            Log(report, "init", "Test plan case started.", data: new()
            {
                ["test_number"] = testCase.TestNumber.ToString(CultureInfo.InvariantCulture),
                ["discipline"] = testCase.DisciplineName
            });

            var disciplineName = $"{testCase.DisciplineName} (Test {testCase.TestNumber})";
            var discipline = await lgApi.CreateDisciplineAsync(new CreateDisciplineRequest
            {
                Name = disciplineName,
                Description = $"Test plan case {testCase.TestNumber} from {Path.GetFileName(planOptions.CsvPath)}."
            }, ct);

            report.Discipline = discipline;
            Log(report, "discipline.create", "Discipline created.", data: new()
            {
                ["discipline_id"] = discipline.Id.ToString(CultureInfo.InvariantCulture),
                ["name"] = discipline.Name
            });

            var selectedMethods = BuildSelectedMethods(testCase, methodsById);

            for (var labIndex = 1; labIndex <= testCase.LabNumber; labIndex++)
            {
                var plan = LabThemeResolver.BuildLabPlan(testCase.DisciplineName, labIndex);

                try
                {
                    var labReport = await RunLabCycleAsync(testCase.DisciplineName, discipline, plan, selectedMethods, report, ct);
                    report.Labs.Add(labReport);
                }
                catch (Exception ex)
                {
                    if (TryGetPartialLabReport(ex, out var partialLabReport))
                    {
                        report.Labs.Add(partialLabReport);
                    }

                    throw;
                }
            }

            report.Succeeded = report.Labs.All(x => x.AllVariantsPassed);
            if (!report.Succeeded)
            {
                report.Error = "One or more labs have unverified variants.";
            }
        }
        catch (Exception ex)
        {
            report.Succeeded = false;
            report.Error = ex.ToString();
            Log(report, "fatal", ex.Message, level: "error");
        }
        finally
        {
            report.FinishedAtUtc = DateTimeOffset.UtcNow;
            Log(report, "done", report.Succeeded ? "Case completed successfully." : "Case completed with failures.");
        }

        return report;
    }

    private async Task<LabExecutionReport> RunLabCycleAsync(
        string originalDisciplineName,
        DisciplineDto discipline,
        LabPlanItem plan,
        List<SelectedVariationMethodExecution> selectedMethods,
        TeacherEmulatorReport report,
        CancellationToken ct)
    {
        var stage = new StageTracker(plan.OrderIndex);
        var labReport = new LabExecutionReport
        {
            LabNumber = plan.OrderIndex,
            Plan = plan
        };

        try
        {
            labReport.CreatedLab = await CreateLabAsync(discipline, plan, stage, report, ct);
            await GenerateAndApproveMasterAsync(originalDisciplineName, labReport, stage, report, ct);
            await ConfigureVariationMethodsAsync(labReport.CreatedLab.Id, selectedMethods, labReport, stage, report, ct);
            await GenerateVariantsAsync(labReport.CreatedLab.Id, labReport, stage, report, ct);
            await RunVerificationAndRecoveryLoopAsync(labReport.CreatedLab.Id, labReport, report, ct);

            stage.Set("complete");
            Log(report, stage.Current, labReport.AllVariantsPassed
                ? "Lab cycle completed successfully."
                : "Lab cycle completed with unresolved verification issues.");

            return labReport;
        }
        catch (Exception ex)
        {
            labReport.FailedStage = stage.Current;
            labReport.Error = ex.Message;
            ex.Data[PartialLabReportDataKey] = labReport;
            throw;
        }
    }

    private async Task<LabDto> CreateLabAsync(
        DisciplineDto discipline,
        LabPlanItem plan,
        StageTracker stage,
        TeacherEmulatorReport report,
        CancellationToken ct)
    {
        stage.Set("create");
        Log(report, stage.Current, "Creating lab.");

        var createdLab = await lgApi.CreateLabAsync(new CreateLabRequest
        {
            DisciplineId = discipline.Id,
            OrderIndex = plan.OrderIndex,
            Title = plan.Title,
            InitialDescription = plan.InitialDescription
        }, ct);

        Log(report, stage.Current, "Lab created.", data: new()
        {
            ["lab_id"] = createdLab.Id.ToString(CultureInfo.InvariantCulture),
            ["title"] = createdLab.Title
        });

        return createdLab;
    }

    private async Task GenerateAndApproveMasterAsync(
        string originalDisciplineName,
        LabExecutionReport labReport,
        StageTracker stage,
        TeacherEmulatorReport report,
        CancellationToken ct)
    {
        var labId = labReport.CreatedLab!.Id;
        var masterKey = BuildMasterKey(originalDisciplineName, labReport.LabNumber);

        stage.Set("master.generate");
        var masterJob = await lgApi.GenerateMasterAsync(labId, ct);
        labReport.MasterGenerationJobId = masterJob.Id;
        Log(report, stage.Current, "Master generation started.", data: new()
        {
            ["job_id"] = masterJob.Id.ToString(CultureInfo.InvariantCulture)
        });
        await WaitJobAsync(masterJob.Id, stage.Current, report, ct);

        stage.Set("master.review");
        var master = await lgApi.GetCurrentMasterAsync(labId, ct);
        labReport.MasterBeforeReview = master;
        Log(report, stage.Current, "Master draft loaded.", data: new()
        {
            ["master_id"] = master.Id.ToString(CultureInfo.InvariantCulture),
            ["version"] = master.Version.ToString(CultureInfo.InvariantCulture)
        });

        if (masterTextOverrides is not null &&
            masterTextOverrides.TryGetValue(masterKey, out var overrideText))
        {
            stage.Set("master.override");
            master = await lgApi.UpdateMasterAsync(labId, master.Id,
                new UpdateMasterAssignmentRequest { Content = overrideText }, ct);
            labReport.MasterUpdated = true;
            labReport.MasterReviewComment = "Content replaced with seed master from benchmark.";
            Log(report, stage.Current, "Master content replaced with seed text.", data: new()
            {
                ["master_key"] = masterKey,
                ["override_length"] = overrideText.Length.ToString(CultureInfo.InvariantCulture)
            });
        }

        stage.Set("master.approve");
        master = await lgApi.ApproveMasterAsync(labId, master.Id, ct);
        labReport.MasterAfterReview = master;
        Log(report, stage.Current, "Master assignment approved.", data: new()
        {
            ["master_id"] = master.Id.ToString(CultureInfo.InvariantCulture),
            ["status"] = master.Status.ToString(CultureInfo.InvariantCulture)
        });

        _collectedMasterTexts[masterKey] = master.Content;
    }

    private async Task ConfigureVariationMethodsAsync(
        int labId,
        List<SelectedVariationMethodExecution> selectedMethods,
        LabExecutionReport labReport,
        StageTracker stage,
        TeacherEmulatorReport report,
        CancellationToken ct)
    {
        stage.Set("variation.apply");

        var request = new UpsertLabVariationMethodsRequest
        {
            Items = selectedMethods
                .Select(x => new LabVariationMethodItemRequest
                {
                    VariationMethodId = x.VariationMethodId,
                    PreserveAcrossLabs = x.PreserveAcrossLabs
                })
                .ToList()
        };

        await lgApi.UpsertLabVariationMethodsAsync(labId, request, ct);
        labReport.AppliedVariationMethods = CloneMethods(selectedMethods);

        Log(report, stage.Current, "Variation methods configured.", data: new()
        {
            ["methods"] = selectedMethods.Count == 0
                ? "<none>"
                : string.Join(", ", selectedMethods.Select(x => $"{x.Code}:{x.PreserveAcrossLabs}"))
        });
    }

    private async Task GenerateVariantsAsync(
        int labId,
        LabExecutionReport labReport,
        StageTracker stage,
        TeacherEmulatorReport report,
        CancellationToken ct)
    {
        stage.Set("variants.generate");

        var variantsJob = await lgApi.GenerateVariantsAsync(labId, new GenerateVariantsRequest
        {
            Count = options.VariantCountPerLab
        }, ct);

        labReport.VariantsGenerationJobId = variantsJob.Id;
        Log(report, stage.Current, "Variants generation started.", data: new()
        {
            ["job_id"] = variantsJob.Id.ToString(CultureInfo.InvariantCulture),
            ["count"] = options.VariantCountPerLab.ToString(CultureInfo.InvariantCulture)
        });

        await WaitJobAsync(variantsJob.Id, stage.Current, report, ct);
    }

    private async Task RunVerificationAndRecoveryLoopAsync(
        int labId,
        LabExecutionReport labReport,
        TeacherEmulatorReport report,
        CancellationToken ct)
    {
        var stagePrefix = $"lab#{labReport.LabNumber}";

        await VerifyAllAsync(labId, labReport, report, $"{stagePrefix}.verification.initial", ct);
        await RefreshVerificationStateAsync(labId, labReport, report, $"{stagePrefix}.verification.initial", ct);

        while (!labReport.AllVariantsPassed && labReport.VerificationRetries < options.MaxVerificationRetries)
        {
            var failedVariantIds = labReport.VerificationReports
                .Where(x => !x.Passed)
                .Select(x => x.VariantId)
                .ToList();

            if (failedVariantIds.Count == 0)
            {
                break;
            }

            labReport.VerificationRetries++;
            Log(report, $"{stagePrefix}.verification.retry", "Retrying verification for failed variants.", data: new()
            {
                ["attempt"] = labReport.VerificationRetries.ToString(CultureInfo.InvariantCulture),
                ["failed_variants"] = string.Join(", ", failedVariantIds)
            });

            foreach (var variantId in failedVariantIds)
            {
                var retryJob = await lgApi.VerifyLabAsync(labId, new VerifyVariantsRequest { VariantId = variantId }, ct);
                labReport.VerificationJobIds.Add(retryJob.Id);
                await WaitJobAsync(retryJob.Id, $"{stagePrefix}.verification.retry.variant{variantId}", report, ct);
            }

            await RefreshVerificationStateAsync(labId, labReport, report, $"{stagePrefix}.verification.retry", ct);
        }

        while (!labReport.AllVariantsPassed && labReport.RegenerationRetries < options.MaxRegenerationRetries)
        {
            var failedCount = labReport.VerificationReports.Count(x => !x.Passed);
            if (failedCount <= 0)
            {
                break;
            }

            labReport.RegenerationRetries++;
            Log(report, $"{stagePrefix}.regeneration", "Regenerating extra variants due to failed verification.", data: new()
            {
                ["attempt"] = labReport.RegenerationRetries.ToString(CultureInfo.InvariantCulture),
                ["count"] = failedCount.ToString(CultureInfo.InvariantCulture)
            });

            var regenJob = await lgApi.GenerateVariantsAsync(labId, new GenerateVariantsRequest
            {
                Count = failedCount
            }, ct);
            labReport.ExtraVariantsGenerationJobIds.Add(regenJob.Id);
            await WaitJobAsync(regenJob.Id, $"{stagePrefix}.regeneration.generate", report, ct);

            await VerifyAllAsync(labId, labReport, report, $"{stagePrefix}.regeneration.verify", ct);
            await RefreshVerificationStateAsync(labId, labReport, report, $"{stagePrefix}.regeneration.verify", ct);
        }
    }

    private async Task VerifyAllAsync(
        int labId,
        LabExecutionReport labReport,
        TeacherEmulatorReport report,
        string stage,
        CancellationToken ct)
    {
        var verifyJob = await lgApi.VerifyLabAsync(labId, new VerifyVariantsRequest(), ct);
        labReport.VerificationJobIds.Add(verifyJob.Id);
        Log(report, stage, "Verification job started.", data: new()
        {
            ["job_id"] = verifyJob.Id.ToString(CultureInfo.InvariantCulture)
        });
        await WaitJobAsync(verifyJob.Id, stage, report, ct);
    }

    private async Task RefreshVerificationStateAsync(
        int labId,
        LabExecutionReport labReport,
        TeacherEmulatorReport report,
        string stage,
        CancellationToken ct)
    {
        var variants = await lgApi.GetVariantsAsync(labId, ct);
        var reports = await lgApi.GetLabVerificationReportsAsync(labId, ct);
        var reportsByVariantId = reports.ToDictionary(x => x.AssignmentVariantId);

        labReport.Variants = variants
            .OrderBy(x => x.VariantIndex)
            .ToList();

        labReport.VerificationReports = variants
            .OrderBy(x => x.VariantIndex)
            .Select(variant => BuildVerificationSummary(variant, reportsByVariantId))
            .ToList();

        labReport.AllVariantsPassed = labReport.VerificationReports.Count > 0 &&
                                      labReport.VerificationReports.All(x => x.Passed);

        Log(report, stage, "Verification reports refreshed.", data: new()
        {
            ["variants_total"] = labReport.Variants.Count.ToString(CultureInfo.InvariantCulture),
            ["passed"] = labReport.VerificationReports.Count(x => x.Passed).ToString(CultureInfo.InvariantCulture),
            ["failed"] = labReport.VerificationReports.Count(x => !x.Passed).ToString(CultureInfo.InvariantCulture)
        });
    }

    private static VerificationReportSummary BuildVerificationSummary(
        AssignmentVariantDto variant,
        IReadOnlyDictionary<int, VerificationReportDto> reportsByVariantId)
    {
        if (!reportsByVariantId.TryGetValue(variant.Id, out var verification))
        {
            return new VerificationReportSummary
            {
                VariantId = variant.Id,
                Passed = false,
                OverallScore = null,
                IssueCount = 1,
                IssuesJson = "[{\"code\":\"MISSING_REPORT\",\"message\":\"No verification report found.\"}]"
            };
        }

        return new VerificationReportSummary
        {
            VariantId = variant.Id,
            Passed = verification.Passed,
            OverallScore = TryExtractOverallScore(verification.JudgeScoreJson),
            IssueCount = TryCountIssues(verification.IssuesJson),
            IssuesJson = verification.IssuesJson
        };
    }

    private async Task WaitJobAsync(int jobId, string stage, TeacherEmulatorReport report, CancellationToken ct)
    {
        var started = DateTimeOffset.UtcNow;
        while (true)
        {
            ct.ThrowIfCancellationRequested();

            var job = await lgApi.GetJobAsync(jobId, ct);
            if (job.Status == JobStatusSucceeded)
            {
                Log(report, stage, "Job completed.", data: new()
                {
                    ["job_id"] = jobId.ToString(CultureInfo.InvariantCulture),
                    ["progress"] = job.Progress.ToString(CultureInfo.InvariantCulture)
                });
                return;
            }

            if (job.Status is JobStatusFailed or JobStatusCanceled)
            {
                LogRejectedGenerationRounds(job.Error, stage, report);
                throw new InvalidOperationException(
                    $"Job {jobId} failed at stage {stage}. Status={job.Status}, Error={job.Error}");
            }

            if (DateTimeOffset.UtcNow - started > options.JobTimeout)
            {
                throw new TimeoutException($"Job {jobId} exceeded timeout at stage {stage}.");
            }

            await Task.Delay(options.JobPollInterval, ct);
        }
    }

    private static List<SelectedVariationMethodExecution> BuildSelectedMethods(
        TestPlanCase testCase,
        IReadOnlyDictionary<int, VariationMethodDto> methodsById)
    {
        var selected = new Dictionary<int, SelectedVariationMethodExecution>();

        AddMethod(selected, testCase.Param1, testCase.PreserveParam1, testCase.TestNumber, methodsById);
        AddMethod(selected, testCase.Param2, testCase.PreserveParam2, testCase.TestNumber, methodsById);

        return selected.Values
            .OrderBy(x => x.VariationMethodId)
            .ToList();
    }

    private static void AddMethod(
        Dictionary<int, SelectedVariationMethodExecution> selected,
        int? methodId,
        bool preserve,
        int testNumber,
        IReadOnlyDictionary<int, VariationMethodDto> methodsById)
    {
        if (methodId is null)
        {
            return;
        }

        if (!methodsById.TryGetValue(methodId.Value, out var method))
        {
            throw new InvalidOperationException(
                $"Unknown VariationMethodId {methodId} in test {testNumber}.");
        }

        if (!selected.TryGetValue(methodId.Value, out var entry))
        {
            selected[methodId.Value] = new SelectedVariationMethodExecution
            {
                VariationMethodId = method.Id,
                Code = method.Code,
                PreserveAcrossLabs = preserve
            };
            return;
        }

        entry.PreserveAcrossLabs = entry.PreserveAcrossLabs || preserve;
    }

    private static bool TryGetPartialLabReport(Exception ex, out LabExecutionReport partialLabReport)
    {
        if (ex.Data[PartialLabReportDataKey] is LabExecutionReport report)
        {
            partialLabReport = report;
            return true;
        }

        partialLabReport = null!;
        return false;
    }

    private static void LogRejectedGenerationRounds(string? jobError, string stage, TeacherEmulatorReport report)
    {
        if (string.IsNullOrWhiteSpace(jobError))
        {
            return;
        }

        var lines = jobError
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var started = false;
        foreach (var line in lines)
        {
            if (!started)
            {
                if (line.Equals("Rejected rounds:", StringComparison.OrdinalIgnoreCase))
                {
                    started = true;
                }

                continue;
            }

            if (!line.StartsWith("- ", StringComparison.Ordinal))
            {
                continue;
            }

            Log(report, $"{stage}.rejection", line[2..].Trim(), level: "warning");
        }
    }

    private TeacherEmulatorOptionsSnapshot BuildSnapshot(string outputRoot)
    {
        var snapshot = options.ToSnapshot();
        snapshot.OutputDirectory = outputRoot;
        return snapshot;
    }

    private static List<SelectedVariationMethodExecution> CloneMethods(
        IEnumerable<SelectedVariationMethodExecution> methods)
    {
        return methods.Select(x => new SelectedVariationMethodExecution
        {
            VariationMethodId = x.VariationMethodId,
            Code = x.Code,
            PreserveAcrossLabs = x.PreserveAcrossLabs
        }).ToList();
    }

    private static int? TryExtractOverallScore(string? judgeScoreJson)
    {
        if (string.IsNullOrWhiteSpace(judgeScoreJson))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(judgeScoreJson);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            if (!doc.RootElement.TryGetProperty("overall", out var overall))
            {
                return null;
            }

            return overall.ValueKind switch
            {
                JsonValueKind.Number when overall.TryGetInt32(out var asInt) => asInt,
                JsonValueKind.String when int.TryParse(overall.GetString(), out var fromString) => fromString,
                _ => null
            };
        }
        catch
        {
            return null;
        }
    }

    private static int TryCountIssues(string? issuesJson)
    {
        if (string.IsNullOrWhiteSpace(issuesJson))
        {
            return 0;
        }

        try
        {
            using var doc = JsonDocument.Parse(issuesJson);
            return doc.RootElement.ValueKind == JsonValueKind.Array
                ? doc.RootElement.GetArrayLength()
                : 0;
        }
        catch
        {
            return 0;
        }
    }

    private static void Log(
        TeacherEmulatorReport report,
        string stage,
        string message,
        string level = "info",
        Dictionary<string, string>? data = null)
    {
        var evt = new JournalEvent
        {
            TimestampUtc = DateTimeOffset.UtcNow,
            Stage = stage,
            Message = message,
            Level = level,
            Data = data ?? new Dictionary<string, string>()
        };

        report.Events.Add(evt);

        var tail = evt.Data.Count == 0
            ? string.Empty
            : $" | {string.Join(", ", evt.Data.Select(x => $"{x.Key}={x.Value}"))}";
        Console.WriteLine($"[{evt.TimestampUtc:O}] [{evt.Level.ToUpperInvariant()}] {evt.Stage}: {evt.Message}{tail}");
    }

    /// <summary>
    /// Tracks the current stage within a lab cycle, prefixing with the lab index.
    /// </summary>
    private sealed class StageTracker(int labIndex)
    {
        public string Current { get; private set; } = $"lab#{labIndex}.create";

        public void Set(string stage) => Current = $"lab#{labIndex}.{stage}";
    }
}
