# Database backups and restore

The production PostgreSQL database of Kedr is a managed database on the adm.tools hosting, not Cloud SQL. The hosting takes automatic backups; nobody runs them by hand, and nothing in this repository or in Google Cloud schedules them.

| Question | Answer |
| --- | --- |
| Who takes backups | The hosting, automatically (confirmed by the owner, 2026-11-17) |
| Where they are | adm.tools control panel → the database → «Резервні копії» |
| How often and how long they are kept | Set by the hosting plan; check the panel. The [quality targets](../../architecture/overview/quality-targets.md) assume at least one backup a day |
| Who can restore | The owner, from the control panel |

## Before a risky operation

A data import, a bulk delete, or any `Host.Seed`/`Host.Jobs` mode that changes many rows has no undo endpoint. Make a manual backup in the control panel right before it, as the runbooks in [migrations and seed data](migrations-and-seeding.md) say.

## Restore

1. Stop writes: pause the Cloud Scheduler triggers of the `Host.Jobs` Cloud Run Jobs (1C sync, outbox, «Фонові задачі») so no task writes during the restore.
2. In the adm.tools panel, restore the chosen backup. Prefer restoring into a new database and switching the `db-connection` secret over overwriting the live one: the live data stays as evidence, and switching back is one secret change.
3. Run the `database-migrate` job once, so a backup older than the current release gets the newest schema (migrations keep the previous release working, so the order is safe).
4. Redeploy or restart the API so it picks up the `db-connection` secret, check `/health` and one admin page, then resume the scheduler triggers.

Everything written after the backup is lost (see RPO in the [quality targets](../../architecture/overview/quality-targets.md)); orders and payments made in that window have to be re-entered from their sources.
