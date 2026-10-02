# Missing product photos — data model

## Context

The stored `Product.Photo` URL is deterministic but does not prove that the CDN
serves an image. The feature needs one latest-result row per active product,
with enough data to reject a stale result after the URL changes and to diagnose
why the image was considered unavailable.

## Model

```text
Product
├── Id: ProductId (existing PK)
├── Photo: string (existing current URL)
├── IsDeleted: bool (existing soft-delete marker)
└── ProductPhotoCheck: optional dependent
    ├── ProductId: ProductId (PK and required FK -> Product.Id)
    ├── PhotoUrl: string, max 1000
    ├── Status: ProductPhotoStatus
    │   ├── Unknown
    │   ├── Available
    │   ├── Missing
    │   ├── InvalidContentType
    │   └── CheckFailed
    ├── CheckedAtUtc: DateTimeOffset?
    ├── HttpStatusCode: int?
    ├── DiagnosticCode: string?, max 100
    └── DiagnosticMessage: string?, max 500
```

`ProductId` is reused as a typed shared primary key because the dependent has no
identity or lifecycle outside its product. No generated scalar or GUID is added.

## Invariants and integrity

- There is at most one photo-check row per product.
- `PhotoUrl` is required, trimmed, absolute HTTPS, at most 1000 characters, and
  represents the URL to which the stored result applies.
- `Unknown` has `CheckedAtUtc = null`, `HttpStatusCode = null`, and no diagnostic.
- Every terminal status has a non-null `CheckedAtUtc`.
- `Available` has no diagnostic message; its HTTP status is in `200..299`.
- `Missing` records `404` or `410`.
- `InvalidContentType` records a `2xx` status and a bounded diagnostic code.
- `CheckFailed` may have no HTTP status for transport failures and records a
  bounded diagnostic code; raw response bodies are never stored.
- Applying a result requires its URL to equal `PhotoUrl`; otherwise the write is
  discarded as stale.
- Physical Product deletion cascades to `ProductPhotoChecks`.
- Soft deletion does not remove the dependent row, but all feature reads and
  checks require an active Product.
- Index `(Status, ProductId)` supports deterministic unavailable-result paging.
- The initial migration backfills only active products as `Unknown` using their
  current `Photo` URL.
