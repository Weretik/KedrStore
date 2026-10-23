# Backend design

## Verified baseline as of 2026-10-21

`AdminOrdersController` owns `/api/admin/orders`; `OrderReadService` already reads Orders with `Counterparties` and `OneCOrderSyncs`, but supports only exact `counterpartyId` and date sorting. `CreateOrderCommandHandler` accepts `Amount` from the client; `OrderProductReader` returns only ID and name. Catalog has `ProductPrices`; Sales Counterparty has `DefaultPriceTypeId` and category pricing rules. The new quote requires a separate price reader that uses the policy of the **selected** customer by `counterpartyId`, not the current identity user's policy.

## Decisions

- Sales owns the feature; Catalog provides product, category, current price, and availability reads through an Application/Infrastructure contract. Do not add a direct Sales Domain dependency on Catalog.
- Use a separate Admin controller/handlers/DTOs for `/api/admin/order-workspace`; existing operations remain compatible. New DTOs do not contain client-supplied `amount`.
- Quote and create use the same server pricing policy. Recalculate on POST Order. If the amount differs from the previous quote, the server returns a conflict with a new safe quote projection so the user can confirm the new amount before retrying. Define the expected quote fingerprint in versioned OpenAPI before coding.
- For reads, use `AsNoTracking`, SQL projection, `Count` after filters and before pagination, a sort whitelist, and no line loading for the list. Avoid `ToLower` on columns in predicates; check PostgreSQL `ILIKE`/indexes and an `EXPLAIN` plan on representative volume before choosing an index. Measure search and amount sorting separately.
- Creation uses the same atomic Order + sync + idempotency mechanism, but a new operation scope/key so retries from the two contracts cannot collide. The existing job does not change drafts.
- Workspace routes currently have `[AllowAnonymous]`; enabling authorization requires a separate role and rollout decision.
- After implementation, update the concise Sales feature map and contract index. An Orders migration is not expected; if unit price is needed in the response, derive it from saved line amount/quantity only after verifying precision.

The task plan must include tests for changed prices, missing prices, repeated keys, multiword search, and customer filtering. Full suite runs and starting the job are outside the documentation scope.
