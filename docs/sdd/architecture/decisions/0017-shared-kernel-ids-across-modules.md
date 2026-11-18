# ADR-0017: Ids that other modules store live in the shared kernel

- Status: accepted
- Date: 2026-11-18 (owner's choice «Общее ядро» after the DDD audit)
- Owner: Виталий (project owner)

## Context

Every aggregate root has its own id type ([module rules, Domain model](../../standards/module-rules.md#domain-model-ddd)). A module also stores ids of other modules' aggregates: Sales and Work keep the signed-in user, Content and Catalog keep media-library files, Sales keeps catalogue products, categories and price types. These references were plain `Guid`, `long`, `int` or `string`, so the compiler could not stop a file id from going where a user id belongs.

A module may use another module only through its `Contracts` ([ADR-0004](0004-contracts-and-business-folders.md)), and `Contracts` carry plain values, not domain types. A module's `Domain` therefore cannot take the id type from the owner's `Domain`.

## Alternatives considered

- Plain values for references to another module: rejected by the owner; a rule that can be broken without the compiler noticing is a violation.
- Each module wraps the foreign id in a type of its own (`SalesUserId`, `ContentMediaFileId`): rejected; the same id gets several types, and nothing keeps them in step.
- Id types in the owner's `Contracts`, referenced by the other module's `Domain`: rejected; a `Domain` would depend on another module's project.
- Shared kernel (chosen): the id type lives in `BuildingBlocks.Domain`, which every `Domain` already references.

## Decision

- An id that another module stores is a `readonly record struct` in `src/BuildingBlocks/BuildingBlocks.Domain/Identifiers/` (namespace `BuildingBlocks.Domain.Identifiers`), written like the module ids: `Value`, a private constructor, `Create` (rejects an impossible value) and `FromStorage` (reads a stored or incoming value, which the domain then checks and answers with its typed error), `IComparable<T>` and `ToString()` of the value. The folder stays flat until it would pass six files; then it splits by meaning (`People/`, `Media/`, `Catalog/`).
- The owner module uses the same type for its own key (Media's `MediaFile.Id` is the kernel `MediaFileId`); an id that no other module stores stays beside its root.
- `BuildingBlocks.Infrastructure/Persistence/KernelIdConventions.AddKernelIds()` maps every kernel id to its plain column type; each `DbContext` that stores one calls it in `ConfigureConventions`. A column that holds another form keeps its own converter (Content's `CompanyRequisitesChanges.ChangedByUserId` stays the text form of the Guid). No column changes type; `ModelSnapshotTests` fails if a mapping change would alter the schema.
- `Contracts`, commands, outbox messages and DTOs keep plain values. Application wraps an incoming value (`MediaFileId.FromStorage(command.LogoMediaFileId)`), reads `.Value` in projections and in calls to another module's port, and compares typed ids inside queries.

## Why

- A wrong id is a compile error in every module, not only in the owner.
- One type per id, owned by no module, needs no project reference between modules.

## Consequences

- The move goes in stages, one pull request each. The [module rules](../../standards/module-rules.md#known-deviations) list what is left as a known deviation until the last stage merges.
- Stage 1 (2026-11-18): `UserId` (an ASP.NET Identity account, `Guid`) and `MediaFileId` (Media, `long`) are kernel types; Media's own copy is gone; user references in Sales (`Counterparty.IdentityUserId`), Work (`WorkTask.OwnerUserId`, `CreatedByUserId`, `WorkActivity.UserId`) and Content (`CompanyRequisitesChange.ChangedByUserId`), and file references in Catalog (product media, attribute swatches) and Content (article covers, gallery photos, brand logos, the share image) use them.
- Stage 2 (2026-11-18): `ProductId`, `ProductCategoryId` and `PriceTypeId` left Vogen for the kernel (`Create` rejects a code outside 1..max, as the Vogen validation did); Catalog's `CatalogConverter` is gone and `AddKernelIds` maps them; `CategoryPath` is hand-written too, so Vogen is no longer used. Sales customers' default price type and category price rules use them.
- Next: Sales order and draft lines store `ProductId` instead of text; their column is `varchar`, so the stored values are checked first.
