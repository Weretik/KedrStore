using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scheduling.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DropLegacyHangfireTables : Migration
    {
        /// <summary>
        /// Hangfire ran background jobs before the scheduler tick (ADR 0009) and no code uses it now. Its tables stay
        /// only in databases created back then, in the <c>hangfire</c> schema. The owner chose to remove them
        /// (2026-11-15). The drop is guarded, so a database without them is left as it is.
        /// </summary>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TABLE IF EXISTS hangfire."jobparameter", hangfire."jobqueue", hangfire."state",
                    hangfire."job", hangfire."counter", hangfire."aggregatedcounter", hangfire."hash",
                    hangfire."list", hangfire."set", hangfire."lock", hangfire."server", hangfire."schema" CASCADE;
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM pg_namespace WHERE nspname = 'hangfire')
                       AND NOT EXISTS (SELECT 1 FROM pg_class c JOIN pg_namespace n ON n.oid = c.relnamespace
                                       WHERE n.nspname = 'hangfire') THEN
                        DROP SCHEMA hangfire;
                    END IF;
                END $$;
                """);
        }

        /// <summary>The Hangfire tables and their history are gone for good; there is nothing to restore.</summary>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
