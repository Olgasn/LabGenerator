using System.Globalization;

namespace LabGenerator.TeacherEmulator;

public sealed class JournalAnalysisOptions
{
    public required string InputDirectory { get; init; }

    public required string OutputDirectory { get; init; }

    public required Uri OllamaBaseUri { get; init; }

    public required string Model { get; init; }

    public required string? ApiKey { get; init; }

    public required TimeSpan RequestTimeout { get; init; }

    public string? CriteriaPath { get; init; }

    public required LlmProvider LlmProvider { get; init; }

    public static bool TryLoad(
        string[] args,
        string currentDirectory,
        out JournalAnalysisOptions? options,
        out string? error)
    {
        var argMap = ParseArgs(args);
        var modeEnabled = HasFlag(args, "--analyze-journals")
                          || argMap.ContainsKey("analysis-dir")
                          || argMap.ContainsKey("input-dir")
                          || argMap.ContainsKey("journals-dir")
                          || IsEnabled("LG_ANALYZER_MODE")
                          || !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("LG_ANALYZER_INPUT_DIR"));

        if (!modeEnabled)
        {
            options = null;
            error = null;
            return false;
        }

        var inputRaw = ReadSetting(argMap, "analysis-dir", "LG_ANALYZER_INPUT_DIR", string.Empty);
        if (string.IsNullOrWhiteSpace(inputRaw))
        {
            inputRaw = ReadSetting(argMap, "input-dir", "LG_ANALYZER_INPUT_DIR", string.Empty);
        }

        if (string.IsNullOrWhiteSpace(inputRaw))
        {
            inputRaw = ReadSetting(argMap, "journals-dir", "LG_ANALYZER_INPUT_DIR", string.Empty);
        }

        if (string.IsNullOrWhiteSpace(inputRaw))
        {
            options = null;
            error = "Input directory is required for analysis (--analysis-dir=...).";
            return true;
        }

        var inputDirectory = ResolveInputDirectory(inputRaw, currentDirectory);
        if (string.IsNullOrWhiteSpace(inputDirectory) || !Directory.Exists(inputDirectory))
        {
            options = null;
            error = $"Input directory not found: {inputRaw}";
            return true;
        }

        var outputRaw = ReadSetting(
            argMap,
            "analysis-output-dir",
            "LG_ANALYZER_OUTPUT_DIR",
            Path.Combine("artifacts", "journal-analysis"));

        var outputDirectory = Path.GetFullPath(Path.IsPathRooted(outputRaw)
            ? outputRaw
            : Path.Combine(currentDirectory, outputRaw));

        var baseUrl = ReadSetting(
            argMap,
            "analysis-ollama-base-url",
            "LG_ANALYZER_OLLAMA_BASE_URL",
            Environment.GetEnvironmentVariable("LLM__Ollama__BaseUrl")?.Trim() ?? "https://openrouter.ai");

        var model = ReadSetting(
            argMap,
            "analysis-ollama-model",
            "LG_ANALYZER_OLLAMA_MODEL",
            Environment.GetEnvironmentVariable("LLM__Ollama__Model")?.Trim() ?? "deepseek-v3.2:cloud");

        var providerRaw = ReadSetting(argMap, "analysis-llm-provider", "LG_ANALYZER_LLM_PROVIDER", string.Empty);
        var llmProvider = DetectProvider(providerRaw, baseUrl);

        var apiKey = ResolveApiKey(argMap, llmProvider);

        var timeoutSeconds = ReadIntSetting(
            argMap,
            "analysis-request-timeout-seconds",
            "LG_ANALYZER_REQUEST_TIMEOUT_SECONDS",
            600,
            min: 10,
            max: 3600);

        var criteriaRaw = ReadSetting(
            argMap,
            "analysis-criteria-path",
            "LG_ANALYZER_CRITERIA_PATH",
            string.Empty);

        var criteriaPath = ResolveCriteriaPath(criteriaRaw, currentDirectory);

