# Admin «Огляд» data

**Static code check:** 2026-11-05. **Modules:** Sales, Catalog, Content.

Three read-only endpoints feed the admin home page «Огляд». Each module counts its own problems; the admin calls all three and shows a line only for a count above zero.

## Structure and relationships

- Sales: [WorkspaceOverviewReader](../../../../src/Sales/Sales.Infrastructure/Orders/Admin/Read/WorkspaceOverviewReader.cs) counts today's orders (Kyiv day from [WorkspaceDateRange](../../../../src/Sales/Sales.Application/Features/OrderWorkspace/Shared/WorkspaceDateRange.cs)), drafts and retryable 1C problems, and lists the newest orders.
- Catalog: [GetAdminCatalogOverviewHandler](../../../../src/Catalog/Catalog.Application/Features/Overview/Queries/GetAdminCatalogOverview/GetAdminCatalogOverviewHandler.cs) reads the product list projection and category texts with the same «filled» rule as the category list.
- Content: [GetAdminContentOverviewHandler](../../../../src/Content/Content.Application/Features/Overview/Queries/GetAdminContentOverview/GetAdminContentOverviewHandler.cs) counts article drafts, legacy covers and document drafts.

## Code structure

```text
src/Sales/
├── Sales.Api/Features/OrderWorkspace/AdminOrderWorkspaceController.cs  # GET overview
├── Sales.Application/Abstractions/Orders/{IWorkspaceOverviewReader,WorkspaceOverviewContracts}.cs
├── Sales.Application/Features/OrderWorkspace/Queries/GetWorkspaceOverview/
└── Sales.Infrastructure/Orders/Admin/Read/WorkspaceOverviewReader.cs
src/Catalog/
├── Catalog.Api/Features/Overview/AdminCatalogOverviewController.cs
├── Catalog.Application/Features/Overview/Queries/GetAdminCatalogOverview/
└── Catalog.Contracts/Overview/AdminCatalogOverviewResponse.cs
src/Content/
├── Content.Api/Features/Overview/AdminContentOverviewController.cs
└── Content.Application/Features/Overview/
```

## Tests

- Integration: [orders](../../../../tests/IntegrationTests/Sales/Persistence/WorkspaceOverviewReaderTests.cs), [catalog](../../../../tests/IntegrationTests/Catalog/Api/AdminCatalogOverviewApiTests.cs), [content](../../../../tests/IntegrationTests/Content/ContentOverviewApiTests.cs) — `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter "FullyQualifiedName~Overview"`.
- Unit: [Kyiv day](../../../../tests/UnitTests/Sales/Application/WorkspaceDateRangeTests.cs) — `dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~WorkspaceDateRangeTests"`.

## Related sources

- [Specification](../../specs/platform/003-admin-overview/README.md)
- Contracts: [orders](../../contracts/sales/admin-overview.openapi.yaml), [catalog](../../contracts/catalog/admin-overview.openapi.yaml), [content](../../contracts/content/admin-overview.openapi.yaml)
