# Database migration job — data model

No application entity or public DTO is added.

The operational state is EF Core's existing `__EFMigrationsHistory` table per database. Its invariants are:

- Catalog and Sales `MigrationId` values are immutable once applied to production.
- The approved Identity reset removes exactly the two obsolete int-key Identity rows before it establishes the Guid initial baseline.
- After the reset, the ordered history must correspond to the source migration lineage for every context.

The job stores no connection strings, credentials, or migration payloads.
