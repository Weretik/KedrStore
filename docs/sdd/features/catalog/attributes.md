# Catalog attributes

**Static code check:** 2026-11-05. **Module:** Catalog.

Product attributes (color, size, side, type, material, …), their dictionary, and the review state of each product. Phase 1 of the [attributes SDD](../../specs/catalog/006-attributes-variants-filters/README.md): data model and import; groups, Admin, parser, filters, and SEO follow.

## Structure and relationships

[ImportCatalogAttributesJob](../../../../src/Catalog/Catalog.Application/Jobs/Attributes/ImportCatalogAttributesJob.cs) reads the [dictionary file](../../../../src/Catalog/Catalog.Application/Attributes/Import/AttributeDictionaryFile.cs) and the [recognizer CSV](../../../../src/Catalog/Catalog.Application/Attributes/Import/AttributeCsvFile.cs), then syncs attributes, values, and aliases by code, product values and reviews by product, and offers category filters. [AttributeImportCatalog](../../../../src/Catalog/Catalog.Application/Attributes/Import/AttributeImportCatalog.cs) holds the built-in settings (kind, unit, URL slug, filterable, variant axis).

[ProductAttributeValue](../../../../src/Catalog/Catalog.Domain/Entities/Attributes/ProductAttributeValue.cs) refuses automatic changes when a person set or locked the value. [ProductAttributeReview](../../../../src/Catalog/Catalog.Domain/Entities/Attributes/ProductAttributeReview.cs) keeps the base name and reasons; an accepted review stays accepted until a person reopens it. Original 1C names are never changed.

## Code structure

```text
src/Catalog/
├── Catalog.Domain/Entities/Attributes/  # attribute, value, alias, category filter, product value, review
├── Catalog.Application/
│   ├── Attributes/Import/  # file readers and built-in attribute settings
│   └── Jobs/Attributes/ImportCatalogAttributesJob.cs  # the import and its report
└── Catalog.Infrastructure/Configurations/Attributes/  # EF mappings
```

## Tests

[AttributeImportTests.cs](../../../../tests/IntegrationTests/Catalog/Attributes/AttributeImportTests.cs) · [AttributeImportFilesTests.cs](../../../../tests/UnitTests/Catalog/Attributes/AttributeImportFilesTests.cs)

Focused run: `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter "FullyQualifiedName~IntegrationTests.Catalog.Attributes"` and `dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~UnitTests.Catalog.Attributes"`.

## Related sources

[Data](../../architecture/data-model/catalog-products.md) · [CLI](../../operations/jobs/host-jobs-cli.md) · [Input files](../../../catalog-attributes/README.md).
