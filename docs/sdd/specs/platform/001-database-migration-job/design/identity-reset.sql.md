# Approved production Identity reset SQL

Run this script only after creating and verifying a fresh backup. It deliberately stops if any non-Identity table has a foreign key to an Identity table. Do not add `CASCADE`.

```sql
BEGIN;

DO $$
DECLARE
    external_reference_count integer;
BEGIN
    SELECT count(*)
    INTO external_reference_count
    FROM pg_constraint fk
    JOIN pg_class child_table ON child_table.oid = fk.conrelid
    JOIN pg_namespace child_schema ON child_schema.oid = child_table.relnamespace
    JOIN pg_class parent_table ON parent_table.oid = fk.confrelid
    JOIN pg_namespace parent_schema ON parent_schema.oid = parent_table.relnamespace
    WHERE fk.contype = 'f'
      AND parent_schema.nspname = 'public'
      AND parent_table.relname IN (
          'AspNetRoles', 'AspNetUsers', 'AspNetRoleClaims', 'AspNetUserClaims',
          'AspNetUserLogins', 'AspNetUserRoles', 'AspNetUserTokens', 'AppRefreshSessions')
      AND (child_schema.nspname <> 'public' OR child_table.relname NOT IN (
          'AspNetRoles', 'AspNetUsers', 'AspNetRoleClaims', 'AspNetUserClaims',
          'AspNetUserLogins', 'AspNetUserRoles', 'AspNetUserTokens', 'AppRefreshSessions'));

    IF external_reference_count <> 0 THEN
        RAISE EXCEPTION 'Identity reset blocked: non-Identity foreign keys reference Identity tables.';
    END IF;
END $$;

DROP TABLE IF EXISTS "AppRefreshSessions";
DROP TABLE IF EXISTS "AspNetUserTokens";
DROP TABLE IF EXISTS "AspNetUserLogins";
DROP TABLE IF EXISTS "AspNetUserClaims";
DROP TABLE IF EXISTS "AspNetRoleClaims";
DROP TABLE IF EXISTS "AspNetUserRoles";
DROP TABLE IF EXISTS "AspNetUsers";
DROP TABLE IF EXISTS "AspNetRoles";

DELETE FROM "__EFMigrationsHistory"
WHERE "MigrationId" IN (
    '20260118230934_InitialAppIdentityDbContext',
    '20260507141045_AddRefreshSessionStore');

COMMIT;
```

After the script commits, execute the dedicated migration job. It must record `20260529142739_InitialAppIdentityDbContext`, recreate the Guid Identity schema, and seed roles/admin credentials. If preflight fails or any statement errors, PostgreSQL rolls the transaction back; stop and investigate before retrying.
