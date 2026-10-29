# Feature specifications

## Existing specifications

This is an index of design decisions and delivery records; the [implemented feature map](../features/README.md) points to the architecture of current code. Check each work item's status in its README and in code.

| Module | Specifications |
| --- | --- |
| Catalog | [category metadata](catalog/001-category-presentation-metadata/README.md), [category reads](catalog/002-category-read-api/README.md), [missing photos](catalog/003-missing-product-photos/README.md), [Admin product details](catalog/004-admin-product-detail/README.md), [Admin product list](catalog/admin-product-list/README.md), [1C/Cosmos root](catalog/cosmos-one-c-root/README.md), [stock projection refresh](catalog/stock-projection-refresh/README.md) |
| Sales | [order creation and 1C](sales/001-admin-order-one-c-sync/README.md), [manager order reads](sales/002-manager-order-read-api/README.md), [customer reads](sales/003-customer-read-api/README.md), [Admin order workspace](sales/004-admin-order-workspace/README.md), [Admin order drafts](sales/005-admin-order-drafts/README.md), [workspace responsibility remediation](sales/006-admin-workspace-structure-remediation/README.md), [customer reconciliation with 1C](sales/007-customer-sync-reconciliation/README.md), [workspace customer prices and adjustments](sales/008-workspace-customer-prices-and-adjustments/README.md) |
| Identity | [authentication and authorization rollout](identity/001-authentication-rollout/README.md) |
| Platform | [separate migration job](platform/001-database-migration-job/README.md), [test architecture](platform/002-test-architecture/README.md) |

This directory contains requirements, design decisions, contracts, task records, and verification evidence for individual changes. Update the specification for a nontrivial behavior change before implementation. A status or plan in a specification alone does not prove that the behavior exists in code.

## Structure

```text
docs/sdd/specs/
└── <module>/<NNN>-<feature>/     specification of one feature
```

- Start new work with `backend-feature-spec` and the relevant [delivery rules](../standards/delivery-rules.md). A feature directory records the observable goal, scope, rules, scenarios, design, contracts, tasks, and verification evidence. Create only the artifacts needed for that change.
- If behavior changes in an existing specification, preserve completed task IDs and verification history. Assign `R-*` and `SC-*` to changed behavior, add traceability for changed scenarios and regression dependencies, and assign `TS-*` or `EN-*` to new tasks. Keep completed old task files as history; do not renumber them or rewrite their evidence. Migrate the entire old specification to a new format only when the user explicitly requests it.
- Store machine-readable public API contracts in [`docs/sdd/contracts/`](../contracts/README.md).
- For catalog terms related to doors, hardware, Cosmos, or 1C product data, use the [product glossary](../../product/glossary.md).

## Lifecycle

Draft → accepted → in progress → verified → complete.

Implementation must not silently diverge from the accepted specification. Record a dated change note with the reason, scope impact, and new verification.
