using LabGenerator.TeacherEmulator;

namespace LabGenerator.Tests.Services;

public sealed class TeacherEmulatorOptionsTests
{
    [Fact]
    public void FromEnvironment_UsesSixVariantsByDefault()
    {
        var options = TeacherEmulatorOptions.FromEnvironment([], Directory.GetCurrentDirectory());

        Assert.Equal(6, options.VariantCountPerLab);
    }

    [Fact]
    public void FromEnvironment_AcceptsVariantCountForTestPlanLaunch()
    {
        var options = TeacherEmulatorOptions.FromEnvironment(
            ["--test-plan", "--variant-count=9"],
            Directory.GetCurrentDirectory());

        Assert.Equal(9, options.VariantCountPerLab);
    }
}
