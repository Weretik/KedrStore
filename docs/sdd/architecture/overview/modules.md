# Module map

## Catalog

Layers: Catalog.Api, Catalog.Application, Catalog.Contracts, Catalog.Domain, Catalog.Infrastructure.

Owns catalogue products, categories, price types, translations, product list projection, order creation integration, 1C synchronization and public/admin catalogue reads.

## Sales

Layers: Sales.Api, Sales.Application, Sales.Domain, Sales.Infrastructure.

Owns sales customers/counterparties, price policy and sales catalogue integration. It consumes Catalog through contracts/reader abstractions, not Catalog infrastructure types.

## Identity

Layers: Identity.Api, Identity.Application, Identity.Domain, Identity.Infrastructure.

Owns identity persistence, roles, authorization policies, session/current-user services and seed data. Business modules consume permissions/current-user abstractions rather than Identity EF types.

## BuildingBlocks

BuildingBlocks.Domain provides domain primitives. BuildingBlocks.Application provides mediator behaviours, result/validation support and domain-event abstractions. BuildingBlocks.Infrastructure provides shared adapter implementations. It is shared plumbing, not a fourth business module.

Read the [BuildingBlocks reference](../platform/building-blocks/README.md) before introducing or duplicating shared primitives.
