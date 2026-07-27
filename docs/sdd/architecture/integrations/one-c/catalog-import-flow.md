# Catalog import flow

## Full synchronization

`SyncOneCFullJob` is the canonical refresh order. It reads configured door and hardware root IDs and performs:

```text
price types
  → categories (doors, hardware)
  → product details (doors, hardware)
  → stocks (doors, hardware)
  → prices (doors, hardware)
  → one ProductListProjections rebuild
```

| Job | Reads | Writes | Empty-response behaviour |
| --- | --- | --- | --- |
| price types | 1C price types | `PriceTypes` | no change |
| categories | one root categories | `Categories` | keeps current rows; upserts root/manual groups |
| product details | one root products + RU CSV | `Products`, `ProductTranslations` | no change; non-empty response mapping to zero throws before deletion |
| stocks | one root stock | `Products.Stock` | no change |
| prices | one root prices | `ProductPrices` | no change |
| rebuild projection | local catalog data | `ProductListProjections` | replaces read model |

Category/product reconciliation can remove rows absent from a non-empty mapped source response for that root. Run root-specific jobs only when 1C returns a complete root.

## Visibility rule

`ExportToSite` travels `1C → Products → ProductListProjections`.

- `false` does not stop import, stock updates or price storage.
- public product list filters `ExportToSite == true`.
- admin lists do not filter it and return `exportToSite`.

`full` suppresses intermediate rebuilds and rebuilds the projection once at the end. A targeted `stocks` job rebuilds the projection after updating stock, so list `InStock` is current.
