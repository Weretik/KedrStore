# Company requisites

**Static code check:** 2026-11-04. **Module:** Content.

The payment requisites the site shows on its requisites page: the ФОП name, ЄДРПОУ/РНОКПП, registration extract number, IBAN, bank name and МФО. Only Admins change them, because a wrong IBAN costs money, and every saved change is logged with who, when, and the old and new value of each field. The site reads them anonymously. The values the site kept in code were moved into the database by the migration that creates the table.

## Structure and relationships

The [public controller](../../../../src/Content/Content.Api/Controllers/CompanyRequisitesController.cs) and [admin controller](../../../../src/Content/Content.Api/Controllers/AdminCompanyRequisitesController.cs) dispatch to two queries and one command. [CompanyRequisites](../../../../src/Content/Content.Domain/Requisites/CompanyRequisites.cs) is a single-row aggregate (`Id = 1`, guarded by a check constraint) that is only replaced as a whole. Its `Update` normalizes the input (trim; IBAN without spaces, upper case), checks the invariants, and returns the list of changed fields. The [handler](../../../../src/Content/Content.Application/Features/Requisites/Update/UpdateCompanyRequisitesCommandHandler.cs) turns a non-empty list into a [CompanyRequisitesChange](../../../../src/Content/Content.Domain/Requisites/CompanyRequisitesChange.cs) with the editor's email, saved in the same transaction; saving the same values writes no entry.

Rules: the code is 8 (ЄДРПОУ) or 10 (РНОКПП) digits, the IBAN is `UA` + 27 digits with a valid mod-97 checksum, the МФО is 6 digits and must equal digits 5–10 of the IBAN. The [validator](../../../../src/Content/Content.Application/Features/Requisites/Update/UpdateCompanyRequisitesCommandValidator.cs) reports each field by its JSON name; the МФО mismatch goes to `bankCode`.

Access: `GET /api/content/requisites` is anonymous; `/api/admin/content/requisites` and its `/history` need `RequireAdminRole`.

## Code structure

```text
src/Content/
├── Content.Api/
│   ├── Controllers/CompanyRequisitesController.cs       # public GET
│   ├── Controllers/AdminCompanyRequisitesController.cs  # admin GET, PUT, GET history
│   └── Requisites/CompanyRequisitesResponses.cs         # responses, request, mapping
├── Content.Application/Features/Requisites/
│   ├── Common/CompanyRequisitesDtos.cs                  # input, details, history, mapper
│   ├── Get/                                             # GetCompanyRequisitesQuery
│   ├── History/                                         # GetCompanyRequisitesHistoryQuery (50 newest)
│   └── Update/                                          # command, handler, validator
├── Content.Domain/Requisites/                           # aggregate, change log entry, field change, errors
└── Content.Infrastructure/
    ├── Configurations/CompanyRequisitesConfiguration.cs
    ├── Configurations/CompanyRequisitesChangeConfiguration.cs  # field changes as jsonb
    └── Migrations/*_AddCompanyRequisites.cs             # both tables + the site's current values
```

## Data

Tables `content."CompanyRequisites"` and `content."CompanyRequisitesChanges"`; see the [database map](../../architecture/data-model/content-articles.md).

## Tests

- Unit: [CompanyRequisitesTests](../../../../tests/UnitTests/Content/CompanyRequisitesTests.cs) — `dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~UnitTests.Content"`.
- Integration: [HTTP, access and history](../../../../tests/IntegrationTests/Content/CompanyRequisitesApiTests.cs), [PostgreSQL seed, single row and jsonb log](../../../../tests/IntegrationTests/Content/CompanyRequisitesPostgresTests.cs) — `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter "FullyQualifiedName~IntegrationTests.Content"`.

## Related sources

- [Specification](../../specs/content/003-company-requisites/README.md)
- [Contract](../../contracts/content/requisites.openapi.yaml)
