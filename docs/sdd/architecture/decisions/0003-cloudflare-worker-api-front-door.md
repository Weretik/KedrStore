# ADR-0003: Cloudflare Worker as the API front door

- Status: accepted
- Date: 2026-10-28

## Context

The admin and the site live on `classkedr.com.ua`. Browser sign-in uses an HttpOnly session cookie and a CSRF cookie. A cookie set by `*.run.app` is third-party for these pages, browsers block it more and more, and a page cannot read a CSRF cookie of another site. The API therefore needs a host on the same site. Cloud Run domain mapping is not available in the chosen region (`europe-central2`), and a Google external load balancer has a fixed monthly cost.

## Alternatives considered

- Cloud Run domain mapping: not available in `europe-central2`.
- Google external HTTPS load balancer: works, but has a fixed monthly cost.
- Calling `*.run.app` directly from the browser: third-party cookies and an unreadable CSRF cookie.

## Decision

- A small Cloudflare Worker serves `api.classkedr.com.ua` and forwards every request to the Cloud Run service.
- The Worker passes the real client IP in `X-Kedr-Client-IP` together with a shared secret in `X-Kedr-Proxy-Secret`.
- In the API, `TrustedProxyClientIpMiddleware` accepts the client IP only when the secret matches and always removes both headers, so a direct caller cannot fake its IP.
- The CSRF cookie is set for the parent domain `classkedr.com.ua`; refresh cookies are first-party.

## Why

- First-party cookies and a readable CSRF token without a load balancer.
- Cloudflare TLS, DDoS protection and caching in front of the API at almost no cost.
- Rate limiting and audit logs see real client IPs.

## Trade-offs

- The Cloud Run URL stays publicly reachable. The API does not reject direct calls, so Cloudflare protection can be bypassed; every endpoint must enforce its own authorization. A future step is to reject requests without the proxy secret (except `/health`).
- The Workers free plan allows 100,000 requests per day; heavy traffic needs a paid plan.
- Local frontends calling the deployed API are cross-site, so token refresh works only against a local API.
