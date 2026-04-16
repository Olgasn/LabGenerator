using LabGenerator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabGenerator.Infrastructure.Data.Configurations;

public class LabSupplementaryMaterialConfiguration : IEntityTypeConfiguration<LabSupplementaryMaterial>
{
    public void Configure(EntityTypeBuilder<LabSupplementaryMaterial> builder)
    {
        builder.ToTable("LabSupplementaryMaterials");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TheoryMarkdown)
            .IsRequired();

        builder.Property(x => x.ControlQuestionsJson)
            .IsRequired();

        builder.Property(x => x.SourceFingerprint)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasOne(x => x.Lab)
            .WithMany(x => x.SupplementaryMaterials)
            .HasForeignKey(x => x.LabId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.LabId)
            .IsUnique();
    }
}
