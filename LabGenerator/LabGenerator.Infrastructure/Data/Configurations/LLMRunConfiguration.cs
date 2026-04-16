using LabGenerator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabGenerator.Infrastructure.Data.Configurations;

public class LLMRunConfiguration : IEntityTypeConfiguration<LLMRun>
{
    public void Configure(EntityTypeBuilder<LLMRun> builder)
    {
        builder.ToTable("LLMRuns");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Provider)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Model)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Purpose)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.RequestJson)
            .IsRequired();

        builder.Property(x => x.ResponseText)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => new { x.Provider, x.Model, x.Purpose });
    }
}
