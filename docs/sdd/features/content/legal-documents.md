# Site documents

**Static code check:** 2026-11-04. **Module:** Content.

The texts of four site pages: the public offer, the privacy policy, the returns policy and delivery & payment, each in Ukrainian and Russian. Every document has numbered editions. Managers and Admins edit one draft at a time in the admin; publishing it gives the next number and the «Редакція від» date and puts it on the site, and the edition it replaces stays as read-only history. The site reads the published edition anonymously. The texts the site kept in code were moved into the database as edition 1 (from 2026-11-03) by a data migration.

## Structure and relationships

The [public and admin endpoint groups](../../../../src/Modules/Content/Content.Api/Endpoints/Documents/DocumentsEndpoints.cs) (one `<UseCase>Endpoint` per operation) turn the URL kind (`public-offer`, `privacy-policy`, `returns-policy`, `delivery-payment`; [LegalDocumentKinds](../../../../src/Modules/Content/Content.Application/Features/Documents/Shared/LegalDocumentKinds.cs)) into the enum and answer 404 for anything else. [LegalDocumentVersion](../../../../src/Modules/Content/Content.Domain/Documents/LegalDocumentVersion.cs) is the aggregate: status Draft → Published → Archived, both language texts owned by it, and only a draft accepts new texts. Handlers live in `Features/Documents/{Public,Admin}`:

- create draft — 409 when a draft exists; copies the published texts when no texts are sent;
- update draft — sanitizes the HTML with the article sanitizer, 409 for a non-draft;
- publish — the date may not be earlier than the edition on the site; archives that edition and publishes the draft as max number + 1 in one save;
- delete draft — 409 for a non-draft.

The database backs the rules with filtered unique indexes: one draft and one published edition per kind.

The HTML may contain `{site}`, `{email}` and `{phone}`; the site replaces them with its host and the saved contacts, so a new phone number needs no new edition.

## Code structure

```text
src/Modules/Content/
├── Content.Api/
│   ├── Endpoints/Documents/ public group        # public GET {lang}/documents/{kind}
│   ├── Endpoints/Documents/ admin group   # list, versions, draft CRUD, publish
│   └── Documents/LegalDocumentResponses.cs            # responses, requests, mapping
├── Content.Application/Features/Documents/
│   ├── Common/                                        # DTOs, kinds, texts validator, specs, editor name + sanitizing
│   ├── Public/GetPublished/
│   └── Admin/{GetList,GetVersions,GetVersion,CreateDraft,UpdateDraft,Publish,DeleteDraft}/
├── Content.Domain/Documents/                          # version aggregate, text, kinds/statuses, errors
└── Content.Infrastructure/
    ├── Configurations/LegalDocumentVersionConfiguration.cs
    ├── Seed/LegacyLegalDocumentSeed.cs + Seed/LegalDocuments/legal-documents.json  # edition 1 texts (embedded)
    └── Migrations/*_AddLegalDocuments.cs, *_SeedLegalDocuments.cs
```

## Data

Tables `content."LegalDocumentVersions"` and `content."LegalDocumentTexts"`; see the [database map](../../architecture/data-model/content-articles.md).

## Tests

- Unit: [LegalDocumentVersionTests](../../../../tests/UnitTests/Content/Documents/LegalDocumentVersionTests.cs) and [GalleryAndDocumentValidatorsTests](../../../../tests/UnitTests/Content/Documents/GalleryAndDocumentValidatorsTests.cs) — `dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~UnitTests.Content"`.
- Integration: [HTTP, access, draft → publish → history](../../../../tests/IntegrationTests/Content/Documents/LegalDocumentsApiTests.cs), [PostgreSQL seed and unique indexes](../../../../tests/IntegrationTests/Content/Documents/LegalDocumentsPostgresTests.cs) — `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter "FullyQualifiedName~IntegrationTests.Content"`.

## Related sources

- [Specification](../../specs/content/004-legal-documents/README.md)
- [Contract](../../contracts/content/documents.openapi.yaml)
