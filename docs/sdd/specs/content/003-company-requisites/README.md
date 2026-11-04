# Content 003: company requisites

**Status:** implemented 2026-11-04. **Owner module:** Content. **Feature card:** [Company requisites](../../../features/content/company-requisites.md).

## Goal

The requisites page and its copy buttons lived in site code, so a new account needed a release, and nobody could see who changed what. Admins now change them in one admin card with a change history; the site reads them from the API (plan [admin-site-content](https://github.com/Weretik/kedr-web/blob/main/docs/ideas/admin-site-content.md), stage 2).

**Out of scope:** several accounts or currencies, approval of a change by a second person, restoring an old version with one click.

## Rules

- R-001 One record (`Id = 1`), created with the current site values by the migration; the API only reads and replaces it.
- R-002 All fields are required and trimmed. Code: 8 or 10 digits. Extract number: up to 30 digits. IBAN: spaces removed, upper case, `UA` + 27 digits, valid mod-97 checksum. МФО: 6 digits, equal to digits 5–10 of the IBAN. Name up to 200, bank up to 120 characters.
- R-003 `GET /api/content/requisites` is anonymous; `GET`/`PUT /api/admin/content/requisites` and `GET /api/admin/content/requisites/history` need the Admin role (Managers get 403).
- R-004 Invalid input answers 400 with an error per field (`legalName`, `code`, `registrationExtractNumber`, `iban`, `bankName`, `bankCode`); nothing is saved and nothing is logged.
- R-005 Each save that changes something writes one history entry in the same transaction: time, user id, email (or user name), and per changed field its API name with the old and new value. A save with the same values writes nothing. History returns the 50 newest entries, newest first.

## Scenarios

- SC-001 After the migration the public endpoint returns the values the site used (`2689400771`, `UA303052990000026007025905216`, ПриватБанк, `305299`).
- SC-002 An Admin changes the name; the public endpoint returns it at once and history shows one entry with `legalName` old → new and the Admin's email.
- SC-003 The same values saved again add no history entry.
- SC-004 An IBAN with a wrong checksum is rejected with an `iban` error; an МФО that differs from the IBAN with a `bankCode` error.
- SC-005 Anonymous, `User` and `Manager` requests to the admin endpoints get 401/403/403.

## Data and rollout

Table `content."CompanyRequisites"` (check constraint `"Id" = 1`) and `content."CompanyRequisitesChanges"` (identity id, changed fields in a jsonb column, index on `ChangedAt`); migration `AddCompanyRequisites` creates both and inserts the row. Deploy runs it automatically. Rollback: the migration's Down drops both tables; the site falls back to the values in its code when the API answers no requisites.

## Evidence

Unit `UnitTests.Content` (36) and integration `IntegrationTests.Content` (34, HTTP on InMemory, migration, single row and jsonb log on PostgreSQL 16) pass; architecture tests (10) pass.