        options = new JournalAnalysisOptions
        {
            InputDirectory = inputDirectory,
            OutputDirectory = outputDirectory,
            OllamaBaseUri = NormalizeBaseUri(baseUrl),
            Model = model,
            ApiKey = string.IsNullOrWhiteSpace(apiKey) ? null : apiKey.Trim(),
            RequestTimeout = TimeSpan.FromSeconds(timeoutSeconds),
            CriteriaPath = criteriaPath,
            LlmProvider = llmProvider
        };
        error = null;
        return true;
    }

    private static string ResolveApiKey(IReadOnlyDictionary<string, string> argMap, LlmProvider provider)
    {
        // Explicit arg always wins
        if (argMap.TryGetValue("analysis-ollama-api-key", out var fromArg) && !string.IsNullOrWhiteSpace(fromArg))
        {
            return fromArg.Trim();
        }

        // Provider-specific env var first
        if (provider == LlmProvider.OpenRouter)
        {
            var orKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY")?.Trim();
            if (!string.IsNullOrWhiteSpace(orKey))
            {
                return orKey;
            }
        }

        // Fallback chain
        return Environment.GetEnvironmentVariable("OLLAMA_API_KEY")?.Trim()
               ?? Environment.GetEnvironmentVariable("LLM__Ollama__ApiKey")?.Trim()
               ?? Environment.GetEnvironmentVariable("LG_EMULATOR_OLLAMA_API_KEY")?.Trim()
               ?? Environment.GetEnvironmentVariable("OPENROUTER_API_KEY")?.Trim()
               ?? string.Empty;
    }

    private static LlmProvider DetectProvider(string providerRaw, string baseUrl)
    {
        if (!string.IsNullOrWhiteSpace(providerRaw))
        {
            if (providerRaw.Trim().Equals("openrouter", StringComparison.OrdinalIgnoreCase))
            {
                return LlmProvider.OpenRouter;
            }

            if (providerRaw.Trim().Equals("ollama", StringComparison.OrdinalIgnoreCase))
            {
                return LlmProvider.Ollama;
            }
        }

        if (Uri.TryCreate(baseUrl.Trim(), UriKind.Absolute, out var uri) &&
            uri.Host.Contains("openrouter.ai", StringComparison.OrdinalIgnoreCase))
        {
            return LlmProvider.OpenRouter;
        }

        return LlmProvider.OpenRouter;
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

    private static string? ResolveCriteriaPath(string raw, string currentDirectory)
    {
        var candidates = new List<string>();

        if (!string.IsNullOrWhiteSpace(raw))
        {
            candidates.Add(Path.IsPathRooted(raw)
                ? Path.GetFullPath(raw)
                : Path.GetFullPath(Path.Combine(currentDirectory, raw)));
        }

        candidates.Add(Path.GetFullPath(Path.Combine(currentDirectory, "Критерии оценки.txt")));
        candidates.Add(Path.GetFullPath(Path.Combine(currentDirectory, "LabGenerator.TeacherEmulator", "Критерии оценки.txt")));
        candidates.Add(Path.GetFullPath(Path.Combine(currentDirectory, "LabGenerator", "LabGenerator.TeacherEmulator", "Критерии оценки.txt")));

        foreach (var candidate in candidates.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    private static Uri NormalizeBaseUri(string value)
    {
        var trimmed = value.Trim();
        if (!trimmed.EndsWith('/'))
        {
            trimmed += "/";
        }

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
        {
            throw new InvalidOperationException($"Invalid URL: {value}");
        }

        return uri;
    }

    private static string? ResolveInputDirectory(string raw, string currentDirectory)
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

                var parent = Directory.GetParent(currentDirectory)?.FullName;
                if (!string.IsNullOrWhiteSpace(parent))
                {
                    candidates.Add(Path.GetFullPath(Path.Combine(parent, raw)));
                }

                var trimmed = raw.Replace('/', '\\');
                const string prefix = "LabGenerator\\";
                if (trimmed.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    var withoutPrefix = trimmed[prefix.Length..];
                    candidates.Add(Path.GetFullPath(Path.Combine(currentDirectory, withoutPrefix)));
                }
            }
        }

        foreach (var candidate in candidates.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (Directory.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
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

    private static int ReadIntSetting(
        IReadOnlyDictionary<string, string> args,
        string argKey,
        string envKey,
        int defaultValue,
        int min,
        int max)
    {
        var raw = ReadSetting(args, argKey, envKey, defaultValue.ToString(CultureInfo.InvariantCulture));
        if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
        {
            throw new InvalidOperationException($"Value for {envKey} is not an integer: {raw}");
        }

        if (parsed < min || parsed > max)
        {
            throw new InvalidOperationException($"Value for {envKey} must be in range [{min}, {max}], got {parsed}.");
        }

        return parsed;
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
