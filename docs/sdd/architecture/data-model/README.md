# KedrStore database map

**Checked:** 2026-11-04 against the four EF Core `DbContext` types, configurations, and snapshots under `src/`. This is the model structure in the repository, not confirmation that migrations have been applied in a particular environment.

Catalog, Sales, and Identity contexts use tables without an explicitly configured PostgreSQL schema. Therefore the model does not define separate schemas for them: their table names use the connection's default schema. With a standard `search_path`, that is `public`; the settings of any deployed database were not checked here.

```text
KedrStore PostgreSQL
└── default schema (usually public)
    ├── Catalog / CatalogDbContext — 10 tables
    │   ├── Categories
    │   │   ├── ProductCategories          # category tree, slug, names, soft delete
    │   │   ├── CategoryContents           # SEO text per category and language
    │   │   ├── CategoryFaqItems           # FAQ of one content row
    │   │   └── CatalogSlugHistory         # former product and category slugs
    │   ├── Products and prices
    │   │   ├── Products                   # product and CategoryId, photo/scheme cache
    │   │   ├── ProductMedia               # photo and scheme links to media files
    │   │   ├── ProductPrices              # product prices
    │   │   ├── ProductTranslations        # localized names
    │   │   └── PriceType                  # price types
    │   └── List projection
    │       └── ProductListProjections     # denormalized product reads
    ├── Sales / SalesDbContext — 10 tables
    │   ├── Customers
    │   │   ├── Counterparties             # counterparties
    │   │   └── CounterpartyCategoryPriceTypes # price type by category
    │   └── Orders and 1C
    │       ├── Orders                     # orders
    │       ├── OrderLines                 # order lines
    │       ├── OneCOrderSyncs             # 1C sync state
    │       ├── OrderIdempotencyRecords   # duplicate request protection
    │       ├── OrderSyncRetryAudits      # retry audit
    │       ├── OrderDrafts               # saved drafts without order number
    │       ├── OrderDraftLines           # draft lines
    │       └── OrderDraftIdempotencyRecords # duplicate draft creation protection
    ├── Identity / AppIdentityDbContext — 8 tables
    │   └── Access and sessions
    │       ├── AspNetUsers               # users
    │       ├── AspNetRoles               # roles
    │       ├── AspNetUserRoles           # role assignments
    │       ├── AspNetUserClaims          # user claims
    │       ├── AspNetRoleClaims          # role claims
    │       ├── AspNetUserLogins          # external logins
    │       ├── AspNetUserTokens          # tokens
    │       └── AppRefreshSessions       # refresh sessions
    ├── infrastructure table
    │   └── __EFMigrationsHistory         # applied EF migration history
└── schema sales
    └── Sales / SalesDbContext — 1 table
        └── OutboxMessages                # messages sent after the commit (platform spec 006)
└── schema content
    └── Content / ContentDbContext — 8 tables
        ├── Articles                      # slug, date, publication flag, cover version
        ├── ArticleTranslations           # uk/ru title, excerpt, sanitized body
        ├── ArticleCovers                 # cover WebP bytes
        ├── CompanyContacts               # the one row of site contacts
        ├── CompanyRequisites             # the one row of payment requisites
        ├── CompanyRequisitesChanges      # who changed requisites, when, old → new (jsonb)
        ├── LegalDocumentVersions         # offer/privacy/returns/delivery editions: draft, published, archived
        ├── LegalDocumentTexts            # uk/ru title and sanitized body of an edition
        └── __EFMigrationsHistory         # Content migration history
└── schema media
    └── Media / MediaDbContext — 2 tables
        ├── MediaFolders                  # virtual folders with an optional parent
        ├── MediaFiles                    # CDN key, display name, type, size, folder
        └── __EFMigrationsHistory         # Media migration history
└── schema work
    └── Work / WorkDbContext — 2 tables
        ├── WorkTasks                     # a staff member's own tasks: title, state, result
        ├── WorkActivities                # automatic log of admin changes, repeats merged
        └── __EFMigrationsHistory         # Work migration history
```

## Open only the relevant part

| Question | Document | Related feature |
| --- | --- | --- |
| Category tree | [Categories](catalog-categories.md) | [Catalog categories](../../features/catalog/categories.md) |
| Products, translations, and prices | [Products and prices](catalog-products.md) | [Public products](../../features/catalog/public-products.md), [1C import](../../features/catalog/one-c-import.md) |
| List and stock data | [List projection](catalog-stock-projection.md) | [Product projection](../../features/catalog/stock-projection.md) |
| Product photos and schemes | [Products and prices](catalog-products.md) | [Product photos and schemes](../../features/catalog/product-media.md) |
| Counterparties and their prices | [Customers](sales-customers.md) | [Customers and counterparties](../../features/sales/customers.md) |
| Orders, drafts, and sync | [Orders](sales-orders.md) | [Manager orders](../../features/sales/admin-orders.md), [Admin workspace](../../features/sales/admin-order-workspace.md) |
| Users and sessions | [Access and sessions](identity-access.md) | [User session](../../features/identity/session.md) |
| Site articles, company contacts, requisites, documents, gallery, home page blocks, the page «Співпраця» and search texts of the site pages | [Content](content-articles.md) | [Site articles](../../features/content/articles.md), [Company contacts](../../features/content/company-contacts.md), [Company requisites](../../features/content/company-requisites.md), [Site documents](../../features/content/legal-documents.md), [Site gallery](../../features/content/gallery.md), [Home page FAQ and brands](../../features/content/home-blocks.md), [Page «Співпраця»](../../features/content/wholesale-page.md), [Search texts of the site pages](../../features/content/page-seo.md) |
| Library folders and CDN files | [Media](media-library.md) | [Media library](../../features/media/media-library.md) |
| Staff tasks and the activity log | [Work](work-tasks.md) | [Staff tasks and the activity log](../../features/work/staff-work.md) |

Each EF model table is described in exactly **one** document above. Physical FKs are shown in its ER diagram; ID references without an FK are explained separately. `__EFMigrationsHistory` is an infrastructure table and is not one of the EF model tables; its presence in a specific environment depends on applied migrations. After a schema change, update the relevant document and this tree only if the table list or placement changed.
