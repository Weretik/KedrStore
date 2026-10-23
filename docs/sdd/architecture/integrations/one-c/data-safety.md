# Data safety for 1C imports

1C is the source of truth for catalog fields in roots it manages. A feature that changes categories, product details, stock, or prices locally must state whether the next import will overwrite that field.

`ExportToSite` is an import-managed visibility flag, not a deletion signal.

## Current safeguards

| Flow | Safeguard |
| --- | --- |
| Categories | Skips deletion when 1C returns no categories |
| Products | Stops before writes on an empty response; if a non-empty response maps to zero rows, throws before deletion |
| Price types/stock/prices | Stops before writes on an empty result |

For valid non-empty category/product results, a job may delete records missing from the source root. At the time of review, jobs had no dry-run mode, distributed lock, automatic retries, or internal scheduler.

Before running, choose the correct environment and root, avoid overlapping runs for the same root, and check received/mapped/synced counts. If they are unexpected, stop subsequent runs, preserve diagnostics without secrets, fix the source/configuration/mapping, and only then retry the full sync. Do not infer deletion from the public list: visibility also depends on `ExportToSite` and list filters.
