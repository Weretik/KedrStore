# Manager order read API — overview and boundaries

## Goal

As a Sales manager, I want to view manager orders and inspect an individual order so that I can follow customer purchases and their 1C delivery state.

## In scope

- Retrieve a list of persisted manager orders.
- Retrieve one persisted manager order with its existing order lines and 1C synchronization state.
- Retrieve persisted manager orders for one existing counterparty.
- Read the existing `Order`, `OrderLine`, `OneCOrderSync`, and `Counterparty` data without modifying it.

## Out of scope

- Creating, editing, cancelling, deleting, or retrying orders.
- Changing the existing manager-order creation, 1C synchronization, or dead-letter workflows.
- Changing counterparty synchronization or customer data.
- New domain entities, identifiers, migrations, or database schema changes unless an agreed performance requirement proves they are necessary.

## Actors and external systems

- Sales manager: consumes the administration read API.
- Sales administrator: may consume the same API if the agreed authorization policy permits it.
- Sales database: authoritative persisted source for orders, lines, counterparties, and delivery state.
- 1C: is not called by these read operations.

## Agreed decisions

- Manager and Admin access is enforced through `PolicyNames.CanManageOrders`; no row-level assignment filter applies.
- `GET /api/admin/orders` provides one paged collection with an optional exact `counterpartyId` filter; there is no duplicate nested customer-orders endpoint.
- The list returns compact order summaries. `GET /api/admin/orders/{orderId}` returns the complete approved order detail.
- Paging defaults to page `1` and page size `20`, with maximum page size `100`; ordering is newest first by `createdAtUtc`, then `orderId`.
- Historical orders remain visible when their counterparty is soft-deleted. The current stored counterparty name and phone may be shown in the authorized order detail.
- Search and additional filters are outside this feature.
