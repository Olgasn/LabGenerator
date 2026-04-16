using LabGenerator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabGenerator.Infrastructure.Data.Configurations;

public class AssignmentVariantConfiguration : IEntityTypeConfiguration<AssignmentVariant>
{
    public void Configure(EntityTypeBuilder<AssignmentVariant> builder)
    {
        builder.ToTable("AssignmentVariants");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(x => x.Content)
            .IsRequired();

        builder.Property(x => x.VariantParamsJson)
            .IsRequired();

        builder.Property(x => x.DifficultyProfileJson)
            .IsRequired();

        builder.Property(x => x.Fingerprint)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasOne(x => x.Lab)
            .WithMany(l => l.AssignmentVariants)
            .HasForeignKey(x => x.LabId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.LabId, x.VariantIndex }).IsUnique();
        builder.HasIndex(x => new { x.LabId, x.Fingerprint }).IsUnique();
    }
}
