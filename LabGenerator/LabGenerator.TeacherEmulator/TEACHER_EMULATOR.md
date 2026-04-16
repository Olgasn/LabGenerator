# Эмулятор преподавателя (LabGenerator.TeacherEmulator)

## 1. Назначение

`LabGenerator.TeacherEmulator` представляет собой консольное приложение, предназначенное для интеграционного тестирования программного комплекса LabGenerator (LG). Приложение имитирует деятельность преподавателя и воспроизводит полный прикладной цикл подготовки лабораторных заданий.

Эмулятор выполняет следующие действия:
1. Создаёт дисциплину с наименованием и описанием.
2. Создаёт лабораторные работы в заданном количестве.
3. Для каждой лабораторной работы инициирует генерацию мастер-задания.
4. Ожидает завершения фоновой задачи генерации.
5. Анализирует мастер-задание, при необходимости редактирует его и утверждает актуальную версию.
6. Настраивает методы вариативности, включая параметры переноса значений между лабораторными работами (`PreserveAcrossLabs`).
7. Инициирует генерацию вариантов заданий.
8. Запускает верификацию полученных вариантов.
9. Анализирует отчёты верификации и, при необходимости, выполняет повторные попытки генерации и проверки.
10. Формирует подробный журнал выполнения в форматах `JSON` и `Markdown`.

Основной целью эмулятора является сквозная проверка взаимодействия компонентов `WebAPI`, `GenerationJobWorker`, подсистемы LLM и слоя хранения данных.

## 2. Область проверки и ограничения

В рамках интеграционного сценария эмулятор проверяет:
- корректность API-контрактов LabGenerator;
- корректность выполнения фоновых задач (`GenerateMaster`, `GenerateVariants`, `VerifyVariants`);
- устойчивость прикладного процесса к отказам генерации;
- согласованность этапов оркестрации при участии LLM-преподавателя.

Эмулятор не предназначен для проверки:
- графического интерфейса пользовательской части;
- механизмов аутентификации и авторизации;
- детерминированности ответов LLM;
- педагогического качества формулировок как самостоятельной метрики.

## 3. Архитектура приложения

### 3.1. Структура модулей

**Точка входа и оркестрация:**
- `Program.cs` — точка входа. Загружает переменные окружения из файла `.env`, определяет активный режим работы, формирует конфигурацию, создаёт HTTP-клиенты и запускает соответствующий Runner.
- `TeacherEmulatorRunner.cs` — центральный оркестратор сценария эмуляции преподавателя.
- `TestPlanRunner.cs` — оркестратор выполнения тест-кейсов плана испытаний. Управляет циклом создания дисциплины, генерации мастер-заданий, вариантов и верификации.
- `JournalAnalysisRunner.cs` — оркестратор анализа журналов (`journal.json`) через LLM.
- `ModelBenchmarkRunner.cs` — оркестратор сравнения LLM-моделей. Переключает модель на WebAPI, прогоняет тест-план, запускает анализ и формирует сводный отчёт.

**Конфигурация:**
- `TeacherEmulatorOptions.cs` — считывает параметры для режима эмулятора из аргументов командной строки и переменных окружения.
- `TestPlanOptions.cs` — считывает параметры для режима `test-plan`.
- `JournalAnalysisOptions.cs` — считывает параметры для режима `analyze-journals`.
- `ModelBenchmarkOptions.cs` — считывает параметры для режима `model-benchmark`.

**Загрузка и подготовка данных тест-плана:**
- `TestPlanCsvReader.cs` — парсинг CSV-файла плана испытаний. Преобразует строки CSV в структуры `TestPlanCase`.
- `LabThemeResolver.cs` — выбор тематики лабораторной работы на основе имени дисциплины и индекса. Использует стабильное хэширование для детерминированного распределения тем.

**Клиенты внешних сервисов:**
- `LgApiClient.cs` — типизированный клиент REST API LabGenerator.
- `OllamaTeacherClient.cs` — клиент LLM-преподавателя через Ollama-совместимый интерфейс.
- `OllamaAnalysisClient.cs` — клиент LLM для режима анализа журналов.

