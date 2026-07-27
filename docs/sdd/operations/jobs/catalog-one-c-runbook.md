# Catalog OneC runbook

For a normal refresh run one `full` job; it uses the safe import order and rebuilds projections last.

```powershell
$env:DOTNET_ENVIRONMENT = 'Development'
dotnet run --project src/Bootstrapper/Host.Jobs/Host.Jobs/Host.Jobs.csproj -- --job=full
```

| Situation | Job | Follow-up |
| --- | --- | --- |
| changed hierarchy | `category --rootId=<id>` | product details if needed, then rebuild projection |
| details/visibility changed | `productdetails --rootId=<id>` | rebuild is automatic |
| stock changed | `stocks --rootId=<id>` | rebuild is automatic |
| prices changed | `prices --rootId=<id>` | rebuild is automatic |
| projection stale after DB repair | `rebuild-projections` | verify lists |

After a run: check final success line and received/mapped/synced counts. Verify a known `exportToSite:false` product exists in `/api/admin/products/all` and is absent from the public product list after projection rebuild. Do not overlap targeted jobs with `full` for the same root.
