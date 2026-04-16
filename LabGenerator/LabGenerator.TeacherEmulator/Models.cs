namespace LabGenerator.TeacherEmulator;

public sealed class TeacherEmulatorOptionsSnapshot
{
    public string LgBaseUrl { get; set; } = string.Empty;

    public string OllamaBaseUrl { get; set; } = string.Empty;

    public string TeacherModel { get; set; } = string.Empty;

    public string LlmProvider { get; set; } = string.Empty;

    public string OutputDirectory { get; set; } = string.Empty;

    public string SeedTopic { get; set; } = string.Empty;

    public int LabCount { get; set; }

    public int VariantCountPerLab { get; set; }

    public int MaxVerificationRetries { get; set; }

    public int MaxRegenerationRetries { get; set; }

    public int JobTimeoutSeconds { get; set; }

    public int JobPollSeconds { get; set; }

    public int RequestTimeoutSeconds { get; set; }
}

public sealed class TeacherEmulatorReport
{
    public string RunId { get; set; } = string.Empty;

    public DateTimeOffset StartedAtUtc { get; set; }

    public DateTimeOffset? FinishedAtUtc { get; set; }

    public bool Succeeded { get; set; }

    public string? Error { get; set; }

    public TeacherEmulatorOptionsSnapshot Options { get; set; } = new();

    public DisciplineDto? Discipline { get; set; }

    public List<LabExecutionReport> Labs { get; set; } = new();

    public List<JournalEvent> Events { get; set; } = new();
}

public sealed class LabExecutionReport
{
    public int LabNumber { get; set; }

    public LabPlanItem Plan { get; set; } = new();

    public string? FailedStage { get; set; }

    public string? Error { get; set; }

    public LabDto? CreatedLab { get; set; }

    public int? MasterGenerationJobId { get; set; }

    public int? VariantsGenerationJobId { get; set; }

    public int? VariationProfileId { get; set; }

    public List<int> ExtraVariantsGenerationJobIds { get; set; } = new();

    public List<int> VerificationJobIds { get; set; } = new();

    public MasterAssignmentDto? MasterBeforeReview { get; set; }

    public MasterAssignmentDto? MasterAfterReview { get; set; }

    public bool MasterUpdated { get; set; }

    public string? MasterReviewComment { get; set; }

    public List<SelectedVariationMethodExecution> AppliedVariationMethods { get; set; } = new();

    public List<AssignmentVariantDto> Variants { get; set; } = new();

    public List<VerificationReportSummary> VerificationReports { get; set; } = new();

    public int VerificationRetries { get; set; }

    public int RegenerationRetries { get; set; }

    public bool AllVariantsPassed { get; set; }
}

public sealed class SelectedVariationMethodExecution
{
    public int VariationMethodId { get; set; }

    public string Code { get; set; } = string.Empty;

    public bool PreserveAcrossLabs { get; set; }
}

public sealed class VerificationReportSummary
{
    public int VariantId { get; set; }

    public bool Passed { get; set; }

    public int? OverallScore { get; set; }

    public int IssueCount { get; set; }

    public string IssuesJson { get; set; } = "[]";
}

public sealed class JournalEvent
{
    public DateTimeOffset TimestampUtc { get; set; }

    public string Level { get; set; } = "info";

    public string Stage { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public Dictionary<string, string> Data { get; set; } = new();
}

public sealed class DisciplinePlan
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public List<LabPlanItem> Labs { get; set; } = new();
}

public sealed class LabPlanItem
{
    public int OrderIndex { get; set; }

    public string Title { get; set; } = string.Empty;

    public string InitialDescription { get; set; } = string.Empty;
}

public sealed class MasterReviewDecision
{
    public bool NeedsUpdate { get; set; }

    public string UpdatedContent { get; set; } = string.Empty;

    public string Comment { get; set; } = string.Empty;
}

public sealed class VariationSelectionDecision
{
    public List<VariationSelectionItemDecision> Items { get; set; } = new();
}

public sealed class VariationSelectionItemDecision
{
    public string Code { get; set; } = string.Empty;

    public bool PreserveAcrossLabs { get; set; }
}

public sealed class DisciplineDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}

public sealed class LabDto
{
    public int Id { get; set; }

    public int OrderIndex { get; set; }

    public string Title { get; set; } = string.Empty;

    public string InitialDescription { get; set; } = string.Empty;

    public int DisciplineId { get; set; }
}

public sealed class MasterAssignmentDto
{
    public int Id { get; set; }

    public int LabId { get; set; }

    public int Version { get; set; }

    public bool IsCurrent { get; set; }

    public int Status { get; set; }

    public string Content { get; set; } = string.Empty;
}

public sealed class VariationMethodDto
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsSystem { get; set; }
}

public sealed class LabVariationMethodDto
{
    public int Id { get; set; }

    public int LabId { get; set; }

    public int VariationMethodId { get; set; }

    public bool PreserveAcrossLabs { get; set; }

    public VariationMethodDto? VariationMethod { get; set; }
}

public sealed class VariationProfileDto
{
    public int Id { get; set; }

    public int LabId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string ParametersJson { get; set; } = "{}";

    public string DifficultyRubricJson { get; set; } = "{}";

    public bool IsDefault { get; set; }
}

public sealed class AssignmentVariantDto
{
    public int Id { get; set; }

    public int LabId { get; set; }

    public int VariantIndex { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string VariantParamsJson { get; set; } = "{}";

    public string DifficultyProfileJson { get; set; } = "{}";

    public string Fingerprint { get; set; } = string.Empty;
}

public sealed class VerificationReportDto
{
    public int Id { get; set; }

