# analysis-visualizer

Скрипт для визуализации результатов анализа журналов LabGenerator TeacherEmulator.

## Что делает

Загружает все файлы `analysis.json`, сформированные режимом `analyze-journals`,
и строит четыре графика:

| Файл | Содержание |
|---|---|
| `quality_heatmap.png` | Тепловая карта оценок (0–5) по каждой ЛР каждого теста, цветовая шкала от красного к зелёному |
| `quality_averages.png` | Сгруппированные столбики средних оценок по прогонам анализа |
| `boolean_criteria.png` | Горизонтальная диаграмма доли выполнения булевых критериев (Да/Нет) |
| `score_distribution.png` | Гистограмма распределения всех числовых оценок с линией среднего |

Графики сохраняются в подкаталог `charts/` рядом с входными данными.
В консоль выводится текстовая сводка: средние оценки и доли критериев.

## Требования

- Python 3.10+
- `matplotlib`
- `numpy`

Установка зависимостей:

```powershell
pip install matplotlib numpy
```

## Использование

### Запуск с настройками по умолчанию

Скрипт автоматически ищет данные в:
```
LabGenerator/artifacts/journal-analysis
```

```powershell
cd LabGenerator\tools\analysis-visualizer
python visualize.py
```

### Указать путь к данным явно

```powershell
# Каталог с несколькими прогонами
python visualize.py --input ..\..\LabGenerator.TeacherEmulator\results\journal-analysis

# Конкретный прогон
python visualize.py --input ..\..\artifacts\journal-analysis\run-20260321-164910

# Один файл
python visualize.py --input ..\..\artifacts\journal-analysis\run-20260321-164910\analysis.json
```

### Задать каталог для сохранения графиков

```powershell
python visualize.py --output C:\reports\charts
```

### Показать графики в окне (дополнительно к сохранению)

```powershell
python visualize.py --show
```

### Ограничить число строк тепловой карты

При большом числе тестов тепловая карта может быть слишком высокой.
По умолчанию отображаются первые 60 строк:

```powershell
python visualize.py --max-heatmap-rows=30
```

### Все параметры

```
usage: visualize.py [-h] [--input PATH] [--output DIR] [--show] [--max-heatmap-rows N]

  --input PATH, -i PATH     Путь к каталогу с analysis.json или к самому файлу
  --output DIR, -o DIR      Каталог для сохранения графиков
                            (по умолчанию: <input>/charts/ для каталога,
                            <input parent>/charts/ для файла)
  --show                    Показать графики в интерактивном окне
  --max-heatmap-rows N      Максимальное число строк в тепловой карте (по умолчанию: 60)
```

## Входные данные

Скрипт рекурсивно ищет все файлы `analysis.json` в указанном каталоге.
Каждый файл — это результат одного запуска режима `analyze-journals`.

Структура `analysis.json`:

```
Results[]
  RunId                         — идентификатор тест-кейса ("test-049")
  Discipline
    AssignmentsMatchDiscipline  — задания соответствуют дисциплине (bool)
    LabsDiffer                  — ЛР отличаются друг от друга (bool)
    SequenceLogical             — последовательность ЛР логична (bool)
  Labs[]
    VariantsDiffer              — варианты отличаются (bool)
    VariantsSameDifficulty      — варианты одинаковой сложности (bool)
    MissingGenerationReason     — причина отсутствия вариантов
  Quality[]
    Correctness                 — корректность (0–5)
    Quality                     — качество (0–5)
    Completeness                — полнота (0–5)
    Clarity                     — ясность (0–5)
    Justification               — обоснование оценки
```

## Расположение в проекте

```
LabGenerator/
├── tools/
│   └── analysis-visualizer/
│       ├── visualize.py        ← скрипт
│       └── README.md           ← это описание
└── LabGenerator.TeacherEmulator/
    └── results/
        └── journal-analysis/
            ├── run-20260312-200539/
            │   └── analysis.json
```

Актуальный путь по умолчанию для новых запусков анализатора:

```
LabGenerator/
└── artifacts/
    └── journal-analysis/
        ├── run-20260321-164910/
        │   ├── analysis.json
        │   └── charts/
        │       ├── quality_heatmap.png
        │       ├── quality_averages.png
        │       ├── boolean_criteria.png
        │       └── score_distribution.png
```
