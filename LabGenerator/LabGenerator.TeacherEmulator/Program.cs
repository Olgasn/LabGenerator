using DotNetEnv;
using LabGenerator.TeacherEmulator;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        LoadDotEnv();

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cts.Cancel();
        };

        if (JournalAnalysisOptions.TryLoad(args, Directory.GetCurrentDirectory(), out var analysisOptions, out var analysisError))
        {
            if (!string.IsNullOrWhiteSpace(analysisError))
            {
                Console.Error.WriteLine($"Invalid analysis configuration: {analysisError}");
                return 1;
            }

            using var analysisHttp = new HttpClient
            {
                BaseAddress = analysisOptions!.OllamaBaseUri,
                Timeout = analysisOptions.RequestTimeout
            };

            var analysisClient = new OllamaAnalysisClient(analysisHttp, analysisOptions.Model, analysisOptions.ApiKey, analysisOptions.LlmProvider);
            var analysisRunner = new JournalAnalysisRunner(analysisOptions, analysisClient);

            try
            {
                await analysisRunner.RunAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                Console.Error.WriteLine("Execution canceled.");
                return 2;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Execution failed: {ex}");
                return 1;
            }

            return 0;
        }

        if (ModelBenchmarkOptions.TryLoad(args, Directory.GetCurrentDirectory(), out var benchmarkOptions, out var benchmarkError))
        {
            if (!string.IsNullOrWhiteSpace(benchmarkError))
            {
                Console.Error.WriteLine($"Invalid benchmark configuration: {benchmarkError}");
                return 1;
            }

            if (!TestPlanOptions.TryLoad(args, Directory.GetCurrentDirectory(), out var benchPlanOptions, out var benchPlanError))
            {
                Console.Error.WriteLine($"Model benchmark requires --test-plan or --test-plan-csv. {benchPlanError}");
                return 1;
            }

            if (!string.IsNullOrWhiteSpace(benchPlanError))
            {
                Console.Error.WriteLine($"Invalid test plan configuration: {benchPlanError}");
                return 1;
            }

            TeacherEmulatorOptions benchEmulatorOptions;
            try
            {
                benchEmulatorOptions = TeacherEmulatorOptions.FromEnvironment(args, Directory.GetCurrentDirectory());
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Invalid configuration: {ex.Message}");
                return 1;
            }

            using var benchLgHttp = new HttpClient
            {
                BaseAddress = benchEmulatorOptions.LgBaseUri,
                Timeout = benchEmulatorOptions.RequestTimeout
            };

            var benchLgClient = new LgApiClient(benchLgHttp);

            JournalAnalysisOptions? analysisTemplate = null;
            if (benchmarkOptions!.RunAnalysis)
            {
                JournalAnalysisOptions.TryLoad(args, Directory.GetCurrentDirectory(), out analysisTemplate, out _);
                analysisTemplate ??= BuildDefaultAnalysisOptions(benchEmulatorOptions);
            }

            var benchRunner = new ModelBenchmarkRunner(
                benchmarkOptions,
                benchPlanOptions!,
                benchEmulatorOptions,
                benchLgClient,
                analysisTemplate);

            ModelBenchmarkSummary benchSummary;
            try
            {
                benchSummary = await benchRunner.RunAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                Console.Error.WriteLine("Execution canceled.");
                return 2;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Execution failed: {ex}");
                return 1;
            }

            var allSucceeded = benchSummary.Models.All(m => m.Succeeded);
            Console.WriteLine(allSucceeded
                ? "Model benchmark completed successfully."
                : "Model benchmark completed with failures.");
            return allSucceeded ? 0 : 1;
        }

        if (TestPlanOptions.TryLoad(args, Directory.GetCurrentDirectory(), out var planOptions, out var planError))
        {
            if (!string.IsNullOrWhiteSpace(planError))
            {
                Console.Error.WriteLine($"Invalid test plan configuration: {planError}");
                return 1;
            }

            TeacherEmulatorOptions planEmulatorOptions;
            try
            {
                planEmulatorOptions = TeacherEmulatorOptions.FromEnvironment(args, Directory.GetCurrentDirectory());
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Invalid configuration: {ex.Message}");
                return 1;
            }

            using var planLgHttp = new HttpClient
            {
                BaseAddress = planEmulatorOptions.LgBaseUri,
                Timeout = planEmulatorOptions.RequestTimeout
            };

            var planLgClient = new LgApiClient(planLgHttp);
            var planRunner = new TestPlanRunner(planEmulatorOptions, planLgClient, planOptions!);

            TestPlanSummary summary;
            try
            {
                summary = await planRunner.RunAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                Console.Error.WriteLine("Execution canceled.");
                return 2;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Execution failed: {ex}");
                return 1;
            }

            return summary.Succeeded ? 0 : 1;
        }

        TeacherEmulatorOptions options;
        try
        {
            options = TeacherEmulatorOptions.FromEnvironment(args, Directory.GetCurrentDirectory());
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Invalid configuration: {ex.Message}");
            return 1;
        }

        using var lgHttp = new HttpClient
        {
            BaseAddress = options.LgBaseUri,
            Timeout = options.RequestTimeout
        };

        using var ollamaHttp = new HttpClient
        {
            BaseAddress = options.OllamaBaseUri,
            Timeout = options.RequestTimeout
        };

        var lgClient = new LgApiClient(lgHttp);
        var teacherClient = new OllamaTeacherClient(ollamaHttp, options.TeacherModel, options.OllamaApiKey, options.LlmProvider);
        var runner = new TeacherEmulatorRunner(options, lgClient, teacherClient);

        TeacherEmulatorReport report;
        try
        {
            report = await runner.RunAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            Console.Error.WriteLine("Execution canceled.");
            return 2;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Execution failed: {ex}");
            return 1;
        }

        var artifactPaths = await ReportWriter.WriteAsync(report, options.OutputDirectory, cts.Token);
        Console.WriteLine($"JSON report: {artifactPaths.JsonPath}");
        Console.WriteLine($"Markdown report: {artifactPaths.MarkdownPath}");
        Console.WriteLine(report.Succeeded
            ? "Teacher emulator completed successfully."
            : "Teacher emulator completed with failures.");

        return report.Succeeded ? 0 : 1;
    }

    private static JournalAnalysisOptions BuildDefaultAnalysisOptions(TeacherEmulatorOptions emulatorOptions)
    {
        return new JournalAnalysisOptions
        {
            InputDirectory = string.Empty,
            OutputDirectory = string.Empty,
            OllamaBaseUri = emulatorOptions.OllamaBaseUri,
            Model = emulatorOptions.TeacherModel,
            ApiKey = emulatorOptions.OllamaApiKey,
            RequestTimeout = TimeSpan.FromSeconds(600),
            CriteriaPath = null,
            LlmProvider = emulatorOptions.LlmProvider
        };
    }

    private static void LoadDotEnv()
    {
        var candidates = new[]
        {
            Path.Combine(Directory.GetCurrentDirectory(), ".env"),
            Path.Combine(AppContext.BaseDirectory, ".env"),
            Path.Combine(Directory.GetCurrentDirectory(), "LabGenerator", ".env"),
        };

        foreach (var path in candidates)
        {
            if (File.Exists(path))
            {
                Env.Load(path);
                return;
            }
        }
    }
}
