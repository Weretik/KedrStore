# Host.Jobs and OneC operations

`Host.Jobs` is a console host for explicit import, synchronization and projection jobs. It does not start a scheduler by itself: a caller must pass `--job=<name>`.

## Run a job

~~~powershell
dotnet run --project src/Bootstrapper/Host.Jobs/Host.Jobs/Host.Jobs.csproj -- --job=<name>
~~~

Jobs `category`, `prices`, `productdetails` and `stocks` additionally require one or more root identifiers:

~~~powershell
dotnet run --project src/Bootstrapper/Host.Jobs/Host.Jobs/Host.Jobs.csproj -- --job=category --rootId=<one-c-root-id>
~~~

## Supported job names

| Job | Purpose |
| --- | --- |
| `full` | Full OneC synchronization. |
| `pricetypes` | Synchronize price types. |
| `category` | Synchronize a category root. |
| `productdetails` | Synchronize product details for a root. |
| `stocks` | Synchronize stock data for a root. |
| `prices` | Synchronize prices for a root. |
| `counterparties` | Synchronize counterparties. |
| `counterparty-category-price-types` | Synchronize counterparty category price types. |
| `sales-customers-full` | Full sales-customer synchronization. |
| `rebuild-projections` | Rebuild product-list projections. |

These jobs can change application data and depend on OneC/database configuration. Run them intentionally, against the correct environment, and inspect console output for the final success/failure result. Do not make a synchronization job part of an ordinary API startup or feature test without an explicit specification.
