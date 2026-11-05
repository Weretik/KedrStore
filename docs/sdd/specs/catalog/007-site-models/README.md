# 007 — Site models and their attributes

**Module:** Catalog. **Status:** accepted 2026-11-05; stage 1 (this specification), stage 2 (models in the database and the admin API), stage 4 (admin) and stage 5 (public API) implemented 2026-11-05; stages 3 and 6–8 planned. **Date:** 2026-11-05.

The owner-facing plan with the audit, the screens, and every decision is the Russian document [«Кедр: модели и признаки — новый подход»](https://claude.ai/artifact/3NDXYvcRvtd7hWTTAgx7NT); the screen concept is the canvas [«Кедр: моделі та ознаки»](https://claude.ai/artifact/DYfVE9Kkxc6RGD293mjXNV). This file is the technical summary, the design, and the evidence log. It changes the direction of [spec 006](../006-attributes-variants-filters/README.md): phases 1–4 of 006 stay as they are and are reused; its phase 5 (filters per product) is reworked here into stage 5 (filters per model), and its phases 6–7 move to stages 7–8.

## Goal and boundaries

The public site is built from **models**. A model is one site page in one final category that brings together products of that category (each 1C product stays a separate product with its own code, price, and stock). What tells the products of a model apart are the model's **attributes** (color, size, side); they are the switchers on the model page and the filters of the category. Description, photos, and SEO texts belong to the model, not to each product.

Models and attributes are a layer for the site only. The 1C products, their names, and the admin «Товари» page stay as they were before spec 006 phase 3.

**Out of scope:** changing original 1C product names; models across categories; the Angular storefront; the mobile app.

**Term in code.** A model is the existing `ProductGroup` (table `CatalogProductGroups`); the code keeps that name, the admin and the site say «модель».

## Owner decisions (2026-11-05)

- **M-01.** Attribute problems and «не розшифровано» hints concern only products on the site (`ExportToSite`). Other 1C products have no problems.
- **M-02.** Every product on the site belongs to exactly one model. A model may have one product.
- **M-03.** A model lives in one final category and takes products of that category only.
- **M-04.** An on-site product outside an accepted model is still shown on the site as a model of its own, and «Огляд» shows it as a **hard error** that must be fixed at once.
- **M-05.** An on-site product of a model has at least one attribute value; a model of several products tells them apart by its attributes, with no repeated combination. Breaking this is an «Огляд» warning.
- **M-06.** Attributes and their values are one shared dictionary (spec 006); a model chooses which attributes are its switchers.
- **M-07.** SEO is kept per model (title, meta description, short and full description in Ukrainian and Russian); a model without them is an «Огляд» warning.
- **M-08.** Models are proposed automatically (semi-automatic): the base-name groups of spec 006 and the remaining on-site products become «Запропоновані моделі»; a person accepts them. Automation never changes an accepted model.
- **M-09.** A final category shows model cards with attribute filters; its parent shows all models of its children without attribute filters (only subcategory links and the common filters: stock, price, sale, new).
- **M-10.** The model page hides attribute values that no product combines with the current choice; an existing product out of stock says «Немає в наявності» and offers in-stock neighbours.
- **M-11.** The old product address answers with a permanent redirect (301) to its model with that product's attributes chosen; only the model address without a query is indexable.
- **M-12.** Search finds models by name, by the 1C codes, and by the full names of their products.
- **M-13.** In the admin, «Товари» stays the general 1C product management; the site management lives in separate menu items of «Каталог»: «Вітрина сайту» (on-site products with model, attributes, and «Чого бракує»), «Моделі», «Ознаки». «Огляд» lines link there.
- **M-14.** Migrations are checked on a clean test database; nothing is applied to production without the owner's «да».

## Rules

- **R-01.** A model has a state: `Proposed` (made by automation) or `Accepted` (a person accepted or created it). The migration marks groups a person created as accepted and the rest as proposed.
- **R-02.** The model build (the spec 006 group build) creates proposed models only. Besides base-name groups of two or more products it proposes a one-product model for each on-site product (not deleted, not No name) without a model. A proposed one-product model disappears when its product leaves the site or joins another model.
- **R-03.** Accepting a model pins its products: the build never moves, adds, or removes products of an accepted model, and never changes its name, axes, or texts. Accepting several models at once is allowed.
- **R-04.** A person creates a model in a final category (a category without children) with a name and products; it is accepted at once. Adding a product to a model checks the category and that the product is not in another accepted model (a proposed one gives it up). Removing a product pins it out of models.
- **R-05.** A model's axes are an ordered list of shared attributes (M-06). A proposed model without saved axes offers the attributes among color, size, and side whose confirmed values differ between its products; accepting saves the offered axes when none are saved.
- **R-06.** «Заповнити з назв» copies, for each product of the model and each axis without a value a person set, the confirmed automatic value the recognizer stored, as a manual value (spec 006 R-13). The person's click is the confirmation; codes nobody decoded or confirmed stay empty (spec 006 D-03).
- **R-07.** Model texts are stored per language (`uk`, `ru`): short description (≤ 500), full description (≤ 8000), SEO title (≤ 120), SEO description (≤ 320). Empty strings are stored as empty values.
- **R-08.** Health of the site, counted only for on-site, not deleted products:
  - `no_model` — not in an accepted model (hard error, M-04);
  - `no_attributes` — in an accepted model, but without a confirmed attribute value;
  - `missing_axis_value` — an axis of its accepted model has no confirmed value on it;
  - `same_combination` — another on-site product of the model has the same axis values;
  - for accepted models with on-site products: `model_no_axes` (two or more on-site products and no axes), `model_no_seo` (a short description or an SEO description is empty in `uk` or `ru`), `model_no_photo` (no on-site product of the model has a photo);
  - `proposed_waiting` — the number of proposed models with on-site products.
- **R-09.** «Огляд» gets these counts; the hard error is a separate field. The admin shows no line at zero.
- **R-10.** The admin model API is open to Manager and Admin (`CanManageProducts`); only an Admin changes a model slug (spec 006 R-12).

## Scenarios (stage 2)

- **SC-M01 (R-01, R-02).** Three products, two with one base name, all on the site: the build proposes one model of two and one model of one; a product not on the site gets no model; a second build changes nothing.
- **SC-M02 (R-03).** After «Прийняти» the build keeps the accepted model as it is, also when a product's base name changes.
- **SC-M03 (R-04).** A person creates a model in a final category with a product taken from a proposed model; a product of another category, a parent category, or a product in another accepted model is refused.
- **SC-M04 (R-05, R-06).** A proposed model of a bronze and a chrome product offers the color axis; accepting saves it; «Заповнити з назв» sets the stored colors as manual values and leaves a product without one empty.
- **SC-M05 (R-07, R-08).** Health counts: a product outside models (`no_model`), a model without SEO, two products with the same color (`same_combination`); filling the texts in both languages and fixing the color clears them.
- **SC-M06 (R-09, R-10).** Over HTTP: «Огляд» returns the counts; a Manager lists, accepts, and edits models; a Manager changing a slug gets 403.

## Design (stage 2)

### Data

Migration `CatalogSiteModels`:

| Table | Change |
| --- | --- |
| `CatalogProductGroups` | `Status` (int, not null, default 0 = proposed; 1 = accepted, set for `IsCreatedManually`), `AcceptedAt` (timestamptz null), `AcceptedBy` (varchar 256 null) |
| `CatalogProductGroupAxes` (new) | `GroupId` (FK cascade), `AttributeId` (FK restrict to `CatalogAttributes`), `Position`; key (`GroupId`, `AttributeId`) |
| `CatalogProductGroupTexts` (new) | `GroupId` (FK cascade), `Language` (varchar 2), `ShortDescription`, `Description`, `SeoTitle`, `SeoDescription`, `UpdatedAt`; key (`GroupId`, `Language`) |

Rollback: the migration's `Down` drops the two tables and the three columns; nothing else reads them before stage 4.

### Admin API (`api/admin/site-models`)

| Method and route | Purpose |
| --- | --- |
| `GET /api/admin/site-models` | Paged list: `categoryId`, `status`, `problem`, `search` (model name, product name, 1C code), `page`, `pageSize` |
| `GET /api/admin/site-models/{id}` | Model: axes, products with axis values, price, stock, problems; texts; characteristics |
| `POST /api/admin/site-models` | Create an accepted model (`categoryId`, `name`, `productIds`) |
| `PUT /api/admin/site-models/{id}` | Rename |
| `POST /api/admin/site-models/accept` | Accept the listed models (`ids`) |
| `PUT /api/admin/site-models/{id}/axes` | Set the axes (`attributeIds`, in order) |
| `POST /api/admin/site-models/{id}/products` | Add products (`productIds`) |
| `DELETE /api/admin/site-models/{id}/products/{productId}` | Take a product out |
| `PUT /api/admin/site-models/{id}/texts/{language}` | Save the texts of one language |
| `POST /api/admin/site-models/{id}/fill-from-names` | R-06 |
| `GET /api/admin/site-models/health` | Counts of R-08 (also inside «Огляд») |
| `GET /api/admin/site-models/showcase` | Stage 4: «Вітрина сайту», on-site products with model, variant values, problems (`categoryId`, `problem`, `search`, paging) |
| `GET /api/admin/site-models/categories` | Stage 4: final categories with on-site products, accepted and proposed models, problem count |

The main product (`PUT api/admin/product-groups/{id}/main`), the slug (`PUT api/admin/product-groups/{id}/slug`), and a product's attribute value (`PUT api/admin/products/{id}/attributes/{attributeId}`) keep their spec 006 routes. `api/admin/product-groups` stays until the admin moves to the new pages (stage 4) and is then removed.

Contract: [site-models.openapi.yaml](../../../contracts/catalog/site-models.openapi.yaml); «Огляд» counts in [admin-overview.openapi.yaml](../../../contracts/catalog/admin-overview.openapi.yaml).

### Files (stage 2)

```text
src/Catalog/
├── Catalog.Domain/
│   ├── Entities/Groups/ProductGroup.cs        # + Status, Accept
│   ├── Entities/Groups/ProductGroupAxis.cs, ProductGroupText.cs   # new: axis and texts rows
│   └── Enums/ProductGroupStatus.cs            # new
├── Catalog.Contracts/SiteModels/              # new: list, detail, requests, health DTOs
├── Catalog.Application/
│   ├── Groups/ProductGroupBuilder.cs          # + one-product proposals, accepted models untouched (R-02, R-03)
│   ├── Features/SiteModels/                   # new: GetList, Get, Create, Rename, Accept, SetAxes, AddProducts,
│   │                                          # RemoveProduct, SetTexts, FillFromNames, GetHealth; Common: state
│   │                                          # reader, state (axis offer, problems, health), mapper, card reader,
│   │                                          # changes (category checks, pins), problem codes, validation
│   └── Features/Overview/                     # + model health counts
├── Catalog.Api/Controllers/AdminSiteModelsController.cs   # new
└── Catalog.Infrastructure/
    ├── Configurations/Groups/                 # + axis and text mappings, status column
    └── Migrations/*_CatalogSiteModels.cs      # new
```

## Stage 5: public API

### Rules

- **R-11.** The site reads only on-site, not deleted products of public categories (not under the Cosmos import root) and only confirmed attribute values. An accepted model shows its on-site products of its own category; a model without on-site products is not shown.
- **R-12.** An on-site product outside an accepted model is a model of its own: its product slug, its product name in the requested language, no axes, no texts (M-04). When its slug equals an accepted model slug, the accepted model keeps the slug.
- **R-13.** A list keeps a model when one of its on-site products matches every filter: attribute values (one attribute any, different attributes all), stock, sale, new, price range. The card's price, stock, sale, and new count only the matching products; colors are listed when color is one of the model's axes.
- **R-14.** Attribute filters work in a final category only (M-09); a parent category refuses them (400) and lists its whole tree with the common filters. Filter counts are models per value of the category's filters («Фільтри категорій»), with the common filters and every other attribute's choice applied.
- **R-15.** The search (`GET models?search=`) keeps a model when every word is in the model name or in one product's Ukrainian or Russian name or 1C code; a search that is exactly the 1C code of an on-site product also returns that product's model address (M-12).
- **R-16.** The model page gives the axis values the products have and each product's value keys (`selection`, axis slug → value slug; several values join with `_`); the site hides combinations no product has and offers in-stock neighbours (M-10). Russian texts fall back to Ukrainian field by field.
- **R-17.** Addresses (M-11): a current or former product slug gives the product's model and its values; a former model slug or the slug of a product inside an accepted model, asked as a model, answers 404 with the address to redirect to. A product off the site or deleted gives 404 with its category, as the product page does.

### Scenarios

- **SC-M07 (R-12, R-13, R-14).** A final category with the accepted model «Завіса» (left in stock, right out of stock) and a handle outside accepted models lists both; `side:right` keeps only «Завіса» with the right hinge's price and «немає в наявності», and still counts the left side; adding «у наявності» empties the list; an unknown value slug is 404.
- **SC-M08 (R-14).** The parent lists the models of both children without filters; an attribute filter there is 400; an unknown category is 404.
- **SC-M09 (R-15).** Search by model name, by words of a product name, in Russian, and by an exact 1C code with its address.
- **SC-M10 (R-16, R-17).** Model page with axis, values, products, texts, breadcrumbs, and characteristics; a product of its own; a product slug of an accepted model gives the redirect address.
- **SC-M11 (R-17).** The address of an old product slug: an accepted model with the side chosen, a product of its own, a product off the site → its category.

### Public API (`api/catalog/{lang}`, anonymous)

| Method and route | Purpose |
| --- | --- |
| `GET /api/catalog/{lang}/{categorySlug}/models` | Model cards of a category tree: `filter` (final category only), `search`, `inStock`, `isSale`, `isNew`, `priceFrom`, `priceTo`, `sort` (`default`, `price_asc`, `price_desc`, `name_asc`, `new`), `page`, `pageSize` (≤ 100); `filters` with counts |
| `GET /api/catalog/{lang}/models` | The search over all models, same parameters without `filter`; `exactMatch` |
| `GET /api/catalog/{lang}/model/{modelSlug}` | The model page; 404 with an address body for a redirect |
| `GET /api/catalog/{lang}/product/{productSlug}/model` | The model address of an old product address |

Contract: [public-site-models.openapi.yaml](../../../contracts/catalog/public-site-models.openapi.yaml). No schema change. The product endpoints stay for the current Angular site. The URL words on the site (`/uk/catalog/<category>/<model>?<axis>=<value>`) come from the attribute and value slugs; stage 7 decides which single-filter pages are indexable.

### Files (stage 5)

```text
src/Catalog/
├── Catalog.Contracts/SiteModels/Public/          # new: request, list (card, filter, address), page DTOs
├── Catalog.Application/Features/SiteModels/Public/
│   ├── Common/                                   # new: PublicModelCatalog + reader + facts, page mapper
│   ├── GetList/                                  # new: query, validator, handler, filter, facets
│   ├── Get/                                      # new: model page
│   └── GetProductAddress/                        # new: product address → model
├── Catalog.Api/Controllers/SiteModelsController.cs   # new
└── Catalog.Infrastructure/DependencyInjection/   # + reader registration
```

**Residual risk.** Each request reads the on-site catalog (about 2.3 thousand products and their values) without a cache; the Next.js site caches pages (stage 6). A server cache is the next step if the load grows.

## Stage 4b: categories on the site and the showcase by model

Owner requests 2026-11-05 18:22–18:27 after using stage 4: a «show on site» mark on categories, «Моделі» and «Вітрина сайту» limited to what goes to the site, a category tree, and the showcase by model without row checkboxes. Owner answer: a new 1C category takes its parent's mark.

### Rules

- **R-18.** A category has the Admin mark «show on site» (`IsShownOnSite`). Hiding hides every subcategory; showing shows every subcategory and every parent; the Cosmos import root is never shown. The 1C sync never changes the mark; a new category takes its parent's, a new top category is shown only in the hardware root. The migration shows the whole «Фурнітура» root (1C `000005513`) once; doors and the rest start hidden. A product is on the site when it is exported from 1C, not deleted, and in a shown category: this holds for the public site (R-11), «Вітрина сайту», «Моделі», the model card, the health counts, and the «Огляд» counts of categories without texts and of attribute problems.
- **R-19.** «Вітрина сайту» lists one row per model with on-site products and one row per on-site product in no model; a row carries its on-site products. «Моделі» lists only models with on-site products, and the model card shows only on-site products. The category filter of both takes the category with its subcategories; the category list is a tree of shown categories with on-site products below them, with counts that include subcategories.

### Scenarios

- **SC-M12 (R-19).** The showcase lists the proposed one-product model with the hard error first, then the accepted model with both products and their values; the problem, search, and parent-category filters work; the category tree counts the parent and both children.
- **SC-M13 (R-18).** Hiding a category removes its products and models from the showcase, the models list, the category tree, and the health counts.
- **SC-M14 (R-18).** The 1C sync shows new hardware categories, hides new door categories, and gives a new child its hidden parent's mark; an Admin hides with subcategories, shows with parents, cannot show Cosmos; over HTTP a Manager gets 403 and a hidden category's public page is 404.

### Files (stage 4b)

```text
src/Catalog/
├── Catalog.Domain/Entities/ProductCategory.cs                 # + IsShownOnSite, SetShownOnSite
├── Catalog.Infrastructure/Migrations/*_CategorySiteVisibility.cs  # column + one-time «Фурнітура» start
├── Catalog.Application/
│   ├── Features/Category/SiteVisibility/                     # new: Admin command with subcategories and parents
│   ├── Features/Category/PublicCatalogScope.cs               # hidden = not shown or Cosmos
│   ├── Integrations/OneC/Jobs/SyncOneCCategoryJob.cs         # new category takes its parent's mark
│   ├── Features/SiteModels/Common/SiteModelState*.cs         # on site needs a shown category; category parents
│   ├── Features/SiteModels/GetShowcase/                      # rows by model
│   ├── Features/SiteModels/GetCategories/                    # category tree with rolled-up counts
│   └── Features/SiteModels/Public/Common/PublicModelCatalogReader.cs  # shown categories only
└── Catalog.Api/Controllers/AdminCategoriesController.cs      # PUT {id}/site-visibility (Admin)
```

## Stages

| Stage | Scope | Status |
| --- | --- | --- |
| 1 | This specification; owner plan and canvas | done 2026-11-05 |
| 2 | Models in the database, proposals, health, admin API | done 2026-11-05 (migration `CatalogSiteModels`) |
| 3 | Run the proposals on production (dry run, then on «да») | planned |
| 4 | Admin (kedr-web): «Товари» back to the 1C list; «Вітрина сайту», «Моделі», model card with SEO, «Ознаки»; «Огляд» lines; «Як це працює» help. Server: showcase and categories lists, `problem=any`, «Не розшифровані коди» count on-site products only (M-01) | done 2026-11-05 (KedrStore #176, kedr-web #65) |
| 4b | Categories «show on site» mark (migration `CategorySiteVisibility`), showcase by model, category tree, only on-site models and products in «Моделі» | in review |
| 5 | Public API: category models with filters (final category only), model page, model search, 301 from product addresses; replaces the reverted spec 006 phase 5 | done 2026-11-05 (no schema change) |
| 6 | Next.js site: category, parent with subcategory links, model page with live switchers, search | planned |
| 7 | SEO: model metadata, canonical, `ProductGroup` JSON-LD, model sitemap, single-filter pages | planned |
| 8 | Optional AI drafts for SEO texts and unrecognized attributes (a person confirms) | planned |

## Evidence (stage 2)

| Check | Result |
| --- | --- |
| `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter "FullyQualifiedName~IntegrationTests.Catalog\|FullyQualifiedName~ApiContractTests"` (local PostgreSQL for the PostgreSQL fixture) | 88 passed: SC-M01–SC-M05 in `SiteModelTests`, SC-M06 in `AdminSiteModelsApiTests`, the new routes in `ApiContractTests`; the builder and real-file tests updated for one-product proposals |
| `dotnet test tests/UnitTests/UnitTests.csproj` and `tests/ArchitectureTests` | 252 + 3 new (`SiteModelDomainTests`) and 10 passed |
| Real files (2330 products, all on the site in the test) | 395 base-name models with 1531 products and 763 one-product proposals; the 36 No name products stay outside; a second run changes nothing |
| Migration `CatalogSiteModels` on an empty local database | up, down to `CatalogAttributesAdmin` (2 tables and 3 columns gone), up again; `has-pending-model-changes` reports none |
| Migration on a local copy with the 395 production groups | all 395 marked proposed (none was created by a person) |

**Rollout note.** The full 1C sync runs the model build after recognition, so the first sync after deploy creates the one-product proposals on production by itself. They are proposals only: nothing on the site reads models before stage 5, and a proposal disappears when its product leaves the site or joins another model. Stage 3 then reviews the proposals with the owner.

## Evidence (stage 5)

| Check | Result |
| --- | --- |
| `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter "FullyQualifiedName~IntegrationTests.Catalog\|FullyQualifiedName~ApiContractTests"` with local PostgreSQL for the PostgreSQL fixture | 97 passed: SC-M07–SC-M11 in `PublicSiteModelTests`, the routes over HTTP in `PublicSiteModelsApiTests`, the reader and filters on real PostgreSQL in `PublicSiteModelPostgresTests`, the four paths in `ApiContractTests` |
| `dotnet test tests/ArchitectureTests` | 10 passed |
| `python3 tools/docs/check_doc_links.py` | links valid |

## Evidence (stage 4b)

| Check | Result |
| --- | --- |
| `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter "FullyQualifiedName~IntegrationTests.Catalog\|FullyQualifiedName~ApiContractTests"` with local PostgreSQL (migrations applied there) | 103 passed: SC-M12, SC-M13 in `SiteModelTests`, SC-M14 in `CategorySiteVisibilityTests` and `CatalogStableUrlsApiTests`; test categories now carry the mark |
| `dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~Catalog"` | 110 passed |
