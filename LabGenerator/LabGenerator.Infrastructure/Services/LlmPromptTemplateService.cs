using System.Text.RegularExpressions;

namespace LabGenerator.Infrastructure.Services;

public sealed partial class LlmPromptTemplateService
{
    private static readonly IReadOnlyDictionary<string, PromptTemplateDefinition> Definitions =
        BuildDefinitions();

    public RenderedLlmPrompt Render(
        string purpose,
        IReadOnlyDictionary<string, string?> variables)
    {
        var def = GetDefinition(purpose);

        return new RenderedLlmPrompt(
            def.Purpose,
            RenderTemplate(def.SystemPromptTemplate, variables),
            RenderTemplate(def.UserPromptTemplate, variables),
            string.IsNullOrWhiteSpace(def.RetryUserPromptSuffixTemplate)
                ? null
                : RenderTemplate(def.RetryUserPromptSuffixTemplate, variables));
    }

    public string? GetRetryUserPromptSuffix(string purpose)
    {
        var def = GetDefinition(purpose);
        return def.RetryUserPromptSuffixTemplate;
    }

    private static PromptTemplateDefinition GetDefinition(string purpose)
    {
        if (string.IsNullOrWhiteSpace(purpose))
            throw new InvalidOperationException("Purpose is required.");

        if (Definitions.TryGetValue(purpose.Trim(), out var definition))
            return definition;

        throw new InvalidOperationException($"Unknown prompt purpose '{purpose}'.");
    }

    private static string RenderTemplate(string template, IReadOnlyDictionary<string, string?> variables)
    {
        return PlaceholderRegex().Replace(
            template,
            match => variables.TryGetValue(match.Groups[1].Value.Trim(), out var value)
                ? value ?? string.Empty
                : match.Value);
    }