**Модели данных:**
- `Models.cs` — DTO запросов/ответов API, структуры итоговых отчётов, а также модели плана испытаний (`TestPlanSummary`, `TestPlanCaseResult`, `TestPlanCase`).
- `JournalAnalysisModels.cs` — модели результатов анализа журналов.
- `LlmProvider.cs` — перечисление поддерживаемых LLM-провайдеров (`Ollama`, `OpenRouter`).
- `BenchmarkModelsFile.cs` — модель и парсер файла `models.json` для режима `model-benchmark`.

**Вспомогательные модули:**
- `ReportWriter.cs` — формирует файлы `journal.json` и `journal.md`.
- `CurriculumDisciplineOverrideLoader.cs` — загружает файл учебной программы (`curriculum.md`) для переопределения метаданных дисциплины.

### 3.2. Последовательность выполнения

1. Модуль `Program` считывает конфигурацию и инициализирует зависимости.
2. `CurriculumDisciplineOverrideLoader` ищет файл `curriculum.md` в рабочем каталоге (необязательно).
3. Оркестратор `TeacherEmulatorRunner.RunAsync` запрашивает у LLM-преподавателя план дисциплины и лабораторных работ, опционально используя содержимое учебной программы.
4. Если файл учебной программы найден, имя и описание дисциплины из него переопределяют сгенерированные LLM значения.
5. Эмулятор создаёт дисциплину (`POST /api/disciplines`).
6. Эмулятор получает список методов вариативности (`GET /api/variation-methods`).
7. Для каждой лабораторной работы выполняется полный цикл:
   - создание лабораторной работы (`POST /api/labs`);
   - запуск генерации мастер-задания (`POST /api/labs/{labId}/master/generate`);
   - ожидание завершения фоновой задачи (`GET /api/jobs/{id}`);
   - загрузка текущего мастер-задания (`GET /api/labs/{labId}/master`);
   - экспертная проверка мастер-задания средствами LLM-преподавателя;
   - сохранение правок при необходимости (`PUT /api/labs/{labId}/master/{masterId}`);
   - утверждение версии (`POST /api/labs/{labId}/master/{masterId}/approve`);
   - выбор и применение методов вариативности LLM-преподавателем (`PUT /api/labs/{labId}/variation-methods`);
   - запуск генерации вариантов (`POST /api/labs/{labId}/variants/generate`);
   - ожидание завершения фоновой задачи генерации;
   - запуск верификации (`POST /api/labs/{labId}/verify`);
   - ожидание завершения фоновой задачи верификации;
   - чтение вариантов и отчётов верификации (`GET /api/labs/{labId}/variants`, `GET /api/labs/{labId}/verification-reports`).
8. Модуль `ReportWriter` сохраняет результирующие артефакты.

## 4. Вызовы LLM-преподавателя

### 4.1. Роль LLM-преподавателя

LLM-преподаватель не формирует варианты заданий напрямую. Его функция заключается в принятии содержательных решений, которые затем исполняются средствами LabGenerator.

### 4.2. Этапы обращения к LLM

LLM-преподаватель вызывается только на трёх этапах:
1. **Планирование дисциплины и лабораторных работ** — один вызов на весь запуск.
2. **Экспертная проверка мастер-задания** — один вызов для каждой лабораторной работы.
3. **Выбор методов вариативности** — один вызов для каждой лабораторной работы.

При стандартной конфигурации (`LG_EMULATOR_LAB_COUNT=3`) общее число вызовов составляет семь.

### 4.3. Технический интерфейс обращения к LLM

Поддерживаются два протокола. Протокол выбирается автоматически по хосту в `--ollama-base-url=` или задаётся явно через `--llm-provider=`.

| Параметр | Ollama | OpenRouter (по умолчанию) |
|---|---|---|
| Эндпоинт | `POST /api/generate` | `POST /api/v1/chat/completions` |
| Формат запроса | `prompt`, `options.num_predict` | `messages[]`, `max_tokens` |
| JSON-режим | `"format": "json"` | `"response_format": {"type": "json_object"}` |
| Формат ответа | `{"response": "..."}` | `{"choices": [{"message": {"content": "..."}}]}` |

Авторизация в обоих случаях выполняется через заголовок `Authorization: Bearer <API_KEY>`.

### 4.4. Параметры по умолчанию

| Параметр | Значение по умолчанию |
|---|---|
| Провайдер | `openrouter` (авто по URL; явно через `--llm-provider=`) |
| Базовый адрес | `https://openrouter.ai` |
| Модель преподавателя | `deepseek-v3.2:cloud` |

