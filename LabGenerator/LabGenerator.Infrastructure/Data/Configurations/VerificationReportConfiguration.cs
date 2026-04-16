using LabGenerator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabGenerator.Infrastructure.Data.Configurations;

public class VerificationReportConfiguration : IEntityTypeConfiguration<VerificationReport>
{
    public void Configure(EntityTypeBuilder<VerificationReport> builder)
    {
        builder.ToTable("VerificationReports");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.JudgeScoreJson)
            .IsRequired();

        builder.Property(x => x.IssuesJson)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasOne(x => x.AssignmentVariant)
            .WithMany()
            .HasForeignKey(x => x.AssignmentVariantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.AssignmentVariantId).IsUnique();
    }
}