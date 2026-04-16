namespace LabGenerator.TeacherEmulator;

public sealed class TestPlanOptions
{
    public required string CsvPath { get; init; }

    public required string OutputRoot { get; init; }

    public static bool TryLoad(
        string[] args,
        string currentDirectory,
        out TestPlanOptions? options,
        out string? error)
    {
        var argMap = ParseArgs(args);
        var modeEnabled = HasFlag(args, "--test-plan")
                          || argMap.ContainsKey("test-plan")
                          || argMap.ContainsKey("test-plan-csv")
                          || IsEnabled("LG_TEST_PLAN_MODE")
                          || !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("LG_TEST_PLAN_CSV"));

        if (!modeEnabled)
        {
            options = null;
            error = null;
            return false;
        }

        var csvRaw = ReadSetting(
            argMap,
            "test-plan-csv",
            "LG_TEST_PLAN_CSV",
            Path.Combine("LabGenerator.TeacherEmulator", "План испытаний.csv"));

        var csvPath = ResolveCsvPath(csvRaw, currentDirectory);
        if (!File.Exists(csvPath))
        {
            options = null;
            error = $"Test plan CSV not found: {csvPath}";
            return true;
        }

        var outputRaw = ReadSetting(
            argMap,
            "test-plan-output-dir",
            "LG_TEST_PLAN_OUTPUT_DIR",
            Path.Combine("artifacts", "test-plan"));

        var outputRoot = Path.GetFullPath(Path.IsPathRooted(outputRaw)
            ? outputRaw
            : Path.Combine(currentDirectory, outputRaw));

        options = new TestPlanOptions
        {
            CsvPath = csvPath,
            OutputRoot = outputRoot
        };
        error = null;
        return true;
    }

    private static bool IsEnabled(string key)
    {
        var value = Environment.GetEnvironmentVariable(key);
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalized = value.Trim();
        return normalized == "1" || normalized.Equals("true", StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolveCsvPath(string raw, string currentDirectory)
    {
        var candidates = new List<string>();

        if (!string.IsNullOrWhiteSpace(raw))
        {
            if (Path.IsPathRooted(raw))
            {
                candidates.Add(Path.GetFullPath(raw));
            }
            else
            {
                candidates.Add(Path.GetFullPath(Path.Combine(currentDirectory, raw)));
                candidates.Add(Path.GetFullPath(Path.Combine(currentDirectory, "LabGenerator", raw)));
            }
        }

        var defaultRelative = Path.Combine("LabGenerator.TeacherEmulator", "План испытаний.csv");
        candidates.Add(Path.GetFullPath(Path.Combine(currentDirectory, defaultRelative)));
        candidates.Add(Path.GetFullPath(Path.Combine(currentDirectory, "LabGenerator", defaultRelative)));

        foreach (var candidate in candidates.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return candidates.FirstOrDefault() ?? Path.GetFullPath(raw);
    }

    private static string ReadSetting(
        IReadOnlyDictionary<string, string> args,
        string argKey,
        string envKey,
        string defaultValue)
    {
        if (args.TryGetValue(argKey, out var fromArgs) && !string.IsNullOrWhiteSpace(fromArgs))
        {
            return fromArgs.Trim();
        }

        var fromEnv = Environment.GetEnvironmentVariable(envKey);
        if (!string.IsNullOrWhiteSpace(fromEnv))
        {
            return fromEnv.Trim();
        }

        return defaultValue;
    }

    private static Dictionary<string, string> ParseArgs(string[] args)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var arg in args)
        {
            if (!arg.StartsWith("--", StringComparison.Ordinal) || arg.Length <= 2)
            {
                continue;
            }

            var eq = arg.IndexOf('=', 2);
            if (eq <= 2 || eq == arg.Length - 1)
            {
                continue;
            }

            var key = arg[2..eq].Trim();
            var value = arg[(eq + 1)..].Trim();
            if (key.Length == 0)
            {
                continue;
            }

            map[key] = value;
        }

        return map;
    }

    private static bool HasFlag(string[] args, string flag)
        => args.Any(x => x.Equals(flag, StringComparison.OrdinalIgnoreCase));
}
