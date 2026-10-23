# Run Host.Api locally

## Prerequisites

- .NET SDK for target `net10.0`; `global.json` defines the exact baseline and roll-forward.
- PostgreSQL available through `ConnectionStrings:Default`, with its schema prepared by a separate migration job.
- Required local secrets; see [configuration and secrets](../configuration/configuration-and-secrets.md).

## Start

From the repository root:

~~~powershell
dotnet restore KedrStore.sln
dotnet run --project src/Bootstrapper/Host.Api/Host.Api.csproj --launch-profile https
~~~

The `https` profile listens on `https://localhost:7230` and `http://localhost:5081`. Use HTTPS to check authentication and refresh: the current refresh cookie has the `Secure` attribute.

To check an endpoint over HTTP only, use the `http` profile:

~~~powershell
dotnet run --project src/Bootstrapper/Host.Api/Host.Api.csproj --launch-profile http
~~~

The API does not run migrations or seeders at startup. The separate `Host.Jobs --job=migrate` command prepares the database; check its order and effects in the [migration and seed data guide](../data/migrations-and-seeding.md). The API still needs configured database access for requests.

## Stop and rebuild

If Windows locks output DLLs, stop the running `Host.Api` before building. Then run the commands required by the [testing standard](../../standards/testing-rules.md).
