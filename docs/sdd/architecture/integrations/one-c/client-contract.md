# 1C client contract and normalization

`OneCSoapClientFactory` creates the generated SOAP/WCF client; `BasicAuthEndpointBehavior` adds Basic authentication. Do not log or document the endpoint, Authorization header, username, or password.

`Catalog.Infrastructure.Integrations.OneC.Client.OneCClient` implements the Application abstraction `IOneCClient`. Each method creates a SOAP client, calls one generated operation, and maps the result to a module DTO.

| Application method | Generated SOAP operation | Consumer |
| --- | --- | --- |
| `GetPriceTypesAsync()` | `GetPriceTypesAsync` | `SyncOneCPriceTypesJob` |
| `GetCategoriesAsync(rootId)` | `GetCategoriesAsync` | `SyncOneCCategoryJob` |
| `GetProductDetailsAsync(rootId)` | `GetProductDetailsAsync` | `SyncOneCProductDetailsJob` |
| `GetProductStocksAsync(rootId)` | `GetProductStocksAsync` | `SyncOneCStocksJob` |
| `GetProductPricesAsync(rootId)` | `GetProductPricesAsync` | `SyncOneCPricesJob` |

`rootId` preserves the source 1C string ID, including leading zeroes.

## Normalization at the Infrastructure boundary

| Value | Current handling | Empty/invalid value |
| --- | --- | --- |
| ID | Remove leading zeroes; parse as integer | `0`; optional parent ID becomes `null` |
| Text | Trim whitespace | Empty string |
| Boolean | `true`, `1`, `yes`, Ukrainian/Russian affirmative value | `false` |
| Price | Parse decimal | `0m` |
| Stock | Decimal using Ukrainian, Russian, then invariant culture; round up | `0m` |

The mapper preserves `ExportToSite=false`: the product is hidden from the public catalog but remains in local storage.

When adding a field, confirm its source semantics, map it into a module DTO (not a WCF type), define behavior for invalid values, update [import safety](data-safety.md), and add a mapping/integration test where supported.
