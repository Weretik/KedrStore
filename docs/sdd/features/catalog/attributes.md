# Catalog attributes

**Static code check:** 2026-11-05 (phase 2). **Module:** Catalog.

Product attributes (color, size, side, type, material, …), their dictionary, and the review state of each product. Phases 1–2 of the [attributes SDD](../../specs/catalog/006-attributes-variants-filters/README.md): data model, import, and variant groups; Admin screens, parser, filters, and SEO follow.

## Structure and relationships

[ImportCatalogAttributesJob](../../../../src/Catalog/Catalog.Application/Jobs/Attributes/ImportCatalogAttributesJob.cs) reads the [dictionary file](../../../../src/Catalog/Catalog.Application/Attributes/Import/AttributeDictionaryFile.cs) and the [recognizer CSV](../../../../src/Catalog/Catalog.Application/Attributes/Import/AttributeCsvFile.cs), then syncs attributes, values, and aliases by code, product values and reviews by product, and offers category filters. [AttributeImportCatalog](../../../../src/Catalog/Catalog.Application/Attributes/Import/AttributeImportCatalog.cs) holds the built-in settings (kind, unit, URL slug, filterable, variant axis).

[ProductGroupBuilder](../../../../src/Catalog/Catalog.Application/Groups/ProductGroupBuilder.cs) runs at the end of the import and after every admin group change. Products with the same base name ([ProductGroupKey](../../../../src/Catalog/Catalog.Application/Groups/ProductGroupKey.cs)) in one category form a [ProductGroup](../../../../src/Catalog/Catalog.Domain/Entities/Groups/ProductGroup.cs) when there are at least two; No name and deleted products never join automatically, and rows a person pinned ([ProductGroupMember](../../../../src/Catalog/Catalog.Domain/Entities/Groups/ProductGroupMember.cs)) are never moved. The main product stays while it is on the site; otherwise the builder takes the best one (on the site, with a photo, in stock, smallest 1C code). Variants that color, size, and side do not tell apart get the review reason `same_variants` («Однакові варіанти»), which the import keeps.

Admin API ([contract](../../contracts/catalog/product-groups.openapi.yaml), policy CanManageProducts; group slug Admin only): [AdminProductGroupsController](../../../../src/Catalog/Catalog.Api/Controllers/AdminProductGroupsController.cs) for groups and `GET/PUT /api/admin/products/{id}/group` in [AdminProductsController](../../../../src/Catalog/Catalog.Api/Controllers/AdminProductsController.cs) for one product.

[ProductAttributeValue](../../../../src/Catalog/Catalog.Domain/Entities/Attributes/ProductAttributeValue.cs) refuses automatic changes when a person set or locked the value. [ProductAttributeReview](../../../../src/Catalog/Catalog.Domain/Entities/Attributes/ProductAttributeReview.cs) keeps the base name and reasons; an accepted review stays accepted until a person reopens it. Original 1C names are never changed.

## Code structure

```text
src/Catalog/
├── Catalog.Domain/Entities/
│   ├── Attributes/  # attribute, value, alias, category filter, product value, review
│   └── Groups/      # variant group and the group of each product
├── Catalog.Application/
│   ├── Attributes/Import/  # file readers and built-in attribute settings
│   ├── Groups/             # group builder, key, report
│   ├── Features/Groups/    # admin queries and commands (picker, group, move, create, rename, main)
│   └── Jobs/Attributes/ImportCatalogAttributesJob.cs  # the import and its report
├── Catalog.Api/Controllers/AdminProductGroupsController.cs
└── Catalog.Infrastructure/Configurations/{Attributes,Groups}/  # EF mappings
```

## Tests

[AttributeImportTests.cs](../../../../tests/IntegrationTests/Catalog/Attributes/AttributeImportTests.cs) · [ProductGroupBuilderTests.cs](../../../../tests/IntegrationTests/Catalog/Groups/ProductGroupBuilderTests.cs) · [AttributeImportFilesTests.cs](../../../../tests/UnitTests/Catalog/Attributes/AttributeImportFilesTests.cs) · [ProductGroupTests.cs](../../../../tests/UnitTests/Catalog/Groups/ProductGroupTests.cs)

Focused run: `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter "FullyQualifiedName~IntegrationTests.Catalog.Attributes|FullyQualifiedName~IntegrationTests.Catalog.Groups"` and `dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~UnitTests.Catalog.Attributes|FullyQualifiedName~UnitTests.Catalog.Groups"`.

## Related sources

[Data](../../architecture/data-model/catalog-products.md) · [CLI](../../operations/jobs/host-jobs-cli.md) · [Input files](../../../catalog-attributes/README.md).
