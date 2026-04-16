using LabGenerator.Application.Models;

namespace LabGenerator.Tests.Models;

public sealed class ApplicationModelsTests
{
    [Fact]
    public void OllamaOptions_HaveExpectedDefaults()
    {
        var options = new OllamaOptions();

        Assert.Equal("LLM:Ollama", OllamaOptions.SectionName);
        Assert.Equal("https://ollama.com", options.BaseUrl);
        Assert.Equal(string.Empty, options.ApiKey);
        Assert.Equal("deepseek-v3.2:cloud", options.Model);
        Assert.Equal(0.2, options.Temperature);
        Assert.Equal(4096, options.MaxOutputTokens);
    }

    [Fact]
    public void LlmCompletionResult_StoresProvidedValues()
    {
        var result = new LLMCompletionResult(
            Provider: "ProviderA",
            Model: "ModelX",
            Text: "Generated text",
            PromptTokens: 100,
            CompletionTokens: 200,
            TotalTokens: 300,
            LatencyMs: 42,
            RawResponseJson: "{\"ok\":true}");

        Assert.Equal("ProviderA", result.Provider);
        Assert.Equal("ModelX", result.Model);
        Assert.Equal("Generated text", result.Text);
        Assert.Equal(100, result.PromptTokens);
        Assert.Equal(200, result.CompletionTokens);
        Assert.Equal(300, result.TotalTokens);
        Assert.Equal(42, result.LatencyMs);
        Assert.Equal("{\"ok\":true}", result.RawResponseJson);
    }
}
