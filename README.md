# KedrStore

KedrStore is a modular-monolith backend for the **Kedr** e-commerce platform. It provides product-catalog management, customer and pricing capabilities, authentication and authorization, and integration with 1C.

> Status: actively developed. This repository contains the public API, background jobs, and technical documentation.

## Features

- Product catalog, categories, prices, translations, and read projections.
- Catalog and sales-data synchronization with 1C over SOAP.
- Customers/contractors and sales pricing rules.
- Session-based authentication, roles, and access policies powered by ASP.NET Core Identity.
- Versioned OpenAPI contracts, Swagger UI, and health-check endpoints.
- A separate CLI host for imports and operational background jobs.

## Technology stack

- .NET 10 and ASP.NET Core
- PostgreSQL and Entity Framework Core
- ASP.NET Core Identity
- Mediator/CQRS and FluentValidation
- Serilog
- OpenAPI and Swagger

## Architecture

KedrStore is built as a modular monolith following Clean Architecture. Business modules are isolated, while dependencies point toward the domain:

```text
Host.Api / Host.Jobs
        |
        +-- <Module>.Api ----------> <Module>.Application --> <Module>.Domain
        +-- <Module>.Infrastructure -----------------------> <Module>.Application + Domain

BuildingBlocks.* — shared technical primitives without business rules
```

Main modules:

| Module | Responsibility |
| --- | --- |
| `Catalog` | Products, categories, prices, translations, projections, and catalog synchronization with 1C |
| `Sales` | Contractors, customers, and pricing rules |
| `Identity` | Users, roles, sessions, authorization, and seed data |
| `BuildingBlocks` | Shared abstractions and infrastructure components |

```text
src/
  Bootstrapper/Host.Api/       HTTP API and composition root
  Bootstrapper/Host.Jobs/      CLI for background jobs and imports
  Catalog/                     Catalog module
  Sales/                       Sales module
  Identity/                    Identity module
  BuildingBlocks/              Shared components
tests/                         Unit, integration, and architecture tests
docs/sdd/                      Current technical documentation
```

## Quick start

### Prerequisites

- .NET SDK 10 (the exact version is pinned in [`global.json`](global.json));
- PostgreSQL available through `ConnectionStrings:Default`;
- Local secrets for the database and, if needed, external integrations.

### Run the API

```powershell
git clone <repository-url>
cd KedrStore

dotnet restore KedrStore.sln
dotnet run --project src/Bootstrapper/Host.Api/Host.Api.csproj --launch-profile https
```

After startup, the following endpoints are available:

- API: `https://localhost:7230`
- Health check: `https://localhost:7230/health`
- Swagger UI (Development only): `https://localhost:7230/swagger`

HTTPS is required to fully test authorization because the refresh cookie is marked as `Secure`.

### Configure local secrets

Do not commit passwords, tokens, or live connection strings to `appsettings*.json` or Git. For local development, use User Secrets:

```powershell
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=kedrdb;Username=<user>;Password=<password>" --project src/Bootstrapper/Host.Api/Host.Api.csproj
dotnet user-secrets set "ADMIN_DEFAULT_PASSWORD" "<local-only-password>" --project src/Bootstrapper/Host.Api/Host.Api.csproj
```

On startup, the API automatically applies migrations and seeds data. Do not point it at a database that must not be modified.

## Background jobs and 1C import

`Host.Jobs` is a standalone CLI host. It has its own User Secrets and uses `appsettings.json` and environment variables.

```powershell
$env:DOTNET_ENVIRONMENT = 'Development'
dotnet run --project src/Bootstrapper/Host.Jobs/Host.Jobs/Host.Jobs.csproj -- --job=full
```

Examples:

```powershell
# Refresh stock levels for a specific 1C root
dotnet run --project src/Bootstrapper/Host.Jobs/Host.Jobs/Host.Jobs.csproj -- --job=stocks --rootId=<one-c-root-id>

# Rebuild the catalog read model
dotnet run --project src/Bootstrapper/Host.Jobs/Host.Jobs/Host.Jobs.csproj -- --job=rebuild-projections
```

For the complete command list and safe-run guidelines, see the [Host.Jobs documentation](docs/sdd/operations/jobs/host-jobs-cli.md).

## Development and verification

```powershell
# Build the solution
dotnet build KedrStore.sln

# Run all tests
dotnet test KedrStore.sln
```

Before making changes, review the [engineering standards](docs/sdd/standards/README.md). For API work, review the [contracts](docs/sdd/contracts/README.md).

## Documentation

- [Architecture and modules](docs/sdd/architecture/README.md)
- [Local setup](docs/sdd/operations/local-development/run-api.md)
- [Configuration and secrets](docs/sdd/operations/configuration/configuration-and-secrets.md)
- [Migrations and seeding](docs/sdd/operations/data/migrations-and-seeding.md)
- [Diagnostics, health checks, and Swagger](docs/sdd/operations/diagnostics/health-logs-swagger.md)
- [OpenAPI contracts](docs/sdd/contracts/README.md)

`docs/legacy` contains historical ADRs and materials. Use `docs/sdd` as the source of truth for new work.

## License

This project is licensed under the PolyForm Noncommercial License 1.0.0. Commercial use is not permitted. See [LICENSE.txt](LICENSE.txt).
