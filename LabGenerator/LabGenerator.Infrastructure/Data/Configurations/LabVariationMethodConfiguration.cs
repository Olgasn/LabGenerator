using LabGenerator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabGenerator.Infrastructure.Data.Configurations;

public class LabVariationMethodConfiguration : IEntityTypeConfiguration<LabVariationMethod>
{
    public void Configure(EntityTypeBuilder<LabVariationMethod> builder)
    {
        builder.ToTable("LabVariationMethods");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PreserveAcrossLabs)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasOne(x => x.Lab)
            .WithMany()
            .HasForeignKey(x => x.LabId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.VariationMethod)
            .WithMany()
            .HasForeignKey(x => x.VariationMethodId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.LabId, x.VariationMethodId }).IsUnique();
    }
}