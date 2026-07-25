# Operations

This section explains how the running backend is configured, started and diagnosed. It describes operational facts, not feature design; feature-specific manual checks remain in the feature specification.

```text
operations/
├── local-development/  run the API and use local URLs
├── configuration/      configuration sources and secret handling
├── data/               automatic migrations and seeders
├── diagnostics/        health, Swagger and logs
└── jobs/               Host.Jobs and OneC import commands
```

## Read by task

- Start the backend locally: [local development](local-development/run-api.md)
- Configure a machine or fix missing settings: [configuration and secrets](configuration/configuration-and-secrets.md)
- Understand what startup changes in the database: [migrations and seeders](data/migrations-and-seeding.md)
- Check a running API or reproduce an endpoint: [diagnostics](diagnostics/health-logs-swagger.md)
- Run a OneC/import operation: [background jobs](jobs/one-c-jobs.md)

Do not put a password, token, full connection string or production endpoint into this documentation.
