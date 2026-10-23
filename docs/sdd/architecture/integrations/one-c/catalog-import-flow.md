# Catalog import flow

## Full synchronization

`SyncOneCFullJob` defines the canonical update order. It reads configured root IDs for Doors, Hardware, and Cosmos, then runs:

```text
price types
  → categories (doors, hardware, cosmos)
  → product details (doors, hardware, cosmos)
  → stock (doors, hardware, cosmos)
  → prices (doors, hardware, cosmos)
  → one ProductListProjections rebuild
```

| Job | Reads | Writes | On empty response |
| --- | --- | --- | --- |
| Price types | 1C price types | `PriceTypes` | No changes |
| Categories | Categories for one root | `Categories` | Preserves existing records; upserts root/manual groups |
| Product details | Products for one root + RU CSV | `Products`, `ProductTranslations` | No changes; if a non-empty response maps to zero rows, throws before deletion |
| Stock | Stock for one root | `Products.Stock` | No changes |
| Prices | Prices for one root | `ProductPrices` | No changes |
| Projection rebuild | Local catalog data | `ProductListProjections` | Replaces the read model |

Category/product reconciliation may delete records missing from a non-empty mapped source response for that root. Run per-root jobs only when 1C returns that root in full.

## Visibility rule

`ExportToSite` flows through `1C → Products → ProductListProjections`.

- `false` does not stop import, stock updates, or price persistence.
- The public product list filters for `ExportToSite == true`.
- Admin lists do not filter on this flag and return `exportToSite`.

`full` disables intermediate rebuilds and rebuilds the projection once at the end. The targeted `stocks` job rebuilds the projection after stock updates by default, keeping list `InStock` current. Sources: `SyncOneCFullJob.cs`, `SyncOneCStocksJob.cs`, and `Host.Jobs/Program.cs`, checked 2026-10-15.
