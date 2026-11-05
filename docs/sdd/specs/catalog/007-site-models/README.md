# 007 — Site models and their attributes

**Module:** Catalog. **Status:** accepted 2026-11-05; stage 1 (this specification) and stage 2 (models in the database and the admin API) implemented 2026-11-05; stages 3–8 planned. **Date:** 2026-11-05.

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

## Stages

| Stage | Scope | Status |
| --- | --- | --- |
| 1 | This specification; owner plan and canvas | done 2026-11-05 |
| 2 | Models in the database, proposals, health, admin API | done 2026-11-05 (migration `CatalogSiteModels`) |
| 3 | Run the proposals on production (dry run, then on «да») | planned |
| 4 | Admin (kedr-web): «Товари» back to the 1C list; «Вітрина сайту», «Моделі», model card with SEO, «Ознаки»; «Огляд» lines; «Як це працює» help | planned |
| 5 | Public API: category models with filters (final category only), model page, model search, 301 from product addresses; replaces KedrStore PR #175 | planned |
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
