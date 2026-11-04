# Content 007: who changed it and «Оновити сайт зараз»

**Status:** implemented 2026-11-04. **Owner module:** Content. **Feature card:** [Refresh the site now](../../../features/content/site-refresh.md); editor fields in [articles](../../../features/content/articles.md) and [company contacts](../../../features/content/company-contacts.md).

## Goal

Stage 6 of the plan [admin-site-content](https://github.com/Weretik/kedr-web/blob/main/docs/ideas/admin-site-content.md): the admin shows who saved the last change of articles, contacts, requisites and documents, and a manager can make the site show a change at once instead of after its cache lifetime.

**Out of scope:** a full change log for articles or contacts (only the last editor), automatic refresh after every save, refreshing one page or one tag.

## Rules

- R-001 Saving an article (create, update, cover upload or removal) or the contacts stores `UpdatedBy`: the editor's email, else user name, else id (`ContentEditor.Name`, shared with documents and requisites), trimmed to 256. Rows saved before this have `null`.
- R-002 `UpdatedBy` is admin-only: the public contacts and article responses never include it. The admin contacts responses use `AdminCompanyContacts`.
- R-003 Requisites keep their change log and documents their `UpdatedBy`/`PublishedBy`; nothing changes there.
- R-004 `POST /api/admin/content/site-refresh` (`CanManageContent`) POSTs `{"tags":["content","catalog"]}` to `{SiteRevalidation:Url}/api/revalidate` with header `x-revalidate-secret: {SiteRevalidation:Secret}` (10 s timeout).
- R-005 Answers: 200 `refreshed`; 503 `notConfigured` when the address or secret is missing or not http(s) (nothing is sent); 502 `siteUnavailable` when the site fails, refuses the secret or does not answer.

## Scenarios

- SC-001 A manager saves an article or the contacts; the admin shows «Змінено … · manager@…».
- SC-002 The public contacts response has no `updatedBy`.
- SC-003 The site is connected: «Оновити сайт зараз» answers 200 and the site reads fresh data on the next visit.
- SC-004 The site is not connected in this environment: 503 `notConfigured`, the admin explains that changes appear within minutes.
- SC-005 Anonymous and `User` requests get 401/403.

## Data and rollout

Migration `AddContentEditors` adds nullable `UpdatedBy varchar(256)` to `content."Articles"` and `content."CompanyContacts"`. The deploy attaches `SiteRevalidation__Url` and `SiteRevalidation__Secret` from GCP secrets `site-revalidate-url` and `site-revalidate-secret` only when both exist; the site gets the same secret as `KEDR_REVALIDATE_SECRET`. Rollback: Down drops both columns; removing the secrets turns the button back to «not connected».

## Evidence

Unit `UnitTests.Content` (68) and integration `IntegrationTests.Content` (73: editor stored on PostgreSQL, public contacts without the editor, refresh endpoint roles and outcomes, the request sent to the site) pass; architecture tests (10) pass.
