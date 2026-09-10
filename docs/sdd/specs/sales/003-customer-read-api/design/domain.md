# Customer read API — design: Domain

## Responsibility

This is a query-only feature. `Counterparty` remains the owner of customer identity, profile data, deletion state, and default price type. `CounterpartyCategoryPriceType` remains the owner of category-specific price type values. No aggregate method, value object, invariant, or domain event is required to read the existing data.

## Intermodule interaction

The query reads Sales-owned data only. It does not call 1C. It must not load Identity user data merely because `IdentityUserId` is stored on the counterparty; any identity enrichment requires a separate agreed boundary.

## Risks

- Email and phone are personal data exposed by the currently anonymous read API only within the agreed DTO boundary; list omits email and detail includes it according to the agreed contract.
- The local counterparty is a synchronized projection and may lag 1C; the API returns the locally persisted state and does not promise a live 1C lookup.
