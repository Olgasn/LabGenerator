using LabGenerator.Domain.Entities;
using LabGenerator.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace LabGenerator.Tests.Data;

public sealed class AssignmentVariantConfigurationTests
{
    [Fact]
    public void Model_HasExpectedAssignmentVariantConstraints()
    {
        using var db = TestDbContextFactory.Create();
        var entityType = db.Model.FindEntityType(typeof(AssignmentVariant));

        Assert.NotNull(entityType);
        Assert.Equal("AssignmentVariants", entityType!.GetTableName());

        var title = entityType.FindProperty(nameof(AssignmentVariant.Title));
        Assert.NotNull(title);
        Assert.False(title!.IsNullable);
        Assert.Equal(250, title.GetMaxLength());

        var content = entityType.FindProperty(nameof(AssignmentVariant.Content));
        Assert.NotNull(content);
        Assert.False(content!.IsNullable);

        var variantParams = entityType.FindProperty(nameof(AssignmentVariant.VariantParamsJson));
        Assert.NotNull(variantParams);
        Assert.False(variantParams!.IsNullable);

        var difficulty = entityType.FindProperty(nameof(AssignmentVariant.DifficultyProfileJson));
        Assert.NotNull(difficulty);
        Assert.False(difficulty!.IsNullable);

        var fingerprint = entityType.FindProperty(nameof(AssignmentVariant.Fingerprint));
        Assert.NotNull(fingerprint);
        Assert.False(fingerprint!.IsNullable);
        Assert.Equal(200, fingerprint.GetMaxLength());

        var createdAt = entityType.FindProperty(nameof(AssignmentVariant.CreatedAt));
        Assert.NotNull(createdAt);
        Assert.False(createdAt!.IsNullable);

        Assert.Contains(
            entityType.GetIndexes(),
            index => index.IsUnique
                     && index.Properties.Select(x => x.Name)
                         .SequenceEqual([nameof(AssignmentVariant.LabId), nameof(AssignmentVariant.VariantIndex)]));

        Assert.Contains(
            entityType.GetIndexes(),
            index => index.IsUnique
                     && index.Properties.Select(x => x.Name)
                         .SequenceEqual([nameof(AssignmentVariant.LabId), nameof(AssignmentVariant.Fingerprint)]));

        var fk = Assert.Single(entityType.GetForeignKeys());
        Assert.Equal(nameof(AssignmentVariant.LabId), fk.Properties.Single().Name);
        Assert.Equal(DeleteBehavior.Cascade, fk.DeleteBehavior);
        Assert.Equal(typeof(Lab), fk.PrincipalEntityType.ClrType);
    }
}
