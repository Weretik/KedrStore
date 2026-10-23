# Host.Jobs CLI

`Host.Jobs` loads `appsettings.json` and environment variables. In Development it uses its **own** User Secrets, separate from `Host.Api`. From the repository root:

```powershell
$env:DOTNET_ENVIRONMENT = 'Development'
dotnet run --project src/Bootstrapper/Host.Jobs/Host.Jobs/Host.Jobs.csproj -- --job=<name>
```

| Job | Argument | Purpose |
| --- | --- | --- |
| `migrate` | — | Runs EF migrations for registered contexts in order, then seeders; changes schema and data |
| `full` | — | Full Catalog refresh for configured Doors, Hardware, and Cosmos root IDs |
| `pricetypes` | — | Catalog price types |
| `category`, `productdetails`, `stocks`, `prices` | One or more `--rootId=<id>` | Targeted Catalog root refresh |
| `rebuild-projections` | — | Rebuild Catalog product-list read model |
| `check-product-photos` | — | Checks photo URLs for every active product and saves the latest status |
| `counterparties` | — | Sales counterparties |
| `counterparty-category-price-types` | — | Sales customer pricing rules |
| `sales-customers-full` | — | Counterparties first, then pricing rules |
| `sync-one-c-orders` | — | One ready-to-run batch of saved Sales manager orders |

Example:

```powershell
dotnet run --project src/Bootstrapper/Host.Jobs/Host.Jobs/Host.Jobs.csproj -- --job=stocks --rootId=<one-c-root-id>
```

Repeat `--rootId` for multiple roots. Exit code `0` and `[SUCCESS] Job finished OK` mean no exception occurred; separately check the number of imported records. Exit code `1` means a job, argument, or execution error; code `2` means a required `--rootId` is missing. For `migrate`, verify the result using the [migration guide](../data/migrations-and-seeding.md).

See the [Sales/1C runbook](sales-one-c-runbook.md#manager-order-delivery) for order delivery prerequisites, result interpretation, stopping, and dead-letter handling.

The [Catalog photo-check runbook](catalog-photo-check-runbook.md) describes CDN classification, results, configuration, and Cloud Run execution.

## Manual 1C write smoke test

This command performs a real `CreateSiteRequest` write. Use only a test 1C endpoint and test counterparty/product data. Provide `OneCSoap:Endpoint`, `Username`, and `Password` through `Host.Jobs` User Secrets or environment variables; do not put them in `appsettings.json` or command arguments.

```powershell
$env:DOTNET_ENVIRONMENT = 'Development'
dotnet run --project src/Bootstrapper/Host.Jobs/Host.Jobs/Host.Jobs.csproj -- --job=one-c-smoke-write --counterpartyId=000000298 --productId=8190 --quantity=10 --amount=1000 --orderNumber=TEST-SO-20260818-001 --comment='TEST: manual SOAP verification.'
```

Numeric product IDs are left-padded with zeroes to nine characters: `8190` is sent as `000008190`. A confirmed 1C document ID yields exit code `0`; a business rejection yields code `3`.

The feature specification's [1C write smoke test](../../specs/sales/001-admin-order-one-c-sync/tasks/test/06.3-one-c-write-smoke-test.md) keeps the recorded live-test evidence, response interpretation, and code map.
