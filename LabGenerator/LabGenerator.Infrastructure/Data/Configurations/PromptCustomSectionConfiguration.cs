using LabGenerator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabGenerator.Infrastructure.Data.Configurations;

public sealed class PromptCustomSectionConfiguration : IEntityTypeConfiguration<PromptCustomSection>
{
    public void Configure(EntityTypeBuilder<PromptCustomSection> builder)
    {
        builder.ToTable("PromptCustomSections");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.SectionKey)
            .IsUnique();

        builder.Property(x => x.SectionKey)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Content)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();
    }
}
