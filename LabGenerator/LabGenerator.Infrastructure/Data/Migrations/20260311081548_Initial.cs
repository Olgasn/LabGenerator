using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LabGenerator.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Disciplines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Disciplines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GenerationJobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    DisciplineId = table.Column<int>(type: "INTEGER", nullable: true),
                    LabId = table.Column<int>(type: "INTEGER", nullable: true),
                    MasterAssignmentId = table.Column<int>(type: "INTEGER", nullable: true),
                    VariationProfileId = table.Column<int>(type: "INTEGER", nullable: true),
                    PayloadJson = table.Column<string>(type: "TEXT", nullable: true),
                    ResultJson = table.Column<string>(type: "TEXT", nullable: true),
                    Error = table.Column<string>(type: "TEXT", nullable: true),
                    Progress = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    FinishedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GenerationJobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LLMRuns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Provider = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Model = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Purpose = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    RequestJson = table.Column<string>(type: "TEXT", nullable: false),
                    ResponseText = table.Column<string>(type: "TEXT", nullable: false),
                    PromptTokens = table.Column<int>(type: "INTEGER", nullable: true),
                    CompletionTokens = table.Column<int>(type: "INTEGER", nullable: true),
                    TotalTokens = table.Column<int>(type: "INTEGER", nullable: true),
                    LatencyMs = table.Column<int>(type: "INTEGER", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LLMRuns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LlmSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Provider = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Model = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LlmSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VariationMethods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    IsSystem = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VariationMethods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Labs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrderIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    InitialDescription = table.Column<string>(type: "TEXT", nullable: false),
                    DisciplineId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Labs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Labs_Disciplines_DisciplineId",
                        column: x => x.DisciplineId,
                        principalTable: "Disciplines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssignmentVariants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LabId = table.Column<int>(type: "INTEGER", nullable: false),
                    VariantIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    VariantParamsJson = table.Column<string>(type: "TEXT", nullable: false),
                    DifficultyProfileJson = table.Column<string>(type: "TEXT", nullable: false),
                    Fingerprint = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentVariants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssignmentVariants_Labs_LabId",
                        column: x => x.LabId,
                        principalTable: "Labs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LabVariationMethods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LabId = table.Column<int>(type: "INTEGER", nullable: false),
                    VariationMethodId = table.Column<int>(type: "INTEGER", nullable: false),
                    PreserveAcrossLabs = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabVariationMethods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabVariationMethods_Labs_LabId",
                        column: x => x.LabId,
                        principalTable: "Labs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LabVariationMethods_VariationMethods_VariationMethodId",
                        column: x => x.VariationMethodId,
                        principalTable: "VariationMethods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MasterAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LabId = table.Column<int>(type: "INTEGER", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false),
                    IsCurrent = table.Column<bool>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasterAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MasterAssignments_Labs_LabId",
                        column: x => x.LabId,
                        principalTable: "Labs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VariationProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LabId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ParametersJson = table.Column<string>(type: "TEXT", nullable: false),
                    DifficultyRubricJson = table.Column<string>(type: "TEXT", nullable: false),
                    IsDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VariationProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VariationProfiles_Labs_LabId",
                        column: x => x.LabId,
                        principalTable: "Labs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssignmentVariantVariationValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AssignmentVariantId = table.Column<int>(type: "INTEGER", nullable: false),
                    VariationMethodId = table.Column<int>(type: "INTEGER", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentVariantVariationValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssignmentVariantVariationValues_AssignmentVariants_AssignmentVariantId",
                        column: x => x.AssignmentVariantId,
                        principalTable: "AssignmentVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssignmentVariantVariationValues_VariationMethods_VariationMethodId",
                        column: x => x.VariationMethodId,
                        principalTable: "VariationMethods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VerificationReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AssignmentVariantId = table.Column<int>(type: "INTEGER", nullable: false),
                    Passed = table.Column<bool>(type: "INTEGER", nullable: false),
                    JudgeScoreJson = table.Column<string>(type: "TEXT", nullable: false),
                    IssuesJson = table.Column<string>(type: "TEXT", nullable: false),
                    JudgeRunId = table.Column<int>(type: "INTEGER", nullable: true),
                    SolverRunId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerificationReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VerificationReports_AssignmentVariants_AssignmentVariantId",
                        column: x => x.AssignmentVariantId,
                        principalTable: "AssignmentVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "VariationMethods",
                columns: new[] { "Id", "Code", "CreatedAt", "Description", "IsSystem", "Name" },
                values: new object[,]
                {
                    { 1, "subject_domain", new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, true, "Предметная область" },
                    { 2, "input_data_sets", new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, true, "Наборы входных данных" },
                    { 3, "output_format", new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, true, "Формат выходных данных (результатов)" },
                    { 4, "algorithmic_requirements", new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, true, "Алгоритмические требования" },
                    { 5, "resource_constraints", new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, true, "Ограничения на ресурсы и модули" },
                    { 6, "tech_stack", new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, true, "Стек инструментов и технологий" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentVariants_LabId_Fingerprint",
                table: "AssignmentVariants",
                columns: new[] { "LabId", "Fingerprint" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentVariants_LabId_VariantIndex",
                table: "AssignmentVariants",
                columns: new[] { "LabId", "VariantIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentVariantVariationValues_AssignmentVariantId_VariationMethodId",
                table: "AssignmentVariantVariationValues",
                columns: new[] { "AssignmentVariantId", "VariationMethodId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentVariantVariationValues_VariationMethodId_Value",
                table: "AssignmentVariantVariationValues",
                columns: new[] { "VariationMethodId", "Value" });

            migrationBuilder.CreateIndex(
                name: "IX_GenerationJobs_LabId_CreatedAt",
                table: "GenerationJobs",
                columns: new[] { "LabId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_GenerationJobs_Status_CreatedAt",
                table: "GenerationJobs",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Labs_DisciplineId_OrderIndex",
                table: "Labs",
                columns: new[] { "DisciplineId", "OrderIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LabVariationMethods_LabId_VariationMethodId",
                table: "LabVariationMethods",
                columns: new[] { "LabId", "VariationMethodId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LabVariationMethods_VariationMethodId",
                table: "LabVariationMethods",
                column: "VariationMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_LLMRuns_Provider_Model_Purpose",
                table: "LLMRuns",
                columns: new[] { "Provider", "Model", "Purpose" });

            migrationBuilder.CreateIndex(
                name: "IX_MasterAssignments_LabId",
                table: "MasterAssignments",
                column: "LabId",
                unique: true,
                filter: "\"IsCurrent\" = TRUE");

            migrationBuilder.CreateIndex(
                name: "IX_MasterAssignments_LabId_Version",
                table: "MasterAssignments",
                columns: new[] { "LabId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VariationMethods_Code",
                table: "VariationMethods",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VariationProfiles_LabId_IsDefault",
                table: "VariationProfiles",
                columns: new[] { "LabId", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "IX_VerificationReports_AssignmentVariantId",
                table: "VerificationReports",
                column: "AssignmentVariantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssignmentVariantVariationValues");

            migrationBuilder.DropTable(
                name: "GenerationJobs");

            migrationBuilder.DropTable(
                name: "LabVariationMethods");

            migrationBuilder.DropTable(
                name: "LLMRuns");

            migrationBuilder.DropTable(
                name: "LlmSettings");

            migrationBuilder.DropTable(
                name: "MasterAssignments");

            migrationBuilder.DropTable(
                name: "VariationProfiles");

            migrationBuilder.DropTable(
                name: "VerificationReports");

            migrationBuilder.DropTable(
                name: "VariationMethods");

            migrationBuilder.DropTable(
                name: "AssignmentVariants");

            migrationBuilder.DropTable(
                name: "Labs");

            migrationBuilder.DropTable(
                name: "Disciplines");
        }
    }
}
