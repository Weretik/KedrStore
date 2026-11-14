using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scheduling.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialScheduling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "scheduling");

            migrationBuilder.CreateTable(
                name: "ScheduledTaskRuns",
                schema: "scheduling",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Trigger = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RequestedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ClaimedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FinishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Outcome = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledTaskRuns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledTasks",
                schema: "scheduling",
                columns: table => new
                {
                    Key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    Schedule = table.Column<string>(type: "jsonb", nullable: false),
                    NextRunAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    State = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CurrentRunId = table.Column<Guid>(type: "uuid", nullable: true),
                    RunningSince = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RunRequestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RunRequestedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LastSuccessAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastSuccessSummary = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LastFailureAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastFailureReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledTasks", x => x.Key);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledTaskRuns_ClaimedAt",
                schema: "scheduling",
                table: "ScheduledTaskRuns",
                column: "ClaimedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledTaskRuns_TaskKey_ClaimedAt",
                schema: "scheduling",
                table: "ScheduledTaskRuns",
                columns: new[] { "TaskKey", "ClaimedAt" },
                descending: new[] { false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduledTaskRuns",
                schema: "scheduling");

            migrationBuilder.DropTable(
                name: "ScheduledTasks",
                schema: "scheduling");
        }
    }
}
