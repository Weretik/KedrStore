# Backend architecture

KedrStore is a modular ASP.NET Core backend on .NET 10. It uses Clean Architecture, DDD, CQRS, Mediator, FluentValidation, Ardalis.Result, EF Core/PostgreSQL and Serilog.

## Choose by question

### I need the big picture

- [System map](overview/system-map.md) — hosts, modules and dependency direction.
- [Project structure](overview/project-structure.md) — solution tree and layer visualisation.
- [Layer boundaries](overview/layers.md) — ownership and forbidden dependencies.
- [Module map](overview/modules.md) — Catalog, Sales, Identity and BuildingBlocks ownership.
- [Product glossary](../../product/glossary.md) — stable catalog, door, hardware, and Cosmos terminology.

### I need to place code in a layer

- [Domain](layers/domain-structure.md)
- [Application](layers/application-structure.md)
- [Infrastructure](layers/infrastructure-structure.md)
- [API and Host](layers/api-host-structure.md)

### I need to understand execution

- [Use-case flow](runtime/use-case-flow.md)
- [Mediator behaviors](runtime/mediator-behaviors.md)
- [Cross-cutting pipeline](runtime/cross-cutting.md)
- [Persistence and integrations](runtime/persistence-and-integrations.md)
- [OneC integration](integrations/one-c/README.md)

### I need platform reference

- [Technology stack](platform/technology-stack.md)
- [BuildingBlocks reference](platform/building-blocks/README.md)

### I need identity or access control

- [Identity, authentication and authorization](security/README.md)

Architecture documents describe durable rules. A planned change belongs in docs/specs, not here, unless it changes a durable rule.
