# Search texts of the site pages

**Static code check:** 2026-11-05. **Module:** Content.

The search title and description, in Ukrainian and Russian, of the site pages that exist once: home, catalog, «Співпраця», delivery, returns, contacts, gallery, articles, requisites, offer, and privacy. Every value is optional; where none is set the site keeps the text it has in code. Managers and Admins save one page at a time; the site reads one language anonymously.

## Structure and relationships

[PageSeo](../../../../src/Content/Content.Domain/Seo/PageSeo.cs) is one row per edited page, keyed by a name from [SitePages](../../../../src/Content/Content.Domain/Seo/SitePages.cs) (the list and its menu order). Values are trimmed and a blank one is stored as null. Handlers live in `Features/SiteSeo/{Public,Admin}`; the first save of a page creates its row.

## Code structure

```text
src/Content/
├── Content.Api/
│   ├── Endpoints/SiteSeo/ public group        # public GET {lang}/page-seo
│   ├── Endpoints/SiteSeo/ admin group   # GET list, PUT {page}
│   └── SiteSeo/PageSeoResponses.cs              # payloads and mapping
├── Content.Application/Features/SiteSeo/
│   ├── Common/PageSeoDtos.cs
│   ├── Public/List/
│   └── Admin/{List,Update}/
├── Content.Domain/Seo/                          # page texts, the site pages, errors
└── Content.Infrastructure/
    ├── Configurations/PageSeoConfiguration.cs
    └── Migrations/*_AddPageSeo.cs
```

## Data

Table `content."PageSeo"`; see the [database map](../../architecture/data-model/content-articles.md).

## Tests

- Unit: [PageSeoTests](../../../../tests/UnitTests/Content/PageSeoTests.cs) — `dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~PageSeoTests"`.
- Integration: [HTTP, access, validation](../../../../tests/IntegrationTests/Content/PageSeoApiTests.cs) — `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter "FullyQualifiedName~PageSeo"`.

## Related sources

- [Specification](../../specs/content/010-page-seo/README.md)
- [Contract](../../contracts/content/page-seo.openapi.yaml)
