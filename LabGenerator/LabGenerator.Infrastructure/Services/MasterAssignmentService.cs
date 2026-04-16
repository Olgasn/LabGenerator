using System.Collections.Generic;
using System.Text.Json;
using LabGenerator.Application.Abstractions;
using LabGenerator.Application.Models;
using LabGenerator.Domain.Entities;
using LabGenerator.Domain.Enums;
using LabGenerator.Infrastructure.Data;
using LabGenerator.Infrastructure.Jobs;
using Microsoft.EntityFrameworkCore;

namespace LabGenerator.Infrastructure.Services;

public sealed class MasterAssignmentService(
    ApplicationDbContext db,
    ILLMClient llmClient,
    LlmPromptTemplateService promptTemplates,
    PromptCustomSectionService promptCustomSections) : IMasterAssignmentService
{
    public async Task<MasterAssignment> GenerateDraftAsync(int labId, CancellationToken cancellationToken)
    {
        var lab = await db.Labs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == labId, cancellationToken);
        if (lab is null)
        {
            throw new InvalidOperationException($"Lab {labId} not found.");
        }

        var current = await db.MasterAssignments
            .Where(x => x.LabId == labId && x.IsCurrent)
            .FirstOrDefaultAsync(cancellationToken);

        var nextVersion = await db.MasterAssignments
            .Where(x => x.LabId == labId)
            .Select(x => (int?)x.Version)
            .MaxAsync(cancellationToken) ?? 0;
        nextVersion += 1;

        var masterRequirements = await promptCustomSections.GetContentAsync(
            PromptCustomSectionService.MasterRequirements, cancellationToken);

        var prompt = promptTemplates.Render(
            "master_assignment",
            new Dictionary<string, string?>
            {
                ["lab_initial_description"] = lab.InitialDescription,
                ["master_requirements_block"] = masterRequirements
            });

        var systemPrompt = prompt.SystemPrompt;
        var userPrompt = prompt.UserPrompt;

        if (current is not null)
        {
            current.IsCurrent = false;
            current.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await JobExecutionContext.AppendLlmPromptAsync(
            db,
            purpose: "master_assignment",
            systemPrompt: systemPrompt,
            userPrompt: userPrompt,
            attempt: 1,
            temperature: 0.2,
            maxOutputTokens: 4096,
            ct: cancellationToken);

        var result = await llmClient.GenerateTextAsync(
            new LLMCompletionRequest(
                Purpose: "master_assignment",
                SystemPrompt: systemPrompt,
                UserPrompt: userPrompt,
                Model: null,
                Temperature: 0.2,
                MaxOutputTokens: 4096
            ),
            cancellationToken);

        var run = new LLMRun
        {
            Provider = result.Provider,
            Model = result.Model,
            Purpose = "master_assignment",
            RequestJson = JsonSerializer.Serialize(new
            {
                labId,
                systemPrompt,
                userPrompt,
                temperature = 0.2,
                maxOutputTokens = 4096
            }),
            ResponseText = result.Text,
            PromptTokens = result.PromptTokens,
            CompletionTokens = result.CompletionTokens,
            TotalTokens = result.TotalTokens,
            LatencyMs = result.LatencyMs,
            Status = "Succeeded",
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.LLMRuns.Add(run);

        var master = new MasterAssignment
        {
            LabId = labId,
            Version = nextVersion,
            IsCurrent = true,
            Status = MasterAssignmentStatus.Draft,
            Content = result.Text,
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.MasterAssignments.Add(master);

        await db.SaveChangesAsync(cancellationToken);

        return master;
    }

    public Task<MasterAssignment?> GetCurrentAsync(int labId, CancellationToken cancellationToken)
    {
        return db.MasterAssignments.AsNoTracking()
            .Where(x => x.LabId == labId && x.IsCurrent)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<MasterAssignment> UpdateContentAsync(int masterAssignmentId, string content, CancellationToken cancellationToken)
    {
        var master = await db.MasterAssignments.FirstOrDefaultAsync(x => x.Id == masterAssignmentId, cancellationToken);
        if (master is null)
        {
            throw new InvalidOperationException($"MasterAssignment {masterAssignmentId} not found.");
        }

        if (master.Status == MasterAssignmentStatus.Approved)
        {
            var current = await db.MasterAssignments
                .Where(x => x.LabId == master.LabId && x.IsCurrent)
                .FirstOrDefaultAsync(cancellationToken);

            if (current is not null)
            {
                current.IsCurrent = false;
                current.UpdatedAt = DateTimeOffset.UtcNow;
            }

            var nextVersion = await db.MasterAssignments
                .Where(x => x.LabId == master.LabId)
                .Select(x => (int?)x.Version)
                .MaxAsync(cancellationToken) ?? 0;
            nextVersion += 1;

            var draft = new MasterAssignment
            {
                LabId = master.LabId,
                Version = nextVersion,
                IsCurrent = true,
                Status = MasterAssignmentStatus.Draft,
                Content = content,
                CreatedAt = DateTimeOffset.UtcNow
            };

            db.MasterAssignments.Add(draft);
            await db.SaveChangesAsync(cancellationToken);
            return draft;
        }

        master.Content = content;
        master.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return master;
    }

    public async Task<MasterAssignment> ApproveAsync(int masterAssignmentId, CancellationToken cancellationToken)
    {
        var master = await db.MasterAssignments.FirstOrDefaultAsync(x => x.Id == masterAssignmentId, cancellationToken);
        if (master is null)
        {
            throw new InvalidOperationException($"MasterAssignment {masterAssignmentId} not found.");
        }

        master.Status = MasterAssignmentStatus.Approved;
        master.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return master;
    }
}
