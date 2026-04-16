using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabGenerator.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_PromptCustomSections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PromptCustomSections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SectionKey = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromptCustomSections", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PromptCustomSections_SectionKey",
                table: "PromptCustomSections",
                column: "SectionKey",
                unique: true);

            migrationBuilder.DropTable(
                name: "LlmPromptTemplates");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PromptCustomSections");

            migrationBuilder.CreateTable(
                name: "LlmPromptTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Purpose = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    SystemPromptTemplate = table.Column<string>(type: "TEXT", nullable: false),
                    UserPromptTemplate = table.Column<string>(type: "TEXT", nullable: false),
                    RetryUserPromptSuffixTemplate = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LlmPromptTemplates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LlmPromptTemplates_Purpose",
                table: "LlmPromptTemplates",
                column: "Purpose",
                unique: true);
        }
    }
}
