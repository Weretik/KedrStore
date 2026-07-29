# Background jobs and import runbooks

`Host.Jobs` is a console host: it builds DI, makes one scope, executes one named operation and exits. It has no internal scheduler, queue, retry/backoff, distributed locking or recurring execution.

```text
operator / cron / CI → Host.Jobs (--job=...) → scoped Catalog/Sales job → PostgreSQL
```

- [CLI commands and configuration](host-jobs-cli.md)
- [Catalog OneC runbook](catalog-one-c-runbook.md)
- [Sales OneC runbook](sales-one-c-runbook.md)
- [Troubleshooting](troubleshooting.md)

See [OneC integration architecture](../../architecture/integrations/one-c/README.md) for contracts and data ownership. Never paste secrets or real endpoint URLs into output, issues or docs.
