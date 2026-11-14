using Content.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Infrastructure.Persistence.Migrations
{
    /// <summary>Moves the six home page questions the site kept in code into the database.</summary>
    public partial class SeedHomeFaq : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) => LegacyHomeFaqSeed.Up(migrationBuilder);

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) => LegacyHomeFaqSeed.Down(migrationBuilder);
    }
}
