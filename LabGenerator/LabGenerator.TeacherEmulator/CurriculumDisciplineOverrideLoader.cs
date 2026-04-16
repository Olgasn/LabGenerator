namespace LabGenerator.TeacherEmulator;

public sealed class CurriculumDisciplineOverride
{
    public string Name { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string Path { get; init; } = string.Empty;
}

public static class CurriculumDisciplineOverrideLoader
{
    private const string CurriculumFileName = "curriculum1.md";
    private const string ProjectFileName = "LabGenerator.TeacherEmulator.csproj";

    public static CurriculumDisciplineOverride? TryLoad(string currentDirectory, string appBaseDirectory)
    {
        var sourceCandidates = EnumerateSourceCandidatePaths(currentDirectory).ToList();
        foreach (var candidate in sourceCandidates)
        {
            var result = TryLoadFromPath(candidate);
            if (result is not null)
            {
                return result;
            }
        }

        if (SourceProjectExists(currentDirectory))
        {
            return null;
        }

        return TryLoadFromPath(Path.Combine(appBaseDirectory, "curriculums", CurriculumFileName));
    }

    private static IEnumerable<string> EnumerateSourceCandidatePaths(string currentDirectory)
    {
        var projectRoots = new[]
        {
            currentDirectory,
            Path.Combine(currentDirectory, "LabGenerator.TeacherEmulator"),
            Path.Combine(currentDirectory, "LabGenerator", "LabGenerator.TeacherEmulator")
        }
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(Path.GetFullPath)
            .Distinct(StringComparer.OrdinalIgnoreCase);

        foreach (var projectRoot in projectRoots)
        {
            yield return Path.Combine(projectRoot, "curriculums", CurriculumFileName);
        }
    }

    private static bool SourceProjectExists(string currentDirectory)
        => new[]
        {
            currentDirectory,
            Path.Combine(currentDirectory, "LabGenerator.TeacherEmulator"),
            Path.Combine(currentDirectory, "LabGenerator", "LabGenerator.TeacherEmulator")
        }
        .Where(x => !string.IsNullOrWhiteSpace(x))
        .Select(Path.GetFullPath)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .Any(x => File.Exists(Path.Combine(x, ProjectFileName)));

    private static CurriculumDisciplineOverride? TryLoadFromPath(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        var content = File.ReadAllText(path);
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        var firstLine = ReadFirstLine(content);
        var disciplineName = NormalizeDisciplineName(firstLine);
        if (string.IsNullOrWhiteSpace(disciplineName))
        {
            return null;
        }

        return new CurriculumDisciplineOverride
        {
            Name = disciplineName,
            Description = content,
            Path = Path.GetFullPath(path)
        };
    }

    private static string ReadFirstLine(string content)
    {
        using var reader = new StringReader(content);
        return reader.ReadLine() ?? string.Empty;
    }

    private static string NormalizeDisciplineName(string firstLine)
    {
        var trimmed = firstLine.Trim();
        if (trimmed.Length == 0)
        {
            return string.Empty;
        }

        return trimmed.TrimStart('#').Trim();
    }
}
