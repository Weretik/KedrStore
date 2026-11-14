# Module map

Every module has Api, Application, Contracts, Domain, and Infrastructure projects. Another module may use only its `Contracts` ([communication rules](../../standards/module-rules.md#communication-between-modules)). Rows below list what each `Contracts` project offers today.

## Catalog

Layers: Catalog.Api, Catalog.Application, Catalog.Contracts, Catalog.Domain, Catalog.Infrastructure.

`Catalog.Contracts` offers `ICatalogProductListReader`, `ICatalogSalesReader`, `ICatalogReferenceDataReader`, pricing options, and API DTOs; Catalog.Infrastructure implements them (`AddCatalogContracts()`).

Owns catalog products, categories, price types, translations, the product-list projection, order-creation integration, 1C synchronization, and public/admin catalog reads.

## Sales

Layers: Sales.Api, Sales.Application, Sales.Domain, Sales.Infrastructure.

Owns sales customers and counterparties, pricing policy, and sales integration with Catalog. It consumes Catalog only through `Catalog.Contracts` and Identity through `Identity.Contracts` (counterparty account provisioning).: products, stock, category parents, and prices through `ICatalogSalesReader`, category and price type names through `ICatalogReferenceDataReader`.

## Content

Layers: Content.Api, Content.Application, Content.Domain, Content.Infrastructure.

Owns site content written in the admin: articles, company contacts, requisites, the versioned site documents (offer, privacy, returns, delivery) and the site gallery now, other site texts later. Its tables live in the PostgreSQL schema `content` with their own migration history. Gallery photos are stored as `MediaFileId` values; Content.Infrastructure reads their URLs through `Media.Contracts` (no table, join or foreign key across modules).

## Media

Layers: Media.Api, Media.Application, Media.Contracts, Media.Domain, Media.Infrastructure.

Owns the picture library on the CDN: virtual folders, file records with CDN keys, uploads over FTP, and the CDN sync job. Its tables live in the PostgreSQL schema `media` with their own migration history; it does not reference other modules. Other modules link to library files by `MediaFileId` and read them through `IMediaFileLookup` in Media.Contracts (URL, file name, size), implemented by Media.Infrastructure.

## Work

Layers: Work.Api, Work.Application, Work.Domain, Work.Infrastructure.

Owns the staff members' own tasks and the automatic log of their admin changes (spec work/001). Its tables live in the PostgreSQL schema `work` with their own migration history; users are Identity user IDs as values. The log is written by `WorkActivityMiddleware` in Work.Api after every successful admin change, so other modules neither reference nor call Work.

## Identity

Layers: Identity.Api, Identity.Application, Identity.Contracts, Identity.Domain, Identity.Infrastructure.

Owns identity persistence, roles, authorization policies, session/current-user services, and seed data. `Identity.Contracts` holds `PolicyNames`, `RoleNames`, and `IImportedCounterpartyIdentityProvisioningService`; other modules reference only it. Business modules use permission/current-user abstractions rather than Identity EF types.

## BuildingBlocks

BuildingBlocks.Domain contains domain primitives. BuildingBlocks.Application contains Mediator behaviors, result/validation support, and domain-event abstractions. BuildingBlocks.Infrastructure contains shared adapter implementations. This is technical foundation, not a fourth business module.

Before adding or duplicating shared primitives, read the [BuildingBlocks guide](../platform/building-blocks/README.md).