    public int AssignmentVariantId { get; set; }

    public bool Passed { get; set; }

    public string JudgeScoreJson { get; set; } = "{}";

    public string IssuesJson { get; set; } = "[]";
}

public sealed class GenerationJobDto
{
    public int Id { get; set; }

    public int Type { get; set; }

    public int Status { get; set; }

    public int? DisciplineId { get; set; }

    public int? LabId { get; set; }

    public int? MasterAssignmentId { get; set; }

    public int? VariationProfileId { get; set; }

    public string? PayloadJson { get; set; }

    public string? ResultJson { get; set; }

    public string? Error { get; set; }

    public int Progress { get; set; }
}

public sealed class CreateDisciplineRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}

public sealed class CreateLabRequest
{
    public int DisciplineId { get; set; }

    public int OrderIndex { get; set; }

    public string Title { get; set; } = string.Empty;

    public string InitialDescription { get; set; } = string.Empty;
}

public sealed class UpdateMasterAssignmentRequest
{
    public string Content { get; set; } = string.Empty;
}

public sealed class GenerateVariantsRequest
{
    public int Count { get; set; }

    public int? VariationProfileId { get; set; }
}

public sealed class UpsertVariationProfileRequest
{
    public string Name { get; set; } = string.Empty;

    public string ParametersJson { get; set; } = "{}";

    public string DifficultyRubricJson { get; set; } = "{}";

    public bool IsDefault { get; set; }
}

public sealed class LlmSettingsDto
{
    public int Id { get; set; }

    public string Provider { get; set; } = "Ollama";

    public string Model { get; set; } = string.Empty;
}

public sealed class UpdateLlmSettingsRequest
{
    public string Provider { get; set; } = "Ollama";

    public string Model { get; set; } = string.Empty;
}

public sealed class LlmProviderSettingsDto
{
    public int Id { get; set; }

    public string Provider { get; set; } = string.Empty;

    public string? Model { get; set; }

    public double? Temperature { get; set; }

    public int? MaxOutputTokens { get; set; }
}

public sealed class UpdateLlmProviderSettingsRequest
{
    public string Provider { get; set; } = "Ollama";

    public string? Model { get; set; }

    public double? Temperature { get; set; }

    public int? MaxOutputTokens { get; set; }
}

public sealed class VerifyVariantsRequest
{
    public int? VariantId { get; set; }
}

public sealed class UpsertLabVariationMethodsRequest
{
    public List<LabVariationMethodItemRequest> Items { get; set; } = new();
}

public sealed class LabVariationMethodItemRequest
{
    public int VariationMethodId { get; set; }

    public bool PreserveAcrossLabs { get; set; }
}

public sealed class TestPlanSummary
{
    public string CsvPath { get; set; } = string.Empty;

    public string OutputRoot { get; set; } = string.Empty;

    public DateTimeOffset StartedAtUtc { get; set; }

    public DateTimeOffset? FinishedAtUtc { get; set; }

    public bool Succeeded { get; set; }

    public List<TestPlanCaseResult> Cases { get; set; } = new();
}

public sealed class TestPlanCaseResult
{
    public int TestNumber { get; set; }

    public string Discipline { get; set; } = string.Empty;

    public int LabNumber { get; set; }

    public bool Succeeded { get; set; }

    public string JsonPath { get; set; } = string.Empty;

    public string MarkdownPath { get; set; } = string.Empty;
}

public sealed class ModelBenchmarkSummary
{
    public DateTimeOffset StartedAtUtc { get; set; }

    public DateTimeOffset? FinishedAtUtc { get; set; }

    public string ModelsFilePath { get; set; } = string.Empty;

    public string TestPlanCsvPath { get; set; } = string.Empty;

    public string OutputRoot { get; set; } = string.Empty;

    public int VariantCount { get; set; }

    public List<ModelBenchmarkResult> Models { get; set; } = new();
}

public sealed class ModelBenchmarkResult
{
    public string ModelName { get; set; } = string.Empty;

    public string Provider { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public DateTimeOffset StartedAtUtc { get; set; }

    public DateTimeOffset? FinishedAtUtc { get; set; }

    public bool Succeeded { get; set; }

    public string? Error { get; set; }

    public string TestPlanOutputRoot { get; set; } = string.Empty;

    public string? AnalysisOutputRoot { get; set; }

    public int TotalCases { get; set; }

    public int SucceededCases { get; set; }

    public int FailedCases { get; set; }

    public BenchmarkQualityAggregates? Quality { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public IReadOnlyDictionary<string, string>? CollectedMasterTexts { get; set; }
}

public sealed class BenchmarkQualityAggregates
{
    public double AvgCorrectness { get; set; }

    public double AvgQuality { get; set; }

    public double AvgCompleteness { get; set; }

    public double AvgClarity { get; set; }

    public int AssignmentsMatchDisciplineCount { get; set; }

    public int LabsDifferCount { get; set; }

    public int SequenceLogicalCount { get; set; }

    public int VariantsDifferCount { get; set; }

    public int VariantsSameDifficultyCount { get; set; }

    public int TotalLabsAnalyzed { get; set; }
}

internal sealed class TestPlanCase
{
    public int TestNumber { get; set; }

    public string DisciplineName { get; set; } = string.Empty;

    public int LabNumber { get; set; }

    public int? Param1 { get; set; }

    public bool PreserveParam1 { get; set; }

    public int? Param2 { get; set; }

    public bool PreserveParam2 { get; set; }
}
