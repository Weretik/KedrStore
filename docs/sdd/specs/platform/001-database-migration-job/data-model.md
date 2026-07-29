# Database migration job — data model

No application entity or public DTO is added.

The operational state is EF Core's existing `__EFMigrationsHistory` table per database. Its invariants are:

- `MigrationId` is immutable once applied to production.
- The ordered history must correspond to the source migration lineage for that context.
- A migration job may append a successfully applied forward migration; it must never rewrite historical rows.

The job stores no connection strings, credentials, or migration payloads.
