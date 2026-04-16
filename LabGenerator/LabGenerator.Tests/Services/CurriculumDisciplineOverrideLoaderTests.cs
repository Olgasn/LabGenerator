using LabGenerator.TeacherEmulator;

namespace LabGenerator.Tests.Services;

public sealed class CurriculumDisciplineOverrideLoaderTests : IDisposable
{
    private readonly string tempDirectory = Path.Combine(
        Path.GetTempPath(),
        "labgenerator-curriculum-tests",
        Guid.NewGuid().ToString("N"));

    [Fact]
    public void TryLoad_ReturnsOverride_WhenCurriculumFileExistsAndHasContent()
    {
        Directory.CreateDirectory(Path.Combine(tempDirectory, "curriculums"));
        var curriculumPath = Path.Combine(tempDirectory, "curriculums", "curriculum1.md");
        var content = "## DATABASES\r\nDiscipline description\r\n";
        File.WriteAllText(curriculumPath, content);

        var result = CurriculumDisciplineOverrideLoader.TryLoad(tempDirectory, Path.Combine(tempDirectory, "bin"));

        Assert.NotNull(result);
        Assert.Equal("DATABASES", result.Name);
        Assert.Equal(content, result.Description);
        Assert.Equal(Path.GetFullPath(curriculumPath), result.Path);
    }

    [Fact]
    public void TryLoad_ReturnsNull_WhenCurriculumFileIsMissing()
    {
        var result = CurriculumDisciplineOverrideLoader.TryLoad(tempDirectory, Path.Combine(tempDirectory, "bin"));

        Assert.Null(result);
    }

    [Fact]
    public void TryLoad_ReturnsNull_WhenCurriculumFileIsEmpty()
    {
        Directory.CreateDirectory(Path.Combine(tempDirectory, "curriculums"));
        File.WriteAllText(Path.Combine(tempDirectory, "curriculums", "curriculum1.md"), "  \r\n\t");

        var result = CurriculumDisciplineOverrideLoader.TryLoad(tempDirectory, Path.Combine(tempDirectory, "bin"));

        Assert.Null(result);
    }

    [Fact]
    public void TryLoad_ReturnsNull_WhenSourceProjectExistsButCurriculumIsMissing()
    {
        var sourceProjectDirectory = Path.Combine(tempDirectory, "LabGenerator.TeacherEmulator");
        Directory.CreateDirectory(sourceProjectDirectory);
        File.WriteAllText(
            Path.Combine(sourceProjectDirectory, "LabGenerator.TeacherEmulator.csproj"),
            "<Project Sdk=\"Microsoft.NET.Sdk\" />");

        var appBaseDirectory = Path.Combine(tempDirectory, "runtime");
        Directory.CreateDirectory(Path.Combine(appBaseDirectory, "curriculums"));
        File.WriteAllText(
            Path.Combine(appBaseDirectory, "curriculums", "curriculum1.md"),
            "## Stale copy\r\nOld content");

        var result = CurriculumDisciplineOverrideLoader.TryLoad(tempDirectory, appBaseDirectory);

        Assert.Null(result);
    }

    [Fact]
    public void TryLoad_UsesAppBaseDirectory_WhenSourceProjectIsUnavailable()
    {
        var appBaseDirectory = Path.Combine(tempDirectory, "runtime");
        Directory.CreateDirectory(Path.Combine(appBaseDirectory, "curriculums"));
        var curriculumPath = Path.Combine(appBaseDirectory, "curriculums", "curriculum1.md");
        var content = "## DATABASES\r\nRuntime copy description\r\n";
        File.WriteAllText(curriculumPath, content);

        var result = CurriculumDisciplineOverrideLoader.TryLoad(
            Path.Combine(tempDirectory, "working-dir"),
            appBaseDirectory);

        Assert.NotNull(result);
        Assert.Equal("DATABASES", result.Name);
        Assert.Equal(content, result.Description);
        Assert.Equal(Path.GetFullPath(curriculumPath), result.Path);
    }

    public void Dispose()
    {
        if (Directory.Exists(tempDirectory))
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }
}
