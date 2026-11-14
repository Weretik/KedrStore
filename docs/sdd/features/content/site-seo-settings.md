# Site SEO settings

**Static code check:** 2026-11-12. **Module:** Content.

One set of site-wide SEO codes that every site page carries: the Google Analytics 4 measurement id, the Google Tag Manager container id, the Google Search Console and Bing Webmaster verification codes, and the picture shown when a page without its own picture is shared. Every value is optional; an empty one adds nothing to the site. The SEO specialist connects the services; staff with `CanManageContent` paste the codes in the admin «Налаштування SEO сайту», and the site reads them anonymously.

## Structure and relationships

The [public and admin endpoint groups](../../../../src/Modules/Content/Content.Api/Endpoints/SeoSettings/SeoSettingsEndpoints.cs) (one `<UseCase>Endpoint` per operation) dispatch to one query and one command. [SiteSeoSettings](../../../../src/Modules/Content/Content.Domain/Seo/SiteSeoSettings.cs) is a single-row aggregate (`Id = 1`, guarded by a check constraint) created empty by the migration and only replaced. Ids are upper-cased; for a verification code a pasted meta tag is reduced to its `content`. The picture is a media-library file ID read through `IMediaImageReader`; the save refuses a file that is not in the library, and a file deleted later is returned to the admin as `missing` and left out of the public answer.

Access: `GET /api/content/seo-settings` is anonymous; `/api/admin/content/seo-settings` needs `CanManageContent` (Manager, Seo, or Admin).

## Code structure

```text
src/Modules/Content/
├── Content.Api/
│   ├── Endpoints/SeoSettings/ public group       # public GET
│   ├── Endpoints/SeoSettings/ admin group  # admin GET and PUT
│   └── SeoSettings/SiteSeoSettingsResponses.cs        # responses, request, mapping
├── Content.Application/Features/SeoSettings/
│   ├── Common/SiteSeoSettingsDtos.cs                  # input, details, picture lookup
│   ├── Get/                                           # GetSiteSeoSettingsQuery
│   └── Update/                                        # command, handler, validator
├── Content.Domain/Seo/SiteSeoSettings*.cs             # aggregate, errors
└── Content.Infrastructure/
    ├── Configurations/SiteSeoSettingsConfiguration.cs
    └── Migrations/*_AddSiteSeoSettings.cs             # table + the empty row
```

## Data

Table `content."SiteSeoSettings"`; see the [database map](../../architecture/data-model/content-articles.md).

## Tests

- Unit: [SiteSeoSettingsTests](../../../../tests/UnitTests/Content/SiteSeoSettingsTests.cs) — `dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~SiteSeoSettingsTests"`.
- Integration: [HTTP, access, validation](../../../../tests/IntegrationTests/Content/SiteSeoSettingsApiTests.cs) — `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter "FullyQualifiedName~SiteSeoSettings"` (in-memory, no Docker).

## Related sources

- [Specification](../../specs/content/011-site-seo-settings/README.md)
- [Contract](../../contracts/content/seo-settings.openapi.yaml)
