# Platform 004: data snapshots for the offline mobile app

**Status:** implemented 2026-11-05 ([feature card](../../../features/platform/mobile-offline-sync.md)). **Owner modules:** Catalog (catalog snapshot), Sales (customers snapshot). **Client:** kedr-web spec `mobile/manager/001-manager-app` (Kedr Manager).

## Goal

The owner chose (2026-11-05) to rebuild the mobile app for managers as an offline-first app: the manager picks a customer, browses products at that customer's prices and builds an order without a connection, then sends it when the network returns. The app keeps a local copy of the catalog and of the customers. Today it can only read them page by page and without the price rules, so each module gets one read endpoint that returns its whole snapshot.

**Out of scope:** debt and settlements (1C does not send them yet, so the fields stay `null`), delta sync (product prices have no change time, so the app replaces whole tables), writes from the app (orders and drafts keep using the existing `order-workspace` endpoints with `Idempotency-Key`), push notifications.

## Rules

- R-001 `GET /api/mobile/sync/catalog` (`CanManageOrders`, bearer) returns `{ version, generatedAt, priceTypes[], categories[], products[] }`. Categories carry id, parentId, name, sortOrder. Each product carries id (the 1C code), slug, Ukrainian name, categoryId, stock, inStock (the sales catalog rule), quantityInPack, photos[] (URLs from the CDN media links, main first), scheme URL, isSale, isNew and prices[] `{ priceTypeId, amount }` in UAH. The snapshot also carries `retailPriceTypeId`. Deleted products and categories are left out; the sales catalog has no other «hidden from sale» flag, so `exportToSite` does not filter (decision at delivery, 2026-11-05). Products have no description and the CDN has no thumbnails yet, so the app makes its own small copies.
- R-002 `GET /api/mobile/sync/customers` (`CanManageOrders`, bearer) returns `{ version, generatedAt, customers[] }`. Each customer carries counterpartyId, name, phone, email, defaultPriceTypeId, categoryPriceTypes[] `{ categoryId, priceTypeId }` (the same rules `CustomerPriceResolver` uses: the nearest rule up the category tree wins, otherwise the default type), `debt: null` and `balance: null`.
- R-003 Both endpoints send a strong `ETag` computed from the snapshot content and answer `304 Not Modified` with no body when `If-None-Match` matches. Responses are gzip-compressed when the client accepts it.
- R-004 Amounts use the same units and rounding as `order-workspace`; the server quote stays the authority when an order is created.
- R-005 The app's 7-day offline window must not outlive the refresh token: `IdentitySessionSecurityOptions.RefreshAbsoluteLifetimeDays` (default 30) stays at 7 or more. A refresh that fails after the window signs the manager out in the app.
- R-006 New fields may be added to the responses later; the app ignores fields it does not know.

## Scenarios

- SC-001 A manager signs in on a new phone; the app downloads both snapshots once and can then show products and prices offline.
- SC-002 The app asks again with the last `ETag` and nothing changed; the server answers `304` and sends no body.
- SC-003 1C changes a price; the next catalog request returns a new `ETag` and the new amount.
- SC-004 A product is deleted or hidden from sale; it is missing from the next catalog snapshot.
- SC-005 A customer gets a price rule for the «Ручки» category; the next customers snapshot carries it in `categoryPriceTypes`.
- SC-006 A signed-in user without the Manager or Admin role gets `403` from both endpoints; without a token, `401`.

## Design

- Each module adds a query in its Application layer that reads through its own read context (`IReadCatalogDbContext`, `IReadSalesDbContext`); no module reads another module's tables. The catalog snapshot lives in `Catalog.*`, the customers snapshot in `Sales.*`, each with its own controller under `/api/mobile/sync/`.
- The `ETag` is a hash of the serialized snapshot; `version` is the same value for logging in the app.
- Response compression is enabled for these two routes if it is not on for the API already.

## Data and rollout

No schema change. Deploy the backend before the app stage that needs it (kedr-web stage 2); the old app does not call these endpoints. Rollback: remove the two actions.

## Tasks

- TS-001 Catalog snapshot query, reader, controller, ETag/304. Covers R-001, R-003, R-004; SC-001–SC-004, SC-006.
- TS-002 Customers snapshot query, reader, controller, ETag/304. Covers R-002, R-003; SC-001, SC-002, SC-005, SC-006.
- TS-003 Compression for the two routes, OpenAPI contract and `ApiContractTests`, feature card. Covers R-003, R-006.
- TS-004 Check `RefreshAbsoluteLifetimeDays` in every environment's configuration. Covers R-005.

## Delivery notes

- TS-004: `RefreshAbsoluteLifetimeDays` is 30 and `RefreshIdleTimeoutDays` 14 in `appsettings.json`; no environment overrides them, and start-up validation keeps idle ≤ absolute. Both stay above the 7-day window.
- Compression is on only for `/api/mobile/sync/*`, so cookie pages are not exposed to compression side channels.

## Tests

Integration tests per endpoint: snapshot content (deleted and hidden products left out, price rules present), `304` with a matching `ETag`, a new `ETag` after a price change, `401` and `403` by role. Unit tests for the ETag hash. Contract test for both operations.
