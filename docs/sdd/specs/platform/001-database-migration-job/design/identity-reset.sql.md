# Погоджений SQL для production reset Identity

Запускай цей скрипт лише після створення й перевірки свіжого backup. Він навмисно зупиняється, якщо будь-яка таблиця поза Identity має зовнішній ключ до таблиці Identity. Не додавай `CASCADE`.

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

Після commit скрипта запусти окремий migration job. Він має записати `20260529142739_InitialAppIdentityDbContext`, відтворити Guid-схему Identity й створити ролі/admin credentials. Якщо preflight або будь-яка команда SQL падає, PostgreSQL відкочує транзакцію; зупинись і досліди причину до повтору.
