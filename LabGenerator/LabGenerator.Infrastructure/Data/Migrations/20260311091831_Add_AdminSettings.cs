using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabGenerator.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_AdminSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LlmProviderSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Provider = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Model = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Temperature = table.Column<double>(type: "REAL", nullable: true),
                    MaxOutputTokens = table.Column<int>(type: "INTEGER", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LlmProviderSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LlmProviderSettings_Provider",
                table: "LlmProviderSettings",
                column: "Provider",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LlmProviderSettings");
        }
    }
}
