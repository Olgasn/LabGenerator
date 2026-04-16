using System;
using LabGenerator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabGenerator.Infrastructure.Data.Configurations;

public class VariationMethodConfiguration : IEntityTypeConfiguration<VariationMethod>
{
    public void Configure(EntityTypeBuilder<VariationMethod> builder)
    {
        builder.ToTable("VariationMethods");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.IsSystem)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => x.Code).IsUnique();

        builder.HasData(
            new VariationMethod
            {
                Id = 1,
                Code = "subject_domain",
                Name = "Предметная область",
                Description = "Например: Магазин, Больница, Кинотеатр, Геометрические фигуры, Библиотека, Игра, Персонажи и т.п.",
                IsSystem = true,
                CreatedAt = DateTimeOffset.UnixEpoch
            },
            new VariationMethod
            {
                Id = 2,
                Code = "input_data_sets",
                Name = "Наборы входных данных",
                Description = null,
                IsSystem = true,
                CreatedAt = DateTimeOffset.UnixEpoch
            },
            new VariationMethod
            {
                Id = 3,
                Code = "output_format",
                Name = "Формат выходных данных (результатов)",
                Description = null,
                IsSystem = true,
                CreatedAt = DateTimeOffset.UnixEpoch
            },
            new VariationMethod
            {
                Id = 4,
                Code = "algorithmic_requirements",
                Name = "Алгоритмические требования",
                Description = "Разные алгоритмические подходы или структуры, например: линейный поиск, бинарный поиск, хеширование, дерево, стек, таблица и т.п.",
                IsSystem = true,
                CreatedAt = DateTimeOffset.UnixEpoch
            },
            new VariationMethod
            {
                Id = 5,
                Code = "resource_constraints",
                Name = "Ограничения на ресурсы и модули",
                Description = null,
                IsSystem = true,
                CreatedAt = DateTimeOffset.UnixEpoch
            },
            new VariationMethod
            {
                Id = 6,
                Code = "tech_stack",
                Name = "Стек инструментов и технологий",
                Description = null,
                IsSystem = true,
                CreatedAt = DateTimeOffset.UnixEpoch
            }
        );
    }
}
