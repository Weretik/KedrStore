# «SEO-огляд» data

**Static code check:** 2026-11-26. **Modules:** Catalog, Content.

Two read-only endpoints feed the admin page «SEO-огляд». Each module reviews its own site pages per language and returns only the ones with an issue: `noText`, `noDescription`, `longTitle`, `longDescription`, `duplicateTitle`. The admin joins both lists.

## Structure and relationships

- Catalog: [GetCatalogSeoReviewHandler](../../../../src/Modules/Catalog/Catalog.Application/Features/Overview/Queries/GetCatalogSeoReview/GetCatalogSeoReviewHandler.cs) takes the shown categories open to search and their accepted models from the public model catalogue, reads their texts fresh, and applies [CatalogSeoChecks](../../../../src/Modules/Catalog/Catalog.Application/Features/Overview/Queries/GetCatalogSeoReview/CatalogSeoChecks.cs). `CanManageSiteCatalog`.
- Content: [GetContentSeoReviewHandler](../../../../src/Modules/Content/Content.Application/Features/Overview/Queries/GetContentSeoReview/GetContentSeoReviewHandler.cs) reviews published articles with the same limits. `CanManageContent`.

## Code structure

```text
src/Modules/Catalog/
├── Catalog.Api/Endpoints/Overview/GetAdminCatalogSeoReview/   # GET api/admin/catalog/seo-review
├── Catalog.Application/Features/Overview/Queries/GetCatalogSeoReview/
└── Catalog.Contracts/Overview/SeoReviewItemResponse.cs
src/Modules/Content/
├── Content.Api/Endpoints/Overview/GetAdminContentSeoReview/   # GET api/admin/content/seo-review
└── Content.Application/Features/Overview/Queries/GetContentSeoReview/
```

## Tests

- Integration: [catalog](../../../../tests/IntegrationTests/Catalog/Categories/CatalogSeoReviewApiTests.cs), [content](../../../../tests/IntegrationTests/Content/ContentSeoReviewApiTests.cs) — `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter "FullyQualifiedName~SeoReview"`.

## Related sources

- [Specification](../../specs/platform/010-seo-review/README.md)
- [Contract](../../contracts/openapi.json)
