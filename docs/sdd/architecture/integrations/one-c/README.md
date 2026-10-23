# 1C integration

This section records the stable boundary between KedrStore and 1C. It describes current code behavior without credentials, endpoints, or real production identifiers.

```text
1C SOAP → BuildingBlocks.Integrations.OneC → module adapter → Application job/service → database
                                   │                         │
                              WCF/auth only             Catalog or Sales responsibility
```

## Browse by task

- SOAP operations and normalization: [client contract](client-contract.md)
- Catalog import order, projection, and visibility: [import flow](catalog-import-flow.md)
- Catalog terms, including Doors, Hardware, and Cosmos: [product glossary](../../../../product/glossary.md)
- Counterparty import and Identity user creation: [Sales/Identity flow](sales-counterparty-identity-sync.md)
- Deletion semantics and safe execution: [data safety](data-safety.md)
- Running and diagnosing jobs: [Host.Jobs runbooks](../../../operations/jobs/README.md)

## Responsibility boundaries

- `BuildingBlocks.Integrations.OneC` owns generated WCF transport and basic authentication. `Generated/Reference.cs` is generated code; do not edit it manually.
- Catalog and Sales Infrastructure own their module adapters and data mapping.
- Catalog Application coordinates jobs through `IOneCClient` and does not depend on WCF types.
- `Host.Jobs` is a one-shot console host with no scheduler of its own; cron, CI, or deployment automation invokes it.

The contract, data ownership, safety, and operations are documented separately so a field mapping change does not silently become a destructive data change.
