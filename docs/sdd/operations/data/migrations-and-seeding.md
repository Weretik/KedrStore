# Migrations and seed data

## Current execution order

As of 2026-10-15, `Host.Api` does **not** run migrations or seeders at startup: `src/Bootstrapper/Host.Api/Program.cs` has no call to `RunStartupTasksAsync()`, `UseAppMigrations()`, or `UseAppSeeders()`. Migration/seeder extension methods remain in `BuildingBlocks.Infrastructure`, but their presence does not mean the API invokes them.

A separate `Host.Jobs` command runs migrations and seed data:

```powershell
dotnet run --project src/Bootstrapper/Host.Jobs/Host.Jobs/Host.Jobs.csproj -- --job=migrate
```

`src/Bootstrapper/Host.Jobs/Host.Jobs/DatabaseMigrationJob.cs` calls registered `IDatabaseMigrator` implementations in order, then each unique `ISeeder` type. On failure, `Host.Jobs/Program.cs` returns a non-zero exit code. Normal API startup no longer prepares the database automatically. `--job=migrate` changes schema and data; verify configuration and target database before running it.

The current `.github/workflows/deploy-cloudrun.yml` creates or updates the Cloud Run Job `database-migrate`, runs it, and waits for success before deploying a new API revision. This describes the deployment workflow code; it does not confirm the state of any deployed environment. First-deployment order and Identity risks are described in the [migration job specification](../../specs/platform/001-database-migration-job/README.md).

## Current Identity seeders

- `RoleSeeder` ensures the Admin, Manager, and User roles exist.
- `IdentitySeeder` creates a configured administrator only when none exists and `ADMIN_DEFAULT_PASSWORD` is set.

The password is not read from `Identity:AdminUser:DefaultPassword`; the seeder uses `ADMIN_DEFAULT_PASSWORD`. Without it, administrator creation is skipped and logged. Do not record the password in documentation or execution logs.

## Change rules

- Add a migration only when a feature changes persistent schema or data contracts.
- Record migration design, rollout order, rollback/recovery, and verification in the relevant [feature specification](../../specs/README.md) under the [database rules](../../standards/database-rules.md). Add `data-model.md` and `design/infrastructure.md` when the change requires them.
- Do not rewrite an already applied migration and alter production history.
- Treat seed-data changes as data changes: document idempotency, behavior for existing rows, and recovery.
- After changing a migration or seeder, verify a separate `--job=migrate` run and its logs; API startup does not prove that the migration succeeded.
