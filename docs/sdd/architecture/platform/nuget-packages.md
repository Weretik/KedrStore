# Direct NuGet Dependencies

**Verified 2026-11-14.** Versions live only in [`Directory.Packages.props`](../../../../Directory.Packages.props) (central package management); a `.csproj` lists `<PackageReference Include="..." />` without a version. Shared settings (target framework, nullable, implicit usings) are in [`Directory.Build.props`](../../../../Directory.Build.props). See the [module rules](../../standards/module-rules.md#solution-root) for which layer may use which package.

Kedr runs on Google Cloud Run: there is no in-process job scheduler (Hangfire or a hosted loop). Scheduled and one-off work runs as Cloud Run Jobs through `Host.Jobs` ([jobs](../../operations/jobs/README.md)).

| Direct dependency | Version | Projects | Purpose |
| --- | --- | --- | --- |
| `Ardalis.Result` | 10.1.0 | BuildingBlocks.Api, Catalog.Contracts, Identity.Contracts | Typed result and response contract; HTTP mapping is described in [BuildingBlocks.Api](building-blocks/api.md). |
| `Ardalis.Result.AspNetCore` | 10.1.0 | BuildingBlocks.Api | `Result` to HTTP mapping in BuildingBlocks.Api. |
| `Ardalis.Result.FluentValidation` | 10.1.0 | BuildingBlocks.Application | Integration of `FluentValidation` errors with `Result`; check exact code usage. |
| `Ardalis.SmartEnum` | 8.2.0 | Catalog.Domain | Typed enumerations in the Catalog domain. |
| `Ardalis.Specification` | 9.3.1 | Catalog.Application, Content.Application, Identity.Application, Media.Application, Sales.Application, Work.Application | Specifications for repository queries; [persistence approach](../runtime/persistence-and-integrations.md). |
| `Ardalis.Specification.EntityFrameworkCore` | 9.3.1 | Catalog.Infrastructure, Content.Infrastructure, Identity.Infrastructure, Media.Infrastructure, Sales.Infrastructure, Work.Infrastructure | EF Core repositories for specifications in module Infrastructure. |
| `ClosedXML` | 0.105.1 | Catalog.Application, Sales.Infrastructure | Excel files: Catalog import and order export. |
| `coverlet.collector` | 10.1.0 | ArchitectureTests, IntegrationTests, UnitTests | Test coverage collection. |
| `FluentFTP` | 55.1.0 | Identity.Infrastructure, Media.Infrastructure |  |
| `FluentValidation` | 12.2.0 | BuildingBlocks.Application | Input validators in use cases. |
| `FluentValidation.DependencyInjectionExtensions` | 12.2.0 | BuildingBlocks.Application | Validator registration in each module's `Add<Module>Module()`. |
| `HtmlSanitizer` | 9.2.1039 | Catalog.Infrastructure, Content.Infrastructure |  |
| `libphonenumber-csharp` | 9.0.42 | BuildingBlocks.Application, Sales.Infrastructure | Phone number processing and validation; look for specific calls in the relevant projects. |
| `Mediator.Abstractions` | 3.0.2 | BuildingBlocks.Application | Mediator request/handler contracts and pipeline. |
| `Mediator.SourceGenerator` | 3.0.2 | Host.Api | Generates the Mediator implementation during host build; registration is in `MediatorRegistrationExtensions.cs`. |
| `Microsoft.AspNetCore.DataProtection.EntityFrameworkCore` | 10.0.14 | Identity.Infrastructure |  |
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | 10.0.14 | Identity.Infrastructure | ASP.NET Core Identity storage in Identity.Infrastructure. |
| `Microsoft.AspNetCore.Mvc.Testing` | 10.0.14 | IntegrationTests | Test hosting for ASP.NET Core. |
| `Microsoft.AspNetCore.OpenApi` | 10.0.14 | Host.Api | Built-in OpenAPI endpoint; `Host.Api/Program.cs` calls `MapOpenApi()` in Development. |
| `Microsoft.EntityFrameworkCore` | 10.0.14 | BuildingBlocks.Infrastructure, Catalog.Application, Content.Application, Media.Application, Sales.Application, Work.Application | EF Core: `DbContext` types and migrations in Infrastructure; module Application projects query the read `DbContext` interface (documented deviation in the [module rules](../../standards/module-rules.md#known-deviations)). |
| `Microsoft.EntityFrameworkCore.Design` | 10.0.14 | Content.Infrastructure, Media.Infrastructure, Sales.Infrastructure, Work.Infrastructure |  |
| `Microsoft.EntityFrameworkCore.InMemory` | 10.0.14 | IntegrationTests | Non-relational EF provider for limited test scenarios only; [rules](../../standards/testing-rules.md). |
| `Microsoft.EntityFrameworkCore.Tools` | 10.0.14 | Host.Api | EF Core design-time tools; not a runtime service. |
| `Microsoft.Extensions.Configuration.Abstractions` | 10.0.14 | BuildingBlocks.Application, BuildingBlocks.Integrations.OneC | Configuration interfaces for shared components and 1C. |
| `Microsoft.Extensions.DependencyInjection.Abstractions` | 10.0.14 | BuildingBlocks.Application | DI interfaces for service registration and resolution. |
| `Microsoft.Extensions.Logging.Abstractions` | 10.0.14 | BuildingBlocks.Application | Logging interfaces for application components. |
| `Microsoft.Extensions.Options` | 10.0.14 | BuildingBlocks.Application | Options binding in BuildingBlocks.Application behaviors. |
| `Microsoft.NET.Test.Sdk` | 18.11.0 | ArchitectureTests, IntegrationTests, UnitTests | Test integration with `dotnet test`. |
| `Microsoft.OpenApi` | 2.13.0 | Host.Api | OpenAPI object model for schema and Swagger configuration. |
| `NetArchTest.Rules` | 1.3.2 | ArchitectureTests | Dependency and architectural-boundary checks. |
| `Newtonsoft.Json` | 13.0.4 | Catalog.Application | Declared JSON serializer; check exact need and call sites before changing. |
| `NodaMoney` | 2.9.0 | Catalog.Application, Catalog.Domain | Monetary values and currencies for Catalog contracts/domain. |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 10.0.3 | BuildingBlocks.Infrastructure, Catalog.Application | EF Core PostgreSQL provider in Infrastructure; Catalog.Application uses `EF.Functions.ILike` (documented deviation). |
| `Scriban` | 7.5.3 | Host.Api | Templating engine declared in the API host; check current calls and need in the code. |
| `Serilog` | 4.4.0 | Host.Api | Structured logging for the API host. |
| `Serilog.AspNetCore` | 10.0.0 | Host.Api | Serilog integration for ASP.NET Core; `Program.cs` calls request logging. |
| `Serilog.Enrichers.Environment` | 3.0.1 | Host.Api | Additional environment properties in logs, according to configuration. |
| `Serilog.Enrichers.Process` | 3.0.0 | Host.Api | Additional process properties in logs, according to configuration. |
| `Serilog.Settings.Configuration` | 10.0.1 | Host.Api | Reads Serilog settings from configuration. |
| `SkiaSharp` | 4.154.0 | Content.Infrastructure, Identity.Infrastructure, Media.Infrastructure |  |
| `SkiaSharp.NativeAssets.Linux.NoDependencies` | 4.154.0 | Content.Infrastructure, Identity.Infrastructure, Media.Infrastructure |  |
| `Slugify.Core` | 5.2.0 | Catalog.Application, UnitTests | URL slug generation and its unit tests. |
| `Swashbuckle.AspNetCore` | 10.3.1 | Host.Api | Generates Swagger UI and OpenAPI in Development. |
| `System.Security.Cryptography.Xml` | 10.0.14 | BuildingBlocks.Integrations.OneC | XML cryptography declared for the 1C integration; check the specific protocol in the adapter. |
| `System.ServiceModel.Duplex` | 6.0.0 | BuildingBlocks.Integrations.OneC | WCF duplex API declared by the 1C project; check whether it is actually needed. |
| `System.ServiceModel.Federation` | 10.1.0 | BuildingBlocks.Integrations.OneC | WCF federation API declared by the 1C project; check whether it is actually needed. |
| `System.ServiceModel.Http` | 10.1.0 | BuildingBlocks.Integrations.OneC | HTTP transport for the 1C SOAP/WCF client. |
| `System.ServiceModel.NetTcp` | 10.1.0 | BuildingBlocks.Integrations.OneC | WCF NetTcp API declared by the 1C project; check whether it is actually needed. |
| `System.ServiceModel.Primitives` | 10.1.0 | BuildingBlocks.Integrations.OneC | Base WCF contracts for the SOAP client. |
| `System.ServiceModel.Security` | 6.0.0 | BuildingBlocks.Integrations.OneC | WCF security API for the SOAP client; check concrete use. |
| `Telegram.Bot` | 22.10.3.3 | BuildingBlocks.Infrastructure | Telegram notifications from BuildingBlocks.Infrastructure; [operational context](../../operations/jobs/sales-one-c-runbook.md). |
| `Testcontainers.PostgreSql` | 4.16.0 | IntegrationTests | Temporary PostgreSQL for integration tests. |
| `Vogen` | 8.0.7 | Catalog.Domain | Declared value-object generator for legacy code; [current rule](../../standards/identifier-strategy.md) does not allow new typed IDs through Vogen. |
| `xunit` | 2.9.3 | ArchitectureTests, IntegrationTests, UnitTests | Test framework. |
| `xunit.runner.visualstudio` | 4.0.1 | ArchitectureTests, IntegrationTests, UnitTests | Runs xUnit in the test runner. |
| `Xunit.SkippableFact` | 1.5.85 | IntegrationTests | Explicitly skips a test when a local prerequisite is unavailable. |

The SDK version is set by [`global.json`](../../../../global.json); target frameworks come from `Directory.Build.props` (`net10.0`). When updating a dependency, change only its version in `Directory.Packages.props` and update this table.
