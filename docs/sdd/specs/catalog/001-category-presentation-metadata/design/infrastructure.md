# Category presentation metadata — design: Infrastructure

## Persistence and integrations

EF Core mapping in `src/Catalog/Catalog.Infrastructure/Configurations/ProductCategoryConfiguration.cs` persists the metadata defined in [the data model](../data-model.md). The migration is added under `src/Catalog/Catalog.Infrastructure/Migrations/`; applied migrations are never edited.

A typed options model, adjacent to `RootCategoryId` in `src/Catalog/Catalog.Application/Integrations/OneC/Options/`, holds approved presentation records. Both host configuration files bind identical approved metadata:

- `src/Bootstrapper/Host.Api/appsettings.json`
- `src/Bootstrapper/Host.Jobs/Host.Jobs/appsettings.json`

`SyncOneCCategoryJob` resolves metadata after its manual hierarchy has been built, upserts categories, and keeps the current empty-response deletion guard. Its existing Cosmos branch continues to create/update the local virtual category without invoking the SOAP categories method. `SyncOneCProductDetailsJob` continues to assign the configured Cosmos category ID directly.

## Migration and rollout

The migration adds the new columns with an explicit, reversible backfill policy. Existing rows receive `Name` as a temporary short-name fallback only where no approved configured value exists. `SortOrder` and `Level` must receive deterministic values before columns become non-nullable, if that is the agreed model.

Deploy the migration before the first import that writes metadata. Run one non-overlapping full import, validate all three roots against the frozen snapshots, and retain storefront constants until the agreed consumer contract is live. Test migration application against a disposable PostgreSQL database.

Do not duplicate fields or relationships here; they belong in `data-model.md`.
