# KedrStore backend feature map

**Static code check completed:** 2026-10-16 for earlier features; the Admin order workspace was checked on 2026-10-21. This map shows HTTP and job scenarios that actually exist, grouped by module. Each description explains the flow, components, data, and relationships. [SDD](../specs/README.md) holds decisions and delivery history; [architecture](../architecture/README.md) holds common boundaries. When adding a feature or changing its structure, update its description and this index in the same change.

See the feature card's “Code structure” section for actual file paths and roles, “Structure diagram” for the flow between parts, and “Tests” for the related test files and a focused run command.

## Catalog

- [Catalog categories](catalog/categories.md) — public list and details by slug, plus separate admin reads.
- [Public products](catalog/public-products.md) — anonymous lists by language/category and details by slug.
- [Admin products and photos](catalog/admin-products.md) — paged lists with server filters, full list, unavailable photos, and product details by slug.
- [Product photo check](catalog/photo-check.md) — job that checks URLs and saves status.
- [Quick order](catalog/quick-order.md) — public `POST /api/orders` with Excel generation and Telegram notification.
- [1C catalog import](catalog/one-c-import.md) — background import of categories, product details, prices, price types, and stock.
- [Product and stock projection](catalog/stock-projection.md) — separate read model for product lists and its updates.

## Sales

- [Manager orders and 1C](sales/admin-orders.md) — order creation, reading, synchronization status, and retry.
- [Admin order workspace](sales/admin-order-workspace.md) — separate protected operations for search, server-side quoting, checkout, and saved drafts in React Admin.
- [Customers and counterparties](sales/customers.md) — Admin counterparty list and details, local synchronization with 1C.
- [Manager catalog](sales/manager-catalog.md) — authorized sales product list.

## Identity

- [User session](identity/session.md) — login, refresh, logout, and me.

## Platform

- [Migrations and seed](platform/database-migration.md) — separate job that changes the schema and initial data.

Planned capabilities from SDDs and technical classes without separate scenarios are not shown here as completed features.
