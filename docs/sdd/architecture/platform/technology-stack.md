# Technology stack

| Concern | Current choice | Rule for new work |
| --- | --- | --- |
| Runtime | .NET 10 / C# | Follow nullable annotations and configured analyzers. |
| HTTP | ASP.NET Core controllers | Keep controllers thin and use Mediator. |
| Mediator | Mediator 3.x | Use request/handler pairs and the existing pipeline. |
| Mediator pipeline | RequestLogging, Exception, Performance, Validation, DomainEventDispatcher behaviors | Preserve registration order; do not put business rules into behaviors. |
| Validation | FluentValidation | Put input validators beside the use case. |
| Result contract | Ardalis.Result | Return Result or Result<T>; use established API mapping. |
| Data access | EF Core 10 + Npgsql/PostgreSQL | Keep EF configuration and migrations in Infrastructure. |
| Repositories | Ardalis.Specification | Reuse the existing repository/specification style where applicable. |
| Identity | ASP.NET Core Identity | Preserve role/policy and fallback-auth conventions. |
| Logging | Serilog | Structured, non-sensitive logs only. |
| Background work | existing job abstractions and Host.Jobs | Do not introduce direct scheduler coupling in Application. |
| External catalogue | 1C SOAP integration | Keep generated/client code behind Infrastructure adapters. |
| Tests | xUnit integration/unit/architecture projects | Add tests proportionally to the changed boundary. |

Do not introduce a package to replace an existing project convention without an explicit decision in the feature specification and approval.
