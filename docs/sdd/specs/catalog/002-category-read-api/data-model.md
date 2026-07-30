# Category read API — data model

## Context

The feature is read-only. It projects the existing `ProductCategory` aggregate; it introduces no table, column, migration, or persisted relation.

## Model

```text
ProductCategory (existing aggregate)
├── Id: positive integer public identity
├── Slug: public route key
├── ParentId: nullable immediate-parent identity
├── ProductTypeIdOneC: admin-visible imported root/source key
├── Name: admin-visible original source name
├── ShortNameUk / ShortNameRu: locale-selected public name
├── SortOrder: sibling display order
├── Level: root-relative depth
└── Path: internal hierarchy source; not exposed
```

## Invariants and integrity

- The API projection does not expose the EF entity or the technical `Path`; it uses task-specific DTOs.
- `ParentId`, `Level`, and `Children` must describe one consistent, acyclic category tree. Existing category import/domain rules remain the owner of that integrity.
- The public response preserves the existing `SortOrder`-then-`Id` sibling ordering. No client-supplied sorting is accepted.
- `name` is selected only from the persisted localized short names; it never changes the stable `id`, `slug`, `parentId`, `sortOrder`, or `level` fields.
- The admin contract may expose the original `Name`, both localized short names, and `ProductTypeIdOneC`; it still does not expose technical `Path` or the persistence entity.
