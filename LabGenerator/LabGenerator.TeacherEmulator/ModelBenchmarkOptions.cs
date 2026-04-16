namespace LabGenerator.TeacherEmulator;

public sealed class ModelBenchmarkOptions
{
    public required string ModelsFilePath { get; init; }

    public required string OutputRoot { get; init; }

    public required bool RunAnalysis { get; init; }

    public static bool TryLoad(
        string[] args,
        string currentDirectory,
        out ModelBenchmarkOptions? options,
        out string? error)
    {
        var argMap = ParseArgs(args);
        var modeEnabled = HasFlag(args, "--model-benchmark")
                          || argMap.ContainsKey("models-file")
                          || IsEnabled("LG_BENCHMARK_MODE");

        if (!modeEnabled)
        {
            options = null;
            error = null;
            return false;
        }

        var modelsRaw = ReadSetting(
            argMap,
            "models-file",
            "LG_BENCHMARK_MODELS_FILE",
            "models.json");

        var modelsPath = ResolveFilePath(modelsRaw, currentDirectory);
        if (!File.Exists(modelsPath))
        {
            options = null;
            error = $"Models file not found: {modelsPath}";
            return true;
        }

        var outputRaw = ReadSetting(
            argMap,
            "benchmark-output-dir",
            "LG_BENCHMARK_OUTPUT_DIR",
            Path.Combine("artifacts", "model-benchmark"));

        var outputRoot = Path.GetFullPath(Path.IsPathRooted(outputRaw)
            ? outputRaw
            : Path.Combine(currentDirectory, outputRaw));

        var skipAnalysis = HasFlag(args, "--skip-analysis") || IsEnabled("LG_BENCHMARK_SKIP_ANALYSIS");

        options = new ModelBenchmarkOptions
        {
            ModelsFilePath = modelsPath,
            OutputRoot = outputRoot,
            RunAnalysis = !skipAnalysis
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

    private static string ResolveFilePath(string raw, string currentDirectory)
    {
        if (Path.IsPathRooted(raw))
        {
            return Path.GetFullPath(raw);
        }

        var candidates = new[]
        {
            Path.GetFullPath(Path.Combine(currentDirectory, raw)),
            Path.GetFullPath(Path.Combine(currentDirectory, "LabGenerator", raw)),
            Path.GetFullPath(Path.Combine(currentDirectory, "LabGenerator.TeacherEmulator", raw))
        };

        foreach (var candidate in candidates)
        {
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return candidates[0];
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
