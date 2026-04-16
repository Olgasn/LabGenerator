using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabGenerator.Infrastructure.Data.Migrations
{
    public partial class AddLabSupplementaryMaterials : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LabSupplementaryMaterials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LabId = table.Column<int>(type: "INTEGER", nullable: false),
                    TheoryMarkdown = table.Column<string>(type: "TEXT", nullable: false),
                    ControlQuestionsJson = table.Column<string>(type: "TEXT", nullable: false),
                    SourceFingerprint = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabSupplementaryMaterials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabSupplementaryMaterials_Labs_LabId",
                        column: x => x.LabId,
                        principalTable: "Labs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LabSupplementaryMaterials_LabId",
                table: "LabSupplementaryMaterials",
                column: "LabId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LabSupplementaryMaterials");
        }
    }
}
