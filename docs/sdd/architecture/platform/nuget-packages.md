# Direct NuGet Dependencies

**Verified 2026-10-15:** all `src/**/*.csproj` and `tests/**/*.csproj` in the repository. The scan found 68 `PackageReference` entries forming 53 unique name + declared-version pairs. There is no `Directory.Packages.props`; versions are specified in the `.csproj` files themselves. The table describes **declared direct dependencies** and does not guarantee that every package is actually called by code or installed in a particular environment. For transitive dependencies and resolved versions, use NuGet restore/assets; do not add them manually to this catalog.

Project-column abbreviations: `B.*` = `src/BuildingBlocks/BuildingBlocks.*`, `C.*` = `src/Catalog/Catalog.*`, `S.*` = `src/Sales/Sales.*`, `H.Api` = `src/Bootstrapper/Host.Api/Host.Api.csproj`, `T.Unit` = `tests/UnitTests/UnitTests.csproj`, `T.Integration` = `tests/IntegrationTests/IntegrationTests.csproj`, and `T.Architecture` = `tests/ArchitectureTests/ArchitectureTests.csproj`. Append `.csproj` to `B.*`, `C.*`, and `S.*` project names; the path for `B.Integrations.OneC` is `src/BuildingBlocks/BuildingBlocks.Integrations.OneC/BuildingBlocks.Integrations.OneC.csproj`. Each table entry can be checked against the indicated manifest. A package used by several projects appears in one row with all owners.

| Direct dependency | Version | Project | Purpose or verification context |
| --- | --- | --- | --- |
| `Ardalis.Result` | 10.1.0 | B.Api, C.Contracts | Typed result and response contract; HTTP mapping is described in [BuildingBlocks.Api](building-blocks/api.md). |
| `Ardalis.Result.AspNetCore` | 10.1.0 | B.Application | `Result` integration with ASP.NET Core; check exact code usage before changing it. |
| `Ardalis.Result.FluentValidation` | 10.1.0 | B.Application | Integration of `FluentValidation` errors with `Result`; check exact code usage. |
| `Ardalis.SmartEnum` | 8.2.0 | C.Domain | Typed enumerations in the Catalog domain. |
| `Ardalis.Specification` | 9.3.1 | B.Application | Specifications for repository queries; [persistence approach](../runtime/persistence-and-integrations.md). |
| `Ardalis.Specification.EntityFrameworkCore` | 9.3.1 | B.Application | EF Core adapter for specifications; check actual calls in code. |
| `ClosedXML` | 0.105.0 | B.Application | Excel file creation; check layer responsibility and export usage. |
| `FluentValidation` | 12.1.1 | B.Application | Input validators in use cases. |
| `FluentValidation.DependencyInjectionExtensions` | 12.1.1 | B.Application | Validator registration in DI; actual registration is in `Host.Api/DependencyInjection/ServiceRegistration/Pipeline/FluentValidationRegistrationExtensions.cs`. |
| `Hangfire` | 1.8.22 | B.Application | Declared background execution library; current `Host.Jobs` runs as a one-shot CLI without its own scheduler ([description](../../operations/jobs/README.md)). Do not attribute job execution to it without checking the code. |
| `libphonenumber-csharp` | 9.0.22 | B.Application, S.Infrastructure | Phone number processing and validation; look for specific calls in the relevant projects. |
| `Mediator.Abstractions` | 3.0.1 | B.Application | Mediator request/handler contracts and pipeline. |
| `Mediator.SourceGenerator` | 3.0.1 | H.Api | Generates the Mediator implementation during host build; registration is in `MediatorRegistrationExtensions.cs`. |
| `Microsoft.AspNetCore.Http.Abstractions` | 2.3.9 | B.Application, B.Infrastructure | HTTP abstractions declared in two BuildingBlocks projects; check current need before changing them. |
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | 10.0.2 | B.Application | ASP.NET Core Identity with EF Core; check concrete usage and layer boundaries in the code. |
| `Microsoft.AspNetCore.OpenApi` | 10.0.2 | H.Api | Built-in OpenAPI endpoint; `Host.Api/Program.cs` calls `MapOpenApi()` in Development. |
| `Microsoft.EntityFrameworkCore` | 10.0.2 | B.Application | EF Core API declared in BuildingBlocks.Application; contexts and migrations belong in Infrastructure. |
| `Microsoft.EntityFrameworkCore.Tools` | 10.0.2 | H.Api | EF Core design-time tools; not a runtime service. |
| `Microsoft.Extensions.Configuration.Abstractions` | 10.0.2 | B.Application, B.Integrations.OneC | Configuration interfaces for shared components and 1C. |
| `Microsoft.Extensions.DependencyInjection.Abstractions` | 10.0.2 | B.Application | DI interfaces for service registration and resolution. |
| `Microsoft.Extensions.Http` | 10.0.2 | B.Application | `HttpClientFactory` and DI for HTTP clients; check concrete adapters in code. |
| `Microsoft.Extensions.Logging.Abstractions` | 10.0.2 | B.Application | Logging interfaces for application components. |
| `Microsoft.OpenApi` | 2.8.0 | H.Api | OpenAPI object model for schema and Swagger configuration. |
| `Newtonsoft.Json` | 13.0.4 | B.Application | Declared JSON serializer; check exact need and call sites before changing. |
| `NodaMoney` | 2.6.0 | B.Application, C.Domain | Monetary values and currencies for Catalog contracts/domain. |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 10.0.0 | B.Application | EF Core PostgreSQL provider; actual `DbContext` types and migrations are in Infrastructure. |
| `Scriban` | 7.2.7 | H.Api | Templating engine declared in the API host; check current calls and need in the code. |
| `Serilog` | 4.3.0 | H.Api | Structured logging for the API host. |
| `Serilog.AspNetCore` | 10.0.0 | H.Api | Serilog integration for ASP.NET Core; `Program.cs` calls request logging. |
| `Serilog.Enrichers.Environment` | 3.0.1 | H.Api | Additional environment properties in logs, according to configuration. |
| `Serilog.Enrichers.Process` | 3.0.0 | H.Api | Additional process properties in logs, according to configuration. |
| `Serilog.Settings.Configuration` | 10.0.0 | H.Api | Reads Serilog settings from configuration. |
| `Slugify.Core` | 5.1.1 | B.Application, T.Unit | URL slug generation and its unit tests. |
| `Swashbuckle.AspNetCore` | 10.0.0 | H.Api | Generates Swagger UI and OpenAPI in Development. |
| `System.Security.Cryptography.Xml` | 10.0.10 | B.Integrations.OneC | XML cryptography declared for the 1C integration; check the specific protocol in the adapter. |
| `System.ServiceModel.Duplex` | 6.0.0 | B.Integrations.OneC | WCF duplex API declared by the 1C project; check whether it is actually needed. |
| `System.ServiceModel.Federation` | 10.0.652802 | B.Integrations.OneC | WCF federation API declared by the 1C project; check whether it is actually needed. |
| `System.ServiceModel.Http` | 10.0.652802 | B.Integrations.OneC | HTTP transport for the 1C SOAP/WCF client. |
| `System.ServiceModel.NetTcp` | 10.0.652802 | B.Integrations.OneC | WCF NetTcp API declared by the 1C project; check whether it is actually needed. |
| `System.ServiceModel.Primitives` | 10.0.652802 | B.Integrations.OneC | Base WCF contracts for the SOAP client. |
| `System.ServiceModel.Security` | 6.0.0 | B.Integrations.OneC | WCF security API for the SOAP client; check concrete use. |
| `Telegram.Bot` | 22.8.1 | B.Application | Telegram transport for notifications; [operational context](../../operations/jobs/sales-one-c-runbook.md). |
| `Unidecode.NET` | 2.2.1 | B.Application | Text transliteration; check concrete use in the code. |
| `Vogen` | 8.0.4 | B.Application, C.Domain | Declared value-object generator for legacy code; [current rule](../../standards/identifier-strategy.md) does not allow new typed IDs through Vogen. |

