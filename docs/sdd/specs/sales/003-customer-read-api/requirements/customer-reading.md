# Customer read API — customer reading

## Behavior

- The system returns current, non-deleted Sales counterparties within the agreed list scope.
- The system returns the agreed current profile for an existing active counterparty.
- Read requests do not invoke 1C, modify counterparty data, or alter category price-type rules.

## Business rules

### R-001 — Customer identity uses the Sales counterparty identifier

Customer list and detail operations identify a customer by existing `Counterparty.Id`, which is the Sales/1C counterparty identifier. This feature does not create or replace identifiers.

### R-002 — Normal reads exclude soft-deleted counterparties

Customer list and detail exclude `Counterparty.IsDeleted = true`; a soft-deleted customer is unavailable through detail and produces the same not-found result as an unknown identifier.

### R-003 — Detail exposes only agreed customer data

The list exposes counterparty ID, name, and phone. Detail exposes counterparty ID, name, phone, email, default price type, and category price-type rules ordered by category ID. `IdentityUserId`, deletion/audit state, and order identifiers/details are excluded.

### R-004 — Reads have no side effects

Customer read operations neither call 1C nor create, update, restore, soft-delete, or otherwise alter `Counterparty` or `CounterpartyCategoryPriceType` records.

### R-005 — Customer reads allow anonymous access

Both operations allow anonymous callers. No authentication, role, policy, or row-level assignment restriction applies during the current rollout stage.

### R-006 — Collection and detail have separate payload sizes

The paged collection contains only counterparty ID, name, and phone. Detail contains the approved profile and category price rules but no orders or order IDs. Collection paging defaults to page 1 and page size 20, accepts at most 100 rows, and orders by name then counterparty ID.

## Acceptance scenarios

### SC-001 — Browse active customers

**Covers:** R-001, R-002, R-005, R-006

Given active and soft-deleted Sales counterparties exist
And the caller may be anonymous
When the caller requests the customer list
Then the result contains active counterparties in the agreed list scope
And it does not contain soft-deleted counterparties
And each row contains only counterparty ID, name, and phone
And rows are paged and ordered according to R-006.

### SC-002 — Open an active customer

**Covers:** R-001, R-003, R-004, R-005

Given an active Sales counterparty exists
And the caller may be anonymous
When the caller requests that customer by counterparty identifier
Then the result contains the agreed current customer information
And category price rules are ordered by category ID
And the result contains no order IDs or order details
And no 1C call or local state change occurs.

### SC-003 — Request an unknown or deleted customer

**Covers:** R-002, R-005

Given no active counterparty exists for a requested identifier
When any caller requests customer detail
Then the system returns the agreed not-found result
And no customer data is exposed.

### SC-004 — Allow an anonymous read

**Covers:** R-005

Given a caller has no authentication token
When the caller requests a customer list or detail
Then the system returns the same approved response available to an authenticated caller
And the response remains within the agreed customer-data boundary.

## Deferred scenarios

- Customer create/update/delete/restore operations, customer history, search, price calculation, embedded customer-order aggregates, and exports are deferred until separately agreed.
