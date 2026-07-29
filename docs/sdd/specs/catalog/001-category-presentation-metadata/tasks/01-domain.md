# Phase 01 — Domain

- [x] T005 Modify `ProductCategory` in `src/Catalog/Catalog.Domain/Entities/ProductCategory.cs` to own localized short names, sort order, and level without changing existing identity, name, slug, parent, or path semantics. **Dependencies:** T002.
- [x] T006 Add explicit domain errors and enforce short-label, sort-order, and non-negative-level invariants in `src/Catalog/Catalog.Domain/Errors/` and the aggregate API. **Dependencies:** T005.
- [x] T007 [P] Add focused aggregate tests under `tests/UnitTests/` following the existing category test convention. **Dependencies:** T005, T006.
- [x] T008 Run the targeted category domain tests and record the command/result here. **Dependencies:** T007.

## T008 verification (2026-07-29)

- `dotnet test tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~ProductCategoryPresentationMetadataTests` — passed: 8/8.

## Checkpoint

Domain has no Infrastructure dependency, original 1C name cannot be replaced by a display label, and aggregate invariants are covered by tests.
