using Content.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Infrastructure.Migrations
{
    /// <summary>Moves the seven gallery videos the site kept in code into the database.</summary>
    public partial class SeedGallery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) => LegacyGallerySeed.Up(migrationBuilder);

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) => LegacyGallerySeed.Down(migrationBuilder);
    }
}
