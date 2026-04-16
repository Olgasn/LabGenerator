using LabGenerator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabGenerator.Infrastructure.Data.Configurations;

public sealed class LlmSettingsConfiguration : IEntityTypeConfiguration<LlmSettings>
{
    public void Configure(EntityTypeBuilder<LlmSettings> builder)
    {
        builder.ToTable("LlmSettings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Provider)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Model)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.UpdatedAt)
            .IsRequired();
    }
}
