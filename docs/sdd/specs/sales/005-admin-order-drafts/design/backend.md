# Data model and backend design

## Baseline

Before implementation, the Sales snapshot had `Orders`, `OrderLines`, and `OneCOrderSyncs`; there were no draft tables. This feature adds draft tables. Drafts are a new Sales model, not a status of an existing Order.

## Planned data

```text
SalesDbContext (current PostgreSQL schema)
├── OrderDrafts
│   ├── Id bigint PK (Domain: typed DraftId)
│   ├── CounterpartyId string FK -> Counterparties.Id
│   ├── Title varchar(200) NOT NULL
│   ├── Note text NULL
│   ├── CreatedAt, UpdatedAt timestamptz
│   └── Version concurrency token
├── OrderDraftLines
│   ├── Id bigint PK (Domain: typed DraftLineId)
│   ├── DraftId bigint FK -> OrderDrafts.Id ON DELETE CASCADE
│   ├── ProductId string
│   └── Quantity integer
└── OrderDraftIdempotencyRecords
    ├── Id bigint PK
    ├── DraftId bigint FK -> OrderDrafts.Id ON DELETE CASCADE
    └── IdempotencyKey, RequestHash, CreatedAtUtc
```

Identifiers follow the [strategy](../../../../standards/identifier-strategy.md); ProductId references Catalog without a database FK, as in OrderLines. Quantity matches the existing `OrderLine.Quantity` type `int` and must be positive. Enforce uniqueness on `(DraftId, ProductId)`; indexes `(CounterpartyId, UpdatedAt DESC, Id DESC)`, `(UpdatedAt DESC, Id DESC)`. Add an index for title/name search only after `EXPLAIN` with representative data; check the join plan for text search on Counterparty. Price and amount are not stored as authoritative draft data; the pricing service [004](../../004-admin-order-workspace/design/backend.md) calculates them.

## Migration and compatibility

A new additive EF migration creates three tables; existing Orders and the 1C job are unchanged. Rollout: migration before Admin deployment; rolling back UI/API does not delete tables or data, while database rollback requires a separate safe decision. Update SalesDbContext, snapshot, database map, and Sales feature after implementation. Concurrent editing uses a token in responses, `If-Match` or version in PUT/DELETE, and the current `version` in the `If-Match` header per planned OpenAPI. Deletion must use authorization and a transaction.
