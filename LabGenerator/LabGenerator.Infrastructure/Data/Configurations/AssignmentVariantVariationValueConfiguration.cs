using LabGenerator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabGenerator.Infrastructure.Data.Configurations;

public class AssignmentVariantVariationValueConfiguration : IEntityTypeConfiguration<AssignmentVariantVariationValue>
{
    public void Configure(EntityTypeBuilder<AssignmentVariantVariationValue> builder)
    {
        builder.ToTable("AssignmentVariantVariationValues");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Value)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasOne(x => x.AssignmentVariant)
            .WithMany()
            .HasForeignKey(x => x.AssignmentVariantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.VariationMethod)
            .WithMany()
            .HasForeignKey(x => x.VariationMethodId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.AssignmentVariantId, x.VariationMethodId }).IsUnique();
        builder.HasIndex(x => new { x.VariationMethodId, x.Value });
    }
}