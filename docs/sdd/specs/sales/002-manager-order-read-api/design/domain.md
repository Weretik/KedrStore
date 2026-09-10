# Manager order read API — design: Domain

## Responsibility

This is a query-only feature. `Order` remains the owner of order number, counterparty reference, comment, and lines. `OneCOrderSync` remains the owner of delivery state and accepted 1C document number. `Counterparty` remains the owner of customer identity and profile data. No aggregate method, value object, invariant, or domain event is required for the agreed read behavior.

## Intermodule interaction

The query reads only Sales-owned data. It does not call Catalog, Identity, or 1C. Authorization consumes the existing Identity-owned policy abstraction after the concrete access decision is agreed.

## Risks

- Historical order detail intentionally shows the counterparty's currently stored name and phone because the order model contains no customer snapshot. Consumers must not interpret those values as the profile at order-creation time.
- Delivery state can change after a list or detail response is produced; the API returns the state observed during that query and does not promise a live 1C status.
