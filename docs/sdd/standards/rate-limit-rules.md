# Rate limit rules

Every endpoint has a rate limit decision, made while the feature is specified, not after an incident. Two layers protect the API: the **edge** (Cloudflare, before the request reaches Cloud Run) and the **host** (`Host.Api`, ASP.NET Core rate limiter). This page is the rule and the table of what exists; the code is `src/Bootstrapper/Host.Api/Network/RateLimitingRegistrationExtensions.cs`.

## The decision for every new endpoint

Answer this in the feature SDD (design section) and record the answer in the PR:

```text
Who calls it?
  | anonymous (public site, login, form)   -> needs its own policy: nobody is accountable for the traffic
  | signed in, cheap (a normal read/write) -> the global soft limit is enough; write "global"
  | signed in, expensive or bulk           -> upload, import, sync, report, export, PDF, snapshot: use Upload or HeavyOperation
  | sends a message outside (Telegram, e-mail, 1C) -> own policy even for staff, plus an Idempotency-Key
  | credentials or codes (login, refresh, reset) -> own strict policy
Is a tighter limit than the existing policies needed?
  | no  -> reuse a policy
  | yes -> add a policy (name, number, reason) in the host and in the table below
```

"Global is enough" is a valid, written answer. "Not thought about" is not. A rate limit never replaces an `Idempotency-Key` ([idempotency rules](idempotency-rules.md)).

## The limits

A request must pass the global limit **and** its named policy. All limits are per calling address and use a fixed one-minute window unless noted. The address is the real client address restored by `TrustedProxyClientIpMiddleware` from the Cloudflare Worker header. Over the limit, the API answers `429` with `Retry-After`.

| Policy | Default | Used by |
| --- | --- | --- |
| global (no name) | 1200 per minute | every request except `/health*` and `/internal/*` |
| `AuthLogin` | 10 per minute | cookie login, mobile login |
| `AuthRefresh` | 30 per minute | mobile refresh |
| `QuickOrder` | 60 per 10 minutes | anonymous quick order (`POST /api/orders`) |
| `PublicRead` | 600 per minute | anonymous catalogue and content reads (groups `api/catalog/{lang}`, `api/content`, articles, documents, gallery, contacts, requisites, wholesale, SEO) |
| `PublicSearch` | 300 per minute | product lists with search and filters (`getPublicProducts`, `getPublicProductsByCategorySlug`) |
| `Upload` | 60 per minute | file uploads (media, avatars, article cover, attribute swatch) |
| `HeavyOperation` | 20 per minute | mobile snapshots, customer snapshot, work reports, order sync retry, site order Telegram resend, retail price file import and unlock |

The numbers for global, `PublicRead`, `PublicSearch`, `Upload` and `HeavyOperation` come from the `RateLimiting` configuration section (`RateLimiting__GlobalPerMinute` and so on as environment variables on the Cloud Run service), so they can be tuned without code. The defaults are soft: they stop a flood or a runaway script, not a normal buyer or a busy manager.

**One address carries the whole site.** The public site (Next.js) calls the API from its server, so every visitor shares the site server's address. The public limits are therefore flood limits, not per-visitor limits; keep them high and put per-visitor protection (bots, scrapers) at the edge.

**The counter lives in one instance.** The limiter is in memory, so with several Cloud Run instances the real limit is the number times the instance count. This is accepted while the service runs one or two instances; a shared counter (Redis) is justified only when the limit must be exact (see the architect check `SEC-08`).

## Edge layer (Cloudflare)

A rate limiting rule on the zone stops a flood before Cloud Run is billed for it. The setup for `api.classkedr.com.ua` is in [Cloudflare API proxy](../operations/deployment/cloudflare-api-proxy.md#edge-rate-limiting). The Worker itself only forwards the request.

## Tests

`tests/ArchitectureTests/Hosts/RateLimitCoverageTests.cs` fails the build when an anonymous endpoint or group names no policy, when a policy name is not registered by the host, or when an upload or a heavy operation (file name `Upload*`, `*Import*`, `*Report*`, `*Pdf*`, `*Export*`, `*Snapshot*`) lacks its policy. Add a new policy to the host, to this table and to the test's expectations in the same change.
