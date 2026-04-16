using System.Text.Json;

namespace LabGenerator.TeacherEmulator;

public sealed class TeacherEmulatorRunner(
    TeacherEmulatorOptions options,
    LgApiClient lgApi,
    OllamaTeacherClient teacherClient)
{
    private const int JobStatusSucceeded = 2;
    private const int JobStatusFailed = 3;
    private const int JobStatusCanceled = 4;

    public async Task<TeacherEmulatorReport> RunAsync(CancellationToken ct)
    {
        var report = new TeacherEmulatorReport
        {
            RunId = DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss"),
            StartedAtUtc = DateTimeOffset.UtcNow,
            Options = options.ToSnapshot()
        };

        try
        {
            Log(report, "init", "Teacher emulator started.");

            var curriculumOverride = CurriculumDisciplineOverrideLoader.TryLoad(
                Directory.GetCurrentDirectory(),
                AppContext.BaseDirectory);
            var plan = await teacherClient.BuildDisciplinePlanAsync(
                options.LabCount,
                options.SeedTopic,
                curriculumOverride,
                ct);
            if (curriculumOverride is not null)
            {
                plan.Name = curriculumOverride.Name;
                plan.Description = curriculumOverride.Description;
                Log(report, "planning.curriculum", "Curriculum file applied to planning and discipline metadata.", data: new()
                {
                    ["path"] = curriculumOverride.Path,
                    ["discipline_name"] = plan.Name
                });
            }

            Log(report, "planning", "Discipline and lab plan prepared.", data: new()
            {
                ["discipline_name"] = plan.Name,
                ["labs"] = plan.Labs.Count.ToString()
            });

            var discipline = await lgApi.CreateDisciplineAsync(new CreateDisciplineRequest
            {
                Name = plan.Name,
                Description = plan.Description
            }, ct);

            report.Discipline = discipline;
            Log(report, "discipline.create", "Discipline created.", data: new()
            {
                ["discipline_id"] = discipline.Id.ToString(),
                ["name"] = discipline.Name
            });

            var allMethods = await lgApi.GetVariationMethodsAsync(ct);
            Log(report, "variation.methods.load", "Variation methods fetched.", data: new()
            {
                ["count"] = allMethods.Count.ToString()
            });

            foreach (var labPlan in plan.Labs.OrderBy(x => x.OrderIndex).Take(options.LabCount))
            {
                var labReport = await RunLabCycleAsync(discipline, labPlan, allMethods, report, ct);
                report.Labs.Add(labReport);
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
            Log(report, "done", report.Succeeded ? "Run completed successfully." : "Run completed with failures.");
        }

        return report;
    }

    private async Task<LabExecutionReport> RunLabCycleAsync(
        DisciplineDto discipline,
        LabPlanItem labPlan,
        IReadOnlyList<VariationMethodDto> availableMethods,
        TeacherEmulatorReport report,
        CancellationToken ct)
    {
        var labStagePrefix = $"lab#{labPlan.OrderIndex}";
        var labReport = new LabExecutionReport
        {
            LabNumber = labPlan.OrderIndex,
            Plan = labPlan
        };

        Log(report, $"{labStagePrefix}.create", "Creating lab.");
        var createdLab = await lgApi.CreateLabAsync(new CreateLabRequest
        {
            DisciplineId = discipline.Id,
            OrderIndex = labPlan.OrderIndex,
            Title = labPlan.Title,
            InitialDescription = labPlan.InitialDescription
        }, ct);
        labReport.CreatedLab = createdLab;
        Log(report, $"{labStagePrefix}.create", "Lab created.", data: new()
        {
            ["lab_id"] = createdLab.Id.ToString(),
            ["title"] = createdLab.Title
        });

        var masterJob = await lgApi.GenerateMasterAsync(createdLab.Id, ct);
        labReport.MasterGenerationJobId = masterJob.Id;
        Log(report, $"{labStagePrefix}.master.generate", "Master generation started.", data: new()
        {
            ["job_id"] = masterJob.Id.ToString()
        });
        await WaitJobAsync(masterJob.Id, $"{labStagePrefix}.master.generate", report, ct);

        var master = await lgApi.GetCurrentMasterAsync(createdLab.Id, ct);
        labReport.MasterBeforeReview = master;
        Log(report, $"{labStagePrefix}.master.review", "Master draft loaded.", data: new()
        {
            ["master_id"] = master.Id.ToString(),
            ["version"] = master.Version.ToString()
        });

        var reviewDecision = await teacherClient.ReviewMasterAsync(labPlan, master.Content, ct);
        labReport.MasterReviewComment = reviewDecision.Comment;
        if (reviewDecision.NeedsUpdate &&
            !string.IsNullOrWhiteSpace(reviewDecision.UpdatedContent) &&
            !string.Equals(reviewDecision.UpdatedContent.Trim(), master.Content.Trim(), StringComparison.Ordinal))
        {
            master = await lgApi.UpdateMasterAsync(createdLab.Id, master.Id, new UpdateMasterAssignmentRequest
            {
                Content = reviewDecision.UpdatedContent
            }, ct);

            labReport.MasterUpdated = true;
            Log(report, $"{labStagePrefix}.master.review", "Master assignment updated by teacher emulator.");
        }
        else
        {
            Log(report, $"{labStagePrefix}.master.review", "Master assignment accepted without changes.");
        }

        master = await lgApi.ApproveMasterAsync(createdLab.Id, master.Id, ct);
        labReport.MasterAfterReview = master;
        Log(report, $"{labStagePrefix}.master.approve", "Master assignment approved.", data: new()
        {
            ["master_id"] = master.Id.ToString(),
            ["status"] = master.Status.ToString()
        });

        var selection = await teacherClient.SelectVariationMethodsAsync(labPlan.OrderIndex, availableMethods, ct);
        var selectedMethods = ResolveVariationSelection(selection, availableMethods);
        var upsertPayload = new UpsertLabVariationMethodsRequest
        {
            Items = selectedMethods
                .Select(x => new LabVariationMethodItemRequest
                {
                    VariationMethodId = x.VariationMethodId,
                    PreserveAcrossLabs = x.PreserveAcrossLabs
                })
                .ToList()
        };
        await lgApi.UpsertLabVariationMethodsAsync(createdLab.Id, upsertPayload, ct);

        labReport.AppliedVariationMethods = selectedMethods;
        Log(report, $"{labStagePrefix}.variation.apply", "Variation methods configured.", data: new()
        {
            ["methods"] = string.Join(", ", selectedMethods.Select(x => $"{x.Code}:{x.PreserveAcrossLabs}"))
        });

        await GenerateInitialVariantsWithFallbackAsync(
            createdLab.Id,
            availableMethods,
            labReport,
            report,
            $"{labStagePrefix}.variants.generate",
            ct);

        await RunVerificationAndRecoveryLoopAsync(createdLab.Id, labReport, report, ct);

        Log(report, $"{labStagePrefix}.complete", labReport.AllVariantsPassed
            ? "Lab cycle completed successfully."
            : "Lab cycle completed with unresolved verification issues.");

        return labReport;
    }

    private async Task GenerateInitialVariantsWithFallbackAsync(
        int labId,
        IReadOnlyList<VariationMethodDto> availableMethods,
        LabExecutionReport labReport,
        TeacherEmulatorReport report,
        string stage,
        CancellationToken ct)
    {
        var baselineMethods = CloneMethods(labReport.AppliedVariationMethods);
        var baselineSignature = MethodsSignature(baselineMethods);
        var appliedSignature = baselineSignature;

        var methodSets = new List<List<SelectedVariationMethodExecution>>
        {
            CloneMethods(baselineMethods),
            BuildMinimalFallbackMethods(availableMethods),
            new List<SelectedVariationMethodExecution>()
        };

        var distinctMethodSets = methodSets
            .GroupBy(MethodsSignature, StringComparer.Ordinal)
            .Select(g => g.First())
            .ToList();

        Exception? lastFailure = null;

        try
        {
            for (var attempt = 0; attempt < distinctMethodSets.Count; attempt++)
            {
                var methods = distinctMethodSets[attempt];
                var label = attempt == 0 ? "primary" : $"fallback-{attempt}";

                appliedSignature = MethodsSignature(methods);
                await ApplyVariationMethodsAsync(labId, methods, ct);

                Log(report, stage, "Starting variants generation attempt.", data: new()
                {
                    ["attempt"] = label,
                    ["methods"] = methods.Count == 0
                        ? "<none>"
                        : string.Join(", ", methods.Select(x => $"{x.Code}:{x.PreserveAcrossLabs}"))
                });

                try
                {
                    var job = await lgApi.GenerateVariantsAsync(labId, new GenerateVariantsRequest
                    {
                        Count = options.VariantCountPerLab
                    }, ct);

                    if (labReport.VariantsGenerationJobId is null)
                    {
                        labReport.VariantsGenerationJobId = job.Id;
                    }
                    else
                    {
                        labReport.ExtraVariantsGenerationJobIds.Add(job.Id);
                    }

                    Log(report, stage, "Variants generation started.", data: new()
                    {
                        ["attempt"] = label,
                        ["job_id"] = job.Id.ToString(),
                        ["count"] = options.VariantCountPerLab.ToString()
                    });

                    await WaitJobAsync(job.Id, stage, report, ct);
                    return;
                }
                catch (Exception ex)
                {
                    lastFailure = ex;

                    if (attempt == distinctMethodSets.Count - 1)
                    {
                        break;
                    }

                    Log(report, stage, "Variants generation attempt failed. Trying fallback strategy.", level: "warning", data: new()
                    {
                        ["attempt"] = label,
                        ["error"] = ex.Message
                    });
                }
            }

            throw new InvalidOperationException(
                $"All initial variants generation attempts failed for lab {labId}. Last error: {lastFailure?.Message}",
                lastFailure);
        }
        finally
        {
            if (!string.Equals(appliedSignature, baselineSignature, StringComparison.Ordinal))
            {
                await ApplyVariationMethodsAsync(labId, baselineMethods, ct);

                Log(report, $"{stage}.restore", "Variation methods restored to initial selection after fallback attempts.", data: new()
                {
                    ["methods"] = baselineMethods.Count == 0
                        ? "<none>"
                        : string.Join(", ", baselineMethods.Select(x => $"{x.Code}:{x.PreserveAcrossLabs}"))
                });
            }
        }
    }

    private async Task ApplyVariationMethodsAsync(
        int labId,
        List<SelectedVariationMethodExecution> methods,
        CancellationToken ct)
    {
        var request = new UpsertLabVariationMethodsRequest
        {
            Items = methods
                .Select(x => new LabVariationMethodItemRequest
                {
                    VariationMethodId = x.VariationMethodId,
                    PreserveAcrossLabs = x.PreserveAcrossLabs
                })
                .ToList()
        };

        await lgApi.UpsertLabVariationMethodsAsync(labId, request, ct);
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
                ["attempt"] = labReport.VerificationRetries.ToString(),
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
                ["attempt"] = labReport.RegenerationRetries.ToString(),
                ["count"] = failedCount.ToString()
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
            ["job_id"] = verifyJob.Id.ToString()
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
            .Select(variant =>
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
            })
            .ToList();

        labReport.AllVariantsPassed = labReport.VerificationReports.Count > 0 &&
                                      labReport.VerificationReports.All(x => x.Passed);

        Log(report, stage, "Verification reports refreshed.", data: new()
        {
            ["variants_total"] = labReport.Variants.Count.ToString(),
            ["passed"] = labReport.VerificationReports.Count(x => x.Passed).ToString(),
            ["failed"] = labReport.VerificationReports.Count(x => !x.Passed).ToString()
        });
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
                    ["job_id"] = jobId.ToString(),
                    ["progress"] = job.Progress.ToString()
                });
                return;
            }

            if (job.Status == JobStatusFailed || job.Status == JobStatusCanceled)
            {
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

    private static List<SelectedVariationMethodExecution> ResolveVariationSelection(
        VariationSelectionDecision decision,
        IReadOnlyList<VariationMethodDto> available)
    {
        var byCode = available.ToDictionary(x => x.Code, StringComparer.OrdinalIgnoreCase);
        var resolved = new List<SelectedVariationMethodExecution>();

        foreach (var item in decision.Items)
        {
            if (string.IsNullOrWhiteSpace(item.Code))
            {
                continue;
            }

            if (!byCode.TryGetValue(item.Code.Trim(), out var method))
            {
                continue;
            }

            if (resolved.Any(x => x.VariationMethodId == method.Id))
            {
                continue;
            }

            resolved.Add(new SelectedVariationMethodExecution
            {
                VariationMethodId = method.Id,
                Code = method.Code,
                PreserveAcrossLabs = item.PreserveAcrossLabs
            });
        }

        if (resolved.Count == 0)
        {
            foreach (var fallback in available.Take(2))
            {
                resolved.Add(new SelectedVariationMethodExecution
                {
                    VariationMethodId = fallback.Id,
                    Code = fallback.Code,
                    PreserveAcrossLabs = resolved.Count == 0
                });
            }
        }

        if (!resolved.Any(x => x.PreserveAcrossLabs) && resolved.Count > 0)
        {
            resolved[0].PreserveAcrossLabs = true;
        }

        return resolved;
    }

    private static List<SelectedVariationMethodExecution> BuildMinimalFallbackMethods(
        IReadOnlyList<VariationMethodDto> availableMethods)
    {
        var prioritized = availableMethods.FirstOrDefault(x =>
            x.Code.Equals("subject_domain", StringComparison.OrdinalIgnoreCase))
            ?? availableMethods.FirstOrDefault();

        if (prioritized is null)
        {
            return new List<SelectedVariationMethodExecution>();
        }

        return new List<SelectedVariationMethodExecution>
        {
            new()
            {
                VariationMethodId = prioritized.Id,
                Code = prioritized.Code,
                PreserveAcrossLabs = true
            }
        };
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

    private static string MethodsSignature(IEnumerable<SelectedVariationMethodExecution> methods)
        => string.Join(
            "|",
            methods
                .OrderBy(x => x.VariationMethodId)
                .Select(x => $"{x.VariationMethodId}:{x.PreserveAcrossLabs}"));

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
}
