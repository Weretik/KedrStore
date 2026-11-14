# Offline app data snapshots

**Static code check:** 2026-11-05. **Modules:** Catalog, Sales.

Two read-only endpoints give the offline manager app «Кедр Менеджер» everything it keeps on the device: the whole sales catalog with all UAH prices, and all active customers with their price rules. The app replaces its local tables with each answer and asks again with the last `ETag`; the server answers `304` when nothing changed.

## Structure and relationships

- Catalog: [GetCatalogSnapshotHandler](../../../../src/Modules/Catalog/Catalog.Application/Features/MobileSync/Queries/GetCatalogSnapshot/GetCatalogSnapshotHandler.cs) reads products, prices, Ukrainian names, media links, categories and price types through `IReadCatalogDbContext` (query filters leave out deleted rows). `inStock` uses the sales catalog rule (hardware needs more than 2).
- Sales: [GetCustomersSnapshotHandler](../../../../src/Modules/Sales/Sales.Application/Features/MobileSync/Queries/GetCustomersSnapshot/GetCustomersSnapshotHandler.cs) reads counterparties and their category price rules through `IReadSalesDbContext`. The app prices a product like `CustomerPriceResolver`: the rule of the nearest category up the tree, otherwise the customer's default type. `debt` and `balance` stay `null`.
- Shared: [SnapshotVersion](../../../../src/BuildingBlocks/BuildingBlocks.Application/Snapshots/SnapshotVersion.cs) hashes the content without `generatedAt`; [SnapshotResult](../../../../src/BuildingBlocks/BuildingBlocks.Api/Snapshots/SnapshotResult.cs) sends it as a strong `ETag` and answers `304` on a matching `If-None-Match`.
- Host: [MobileSyncCompressionRegistrationExtensions](../../../../src/Bootstrapper/Host.Api/DependencyInjection/ServiceRegistration/Web/MobileSyncCompressionRegistrationExtensions.cs) compresses (Brotli, gzip) only `/api/mobile/sync/*`.
- Both endpoints require `CanManageOrders` (Manager, Seo, or Admin).

## Code structure

```text
src/BuildingBlocks/
├── BuildingBlocks.Application/Snapshots/SnapshotVersion.cs
└── BuildingBlocks.Api/Snapshots/SnapshotResult.cs
src/Modules/Catalog/
├── Catalog.Api/Endpoints/MobileSync/GetMobileCatalogSnapshotEndpoint.cs # GET /api/mobile/sync/catalog
├── Catalog.Application/Features/MobileSync/Queries/GetCatalogSnapshot/GetCatalogSnapshot{Query,Handler}.cs
└── Catalog.Contracts/MobileSync/CatalogSnapshotResponse.cs
src/Modules/Sales/
├── Sales.Api/Endpoints/MobileSync/GetCustomersSnapshotEndpoint.cs # GET /api/mobile/sync/customers
└── Sales.Application/Features/MobileSync/Queries/GetCustomersSnapshot/{GetCustomersSnapshotQuery,GetCustomersSnapshotHandler,CustomersSnapshotDto}.cs
src/Bootstrapper/Host.Api/DependencyInjection/ServiceRegistration/Web/MobileSyncCompressionRegistrationExtensions.cs
```

## Tests

- Integration: [catalog](../../../../tests/IntegrationTests/Catalog/Api/MobileCatalogSnapshotApiTests.cs), [customers](../../../../tests/IntegrationTests/Sales/Api/MobileCustomersSnapshotApiTests.cs) — `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter "FullyQualifiedName~SnapshotApiTests"` (in-memory, no Docker).
- Unit: [version hash](../../../../tests/UnitTests/BuildingBlocks/SnapshotVersionTests.cs) — `dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~SnapshotVersionTests"`.

## Related sources

- [Specification](../../specs/platform/004-mobile-offline-sync/README.md)
- Contracts: [catalog](../../contracts/catalog/mobile-sync.openapi.yaml), [customers](../../contracts/sales/mobile-sync.openapi.yaml)
