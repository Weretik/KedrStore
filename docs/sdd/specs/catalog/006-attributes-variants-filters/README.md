# 006 — Catalog attributes, variant groups, filters, and SEO

**Module:** Catalog. **Status:** phases 1 (model + import) and 2 (groups) implemented 2026-11-05; phases 3–7 planned. **Date:** 2026-11-05.

The owner-facing plan with every decision is the Russian document [«Каталог: атрибути, варіанти, фільтри і SEO — SDD»](https://claude.ai/artifact/5QwjXyutiyQg4FyzhdaPJR). This file is its technical summary and the evidence log. Input files and their description: [docs/catalog-attributes](../../../../catalog-attributes/README.md).

## Goal and boundaries

Products get structured attributes (color, size, side, type, material, and so on) recognized from their 1C names. Products with the same base name in one category form a variant group: one card in the site listing and variant switchers on the product page. The new Next.js site gets per-category filters with counts and indexable single-filter pages.

**Out of scope:** changing original 1C product names (owner rule), the Angular storefront, the mobile app, merging variants into one product (each color, size, and side stays a separate product with its own 1C code, price, stock, and page).

## Owner decisions (2026-11-05)

- **D-01.** Original product names never change; the base name is stored separately.
- **D-02.** Variant axes are color, size, and side in every category. Group key: category + base name.
- **D-03.** Unconfirmed or undecoded codes (AL, ZCN, BK, 2BB, SB/PB, size axes) are imported with a review reason and never decoded by guess. Assistant-picked decodings are stored as not confirmed.
- **D-04.** Attributes are imported for every product, including deleted and not exported ones.
- **D-05.** «Прийняти як є» removes the problem permanently, also after 1C changes; only a manual reopen brings it back.
- **D-06.** No name products are always problems (reason `no_name`) and never join groups automatically; «Огляд» shows them as a separate line.
- **D-07.** The canonical URL of a variant is the main product of its group.
- **D-08.** A single-filter page is indexable from 2 cards (one setting).
- **D-09.** Migrations are checked on a clean test database; nothing is applied to production without the owner's «да».

## Rules

- **R-01.** Attributes, values, aliases, and category filters live in the database (tables `CatalogAttributes`, `CatalogAttributeValues`, `CatalogAttributeAliases`, `CatalogCategoryAttributes`), not in code. An alias without a value is a known undecoded code.
- **R-02.** A product attribute value (`CatalogProductAttributeValues`) has a source: import, parser, or manual. Manual or locked values are never changed or removed by the import, the parser, or the 1C sync.
- **R-03.** The import is idempotent: a second run with the same files writes nothing. Without `--apply` it only reports.
- **R-04.** A row of the CSV with `needs_review=true` is imported; the product's review (`CatalogProductAttributeReviews`) keeps the reason codes and the recognizer's text. An accepted review stays accepted (D-05).
- **R-05.** The import offers a filter in a category when the category's products have at least two different values of a filterable attribute. Existing category filters are never changed by the import.
- **R-06.** Values decoded only by an assistant are stored with `IsConfirmed=false`; the site does not show them in filters until a person confirms the alias.

- **R-07.** A variant group (`CatalogProductGroups`) is found by category and group key: the base name with single spaces, compared without case. It exists when at least two products share the key; a single product has no group. A product belongs to at most one group (`CatalogProductGroupMembers`).
- **R-08.** No name products (D-06) and deleted products never join a group automatically. A person can still put them into a group.
- **R-09.** A row a person set (move, «Видалити з групи», new group) is pinned: the import and the group build never move it. «Повернути автоматичне» unpins it. An automatic group without products disappears; a group a person created stays.
- **R-10.** The main product of a group stays while it is in the group and on the site, also when a person chose it. Otherwise the build takes the best product: on the site, with a photo, in stock, then the smallest 1C code. A person can choose only a product that is on the site.
- **R-11.** Products of one group that color (main and second), size, and side do not tell apart get the review reason `same_variants` («Однакові варіанти») with the matching product codes. The group build owns this reason: the recognizer import keeps it, and it disappears when the variants differ again. An accepted product stays accepted (D-05).
- **R-12.** Group slugs come from the group name (KMU-2010), live in the same slug history as products and categories (type 3), and only an Admin changes them. Renaming a group keeps its slug.

## Scenarios (phase 1)

- **SC-A01 (R-03).** Without `--apply` the import reports counts and writes nothing.
- **SC-A02 (R-01, R-03).** With `--apply` the dictionary, values, aliases, reviews, and category filters are stored; a second run reports only «unchanged».
- **SC-A03 (R-02).** A manual value survives a reimport (counted as skipped); an automatic value missing from the new file is removed.
- **SC-A04 (R-04, D-05).** An accepted review stays accepted after a reimport that still finds a problem.
- **SC-A05 (D-06).** A product with brand marking `NO_NAME` gets the review reason `no_name`.
- **SC-A06.** A CSV row for a product missing from the catalog is reported; the other rows are imported.

## Scenarios (phase 2)

- **SC-G01 (R-07).** Products with the same base name (spaces and case aside) in one category form one group named after the base name; a second run changes nothing; a dry run writes nothing.
- **SC-G02 (R-08, R-09).** A No name product stays out; a deleted product leaves its group, and a group left without products disappears.
- **SC-G03 (R-10).** The product on the site and in stock becomes main; a person's choice stays until that product leaves the site; a product not on the site cannot be chosen.
- **SC-G04 (R-11).** Two variants with the same color and side get «Однакові варіанти»; a reimport keeps the reason and an accepted product stays accepted; once the side differs, the reason is cleared.
- **SC-G05 (R-09).** A new group a person creates pins its products; products of another category are refused; a product taken out of groups stays out after a reimport; back to automatic it regroups at once.

## Phases

| Phase | Scope | Status |
| --- | --- | --- |
| 1 | Model, migration `CatalogAttributes`, import job `import-catalog-attributes` | done 2026-11-05 |
| 2 | Product groups (`ProductGroup` slug type 3), main product, «Однакові варіанти», group API | done 2026-11-05 |
| 3 | Admin: «На сайті» attributes and group panel, problems filter, «Прийняти як є», «Властивості» page, «Огляд» lines | planned |
| 4 | .NET parser port with a golden test against the CSV (2330/2330), runs on 1C sync and dictionary changes | planned |
| 5 | Facet API and product list `filter`/`groupVariants`; site filter panel and variant switchers | planned |
| 6 | Single-filter pages, templates, noindex/canonical, redirects, sitemap, `ProductGroup` JSON-LD | planned |
| 7 | Optional AI suggestions for problem products (person confirms) | planned |

## Code (phase 1)

```text
src/Catalog/
├── Catalog.Domain/
│   ├── Entities/Attributes/  # CatalogAttribute, AttributeValue, AttributeAlias, CategoryAttribute,
│   │                         # ProductAttributeValue, ProductAttributeReview (+ reason)
│   ├── Enums/AttributeKind.cs, AttributeValueSource.cs, AttributeReviewState.cs
│   └── ValueObjects/AttributeId.cs, AttributeValueId.cs, AttributeAliasId.cs
├── Catalog.Application/
│   ├── Attributes/Import/    # file readers (dictionary JSON, CSV), built-in attribute settings, models
│   └── Jobs/Attributes/ImportCatalogAttributesJob.cs  # dictionary → products → category filters, report
└── Catalog.Infrastructure/
    ├── Configurations/Attributes/  # EF mappings
    └── Migrations/20261105065843_CatalogAttributes.cs  # new tables only
```

## Code (phase 2)

```text
src/Catalog/
├── Catalog.Domain/Entities/Groups/   # ProductGroup, ProductGroupMember; CatalogEntityType.ProductGroup = 3
├── Catalog.Application/
│   ├── Groups/                       # ProductGroupBuilder (R-07–R-11), ProductGroupKey, report
│   └── Features/Groups/              # admin queries and commands; Common: reader, pins, validation errors
├── Catalog.Api/Controllers/AdminProductGroupsController.cs  # + GET/PUT /api/admin/products/{id}/group
└── Catalog.Infrastructure/
    ├── Configurations/Groups/        # EF mappings
    └── Migrations/*_CatalogProductGroups.cs  # new tables only
```

Contract: [product-groups.openapi.yaml](../../../contracts/catalog/product-groups.openapi.yaml). The import job ends with the group build, so the existing Cloud Run jobs `catalog-attributes-import-dry-run` and `-apply` also build the groups.

## Evidence (phase 2, 2026-11-05)

| Check | Result |
| --- | --- |
| `dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~UnitTests.Catalog"` | 91 passed (4 new: key, main, pins, «Однакові варіанти» reason) |
| `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter "FullyQualifiedName~IntegrationTests.Catalog"` | 43 passed, 4 skipped (Docker-only); SC-G01–SC-G05 and the real files: 395 groups, 1531 products, 33 groups with «Однакові варіанти», a second run with zero changes |
| `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter "FullyQualifiedName~ApiContractTests"` | 13 passed; the new admin routes are in the generated OpenAPI |
| `dotnet test tests/ArchitectureTests/ArchitectureTests.csproj` | 10 passed |
| Clean PostgreSQL 16 test database with the phase 1 data: `dotnet ef database update`, then the import dry run and `--apply` twice | migration applied (2 new tables); dry run and first apply: 395 groups, 1531 members, 33 groups (84 products) with «Однакові варіанти», 0 errors; second apply: all unchanged. The admin group reader and the group search ran against this database |

The expected 403 groups and 37 «Однакові варіанти» groups of the plan counted No name products; without them (D-06) the data gives 395 and 33.

## Evidence (phase 1, 2026-11-05)

| Check | Result |
| --- | --- |
| `dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~UnitTests.Catalog"` | 87 passed (6 new: CSV reader, size code, no-name reason, accepted review, manual value) |
| `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter "FullyQualifiedName~IntegrationTests.Catalog.Attributes"` | 5 passed: SC-A01–SC-A06, real files without errors and a second run with zero changes |
| `dotnet test tests/ArchitectureTests/ArchitectureTests.csproj` | 10 passed |
| `dotnet ef migrations has-pending-model-changes` (Catalog) | no changes |
| Clean PostgreSQL 16 database: `dotnet ef database update` (all Catalog migrations) | applied; 6 new tables |
| Same database seeded with the 2330 CSV products: `--job=import-catalog-attributes`, then `--apply` twice | dry run and first apply: 52 attributes, 615 values, 688 aliases, 7874 product values, 2330 reviews (1639 need review), 147 category filters, 0 errors; second apply: everything unchanged |

Not done here: applying the migration and the import to production. Both wait for the owner's «да» (D-09).
