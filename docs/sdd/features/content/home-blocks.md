# Home page FAQ and brands

**Static code check:** 2026-11-04. **Module:** Content (logos read from Media).

The two editable lists of the site home page: the FAQ (question and answer in Ukrainian and Russian) and the brands (name and a logo from the media library), each in the order the admin sets. Managers and Admins edit them; the site reads both in one anonymous request. The six questions the site kept in code were moved into the database by a data migration; brands start empty.

## Structure and relationships

[HomeFaqItem](../../../../src/Content/Content.Domain/Home/HomeFaqItem.cs) and [HomeBrand](../../../../src/Content/Content.Domain/Home/HomeBrand.cs) are separate aggregates with a position. A brand stores `LogoMediaFileId` (a value, not a foreign key); its URL comes through the shared port [IMediaImageReader](../../../../src/Content/Content.Application/Contracts/MediaLibrary/IMediaImageReader.cs), the same one the gallery uses. A brand whose logo file was deleted is hidden on the site and marked `logoMissing` in the admin.

Handlers live in `Features/Home/{Public,Admin}`; every admin change answers with both lists ([HomeAdminBlocks](../../../../src/Content/Content.Application/Features/Home/Common/HomeAdminBlocks.cs)):

- FAQ: create (to the end), update, delete, reorder (a stale list answers 409);
- brands: the same, and a logo that is not in the library answers 400.

## Code structure

```text
src/Content/
├── Content.Api/
│   ├── Controllers/HomeBlocksController.cs        # public GET {lang}/home
│   ├── Controllers/AdminHomeBlocksController.cs   # GET; faq, faq/{id}, faq/order; brands, brands/{id}, brands/order
│   └── Home/HomeResponses.cs                      # responses, requests, mapping
├── Content.Application/Features/Home/
│   ├── Common/                                    # DTOs, admin loader and reorder helper, validators
│   ├── Public/GetBlocks/
│   └── Admin/{GetBlocks,Faq/{Create,Update,Delete,Reorder},Brands/{Create,Update,Delete,Reorder}}/
├── Content.Domain/Home/                           # FAQ item, brand, guard, errors
└── Content.Infrastructure/
    ├── Configurations/HomeFaqItemConfiguration.cs, HomeBrandConfiguration.cs
    ├── Seed/LegacyHomeFaqSeed.cs                  # the six site questions
    └── Migrations/*_AddHomeBlocks.cs, *_SeedHomeFaq.cs
```

## Data

Tables `content."HomeFaqItems"` and `content."HomeBrands"`; see the [database map](../../architecture/data-model/content-articles.md).

## Tests

- Unit: [HomeBlocksTests](../../../../tests/UnitTests/Content/HomeBlocksTests.cs) — `dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~UnitTests.Content"`.
- Integration: [HTTP, access, FAQ and brand editing](../../../../tests/IntegrationTests/Content/HomeBlocksApiTests.cs), [PostgreSQL seed](../../../../tests/IntegrationTests/Content/HomeBlocksPostgresTests.cs) — `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter "FullyQualifiedName~IntegrationTests.Content"`.

## Related sources

- [Specification](../../specs/content/006-home-blocks/README.md)
- [Contract](../../contracts/content/home.openapi.yaml)
- [Site gallery](gallery.md) (shares the media picture port)
