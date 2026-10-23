# Build, Verification, and Delivery Tooling

**Verified fact, checked 2026-10-15:** this document was checked against `global.json`, `KedrStore.sln`, `.github/workflows/deploy-cloudrun.yml`, Dockerfiles, `Program.cs` files for both hosts, and launch settings. The presence of a CI step does not prove successful deployment to a particular environment.

| Area | Repository source | Current use |
| --- | --- | --- |
| SDK | `global.json` | .NET SDK `10.0.101`, `rollForward: latestFeature`; projects target `net10.0`. |
| Solution | `KedrStore.sln` | Single entry point for `dotnet restore`, `dotnet build`, and `dotnet test`. |
| Local API profiles | `src/Bootstrapper/Host.Api/Properties/launchSettings.json` | Profiles `http` and `https`; addresses and launch instructions are in the [operations guide](../../operations/local-development/run-api.md). |
| Configuration | `src/Bootstrapper/Host.Api/appsettings*.json`, `src/Bootstrapper/Host.Jobs/Host.Jobs/appsettings.json`, ASP.NET Core User Secrets, environment variables | See [configuration and secrets](../../operations/configuration/configuration-and-secrets.md). `Host.Jobs` uses its own User Secrets in Development. |
| Containers | `src/Bootstrapper/Host.Api/Dockerfile`, `src/Bootstrapper/Host.Jobs/Host.Jobs/Dockerfile` | Separate images for the HTTP host and CLI jobs. |
| CI | `.github/workflows/deploy-cloudrun.yml`, `verify` job | Checks out the code, installs .NET from `global.json`, restores dependencies, builds Release, UnitTests, ArchitectureTests, IntegrationTests and uploads coverage in Cobertura format. IntegrationTests may require Docker for PostgreSQL Testcontainers. |
| Deployment | `.github/workflows/deploy-cloudrun.yml`, job `deploy` | After verification succeeds, builds images, runs Cloud Run `database-migrate` job with `--job=migrate` and waits for completion, then deploys `Host.Api` and updates job images. The workflow runs on pushes to `master`. |

Local commands defined by [testing rules](../../standards/testing-rules.md):

```powershell
dotnet restore KedrStore.sln
dotnet build KedrStore.sln --no-restore
dotnet test KedrStore.sln --no-build
```

`Host.Jobs` is a one-shot CLI: `--job=<name>` selects an operation, and `--job=migrate` runs migrations and seeders. No internal scheduler starts jobs automatically. See [CLI and operations guides](../../operations/jobs/README.md). Do not run the migration job simply because you are reading this documentation: the command changes data and schema.

When changing the SDK, manifests, Dockerfiles, CI process, or launch command, update this description, [stack](technology-stack.md), [package catalog](nuget-packages.md) and the relevant operations guides.