## 5. Переопределение дисциплины через файл учебной программы

При наличии файла `curriculum.md` (или `curriculum1.md`) в каталоге `curriculums/` рядом с исполняемым файлом или в рабочем каталоге `CurriculumDisciplineOverrideLoader` загружает его и передаёт содержимое LLM-преподавателю.

Поведение при применении файла:
- LLM учитывает темы, цели и разделы учебной программы при формировании плана лабораторных работ.
- Первая строка файла (с префиксом `#`) используется как наименование дисциплины.
- Полное содержимое файла используется как описание дисциплины.
- Эти значения заменяют сгенерированные LLM метаданные.

Если файл учебной программы не найден, LLM самостоятельно генерирует наименование и описание дисциплины на основе `--seed-topic`.

## 6. Анализ отчётов и повторные попытки

После завершения верификации эмулятор запрашивает отчёты по всем вариантам и анализирует их статусы.

Если обнаружены варианты со статусом, отличным от успешного, применяется поэтапная стратегия восстановления:
1. Выполняется повторная верификация проблемных вариантов в пределах `LG_EMULATOR_MAX_VERIFY_RETRIES`.
2. Если результат остаётся неудовлетворительным, выполняется повторная генерация вариантов и затем повторная верификация в пределах `LG_EMULATOR_MAX_REGEN_RETRIES`.

Таким образом, анализ выполняется не посредством дополнительной экспертной оценки LLM-преподавателя, а детерминированно по данным API LabGenerator.

## 7. Конфигурация

### 7.1. Приоритет источников параметров

Параметры считываются в следующем порядке (от высшего к низшему приоритету):
1. Аргументы командной строки (`--key=value`).
2. Переменные окружения (`LG_EMULATOR_*`, `LG_ANALYZER_*`, `LG_TEST_PLAN_*`).
3. Резервные переменные окружения (`LLM__Ollama__*`).
4. Встроенные значения по умолчанию.

При запуске приложение автоматически загружает файл `.env` (если найден) в переменные окружения. Файл ищется в следующих каталогах (используется первый найденный):
1. Рабочий каталог (`cwd`).
2. Каталог исполняемого файла (`AppContext.BaseDirectory`).
3. `LabGenerator/` относительно рабочего каталога.

Файл `.env` содержит пары `KEY=VALUE`, по одной на строку. Пример — в файле `LabGenerator/.env.example`.

### 7.2. Переменные окружения: режим эмулятора

| Переменная | По умолчанию | Диапазон | Описание |
|---|---|---|---|
| `LG_EMULATOR_API_BASE_URL` | `http://localhost:8080` | — | Адрес API LabGenerator |
| `LG_EMULATOR_OLLAMA_BASE_URL` | `https://openrouter.ai` | — | Адрес Ollama-совместимого сервиса |
| `LG_EMULATOR_OLLAMA_API_KEY` | — | — | Ключ доступа к LLM-сервису |
| `LG_EMULATOR_TEACHER_MODEL` | `deepseek-v3.2:cloud` | — | Идентификатор модели преподавателя |
| `LG_EMULATOR_OUTPUT_DIR` | `artifacts/teacher-emulator` | — | Каталог результирующих артефактов |
| `LG_EMULATOR_SEED_TOPIC` | `Applied software engineering` | — | Тематический ориентир для планирования |
| `LG_EMULATOR_LAB_COUNT` | `3` | 1–20 | Количество лабораторных работ |
| `LG_EMULATOR_VARIANT_COUNT` | `6` | 1–100 | Целевое количество вариантов на лабораторную работу |
| `LG_EMULATOR_MAX_VERIFY_RETRIES` | `1` | 0–10 | Количество повторных попыток верификации |
| `LG_EMULATOR_MAX_REGEN_RETRIES` | `1` | 0–10 | Количество повторных попыток генерации |
| `LG_EMULATOR_JOB_TIMEOUT_SECONDS` | `900` | 30–7200 | Предельное время ожидания фоновой задачи (с) |
| `LG_EMULATOR_JOB_POLL_SECONDS` | `2` | 1–30 | Интервал опроса статуса фоновой задачи (с) |
| `LG_EMULATOR_REQUEST_TIMEOUT_SECONDS` | `180` | 10–3600 | Тайм-аут HTTP-запросов (с) |
| `LG_EMULATOR_LLM_PROVIDER` | _(авто)_ | `ollama`, `openrouter` | Протокол обращения к LLM |

