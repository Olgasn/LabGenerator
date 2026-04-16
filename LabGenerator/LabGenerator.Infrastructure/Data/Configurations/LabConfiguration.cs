using LabGenerator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabGenerator.Infrastructure.Data.Configurations;

public class LabConfiguration : IEntityTypeConfiguration<Lab>
{
    public void Configure(EntityTypeBuilder<Lab> builder)
    {
        builder.ToTable("Labs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(x => x.InitialDescription)
            .IsRequired();

        builder.Property(x => x.OrderIndex)
            .IsRequired();

        builder.HasIndex(x => new { x.DisciplineId, x.OrderIndex }).IsUnique();

        builder.HasOne(x => x.Discipline)
            .WithMany(d => d.Labs)
            .HasForeignKey(x => x.DisciplineId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}