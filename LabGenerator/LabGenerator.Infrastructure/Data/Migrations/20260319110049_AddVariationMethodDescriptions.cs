using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabGenerator.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVariationMethodDescriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "VariationMethods",
                keyColumn: "Id",
                keyValue: 1,
                column: "Description",
                value: "Например: Магазин, Больница, Кинотеатр, Геометрические фигуры, Библиотека, Игра, Персонажи и т.п.");

            migrationBuilder.UpdateData(
                table: "VariationMethods",
                keyColumn: "Id",
                keyValue: 4,
                column: "Description",
                value: "Разные алгоритмические подходы или структуры, например: линейный поиск, бинарный поиск, хеширование, дерево, стек, таблица и т.п.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "VariationMethods",
                keyColumn: "Id",
                keyValue: 1,
                column: "Description",
                value: null);

            migrationBuilder.UpdateData(
                table: "VariationMethods",
                keyColumn: "Id",
                keyValue: 4,
                column: "Description",
                value: null);
        }
    }
}
