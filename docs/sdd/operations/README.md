# Operations documentation

This section explains how to configure, run, and diagnose the backend. It records operational facts; manual checks for a specific feature belong in that feature's specification.

```text
operations/
├── local-development/  API startup and local addresses
├── configuration/      configuration sources and secrets
├── deployment/         public API host and Cloudflare proxy
├── data/               separate migration job and seed data
├── diagnostics/        health, Swagger, and logs
└── jobs/               Host.Jobs commands and 1C imports
```

## Browse by task

- Run the backend locally: [local development](local-development/run-api.md)
- Configure the environment: [configuration and secrets](configuration/configuration-and-secrets.md)
- Serve the API on its public host: [Cloudflare API proxy](deployment/cloudflare-api-proxy.md)
- Prepare the database with a separate job: [migrations and seed data](data/migrations-and-seeding.md)
- Check the API or reproduce an HTTP request: [diagnostics](diagnostics/health-logs-swagger.md)
- Run a 1C/import operation: [jobs and runbooks](jobs/README.md)

Do not record passwords, tokens, full connection strings, or production endpoints here.
