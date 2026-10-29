# 008 — Customer prices and per-unit adjustments in the Admin order workspace

- **Module:** Sales
- **Consumer:** React Admin (kedr-web spec `admin/orders/002-customer-prices-and-adjustments`)
- **Status:** Implemented 2026-10-29 (branch `claude/project-thread-0i1fvf`)
- **Source:** user request in the project thread of 2026-10-29

## Goal and scope

On the checkout page a manager sees each product at the selected customer's price and may give a discount or markup per unit before placing the order or saving a draft. Catalog pages keep showing the retail price.

**Out of scope:** storefront and mobile client pricing (their category rules still match only the exact category), sending the discount to 1C as a separate field, limits on the discount size.

## Rules

- **R-001.** The customer price of a product uses the price type of the customer's category rule on the product's category; if there is none, the nearest rule on a parent category; otherwise the customer's default price type.
- **R-002.** `POST /api/admin/order-workspace/customer-prices` returns these prices for 1-100 products without checking stock. A product without a UAH price for that type, or an unknown product, returns `unitPrice: null`.
- **R-003.** Every quote, order and draft line may carry `priceAdjustment`: UAH per unit, at most ±1 000 000, two decimals. Negative is a discount, positive a markup, missing means 0.
- **R-004.** Final unit price = customer price + adjustment; line amount = final unit price × quantity. A discount that takes the final unit price below 1 UAH returns 400 `price_below_minimum` with the product name and the largest allowed discount (revision 2026-10-29; a product cheaper than 1 UAH cannot be discounted, a markup is always allowed); an invalid value returns 400 `invalid_price_adjustment`.
- **R-005.** The quote returns `baseUnitPrice`, `priceAdjustment` and the final `unitPrice`. The fingerprint covers the base price and the adjustment, so checkout rejects a stale quote with `quote_changed`.
- **R-006.** Orders and drafts store `PriceAdjustment` per line. The order detail returns it per line; the order list returns `adjustmentAmount` (sum over all units). Draft details return the stored adjustment with the current base price.
- **R-007.** 1C receives only the final line amounts, as before.

## Scenarios

| ID | Scenario |
| --- | --- |
| **SC-001** | A customer has a rule on "Locks"; a product in a grandchild category of "Locks" is quoted at the rule's price type, other products at the default type. |
| **SC-002** | Lines with −50 ₴ and +10.50 ₴ per unit produce line amounts and a total that include the adjustments. |
| **SC-003** | Changing only the adjustment changes the fingerprint; `0` and a missing value give the same fingerprint. |
| **SC-004** | A discount larger than the price, a third decimal, or a value above 1 000 000 returns 400 with the matching code. |
| **SC-005** | Customer prices are returned for an out-of-stock product; a product without a price and an unknown product return null. |
| **SC-006** | A stored order reports per-line adjustments and the list total adjustment. |

## Design

- `CustomerPriceResolver` (Infrastructure) loads the customer, its rules, the category parent map (only when the customer has rules) and the products' UAH prices, and walks up the parent chain (depth guard 32).
- `AdminWorkspacePricing` validates lines, applies stock checks and adjustments, and builds the fingerprint from `productId:quantity:basePrice:adjustment`.
- `OrderLine.PriceAdjustment` and `OrderDraftLine.PriceAdjustment`: `numeric(18,2)`, not null, default 0.
- Contract: [order workspace](../../../contracts/sales/admin-order-workspace.openapi.yaml), [drafts](../../../contracts/sales/admin-order-drafts.openapi.yaml). All new request fields are optional; new response fields are additive.

## Data and rollout

Migration `20261029130909_AddOrderLinePriceAdjustment` adds two columns with default 0; existing rows keep their amounts. Quote fingerprints issued before the deploy no longer match, so an open checkout page re-quotes once (`quote_changed`). Rollback: revert the code; the columns can stay.

## Verification evidence

2026-10-29:

- `WorkspacePricingTests` (EF in-memory): SC-001…SC-005, 9 passed.
- `WorkspaceOrderAdjustmentPostgresTests` and `OrderDraftPostgresTests` against local PostgreSQL 16: SC-006 and draft adjustment persistence passed; all `IntegrationTests.Sales` (92) passed on a fresh database.
- Unit tests 106 and architecture tests 10 passed; `Platform.Api` contract tests 20 passed; Redocly lint of the aggregate contract shows only the two existing warnings; documentation links valid.
