using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImageProcessor.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class MoveOperationsToImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Operations",
                table: "Images",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'[]'::jsonb");

            migrationBuilder.Sql(
                """
                UPDATE "Images" AS i
                SET "Operations" = COALESCE(b."Operations", '[]'::jsonb)
                FROM "Batches" AS b
                WHERE b."Id" = i."BatchId";
                """);

            migrationBuilder.DropColumn(
                name: "Operations",
                table: "Batches");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Operations",
                table: "Batches",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'[]'::jsonb");

            migrationBuilder.Sql(
                """
                UPDATE "Batches" AS b
                SET "Operations" = COALESCE(i."Operations", '[]'::jsonb)
                FROM (
                    SELECT DISTINCT ON ("BatchId") "BatchId", "Operations"
                    FROM "Images"
                    ORDER BY "BatchId", "Id"
                ) AS i
                WHERE b."Id" = i."BatchId";
                """);

            migrationBuilder.DropColumn(
                name: "Operations",
                table: "Images");
        }
    }
}
