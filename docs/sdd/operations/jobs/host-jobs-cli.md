# Host.Jobs CLI

`Host.Jobs` loads `appsettings.json` and environment variables. In Development it loads **its own** User Secrets (separate from `Host.Api`).

```powershell
Set-Location C:\Users\Віталій\RiderProjects\KedrStore\src\Bootstrapper\Host.Jobs\Host.Jobs
$env:DOTNET_ENVIRONMENT = 'Development'
dotnet run --project .\Host.Jobs.csproj -- --job=<name>
```

| Job | Argument | Purpose |
| --- | --- | --- |
| `full` | — | catalog full refresh for both configured roots |
| `pricetypes` | — | catalog price types |
| `category`, `productdetails`, `stocks`, `prices` | one or more `--rootId=<id>` | targeted catalog root refresh |
| `rebuild-projections` | — | rebuild catalog list read model |
| `counterparties` | — | Sales counterparties |
| `counterparty-category-price-types` | — | Sales customer price rules |
| `sales-customers-full` | — | counterparties, then price rules |

Example:

```powershell
dotnet run --project .\Host.Jobs.csproj -- --job=stocks --rootId=<one-c-root-id>
```

Repeat `--rootId` for multiple roots. Exit code `0` and `[SUCCESS] Job finished OK` mean no exception; still validate import counts. Code `1` means job/argument/runtime failure; code `2` means a root-dependent job lacked `--rootId`.

## Manual 1C write smoke test

This command performs a real `CreateSiteRequest` write. Use it only with a test 1C endpoint and test counterparty/product data. `OneCSoap:Endpoint`, `Username`, and `Password` must be supplied through the Host.Jobs User Secrets or environment variables; never put them in `appsettings.json` or command arguments.

```powershell
$jobProject = 'C:\Users\Віталій\RiderProjects\KedrStore\src\Bootstrapper\Host.Jobs\Host.Jobs'
Set-Location $jobProject
$env:DOTNET_ENVIRONMENT = 'Development'
dotnet run --project .\Host.Jobs.csproj -- --job=one-c-smoke-write --counterpartyId=000000298 --productId=8190 --quantity=10 --amount=1000 --orderNumber=TEST-SO-20260818-001 --comment='TEST: manual SOAP verification.'
```

Numeric product IDs are left-padded to nine characters before transmission, so `8190` is sent as `000008190`. A confirmed 1C document ID produces exit code `0`; a business rejection produces exit code `3`.

Recorded live-test evidence, response interpretation, and the code map are maintained in the feature's [1C write smoke verification](../../specs/sales/001-admin-order-one-c-sync/tasks/test/06.3-one-c-write-smoke-test.md).
