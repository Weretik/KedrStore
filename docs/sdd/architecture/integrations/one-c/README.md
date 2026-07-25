# OneC integration

This section records the durable boundary between KedrStore and 1C. It documents current code behaviour, never credentials, endpoints or real production identifiers.

```text
1C SOAP → BuildingBlocks.Integrations.OneC → module adapter → Application job/service → database
                                   │                         │
                              WCF/auth only            Catalog or Sales ownership
```

## Read by task

- SOAP operations and value normalization: [client contract](client-contract.md)
- Catalog import order, projection and visibility: [catalog import flow](catalog-import-flow.md)
- Deletion semantics and safe operation: [data safety](data-safety.md)
- Running and diagnosing jobs: [Host.Jobs runbooks](../../../operations/jobs/README.md)

## Boundaries

- `BuildingBlocks.Integrations.OneC` owns generated WCF transport and Basic authentication. `Generated/Reference.cs` is generated code: do not edit it manually.
- Catalog and Sales Infrastructure own their module-specific adapters and mappings.
- Catalog Application owns job orchestration through `IOneCClient`; it does not depend on WCF types.
- `Host.Jobs` is a one-run console host, not a scheduler. Cron/CI/deployment automation invokes it.

This mirrors production runbook practice: contract, data ownership, safety and operations are separate so a field mapping change cannot silently become a destructive data change.
