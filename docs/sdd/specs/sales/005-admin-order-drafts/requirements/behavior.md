# Rules and acceptance scenarios

## Rules

- **R-001.** A saved draft has an internal `draftId`, required title and `counterpartyId`, optional note, and at least one line. It has no order number, `Order`, or `OneCOrderSync`.
- **R-002.** Lines have unique `productId` values and positive quantities. The server validates the active customer and current products on create and update. The client does not provide prices; viewing/editing shows the current server quote. If the customer, product, stock, or price becomes unavailable after saving, GET detail still returns saved IDs/quantities, title, version, and `pricingStatus=Unavailable`; prices and amount are `null`, and `pricingErrorCode` contains a safe reason. This lets the user fix or delete the draft without substituting zero for a missing price. PUT with new lines requires an available price.
- **R-003.** A draft can be viewed, edited (title/note/customer/lines), and deleted after confirmation in the UI. The API does not handle modal confirmation. Updates use an optimistic concurrency token; concurrent edits return 409 and require reloading.
- **R-004.** The list uses server pagination (1/20, max 100), search by part of the draft title/customer name or exact numeric ID, exact `counterpartyId`, and AND-combined multiword tokens. Sort by `updatedAt` (descending by default), `title`, and `customerName` in both directions, with stable `draftId` tie-breaker.
- **R-005.** “Create order from draft” only reads the draft and passes its lines to a temporary Admin workspace; checkout is handled separately by [004](../../004-admin-order-workspace/README.md). A successful order **does not modify or delete** the source draft, so it can be reused.
- **R-006.** New operations require `CanManageOrders`; deletion physically removes the draft and its lines in one transaction. Errors do not expose internal data.

## Acceptance scenarios

### SC-001 — Save a draft

**Covers:** R-001, R-002

- Given an active customer and a valid set of products exist
- When Admin submits a title, note, and lines to save
- Then Sales creates a draft for that customer with its own ID
- And creates no order number, Order, or OneCOrderSync.

### SC-002 — Update a draft

**Covers:** R-002, R-003

- Given a draft exists and its version is current
- When Admin changes the customer, title, note, or lines
- Then the new data is saved and the current server-calculated amount is returned
- And a stale version returns 409 without losing another user's saved changes.

### SC-003 — Find drafts

**Covers:** R-004

- Given drafts for multiple customers exist
- When Admin searches with multiple words, filters by customer ID, sorts, and pages
- Then each page and `totalRecords` reflect conditions before pagination
- And ordering is stable when primary sort values are equal.

### SC-004 — Delete a draft

**Covers:** R-003, R-006

- Given a draft exists and the caller has required access
- When Admin confirms deletion and calls DELETE
- Then the draft and its lines are no longer available
- And Order and sync records are unchanged.

### SC-005 — Reuse a draft

**Covers:** R-005

- Given a saved draft has been used as the basis of a new completed order
- When Admin requests its details again
- Then the same lines, title, and customer are shown
- And the draft remains available for another order.

### SC-006 — Open a draft after a price or product becomes unavailable

**Covers:** R-002, R-003

- Given a draft was saved and its product, stock, customer, or price later becomes unavailable
- When Admin requests its details
- Then 200 is returned with saved IDs/quantities, title, and version
- And `pricingStatus=Unavailable`, a safe reason code, and `null` for unavailable prices and amount are shown
- And the draft can be edited or deleted using the returned version.

### SC-007 — Concurrent creation with one key

**Covers:** R-001, R-006

- Given two concurrent POST requests have the same `Idempotency-Key`
- When the requests have the same body
- Then one draft is created and the second request returns it as a replay
- And if bodies differ, the second request gets 409 and no second record is created.
