using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace LabGenerator.TeacherEmulator;

public sealed class LgApiClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public Task<DisciplineDto> CreateDisciplineAsync(CreateDisciplineRequest request, CancellationToken ct)
        => SendAsync<DisciplineDto>(HttpMethod.Post, "api/disciplines", request, ct);

    public Task<LabDto> CreateLabAsync(CreateLabRequest request, CancellationToken ct)
        => SendAsync<LabDto>(HttpMethod.Post, "api/labs", request, ct);

    public Task<GenerationJobDto> GenerateMasterAsync(int labId, CancellationToken ct)
        => SendAsync<GenerationJobDto>(HttpMethod.Post, $"api/labs/{labId}/master/generate", payload: null, ct);

    public Task<MasterAssignmentDto> GetCurrentMasterAsync(int labId, CancellationToken ct)
        => SendAsync<MasterAssignmentDto>(HttpMethod.Get, $"api/labs/{labId}/master", payload: null, ct);

    public Task<MasterAssignmentDto> UpdateMasterAsync(
        int labId,
        int masterAssignmentId,
        UpdateMasterAssignmentRequest request,
        CancellationToken ct)
        => SendAsync<MasterAssignmentDto>(HttpMethod.Put, $"api/labs/{labId}/master/{masterAssignmentId}", request, ct);

    public Task<MasterAssignmentDto> ApproveMasterAsync(int labId, int masterAssignmentId, CancellationToken ct)
        => SendAsync<MasterAssignmentDto>(HttpMethod.Post, $"api/labs/{labId}/master/{masterAssignmentId}/approve", payload: null, ct);

    public Task<List<VariationMethodDto>> GetVariationMethodsAsync(CancellationToken ct)
        => SendAsync<List<VariationMethodDto>>(HttpMethod.Get, "api/variation-methods", payload: null, ct);

    public Task<List<LabVariationMethodDto>> UpsertLabVariationMethodsAsync(
        int labId,
        UpsertLabVariationMethodsRequest request,
        CancellationToken ct)
        => SendAsync<List<LabVariationMethodDto>>(HttpMethod.Put, $"api/labs/{labId}/variation-methods", request, ct);

    public Task<List<VariationProfileDto>> GetVariationProfilesAsync(int labId, CancellationToken ct)
        => SendAsync<List<VariationProfileDto>>(HttpMethod.Get, $"api/labs/{labId}/variation-profiles", payload: null, ct);

    public Task<VariationProfileDto> CreateVariationProfileAsync(int labId, UpsertVariationProfileRequest request, CancellationToken ct)
        => SendAsync<VariationProfileDto>(HttpMethod.Post, $"api/labs/{labId}/variation-profiles", request, ct);

    public Task<LlmSettingsDto> GetLlmSettingsAsync(CancellationToken ct)
        => SendAsync<LlmSettingsDto>(HttpMethod.Get, "api/admin/llm-settings", payload: null, ct);

    public Task<LlmSettingsDto> UpdateLlmSettingsAsync(UpdateLlmSettingsRequest request, CancellationToken ct)
        => SendAsync<LlmSettingsDto>(HttpMethod.Put, "api/admin/llm-settings", request, ct);

    public Task<LlmProviderSettingsDto> UpdateLlmProviderSettingsAsync(
        string provider,
        UpdateLlmProviderSettingsRequest request,
        CancellationToken ct)
        => SendAsync<LlmProviderSettingsDto>(HttpMethod.Put, $"api/admin/llm-provider-settings/{provider}", request, ct);

    public Task<GenerationJobDto> GenerateVariantsAsync(int labId, GenerateVariantsRequest request, CancellationToken ct)
        => SendAsync<GenerationJobDto>(HttpMethod.Post, $"api/labs/{labId}/variants/generate", request, ct);

    public Task<List<AssignmentVariantDto>> GetVariantsAsync(int labId, CancellationToken ct)
        => SendAsync<List<AssignmentVariantDto>>(HttpMethod.Get, $"api/labs/{labId}/variants", payload: null, ct);

    public Task<GenerationJobDto> VerifyLabAsync(int labId, VerifyVariantsRequest request, CancellationToken ct)
        => SendAsync<GenerationJobDto>(HttpMethod.Post, $"api/labs/{labId}/verify", request, ct);

    public Task<List<VerificationReportDto>> GetLabVerificationReportsAsync(int labId, CancellationToken ct)
        => SendAsync<List<VerificationReportDto>>(HttpMethod.Get, $"api/labs/{labId}/verification-reports", payload: null, ct);

    public Task<GenerationJobDto> GetJobAsync(int jobId, CancellationToken ct)
        => SendAsync<GenerationJobDto>(HttpMethod.Get, $"api/jobs/{jobId}", payload: null, ct);

    private async Task<T> SendAsync<T>(HttpMethod method, string path, object? payload, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, path);
        if (payload is not null)
        {
            request.Content = JsonContent.Create(payload);
        }

        using var response = await httpClient.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"LG API call failed: {method} {path} -> {(int)response.StatusCode} {response.ReasonPhrase}. Body: {Trim(body, 2000)}");
        }

        if (typeof(T) == typeof(string))
        {
            return (T)(object)body;
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            throw new InvalidOperationException($"LG API call returned empty body: {method} {path}");
        }

        try
        {
            var parsed = JsonSerializer.Deserialize<T>(body, JsonOptions);
            if (parsed is null)
            {
                throw new InvalidOperationException($"Deserialized response is null for {method} {path}");
            }

            return parsed;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to parse LG API response for {method} {path}. Body: {Trim(body, 2000)}. Error: {ex.Message}",
                ex);
        }
    }

    private static string Trim(string text, int maxChars)
    {
        if (text.Length <= maxChars)
        {
            return text;
        }

        return new StringBuilder(text, 0, maxChars, maxChars + 16).Append("...").ToString();
    }
}
