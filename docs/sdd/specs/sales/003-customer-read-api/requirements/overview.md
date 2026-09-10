# Customer read API — overview and boundaries

## Goal

As a customer-read API consumer, I want to browse customers and open one customer’s details so that I can identify the counterparty used for sales work.

## In scope

- Retrieve a list of current Sales customers.
- Retrieve one current Sales customer by its `Counterparty.Id`.
- Read existing counterparty profile data and its existing category price-type rules in customer detail.
- Exclude soft-deleted counterparties from normal read results unless an agreed administrative history requirement states otherwise.

## Out of scope

- Creating, editing, deleting, restoring, or synchronizing customers.
- Changing Identity users, roles, sessions, or customer authentication.
- Calculating catalog prices or modifying counterparty category price-type rules.
- Returning order history; manager-order reading is specified separately in feature 002.
- New domain entities, identifiers, migrations, or database schema changes unless an agreed performance requirement proves they are necessary.

## Actors and external systems

- Administration frontend: consumes the customer read API without authentication during the current rollout stage.
- Anonymous API caller: can read the approved customer projections.
- Sales database: authoritative local projection for counterparties and their category price-type rules.
- 1C: is not called by these read operations.
- Identity: owns the referenced identity user; this feature must not disclose identity data unless explicitly agreed.

## Agreed decisions

- Both customer-read routes allow anonymous access during the current rollout stage; no row-level assignment filter applies.
- Routes are `GET /api/admin/customers` and `GET /api/admin/customers/{counterpartyId}`, using the existing Sales/1C string identifier.
- The compact list contains `counterpartyId`, `name`, and `phone`. Detail adds email, default price type, and ordered category price-type rules.
- `IdentityUserId`, soft-delete/audit fields, order IDs, counts, and details are never returned by this feature.
- Paging defaults to page `1` and page size `20`, with maximum page size `100`; ordering is by name, then counterparty ID.
- Soft-deleted counterparties are excluded from list and detail; detail returns not found.
- A customer page loads detail and `GET /api/admin/orders?counterpartyId=...` independently, and may issue both requests in parallel.
- Search and additional filters are outside this feature.