Резервные источники для параметров LLM:
- `LLM__Ollama__BaseUrl` → `LG_EMULATOR_OLLAMA_BASE_URL`
- `LLM__Ollama__Model` → `LG_EMULATOR_TEACHER_MODEL`
- `LLM__Ollama__ApiKey` → `LG_EMULATOR_OLLAMA_API_KEY`
- `OPENROUTER_API_KEY` → `LG_EMULATOR_OLLAMA_API_KEY` (резерв при использовании OpenRouter)

### 7.3. Аргументы командной строки: режим эмулятора

Формат: `--key=value`. Соответствие аргументов переменным окружения:

| Аргумент | Переменная окружения |
|---|---|
| `--lg-base-url=` | `LG_EMULATOR_API_BASE_URL` |
| `--ollama-base-url=` | `LG_EMULATOR_OLLAMA_BASE_URL` |
| `--ollama-api-key=` | `LG_EMULATOR_OLLAMA_API_KEY` |
| `--teacher-model=` | `LG_EMULATOR_TEACHER_MODEL` |
| `--output-dir=` | `LG_EMULATOR_OUTPUT_DIR` |
| `--seed-topic=` | `LG_EMULATOR_SEED_TOPIC` |
| `--lab-count=` | `LG_EMULATOR_LAB_COUNT` |
| `--variant-count=` | `LG_EMULATOR_VARIANT_COUNT` |
| `--max-verify-retries=` | `LG_EMULATOR_MAX_VERIFY_RETRIES` |
| `--max-regen-retries=` | `LG_EMULATOR_MAX_REGEN_RETRIES` |
| `--job-timeout-seconds=` | `LG_EMULATOR_JOB_TIMEOUT_SECONDS` |
| `--job-poll-seconds=` | `LG_EMULATOR_JOB_POLL_SECONDS` |
| `--request-timeout-seconds=` | `LG_EMULATOR_REQUEST_TIMEOUT_SECONDS` |
| `--llm-provider=` | `LG_EMULATOR_LLM_PROVIDER` |

### 7.4. Выбор LLM-провайдера

Параметр `--llm-provider=` (или `LG_EMULATOR_LLM_PROVIDER`) принимает значения `ollama` и `openrouter`.

**Автоопределение:** если хост в `--ollama-base-url=` содержит `openrouter.ai`, провайдер определяется как `openrouter` без явного указания. Если провайдер не указан явно и URL не содержит `openrouter.ai`, по умолчанию используется `openrouter`.

**Явное указание** применяется, когда URL нестандартный или автоопределение нежелательно:

```powershell
dotnet run --project .\LabGenerator.TeacherEmulator -- `
  --llm-provider=openrouter `
  --ollama-base-url=https://my-proxy.example.com `
  --teacher-model=deepseek/deepseek-chat `
  --ollama-api-key=<KEY>
```

Идентификатор провайдера записывается в `journal.json` в поле `Options.LlmProvider`. Примеры запуска с каждым провайдером — в разделе 8.

## 8. Запуск

### 8.1. Предварительные условия

Перед запуском эмулятора необходимо обеспечить:
1. Доступность WebAPI и базы данных LabGenerator.
2. Работу фонового обработчика задач `GenerationJobWorker`.
3. Доступность LLM-сервиса и корректного ключа авторизации.

### 8.2. Запуск с провайдером OpenRouter (по умолчанию)

