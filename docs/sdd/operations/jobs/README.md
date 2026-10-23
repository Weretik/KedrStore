# Background jobs and import runbooks

`Host.Jobs` is a console host: it composes DI, creates one scope, runs the named operation, and exits. It has no internal scheduler, queue, or repeat loop. Some jobs coordinate through persisted state; for example, Sales order delivery stores the next attempt time and uses optimistic concurrency in PostgreSQL.

```text
operator / cron / CI → Host.Jobs (--job=...) → scoped Catalog/Sales job → PostgreSQL
```

- [CLI commands and configuration](host-jobs-cli.md)
- [Migrations and seed data](../data/migrations-and-seeding.md) — `--job=migrate`
- [Catalog/1C runbook](catalog-one-c-runbook.md)
- [Sales/1C runbook](sales-one-c-runbook.md)
- [Troubleshooting](troubleshooting.md)

Contracts and data ownership are described in the [1C integration architecture](../../architecture/integrations/one-c/README.md). Do not put secrets or real endpoint URLs in command output, tasks, or documentation.
