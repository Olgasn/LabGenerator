using LabGenerator.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LabGenerator.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Discipline> Disciplines { get; set; }
    public DbSet<Lab> Labs { get; set; }

    public DbSet<VariationMethod> VariationMethods { get; set; }
    public DbSet<LabVariationMethod> LabVariationMethods { get; set; }

    public DbSet<MasterAssignment> MasterAssignments { get; set; }
    public DbSet<VariationProfile> VariationProfiles { get; set; }
    public DbSet<AssignmentVariant> AssignmentVariants { get; set; }
    public DbSet<LabSupplementaryMaterial> LabSupplementaryMaterials { get; set; }
    public DbSet<VerificationReport> VerificationReports { get; set; }
    public DbSet<LLMRun> LLMRuns { get; set; }
    public DbSet<GenerationJob> GenerationJobs { get; set; }

    public DbSet<LlmSettings> LlmSettings { get; set; }

    public DbSet<LlmProviderSettings> LlmProviderSettings { get; set; }

    public DbSet<PromptCustomSection> PromptCustomSections { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
