# 008 — Codes: products and decodings by category

**Module:** Catalog. **Status:** accepted and implemented 2026-11-06 (backend; admin in kedr-web). **Date:** 2026-11-06.

The owner picked variant В of the [mockup «Не розшифровані коди»](https://claude.ai/artifact/6rUFbwvzuogodH1KopmzkT): a queue of codes and, for the chosen code, its products grouped by category with a decoding for all products and an own decoding per category.

## Goal and boundaries

In «Вітрина сайту → Не розшифровані коди» the owner sees which products contain a 1C code (for example `AL`) and decodes it. The same code may mean different things in different categories (`AL` in handles and in Korfad doors), so a decoding can be given per category.

**Out of scope:** changing 1C names; decodings per product; new attributes from this screen.

## Rules

- **C-01.** The products of a code are the on-site, not deleted products whose original names contain the code as the recognizer finds it (look-alike Cyrillic letters and case included), grouped by their own (final) category, most products first, with the full category path.
- **C-02.** A code has a decoding for all products (spec 006: a value, or none) and, per category, its own decoding: a value of the code's attribute, or none (the code means nothing there). A category without its own decoding follows the one for all.
- **C-03.** The recognizer takes the category decoding of the product's category first, then the decoding for all, then the dictionary file.
- **C-04.** One request saves the decoding for all (unless `applyToAll` is false, which keeps the code undecided for all) and replaces the whole list of category decodings when `categories` is sent. Values must belong to the code's attribute; categories must exist. Products that contain the code are recognized again at once.
- **C-05.** Reading is open to Manager, Seo, and Admin; deciding to Admin and Seo (`CanManageSiteCatalog`), as for spec 006 codes.

## Data

New table `CatalogAttributeAliasCategoryValues` (`AliasId`, `CategoryId`) → `ValueId` (nullable), migration `CatalogCodeCategoryValues` (additive; checked on a clean test database by the integration tests, M-14 of spec 007 applies to production).

## File-role map

| Role | File |
| --- | --- |
| Domain entity | `Catalog.Domain/Entities/Attributes/AttributeAliasCategoryValue.cs` |
| EF mapping, DbSet, migration | `Catalog.Infrastructure/Configurations/Attributes/AttributeAliasCategoryValueConfiguration.cs`, `CatalogDbContext.cs`, `Migrations/*_CatalogCodeCategoryValues.cs` |
| Products of a code (C-01) | `Catalog.Application/Features/Attributes/GetCodeProducts/` |
| Decide (C-02, C-04) | `Catalog.Application/Features/Attributes/DecideCode/` |
| Recognizer (C-03) | `Catalog.Application/Attributes/Recognition/RecognitionDictionary.cs`, `ProductNameRecognition.Lexical.cs`, `Jobs/Attributes/RecognizeCatalogAttributesJob.cs` |
| Contract | `Catalog.Contracts/Attributes/CatalogAttributeDtos.cs`, [attributes.openapi.yaml](../../../contracts/catalog/attributes.openapi.yaml) |
| HTTP | `Catalog.Api/Controllers/AdminCatalogAttributesController.cs` |

## Scenarios and evidence

- **SC-C01 (C-02, C-03).** A category decoding wins over the decoding for all; a decoding for another category leaves the product undecided — `AttributeRecognizerDecisionTests` (unit).
- **SC-C02 (C-01, C-04).** `AL` decided as «none» only for «На планці»: product 60 loses the reason, product 80 of another category keeps it, the code stays undecided for all, the products are listed by category with the own decoding; an empty list removes it again — `AttributeRecognitionTests.OwnerDecidesACode_ForOneCategory_…` (integration).
