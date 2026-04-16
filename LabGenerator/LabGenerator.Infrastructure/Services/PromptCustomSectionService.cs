using LabGenerator.Domain.Entities;
using LabGenerator.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LabGenerator.Infrastructure.Services;

public sealed class PromptCustomSectionService(ApplicationDbContext db)
{
    public const string MasterRequirements = "master_requirements";
    public const string MaterialRequirements = "material_requirements";

    private static readonly IReadOnlyDictionary<string, SectionDefault> Defaults = new Dictionary<string, SectionDefault>(StringComparer.OrdinalIgnoreCase)
    {
        [MasterRequirements] = new(
            DisplayName: "Требования к мастер-заданию",
            DefaultContent: """
                Requirements:
                - Clear goal
                - Task statement
                - Input/Output contract
                - Requirements list (what exactly to implement, constraints, edge cases)
                """),

        [MaterialRequirements] = new(
            DisplayName: "Требования к теории и контрольным вопросам",
            DefaultContent: """
                Требования к theory_markdown:
                - русский язык;
                - 8-12 тематических подразделов, каждый с заголовком уровня ## или ###;
                - каждый подраздел должен содержать: чёткое определение понятия или явления, объяснение как это работает и зачем нужно, а также конкретный практический пример (SQL-код, команду или вывод — оформляй в блоках ```);
                - ключевые термины выделяй жирным (**термин**) при первом упоминании и давай краткое определение прямо в тексте;
                - используй сравнения и аналогии там, где они помогают понять концепцию;
                - структура должна соответствовать порядку изложения в мастер-задании: сначала базовые понятия, затем более сложные;
                - теория должна быть самодостаточной: студент, читая её, должен понять всё необходимое для выполнения любого варианта работы;
                - без воды и без повторения текста вариантов задания;
                - объём: не менее 600 слов.

                Требования к control_questions:
                - 8-12 вопросов;
                - вопросы должны проверять понимание теории и умение обосновать решения;
                - сочетай базовые, аналитические и прикладные вопросы;
                - не дублируй вопросы.
                """)
    };

    public IReadOnlyList<PromptCustomSectionInfo> GetAllSections()
    {
        return Defaults.Select(kv => new PromptCustomSectionInfo(kv.Key, kv.Value.DisplayName)).ToList();
    }

    public async Task<PromptCustomSectionDetails> GetAsync(string sectionKey, CancellationToken ct)
    {
        var key = Normalize(sectionKey);
        var def = GetDefault(key);

        var entity = await db.PromptCustomSections.AsNoTracking()
            .FirstOrDefaultAsync(x => x.SectionKey == key, ct);

        return new PromptCustomSectionDetails(
            SectionKey: key,
            DisplayName: def.DisplayName,
            Content: entity?.Content ?? Unindent(def.DefaultContent),
            DefaultContent: Unindent(def.DefaultContent),
            IsCustomized: entity is not null,
            UpdatedAt: entity?.UpdatedAt);
    }

    public async Task<IReadOnlyList<PromptCustomSectionDetails>> GetAllAsync(CancellationToken ct)
    {
        var entities = await db.PromptCustomSections.AsNoTracking().ToListAsync(ct);
        var map = entities.ToDictionary(x => x.SectionKey, StringComparer.OrdinalIgnoreCase);

        return Defaults.Select(kv =>
        {
            map.TryGetValue(kv.Key, out var entity);
            return new PromptCustomSectionDetails(
                SectionKey: kv.Key,
                DisplayName: kv.Value.DisplayName,
                Content: entity?.Content ?? Unindent(kv.Value.DefaultContent),
                DefaultContent: Unindent(kv.Value.DefaultContent),
                IsCustomized: entity is not null,
                UpdatedAt: entity?.UpdatedAt);
        }).ToList();
    }

    public async Task<PromptCustomSectionDetails> UpdateAsync(string sectionKey, string content, CancellationToken ct)
    {
        var key = Normalize(sectionKey);
        GetDefault(key);

        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidOperationException("Content cannot be empty.");

        var entity = await db.PromptCustomSections
            .FirstOrDefaultAsync(x => x.SectionKey == key, ct);

        if (entity is null)
        {
            entity = new PromptCustomSection { SectionKey = key };
            db.PromptCustomSections.Add(entity);
        }

        entity.Content = content.Trim();
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

        return await GetAsync(key, ct);
    }

    public async Task<PromptCustomSectionDetails> ResetAsync(string sectionKey, CancellationToken ct)
    {
        var key = Normalize(sectionKey);
        GetDefault(key);

        var entity = await db.PromptCustomSections
            .FirstOrDefaultAsync(x => x.SectionKey == key, ct);

        if (entity is not null)
        {
            db.PromptCustomSections.Remove(entity);
            await db.SaveChangesAsync(ct);
        }

        return await GetAsync(key, ct);
    }

    public async Task<string> GetContentAsync(string sectionKey, CancellationToken ct)
    {
        var key = Normalize(sectionKey);
        var def = GetDefault(key);

        var entity = await db.PromptCustomSections.AsNoTracking()
            .FirstOrDefaultAsync(x => x.SectionKey == key, ct);

        return entity?.Content ?? Unindent(def.DefaultContent);
    }

    private static string Normalize(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new InvalidOperationException("SectionKey is required.");
        return key.Trim();
    }

    private static SectionDefault GetDefault(string key)
    {
        if (Defaults.TryGetValue(key, out var def))
            return def;
        throw new InvalidOperationException($"Unknown section key '{key}'.");
    }

    private static string Unindent(string text)
    {
        var lines = text.Split('\n');
        var minIndent = lines
            .Where(l => l.TrimStart().Length > 0)
            .Select(l => l.Length - l.TrimStart().Length)
            .DefaultIfEmpty(0)
            .Min();

        return string.Join('\n', lines.Select(l => l.Length >= minIndent ? l[minIndent..] : l)).Trim();
    }

    private sealed record SectionDefault(string DisplayName, string DefaultContent);
}

public sealed record PromptCustomSectionInfo(string SectionKey, string DisplayName);

public sealed record PromptCustomSectionDetails(
    string SectionKey,
    string DisplayName,
    string Content,
    string DefaultContent,
    bool IsCustomized,
    DateTimeOffset? UpdatedAt);
