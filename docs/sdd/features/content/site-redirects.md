# Site redirects

**Static code check:** 2026-11-26. **Module:** Content.

The admin «Переадресації»: old site address → new address. The site answers each old address with a permanent 301 before rendering anything. Managers, SEO specialists and Admins edit; the site reads the whole list anonymously and keeps it in memory for five minutes.

## Structure and relationships

[SiteRedirect](../../../../src/Modules/Content/Content.Domain/Redirects/SiteRedirect.cs) is one redirect; [SiteRedirectPath](../../../../src/Modules/Content/Content.Domain/Redirects/SiteRedirectPath.cs) stores the old address as the site looks it up (path, decoded, lower case, no trailing slash) and checks the new one (site path or full http(s) URL). [SiteRedirectRules](../../../../src/Modules/Content/Content.Application/Features/Redirects/Shared/SiteRedirectRules.cs) refuses a second redirect for an address and chains. A racing duplicate breaks `UX_SiteRedirects_FromPath`; `IContentUniqueSave` turns it into 409. The admin search runs in Infrastructure ([SiteRedirectSearch](../../../../src/Modules/Content/Content.Infrastructure/Persistence/Seo/SiteRedirectSearch.cs)): word by word, escaped `ILIKE`, trigram indexes, paged in SQL.

## Code structure

```text
src/Modules/Content/
├── Content.Api/Endpoints/Redirects/        # admin GET/POST, PUT/DELETE {id}; public GET redirects
├── Content.Application/
│   ├── Abstractions/Persistence/{IContentUniqueSave,ISiteRedirectSearch}.cs
│   └── Features/Redirects/{Commands,Queries,Shared}/
├── Content.Domain/Redirects/               # aggregate, id, address rules, errors
└── Content.Infrastructure/Persistence/
    ├── ContentUniqueSave.cs
    ├── Seo/{SiteRedirectConfiguration,SiteRedirectSearch}.cs
    └── Migrations/*_SiteRedirects.cs
```

## Data

Table `content."SiteRedirects"`; see the [database map](../../architecture/data-model/content-articles.md).

## Tests

- Unit: [addresses](../../../../tests/UnitTests/Content/Redirects/SiteRedirectPathTests.cs), [aggregate](../../../../tests/UnitTests/Content/Redirects/SiteRedirectTests.cs) — `dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~Content.Redirects"`.
- Integration: [HTTP, access, rules](../../../../tests/IntegrationTests/Content/Redirects/SiteRedirectsApiTests.cs) — `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter "FullyQualifiedName~SiteRedirects"`.

## Related sources

- [Specification](../../specs/content/013-site-redirects/README.md)
- [Contract](../../contracts/openapi.json)
