namespace LabGenerator.TeacherEmulator;

internal static class LabThemeResolver
{
    private static readonly LabTheme[] Themes =
    [
        new("Постановка задачи и требования", "сбор требований, критерии качества, ограничения"),
        new("Моделирование и спецификация", "формализация предметной области и моделей решения"),
        new("Алгоритмическое решение", "разработка алгоритма и анализ его сложности"),
        new("Проектирование архитектуры", "структура системы, интерфейсы и компоненты"),
        new("Реализация прототипа", "создание рабочего прототипа ключевых модулей"),
        new("Тестирование и отладка", "проектирование тестов и поиск дефектов"),
        new("Оптимизация и масштабирование", "повышение производительности и устойчивости"),
        new("Эксперимент и анализ результатов", "план эксперимента и интерпретация результатов"),
        new("Интеграция и эксплуатация", "встраивание решения и подготовка к запуску"),
        new("Безопасность и надежность", "анализ рисков, меры защиты и устойчивость"),
        new("Работа с данными", "подготовка, хранение и анализ данных"),
        new("Документирование и презентация", "оформление отчета и защита результатов")
    ];

    public static LabPlanItem BuildLabPlan(string disciplineName, int labIndex)
    {
        var theme = Resolve(disciplineName, labIndex);
        var title = $"Лабораторная работа {labIndex}: {theme.Title}";
        var description = $"Лабораторная работа {labIndex} по дисциплине \"{disciplineName}\". " +
                          $"Тематический фокус: {theme.Focus}. " +
                          "Сформулируй уникальное задание, отличающееся от других лабораторных работ этой дисциплины.";

        return new LabPlanItem
        {
            OrderIndex = labIndex,
            Title = title,
            InitialDescription = description
        };
    }

    private static LabTheme Resolve(string disciplineName, int labIndex)
    {
        if (Themes.Length == 0)
        {
            return new LabTheme("Основы дисциплины", "базовые термины и ключевые подходы");
        }

        var offset = Math.Abs(ComputeStableHash(disciplineName)) % Themes.Length;
        var index = (offset + Math.Max(labIndex - 1, 0)) % Themes.Length;
        return Themes[index];
    }

    private static int ComputeStableHash(string value)
    {
        unchecked
        {
            var hash = (int)2166136261;
            const int prime = 16777619;

            if (string.IsNullOrEmpty(value))
            {
                return hash;
            }

            foreach (var ch in value)
            {
                var normalized = char.ToUpperInvariant(ch);
                hash ^= normalized;
                hash *= prime;
            }

            return hash;
        }
    }
}

internal sealed record LabTheme(string Title, string Focus);
