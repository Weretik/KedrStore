# Content 012: article search texts and the shop address

**Status:** implemented 2026-11-26. **Owner module:** Content. **Feature cards:** [Site articles](../../../features/content/articles.md), [Company contacts](../../../features/content/company-contacts.md).

## Goal

The SEO audit of 2026-11-26: articles have no search title or description of their own, and the site has no `LocalBusiness` markup because the backend holds no shop address or opening hours. The owner gave the shop (2026-11-26): «Крамниця Дверна фурнітура CLASS», вул. Ахієзерів, 46, Харків, Харківська обл., 61038; Monday to Friday 9:00–16:00, weekends closed.

**Out of scope:** writing the texts (the SEO specialist's job), the site markup itself (kedr-web).

## Rules

- R-01 **Article search texts.** Each `ArticleTranslation` has optional `SeoTitle` (up to 70) and `MetaDescription` (up to 160); blank is stored as null. The admin read and save carry them per language; the public article page returns them, and the site falls back to the title and the excerpt when they are null.
- R-02 **Shop.** `CompanyContacts.Store` ([CompanyStore](../../../../../src/Modules/Content/Content.Domain/Contacts/CompanyStore.cs), columns of the same row) holds the shop name, street and region in uk/ru, a five-digit postal code, and opening hours for weekdays, Saturday and Sunday as `HH:mm` pairs: both times or neither (a day off), and the shop opens before it closes. Every field is optional; a contacts save without `store` keeps the shop as it is, so an older admin form cannot clear it. The public and admin contacts carry `store`. The site marks up `LocalBusiness` only when the Ukrainian street is filled in.
- R-03 **Data.** Migration `ArticleSeoAndStoreAddress` adds the columns and fills the shop above once, only while the street is empty.
- R-04 **Access.** Unchanged: article and contacts edits need `CanManageContent`; the public reads stay anonymous under their rate limits.

## Scenarios

- SC-01 The SEO specialist writes a search title for the Ukrainian article; the Ukrainian page returns it, the Russian page returns null and the site uses the Russian title.
- SC-02 After the deploy the site reads the owner's shop and marks it up; a manager changes the hours in «Контакти» and the site follows.
- SC-03 A four-digit postal code or a closing time before the opening is refused with a field error and nothing changes.

## Data and rollout

Nullable columns on `content."ArticleTranslations"` and `content."CompanyContacts"`; the previous release keeps working. Deploy the backend before the admin; an older admin form saves contacts without `store`, which keeps the shop. Rollback: Down drops the columns.

## Evidence

Unit `CompanyStoreTests` (7), `ArticleTests.SeoFields_*`. Integration `CompanyContactsApiTests` (shop round trip and refusal) and `ArticlesApiTests` (search texts per language) pass.
