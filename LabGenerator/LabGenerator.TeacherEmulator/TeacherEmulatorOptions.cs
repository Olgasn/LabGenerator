using System.Globalization;

namespace LabGenerator.TeacherEmulator;

public sealed class TeacherEmulatorOptions
{
    public required Uri LgBaseUri { get; init; }

    public required Uri OllamaBaseUri { get; init; }

    public required string TeacherModel { get; init; }

    public required string? OllamaApiKey { get; init; }

    public required string OutputDirectory { get; init; }

    public required string SeedTopic { get; init; }

    public required int LabCount { get; init; }

    public required int VariantCountPerLab { get; init; }

    public required int MaxVerificationRetries { get; init; }

    public required int MaxRegenerationRetries { get; init; }

    public required TimeSpan JobTimeout { get; init; }

    public required TimeSpan JobPollInterval { get; init; }

    public required TimeSpan RequestTimeout { get; init; }

    public required LlmProvider LlmProvider { get; init; }

    public static TeacherEmulatorOptions FromEnvironment(string[] args, string currentDirectory)
    {
        var argMap = ParseArgs(args);

        var lgBase = ReadSetting(argMap, "lg-base-url", "LG_EMULATOR_API_BASE_URL", "http://localhost:8080");
        var ollamaBase = ReadSetting(
            argMap,
            "ollama-base-url",
            "LG_EMULATOR_OLLAMA_BASE_URL",
            Environment.GetEnvironmentVariable("LLM__Ollama__BaseUrl")?.Trim() ?? "https://openrouter.ai");
        var teacherModel = ReadSetting(
            argMap,
            "teacher-model",
            "LG_EMULATOR_TEACHER_MODEL",
            Environment.GetEnvironmentVariable("LLM__Ollama__Model")?.Trim() ?? "deepseek-v3.2:cloud");
        var outputDir = ReadSetting(argMap, "output-dir", "LG_EMULATOR_OUTPUT_DIR", "artifacts/teacher-emulator");
        var seedTopic = ReadSetting(argMap, "seed-topic", "LG_EMULATOR_SEED_TOPIC", "Applied software engineering");

        var labs = ReadIntSetting(argMap, "lab-count", "LG_EMULATOR_LAB_COUNT", 3, min: 1, max: 20);
        var variants = ReadIntSetting(argMap, "variant-count", "LG_EMULATOR_VARIANT_COUNT", 6, min: 1, max: 100);
        var maxVerifyRetries = ReadIntSetting(argMap, "max-verify-retries", "LG_EMULATOR_MAX_VERIFY_RETRIES", 1, min: 0, max: 10);
        var maxRegenRetries = ReadIntSetting(argMap, "max-regen-retries", "LG_EMULATOR_MAX_REGEN_RETRIES", 1, min: 0, max: 10);
        var jobTimeoutSec = ReadIntSetting(argMap, "job-timeout-seconds", "LG_EMULATOR_JOB_TIMEOUT_SECONDS", 900, min: 30, max: 7200);
        var pollSec = ReadIntSetting(argMap, "job-poll-seconds", "LG_EMULATOR_JOB_POLL_SECONDS", 2, min: 1, max: 30);
        var requestTimeoutSec = ReadIntSetting(argMap, "request-timeout-seconds", "LG_EMULATOR_REQUEST_TIMEOUT_SECONDS", 180, min: 10, max: 3600);

        var providerRaw = ReadSetting(argMap, "llm-provider", "LG_EMULATOR_LLM_PROVIDER", string.Empty);
        var llmProvider = DetectProvider(providerRaw, ollamaBase);

        var apiKey = ResolveApiKey(argMap, llmProvider);

        var normalizedOutput = Path.GetFullPath(Path.IsPathRooted(outputDir)
            ? outputDir
            : Path.Combine(currentDirectory, outputDir));

        return new TeacherEmulatorOptions
        {
            LgBaseUri = NormalizeBaseUri(lgBase),
            OllamaBaseUri = NormalizeBaseUri(ollamaBase),
            TeacherModel = teacherModel,
            OllamaApiKey = string.IsNullOrWhiteSpace(apiKey) ? null : apiKey.Trim(),
            OutputDirectory = normalizedOutput,
            SeedTopic = seedTopic,
            LabCount = labs,
            VariantCountPerLab = variants,
            MaxVerificationRetries = maxVerifyRetries,
            MaxRegenerationRetries = maxRegenRetries,
            JobTimeout = TimeSpan.FromSeconds(jobTimeoutSec),
            JobPollInterval = TimeSpan.FromSeconds(pollSec),
            RequestTimeout = TimeSpan.FromSeconds(requestTimeoutSec),
            LlmProvider = llmProvider
        };
    }

    public TeacherEmulatorOptionsSnapshot ToSnapshot()
        => new()
        {
            LgBaseUrl = LgBaseUri.ToString(),
            OllamaBaseUrl = OllamaBaseUri.ToString(),
            TeacherModel = TeacherModel,
            LlmProvider = LlmProvider.ToString(),
            OutputDirectory = OutputDirectory,
            SeedTopic = SeedTopic,
            LabCount = LabCount,
            VariantCountPerLab = VariantCountPerLab,
            MaxVerificationRetries = MaxVerificationRetries,
            MaxRegenerationRetries = MaxRegenerationRetries,
            JobTimeoutSeconds = (int)JobTimeout.TotalSeconds,
            JobPollSeconds = (int)JobPollInterval.TotalSeconds,
            RequestTimeoutSeconds = (int)RequestTimeout.TotalSeconds
        };

    private static string ResolveApiKey(IReadOnlyDictionary<string, string> argMap, LlmProvider provider)
    {
        if (argMap.TryGetValue("ollama-api-key", out var fromArg) && !string.IsNullOrWhiteSpace(fromArg))
        {
            return fromArg.Trim();
        }

        if (provider == LlmProvider.OpenRouter)
        {
            var orKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY")?.Trim();
            if (!string.IsNullOrWhiteSpace(orKey))
            {
                return orKey;
            }
        }

        return Environment.GetEnvironmentVariable("LG_EMULATOR_OLLAMA_API_KEY")?.Trim()
               ?? Environment.GetEnvironmentVariable("LLM__Ollama__ApiKey")?.Trim()
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
}
