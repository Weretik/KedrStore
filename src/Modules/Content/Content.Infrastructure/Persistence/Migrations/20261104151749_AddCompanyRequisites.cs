using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyRequisites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompanyRequisites",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    LegalName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    RegistrationExtractNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Iban = table.Column<string>(type: "character varying(29)", maxLength: 29, nullable: false),
                    BankName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    BankCode = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyRequisites", x => x.Id);
                    table.CheckConstraint("CK_CompanyRequisites_Singleton", "\"Id\" = 1");
                });

            migrationBuilder.CreateTable(
                name: "CompanyRequisitesChanges",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    ChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ChangedByUserId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ChangedByName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Changes = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyRequisitesChanges", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyRequisitesChanges_ChangedAt",
                schema: "content",
                table: "CompanyRequisitesChanges",
                column: "ChangedAt");

            // The values the site kept in code (owner-confirmed 2026-11-03); later edits are logged.
            migrationBuilder.InsertData(
                schema: "content",
                table: "CompanyRequisites",
                columns: ["Id", "LegalName", "Code", "RegistrationExtractNumber", "Iban", "BankName", "BankCode", "UpdatedAt"],
                values: new object[]
                {
                    1, "ФОП Овчинников Максим Володимирович", "2689400771", "24800170000032534",
                    "UA303052990000026007025905216", "ПриватБанк", "305299",
                    new DateTimeOffset(2026, 11, 4, 0, 0, 0, TimeSpan.Zero)
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanyRequisites",
                schema: "content");

            migrationBuilder.DropTable(
                name: "CompanyRequisitesChanges",
                schema: "content");
        }
    }
}
