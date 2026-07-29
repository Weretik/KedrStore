# OneC client contract and normalization

`OneCSoapClientFactory` creates the generated SOAP/WCF client; `BasicAuthEndpointBehavior` attaches Basic authentication. Neither endpoint, Authorization header, username nor password may be logged or documented.

`Catalog.Infrastructure.Integrations.OneC.Client.OneCClient` implements the Application abstraction `IOneCClient`. Every method creates a SOAP client, calls one generated operation and maps return values to module DTOs.

| Application method | Generated SOAP operation | Consumer |
| --- | --- | --- |
| `GetPriceTypesAsync()` | `GetPriceTypesAsync` | `SyncOneCPriceTypesJob` |
| `GetCategoriesAsync(rootId)` | `GetCategoriesAsync` | `SyncOneCCategoryJob` |
| `GetProductDetailsAsync(rootId)` | `GetProductDetailsAsync` | `SyncOneCProductDetailsJob` |
| `GetProductStocksAsync(rootId)` | `GetProductStocksAsync` | `SyncOneCStocksJob` |
| `GetProductPricesAsync(rootId)` | `GetProductPricesAsync` | `SyncOneCPricesJob` |

`rootId` remains the original 1C string identifier, including leading zeroes.

## Normalization at the Infrastructure boundary

| Value | Current handling | Empty/invalid |
| --- | --- | --- |
| ID | trim leading zeroes; parse integer | `0`, optional parent ID is `null` |
| text | trim | empty string |
| boolean | `true`, `1`, `yes`, Ukrainian/Russian affirmative value | `false` |
| price | decimal parsing | `0m` |
| stock | Ukrainian, Russian, then invariant decimal parsing; ceiling | `0m` |

The mapper preserves `ExportToSite=false`: it means hidden from public catalogue, not excluded from local storage.

When adding a field: confirm source semantics, map to a module DTO (not a WCF type), define invalid-value behaviour, update the import-safety docs, and add a mapping/integration test where supported.
