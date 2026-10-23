# Technology stack

| Area | Current choice | Rule for new work |
| --- | --- | --- |
| Runtime | .NET 10 / C#; `global.json` sets SDK `10.0.101` with `latestFeature` roll-forward | Follow nullable annotations and configured analyzers. |
| HTTP | ASP.NET Core controllers | Keep controllers thin and use Mediator. |
| Mediator | Mediator 3.x | Use request/handler pairs and the existing pipeline. |
| Mediator pipeline | RequestLogging, Exception, Performance, Validation, DomainEventDispatcher behaviors | Preserve registration order; do not put business rules in behaviors. |
| Validation | FluentValidation | Keep input validators beside the corresponding use case. |
| Results | Ardalis.Result | Return `Result` or `Result<T>` and use existing API mapping. |
| Data | EF Core 10 + Npgsql/PostgreSQL | EF configuration and migrations belong in Infrastructure; migrations run through separate `Host.Jobs --job=migrate`. |
| Repositories | Ardalis.Specification | Reuse the existing repository/specification style where it applies. |
| Identity | ASP.NET Core Identity | Preserve role, policy, and fallback-auth conventions. |
| Logging | Serilog | Use structured logs without sensitive data. |
| Background work | Existing background-job abstractions and `Host.Jobs` | Do not couple Application directly to a scheduler; `Host.Jobs` is a one-shot CLI. |
| External catalog | 1C integration over SOAP | Keep generated code and client behind Infrastructure adapters. |
| Tests | xUnit: unit, integration, and architecture | Add tests appropriate to the changed boundary and risk. |

The [NuGet package catalog](nuget-packages.md) lists all declared **direct** dependencies, versions, and projects. A package's role in this table does not prove its actual use without checking code.

Do not add a package to replace an existing project approach without an explicit feature-spec decision and approval.
