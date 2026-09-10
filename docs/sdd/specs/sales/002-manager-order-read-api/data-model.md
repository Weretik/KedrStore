# Manager order read API — data model

## Context

The feature reads the already-persisted Sales order graph. It introduces no new stored data and must keep the original ownership established by feature 001.

## Model

```text
Counterparty (existing; Id: string)
└── 0..* Order
    ├── Id (OrderId: long)
    ├── OrderNumber
    ├── CounterpartyId
    ├── Comment
    ├── CreatedAtUtc / audit timestamps
    ├── 1..* OrderLine
    │   ├── ProductId
    │   ├── ProductName
    │   ├── Quantity
    │   └── Amount
    └── 1..1 OneCOrderSync
        ├── Status
        ├── AttemptCount
        ├── OneCDocumentNumber
        └── AcceptedAtUtc and bounded diagnostics
```

## Invariants and integrity

- `Order.CounterpartyId` is the Sales counterparty identifier used for counterparty-scoped reads.
- Each order retains one or more persisted `OrderLine` records and exactly one `OneCOrderSync` record, as defined by feature 001.
- The read API must treat `OneCOrderSync` as the source of current delivery status; it must not reconstruct status from job history.
- Internal sync diagnostics, request hashes, SOAP response body, dead-letter notification data, and retry-audit records are excluded from public responses unless explicitly agreed.
- No migration or new persistence structure is planned. The existing index on `Orders.CounterpartyId` supports exact customer filtering. Implementation verification must inspect the newest-first paged query and add an index only if representative PostgreSQL evidence shows it is required.
