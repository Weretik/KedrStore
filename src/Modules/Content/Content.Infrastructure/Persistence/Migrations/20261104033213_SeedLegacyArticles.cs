using Content.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Infrastructure.Persistence.Migrations
{
    /// <summary>Moves the six articles the site kept in code into the database, with their covers.</summary>
    public partial class SeedLegacyArticles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) => LegacyArticleSeed.Up(migrationBuilder);

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) => LegacyArticleSeed.Down(migrationBuilder);
    }
}
