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
