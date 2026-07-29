# OneC import data safety

1C is authoritative for catalog fields in the roots it manages. A feature that changes categories, product details, stock or prices locally must define whether the next import will overwrite that field.

`ExportToSite` is import-owned visibility, not a delete signal.

## Current guards

| Flow | Guard |
| --- | --- |
| categories | skips deletion when 1C returns no categories |
| products | returns before writes when response is empty; throws before deletion when non-empty response maps to zero |
| price types/stocks/prices | empty result returns before writes |

For a valid non-empty category/product result, the job can remove rows absent from the source root. Jobs have no dry run, distributed lock, automatic retry or scheduler today.

Before running: select the correct environment/root, avoid overlapping runs for the same root, and inspect received/mapped/synced counts. If counts are unexpected, stop subsequent runs, preserve non-secret diagnostics, correct source/configuration/mapping, then rerun a complete synchronization. Do not infer deletion from the public list: its visibility additionally depends on `ExportToSite` and list filters.
