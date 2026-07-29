# Database migration job — infrastructure and rollout

## Migration lineage

Production Identity history contains `20260118230934_InitialAppIdentityDbContext` and `20260507141045_AddRefreshSessionStore`. Restore their source migration files and the matching snapshot from Git history before generating any new Identity migration. The currently source-only replacement ID `20260529142739_InitialAppIdentityDbContext` must not be applied to a database that already owns the Identity tables.

After restoring the lineage, create a forward migration for differences between the restored snapshot and the current Identity model. Review generated SQL against a disposable copy of the production schema before rollout.

## Job contract

Add `--job=migrate` to `Host.Jobs/Program.cs`. It invokes a dedicated migration orchestration service in the `Host.Jobs` root project; the service resolves all `IDatabaseMigrator` registrations in an explicit context order. It emits context names and outcome only, without connection strings or secrets.

The job returns `0` on success and non-zero on the first error. It accepts no `--rootId` and has no 1C dependency.

## Cloud Run sequence

1. Build and push the Jobs image.
2. Update/create and execute a dedicated `database-migrate` Cloud Run Job using the Jobs image and only the database connection secret.
3. Wait for execution success.
4. Deploy the API revision.

The API `Program.cs` removes `RunStartupTasksAsync` migration execution. Seeding is a separate decision and must not be silently retained in API startup.

## Rollback

Before the first production execution, create a Cloud SQL backup. If the forward migration fails, stop deployment and restore from the verified backup or apply an explicitly reviewed reverse migration; do not retry by changing `__EFMigrationsHistory` manually.
