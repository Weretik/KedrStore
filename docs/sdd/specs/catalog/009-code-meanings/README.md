# 009 — Codes: several meanings in other attributes

**Module:** Catalog. **Status:** accepted and implemented 2026-11-06 (backend; admin in kedr-web). **Date:** 2026-11-06.

The owner asked why a code in «Не розшифровані коди» can be decoded only within the attribute the import gave it (`ZCG` → «Код конфігурації циліндра», which has no values), and chose «Несколько ознак»: a code may mean values of any attributes, for example `ZCG` = a configuration and a colour.

## Goal and boundaries

The owner decodes a code as a list of «ознака → значення» pairs, for all products and per category (spec [008](../008-code-decisions-by-category/README.md)).

**Out of scope:** changing 1C names; moving the code itself to another attribute (the recognizer still finds it as the dictionary says); decodings per product.

## Rules

- **CM-01.** A decoding is a list of meanings: values of any attributes, at most one per attribute; an empty list means the code means nothing to filter by. A meaning in the code's own attribute is stored as before (spec 006 alias value, spec 008 category value); meanings in other attributes are stored as extra meanings.
- **CM-02.** `meanings`, when sent, replaces `valueId` in the request, for all products and per category. Each value must belong to its attribute. Saving replaces the extra meanings of that scope (for all, or all categories when `categories` is sent).
- **CM-03.** Extra meanings reach the products whose names contain the code as the recognizer finds it (attribute, scope, and text), so a hinge named `ZCG-40KGS` does not get a cylinder code's colour. A category with its own decoding uses only its own meanings, even none.
- **CM-04.** An extra meaning wins over what the recognizer found for the same attribute and is confirmed. Values a person set or locked stay, as for every automatic value.
- **CM-05.** The queue and the products of a code show the meanings: for all products, and the category's own ones.

## Data

New table `CatalogAttributeAliasMeanings` (`Id`, `AliasId`, `CategoryId` nullable, `AttributeId`, `ValueId`), unique (`AliasId`, `CategoryId`, `AttributeId`) with nulls not distinct; migration `CatalogCodeMeanings` (additive).

## File-role map

| Role | File |
| --- | --- |
| Domain entity | `Catalog.Domain/Entities/Attributes/AttributeAliasMeaning.cs` |
| EF mapping, DbSet, migration | `Catalog.Infrastructure/Configurations/Attributes/AttributeAliasMeaningConfiguration.cs`, `CatalogDbContext.cs`, `Migrations/*_CatalogCodeMeanings.cs` |
| Read meanings (CM-05) | `Catalog.Application/Features/Attributes/Common/CodeMeaningReader.cs`, `UnresolvedCodeReader.cs`, `GetCodeProducts/` |
| Decide (CM-01, CM-02) | `Catalog.Application/Features/Attributes/DecideCode/` |
| Apply (CM-03, CM-04) | `Catalog.Application/Attributes/Import/CodeMeanings.cs`, `Attributes/Recognition/RecognitionRow.cs` (`FoundCodes`), `ProductNameRecognition.Lexical.cs`, `Jobs/Attributes/RecognizeCatalogAttributesJob.cs` |
| Contract | `Catalog.Contracts/Attributes/CatalogAttributeDtos.cs`, [attributes.openapi.yaml](../../../contracts/catalog/attributes.openapi.yaml) |

## Scenarios and evidence

- **SC-CM01 (CM-03, CM-04).** Meanings replace recognized values of their attribute, a code not found changes nothing, a category with its own decoding keeps only its meanings, the recognizer reports `ZCG` in a cylinder name — `CodeMeaningsTests` (unit).
- **SC-CM02 (CM-01…CM-05).** `ZCG` decoded as colour «Золото» for all: cylinder 7894 gets it confirmed and loses the reason, hinge 7158 does not; the cylinders' own empty decoding takes it away; a value of another attribute is refused — `AttributeRecognitionTests.OwnerGivesACompositeCode_…` (integration).
