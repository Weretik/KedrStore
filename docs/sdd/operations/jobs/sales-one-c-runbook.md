# Sales OneC runbook

Sales has an independent sync boundary; it only shares low-level SOAP transport with Catalog.

| Command | Operation |
| --- | --- |
| `--job=counterparties` | imports/updates/restores counterparties according to Sales service policy |
| `--job=counterparty-category-price-types` | imports customer/category price-type rules |
| `--job=sales-customers-full` | counterparties first, then price rules |

```powershell
$env:DOTNET_ENVIRONMENT = 'Development'
dotnet run --project src/Bootstrapper/Host.Jobs/Host.Jobs/Host.Jobs.csproj -- --job=sales-customers-full
```

Review imported, updated, restored, deleted and skipped counts in the final logs before accepting the refresh.
