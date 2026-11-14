# Page «Співпраця»

**Static code check:** 2026-11-05. **Module:** Content.

The editable copy of the site page «Співпраця» (wholesale offer) in Ukrainian and Russian: hero and its contact card, assortment intro, company block, three order steps, wholesale questions, and the closing call. Managers and Admins replace both languages at once; the site reads one language anonymously. Buttons, links, category cards, and contact values are not here.

## Structure and relationships

[WholesalePage](../../../../src/Content/Content.Domain/Wholesale/WholesalePage.cs) is a single-row aggregate; each language is a [WholesalePageText](../../../../src/Content/Content.Domain/Wholesale/WholesalePageText.cs) value (exactly three steps, 1–12 questions, plain trimmed texts with limits) stored as one jsonb document. Handlers live in `Features/Wholesale/{Public,Admin}`; the admin update validates both texts per field ([WholesaleTextValidator](../../../../src/Content/Content.Application/Features/Wholesale/Shared/WholesaleTextValidator.cs)).

## Code structure

```text
src/Content/
├── Content.Api/
│   ├── Endpoints/Wholesale/ public group        # public GET {lang}/wholesale
│   ├── Endpoints/Wholesale/ admin group   # GET, PUT
│   └── Wholesale/WholesaleResponses.cs                # payloads and mapping
├── Content.Application/Features/Wholesale/
│   ├── Common/                                        # DTOs, mapper, per-field validator
│   ├── Public/Get/
│   └── Admin/{Get,Update}/
├── Content.Domain/Wholesale/                          # page, one language, step, question, errors
└── Content.Infrastructure/
    ├── Configurations/WholesalePageConfiguration.cs   # jsonb per language
    ├── Seed/LegacyWholesaleSeed.cs                    # the texts the site kept in code
    └── Migrations/*_AddWholesalePage.cs
```

## Data

Table `content."WholesalePages"`; see the [database map](../../architecture/data-model/content-articles.md).

## Tests

- Unit: [WholesalePageTests](../../../../tests/UnitTests/Content/WholesalePageTests.cs) — `dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~WholesalePageTests"`.
- Integration: [HTTP, access, validation](../../../../tests/IntegrationTests/Content/WholesalePageApiTests.cs), [PostgreSQL seed and jsonb](../../../../tests/IntegrationTests/Content/WholesalePagePostgresTests.cs) — `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter "FullyQualifiedName~Wholesale"`.

## Related sources

- [Specification](../../specs/content/009-wholesale-page/README.md)
- [Contract](../../contracts/content/wholesale.openapi.yaml)
