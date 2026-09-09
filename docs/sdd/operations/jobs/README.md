# Background jobs and import runbooks

`Host.Jobs` is a console host: it builds DI, makes one scope, executes one named operation and exits. The host has no internal scheduler, queue, or recurring execution. Individual jobs may coordinate through persisted state; for example, Sales order delivery stores retry timing and uses optimistic concurrency in PostgreSQL.

```text
operator / cron / CI → Host.Jobs (--job=...) → scoped Catalog/Sales job → PostgreSQL
```

- [CLI commands and configuration](host-jobs-cli.md)
- [Catalog OneC runbook](catalog-one-c-runbook.md)
- [Sales OneC runbook](sales-one-c-runbook.md)
- [Troubleshooting](troubleshooting.md)

See [OneC integration architecture](../../architecture/integrations/one-c/README.md) for contracts and data ownership. Never paste secrets or real endpoint URLs into output, issues or docs.
