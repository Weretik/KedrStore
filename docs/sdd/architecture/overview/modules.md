# Module map

Every module has Api, Application, Contracts, Domain, and Infrastructure projects. Another module may use only its `Contracts` ([communication rules](../../standards/module-rules.md#communication-between-modules)). Each section lists the module's projects, its aggregates (only the roots carry `IAggregateRoot` and get a repository; children change through the root, [DDD rules](../../standards/module-rules.md#domain-model-ddd)), and what its `Contracts` project offers today.

## Catalog

Layers: Catalog.Api, Catalog.Application, Catalog.Contracts, Catalog.Domain, Catalog.Infrastructure.

Aggregates (root → children): `Product` → translations; `ProductCategory`; `ProductGroup` (site model) → axes; `CatalogAttribute` → values; `AttributeAlias` → category values and meanings; `PriceType`. Own records edited on their own: `ProductPrice`, `CategoryContent`, `ProductGroupText`, `ProductGroupMember`, `CategoryAttribute`, `ProductAttributeValue`, `ProductAttributeReview`; append-only log: `SlugHistoryEntry`.

`Catalog.Contracts` offers `ICatalogProductListReader`, `ICatalogSalesReader`, `ICatalogReferenceDataReader`, pricing options, and API DTOs; Catalog.Infrastructure implements them (`AddCatalogContracts()`).

Owns catalog products, categories, price types, translations, the product-list projection, order-creation integration, 1C synchronization, and public/admin catalog reads.

## Sales

Layers: Sales.Api, Sales.Application, Sales.Contracts, Sales.Domain, Sales.Infrastructure.

Aggregates (root → children): `Order` → lines; `OrderDraft` → lines; `Counterparty`. Own records written by the 1C sync: `CounterpartyCategoryPriceType`.

`Sales.Contracts` offers nothing yet; no other module uses Sales.

Owns sales customers and counterparties, pricing policy, manager and site orders, the order workspace, and sending orders to 1C (outbox lane `one-c-orders`) and Telegram. It consumes Catalog only through `Catalog.Contracts` (products, stock, category parents, and prices through `ICatalogSalesReader`; category and price type names through `ICatalogReferenceDataReader`) and Identity through `Identity.Contracts` (counterparty account provisioning).

## Content

Layers: Content.Api, Content.Application, Content.Contracts, Content.Domain, Content.Infrastructure.

Aggregates (root → children): `Article` → legacy cover; `CompanyContacts`; `CompanyRequisites`; `LegalDocumentVersion`; `GalleryItem`; `HomeBrand`; `HomeFaqItem`; `PageSeo`; `SiteSeoSettings`; `WholesalePage`. Append-only log: `CompanyRequisitesChange`.

`Content.Contracts` offers nothing yet; no other module uses Content.

Owns site content written in the admin: articles, company contacts, requisites, the versioned site documents (offer, privacy, returns, delivery) and the site gallery now, other site texts later. Its tables live in the PostgreSQL schema `content` with their own migration history. Gallery photos are stored as `MediaFileId` values; Content.Infrastructure reads their URLs through `Media.Contracts` (no table, join or foreign key across modules).

## Media

Layers: Media.Api, Media.Application, Media.Contracts, Media.Domain, Media.Infrastructure.

Aggregates: `MediaFile`, `MediaFolder`.

Owns the picture library on the CDN: virtual folders, file records with CDN keys, uploads over FTP, and the CDN sync job. Its tables live in the PostgreSQL schema `media` with their own migration history; it does not reference other modules. Other modules link to library files by `MediaFileId` and read them through `IMediaFileLookup` in Media.Contracts (URL, file name, size), implemented by Media.Infrastructure.

## Work

Layers: Work.Api, Work.Application, Work.Contracts, Work.Domain, Work.Infrastructure.

Aggregates (root → children): `WorkTask` → steps. Append-only log: `WorkActivity`.

`Work.Contracts` offers nothing yet; no other module uses Work.

Owns the staff members' own tasks and the automatic log of their admin changes (spec work/001). Its tables live in the PostgreSQL schema `work` with their own migration history; users are Identity user IDs as values. The log is written by `WorkActivityMiddleware` in Work.Api after every successful admin change, so other modules neither reference nor call Work.

## Scheduling

Layers: Scheduling.Api, Scheduling.Application, Scheduling.Contracts, Scheduling.Domain, Scheduling.Infrastructure.

Aggregates: `TaskSchedule`. Append-only journal: `ScheduledTaskRun`. `ScheduledTask` is the code list of tasks, not stored.

`Scheduling.Contracts` offers `IScheduledTaskRunJournal`, which `Host.Jobs` uses to mark a run started and record its result.

Owns the background tasks «Фонові задачі»: the code list of tasks, their schedules in Kyiv time, the scheduler tick that runs the outbox and starts the Cloud Run Job `kedr-tasks`, and the run journal. Its tables live in the PostgreSQL schema `scheduling`. The task runners themselves are the existing Catalog, Sales and Media jobs in `Host.Jobs`.

## Identity

Layers: Identity.Api, Identity.Application, Identity.Contracts, Identity.Domain, Identity.Infrastructure.

Owns identity persistence, roles, authorization policies, session/current-user services, and seed data. `Identity.Contracts` holds `PolicyNames`, `RoleNames`, and `IImportedCounterpartyIdentityProvisioningService`; other modules reference only it. Business modules use permission/current-user abstractions rather than Identity EF types.

## BuildingBlocks

BuildingBlocks.Domain contains domain primitives. BuildingBlocks.Application contains Mediator behaviors, and result/validation support. BuildingBlocks.Infrastructure contains shared adapter implementations. This is technical foundation, not a fourth business module.

Before adding or duplicating shared primitives, read the [BuildingBlocks guide](../platform/building-blocks/README.md).
