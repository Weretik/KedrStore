using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Content.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHomeBlocks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HomeBrands",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LogoMediaFileId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomeBrands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HomeFaqItems",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    QuestionUk = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    QuestionRu = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AnswerUk = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    AnswerRu = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomeFaqItems", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HomeBrands_Position",
                schema: "content",
                table: "HomeBrands",
                column: "Position");

            migrationBuilder.CreateIndex(
                name: "IX_HomeFaqItems_Position",
                schema: "content",
                table: "HomeFaqItems",
                column: "Position");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HomeBrands",
                schema: "content");

            migrationBuilder.DropTable(
                name: "HomeFaqItems",
                schema: "content");
        }
    }
}
