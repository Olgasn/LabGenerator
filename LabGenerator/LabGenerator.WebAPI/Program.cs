using LabGenerator.Infrastructure.Data;
using LabGenerator.Application.Abstractions;
using LabGenerator.Application.Models;
using LabGenerator.Infrastructure.Jobs;
using LabGenerator.Infrastructure.Llm;
using LabGenerator.Infrastructure.Services;
using LabGenerator.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("difficulty_defaults.json", optional: false, reloadOnChange: false);

builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
builder.Logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.Extensions.Http", LogLevel.Warning);

builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        if (origins is { Length: > 0 })
            policy.WithOrigins(origins);
        else
            policy.AllowAnyOrigin();

        policy.AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.Configure<ApplicationSettings>(builder.Configuration.GetSection("ApplicationSettings"));

builder.Services.Configure<OllamaOptions>(builder.Configuration.GetSection(OllamaOptions.SectionName));
builder.Services.Configure<OpenRouterOptions>(builder.Configuration.GetSection(OpenRouterOptions.SectionName));
builder.Services.Configure<DifficultyDefaults>(builder.Configuration.GetSection(DifficultyDefaults.SectionName));

builder.Services.AddHttpClient<OllamaTextGenerationService>()
    .ConfigureHttpClient((sp, http) =>
    {
        var appSettings = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ApplicationSettings>>().Value;
        http.Timeout = TimeSpan.FromSeconds(Math.Clamp(appSettings.LlmRequestTimeoutSeconds, 10, 3600));
    });

builder.Services.AddScoped<Microsoft.SemanticKernel.TextGeneration.ITextGenerationService>(sp =>
    sp.GetRequiredService<OllamaTextGenerationService>());

builder.Services.AddScoped<SemanticKernelLLMClient>();

builder.Services.AddHttpClient<OpenRouterLLMClient>()
    .ConfigureHttpClient((sp, http) =>
    {
        var appSettings = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ApplicationSettings>>().Value;
        http.Timeout = TimeSpan.FromSeconds(Math.Clamp(appSettings.LlmRequestTimeoutSeconds, 10, 3600));
    });

builder.Services.AddScoped<RoutingLLMClient>();

builder.Services.AddScoped<ILLMClient>(sp =>
    sp.GetRequiredService<RoutingLLMClient>());

builder.Services.AddSingleton<LlmPromptTemplateService>();
builder.Services.AddScoped<PromptCustomSectionService>();
builder.Services.AddScoped<LlmAccessGuardService>();
builder.Services.AddScoped<IMasterAssignmentService, MasterAssignmentService>();
builder.Services.AddScoped<IVariantGenerationService, VariantGenerationService>();
builder.Services.AddScoped<ILabSupplementaryMaterialService, LabSupplementaryMaterialService>();
builder.Services.AddScoped<IVerificationService, VerificationService>();
builder.Services.AddScoped<IDocxExportService, DocxExportService>();

builder.Services.AddHostedService<GenerationJobWorker>();
builder.Services.AddHostedService<DatabaseMigrationService>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors();

app.MapControllers();

app.Run();
