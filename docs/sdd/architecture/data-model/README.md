# KedrStore database map

**Checked:** 2026-10-21 against the three EF Core `DbContext` types, configurations, and snapshots under `src/`. This is the model structure in the repository, not confirmation that migrations have been applied in a particular environment.

All three contexts use tables without an explicitly configured PostgreSQL schema. Therefore the model does not define separate Catalog, Sales, and Identity schemas: their table names use the connection's default schema. With a standard `search_path`, that is `public`; the settings of any deployed database were not checked here.

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
    │   │   ├── Products                   # product and CategoryId
    │   │   ├── ProductPrices              # product prices
    │   │   ├── ProductTranslations        # localized names
    │   │   └── PriceType                  # price types
    │   ├── List projection
    │   │   └── ProductListProjections     # denormalized product reads
    │   └── Photo check
    │       └── ProductPhotoChecks         # photo-check result
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
    └── infrastructure table
        └── __EFMigrationsHistory         # applied EF migration history
```

## Open only the relevant part

| Question | Document | Related feature |
| --- | --- | --- |
| Category tree | [Categories](catalog-categories.md) | [Catalog categories](../../features/catalog/categories.md) |
| Products, translations, and prices | [Products and prices](catalog-products.md) | [Public products](../../features/catalog/public-products.md), [1C import](../../features/catalog/one-c-import.md) |
| List and stock data | [List projection](catalog-stock-projection.md) | [Product projection](../../features/catalog/stock-projection.md) |
| Photo status | [Photo check](catalog-photo-check.md) | [Photo check](../../features/catalog/photo-check.md) |
| Counterparties and their prices | [Customers](sales-customers.md) | [Customers and counterparties](../../features/sales/customers.md) |
| Orders, drafts, and sync | [Orders](sales-orders.md) | [Manager orders](../../features/sales/admin-orders.md), [Admin workspace](../../features/sales/admin-order-workspace.md) |
| Users and sessions | [Access and sessions](identity-access.md) | [User session](../../features/identity/session.md) |

Each EF model table is described in exactly **one** document above. Physical FKs are shown in its ER diagram; ID references without an FK are explained separately. `__EFMigrationsHistory` is an infrastructure table and is not one of the 25 EF model tables; its presence in a specific environment depends on applied migrations. After a schema change, update the relevant document and this tree only if the table list or placement changed.
