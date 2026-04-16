using LabGenerator.Infrastructure.Services;
using LabGenerator.Tests.Helpers;

namespace LabGenerator.Tests.Services;

public sealed class PromptCustomSectionServiceTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsAllSectionsWithDefaults()
    {
        await using var db = TestDbContextFactory.Create();
        var svc = new PromptCustomSectionService(db);

        var sections = await svc.GetAllAsync(CancellationToken.None);

        Assert.Equal(2, sections.Count);
        Assert.Contains(sections, s => s.SectionKey == "master_requirements");
        Assert.Contains(sections, s => s.SectionKey == "material_requirements");
        Assert.All(sections, s =>
        {
            Assert.False(s.IsCustomized);
            Assert.Equal(s.DefaultContent, s.Content);
            Assert.Null(s.UpdatedAt);
        });
    }

    [Fact]
    public async Task GetAsync_ReturnsDefault_WhenNotCustomized()
    {
        await using var db = TestDbContextFactory.Create();
        var svc = new PromptCustomSectionService(db);

        var section = await svc.GetAsync("master_requirements", CancellationToken.None);

        Assert.Equal("master_requirements", section.SectionKey);
        Assert.False(section.IsCustomized);
        Assert.Equal(section.DefaultContent, section.Content);
        Assert.False(string.IsNullOrWhiteSpace(section.Content));
    }

    [Fact]
    public async Task UpdateAsync_SavesCustomContent()
    {
        await using var db = TestDbContextFactory.Create();
        var svc = new PromptCustomSectionService(db);

        var updated = await svc.UpdateAsync("master_requirements", "Custom requirements content", CancellationToken.None);

        Assert.True(updated.IsCustomized);
        Assert.Equal("Custom requirements content", updated.Content);
        Assert.NotNull(updated.UpdatedAt);
        Assert.NotEqual(updated.DefaultContent, updated.Content);
    }

    [Fact]
    public async Task UpdateAsync_OverwritesPreviousCustomization()
    {
        await using var db = TestDbContextFactory.Create();
        var svc = new PromptCustomSectionService(db);

        await svc.UpdateAsync("master_requirements", "First version", CancellationToken.None);
        var updated = await svc.UpdateAsync("master_requirements", "Second version", CancellationToken.None);

        Assert.True(updated.IsCustomized);
        Assert.Equal("Second version", updated.Content);
        Assert.Equal(1, db.PromptCustomSections.Count());
    }

    [Fact]
    public async Task ResetAsync_RestoresDefault()
    {
        await using var db = TestDbContextFactory.Create();
        var svc = new PromptCustomSectionService(db);

        await svc.UpdateAsync("master_requirements", "Custom content", CancellationToken.None);
        var reset = await svc.ResetAsync("master_requirements", CancellationToken.None);

        Assert.False(reset.IsCustomized);
        Assert.Equal(reset.DefaultContent, reset.Content);
        Assert.Equal(0, db.PromptCustomSections.Count());
    }

    [Fact]
    public async Task ResetAsync_IsIdempotent_WhenNotCustomized()
    {
        await using var db = TestDbContextFactory.Create();
        var svc = new PromptCustomSectionService(db);

        var reset = await svc.ResetAsync("master_requirements", CancellationToken.None);

        Assert.False(reset.IsCustomized);
        Assert.Equal(reset.DefaultContent, reset.Content);
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenContentEmpty()
    {
        await using var db = TestDbContextFactory.Create();
        var svc = new PromptCustomSectionService(db);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => svc.UpdateAsync("master_requirements", "", CancellationToken.None));
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenContentWhitespace()
    {
        await using var db = TestDbContextFactory.Create();
        var svc = new PromptCustomSectionService(db);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => svc.UpdateAsync("master_requirements", "   ", CancellationToken.None));
    }

    [Fact]
    public async Task GetAsync_Throws_WhenKeyUnknown()
    {
        await using var db = TestDbContextFactory.Create();
        var svc = new PromptCustomSectionService(db);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => svc.GetAsync("nonexistent_key", CancellationToken.None));
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenKeyUnknown()
    {
        await using var db = TestDbContextFactory.Create();
        var svc = new PromptCustomSectionService(db);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => svc.UpdateAsync("nonexistent_key", "content", CancellationToken.None));
    }

    [Fact]
    public async Task GetContentAsync_ReturnsDefault_WhenNotCustomized()
    {
        await using var db = TestDbContextFactory.Create();
        var svc = new PromptCustomSectionService(db);

        var content = await svc.GetContentAsync("material_requirements", CancellationToken.None);

        Assert.False(string.IsNullOrWhiteSpace(content));
    }

    [Fact]
    public async Task GetContentAsync_ReturnsCustom_WhenCustomized()
    {
        await using var db = TestDbContextFactory.Create();
        var svc = new PromptCustomSectionService(db);

        await svc.UpdateAsync("material_requirements", "My custom material reqs", CancellationToken.None);
        var content = await svc.GetContentAsync("material_requirements", CancellationToken.None);

        Assert.Equal("My custom material reqs", content);
    }

    [Fact]
    public void GetAllSections_ReturnsSectionInfoList()
    {
        using var db = TestDbContextFactory.Create();
        var svc = new PromptCustomSectionService(db);

        var sections = svc.GetAllSections();

        Assert.Equal(2, sections.Count);
        Assert.All(sections, s =>
        {
            Assert.False(string.IsNullOrWhiteSpace(s.SectionKey));
            Assert.False(string.IsNullOrWhiteSpace(s.DisplayName));
        });
    }
}