## Direct test-project packages

| Direct dependency | Version | Project | Purpose |
| --- | --- | --- | --- |
| `coverlet.collector` | 6.0.4 | T.Unit, T.Integration, T.Architecture | Test coverage collection. |
| `Microsoft.AspNetCore.Mvc.Testing` | 10.0.2 | T.Integration | Test hosting for ASP.NET Core. |
| `Microsoft.EntityFrameworkCore.InMemory` | 10.0.2 | T.Integration | Non-relational EF provider for limited test scenarios only; [rules](../../standards/testing-rules.md). |
| `Microsoft.NET.Test.Sdk` | 18.0.1 | T.Unit, T.Integration, T.Architecture | Test integration with `dotnet test`. |
| `NetArchTest.Rules` | 1.3.2 | T.Architecture | Dependency and architectural-boundary checks. |
| `Testcontainers.PostgreSql` | 4.15.0 | T.Integration | Temporary PostgreSQL for integration tests. |
| `xunit` | 2.9.3 | T.Unit, T.Integration, T.Architecture | Test framework. |
| `xunit.runner.visualstudio` | 3.1.5 | T.Unit, T.Integration, T.Architecture | Runs xUnit in the test runner. |
| `Xunit.SkippableFact` | 1.4.13 | T.Integration | Explicitly skips a test when a local prerequisite is unavailable. |

The SDK version is set by [`global.json`](../../../../global.json): `10.0.101` with `rollForward: latestFeature`; target frameworks in `.csproj` files are `net10.0`. When updating dependencies, compare this table with all manifests and actual usage with imports, DI, and runtime configuration. Do not describe a merely declared package as a required architectural pattern without that confirmation.
