using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Work.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialWork : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "work");

            migrationBuilder.CreateTable(
                name: "WorkActivities",
                schema: "work",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Area = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Operation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Method = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Target = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Count = table.Column<int>(type: "integer", nullable: false),
                    FirstAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkActivities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkTasks",
                schema: "work",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Details = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Result = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkTasks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkActivities_UserId_OccurredAt",
                schema: "work",
                table: "WorkActivities",
                columns: new[] { "UserId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkTasks_OwnerUserId_CompletedAt",
                schema: "work",
                table: "WorkTasks",
                columns: new[] { "OwnerUserId", "CompletedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkTasks_OwnerUserId_Status",
                schema: "work",
                table: "WorkTasks",
                columns: new[] { "OwnerUserId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkActivities",
                schema: "work");

            migrationBuilder.DropTable(
                name: "WorkTasks",
                schema: "work");
        }
    }
}
