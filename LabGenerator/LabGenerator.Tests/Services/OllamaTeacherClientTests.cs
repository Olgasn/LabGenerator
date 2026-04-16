using System.Net;
using System.Text;
using System.Text.Json;
using LabGenerator.TeacherEmulator;

namespace LabGenerator.Tests.Services;

public sealed class OllamaTeacherClientTests
{
    [Fact]
    public async Task BuildDisciplinePlanAsync_IncludesCurriculumContentInPrompt_WhenProvided()
    {
        var handler = new RecordingHttpMessageHandler(CreatePlanResponseJson());
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.test/")
        };
        var client = new OllamaTeacherClient(httpClient, "teacher-model", apiKey: null);
        var curriculum = new CurriculumDisciplineOverride
        {
            Name = "DATABASES",
            Description = "DATABASES\nTopic 1\nTopic 2",
            Path = "curriculums/curriculum1.md"
        };

        var plan = await client.BuildDisciplinePlanAsync(1, "Applied software engineering", curriculum, CancellationToken.None);

        Assert.Equal("Databases", plan.Name);
        Assert.NotNull(handler.LastRequestBody);

        using var payload = JsonDocument.Parse(handler.LastRequestBody);
        var prompt = payload.RootElement.GetProperty("prompt").GetString();
        Assert.NotNull(prompt);
        Assert.Contains("Use the following curriculum document as the source material for planning.", prompt, StringComparison.Ordinal);
        Assert.Contains(curriculum.Description, prompt, StringComparison.Ordinal);
        Assert.Contains("topic area: Applied software engineering.", prompt, StringComparison.Ordinal);
    }

    [Fact]
    public async Task BuildDisciplinePlanAsync_DoesNotIncludeCurriculumSection_WhenNotProvided()
    {
        var handler = new RecordingHttpMessageHandler(CreatePlanResponseJson());
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.test/")
        };
        var client = new OllamaTeacherClient(httpClient, "teacher-model", apiKey: null);

        await client.BuildDisciplinePlanAsync(1, "Applied software engineering", CancellationToken.None);

        Assert.NotNull(handler.LastRequestBody);

        using var payload = JsonDocument.Parse(handler.LastRequestBody);
        var prompt = payload.RootElement.GetProperty("prompt").GetString();
        Assert.NotNull(prompt);
        Assert.DoesNotContain("Use the following curriculum document as the source material for planning.", prompt, StringComparison.Ordinal);
        Assert.DoesNotContain("Curriculum document (verbatim):", prompt, StringComparison.Ordinal);
    }

    private static string CreatePlanResponseJson()
    {
        var planJson = """
{"name":"Databases","description":"Discipline description","labs":[{"orderIndex":1,"title":"Lab 1","initialDescription":"Lab description"}]}
""";

        return JsonSerializer.Serialize(new { response = planJson });
    }

    private sealed class RecordingHttpMessageHandler(string responseBody) : HttpMessageHandler
    {
        public string? LastRequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequestBody = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
            };
        }
    }
}
