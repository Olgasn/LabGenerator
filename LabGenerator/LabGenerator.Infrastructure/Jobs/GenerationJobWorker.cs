using LabGenerator.Domain.Entities;
using LabGenerator.Domain.Enums;
using LabGenerator.Infrastructure.Data;
using LabGenerator.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LabGenerator.Infrastructure.Jobs;

public sealed class GenerationJobWorker(IServiceProvider serviceProvider, ILogger<GenerationJobWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("GenerationJobWorker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = serviceProvider.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var stuckThreshold = DateTimeOffset.UtcNow.AddMinutes(-10);
                var jobsSnapshot = await db.GenerationJobs.ToListAsync(stoppingToken);

                var stuckJobs = jobsSnapshot
                    .Where(x => x.Status == GenerationJobStatus.InProgress
                                && x.StartedAt != null
                                && x.StartedAt < stuckThreshold)
                    .ToList();

                if (stuckJobs.Count > 0)
                {
                    foreach (var j in stuckJobs)
                    {
                        logger.LogWarning(
                            "Recovering stuck job {JobId} type={JobType} startedAt={StartedAt}. Marking as Failed.",
                            j.Id,
                            j.Type,
                            j.StartedAt);
                        j.Status = GenerationJobStatus.Failed;
                        j.Error = "Job was in progress but exceeded stuck threshold; marked as failed to avoid automatic re-processing.";
                        j.FinishedAt = DateTimeOffset.UtcNow;
                    }

                    await db.SaveChangesAsync(stoppingToken);
                }

                var job = jobsSnapshot
                    .Where(x => x.Status == GenerationJobStatus.Pending)
                    .OrderBy(x => x.CreatedAt)
                    .FirstOrDefault();

                if (job is null)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                    continue;
                }

                logger.LogInformation("Picked job {JobId} type={JobType} labId={LabId}", job.Id, job.Type, job.LabId);

                job.Status = GenerationJobStatus.InProgress;
                job.StartedAt = DateTimeOffset.UtcNow;
                job.Progress = 0;
                await db.SaveChangesAsync(stoppingToken);

                try
                {
                    JobExecutionContext.CurrentJobId = job.Id;
                    await ProcessJobAsync(scope.ServiceProvider, job, stoppingToken);

                    job.Status = GenerationJobStatus.Succeeded;
                    job.Progress = 100;
                    job.FinishedAt = DateTimeOffset.UtcNow;
                    await db.SaveChangesAsync(stoppingToken);

                    logger.LogInformation("Job {JobId} succeeded", job.Id);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Job {JobId} failed", job.Id);
                    job.Status = GenerationJobStatus.Failed;
                    job.Error = ex.Message;
                    job.FinishedAt = DateTimeOffset.UtcNow;
                    await db.SaveChangesAsync(stoppingToken);
                }
                finally
                {
                    JobExecutionContext.CurrentJobId = null;
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Job worker iteration failed");
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }
    }

    private static async Task ProcessJobAsync(IServiceProvider scopedProvider, GenerationJob job, CancellationToken ct)
    {
        var db = scopedProvider.GetRequiredService<ApplicationDbContext>();

        switch (job.Type)
        {
            case GenerationJobType.GenerateMasterAssignment:
            {
                if (job.LabId is null)
                {
                    throw new InvalidOperationException("GenerateMasterAssignment job requires LabId");
                }

                var masterService = scopedProvider.GetRequiredService<IMasterAssignmentService>();

                var master = await masterService.GenerateDraftAsync(job.LabId.Value, ct);

                job.MasterAssignmentId = master.Id;
                job.ResultJson = $"{{\"masterAssignmentId\":{master.Id}}}";

                break;
            }

            case GenerationJobType.GenerateVariants:
            {
                if (job.LabId is null)
                {
                    throw new InvalidOperationException("GenerateVariants job requires LabId");
                }

                var payload = TryParsePayload(job.PayloadJson);
                var count = payload.Count ?? 10;

                var variantsService = scopedProvider.GetRequiredService<IVariantGenerationService>();
                var variants = await variantsService.GenerateVariantsAsync(job.LabId.Value, count, payload.VariationProfileId, ct);

                job.ResultJson = JsonSerializer.Serialize(new
                {
                    generated = variants.Count,
                    variantIds = variants.Select(v => v.Id).ToArray()
                });

                break;
            }

            case GenerationJobType.GenerateTheory:
            {
                if (job.LabId is null)
                {
                    throw new InvalidOperationException("GenerateTheory job requires LabId");
                }

                var payload = TryParsePayload(job.PayloadJson);
                var supplementaryMaterialService = scopedProvider.GetRequiredService<ILabSupplementaryMaterialService>();
                var material = await supplementaryMaterialService.GenerateAsync(job.LabId.Value, payload.Force ?? false, ct);

                job.ResultJson = JsonSerializer.Serialize(new
                {
                    supplementaryMaterialId = material.Id,
                    updatedAt = material.UpdatedAt ?? material.CreatedAt
                });

                break;
            }

            case GenerationJobType.VerifyVariants:
            {
                var verificationService = scopedProvider.GetRequiredService<IVerificationService>();
                var payload = TryParsePayload(job.PayloadJson);

                if (payload.VariantId is not null)
                {
                    var report = await verificationService.VerifyVariantAsync(payload.VariantId.Value, ct);
                    job.ResultJson = JsonSerializer.Serialize(new { verified = 1, passed = report.Passed });
                    break;
                }

                if (job.LabId is null)
                {
                    throw new InvalidOperationException("VerifyVariants job requires LabId or VariantId in payload");
                }

                var variantIds = await db.AssignmentVariants.AsNoTracking()
                    .Where(x => x.LabId == job.LabId.Value)
                    .OrderBy(x => x.VariantIndex)
                    .Select(x => x.Id)
                    .ToListAsync(ct);

                var passedCount = 0;
                for (var i = 0; i < variantIds.Count; i++)
                {
                    var report = await verificationService.VerifyVariantAsync(variantIds[i], ct);
                    if (report.Passed) passedCount++;

                    job.Progress = variantIds.Count == 0 ? 100 : (int)Math.Round(((i + 1) * 100.0) / variantIds.Count);
                    await db.SaveChangesAsync(ct);
                }

                job.ResultJson = JsonSerializer.Serialize(new { verified = variantIds.Count, passed = passedCount });
                break;
            }

            default:
                throw new NotSupportedException($"Job type {job.Type} is not supported yet.");
        }

        await db.SaveChangesAsync(ct);
    }

    private static JobPayload TryParsePayload(string? payloadJson)
    {
        if (string.IsNullOrWhiteSpace(payloadJson))
        {
            return new JobPayload(null, null, null, null);
        }

        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<JobPayload>(payloadJson, options) ?? new JobPayload(null, null, null, null);
        }
        catch
        {
            return new JobPayload(null, null, null, null);
        }
    }

    private sealed record JobPayload(int? Count, int? VariationProfileId, int? VariantId, bool? Force);
}
