# Migrations and seeders

## Startup order

`Host.Api` calls `RunStartupTasksAsync()` before middleware and endpoints begin serving requests.

```text
Host.Api startup
  -> UseAppMigrations()
       -> every registered IDatabaseMigrator / DbContext
  -> UseAppSeeders()
       -> every registered ISeeder (one instance per seeder type)
  -> HTTP host starts
```

Registered module contexts include Catalog, Identity and Sales. Thus a normal API start can change the database schema and insert/update seed data. Do not point a local process at a database unless that effect is intended.

## Current Identity seeders

- `RoleSeeder` ensures Admin, Manager and User roles exist.
- `IdentitySeeder` creates the configured administrator only when it is missing and `ADMIN_DEFAULT_PASSWORD` is configured.

The password is not read from the `Identity:AdminUser:DefaultPassword` field; the seeder reads `ADMIN_DEFAULT_PASSWORD`. A missing value skips administrator creation and logs the condition.

## Change rules

- Create a migration only when the feature changes persistent schema or data contract.
- Put migration design and rollout/verification steps in the [migration template](../../specs/_templates/migration/template-migration.md).
- Do not edit an already-applied migration to alter production history.
- Treat seed changes as data changes: document idempotency, existing-data behaviour and rollback/recovery approach.
- Check application startup logs after a migration or seeder change.
