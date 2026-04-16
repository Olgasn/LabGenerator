using LabGenerator.Infrastructure.Services;

namespace LabGenerator.Tests.Services;

public sealed class LlmPromptTemplateServiceTests
{
    private readonly LlmPromptTemplateService _svc = new();

    [Theory]
    [InlineData("master_assignment")]
    [InlineData("variant_generation")]
    [InlineData("constraints_check")]
    [InlineData("supplementary_material")]
    [InlineData("variant_judge")]
    [InlineData("variant_repair")]
    public void Render_ReturnsNonEmptyPrompts_ForAllPurposes(string purpose)
    {
        var vars = new Dictionary<string, string?>
        {
            ["lab_initial_description"] = "desc",
            ["master_assignment_excerpt"] = "master",
            ["master_requirements_block"] = "reqs",
            ["variation_profile_block"] = "",
            ["difficulty_requirements_block"] = "",
            ["variant_index"] = "1",
            ["variation_constraints_block"] = "",
            ["existing_variants_block"] = "",
            ["test_plan_warning_block"] = "",
            ["rejection_reason_block"] = "",
            ["candidate_title"] = "title",
            ["candidate_content_excerpt"] = "content",
            ["existing_variants_summary"] = "",
            ["difficulty_target_summary"] = "medium",
            ["master_assignment_markdown"] = "master md",
            ["variant_markdown"] = "variant md",
            ["issues_json"] = "[]",
            ["lab_title"] = "Lab 1",
            ["material_requirements_block"] = "",
            ["variants_count"] = "3",
            ["variants_summary"] = "summary"
        };

        var result = _svc.Render(purpose, vars);

        Assert.Equal(purpose, result.Purpose);
        Assert.False(string.IsNullOrWhiteSpace(result.SystemPrompt));
        Assert.False(string.IsNullOrWhiteSpace(result.UserPrompt));
    }

    [Fact]
    public void Render_SubstitutesVariables()
    {
        var vars = new Dictionary<string, string?>
        {
            ["master_assignment_markdown"] = "MASTER_CONTENT_HERE",
            ["variant_markdown"] = "VARIANT_CONTENT_HERE"
        };

        var result = _svc.Render("variant_judge", vars);

        Assert.Contains("MASTER_CONTENT_HERE", result.UserPrompt);
        Assert.Contains("VARIANT_CONTENT_HERE", result.UserPrompt);
        Assert.DoesNotContain("{{master_assignment_markdown}}", result.UserPrompt);
        Assert.DoesNotContain("{{variant_markdown}}", result.UserPrompt);
    }

    [Fact]
    public void Render_LeavesUnknownPlaceholders_WhenVariableMissing()
    {
        var vars = new Dictionary<string, string?>();

        var result = _svc.Render("variant_judge", vars);

        Assert.Contains("{{master_assignment_markdown}}", result.UserPrompt);
    }

    [Fact]
    public void Render_SubstitutesNullValueAsEmpty()
    {
        var vars = new Dictionary<string, string?>
        {
            ["master_assignment_markdown"] = null,
            ["variant_markdown"] = "text"
        };

        var result = _svc.Render("variant_judge", vars);

        Assert.DoesNotContain("{{master_assignment_markdown}}", result.UserPrompt);
        Assert.Contains("text", result.UserPrompt);
    }

    [Fact]
    public void Render_Throws_ForUnknownPurpose()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => _svc.Render("nonexistent_purpose", new Dictionary<string, string?>()));

        Assert.Contains("nonexistent_purpose", ex.Message);
    }

    [Fact]
    public void Render_Throws_ForEmptyPurpose()
    {
        Assert.Throws<InvalidOperationException>(
            () => _svc.Render("", new Dictionary<string, string?>()));
    }

    [Theory]
    [InlineData("master_assignment")]
    [InlineData("constraints_check")]
    [InlineData("supplementary_material")]
    [InlineData("variant_repair")]
    public void GetRetryUserPromptSuffix_ReturnsNull_ForPurposesWithoutRetry(string purpose)
    {
        var suffix = _svc.GetRetryUserPromptSuffix(purpose);

        Assert.Null(suffix);
    }

    [Theory]
    [InlineData("variant_generation")]
    [InlineData("variant_judge")]
    public void GetRetryUserPromptSuffix_ReturnsValue_ForPurposesWithRetry(string purpose)
    {
        var suffix = _svc.GetRetryUserPromptSuffix(purpose);

        Assert.False(string.IsNullOrWhiteSpace(suffix));
    }

    [Fact]
    public void Render_MasterAssignment_IncludesLabDescription()
    {
        var vars = new Dictionary<string, string?>
        {
            ["lab_initial_description"] = "UNIQUE_LAB_DESC",
            ["master_requirements_block"] = "REQS_BLOCK"
        };

        var result = _svc.Render("master_assignment", vars);

        Assert.Contains("UNIQUE_LAB_DESC", result.UserPrompt);
        Assert.Contains("REQS_BLOCK", result.UserPrompt);
    }

    [Fact]
    public void Render_VariantGeneration_IncludesMasterExcerpt()
    {
        var vars = new Dictionary<string, string?>
        {
            ["master_assignment_excerpt"] = "MASTER_EXCERPT_VALUE",
            ["variant_index"] = "42"
        };

        var result = _svc.Render("variant_generation", vars);

        Assert.Contains("MASTER_EXCERPT_VALUE", result.UserPrompt);
        Assert.Contains("42", result.UserPrompt);
    }
}
