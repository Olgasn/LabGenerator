using LabGenerator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabGenerator.Infrastructure.Data.Configurations;

public sealed class LlmProviderSettingsConfiguration : IEntityTypeConfiguration<LlmProviderSettings>
{
    public void Configure(EntityTypeBuilder<LlmProviderSettings> builder)
    {
        builder.ToTable("LlmProviderSettings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Provider)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Model)
            .HasMaxLength(200);

        builder.Property(x => x.ApiKey)
            .HasMaxLength(1000);

        builder.HasIndex(x => x.Provider)
            .IsUnique();
    }
}
