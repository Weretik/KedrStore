# Customer read API — design: Infrastructure

## Persistence and integrations

Infrastructure will provide no-tracking Sales database projections for active counterparties, applying the established soft-delete predicate. The paged list projects only ID, name, and phone and orders by name then ID. Detail additionally projects email, default price type, and category price-type rules ordered by category ID. It must not load order data or invoke the existing 1C synchronization services or Identity persistence.

## Migration and rollout

No migration is planned because the feature reads existing `Counterparties` and `CounterpartyCategoryPriceTypes` tables. Before implementation, validate the agreed list search, ordering, and pagination against representative data; add an index only with an agreed performance requirement and measured need. The additive API must be deployed with its aggregate OpenAPI reference and focused contract tests.
