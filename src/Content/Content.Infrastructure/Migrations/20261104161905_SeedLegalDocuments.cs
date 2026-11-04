using Content.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Infrastructure.Migrations
{
    /// <summary>Moves the offer, privacy, returns and delivery texts the site kept in code into the database.</summary>
    public partial class SeedLegalDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) => LegacyLegalDocumentSeed.Up(migrationBuilder);

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) => LegacyLegalDocumentSeed.Down(migrationBuilder);
    }
}
