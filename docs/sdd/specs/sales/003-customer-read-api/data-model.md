# Customer read API — data model

## Context

The feature reads the existing local Sales customer projection produced by existing synchronization. It introduces no stored data and does not alter the relationship to Identity or 1C.

## Model

```text
Counterparty (existing Sales aggregate; Id: string)
├── IdentityUserId (Guid; Identity-owned referenced user)
├── Name
├── Email
├── Phone (nullable)
├── DefaultPriceTypeId
├── IsDeleted / audit timestamps
└── 0..* CounterpartyCategoryPriceType
    ├── CounterpartyId
    ├── CategoryId
    └── PriceTypeId
```

## Invariants and integrity

- `Counterparty.Id` is the existing stable Sales/1C counterparty identifier and is the detail lookup key.
- `Counterparty` is soft-deletable; normal reads exclude `IsDeleted` records.
- `CounterpartyCategoryPriceType` is keyed by `CounterpartyId` and `CategoryId`; it represents a category-specific rule that may override the counterparty’s default price type.
- `IdentityUserId` remains an Identity reference. It is not a customer-facing identifier and must not be exposed unless the agreed API contract requires it.
- No migration or new persistence structure is planned. Implementation verification must inspect active-customer paging ordered by `Name` and `Id`; add an index only if representative PostgreSQL evidence shows it is required.
