using LabGenerator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabGenerator.Infrastructure.Data.Configurations;

public class MasterAssignmentConfiguration : IEntityTypeConfiguration<MasterAssignment>
{
    public void Configure(EntityTypeBuilder<MasterAssignment> builder)
    {
        builder.ToTable("MasterAssignments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Content)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.IsCurrent)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasOne(x => x.Lab)
            .WithMany(l => l.MasterAssignments)
            .HasForeignKey(x => x.LabId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.LabId)
            .IsUnique()
            .HasFilter("\"IsCurrent\" = TRUE");
        builder.HasIndex(x => new { x.LabId, x.Version }).IsUnique();
    }
}
