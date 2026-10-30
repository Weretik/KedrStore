# Catalog/1C Integration Runbook

For a routine refresh, run the single `full` job: it uses a safe import order and rebuilds projections at the end.

```powershell
# Run from the repository root
$env:DOTNET_ENVIRONMENT = 'Development'
dotnet run --project src/Bootstrapper/Host.Jobs/Host.Jobs/Host.Jobs.csproj -- --job=full
```

| Situation | Job | Next action |
| --- | --- | --- |
| Hierarchy changed | `category --rootId=<id>` | Refresh product details if needed, then rebuild the projection |
| Details/visibility changed | `productdetails --rootId=<id>` | Projection is rebuilt automatically |
| Stock changed | `stocks --rootId=<id>` | Projection is rebuilt automatically |
| Prices changed | `prices --rootId=<id>` | Projection is rebuilt automatically |
| Projection is stale after database repair | `rebuild-projections` | Check the lists |

After execution, check the final success line and, for each root, the `Category import` and `Product details import` lines: duration, received, created, updated, unchanged, deleted, restored, skipped, and guard-blocked counts. Missing rows are soft-deleted, not removed; if `GuardBlocked` is not zero, the run would have deleted more than 30% of the root's active rows and deleted none, so check the 1C response before rerunning. A warning about unresolved category names lists names that match no category or several categories; those products keep their category. After rebuilding the projection, confirm that a known product with `exportToSite:false` appears in `/api/admin/products/all` and is absent from the public list. Do not run targeted jobs in parallel with `full` for the same root.