    private static IReadOnlyDictionary<string, PromptTemplateDefinition> BuildDefinitions()
    {
        var items = new[]
        {
            new PromptTemplateDefinition(
                Purpose: "master_assignment",
                SystemPromptTemplate: "You are a senior university instructor. Follow the output format strictly.",
                UserPromptTemplate: """
You will be given a short lab description. Generate an expanded MASTER ASSIGNMENT for students in Russian.

{{master_requirements_block}}

STRICT PROHIBITIONS — do NOT include:
- A list or table of individual variants (e.g. "Вариант 1 / Вариант 2 / Вариант 3")
- Any section titled "Варианты заданий", "Индивидуальные варианты", "Варианты для студентов" or similar
- Specific variant-level choices that pre-assign different subjects, algorithms or tech stacks to numbered variants
- Any section titled "Критерии оценки", "Критерии оценивания", "Evaluation criteria", "Шкала оценки" or similar — grading rubrics are managed separately and must NOT appear in the assignment
The master assignment must describe the GENERAL task framework only. Individual variants will be generated separately by the system.

Short lab description:
{{lab_initial_description}}

Output format: Markdown, with headings.
""",
                RetryUserPromptSuffixTemplate: null),
            new PromptTemplateDefinition(
                Purpose: "variant_generation",
                SystemPromptTemplate: """
Ты — генератор учебных заданий. Формат ответа — только JSON.
Язык: русский. Один JSON-объект без обёрток и комментариев.
""",
                UserPromptTemplate: """
Ответь ОДНИМ JSON-объектом. Никакого текста до или после.
{
  "title": "краткое название варианта, 5-15 слов",
  "content_markdown": "полный текст задания в Markdown, 1500-3000 символов",
  "variant_params": { "ключевые параметры варианта" },
  "difficulty_profile": { "complexity": "low|medium|high", "estimated_hours": number }
}

НЕ ВКЛЮЧАЙ в задание раздел "Критерии оценки", "Критерии оценивания", "Шкала оценки" и т.п. — критерии оценивания формируются отдельно.

--- МАСТЕР-ЗАДАНИЕ ---
{{master_assignment_excerpt}}
--- КОНЕЦ ---

{{variation_profile_block}}
{{difficulty_requirements_block}}
Номер варианта: {{variant_index}}

{{variation_constraints_block}}
{{existing_variants_block}}
{{test_plan_warning_block}}
{{rejection_reason_block}}
""",
                RetryUserPromptSuffixTemplate: "[Предыдущий ответ невалиден. Ответь ТОЛЬКО JSON.]"),
            new PromptTemplateDefinition(
                Purpose: "constraints_check",
                SystemPromptTemplate: """
Ты — эксперт по оценке учебных заданий.
Отвечай только JSON.
""",
                UserPromptTemplate: """
Ответь ОДНИМ JSON-объектом.
{
  "is_unique": true/false,
  "most_similar_to": "название наиболее похожего варианта или null",
  "similarity_reason": "краткое пояснение если не уникален или null",
  "difficulty_consistent": true/false,
  "difficulty_reason": "пояснение если реальная трудоёмкость задания не соответствует ожидаемой или null"
}

Выполни проверки НОВОГО ВАРИАНТА:

1. УНИКАЛЬНОСТЬ: Определи, является ли НОВЫЙ ВАРИАНТ уникальным по сравнению с существующими.
Вариант уникален, если НЕ совпадает использованный набор значений варьируемых параметров.
Если хотя бы один из существующих значений варьируемых параметров отличается, то вариант уникален, даже если по смыслу он похож на существующий.
2. СЛОЖНОСТЬ: оцени, соответствует ли реальная трудоёмкость задания по его СОДЕРЖАНИЮ ожидаемому уровню {{difficulty_target_summary}}.
difficulty_consistent=false, если объём и сложность задания явно выходят за пределы ожидаемого диапазона.

НОВЫЙ ВАРИАНТ:
Название: {{candidate_title}}
Содержание: {{candidate_content_excerpt}}

СУЩЕСТВУЮЩИЕ ВАРИАНТЫ:
{{existing_variants_summary}}
""",
                RetryUserPromptSuffixTemplate: null),
            new PromptTemplateDefinition(
                Purpose: "supplementary_material",
                SystemPromptTemplate: """
Ты — методист вуза и преподаватель ИТ-дисциплин.
Подготавливай краткие, точные и практико-ориентированные учебные материалы на русском языке.
""",
                UserPromptTemplate: """
Ответь ОДНИМ JSON-объектом без пояснений и markdown-оберток.
{
  "theory_markdown": "Markdown с теоретическими сведениями для лабораторной работы",
  "control_questions": ["вопрос 1", "вопрос 2", "вопрос 3"]
}

Подготовь два блока для лабораторной работы:
1. Теоретические сведения перед вариантами задания.
2. Контрольные вопросы после вариантов.

{{material_requirements_block}}

Лабораторная работа: {{lab_title}}
Краткое описание: {{lab_initial_description}}

--- МАСТЕР-ЗАДАНИЕ ---
{{master_assignment_excerpt}}
--- КОНЕЦ МАСТЕР-ЗАДАНИЯ ---

Сгенерировано вариантов: {{variants_count}}.
Для контекста используй только компактную сводку по вариантам, не привязывай теорию к одному частному варианту.

--- КОМПАКТНАЯ СВОДКА ПО ВАРИАНТАМ ---
{{variants_summary}}
--- КОНЕЦ СВОДКИ ---

Важно: теория должна быть общей для лабораторной работы, а не для одного частного варианта.
Не пересказывай варианты. Опирайся прежде всего на мастер-задание и краткое описание.
""",
                RetryUserPromptSuffixTemplate: null),
            new PromptTemplateDefinition(
                Purpose: "variant_judge",
                SystemPromptTemplate: "Ты — строгий университетский QA-рецензент. Отвечай только валидным JSON без какого-либо текста.",
                UserPromptTemplate: """
Проверь ВАРИАНТ относительно МАСТЕР-ЗАДАНИЯ.
Ответь ТОЛЬКО одним JSON-объектом (без markdown и без текста до/после JSON):
{
  "passed": boolean,
  "score": { "overall": integer },
  "issues": [
    { "code": string, "message": string, "severity": "low"|"medium"|"high" }
  ]
}

ЖЕСТКИЕ ПРАВИЛА:
- Никаких рассуждений. Никаких пояснений. Только JSON.
- Все поля должны присутствовать.
- score.overall: целое 0..10.
- issues: массив (может быть пустым).
- message: всегда русский, <= 200 символов.
- code: UPPER_SNAKE_CASE.
- severity: low|medium|high.
- Если passed=true, severity может быть только low.

МАСТЕР (сокращённо):
{{master_assignment_markdown}}

ВАРИАНТ (сокращённо):
{{variant_markdown}}
""",
                RetryUserPromptSuffixTemplate: "ВАЖНО: Ответь СРАЗУ одним JSON-объектом. Если не уверен — поставь passed=false и добавь 1 issue с code=LLM_OUTPUT_INVALID."),
            new PromptTemplateDefinition(
                Purpose: "variant_repair",
                SystemPromptTemplate: "You are a senior instructor. Fix the variant according to issues. Output only Markdown.",
                UserPromptTemplate: """
Fix the VARIANT assignment according to the issues.

MASTER ASSIGNMENT (Markdown):
{{master_assignment_markdown}}

ISSUES (JSON):
{{issues_json}}

CURRENT VARIANT (Markdown):
{{variant_markdown}}

Output: corrected VARIANT in Markdown only.
""",
                RetryUserPromptSuffixTemplate: null)
        };

        return items.ToDictionary(x => x.Purpose, StringComparer.OrdinalIgnoreCase);
    }

    [GeneratedRegex(@"\{\{\s*([a-zA-Z0-9_]+)\s*\}\}", RegexOptions.CultureInvariant)]
    private static partial Regex PlaceholderRegex();

    private sealed record PromptTemplateDefinition(
        string Purpose,
        string SystemPromptTemplate,
        string UserPromptTemplate,
        string? RetryUserPromptSuffixTemplate);
}

public sealed record RenderedLlmPrompt(
    string Purpose,
    string SystemPrompt,
    string UserPrompt,
    string? RetryUserPromptSuffix);
