using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Work.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class WorkTaskStepsAndDueDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "DueDate",
                schema: "work",
                table: "WorkTasks",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Steps",
                schema: "work",
                table: "WorkTasks",
                type: "jsonb",
                nullable: true);

            // Tasks made before steps existed read as having none.
            migrationBuilder.Sql("UPDATE work.\"WorkTasks\" SET \"Steps\" = '[]'::jsonb WHERE \"Steps\" IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DueDate",
                schema: "work",
                table: "WorkTasks");

            migrationBuilder.DropColumn(
                name: "Steps",
                schema: "work",
                table: "WorkTasks");
        }
    }
}
