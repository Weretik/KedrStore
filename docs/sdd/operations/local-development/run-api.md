# Run Host.Api locally

## Prerequisites

- .NET SDK matching the solution target (`net10.0`).
- PostgreSQL reachable through `ConnectionStrings:Default`.
- Required local secrets configured; see [configuration and secrets](../configuration/configuration-and-secrets.md).

## Start

From the repository root:

~~~powershell
dotnet restore KedrStore.sln
dotnet run --project src/Bootstrapper/Host.Api/Host.Api.csproj --launch-profile https
~~~

The `https` launch profile listens on `https://localhost:7230` and also on `http://localhost:5081`. Use the HTTPS address for authentication/refresh testing: the current refresh cookie is configured as `Secure`.

For an HTTP-only endpoint check, use the `http` profile:

~~~powershell
dotnet run --project src/Bootstrapper/Host.Api/Host.Api.csproj --launch-profile http
~~~

The API host starts migrations and seeders before it begins serving requests. Read [migrations and seeders](../data/migrations-and-seeding.md) before running it against a non-local database.

## Stop and rebuild

Stop the running Host.Api process before building when Windows has locked its output DLLs. Then run the relevant build/test commands from [testing standards](../../standards/quality/testing.md).
