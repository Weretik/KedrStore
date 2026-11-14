# Company contacts

**Static code check:** 2026-11-04. **Module:** Content.

One set of company contacts that every site page shows: sales phone, email, Telegram link, Viber number, map link, and the city in Ukrainian and Russian. Managers and Admins change them in the admin; the site reads them anonymously. The values the site kept in code were moved into the database by the migration that creates the table.

## Structure and relationships

The [public and admin endpoint groups](../../../../src/Content/Content.Api/Endpoints/Contacts/ContactsEndpoints.cs) (one `<UseCase>Endpoint` per operation) dispatch to one query and one command. [CompanyContacts](../../../../src/Content/Content.Domain/Contacts/CompanyContacts.cs) is a single-row aggregate (`Id = 1`, guarded by a check constraint): it is never created or deleted through the API, only replaced as a whole. Phones are stored in E.164 (`+380XXXXXXXXX`) and formatted by clients; Telegram must be an `https://t.me/` or `https://telegram.me/` link, the map any `https` link. The [validator](../../../../src/Content/Content.Application/Features/Contacts/Commands/UpdateCompanyContacts/UpdateCompanyContactsValidator.cs) reports each field by its JSON name, so the admin form can show it.

A save stores `UpdatedBy`, the editor's email ([ContentEditor](../../../../src/Content/Content.Application/Shared/ContentEditor.cs)); only the admin responses (`AdminCompanyContacts`) return it.

Access: `GET /api/content/contacts` is anonymous; `/api/admin/content/contacts` needs `CanManageContent` (Manager, Seo, or Admin).

## Code structure

```text
src/Content/
├── Content.Api/
│   ├── Endpoints/Contacts/ public group       # public GET
│   ├── Endpoints/Contacts/ admin group  # admin GET and PUT
│   └── Contacts/CompanyContactsResponses.cs           # response, request, mapping
├── Content.Application/Features/Contacts/
│   ├── Common/CompanyContactsDtos.cs                  # input, details, mapper
│   ├── Get/                                           # GetCompanyContactsQuery
│   └── Update/                                        # command, handler, validator
├── Content.Domain/Contacts/                           # CompanyContacts, errors
└── Content.Infrastructure/
    ├── Configurations/CompanyContactsConfiguration.cs
    └── Migrations/*_AddCompanyContacts.cs             # table + the site's current values
```

## Data

Table `content."CompanyContacts"`; see the [database map](../../architecture/data-model/content-articles.md).

## Tests

- Unit: [CompanyContactsTests](../../../../tests/UnitTests/Content/CompanyContactsTests.cs) — `dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~UnitTests.Content"`.
- Integration: [HTTP and access](../../../../tests/IntegrationTests/Content/CompanyContactsApiTests.cs), [PostgreSQL seed and single row](../../../../tests/IntegrationTests/Content/CompanyContactsPostgresTests.cs) — `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter "FullyQualifiedName~IntegrationTests.Content"`.

## Related sources

- [Specification](../../specs/content/002-company-contacts/README.md)
- [Contract](../../contracts/content/contacts.openapi.yaml)
