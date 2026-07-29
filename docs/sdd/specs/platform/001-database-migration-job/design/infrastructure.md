# Database migration job — infrastructure and rollout

## Approved Identity reset

Production Identity history contains the int-key migrations `20260118230934_InitialAppIdentityDbContext` and `20260507141045_AddRefreshSessionStore`, while the current application model uses Guid keys. The approved rollout deliberately discards non-critical Identity data rather than attempting an unsafe key conversion.

After a fresh backup and foreign-key review, the reset drops only `AppRefreshSessions` and `AspNet*` Identity tables, then removes only the two historical Identity rows from `__EFMigrationsHistory`. The root migration job applies `20260529142739_InitialAppIdentityDbContext`, which recreates the current Guid Identity schema. Catalog and Sales tables and migration history remain untouched.

## Job contract

Add `--job=migrate` to `Host.Jobs/Program.cs`. It invokes a dedicated migration orchestration service in the `Host.Jobs` root project; the service resolves all `IDatabaseMigrator` registrations in an explicit context order, then runs the existing seeders so the baseline Identity roles/admin account are recreated. It emits context names and outcome only, without connection strings or secrets.

The job returns `0` on success and non-zero on the first error. It accepts no `--rootId` and has no 1C dependency.

## Cloud Run sequence

1. Build and push the Jobs image.
2. Update/create and execute a dedicated `database-migrate` Cloud Run Job using the Jobs image, database connection secret, and the admin-password secret required by the Identity seeder.
3. Wait for execution success.
4. Deploy the API revision.

The API `Program.cs` removes `RunStartupTasksAsync` migration execution. Seeding is a separate decision and must not be silently retained in API startup.

## Rollback

Before the first production execution, create a Cloud SQL backup. If the forward migration fails, stop deployment and restore from the verified backup or apply an explicitly reviewed reverse migration; do not retry by changing `__EFMigrationsHistory` manually.
