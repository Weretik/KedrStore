# Content 013: site redirects

**Status:** implemented 2026-11-26. **Owner module:** Content. **Feature card:** [Site redirects](../../../features/content/site-redirects.md).

## Goal

SEO stage 3 (owner, 2026-11-26): the SEO specialist moves pages and must keep old addresses working without a developer. Today every redirect is in the site code (`next.config.mjs`). The admin «Переадресації» keeps a list of old address → new address; the site answers each old address with a permanent 301.

**Out of scope:** the rule for the old regional pages `/region/*` (owner decision 2026-11-26: they go to the home page) stays in the site code, because it is a pattern, not one address; the site part itself (kedr-web); temporary (302) redirects; hit counters.

## Rules

- R-01 **Redirect.** [SiteRedirect](../../../../../src/Modules/Content/Content.Domain/Redirects/SiteRedirect.cs) is an old path, a new address, and who changed it when. At most 2000 redirects (the site loads them all at once); the next create answers 409.
- R-02 **Addresses** ([SiteRedirectPath](../../../../../src/Modules/Content/Content.Domain/Redirects/SiteRedirectPath.cs)). The old address is stored as the site matches it: the path alone (a pasted full URL gives its path; a query or fragment is dropped), percent-decoded, lower case, without a trailing slash, at most 500 characters. The home pages (`/`, `/uk`, `/ru`), `/api/` and `/_next/` are refused. The new address is a site path starting with `/` (its query kept) or a full `http(s)` URL, at most 1000 characters; anything else (`//host`, `javascript:`, spaces) is refused. A redirect to the same path is refused.
- R-03 **One per address, no chains** ([SiteRedirectRules](../../../../../src/Modules/Content/Content.Application/Features/Redirects/Shared/SiteRedirectRules.cs)). A second redirect for the same old path answers 409 (also for a racing copy, through the unique index `UX_SiteRedirects_FromPath` caught by `IContentUniqueSave`). A new address that is itself redirected, or an old address another redirect leads to, is refused with 400: search engines follow one hop, so the editor gives the final address.
- R-04 **Admin.** `GET/POST api/admin/content/redirects`, `PUT/DELETE api/admin/content/redirects/{id}` under `CanManageContent` (Manager, Seo, Admin). The list is newest change first, paged in SQL (`pageSize` 1–100), searched word by word with escaped `ILIKE` on trigram indexes of both addresses ([SiteRedirectSearch](../../../../../src/Modules/Content/Content.Infrastructure/Persistence/Seo/SiteRedirectSearch.cs)). A save sends the `version` it read; a newer row answers 409.
- R-05 **Site.** `GET api/content/redirects` is anonymous under the `PublicRead` rate limit and returns every redirect as `{from, to}`. The site keeps the list in memory for five minutes and answers a request whose path matches `from` with 301 to `to` before any page renders; when the API is down it keeps the last list.

## Scenarios

- SC-01 The SEO specialist adds `/uk/stara-storinka` → `/uk/catalog/ruchky`; within five minutes the old address answers 301 to the new one.
- SC-02 They paste `https://classkedr.com.ua/UK/Stara-Storinka/?utm=1` as the old address; it is saved as `/uk/stara-storinka`.
- SC-03 They add `/uk/x` → `/uk/stara-storinka` while `/uk/stara-storinka` is redirected; the form says to give the final address.
- SC-04 Two editors add the same old address at once; one gets 409.

## Rate limits and idempotency

Admin endpoints are cheap signed-in calls under the global soft limit; the public list is anonymous and names `PublicRead`. Create has a natural key (the old path, unique index, caught violation → 409); update and delete are idempotent; see [idempotency rules](../../../standards/idempotency-rules.md).

## Data and rollout

Migration `SiteRedirects` creates `content."SiteRedirects"` (identity id, `FromPath` varchar 500 unique, `ToUrl` varchar 1000, times, editor, `xmin`), trigram indexes on both addresses, an index on `UpdatedAt`, and makes sure `pg_trgm` exists; the previous release ignores the table. Deploy the backend before the site; until then the site's read fails and it simply redirects nothing. Rollback: Down drops the table (the extension stays: other modules use it).

## File-role map

| Role | File |
| --- | --- |
| Aggregate, id, errors, address rules | `Content.Domain/Redirects/` |
| Use cases | `Content.Application/Features/Redirects/{Commands,Queries,Shared}/` |
| Ports | `Content.Application/Abstractions/Persistence/{IContentUniqueSave,ISiteRedirectSearch}.cs` |
| Persistence | `Content.Infrastructure/Persistence/Seo/{SiteRedirectConfiguration,SiteRedirectSearch}.cs`, `Persistence/ContentUniqueSave.cs`, migration `*_SiteRedirects` |
| Endpoints | `Content.Api/Endpoints/Redirects/` |

## Evidence

Unit `SiteRedirectPathTests`, `SiteRedirectTests` (27). Integration `SiteRedirectsApiTests` (8: access, normalized save and anonymous read, duplicate, both chains, self, reserved path, stale version, delete and repeat). Architecture tests (folders, rate limits, OpenAPI rules) pass.

## Final audit (2026-11-26)

Scope: the staged diff of this spec and `platform/010-seo-review`. Step 0: the public list is one indexed read of at most 2000 short rows, asked by each site worker once per five minutes; the admin calls are cheap signed-in reads and writes. No hot-path target in [quality targets](../../../architecture/overview/quality-targets.md) changes. Checked: module ownership (Content owns the table, Catalog and Content each review their own pages), use-case folders, validators, the error mapper, one `SaveChanges` per command, the folder axiom, rate limits, OpenAPI rules, idempotency table.

| ID | Severity | Finding | Status |
| --- | --- | --- | --- |
| AF-01 | low | The chain check is not serialized: two editors saving A→B and B→A at the same moment could store a two-row loop. Browsers stop after about 20 hops; the site ignores a redirect to the same address. | accepted: admin edits are rare and done by one SEO person; the next edit of either row refuses the chain |
| AF-02 | low | The 2000-row limit is checked before the insert, so two creates at the same moment can pass it by one. | accepted: the limit guards size, not money |
| AF-03 | low | No end-to-end test opens «Переадресації» and follows a 301 on the site. | deferred: E2E runs only with the owner's separate consent |
