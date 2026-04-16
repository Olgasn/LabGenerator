using LabGenerator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabGenerator.Infrastructure.Data.Configurations;

public class VariationProfileConfiguration : IEntityTypeConfiguration<VariationProfile>
{
    public void Configure(EntityTypeBuilder<VariationProfile> builder)
    {
        builder.ToTable("VariationProfiles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.ParametersJson)
            .IsRequired();

        builder.Property(x => x.DifficultyRubricJson)
            .IsRequired();

        builder.Property(x => x.IsDefault)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasOne(x => x.Lab)
            .WithMany(l => l.VariationProfiles)
            .HasForeignKey(x => x.LabId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.LabId, x.IsDefault });
    }
}
