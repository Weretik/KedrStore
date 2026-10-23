# Module map

## Catalog

Layers: Catalog.Api, Catalog.Application, Catalog.Contracts, Catalog.Domain, Catalog.Infrastructure.

Owns catalog products, categories, price types, translations, the product-list projection, order-creation integration, 1C synchronization, and public/admin catalog reads.

## Sales

Layers: Sales.Api, Sales.Application, Sales.Domain, Sales.Infrastructure.

Owns sales customers and counterparties, pricing policy, and sales integration with Catalog. It consumes Catalog through contracts and read abstractions, not Catalog.Infrastructure types.

## Identity

Layers: Identity.Api, Identity.Application, Identity.Domain, Identity.Infrastructure.

Owns identity persistence, roles, authorization policies, session/current-user services, and seed data. Business modules use permission/current-user abstractions rather than Identity EF types.

## BuildingBlocks

BuildingBlocks.Domain contains domain primitives. BuildingBlocks.Application contains Mediator behaviors, result/validation support, and domain-event abstractions. BuildingBlocks.Infrastructure contains shared adapter implementations. This is technical foundation, not a fourth business module.

Before adding or duplicating shared primitives, read the [BuildingBlocks guide](../platform/building-blocks/README.md).
