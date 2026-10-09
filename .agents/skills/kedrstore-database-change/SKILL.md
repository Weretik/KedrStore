---
name: kedrstore-database-change
description: "Use when a KedrStore feature changes EF Core schema, mappings, migrations, seed data, transactions, or database rollout behavior."
---

# KedrStore Database Change

Проєктуй і перевіряй data change як окремий ризиковий delivery slice.

## Workflow

1. Прочитай feature data model/design, identifier strategy, database/testing rules і `docs/sdd/operations/data/migrations-and-seeding.md`.
2. Зафіксуй affected DbContext, current schema/data baseline, integrity constraints, compatibility, rollout і rollback/recovery до зміни code.
3. Зміни Domain model лише за business need; EF mapping і migrations тримай в owning Infrastructure project.
4. Не редагуй applied migration. Створи нову migration у правильному context/startup project і переглянь generated Up/Down, indexes, constraints та destructive operations.
5. Перевір migration на disposable PostgreSQL database, включно з empty baseline і потрібним upgrade path. Seeders мають бути idempotent і визначати behavior для existing data.
6. Пам'ятай, що `Host.Api` виконує migrations і seeders до serving requests. Перевір startup logs і failure behavior.
7. Запиши exact commands/results та residual rollout risk у feature evidence.

## Safety boundary

Не запускай API, migration, seeder або SQL проти shared, staging чи production database без явного дозволу користувача та точно підтвердженої target configuration. Не виводь connection strings або secrets.

## Example

`Use $kedrstore-database-change to add the approved Catalog archive fields and verify the migration on a disposable database.`
