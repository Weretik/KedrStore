# Content 002: company contacts

**Status:** implemented 2026-11-04. **Owner module:** Content. **Feature card:** [Company contacts](../../../features/content/company-contacts.md).

## Goal

The sales phone and other contacts appear on every site page and in JSON-LD, but lived in site code, so a change needed a release and the legal texts drifted. Managers and Admins now change them in one admin card; the site reads them from the API (audit [admin-site-content](https://github.com/Weretik/kedr-web/blob/main/docs/ideas/admin-site-content.md), item 1).

**Out of scope:** requisites (separate, Admin only), working hours, several phones or offices, history of changes.

## Rules

- R-001 One record (`Id = 1`), created with the current site values by the migration; the API only reads and replaces it.
- R-002 Phones are Ukrainian numbers in E.164 (`+380` and nine digits); clients format them. Telegram is an `https://t.me/` or `https://telegram.me/` link; the map any `https` link; email, cities required. All fields are trimmed.
- R-003 `GET /api/content/contacts` is anonymous; `GET`/`PUT /api/admin/content/contacts` need `CanManageContent`.
- R-004 Invalid input answers 400 with an error per field (`salesPhone`, `email`, `telegramUrl`, `viberPhone`, `mapUrl`, `cityUk`, `cityRu`); nothing is saved.

## Scenarios

- SC-001 After the migration the public endpoint returns the values the site used (`+380687135049`, `kedr.class19@gmail.com`, Харків/Харьков).
- SC-002 A Manager saves a new phone; the public endpoint returns it at once.
- SC-003 A phone typed as `068 713 50 49` is rejected with a `salesPhone` error.
- SC-004 Anonymous and `User` requests to the admin endpoint get 401/403.

## Data and rollout

Table `content."CompanyContacts"` with a check constraint `"Id" = 1`; migration `AddCompanyContacts` creates it and inserts the row. Deploy runs it automatically. Rollback: the migration's Down drops the table; the site falls back to the values in its code when the API answers no contacts.

## Evidence

Unit `UnitTests.Content` and integration `IntegrationTests.Content` (HTTP on InMemory, migration and single row on PostgreSQL 16) pass; architecture tests pass.
