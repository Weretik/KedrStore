# Backend architecture

KedrStore is a modular backend built on ASP.NET Core and .NET 10. Current approaches include Clean Architecture, DDD, CQRS, Mediator, FluentValidation, Ardalis.Result, EF Core/PostgreSQL, and Serilog. The [NuGet package catalog](platform/nuget-packages.md) lists declared direct dependencies and versions.

## Browse by question

### Overview

- [System map](overview/system-map.md) — hosts, modules, and dependency direction.
- [Project structure](overview/project-structure.md) — solution tree and layers.
- [Layer boundaries](overview/layers.md) — responsibilities and forbidden dependencies.
- [Module map](overview/modules.md) — Catalog, Sales, Identity, and BuildingBlocks responsibilities.
- [Database map](data-model/README.md) — feature table trees and key relationships.
- [Product glossary](../../product/glossary.md) — catalog, doors, hardware, and Cosmos terms.

### Code placement by layer

- [Domain](layers/domain-structure.md)
- [Application](layers/application-structure.md)
- [Infrastructure](layers/infrastructure-structure.md)
- [API and Host](layers/api-host-structure.md)

### Request execution and integrations

- [Use-case flow](runtime/use-case-flow.md)
- [Mediator behaviors](runtime/mediator-behaviors.md)
- [Cross-cutting pipeline](runtime/cross-cutting.md)
- [Data and integrations](runtime/persistence-and-integrations.md)
- [1C integration](integrations/one-c/README.md)

### Platform

- [Technology stack](platform/technology-stack.md)
- [Direct NuGet dependencies](platform/nuget-packages.md)
- [Build and delivery tools](platform/tooling-and-delivery.md)
- [BuildingBlocks guide](platform/building-blocks/README.md)
- [Architectural decisions](decisions/README.md)

### Identity and access control

- [Identity, authentication, and authorization](security/README.md)

This section describes the current architecture and stable boundaries. A planned change belongs first in a [specification](../specs/README.md); after a stable decision is accepted and implemented, update the relevant architecture document and, if needed, an [ADR](decisions/README.md).
