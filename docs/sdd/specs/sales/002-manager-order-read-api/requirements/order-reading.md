# Manager order read API — order reading

## Behavior

- The system returns persisted manager orders according to the agreed list scope.
- The system returns the complete persisted order representation when an existing order is requested, including its original lines and the current 1C synchronization state.
- The system can limit the order list to the exact counterparty identifier supplied by the caller.
- Read requests do not invoke 1C and do not change the order, counterparty, synchronization record, or audit data.

## Business rules

### R-001 — Only persisted manager orders are returned

The list and detail operations read the existing Sales `Order` records. Public Catalog quick orders are outside this feature and must not be returned.

### R-002 — Order detail preserves the persisted order composition

For an existing manager order, detail contains its identifier and number, creation time, current counterparty summary, optional comment, persisted lines, computed total amount, and safe current `OneCOrderSync` summary. Internal delivery diagnostics and audit data are excluded.

### R-003 — Counterparty-scoped reading uses the Sales counterparty identifier

When `counterpartyId` is supplied to the common order-list operation, the system returns only orders whose `Order.CounterpartyId` exactly matches it. Unknown identifiers produce an empty page. Historical orders remain readable for a soft-deleted counterparty.

### R-004 — Reads have no side effects

Order read operations neither call 1C nor alter local persistence, including `OneCOrderSync` attempt, retry, notification, and audit fields.

### R-005 — Read access is restricted

Every operation requires `PolicyNames.CanManageOrders` and allows Manager and Admin. Authorized callers can read all manager orders; no row-level assignment restriction applies.

### R-006 — Collection and detail have separate payload sizes

The collection is paged and contains compact summaries without line arrays or comments. Detail contains the approved complete order projection. Collection paging defaults to page 1 and page size 20, accepts at most 100 rows, and orders newest first by creation time then order ID.

## Acceptance scenarios

### SC-001 — Browse manager orders

**Covers:** R-001, R-005, R-006

Given persisted manager orders exist
And the caller has the agreed read permission
When the caller requests the manager-order list
Then the result contains only persisted manager orders within the agreed list scope
And it does not contain Catalog quick orders
And every row contains the compact summary without comment or order-line array
And rows are paged and ordered newest first according to R-006.

### SC-002 — Open an existing manager order

**Covers:** R-002, R-004, R-005

Given a persisted manager order has lines and a current 1C synchronization record
And the caller has the agreed read permission
When the caller requests that order
Then the result contains the agreed order information, its persisted lines, and current delivery state
And every line's `amount` represents the complete line quantity
And the result contains the sum of line amounts but no manufactured unit-price field
And no 1C call or local state change occurs.

### SC-003 — Browse orders for one counterparty

**Covers:** R-003, R-005

Given orders exist for more than one counterparty
And the caller has the agreed read permission
When the caller requests orders for one counterparty identifier
Then every returned order belongs to that counterparty
And no order for another counterparty is returned.

### SC-004 — Request an unknown order

**Covers:** R-002, R-005

Given no manager order exists for the requested identifier
When an authorized caller requests the order detail
Then the system returns the agreed not-found result
And no 1C call or local state change occurs.

### SC-005 — Reject an unauthorized read

**Covers:** R-005

Given a caller does not satisfy the agreed Sales order-read access policy
When the caller requests an order list or detail
Then the system rejects access using the agreed authentication or authorization result
And it exposes no order data.

## Deferred scenarios

- Search, date-range filtering, status filtering, sorting, export, aggregates, and cursor navigation are deferred until their product requirements are agreed.
