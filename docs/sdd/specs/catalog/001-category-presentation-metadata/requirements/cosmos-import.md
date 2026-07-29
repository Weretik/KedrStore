# Category presentation metadata — Cosmos import safety

## Behavior

- The backend owns one virtual category for the configured Cosmos root.
- When the category job runs for Cosmos, it ensures that category exists or is updated and returns without calling 1C `GetCategories`.
- When the product-details job runs for Cosmos, every mapped product receives the virtual category ID.
- The virtual category receives the same presentation metadata rules as other configured root categories.

## Rules and invariants

- The virtual category ID is stable and may not be owned by another root.
- A Cosmos run must not delete categories belonging to another root.
- Empty-response deletion protection remains in place for category and product imports.
- The feature must preserve current full-job ordering and projection rebuild behavior.

## Acceptance scenarios

1. Given the Cosmos root, when the category job runs, then no 1C categories operation is invoked and the configured virtual category exists with its presentation metadata.
2. Given Cosmos product details, when products are mapped, then each product uses the configured virtual category ID regardless of the source category name.
3. Given the virtual category ID is already associated with another root, when the Cosmos category job runs, then it fails explicitly and makes no conflicting reassignment.
