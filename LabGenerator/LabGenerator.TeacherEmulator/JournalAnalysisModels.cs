namespace LabGenerator.TeacherEmulator;

public sealed class JournalAnalysisSummary
{
    public DateTimeOffset StartedAtUtc { get; set; }

    public DateTimeOffset? FinishedAtUtc { get; set; }

    public string InputDirectory { get; set; } = string.Empty;

    public string OutputDirectory { get; set; } = string.Empty;

    public string? CriteriaPath { get; set; }

    public List<JournalAnalysisResult> Results { get; set; } = new();
}

public sealed class JournalAnalysisResult
{
    public string RunId { get; set; } = string.Empty;

    public string SourceJsonPath { get; set; } = string.Empty;

    public string? SourceMarkdownPath { get; set; }

    public DisciplineAnalysis Discipline { get; set; } = new();

    public List<LabComparisonAnalysis> Labs { get; set; } = new();

    public List<LabQualityAnalysis> Quality { get; set; } = new();
}

public sealed class DisciplineAnalysis
{
    public string Name { get; set; } = string.Empty;

    public bool AssignmentsMatchDiscipline { get; set; }

    public string AssignmentsMatchReason { get; set; } = string.Empty;

    public bool LabsDiffer { get; set; }

    public string LabsDifferReason { get; set; } = string.Empty;

    public bool SequenceLogical { get; set; }

    public string SequenceReason { get; set; } = string.Empty;
}

public sealed class LabComparisonAnalysis
{
    public int LabNumber { get; set; }

    public string AssignmentTitle { get; set; } = string.Empty;

    public bool VariantsDiffer { get; set; }

    public string VariantsDifferences { get; set; } = string.Empty;

    public bool VariantsSameDifficulty { get; set; }

    public string VariantsDifficultyReason { get; set; } = string.Empty;

    public string MissingGenerationReason { get; set; } = string.Empty;

    public int VerificationRetries { get; set; }

    public int RegenerationRetries { get; set; }
}

public sealed class LabQualityAnalysis
{
    public int LabNumber { get; set; }

    public string AssignmentTitle { get; set; } = string.Empty;

    public int Correctness { get; set; }

    public int Quality { get; set; }

    public int Completeness { get; set; }

    public int Clarity { get; set; }

    public string Justification { get; set; } = string.Empty;
}