Провайдер: `openrouter`. Базовый URL: `https://openrouter.ai`. Модель: `deepseek-v3.2:cloud`. Актуальный список моделей — на [openrouter.ai/models](https://openrouter.ai/models).

Через файл `.env` (рекомендуемый способ):

```
# LabGenerator/.env
OPENROUTER_API_KEY=sk-or-v1-...
```

```powershell
cd LabGenerator
dotnet run --project .\LabGenerator.TeacherEmulator
```

Через аргумент командной строки:

```powershell
cd LabGenerator
dotnet run --project .\LabGenerator.TeacherEmulator -- --ollama-api-key=<OPENROUTER_KEY>
```

Через переменную окружения:

```powershell
$env:OPENROUTER_API_KEY = "<OPENROUTER_KEY>"
dotnet run --project .\LabGenerator.TeacherEmulator
```

### 8.3. Запуск с провайдером Ollama Cloud

Провайдер указывается явно через `--llm-provider=ollama`.

```powershell
cd LabGenerator
dotnet run --project .\LabGenerator.TeacherEmulator -- `
  --llm-provider=ollama `
  --ollama-base-url=https://ollama.com `
  --ollama-api-key=<OLLAMA_KEY>
```

Через переменные окружения:

```powershell
$env:LG_EMULATOR_LLM_PROVIDER     = "ollama"
$env:LG_EMULATOR_OLLAMA_BASE_URL  = "https://ollama.com"
$env:LG_EMULATOR_OLLAMA_API_KEY   = "<OLLAMA_KEY>"
dotnet run --project .\LabGenerator.TeacherEmulator
```

### 8.4. Коды завершения процесса

- `0` — сценарий завершён успешно.
- `1` — зафиксирована ошибка конфигурации или выполнения.
- `2` — выполнение прервано пользователем.

## 9. Формируемые артефакты

Для каждого запуска создаётся каталог `artifacts/teacher-emulator/run-<yyyyMMdd-HHmmss>/`.

В каталоге формируются файлы:
- `journal.json` — полная машиночитаемая структура результатов.
- `journal.md` — детальный человекочитаемый отчёт.

Отчёты включают:
- описание дисциплины;
- тексты мастер-заданий;
- тексты вариантов заданий;
- параметры вариативности и профиль сложности;
- результаты верификации;
- последовательный журнал событий.

## 10. Устойчивость к отказам

Оркестратор ожидает завершение фоновых задач по статусам `Succeeded`, `Failed`, `Canceled` с учётом установленного тайм-аута.

Если генерация вариантов завершается ошибкой, эмулятор применяет резервные шаги:
1. Повторяет попытку с исходным набором методов вариативности.
2. Повторяет попытку с минимальным набором методов.
3. Повторяет попытку без методов вариативности.

Резервные наборы методов применяются только на время конкретной попытки генерации. После завершения цикла попыток эмулятор восстанавливает исходную матрицу вариативности, выбранную LLM-преподавателем для данной лабораторной работы.

Сценарий завершается ошибкой только после исчерпания всех предусмотренных попыток.

## 11. Требования информационной безопасности

- Не размещайте действующие API-ключи в репозитории.
- Храните `OLLAMA_API_KEY` и `OPENROUTER_API_KEY` в локальном `LabGenerator/.env` (файл включён в `.gitignore`). Образец — `LabGenerator/.env.example`.
- Не публикуйте ключи в отчётах, сообщениях и служебных журналах.
- При компрометации ключа немедленно выполните его отзыв и замену.

## 12. Практические замечания

- Ответы LLM имеют вероятностный характер, поэтому длительность и содержание результатов могут отличаться между запусками.
- После неуспешных попыток генерации возможно временное расхождение между целевым и фактическим числом вариантов.
- Для облачных моделей необходимо учитывать ограничения поставщика по квотам и задержкам обработки.

## 13. Режим test-plan (план испытаний)

Для прогонов интеграционного тестирования по заранее заданному плану предусмотрен режим `test-plan`.

Особенности режима:
- CSV читается построчно, каждый тест-кейс выполняется независимо.
- LLM-преподаватель не используется; выполняются только стандартные LLM-вызовы генерации и верификации внутри LabGenerator.
- Для каждого теста создаётся отдельная дисциплина с именем `<Дисциплина> (Test N)` и генерируются лабораторные работы `1..N`, чтобы корректно проверить `PreserveAcrossLabs`.

### 13.1. Активация режима

Режим активируется при наличии одного из следующих признаков:
- флаг `--test-plan` в аргументах командной строки;
- аргумент `--test-plan-csv=...`;
- переменная `LG_TEST_PLAN_MODE=1` (или `true`);
- переменная `LG_TEST_PLAN_CSV` с непустым значением.

### 13.2. Источник данных

По умолчанию используется файл:
```
LabGenerator/LabGenerator.TeacherEmulator/План испытаний.csv
```

Переопределение пути к CSV:
- аргумент `--test-plan-csv=<путь>`;
- переменная `LG_TEST_PLAN_CSV`.

Поддерживаются абсолютные и относительные пути (относительно рабочего каталога и каталога `LabGenerator/`).

### 13.3. Запуск

```powershell
cd LabGenerator
dotnet run --project .\LabGenerator.TeacherEmulator -- --test-plan
```

С явным путём к CSV:

```powershell
dotnet run --project .\LabGenerator.TeacherEmulator -- --test-plan-csv=.\LabGenerator.TeacherEmulator\План испытаний.csv
```

С явным количеством вариантов:

```powershell
dotnet run --project .\LabGenerator.TeacherEmulator -- --test-plan --variant-count=8
```

### 13.4. Аргументы командной строки

| Аргумент | Переменная окружения | По умолчанию |
|---|---|---|
| `--test-plan` | `LG_TEST_PLAN_MODE` | — |
| `--test-plan-csv=` | `LG_TEST_PLAN_CSV` | `LabGenerator.TeacherEmulator/План испытаний.csv` |
| `--test-plan-output-dir=` | `LG_TEST_PLAN_OUTPUT_DIR` | `artifacts/test-plan` |
| `--variant-count=` | `LG_EMULATOR_VARIANT_COUNT` | `6` |

Параметры подключения к LG API, тайм-ауты и количество вариантов берутся из стандартных переменных `LG_EMULATOR_*` (см. раздел 7.2).

### 13.5. Артефакты

По итогам запуска создаётся каталог:
```
artifacts/test-plan/run-<yyyyMMdd-HHmmss>/
```

Внутри:
- `summary.json` — сводка по всем тест-кейсам с признаком успеха каждого;
- `run-test-XXX/journal.json` и `run-test-XXX/journal.md` — подробные отчёты по каждому тесту.

Каталог можно переопределить через `--test-plan-output-dir=` или `LG_TEST_PLAN_OUTPUT_DIR`.

## 14. Режим analyze-journals (анализ журналов прогонов)

Режим предназначен для автоматизированного анализа файлов `journal.json` в указанном каталоге. Анализ выполняется через LLM и формирует итоговые отчёты в форматах `JSON` и `Markdown`.

### 14.1. Активация режима

Режим активируется при наличии одного из следующих признаков:
- флаг `--analyze-journals` в аргументах командной строки;
- аргумент `--analysis-dir=...`, `--input-dir=...` или `--journals-dir=...`;
- переменная `LG_ANALYZER_MODE=1` (или `true`);
- переменная `LG_ANALYZER_INPUT_DIR` с непустым значением.

**Приоритет режимов:** `analyze-journals` проверяется первым; затем `model-benchmark`; затем `test-plan`; иначе запускается стандартный режим эмулятора.

### 14.2. Запуск с провайдером OpenRouter (по умолчанию)

Провайдер: `openrouter`. Базовый URL: `https://openrouter.ai`. Модель: `deepseek-v3.2:cloud`.

Через файл `.env` (рекомендуемый способ):

```powershell
cd LabGenerator
dotnet run --project .\LabGenerator.TeacherEmulator -- `
  --analyze-journals `
  --analysis-dir=artifacts\test-plan\run-YYYYMMDD-HHMMSS
```

Через аргумент:

```powershell
cd LabGenerator
dotnet run --project .\LabGenerator.TeacherEmulator -- `
  --analyze-journals `
  --analysis-dir=artifacts\test-plan\run-YYYYMMDD-HHMMSS `
  --analysis-ollama-api-key=<OPENROUTER_KEY>
```

### 14.3. Запуск с провайдером Ollama Cloud

Провайдер указывается явно.

```powershell
cd LabGenerator
dotnet run --project .\LabGenerator.TeacherEmulator -- `
  --analyze-journals `
  --analysis-dir=artifacts\test-plan\run-YYYYMMDD-HHMMSS `
  --analysis-llm-provider=ollama `
  --analysis-ollama-base-url=https://ollama.com `
  --analysis-ollama-api-key=<OLLAMA_KEY>
```

### 14.4. Входные данные

В каталоге `--analysis-dir` утилита ищет все файлы `journal.json` рекурсивно. Для каждого `journal.json` при наличии берётся соседний `journal.md`.

Поддерживаются пути:
- абсолютный путь;
- путь относительно рабочего каталога;
- путь относительно родительского каталога;
- путь с префиксом `LabGenerator\...` (префикс автоматически отсекается).

### 14.5. Аргументы командной строки и переменные окружения

| Аргумент | Переменная окружения | По умолчанию |
|---|---|---|
| `--analyze-journals` | `LG_ANALYZER_MODE` | — |
| `--analysis-dir=` | `LG_ANALYZER_INPUT_DIR` | обязателен |
| `--input-dir=` | `LG_ANALYZER_INPUT_DIR` | обязателен |
| `--journals-dir=` | `LG_ANALYZER_INPUT_DIR` | обязателен |
| `--analysis-output-dir=` | `LG_ANALYZER_OUTPUT_DIR` | `artifacts/journal-analysis` |
| `--analysis-ollama-base-url=` | `LG_ANALYZER_OLLAMA_BASE_URL` | `https://openrouter.ai` |
| `--analysis-ollama-model=` | `LG_ANALYZER_OLLAMA_MODEL` | `deepseek-v3.2:cloud` |
| `--analysis-ollama-api-key=` | `OLLAMA_API_KEY` | — |
| `--analysis-request-timeout-seconds=` | `LG_ANALYZER_REQUEST_TIMEOUT_SECONDS` | `600` (10–3600) |
| `--analysis-criteria-path=` | `LG_ANALYZER_CRITERIA_PATH` | `Критерии оценки.txt` |
| `--analysis-llm-provider=` | `LG_ANALYZER_LLM_PROVIDER` | _(авто)_ |

Резервные источники для параметров LLM:
- `LLM__Ollama__BaseUrl` → `--analysis-ollama-base-url`
- `LLM__Ollama__Model` → `--analysis-ollama-model`
- `LLM__Ollama__ApiKey` → `--analysis-ollama-api-key`
- `LG_EMULATOR_OLLAMA_API_KEY` → `--analysis-ollama-api-key` (второй резерв)
- `OPENROUTER_API_KEY` → `--analysis-ollama-api-key` (третий резерв, при использовании OpenRouter)

### 14.6. Файл критериев оценки

Файл `Критерии оценки.txt` автоматически ищется в следующих каталогах (в порядке приоритета):
1. Путь, указанный в `--analysis-criteria-path=`.
2. Рабочий каталог.
3. `LabGenerator.TeacherEmulator/` рядом с рабочим каталогом.
4. `LabGenerator/LabGenerator.TeacherEmulator/`.

Если файл не найден, анализ выполняется без дополнительных критериев.

### 14.7. Артефакты

Создаётся каталог:
```
artifacts/journal-analysis/run-<yyyyMMdd-HHmmss>/
```

Внутри:
- `analysis.json` — полная структура анализа по каждому журналу;
- `analysis.md` — таблицы сравнения и качества по всем прогонам.

## 15. Режим model-benchmark (сравнение LLM-моделей)

Режим предназначен для автоматизированного сравнения способности различных LLM генерировать варианты заданий по алгоритмам программного комплекса. Для каждой модели из входного списка выполняется полный прогон тест-плана с последующим анализом качества. Итогом является сводный сравнительный отчёт.

### 15.1. Принцип работы

Для каждой модели из файла `models.json` оркестратор:
1. Переключает LLM-провайдер и модель на WebAPI через `PUT /api/admin/llm-settings` и `PUT /api/admin/llm-provider-settings/{provider}`.
2. Прогоняет весь тест-план CSV (переиспользует `TestPlanRunner`).
3. Запускает анализ журналов (переиспользует `JournalAnalysisRunner`, если не указан `--skip-analysis`).
4. Агрегирует метрики качества из результатов анализа.
5. После завершения всех прогонов восстанавливает исходные настройки LLM.

### 15.2. Активация режима

Режим активируется при наличии одного из следующих признаков:
- флаг `--model-benchmark` в аргументах командной строки;
- аргумент `--models-file=...`;
- переменная `LG_BENCHMARK_MODE=1` (или `true`).

Режим `model-benchmark` также **требует** одновременного указания тест-плана (`--test-plan` или `--test-plan-csv=...`).

**Приоритет режимов:** `analyze-journals` → `model-benchmark` → `test-plan` → стандартный эмулятор.

### 15.3. Входной файл models.json

Файл содержит JSON-массив с описаниями моделей. Пример — `LabGenerator/LabGenerator.TeacherEmulator/models.example.json`.

```json
[
  {
    "name": "deepseek-v3.2",
    "provider": "OpenRouter",
    "model": "deepseek/deepseek-v3.2",
    "apiKeyEnv": "OPENROUTER_API_KEY"
  },
  {
    "name": "claude-sonnet-4",
    "provider": "OpenRouter",
    "model": "anthropic/claude-sonnet-4",
    "apiKeyEnv": "OPENROUTER_API_KEY"
  },
  {
    "name": "cogito-2.1-671b",
    "provider": "Ollama",
    "model": "cogito-2.1:671b-cloud",
    "apiKeyEnv": "OLLAMA_API_KEY"
  }
]
```

Поля записи:

| Поле | Обязательное | Описание |
|------|:---:|---|
| `name` | да | Уникальное отображаемое имя модели (используется в отчёте и как имя каталога артефактов) |
| `provider` | нет | `Ollama` или `OpenRouter` (по умолчанию `OpenRouter`) |
| `model` | да | Идентификатор модели для API |
| `baseUrl` | нет | Базовый URL провайдера (по умолчанию определяется из конфигурации WebAPI) |
| `apiKeyEnv` | нет | Имя переменной окружения, содержащей API-ключ |
| `temperature` | нет | Температура генерации (передаётся в `PUT /api/admin/llm-provider-settings`) |
| `maxOutputTokens` | нет | Максимальное количество выходных токенов |

### 15.4. Запуск

```powershell
cd LabGenerator
dotnet run --project .\LabGenerator.TeacherEmulator -- `
  --model-benchmark `
  --models-file=.\LabGenerator.TeacherEmulator\models.example.json `
  --test-plan-csv=".\LabGenerator.TeacherEmulator\План испытаний.csv" `
  --variant-count=15
```

С пропуском этапа анализа качества:

```powershell
cd LabGenerator
dotnet run --project .\LabGenerator.TeacherEmulator -- `
  --model-benchmark `
  --models-file=models.json `
  --test-plan `
  --variant-count=10 `
  --skip-analysis
```

### 15.5. Аргументы командной строки и переменные окружения

| Аргумент | Переменная окружения | По умолчанию |
|---|---|---|
| `--model-benchmark` | `LG_BENCHMARK_MODE` | — |
| `--models-file=` | `LG_BENCHMARK_MODELS_FILE` | `models.json` |
| `--benchmark-output-dir=` | `LG_BENCHMARK_OUTPUT_DIR` | `artifacts/model-benchmark` |
| `--skip-analysis` | `LG_BENCHMARK_SKIP_ANALYSIS` | — |

Параметры подключения к LG API, тайм-ауты и количество вариантов берутся из стандартных переменных `LG_EMULATOR_*` (см. раздел 7.2). Параметры анализа — из `LG_ANALYZER_*` (см. раздел 14.5) или, при отсутствии, автоматически наследуются от параметров эмулятора.

### 15.6. Артефакты

По итогам запуска создаётся каталог:
```
artifacts/model-benchmark/run-<yyyyMMdd-HHmmss>/
```

Структура:
```
run-<timestamp>/
  benchmark-report.json     — сводка по всем моделям (метрики, статусы, ошибки)
  benchmark-report.md       — сравнительные таблицы для человека
  <model-name>/             — каталог тест-плана конкретной модели
    run-<timestamp>/
      summary.json
      run-test-XXX/journal.json
      run-test-XXX/journal.md
  analysis/
    <model-name>/           — результаты анализа конкретной модели
      analysis.json
      analysis.md
```

### 15.7. Сводный отчёт

Файл `benchmark-report.md` содержит две таблицы:

**Таблица генерации:** успешность по сценариям, время выполнения.

**Таблица качества** (при включённом анализе): средние оценки по критериям (корректность, качество, полнота, ясность), булевы критерии (соответствие дисциплине, различие ЛР, логичность, различие вариантов, единообразие сложности).

### 15.8. Безопасность

- После завершения всех прогонов (включая аварийное завершение) Runner автоматически восстанавливает исходные настройки LLM на WebAPI.
- API-ключи для моделей считываются из переменных окружения, указанных в поле `apiKeyEnv`; сами ключи не хранятся в файле `models.json`.

## 16. Направления развития

1. Ввести режим строгого выравнивания количества вариантов до заданного значения.
2. Добавить предварительную проверку окружения перед запуском сценария.
3. Реализовать экспорт отчётов в дополнительные форматы, включая `CSV` и `HTML`.
4. Расширить телеметрию длительности этапов и затрат токенов.
5. Добавить учебный режим без обращения к внешним API для отладки оркестрации.
