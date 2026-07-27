# Phase 05 — Query contract and frontend handoff

**Status:** draft

## Outcome

Expose category metadata additively where frontend category navigation requires it and give the storefront a migration path away from its constants file.

## Work

- Inventory existing category DTOs and endpoints.
- Extend only the necessary response contracts with explicit full/short UA/RU names, `SortOrder` and `Level`.
- Sort category-tree query results by `SortOrder`, then a deterministic fallback.
- Document the frontend cutover and compatibility period.

## Acceptance criteria

- [ ] Existing endpoint fields and route behavior are unchanged.
- [ ] New fields are additive and documented.
- [ ] Frontend can render and sort using backend data without its manual mapping.

## Verification

- API contract/integration tests and frontend consumer smoke check.
