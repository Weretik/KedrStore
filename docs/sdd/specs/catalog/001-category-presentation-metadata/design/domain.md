# Category presentation metadata — design: Domain

## Responsibility

`ProductCategory` remains the aggregate that owns category identity, original 1C name, slug, parent relation, and `CategoryPath`. It will also own short localized names, sort order, and level. Its factory/update API must make the distinction between source name and presentation metadata explicit and enforce required/length constraints plus `Level >= 0`.

`Path` and `ParentId` remain the structural source of truth. The application import flow computes the level only after the final hierarchy, including configured manual groups, is known; the aggregate rejects an invalid level but does not need to know the complete imported tree.

## Cross-module interaction

The 1C import jobs in Catalog Application read typed configuration through an Application abstraction and update the aggregate. Infrastructure provides persistence and options binding. No domain dependency on EF Core, configuration, generated SOAP types, or API DTOs is introduced.

The existing `GetCategoriesQuery` remains a query. If metadata is exposed, it uses a no-tracking projection and returns additive fields; it performs no import or write operation.

## Risks

- Imported historical paths or manually configured groups can be malformed or cyclic; level calculation must fail safely or use the agreed documented fallback rather than persist inconsistent metadata.
- Existing `ProductCategory.Update` is used by import. Its signature change must keep all current callers correct and prevent a missing configuration entry from erasing previously approved metadata unintentionally.
