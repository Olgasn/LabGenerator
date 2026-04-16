using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using LabGenerator.Application.Abstractions;
using LabGenerator.Application.Models;
using LabGenerator.Domain.Entities;
using LabGenerator.Domain.Enums;
using LabGenerator.Infrastructure.Data;
using LabGenerator.Infrastructure.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LabGenerator.Infrastructure.Services;

public sealed class VariantGenerationService(
    ApplicationDbContext db,
    ILLMClient llmClient,
    LlmPromptTemplateService promptTemplates,
    ILogger<VariantGenerationService> logger,
    IOptions<DifficultyDefaults> difficultyOptions) : IVariantGenerationService
{
    private readonly DifficultyDefaults _difficultyDefaults = difficultyOptions.Value;

    private static bool TryValidateVariationConstraints(
        List<ConfiguredVariationMethod> configuredMethods,
        IReadOnlyDictionary<int, string> preservedValuesForThisVariant,
        IReadOnlyDictionary<int, HashSet<string>> usedValuesWithinLab,
        string variantParamsJson,
        out string error)
    {
        var map = ParseVariantParamsObject(variantParamsJson);

        foreach (var m in configuredMethods)
        {
            map.TryGetValue(m.MethodCode, out var value);
            value = value?.Trim() ?? string.Empty;

            if (preservedValuesForThisVariant.TryGetValue(m.MethodId, out var fixedVal)
                && !string.IsNullOrWhiteSpace(fixedVal))
            {
                var expected = fixedVal.Trim();
                if (!string.Equals(value, expected, StringComparison.OrdinalIgnoreCase))
                {
                    error = $"Параметр '{m.MethodCode}' должен быть ТОЧНО '{expected}' (Preserve), но получено '{value}'.";
                    return false;
                }

                continue;
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                error = $"Не указан обязательный параметр вариации '{m.MethodCode}' в variant_params.";
                return false;
            }

            if (usedValuesWithinLab.TryGetValue(m.MethodId, out var used)
                && used.Contains(value))
            {
                error = $"Значение '{value}' для параметра '{m.MethodCode}' уже использовано в этой ЛР. Выбери другое.";
                return false;
            }
        }

        error = string.Empty;
        return true;
    }


    public async Task<IReadOnlyList<AssignmentVariant>> GenerateVariantsAsync(
        int labId,
        int count,
        int? variationProfileId,
        CancellationToken cancellationToken)
    {
        var lab = await db.Labs.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == labId, cancellationToken)
            ?? throw new InvalidOperationException($"Lab {labId} not found.");

        var configuredMethods = await GetConfiguredVariationMethodsAsync(labId, cancellationToken);

        var master = await db.MasterAssignments.AsNoTracking()
            .Where(x => x.LabId == labId && x.IsCurrent)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("MasterAssignment is not generated.");

        if (master.Status != MasterAssignmentStatus.Approved)
            throw new InvalidOperationException(
                "MasterAssignment must be approved before generating variants.");

        var profile = variationProfileId is not null
            ? await db.VariationProfiles.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == variationProfileId.Value, cancellationToken)
            : await db.VariationProfiles.AsNoTracking()
                .Where(x => x.LabId == labId && x.IsDefault)
                .FirstOrDefaultAsync(cancellationToken);

        var existingVariants = await db.AssignmentVariants.AsNoTracking()
            .Where(x => x.LabId == labId)
            .OrderBy(x => x.VariantIndex)
            .Select(x => new ExistingVariantInfo(x.Title, x.Content, x.Fingerprint))
            .ToListAsync(cancellationToken);

        var existingFingerprints = existingVariants
            .Select(x => x.Fingerprint)
            .ToHashSet();

        var maxIndex = await db.AssignmentVariants
            .Where(x => x.LabId == labId)
            .Select(x => (int?)x.VariantIndex)
            .MaxAsync(cancellationToken) ?? 0;

        var difficultyTarget = GetDifficultyTarget(profile);

        var created = new List<AssignmentVariant>(count);

        for (var i = 0; i < count; i++)
        {
            var variantIndex = maxIndex + i + 1;

            var variant = await GenerateSingleVariantAsync(
                lab, master, profile,
                variantIndex,
                existingVariants,
                existingFingerprints,
                configuredMethods,
                difficultyTarget,
                cancellationToken);

            created.Add(variant);

            existingVariants.Add(new ExistingVariantInfo(
                variant.Title, variant.Content, variant.Fingerprint));
            existingFingerprints.Add(variant.Fingerprint);
        }

        return created;
    }

    private async Task<AssignmentVariant> GenerateSingleVariantAsync(
        Lab lab,
        MasterAssignment master,
        VariationProfile? profile,
        int variantIndex,
        List<ExistingVariantInfo> existingVariants,
        HashSet<string> existingFingerprints,
        List<ConfiguredVariationMethod> configuredMethods,
        DifficultyDefaults difficultyTarget,
        CancellationToken ct)
    {
        string? rejectionReason = null;

        var prevLabSameVariantValues = await GetPreservedValuesFromPreviousLabAsync(
            lab, configuredMethods, variantIndex, ct);

        var usedValues = await GetUsedValuesWithinLabAsync(
            lab.Id, configuredMethods, ct);

        for (var round = 1; round <= 5; round++)
        {
            var parsed = await GenerateRawVariantAsync(
                master.Content, profile, variantIndex,
                existingVariants,
                configuredMethods,
                prevLabSameVariantValues,
                usedValues,
                difficultyTarget,
                rejectionReason,
                ct);

            if (parsed is null)
            {
                logger.LogWarning(
                    "Variant {Index} round {Round}: generation failed, retrying",
                    variantIndex, round);
                continue;
            }

            if (configuredMethods.Count > 0)
            {
                if (!TryValidateVariationConstraints(
                        configuredMethods,
                        prevLabSameVariantValues,
                        usedValues,
                        parsed.VariantParamsJson,
                        out var variationError))
                {
                    logger.LogWarning(
                        "Variant {Index} round {Round}: variation constraints violated: {Error}. Regenerating.",
                        variantIndex, round, variationError);
                    rejectionReason = variationError;
                    continue;
                }
            }

            // Level 1: структурная проверка difficulty_profile (без LLM)
            if (!TryValidateDifficultyConsistency(parsed.DifficultyProfileJson, difficultyTarget, out var difficultyError))
            {
                logger.LogWarning(
                    "Variant {Index} round {Round}: difficulty mismatch: {Error}. Regenerating.",
                    variantIndex, round, difficultyError);
                rejectionReason = difficultyError;
                continue;
            }

            if (existingVariants.Count > 0)
            {
                // Level 2: LLM-проверка уникальности и согласованности сложности по содержанию
                var constraintsResult = await CheckVariantConstraintsAsync(
                    parsed, existingVariants, difficultyTarget, ct);

                if (!constraintsResult.Parsed)
                {
                    logger.LogWarning(
                        "Variant {Index} round {Round}: constraints check returned invalid JSON. Regenerating.",
                        variantIndex, round);

                    rejectionReason = "Automatic uniqueness and difficulty check returned invalid JSON. " +
                                      "Generate a substantially different variant and preserve machine-readable structure.";
                    continue;
                }

                if (!constraintsResult.IsUnique)
                {
                    logger.LogWarning(
                        "Variant {Index} round {Round}: semantically similar to '{SimilarTo}'. " +
                        "Reason: {Reason}. Regenerating.",
                        variantIndex, round,
                        constraintsResult.MostSimilarTo,
                        constraintsResult.SimilarityReason);

                    rejectionReason = $"Вариант слишком похож на «{constraintsResult.MostSimilarTo}»: " +
                                      $"{constraintsResult.SimilarityReason}. " +
                                      "Измени например предметную область, алгоритм или структуру данных.";
                    continue;
                }

                if (!constraintsResult.DifficultyConsistent)
                {
                    if (round == 1)
                    {
                        logger.LogWarning(
                            "Variant {Index} round {Round}: difficulty inconsistent by content. " +
                            "Reason: {Reason}. Regenerating.",
                            variantIndex, round,
                            constraintsResult.DifficultyReason);

                        rejectionReason = $"Реальная трудоёмкость задания не соответствует ожидаемому уровню " +
                                          $"(complexity={difficultyTarget.Complexity}, " +
                                          $"{difficultyTarget.EstimatedHoursMin}–{difficultyTarget.EstimatedHoursMax} ч): " +
                                          $"{constraintsResult.DifficultyReason}. " +
                                          "Скорректируй объём и сложность задания.";
                        continue;
                    }

                    logger.LogWarning(
                        "Variant {Index} round {Round}: difficulty inconsistent by content (soft warning, accepted). " +
                        "Reason: {Reason}.",
                        variantIndex, round,
                        constraintsResult.DifficultyReason);
                }
            }

            var fingerprint = GenerateFingerprint(
                lab.Id, variantIndex, parsed.ContentMarkdown, existingFingerprints);

            var entity = new AssignmentVariant
            {
                LabId = lab.Id,
                VariantIndex = variantIndex,
                Title = string.IsNullOrWhiteSpace(parsed.Title)
                    ? $"Вариант {variantIndex}"
                    : parsed.Title,
                Content = parsed.ContentMarkdown,
                VariantParamsJson = parsed.VariantParamsJson,
                DifficultyProfileJson = parsed.DifficultyProfileJson,
                Fingerprint = fingerprint,
                CreatedAt = DateTimeOffset.UtcNow
            };

            db.AssignmentVariants.Add(entity);
            await db.SaveChangesAsync(ct);

            logger.LogInformation(
                "Variant {Index} created successfully on round {Round}",
                variantIndex, round);

            return entity;
        }

        throw new InvalidOperationException(
            $"Failed to generate unique variant #{variantIndex} after 5 rounds.");
    }
    

    private async Task<VariantParsed?> GenerateRawVariantAsync(
        string masterContent,
        VariationProfile? profile,
        int variantIndex,
        List<ExistingVariantInfo> existingVariants,
        List<ConfiguredVariationMethod> configuredMethods,
        IReadOnlyDictionary<int, string> preservedValuesForThisVariant,
        IReadOnlyDictionary<int, HashSet<string>> usedValuesWithinLab,
        DifficultyDefaults difficultyTarget,
        string? rejectionReason,
        CancellationToken ct)
    {
        var promptTemplate = promptTemplates.Render(
            "variant_generation",
            BuildGenerationPromptVariables(
                masterContent,
                profile,
                variantIndex,
                existingVariants,
                configuredMethods,
                preservedValuesForThisVariant,
                usedValuesWithinLab,
                difficultyTarget,
                rejectionReason));

        var systemPrompt = promptTemplate.SystemPrompt;
        var userPrompt = promptTemplate.UserPrompt;

        string? lastParseError = null;

        for (var attempt = 1; attempt <= 3; attempt++)
        {
            string prompt;
            if (attempt == 1)
            {
                prompt = userPrompt;
            }
            else if (lastParseError is not null)
            {
                prompt = userPrompt + "\n\n" +
                         $"[Предыдущий ответ не удалось разобрать: {lastParseError}. " +
                         "Ответь СТРОГО одним JSON-объектом с обязательным полем \"content_markdown\". " +
                         "Никакого текста до или после JSON.]";
            }
            else
            {
                prompt = string.IsNullOrWhiteSpace(promptTemplate.RetryUserPromptSuffix)
                    ? userPrompt
                    : userPrompt + "\n\n" + promptTemplate.RetryUserPromptSuffix;
            }

            await JobExecutionContext.AppendLlmPromptAsync(
                db,
                purpose: "variant_generation",
                systemPrompt: systemPrompt,
                userPrompt: prompt,
                attempt: attempt,
                temperature: 0.3 + (attempt - 1) * 0.15,
                maxOutputTokens: 8192,
                ct: ct);

            var llm = await llmClient.GenerateTextAsync(
                new LLMCompletionRequest(
                    Purpose: "variant_generation",
                    SystemPrompt: systemPrompt,
                    UserPrompt: prompt,
                    Model: null,
                    Temperature: 0.3 + (attempt - 1) * 0.15,
                    MaxOutputTokens: 8192
                ), ct);

            SaveLLMRun(llm, "variant_generation", attempt);

            var parsed = TryParseVariantJson(llm.Text);
            if (parsed is not null)
                return parsed;

            lastParseError = DiagnoseJsonFailure(llm.Text);
            logger.LogWarning(
                "Variant generation attempt {Attempt}: JSON parse failed. Diagnosis: {Error}",
                attempt, lastParseError);
        }

        return null;
    }

    private static IReadOnlyDictionary<string, string?> BuildGenerationPromptVariables(
        string masterContent,
        VariationProfile? profile,
        int variantIndex,
        List<ExistingVariantInfo> existingVariants,
        List<ConfiguredVariationMethod> configuredMethods,
        IReadOnlyDictionary<int, string> preservedValuesForThisVariant,
        IReadOnlyDictionary<int, HashSet<string>> usedValuesWithinLab,
        DifficultyDefaults difficultyTarget,
        string? rejectionReason)
    {
        var masterAssignmentExcerpt = masterContent.Length > 3000
            ? masterContent[..3000] + "\n[...обрезано...]"
            : masterContent;

        var variationProfileBlock = string.Empty;
        if (profile is not null)
        {
            variationProfileBlock = $"""
Параметры вариативности:
{profile.ParametersJson}
""";
        }

        var difficultyRequirementsBlock = $"""
Требования к сложности (ОБЯЗАТЕЛЬНО для всех вариантов данной ЛР):
Все варианты должны иметь одинаковый уровень сложности.
  difficulty_profile.complexity = "{difficultyTarget.Complexity}"
  difficulty_profile.estimated_hours = от {difficultyTarget.EstimatedHoursMin} до {difficultyTarget.EstimatedHoursMax}
""";

        var variationConstraintsBlock = string.Empty;
        if (configuredMethods.Count > 0)
        {
            var block = new StringBuilder();
            block.AppendLine("Параметры варьирования (обязательные ограничения):");

            foreach (var m in configuredMethods)
            {
                var label = $"{m.MethodCode} ({m.MethodName})";
                if (!string.IsNullOrWhiteSpace(m.MethodDescription))
                {
                    label += $": {m.MethodDescription.Trim()}";
                }

                if (preservedValuesForThisVariant.TryGetValue(m.MethodId, out var fixedVal)
                    && !string.IsNullOrWhiteSpace(fixedVal))
                {
                    block.AppendLine($"- {label}: ДОЛЖНО БЫТЬ ТОЧНО '{fixedVal}'.");
                    continue;
                }

                if (usedValuesWithinLab.TryGetValue(m.MethodId, out var used)
                    && used.Count > 0)
                {
                    var preview = string.Join(", ", used.Take(50).Select(x => $"'{x}'"));
                    block.AppendLine($"- {label}: выбери значение, которого НЕТ среди уже использованных: {preview}.");
                }
                else
                {
                    block.AppendLine($"- {label}: выбери чёткое конкретное значение и укажи его в variant_params.");
                }
            }

            block.AppendLine();
            block.AppendLine("ВАЖНО: в variant_params обязательно верни значения для каждого параметра варьирования.");
            block.AppendLine("Формат ключей variant_params: используй коды параметров, если они указаны; иначе используй краткие латинские ключи.");
            block.AppendLine();
            block.AppendLine("Обязательные ключи variant_params (строки):");
            foreach (var m in configuredMethods)
            {
                block.AppendLine($"- {m.MethodCode}");
            }

            block.AppendLine();
            block.AppendLine("Пример блока variant_params:");
            block.AppendLine("{");
            for (var i = 0; i < configuredMethods.Count; i++)
            {
                var m = configuredMethods[i];
                var comma = i == configuredMethods.Count - 1 ? string.Empty : ",";
                block.AppendLine($"  \"{m.MethodCode}\": \"<значение>\"{comma}");
            }
            block.AppendLine("}");
            variationConstraintsBlock = block.ToString().TrimEnd();
        }

        var existingVariantsBlock = string.Empty;
        if (existingVariants.Count > 0)
        {
            var block = new StringBuilder();
            block.AppendLine("Существующие варианты:");
            foreach (var ev in existingVariants.TakeLast(15))
            {
                var preview = ev.Content.Length > 150
                    ? ev.Content[..150] + "..."
                    : ev.Content;
                preview = preview.Replace("\r", "").Replace("\n", " ");
                block.AppendLine($"  • {ev.Title}: {preview}");
            }
            existingVariantsBlock = block.ToString().TrimEnd();
        }

        var testPlanWarningBlock = string.Empty;
        if (existingVariants.Any(ev =>
                ContainsTestPlanKeywords(ev.Title) ||
                ContainsTestPlanKeywords(ev.Content)))
        {
            testPlanWarningBlock = "Новый вариант ДОЛЖЕН быть с отличающимся набором значений варьируемых параметров.";
        }

        var rejectionReasonBlock = string.Empty;
        if (!string.IsNullOrWhiteSpace(rejectionReason))
        {
            rejectionReasonBlock = $"""
ВАЖНО: предыдущий вариант отклонён. Причина: {rejectionReason}
Сгенерируй СУЩЕСТВЕННО другой вариант.
""";
        }

        return new Dictionary<string, string?>
        {
            ["master_assignment_excerpt"] = masterAssignmentExcerpt,
            ["variation_profile_block"] = variationProfileBlock,
            ["difficulty_requirements_block"] = difficultyRequirementsBlock,
            ["variant_index"] = variantIndex.ToString(),
            ["variation_constraints_block"] = variationConstraintsBlock,
            ["existing_variants_block"] = existingVariantsBlock,
            ["test_plan_warning_block"] = testPlanWarningBlock,
            ["rejection_reason_block"] = rejectionReasonBlock
        };
    }

    private static bool ContainsTestPlanKeywords(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var lowered = value.ToLowerInvariant();
        return lowered.Contains("тестов", StringComparison.Ordinal) ||
               lowered.Contains("тестир", StringComparison.Ordinal) ||
               lowered.Contains("test plan", StringComparison.Ordinal) ||
               lowered.Contains("testing", StringComparison.Ordinal);
    }


    private async Task<List<ConfiguredVariationMethod>> GetConfiguredVariationMethodsAsync(int labId, CancellationToken ct)
    {
        return await db.LabVariationMethods.AsNoTracking()
            .Include(x => x.VariationMethod)
            .Where(x => x.LabId == labId)
            .OrderBy(x => x.Id)
            .Select(x => new ConfiguredVariationMethod(
                x.VariationMethodId,
                x.VariationMethod.Code,
                x.VariationMethod.Name,
                x.VariationMethod.Description,
                x.PreserveAcrossLabs))
            .ToListAsync(ct);
    }

    private async Task<IReadOnlyDictionary<int, string>> GetPreservedValuesFromPreviousLabAsync(
        Lab lab,
        List<ConfiguredVariationMethod> configuredMethods,
        int variantIndex,
        CancellationToken ct)
    {
        var preserving = configuredMethods.Where(x => x.PreserveAcrossLabs).ToList();
        if (preserving.Count == 0) return new Dictionary<int, string>();

        var prevLabId = await db.Labs.AsNoTracking()
            .Where(x => x.DisciplineId == lab.DisciplineId && x.OrderIndex < lab.OrderIndex)
            .OrderByDescending(x => x.OrderIndex)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync(ct);

        if (prevLabId is null) return new Dictionary<int, string>();

        var prevVariantId = await db.AssignmentVariants.AsNoTracking()
            .Where(x => x.LabId == prevLabId.Value && x.VariantIndex == variantIndex)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync(ct);

        if (prevVariantId is null) return new Dictionary<int, string>();

        var prevParamsJson = await db.AssignmentVariants.AsNoTracking()
            .Where(x => x.Id == prevVariantId.Value)
            .Select(x => x.VariantParamsJson)
            .FirstOrDefaultAsync(ct);

        var map = ParseVariantParamsObject(prevParamsJson);
        var result = new Dictionary<int, string>();
        foreach (var m in preserving)
        {
            if (map.TryGetValue(m.MethodCode, out var v) && !string.IsNullOrWhiteSpace(v))
                result[m.MethodId] = v;
        }

        return result;
    }

    private async Task<IReadOnlyDictionary<int, HashSet<string>>> GetUsedValuesWithinLabAsync(
        int labId,
        List<ConfiguredVariationMethod> configuredMethods,
        CancellationToken ct)
    {
        if (configuredMethods.Count == 0)
            return new Dictionary<int, HashSet<string>>();

        var variantsParams = await db.AssignmentVariants.AsNoTracking()
            .Where(x => x.LabId == labId)
            .Select(x => x.VariantParamsJson)
            .ToListAsync(ct);

        if (variantsParams.Count == 0)
            return new Dictionary<int, HashSet<string>>();

        var byCode = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var json in variantsParams)
        {
            var map = ParseVariantParamsObject(json);
            foreach (var kv in map)
            {
                if (string.IsNullOrWhiteSpace(kv.Value)) continue;
                if (!byCode.TryGetValue(kv.Key, out var set))
                {
                    set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    byCode[kv.Key] = set;
                }
                set.Add(kv.Value.Trim());
            }
        }

        var dict = new Dictionary<int, HashSet<string>>();
        foreach (var m in configuredMethods)
        {
            if (byCode.TryGetValue(m.MethodCode, out var set) && set.Count > 0)
                dict[m.MethodId] = set;
        }

        return dict;
    }

    private static Dictionary<string, string> ParseVariantParamsObject(string? variantParamsJson)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(variantParamsJson))
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            using var doc = JsonDocument.Parse(variantParamsJson);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            return doc.RootElement.EnumerateObject()
                .Where(p => p.Value.ValueKind == JsonValueKind.String)
                .ToDictionary(
                    p => p.Name,
                    p => (p.Value.GetString() ?? string.Empty).Trim(),
                    StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }
    

    private async Task<ConstraintsCheckResult> CheckVariantConstraintsAsync(
        VariantParsed candidate,
        List<ExistingVariantInfo> existingVariants,
        DifficultyDefaults difficultyTarget,
        CancellationToken ct)
    {
        var existingSummary = new StringBuilder();
        foreach (var ev in existingVariants.TakeLast(20))
        {
            var preview = ev.Content.Length > 2000
                ? ev.Content[..2000] + "..."
                : ev.Content;
            preview = preview.Replace("\r", "").Replace("\n", " ");
            existingSummary.AppendLine($"• «{ev.Title}»: {preview}");
        }

        var promptTemplate = promptTemplates.Render(
            "constraints_check",
            new Dictionary<string, string?>
            {
                ["difficulty_target_summary"] =
                    $"complexity=\"{difficultyTarget.Complexity}\", estimated_hours={difficultyTarget.EstimatedHoursMin}–{difficultyTarget.EstimatedHoursMax}",
                ["candidate_title"] = candidate.Title,
                ["candidate_content_excerpt"] = candidate.ContentMarkdown[..Math.Min(candidate.ContentMarkdown.Length, 2000)],
                ["existing_variants_summary"] = existingSummary.ToString().TrimEnd()
            });

        var systemPrompt = promptTemplate.SystemPrompt;
        var userPrompt = promptTemplate.UserPrompt;

        await JobExecutionContext.AppendLlmPromptAsync(
            db,
            purpose: "constraints_check",
            systemPrompt: systemPrompt,
            userPrompt: userPrompt,
            attempt: 1,
            temperature: 0.1,
            maxOutputTokens: 768,
            ct: ct);

        var llm = await llmClient.GenerateTextAsync(
            new LLMCompletionRequest(
                Purpose: "constraints_check",
                SystemPrompt: systemPrompt,
                UserPrompt: userPrompt,
                Model: null,
                Temperature: 0.1,
                MaxOutputTokens: 768
            ), ct);

        SaveLLMRun(llm, "constraints_check", 1);

        return ParseConstraintsCheckResult(llm.Text);
    }

    private static ConstraintsCheckResult ParseConstraintsCheckResult(string text)
    {
        try
        {
            var json = ExtractJsonObject(text);
            if (string.IsNullOrWhiteSpace(json))
                return ConstraintsCheckResult.Invalid();
            if (string.IsNullOrWhiteSpace(json))
                return new ConstraintsCheckResult(true, null, null, true, null); // при ошибке — пропускаем

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var isUnique = root.TryGetProperty("is_unique", out var u)
                           && u.ValueKind == JsonValueKind.True;

            var similarTo = root.TryGetProperty("most_similar_to", out var s)
                            && s.ValueKind == JsonValueKind.String
                ? s.GetString()
                : null;

            var similarityReason = root.TryGetProperty("similarity_reason", out var sr)
                                   && sr.ValueKind == JsonValueKind.String
                ? sr.GetString()
                : null;

            // difficulty_consistent: отсутствие поля трактуем как true (backward-compatible)
            var difficultyConsistent = !root.TryGetProperty("difficulty_consistent", out var dc)
                                       || dc.ValueKind != JsonValueKind.False;

            var difficultyReason = root.TryGetProperty("difficulty_reason", out var dr)
                                   && dr.ValueKind == JsonValueKind.String
                ? dr.GetString()
                : null;

            return new ConstraintsCheckResult(true, isUnique, similarTo, similarityReason, difficultyConsistent, difficultyReason);
        }
        catch
        {
            return ConstraintsCheckResult.Invalid();
        }
    }
    

    private static string GenerateFingerprint(
        int labId, int variantIndex, string content,
        HashSet<string> existing)
    {
        var hash = ComputeSha256Short(content);
        var fp = $"lab{labId}-v{variantIndex}-{hash}";

        if (!existing.Contains(fp)) return fp;

        for (var i = 1; i < 100; i++)
        {
            var candidate = $"{fp}-{i}";
            if (!existing.Contains(candidate)) return candidate;
        }

        return $"lab{labId}-v{variantIndex}-{Guid.NewGuid():N}"[..60];
    }

    private static string ComputeSha256Short(string input)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash)[..12].ToLowerInvariant();
    }
    

    private void SaveLLMRun(LLMCompletionResult llm, string purpose, int attempt)
    {
        db.LLMRuns.Add(new LLMRun
        {
            Provider = llm.Provider,
            Model = llm.Model,
            Purpose = purpose,
            RequestJson = JsonSerializer.Serialize(new { attempt }),
            ResponseText = llm.Text,
            PromptTokens = llm.PromptTokens,
            CompletionTokens = llm.CompletionTokens,
            TotalTokens = llm.TotalTokens,
            LatencyMs = llm.LatencyMs,
            Status = "Succeeded",
            CreatedAt = DateTimeOffset.UtcNow
        });
    }

    private static VariantParsed? TryParseVariantJson(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;

        // First attempt: parse as-is
        var json = ExtractJsonObject(text, requiredProperty: "content_markdown");
        if (!string.IsNullOrWhiteSpace(json))
        {
            var result = ParseVariantFromJson(json);
            if (result is not null) return result;
        }

        // Second attempt: repair common LLM JSON issues and retry
        var repaired = RepairLlmJson(text);
        json = ExtractJsonObject(repaired, requiredProperty: "content_markdown");
        if (!string.IsNullOrWhiteSpace(json))
        {
            var result = ParseVariantFromJson(json);
            if (result is not null) return result;
        }

        return null;
    }

    private static VariantParsed? ParseVariantFromJson(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var content = GetStr(root, "content_markdown");
            if (string.IsNullOrWhiteSpace(content)) return null;

            return new VariantParsed(
                GetStr(root, "title"),
                content,
                root.TryGetProperty("variant_params", out var vp) ? vp.GetRawText() : "{}",
                root.TryGetProperty("difficulty_profile", out var dp) ? dp.GetRawText() : "{}");
        }
        catch { return null; }
    }

    /// <summary>
    /// Repairs common LLM JSON output issues:
    /// unescaped control chars in strings, trailing commas, truncated output.
    /// </summary>
    private static string RepairLlmJson(string text)
    {
        // Step 1: Escape unescaped control characters inside JSON string values
        var sb = new StringBuilder(text.Length + 64);
        var inString = false;
        var escaped = false;

        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];

            if (escaped)
            {
                sb.Append(c);
                escaped = false;
                continue;
            }

            if (c == '\\' && inString)
            {
                sb.Append(c);
                escaped = true;
                continue;
            }

            if (c == '"')
            {
                inString = !inString;
                sb.Append(c);
                continue;
            }

            if (inString)
            {
                switch (c)
                {
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default: sb.Append(c); break;
                }
            }
            else
            {
                sb.Append(c);
            }
        }

        var repaired = sb.ToString();

        // Step 2: Remove trailing commas before } or ]
        repaired = Regex.Replace(repaired, @",\s*([}\]])", "$1");

        // Step 3: Close truncated JSON (unclosed strings, braces, brackets)
        repaired = CloseTruncatedJson(repaired);

        return repaired;
    }

    private static string CloseTruncatedJson(string json)
    {
        var inString = false;
        var escaped = false;
        var stack = new Stack<char>();

        for (var i = 0; i < json.Length; i++)
        {
            var c = json[i];

            if (escaped) { escaped = false; continue; }
            if (c == '\\' && inString) { escaped = true; continue; }
            if (c == '"') { inString = !inString; continue; }

            if (!inString)
            {
                if (c == '{') stack.Push('}');
                else if (c == '[') stack.Push(']');
                else if ((c == '}' || c == ']') && stack.Count > 0 && stack.Peek() == c)
                    stack.Pop();
            }
        }

        if (!inString && stack.Count == 0)
            return json;

        var result = new StringBuilder(json);
        if (inString) result.Append('"');
        while (stack.Count > 0) result.Append(stack.Pop());

        return result.ToString();
    }

    private static string DiagnoseJsonFailure(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText))
            return "пустой ответ от LLM";

        var trimmed = rawText.Trim();

        if (!trimmed.Contains('{'))
            return "ответ не содержит JSON-объект (нет символа '{')";

        // Try to extract any JSON object (without requiring content_markdown)
        var anyJson = ExtractJsonObject(trimmed);
        if (string.IsNullOrWhiteSpace(anyJson))
        {
            // Found { but could not extract valid JSON — get specific parse error
            var braceStart = trimmed.IndexOf('{');
            var braceEnd = trimmed.LastIndexOf('}');
            if (braceStart >= 0 && braceEnd > braceStart)
            {
                var candidate = trimmed[braceStart..(braceEnd + 1)];
                try
                {
                    JsonDocument.Parse(candidate);
                }
                catch (JsonException ex)
                {
                    return $"невалидный JSON: {ex.Message}";
                }
            }

            return "не удалось извлечь валидный JSON-объект из ответа";
        }

        // Valid JSON found but missing content_markdown
        try
        {
            using var doc = JsonDocument.Parse(anyJson);
            if (!doc.RootElement.TryGetProperty("content_markdown", out _))
            {
                var keys = string.Join(", ",
                    doc.RootElement.EnumerateObject().Select(p => p.Name).Take(10));
                return $"JSON валиден, но отсутствует обязательное поле 'content_markdown'. Найденные поля: {keys}";
            }

            var content = GetStr(doc.RootElement, "content_markdown");
            if (string.IsNullOrWhiteSpace(content))
                return "поле 'content_markdown' пустое";
        }
        catch (JsonException ex)
        {
            return $"ошибка парсинга JSON: {ex.Message}";
        }

        return "неизвестная ошибка парсинга";
    }

    private static string GetStr(JsonElement el, string prop)
        => el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String
            ? v.GetString() ?? "" : "";

    private static string ExtractJsonObject(string text, string? requiredProperty = null)
    {
        var t = text.Trim();

        var fs = t.IndexOf("```json", StringComparison.OrdinalIgnoreCase);
        if (fs >= 0)
        {
            var cs = t.IndexOf('\n', fs);
            if (cs >= 0)
            {
                var fe = t.IndexOf("```", cs + 1, StringComparison.Ordinal);
                if (fe > cs)
                {
                    var inner = t[(cs + 1)..fe].Trim();
                    if (IsValidJsonObject(inner, requiredProperty)) return inner;
                }
            }
        }

        var depth = 0;
        var start = -1;
        for (var i = 0; i < t.Length; i++)
        {
            switch (t[i])
            {
                case '{':
                    if (depth == 0) start = i;
                    depth++;
                    break;
                case '}' when depth > 0:
                    depth--;
                    if (depth == 0 && start >= 0)
                    {
                        var c = t[start..(i + 1)];
                        if (IsValidJsonObject(c, requiredProperty)) return c;
                    }
                    break;
            }
        }

        return string.Empty;
    }

    private static bool IsValidJsonObject(string json, string? requiredProperty = null)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
                return false;

            return string.IsNullOrWhiteSpace(requiredProperty)
                   || doc.RootElement.TryGetProperty(requiredProperty, out _);
        }
        catch { return false; }
    }

    /// <summary>
    /// Level 1: структурная проверка difficulty_profile без LLM.
    /// Сравнивает поля complexity и estimated_hours с целевыми значениями.
    /// </summary>
    private static bool TryValidateDifficultyConsistency(
        string difficultyProfileJson,
        DifficultyDefaults target,
        out string error)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(difficultyProfileJson) || difficultyProfileJson == "{}")
            {
                error = $"Отсутствует difficulty_profile. Укажи complexity=\"{target.Complexity}\" " +
                        $"и estimated_hours в диапазоне {target.EstimatedHoursMin}–{target.EstimatedHoursMax}.";
                return false;
            }

            using var doc = JsonDocument.Parse(difficultyProfileJson);
            var root = doc.RootElement;

            var complexity = root.TryGetProperty("complexity", out var c) && c.ValueKind == JsonValueKind.String
                ? c.GetString()?.Trim()
                : null;

            double estimatedHours = -1;
            if (root.TryGetProperty("estimated_hours", out var h) && h.ValueKind == JsonValueKind.Number)
                estimatedHours = h.GetDouble();

            if (!string.Equals(complexity, target.Complexity, StringComparison.OrdinalIgnoreCase))
            {
                error = $"complexity должна быть \"{target.Complexity}\", получено \"{complexity ?? "не указано"}\". " +
                        "Скорректируй difficulty_profile.complexity.";
                return false;
            }

            if (estimatedHours >= 0 &&
                (estimatedHours < target.EstimatedHoursMin || estimatedHours > target.EstimatedHoursMax))
            {
                error = $"estimated_hours={estimatedHours:0.#} выходит за допустимый диапазон " +
                        $"{target.EstimatedHoursMin}–{target.EstimatedHoursMax} ч. " +
                        "Скорректируй объём задания или difficulty_profile.estimated_hours.";
                return false;
            }

            error = string.Empty;
            return true;
        }
        catch
        {
            // При ошибке парсинга не блокируем генерацию
            error = string.Empty;
            return true;
        }
    }

    /// <summary>
    /// Возвращает целевые параметры сложности: из профиля варьирования (если задано)
    /// или из глобальных умолчаний (difficulty_defaults.json).
    /// </summary>
    private DifficultyDefaults GetDifficultyTarget(VariationProfile? profile)
    {
        if (string.IsNullOrWhiteSpace(profile?.DifficultyTargetJson))
            return _difficultyDefaults;

        try
        {
            var overridden = JsonSerializer.Deserialize<DifficultyDefaults>(
                profile.DifficultyTargetJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (overridden is not null &&
                !string.IsNullOrWhiteSpace(overridden.Complexity) &&
                overridden.EstimatedHoursMax >= overridden.EstimatedHoursMin)
            {
                return overridden;
            }
        }
        catch
        {
            // ignore: использовать глобальные умолчания
        }

        return _difficultyDefaults;
    }

    private sealed record VariantParsed(
        string Title, string ContentMarkdown,
        string VariantParamsJson, string DifficultyProfileJson);

    private sealed record ExistingVariantInfo(
        string Title, string Content, string Fingerprint);

    private sealed record ConfiguredVariationMethod(
        int MethodId,
        string MethodCode,
        string MethodName,
        string? MethodDescription,
        bool PreserveAcrossLabs);

    private sealed record ConstraintsCheckResult(
        bool Parsed,
        bool IsUnique,
        string? MostSimilarTo,
        string? SimilarityReason,
        bool DifficultyConsistent,
        string? DifficultyReason)
    {
        public ConstraintsCheckResult(
            bool isUnique,
            string? mostSimilarTo,
            string? similarityReason,
            bool difficultyConsistent,
            string? difficultyReason)
            : this(true, isUnique, mostSimilarTo, similarityReason, difficultyConsistent, difficultyReason)
        {
        }

        public static ConstraintsCheckResult Invalid()
            => new(false, false, null, null, false, null);
    }
}
